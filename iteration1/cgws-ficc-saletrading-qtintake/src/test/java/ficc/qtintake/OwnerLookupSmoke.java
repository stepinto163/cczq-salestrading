package ficc.qtintake;

import java.io.BufferedReader;
import java.nio.file.*;
import java.sql.*;
import java.util.Properties;

/** Read-path check against an isolated local PostgreSQL table, never the real STC user table. */
public final class OwnerLookupSmoke {
    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(args[0]))) {
            configuration.load(reader);
        }
        String url = configuration.getProperty("db.url");
        Properties credentials = new Properties();
        credentials.setProperty("user", configuration.getProperty("db.user"));
        credentials.setProperty("password", configuration.getProperty("db.password"));
        credentials.setProperty("connectTimeout", "3");
        credentials.setProperty("socketTimeout", "5");
        String schema = "ficc_qtowner_check_" + System.currentTimeMillis();
        try (Connection setup = DriverManager.getConnection(url, credentials);
                Statement statement = setup.createStatement()) {
            statement.execute("CREATE SCHEMA " + schema);
            try {
                statement.execute("CREATE TABLE " + schema + ".stc_tbook_users(userid text, qtAccount text, enable integer)");
                statement.execute("INSERT INTO " + schema + ".stc_tbook_users VALUES ('STC-1','QT-1',1),('STC-2','QT-2',0),('STC-3','QT-3',1),('STC-4','QT-3',1)");
                StcQuoteOwnerResolver resolver = new StcQuoteOwnerResolver(() -> {
                    Connection connection = DriverManager.getConnection(url, credentials);
                    connection.setSchema(schema);
                    connection.setReadOnly(true);
                    return connection;
                });
                if (!"STC-1".equals(resolver.resolve("QT-1"))) {
                    throw new AssertionError("STC userid lookup failed");
                }
                for (String account : new String[] {"QT-2", "QT-3", "UNKNOWN", "' OR 1=1 --"}) {
                    try {
                        resolver.resolve(account);
                        throw new AssertionError("Expected unmapped/ambiguous rejection for " + account);
                    } catch (IllegalStateException expected) {
                        // An operator can repair STC data and the retained quote will be retried.
                    }
                }
                System.out.println("PASS PostgreSQL owner lookup: matched, disabled, ambiguous, unknown, bound SQL parameter");
            } finally {
                if (!schema.matches("ficc_qtowner_check_[0-9]+")) {
                    throw new AssertionError("Unexpected test schema");
                }
                statement.execute("DROP SCHEMA " + schema + " CASCADE");
            }
        }
    }
}
