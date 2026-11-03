-- TBase历史报价100券慢查询定位 V1.1：01一次性准备
-- 只在获准的QA测试库执行；单连接，建议外部8分钟上限。
-- 本文件COMMIT并保留100万测试报价/10万券及5个索引；不是执行后删除。
-- 仅新建ficc_history_100_fixture，不读取/修改业务表；不使用IF NOT EXISTS覆盖未知同名对象。
-- 已准备成功时不要再执行此文件，直接执行02查询文件。准备失败需ROLLBACK/关闭连接。
-- 密码不要写入脚本；本机功能验证可将唯一规模行改为1000/1000。
BEGIN;
SET LOCAL application_name = 'ficc_history_100_fixture_v1';
SET LOCAL timezone = 'Asia/Shanghai';
SET LOCAL lock_timeout = '3s';
SET LOCAL statement_timeout = '180s';
CREATE SCHEMA ficc_history_100_fixture;
SET LOCAL search_path TO ficc_history_100_fixture, public;

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



-- 只登记本脚本创建并验证过的数据集，供后续查询核对身份；不自动复用未知同名表。
CREATE TABLE fixture_manifest (
    fixture_key text PRIMARY KEY, dataset_version integer NOT NULL,
    quote_count integer NOT NULL, bond_count integer NOT NULL,
    created_at timestamptz NOT NULL DEFAULT clock_timestamp()
);
DO $verify$
BEGIN
    IF (SELECT count(*) FROM quote_history_daily) <> (SELECT quote_count FROM probe_config)
       OR (SELECT count(DISTINCT quote_id) FROM quote_history_daily) <> (SELECT quote_count FROM probe_config)
       OR (SELECT count(DISTINCT security_id) FROM quote_history_daily) <> (SELECT bond_count FROM probe_config) THEN
        RAISE EXCEPTION '测试数据数量不符，不提交数据集';
    END IF;
END $verify$;
INSERT INTO fixture_manifest(fixture_key,dataset_version,quote_count,bond_count)
SELECT 'ficc_history_100_diagnostic',1,quote_count,bond_count FROM probe_config;
COMMIT;
SELECT '数据集已保留，后续直接执行02文件' AS stage,* FROM ficc_history_100_fixture.fixture_manifest;
