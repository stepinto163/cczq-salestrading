package ficc.common.client;

import ficc.common.ApiException;
import ficc.common.stc.LoginSvrClient;

import sc.app.stc.net.NetClientWrapper;

import java.util.Properties;
import java.util.function.Consumer;

/** FICC adapter around the supplied STC LoginSvrClient source implementation. */
public final class StcLoginSessionClient implements LoginSessionClient {
    private static final String TOPIC = "SYS.ATS.LOGIN";
    private final StcTransport transport;
    private volatile Consumer<String> invalidated = sessionId -> {};

    public StcLoginSessionClient(NetClientWrapper network, Properties configuration) {
        String serverKey = configuration.getProperty("identity.serverKey", "SERVER.LoginSvr");
        long timeout =
                Long.parseLong(configuration.getProperty("identity.requestTimeoutMillis", "3000"));
        if (timeout < 100 || timeout > 10000) {
            throw new IllegalArgumentException(
                    "identity.requestTimeoutMillis must be between 100 and 10000");
        }
        transport = new NetClientTransport(network, serverKey, timeout);
        LoginSvrClient.clear();
        LoginSvrClient.setRequestTimeoutMillis(timeout);
        LoginSvrClient.initialize(serverKey, network);
        transport.connectionChanged(() -> invalidate(null));
        transport.subscribe(
                TOPIC,
                event -> {
                    String type = event.getString("type");
                    if ("logout".equals(type) || "update".equals(type)) {
                        invalidate(event.getString("sid"));
                    }
                });
    }

    @Override
    public void onInvalidated(Consumer<String> listener) {
        invalidated = listener;
    }

    private void invalidate(String sessionId) {
        LoginSvrClient.clear();
        invalidated.accept(sessionId);
    }

    @Override
    public String resolveUser(String sessionId) {
        if (missingIdentity(sessionId)) {
            throw ApiException.forField(
                    "1003",
                    "FORBIDDEN",
                    "STC login did not provide a valid session ID; use a registered STC business"
                            + " account",
                    null);
        }
        if (!transport.online()) {
            invalidate(null);
            throw ApiException.forField("1009", "NOT_READY", "LoginSvr is unavailable", null);
        }
        LoginSvrClient.UserInfo user = LoginSvrClient.get(sessionId);
        if (user == null || !sessionId.equals(user.sid) || !transport.online()) {
            return null;
        }
        if (missingIdentity(user.userid)) {
            throw ApiException.forField(
                    "1003",
                    "FORBIDDEN",
                    "STC session has no business user ID; use a registered STC business account",
                    null);
        }
        return user.userid;
    }

    private static boolean missingIdentity(String value) {
        return value == null || value.trim().isEmpty() || "null".equalsIgnoreCase(value.trim());
    }
}
