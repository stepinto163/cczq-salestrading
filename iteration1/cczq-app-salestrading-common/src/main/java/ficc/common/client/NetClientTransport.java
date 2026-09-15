package ficc.common.client;

import ats.app.ym.ceda.ICedaConnectionListener;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import sc.app.stc.net.NetClientWrapper;
import java.util.function.Consumer;

public final class NetClientTransport implements StcTransport {
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
        if (!online()) {
            throw unavailable(topic, "OFFLINE", started);
        }
        String content;
        try {
            content = network.requestSync(serverKey, sessionId, topic, JsonUtils.toJson(request), timeoutMillis);
        } catch (Exception error) {
            throw unavailable(topic, "REQUEST_FAILED(" + error.getClass().getSimpleName() + ")", started);
        }
        if (content == null || content.isBlank()) {
            throw unavailable(topic, "NO_REPLY", started);
        }
        JSONObject reply;
        try {
            reply = JsonUtils.parseObject(content);
        } catch (Exception error) {
            throw unavailable(topic, "INVALID_JSON", started);
        }
        if (reply == null) {
            throw unavailable(topic, "NULL_JSON", started);
        }
        return reply;
    }

    private ApiException unavailable(String topic, String reason, long started) {
        long elapsedMillis = (System.nanoTime() - started) / 1_000_000;
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
        network.subscribe(serverKey, topic, (name, content) -> listener.accept(JsonUtils.parseObject(content)));
    }

    @Override
    public void connectionChanged(Runnable listener) {
        network.addCedaClientConnectListener(serverKey, new ICedaConnectionListener() {
            @Override
            public void onConnected(String name) {
                listener.run();
            }
            @Override
            public void onDisconnected(String name) {
                listener.run();
            }
        });
    }
}
