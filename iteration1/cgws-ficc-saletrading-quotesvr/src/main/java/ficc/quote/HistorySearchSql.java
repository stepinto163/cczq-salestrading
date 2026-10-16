package ficc.quote;

/** 历史查询先在数据库限定权限、日期并选取每天最后一版，避免向 JVM 传输全部操作历史。 */
final class HistorySearchSql {
    private HistorySearchSql() {}

    static String latestDaily(boolean allOwners) {
        // 日期沿用历史 API 的 operationTime 日期部分，不替换为报价的交易日期。
        // 相同操作时刻按 revision 决定最后版本；不同报价的展示顺序增加稳定的 ID 次序。
        return "SELECT jsonb_build_object('tradeDate', operation_date, 'quote', snapshot)"
                + " FROM (SELECT DISTINCT ON (operation_date, quote_id)"
                + " operation_date, quote_id, snapshot, first_operation"
                + " FROM (SELECT quote_id, revision, operation_time,"
                + " substring(payload->>'operationTime', 1, 10) AS operation_date,"
                + " payload->'after' AS snapshot,"
                + " min(operation_time) OVER (PARTITION BY quote_id,"
                + " substring(payload->>'operationTime', 1, 10)) AS first_operation"
                + " FROM history WHERE operation_time>=?::date"
                + " AND operation_time<(?::date + INTERVAL '1 day')"
                + " AND jsonb_typeof(payload->'after')='object'"
                + (allOwners ? "" : " AND payload->'after'->>'ownerUserId'=?")
                + ") candidates ORDER BY operation_date, quote_id, operation_time DESC, revision DESC) daily"
                + " ORDER BY first_operation, operation_date, quote_id";
    }
}
