-- 仅用于已有安装的 schema 名称升级；全新数据库直接执行 001。
-- 先停止 QuoteSvr、NotificationSvr 并备份数据库。
-- 保留表、数据、序列、索引及对象权限，不复制或删除业务数据。
-- 完成后把两个服务的 db.schema 都改为 ficc_salestrading。

BEGIN;
SET LOCAL lock_timeout = '5s';
SET LOCAL statement_timeout = '30s';

DO $rename_schema$
BEGIN
    IF NOT pg_try_advisory_xact_lock(hashtext('ficc_demo.quotesvr.writer'))
       OR NOT pg_try_advisory_xact_lock(hashtext('ficc_demo.notificationsvr.writer'))
       OR NOT pg_try_advisory_xact_lock(hashtext('ficc_salestrading.quotesvr.writer'))
       OR NOT pg_try_advisory_xact_lock(hashtext('ficc_salestrading.notificationsvr.writer')) THEN
        RAISE EXCEPTION 'Stop QuoteSvr and NotificationSvr before renaming the schema';
    END IF;

    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'ficc_demo') THEN
        IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'ficc_salestrading') THEN
            RAISE EXCEPTION 'Both schemas exist; inspect the data before migrating. No changes made';
        END IF;
        ALTER SCHEMA ficc_demo RENAME TO ficc_salestrading;
    ELSIF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'ficc_salestrading') THEN
        RAISE NOTICE 'Schema already has the product name';
    ELSE
        RAISE EXCEPTION 'No existing schema to rename; use 001-create-tables.sql for a new database';
    END IF;
END
$rename_schema$;

COMMIT;
