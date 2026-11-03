-- TBase历史报价单索引移除对照 V1.2（2026-11-03）
-- 直接复用V1.1保留的数据，不重新造数/建索引/ANALYZE；只在QA测试库运行。
-- A保留全部5索引，B仅临时移除formal_order；普通order索引始终保留。
-- 单连接串行；每条20秒，整轮外部12分钟上限。末尾ROLLBACK恢复索引并保留数据。
-- 每场景两次计时；有成功查询才追加执行计划，超时不反复追加计划。
-- 没有任何业务表或服务配置改动，严禁将末尾ROLLBACK改为COMMIT。
BEGIN;
SET LOCAL application_name='ficc_history_index_compare_v12';
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
    phase text, scenario text, iteration integer, status text,
    query_ms numeric, total_count bigint, returned_rows integer,
    result_fingerprint text, detail text
);
CREATE TABLE probe_reference(scenario text PRIMARY KEY,payload jsonb);
CREATE TABLE probe_plans(phase text,scenario text,plan json);
CREATE FUNCTION run_index_compare(phase_name text, scenario_name text,
    wanted_bonds integer,wanted_offset integer,run_number integer,explain_only boolean DEFAULT false)
RETURNS jsonb LANGUAGE plpgsql SET search_path FROM CURRENT AS $function$
DECLARE
    security_ids text[];
    config probe_config%ROWTYPE;
    selected_bonds integer;
    expected_count bigint;
    expected_rows integer;
    result jsonb;
    reference_result jsonb;
    plan json;
    sql_text text;
    started timestamptz;
    elapsed_ms numeric;
BEGIN
    IF explain_only AND NOT EXISTS(SELECT 1 FROM probe_results
        WHERE phase=phase_name AND scenario=scenario_name AND iteration>0 AND status='OK') THEN
        RETURN jsonb_build_object('phase',phase_name,'scenario',scenario_name,
            'status','SKIPPED_PLAN','reason','本方案本场景无成功查询，不再重复执行耗时计划');
    END IF;
    SELECT * INTO config FROM probe_config;
    IF wanted_bonds IS NULL THEN
        expected_count := config.quote_count;
    ELSE
        selected_bonds := least(wanted_bonds,config.bond_count);
        SELECT array_agg('S-' || lpad(number::text,6,'0') || '.IB' ORDER BY number)
        INTO security_ids FROM generate_series(1,selected_bonds) numbers(number);
        expected_count := (config.quote_count / config.bond_count)::bigint * selected_bonds
            + least(config.quote_count % config.bond_count,selected_bonds);
    END IF;
    expected_rows := least(50::bigint,greatest(0::bigint,expected_count-wanted_offset))::integer;
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
    IF wanted_bonds IS NULL THEN
        -- 无券筛选单独去掉券条件，不使用OR或空数组替代，模拟真实无券过滤查询。
        sql_text := replace(sql_text,'AND security_id = ANY($4::text[])','');
    END IF;
    started := clock_timestamp();
    IF explain_only THEN
        EXECUTE 'EXPLAIN (ANALYZE,BUFFERS,FORMAT JSON) ' || sql_text INTO plan
        USING date '2026-10-01',date '2026-10-28','U-1'::text,security_ids,50,wanted_offset;
        -- 仅缩短日志的十万券ID字面量，实际执行仍使用全部ID。
        IF selected_bonds > 100 THEN
            plan := replace(plan::text,array_to_string(security_ids,','),
                '测试券ID数组共' || selected_bonds::text || '项')::json;
        END IF;
        INSERT INTO probe_plans VALUES(phase_name,scenario_name,plan);
        RETURN jsonb_build_object('phase',phase_name,'scenario',scenario_name,'status','PLAN','plan',plan);
    END IF;
    EXECUTE sql_text INTO result
    USING date '2026-10-01',date '2026-10-28','U-1'::text,security_ids,50,wanted_offset;
    elapsed_ms := extract(epoch FROM clock_timestamp()-started)*1000;
    IF (result->>'totalCount')::bigint IS DISTINCT FROM expected_count
       OR jsonb_array_length(result->'rows') IS DISTINCT FROM expected_rows THEN
        RAISE EXCEPTION '计数/行数错误，场景%，预期总数%/行数%，实际%/%',scenario_name,
            expected_count,expected_rows,result->>'totalCount',jsonb_array_length(result->'rows');
    END IF;
    SELECT payload INTO reference_result FROM probe_reference WHERE scenario=scenario_name;
    IF reference_result IS NULL THEN
        INSERT INTO probe_reference VALUES(scenario_name,result);
    ELSIF result IS DISTINCT FROM reference_result THEN
        RAISE EXCEPTION '场景%的完整JSON与此前成功结果不一致',scenario_name;
    END IF;
    INSERT INTO probe_results VALUES(phase_name,scenario_name,run_number,'OK',elapsed_ms,
        expected_count,expected_rows,md5(result::text),NULL);
    RETURN jsonb_build_object('phase',phase_name,'scenario',scenario_name,'iteration',run_number,
        'status','OK','queryMs',round(elapsed_ms,2),'totalCount',expected_count,
        'rows',expected_rows,'fingerprint',md5(result::text));
