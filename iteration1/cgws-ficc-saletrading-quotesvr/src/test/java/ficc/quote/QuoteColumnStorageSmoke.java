package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.ProtocolCodec;
import ficc.api.QuoteInput;
import ficc.api.QuoteRecord;
import ficc.common.JdbcStore;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;

import java.io.BufferedReader;
import java.lang.reflect.Field;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.sql.SQLException;
import java.time.LocalDate;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Properties;
import java.util.Set;
import java.util.UUID;

/** 小样本 PostgreSQL 迁移/持久化检查；使用独立 schema，不连接 DPS 或修改运行服务数据。 */
public final class QuoteColumnStorageSmoke {
    private static int checks;
    private static final String LEGACY_TABLE = "CREATE TABLE quotes (id text primary key,"
            + "owner_id text not null,client_id text,security_id text,status text not null,"
            + "revision bigint not null,update_time timestamptz not null,payload jsonb not null,"
            + "unique(owner_id,client_id))";

    private QuoteColumnStorageSmoke() {}

    private static void check(boolean condition, String description) {
        if (!condition) {
            throw new AssertionError(description);
        }
        checks++;
        System.out.println("PASS " + description);
    }

    private static JSONObject fixture(String id) {
        return JsonUtils.object(
                "quoteId", id, "ownerUserId", "U-STORAGE", "clientQuoteId", "CLIENT-" + id,
                "securityId", "240210.IB", "status", "PENDING_CONFIRM", "revision", "1",
                "side", "1", "quoteType", "1", "yieldType", "1", "price", "-0.1250",
                "quantity", "1000.5000", "quantityUnit", "WAN", "currency", "CNY",
                "tradeDate", "2026-10-09", "settlType", "1", "institutionId", "I-1",
                "institutionName", "测试机构", "contactId", "C-1", "contactName", "测试交易员",
                "urgent", true, "recommended", false, "whole", true, "oco", true,
                "packing", false, "remark", "属性迁移测试",
                "sourceMessageId", "SM-1", "source", JsonUtils.object("type", "QT",
                        "negotiationId", "NEG-1", "sourceUserId", "QT-1", "futureField", "preserved"),
                "createdBy", "U-STORAGE", "updatedBy", "U-STORAGE", "confirmedBy", null,
                "createTime", "2026-10-09T08:01:02.123456789+08:00",
                "updateTime", "2026-10-09T08:01:02.123456789+08:00",
                "quoteTime", null, "confirmTime", null, "futureExtension", JsonUtils.object("value", 42),
                "futureNull", null);
    }

    private static Properties schema(Properties configuration, String suffix) {
        Properties isolated = new Properties();
        isolated.putAll(configuration);
        isolated.setProperty("db.schema", "ficc_columns_" + suffix + "_"
                + UUID.randomUUID().toString().replace("-", ""));
        isolated.setProperty("diagnostics.serviceName", "QuoteSvr");
        return isolated;
    }

    private static void seedLegacy(Properties configuration, JSONObject quote) throws Exception {
        try (JdbcStore store = new JdbcStore(configuration)) {
            store.exec(LEGACY_TABLE);
            store.update("INSERT INTO quotes VALUES (?,?,?,?,?,?,?::timestamptz,?::jsonb)",
                    quote.getString("quoteId"), quote.getString("ownerUserId"),
                    quote.getString("clientQuoteId"), quote.getString("securityId"),
                    quote.getString("status"), quote.getLongValue("revision"),
                    quote.getString("updateTime"), JsonUtils.toJson(quote));
            store.event(JsonUtils.object("quote", quote));
        }
    }

    private static void cleanup(Properties configuration) throws Exception {
        String name = configuration.getProperty("db.schema");
        if (!name.matches("ficc_columns_[a-z]+_[a-f0-9]{32}")) {
            throw new AssertionError("Refusing to clean an unexpected schema");
        }
        try (JdbcStore store = new JdbcStore(configuration)) {
            store.exec("DROP SCHEMA " + name + " CASCADE");
        }
    }

    private static void requireSqlFailure(PgStore store, String sql, String description) throws Exception {
        try {
            store.exec(sql);
            throw new AssertionError("Expected SQL failure: " + description);
        } catch (SQLException expected) {
            check(true, description);
        }
    }

