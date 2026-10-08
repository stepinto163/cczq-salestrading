package sc.app.stc.parser.utils;

import cgws.ficc.parser.parse.BondQuoteEngine;
import cgws.ficc.parser.parse.JavaBondQuoteEngine;
import com.alibaba.fastjson.JSONObject;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import sc.app.stc.utils.CommonDataUtils;
import java.util.Locale;
import java.util.function.Supplier;

/** Explicit selection: a failed Java engine never silently falls back to native parsing. */
public final class ParserEngineFactory {
    private static final Logger LOG = LoggerFactory.getLogger("ficc.startup");

    private ParserEngineFactory() {
    }

    public static BondQuoteEngine create(String configuredMode) {
        BondQuoteEngine engine = create(configuredMode,
                () -> new JavaBondQuoteEngine(new JdbcParserReferenceLoader(
                        () -> CommonDataUtils.dbPoolResource.getConnection()), true),
                NativeBondQuoteEngine::new);
        if ("native".equalsIgnoreCase(configuredMode)) {
            return engine;
        }
        return new ComparativeBondQuoteEngine(engine, () -> {
            long started = System.nanoTime();
            log("native-shadow", "STARTING", started, null);
            try {
                BondQuoteEngine shadow = new NativeShadowProcess();
                log("native-shadow", "INITIALIZED", started, null);
                return shadow;
            } catch (RuntimeException | Error error) {
                log("native-shadow", "FAILED", started, error);
                throw error;
            }
        });
    }

    static BondQuoteEngine create(String configuredMode, Supplier<BondQuoteEngine> javaEngine,
            Supplier<BondQuoteEngine> nativeEngine) {
        String mode = configuredMode == null || configuredMode.trim().isEmpty()
                ? "java" : configuredMode.trim().toLowerCase(Locale.ROOT);
        long started = System.nanoTime();
        log(mode, "STARTING", started, null);
        try {
            BondQuoteEngine engine;
            if ("java".equals(mode)) {
                engine = javaEngine.get();
            } else if ("native".equals(mode)) {
                engine = nativeEngine.get();
            } else {
                throw new IllegalArgumentException("ParserEngine must be java or native");
            }
            if (engine == null) {
                throw new IllegalStateException("Parser engine initialization returned null");
            }
            log(mode, "INITIALIZED", started, null);
            return engine;
        } catch (RuntimeException | Error error) {
            log(mode, "FAILED", started, error);
            throw error;
        }
    }

    private static void log(String mode, String state, long started, Throwable error) {
        JSONObject message = new JSONObject(true);
        message.put("service", "ParserSvr");
        message.put("component", "native-shadow".equals(mode) ? "NATIVE_COMPARISON" : "QUOTE_PARSER");
        message.put("state", state);
        message.put("mode", mode);
        message.put("implementation", "java".equals(mode) ? "JavaBondQuoteEngine"
                : "native-shadow".equals(mode) ? "NativeShadowProcess" : "NativeBondQuoteEngine");
        message.put("target", Consts.dbpoolDefault);
        message.put("elapsedMillis", (System.nanoTime() - started) / 1000000);
        if (error == null) {
            LOG.info("COMPONENT {}", message.toJSONString());
        } else {
            message.put("errorType", error.getClass().getName());
            LOG.error("COMPONENT " + message.toJSONString(), error);
        }
    }
}
