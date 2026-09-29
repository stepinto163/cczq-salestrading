package ficc.quote;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.api.ProtocolBoundary;
import ficc.api.ProtocolCodec;
import ficc.api.QuoteRecord;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import ficc.common.client.BaseDataClient;
import ficc.common.client.SimulatorCrmClient;

import java.io.BufferedReader;
import java.lang.management.GarbageCollectorMXBean;
import java.lang.management.ManagementFactory;
import java.math.BigDecimal;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.time.LocalDate;
import java.util.*;
import java.util.concurrent.*;
import java.util.concurrent.atomic.AtomicInteger;

/** Explicit opt-in benchmark. The launcher enforces a separate 15-second process watchdog. */
public final class QuoteViewBenchmark {
    private static final int QUOTES = 5000;
    private static final int USERS = 10;
    private static final int SAMPLES = 3;
    private final long deadline = System.nanoTime() + TimeUnit.SECONDS.toNanos(12);
    private final AtomicInteger ids = new AtomicInteger();
    private final List<JSONObject> samples = Collections.synchronizedList(new ArrayList<>());
    private final List<JSONObject> pushes = new ArrayList<>();
    private final Map<String, Panel> panels = new LinkedHashMap<>();
    private final ThreadLocal<Publication> publication = new ThreadLocal<>();
    private final JSONObject report = JsonUtils.object("status", "RUNNING", "quotes", QUOTES,
            "users", USERS, "viewsPerUser", 4, "samplesPerSequentialScenario", SAMPLES,
            "warmupCyclesPerScenario", 1, "fixtureSeed", 20260929,
            "transportTested", false, "uiTested", false,
            "timingScope", "in-process handlers, PostgreSQL writes and encoded publication callback; no GW/network/UI",
            "tailWarning", "Three sequential samples are diagnostic only; p95/p99 may equal maximum",
            "scenarios", new JSONArray());
    private QuoteService service;
    private FixtureBase base;
    private Path output;
    private volatile String scenario;
    private volatile boolean measuring;
    private int checks;
    private ExecutorService workers;

    private static final class Panel {
        final int user;
        final int slot;
        final String id;
        final String domain;
        final JSONObject filter;
        final Set<String> members = new LinkedHashSet<>();

        Panel(int user, int slot, String id, JSONObject filter) {
            this.user = user;
            this.slot = slot;
            this.id = id;
            this.domain = slot < 2 ? "quote" : "price";
            this.filter = filter;
        }
    }

    private static final class Publication {
        long start;
        long first;
        long last;
        long nanos;
        long bytes;
        int count;
    }

    private static final class FixtureBase implements BaseDataClient {
        final Map<String, JSONObject> rows = new LinkedHashMap<>();

        FixtureBase() {
            for (int index = 0; index <= 100; index++) {
                String security = QuoteViewBenchmark.security(index);
                rows.put(security, JsonUtils.object("securityId", security, "securityName", "Benchmark " + index,
                        "market", "RATE", "issuerType", index % 2 == 0 ? "POLICY_BANK" : "GOVERNMENT",
                        "bondRating", index % 4 == 0 ? "AAA" : "AA+",
                        "region", index % 3 == 0 ? "110000" : index % 3 == 1 ? "440000" : "320000",
                        "hasExerciseOption", false, "remainingYears", "3.5",
                        "valuationYield", "2.4500", "valuationBasis", "MATURITY"));
            }
        }

        public Map<String, JSONObject> securities() {

            return rows;

        }
        public List<JSONObject> records(String kind) {
            return new ArrayList<>(rows.values());
        }
        public String version() {
            return "BENCHMARK-FIXED-20260929";
        }
    }

    private static String user(int index) {

        return String.format(Locale.ROOT, "U-%03d", index + 1);

    }
    private static String session(int index) {
        return "BENCH-S" + index;
    }
    private static String security(int index) {
        return String.format(Locale.ROOT, "%06d.IB", 900000 + index);
    }
    private static String quoteId(int index) {
        return "BENCH-Q" + index;
    }

    private void check(boolean condition, String name) {
        if (!condition) {
            throw new AssertionError(name);
        }
        checks++;
    }

    private void guard() {
        if (System.nanoTime() >= deadline) {
            throw new IllegalStateException("BENCHMARK_TIME_BUDGET");
        }
    }

