package ficc.common.client;

import java.util.Arrays;
import java.util.Properties;
import java.util.Set;
import java.util.stream.Collectors;

/** Explicit FICC grants; preserve original STC userid in business data and audit. */
public final class AccessPolicy {
    private final Set<String> administrators;
    private final Set<String> internalUsers;

    public AccessPolicy(Properties configuration) {
        boolean simulated = "simulator".equals(configuration.getProperty("identity.mode", "simulator"));
        administrators = values(configuration.getProperty("identity.adminUserIds", simulated ? "U-ADMIN" : ""));
        internalUsers = values(configuration.getProperty("identity.internalUserIds", simulated ? "S-SEARCH,S-INTAKE" : ""));
    }

    private static Set<String> values(String content) {
        return Arrays.stream(content.split(",")).map(String::trim).filter(value -> !value.isEmpty()).collect(Collectors.toSet());
    }

    public boolean administrator(String userId) {
        return userId != null && administrators.contains(userId);
    }

    public boolean internal(String userId) {
        return userId != null && internalUsers.contains(userId);
    }

    public boolean owns(String userId, String ownerUserId) {
        return userId != null && (userId.equals(ownerUserId) || administrator(userId) || internal(userId));
    }
}
