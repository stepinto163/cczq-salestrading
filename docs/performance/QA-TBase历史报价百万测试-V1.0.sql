-- QA TBase 历史报价查询性能测试 V1.0（2026-11-03）
-- 用法：用psql在QA测试库一次执行整个文件，保存全部输出：
-- psql -X -v ON_ERROR_STOP=1 -h 主机 -p 端口 -U 用户 -d 数据库 -f 本文件.sql > history-perf.txt 2>&1
-- 密码由psql交互输入，不写进脚本。也可用数据库工具的“执行脚本”，必须使用同一连接。
-- 默认100万独立quoteId、10万securityId、单连接串行；不要多开执行，不在生产库执行。
-- 需要CREATE SCHEMA权限和足够测试磁盘空间，建议至少预留3GB；不是业务schema。
-- 建表/造数/索引各语句最多180秒；每次测试（含ID数组构建）最多8秒。
-- 全程单事务，最后ROLLBACK清理。中途报错/取消：在同一连接执行ROLLBACK或关闭该连接。
-- 不使用DROP SCHEMA、不使用IF NOT EXISTS：已有同名schema时直接失败，不碰已有数据。
-- 当前兼容版来源：QuoteLifecycle.formalSnapshotSql、HistorySearchSql.numberedPage、
-- DailyQuoteHistoryMigration版本3。保留全部5个普通/部分索引，不使用INCLUDE。
-- 直接生成每日查询表，不生成审计表；不测迁移回填、触发器、DM匹配、JDBC/网关/界面耗时。
-- securityId数组模拟DM筛选结果，返回50条JSON并精确计数，使用真实兼容版SQL结构。
-- 刚造数后的首次查询不等于磁盘冷缓存；不执行VACUUM，不更改QA work_mem等调优参数。

BEGIN;
SET LOCAL application_name = 'ficc_history_perf_probe_v1';
SET LOCAL timezone = 'Asia/Shanghai';
SET LOCAL lock_timeout = '3s';
SET LOCAL statement_timeout = '180s';
CREATE SCHEMA ficc_history_perf_probe;
SET LOCAL search_path TO ficc_history_perf_probe, public;

CREATE TABLE probe_config (quote_count integer NOT NULL, bond_count integer NOT NULL);
-- 仅此处控制造数规模。首次QA正式测试保持1000000/100000；小样本验证可改1000/100。
INSERT INTO probe_config VALUES (1000000, 100000);
DO $check$
BEGIN
    IF EXISTS (SELECT 1 FROM probe_config WHERE quote_count < 1 OR quote_count > 1000000
               OR bond_count < 1 OR bond_count > 100000 OR bond_count > quote_count) THEN
        RAISE EXCEPTION '测试规模不合法：最多100万报价、10万券，券数不能超过报价数';
    END IF;
END $check$;

SELECT '环境信息' AS stage, version() AS database_version,
       current_setting('work_mem') AS work_mem,
       current_setting('shared_buffers') AS shared_buffers,
       current_setting('max_parallel_workers_per_gather', true) AS parallel_workers,
       current_setting('statement_timeout') AS preparation_timeout,
       quote_count, bond_count FROM probe_config;

CREATE TABLE probe_phases (stage text, measured_at timestamptz);
INSERT INTO probe_phases VALUES ('开始建表造数', clock_timestamp());
CREATE TABLE quote_history_daily (
    operation_date date NOT NULL, quote_id text NOT NULL,
    first_operation timestamptz NOT NULL, last_operation timestamptz NOT NULL,
    revision bigint NOT NULL, event_id text NOT NULL, security_id text, owner_id text,
    snapshot jsonb NOT NULL,
    PRIMARY KEY(operation_date, quote_id),
    CONSTRAINT history_daily_date_consistent CHECK (
        operation_date = (last_operation AT TIME ZONE 'Asia/Shanghai')::date
        AND operation_date = (first_operation AT TIME ZONE 'Asia/Shanghai')::date)
);

