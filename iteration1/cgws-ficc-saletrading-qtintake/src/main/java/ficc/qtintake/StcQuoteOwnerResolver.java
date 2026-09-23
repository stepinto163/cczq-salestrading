package ficc.qtintake;

import org.apache.log4j.Logger;

import java.sql.*;

/** Same lookup as legacy UserInfoDao.queryUserByQt; no online user session is required. */
public final class StcQuoteOwnerResolver implements QuoteForwarder.OwnerResolver {
    private static final Logger LOGGER = Logger.getLogger(StcQuoteOwnerResolver.class);
    private final Connections connections;

    public interface Connections {
        Connection open() throws SQLException;
    }

    public StcQuoteOwnerResolver(Connections connections) {
        this.connections = connections;
    }

    @Override
    public String resolve(String qtradeAccount) throws SQLException {
        String sql = "SELECT userid FROM stc_tbook_users WHERE qtAccount = ? AND enable = 1";
        try (Connection connection = connections.open();
                PreparedStatement statement = connection.prepareStatement(sql)) {
            statement.setQueryTimeout(3);
            statement.setMaxRows(2);
            statement.setString(1, qtradeAccount);
            try (ResultSet rows = statement.executeQuery()) {
                if (!rows.next()) {
                    throw new IllegalStateException(
                            "No enabled STC user matches QTrade OwnQtNO=" + qtradeAccount);
                }
                String owner = rows.getString("userid");
                if (owner == null || owner.trim().isEmpty() || rows.next()) {
                    throw new IllegalStateException(
                            "QTrade account has a missing or ambiguous STC userid: "
                                    + qtradeAccount);
                }
                LOGGER.info(
                        "OWNER_RESOLVED sourceUserId=" + qtradeAccount + " ownerUserId=" + owner);
                return owner;
            }
        }
    }
}
