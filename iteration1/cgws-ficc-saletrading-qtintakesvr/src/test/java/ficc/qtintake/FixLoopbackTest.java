package ficc.qtintake;

import static org.junit.jupiter.api.Assertions.*;

import org.apache.log4j.*;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.Timeout;
import org.junit.jupiter.api.io.TempDir;

import quickfix.*;

import java.io.ByteArrayInputStream;
import java.net.ServerSocket;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.Properties;
import java.util.concurrent.*;

class FixLoopbackTest {
    @TempDir Path directory;

    @Test
    @Timeout(20)
    void receivesOneRealFixMessageAndLogsMaskedPayload() throws Exception {
        int port;
        try (ServerSocket socket =
                new ServerSocket(0, 1, java.net.InetAddress.getByName("127.0.0.1"))) {
            port = socket.getLocalPort();
        }
        String common =
                "[default]\n"
                        + "StartTime=00:00:00\n"
                        + "EndTime=00:00:00\n"
                        + "HeartBtInt=30\n"
                        + "UseDataDictionary=N\n"
                        + "ValidateIncomingMessage=N\n"
                        + "ValidateUserDefinedFields=N\n";
        String server =
                common
                        + "ConnectionType=acceptor\n"
                        + "SocketAcceptAddress=127.0.0.1\n"
                        + "SocketAcceptPort="
                        + port
                        + "\n"
                        + "[session]\n"
                        + "BeginString=FIX.4.4\n"
                        + "SenderCompID=SERVER\n"
                        + "TargetCompID=CLIENT\n";
        Path settingsFile = directory.resolve("fix.cfg");
        Files.write(
                settingsFile,
                (common
                                + "ConnectionType=initiator\n"
                                + "ReconnectInterval=1\n"
                                + "PersistMessages=N\n"
                                + "FileStorePath="
                                + directory.resolve("store").toString().replace('\\', '/')
                                + "\n"
                                + "[session]\n"
                                + "BeginString=FIX.4.4\n"
                                + "SenderCompID=CLIENT\n"
                                + "TargetCompID=SERVER\n"
                                + "SocketConnectHost=127.0.0.1\n"
                                + "SocketConnectPort="
                                + port
                                + "\n")
                        .getBytes(StandardCharsets.UTF_8));
        CountDownLatch connected = new CountDownLatch(1);
        ApplicationAdapter acceptorApplication =
                new ApplicationAdapter() {
                    @Override
                    public void onLogon(SessionID id) {
                        connected.countDown();
                    }
                };
        SessionSettings serverSettings =
                new SessionSettings(
                        new ByteArrayInputStream(server.getBytes(StandardCharsets.UTF_8)));
        SocketAcceptor acceptor =
                new SocketAcceptor(
                        acceptorApplication,
                        new MemoryStoreFactory(),
                        serverSettings,
                        new ScreenLogFactory(false, false, false),
                        new DefaultMessageFactory());
        Properties configuration = new Properties();
        configuration.setProperty("fix.settings", settingsFile.toString());
        configuration.setProperty("fix.username", "loopback-user");
        configuration.setProperty("fix.password", "never-print-this-password");
        Path log = directory.resolve("messages.log");
        FileAppender appender = new FileAppender(new PatternLayout("%m%n"), log.toString());
        Logger logger = Logger.getLogger("ficc.qtintake.messages");
        logger.setLevel(Level.DEBUG);
        logger.addAppender(appender);
        try (DurableInbox inbox = new DurableInbox(directory.resolve("inbox"), 5);
                QTradeFixReceiver receiver = new QTradeFixReceiver(configuration, inbox)) {
            acceptor.start();
            receiver.start();
            assertTrue(connected.await(5, TimeUnit.SECONDS), "real FIX logon completed");
            SessionID id = acceptor.getSessions().get(0);
            assertTrue(Session.sendToTarget(QtIntakeTest.capturedPrlQuote(), id));
            Path pending = null;
            long deadline = System.nanoTime() + TimeUnit.SECONDS.toNanos(5);
            while (pending == null && System.nanoTime() < deadline) {
                Thread.sleep(25);
                pending = inbox.next();
            }
            assertNotNull(pending, "quote durably received through the FIX socket");
            com.alibaba.fastjson2.JSONObject quote =
                    inbox.request(pending).getJSONArray("quotes").getJSONObject(0);
            assertEquals("240210.IB", quote.getString("securityId"));
            assertEquals("2026-09-22", quote.getString("tradeDate"));
            assertEquals("2", quote.getString("settlType"));
            String output = new String(Files.readAllBytes(log), StandardCharsets.UTF_8);
            assertTrue(output.contains("FIX RECEIVE"));
            assertTrue(output.contains("117=QT-CAPTURE-1"));
            assertTrue(output.contains("554=[REDACTED]"));
            assertFalse(output.contains("never-print-this-password"));
        } finally {
            acceptor.stop(true);
            logger.removeAppender(appender);
            appender.close();
        }
    }
}
