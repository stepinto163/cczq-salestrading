package ats.app.ym.utils.logger;

import org.apache.commons.configuration.PropertiesConfiguration;
import org.apache.log4j.PropertyConfigurator;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.util.Map;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

/** Preserves log4j settings without the old asynchronous utility framework. */
public final class AsyncSlf4jLoggerFactory {
    private static final Map<Logger, AsyncSlf4jLogger> LOGGERS = new ConcurrentHashMap<>();
    private static volatile ThreadPoolExecutor executor;

    private AsyncSlf4jLoggerFactory() {}

    public static synchronized void initialize(PropertiesConfiguration configuration) {
        String file = configuration.getString("log4j.file");
        if (file == null || !new File(file).isFile()) {
            throw new IllegalStateException("log4j.file must name an existing file");
        }
        PropertyConfigurator.configureAndWatch(file, 1000L);
        AsyncLogger.setWriteTime(configuration.getBoolean("log4j.writeTime", false));
        if (!configuration.getBoolean("log4j.async", true) || executor != null) {
            return;
        }
        int workers = configuration.getInt("log4j.thread", 10);
        int queueSize = configuration.getInt("log4j.size", 10);
        if (workers <= 0 || queueSize <= 0) {
            throw new IllegalArgumentException("log4j.thread and log4j.size must be positive");
        }
        executor =
                new ThreadPoolExecutor(
                        workers,
                        workers,
                        0,
                        TimeUnit.MILLISECONDS,
                        new ArrayBlockingQueue<>(Math.multiplyExact(workers, queueSize)),
                        task -> {
                            Thread worker = new Thread(task, "ficc-log-writer");
                            worker.setDaemon(true);
                            return worker;
                        },
                        (task, pool) -> task.run());
        // Saturation applies backpressure instead of silently discarding errors.
        Runtime.getRuntime()
                .addShutdownHook(new Thread(AsyncSlf4jLoggerFactory::flush, "ficc-log-flush"));
    }

    public static Logger getLogger(Class<?> type) {
        Logger logger = LoggerFactory.getLogger(type);
        return executor == null ? logger : LOGGERS.computeIfAbsent(logger, AsyncSlf4jLogger::new);
    }

    public static void asyncLog(
            AsyncSlf4jLogger logger,
            String format,
            int level,
            Throwable error,
            Object[] arguments,
            long time,
            boolean asynchronous) {
        Runnable write =
                () -> write(logger.getSlf4jLogger(), format, level, error, arguments, time);
        ThreadPoolExecutor current = executor;
        if (asynchronous && current != null && !current.isShutdown()) {
            current.execute(write);
        } else {
            write.run();
        }
    }

    private static void write(
            Logger logger,
            String format,
            int level,
            Throwable error,
            Object[] arguments,
            long time) {
        switch (level) {
            case 10000:
                AsyncLogger.debug(logger, format, error, arguments, time);
                break;
            case 20000:
                AsyncLogger.info(logger, format, error, arguments, time);
                break;
            case 30000:
                AsyncLogger.warn(logger, format, error, arguments, time);
                break;
            case 40000:
                AsyncLogger.error(logger, format, error, arguments, time);
                break;
            default:
                throw new IllegalArgumentException("Unsupported legacy log level: " + level);
        }
    }

    public static void flush() {
        ThreadPoolExecutor current = executor;
        if (current == null) {
            return;
        }
        current.shutdown();
        try {
            if (!current.awaitTermination(5, TimeUnit.SECONDS)) {
                LoggerFactory.getLogger("ficc.startup")
                        .error("LOGGING state=FAILED reason=FLUSH_TIMEOUT");
            }
        } catch (InterruptedException interrupted) {
            Thread.currentThread().interrupt();
        }
    }
}
