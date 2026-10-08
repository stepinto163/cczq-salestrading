package cgws.ficc.framework.dps;

import static org.junit.jupiter.api.Assertions.*;

import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.server.SystemMessage;

import org.junit.jupiter.api.Test;

import java.nio.charset.StandardCharsets;
import java.util.concurrent.atomic.AtomicReference;

class DpsDispatchTest {
    @Test
    void preservesGatewayLogicalSessionAndUtf8Reply() {
        AtomicReference<String> received = new AtomicReference<>();
        try (DpsServer server =
                new DpsServer(
                        (session, topic, content) -> {
                            received.set(session + ":" + topic + ":" + content);
                            return new DpsServer.Reply(
                                    "报价".getBytes(StandardCharsets.UTF_8),
                                    java.util.Collections.emptyMap());
                        },
                        ignored -> fail("unexpected session status"))) {
            Proto protocol = new Proto();
            protocol.setSessionId("gateway=NOP=trader");
            assertEquals(
                    "gateway",
                    server.OnRequestReply(
                                    new SystemMessage(),
                                    protocol,
                                    "SERVER.QuoteSvr",
                                    "ficc.quote.query",
                                    "{}")
                            .getSessionId());
            assertEquals("trader:ficc.quote.query:{}", received.get());
            assertEquals("报价", new String(protocol.getBody(), StandardCharsets.UTF_8));
        }
    }

    @Test
    void sessionStatusIsNotABusinessRequest() {
        AtomicReference<String> received = new AtomicReference<>();
        try (DpsServer server =
                new DpsServer(
                        (session, topic, content) -> {
                            fail("SessionStatus must not reach a business handler");
                            return null;
                        },
                        received::set)) {
            Proto protocol = new Proto();
            protocol.setSessionId("gateway");
            server.OnRequestReply(new SystemMessage(), protocol, "QuoteSvr", "SessionStatus", "{}");
            assertEquals("{}", received.get());
            assertNull(protocol.getBody());
        }
    }
}
