package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.ReferenceData;
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
        if ("stc".equals(mode(configuration, "identity.mode", "simulator"))) {
            return new StcLoginSessionClient(network, configuration);
        }
        if (hostsSimulator) {
            return new LocalLoginSimulator();
        }
        return new SimulatorLoginSvrClient(network, configuration.getProperty("identity.serverKey", "SERVER.LoginSvr"),
                configuration.getProperty("identity.username", "search"), configuration.getProperty("identity.password", "trader1"),
                Long.parseLong(configuration.getProperty("identity.requestTimeoutMillis", "3000")));
    }

    public static ReferenceData references(Properties configuration, NetClientWrapper network) throws Exception {
        BaseDataClient base = "stc".equals(mode(configuration, "base.mode", "simulator"))
                ? new StcBaseDataClient(transport(network, configuration, "base", "SERVER.BaseSvr"), configuration)
                : new SimulatorBaseDataClient();
        CrmClient crm = crm(configuration, network);
        try {
            base.start();
            return new ReferenceData(base, crm);
        } catch (Exception error) {
            base.close();
            crm.close();
            throw error;
        }
    }

    private static StcTransport transport(NetClientWrapper network, Properties configuration, String prefix, String serverKey) {
        long timeout = Long.parseLong(configuration.getProperty(prefix + ".requestTimeoutMillis", "3000"));
        if (timeout < 100 || timeout > 10000) {
            throw new IllegalArgumentException(prefix + ".requestTimeoutMillis must be between 100 and 10000");
        }
        return new NetClientTransport(network, configuration.getProperty(prefix + ".serverKey", serverKey), timeout);
    }

    public static CrmClient crm(Properties configuration, NetClientWrapper network) throws Exception {
        String mode = configuration.getProperty("crm.mode", "simulator");
        CrmClient client;
        switch (mode) {
            case "simulator" -> client = new SimulatorCrmClient();
            case "empty" -> client = new EmptyCrmClient();
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
                    loader.close();
                    throw error;
                }
            }
            default -> throw new IllegalArgumentException("crm.mode must be simulator, empty or custom");
        }
        client.initialize(network, configuration);
        return client;
    }

    public static String secret(Properties configuration, String key) {
        String variable = configuration.getProperty(key, "");
        String value = variable.isBlank() ? null : System.getenv(variable);
        if (value == null || value.isBlank()) {
            throw new IllegalStateException("Configure an environment variable through " + key);
        }
        return value;
    }

    private record LoadedCrmClient(CrmClient delegate, URLClassLoader loader) implements CrmClient {
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
