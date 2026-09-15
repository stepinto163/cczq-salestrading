package ficc.common.client;

import ats.app.api.BaseApi.IMessage;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import org.junit.jupiter.api.Test;
import sc.app.stc.net.NetClientWrapper;
import static org.junit.jupiter.api.Assertions.*;

final class NetClientTransportTest {
    @Test
    void distinguishesOfflineNoReplyAndMalformedReplyWithoutLoggingCredentials() {
        FakeNetwork network = new FakeNetwork();
        NetClientTransport transport = new NetClientTransport(network, "SERVER.BaseSvr", 3000);
        network.online = false;
        assertReason(transport, "OFFLINE");
        assertEquals(0, network.requests);
        network.online = true;
        network.reply = null;
        assertReason(transport, "NO_REPLY");
        network.reply = "not-json-SENSITIVE-CONTENT";
        assertReason(transport, "INVALID_JSON");
        network.reply = "null";
        assertReason(transport, "NULL_JSON");
        network.fail = true;
        assertReason(transport, "REQUEST_FAILED(IllegalStateException)");
    }

    @Test
    void keepsBusinessErrorCodesForTheCallerToInterpret() {
        FakeNetwork network = new FakeNetwork();
        network.reply = "{\"Code\":\"1003\",\"Msg\":\"denied\"}";
        NetClientTransport transport = new NetClientTransport(network, "SERVER.BaseSvr", 3000);
        assertEquals("1003", transport.request("querySecurity", null, new JSONObject()).getString("Code"));
    }

    private static void assertReason(NetClientTransport transport, String reason) {
        JSONObject request = new JSONObject();
        request.put("password", "SENSITIVE-CONTENT");
        ApiException failure = assertThrows(ApiException.class,
                () -> transport.request("querySecurity", "PRIVATE-SID", request));
        assertEquals("1009", failure.code);
        assertTrue(failure.getMessage().contains("server=SERVER.BaseSvr"));
        assertTrue(failure.getMessage().contains("topic=querySecurity"));
        assertTrue(failure.getMessage().contains("reason=" + reason));
        assertTrue(failure.getMessage().contains("timeoutMillis=3000"));
        assertFalse(failure.getMessage().contains("SENSITIVE-CONTENT"));
        assertFalse(failure.getMessage().contains("PRIVATE-SID"));
    }

    private static final class FakeNetwork extends NetClientWrapper {
        boolean online = true;
        boolean fail;
        int requests;
        String reply;

        @Override
        public void init(String key, String... names) {}

        @Override
        public void init(String key, IMessage<String> listener) {}

        @Override
        public boolean isOnline(String key) {
            return online;
        }

        @Override
        public String requestSync(String key, String sid, String topic, String content, long timeout) {
            requests++;
            if (fail) {
                throw new IllegalStateException("SENSITIVE-CONTENT");
            }
            return reply;
        }
    }
}
