package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.ComponentLog;
import ficc.common.ReferenceData;

import sc.app.stc.net.NetClientWrapper;

import java.net.URLClassLoader;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.List;
import java.util.Objects;
import java.util.Properties;

public final class AdapterFactory {
    private AdapterFactory() {}

    public static String mode(Properties configuration, String key, String fallback) {
        String mode = configuration.getProperty(key, fallback);
        if (!Arrays.asList("simulator", "stc").contains(mode)) {
            throw new IllegalArgumentException(key + " must be simulator or stc");
        }
        return mode;
    }

    public static LoginSessionClient identity(
            Properties configuration, NetClientWrapper network, boolean hostsSimulator) {
        String selectedMode = configuration.getProperty("identity.mode", "simulator");
        String implementation =
                "stc".equals(selectedMode)
                        ? "StcLoginSessionClient"
                        : hostsSimulator ? "LocalLoginSimulator" : "SimulatorLoginSvrClient";
        try {
            return ComponentLog.initialize(
                    configuration,
                    "IDENTITY",
                    selectedMode,
                    implementation,
                    hostsSimulator && "simulator".equals(selectedMode)
                            ? "local"
                            : configuration.getProperty(
                                    "identity.serverKey",
                                    "stc".equals(selectedMode)
                                            ? "SERVER.LoginSvr"
                                            : "SERVER.QuoteSvr"),
                    () -> createIdentity(configuration, network, hostsSimulator));
        } catch (RuntimeException error) {
            throw error;
        } catch (Exception error) {
            throw new IllegalStateException(error);
        }
    }

    private static LoginSessionClient createIdentity(
            Properties configuration, NetClientWrapper network, boolean hostsSimulator) {
        if ("stc".equals(mode(configuration, "identity.mode", "simulator"))) {
            return new StcLoginSessionClient(network, configuration);
        }
        if (hostsSimulator) {
            return new LocalLoginSimulator();
        }
        return new SimulatorLoginSvrClient(
                network,
                configuration.getProperty("identity.serverKey", "SERVER.QuoteSvr"),
                Long.parseLong(configuration.getProperty("identity.requestTimeoutMillis", "3000")));
    }

    public static ReferenceData references(Properties configuration, NetClientWrapper network)
            throws Exception {
        CrmClient crm = crm(configuration, network);
        try {
            String selectedMode = configuration.getProperty("base.mode", "simulator");
            BaseDataClient base =
                    ComponentLog.initialize(
                            configuration,
                            "BASE_DATA",
                            selectedMode,
                            "stc".equals(selectedMode)
                                    ? "StcBaseDataClient"
                                    : "SimulatorBaseDataClient",
                            "stc".equals(selectedMode)
                                    ? configuration.getProperty("base.serverKey", "SERVER.BaseSvr")
                                    : "local",
                            () -> {
                                BaseDataClient client =
                                        "stc".equals(mode(configuration, "base.mode", "simulator"))
                                                ? new StcBaseDataClient(network, configuration)
                                                : new SimulatorBaseDataClient();
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

    public static CrmClient crm(Properties configuration, NetClientWrapper network)
            throws Exception {
        String selectedMode = configuration.getProperty("crm.mode", "simulator");
        String implementation;
        if ("postgresql".equals(selectedMode)) {
            implementation = "PostgresCrmClient";
        } else if ("stc".equals(selectedMode)) {
            implementation = "StcCrmClient";
        } else if ("simulator".equals(selectedMode)) {
            implementation = "SimulatorCrmClient";
        } else if ("empty".equals(selectedMode)) {
            implementation = "EmptyCrmClient";
        } else if ("custom".equals(selectedMode)) {
            implementation = configuration.getProperty("crm.adapterClass", "<missing>");
        } else {
            implementation = "<invalid mode>";
        }
        String target =
                "postgresql".equals(selectedMode)
                        ? ComponentLog.databaseTarget(configuration, "crm.jdbcUrl", "crm.schema")
                        : "stc".equals(selectedMode)
                                ? configuration.getProperty("crm.serverKey", "SERVER.CRMSvr")
                                : "custom".equals(selectedMode)
                                        ? configuration.getProperty(
                                                "crm.adapterJar", "<missing>")
                                        : "local";
        return ComponentLog.initialize(
                configuration,
                "CRM",
                selectedMode,
                implementation,
                target,
                () -> createCrm(configuration, network));
    }

    private static CrmClient createCrm(Properties configuration, NetClientWrapper network)
            throws Exception {
        String mode = configuration.getProperty("crm.mode", "simulator");
        CrmClient client;
        switch (mode) {
            case "simulator":
                client = new SimulatorCrmClient();
                break;
            case "empty":
                client = new EmptyCrmClient();
                break;
            case "postgresql":
                client = new PostgresCrmClient();
                break;
            case "stc":
                client = new StcCrmClient();
                break;
            case "custom":
                {
                    String className = configuration.getProperty("crm.adapterClass", "");
                    String jar = configuration.getProperty("crm.adapterJar", "");
                    if (className.trim().isEmpty() || jar.trim().isEmpty()) {
                        throw new IllegalArgumentException(
                                "crm.adapterClass and crm.adapterJar are required");
                    }
                    URLClassLoader loader =
                            new URLClassLoader(
                                    new java.net.URL[] {
                                        Paths.get(jar).toAbsolutePath().toUri().toURL()
                                    },
                                    CrmClient.class.getClassLoader());
                    try {
                        Class<? extends CrmClient> type =
                                loader.loadClass(className).asSubclass(CrmClient.class);
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
            default:
                throw new IllegalArgumentException(
                        "crm.mode must be simulator, empty, postgresql, stc or custom");
        }
        client.initialize(network, configuration);
        return client;
    }

    private static final class LoadedCrmClient implements CrmClient {
        private final CrmClient delegate;
        private final URLClassLoader loader;

        private LoadedCrmClient(CrmClient delegate, URLClassLoader loader) {
            this.delegate = delegate;
            this.loader = loader;
        }

        @Override
        public String toString() {
            return "LoadedCrmClient[delegate=" + delegate + ", loader=" + loader + "]";
        }

        @Override
        public boolean equals(Object other) {
            if (this == other) return true;
            if (!(other instanceof LoadedCrmClient)) return false;
            LoadedCrmClient that = (LoadedCrmClient) other;
            return Objects.equals(delegate, that.delegate) && Objects.equals(loader, that.loader);
        }

        @Override
        public int hashCode() {
            return Objects.hash(delegate, loader);
        }

        @Override
        public JSONObject query(
                String userId,
                String kind,
                String text,
                String institutionId,
                int offset,
                int limit) {
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
