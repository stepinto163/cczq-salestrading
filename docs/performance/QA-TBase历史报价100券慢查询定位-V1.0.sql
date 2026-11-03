-- TBase历史报价100券慢查询定位测试 V1.0（2026-11-03）
-- 专用QA测试库、单连接、单事务；只测100券，不自动升级至1万/10万券或并发。
-- 造数沿用上一轮：100万报价、10万券、28天；每页50，精确计数。
-- A 原查询；C 逐券LATERAL定位（OFFSET 0保留子查询边界）；B 同SQL移除测试表排序索引。
-- A/C交替执行各2次，然后各取1份计划；最后B执行2次及1份计划。
-- 所有状态/权限条件保留，成功结果逐一比较完整JSON；超时不计成功。
-- 只删除本事务新建schema内的两个索引，不接触业务表/索引；结束全部ROLLBACK。
-- 准备语句180秒、诊断查询20秒、锁3秒；建议外部运行上限8分钟，超时关闭连接。
-- psql -X -v ON_ERROR_STOP=1 -f 本文件.sql > tbase-history-100-diagnostic.txt 2>&1
-- 中断/报错后同连接ROLLBACK或关闭连接，严禁COMMIT；不要同时执行多份。
BEGIN;
SET LOCAL application_name = 'ficc_history_100_probe_v1';
SET LOCAL timezone = 'Asia/Shanghai';
SET LOCAL lock_timeout = '3s';
SET LOCAL statement_timeout = '180s';
CREATE SCHEMA ficc_history_100_probe;
SET LOCAL search_path TO ficc_history_100_probe, public;

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


SELECT indexname,indexdef FROM pg_indexes
WHERE schemaname='ficc_history_100_probe' ORDER BY indexname;
SELECT name,setting,unit FROM pg_settings WHERE name IN
('random_page_cost','seq_page_cost','effective_cache_size','default_statistics_target',
 'enable_bitmapscan','enable_indexscan','enable_indexonlyscan','enable_seqscan',
 'work_mem','shared_buffers','max_parallel_workers_per_gather') ORDER BY name;
SELECT attname,n_distinct,null_frac,correlation,most_common_freqs
FROM pg_stats WHERE schemaname='ficc_history_100_probe' AND tablename='quote_history_daily'
AND attname IN ('security_id','operation_date','first_operation','owner_id');

CREATE TABLE probe_results (
    scenario text, iteration integer, status text, query_ms numeric,
    total_count bigint, returned_rows integer, result_fingerprint text, detail text
);
CREATE TABLE probe_reference (scenario text, payload jsonb);
CREATE TABLE probe_plans (scenario text, plan json);

CREATE FUNCTION run_diagnostic(variant text, run_number integer, explain_only boolean DEFAULT false)
RETURNS jsonb LANGUAGE plpgsql SET search_path FROM CURRENT AS $function$
DECLARE
    security_ids text[];
    config probe_config%ROWTYPE;
    selected_bonds integer;
    expected_count bigint;
    result jsonb;
    reference_result jsonb;
    plan json;
    sql_text text;
    started timestamptz;
    elapsed_ms numeric;
