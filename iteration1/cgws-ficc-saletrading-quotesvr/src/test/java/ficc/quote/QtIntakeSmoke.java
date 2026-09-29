package ficc.quote;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.*;

import java.io.BufferedReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.*;

/** Bounded PostgreSQL intake and grouping checks in a temporary isolated schema. */
public final class QtIntakeSmoke {
    private static final String TOKEN = "local-test-token-at-least-thirty-two-characters";
    private static int checks;

    private static void check(boolean condition, String message) {
        if (!condition) {
            throw new AssertionError(message);
        }
        checks++;
        System.out.println("PASS " + message);
    }

    private static JSONObject request(String account, String owner) {
        String id = "qtintake-smoke-" + account;
        return JsonUtils.object(
                "requestId",
                id,
                "intake",
                JsonUtils.object(
                        "token",
                        TOKEN,
                        "sourceUserId",
                        account,
                        "ownerUserId",
                        owner,
                        "externalMessageId",
                        id,
                        "externalQuoteId",
                        "QT-Q-1",
                        "sourceTime",
                        "20260922-01:02:03.123",
                        "transType",
                        "N",
                        "text",
                        "Original QTrade quote"),
                "quotes",
                JsonUtils.array(
                        JsonUtils.object(
                                "clientQuoteId",
                                id,
                                "securityId",
                                "250001.IB",
                                "side",
                                "1",
                                "quoteType",
                                "1",
                                "yieldType",
                                "1",
                                "price",
                                "2.5",
                                "quantity",
                                "1000",
                                "quantityUnit",
                                "WAN",
                                "currency",
                                "CNY",
                                "tradeDate",
                                "2026-09-22",
                                "settlType",
                                "2",
                                "institutionName",
                                "Unresolved institution",
                                "contactName",
                                "Unresolved contact")));
    }

    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(args[0]))) {
            configuration.load(reader);
        }
        String schema = "ficc_qtintake_check_" + System.currentTimeMillis();
        configuration.setProperty("db.schema", schema);
        String firstId;
        try (PgStore database = new PgStore(configuration)) {
            QuoteService service = new QuoteService(database, new ReferenceData());
            List<JSONObject> events = new ArrayList<>();
            service.publish =
                    (topic, event) -> {
                        if ("ficc.quote.events".equals(topic)) {
                            events.add(event);
                        }
                    };
            IntakeCreateEndpoint endpoint = new IntakeCreateEndpoint(TOKEN);
            JSONObject request = request("qt-a", "U-001");
            ficc.common.client.BaseDataClient unavailableBase =
                    new ficc.common.client.BaseDataClient() {
                        public Map<String, JSONObject> securities() {
                            return Collections.emptyMap();
                        }

                        public JSONObject security(String securityId) {
                            throw ApiException.forField("1009", "NOT_READY", "BaseSvr offline", null);
                        }

                        public List<JSONObject> records(String kind) {
                            return Collections.emptyList();
                        }

                        public String version() {
                            return "offline";
                        }
                    };
            QuoteService unavailableService = new QuoteService(database,
                    new ReferenceData(unavailableBase, new ficc.common.client.SimulatorCrmClient()));
            try {
                endpoint.create(unavailableService, request);
                throw new AssertionError("offline BaseSvr accepted");
            } catch (ApiException expected) {
                check("1009".equals(expected.code), "dependency outage remains retryable");
            }
            check("[]".equals(database.scalar("SELECT results::text FROM requests WHERE request_id=?",
                    request.getString("requestId"))), "transient failure is not cached as final result");
            try {
                new IntakeCreateEndpoint("").authorize(request);
                throw new AssertionError("disabled intake accepted");
            } catch (ApiException expected) {
                check("1003".equals(expected.code), "intake disabled by default");
            }
            JSONObject unauthorized = JsonUtils.deepCopy(request);
            unauthorized.getJSONObject("intake").put("token", "wrong");
            try {
                endpoint.authorize(unauthorized);
                throw new AssertionError("bad token accepted");
            } catch (ApiException expected) {
                check("1003".equals(expected.code), "invalid service credential rejected");
            }
            JSONObject result =
                    endpoint.create(service, request)
                            .getJSONObject("Data")
                            .getJSONArray("results")
                            .getJSONObject(0);
            check(
                    "0000".equals(result.getString("Code")),
                    "create succeeds with unresolved CRM names");
            firstId = result.getString("quoteId");
            JSONObject quote = service.quotes.get(firstId).toJson();
            check("PENDING_CONFIRM".equals(quote.getString("status")), "quote awaits confirmation");
            check("U-001".equals(quote.getString("ownerUserId")), "STC userid owns quote");
            check(
                    "QT".equals(quote.getJSONObject("source").getString("type")),
                    "QTrade source provenance saved");
            check(
                    quote.getString("institutionId") == null
                            && "Unresolved institution".equals(quote.getString("institutionName")),
                    "CRM names preserved without invented IDs");
            check(events.size() == 1, "normal quote event published");
            endpoint.create(service, request);
            check(
                    service.quotes.size() == 1 && events.size() == 1,
                    "lost-reply replay creates no duplicate or push");
            JSONObject upgradedReplay = JsonUtils.deepCopy(request);
            upgradedReplay.getJSONObject("intake").put("originalMessageId", "new-group-metadata");
            // Simulate a persisted source written by the previous version, which did
            // not include originalMessageId at all.
            JSONObject legacySource = database.rows("SELECT payload FROM sources WHERE id=?",
                    quote.getString("sourceMessageId")).get(0);
            legacySource.remove("originalMessageId");
            database.update("UPDATE sources SET payload=?::jsonb WHERE id=?",
                    JsonUtils.toJson(legacySource), quote.getString("sourceMessageId"));
            endpoint.create(service, upgradedReplay);
            check(service.quotes.size() == 1 && events.size() == 1,
                    "pre-grouping receipt accepts replay without changing the existing source");
            endpoint.create(service, request("qt-b", "U-002"));
            check(service.quotes.size() == 2, "different QTrade accounts remain separate");
            JSONObject denied =
                    service.handle(
                            null,
                            null,
                            "ficc.quote.create",
                            JsonUtils.object(
                                    "requestId", "unauthenticated", "quotes", JsonUtils.array()));
            check(
                    "1003".equals(denied.getString("Code")),
                    "ordinary creates still require a login");
            JSONObject source =
                    database.rows(
                                    "SELECT payload FROM sources WHERE id=?",
                                    quote.getString("sourceMessageId"))
                            .get(0);
            check(
                    source.getJSONArray("quoteIds").contains(firstId),
                    "source text links back to created quote");
            check(
                    !JsonUtils.toJson(source).contains(TOKEN)
                            && !JsonUtils.toJson(quote).contains(TOKEN),
                    "service token not persisted in source or quote");
            JSONObject firstGrouped = importGrouped(endpoint, service, "group-row-1", "message-a", "U-001");
            JSONObject secondGrouped = importGrouped(endpoint, service, "group-row-2", "message-a", "U-001");
            JSONObject otherMessage = importGrouped(endpoint, service, "group-row-3", "message-b", "U-001");
            JSONObject otherOwner = importGrouped(endpoint, service, "group-row-4", "message-a", "U-002");
            String groupSource = firstGrouped.getString("sourceMessageId");
            check(groupSource.equals(secondGrouped.getString("sourceMessageId")),
                    "one original message shares one persisted source across two quotes");
            check(!groupSource.equals(otherMessage.getString("sourceMessageId")),
                    "a later identical text message forms a separate group");
            check(!groupSource.equals(otherOwner.getString("sourceMessageId")),
                    "groups never cross resolved STC owners");
            JSONObject groupedSource = database.rows("SELECT payload FROM sources WHERE id=?", groupSource).get(0);
            check(groupedSource.getJSONArray("quoteIds").size() == 2,
                    "original source links to all its quotes");
            int eventCount = events.size();
            importGrouped(endpoint, service, "group-row-2", "message-a", "U-001");
            check(service.quotes.size() == 6 && events.size() == eventCount,
                    "grouped quote replay adds neither row nor event");
            check(events.stream().anyMatch(event -> JsonUtils.toJson(event).contains(groupSource)),
                    "group identity reaches normal published quote events");
            if (args.length > 1) {
                Files.write(Paths.get(args[1]), JsonUtils.toJson(JsonUtils.array(
                        firstGrouped, secondGrouped, otherMessage)).getBytes(StandardCharsets.UTF_8));
            }
            JSONObject confirmed = (JSONObject) service.write(new QuoteService.Call(
                    "U-001", "<test>", "ficc.quote.confirm", JsonUtils.object(
                            "requestId", "confirm-group-member",
                            "quotes", JsonUtils.array(JsonUtils.object(
                                    "quoteId", firstGrouped.getString("quoteId"),
                                    "expectedRevision", "1",
                                    "institutionName", null,
                                    "contactName", null)))), "confirm", null);
            check("0000".equals(confirmed.getJSONArray("results").getJSONObject(0).getString("Code"))
                            && "PENDING_CONFIRM".equals(service.quotes.get(
                                    secondGrouped.getString("quoteId")).status),
                    "confirming one grouped quote leaves its sibling pending");
            JSONObject deleted = (JSONObject) service.write(new QuoteService.Call(
                    "U-001", "<test>", "ficc.quote.delete", JsonUtils.object(
                            "requestId", "delete-group-member",
                            "quotes", JsonUtils.array(JsonUtils.object(
                                    "quoteId", secondGrouped.getString("quoteId"),
                                    "expectedRevision", "1")))), "delete", null);
            check("0000".equals(deleted.getJSONArray("results").getJSONObject(0).getString("Code"))
                            && "ACTIVE".equals(service.quotes.get(
                                    firstGrouped.getString("quoteId")).status)
                            && groupSource.equals(service.quotes.get(
                                    firstGrouped.getString("quoteId")).sourceMessageId),
                    "deleting a sibling preserves confirmed quote and source identity");
        }
        try (PgStore database = new PgStore(configuration)) {
            QuoteService restarted = new QuoteService(database, new ReferenceData());
            JSONObject result =
                    new IntakeCreateEndpoint(TOKEN).create(restarted, request("qt-a", "U-001"));
            check(
                    firstId.equals(
                            result.getJSONObject("Data")
                                    .getJSONArray("results")
                                    .getJSONObject(0)
                                    .getString("quoteId")),
                    "idempotency survives QuoteSvr restart");
            check(restarted.quotes.size() == 6, "restart keeps all persisted quotes");
            JSONObject group = importGrouped(new IntakeCreateEndpoint(TOKEN), restarted,
                    "group-row-2", "message-a", "U-001");
            JSONObject groupedSource = database.rows("SELECT payload FROM sources WHERE id=?",
                    group.getString("sourceMessageId")).get(0);
            check(groupedSource.getJSONArray("quoteIds").size() == 2 && restarted.quotes.size() == 6,
                    "group membership and per-quote replay survive restart");
            // Only the generated test schema is removed; never a configured service schema.
            if (!schema.matches("ficc_qtintake_check_[0-9]+")) {
                throw new AssertionError("unexpected test schema");
            }
            database.exec("DROP SCHEMA " + schema + " CASCADE");
        }
        System.out.println("QtIntake PostgreSQL checks passed: " + checks);
    }

    private static JSONObject importGrouped(IntakeCreateEndpoint endpoint, QuoteService service,
            String eventId, String originalMessageId, String owner) throws Exception {
        JSONObject input = request("qt-group", owner);
        input.put("requestId", eventId);
        input.getJSONObject("intake").put("externalMessageId", eventId);
        input.getJSONObject("intake").put("externalQuoteId", eventId);
        input.getJSONObject("intake").put("originalMessageId", originalMessageId);
        input.getJSONArray("quotes").getJSONObject(0).put("clientQuoteId", eventId);
        JSONObject result = endpoint.create(service, input).getJSONObject("Data")
                .getJSONArray("results").getJSONObject(0);
        check("0000".equals(result.getString("Code")), "grouped intake succeeds: " + eventId);
        return service.quotes.get(result.getString("quoteId")).toJson();
    }
}
