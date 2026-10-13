package ficc.quote;

import com.alibaba.fastjson2.*;
import ficc.api.*;
import ficc.common.*;
import java.nio.file.*;
import java.io.*;
import java.util.*;
import java.time.*;

/** 独立数据库 schema 的小规模功能验证；不写业务报价，不发送外部消息。 */
public final class RecommendationSmoke {
    private static int checks;
    private static final JSONArray fixtures = new JSONArray();
    private static void check(boolean ok, String message) {
        if (!ok) throw new AssertionError(message);
        checks++;
        System.out.println("PASS " + message);
    }
    private static JSONObject call(QuoteService service, String user, String topic, JSONObject request) {
        request.put("requestId", UUID.randomUUID().toString());
        JSONObject result = service.handle(user, "recommendation-smoke", topic, request);
        fixtures.add(JsonUtils.object("topic", topic, "request", request, "reply", result));
        return result;
    }
    private static JSONObject query(String start, String end) {
        return JsonUtils.object("securityId", "250001.IB", "startDate", start, "endDate", end);
    }
    private static void history(PgStore store, String id, String owner, String side, String status, String date, int revision) throws Exception {
        QuoteRecord quote = new QuoteRecord();
        quote.quoteId = id;
        quote.securityId = "250001.IB";
        quote.ownerUserId = owner;
        quote.institutionId = "institution-" + owner;
        quote.institutionName = "private-institution-" + owner;
        quote.contactId = "contact-" + owner;
        quote.contactName = "private-contact-" + owner;
        quote.side = side;
        quote.status = status;
        quote.quoteType = "1";
        quote.yieldType = "1";
        quote.price = "2.00";
        quote.quantity = "1000";
        quote.updateTime = date + "T09:00:00+08:00";
        quote.revision = Integer.toString(revision);
        store.history(JsonUtils.object("eventId", id + "-" + revision, "quoteId", id, "revision", revision,
                "operationTime", quote.updateTime, "after", quote.toJson()));
    }
    public static void main(String[] args) throws Exception {
        Properties config = new Properties();
        try (Reader reader = Files.newBufferedReader(Paths.get(args[0]))) { config.load(reader); }
        String schema = "ficc_rec_test_" + UUID.randomUUID().toString().replace("-", "");
        config.setProperty("db.schema", schema);
        String today = LocalDate.now().toString();
        String old = LocalDate.now().minusDays(31).toString();
        String start = LocalDate.now().minusDays(89).toString();
        String recommendationId;
        try {
            try (PgStore store = new PgStore(config)) {
                history(store, "a", "A", "1", "ACTIVE", today, 1);
                history(store, "a", "A", "1", "ACTIVE", today, 2);
                history(store, "a", "A", "1", "DELETED", today, 3);
                history(store, "b", "B", "1", "ACTIVE", old, 1);
                history(store, "c", "B", "2", "ACTIVE", today, 1);
                history(store, "pending", "A", "1", "PENDING_CONFIRM", today, 1);
                history(store, "pending", "A", "1", "DELETED", today, 2);
                history(store, "out-of-window", "A", "1", "ACTIVE", LocalDate.now().minusDays(366).toString(), 1);
                QuoteService service = new QuoteService(store, new ReferenceData());
                JSONObject reply = call(service, "A", "ficc.recommendation.query", query(start, today));
                check("0000".equals(reply.getString("Code")), "Recommendation query succeeds");
                JSONArray rows = reply.getJSONObject("Data").getJSONArray("recommendations");
                check(rows.size() == 3, "Both sides and all owners; pending and outside-window quotes excluded");
                JSONObject mine = null;
                double ownScore = 0;
                double oldScore = 0;
                for (int index = 0; index < rows.size(); index++) {
                    JSONObject row = rows.getJSONObject(index);
                    check(row.getIntValue("quoteCount") == 1, "Updates do not multiply quote count");
                    check(row.getJSONArray("evidence").size() == 1, "One latest confirmed snapshot per quote");
                    if ("A".equals(row.getString("ownerUserId"))) {
                        mine = row;
                        ownScore = row.getDoubleValue("score");
                        check(row.getString("institutionName").equals("private-institution-A"), "Owner sees institution");
                        check(row.getJSONArray("evidence").getJSONObject(0).getString("contactName").equals("private-contact-A"), "Owner sees evidence trader");
                        check(row.getJSONArray("evidence").getJSONObject(0).getString("quoteId").equals("a"), "Revoked formal quote still counts");
                    } else {
                        String json = row.toJSONString();
                        check(!json.contains("private-") && !json.contains("institution-B") && !json.contains("contact-B"), "Cross-owner list and evidence disclose no sensitive identity");
                        check("B".equals(row.getString("ownerUserId")), "Internal owner stays visible");
                        if ("1".equals(row.getString("side"))) oldScore = row.getDoubleValue("score");
                    }
                }
                check(ownScore > oldScore && oldScore > 0, "Time decay ranks newer equal-count evidence higher");
                recommendationId = mine.getString("recommendationId");
                JSONObject narrow = call(service, "A", "ficc.recommendation.query", query(today, today));
                check(narrow.getJSONObject("Data").getJSONArray("recommendations").size() == 2, "Date window actually changes candidates");
                JSONObject emptyRequest = query(today, today);
                emptyRequest.put("securityId", "240210.IB");
                check(call(service, "A", "ficc.recommendation.query", emptyRequest).getJSONObject("Data").getJSONArray("recommendations").isEmpty(), "No evidence gives empty, no mock fallback");
                check("1001".equals(call(service, "A", "ficc.recommendation.query", query(LocalDate.now().minusDays(365).toString(), today)).getString("Code")), "366 inclusive days rejected");
                check("0000".equals(call(service, "A", "ficc.recommendation.query", query(LocalDate.now().minusDays(364).toString(), today)).getString("Code")), "365 inclusive days allowed");
                check("1003".equals(call(service, null, "ficc.recommendation.query", query(today, today)).getString("Code")), "Unauthenticated query rejected");
                JSONObject feedback = query(start, today);
                feedback.put("recommendationId", recommendationId);
                feedback.put("feedback", "USEFUL");
                check("USEFUL".equals(call(service, "A", "ficc.recommendation.feedback", feedback).getJSONObject("Data").getString("feedback")), "Feedback stored");
                feedback.put("feedback", "NOT_USEFUL");
                check("USEFUL".equals(call(service, "A", "ficc.recommendation.feedback", feedback).getJSONObject("Data").getString("feedback")), "Duplicate feedback cannot overwrite first choice");
                check("NOT_USEFUL".equals(call(service, "B", "ficc.recommendation.feedback", feedback).getJSONObject("Data").getString("feedback")), "Feedback is scoped to viewer");
                feedback.put("recommendationId", "unknown");
                check("1002".equals(call(service, "A", "ficc.recommendation.feedback", feedback).getString("Code")), "Unknown recommendation feedback rejected");
                JSONObject again = call(service, "A", "ficc.recommendation.query", query(start, today));
                for (Object value : again.getJSONObject("Data").getJSONArray("recommendations")) {
                    JSONObject row = (JSONObject)value;
                    if (recommendationId.equals(row.getString("recommendationId"))) {
                        check("USEFUL".equals(row.getString("feedback")) && row.getDoubleValue("score") == ownScore, "Feedback visible without changing score");
                    }
                }
            }
            try (PgStore store = new PgStore(config)) {
                check("USEFUL".equals(store.scalar("SELECT feedback FROM recommendation_feedback WHERE owner_id='A' AND recommendation_id=?", recommendationId)), "Feedback survives store restart");
            }
        } finally {
            try (PgStore cleanup = new PgStore(config)) { cleanup.exec("DROP SCHEMA " + schema + " CASCADE"); }
        }
        Files.write(Paths.get(args[1]), fixtures.toJSONString().getBytes(java.nio.charset.StandardCharsets.UTF_8));
        System.out.println("PASS RecommendationSmoke: " + checks + " checks; isolated PostgreSQL schema removed");
    }
}
