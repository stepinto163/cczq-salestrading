package sc.app.stc.parser.utils;

import com.alibaba.fastjson.JSONObject;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

/** Logs elapsed initialization time, not native-library row progress. */
public final class ParserInitializationLog {
    private static final Logger LOG = LoggerFactory.getLogger("ficc.startup");

    private ParserInitializationLog() {
    }

    public static void run(String component, String implementation, String target, Runnable action) {
        long started = System.nanoTime();
        ScheduledExecutorService progress = Executors.newSingleThreadScheduledExecutor(task -> {
            Thread thread = new Thread(task, "parser-initialization-progress");
            thread.setDaemon(true);
            return thread;
        });
        log(component, implementation, target, "STARTING", started, null);
        progress.scheduleAtFixedRate(() ->
                log(component, implementation, target, "LOADING", started, null),
                30, 30, TimeUnit.SECONDS);
        try {
            action.run();
            progress.shutdownNow();
            log(component, implementation, target, "INITIALIZED", started, null);
        } catch (RuntimeException | Error error) {
            progress.shutdownNow();
            log(component, implementation, target, "FAILED", started, error);
            throw error;
        } finally {
            progress.shutdownNow();
        }
    }

    private static void log(String component, String implementation, String target,
            String state, long started, Throwable error) {
        JSONObject details = new JSONObject(true);
        details.put("service", "ParserSvr");
        details.put("component", component);
        details.put("state", state);
        details.put("mode", "stc");
        details.put("implementation", implementation);
        details.put("target", target);
        details.put("elapsedMillis", (System.nanoTime() - started) / 1000000);
        if (error == null) {
            LOG.info("COMPONENT {}", details.toJSONString());
        } else {
            details.put("errorType", error.getClass().getName());
            LOG.error("COMPONENT " + details.toJSONString(), error);
        }
    }
}