    private static void migrationAndRestart(Properties configuration) throws Exception {
        Properties isolated = schema(configuration, "legacy");
        JSONObject original = fixture("Q-OLD");
        seedLegacy(isolated, original);
        try {
            try (PgStore store = new PgStore(isolated)) {
                check("1".equals(store.scalar("SELECT value FROM metadata WHERE key=?",
                        QuoteSchemaMigration.VERSION_KEY)), "legacy migration is versioned");
                check(original.equals(store.quotes().get(0).toJson()),
                        "all fields, decimal scale, timestamp precision and extensions survive migration");
                check(original.equals(store.rows("SELECT payload FROM events").get(0).getJSONObject("quote")),
                        "historical event snapshots stay complete and unchanged");
                JSONObject metadata = store.rows("SELECT payload FROM quotes").get(0);
                for (QuoteColumns.Column column : QuoteColumns.ALL) {
                    check(!metadata.containsKey(column.field), "metadata excludes " + column.field);
                }
                check("numeric".equals(store.scalar("SELECT pg_typeof(price)::text FROM quotes"))
                                && "boolean".equals(store.scalar("SELECT pg_typeof(oco)::text FROM quotes"))
                                && "date".equals(store.scalar("SELECT pg_typeof(trade_date)::text FROM quotes")),
                        "price, flags and trade date have native SQL types");
                store.exec("SET DateStyle TO 'ISO, DMY'");
                check("2026-10-09".equals(store.quotes().get(0).tradeDate),
                        "protocol date format stays ISO with day-first date ordering");
                store.exec("SET DateStyle TO 'ISO, MDY'");
                store.exec("UPDATE quotes SET oco=false,packing=true,price=3.2500,contact_id=NULL,"
                        + "payload=payload || '{\"oco\":true,\"price\":\"9\",\"contactId\":\"STALE\"}'::jsonb");
                QuoteRecord quote = store.quotes().get(0);
                check(!quote.oco && quote.packing && "3.2500".equals(quote.price) && quote.contactId == null,
                        "columns override stale JSON including SQL NULL");
                quote.recommended = true;
                quote.whole = false;
                quote.urgent = false;
                quote.remark = null;
                quote.clearField("remark");
                quote.revision = "2";
                store.saveQuote(quote);
                check(!store.rows("SELECT payload FROM quotes").get(0).containsKey("price"),
                        "saving removes stale business keys from metadata");
                requireSqlFailure(store, "UPDATE quotes SET oco=true", "database enforces OCO/packing exclusion");
                requireSqlFailure(store, "UPDATE quotes SET urgent=NULL", "database enforces non-null flags");
                requireSqlFailure(store, "UPDATE quotes SET quantity=0", "database rejects zero quantity");
                requireSqlFailure(store, "UPDATE quotes SET price='NaN'::numeric", "database rejects non-finite price");
                store.begin();
                quote.price = "4.5";
                store.saveQuote(quote);
                store.rollback();
                check("3.2500".equals(store.quotes().get(0).price), "quote transaction rollback restores columns");
            }
            try (PgStore store = new PgStore(isolated)) {
                QuoteRecord quote = store.quotes().get(0);
                check(!quote.oco && quote.packing && !quote.urgent && quote.recommended && !quote.whole,
                        "all five edited flags survive restart");
                check("3.2500".equals(quote.price) && quote.contactId == null && quote.remark == null,
                        "repeat startup never backfills stale original values");
                check(quote.toJson().containsKey("futureNull") && quote.toJson().get("futureNull") == null,
                        "explicit null extension survives save and restart");
            }
        } finally {
            cleanup(isolated);
        }
    }

    private static void invalidMigration(Properties configuration, String field, Object invalid) throws Exception {
        Properties isolated = schema(configuration, "invalid");
        JSONObject original = fixture("Q-BAD");
        original.put(field, invalid);
        seedLegacy(isolated, original);
        try {
            try (PgStore unexpected = new PgStore(isolated)) {
                throw new AssertionError("Migration should fail for " + field);
            } catch (SQLException expected) {
                check(true, "invalid legacy " + field + " prevents startup");
            }
            // 能重新获取相同 writer 锁，证明失败的 PgStore 构造已释放连接。
            try (JdbcStore store = new JdbcStore(isolated)) {
                check(store.scalar("SELECT value FROM metadata WHERE key=?", QuoteSchemaMigration.VERSION_KEY) == null,
                        "failed migration does not commit version for " + field);
                check("0".equals(store.scalar("SELECT count(*) FROM information_schema.columns"
                        + " WHERE table_schema=current_schema() AND table_name='quotes' AND column_name='oco'")),
                        "failed migration rolls back DDL for " + field);
                check(original.equals(store.rows("SELECT payload FROM quotes").get(0)),
                        "failed migration preserves original payload for " + field);
                store.update("UPDATE quotes SET payload=?::jsonb", JsonUtils.toJson(fixture("Q-BAD")));
            }
            try (PgStore store = new PgStore(isolated)) {
                check(store.quotes().get(0).oco, "corrected data can migrate after failure for " + field);
            }
        } finally {
            cleanup(isolated);
        }
    }

    private static void newSchema(Properties configuration) throws Exception {
        Properties isolated = schema(configuration, "new");
        try {
            try (PgStore store = new PgStore(isolated)) {
                JSONObject draft = fixture("Q-DRAFT");
                for (String field : Arrays.asList("price", "quantity", "tradeDate", "side", "settlType")) {
                    draft.put(field, null);
                }
                store.saveQuote(ProtocolCodec.decode(draft, QuoteRecord.class));
                check(draft.equals(store.quotes().get(0).toJson()), "incomplete pending draft preserves null fields");
                store.exec("UPDATE metadata SET value='999' WHERE key='quotes.columns.version'");
            }
            try (PgStore unexpected = new PgStore(isolated)) {
                throw new AssertionError("Unknown schema version must fail");
            } catch (SQLException expected) {
                check(true, "unsupported future schema version prevents startup");
            }
        } finally {
            cleanup(isolated);
        }
    }

