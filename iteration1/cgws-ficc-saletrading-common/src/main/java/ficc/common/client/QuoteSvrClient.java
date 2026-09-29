package ficc.common.client;

import ats.app.ym.ceda.ICedaConnectionListener;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.IdGenerator;
import ficc.api.*;
import ficc.common.JsonUtils;

import sc.app.stc.net.NetClientWrapper;

import java.util.function.Consumer;

/** Business client over STC discovery, connection management and subscriptions. */
public final class QuoteSvrClient {
    public static final String SERVER_KEY = "SERVER.QuoteSvr";
    public static final String QUOTE_EVENTS = "ficc.quote.events";
    public static final String PRICE_SNAPSHOTS = "ficc.price.snapshots";
    public static final String INTERNAL_STATE_QUERY = "ficc.quote.internal.state.query";
    public static final String INTERNAL_PRICE_QUERY = "ficc.price.internal.snapshot.query";

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

    public void subscribeQuoteEvents(Consumer<QuoteEventBatch> listener) {
        network.subscribe(
                SERVER_KEY,
                QUOTE_EVENTS,
                (topic, content) -> listener.accept(ProtocolCodec.decode(JsonUtils.parseObject(content), QuoteEventBatch.class)));
    }

    public void subscribePrices(Consumer<PriceFeedBatch> listener) {
        network.subscribe(
                SERVER_KEY,
                PRICE_SNAPSHOTS,
                (topic, content) -> listener.accept(ProtocolCodec.decode(JsonUtils.parseObject(content), PriceFeedBatch.class)));
    }

    public QuoteStateData queryState(StateQueryRequest request) throws Exception {
        return ProtocolCodec.decode(queryState(request.toJson()), QuoteStateData.class);
    }

    public PriceFeedBatch queryPriceFeed() throws Exception {
        return ProtocolCodec.decode(queryPriceSnapshot(), PriceFeedBatch.class);
    }

    public JSONObject queryState(JSONObject request) throws Exception {
        JSONObject payload = new JSONObject(request);
        payload.put("requestId", IdGenerator.newId("NS-"));
        String content =
                network.requestSync(
                        SERVER_KEY,
                        null,
                        INTERNAL_STATE_QUERY,
                        JsonUtils.toJson(payload),
                        requestTimeoutMillis);
        JSONObject reply = JsonUtils.parseObject(content);
        if (reply == null || !"0000".equals(reply.getString("Code"))) {
            throw new IllegalStateException("QuoteSvr state query failed");
        }
        JSONObject state = reply.getJSONObject("Data");
        if (state == null) {
            throw new IllegalStateException("QuoteSvr did not return synchronization state");
        }
        return state;
    }

    public JSONObject queryPriceSnapshot() throws Exception {
        // Internal service calls use STC discovery/transport, not an end-user login session.
        JSONObject request = JsonUtils.object("requestId", IdGenerator.newId("NS-"));
        String content =
                network.requestSync(
                        SERVER_KEY,
                        null,
                        INTERNAL_PRICE_QUERY,
                        JsonUtils.toJson(request),
                        requestTimeoutMillis);
        JSONObject reply = JsonUtils.parseObject(content);
        if (reply == null || !"0000".equals(reply.getString("Code"))) {
            throw new IllegalStateException("QuoteSvr price snapshot query failed");
        }
        JSONObject snapshot = reply.getJSONObject("Data");
        if (snapshot == null) {
            throw new IllegalStateException("QuoteSvr did not return a price snapshot");
        }
        return snapshot;
    }
}
