package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.Filter;
import ficc.api.ProtocolCodec;
import ficc.api.QuoteHistoryRow;
import ficc.api.QuoteHistorySearchRequest;
import ficc.api.SettingRecord;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import ficc.common.client.AccessPolicy;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.io.Reader;
import java.util.Collections;
import java.util.List;
import java.util.Properties;
import java.util.UUID;

/** 小样本 PostgreSQL 功能回归：独立 schema，不连接 DPS，不写业务环境数据，不做性能压测。 */
public final class HistorySearchSmoke {
    private static int checks;

    private static void check(boolean condition, String message) {
        if (!condition) {
            throw new AssertionError(message);
        }
        checks++;
        System.out.println("PASS " + message);
    }

    private static void event(PgStore store, String id, int revision, String owner,
            String time, String price, String status) throws Exception {
        JSONObject quote = JsonUtils.object("quoteId", id, "ownerUserId", owner,
                "securityId", "250001.IB", "side", "1", "price", price,
                "quantity", "1000", "quoteType", "1", "yieldType", "1",
                "status", status, "revision", Integer.toString(revision),
                "tradeDate", "2026-10-19", "updateTime", time);
        store.history(JsonUtils.object("eventId", id + "-" + revision, "quoteId", id,
                "revision", revision, "operationTime", time, "after", quote));
    }

    private static QuoteHistorySearchRequest request() {
        QuoteHistorySearchRequest request = new QuoteHistorySearchRequest();
        request.fromDate = "2026-10-15";
        request.toDate = "2026-10-16";
        return request;
    }