    private void checkpoint() throws Exception {
        report.put("checks", checks);
        long heapPeak = ManagementFactory.getMemoryPoolMXBeans().stream()
                .filter(pool -> pool.getType() == java.lang.management.MemoryType.HEAP)
                .mapToLong(pool -> pool.getPeakUsage().getUsed()).sum();
        report.put("heapPoolPeakSumBytes", heapPeak);
        report.put("heapUsedBytes", ManagementFactory.getMemoryMXBean().getHeapMemoryUsage().getUsed());
        report.put("heapPeakMeaning", "sum of heap pool peaks; not necessarily simultaneous and not process RSS");
        report.put("rawSamples", new JSONArray(samples));
        Path temporary = output.resolveSibling(output.getFileName() + ".tmp");
        Files.write(temporary, JsonUtils.toJson(report).getBytes(StandardCharsets.UTF_8));
        Files.move(temporary, output, StandardCopyOption.REPLACE_EXISTING, StandardCopyOption.ATOMIC_MOVE);
    }

    private void seed(PgStore store, boolean concentrated) throws Exception {
        store.begin();
        for (int index = 0; index < QUOTES; index++) {
            guard();
            String price = new BigDecimal("2.4000").add(new BigDecimal(index % 100).movePointLeft(3)).toPlainString();
            JSONObject row = JsonUtils.object("quoteId", quoteId(index), "clientQuoteId", quoteId(index),
                    "securityId", security(concentrated ? 0 : index % 100), "ownerUserId", user(index % USERS),
                    "status", "ACTIVE", "revision", "1", "side", index % 2 == 0 ? "1" : "2",
                    "quoteType", "1", "yieldType", "1", "price", price, "quantity", "1000",
                    "quantityUnit", "WAN", "currency", "CNY", "settlType", "1",
                    "urgent", false, "recommended", false, "whole", false, "oco", false, "packing", false,
                    "source", JsonUtils.object("type", "MANUAL"),
                    "createTime", "2026-09-29T09:00:00+08:00", "createdBy", user(index % USERS),
                    "updatedBy", user(index % USERS),
                    "tradeDate", LocalDate.now().toString(), "updateTime", "2026-09-29T09:00:00+08:00");
            store.saveQuote(ProtocolCodec.decode(row, QuoteRecord.class));
        }
        store.commit();
    }

    private JSONObject filter(String kind, int user) {
        JSONArray conditions = new JSONArray();
        if ("narrow".equals(kind)) {
            conditions.add(JsonUtils.object("field", "securityId", "operator", "EQ", "value", security(user)));
        } else if ("combined".equals(kind)) {
            conditions.add(JsonUtils.object("field", "region", "operator", "EQ", "value", user % 2 == 0 ? "110000" : "440000"));
            conditions.add(JsonUtils.object("field", "issuerType", "operator", "EQ", "value", "POLICY_BANK"));
            conditions.add(JsonUtils.object("field", "bondRating", "operator", "EQ", "value", "AAA"));
            conditions.add(JsonUtils.object("field", "price", "operator", "BETWEEN", "value", JsonUtils.array("2.4", "2.46")));
        } else if ("threshold".equals(kind)) {
            conditions.add(JsonUtils.object("field", "price", "operator", "LTE", "value", "2.45"));
        }
        return JsonUtils.object("conditions", conditions);
    }

    private JSONObject request(int user, String topic, JSONObject body) {
        guard();
        String requestId = "BENCH-R" + ids.incrementAndGet();
        body.put("requestId", requestId);
        QuotePerformanceTrace trace = QuotePerformanceTrace.begin(topic);
        Publication published = new Publication();
        published.start = trace.start;
        publication.set(published);
        JSONObject reply = null;
        try {
            reply = service.handle(user(user), session(user), topic, body);
        } finally {
            JSONObject sample = trace.complete(topic, requestId);
            publication.remove();
            if (measuring) {
                sample.put("replyCode", reply == null ? "EXCEPTION" : reply.getString("Code"));
                JSONArray batch = reply == null || reply.getJSONObject("Data") == null
                        ? null : reply.getJSONObject("Data").getJSONArray("results");
                sample.put("batchErrors", batch == null ? 0 : batch.toJavaList(JSONObject.class).stream()
                        .filter(item -> !"0000".equals(item.getString("Code"))).count());
                sample.put("scenario", scenario);
                sample.put("user", user(user));
                sample.put("publishCallbackMs", published.nanos / 1_000_000.0);
                sample.put("firstViewPublishMs", published.first == 0 ? null : (published.first - published.start) / 1_000_000.0);
                sample.put("lastViewPublishMs", published.last == 0 ? null : (published.last - published.start) / 1_000_000.0);
                sample.put("publishedBytes", published.bytes);
                sample.put("viewPublications", published.count);
                samples.add(sample);
            }
        }
        if (!"0000".equals(reply.getString("Code"))) {
            throw new AssertionError(topic + " failed: " + reply);
        }
        JSONArray results = reply.getJSONObject("Data").getJSONArray("results");
        if (results != null) {
            for (JSONObject item : results.toJavaList(JSONObject.class)) {
                if (!"0000".equals(item.getString("Code"))) {
                    throw new AssertionError(topic + " item failed: " + item);
                }
            }
        }
        return reply.getJSONObject("Data");
    }

