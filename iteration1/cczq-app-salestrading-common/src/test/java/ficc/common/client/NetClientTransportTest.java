package ficc.common.client;

import ats.app.api.BaseApi.IMessage;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import org.junit.jupiter.api.Test;
import sc.app.stc.net.NetClientWrapper;
import java.io.StringWriter;
import org.apache.log4j.Level;
import org.apache.log4j.Logger;
import org.apache.log4j.PatternLayout;
import org.apache.log4j.WriterAppender;
import static org.junit.jupiter.api.Assertions.*;

final class NetClientTransportTest {
    @Test
    void logsCorrelatedRequestReplyAndPushWithoutChangingOrExposingSecrets() {
        Logger logger = Logger.getLogger(NetClientTransport.class);
        Level previousLevel = logger.getLevel();
        StringWriter output = new StringWriter();
        WriterAppender appender = new WriterAppender(new PatternLayout("%m%n"), output);
        logger.addAppender(appender);
        logger.setLevel(Level.DEBUG);
        try {
            FakeNetwork network = new FakeNetwork();
            network.reply = "{\"Code\":\"0000\",\"Datas\":[{\"Bond_ID\":\"250001\",\"password\":\"reply-secret\"}]}";
            NetClientTransport transport = new NetClientTransport(network, "SERVER.BaseSvr", 3000);
            JSONObject request = new JSONObject();
            request.put("SecurityID", "250001.IB");
            request.put("password", "request-secret");
            transport.request("querySecurity", "private-session", request);
            transport.subscribe("stc.base.clear", message -> {});
            network.subscription.onMessage("stc.base.clear", "{\"sid\":\"push-secret\",\"type\":\"ALL\"}");
            network.reply = null;
            assertThrows(ApiException.class, () -> transport.request("querySecurity", null, request));
            String log = output.toString();
            assertTrue(log.contains("DPS REQUEST"));
            assertTrue(log.contains("DPS REPLY"));
            assertTrue(log.contains("DPS PUSH"));
            assertTrue(log.contains("DPS SUBSCRIBE_REGISTERED"));
            assertTrue(log.contains("reason=NO_REPLY"));
            assertTrue(log.contains("250001.IB"));
            assertTrue(log.contains("Bond_ID"));
            String callId = log.substring(log.indexOf("callId="), log.indexOf(", elapsedMillis="));
            assertTrue(log.lines().filter(line -> line.contains(callId)).count() >= 2);
            assertFalse(log.contains("request-secret"));
            assertFalse(log.contains("reply-secret"));
            assertFalse(log.contains("push-secret"));
            assertFalse(log.contains("private-session"));
            assertEquals("request-secret", request.getString("password"));
            assertTrue(network.lastRequest.contains("request-secret"));
            assertFalse(DpsMessageLog.body("{\"content\":\"{\\\"password\\\":\\\"nested-secret\\\"}\"}").contains("nested-secret"));
        } finally {
            logger.removeAppender(appender);
            appender.close();
            logger.setLevel(previousLevel);
        }
    }

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
        String lastRequest;
        IMessage<String> subscription;

        @Override
        public void subscribe(String key, String topic, IMessage<String> listener) {
            subscription = listener;
        }

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
            lastRequest = content;
            if (fail) {
                throw new IllegalStateException("SENSITIVE-CONTENT");
            }
            return reply;
        }
    }
}
