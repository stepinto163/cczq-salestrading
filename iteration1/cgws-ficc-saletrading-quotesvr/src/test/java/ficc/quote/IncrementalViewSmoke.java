package ficc.quote;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.api.ProtocolBoundary;
import ficc.api.ProtocolCodec;
import ficc.api.QuoteRecord;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import ficc.common.client.BaseDataClient;
import ficc.common.client.SimulatorBaseDataClient;
import ficc.common.client.SimulatorCrmClient;

import java.io.BufferedReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.Set;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicReference;

/** Four persisted fixtures plus one in-memory conversion fixture; no load, UI or transport test. */
public final class IncrementalViewSmoke {
    private final CountingBaseData base = new CountingBaseData();
    private final List<JSONObject> messages = new ArrayList<>();
    private final Map<String, Map<String, JSONObject>> clientRows = new LinkedHashMap<>();
    private QuoteService service;
    private int checks;
    private int operation;

    private static final class CountingBaseData implements BaseDataClient {
        private final Map<String, JSONObject> rows = new LinkedHashMap<>();
        private final Set<String> reads = new LinkedHashSet<>();
        private String version = "FIXED-1";

        CountingBaseData() {
            new SimulatorBaseDataClient().securities().forEach(
                    (id, row) -> rows.put(id, JsonUtils.deepCopy(row)));
        }

        public Map<String, JSONObject> securities() {
            return rows;
        }

        public JSONObject security(String id) {
            reads.add(id);
            return rows.get(id);
        }

        public List<JSONObject> records(String kind) {
            return new ArrayList<>(rows.values());
        }

        public String version() {
            return version;
        }
    }

    private void check(boolean condition, String message) {
        if (!condition) {
            throw new AssertionError(message);
        }
        checks++;
        System.out.println("PASS " + message);
    }

    private static QuoteRecord quote(String id, String security, String owner, String price) {
        return ProtocolCodec.decode(JsonUtils.object(
                "quoteId", id, "clientQuoteId", id, "securityId", security,
                "ownerUserId", owner, "status", "ACTIVE", "revision", "1",
                "side", "1", "quoteType", "1", "yieldType", "1", "price", price,
                "quantity", "1000", "settlType", "1", "tradeDate", "2026-09-29",
                "urgent", false, "recommended", false, "whole", false,
                "oco", false, "packing", false,
                "updateTime", "2026-09-29T09:00:00+08:00"), QuoteRecord.class);
    }

    private JSONObject call(String user, String session, String topic, JSONObject request) {
        request.put("requestId", "IV-" + (++operation));
        JSONObject reply = service.handle(user, session, topic, request);
        check("0000".equals(reply.getString("Code")), topic + " succeeded");
        return reply.getJSONObject("Data");
    }

    private void open(String user, String session, String domain, String id, JSONObject filter,
            String mode, String scope) {
        JSONObject stream = call(user, session, "ficc." + domain + ".stream.open", JsonUtils.object());
        service.views.authorize(session, user, stream.getString("pushTopic"));
        JSONObject request = JsonUtils.object("viewId", id, "pageSize", 1, "filter", filter);
        if ("quote".equals(domain)) {
            request.put("ownerScope", scope);
        } else {
            request.put("mode", mode);
            if ("DEPTH".equals(mode)) {
                request.put("depth", 1);
            }
        }
        call(user, session, "ficc." + domain + ".view.open", request);
        ViewService.Stream state = service.views.find(session, domain);
        ViewService.View view = state.views.get(id);
        clientRows.put(id, fullRows(state, view));
    }

    private Map<String, JSONObject> fullRows(ViewService.Stream stream, ViewService.View view) {
        Map<String, JSONObject> rows = new LinkedHashMap<>();
        String idField = view instanceof ViewService.QuoteView ? "quoteId" : "securityId";
        for (JSONObject row : service.views.matchingRows(stream, view)) {
            rows.put(row.getString(idField), row);
        }
        return rows;
    }

