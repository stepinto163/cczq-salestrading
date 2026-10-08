package ats.bbox.framework;

import cgws.ficc.framework.LifecycleActions;

import org.apache.commons.configuration.Configuration;
import org.apache.commons.configuration.PropertiesConfiguration;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.net.URL;
import java.util.Collections;

public class Server {
    private static final Logger LOGGER = LoggerFactory.getLogger("ficc.startup");
    private static volatile IServer server;
    private static Thread shutdownHook;

    public static IServer getServer() {
        return server;
    }

    public static synchronized void main(String[] arguments) {
        if (server != null && server.getStatus() != ILifecycle.STATUS.DISPOSED) {
            throw new IllegalStateException("A server is already active in this process");
        }
        SystemConfig.init(arguments);
        try {
            LifecycleActions.run(
                    Consts.SYS_NAME,
                    "FRAMEWORK",
                    Server.class.getName(),
                    "STARTING",
                    "INITIALIZED",
                    () -> start(arguments));
        } catch (Exception | LinkageError error) {
            throw new IllegalStateException("Service startup failed: " + Consts.SYS_NAME, error);
        }
    }

    private static void start(String[] arguments) throws Exception {
        // Duplicated classes can otherwise silently select the old runtime by classpath order.
        java.util.List<URL> providers =
                Collections.list(
                        Server.class
                                .getClassLoader()
                                .getResources("ats/bbox/framework/Server.class"));
        if (providers.size() != 1) {
            throw new IllegalStateException(
                    "Expected one framework provider; found " + providers.size());
        }
        String file =
                SystemConfig.getConfig()
                        .getString(
                                SystemConfig.SERVER_CFG_KEY,
                                SystemConfig.getConfig()
                                        .getString(
                                                SystemConfig.PARA_PREFIX + "Cfg",
                                                Consts.getDefaultServCfg()));
        Configuration configuration = new PropertiesConfiguration(file);
        // These features require separate compatibility work; never silently ignore them.
        for (String pending : new String[] {"monitor.file", "encryption.file"}) {
            if (configuration.containsKey(pending)) {
                throw new IllegalStateException("Compatibility not yet implemented for " + pending);
            }
        }
        String aliases = configuration.getString("inner.file", "./control/inner.ini");
        if (new File(aliases).isFile()) {
            InnerComponent.load(new PropertiesConfiguration(aliases));
        }
        server = new ServerImpl(configuration);
        server.init();
        server.start();
        if (shutdownHook == null) {
            shutdownHook =
                    new Thread(
                            () -> {
                                IServer active = server;
                                if (active != null) {
                                    try {
                                        active.stop();
                                    } catch (LifecycleException error) {
                                        LOGGER.error("Service shutdown failed", error);
                                    }
                                }
                            },
                            "ficc-shutdown");
            Runtime.getRuntime().addShutdownHook(shutdownHook);
        }
    }
}
