-- 仅供旧双 schema 数据库升级；全新 QA 环境不需要执行。
-- 先停止 QuoteSvr、NotificationSvr，备份数据库，再执行 001 和本脚本。
-- 只复制旧通知数据；旧 schema 保留作为迁移备份，不删除、不覆盖。
-- 目标通知表必须为空。迁移已完成时重复执行会报错，防止覆盖。

BEGIN;
SET LOCAL lock_timeout = '5s';
SET LOCAL statement_timeout = '30s';

DO $migration$
DECLARE
    source_table text;
    target_table text;
    contains_rows boolean;
BEGIN
    IF NOT pg_try_advisory_xact_lock(hashtext('ficc_salestrading.quotesvr.writer'))
       OR NOT pg_try_advisory_xact_lock(hashtext('ficc_salestrading.notificationsvr.writer'))
       OR NOT pg_try_advisory_xact_lock(hashtext('ficc_notification.quotesvr.writer')) THEN
        RAISE EXCEPTION 'Stop QuoteSvr and NotificationSvr before migrating';
    END IF;

    FOREACH source_table IN ARRAY ARRAY[
        'metadata', 'events', 'requests', 'settings',
        'notifications', 'notification_preferences', 'notification_reads'
    ] LOOP
        target_table := CASE
            WHEN source_table IN ('metadata', 'events', 'requests', 'settings')
                THEN 'notification_' || source_table
            ELSE source_table
        END;
        IF to_regclass(format('ficc_notification.%I', source_table)) IS NULL THEN
            RAISE EXCEPTION 'Missing source table: ficc_notification.%', source_table;
        END IF;
        EXECUTE format('SELECT EXISTS (SELECT 1 FROM ficc_salestrading.%I)', target_table)
            INTO contains_rows;
        IF contains_rows THEN
            RAISE EXCEPTION 'Target ficc_salestrading.% is not empty; migration aborted', target_table;
        END IF;
    END LOOP;
END
$migration$;

INSERT INTO ficc_salestrading.notification_metadata (key, value)
SELECT key, value FROM ficc_notification.metadata;

INSERT INTO ficc_salestrading.notification_events (sequence, payload)
SELECT sequence, payload FROM ficc_notification.events;

INSERT INTO ficc_salestrading.notification_requests (owner_id, topic, request_id, hash, results)
SELECT owner_id, topic, request_id, hash, results FROM ficc_notification.requests;

INSERT INTO ficc_salestrading.notification_settings (id, owner_id, kind, revision, payload)
SELECT id, owner_id, kind, revision, payload FROM ficc_notification.settings;

INSERT INTO ficc_salestrading.notifications (id, user_id, active, payload)
SELECT id, user_id, active, payload FROM ficc_notification.notifications;

INSERT INTO ficc_salestrading.notification_preferences (user_id, payload)
SELECT user_id, payload FROM ficc_notification.notification_preferences;

INSERT INTO ficc_salestrading.notification_reads (id, content_token, read_time)
SELECT id, content_token, read_time FROM ficc_notification.notification_reads;

-- 平台按已提交最大事件号继续分配；同时使序列默认值与数据保持一致。
SELECT setval(
    pg_get_serial_sequence('ficc_salestrading.notification_events', 'sequence'),
    greatest(coalesce(max(sequence), 0), 1),
    coalesce(max(sequence), 0) > 0
)
FROM ficc_salestrading.notification_events;

COMMIT;