EXCEPTION WHEN query_canceled OR OTHERS THEN
    INSERT INTO probe_results VALUES(phase_name,scenario_name,run_number,
        CASE WHEN SQLSTATE='57014' THEN 'TIMEOUT' ELSE 'ERROR' END,
        NULL,NULL,NULL,NULL,SQLSTATE || ': ' || SQLERRM);
    RETURN jsonb_build_object('phase',phase_name,'scenario',scenario_name,'iteration',run_number,
        'status',CASE WHEN SQLSTATE='57014' THEN 'TIMEOUT' ELSE 'ERROR' END,
        'error',SQLSTATE || ': ' || SQLERRM);
END $function$;
SET LOCAL statement_timeout='20s';
SELECT run_index_compare('A_完整索引','100券首页',100,0,1,false);
SELECT run_index_compare('A_完整索引','100券首页',100,0,2,false);
SELECT run_index_compare('A_完整索引','100券首页',100,0,0,true);
SELECT run_index_compare('A_完整索引','1万券首页',10000,0,1,false);
SELECT run_index_compare('A_完整索引','1万券首页',10000,0,2,false);
SELECT run_index_compare('A_完整索引','1万券首页',10000,0,0,true);
SELECT run_index_compare('A_完整索引','10万券首页',100000,0,1,false);
SELECT run_index_compare('A_完整索引','10万券首页',100000,0,2,false);
SELECT run_index_compare('A_完整索引','10万券首页',100000,0,0,true);
SELECT run_index_compare('A_完整索引','无券筛选首页',NULL,0,1,false);
SELECT run_index_compare('A_完整索引','无券筛选首页',NULL,0,2,false);
SELECT run_index_compare('A_完整索引','无券筛选首页',NULL,0,0,true);
SELECT run_index_compare('A_完整索引','10万券第1500页',100000,74950,1,false);
SELECT run_index_compare('A_完整索引','10万券第1500页',100000,74950,2,false);
SELECT run_index_compare('A_完整索引','10万券第1500页',100000,74950,0,true);

-- 只动这一条测试索引，保留history_daily_order_idx和其余索引。
DROP INDEX ficc_history_100_fixture.history_daily_formal_order_idx;
SELECT 'B阶段索引核对' AS stage,indexname,indexdef FROM pg_indexes
WHERE schemaname='ficc_history_100_fixture' AND tablename='quote_history_daily' ORDER BY indexname;
SELECT run_index_compare('B_仅移除formal_order','100券首页',100,0,1,false);
SELECT run_index_compare('B_仅移除formal_order','100券首页',100,0,2,false);
SELECT run_index_compare('B_仅移除formal_order','100券首页',100,0,0,true);
SELECT run_index_compare('B_仅移除formal_order','1万券首页',10000,0,1,false);
SELECT run_index_compare('B_仅移除formal_order','1万券首页',10000,0,2,false);
SELECT run_index_compare('B_仅移除formal_order','1万券首页',10000,0,0,true);
SELECT run_index_compare('B_仅移除formal_order','10万券首页',100000,0,1,false);
SELECT run_index_compare('B_仅移除formal_order','10万券首页',100000,0,2,false);
SELECT run_index_compare('B_仅移除formal_order','10万券首页',100000,0,0,true);
SELECT run_index_compare('B_仅移除formal_order','无券筛选首页',NULL,0,1,false);
SELECT run_index_compare('B_仅移除formal_order','无券筛选首页',NULL,0,2,false);
SELECT run_index_compare('B_仅移除formal_order','无券筛选首页',NULL,0,0,true);
SELECT run_index_compare('B_仅移除formal_order','10万券第1500页',100000,74950,1,false);
SELECT run_index_compare('B_仅移除formal_order','10万券第1500页',100000,74950,2,false);
SELECT run_index_compare('B_仅移除formal_order','10万券第1500页',100000,74950,0,true);

SELECT '明细' AS stage,* FROM probe_results ORDER BY scenario,phase,iteration;
SELECT phase,scenario,count(*) FILTER(WHERE iteration>0) AS attempts,
    count(*) FILTER(WHERE iteration>0 AND status='OK') AS successes,
    round(min(query_ms),2) AS min_ms,round(max(query_ms),2) AS max_ms
FROM probe_results GROUP BY phase,scenario ORDER BY scenario,phase;
-- 两个成功phase且指纹数为1才证明本场景跨方案一致；一方超时不能判为通过。
SELECT scenario,count(*) AS successful_queries,count(DISTINCT phase) AS successful_phases,
    count(DISTINCT result_fingerprint) AS distinct_fingerprints
FROM probe_results WHERE status='OK' GROUP BY scenario ORDER BY scenario;
SELECT phase,scenario FROM probe_plans ORDER BY phase,scenario;
SELECT '失败明细' AS stage,* FROM probe_results WHERE status<>'OK';
ROLLBACK;
SELECT '数据和原索引保留' AS stage,
    to_regclass('ficc_history_100_fixture.quote_history_daily') IS NOT NULL AS dataset_retained,
    to_regclass('ficc_history_100_fixture.history_daily_formal_order_idx') IS NOT NULL AS formal_order_restored,
    to_regclass('ficc_history_100_fixture.history_daily_order_idx') IS NOT NULL AS order_retained,
    to_regclass('ficc_history_100_fixture.probe_results') IS NULL AS run_objects_cleaned;