    private void verifyDeltas(String label) {
        for (JSONObject envelope : messages) {
            JSONObject message = envelope.getJSONObject("message");
            check(!"RESET_REQUIRED".equals(message.getString("kind")), label + " no reset");
            JSONArray deltas = message.getJSONArray("views");
            if (deltas == null) {
                continue;
            }
            for (JSONObject delta : deltas.toJavaList(JSONObject.class)) {
                String viewId = delta.getString("viewId");
                Map<String, JSONObject> rows = clientRows.get(viewId);
                check(rows != null, label + " only open views receive deltas");
                for (String id : delta.getJSONArray("removeIds").toJavaList(String.class)) {
                    rows.remove(id);
                }
                JSONArray records = message.containsKey("quotes")
                        ? message.getJSONArray("quotes") : message.getJSONArray("books");
                String key = message.containsKey("quotes") ? "quoteId" : "securityId";
                for (String id : delta.getJSONArray("upsertIds").toJavaList(String.class)) {
                    JSONObject found = null;
                    for (JSONObject record : records.toJavaList(JSONObject.class)) {
                        if (id.equals(record.getString(key))
                                && (!record.containsKey("viewId") || viewId.equals(record.getString("viewId")))) {
                            found = JsonUtils.deepCopy(record);
                            found.remove("viewId");
                            break;
                        }
                    }
                    check(found != null, label + " upsert has payload");
                    rows.put(id, found);
                }
                check(rows.size() == delta.getIntValue("totalCount"), label + " totalCount");
            }
        }
        for (ViewService.Stream stream : service.views.streams.values()) {
            for (ViewService.View view : stream.views.values()) {
                Map<String, JSONObject> expected = fullRows(stream, view);
                check(JsonUtils.canonicalHash(expected).equals(JsonUtils.canonicalHash(clientRows.get(view.id))),
                        label + " " + view.id + " matches full-query oracle");
                check(view.page.totalCount() == expected.size(), label + " cached membership count");
            }
        }
    }

    private void change(String id, JSONObject fields, String label, boolean referenceChanged) {
        QuoteRecord before = service.quotes.get(id);
        JSONObject json = before.toJson();
        json.putAll(fields);
        json.put("revision", Long.toString(Long.parseLong(before.revision) + 1));
        QuoteRecord after = ProtocolCodec.decode(json, QuoteRecord.class);
        messages.clear();
        base.reads.clear();
        synchronized (service) {
            service.quotes.put(id, after);
            service.views.changed(after, JsonUtils.object("eventId", "E-" + (++operation),
                    "requestId", "IV-" + operation, "action", "UPDATE",
                    "operatorUserId", after.ownerUserId, "operationTime", after.updateTime), before.securityId);
        }
        if (!referenceChanged) {
            Set<String> affected = new LinkedHashSet<>(Arrays.asList(before.securityId, after.securityId));
            check(affected.containsAll(base.reads), label + " never reads unrelated bonds");
        }
        verifyDeltas(label);
    }

    private JSONObject groupRequest(String groupId, String revision, String price) {
        JSONObject request = JsonUtils.object("name", "Incremental test", "market", "RATE", "scope", "MARKET",
                "subgroups", JsonUtils.array(JsonUtils.object("conditions", JsonUtils.array(
                        JsonUtils.object("field", "price", "operator", "LTE", "value", price)))));
        if (groupId != null) {
            request.put("filterGroupId", groupId);
            request.put("expectedRevision", revision);
        }
        return request;
    }

    private void checkSettings() {
        JSONObject group = call("U-002", "S2", "ficc.filterGroup.save", groupRequest(null, null, "2.6"))
                .getJSONObject("filterGroup");
        String id = group.getString("filterGroupId");
        open("U-002", "S2", "quote", "Q-GROUP",
                JsonUtils.object("filterGroupIds", JsonUtils.array(id)), null, "VISIBLE");
        messages.clear();
        group = call("U-002", "S2", "ficc.filterGroup.save",
                groupRequest(id, group.getString("revision"), "2.9")).getJSONObject("filterGroup");
        verifyDeltas("saved group update recalculates dependent view");
        for (JSONObject envelope : messages) {
            JSONArray views = envelope.getJSONObject("message").getJSONArray("views");
            if (views != null) {
                for (JSONObject delta : views.toJavaList(JSONObject.class)) {
                    check("Q-GROUP".equals(delta.getString("viewId")), "unrelated views are not recalculated for group edit");
                }
            }
        }
        messages.clear();
        call("U-002", "S2", "ficc.filterGroup.delete", JsonUtils.object(
                "filterGroupId", id, "expectedRevision", group.getString("revision")));
        check(messages.stream().anyMatch(envelope -> "RESET_REQUIRED".equals(
                envelope.getJSONObject("message").getString("kind"))), "deleting referenced group requires reset");
        call("U-002", "S2", "ficc.quote.view.close", JsonUtils.object("viewId", "Q-GROUP"));
        clientRows.remove("Q-GROUP");
    }

