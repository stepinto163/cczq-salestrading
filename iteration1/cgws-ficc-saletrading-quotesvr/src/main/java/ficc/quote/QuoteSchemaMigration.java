package ficc.quote;

import ficc.common.JdbcStore;

import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;

/**
 * 在 QuoteSvr 获得单写入者锁后自动执行一次迁移；DDL、回填、移除重复字段和版本标记同事务提交。
 * 转换失败完整回滚并阻止启动，不丢弃原 payload；不修改历史、事件或请求回执中的完整快照。
 */
final class QuoteSchemaMigration {
    static final String VERSION_KEY = "quotes.columns.version";
    static final String VERSION = "1";

    private QuoteSchemaMigration() {}

    static void migrate(JdbcStore store) throws SQLException {
        String installed = store.scalar("SELECT value FROM metadata WHERE key=?", VERSION_KEY);
        if (VERSION.equals(installed)) {
            // 检查列是否仍可读取，避免版本标记存在但表结构缺失时宣称迁移成功。
            store.rows(QuoteColumns.readSql() + " LIMIT 0");
            return;
        }
        if (installed != null) {
            throw new SQLException("Unsupported quote column schema version: " + installed);
        }
        store.begin();
        try {
            List<String> assignments = new ArrayList<>();
            for (QuoteColumns.Column column : QuoteColumns.ALL) {
                if (!column.existing) {
                    store.exec("ALTER TABLE quotes ADD COLUMN IF NOT EXISTS "
                            + column.name + " " + column.type);
                    assignments.add(column.name + "=" + column.legacyExpression());
                }
            }
            store.exec("UPDATE quotes SET " + String.join(",", assignments));
            for (QuoteColumns.Column column : QuoteColumns.ALL) {
                if ("boolean".equals(column.type)) {
                    store.exec("ALTER TABLE quotes ALTER COLUMN " + column.name + " SET NOT NULL");
                    store.exec("ALTER TABLE quotes ALTER COLUMN " + column.name + " SET DEFAULT false");
                }
            }
            // 与业务层已有规则一致；负收益率允许，数量必须为正，OCO 与打包不能同时启用。
            store.exec("ALTER TABLE quotes ADD CONSTRAINT quotes_quantity_positive"
                    + " CHECK (quantity IS NULL OR (quantity > 0"
                    + " AND quantity::text NOT IN ('NaN','Infinity','-Infinity')))");
            store.exec("ALTER TABLE quotes ADD CONSTRAINT quotes_price_finite"
                    + " CHECK (price IS NULL OR price::text NOT IN ('NaN','Infinity','-Infinity'))");
            store.exec("ALTER TABLE quotes ADD CONSTRAINT quotes_oco_packing_exclusive"
                    + " CHECK (NOT (oco AND packing))");
            store.exec("UPDATE quotes SET payload=" + QuoteColumns.metadataExpression());
            store.exec("COMMENT ON COLUMN quotes.payload IS"
                    + " '来源、审计时间及扩展元数据；当前报价业务属性以关系列为准'");
            for (QuoteColumns.Column column : QuoteColumns.ALL) {
                store.exec("COMMENT ON COLUMN quotes." + column.name + " IS '报价协议字段 "
                        + column.field + "；当前值的持久化来源'");
            }
            store.update("INSERT INTO metadata (key,value) VALUES (?,?)", VERSION_KEY, VERSION);
            store.commit();
        } catch (SQLException | RuntimeException error) {
            store.rollback();
            throw error;
        }
    }
}
