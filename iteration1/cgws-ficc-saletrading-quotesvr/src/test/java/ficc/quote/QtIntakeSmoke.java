package ficc.quote;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.*;

import java.io.BufferedReader;
import java.nio.file.*;
import java.util.*;

/** Small PostgreSQL functional check: two imported quotes, temporary isolated schema. */
public final class QtIntakeSmoke {
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
            IntakeCreateEndpoint endpoint = new IntakeCreateEndpoint();
            JSONObject request = request("qt-a", "U-001");
            JSONObject missingOwner = JsonUtils.deepCopy(request);
            missingOwner.getJSONObject("intake").remove("ownerUserId");
            try {
                endpoint.create(service, missingOwner);
                throw new AssertionError("missing owner accepted");
            } catch (ApiException expected) {
                check("1001".equals(expected.code), "intake still requires resolved owner");
            }
            JSONObject missingIntake = JsonUtils.deepCopy(request);
            missingIntake.remove("intake");
            try {
                endpoint.create(service, missingIntake);
                throw new AssertionError("missing intake accepted");
            } catch (ApiException expected) {
                check("1001".equals(expected.code), "intake metadata is required");
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
            JSONObject quote = service.quotes.get(firstId);
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
                    !request.getJSONObject("intake").containsKey("token"),
                    "service create succeeds without a custom token");
        }
        try (PgStore database = new PgStore(configuration)) {
            QuoteService restarted = new QuoteService(database, new ReferenceData());
            JSONObject result =
                    new IntakeCreateEndpoint().create(restarted, request("qt-a", "U-001"));
            check(
                    firstId.equals(
                            result.getJSONObject("Data")
                                    .getJSONArray("results")
                                    .getJSONObject(0)
                                    .getString("quoteId")),
                    "idempotency survives QuoteSvr restart");
            check(restarted.quotes.size() == 2, "restart keeps both persisted quotes");
            // Only the generated test schema is removed; never a configured service schema.
            if (!schema.matches("ficc_qtintake_check_[0-9]+")) {
                throw new AssertionError("unexpected test schema");
            }
            database.exec("DROP SCHEMA " + schema + " CASCADE");
        }
        System.out.println("QtIntake PostgreSQL checks passed: " + checks);
    }
}
