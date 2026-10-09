package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.QuoteRecord;
import ficc.common.JsonUtils;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.stream.Collectors;

/**
 * 当前报价的关系列与协议字段映射。业务字段只存列；payload 保留来源、审计及未知扩展字段。
 * SQL 标识符全部由源码常量生成，不接受请求传入的列名。价格/数量在库内为 numeric，协议仍为字符串。
 */
final class QuoteColumns {
    static final class Column {
        final String name;
        final String field;
        final String type;
        final boolean existing;

        Column(String name, String field, String type, boolean existing) {
            this.name = name;
            this.field = field;
            this.type = type;
            this.existing = existing;
        }

        String readExpression() {
            // JSON 协议中金额、版本和日期是字符串，不能因数据库类型变化而改成 JSON 数字。
            if ("date".equals(type)) {
                return "to_char(" + name + ",'YYYY-MM-DD')";
            }
            return "boolean".equals(type) ? name : name + "::text";
        }

        String legacyExpression() {
            String value = "(payload->>'" + field + "')::" + type;
            if ("boolean".equals(type)) {
                // 早期缺失的旗标按创建报价时的默认 false 回填；显式 null 或非法值不静默修复。
                return "CASE WHEN payload ? '" + field + "' THEN " + value + " ELSE false END";
            }
            return value;
        }
    }

    static final List<Column> ALL = Collections.unmodifiableList(Arrays.asList(
            new Column("id", "quoteId", "text", true),
            new Column("owner_id", "ownerUserId", "text", true),
            new Column("client_id", "clientQuoteId", "text", true),
            new Column("security_id", "securityId", "text", true),
            new Column("status", "status", "text", true),
            new Column("revision", "revision", "bigint", true),
            new Column("side", "side", "text", false),
            new Column("quote_type", "quoteType", "text", false),
            new Column("yield_type", "yieldType", "text", false),
            new Column("price", "price", "numeric", false),
            new Column("quantity", "quantity", "numeric", false),
            new Column("quantity_unit", "quantityUnit", "text", false),
            new Column("currency", "currency", "text", false),
            new Column("trade_date", "tradeDate", "date", false),
            new Column("settl_type", "settlType", "text", false),
            new Column("institution_id", "institutionId", "text", false),
            new Column("institution_name", "institutionName", "text", false),
            new Column("contact_id", "contactId", "text", false),
            new Column("contact_name", "contactName", "text", false),
            new Column("urgent", "urgent", "boolean", false),
            new Column("recommended", "recommended", "boolean", false),
            new Column("whole", "whole", "boolean", false),
            new Column("oco", "oco", "boolean", false),
            new Column("packing", "packing", "boolean", false),
            new Column("remark", "remark", "text", false)));

    private QuoteColumns() {}

    static String readSql() {
        String fields = ALL.stream()
                .map(column -> "'" + column.field + "'," + column.readExpression())
                .collect(Collectors.joining(","));
        // 右侧覆盖同名 JSON 键，包括 SQL NULL，旧 payload 不能反向覆盖列值。
        return "SELECT payload || jsonb_build_object(" + fields
                + ") FROM quotes ORDER BY update_time DESC,id";
    }

    static String writeSql() {
        String names = ALL.stream().map(column -> column.name).collect(Collectors.joining(","));
        String parameters = ALL.stream().map(column -> "?::" + column.type)
                .collect(Collectors.joining(","));
        String assignments = ALL.stream()
                .filter(column -> !Arrays.asList("id", "owner_id", "client_id").contains(column.name))
                .map(column -> column.name + "=excluded." + column.name)
                .collect(Collectors.joining(","));
        return "INSERT INTO quotes (" + names + ",update_time,payload) VALUES ("
                + parameters + ",?::timestamptz,?::jsonb) ON CONFLICT(id) DO UPDATE SET "
                + assignments + ",update_time=excluded.update_time,payload=excluded.payload";
    }

    static Object[] values(QuoteRecord quote) {
        JSONObject metadata = quote.toJson();
        List<Object> values = new ArrayList<>();
        for (Column column : ALL) {
            values.add(metadata.remove(column.field));
        }
        // 审计时间保留原协议文本（含小数精度和时区）；update_time 列仅用于数据库排序索引。
        values.add(quote.updateTime);
        values.add(JsonUtils.toJson(metadata));
        return values.toArray();
    }

    static String metadataExpression() {
        return "payload" + ALL.stream()
                .map(column -> " - '" + column.field + "'")
                .collect(Collectors.joining());
    }
}
