package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import sc.app.stc.net.NetClientWrapper;

/** Adapter for the existing local LoginSvr simulator hosted by QuoteSvr. */
public final class SimulatorLoginSvrClient implements LoginSessionClient {
    private final NetClientWrapper network;
    private final String serverKey;
    private final String username;
    private final String password;
    private final long requestTimeoutMillis;

    public SimulatorLoginSvrClient(NetClientWrapper network, String serverKey,
                                  String username, String password, long requestTimeoutMillis) {
        this.network = network;
        this.serverKey = serverKey;
        this.username = username;
        this.password = password;
        this.requestTimeoutMillis = requestTimeoutMillis;
        network.init(serverKey);
    }

    @Override
    public String loginService() throws Exception {
        JSONObject request = JsonUtils.object(
                "method", "login", "username", username, "password", password);
        JSONObject reply = request(request);
        if (!"0001".equals(reply.getString("Code")) || reply.getString("sid") == null) {
            throw new IllegalStateException("Simulator service login failed");
        }
        return reply.getString("sid");
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
