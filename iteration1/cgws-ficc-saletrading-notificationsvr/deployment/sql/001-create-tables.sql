-- 销售交易第一轮平台：QA 空库初始化
-- 依据当前 JdbcStore、PgStore、NotificationService 的建表代码。
-- 使用应用数据库账号连接目标数据库后执行；不创建数据库或账号。
-- 不导入本机测试数据，不修改已有表结构。重复执行可跳过已有对象。
-- quotes 的字段回填、校验约束及版本标记由新版 QuoteSvr 启动时自动完成。
-- 其他表仍有按位置写入，必须保持其列顺序。

BEGIN;

-- 已有安装应先改名，避免新建空 schema 后误以为原数据丢失。
DO $existing_installation$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'ficc_demo') THEN
        RAISE EXCEPTION 'Existing installation: run 003-rename-salestrading-schema.sql before initialization';
    END IF;
END
$existing_installation$;

CREATE SCHEMA IF NOT EXISTS ficc_salestrading;

-- QuoteSvr：4 张公共表 + 3 张业务表。

CREATE TABLE IF NOT EXISTS ficc_salestrading.metadata (
    key text PRIMARY KEY,
    value text NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_salestrading.events (
    sequence bigserial PRIMARY KEY,
    payload jsonb NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_salestrading.requests (
    owner_id text,
    topic text,
    request_id text,
    hash text NOT NULL,
    results jsonb NOT NULL,
    PRIMARY KEY (owner_id, topic, request_id)
);

CREATE TABLE IF NOT EXISTS ficc_salestrading.settings (
    id text PRIMARY KEY,
    owner_id text NOT NULL,
    kind text NOT NULL,
    revision bigint NOT NULL,
    payload jsonb NOT NULL
);

COMMENT ON TABLE ficc_salestrading.metadata IS '服务元数据；epoch 由服务首次启动时生成';
COMMENT ON TABLE ficc_salestrading.events IS '已提交的业务事件及服务内序号';
COMMENT ON TABLE ficc_salestrading.requests IS '请求幂等记录与处理结果';
COMMENT ON TABLE ficc_salestrading.settings IS '通用设置存储；具体用途由所属服务定义';

CREATE TABLE IF NOT EXISTS ficc_salestrading.quotes (
    id text PRIMARY KEY,
    owner_id text NOT NULL,
    client_id text,
    security_id text,
    status text NOT NULL,
    revision bigint NOT NULL,
    update_time timestamptz NOT NULL,
    payload jsonb NOT NULL,
    side text,
    quote_type text,
    yield_type text,
    price numeric,
    quantity numeric,
    quantity_unit text,
    currency text,
    trade_date date,
    settl_type text,
    institution_id text,
    institution_name text,
    contact_id text,
    contact_name text,
    urgent boolean NOT NULL DEFAULT false,
    recommended boolean NOT NULL DEFAULT false,
    whole boolean NOT NULL DEFAULT false,
    oco boolean NOT NULL DEFAULT false,
    packing boolean NOT NULL DEFAULT false,
    remark text,
    UNIQUE (owner_id, client_id)
);

CREATE TABLE IF NOT EXISTS ficc_salestrading.history (
    event_id text PRIMARY KEY,
    quote_id text NOT NULL,
    revision bigint NOT NULL,
    operation_time timestamptz NOT NULL,
    payload jsonb NOT NULL,
    UNIQUE (quote_id, revision)
);

CREATE TABLE IF NOT EXISTS ficc_salestrading.sources (
    id text PRIMARY KEY,
    source_type text NOT NULL,
    external_id text NOT NULL,
    payload jsonb NOT NULL,
    UNIQUE (source_type, external_id)
);

CREATE INDEX IF NOT EXISTS quotes_search_idx
    ON ficc_salestrading.quotes (status, security_id, update_time DESC);

COMMENT ON TABLE ficc_salestrading.quotes IS '报价当前状态；删除为业务状态，保留记录';
COMMENT ON COLUMN ficc_salestrading.quotes.payload IS '来源、审计时间及扩展元数据；当前报价业务属性以关系列为准（旧库由新版 QuoteSvr 启动迁移）';
COMMENT ON TABLE ficc_salestrading.history IS '报价连续变更历史；payload 保存动作及变更前后内容';
COMMENT ON TABLE ficc_salestrading.sources IS '外部来源消息及来源标识';
COMMENT ON TABLE ficc_salestrading.settings IS '用户关注债券与筛选分组；成员和条件保存在 payload';

-- NotificationSvr：同一 schema；公共表使用 notification_ 前缀，隔离事件序号与幂等状态。

CREATE TABLE IF NOT EXISTS ficc_salestrading.notification_metadata (
    key text PRIMARY KEY,
    value text NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_salestrading.notification_events (
    sequence bigserial PRIMARY KEY,
    payload jsonb NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_salestrading.notification_requests (
    owner_id text,
    topic text,
    request_id text,
    hash text NOT NULL,
    results jsonb NOT NULL,
    PRIMARY KEY (owner_id, topic, request_id)
);

CREATE TABLE IF NOT EXISTS ficc_salestrading.notification_settings (
    id text PRIMARY KEY,
    owner_id text NOT NULL,
    kind text NOT NULL,
    revision bigint NOT NULL,
    payload jsonb NOT NULL
);

COMMENT ON TABLE ficc_salestrading.notification_metadata IS '服务元数据；epoch 由服务首次启动时生成';
COMMENT ON TABLE ficc_salestrading.notification_events IS '已提交的业务事件及服务内序号';
COMMENT ON TABLE ficc_salestrading.notification_requests IS '请求幂等记录与处理结果';
COMMENT ON TABLE ficc_salestrading.notification_settings IS '通用设置存储；具体用途由所属服务定义';

CREATE TABLE IF NOT EXISTS ficc_salestrading.notifications (
    id text PRIMARY KEY,
    user_id text NOT NULL,
    active boolean NOT NULL,
    payload jsonb NOT NULL
);

CREATE INDEX IF NOT EXISTS notification_user_idx
    ON ficc_salestrading.notifications (user_id, active);

CREATE TABLE IF NOT EXISTS ficc_salestrading.notification_preferences (
    user_id text PRIMARY KEY,
    payload jsonb NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_salestrading.notification_reads (
    id text,
    content_token text,
    read_time text,
    PRIMARY KEY (id, content_token)
);

COMMENT ON TABLE ficc_salestrading.notifications IS '各用户的业务提醒及当前有效状态';
COMMENT ON TABLE ficc_salestrading.notification_preferences IS '用户提醒偏好对象';
COMMENT ON TABLE ficc_salestrading.notification_reads IS '按提醒内容版本保存已读时间';
COMMENT ON COLUMN ficc_salestrading.notification_reads.read_time IS '与当前代码保持一致，使用文本保存时间';

-- metadata 和 notification_metadata 中的 epoch 由对应服务首次启动时写入。
COMMIT;
