package ficc.common.client;

import static org.junit.jupiter.api.Assertions.*;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.JsonUtils;

import org.junit.jupiter.api.Test;

import sc.app.stc.net.NetClientWrapper;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

final class QuoteSvrClientTest {
    @Test
    void synchronizesStateAndPricesWithoutLoggingInOrSendingUserCredentials() throws Exception {
        RecordingNetwork network = new RecordingNetwork();
        QuoteSvrClient client = new QuoteSvrClient(network, 3000);
        JSONObject request = JsonUtils.object("limit", 2);
        client.queryState(request);
        client.queryState(JsonUtils.object("cursor", "next-page"));
        client.queryPriceSnapshot();

        assertEquals(
                Arrays.asList(
                        QuoteSvrClient.INTERNAL_STATE_QUERY,
                        QuoteSvrClient.INTERNAL_STATE_QUERY,
                        QuoteSvrClient.INTERNAL_PRICE_QUERY),
                network.topics);
        assertFalse(request.containsKey("requestId"), "The caller's request must not be mutated");
        assertEquals("next-page", network.requests.get(1).getString("cursor"));
        assertEquals(
                3,
                network.requests.stream()
                        .map(value -> value.getString("requestId"))
                        .distinct()
                        .count());
    }

    @Test
    void refusesFailedOrIncompleteSynchronizationReplies() {
        RecordingNetwork network = new RecordingNetwork();
        QuoteSvrClient client = new QuoteSvrClient(network, 3000);
        network.reply = "{\"Code\":\"1009\"}";
        assertThrows(IllegalStateException.class, () -> client.queryState(new JSONObject()));
        assertThrows(IllegalStateException.class, client::queryPriceSnapshot);
        network.reply = "{\"Code\":\"0000\"}";
        assertThrows(IllegalStateException.class, () -> client.queryState(new JSONObject()));
        assertThrows(IllegalStateException.class, client::queryPriceSnapshot);
    }

    private static final class RecordingNetwork extends NetClientWrapper {
        final List<String> topics = new ArrayList<>();
        final List<JSONObject> requests = new ArrayList<>();
        String reply = "{\"Code\":\"0000\",\"Data\":{}}";

        @Override
        public String requestSync(
                String server, String sessionId, String topic, String content, long timeout) {
            assertEquals("SERVER.QuoteSvr", server);
            assertNull(sessionId, "Internal synchronization must not require an end-user session");
            assertEquals(3000, timeout);
            JSONObject request = JsonUtils.parseObject(content);
            assertNotNull(request.getString("requestId"));
            assertFalse(request.containsKey("userName"));
            assertFalse(request.containsKey("password"));
            topics.add(topic);
            requests.add(request);
            return reply;
        }
    }
}
