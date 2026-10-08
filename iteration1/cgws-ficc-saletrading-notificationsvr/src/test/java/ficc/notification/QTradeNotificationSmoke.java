package ficc.notification;

import com.alibaba.fastjson2.JSON;
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

/** Consumes QuoteSvr's actual negotiation snapshots; no GW or desktop connection. */
public final class QTradeNotificationSmoke {
    private static int checks;

    private static void check(boolean valid, String message) {
        if (!valid) throw new AssertionError(message);
        checks++;
        System.out.println("PASS " + message);
    }

    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (Reader reader = Files.newBufferedReader(Paths.get(args[0]), StandardCharsets.UTF_8)) {
            configuration.load(reader);
        }
        String schema = "ficc_negotiation_notify_" + System.currentTimeMillis();
        configuration.setProperty("db.schema", schema);
        JSONArray stages = JSON.parseArray(new String(Files.readAllBytes(Paths.get(args[1])), StandardCharsets.UTF_8));
        try (JdbcStore store = new JdbcStore(configuration, "notification_")) {
            try {
                NotificationService service = new NotificationService(store);
                String market = new ReferenceData().require("250001.IB").getString("market");
                List<JSONObject> settings = new ArrayList<>();
                for (String user : Arrays.asList("A", "B", "C")) {
                    settings.add(JsonUtils.object("watchlistId", "WATCH-" + user, "userId", user,
                            "scope", "MARKET", "market", market,
                            "securities", JsonUtils.array(JsonUtils.object("securityId", "250001.IB"))));
                }
                Map<String, String> confirmedNotifications = new HashMap<>();
                for (JSONObject stage : stages.toJavaList(JSONObject.class)) {
                    List<JSONObject> quotes = stage.getJSONArray("quotes").toJavaList(JSONObject.class);
                    service.replaceQuotes(stage.getJSONObject("state"), quotes, settings);
                    check(service.priceMessage(stage.getJSONObject("feed")), "price snapshot accepted");
                    for (String user : Arrays.asList("A", "B", "C")) {
                        Set<String> permitted = new HashSet<>();
                        for (JSONObject quote : quotes) {
                            if (user.equals(quote.getString("ownerUserId")) && "ACTIVE".equals(quote.getString("status"))) {
                                permitted.add(quote.getString("quoteId"));
                            }
                        }
                        List<JSONObject> notifications = service.select(user, JsonUtils.object());
                        Set<String> linked = new HashSet<>();
                        for (JSONObject notification : notifications) {
                            linked.addAll(notification.getJSONArray("quoteIds").toJavaList(String.class));
                        }
                        check(permitted.equals(linked), stage.getString("stage") + " " + user
                                + " watchlist links exactly own confirmed quotes");
                        String serialized = JsonUtils.toJson(notifications);
                        if ("both-counteroffers-confirmed".equals(stage.getString("stage"))) {
                            confirmedNotifications.put(user, serialized);
                        } else if ("replacement-pending".equals(stage.getString("stage"))) {
                            check(serialized.equals(confirmedNotifications.get(user)),
                                    "pending replacement creates no notification change for " + user);
                        }
                    }
                }
            } finally {
                if (!schema.matches("ficc_negotiation_notify_[0-9]+")) throw new IllegalStateException(schema);
                store.exec("DROP SCHEMA " + schema + " CASCADE");
            }
        }
        System.out.println("PASS negotiation notification: " + checks + " checks");
    }
}
