package ficc.qtintake;

import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.Server;
import ats.bbox.framework.resource.ResourceAdapter;

import ficc.common.ComponentLog;
import ficc.common.JsonUtils;
import ficc.common.client.NetClientTransport;

import org.apache.log4j.Logger;

import sc.app.stc.net.NetClientResource;

import java.io.BufferedReader;
import java.nio.file.*;
import java.util.Map;
import java.util.Properties;

public final class QtIntakeResource extends ResourceAdapter {
    private static final Logger LOGGER = Logger.getLogger(QtIntakeResource.class);
    private final Properties configuration = new Properties();
    private DurableInbox inbox;
    private QTradeFixReceiver receiver;
    private QuoteForwarder forwarder;
    private NetClientTransport transport;

    @Override
    public void init() throws LifecycleException {
        configuration.setProperty("diagnostics.serviceName", "QtIntakeSvr");
        try {
            ComponentLog.initialize(
                    configuration,
                    "CONFIGURATION",
                    "files",
                    "Properties",
                    getConfig().getString("intake.properties"),
                    () -> {
                        load(getConfig().getString("intake.properties"));
                        load(QTradeFixReceiver.required(configuration, "secrets.file"));
                        if (QTradeFixReceiver.required(configuration, "intake.token").length()
                                < 32) {
                            throw new IllegalArgumentException(
                                    "intake.token must contain at least 32 characters");
                        }
                        return configuration;
                    });
            inbox =
                    ComponentLog.initialize(
                            configuration,
                            "INBOX",
                            "files",
                            "DurableInbox",
                            configuration.getProperty("inbox.path", "./data/inbox"),
                            () ->
                                    new DurableInbox(
                                            Paths.get(
                                                    configuration.getProperty(
                                                            "inbox.path", "./data/inbox")),
                                            Integer.parseInt(
                                                    configuration.getProperty(
                                                            "inbox.capacity", "10000"))));
            super.init();
        } catch (Exception error) {
            throw new LifecycleException(error);
        }
    }

    private void load(String filename) throws Exception {
        if (filename.startsWith("$ATS_ROOT/")) {
            String root = System.getenv("ATS_ROOT");
            if (root == null || root.trim().isEmpty()) {
                throw new IllegalStateException("ATS_ROOT is required");
            }
            filename = Paths.get(root).resolve(filename.substring(10)).toString();
        }
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(filename))) {
            configuration.load(reader);
        }
    }

    @Override
    public void start() throws LifecycleException {
        try {
            NetClientResource network =
                    (NetClientResource) Server.getServer().getResource("NetClientResource");
            transport =
                    ComponentLog.initialize(
                            configuration,
                            "QUOTE_CLIENT",
                            "stc",
                            "NetClientTransport",
                            "SERVER.QuoteSvr",
                            () ->
                                    new NetClientTransport(
                                            network.getNetClientWrapper(),
                                            "SERVER.QuoteSvr",
                                            Long.parseLong(
                                                    configuration.getProperty(
                                                            "quote.timeoutMillis", "3000"))));
            // STC has no QTrade account column, so the owner comes from an operator-owned file.
            OwnerMapping owners =
                    ComponentLog.initialize(
                            configuration,
                            "OWNER_LOOKUP",
                            "files",
                            "OwnerMapping",
                            configuration.getProperty("owner.mapFile", "owner.defaultUserId"),
                            () -> OwnerMapping.load(configuration));
            forwarder =
                    new QuoteForwarder(
                            inbox, transport, configuration.getProperty("intake.token"), owners);
            receiver =
                    ComponentLog.initialize(
                            configuration,
                            "FIX_RECEIVER",
                            "quickfix",
                            "QTradeFixReceiver",
                            configuration.getProperty("fix.settings"),
                            () -> {
                                QTradeFixReceiver candidate =
                                        new QTradeFixReceiver(configuration, inbox);
                                try {
                                    candidate.start();
                                    return candidate;
                                } catch (Exception error) {
                                    candidate.close();
                                    throw error;
                                }
                            });
            super.start();
        } catch (Exception error) {
            if (receiver != null) {
                receiver.close();
            }
            if (inbox != null) {
                try {
                    inbox.close();
                } catch (Exception closeError) {
                    error.addSuppressed(closeError);
                }
            }
            throw new LifecycleException(error);
        }
    }

    public void forward() {
        try {
            forwarder.tick();
        } catch (Exception error) {
            LOGGER.error("FORWARD state=RETRY; durable request retained", error);
        }
    }

    public Map<String, Object> status() {
        return JsonUtils.object(
                "Code",
                "0000",
                "Data",
                JsonUtils.object(
                        "fixConnected", receiver != null && receiver.loggedOn(),
                        "storageFailed", receiver != null && receiver.storageFailed(),
                        "quoteSvrOnline", transport != null && transport.online()));
    }

    @Override
    public void stop() throws LifecycleException {
        if (receiver != null) {
            receiver.close();
        }
        try {
            if (inbox != null) {
                inbox.close();
            }
        } catch (Exception error) {
            LOGGER.error("INBOX state=FAILED during close", error);
            throw new LifecycleException(error);
        }
        super.stop();
    }
}
