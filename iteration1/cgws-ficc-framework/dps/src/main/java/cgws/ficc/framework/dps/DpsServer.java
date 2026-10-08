package cgws.ficc.framework.dps;

import com.gateway.connector.proto.Format;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.server.AbstractApiProxy;
import com.gateway.connector.tcp.server.MessageWrapper;
import com.gateway.connector.tcp.server.SystemMessage;
import com.gateway.connector.tcp.server.TcpServer;
import com.gateway.connector.utils.LicenseUtil;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.nio.charset.StandardCharsets;
import java.util.Map;
import java.util.function.Consumer;

/** Adapts service dispatch to the unchanged DPS implementation; it does not encode packets. */
public final class DpsServer extends AbstractApiProxy implements AutoCloseable {
    private static final Logger PAYLOAD = LoggerFactory.getLogger("ficc.dps");
    private final TcpServer transport;
    private final RequestHandler handler;
    private final Consumer<String> sessionStatus;
    private boolean closed;
    private boolean started;

    @FunctionalInterface
    public interface RequestHandler {
        Reply handle(String sessionId, String topic, String content);
    }

    public static final class Reply {
        private final byte[] body;
        private final Map<String, Object> values;

        public Reply(byte[] body, Map<String, Object> values) {
            this.body = body;
            this.values = values;
        }

        public static Reply text(String body) {
            return new Reply(
                    body == null ? null : body.getBytes(StandardCharsets.UTF_8),
                    body == null
                            ? null
                            : com.gateway.connector.utils.JsonUtils.Deserialize(body, Map.class));
        }
    }

    public DpsServer(RequestHandler handler, Consumer<String> sessionStatus) {
        this.transport = new TcpServer();
        this.handler = handler;
        this.sessionStatus = sessionStatus;
    }

    public synchronized void start(ServiceEndpoint endpoint, String licensePath) throws Exception {
        if (closed || started) {
            throw new IllegalStateException("DPS server already started or closed");
        }
        LicenseUtil.license_path = licensePath;
        try {
            transport.start(this, endpoint.getHost(), endpoint.getPort());
            started = true;
        } catch (Exception | LinkageError failure) {
            close();
            throw failure;
        }
    }

    @Override
    public MessageWrapper OnRequestReply(
            SystemMessage message, Proto protocol, String server, String topic, String content) {
        String session = protocol.getSessionId();
        String[] parts = session.split("=NOP=", 2);
        String connectionSession = parts[0];
        String userSession = parts.length == 2 ? parts[1] : session;
        MessageWrapper response =
                new MessageWrapper(
                        MessageWrapper.MessageProtocol.REPLY, connectionSession, protocol);
        if ("SessionStatus".equals(topic)) {
            sessionStatus.accept(content);
            protocol.setBody(null);
        } else {
            if (PAYLOAD.isDebugEnabled()) {
                PAYLOAD.debug(
                        "DPS direction=IN server={} topic={} body={}",
                        server,
                        topic,
                        DpsPayloadLog.body(content));
            }
            Reply reply = handler.handle(userSession, topic, content);
            protocol.setBody(reply.body);
            response.setBodyMap(reply.values);
            if (PAYLOAD.isDebugEnabled()) {
                PAYLOAD.debug(
                        "DPS direction=OUT server={} topic={} body={}",
                        server,
                        topic,
                        DpsPayloadLog.body(
                                reply.body == null
                                        ? null
                                        : new String(reply.body, StandardCharsets.UTF_8)));
            }
        }
        protocol.setFormat(Format.REPLY);
        return response;
    }

    @Override
    public synchronized void close() {
        if (closed) {
            return;
        }
        closed = true;
        try {
            transport.stop();
        } finally {
            executorService.shutdownNow();
        }
    }
}
