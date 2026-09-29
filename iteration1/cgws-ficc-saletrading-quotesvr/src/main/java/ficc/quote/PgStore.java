package ficc.quote;

import com.alibaba.fastjson2.*;

import ficc.common.*;
import ficc.api.QuoteRecord;
import ficc.api.ProtocolCodec;
import java.util.stream.Collectors;

import java.sql.*;
import java.util.*;

/** Quote-owned schema and persistence; shared JDBC/receipt/event utilities live in common. */
final class PgStore extends JdbcStore {
    PgStore(Properties cfg) throws Exception {
        super(cfg);
        exec(
                "CREATE TABLE IF NOT EXISTS quotes (id text primary key,owner_id text not"
                    + " null,client_id text,security_id text,status text not null,revision bigint"
                    + " not null,update_time timestamptz not null,payload jsonb not"
                    + " null,unique(owner_id,client_id))");
        exec(
                "CREATE TABLE IF NOT EXISTS history (event_id text primary key,quote_id text not"
                    + " null,revision bigint not null,operation_time timestamptz not null,payload"
                    + " jsonb not null,unique(quote_id,revision))");
        exec(
                "CREATE TABLE IF NOT EXISTS sources (id text primary key,source_type text not"
                        + " null,external_id text not null,payload jsonb not"
                        + " null,unique(source_type,external_id))");
        exec(
                "CREATE INDEX IF NOT EXISTS quotes_search_idx ON"
                        + " quotes(status,security_id,update_time DESC)");
    }

    List<QuoteRecord> quotes() throws SQLException {
        return rows("SELECT payload FROM quotes ORDER BY update_time DESC,id").stream()
                .map(row -> ProtocolCodec.decode(row, QuoteRecord.class)).collect(Collectors.toList());
    }

    void saveQuote(QuoteRecord q) throws SQLException {
        update(
                "INSERT INTO quotes VALUES (?,?,?,?,?,?,?::timestamptz,?::jsonb) ON CONFLICT(id) DO"
                    + " UPDATE SET"
                    + " security_id=excluded.security_id,status=excluded.status,revision=excluded.revision,update_time=excluded.update_time,payload=excluded.payload",
                q.quoteId,
                q.ownerUserId,
                q.clientQuoteId,
                q.securityId,
                q.status,
                Long.parseLong(q.revision),
                q.updateTime,
                JsonUtils.toJson(q.toJson()));
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
