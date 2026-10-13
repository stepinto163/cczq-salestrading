package ficc.quote;

import com.alibaba.fastjson2.*;

import ficc.common.*;
import ficc.api.QuoteRecord;
import ficc.api.ProtocolCodec;
import java.util.stream.Collectors;

import java.sql.*;
import java.util.*;

/** 当前报价使用类型化列，来源/审计元数据与历史快照保留 JSONB；启动自动迁移旧表。 */
final class PgStore extends JdbcStore {
    private static final String READ_QUOTES = QuoteColumns.readSql();
    private static final String SAVE_QUOTE = QuoteColumns.writeSql();

    PgStore(Properties cfg) throws Exception {
        super(cfg);
        try {
            initializeTables();
            ComponentLog.initialize(cfg, "QUOTE_SCHEMA", "postgresql", "QuoteSchemaMigration",
                    cfg.getProperty("db.schema", "ficc_salestrading") + ".quotes version="
                            + QuoteSchemaMigration.VERSION,
                    () -> {
                        QuoteSchemaMigration.migrate(this);
                        return this;
                    });
        } catch (Exception | LinkageError error) {
            // 子类初始化失败也必须释放父类连接和单写入者锁，使修正数据后的启动可以重试。
            try {
                close();
            } catch (SQLException closeError) {
                error.addSuppressed(closeError);
            }
            throw error;
        }
    }

    private void initializeTables() throws SQLException {
        exec("CREATE TABLE IF NOT EXISTS recommendation_feedback (owner_id text NOT NULL,"
                + " recommendation_id text NOT NULL, feedback text NOT NULL,"
                + " created_at timestamptz NOT NULL DEFAULT now(), PRIMARY KEY(owner_id,recommendation_id))");
        exec(
                "CREATE TABLE IF NOT EXISTS quotes (id text primary key,owner_id text not"
                    + " null,client_id text,security_id text,status text not null,revision bigint"
                    + " not null,update_time timestamptz not null,payload jsonb not"
                    + " null,unique(owner_id,client_id))");
        exec(
                "CREATE TABLE IF NOT EXISTS history (event_id text primary key,quote_id text not"
                    + " null,revision bigint not null,operation_time timestamptz not null,payload"
                    + " jsonb not null,unique(quote_id,revision))");
        exec("CREATE INDEX IF NOT EXISTS history_recommendation_idx ON history"
                + " ((payload->'after'->>'securityId'), operation_time DESC)"
                + " WHERE payload->'after'->>'status'='ACTIVE'");
        exec(
                "CREATE TABLE IF NOT EXISTS sources (id text primary key,source_type text not"
                        + " null,external_id text not null,payload jsonb not"
                        + " null,unique(source_type,external_id))");
        exec(
                "CREATE INDEX IF NOT EXISTS quotes_search_idx ON"
                        + " quotes(status,security_id,update_time DESC)");
    }

    List<QuoteRecord> recommendationEvidence(String security, String start, String end) throws SQLException {
        // 每条正式报价在窗口内只保留最新有效快照；撤销、过期后仍可使用此前正式报价证据。
        return rows("SELECT snapshot FROM (SELECT DISTINCT ON (quote_id) quote_id, payload->'after' AS snapshot"
                + " FROM history WHERE payload->'after'->>'securityId'=?"
                + " AND payload->'after'->>'status'='ACTIVE'"
                + " AND operation_time>=?::date AND operation_time<(?::date + INTERVAL '1 day')"
                + " ORDER BY quote_id, operation_time DESC, revision DESC) evidence", security, start, end)
                .stream().map(row -> ProtocolCodec.decode(row, QuoteRecord.class)).collect(Collectors.toList());
    }

    List<QuoteRecord> quotes() throws SQLException {
        return rows(READ_QUOTES).stream()
                .map(row -> ProtocolCodec.decode(row, QuoteRecord.class)).collect(Collectors.toList());
    }

    void saveQuote(QuoteRecord q) throws SQLException {
        update(SAVE_QUOTE, QuoteColumns.values(q));
    }

    void history(JSONObject history) throws SQLException {
        update(
                "INSERT INTO history VALUES (?,?,?,?::timestamptz,?::jsonb)",
                history.getString("eventId"),
                history.getString("quoteId"),
                history.getLongValue("revision"),
                history.getString("operationTime"),
                JsonUtils.toJson(history));
    }
}
