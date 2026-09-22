package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.JsonUtils;

import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

public final class LocalLoginSimulator implements LoginSessionClient {
    private final Map<String, String> sessions = new ConcurrentHashMap<>();
    private static final Map<String, String> USERS = LocalLoginSimulator.defaultUsers();

    public JSONObject login(JSONObject request) {
        JSONObject parameters = request.getJSONObject("parameter");
        if ("userInfo".equals(request.getString("method"))) {
            String sessionId = parameters == null ? null : parameters.getString("sid");
            String userId = resolveUser(sessionId);
            return userId == null
                    ? JsonUtils.object("Code", "1003", "Msg", "Unknown simulator session")
                    : result(sessionId, userId);
        }
        String username =
                parameters == null
                        ? request.getString("username")
                        : parameters.getString("userName");
        String password =
                parameters == null
                        ? request.getString("password")
                        : parameters.getString("password");
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
        return JsonUtils.object(
                "Code",
                "0001",
                "Msg",
                "Fixed LoginSvr simulator",
                "sid",
                sessionId,
                "userId",
                userId,
                "userid",
                userId,
                "companyId",
                "FICC-TEST");
    }

    @Override
    public String resolveUser(String sessionId) {
        return sessionId == null ? null : sessions.get(sessionId);
    }

    private static Map<String, String> defaultUsers() {
        Map<String, String> users = new java.util.LinkedHashMap<>();
        users.put("trader1", "U-001");
        users.put("trader2", "U-002");
        users.put("admin", "U-ADMIN");
        users.put("intake", "S-INTAKE");
        users.put("search", "S-SEARCH");
        return java.util.Collections.unmodifiableMap(users);
    }
}
