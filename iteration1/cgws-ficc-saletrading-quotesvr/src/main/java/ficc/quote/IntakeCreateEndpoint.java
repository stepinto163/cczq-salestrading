package ficc.quote;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import ficc.common.*;

import java.io.BufferedReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.security.MessageDigest;
import java.util.Properties;

/**
 * Restricted service-to-service use of quote.create; ordinary client creates still require login.
 */
final class IntakeCreateEndpoint {
    private final String token;

    IntakeCreateEndpoint(String token) {
        this.token = token == null ? "" : token;
        if (!this.token.isEmpty() && this.token.length() < 32) {
            // The length is the whole point: the file is shared with QtIntakeSvr, which does not
            // check it, so a short token looks fine on that side and aborts QuoteSvr on this one.
            throw new IllegalArgumentException(
                    "intake.token must contain at least 32 characters, got " + this.token.length());
        }
    }

    static IntakeCreateEndpoint load(Properties configuration) throws Exception {
        String file = configuration.getProperty("intake.authFile", "").trim();
        if (file.isEmpty()) {
            return new IntakeCreateEndpoint("");
        }
        return ComponentLog.initialize(
                configuration,
                "INTAKE_AUTH",
                "service",
                "IntakeCreateEndpoint",
                file,
                () -> {
                    String resolved = file;
                    if (file.startsWith("$ATS_ROOT/")) {
                        String root = System.getenv("ATS_ROOT");
                        if (root == null || root.trim().isEmpty()) {
                            throw new IllegalStateException(
                                    "ATS_ROOT is required for intake.authFile");
                        }
                        resolved = Paths.get(root).resolve(file.substring(10)).toString();
                    }
                    Properties secrets = new Properties();
                    try (BufferedReader reader = Files.newBufferedReader(Paths.get(resolved))) {
                        secrets.load(reader);
                    }
                    String value = secrets.getProperty("intake.token", "");
                    if (value.trim().isEmpty()) {
                        throw new IllegalArgumentException(
                                "Missing intake.token in intake.authFile: " + resolved);
                    }
                    return new IntakeCreateEndpoint(value);
                });
    }

    void authorize(JSONObject request) {
        JSONObject source = request.getJSONObject("intake");
        String supplied = source == null ? null : source.getString("token");
        if (token.isEmpty()
                || supplied == null
                || !MessageDigest.isEqual(
                        token.getBytes(StandardCharsets.UTF_8),
                        supplied.getBytes(StandardCharsets.UTF_8))) {
            throw ApiException.forField(
                    "1003", "FORBIDDEN", "Quote intake service is not authorized", null);
        }
    }

    JSONObject create(QuoteService service, JSONObject input) throws Exception {
        authorize(input);
        RequestValidator.allowOnlyFields(input, "requestId", "quotes", "intake");
        JSONObject request = JsonUtils.deepCopy(input);
        JSONObject source = request.getJSONObject("intake");
        source.remove("token");
        RequestValidator.allowOnlyFields(
                source,
                "ownerUserId",
                "sourceUserId",
                "externalMessageId",
                "externalQuoteId",
                "sourceTime",
                "transType",
                "text");
        String requestId = RequestValidator.requireString(request, "requestId");
        String owner = RequestValidator.requireString(source, "ownerUserId");
        String account = RequestValidator.requireString(source, "sourceUserId");
        String externalId = RequestValidator.requireString(source, "externalMessageId");
        String externalQuote = RequestValidator.requireString(source, "externalQuoteId");
        String sourceTime = RequestValidator.requireString(source, "sourceTime");
        String hash = JsonUtils.canonicalHash(request);
        JSONArray quotes = request.getJSONArray("quotes");
        if (quotes == null || quotes.size() != 1) {
            throw ApiException.forField(
                    "1001",
                    "VALIDATION_FAILED",
                    "Intake create requires exactly one quote",
                    "quotes");
        }
        String sourceId =
                service.store.scalar(
                        "SELECT id FROM sources WHERE source_type=? AND external_id=?",
                        "QT",
                        externalId);
        if (sourceId == null) {
            sourceId = IdGenerator.newId("SM-");
            JSONObject raw =
                    JsonUtils.object(
                            "sourceMessageId",
                            sourceId,
                            "sourceType",
                            "QT",
                            "externalMessageId",
                            externalId,
                            "sourceUserId",
                            account,
                            "ownerUserId",
                            owner,
                            "sourceTime",
                            sourceTime,
                            "text",
                            source.getString("text"),
                            "quoteIds",
                            JsonUtils.array());
            service.store.update(
                    "INSERT INTO sources VALUES (?,?,?,?::jsonb)",
                    sourceId,
                    "QT",
                    externalId,
                    JsonUtils.toJson(raw));
        }
        JSONObject provenance = JsonUtils.deepCopy(source);
        provenance.put("type", "QT");
        provenance.put("originService", "QtIntake");
        provenance.put("externalQuoteId", externalQuote);
        JSONObject metadata = JsonUtils.object("sourceMessageId", sourceId, "source", provenance);
        JSONObject create = JsonUtils.object("requestId", requestId, "quotes", quotes);
        JSONObject result =
                (JSONObject)
                        service.write(
                                new QuoteService.Call(
                                        owner, "<qtintake>", "ficc.quote.create/qtintake", create),
                                "create",
                                metadata,
                                hash);
        JSONObject raw =
                service.store.rows("SELECT payload FROM sources WHERE id=?", sourceId).get(0);
        for (Object item : result.getJSONArray("results")) {
            JSONObject entry = (JSONObject) item;
            String quoteId = entry.getString("quoteId");
            if ("0000".equals(entry.getString("Code"))
                    && !raw.getJSONArray("quoteIds").contains(quoteId)) {
                raw.getJSONArray("quoteIds").add(quoteId);
            }
        }
        service.store.update(
                "UPDATE sources SET payload=?::jsonb WHERE id=?", JsonUtils.toJson(raw), sourceId);
        return ApiResponse.success(requestId, result);
    }
}
