package ficc.common.client;

import java.util.Arrays;
import java.util.Properties;
import java.util.Set;
import java.util.stream.Collectors;

/**
 * 报价归属与可见性规则，使用 STC userid 判定，不使用 QTrade 昵称或机构名称代替身份。
 * 所有人、配置的管理员和内部用户可访问归属报价；普通用户还能看非 QT 来源的 ACTIVE 报价。
 * QT 报价确认后仍按接收人隔离；本类判断业务授权，不负责登录会话验证。
 */
public final class AccessPolicy {
    private final Set<String> administrators;
    private final Set<String> internalUsers;

    public AccessPolicy(Properties configuration) {
        boolean simulated =
                "simulator".equals(configuration.getProperty("identity.mode", "simulator"));
        administrators =
                values(
                        configuration.getProperty(
                                "identity.adminUserIds", simulated ? "U-ADMIN" : ""));
        internalUsers =
                values(
                        configuration.getProperty(
                                "identity.internalUserIds", simulated ? "S-SEARCH,S-INTAKE" : ""));
    }

    private static Set<String> values(String content) {
        return Arrays.stream(content.split(","))
                .map(String::trim)
                .filter(value -> !value.isEmpty())
                .collect(Collectors.toSet());
    }

    public boolean administrator(String userId) {
        return userId != null && administrators.contains(userId);
    }

    public boolean internal(String userId) {
        return userId != null && internalUsers.contains(userId);
    }

    public boolean owns(String userId, String ownerUserId) {
        return userId != null
                && (userId.equals(ownerUserId) || administrator(userId) || internal(userId));
    }

    /** QT 来源的报价始终归接收人；确认只改变状态，不将双边报价公开给其他普通用户。 */
    public boolean canViewQuote(String userId, String ownerUserId, String sourceType, String status) {
        return owns(userId, ownerUserId)
                || (userId != null && "ACTIVE".equals(status) && !"QT".equals(sourceType));
    }
}