    private void checkConcurrentClose(JSONObject filter) throws Exception {
        open("U-001", "S3", "quote", "Q-CLOSE", filter, null, "VISIBLE");
        CountDownLatch closeRequested = new CountDownLatch(1);
        AtomicReference<Throwable> failure = new AtomicReference<>();
        Thread closing = new Thread(() -> {
            closeRequested.countDown();
            try {
                JSONObject reply = service.handle("U-001", "S3", "ficc.quote.view.close",
                        JsonUtils.object("requestId", "concurrent-close", "viewId", "Q-CLOSE"));
                if (!"0000".equals(reply.getString("Code"))) {
                    throw new AssertionError("Concurrent close failed: " + reply);
                }
            } catch (Throwable error) {
                failure.set(error);
            }
        }, "incremental-view-close-check");
        closing.setDaemon(true);
        synchronized (service) {
            closing.start();
            check(closeRequested.await(2, TimeUnit.SECONDS), "close caller started");
            change("B", JsonUtils.object("quantity", "6000"), "update ordered before waiting close", false);
        }
        closing.join(5000);
        check(!closing.isAlive() && failure.get() == null, "waiting close completes without error");
        clientRows.remove("Q-CLOSE");
        change("B", JsonUtils.object("quantity", "7000"), "no new delta after concurrent close completes", false);
    }

    private void checkDetachedPage() throws Exception {
        QuotePageSnapshot snapshot;
        synchronized (service) {
            snapshot = (QuotePageSnapshot) service.views.queryPage(new QuoteService.Call(
                    "U-001", "S1", "ficc.quote.view.query", JsonUtils.object(
                    "requestId", "SNAPSHOT-CHECK", "viewId", "Q-ALL", "pageNumber", 1, "pageSize", 2)), "quote");
        }
        String sequence = Long.toString(service.views.find("S1", "quote").sequence);
        change("A", JsonUtils.object("quantity", "1234"), "update after page capture", false);
        AtomicReference<JSONObject> materialized = new AtomicReference<>();
        Thread encoder = new Thread(() -> materialized.set(snapshot.materialize()), "page-encoding-check");
        encoder.setDaemon(true);
        synchronized (service) {
            encoder.start();
            encoder.join(2000);
            check(!encoder.isAlive(), "captured page encodes without acquiring service lock");
        }
        JSONObject result = materialized.get();
        check(sequence.equals(result.getString("sequence")), "page retains captured stream sequence");
        JSONArray rows = result.getJSONArray("quotes");
        check(rows.size() == 2, "only captured page rows are materialized");
        check("1000".equals(rows.getJSONObject(0).getString("quantity")),
                "page retains pre-update quote version");
        QuoteRecord own = service.quotes.get("A");
        QuoteRecord other = ProtocolCodec.decode(service.quotes.get("B").toJson(), QuoteRecord.class);
        other.institutionId = "PRIVATE-INSTITUTION";
        other.contactName = "PRIVATE-CONTACT";
        check(QuotePageSnapshot.visibleJson(other, "U-001", service.access).getString("institutionId").equals("PRIVATE-INSTITUTION"),
                "page shares confirmed counterparty institution");
        check(QuotePageSnapshot.visibleJson(other, "U-001", service.access).getString("contactName").equals("PRIVATE-CONTACT"),
                "page shares confirmed counterparty contact");
        check("PRIVATE-INSTITUTION".equals(other.institutionId), "redaction never mutates stored version");
        check(JsonUtils.canonicalHash(service.visible(own, "U-001").toJson()).equals(
                JsonUtils.canonicalHash(QuotePageSnapshot.visibleJson(own, "U-001", service.access))),
                "page preserves owner-visible wire representation");
    }

