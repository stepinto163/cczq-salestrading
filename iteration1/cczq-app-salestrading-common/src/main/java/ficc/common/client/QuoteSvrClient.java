package ficc.common.client;

import ats.app.ym.ceda.ICedaConnectionListener;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.IdGenerator;
import ficc.common.JsonUtils;
import sc.app.stc.net.NetClientWrapper;

import java.util.function.Consumer;

/** Business client over STC discovery, connection management and subscriptions. */
public final class QuoteSvrClient {
    public static final String SERVER_KEY = "SERVER.QuoteSvr";
    public static final String QUOTE_EVENTS = "ficc.quote.events";
    public static final String PRICE_SNAPSHOTS = "ficc.price.snapshots";

    private final NetClientWrapper network;
    private final long requestTimeoutMillis;

    public QuoteSvrClient(NetClientWrapper network, long requestTimeoutMillis) {
        this.network = network;
        this.requestTimeoutMillis = requestTimeoutMillis;
    }

    public void initialize(ICedaConnectionListener listener) {
        network.addCedaClientConnectListener(SERVER_KEY, listener);
        network.init(SERVER_KEY);
    }

    public boolean isOnline() {
        return network.isOnline(SERVER_KEY);
    }

    public void subscribeQuoteEvents(Consumer<JSONObject> listener) {
        network.subscribe(SERVER_KEY, QUOTE_EVENTS,
                (topic, content) -> listener.accept(JsonUtils.parseObject(content)));
    }

    public void subscribePrices(Consumer<JSONObject> listener) {
        network.subscribe(SERVER_KEY, PRICE_SNAPSHOTS,
                (topic, content) -> listener.accept(JsonUtils.parseObject(content)));
    }

    public JSONObject queryState(String sessionId, JSONObject request) throws Exception {
        JSONObject payload = new JSONObject(request);
        payload.put("requestId", IdGenerator.newId("NS-"));
        String content = network.requestSync(SERVER_KEY, sessionId,
                "ficc.quote.state.query", JsonUtils.toJson(payload), requestTimeoutMillis);
        JSONObject reply = JsonUtils.parseObject(content);
        if (reply == null || !"0000".equals(reply.getString("Code"))) {
            throw new IllegalStateException("QuoteSvr state query failed");
        }
        return reply.getJSONObject("Data");
    }

    public JSONObject queryPriceSnapshot(String sessionId) throws Exception {
        // The supplied GwClient.subscribeWithImage does not forward its business sid.
        // Keep the ordinary subscription and fetch the native image with an explicit sid.
        String content = network.requestSync(SERVER_KEY, sessionId,
                PRICE_SNAPSHOTS, "", requestTimeoutMillis);
        JSONObject reply = JsonUtils.parseObject(content);
        if (reply == null || reply.containsKey("Code")) {
            throw new IllegalStateException("QuoteSvr price snapshot query failed");
        }
        String snapshot = reply.getString(PRICE_SNAPSHOTS);
        if (snapshot == null) {
            throw new IllegalStateException("QuoteSvr did not return a price snapshot");
        }
        return JsonUtils.parseObject(snapshot);
    }
}