    private void publish(String topic, JSONObject message) {
        long started = System.nanoTime();
        JSONObject wire = ProtocolBoundary.push(topic, message);
        byte[] encoded = JsonUtils.toJson(wire).getBytes(StandardCharsets.UTF_8);
        Publication current = publication.get();
        if (current != null) {
            current.bytes += encoded.length;
            current.nanos += System.nanoTime() - started;
            if (topic.startsWith("ficc.quote.report.") || topic.startsWith("ficc.price.report.")) {
                long completed = System.nanoTime();
                if (current.first == 0) {
                    current.first = completed;
                }
                current.last = completed;
                current.count++;
            }
        }
        if (wire.containsKey("views") || "RESET_REQUIRED".equals(wire.getString("kind"))) {
            pushes.add(wire);
        }
    }

    private Panel panel(int user, int slot) {
        synchronized (service) {
            for (Panel panel : panels.values()) {
                if (panel.user == user && panel.slot == slot) {
                    return panel;
                }
            }
        }
        return null;
    }

    private void replace(int user, int slot, JSONObject filter) {
        Panel previous = panel(user, slot);
        if (previous != null) {
            request(user, "ficc." + previous.domain + ".view.close", JsonUtils.object("viewId", previous.id));
            synchronized (service) {
                panels.remove(previous.id);
            }
        }
        Panel next = new Panel(user, slot, "BV-" + ids.incrementAndGet(), filter);
        JSONObject body = JsonUtils.object("viewId", next.id, "filter", filter, "pageSize", 50);
        if (slot >= 2) {
            body.put("mode", slot == 2 ? "BEST" : "DEPTH");
            if (slot == 3) {
                body.put("depth", 2);
            }
        }
        request(user, "ficc." + next.domain + ".view.open", body);
        List<QuoteRecord> snapshot;
        synchronized (service) {
            snapshot = new ArrayList<>(service.quotes.values());
        }
        // Churn updates quantities only; compute the independent oracle outside the service lock.
        next.members.addAll(expected(next, snapshot));
        synchronized (service) {
            panels.put(next.id, next);
        }
    }

    /** Independent fixture oracle; deliberately does not call FilterEngine or QuoteMatcher. */
    private Set<String> expected(Panel panel) {
        return expected(panel, service.quotes.values());
    }

    private Set<String> expected(Panel panel, Collection<QuoteRecord> quotes) {
        Set<String> expected = new LinkedHashSet<>();
        List<JSONObject> conditions = panel.filter.getJSONArray("conditions").toJavaList(JSONObject.class);
        for (QuoteRecord quote : quotes) {
            if (!"ACTIVE".equals(quote.status)) {
                continue;
            }
            JSONObject reference = base.rows.get(quote.securityId);
            boolean matches = true;
            for (JSONObject condition : conditions) {
                String field = condition.getString("field");
                Object actual = "securityId".equals(field) ? quote.securityId
                        : "price".equals(field) ? quote.price : reference.get(field);
                if ("BETWEEN".equals(condition.getString("operator"))) {
                    BigDecimal price = new BigDecimal(actual.toString());
                    JSONArray bounds = condition.getJSONArray("value");
                    matches &= price.compareTo(bounds.getBigDecimal(0)) >= 0 && price.compareTo(bounds.getBigDecimal(1)) <= 0;
                } else if ("LTE".equals(condition.getString("operator"))) {
                    matches &= new BigDecimal(actual.toString()).compareTo(condition.getBigDecimal("value")) <= 0;
                } else {
                    matches &= Objects.equals(actual, condition.get("value"));
                }
            }
            if (matches) {
                expected.add("quote".equals(panel.domain) ? quote.quoteId : quote.securityId);
            }
        }
        return expected;
    }

