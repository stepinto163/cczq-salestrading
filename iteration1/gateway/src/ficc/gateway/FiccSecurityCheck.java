package ficc.gateway;

import com.gateway.invoke.security.*;
import com.gateway.message.SystemMessage;
import com.gateway.connector.proto.Proto;

/** FICC private stream authorization through the existing GW security extension. */
public final class FiccSecurityCheck implements ISecurityCheck {
    public SecurityResult check(SystemMessage system, Proto message, String server, String method, String content) {
        SecurityResult result = new SecurityResult();
        if ("ficc.quote.internal.state.query".equals(method) || "ficc.price.internal.snapshot.query".equals(method)) {
            result.Code = "1003";
            result.Msg = "Internal synchronization is not available through the frontend gateway";
            return result;
        }
        if ("subscribe".equals(method) || "unsubscribe".equals(method)) {
            String sessionId = message.getSessionId();
            if (sessionId == null || sessionId.isEmpty()
                    || !(content.equals("ficc.quote.report." + sessionId)
                    || content.equals("ficc.price.report." + sessionId)
                    || content.equals("ficc.notification.report." + sessionId))) {
                result.Code = "1003";
                result.Msg = "Only this session's FICC report topics may be subscribed";
            }
        }
        return result;
    }
}
