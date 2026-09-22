package ficc.quote;

import com.alibaba.fastjson2.JSONObject;

/** Monotonic local durations, correlated by the existing business requestId. */
final class QuotePerformanceTrace {
    private static final boolean ENABLED = Boolean.getBoolean("ficc.performance.trace");
    private static final ThreadLocal<QuotePerformanceTrace> CURRENT = new ThreadLocal<>();
    final long start = System.nanoTime();
    long lockRequested;
    long lockAcquired;
    long businessFinished;
    private long searchNanos;
    private int searchCalls;

    static QuotePerformanceTrace begin(String topic) {
        if (!ENABLED || !topic.startsWith("ficc.quote.view.")) {
            return null;
        }
        QuotePerformanceTrace trace = new QuotePerformanceTrace();
        CURRENT.set(trace);
        return trace;
    }

    static long searchStart() {
        return CURRENT.get() == null ? 0 : System.nanoTime();
    }

    static void searchFinished(long started) {
        QuotePerformanceTrace trace = CURRENT.get();
        if (trace != null && started != 0) {
            trace.searchNanos += System.nanoTime() - started;
            trace.searchCalls++;
        }
    }

    void finish(String topic, String requestId) {
        long end = System.nanoTime();
        CURRENT.remove();
        JSONObject result = new JSONObject();
        result.put("node", "QuoteSvr");
        result.put("requestId", requestId);
        result.put("topic", topic);
        result.put("durationMs", (end - start) / 1_000_000.0);
        result.put(
                "lockWaitMs",
                lockAcquired == 0 ? null : (lockAcquired - lockRequested) / 1_000_000.0);
        result.put(
                "businessMs",
                businessFinished == 0 ? null : (businessFinished - lockAcquired) / 1_000_000.0);
        result.put("searchMs", searchNanos / 1_000_000.0);
        result.put("searchCalls", searchCalls);
        System.out.println("FICC_PERF " + result.toJSONString());
    }
}
