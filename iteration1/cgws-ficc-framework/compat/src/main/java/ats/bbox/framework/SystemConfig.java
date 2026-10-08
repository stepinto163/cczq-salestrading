package ats.bbox.framework;

import org.apache.commons.configuration.BaseConfiguration;
import org.apache.commons.configuration.Configuration;

import java.util.Locale;
import java.util.Map;

public class SystemConfig {
    public static String PARA_PREFIX = Consts.SYS_NAME;
    public static final String SERVER_CFG_KEY = "serverCfg";
    private static final Configuration CONFIGURATION = new BaseConfiguration();

    public static synchronized void init(String[] arguments) {
        CONFIGURATION.clear();
        for (int index = 0; index < arguments.length; index++) {
            String key = arguments[index];
            if (!key.startsWith("-")) {
                continue;
            }
            String value =
                    index + 1 < arguments.length && !arguments[index + 1].startsWith("-")
                            ? arguments[++index]
                            : "";
            key = key.substring(1).toLowerCase(Locale.ROOT);
            if ("name".equals(key)) {
                Consts.SYS_NAME = value;
                PARA_PREFIX = value;
            } else if ("cfg".equals(key)) {
                CONFIGURATION.setProperty(SERVER_CFG_KEY, value);
            }
        }
        for (Map.Entry<String, String> entry : System.getenv().entrySet()) {
            if (entry.getKey().startsWith(PARA_PREFIX)) {
                CONFIGURATION.addProperty(entry.getKey(), entry.getValue());
            }
        }
    }

    public static Configuration getConfig() {
        return CONFIGURATION;
    }
}
