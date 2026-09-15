package ficc.common.client;

import ats.app.ym.ceda.ICedaConnectionListener;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import sc.app.stc.net.NetClientWrapper;
import java.util.function.Consumer;
import java.util.concurrent.atomic.AtomicLong;
import org.apache.log4j.Logger;

public final class NetClientTransport implements StcTransport {
    private static final Logger LOGGER = Logger.getLogger(NetClientTransport.class);
    private static final AtomicLong CALL_SEQUENCE = new AtomicLong();
    private final NetClientWrapper network;
    private final String serverKey;
    private final long timeoutMillis;

    public NetClientTransport(NetClientWrapper network, String serverKey, long timeoutMillis) {
        this.network = network;
        this.serverKey = serverKey;
        this.timeoutMillis = timeoutMillis;
        network.init(serverKey);
    }

    @Override
    public JSONObject request(String topic, String sessionId, JSONObject request) {
        long started = System.nanoTime();
        long callId = CALL_SEQUENCE.incrementAndGet();
        String requestContent = JsonUtils.toJson(request);
        log("REQUEST", topic, callId, started, requestContent);
        if (!online()) {
            throw unavailable(topic, "OFFLINE", started, callId);
        }
        String content;
        try {
            content = network.requestSync(serverKey, sessionId, topic, requestContent, timeoutMillis);
        } catch (Exception error) {
            throw unavailable(topic, "REQUEST_FAILED(" + error.getClass().getSimpleName() + ")", started, callId);
        }
        log("REPLY", topic, callId, started, content);
        if (content == null || content.isBlank()) {
            throw unavailable(topic, "NO_REPLY", started, callId);
        }
        JSONObject reply;
        try {
            reply = JsonUtils.parseObject(content);
        } catch (Exception error) {
            throw unavailable(topic, "INVALID_JSON", started, callId);
        }
        if (reply == null) {
            throw unavailable(topic, "NULL_JSON", started, callId);
        }
        return reply;
    }

    private ApiException unavailable(String topic, String reason, long started, long callId) {
        long elapsedMillis = (System.nanoTime() - started) / 1_000_000;
        LOGGER.warn("DPS ERROR server=" + serverKey + ", topic=" + topic + ", callId=" + callId
                + ", reason=" + reason + ", elapsedMillis=" + elapsedMillis + ", timeoutMillis=" + timeoutMillis);
        return ApiException.forField("1009", "NOT_READY",
                "STC request failed: server=" + serverKey + ", topic=" + topic + ", reason=" + reason
                        + ", elapsedMillis=" + elapsedMillis + ", timeoutMillis=" + timeoutMillis, null);
    }

    @Override
    public boolean online() {
        return network.isOnline(serverKey);
    }

    @Override
    public void subscribe(String topic, Consumer<JSONObject> listener) {
        log("SUBSCRIBE", topic, 0, System.nanoTime(), null);
        try {
            network.subscribe(serverKey, topic, (name, content) -> {
                log("PUSH", name, 0, System.nanoTime(), content);
                try {
                    listener.accept(JsonUtils.parseObject(content));
                } catch (RuntimeException error) {
                    LOGGER.warn("DPS PUSH_FAILED server=" + serverKey + ", topic=" + name
                            + ", errorType=" + error.getClass().getName());
                    throw error;
                }
            });
            log("SUBSCRIBE_REGISTERED", topic, 0, System.nanoTime(), null);
        } catch (RuntimeException error) {
            LOGGER.warn("DPS SUBSCRIBE_FAILED server=" + serverKey + ", topic=" + topic
                    + ", errorType=" + error.getClass().getName());
            throw error;
        }
    }

    @Override
    public void connectionChanged(Runnable listener) {
        network.addCedaClientConnectListener(serverKey, new ICedaConnectionListener() {
            @Override
            public void onConnected(String name) {
                LOGGER.info("DPS CONNECTED server=" + serverKey + ", name=" + name);
                listener.run();
            }
            @Override
            public void onDisconnected(String name) {
                LOGGER.warn("DPS DISCONNECTED server=" + serverKey + ", name=" + name);
                listener.run();
            }
        });
    }

    private void log(String direction, String topic, long callId, long started, String content) {
        if (LOGGER.isInfoEnabled()) {
            String context = "DPS " + direction + " server=" + serverKey + ", topic=" + topic
                    + ", callId=" + callId + ", elapsedMillis=" + (System.nanoTime() - started) / 1_000_000
                    + ", timeoutMillis=" + timeoutMillis;
            LOGGER.info(context + ", characters=" + (content == null ? 0 : content.length()));
            if (LOGGER.isDebugEnabled()) {
                LOGGER.debug(context + ", body=" + DpsMessageLog.body(content));
            }
        }
    }
}
