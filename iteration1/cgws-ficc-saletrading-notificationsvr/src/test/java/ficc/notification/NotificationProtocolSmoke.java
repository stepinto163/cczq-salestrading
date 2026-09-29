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
                String market = new ReferenceData().require("250001.IB").getString("market");
                JSONObject watchlist = JsonUtils.object("watchlistId", "W1", "userId", "U-001",
                        "scope", "MARKET", "market", market,
                        "securities", JsonUtils.array(JsonUtils.object("securityId", "250001.IB")));
                JSONObject group = JsonUtils.object("filterGroupId", "G1", "userId", "U-001",
                        "scope", "MARKET", "market", market, "subgroups",
                        JsonUtils.array(JsonUtils.object("conditions", new JSONArray())));
                service.replaceQuotes(JsonUtils.object("epoch", "E", "sequence", "0"),
                        Arrays.asList(own, other), Arrays.asList(watchlist, group));
                JSONObject book = JsonUtils.object("securityId", "250001.IB", "quoteRefs",
                        JsonUtils.object("FICC:Q1", JsonUtils.object("quoteRevision", "1"),
                                "FICC:Q2", JsonUtils.object("quoteRevision", "1")),
                        "marketData", JsonUtils.object("NoMDEntries", JsonUtils.array(
                                JsonUtils.object("MDEntryID", "FICC:Q1", "MDEntryType", "0", "MDEntryPx", "2.5"),
                                JsonUtils.object("MDEntryID", "FICC:Q2", "MDEntryType", "1", "MDEntryPx", "2.6"))));
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
