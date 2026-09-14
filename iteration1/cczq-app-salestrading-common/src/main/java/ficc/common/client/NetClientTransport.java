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
        if (!online()) {
            throw unavailable();
        }
        try {
            String content = network.requestSync(serverKey, sessionId, topic, JsonUtils.toJson(request), timeoutMillis);
            if (content == null || content.isBlank()) {
                throw unavailable();
            }
            return JsonUtils.parseObject(content);
        } catch (Exception error) {
            throw unavailable();
        }
    }

    private ApiException unavailable() {
        return ApiException.forField("1009", "NOT_READY", serverKey + " is unavailable", null);
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
