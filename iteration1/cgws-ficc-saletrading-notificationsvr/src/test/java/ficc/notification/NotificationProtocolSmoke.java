package ficc.notification;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.api.*;
import ficc.common.JdbcStore;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import java.io.Reader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.*;

/** 两条报价的通知协议回归，使用独立临时 schema，不经过 GW。 */
public final class NotificationProtocolSmoke {
    private static int checks;
    private static final JSONArray fixtures = new JSONArray();

    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (Reader reader = Files.newBufferedReader(Paths.get(args[0]), StandardCharsets.UTF_8)) {
            configuration.load(reader);
        }
        String schema = "ficc_notification_protocol_" + System.currentTimeMillis();
        configuration.setProperty("db.schema", schema);
        try (JdbcStore store = new JdbcStore(configuration, "notification_")) {
            try {
                NotificationService service = new NotificationService(store);
                service.publish = (topic, message) -> {
                    JSONObject wire = ProtocolBoundary.push(topic, message);
                    check(JsonUtils.canonicalHash(message).equals(JsonUtils.canonicalHash(wire)), "push round trip");
                    fixtures.add(JsonUtils.object("topic", topic, "message", wire));
                };
                call(service, SalesTradingApi.NotificationStreamOpen, new Request());
                JSONObject own = quote("Q1", "U-001", "1");
                JSONObject other = quote("Q2", "U-002", "2");
                JSONObject competitor = quote("Q3", "U-002", "1");
                String market = new ReferenceData().require("250001.IB").getString("market");
                JSONObject watchlist = JsonUtils.object("watchlistId", "W1", "userId", "U-001",
                        "scope", "MARKET", "market", market,
                        "securities", JsonUtils.array(JsonUtils.object("securityId", "250001.IB")));
                JSONObject group = JsonUtils.object("filterGroupId", "G1", "userId", "U-001",
                        "scope", "MARKET", "market", market, "subgroups",
                        JsonUtils.array(JsonUtils.object("conditions", new JSONArray())));
                service.replaceQuotes(JsonUtils.object("epoch", "E", "sequence", "0"),
                        Arrays.asList(own, other, competitor), Arrays.asList(watchlist, group));
                JSONObject book = JsonUtils.object("securityId", "250001.IB", "quoteRefs",
                        JsonUtils.object("FICC:Q1", JsonUtils.object("quoteRevision", "1"),
                                "FICC:Q2", JsonUtils.object("quoteRevision", "1"),
                                "FICC:Q3", JsonUtils.object("quoteRevision", "1")),
                        "marketData", JsonUtils.object("NoMDEntries", JsonUtils.array(
                                JsonUtils.object("MDEntryID", "FICC:Q1", "MDEntryType", "0", "MDEntryPx", "2.5"),
                                JsonUtils.object("MDEntryID", "FICC:Q2", "MDEntryType", "1", "MDEntryPx", "2.6"),
                                JsonUtils.object("MDEntryID", "FICC:Q3", "MDEntryType", "0", "MDEntryPx", "2.7"))));
                JSONObject feed = JsonUtils.object("kind", "SNAPSHOT", "streamId", "P", "sequence", "0",
                        "prices", JsonUtils.array(book));
                check(service.priceMessage(ProtocolCodec.decode(feed, PriceFeedBatch.class).toJson()), "price feed accepted");
                NotificationViewOpenRequest view = new NotificationViewOpenRequest();
                view.viewId = "V";
                view.pageSize = 10;
                NotificationViewData data = call(service, SalesTradingApi.NotificationViewOpen, view);
                Set<String> reasons = new HashSet<>();
                for (NotificationRecord record : data.notifications) {
                    for (NotificationReason reason : record.reasons) {
                        reasons.add(reason.type);
                    }
                }
                check(reasons.containsAll(Arrays.asList("WATCHLIST", "FILTER_GROUP", "MY_BEST", "COUNTER_QUOTE")),
                        "all four notification reasons survive typed models");
                NotificationReadRequest read = new NotificationReadRequest();
                read.notificationIds = Arrays.asList(data.notifications.get(0).notificationId, "missing");
                read.read = true;
                NotificationReadData readData = call(service, SalesTradingApi.NotificationRead, read);
                check("0000".equals(readData.results.get(0).Code) && "1002".equals(readData.results.get(1).Code),
                        "partial read result retained");
                read.requestId = null;
                read.notificationIds = Collections.singletonList(data.notifications.get(0).notificationId);
                read.read = false;
                check(call(service, SalesTradingApi.NotificationRead, read).results.get(0).readTime == null,
                        "mark unread retains explicit null");
                checkTriggerIsolation(service, store, own, other, competitor, book);
                NotificationPreferenceSaveRequest preference = new NotificationPreferenceSaveRequest();
                preference.expectedRevision = "0";
                preference.watchlistEnabled = false;
                preference.filterGroupEnabled = false;
                preference.counterQuoteEnabled = false;
                preference.myBestEnabled = false;
                check(!call(service, SalesTradingApi.NotificationPreferenceSave, preference).preference.myBestEnabled,
                        "false preference retained");
                call(service, SalesTradingApi.NotificationStateQuery, new StateQueryRequest());
                check(!fixtures.isEmpty(), "events and replies captured");
                Files.write(Paths.get(args[1]), JsonUtils.toJson(fixtures).getBytes(StandardCharsets.UTF_8));
            } finally {
                if (!schema.matches("ficc_notification_protocol_[0-9]+")) {
                    throw new IllegalStateException("Unexpected test schema");
                }
                store.exec("DROP SCHEMA " + schema + " CASCADE");
            }
        }
        System.out.println("PASS notification protocol: " + checks + " checks");
    }

    private static Set<String> triggers(NotificationService service, String type, String quoteId) {
        Set<String> keys = new HashSet<>();
        for (JSONObject row : service.select("U-001", JsonUtils.object())) {
            for (JSONObject trigger : row.getJSONArray("triggers").toJavaList(JSONObject.class)) {
                if (type.equals(trigger.getString("type"))
                        && trigger.getJSONArray("quoteIds").contains(quoteId)) {
                    keys.add(trigger.getString("alertKey"));
                }
            }
        }
        return keys;
    }

    private static void checkTriggerIsolation(NotificationService service, JdbcStore store,
            JSONObject own, JSONObject other, JSONObject competitor, JSONObject book) throws Exception {
        Set<String> original = triggers(service, "WATCHLIST", "Q1");
        check(original.size() == 1, "watch trigger is per quote and version");
        Set<String> counter = triggers(service, "COUNTER_QUOTE", "Q1");
        check(counter.size() == 1, "counter trigger contains bilateral pair");
        long tail = store.tail();
        service.recompute();
        check(tail == store.tail(), "unchanged global recomputation emits no event");
        NotificationReadRequest mark = new NotificationReadRequest();
        mark.read = true;
        mark.notificationIds = new ArrayList<>();
        for (JSONObject notification : service.select("U-001", JsonUtils.object())) {
            mark.notificationIds.add(notification.getString("notificationId"));
        }
        call(service, SalesTradingApi.NotificationRead, mark);
        JSONObject removed = JsonUtils.deepCopy(competitor);
        removed.put("status", "REFERRED");
        removed.put("revision", "2");
        JSONObject message = JsonUtils.object("epoch", "E", "events", JsonUtils.array(JsonUtils.object(
                "sequence", "1", "type", "QUOTE", "quoteId", "Q3", "quote", removed)));
        check(service.quoteMessage(message), "refer event accepted");
        check(original.equals(triggers(service, "WATCHLIST", "Q1")), "removing same-bond quote preserves unchanged trigger");
        check(service.select("U-001", JsonUtils.object()).stream().allMatch(row -> row.getString("readTime") != null),
                "removing a linked quote preserves read state of unchanged triggers");
        check(triggers(service, "WATCHLIST", "Q3").isEmpty(), "referred quote has no active trigger");
        check(counter.equals(triggers(service, "COUNTER_QUOTE", "Q1")), "unrelated removal preserves bilateral identity");
        JSONObject singletonBook = JsonUtils.deepCopy(book);
        singletonBook.getJSONObject("quoteRefs").remove("FICC:Q3");
        singletonBook.getJSONObject("marketData").getJSONArray("NoMDEntries").remove(2);
        service.priceMessage(JsonUtils.object("kind", "SNAPSHOT", "streamId", "P2", "sequence", "0", "prices", JsonUtils.array(singletonBook)));
        check(triggers(service, "MY_BEST", "Q1").isEmpty(), "single same-side quote is not a my-best reminder");
        tail = store.tail();
        check(service.quoteMessage(message), "duplicate delivery accepted");
        check(tail == store.tail(), "duplicate delivery emits no notification");
        JSONObject edited = JsonUtils.deepCopy(own);
        edited.put("revision", "2");
        edited.put("price", "2.4");
        service.quoteMessage(JsonUtils.object("epoch", "E", "events", JsonUtils.array(JsonUtils.object(
                "sequence", "2", "type", "QUOTE", "quoteId", "Q1", "quote", edited))));
        check(!original.equals(triggers(service, "WATCHLIST", "Q1")), "source version change creates new trigger");
        check(!counter.equals(triggers(service, "COUNTER_QUOTE", "Q1")), "either bilateral version changes pair trigger");
        JSONObject pending = quote("QP", "U-001", "1");
        pending.put("status", "PENDING_CONFIRM");
        tail = store.tail();
        service.quoteMessage(JsonUtils.object("epoch", "E", "events", JsonUtils.array(JsonUtils.object(
                "sequence", "3", "type", "QUOTE", "quoteId", "QP", "quote", pending))));
        check(triggers(service, "WATCHLIST", "QP").isEmpty() && tail == store.tail(), "pending quote does not generate reminders");
    }

    private static JSONObject quote(String id, String owner, String side) {
        return JsonUtils.object("quoteId", id, "ownerUserId", owner, "securityId", "250001.IB",
                "status", "ACTIVE", "revision", "1", "side", side, "price", "2.5",
                "quoteType", "1", "yieldType", "1", "settlType", "2",
                "updateTime", "2026-09-29T01:00:00Z");
    }

    private static <Q extends Request, R extends ProtocolModel> R call(
            NotificationService service, Endpoint<Q, R> endpoint, Q request) {
        request.requestId = UUID.randomUUID().toString();
        JSONObject wire = request.toJson();
        String user = endpoint == SalesTradingApi.NotificationStateQuery ? "S-SEARCH" : "U-001";
        JSONObject reply = service.handle(user, "notification-protocol-check", endpoint.topic, wire);
        check("0000".equals(reply.getString("Code")), endpoint.topic + ": " + reply.getString("Msg"));
        fixtures.add(JsonUtils.object("topic", endpoint.topic, "request", wire, "reply", reply));
        return endpoint.reply(reply).Data;
    }

    private static void check(boolean condition, String message) {
        if (!condition) {
            throw new AssertionError(message);
        }
        checks++;
        System.out.println("PASS " + message);
    }
}
