package ficc.quote;

import com.alibaba.fastjson2.*;

import ficc.common.*;
import ficc.api.ProtocolBoundary;

import java.io.BufferedReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.*;

/** Bounded functional test: five quotes only. Not a transport or performance test. */
public final class BusinessSmoke {
    static QuoteService svc;
    static String sid = "functional-smoke", prefix = "SMOKE-" + UUID.randomUUID();
    static int checks;
    static final JSONArray protocolFixtures = new JSONArray();

    static void check(boolean condition, String name) {
        if (!condition) throw new AssertionError(name);
        checks++;
        System.out.println("PASS " + name);
    }

    static JSONObject response(String user, String topic, JSONObject req) {
        req.putIfAbsent("requestId", UUID.randomUUID().toString());
        JSONObject reply = svc.handle(user, sid, topic, req);
        protocolFixtures.add(JsonUtils.deepCopy(JsonUtils.object(
                "topic", topic, "request", req, "reply", reply)));
        return reply;
    }

    static JSONObject call(String topic, JSONObject req) {
        JSONObject r = response("U-001", topic, req);
        if (!"0000".equals(r.getString("Code"))) throw new AssertionError(topic + ": " + r);
        return r.getJSONObject("Data");
    }

    static JSONObject quote(String id) {
        return call("ficc.quote.get", JsonUtils.object("quoteId", id)).getJSONObject("quote");
    }

    static JSONObject input(String client, String px) {
        return JsonUtils.object(
                "clientQuoteId",
                prefix + client,
                "securityId",
                "250001.IB",
                "side",
                "1",
                "quoteType",
                "1",
                "yieldType",
                "1",
                "price",
                px,
                "quantity",
                "1000",
                "settlType",
                "1",
                "tradeDate",
                java.time.LocalDate.now().toString());
    }

    static JSONObject action(String op, String id) {
        return call(
                        "ficc.quote." + op,
                        JsonUtils.object(
                                "quotes",
                                JsonUtils.array(
                                        JsonUtils.object(
                                                "quoteId",
                                                id,
                                                "expectedRevision",
                                                quote(id).getString("revision")))))
                .getJSONArray("results")
                .getJSONObject(0);
    }

    static int depthSize() {
        return call(
                        "ficc.price.query",
                        JsonUtils.object(
                                "securityIds",
                                JsonUtils.array("250001.IB"),
                                "mode",
                                "DEPTH",
                                "depth",
                                2))
                .getJSONArray("prices")
                .getJSONObject(0)
                .getJSONObject("marketData")
                .getJSONArray("NoMDEntries")
                .size();
    }

