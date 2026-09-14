package ficc.gateway;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import com.gateway.connector.proto.Proto;
import java.util.concurrent.ConcurrentHashMap;

/** Opt-in benchmark instrumentation; never changes the DPS message body. */
final class PerformanceTrace {
    private static final boolean ENABLED = Boolean.getBoolean("ficc.performance.trace");
    private static final ConcurrentHashMap<String, Pending> PENDING = new ConcurrentHashMap<>();

    private record Pending(String requestId, String topic, long start) {}

    private static String key(Proto message) {
        return message.getSessionId() + ":" + message.getSeq();
    }

    static void begin(Proto message, String topic, String content) {
        if (!ENABLED || !topic.startsWith("ficc.quote.view.")) {
            return;
        }
        long start = System.nanoTime();
        PENDING.entrySet().removeIf(entry -> start - entry.getValue().start() > 30_000_000_000L);
        if (PENDING.size() >= 1000) {
            return;
        }
        try {
            String requestId = JSON.parseObject(content).getString("requestId");
            PENDING.put(key(message), new Pending(requestId, topic, start));
        } catch (RuntimeException ignored) {
            // Invalid requests still go through the normal gateway validation path.
        }
    }

    static void finish(Object body) {
        if (!ENABLED || !(body instanceof Proto message)) {
            return;
        }
        Pending pending = PENDING.remove(key(message));
        if (pending == null) {
            return;
        }
        double elapsed = (System.nanoTime() - pending.start()) / 1_000_000.0;
        JSONObject result = new JSONObject(true);
        result.put("node", "GW");
        result.put("requestId", pending.requestId());
        result.put("topic", pending.topic());
        result.put("durationMs", elapsed);
        result.put("replyBytes", message.getBody() == null ? 0 : message.getBody().length);
        System.out.println("FICC_PERF " + result.toJSONString());
    }
}
