package ficc.qtintake;

import ficc.common.ComponentLog;
import org.apache.log4j.Logger;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Properties;

/**
 * 用收到消息中的 OwnQtNO(18004) 精确匹配 stc_tbook_users.inf1，返回启用用户的 STC userid。
 * 这是本方接收人映射，不是对手方联系人查找；缺失、重复或空 userid 都抛异常，不设默认归属人。
 * 每次解析直接查询数据库，修正映射后队首重试可以重新解析；成功映射随后由转发器写回收件箱。
 */
public final class StcQuoteOwnerResolver implements QuoteForwarder.OwnerResolver {
    private static final Logger LOGGER = Logger.getLogger(StcQuoteOwnerResolver.class);
    private static final String LOOKUP_SQL =
            "SELECT userid FROM stc_tbook_users WHERE inf1 = ? AND enable = '1'";
    private final Connections connections;

    @FunctionalInterface
    public interface Connections {
        Connection open() throws SQLException;
    }

    private StcQuoteOwnerResolver(Connections connections) {
        this.connections = connections;
    }

    public static StcQuoteOwnerResolver initialize(Properties configuration, Connections connections)
            throws Exception {
        return ComponentLog.initialize(configuration, "OWNER_LOOKUP", "stc",
                "StcQuoteOwnerResolver", "MYSQL0/stc_tbook_users.inf1", () -> {
                    // Check connectivity and the required columns before starting the FIX receiver.
                    try (Connection connection = connections.open();
                            PreparedStatement statement = connection.prepareStatement(
                                    "SELECT userid, inf1, enable FROM stc_tbook_users WHERE 1 = 0")) {
                        statement.setQueryTimeout(3);
                        try (ResultSet ignored = statement.executeQuery()) {
                            return new StcQuoteOwnerResolver(connections);
                        }
                    }
                });
    }

    @Override
    public String resolve(String qtradeAccount) throws SQLException {
        if (qtradeAccount == null || qtradeAccount.trim().isEmpty()) {
            throw new IllegalArgumentException("QTrade OwnQtNO(18004) is required for owner lookup");
        }
        String account = qtradeAccount.trim();
        try (Connection connection = connections.open();
                PreparedStatement statement = connection.prepareStatement(LOOKUP_SQL)) {
            statement.setQueryTimeout(3);
            statement.setMaxRows(2);
            statement.setString(1, account);
            try (ResultSet rows = statement.executeQuery()) {
                if (!rows.next()) {
                    throw new IllegalStateException("No enabled STC user matches OwnQtNO=" + account
                            + " in stc_tbook_users.inf1");
                }
                String userId = rows.getString("userid");
                if (userId == null || userId.trim().isEmpty()) {
                    throw new IllegalStateException("STC userid is blank for OwnQtNO=" + account);
                }
                if (rows.next()) {
                    throw new IllegalStateException("Multiple enabled STC users match OwnQtNO="
                            + account + " in stc_tbook_users.inf1");
                }
                LOGGER.info("OWNER_RESOLVED sourceUserId=" + account + " ownerUserId=" + userId);
                return userId;
            }
        }
    }
}
