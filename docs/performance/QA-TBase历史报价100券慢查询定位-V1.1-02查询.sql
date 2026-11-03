-- TBase历史报价100券慢查询定位 V1.1：02重复查询
-- 先执行一次01准备；之后只重复执行本文件，不重新造数/建索引/ANALYZE。
-- 每轮事务只在保留的测试区添加临时测试对象及移除两个排序索引，最后ROLLBACK恢复。
-- 原100万测试报价、5个索引仍保留；严禁将本文件的ROLLBACK改为COMMIT。
-- 测试期间独占本测试表，避免两轮互相影响；不锁业务表。
-- 每条诊断20秒，整轮建议外部4分钟上限；单连接，不并发执行。
BEGIN;
SET LOCAL application_name='ficc_history_100_diagnostic_v11';
SET LOCAL timezone='Asia/Shanghai';
SET LOCAL lock_timeout='3s';
SET LOCAL statement_timeout='20s';
SET LOCAL search_path TO ficc_history_100_fixture, public;
DO $identity$
BEGIN
    IF (SELECT count(*) FROM ficc_history_100_fixture.fixture_manifest) <> 1
       OR NOT EXISTS (
           SELECT 1 FROM ficc_history_100_fixture.fixture_manifest m
           JOIN ficc_history_100_fixture.probe_config c
             ON m.quote_count=c.quote_count AND m.bond_count=c.bond_count
           WHERE m.fixture_key='ficc_history_100_diagnostic' AND m.dataset_version=1
       ) THEN
        RAISE EXCEPTION '测试数据集身份或版本不符，请停止并检查01准备结果';
    END IF;
END $identity$;
LOCK TABLE ficc_history_100_fixture.quote_history_daily IN SHARE ROW EXCLUSIVE MODE NOWAIT;
DO $indexes$
BEGIN
    IF (SELECT count(*) FROM pg_indexes WHERE schemaname='ficc_history_100_fixture'
        AND tablename='quote_history_daily' AND indexname IN (
            'history_daily_security_date_idx','history_daily_owner_date_idx','history_daily_order_idx',
            'history_daily_formal_security_idx','history_daily_formal_order_idx')) <> 5 THEN
        RAISE EXCEPTION '测试数据集的5个原始索引不完整，请停止并检查，不能作为相同基线比较';
    END IF;
END $indexes$;
SELECT '复用数据集' AS stage,version() AS database_version,* FROM fixture_manifest;
SELECT indexname,indexdef FROM pg_indexes
WHERE schemaname='ficc_history_100_fixture' ORDER BY indexname;
SELECT name,setting,unit FROM pg_settings WHERE name IN
('random_page_cost','seq_page_cost','effective_cache_size','default_statistics_target',
 'enable_bitmapscan','enable_indexscan','enable_indexonlyscan','enable_seqscan',
 'work_mem','shared_buffers','max_parallel_workers_per_gather') ORDER BY name;
SELECT attname,n_distinct,null_frac,correlation,most_common_freqs
FROM pg_stats WHERE schemaname='ficc_history_100_fixture' AND tablename='quote_history_daily'
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

-- 因果对照：只对独立测试表暂时移除两个排序索引，其余表、数据、统计、SQL不变。
-- 不宣称强制选中了指定索引：实际选择必须以B执行计划为准。
DROP INDEX ficc_history_100_fixture.history_daily_formal_order_idx;
DROP INDEX ficc_history_100_fixture.history_daily_order_idx;
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
SELECT '测试对象已回滚、原数据集保留' AS stage,
    to_regclass('ficc_history_100_fixture.quote_history_daily') IS NOT NULL AS dataset_retained,
    to_regclass('ficc_history_100_fixture.history_daily_formal_order_idx') IS NOT NULL AS formal_order_restored,
    to_regclass('ficc_history_100_fixture.history_daily_order_idx') IS NOT NULL AS order_restored,
    to_regclass('ficc_history_100_fixture.probe_results') IS NULL AS run_objects_cleaned;
