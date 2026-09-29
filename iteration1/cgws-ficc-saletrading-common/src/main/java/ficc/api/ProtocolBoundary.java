package ficc.api;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import java.util.Map;

/** STC handler/stream 适配层与内部 JSON 持久化模型之间的单一转换边界。 */
public final class ProtocolBoundary {
    private ProtocolBoundary() {}

    public static JSONObject request(String topic, JSONObject value) {
        Endpoint<?, ?> endpoint = SalesTradingApi.find(topic);
        if (endpoint == null) {
            return value; // The service retains its existing unsupported-topic response.
        }
        try {
            return endpoint.request(value).toJson();
        } catch (RuntimeException error) {
            throw ApiException.forField("1001", "VALIDATION_FAILED",
                    "Invalid request structure for " + topic, null);
        }
    }

    public static JSONObject reply(String topic, Map<String, Object> value) {
        JSONObject result = new JSONObject(value);
        Endpoint<?, ?> endpoint = SalesTradingApi.find(topic);
        // STC CRM keeps its original flat reply instead of a FICC Data envelope.
        if (endpoint != null && topic.startsWith("stc.crm.")
                && (!result.containsKey("Code") || "0000".equals(result.getString("Code")))) {
            return endpoint.data(result).toJson();
        }
        if (endpoint != null && "0000".equals(result.getString("Code"))
                && result.get("Data") instanceof JSONObject) {
            result.put("Data", endpoint.data(result.getJSONObject("Data")).toJson());
        }
        return ProtocolCodec.decode(result, Reply.class).toJson();
    }

    public static JSONObject push(String topic, JSONObject value) {
        if (topic.startsWith("ficc.quote.report.")) {
            return ProtocolCodec.decode(value, QuoteStreamBatch.class).toJson();
        }
        if (topic.startsWith("ficc.price.report.")) {
            return ProtocolCodec.decode(value, PriceStreamBatch.class).toJson();
        }
        if (topic.startsWith("ficc.notification.report.")) {
            return ProtocolCodec.decode(value, NotificationStreamBatch.class).toJson();
        }
        if (topic.equals("ficc.quote.events")) {
            return ProtocolCodec.decode(value, QuoteEventBatch.class).toJson();
        }
        if (topic.equals("ficc.notification.events")) {
            return ProtocolCodec.decode(value, NotificationEventBatch.class).toJson();
        }
        if (topic.equals("ficc.price.snapshots")) {
            return ProtocolCodec.decode(value, PriceFeedBatch.class).toJson();
        }
        if (topic.startsWith("ficc.price.book.")) {
            return ProtocolCodec.decode(value, PriceBook.class).toJson();
        }
        return value;
    }
}
