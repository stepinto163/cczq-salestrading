package ficc.notification;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.JdbcStore;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import java.io.Reader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.*;
import java.util.stream.Collectors;

/** 提醒业务组合回归：真实服务核心及 PostgreSQL 临时 schema，不冒充桌面或网络恢复测试。 */
public final class NotificationScenarioMatrix {
    private static final String SECURITY = "250001.IB";
    private static final String[] TYPES = {"WATCHLIST", "FILTER_GROUP", "COUNTER_QUOTE", "MY_BEST"};
    private static int checks;
    private final JdbcStore store;
    private NotificationService service;
    private List<JSONObject> settings;

    private NotificationScenarioMatrix(JdbcStore store) {
        this.store = store;
    }

    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (Reader reader = Files.newBufferedReader(Paths.get(args[0]), StandardCharsets.UTF_8)) {
            configuration.load(reader);
        }
        String schema = "ficc_notification_matrix_" + System.currentTimeMillis();
        configuration.setProperty("db.schema", schema);
        try (JdbcStore store = new JdbcStore(configuration, "notification_")) {
            try {
                NotificationScenarioMatrix matrix = new NotificationScenarioMatrix(store);
                matrix.lifecycle();
                matrix.snapshotFields();
                matrix.preferencesAndReasons();
                matrix.filterCombinations();
                matrix.comparisonsAndUsers();
                matrix.recoveryAndPaging();
            } finally {
                if (!schema.matches("ficc_notification_matrix_[0-9]+")) {
                    throw new IllegalStateException("Unexpected test schema");
                }
                store.exec("DROP SCHEMA " + schema + " CASCADE");
            }
        }
        System.out.println("PASS notification scenario matrix: " + checks + " checks");
    }

    private void reset() throws Exception {
        service = new NotificationService(store);
        store.exec("DELETE FROM notifications");
        store.exec("DELETE FROM notification_reads");
        store.exec("DELETE FROM notification_preferences");
        service = new NotificationService(store);
        String market = new ReferenceData().require(SECURITY).getString("market");
        JSONObject watch = JsonUtils.object("watchlistId", "W1", "userId", "U1", "scope", "MARKET",
                "market", market, "securities", JsonUtils.array(JsonUtils.object("securityId", SECURITY)));
        JSONObject filter = JsonUtils.object("filterGroupId", "G1", "userId", "U1", "scope", "MARKET",
                "market", market, "subgroups", JsonUtils.array(JsonUtils.object("conditions", JsonUtils.array(
                        JsonUtils.object("field", "side", "operator", "EQ", "value", "1")))));
        settings = new ArrayList<>(Arrays.asList(watch, filter));
        snapshot(Arrays.asList(quote("A", "U1", "1"), quote("B", "U2", "1"), quote("C", "U2", "2")));
    }

    private static JSONObject quote(String id, String owner, String side) {
        return JsonUtils.object("quoteId", id, "ownerUserId", owner, "securityId", SECURITY,
                "status", "ACTIVE", "revision", "1", "side", side, "price", "2.5",
                "quoteType", "1", "yieldType", "1", "settlType", "2", "quantity", "100",
                "quantityUnit", "WAN", "updateTime", "2026-10-18T01:00:00Z");
    }

    private void snapshotFields() throws Exception {
        reset();
        JSONObject changed = JsonUtils.deepCopy(service.quotes.get("A"));
        changed.put("revision", "2");
        changed.put("institutionId", "I1");
        changed.put("institutionName", "测试机构甲");
        changed.put("contactId", "C1");
        changed.put("contactName", "测试联系人甲");
        changed.put("quoteTime", "2026-10-18T01:02:03Z");
        changed.put("createTime", "2026-10-18T01:00:00Z");
        changed.put("remark", "not part of notification snapshot");
        check(service.quoteMessage(event(changed, service.quoteSequence + 1)), "snapshot field update accepted");
        book();
        int checkedSnapshots = 0;
        for (JSONObject row : service.select("U1", new JSONObject())) {
            JSONObject wire = ficc.api.ProtocolCodec.decode(row, ficc.api.NotificationRecord.class).toJson();
            for (JSONObject trigger : wire.getJSONArray("triggers").toJavaList(JSONObject.class)) {
                for (JSONObject snapshot : trigger.getJSONArray("quotes").toJavaList(JSONObject.class)) {
                    if (!"A".equals(snapshot.getString("quoteId"))) continue;
                    for (String field : Arrays.asList("institutionId", "institutionName", "contactId", "contactName",
                            "quoteTime", "createTime", "ownerUserId", "price", "quantity", "side")) {
                        check(changed.get(field).equals(snapshot.get(field)), "trigger snapshot preserves " + field);
                    }
                    check(!snapshot.containsKey("remark"), "snapshot excludes unrelated remark");
                    checkedSnapshots++;
                }
            }
        }
        check(checkedSnapshots > 0, "snapshot assertions exercised actual notification records");
    }

    private void snapshot(List<JSONObject> quotes) throws Exception {
        service.replaceQuotes(JsonUtils.object("epoch", "E", "sequence", "0"), quotes, settings);
        book();
    }

    // 输入已排序价格簿；测试 NotificationSvr 的成员/版本校验，不替代 QuoteSvr 排序测试。
    private void book() throws Exception {
        JSONObject refs = new JSONObject();
        JSONArray entries = new JSONArray();
        for (JSONObject quote : service.quotes.values()) {
            if (!"ACTIVE".equals(quote.getString("status"))) continue;
            String id = "FICC:" + quote.getString("quoteId");
            refs.put(id, JsonUtils.object("quoteRevision", quote.getString("revision")));
            entries.add(JsonUtils.object("MDEntryID", id, "MDEntryType",
                    "1".equals(quote.getString("side")) ? "0" : "1", "MDEntryPx", quote.getString("price")));
        }
        check(service.priceMessage(JsonUtils.object("kind", "SNAPSHOT", "streamId", UUID.randomUUID().toString(),
                "sequence", "0", "prices", JsonUtils.array(JsonUtils.object("securityId", SECURITY,
                "quoteRefs", refs, "marketData", JsonUtils.object("NoMDEntries", entries))))), "price snapshot accepted");
    }

    private void change(String id, String status) throws Exception {
        JSONObject quote = JsonUtils.deepCopy(service.quotes.get(id));
        quote.put("status", status);
        quote.put("revision", String.valueOf(quote.getLongValue("revision") + 1));
        check(service.quoteMessage(event(quote, service.quoteSequence + 1)), "ordered " + status + " event accepted");
        book();
    }

    private JSONObject event(JSONObject quote, long sequence) {
        return JsonUtils.object("epoch", "E", "events", JsonUtils.array(JsonUtils.object("sequence", sequence,
                "type", "QUOTE", "quoteId", quote.getString("quoteId"), "quote", quote)));
    }

    private Set<String> keys(String user, String type, String quoteId) {
        Set<String> keys = new HashSet<>();
        for (JSONObject row : service.select(user, new JSONObject())) {
            for (JSONObject trigger : row.getJSONArray("triggers").toJavaList(JSONObject.class)) {
                if (type.equals(trigger.getString("type")) && trigger.getJSONArray("quoteIds").contains(quoteId)) {
                    keys.add(trigger.getString("alertKey"));
                }
            }
        }
        return keys;
    }

    private void lifecycle() throws Exception {
        for (String type : TYPES) {
            reset();
            check(keys("U1", type, "A").size() == 1, type + " initial active trigger");
            change("A", "PENDING_CONFIRM");
            check(keys("U1", type, "A").isEmpty(), type + " pending excluded");
            change("A", "ACTIVE");
            Set<String> confirmed = keys("U1", type, "A");
            check(confirmed.size() == 1, type + " confirmation creates trigger");
            change("A", "ACTIVE");
            Set<String> edited = keys("U1", type, "A");
            check(edited.size() == 1 && !edited.equals(confirmed), type + " active edit replaces version");
            change("A", "REFERRED");
            check(keys("U1", type, "A").isEmpty(), type + " Refer removes trigger");
            change("A", "ACTIVE");
            check(keys("U1", type, "A").size() == 1 && !keys("U1", type, "A").equals(edited),
                    type + " Unrefer creates current version");
            change("A", "DELETED");
            check(keys("U1", type, "A").isEmpty(), type + " delete removes trigger");
        }
    }

    private void preferencesAndReasons() throws Exception {
        reset();
        String[] switches = {"watchlistEnabled", "filterGroupEnabled", "counterQuoteEnabled", "myBestEnabled"};
        for (int mask = 0; mask < 16; mask++) {
            JSONObject request = JsonUtils.object("expectedRevision", service.preference("U1").getString("revision"));
            for (int index = 0; index < switches.length; index++) {
                request.put(switches[index], (mask & (1 << index)) != 0);
            }
            call("U1", "preference.save", request);
            for (int index = 0; index < TYPES.length; index++) {
                check(!keys("U1", TYPES[index], "A").isEmpty() == ((mask & (1 << index)) != 0),
                        "preference mask=" + mask + " type=" + TYPES[index]);
            }
        }
        Set<String> original = keys("U1", "WATCHLIST", "A");
        check(keys("U1", "FILTER_GROUP", "C").isEmpty(), "Bid filter excludes Ofr while watch includes it");
        JSONObject duplicate = JsonUtils.deepCopy(settings.get(0));
        duplicate.put("watchlistId", "W2");
        settings.add(duplicate);
        snapshot(new ArrayList<>(service.quotes.values()));
        check(keys("U1", "WATCHLIST", "A").equals(original), "overlapping groups do not duplicate trigger");
        settings.remove(0);
        snapshot(new ArrayList<>(service.quotes.values()));
        check(keys("U1", "WATCHLIST", "A").equals(original), "removing one matching group preserves trigger");
        settings.removeIf(group -> group.containsKey("watchlistId"));
        snapshot(new ArrayList<>(service.quotes.values()));
        check(keys("U1", "WATCHLIST", "A").isEmpty() && keys("U1", "FILTER_GROUP", "A").size() == 1,
                "removing one reason preserves other reason");
        check(keys("U2", "FILTER_GROUP", "A").isEmpty(), "saved groups belong only to their user");
        check(service.select("U3", new JSONObject()).isEmpty(), "uninterested user receives no notification");
    }

    private void filterCombinations() throws Exception {
        reset();
        JSONObject first = quote("A", "U1", "1");
        JSONObject second = quote("B", "U2", "1");
        second.put("settlType", "1");
        JSONObject third = quote("C", "U2", "2");
        List<JSONObject> quotes = Arrays.asList(first, second, third);
        String[][] values = {{"1", "2"}, {"1", "1"}, {"2", "2"}, {"2", "1"}};
        String[] expected = {"A", "B", "C", ""};
        JSONObject group = settings.get(1);
        for (int index = 0; index < values.length; index++) {
            group.put("subgroups", JsonUtils.array(JsonUtils.object("conditions", JsonUtils.array(
                    JsonUtils.object("field", "side", "operator", "EQ", "value", values[index][0]),
                    JsonUtils.object("field", "settlType", "operator", "EQ", "value", values[index][1])))));
            snapshot(quotes);
            for (String id : Arrays.asList("A", "B", "C")) {
                check(!keys("U1", "FILTER_GROUP", id).isEmpty() == id.equals(expected[index]),
                        "filter AND side=" + values[index][0] + " settlement=" + values[index][1] + " quote=" + id);
            }
        }
        group.put("subgroups", JsonUtils.array(JsonUtils.object("conditions", JsonUtils.array(
                JsonUtils.object("field", "side", "operator", "EQ", "value", "1"),
                JsonUtils.object("field", "side", "operator", "EQ", "value", "2"),
                JsonUtils.object("field", "settlType", "operator", "EQ", "value", "2")))));
        snapshot(quotes);
        check(!keys("U1", "FILTER_GROUP", "A").isEmpty() && !keys("U1", "FILTER_GROUP", "C").isEmpty()
                && keys("U1", "FILTER_GROUP", "B").isEmpty(), "same-field OR combines with cross-field AND");
        group.put("scope", "NOTIFICATION");
        snapshot(quotes);
        check(keys("U1", "FILTER_GROUP", "A").isEmpty(), "non-market group is not a trigger source");
        group.put("scope", "MARKET");
        group.put("market", "CREDIT");
        snapshot(quotes);
        check(keys("U1", "FILTER_GROUP", "A").isEmpty(), "other market group cannot trigger this bond");
    }

    private void comparisonsAndUsers() throws Exception {
        reset();
        check(keys("U1", "MY_BEST", "A").size() == 1 && keys("U2", "MY_BEST", "B").size() == 1,
                "tied best quotes notify their respective owners");
        check(keys("U1", "MY_BEST", "B").isEmpty(), "other owner best is not my best");
        check(keys("U1", "COUNTER_QUOTE", "C").size() == 1 && keys("U2", "COUNTER_QUOTE", "A").size() == 1,
                "bilateral pair is visible to both participating owners");
        check(keys("U1", "COUNTER_QUOTE", "B").isEmpty(), "same side is not a counterquote");
        JSONObject own = quote("A", "U1", "1");
        JSONObject other = quote("B", "U2", "1");
        other.put("price", "2.4");
        snapshot(Arrays.asList(other, own));
        check(keys("U1", "MY_BEST", "A").isEmpty() && keys("U2", "MY_BEST", "B").size() == 1,
                "competitor moves ahead in sorted book");
        for (String field : Arrays.asList("side", "quoteType", "yieldType", "settlType")) {
            other = quote("B", "U2", "1");
            other.put(field, "2".equals(other.getString(field)) ? "1" : "2");
            snapshot(Arrays.asList(own, other));
            check(keys("U1", "MY_BEST", "A").isEmpty(), "incomparable " + field + " does not create best");
        }
        other = quote("C", "U2", "2");
        other.put("price", "99");
        other.put("quoteType", "2");
        other.put("settlType", "1");
        snapshot(Arrays.asList(own, other));
        check(keys("U1", "COUNTER_QUOTE", "A").size() == 1, "counter does not require price crossing or same settlement/type");
        other.put("ownerUserId", "U1");
        snapshot(Arrays.asList(own, other));
        check(keys("U1", "COUNTER_QUOTE", "A").isEmpty(), "own opposite quote cannot counter itself");
    }

    private JSONObject call(String user, String operation, JSONObject request) {
        request.put("requestId", UUID.randomUUID().toString());
        JSONObject reply = service.handle(user, "matrix-" + user, "ficc.notification." + operation, request);
        check("0000".equals(reply.getString("Code")), operation + " response: " + reply.getString("Msg"));
        return reply.getJSONObject("Data");
    }

    private void recoveryAndPaging() throws Exception {
        reset();
        List<String> ids = service.select("U1", new JSONObject()).stream()
                .map(row -> row.getString("notificationId")).collect(Collectors.toList());
        call("U1", "read", JsonUtils.object("notificationIds", ids, "read", true));
        check(service.select("U1", JsonUtils.object("readState", "UNREAD")).isEmpty(), "mark all read");
        List<JSONObject> quotes = new ArrayList<>(service.quotes.values());
        Set<String> original = keys("U1", "WATCHLIST", "A");
        service = new NotificationService(store);
        snapshot(quotes);
        check(keys("U1", "WATCHLIST", "A").equals(original), "service reconstruction retains trigger identity");
        check(service.select("U1", JsonUtils.object("readState", "UNREAD")).isEmpty(), "read state survives database reload");
        long tail = store.tail();
        snapshot(quotes);
        check(store.tail() == tail, "identical upstream resnapshot emits no notification");
        JSONObject next = JsonUtils.deepCopy(quotes.get(0));
        next.put("revision", "2");
        check(!service.quoteMessage(event(next, 2)), "quote sequence gap requests resync");
        check(store.tail() == tail && keys("U1", "WATCHLIST", "A").equals(original), "gap cannot apply missing-order quote");
        JSONObject wrongEpoch = event(next, 1);
        wrongEpoch.put("epoch", "RESTARTED");
        check(!service.quoteMessage(wrongEpoch), "changed quote epoch requires full synchronization");
        check(!service.priceMessage(JsonUtils.object("kind", "DELTA", "streamId", "wrong", "sequence", 1,
                "prices", new JSONArray())), "wrong price stream rejected");
        check(service.quoteMessage(event(next, 1)), "current ordered version accepted after rejected gap");
        check(keys("U1", "MY_BEST", "A").isEmpty(), "stale book must not calculate new-version best");
        book();
        check(keys("U1", "MY_BEST", "A").size() == 1, "matching book restores best evaluation");
        check(!service.select("U1", JsonUtils.object("readState", "UNREAD")).isEmpty(), "new version remains unread after recovery");
        for (int index = 0; index < 3; index++) {
            change("A", "ACTIVE");
        }
        check(keys("U1", "WATCHLIST", "A").size() == 1, "rapid ordered updates retain only current version");
        Set<String> paged = new HashSet<>();
        JSONObject page = call("U1", "query", JsonUtils.object("limit", 1));
        int pages = 0;
        while (true) {
            for (JSONObject row : page.getJSONArray("notifications").toJavaList(JSONObject.class)) {
                check(paged.add(row.getString("notificationId")), "paging does not duplicate a row");
            }
            if (page.getString("nextCursor") == null) break;
            check(++pages < 10, "paging terminates");
            page = call("U1", "query", JsonUtils.object("cursor", page.getString("nextCursor")));
        }
        check(paged.size() == service.select("U1", new JSONObject()).size(), "paging returns every current reminder");
        JSONObject staleRead = call("U1", "read", JsonUtils.object("notificationIds", ids, "read", true));
        check(staleRead.getJSONArray("results").size() == ids.size(), "stale read returns per-item results");
        check(!service.select("U1", JsonUtils.object("readState", "UNREAD")).isEmpty(), "old row read cannot consume new version");
    }

    private static void check(boolean condition, String message) {
        if (!condition) throw new AssertionError(message);
        checks++;
        System.out.println("PASS " + message);
    }
}
