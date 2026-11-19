-- 只读验收；将下面schema改成QA实际业务schema。
SET search_path TO ficc_salestrading3, public;
BEGIN READ ONLY;
SET LOCAL statement_timeout='30s';
SELECT version();
SELECT value FROM metadata WHERE key='history.daily.version'; -- 6
SELECT pg_get_constraintdef(oid) FROM pg_constraint
 WHERE conrelid='quote_history_daily'::regclass AND contype='p'; -- PRIMARY KEY (quote_id)
SELECT count(*) AS final_rows,count(DISTINCT quote_id) AS unique_quotes FROM quote_history_daily; -- 相等
SELECT count(*) AS audit_rows FROM history; -- 对照迁移前，不应减少
SELECT indexname,indexdef FROM pg_indexes WHERE tablename='quote_history_daily' AND schemaname=current_schema();
SELECT tgname,tgenabled FROM pg_trigger WHERE tgrelid='history'::regclass AND tgname='history_daily_insert';
SELECT quote_id,revision,quote_time,operation_date,snapshot->>'status' AS status
 FROM quote_history_daily WHERE quote_time>='2026-11-16 00:00:00+08'::timestamptz
 AND quote_time<'2026-11-17 00:00:00+08'::timestamptz
 ORDER BY quote_time DESC NULLS LAST,quote_id LIMIT 50;
ROLLBACK;