    private void verify(boolean allowReset) {
        synchronized (service) {
            for (JSONObject push : pushes) {
                if ("RESET_REQUIRED".equals(push.getString("kind"))) {
                    check(allowReset, "unexpected RESET_REQUIRED");
                    continue;
                }
                for (JSONObject delta : push.getJSONArray("views").toJavaList(JSONObject.class)) {
                    Panel panel = panels.get(delta.getString("viewId"));
                    // A queued message may precede the completed close, as allowed by the protocol.
                    if (panel == null) {
                        continue;
                    }
                    panel.members.removeAll(delta.getJSONArray("removeIds").toJavaList(String.class));
                    panel.members.addAll(delta.getJSONArray("upsertIds").toJavaList(String.class));
                    check(panel.members.size() == delta.getIntValue("totalCount"), "delta count mismatch");
                }
            }
            pushes.clear();
            for (Panel panel : panels.values()) {
                ViewService.View view = service.views.find(session(panel.user), panel.domain).views.get(panel.id);
                Set<String> expected = expected(panel);
                check(expected.equals(panel.members), "membership mismatch " + panel.id);
                check(view.page.totalCount() == expected.size(), "server membership count mismatch " + panel.id);
            }
            check(panels.size() <= 40, "active view budget exceeded");
        }
    }

    private void edit(int user, JSONObject fields) {
        String id = quoteId(user);
        JSONObject input;
        synchronized (service) {
            input = JsonUtils.object("quoteId", id, "expectedRevision", service.quotes.get(id).revision);
        }
        input.putAll(fields);
        request(user, "ficc.quote.update", JsonUtils.object("quotes", JsonUtils.array(input)));
    }

    private void query(int user, int slot, int page) {
        Panel panel = panel(user, slot);
        request(user, "ficc." + panel.domain + ".view.query",
                JsonUtils.object("viewId", panel.id, "pageNumber", page, "pageSize", 50));
    }

    private void parallel(java.util.function.IntConsumer action) throws Exception {
        CountDownLatch ready = new CountDownLatch(USERS);
        CountDownLatch go = new CountDownLatch(1);
        List<Future<?>> futures = new ArrayList<>();
        for (int user = 0; user < USERS; user++) {
            final int owner = user;
            futures.add(workers.submit(() -> {
                ready.countDown();
                try {
                    go.await();
                } catch (InterruptedException error) {
                    Thread.currentThread().interrupt();
                    throw new RuntimeException(error);
                }
                action.accept(owner);
            }));
        }
        if (!ready.await(1, TimeUnit.SECONDS)) {
            throw new AssertionError("workers did not become ready");
        }
        go.countDown();
        for (Future<?> future : futures) {
            future.get(12, TimeUnit.SECONDS);
        }
    }

    private interface Action {

        void run() throws Exception;

    }

    private void runScenario(String id, Action action) throws Exception {
        guard();
        scenario = id;
        report.put("currentScenario", id);
        checkpoint();
        measuring = false;
        action.run();
        verify(false);
        long cpu = cpuNanos();
        long gc = gcMillis();
        long collections = gcCount();
        long started = System.nanoTime();
        measuring = true;
        int completedCycles = 0;
        try {
            for (int sample = 0; sample < SAMPLES; sample++) {
                guard();
                action.run();
                completedCycles++;
            }
        } finally {
            measuring = false;
            JSONObject result = JsonUtils.object("id", id, "wallMs", (System.nanoTime() - started) / 1_000_000.0,
                    "cpuMs", (cpuNanos() - cpu) / 1_000_000.0, "gcMs", gcMillis() - gc, "gcCollections", gcCount() - collections,
                    "heapUsedBytes", ManagementFactory.getMemoryMXBean().getHeapMemoryUsage().getUsed(),
                    "completedCycles", completedCycles,
                    "status", completedCycles == SAMPLES ? "MEASURED" : "INCOMPLETE");
            report.getJSONArray("scenarios").add(result);
        }
        verify(false);
        checkpoint();
    }

    private static long cpuNanos() {
        return ((com.sun.management.OperatingSystemMXBean) ManagementFactory.getOperatingSystemMXBean()).getProcessCpuTime();
    }