    private void run(PgStore store) throws Exception {
        store.saveQuote(quote("A", "250001.IB", "U-001", "2.5"));
        store.saveQuote(quote("B", "250001.IB", "U-002", "2.6"));
        store.saveQuote(quote("C", "250002.IB", "U-001", "2.7"));
        store.saveQuote(quote("D", "240210.IB", "U-002", "2.8"));
        service = new QuoteService(store, new ReferenceData(base, new SimulatorCrmClient()));
        service.publish = (topic, message) -> messages.add(JsonUtils.object(
                "topic", topic, "message", ProtocolBoundary.push(topic, message)));
        JSONObject filter = JsonUtils.object("conditions", JsonUtils.array(
                JsonUtils.object("field", "price", "operator", "LTE", "value", "2.6"),
                JsonUtils.object("field", "region", "operator", "EQ", "value", "110000")));
        open("U-001", "S1", "quote", "Q-ALL", filter, null, "VISIBLE");
        open("U-002", "S2", "quote", "Q-MINE", filter, null, "MINE");
        open("U-001", "S1", "price", "P-BEST", filter, "BEST", null);
        open("U-002", "S2", "price", "P-DEPTH", filter, "DEPTH", null);
        check(service.views.find("S1", "quote").views.get("Q-ALL").page.totalCount() == 2,
                "membership covers matches beyond page size one");

        checkDetachedPage();
        change("A", JsonUtils.object("price", "2.9"), "leave filter; next best fills book", false);
        change("C", JsonUtils.object("price", "2.4"), "enter filter on another bond", false);
        change("C", JsonUtils.object("quantity", "2000"), "remain matching with new payload", false);
        change("B", JsonUtils.object("status", "REFERRED"), "last active quote removes book", false);
        change("B", JsonUtils.object("status", "ACTIVE"), "restore book", false);
        change("C", JsonUtils.object("securityId", "250001.IB"), "move quote between bonds", false);
        change("C", JsonUtils.object("status", "DELETED"), "delete matching quote", false);
        checkSettings();

        base.rows.get("250001.IB").put("region", "440000");
        base.version = "FIXED-2";
        change("D", JsonUtils.object("quantity", "3000"), "reference version rebuilds all membership", true);
        base.rows.get("250001.IB").put("region", "110000");
        base.version = "FIXED-3";
        change("D", JsonUtils.object("quantity", "4000"), "reference restoration", true);

        call("U-001", "S1", "ficc.quote.view.close", JsonUtils.object("viewId", "Q-ALL"));
        clientRows.remove("Q-ALL");
        change("B", JsonUtils.object("quantity", "5000"), "closed view stays closed", false);
        JSONObject reply = call("U-002", "S2", "ficc.quote.view.query",
                JsonUtils.object("viewId", "Q-MINE", "pageNumber", 1, "pageSize", 1));
        check(reply.getJSONObject("view").getIntValue("totalCount") == 1, "page query after incremental changes");
        check(reply.getJSONArray("quotes").getJSONObject(0).getString("quantity").equals("5000"),
                "page query returns current payload");
        change("B", JsonUtils.object("price", "2.55"), "incremental update after page query", false);
        checkConcurrentClose(filter);
        checkCalculationIsolation();
    }

    private void checkCalculationIsolation() {
        QuoteRecord clean = quote("CAL-FAILURE", "250001.IB", "U-001", "101.25");
        clean.quoteType = "2";
        service.quotes.put(clean.quoteId, clean);
        service.priceBooks.changed(clean, null);
        open("U-001", "S-CAL", "price", "P-CAL", JsonUtils.object(), "FULL", null);
        JSONObject snapshot = ProtocolBoundary.push("ficc.price.snapshots",
                service.views.feed(true, Arrays.asList("250001.IB", "250002.IB")));
        check("0000".equals(snapshot.getString("Code")), "Notification price snapshot succeeds with missing conversion");
        check(snapshot.getJSONArray("prices").getJSONObject(0).getJSONArray("calculationIssues").size() == 1,
                "Internal snapshot preserves calculation issue through protocol boundary");
        messages.clear();
        service.priceBooks.yieldResolver = (quote, yieldType, version) ->
                new QuoteYieldResolver.Result("2.45", null);
        service.views.calculationChanged(clean.securityId);
        check(messages.stream().noneMatch(message -> message.getString("topic").startsWith("ficc.quote.")),
                "Calculation completion never emits a quote operation or changes quote lifecycle");
        check(messages.stream().anyMatch(message -> message.getString("topic").equals("ficc.price.snapshots")),
                "Calculation completion refreshes NotificationSvr feed");
        check(messages.stream().anyMatch(message -> message.getString("topic").equals("ficc.price.report.S-CAL")
                        && message.getJSONObject("message").getJSONArray("books").size() > 0),
                "Calculation completion pushes price view without reopening it");
        JSONObject page = call("U-001", "S-CAL", "ficc.price.view.query",
                JsonUtils.object("viewId", "P-CAL", "pageNumber", 1, "pageSize", 100));
        check(page.getJSONArray("books").toJSONString().contains("FICC:CAL-FAILURE"),
                "Recovered conversion is visible in the existing page");
        check("101.25".equals(clean.price) && "2".equals(clean.quoteType) && "1".equals(clean.revision),
                "Asynchronous recovery preserves stored quote attributes and version");
    }

    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(args[0]), StandardCharsets.UTF_8)) {
            configuration.load(reader);
        }
        configuration.setProperty("db.schema", "ficc_incremental_" + System.currentTimeMillis());
        IncrementalViewSmoke test = new IncrementalViewSmoke();
        try (PgStore store = new PgStore(configuration)) {
            test.run(store);
        }
        Files.write(Paths.get(args[1]), JsonUtils.toJson(JsonUtils.object(
                "result", "PASS", "checks", test.checks, "fixtureQuotes", 5,
                "transportTested", false, "performanceTested", false)).getBytes(StandardCharsets.UTF_8));
        System.out.println("PASS " + test.checks + " checks; five fixtures; no load or transport test");
    }
}
