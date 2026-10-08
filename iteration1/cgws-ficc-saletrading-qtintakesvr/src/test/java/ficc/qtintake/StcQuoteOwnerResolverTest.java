package ficc.qtintake;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import ficc.common.client.StcTransport;
import org.apache.log4j.FileAppender;
import org.apache.log4j.Level;
import org.apache.log4j.Logger;
import org.apache.log4j.PatternLayout;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.Timeout;
import org.junit.jupiter.api.condition.EnabledIfEnvironmentVariable;
import org.junit.jupiter.api.io.TempDir;
import quickfix.Message;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Properties;
import java.util.UUID;
import java.util.function.Consumer;

import static org.junit.jupiter.api.Assertions.*;

/** Real PostgreSQL tests in a temporary schema; never modifies STC users in public. */
@EnabledIfEnvironmentVariable(named = "FICC_TEST_PG_URL", matches = ".+")
@Timeout(15)
class StcQuoteOwnerResolverTest {
    @TempDir Path directory;
    private String schema;
    private String url;
    private final Properties credentials = new Properties();
    private final Properties configuration = new Properties();

    private Connection connect() throws SQLException {
        return DriverManager.getConnection(url, credentials);
    }

    private Connection connectionInSchema() throws SQLException {
        Connection connection = connect();
        try {
            connection.setSchema(schema);
            return connection;
        } catch (SQLException error) {
            connection.close();
            throw error;
        }
    }

    @BeforeEach
    void createIsolatedSchema() throws Exception {
        url = System.getenv("FICC_TEST_PG_URL");
        assertTrue(url.matches("jdbc:postgresql://(127\\.0\\.0\\.1|localhost):[0-9]+/[^?]+"),
                "Owner lookup integration tests require local PostgreSQL");
        credentials.setProperty("user", System.getenv("FICC_TEST_PG_USER"));
        credentials.setProperty("password", System.getenv("FICC_TEST_PG_PASSWORD"));
        credentials.setProperty("connectTimeout", "3");
        credentials.setProperty("socketTimeout", "5");
        configuration.setProperty("diagnostics.serviceName", "QtIntakeSvr");
        schema = "qt_owner_test_" + UUID.randomUUID().toString().replace("-", "");
        try (Connection connection = connect(); Statement statement = connection.createStatement()) {
            statement.execute("CREATE SCHEMA " + schema);
        }
        execute("CREATE TABLE stc_tbook_users (userid varchar(64), inf1 text, enable char(1))");
    }

    @AfterEach
    void removeIsolatedSchema() throws Exception {
        if (schema == null) {
            return;
        }
        try (Connection connection = connect(); Statement statement = connection.createStatement()) {
            statement.execute("DROP SCHEMA IF EXISTS " + schema + " CASCADE");
        }
    }

    private void execute(String sql) throws SQLException {
        try (Connection connection = connectionInSchema(); Statement statement = connection.createStatement()) {
            statement.execute(sql);
        }
    }

    private void insert(String user, String account, String enabled) throws SQLException {
        try (Connection connection = connectionInSchema();
                PreparedStatement statement = connection.prepareStatement(
                        "INSERT INTO stc_tbook_users(userid, inf1, enable) VALUES (?, ?, ?)")) {
            statement.setString(1, user);
            statement.setString(2, account);
            statement.setString(3, enabled);
            statement.executeUpdate();
        }
    }

    private StcQuoteOwnerResolver resolver() throws Exception {
        return StcQuoteOwnerResolver.initialize(configuration, this::connectionInSchema);
    }

    @Test
    void resolvesExactInf1OfEnabledUserWithoutFileOrDefault() throws Exception {
        insert("STC-1", "2881631456", "1");
        insert("STC-2", "2881631456", "0");
        assertEquals("STC-1", resolver().resolve("2881631456"));
    }

    @Test
    void missingAndDisabledUsersThrowEvenWithObsoleteDefaultConfigured() throws Exception {
        configuration.setProperty("owner.defaultUserId", "MUST-NOT-BE-USED");
        configuration.setProperty("owner.mapFile", directory.resolve("missing.properties").toString());
        insert("DISABLED", "2881631456", "0");
        StcQuoteOwnerResolver owners = resolver();
        assertThrows(IllegalStateException.class, () -> owners.resolve("2881631456"));
        assertThrows(IllegalStateException.class, () -> owners.resolve("unknown"));
    }

    @Test
    void duplicateEnabledUsersThrow() throws Exception {
        insert("STC-1", "2881631456", "1");
        insert("STC-2", "2881631456", "1");
        assertThrows(IllegalStateException.class, () -> resolver().resolve("2881631456"));
    }

    @Test
    void blankUserIdsAndBlankAccountsThrow() throws Exception {
        insert(" ", "2881631456", "1");
        StcQuoteOwnerResolver owners = resolver();
        assertThrows(IllegalStateException.class, () -> owners.resolve("2881631456"));
        assertThrows(IllegalArgumentException.class, () -> owners.resolve(null));
        assertThrows(IllegalArgumentException.class, () -> owners.resolve(" "));
    }