    private static long gcCount() {
        long count = 0;
        for (GarbageCollectorMXBean bean : ManagementFactory.getGarbageCollectorMXBeans()) {
            count += Math.max(0, bean.getCollectionCount());
        }
        return count;
    }

    private static long gcMillis() {
        long value = 0;
        for (GarbageCollectorMXBean bean : ManagementFactory.getGarbageCollectorMXBeans()) {
            if (bean.getCollectionTime() >= 0) {
                value += bean.getCollectionTime();
            }
        }
        return value;
    }

    private void run(PgStore store, boolean concentrated) throws Exception {
        base = new FixtureBase();
        seed(store, concentrated);
        service = new QuoteService(store, new ReferenceData(base, new SimulatorCrmClient()));
        service.publish = this::publish;
        workers = Executors.newFixedThreadPool(USERS, task -> {
            Thread thread = new Thread(task, "quote-benchmark-user");
            thread.setDaemon(true);
            return thread;
        });
        for (int user = 0; user < USERS; user++) {
            for (String domain : Arrays.asList("quote", "price")) {
                JSONObject stream = request(user, "ficc." + domain + ".stream.open", JsonUtils.object());
                service.views.authorize(session(user), user(user), stream.getString("pushTopic"));
            }
            replace(user, 0, filter("broad", user));
            replace(user, 1, filter("narrow", user));
            replace(user, 2, filter("broad", user));
            replace(user, 3, filter("combined", user));
        }
        verify(false);
        report.put("initialActiveViews", panels.size());
        for (String kind : Arrays.asList("broad", "narrow", "combined")) {
            runScenario("S01-open-" + kind, () -> replace(0, 0, filter(kind, 0)));
        }
        runScenario("S02-switch", () -> replace(0, 0, filter(ids.get() % 2 == 0 ? "broad" : "combined", 0)));
        replace(0, 0, filter("threshold", 0));
        runScenario("S03-matching-update", () -> edit(0, JsonUtils.object("quantity", Integer.toString(1000 + ids.get()))));
        runScenario("S04-enter-leave", () -> {
            edit(0, JsonUtils.object("price", "2.9"));
            edit(0, JsonUtils.object("price", "2.4"));
        });
        runScenario("S05-book-move", () -> {
            edit(0, JsonUtils.object("securityId", security(100), "price", "2.1"));
            edit(0, JsonUtils.object("securityId", security(0), "price", "2.4"));
        });
        runScenario("S06-concurrent-query", () -> parallel(user -> query(user, 0, 1)));
        runScenario("S07-view-churn-and-updates", () -> parallel(user -> {
            if (user < 5) {
                replace(user, 1, filter("narrow", user));
            }
            else {
                edit(user, JsonUtils.object("quantity", Integer.toString(2000 + user)));
            }
        }));
        // Saved group lifecycle includes real persistence and a dependent view, then expected reset.
        scenario = "S08-filter-group";
        report.put("currentScenario", scenario);
        checkpoint();
        long groupStarted = System.nanoTime();
        long groupCpu = cpuNanos();
        long groupGc = gcMillis();
        measuring = true;
        JSONObject groupRequest = JsonUtils.object("name", "Benchmark", "market", "RATE", "scope", "MARKET",
                "subgroups", JsonUtils.array(JsonUtils.object("conditions", filter("threshold", 0).getJSONArray("conditions"))));
        JSONObject group = request(0, "ficc.filterGroup.save", groupRequest).getJSONObject("filterGroup");
        Panel previous = panel(0, 1);
        request(0, "ficc.quote.view.close", JsonUtils.object("viewId", previous.id));
        synchronized (service) {
            panels.remove(previous.id);
        }
        String dependentId = "GROUP-V" + ids.incrementAndGet();
        request(0, "ficc.quote.view.open", JsonUtils.object("viewId", dependentId, "pageSize", 50,
                "filter", JsonUtils.object("filterGroupIds", JsonUtils.array(group.getString("filterGroupId")))));
        check(service.views.find(session(0), "quote").views.get(dependentId).page.totalCount()
                == expected(new Panel(0, 1, dependentId, filter("threshold", 0))).size(), "initial group count");
        groupRequest = JsonUtils.object("filterGroupId", group.getString("filterGroupId"), "expectedRevision", group.getString("revision"),
                "name", "Benchmark updated", "market", "RATE", "scope", "MARKET",
                "subgroups", JsonUtils.array(JsonUtils.object("conditions", filter("combined", 0).getJSONArray("conditions"))));
        group = request(0, "ficc.filterGroup.save", groupRequest).getJSONObject("filterGroup");
        check(service.views.find(session(0), "quote").views.get(dependentId).page.totalCount()
                == expected(new Panel(0, 1, dependentId, filter("combined", 0))).size(), "updated group count");
        request(0, "ficc.filterGroup.delete", JsonUtils.object("filterGroupId", group.getString("filterGroupId"), "expectedRevision", group.getString("revision")));
        check(pushes.stream().anyMatch(push -> "RESET_REQUIRED".equals(push.getString("kind"))), "deleted group must reset dependent stream");
        request(0, "ficc.quote.view.close", JsonUtils.object("viewId", dependentId));
        measuring = false;
        replace(0, 1, filter("narrow", 0));
        verify(true);
        report.getJSONArray("scenarios").add(JsonUtils.object("id", scenario, "status", "MEASURED", "cycles", 1,
                "wallMs", (System.nanoTime() - groupStarted) / 1_000_000.0,
                "cpuMs", (cpuNanos() - groupCpu) / 1_000_000.0, "gcMs", gcMillis() - groupGc,
                "heapUsedBytes", ManagementFactory.getMemoryMXBean().getHeapMemoryUsage().getUsed()));
        checkpoint();
        runScenario("S09-page-and-updates", () -> parallel(user -> {
            if (user < 5) {
                query(user, 0, 2);
            }
            else {
                edit(user, JsonUtils.object("quantity", Integer.toString(3000 + user)));
            }
        }));
        measuring = false;
        for (Panel panel : new ArrayList<>(panels.values())) {
            request(panel.user, "ficc." + panel.domain + ".view.close", JsonUtils.object("viewId", panel.id));
        }
        panels.clear();
        int remaining = service.views.streams.values().stream().mapToInt(stream -> stream.views.size()).sum();
        check(remaining == 0, "views must be released");
        report.put("remainingViews", remaining);
        report.put("status", "PASS");
    }

