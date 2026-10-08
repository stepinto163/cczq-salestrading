package cgws.ficc.framework;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/** Executes lifecycle transitions without creating worker threads or changing their order. */
public final class LifecycleActions {
    private static final Logger LOGGER = LoggerFactory.getLogger("ficc.startup");

    private LifecycleActions() {}

    @FunctionalInterface
    public interface Action {
        void run() throws Exception;
    }

    public static void run(
            String service,
            String component,
            String implementation,
            String before,
            String after,
            Action action)
            throws Exception {
        run(service, component, implementation, "local", "local", before, after, action);
    }

    public static void run(
            String service,
            String component,
            String implementation,
            String mode,
            String target,
            String before,
            String after,
            Action action)
            throws Exception {
        long started = System.nanoTime();
        LOGGER.info(message(service, component, implementation, mode, target, before, 0));
        try {
            action.run();
            LOGGER.info(
                    message(
                            service,
                            component,
                            implementation,
                            mode,
                            target,
                            after,
                            elapsed(started)));
        } catch (Exception | Error error) {
            // Do not log configuration values. Preserve the original failure for the caller.
            LOGGER.error(
                    message(
                            service,
                            component,
                            implementation,
                            mode,
                            target,
                            "FAILED",
                            elapsed(started)),
                    error);
            throw error;
        }
    }

    private static long elapsed(long started) {
        return (System.nanoTime() - started) / 1_000_000;
    }

    private static String message(
            String service,
            String component,
            String implementation,
            String mode,
            String target,
            String state,
            long elapsedMillis) {
        return "COMPONENT {\"service\":\""
                + escape(service)
                + "\",\"component\":\""
                + escape(component)
                + "\",\"state\":\""
                + state
                + "\",\"mode\":\""
                + escape(mode)
                + "\",\"implementation\":\""
                + escape(implementation)
                + "\",\"target\":\""
                + escape(target)
                + "\",\"elapsedMillis\":"
                + elapsedMillis
                + "}";
    }

    private static String escape(String value) {
        if (value == null) {
            return "";
        }
        return value.replace("\\", "\\\\")
                .replace("\"", "\\\"")
                .replace("\r", "\\r")
                .replace("\n", "\\n")
                .replace("\t", "\\t");
    }
}
