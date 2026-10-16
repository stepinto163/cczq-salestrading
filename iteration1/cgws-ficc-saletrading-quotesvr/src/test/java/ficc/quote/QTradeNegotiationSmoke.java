package ficc.quote;

import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.api.QuoteRecord;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import java.io.Reader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.*;
import java.util.stream.Collectors;

/** Six mapped FIX inputs, isolated PostgreSQL schema, real intake/write/query/push code.
 * This is a component integration test, not an external FIX/GW/desktop test.
 */
public final class QTradeNegotiationSmoke {
    private static final String SECURITY = "250001.IB";
    private final IntakeCreateEndpoint intake =
            new IntakeCreateEndpoint("negotiation-local-test-token-32-characters");
    private final JSONArray snapshots = new JSONArray();
    private final JSONArray protocolFixtures = new JSONArray();
    private final List<String> pushedTopics = new ArrayList<>();
    private QuoteService service;
    private int checks;
    private int requestSequence;

    private void check(boolean valid, String description) {
        if (!valid) throw new AssertionError(description);
        checks++;
        System.out.println("PASS " + description);
    }

    private JSONObject call(String user, String topic, JSONObject request) {
        request.put("requestId", "TEST-" + (++requestSequence));
        JSONObject reply = service.handle(user, "SESSION-" + user, topic, request);
        protocolFixtures.add(JsonUtils.deepCopy(JsonUtils.object(
                "topic", topic, "request", request, "reply", reply)));
        check("0000".equals(reply.getString("Code")), topic + " " + user + ": " + reply.getString("Msg"));
        return reply.getJSONObject("Data");
    }

    private String receive(JSONObject request) throws Exception {
        JSONObject item = intake.create(service, request).getJSONObject("Data")
                .getJSONArray("results").getJSONObject(0);
        check("0000".equals(item.getString("Code")), "intake accepted: " + item.getString("Msg"));
        return item.getString("quoteId");
    }

    private void action(String action, String id) {
        QuoteRecord quote = service.quotes.get(id);
        JSONObject edit = JsonUtils.object("quoteId", id, "expectedRevision", quote.revision);
        if ("confirm".equals(action)) {
            // 两个会话对应不同对手；同一对手的新提案使用相同的已匹配 CRM 身份。
            boolean counterpartyC = quote.source.negotiationId.startsWith("A-C");
            edit.put("institutionId", counterpartyC ? "I-002" : "I-001");
            edit.put("contactId", counterpartyC ? "C-002" : "C-001");
        }
        JSONObject result = call(quote.ownerUserId, "ficc.quote." + action,
                JsonUtils.object("quotes", JsonUtils.array(edit))).getJSONArray("results").getJSONObject(0);
        check("0000".equals(result.getString("Code")), action + " result: " + result.getString("Msg"));
    }

    private JSONObject book(String user, String mode) {
        return service.views.book(user, SECURITY, JsonUtils.object(), mode, 1);
    }

    private Set<String> ids(JSONObject book) {
        return book.getJSONObject("quoteRefs").values().stream()
                .map(value -> ((JSONObject) value).getString("quoteId"))
                .collect(Collectors.toCollection(LinkedHashSet::new));
    }

    private void expectBook(String user, String mode, String... expected) {
        check(ids(book(user, mode)).equals(new LinkedHashSet<>(Arrays.asList(expected))),
                user + " " + mode + " contains exactly " + Arrays.toString(expected));
    }

    private void pendingHasNoPriceEffect(JSONObject request) throws Exception {
        String before = JsonUtils.toJson(book("A", "FULL"));
        pushedTopics.clear();
        receive(request);
        check(before.equals(JsonUtils.toJson(book("A", "FULL"))),
                "pending proposal leaves confirmed price, quantity and book revision unchanged");
        check(pushedTopics.stream().noneMatch(topic -> topic.startsWith("ficc.price.report.")
                        || topic.startsWith("ficc.price.book.")),
                "pending proposal publishes no user price/book update");
    }

    private void snapshot(String stage) {
        snapshots.add(JsonUtils.object("stage", stage,
                "state", JsonUtils.object("epoch", service.store.epoch, "sequence", service.lastSequence),
                "quotes", service.quotes.values().stream().map(QuoteRecord::toJson).collect(Collectors.toList()),
                "feed", service.views.feed(true, Collections.singletonList(SECURITY))));
    }