    @Test
    void lookupUsesBoundExactValueNotSqlOrSubstringMatching() throws Exception {
        insert("STC-1", "2881631456", "1");
        StcQuoteOwnerResolver owners = resolver();
        assertThrows(IllegalStateException.class, () -> owners.resolve("288163"));
        assertThrows(IllegalStateException.class, () -> owners.resolve("' OR '1'='1"));
    }

    @Test
    void databaseChangesAreVisibleWithoutRestart() throws Exception {
        StcQuoteOwnerResolver owners = resolver();
        assertThrows(IllegalStateException.class, () -> owners.resolve("2881631456"));
        insert("STC-1", "2881631456", "1");
        assertEquals("STC-1", owners.resolve("2881631456"));
        execute("UPDATE stc_tbook_users SET enable='0'");
        assertThrows(IllegalStateException.class, () -> owners.resolve("2881631456"));
    }

    @Test
    void databaseErrorsPropagateDuringLookup() throws Exception {
        StcQuoteOwnerResolver owners = resolver();
        execute("DROP TABLE stc_tbook_users");
        assertThrows(SQLException.class, () -> owners.resolve("2881631456"));
    }

    @Test
    void startupLogsSuccessAndMissingSchemaFailureToFileAndPropagates() throws Exception {
        Logger logger = Logger.getLogger("ficc.startup");
        Level previous = logger.getLevel();
        Path log = directory.resolve("startup.log");
        FileAppender appender = new FileAppender(new PatternLayout("%p %m%n"), log.toString());
        logger.setLevel(Level.INFO);
        logger.addAppender(appender);
        try {
            resolver();
            execute("ALTER TABLE stc_tbook_users DROP COLUMN inf1");
            assertThrows(SQLException.class, this::resolver);
        } finally {
            logger.removeAppender(appender);
            appender.close();
            logger.setLevel(previous);
        }
        String output = new String(Files.readAllBytes(log), StandardCharsets.UTF_8);
        assertTrue(output.contains("\"state\":\"INITIALIZED\""));
        assertTrue(output.contains("ERROR COMPONENT"));
        assertTrue(output.contains("\"state\":\"FAILED\""));
        assertTrue(output.contains("OWNER_LOOKUP"));
        assertTrue(output.contains("MYSQL0/stc_tbook_users.inf1"));
        assertTrue(output.contains("org.postgresql.util.PSQLException"));
        assertTrue(output.contains("at ficc.qtintake.StcQuoteOwnerResolver"));
    }

    @Test
    void startupConnectionFailurePropagates() {
        SQLException failure = new SQLException("test database connection unavailable");
        assertSame(failure, assertThrows(SQLException.class,
                () -> StcQuoteOwnerResolver.initialize(configuration, () -> { throw failure; })));
    }

    @Test
    void repairedMappingDrainsOriginalHeadBeforeNextMessage() throws Exception {
        List<String> sent = new ArrayList<>();
        StcTransport transport = new StcTransport() {
            public boolean online() { return true; }
            public void subscribe(String topic, Consumer<JSONObject> listener) { }
            public void connectionChanged(Runnable listener) { }
            public JSONObject request(String topic, String session, JSONObject request) {
                sent.add(request.getString("requestId"));
                assertEquals("STC-1", request.getJSONObject("intake").getString("ownerUserId"));
                return JsonUtils.object("Code", "0000", "Data", JsonUtils.object("results",
                        JsonUtils.array(JsonUtils.object("Code", "0000"))));
            }
        };
        Message firstMessage = QtIntakeTest.quote("1", "1");
        firstMessage.setString(117, "FIRST");
        firstMessage.setString(18004, "2881631456");
        JSONObject first = new QTradeQuoteMapper().map(firstMessage);
        firstMessage.setString(117, "SECOND");
        JSONObject second = new QTradeQuoteMapper().map(firstMessage);
        try (DurableInbox inbox = new DurableInbox(directory.resolve("inbox"), 10)) {
            inbox.enqueue(first);
            inbox.enqueue(second);
            Path head = inbox.next();
            QuoteForwarder forwarder = new QuoteForwarder(inbox, transport, "test-token", resolver());
            for (int attempt = 0; attempt < 12; attempt++) {
                assertThrows(IllegalStateException.class, forwarder::tick);
                assertEquals(head, inbox.next());
                assertEquals(2, inbox.pendingCount());
            }
            assertTrue(sent.isEmpty());
            insert("STC-1", "2881631456", "1");
            assertEquals(QuoteForwarder.Outcome.COMPLETED, forwarder.tick());
            assertEquals(QuoteForwarder.Outcome.COMPLETED, forwarder.tick());
            assertEquals(Arrays.asList(first.getString("requestId"), second.getString("requestId")), sent);
            assertEquals(0, inbox.pendingCount());
        }
    }
}
