package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import sc.app.stc.net.NetClientWrapper;

/** Adapter for the existing local LoginSvr simulator hosted by QuoteSvr. */
public final class SimulatorLoginSvrClient implements LoginSessionClient {
    private final NetClientTransport transport;

    public SimulatorLoginSvrClient(NetClientWrapper network, String serverKey,
                                  long requestTimeoutMillis) {
        transport = new NetClientTransport(network, serverKey, requestTimeoutMillis);
    }

    @Override
    public String resolveUser(String sessionId) throws Exception {
        JSONObject request = JsonUtils.object("method", "userInfo",
                "parameter", JsonUtils.object("sid", sessionId));
        JSONObject reply = request(request);
        return "0001".equals(reply.getString("Code")) ? reply.getString("userId") : null;
    }

    private JSONObject request(JSONObject request) throws Exception {
        return transport.request("SYS.ATS.LOGIN", null, request);
    }
}
