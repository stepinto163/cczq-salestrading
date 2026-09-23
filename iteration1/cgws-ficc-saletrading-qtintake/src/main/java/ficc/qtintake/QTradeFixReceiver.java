package ficc.qtintake;

import org.apache.log4j.Logger;
import org.quickfixj.CharsetSupport;

import quickfix.*;

import java.io.IOException;
import java.util.Properties;

/** Initiator based on the working PRL receiver; never sends trading application messages. */
public final class QTradeFixReceiver implements Application, AutoCloseable {
    private static final Logger LOGGER = Logger.getLogger(QTradeFixReceiver.class);
    private static final Logger MESSAGES = Logger.getLogger("ficc.qtintake.messages");
    private final Properties configuration;
    private final DurableInbox inbox;
    private final QTradeQuoteMapper mapper = new QTradeQuoteMapper();
    private ThreadedSocketInitiator initiator;
    private volatile boolean loggedOn;
    private volatile boolean storageFailed;

    public QTradeFixReceiver(Properties configuration, DurableInbox inbox) {
        this.configuration = configuration;
        this.inbox = inbox;
        required(configuration, "fix.username");
        required(configuration, "fix.password");
    }

    public void start() throws Exception {
        CharsetSupport.setCharset(configuration.getProperty("fix.charset", "UTF-8"));
        SessionSettings settings = new SessionSettings(required(configuration, "fix.settings"));
        SessionFactory delegate =
                new DefaultSessionFactory(
                        this,
                        new FileStoreFactory(settings),
                        new ScreenLogFactory(false, false, false),
                        new DefaultMessageFactory());
        boolean replay =
                Boolean.parseBoolean(configuration.getProperty("fix.replayOnStartup", "false"));
        SessionFactory factory =
                (sessionId, sessionSettings) -> {
                    Session session = delegate.create(sessionId, sessionSettings);
                    if (replay) {
                        try {
                            session.setNextTargetMsgSeqNum(1);
                        } catch (IOException error) {
                            try {
                                session.close();
                            } catch (IOException closeError) {
                                error.addSuppressed(closeError);
                            }
                            throw new ConfigError(error);
                        }
                    }
                    return session;
                };
        initiator = new ThreadedSocketInitiator(factory, settings);
        initiator.start();
        LOGGER.info("FIX state=CONNECTING settings=" + configuration.getProperty("fix.settings"));
    }

    public boolean loggedOn() {
        return loggedOn;
    }

    public boolean storageFailed() {
        return storageFailed;
    }

    @Override
    public void onCreate(SessionID sessionId) {
        LOGGER.info("FIX state=CREATED session=" + sessionId);
    }

    @Override
    public void onLogon(SessionID sessionId) {
        loggedOn = true;
        LOGGER.info("FIX state=CONNECTED session=" + sessionId);
    }

    @Override
    public void onLogout(SessionID sessionId) {
        loggedOn = false;
        LOGGER.warn("FIX state=DISCONNECTED session=" + sessionId);
    }

    @Override
    public void toAdmin(Message message, SessionID sessionId) {
        try {
            if ("A".equals(message.getHeader().getString(35))) {
                message.setString(553, required(configuration, "fix.username"));
                message.setString(554, required(configuration, "fix.password"));
                message.setInt(98, 0);
            }
            log("SEND", message, sessionId);
        } catch (FieldNotFound error) {
            throw new IllegalStateException("Cannot prepare FIX administrative message", error);
        }
    }

    @Override
    public void fromAdmin(Message message, SessionID sessionId) {
        log("RECEIVE", message, sessionId);
    }

    @Override
    public void toApp(Message message, SessionID sessionId) throws DoNotSend {
        log("SUPPRESSED", message, sessionId);
        throw new DoNotSend();
    }

    // Compatibility with the modified QuickFIX interface in older STC runtimes.
    public void checkRateLimit(Message message) {}

    public boolean checkPassword(Message message, String username, String password) {
        return false;
    }

    @Override
    public void fromApp(Message message, SessionID sessionId) {
        log("RECEIVE", message, sessionId);
        if (storageFailed) {
            throw new IllegalStateException(
                    "QtIntake storage failed; FIX replay is required after recovery");
        }
        try {
            com.alibaba.fastjson2.JSONObject request;
            try {
                request = mapper.map(message);
            } catch (FieldNotFound | IllegalArgumentException | java.time.DateTimeException error) {
                inbox.rejectRaw(formatFixMessage(message), error.getMessage());
                LOGGER.error("FIX state=REJECTED session=" + sessionId, error);
                return;
            }
            if (request == null) {
                LOGGER.info(
                        "FIX state=IGNORED session="
                                + sessionId
                                + " reason=OUTSIDE_QUOTE_CREATE_SCOPE");
                return;
            }
            inbox.enqueue(request);
            LOGGER.info("FIX state=BUFFERED requestId=" + request.getString("requestId"));
        } catch (IOException error) {
            storageFailed = true;
            LOGGER.error(
                    "FIX state=FAILED reason=INBOX_UNAVAILABLE; disconnecting; replay required",
                    error);
            // Do not join QuickFIX worker threads from inside their callback.
            Thread stop = new Thread(this::close, "qtintake-storage-failure");
            stop.setDaemon(true);
            stop.start();
            throw new IllegalStateException("Unable to persist QTrade input", error);
        }
    }

    static String formatFixMessage(Message message) {
        return message.toString()
                .replace('\u0001', '|')
                .replace("\r", "\\r")
                .replace("\n", "\\n");
    }

    private void log(String direction, Message message, SessionID sessionId) {
        if (MESSAGES.isDebugEnabled()) {
            MESSAGES.debug(
                    "FIX " + direction + " session=" + sessionId + " body=" + formatFixMessage(message));
        }
    }

    static String required(Properties configuration, String name) {
        String value = configuration.getProperty(name);
        if (value == null || value.trim().isEmpty()) {
            throw new IllegalArgumentException("Missing configuration: " + name);
        }
        return value;
    }

    @Override
    public void close() {
        if (initiator != null) {
            initiator.stop(true);
        }
        loggedOn = false;
    }
}
