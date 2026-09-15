package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

public final class LocalLoginSimulator implements LoginSessionClient {
    private final Map<String, String> sessions = new ConcurrentHashMap<>();
    private static final Map<String, String> USERS = Map.of(
            "trader1", "U-001", "trader2", "U-002", "admin", "U-ADMIN",
            "intake", "S-INTAKE", "search", "S-SEARCH");

    public JSONObject login(JSONObject request) {
        JSONObject parameters = request.getJSONObject("parameter");
        if ("userInfo".equals(request.getString("method"))) {
            String sessionId = parameters == null ? null : parameters.getString("sid");
            String userId = resolveUser(sessionId);
            return userId == null ? JsonUtils.object("Code", "1003", "Msg", "Unknown simulator session")
                    : result(sessionId, userId);
        }
        String username = parameters == null ? request.getString("username") : parameters.getString("userName");
        String password = parameters == null ? request.getString("password") : parameters.getString("password");
        String userId = username == null ? null : USERS.get(username);
        if (userId == null || !"trader1".equals(password)) {
            return JsonUtils.object("Code", "1003", "Msg", "Invalid simulator login");
        }
        // A session token is a credential, not a time-ordered business identifier.
        String sessionId = "SIM-" + UUID.randomUUID();
        sessions.put(sessionId, userId);
        return result(sessionId, userId);
    }

    private JSONObject result(String sessionId, String userId) {
        return JsonUtils.object("Code", "0001", "Msg", "Fixed LoginSvr simulator",
                "sid", sessionId, "userId", userId, "userid", userId, "companyId", "FICC-TEST");
    }

    @Override
    public String resolveUser(String sessionId) {
        return sessionId == null ? null : sessions.get(sessionId);
    }

    @Override
    public String loginService() {
        return login(JsonUtils.object("method", "login", "username", "search", "password", "trader1")).getString("sid");
    }
}
