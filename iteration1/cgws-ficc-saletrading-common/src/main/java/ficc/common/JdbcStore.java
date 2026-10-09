package ficc.common;

import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Properties;

/**
 * PostgreSQL 持久化基础：维护事务、事件序列和请求回执，供业务服务协调写入。
 * QuoteSvr 与 NotificationSvr 可共用 schema，但各自使用独立的事件/回执表前缀和写入锁。
 * advisory lock 限制同一服务存储只有一个写入进程；它不是线程锁，也不负责服务发现。
 * 本类持有单个连接，调用方必须串行协调事务，不能将同一个实例当作并发连接池使用。
 */
public class JdbcStore implements AutoCloseable {
    private final Connection db;
    private final String tablePrefix;
    public final String epoch;

    public JdbcStore(Properties configuration) throws Exception {
        this(configuration, "");
    }

    public JdbcStore(Properties configuration, String tablePrefix) throws Exception {
        if (!Arrays.asList("", "notification_").contains(tablePrefix)) {
            throw new IllegalArgumentException("Unsupported storage table prefix");
        }
        this.tablePrefix = tablePrefix;
        String schema = configuration.getProperty("db.schema", "ficc_salestrading");
        if (!schema.matches("ficc_[a-z0-9_]+")) {
            throw new IllegalArgumentException("Use a dedicated ficc_* schema");
        }

        Properties connectionProperties = new Properties();
        connectionProperties.setProperty("user", configuration.getProperty("db.user"));
        connectionProperties.setProperty("password", configuration.getProperty("db.password", ""));
        connectionProperties.setProperty("connectTimeout", "5");
        connectionProperties.setProperty("socketTimeout", "10");
        db = DriverManager.getConnection(configuration.getProperty("db.url"), connectionProperties);

        try {
            // Do not silently create an empty product schema beside an existing installation.
            if ("ficc_salestrading".equals(schema)) {
                try (PreparedStatement statement =
                                db.prepareStatement(
                                        "SELECT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname ="
                                            + " 'ficc_demo') AND NOT EXISTS (SELECT 1 FROM"
                                            + " pg_namespace WHERE nspname = 'ficc_salestrading')");
                        ResultSet result = statement.executeQuery()) {
                    result.next();
                    if (result.getBoolean(1)) {
                        throw new IllegalStateException(
                                "Existing schema requires 003-rename-salestrading-schema.sql;"
                                        + " retain the existing db.schema until migration is"
                                        + " completed");
                    }
                }
            }
            // Two different services can share a schema; each still has only one writer.
            String writer = tablePrefix.isEmpty() ? "quotesvr" : "notificationsvr";
            try (PreparedStatement statement =
                    db.prepareStatement("SELECT pg_try_advisory_lock(hashtext(?))")) {
                statement.setString(1, schema + "." + writer + ".writer");
                try (ResultSet result = statement.executeQuery()) {
                    result.next();
                    if (!result.getBoolean(1)) {
                        throw new IllegalStateException("Another " + writer + " owns this storage");
                    }
                }
            }

            exec("CREATE SCHEMA IF NOT EXISTS " + schema);
            exec("SET search_path TO " + schema + ",public");
            exec("SET statement_timeout = '8s'");
            exec("SET timezone = 'Asia/Shanghai'");
            exec(
                    "CREATE TABLE IF NOT EXISTS "
                            + table("metadata")
                            + " (key text PRIMARY KEY, value text NOT NULL)");
            exec(
                    "CREATE TABLE IF NOT EXISTS "
                            + table("events")
                            + " (sequence bigserial PRIMARY KEY, payload jsonb NOT NULL)");
            exec(
                    "CREATE TABLE IF NOT EXISTS "
                            + table("requests")
                            + " (owner_id text, topic text, request_id text, hash text NOT NULL,"
                            + " results jsonb NOT NULL, PRIMARY KEY (owner_id, topic,"
                            + " request_id))");
            exec(
                    "CREATE TABLE IF NOT EXISTS "
                            + table("settings")
                            + " (id text PRIMARY KEY, owner_id text NOT NULL, kind text NOT NULL,"
                            + " revision bigint NOT NULL, payload jsonb NOT NULL)");
            update(
                    "INSERT INTO "
                            + table("metadata")
                            + " VALUES ('epoch', ?) ON CONFLICT DO NOTHING",
                    IdGenerator.newId("QE-"));
            epoch = scalar("SELECT value FROM " + table("metadata") + " WHERE key='epoch'");
        } catch (Exception error) {
            try {
                db.close();
            } catch (SQLException closeError) {
                error.addSuppressed(closeError);
            }
            throw error;
        }
    }

