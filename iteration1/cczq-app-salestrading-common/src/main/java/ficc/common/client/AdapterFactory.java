package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.ReferenceData;
import ficc.common.ComponentLog;
import sc.app.stc.net.NetClientWrapper;
import java.net.URLClassLoader;
import java.nio.file.Path;
import java.util.List;
import java.util.Properties;

public final class AdapterFactory {
    private AdapterFactory() {}

    public static String mode(Properties configuration, String key, String fallback) {
        String mode = configuration.getProperty(key, fallback);
        if (!List.of("simulator", "stc").contains(mode)) {
            throw new IllegalArgumentException(key + " must be simulator or stc");
        }
        return mode;
    }

    public static LoginSessionClient identity(Properties configuration, NetClientWrapper network, boolean hostsSimulator) {
        String selectedMode = configuration.getProperty("identity.mode", "simulator");
        String implementation = "stc".equals(selectedMode) ? "StcLoginSessionClient"
                : hostsSimulator ? "LocalLoginSimulator" : "SimulatorLoginSvrClient";
        try {
            return ComponentLog.initialize(configuration, "IDENTITY", selectedMode, implementation,
                    hostsSimulator && "simulator".equals(selectedMode) ? "local"
                            : configuration.getProperty("identity.serverKey",
                                    "stc".equals(selectedMode) ? "SERVER.LoginSvr" : "SERVER.QuoteSvr"),
                    () -> createIdentity(configuration, network, hostsSimulator));
        } catch (RuntimeException error) {
            throw error;
        } catch (Exception error) {
            throw new IllegalStateException(error);
        }
    }

    private static LoginSessionClient createIdentity(Properties configuration, NetClientWrapper network, boolean hostsSimulator) {
        if ("stc".equals(mode(configuration, "identity.mode", "simulator"))) {
            return new StcLoginSessionClient(network, configuration);
        }
        if (hostsSimulator) {
            return new LocalLoginSimulator();
        }
        return new SimulatorLoginSvrClient(network, configuration.getProperty("identity.serverKey", "SERVER.QuoteSvr"),
                Long.parseLong(configuration.getProperty("identity.requestTimeoutMillis", "3000")));
    }

    public static ReferenceData references(Properties configuration, NetClientWrapper network) throws Exception {
        CrmClient crm = crm(configuration, network);
        try {
            String selectedMode = configuration.getProperty("base.mode", "simulator");
            BaseDataClient base = ComponentLog.initialize(configuration, "BASE_DATA", selectedMode,
                    "stc".equals(selectedMode) ? "StcBaseDataClient" : "SimulatorBaseDataClient",
                    "stc".equals(selectedMode) ? configuration.getProperty("base.serverKey", "SERVER.BaseSvr") : "local",
                    () -> {
                        BaseDataClient client = "stc".equals(mode(configuration, "base.mode", "simulator"))
                                ? new StcBaseDataClient(network, configuration) : new SimulatorBaseDataClient();
                        try {
                            client.start();
                            return client;
                        } catch (Exception error) {
                            try {
                                client.close();
                            } catch (Exception cleanupError) {
                                error.addSuppressed(cleanupError);
                            }
                            throw error;
                        }
                    });
            return new ReferenceData(base, crm);
        } catch (Exception error) {
            try {
                crm.close();
            } catch (Exception cleanupError) {
                error.addSuppressed(cleanupError);
            }
            throw error;
        }
    }

    public static CrmClient crm(Properties configuration, NetClientWrapper network) throws Exception {
        String selectedMode = configuration.getProperty("crm.mode", "simulator");
        String implementation = switch (selectedMode) {
            case "postgresql" -> "PostgresCrmClient";
            case "simulator" -> "SimulatorCrmClient";
            case "empty" -> "EmptyCrmClient";
            case "custom" -> configuration.getProperty("crm.adapterClass", "<missing>");
            default -> "<invalid mode>";
        };
        String target = "postgresql".equals(selectedMode)
                ? ComponentLog.databaseTarget(configuration, "crm.jdbcUrl", "crm.schema")
                : "custom".equals(selectedMode) ? configuration.getProperty("crm.adapterJar", "<missing>") : "local";
        return ComponentLog.initialize(configuration, "CRM", selectedMode, implementation, target,
                () -> createCrm(configuration, network));
    }

    private static CrmClient createCrm(Properties configuration, NetClientWrapper network) throws Exception {
        String mode = configuration.getProperty("crm.mode", "simulator");
        CrmClient client;
        switch (mode) {
            case "simulator" -> client = new SimulatorCrmClient();
            case "empty" -> client = new EmptyCrmClient();
            case "postgresql" -> client = new PostgresCrmClient();
            case "custom" -> {
                String className = configuration.getProperty("crm.adapterClass", "");
                String jar = configuration.getProperty("crm.adapterJar", "");
                if (className.isBlank() || jar.isBlank()) {
                    throw new IllegalArgumentException("crm.adapterClass and crm.adapterJar are required");
                }
                URLClassLoader loader = new URLClassLoader(
                        new java.net.URL[] {Path.of(jar).toAbsolutePath().toUri().toURL()}, CrmClient.class.getClassLoader());
                try {
                    Class<? extends CrmClient> type = loader.loadClass(className).asSubclass(CrmClient.class);
                    CrmClient delegate = type.getConstructor().newInstance();
                    delegate.initialize(network, configuration);
                    return new LoadedCrmClient(delegate, loader);
                } catch (Exception error) {
                    try {
                        loader.close();
                    } catch (Exception cleanupError) {
                        error.addSuppressed(cleanupError);
                    }
                    throw error;
                }
            }
            default -> throw new IllegalArgumentException("crm.mode must be simulator, empty, postgresql or custom");
        }
        client.initialize(network, configuration);
        return client;
    }

    private record LoadedCrmClient(CrmClient delegate, URLClassLoader loader) implements CrmClient {
        @Override
        public JSONObject query(String userId, String kind, String text, String institutionId, int offset, int limit) {
            return delegate.query(userId, kind, text, institutionId, offset, limit);
        }
        @Override
        public List<JSONObject> institutions(String userId) {
            return delegate.institutions(userId);
        }
        @Override
        public List<JSONObject> contacts(String userId) {
            return delegate.contacts(userId);
        }
        @Override
        public JSONObject institution(String userId, String institutionId) {
            return delegate.institution(userId, institutionId);
        }
        @Override
        public JSONObject contact(String userId, String contactId) {
            return delegate.contact(userId, contactId);
        }
        @Override
        public void close() throws Exception {
            try {
                delegate.close();
            } finally {
                loader.close();
            }
        }
    }
}