    private static JSONObject query(QuoteService service, String user, String session, JSONObject body) {
        body.put("requestId", UUID.randomUUID().toString());
        return service.handle(user, session, "ficc.quote.history.search", body);
    }

    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (Reader reader = Files.newBufferedReader(Paths.get(args[0]))) {
            configuration.load(reader);
        }
        String schema = "ficc_history_test_" + UUID.randomUUID().toString().replace("-", "");
        configuration.setProperty("db.schema", schema);
        configuration.setProperty("identity.mode", "simulator");
        configuration.setProperty("identity.adminUserIds", "ADMIN");
        configuration.setProperty("identity.internalUserIds", "INTERNAL");
        try (PgStore store = new PgStore(configuration)) {
            try {
                event(store, "A", 1, "U1", "2026-10-15T00:00:00+08:00", "2", "ACTIVE");
                event(store, "A", 2, "U1", "2026-10-15T12:00:00+08:00", "3", "REFERRED");
                event(store, "A", 3, "U1", "2026-10-15T12:00:00+08:00", "4", "DELETED");
                event(store, "A", 4, "U1", "2026-10-16T23:59:59.999+08:00", "5", "ACTIVE");
                event(store, "A", 5, "U1", "2026-10-17T00:00:00+08:00", "6", "ACTIVE");
                event(store, "B", 1, "U2", "2026-10-15T01:00:00+08:00", "1", "ACTIVE");
                event(store, "C", 1, "U1", "2026-10-15T02:00:00+08:00", "7", "PENDING_CONFIRM");
                store.history(JsonUtils.object("eventId", "null", "quoteId", "null", "revision", 1,
                        "operationTime", "2026-10-15T02:00:00+08:00", "after", null));
                ReferenceData references = new ReferenceData();
                LocalSearchSvrClient search = new LocalSearchSvrClient(store, references,
                        Collections.emptyMap(), new QuoteMatcher(references,
                        Collections.<String, SettingRecord>emptyMap()), new AccessPolicy(configuration), null);
                QuoteHistorySearchRequest request = request();
                List<QuoteHistoryRow> rows = search.searchHistory("U1", request);
                check(rows.size() == 3, "one snapshot per day and quote, own records only");
                check("3".equals(rows.get(0).quote.revision), "equal timestamps use highest revision");
                check("DELETED".equals(rows.get(0).quote.status), "terminal snapshot retained");
                check("2026-10-15".equals(rows.get(0).tradeDate)
                        && "2026-10-19".equals(rows.get(0).quote.tradeDate), "operation day differs from trade date");
                check("5".equals(rows.get(2).quote.price), "inclusive end day excludes next midnight");
                check(search.searchHistory("U2", request).size() == 1, "other owner isolated");
                check(search.searchHistory("ADMIN", request).size() == 4, "administrator scope preserved");
                check(search.searchHistory("INTERNAL", request).size() == 4, "internal scope preserved");
                check(search.searchHistory(null, request).isEmpty(), "null identity sees no history");
                request.filter = ProtocolCodec.decode(JsonUtils.object("conditions", JsonUtils.array(
                        JsonUtils.object("field", "bidPrice", "operator", "EQ", "value", "2"))), Filter.class);
                check(search.searchHistory("U1", request).isEmpty(), "filter after latest snapshot, no resurrection");
                request.filter = ProtocolCodec.decode(JsonUtils.object("conditions", JsonUtils.array(
                        JsonUtils.object("field", "bidPrice", "operator", "BETWEEN", "value", JsonUtils.array("4", "5")),
                        JsonUtils.object("field", "bidQuantity", "operator", "GTE", "value", "1000"))), Filter.class);
                check(search.searchHistory("U1", request).size() == 2, "combined numeric bounds");
                request.filter = ProtocolCodec.decode(JsonUtils.object("conditions", JsonUtils.array(
                        JsonUtils.object("field", "bondRating", "operator", "EQ", "value", "AAA"))), Filter.class);
                references.securities().get("250001.IB").put("bondRating", "AAA");
                check(search.searchHistory("U1", request).size() == 3, "current reference data used");
                references.securities().get("250001.IB").put("bondRating", "AA+");
                check(search.searchHistory("U1", request).isEmpty(), "reference refresh affects next query");
                request.fromDate = "invalid";
                try {
                    search.searchHistory("U1", request);
                    throw new AssertionError("invalid date accepted");
                } catch (ApiException expected) {
                    check("1001".equals(expected.code), "invalid dates reported as validation errors");
                }
                try {
                    store.scanRows("SELECT '{}'::jsonb", row -> { throw new IllegalStateException("callback"); });
                    throw new AssertionError("callback failure swallowed");
                } catch (IllegalStateException expected) {
                    check("1".equals(store.scalar("SELECT 1")), "connection reusable after callback failure");
                }
                int[] streamed = {0};
                store.scanRows("SELECT jsonb_build_object('n', n) FROM generate_series(1,257) n",
                        row -> streamed[0]++);
                check(streamed[0] == 257, "reader traverses multiple fetch batches");
                store.begin();
                event(store, "ROLLBACK", 1, "U1", "2026-10-15T03:00:00+08:00", "2", "ACTIVE");
                store.scanRows("SELECT '{}'::jsonb", row -> {});
                store.rollback();
                check("0".equals(store.scalar("SELECT count(*) FROM history WHERE quote_id='ROLLBACK'")),
                        "reader does not commit caller-owned transaction");
                QuoteService service = new QuoteService(store, references, new AccessPolicy(configuration));
                JSONObject first = query(service, "U1", "SESSION", JsonUtils.object(
                        "fromDate", "2026-10-15", "toDate", "2026-10-16", "limit", 1));
                check("0000".equals(first.getString("Code")), "history endpoint succeeds");
                JSONObject data = first.getJSONObject("Data");
                check(data.getIntValue("totalCount") == 3 && data.getJSONArray("rows").size() == 1,
                        "page limit and totalCount preserved");
                String cursor = data.getString("nextCursor");
                event(store, "C", 2, "U1", "2026-10-15T03:00:00+08:00", "8", "DELETED");
                JSONObject second = query(service, "U1", "SESSION", JsonUtils.object("cursor", cursor)).getJSONObject("Data");
                check("7".equals(second.getJSONArray("rows").getJSONObject(0).getJSONObject("quote").getString("price")),
                        "cursor remains a stable snapshot after a write");
                check("1007".equals(query(service, "U2", "SESSION", JsonUtils.object("cursor", cursor)).getString("Code")),
                        "cursor cannot cross users");
                check("1007".equals(query(service, "U1", "OTHER", JsonUtils.object("cursor", cursor)).getString("Code")),
                        "cursor cannot cross sessions");
                check("2".equals(store.scalar("SELECT count(*) FROM pg_indexes WHERE schemaname=?"
                        + " AND indexname IN ('history_owner_time_idx','history_time_idx')", schema)),
                        "history date and owner indexes installed");
            } finally {
                store.exec("DROP SCHEMA " + schema + " CASCADE");
            }
        }
        System.out.println("History search functional checks passed: " + checks);
    }
}
