package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import sc.app.stc.net.NetClientWrapper;

/** Adapter for the existing local LoginSvr simulator hosted by QuoteSvr. */
public final class SimulatorLoginSvrClient implements LoginSessionClient {
    private final NetClientWrapper network;
    private final String serverKey;
    private final long requestTimeoutMillis;

    public SimulatorLoginSvrClient(NetClientWrapper network, String serverKey,
                                  long requestTimeoutMillis) {
        this.network = network;
        this.serverKey = serverKey;
        this.requestTimeoutMillis = requestTimeoutMillis;
        network.init(serverKey);
    }

    @Override
    public String resolveUser(String sessionId) throws Exception {
        JSONObject request = JsonUtils.object("method", "userInfo",
                "parameter", JsonUtils.object("sid", sessionId));
        JSONObject reply = request(request);
        return "0001".equals(reply.getString("Code")) ? reply.getString("userId") : null;
    }

    private JSONObject request(JSONObject request) throws Exception {
        String content = network.requestSync(serverKey, null, "SYS.ATS.LOGIN",
                JsonUtils.toJson(request), requestTimeoutMillis);
        JSONObject reply = JsonUtils.parseObject(content);
        if (reply == null) {
            throw new IllegalStateException("Simulator LoginSvr unavailable");
        }
        return reply;
    }
}
