package ficc.quote;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.JsonUtils;
import ficc.common.client.LoginSessionClient;
import ficc.common.client.QuoteSvrClient;

import java.lang.reflect.Field;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

/** Small regression checks run against BusinessSmoke's isolated five-quote database. */
final class InternalSyncChecks {
    static void verify(QuoteService service, int expectedQuotes) throws Exception {
        QuoteResource resource = new QuoteResource();
        setField(resource, "service", service);
        setField(
                resource,
                "identity",
                (LoginSessionClient)
                        sessionId -> {
                            if (sessionId == null || sessionId.trim().isEmpty()) {
                                return null;
                            }
                            throw new AssertionError(
                                    "Internal synchronization must not query LoginSvr");
                        });

        Set<String> quoteIds = new HashSet<>();
        JSONObject request = JsonUtils.object("requestId", "internal-first", "limit", 2);
        String sequence = null;
        int pages = 0;
        while (true) {
            JSONObject reply =
                    new JSONObject(
                            resource.dispatch(
                                    QuoteSvrClient.INTERNAL_STATE_QUERY,
                                    null,
                                    JsonUtils.toJson(request)));
            BusinessSmoke.check(
                    "0000".equals(reply.getString("Code")),
                    "internal state read without user login");
            JSONObject data = reply.getJSONObject("Data");
            if (sequence == null) {
                sequence = data.getString("sequence");
            }
            BusinessSmoke.check(
                    sequence.equals(data.getString("sequence")),
                    "synchronization cursor preserves sequence");
            for (JSONObject quote :
                    data.getJSONObject("records")
                            .getJSONArray("quotes")
                            .toJavaList(JSONObject.class)) {
                BusinessSmoke.check(
                        quoteIds.add(quote.getString("quoteId")),
                        "synchronization does not duplicate quotes");
            }
            String cursor = data.getString("nextCursor");
            if (cursor == null) {
                break;
            }
            if (++pages > 4) {
                throw new AssertionError("Unexpected synchronization page count");
            }
            request = JsonUtils.object("requestId", "internal-next-" + pages, "cursor", cursor);
        }
        BusinessSmoke.check(
                quoteIds.size() == expectedQuotes,
                "internal synchronization returns all test quotes");

        String content = "{\"requestId\":\"internal-check\"}";
        Map<String, Object> prices =
                resource.dispatch(QuoteSvrClient.INTERNAL_PRICE_QUERY, null, content);
        BusinessSmoke.check(
                "0000".equals(prices.get("Code")), "internal price read without user login");
        for (String topic :
                new String[] {
                    QuoteSvrClient.INTERNAL_STATE_QUERY, QuoteSvrClient.INTERNAL_PRICE_QUERY
                }) {
            BusinessSmoke.check(
                    "0000"
                            .equals(
                                    resource.dispatch(topic, "transport-connection-id", content)
                                            .get("Code")),
                    "internal read does not mistake transport connection ID for a logged-in user");
        }
        for (String topic :
                new String[] {"ficc.quote.create", "ficc.quote.query", "ficc.price.query"}) {
            BusinessSmoke.check(
                    !"0000".equals(resource.dispatch(topic, null, content).get("Code")),
                    "business API still requires login");
        }
        BusinessSmoke.check(
                "1003"
                        .equals(
                                service.handleInternalRead(
                                                "ficc.quote.create", JsonUtils.parseObject(content))
                                        .getString("Code")),
                "internal dispatcher refuses quote mutations");
    }

    private static void setField(QuoteResource resource, String name, Object value)
            throws Exception {
        Field field = QuoteResource.class.getDeclaredField(name);
        field.setAccessible(true);
        field.set(resource, value);
    }
}