    public static void main(String[] args) throws Exception {
        Properties cfg = new Properties();
        try (BufferedReader r = Files.newBufferedReader(Paths.get(args[0]))) {
            cfg.load(r);
        }
        cfg.setProperty("db.schema", "ficc_smoke_" + System.currentTimeMillis());
        List<JSONObject> pushes = new ArrayList<>();
        String id;
        JSONObject create =
                JsonUtils.object(
                        "requestId",
                        prefix + "create",
                        "quotes",
                        JsonUtils.array(
                                input("a", "2.5000"),
                                input("b", "2.5000"),
                                input("c", "2.6000"),
                                input("d", "2.7000")));
        try (PgStore db = new PgStore(cfg)) {
            svc = new QuoteService(db, new ReferenceData());
            svc.publish =
                    (t, v) -> {
                        JSONObject wire = ProtocolBoundary.push(t, v);
                        if (!JsonUtils.canonicalHash(v).equals(JsonUtils.canonicalHash(wire))) {
                            throw new AssertionError("Protocol changed push: " + t);
                        }
                        protocolFixtures.add(JsonUtils.object("topic", t, "message", JsonUtils.deepCopy(wire)));
                        if (pushes.size() < 200)
                            pushes.add(
                                    JsonUtils.object("topic", t, "message", JsonUtils.deepCopy(v)));
                    };
            JSONObject first = call("ficc.quote.create", create);
            JSONArray created = first.getJSONArray("results");
            check(created.size() == 4, "batch creates exactly four quotes");
            for (JSONObject q : created.toJavaList(JSONObject.class))
                check("0000".equals(q.getString("Code")), "create accepted");
            id = created.getJSONObject(0).getString("quoteId");
            check(
                    JsonUtils.canonicalHash(first)
                            .equals(JsonUtils.canonicalHash(call("ficc.quote.create", create))),
                    "idempotent request does not duplicate quotes");
            check(
                    "PENDING_CONFIRM".equals(quote(id).getString("status")),
                    "new quote starts pending");
            JSONObject stream = call("ficc.quote.stream.open", JsonUtils.object());
            svc.views.authorize(sid, "U-001", stream.getString("pushTopic"));
            check(
                    svc.views.image(stream.getString("pushTopic"), sid).isEmpty(),
                    "quote subscription has no snapshot");
            JSONObject filter =
                    JsonUtils.object(
                            "conditions",
                            JsonUtils.array(
                                    JsonUtils.object(
                                            "field",
                                            "securityId",
                                            "operator",
                                            "EQ",
                                            "value",
                                            "250001.IB")));
            call(
                    "ficc.quote.view.open",
                    JsonUtils.object(
                            "viewId",
                            prefix,
                            "ownerScope",
                            "MINE",
                            "status",
                            JsonUtils.array("ACTIVE"),
                            "filter",
                            filter,
                            "pageSize",
                            20));
            for (JSONObject q : created.toJavaList(JSONObject.class))
                check(
                        "0000".equals(action("confirm", q.getString("quoteId")).getString("Code")),
                        "confirm activates quote");
            check(depthSize() == 3, "depth two includes every quote at two price levels");
            InternalSyncChecks.verify(svc, 4);
            check(
                    pushes.stream()
                            .anyMatch(
                                    x ->
                                            x.getString("topic")
                                                            .equals(stream.getString("pushTopic"))
                                                    && x.getJSONObject("message")
                                                                    .getJSONArray("quotes")
                                                            != null),
                    "confirmation inserts quote into subscribed view");
            check(
                    pushes.stream()
                            .filter(x -> x.getString("topic").startsWith("ficc.quote.report."))
                            .noneMatch(x -> x.getJSONObject("message").containsKey("books")),
                    "quote push never carries price books");
            JSONObject conflict =
                    call(
                                    "ficc.quote.update",
                                    JsonUtils.object(
                                            "quotes",
                                            JsonUtils.array(
                                                    JsonUtils.object(
                                                            "quoteId",
                                                            id,
                                                            "expectedRevision",
                                                            "1",
                                                            "price",
                                                            "2.4000"))))
                            .getJSONArray("results")
                            .getJSONObject(0);
            check("1004".equals(conflict.getString("Code")), "stale revision rejected");
            JSONObject other =
                    response(
                            "U-002",
                            "ficc.quote.refer",
                            JsonUtils.object(
                                    "quotes",
                                    JsonUtils.array(
                                            JsonUtils.object(
                                                    "quoteId",
                                                    id,
                                                    "expectedRevision",
                                                    quote(id).getString("revision")))));
            check(
                    "1003"
                            .equals(
                                    other.getJSONObject("Data")
                                            .getJSONArray("results")
                                            .getJSONObject(0)
                                            .getString("Code")),
                    "non-owner cannot modify quote");
            check("0000".equals(action("refer", id).getString("Code")), "refer accepted");
            check("REFERRED".equals(quote(id).getString("status")), "refer changes state");
            check("0000".equals(action("unrefer", id).getString("Code")), "unrefer accepted");
            check(id.equals(quote(id).getString("quoteId")), "unrefer preserves quote identity");
            String rev = quote(id).getString("revision");
            JSONObject bad =
                    call(
                                    "ficc.quote.update",
                                    JsonUtils.object(
                                            "quotes",
                                            JsonUtils.array(
                                                    JsonUtils.object(
                                                            "quoteId",
                                                            id,
                                                            "expectedRevision",
                                                            rev,
                                                            "price",
                                                            "2.50005"))))
                            .getJSONArray("results")
                            .getJSONObject(0);
            check(
                    "1001".equals(bad.getString("Code"))
                            && rev.equals(quote(id).getString("revision")),
                    "invalid precision leaves state unchanged");
            JSONObject change =
                    call(
                            "ficc.quote.update",
                            JsonUtils.object(
                                    "quotes",
                                    JsonUtils.array(
                                            JsonUtils.object(
                                                    "quoteId",
                                                    id,
                                                    "expectedRevision",
                                                    rev,
                                                    "securityId",
                                                    "250002.IB"))));
            check(
                    "0000"
                            .equals(
                                    change.getJSONArray("results")
                                            .getJSONObject(0)
                                            .getString("Code")),
                    "active quote changes security");
            check(
                    pushes.stream()
                            .anyMatch(
                                    x -> x.getString("topic").equals("ficc.price.book.250001.IB")),
                    "old security receives price refresh");
            check(
                    call("ficc.quote.history.query", JsonUtils.object("quoteId", id))
                                    .getJSONArray("records")
                                    .size()
                            == 5,
                    "continuous history across refer/unrefer");
            String date = java.time.LocalDate.now().toString();
            JSONObject hist =
                    call(
                            "ficc.quote.history.search",
                            JsonUtils.object(
                                    "fromDate",
                                    date,
                                    "toDate",
                                    date,
                                    "filter",
                                    JsonUtils.object(
                                            "conditions",
                                            JsonUtils.array(
                                                    JsonUtils.object(
                                                            "field",
                                                            "bondRating",
                                                            "operator",
                                                            "EQ",
                                                            "value",
                                                            "AAA")))));
            check(hist.getJSONArray("rows").size() == 4, "history searches real persisted quotes");
            svc.refs.securities().get("250002.IB").put("bondRating", "AA+");
            JSONObject currentFilter =
                    JsonUtils.object(
                            "conditions",
                            JsonUtils.array(
                                    JsonUtils.object(
                                            "field",
                                            "bondRating",
                                            "operator",
                                            "EQ",
                                            "value",
                                            "AA+")));
            check(
                    call(
                                            "ficc.quote.history.search",
                                            JsonUtils.object(
                                                    "fromDate",
                                                    date,
                                                    "toDate",
                                                    date,
                                                    "filter",
                                                    currentFilter))
                                    .getJSONArray("rows")
                                    .size()
                            == 1,
                    "historical filter uses current bond rating");
            for (JSONObject q : created.toJavaList(JSONObject.class)) {
                String qid = q.getString("quoteId");
                action("refer", qid);
                action("delete", qid);
            }
            check(
                    "DELETED".equals(quote(id).getString("status")),
                    "delete persists terminal status");
            JSONObject qi = input("intake", "2.5000");
            qi.remove("clientQuoteId");
            qi.put("externalQuoteId", prefix + "qt");
            qi.put("externalRevision", "1");
            qi.put("action", "NEW");
            JSONObject intake =
                    JsonUtils.object(
                            "requestId",
                            prefix + "intake",
                            "sourceType",
                            "QT",
                            "externalMessageId",
                            prefix + "message",
                            "sourceUserId",
                            "QT-U-88",
                            "sourceTime",
                            BusinessClock.currentTime(),
                            "quotes",
                            JsonUtils.array(qi));
            JSONObject accepted =
                    response("S-INTAKE", "ficc.quote.intake.apply", intake).getJSONObject("Data");
            String intakeId =
                    accepted.getJSONArray("results").getJSONObject(0).getString("quoteId");
            check(intakeId != null, "intake creates fifth quote");
            // Emulate a missing outer completion receipt after an item transaction committed.
            db.update(
                    "UPDATE requests SET results='[]'::jsonb WHERE owner_id=? AND topic=? AND"
                            + " request_id=?",
                    "S-INTAKE",
                    "ficc.quote.intake.apply",
                    intake.getString("requestId"));
            JSONObject resumed =
                    response("S-INTAKE", "ficc.quote.intake.apply", intake).getJSONObject("Data");
            check(
                    JsonUtils.canonicalHash(accepted).equals(JsonUtils.canonicalHash(resumed))
                            && svc.quotes.size() == 5,
                    "intake resumes committed NEW without duplicate");
            qi.put("action", "UPDATE");
            qi.put("externalRevision", "2");
            qi.put("price", "2.4000");
            intake.put("requestId", prefix + "intake-update");
            intake.put("externalMessageId", prefix + "message-update");
            JSONObject updated =
                    response("S-INTAKE", "ficc.quote.intake.apply", intake).getJSONObject("Data");
            check(
                    "2".equals(quote(intakeId).getString("revision")),
                    "intake update advances revision once");
            db.update(
                    "UPDATE requests SET results='[]'::jsonb WHERE owner_id=? AND topic=? AND"
                            + " request_id=?",
                    "S-INTAKE",
                    "ficc.quote.intake.apply",
                    intake.getString("requestId"));
            check(
                    JsonUtils.canonicalHash(updated)
                                    .equals(
                                            JsonUtils.canonicalHash(
                                                    response(
                                                                    "S-INTAKE",
                                                                    "ficc.quote.intake.apply",
                                                                    intake)
                                                            .getJSONObject("Data")))
                            && "2".equals(quote(intakeId).getString("revision")),
                    "intake resumes UPDATE without stale-revision failure");
            action("delete", intakeId);
        }
        try (PgStore db = new PgStore(cfg)) {
            svc = new QuoteService(db, new ReferenceData());
            check(
                    "DELETED".equals(quote(id).getString("status")),
                    "PostgreSQL state survives service recreation");
            check(
                    call("ficc.quote.create", create).getJSONArray("results").size() == 4,
                    "idempotency receipt survives restart");
        }
        Files.write(
                Paths.get(args[1]),
                JsonUtils.toJson(
                                JsonUtils.object(
                                        "test",
                                        "business-direct-postgresql",
                                        "checks",
                                        checks,
                                        "quotesCreated",
                                        5,
                                        "transportTested",
                                        false,
                                        "result",
                                        "PASS"))
                        .getBytes(StandardCharsets.UTF_8));
        Files.write(Paths.get(args[1] + ".protocol.json"),
                JsonUtils.toJson(protocolFixtures).getBytes(StandardCharsets.UTF_8));
        System.out.println(
                "PASS " + checks + " checks; five quotes; DPS/GW not tested by this probe.");
    }
}