    private void run(JSONArray inputs) throws Exception {
        // 新会话对应相同 CRM 业务键；确认后仍必须替换旧会话的有效报价。
        inputs.getJSONObject(4).getJSONObject("intake").put("negotiationId", "A-C-NEW");
        inputs.getJSONObject(5).getJSONObject("intake").put("negotiationId", "A-C-NEW");
        for (String user : Arrays.asList("A", "B", "C")) {
            for (String domain : Arrays.asList("quote", "price")) {
                JSONObject stream = call(user, "ficc." + domain + ".stream.open", JsonUtils.object());
                service.views.authorize("SESSION-" + user, user, stream.getString("pushTopic"));
                JSONObject view = JsonUtils.object("viewId", domain + "-" + user, "pageSize", 20);
                if ("price".equals(domain)) view.put("mode", "FULL");
                call(user, "ficc." + domain + ".view.open", view);
            }
        }
        String bidB = receive(inputs.getJSONObject(0));
        String bidC = receive(inputs.getJSONObject(1));
        for (String user : Arrays.asList("A", "B", "C")) expectBook(user, "FULL");
        JSONObject privateRead = service.handle("A", "SESSION-A", "ficc.quote.get",
                JsonUtils.object("requestId", "pending-private", "quoteId", bidB));
        check("1002".equals(privateRead.getString("Code")), "pending quote is private to its recipient");
        snapshot("pending-only");
        action("confirm", bidB);
        expectBook("B", "FULL", bidB);
        expectBook("C", "FULL", bidB);
        action("confirm", bidC);
        expectBook("B", "FULL", bidB, bidC);
        expectBook("C", "FULL", bidB, bidC);
        String offerB = receive(inputs.getJSONObject(2));
        String offerC = receive(inputs.getJSONObject(3));
        check(!offerB.equals(offerC), "same intention produces two independent bilateral proposals");
        check("INTENTION-1".equals(service.quotes.get(offerB).source.externalQuoteId)
                && "INTENTION-1".equals(service.quotes.get(offerC).source.externalQuoteId),
                "both proposals retain their common intention identity");
        expectBook("A", "FULL", bidB, bidC);
        snapshot("counteroffers-pending");
        action("confirm", offerB);
        action("confirm", offerC);
        expectBook("A", "FULL", bidB, bidC, offerC, offerB);
        expectBook("A", "BEST", bidB, bidC, offerC);
        expectBook("A", "DEPTH", bidB, bidC, offerC);
        expectBook("B", "BEST", bidB, bidC, offerC);
        expectBook("C", "BEST", bidB, bidC, offerC);
        JSONObject denied = service.handle("B", "SESSION-B", "ficc.quote.get",
                JsonUtils.object("requestId", "denied", "quoteId", offerC));
        check("0000".equals(denied.getString("Code")), "confirmed A-C proposal is shared with B");
        JSONObject forbidden = call("B", "ficc.quote.refer", JsonUtils.object("quotes", JsonUtils.array(
                JsonUtils.object("quoteId", offerC, "expectedRevision", service.quotes.get(offerC).revision))));
        check("1003".equals(forbidden.getJSONArray("results").getJSONObject(0).getString("Code")),
                "shared visibility never grants modification rights");
        JSONObject query = call("B", "ficc.quote.query", JsonUtils.object());
        check(query.getJSONArray("quotes").size() == 4,
                "default active quote query shares all confirmed quotes");
        JSONObject mine = call("B", "ficc.quote.query", JsonUtils.object("ownerScope", "MINE"));
        check(mine.getJSONArray("quotes").size() == 1, "MINE remains an optional owner filter");
        snapshot("both-counteroffers-confirmed");
        pendingHasNoPriceEffect(inputs.getJSONObject(4));
        String replacement = receive(inputs.getJSONObject(4));
        check(!replacement.equals(offerC), "new proposal does not overwrite confirmed local record");
        check(!service.quotes.get(replacement).source.negotiationId.equals(
                service.quotes.get(offerC).source.negotiationId), "replacement crosses NegotiationIDs");
        pendingHasNoPriceEffect(inputs.getJSONObject(5));
        check(replacement.equals(receive(inputs.getJSONObject(5))), "pending conversation update reuses pending record");
        check("2.03".equals(service.quotes.get(replacement).price)
                        && "2.01".equals(service.quotes.get(offerC).price),
                "latest pending and last confirmed prices remain separate");
        snapshot("replacement-pending");
        JSONObject staleConfirm = call("A", "ficc.quote.confirm", JsonUtils.object("quotes",
                JsonUtils.array(JsonUtils.object("quoteId", replacement, "expectedRevision", "0"))));
        check("1004".equals(staleConfirm.getJSONArray("results").getJSONObject(0).getString("Code")),
                "stale confirmation is rejected before replacing the confirmed quote");
        expectBook("A", "FULL", bidB, bidC, offerC, offerB);
        check("PENDING_CONFIRM".equals(service.quotes.get(replacement).status),
                "rejected confirmation keeps proposal pending");
        action("confirm", replacement);
        expectBook("A", "FULL", bidB, bidC, replacement, offerB);
        check("REFERRED".equals(service.quotes.get(offerC).status), "confirmation retires previous A-C offer");
        check("ACTIVE".equals(service.quotes.get(offerB).status), "A-B offer remains active");
        expectBook("B", "FULL", bidB, bidC, replacement, offerB);
        expectBook("C", "FULL", bidB, bidC, replacement, offerB);
        String revision = service.quotes.get(replacement).revision;
        receive(inputs.getJSONObject(4));
        check(revision.equals(service.quotes.get(replacement).revision),
                "repeated old intake request does not roll back a confirmed record");
        action("unrefer", offerC);
        expectBook("A", "FULL", bidB, bidC, offerC, offerB);
        check("REFERRED".equals(service.quotes.get(replacement).status),
                "explicit restore cannot activate two quotes with the same business key");
        snapshot("restored");
        service = new QuoteService(service.store, new ReferenceData());
        expectBook("A", "FULL", bidB, bidC, offerC, offerB);
        expectBook("B", "FULL", bidB, bidC, offerC, offerB);
        expectBook("C", "FULL", bidB, bidC, offerC, offerB);
        check("A-C".equals(service.quotes.get(offerC).source.negotiationId),
                "reload preserves conversation identity and shared prices");
        QuoteRecord expired = ficc.api.ProtocolCodec.decode(service.quotes.get(bidB).toJson(), QuoteRecord.class);
        expired.quoteId = "TEST-EXPIRED-REFERENCE";
        expired.securityId = "NO-LONGER-IN-BASE.IB";
        for (String status : Arrays.asList("PENDING_CONFIRM", "REFERRED", "DELETED")) {
            expired.status = status;
            service.quotes.put(expired.quoteId, expired);
            JSONObject snapshot = service.handleInternalRead(
                    ficc.common.client.QuoteSvrClient.INTERNAL_PRICE_QUERY,
                    JsonUtils.object("requestId", "snapshot-" + status));
            check("0000".equals(snapshot.getString("Code")),
                    status + " with missing reference cannot break NotificationSvr initial price snapshot");
            check(snapshot.getJSONObject("Data").getJSONArray("prices").size() == 1,
                    "initial snapshot contains only the active security");
        }
        service.quotes.remove(expired.quoteId);
    }

    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (Reader reader = Files.newBufferedReader(Paths.get(args[0]), StandardCharsets.UTF_8)) {
            configuration.load(reader);
        }
        String schema = "ficc_negotiation_check_" + System.currentTimeMillis();
        configuration.setProperty("db.schema", schema);
        QTradeNegotiationSmoke test = new QTradeNegotiationSmoke();
        try (PgStore store = new PgStore(configuration)) {
            try {
                test.service = new QuoteService(store, new ReferenceData());
                test.service.publish = (topic, message) -> {
                    test.pushedTopics.add(topic);
                    if (!topic.startsWith("ficc.price.book.")) {
                        test.protocolFixtures.add(JsonUtils.deepCopy(JsonUtils.object(
                                "topic", topic, "message", message)));
                    }
                };
                test.run(JSON.parseArray(new String(Files.readAllBytes(Paths.get(args[1])), StandardCharsets.UTF_8)));
                Files.write(Paths.get(args[2]), JsonUtils.toJson(test.snapshots).getBytes(StandardCharsets.UTF_8));
                Files.write(Paths.get(args[3]), JsonUtils.toJson(test.protocolFixtures).getBytes(StandardCharsets.UTF_8));
            } finally {
                if (!schema.matches("ficc_negotiation_check_[0-9]+")) throw new IllegalStateException(schema);
                store.exec("DROP SCHEMA " + schema + " CASCADE");
            }
        }
        System.out.println("PASS negotiation integration: " + test.checks + " checks");
    }
}