    public String table(String name) {
        if (!Arrays.asList("metadata", "events", "requests", "settings").contains(name)) {
            throw new IllegalArgumentException("Unsupported common table: " + name);
        }
        return tablePrefix + name;
    }

    public void exec(String sql) throws SQLException {
        try (Statement statement = db.createStatement()) {
            statement.execute(sql);
        }
    }

    public int update(String sql, Object... arguments) throws SQLException {
        try (PreparedStatement statement = db.prepareStatement(sql)) {
            bind(statement, arguments);
            return statement.executeUpdate();
        }
    }

    public static void bind(PreparedStatement statement, Object... arguments) throws SQLException {
        for (int index = 0; index < arguments.length; index++) {
            statement.setObject(index + 1, arguments[index]);
        }
    }

    public List<JSONObject> rows(String sql, Object... arguments) throws SQLException {
        List<JSONObject> rows = new ArrayList<>();
        try (PreparedStatement statement = db.prepareStatement(sql)) {
            bind(statement, arguments);
            try (ResultSet result = statement.executeQuery()) {
                while (result.next()) {
                    rows.add(JsonUtils.parseObject(result.getString(1)));
                }
            }
        }
        return rows;
    }

    public String scalar(String sql, Object... arguments) throws SQLException {
        try (PreparedStatement statement = db.prepareStatement(sql)) {
            bind(statement, arguments);
            try (ResultSet result = statement.executeQuery()) {
                return result.next() ? result.getString(1) : null;
            }
        }
    }

    public void begin() throws SQLException {
        db.setAutoCommit(false);
    }

    public void commit() throws SQLException {
        db.commit();
        db.setAutoCommit(true);
    }

    public void rollback() {
        try {
            db.rollback();
            db.setAutoCommit(true);
        } catch (SQLException ignored) {
            // Preserve the original business exception.
        }
    }

    public long tail() throws SQLException {
        return Long.parseLong(scalar("SELECT coalesce(max(sequence), 0) FROM " + table("events")));
    }

    public JSONObject event(JSONObject payload) throws SQLException {
        // Single writer allocates committed contiguous values; sequence defaults can have rollback
        // gaps.
        long sequence = tail() + 1;
        payload.put("sequence", Long.toString(sequence));
        payload.put("eventId", epoch + "-" + sequence);
        update(
                "INSERT INTO " + table("events") + " (sequence, payload) VALUES (?, ?::jsonb)",
                sequence,
                JsonUtils.toJson(payload));
        return payload;
    }

    public JSONArray receipt(String owner, String topic, String id, String hash)
            throws SQLException {
        String existing =
                scalar(
                        "SELECT hash FROM "
                                + table("requests")
                                + " WHERE owner_id=? AND topic=? AND request_id=?",
                        owner,
                        topic,
                        id);
        if (existing != null && !existing.equals(hash)) {
            throw ApiException.forField(
                    "1006", "IDEMPOTENCY_CONFLICT", "requestId has different content", "requestId");
        }
        if (existing == null) {
            update(
                    "INSERT INTO " + table("requests") + " VALUES (?, ?, ?, ?, ?::jsonb)",
                    owner,
                    topic,
                    id,
                    hash,
                    "[]");
            return new JSONArray();
        }
        return JSON.parseArray(
                scalar(
                        "SELECT results FROM "
                                + table("requests")
                                + " WHERE owner_id=? AND topic=? AND request_id=?",
                        owner,
                        topic,
                        id));
    }

    public void receipt(String owner, String topic, String id, JSONArray results)
            throws SQLException {
        update(
                "UPDATE "
                        + table("requests")
                        + " SET results=?::jsonb WHERE owner_id=? AND topic=? AND request_id=?",
                JsonUtils.toJson(results),
                owner,
                topic,
                id);
    }

    @Override
    public void close() throws SQLException {
        db.close();
    }
}