    public static void main(String[] args) throws Exception {
        if (!Boolean.getBoolean("ficc.benchmark.authorized")) {
            throw new IllegalStateException("Use the authorized benchmark launcher");
        }
        QuoteViewBenchmark benchmark = new QuoteViewBenchmark();
        benchmark.output = Paths.get(args[1]);
        Properties configuration = new Properties();
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(args[0]), StandardCharsets.UTF_8)) {
            configuration.load(reader);
        }
        String schema = args[2];
        if (!schema.matches("ficc_bench_[a-f0-9]{12}")) { throw new IllegalArgumentException("Invalid isolated schema name"); }
        configuration.setProperty("db.schema", schema);
        benchmark.report.put("schema", schema);
        benchmark.report.put("distribution", args[3]);
        benchmark.checkpoint();
        int exit = 0;
        try (PgStore store = new PgStore(configuration)) {
            try {
                benchmark.run(store, "concentrated".equals(args[3]));
            } finally {
                boolean stopped = true;
                if (benchmark.workers != null) {
                    benchmark.workers.shutdownNow();
                    stopped = benchmark.workers.awaitTermination(1, TimeUnit.SECONDS);
                }
                if (stopped) {
                    store.exec("DROP SCHEMA " + schema + " CASCADE");
                    benchmark.report.put("schemaRemoved", true);
                } else {
                    benchmark.report.put("schemaRemoved", false);
                    throw new IllegalStateException("Benchmark workers did not stop; isolated schema retained");
                }
            }
        } catch (Throwable error) {
            Throwable cause = error;
            while (cause.getCause() != null) {
                cause = cause.getCause();
            }
            benchmark.report.put("status", "BENCHMARK_TIME_BUDGET".equals(cause.getMessage())
                    ? "INCOMPLETE_TIME_BUDGET" : "FAILED");
            benchmark.report.put("failure", error.getClass().getSimpleName() + ": " + error.getMessage());
            error.printStackTrace();
            exit = 1;
        } finally {
            benchmark.checkpoint();
        }
        System.out.println("BENCHMARK " + benchmark.report.getString("status"));
        if (exit != 0) {
            System.exit(exit);
        }
    }
}
