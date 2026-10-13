package ficc.qtintake;

import ficc.api.QTradePublishRequest;
import ficc.common.ApiException;
import ficc.common.client.LoginSessionClient;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;

/** 服务凭据与普通用户会话两种入口；不接受请求正文里的 ownerUserId。 */
final class QTradePublishAuthorization {
    private QTradePublishAuthorization() {}

    static String authorize(QTradePublishRequest request, String sessionId, String serviceToken,
            LoginSessionClient identity, QuoteForwarder.OwnerResolver owners) throws Exception {
        if (request.token != null) {
            if (serviceToken == null || serviceToken.length() < 32 || !MessageDigest.isEqual(
                    request.token.getBytes(StandardCharsets.UTF_8), serviceToken.getBytes(StandardCharsets.UTF_8))) {
                throw forbidden();
            }
            return "service";
        }
        String userId = identity.resolveUser(sessionId);
        if (userId == null || !userId.equals(owners.resolve(request.senderQtAccount))) {
            throw forbidden();
        }
        return userId;
    }

    private static ApiException forbidden() {
        return ApiException.forField("1003", "FORBIDDEN",
                "Publishing requires a valid service credential or the sender's authenticated STC user", "senderQtAccount");
    }
}
