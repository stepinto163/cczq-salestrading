package ficc.quote;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import ficc.common.*;

/** STC trusted inter-service quote intake; ordinary client creates still require login. */
final class IntakeCreateEndpoint {
    JSONObject create(QuoteService service, JSONObject input) throws Exception {
        RequestValidator.allowOnlyFields(input, "requestId", "quotes", "intake");
        JSONObject request = JsonUtils.deepCopy(input);
        JSONObject source = request.getJSONObject("intake");
        if (source == null) {
            throw ApiException.forField(
                    "1001", "VALIDATION_FAILED", "intake is required", "intake");
        }
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
