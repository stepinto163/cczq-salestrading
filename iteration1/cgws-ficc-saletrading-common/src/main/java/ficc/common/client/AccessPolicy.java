package ficc.common.client;

import java.util.Arrays;
import java.util.Properties;
import java.util.Set;
import java.util.stream.Collectors;

/**
 * 报价归属与可见性规则，使用 STC userid 判定，不使用 QTrade 昵称或机构名称代替身份。
 * 已确认报价在本平台内共享；待确认和未确认即删除的输入仍按归属隔离。
 * 查看权限不代表修改权限，修改仍通过 owns 检查；本类不负责登录会话验证。
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

    /** 兼容只查询有效报价的调用；来源不再决定已确认报价的可见范围。 */
    public boolean canViewQuote(String userId, String ownerUserId, String sourceType, String status) {
        return canViewQuote(userId, ownerUserId, sourceType, status, null);
    }

    /** 删除状态必须有确认时间才可共享，避免公开被删除的私人待确认输入。 */
    public boolean canViewQuote(String userId, String ownerUserId, String sourceType,
            String status, String confirmTime) {
        return owns(userId, ownerUserId)
                || (userId != null && ("ACTIVE".equals(status) || "REFERRED".equals(status)
                || ("DELETED".equals(status) && confirmTime != null && !confirmTime.trim().isEmpty())));
    }
}
