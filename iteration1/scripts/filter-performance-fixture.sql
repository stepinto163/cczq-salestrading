-- 仅由独立性能测试驱动替换 schema 与数量；不调用业务写入 API，不产生提醒或推送。
BEGIN;
SET LOCAL statement_timeout = '10s';
SET LOCAL work_mem = '1MB';
INSERT INTO PERFORMANCE_SCHEMA.quotes
    (id, owner_id, client_id, security_id, status, revision, update_time, payload)
SELECT
    'PERF-' || lpad(i::text, 6, '0'),
    'U-001',
    'PERF-CLIENT-' || i,
    security_id,
    'ACTIVE',
    1,
    update_time,
    jsonb_build_object(
        'quoteId', 'PERF-' || lpad(i::text, 6, '0'),
        'clientQuoteId', 'PERF-CLIENT-' || i,
        'ownerUserId', 'U-001',
        'securityId', security_id,
        'status', 'ACTIVE',
        'revision', '1',
        'side', ((i / 4) % 2 + 1)::text,
        'settlType', ((i / 8) % 2 + 1)::text,
        'quoteType', '1',
        'yieldType', CASE WHEN i % 4 = 3 THEN '2' ELSE '1' END,
        'price', to_char(2.0 + (i % 500) * 0.0001, 'FM0.0000'),
        'quantity', (1000 + (i % 10) * 100)::text,
        'quantityUnit', 'TEN_THOUSAND',
        'currency', 'CNY',
        'tradeDate', '2026-09-14',
        'institutionId', 'I-001',
        'institutionName', '示例对手机构',
        'contactId', 'C-001',
        'contactName', '示例联系人',
        'urgent', i % 10 = 0,
        'recommended', i % 7 = 0,
        'whole', false,
        'oco', false,
        'packing', false,
        'remark', '固定性能测试数据',
        'createTime', to_char(update_time AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'updateTime', to_char(update_time AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'createdBy', 'U-001',
        'updatedBy', 'U-001'
    )
FROM (
    SELECT i,
        (ARRAY['250001.IB', '250002.IB', '102500001.IB', '102500002.IB'])[i % 4 + 1] AS security_id,
        '2026-09-14 09:00:00+00'::timestamptz + i * interval '1 millisecond' AS update_time
    FROM generate_series(0, QUOTE_COUNT - 1) AS generated(i)
) AS fixtures;
COMMIT;
