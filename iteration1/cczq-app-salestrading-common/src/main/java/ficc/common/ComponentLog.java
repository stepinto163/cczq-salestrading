package ficc.common;

import java.net.URI;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.util.Properties;
import org.apache.log4j.Logger;

/** Consistent component initialization diagnostics, separate from DPS payload logging. */
public final class ComponentLog {
    private static final Logger LOGGER = Logger.getLogger("ficc.startup");

    private ComponentLog() {}

    @FunctionalInterface
    public interface Initializer<T> {
        T initialize() throws Exception;
    }

    public static <T> T initialize(Properties configuration, String component, String mode,
            String implementation, String target, Initializer<T> initializer) throws Exception {
        long started = System.nanoTime();
        report(configuration, component, "STARTING", mode, implementation, target, 0, null);
        try {
            T result = initializer.initialize();
            report(configuration, component, "INITIALIZED", mode, implementation, target,
                    (System.nanoTime() - started) / 1000000, null);
            return result;
        } catch (Exception | LinkageError error) {
            report(configuration, component, "FAILED", mode, implementation, target,
                    (System.nanoTime() - started) / 1000000, error);
            throw error;
        }
    }

    private static void report(Properties configuration, String component, String state, String mode,
            String implementation, String target, long elapsedMillis, Throwable error) {
        var record = JsonUtils.object("service", configuration.getProperty("diagnostics.serviceName", "UNSPECIFIED"),
                "component", component, "state", state, "mode", mode, "implementation", implementation,
                "target", target, "elapsedMillis", elapsedMillis);
        if (error != null) {
            record.put("errorType", error.getClass().getName());
            record.put("reason", safeReason(configuration, error));
            StringWriter stack = new StringWriter();
            error.printStackTrace(new PrintWriter(stack));
            LOGGER.error("COMPONENT " + JsonUtils.toJson(record) + System.lineSeparator()
                    + redact(configuration, stack.toString()));
        } else {
            LOGGER.info("COMPONENT " + JsonUtils.toJson(record));
        }
    }

    static String safeReason(Properties configuration, Throwable error) {
        String reason = error.getMessage() == null ? error.getClass().getSimpleName() : error.getMessage();
        return redact(configuration, reason);
    }

    private static String redact(Properties configuration, String reason) {
        for (String key : configuration.stringPropertyNames()) {
            if (key.toLowerCase(java.util.Locale.ROOT).matches(".*(password|secret|token|jdbcurl|db.url).*")) {
                String value = configuration.getProperty(key);
                if (value != null && !value.isBlank()) {
                    reason = reason.replace(value, "<redacted>");
                }
            }
            if (key.endsWith("passwordEnv")) {
                String variable = configuration.getProperty(key);
                String value = variable.isBlank() ? null : System.getenv(variable);
                if (value != null && !value.isBlank()) {
                    reason = reason.replace(value, "<redacted>");
                }
            }
        }
        return reason;
    }

    /** Report the address and database only, never JDBC query options or user info. */
    public static String databaseTarget(Properties configuration, String urlKey, String schemaKey) {
        try {
            String url = configuration.getProperty(urlKey, "");
            URI address = URI.create(url.startsWith("jdbc:") ? url.substring(5) : url);
            if (address.getHost() != null) {
                return address.getHost() + (address.getPort() < 0 ? "" : ":" + address.getPort())
                        + address.getPath() + " schema=" + configuration.getProperty(schemaKey, "public");
            }
        } catch (IllegalArgumentException ignored) {
            // The component will report its own configuration failure.
        }
        return "<missing or nonstandard database address>";
    }
}
