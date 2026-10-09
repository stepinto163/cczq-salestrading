package sc.app.stc.parser.utils;

import cgws.ficc.parser.model.AllQuote;
import cgws.ficc.parser.parse.BondQuoteEngine;
import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import java.time.LocalDate;
import java.util.UUID;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.RejectedExecutionException;
import java.util.function.Supplier;

/**
 * Java 主路径与 C++ 对照路径的组合：调用方始终获得主引擎结果，native 在隔离工作进程中对照。
 * 先序列化 Java 结果留证，避免随后 CRM 补全修改对象而污染比较基准。
 * 对照队列有界且单线程执行；初始化失败或队列满会记录日志，不阻塞等待 native 后再返回报价。
 */
public final class ComparativeBondQuoteEngine implements BondQuoteEngine, AutoCloseable {
    private static final Logger LOG = LoggerFactory.getLogger("ficc.parser.compare");
    private final BondQuoteEngine primary;
    private final ThreadPoolExecutor comparisons;
    private volatile BondQuoteEngine shadow;
    private volatile Throwable shadowFailure;

    ComparativeBondQuoteEngine(BondQuoteEngine primary, Supplier<BondQuoteEngine> shadowFactory) {
        this.primary = primary;
        comparisons = new ThreadPoolExecutor(1, 1, 0, TimeUnit.MILLISECONDS,
                new ArrayBlockingQueue<Runnable>(1024), task -> {
                    Thread thread = new Thread(task, "parser-native-comparison");
                    thread.setDaemon(true);
                    return thread;
                });
        comparisons.execute(() -> {
            try {
                shadow = shadowFactory.get();
            } catch (Throwable error) {
                shadowFailure = error;
                LOG.error("Native comparison initialization failed; Java remains authoritative", error);
            }
        });
    }

    @Override
    public AllQuote parse(String text, LocalDate date) {
        String id = UUID.randomUUID().toString();
        event(id, "java", "INPUT", text, null, null);
        event(id, "native", "INPUT", text, null, null);
        String javaOutput = null;
        try {
            AllQuote reply = primary.parse(text, date);
            // Capture before CRM post-processing can mutate the returned object.
            javaOutput = JSON.toJSONString(reply);
            event(id, "java", "OUTPUT", null, JSON.parse(javaOutput), null);
            return reply;
        } catch (RuntimeException | Error error) {
            event(id, "java", "FAILED", null, null, error);
            throw error;
        } finally {
            final String expected = javaOutput;
            submit(id, () -> compare(id, text, date, expected));
        }
    }

    private void compare(String id, String text, LocalDate date, String expected) {
        try {
            if (shadowFailure != null || shadow == null) {
                throw new IllegalStateException("Native comparison engine unavailable", shadowFailure);
            }
            String actual = JSON.toJSONString(shadow.parse(text, date));
            event(id, "native", "OUTPUT", null, JSON.parse(actual), null);
            boolean matches = expected != null && JSON.parse(expected).equals(JSON.parse(actual));
            event(id, "comparison", matches ? "MATCH" : "DIFFERENT", null, null, null);
        } catch (Throwable error) {
            event(id, "native", "FAILED", null, null, error);
        }
    }

    private void submit(String id, Runnable task) {
        try {
            comparisons.execute(task);
        } catch (RejectedExecutionException error) {
            // Input was already logged in full and can be replayed. Never hide an unprocessed comparison.
            event(id, "native", "QUEUE_FULL_OR_CLOSED", null, null, error);
        }
    }

    @Override
    public void reload() {
        primary.reload();
        submit("refresh", () -> {
            try {
                if (shadow == null) {
                    throw new IllegalStateException("Native comparison engine unavailable", shadowFailure);
                }
                shadow.reload();
            } catch (Throwable error) {
                event("refresh", "native", "FAILED", null, null, error);
            }
        });
    }

    private static void event(String id, String engine, String state, String input, Object output, Throwable error) {
        JSONObject entry = new JSONObject(true);
        entry.put("service", "ParserSvr");
        entry.put("comparisonId", id);
        entry.put("engine", engine);
        entry.put("state", state);
        if (input != null) {
            entry.put("input", input);
        }
        if (output != null) {
            entry.put("output", output);
        }
        if (error == null) {
            LOG.info("PARSER_COMPARE {}", entry.toJSONString());
        } else {
            LOG.error("PARSER_COMPARE " + entry.toJSONString(), error);
        }
    }

    @Override
    public void close() {
        comparisons.shutdownNow();
        if (shadow instanceof AutoCloseable) {
            try {
                ((AutoCloseable) shadow).close();
            } catch (Exception error) {
                LOG.error("Native comparison close failed", error);
            }
        }
    }
}