BEGIN
    SELECT * INTO config FROM probe_config;
    selected_bonds := least(100, config.bond_count);
    SELECT array_agg('S-' || lpad(number::text,6,'0') || '.IB' ORDER BY number)
    INTO security_ids FROM generate_series(1,selected_bonds) numbers(number);
    expected_count := (config.quote_count / config.bond_count)::bigint * selected_bonds
        + least(config.quote_count % config.bond_count,selected_bonds);
    IF variant IN ('A_原查询','B_无排序索引原查询') THEN
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
    ELSIF variant='C_逐券定位' THEN
        sql_text := $sql$
        WITH filtered AS (
            SELECT first_operation,operation_date,quote_id FROM unnest($4::text[]) requested(security_id)
            CROSS JOIN LATERAL (
                SELECT first_operation,operation_date,quote_id FROM quote_history_daily
                WHERE security_id = requested.security_id AND operation_date >= $1::date AND operation_date <= $2::date
            AND (snapshot->>'status' IN ('ACTIVE','REFERRED') OR (snapshot->>'status'='DELETED'
                 AND NULLIF(btrim(snapshot->>'confirmTime'),'') IS NOT NULL))
            AND (owner_id=$3 OR snapshot->>'status' IN ('ACTIVE','REFERRED')
                 OR (snapshot->>'status'='DELETED'
                     AND NULLIF(btrim(snapshot->>'confirmTime'),'') IS NOT NULL))
                OFFSET 0
            ) matched
        ), page AS (
            SELECT * FROM filtered ORDER BY first_operation,operation_date,quote_id LIMIT $5 OFFSET $6
        )
        SELECT jsonb_build_object('totalCount',(SELECT count(*) FROM filtered),
            'rows',COALESCE(jsonb_agg(jsonb_build_object('tradeDate',page.operation_date,'quote',details.snapshot)
                ORDER BY page.first_operation,page.operation_date,page.quote_id),'[]'::jsonb))
        FROM page JOIN quote_history_daily details ON details.operation_date=page.operation_date
            AND details.quote_id=page.quote_id
    $sql$;
    ELSE
        RAISE EXCEPTION '未知测试方案：%', variant;
    END IF;
    started := clock_timestamp();
    IF explain_only THEN
        EXECUTE 'EXPLAIN (ANALYZE,BUFFERS,FORMAT JSON) ' || sql_text INTO plan
        USING date '2026-10-01', date '2026-10-28', 'U-1'::text, security_ids, 50, 0;
        INSERT INTO probe_plans VALUES(variant,plan);
        RETURN jsonb_build_object('scenario',variant,'status','PLAN','plan',plan);
    END IF;
    EXECUTE sql_text INTO result
    USING date '2026-10-01', date '2026-10-28', 'U-1'::text, security_ids, 50, 0;
    elapsed_ms := extract(epoch FROM clock_timestamp()-started)*1000;
    IF (result->>'totalCount')::bigint IS DISTINCT FROM expected_count
       OR jsonb_array_length(result->'rows') IS DISTINCT FROM least(50::bigint,expected_count)::integer THEN
        RAISE EXCEPTION '计数/行数不符，预期总数%，实际结果%',expected_count,result;
    END IF;
    SELECT payload INTO reference_result FROM probe_reference LIMIT 1;
    IF reference_result IS NULL THEN
        INSERT INTO probe_reference VALUES(variant,result);
    ELSIF result IS DISTINCT FROM reference_result THEN
        RAISE EXCEPTION '与首个成功方案的完整JSON不一致：%',variant;
    END IF;
    INSERT INTO probe_results VALUES(variant,run_number,'OK',elapsed_ms,
        expected_count,jsonb_array_length(result->'rows'),md5(result::text),NULL);
    RETURN jsonb_build_object('scenario',variant,'iteration',run_number,'status','OK',
        'queryMs',round(elapsed_ms,2),'totalCount',expected_count,
        'rows',jsonb_array_length(result->'rows'),'fingerprint',md5(result::text));
EXCEPTION WHEN query_canceled OR OTHERS THEN
    INSERT INTO probe_results VALUES(variant,run_number,
        CASE WHEN SQLSTATE='57014' THEN 'TIMEOUT' ELSE 'ERROR' END,
        NULL,NULL,NULL,NULL,SQLSTATE || ': ' || SQLERRM);
    RETURN jsonb_build_object('scenario',variant,'iteration',run_number,
        'status',CASE WHEN SQLSTATE='57014' THEN 'TIMEOUT' ELSE 'ERROR' END,
        'error',SQLSTATE || ': ' || SQLERRM);
END $function$;

SET LOCAL statement_timeout='20s';
SELECT run_diagnostic('A_原查询',1);
SELECT run_diagnostic('C_逐券定位',1);
SELECT run_diagnostic('A_原查询',2);
SELECT run_diagnostic('C_逐券定位',2);
SELECT run_diagnostic('A_原查询',0,true);
SELECT run_diagnostic('C_逐券定位',0,true);

-- 因果对照：只对本事务测试表删两个排序索引，其余表、数据、统计、SQL不变。
-- 不宣称强制选中了指定索引：实际选择必须以B执行计划为准。
DROP INDEX ficc_history_100_probe.history_daily_formal_order_idx;
DROP INDEX ficc_history_100_probe.history_daily_order_idx;
SELECT run_diagnostic('B_无排序索引原查询',1);
SELECT run_diagnostic('B_无排序索引原查询',2);
SELECT run_diagnostic('B_无排序索引原查询',0,true);

SELECT '明细' AS stage,* FROM probe_results ORDER BY scenario,iteration;
SELECT scenario,count(*) FILTER(WHERE iteration>0) AS attempts,
    count(*) FILTER(WHERE iteration>0 AND status='OK') AS successes,
    round(min(query_ms),2) AS min_ms,round(max(query_ms),2) AS max_ms
FROM probe_results GROUP BY scenario ORDER BY scenario;
SELECT '结果一致性' AS stage,count(*) AS successful_queries,
    count(DISTINCT scenario) AS successful_variants,
    count(DISTINCT result_fingerprint) AS distinct_result_fingerprints
FROM probe_results WHERE status='OK';
SELECT '计划数量' AS stage,count(*) AS completed_plans FROM probe_plans;
SELECT '失败明细' AS stage,* FROM probe_results WHERE status<>'OK';
ROLLBACK;
SELECT '清理检查' AS stage,to_regnamespace('ficc_history_100_probe') AS remaining_schema;
