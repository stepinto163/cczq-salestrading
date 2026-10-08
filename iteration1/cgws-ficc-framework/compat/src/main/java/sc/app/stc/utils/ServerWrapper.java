package sc.app.stc.utils;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import ats.bbox.framework.IServer;
import ats.bbox.framework.Server;
import ats.bbox.framework.ServerShell;
import ats.foundation.utils.config.ATSROOTConfig;
import ats.foundation.utils.config.PropertiesHelper;

import cgws.ficc.framework.LifecycleActions;
import cgws.ficc.framework.compat.ServiceSettings;
import cgws.ficc.framework.dps.ServiceEndpoint;
import cgws.ficc.framework.dps.ZookeeperDiscovery;

import org.apache.commons.configuration.PropertiesConfiguration;
import org.apache.log4j.BasicConfigurator;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/** Entry-point compatibility: application sources retain ServerWrapper.Start(name, ini). */
public final class ServerWrapper {
    private static final Logger LOGGER = LoggerFactory.getLogger("ficc.startup");
    private static ZookeeperDiscovery discovery;
    private static ZookeeperDiscovery.Registration registration;
    private static final Object lifecycleLock = new Object();
    private static volatile boolean discoveryFailed;

    private ServerWrapper() {}

    public static synchronized void Start(String name, String configurationFile) throws Exception {
        discoveryFailed = false;
        BasicConfigurator.configure();
        PropertiesConfiguration configuration = new PropertiesConfiguration(configurationFile);
        AsyncSlf4jLoggerFactory.initialize(configuration);
        ats.bbox.framework.Consts.SYS_NAME = name;
        ats.bbox.framework.Consts.isShowActiveThread = false;
        LifecycleActions.run(
                name,
                "SERVICE_HOST",
                ServerWrapper.class.getName(),
                "STARTING",
                "STARTED",
                () -> start(name, configurationFile, configuration));
    }

    private static void start(
            String name, String configurationFile, PropertiesConfiguration configuration)
            throws Exception {
        ServiceSettings settings = ServiceSettings.load(configuration.getString("serverKey"));
        sc.app.stc.net.NetClientWrapper.defaultClientName = settings.name;
        PropertiesHelper global = ATSROOTConfig.getATSROOTConfig();
        String cluster = global.getProperty(settings.key + ".Cluster.ClusterName", "ZK");
        if (!"ZK".equalsIgnoreCase(cluster)) {
            throw new IllegalStateException(
                    "Only the deployed ZK discovery mode has been migrated: " + cluster);
        }
        try {
            if (settings.register) {
                LifecycleActions.run(
                        name,
                        "DISCOVERY",
                        ZookeeperDiscovery.class.getName(),
                        "zookeeper",
                        settings.registryHosts,
                        "STARTING",
                        "REGISTERED",
                        () -> {
                            discovery =
                                    new ZookeeperDiscovery(
                                            settings.registryHosts,
                                            120000,
                                            60000,
                                            global.getProperty("REGISTER.UserName", ""),
                                            global.getProperty("REGISTER.Password", ""),
                                            ServerWrapper::stopAfterDiscoveryFailure);
                            ServiceEndpoint endpoint =
                                    new ServiceEndpoint(
                                            settings.endpoint.getHost(),
                                            settings.endpoint.getPort(),
                                            false);
                            Runnable leaseLost =
                                    () ->
                                            stopAfterDiscoveryFailure(
                                                    new IllegalStateException(
                                                            "Standby registration lease lost"));
                            registration =
                                    settings.standby
                                            ? discovery.standby(settings.name, endpoint, leaseLost)
                                            : discovery.register(
                                                    settings.name, endpoint, false, null);
                            Runtime.getRuntime()
                                    .addShutdownHook(
                                            new Thread(
                                                    ServerWrapper::shutdown,
                                                    "ficc-discovery-shutdown"));
                            if (registration.getNodePath() == null) {
                                LOGGER.info(
                                        "DISCOVERY service={} state=STANDBY_WAITING target={}",
                                        name,
                                        settings.registryHosts);
                            }
                            registration.awaitOwnership();
                        });
                String node = registration.getNodePath();
                String sequenceName = node.substring(node.lastIndexOf('/') + 1);
                com.app.common.utils.IdUtil.setWorkerId(Math.abs(sequenceName.hashCode() % 31));
            }
            synchronized (lifecycleLock) {
                if (discoveryFailed) {
                    throw new IllegalStateException("Discovery failed before service startup");
                }
                ServerShell.startServer(name, configurationFile);
                if (registration != null) {
                    registration.markReady();
                }
            }
        } catch (Exception | Error error) {
            try {
                IServer server = Server.getServer();
                if (server != null) {
                    server.stop();
                }
            } catch (Exception cleanup) {
                error.addSuppressed(cleanup);
            }
            try {
                closeDiscovery();
            } catch (Exception cleanup) {
                error.addSuppressed(cleanup);
            }
            throw error;
        }
    }

    private static void stopAfterDiscoveryFailure(Throwable failure) {
        discoveryFailed = true;
        LOGGER.error("DISCOVERY state=FAILED serviceStopRequested=true", failure);
        // Never close/join the ZooKeeper client or start/stop business code on its event thread.
        Thread shutdown = new Thread(ServerWrapper::shutdown, "ficc-discovery-failure");
        shutdown.start();
    }

    private static void shutdown() {
        synchronized (lifecycleLock) {
            try {
                IServer server = Server.getServer();
                if (server != null) {
                    server.stop();
                }
            } catch (Exception cleanup) {
                LOGGER.error("Service stop after discovery failure failed", cleanup);
            } finally {
                // Release the claim only after local application resources have stopped.
                closeDiscovery();
            }
        }
    }

    private static void closeDiscovery() {
        if (discovery != null) {
            discovery.close();
        }
    }
}