INSERT INTO quote_history_daily
SELECT operation_time::date, quote_id, operation_time, operation_time,
       3, 'E-' || quote_id, security_id, owner_id,
       jsonb_build_object(
           'quoteId', quote_id, 'securityId', security_id, 'ownerUserId', owner_id,
           'revision', 3, 'status', CASE WHEN number % 5 = 0 THEN 'REFERRED' ELSE 'ACTIVE' END,
           'confirmTime', to_char(operation_time, 'YYYY-MM-DD"T"HH24:MI:SSOF'),
           'updateTime', to_char(operation_time, 'YYYY-MM-DD"T"HH24:MI:SSOF'),
           'createTime', to_char(operation_time, 'YYYY-MM-DD"T"HH24:MI:SSOF'),
           'side', CASE WHEN number % 2 = 0 THEN '1' ELSE '2' END,
           'price', (2.0 + (number % 1000)::numeric / 1000)::text,
           'quantity', ((number % 20 + 1) * 100)::text,
           'quoteType', 'YIELD', 'yieldType', 'MATURITY', 'settlementType', 'T+1',
           'tradeDate', operation_time::date::text,
           'institutionId', 'I-' || (number % 200)::text,
           'traderId', 'T-' || (number % 1000)::text,
           'source', 'PRECISE', 'urgent', false, 'recommended', false,
           'whole', false, 'oco', false, 'packing', false,
           'originalText', security_id || ' 测试报价 ' || md5(number::text))
FROM (
    SELECT number, 'Q-' || lpad(number::text, 7, '0') AS quote_id,
           'S-' || lpad((((number - 1) % bond_count) + 1)::text, 6, '0') || '.IB' AS security_id,
           'U-' || (number % 20)::text AS owner_id,
           timestamptz '2026-10-01 00:00:00+08' + ((number - 1) % 28) * interval '1 day'
               + ((number * 37) % 86400) * interval '1 second' AS operation_time
    FROM probe_config CROSS JOIN LATERAL generate_series(1, quote_count) AS numbers(number)
) generated;
INSERT INTO probe_phases VALUES ('造数完成', clock_timestamp());

CREATE INDEX history_daily_security_date_idx ON quote_history_daily(security_id, operation_date);
CREATE INDEX history_daily_owner_date_idx ON quote_history_daily(owner_id, operation_date);
CREATE INDEX history_daily_order_idx ON quote_history_daily(first_operation, operation_date, quote_id);
CREATE INDEX history_daily_formal_security_idx
    ON quote_history_daily(security_id, operation_date, first_operation, quote_id)
    WHERE (snapshot->>'status' IN ('ACTIVE','REFERRED') OR (snapshot->>'status'='DELETED'
           AND NULLIF(btrim(snapshot->>'confirmTime'),'') IS NOT NULL));
CREATE INDEX history_daily_formal_order_idx
    ON quote_history_daily(first_operation, operation_date, quote_id, security_id)
    WHERE (snapshot->>'status' IN ('ACTIVE','REFERRED') OR (snapshot->>'status'='DELETED'
           AND NULLIF(btrim(snapshot->>'confirmTime'),'') IS NOT NULL));
ANALYZE quote_history_daily;
INSERT INTO probe_phases VALUES ('索引及统计完成', clock_timestamp());

-- 这些验证扫描不计入查询耗时，也会预热数据；不是冷缓存性能测试。
SELECT '数据验证' AS stage, count(*) AS daily_rows, count(DISTINCT quote_id) AS distinct_quotes,
       count(DISTINCT security_id) AS distinct_bonds,
       round(avg(pg_column_size(snapshot)), 1) AS average_snapshot_bytes,
       pg_total_relation_size('quote_history_daily') AS table_and_index_bytes