    private static void missingLegacyFlags(Properties configuration) throws Exception {
        Properties isolated = schema(configuration, "missing");
        JSONObject original = fixture("Q-MISSING");
        for (String field : QuoteService.FLAGS) {
            original.remove(field);
        }
        seedLegacy(isolated, original);
        try {
            try (PgStore store = new PgStore(isolated)) {
                QuoteRecord quote = store.quotes().get(0);
                check(!quote.oco && !quote.packing && !quote.urgent && !quote.recommended && !quote.whole,
                        "absent legacy flags receive the existing create default false");
            }
        } finally {
            cleanup(isolated);
        }
    }

    private static JSONObject call(QuoteService service, String topic, JSONObject request) {
        request.put("requestId", UUID.randomUUID().toString());
        JSONObject reply = service.handle("U-001", "column-storage-test", topic, request);
        check("0000".equals(reply.getString("Code")), topic + " envelope succeeds");
        JSONObject result = reply.getJSONObject("Data").getJSONArray("results").getJSONObject(0);
        check("0000".equals(result.getString("Code")), topic + " row succeeds");
        return result;
    }

    private static void businessLifecycle(Properties configuration) throws Exception {
        Properties isolated = schema(configuration, "lifecycle");
        String id;
        try {
            try (PgStore store = new PgStore(isolated)) {
                QuoteService service = new QuoteService(store, new ReferenceData());
                JSONObject input = JsonUtils.object("clientQuoteId", "ATTRIBUTES", "securityId", "250001.IB",
                        "side", "1", "quoteType", "1", "yieldType", "1", "price", "2.5000",
                        "quantity", "1000", "tradeDate", LocalDate.now().toString(), "settlType", "1",
                        "oco", true, "packing", false, "urgent", true, "recommended", false, "whole", true);
                JSONObject created = call(service, "ficc.quote.create", JsonUtils.object("quotes", JsonUtils.array(input)));
                id = created.getString("quoteId");
                check("PENDING_CONFIRM".equals(store.quotes().get(0).status), "create persists a pending quote");
                JSONObject confirmed = call(service, "ficc.quote.confirm", JsonUtils.object("quotes", JsonUtils.array(
                        JsonUtils.object("quoteId", id, "expectedRevision", created.getString("revision")))));
                call(service, "ficc.quote.update", JsonUtils.object("quotes", JsonUtils.array(
                        JsonUtils.object("quoteId", id, "expectedRevision", confirmed.getString("revision"),
                                "oco", false, "packing", true, "urgent", false, "recommended", true, "whole", false))));
                QuoteRecord persisted = store.quotes().get(0);
                check("ACTIVE".equals(persisted.status) && !persisted.oco && persisted.packing
                                && !persisted.urgent && persisted.recommended && !persisted.whole,
                        "real create/confirm/update writes all flags to columns");
                JSONObject history = store.rows("SELECT payload FROM history ORDER BY revision DESC LIMIT 1").get(0);
                check(history.getJSONObject("before").getBooleanValue("oco")
                                && history.getJSONObject("after").getBooleanValue("packing"),
                        "history retains complete before/after flag snapshots");
                check(!store.rows("SELECT payload FROM quotes").get(0).containsKey("oco"),
                        "business write keeps quote metadata free of editable attributes");
            }
            try (PgStore store = new PgStore(isolated)) {
                QuoteService restarted = new QuoteService(store, new ReferenceData());
                QuoteRecord quote = restarted.quotes.get(id);
                check(quote.packing && !quote.oco && "ACTIVE".equals(quote.status),
                        "service recreation loads confirmed quote attributes from columns");
            }
        } finally {
            cleanup(isolated);
        }
    }

    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(args[0]))) {
            configuration.load(reader);
        }
        Set<String> mapped = new HashSet<>();
        for (QuoteColumns.Column column : QuoteColumns.ALL) {
            mapped.add(column.field);
        }
        for (Field field : QuoteInput.class.getFields()) {
            check(mapped.contains(field.getName()), "editable field has a column: " + field.getName());
        }
        migrationAndRestart(configuration);
        newSchema(configuration);
        missingLegacyFlags(configuration);
        businessLifecycle(configuration);
        invalidMigration(configuration, "price", "not-a-number");
        invalidMigration(configuration, "urgent", null);
        invalidMigration(configuration, "packing", true);
        invalidMigration(configuration, "quantity", "-1");
        invalidMigration(configuration, "tradeDate", "not-a-date");
        Files.write(Paths.get(args[1]), JsonUtils.toJson(JsonUtils.object(
                "result", "PASS", "checks", checks, "transportTested", false,
                "database", "isolated PostgreSQL schemas", "productionDataModified", false))
                .getBytes(StandardCharsets.UTF_8));
        System.out.println("Quote column storage checks passed: " + checks);
    }
}