FROM quote_history_daily;
SELECT stage, measured_at,
       round((extract(epoch FROM measured_at - lag(measured_at) OVER (ORDER BY measured_at)) * 1000)::numeric, 2)
           AS phase_elapsed_ms FROM probe_phases ORDER BY measured_at;

CREATE TABLE probe_results (
    scenario text, iteration integer, matched_bonds integer, page_offset integer,
    status text, ids_build_ms numeric, query_ms numeric, total_ms numeric,
    total_count bigint, returned_rows integer, detail text
);

CREATE FUNCTION run_probe(scenario_name text, wanted_bonds integer, wanted_offset integer,
                          run_number integer, explain_only boolean DEFAULT false)
RETURNS jsonb LANGUAGE plpgsql SET search_path FROM CURRENT AS $probe$
DECLARE
    started timestamptz := clock_timestamp();
    query_started timestamptz;
    finished timestamptz;
    security_ids text[];
    config probe_config%ROWTYPE;
    selected_bonds integer;
    expected_count bigint;
    expected_rows integer;
    result jsonb;
    plan json;
    sql_text text;
BEGIN
    SELECT * INTO config FROM probe_config;
    selected_bonds := least(wanted_bonds, config.bond_count);
    SELECT array_agg('S-' || lpad(number::text, 6, '0') || '.IB' ORDER BY number)
        INTO security_ids FROM generate_series(1, selected_bonds) AS numbers(number);
    expected_count := (config.quote_count / config.bond_count)::bigint * selected_bonds
        + least(config.quote_count % config.bond_count, selected_bonds);
    expected_rows := least(50::bigint, greatest(0::bigint, expected_count - wanted_offset))::integer;
    -- 与HistorySearchSql.numberedPage(false,false,prefilter)一致；参数依次绑定日期、用户、券数组、分页。
    sql_text := $sql$
        WITH filtered AS (
            SELECT first_operation,operation_date,quote_id FROM quote_history_daily
            WHERE operation_date >= $1::date AND operation_date <= $2::date
            AND (snapshot->>'status' IN ('ACTIVE','REFERRED') OR (snapshot->>'status'='DELETED'
                 AND NULLIF(btrim(snapshot->>'confirmTime'),'') IS NOT NULL))
            AND (owner_id=$3 OR snapshot->>'status' IN ('ACTIVE','REFERRED')
                 OR (snapshot->>'status'='DELETED'
                     AND NULLIF(btrim(snapshot->>'confirmTime'),'') IS NOT NULL))
            AND security_id = ANY($4::text[])
        ), page AS (
            SELECT * FROM filtered ORDER BY first_operation,operation_date,quote_id LIMIT $5 OFFSET $6
        )
        SELECT jsonb_build_object('totalCount',(SELECT count(*) FROM filtered),
            'rows',COALESCE(jsonb_agg(jsonb_build_object('tradeDate',page.operation_date,'quote',details.snapshot)
                ORDER BY page.first_operation,page.operation_date,page.quote_id),'[]'::jsonb))
        FROM page JOIN quote_history_daily details ON details.operation_date=page.operation_date
            AND details.quote_id=page.quote_id
    $sql$;
    query_started := clock_timestamp();
    IF explain_only THEN
        EXECUTE 'EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) ' || sql_text INTO plan
            USING date '2026-10-01', date '2026-10-28', 'U-1'::text, security_ids, 50, wanted_offset;
        -- 仅缩短报告中的长券ID字面量，避免执行计划打印十万项；实际SQL仍使用完整数组。
        RETURN jsonb_build_object('scenario', scenario_name, 'status', 'PLAN', 'plan',
            replace(plan::text, array_to_string(security_ids, ','),
                '测试券ID数组共' || selected_bonds::text || '项')::json);
    END IF;
    EXECUTE sql_text INTO result
        USING date '2026-10-01', date '2026-10-28', 'U-1'::text, security_ids, 50, wanted_offset;
    finished := clock_timestamp();
    IF (result->>'totalCount')::bigint IS DISTINCT FROM expected_count
       OR jsonb_array_length(result->'rows') IS DISTINCT FROM expected_rows THEN
        RAISE EXCEPTION '结果不符：预期总数%/行数%，实际总数%/行数%', expected_count, expected_rows,
            result->>'totalCount', jsonb_array_length(result->'rows');
    END IF;
    INSERT INTO probe_results VALUES (scenario_name, run_number, selected_bonds, wanted_offset, 'OK',
        extract(epoch FROM query_started-started)*1000, extract(epoch FROM finished-query_started)*1000,
        extract(epoch FROM finished-started)*1000, expected_count, expected_rows, NULL);
    RETURN jsonb_build_object('scenario',scenario_name,'iteration',run_number,'status','OK',
        'queryMs',round((extract(epoch FROM finished-query_started)*1000)::numeric,2),
        'matchedBonds',selected_bonds,'totalCount',expected_count,'rows',expected_rows);
EXCEPTION WHEN query_canceled OR OTHERS THEN
    finished := clock_timestamp();
    INSERT INTO probe_results VALUES (scenario_name, run_number, selected_bonds, wanted_offset,
        CASE WHEN SQLSTATE='57014' THEN 'TIMEOUT' ELSE 'ERROR' END, NULL, NULL,
        extract(epoch FROM finished-started)*1000, NULL, NULL, SQLSTATE || ': ' || SQLERRM);
    RETURN jsonb_build_object('scenario',scenario_name,'iteration',run_number,'status',
        CASE WHEN SQLSTATE='57014' THEN 'TIMEOUT' ELSE 'ERROR' END,'error',SQLSTATE || ': ' || SQLERRM);
END $probe$;

SET LOCAL statement_timeout = '8s';
-- 每条SELECT独立执行一次测试，避免用一个DO包住所有查询而共享一个8秒预算。
SELECT run_probe('少量券第一页',100,0,1);
SELECT run_probe('少量券第一页',100,0,2);
SELECT run_probe('少量券第一页',100,0,3);
SELECT run_probe('一万券第一页',10000,0,1);
SELECT run_probe('一万券第一页',10000,0,2);
SELECT run_probe('一万券第一页',10000,0,3);
SELECT run_probe('十万券第一页',100000,0,1);
SELECT run_probe('十万券第一页',100000,0,2);
SELECT run_probe('十万券第一页',100000,0,3);
SELECT run_probe('十万券第1500页',100000,74950,1);
SELECT run_probe('十万券第1500页',100000,74950,2);
SELECT run_probe('十万券第1500页',100000,74950,3);

SELECT '每轮明细' AS stage, * FROM probe_results ORDER BY scenario, iteration;
SELECT scenario, count(*) AS attempts, sum(CASE WHEN status='OK' THEN 1 ELSE 0 END) AS successes,
       round(min(query_ms),2) AS minimum_ms,
       round((percentile_cont(0.5) WITHIN GROUP (ORDER BY query_ms))::numeric,2) AS median_ms,
       round(max(query_ms),2) AS maximum_ms
FROM probe_results GROUP BY scenario ORDER BY scenario;
-- 三次样本不足以给出可靠P95，不输出P95；执行计划另跑一次，不纳入以上统计。
SELECT run_probe('少量券第一页',100,0,0,true);
SELECT run_probe('一万券第一页',10000,0,0,true);
SELECT run_probe('十万券第一页',100000,0,0,true);
SELECT run_probe('十万券第1500页',100000,74950,0,true);
SELECT '最终检查' AS stage, count(*) AS failed_queries_or_plans
FROM probe_results WHERE status <> 'OK';

ROLLBACK;
SELECT '清理检查' AS stage, to_regnamespace('ficc_history_perf_probe') AS remaining_schema;
-- remaining_schema应为null。请回传整个history-perf.txt，包含环境、造数规模、耗时、错误和执行计划。
