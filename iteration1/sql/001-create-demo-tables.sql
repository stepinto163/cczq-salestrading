-- 销售交易第一轮原型：QA 空库初始化
-- 依据当前 JdbcStore、PgStore、NotificationService 的建表代码。
-- 使用应用数据库账号连接目标数据库后执行；不创建数据库或账号。
-- 不导入本机测试数据，不修改已有表结构。重复执行可跳过已有对象。
-- 部分业务写入使用 INSERT INTO ... VALUES，必须保持下列列顺序。

BEGIN;

CREATE SCHEMA IF NOT EXISTS ficc_demo;

-- QuoteSvr：4 张公共表 + 3 张业务表。

CREATE TABLE IF NOT EXISTS ficc_demo.metadata (
    key text PRIMARY KEY,
    value text NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_demo.events (
    sequence bigserial PRIMARY KEY,
    payload jsonb NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_demo.requests (
    owner_id text,
    topic text,
    request_id text,
    hash text NOT NULL,
    results jsonb NOT NULL,
    PRIMARY KEY (owner_id, topic, request_id)
);

CREATE TABLE IF NOT EXISTS ficc_demo.settings (
    id text PRIMARY KEY,
    owner_id text NOT NULL,
    kind text NOT NULL,
    revision bigint NOT NULL,
    payload jsonb NOT NULL
);

COMMENT ON TABLE ficc_demo.metadata IS '服务元数据；epoch 由服务首次启动时生成';
COMMENT ON TABLE ficc_demo.events IS '已提交的业务事件及服务内序号';
COMMENT ON TABLE ficc_demo.requests IS '请求幂等记录与处理结果';
COMMENT ON TABLE ficc_demo.settings IS '通用设置存储；具体用途由所属服务定义';

CREATE TABLE IF NOT EXISTS ficc_demo.quotes (
    id text PRIMARY KEY,
    owner_id text NOT NULL,
    client_id text,
    security_id text,
    status text NOT NULL,
    revision bigint NOT NULL,
    update_time timestamptz NOT NULL,
    payload jsonb NOT NULL,
    UNIQUE (owner_id, client_id)
);

CREATE TABLE IF NOT EXISTS ficc_demo.history (
    event_id text PRIMARY KEY,
    quote_id text NOT NULL,
    revision bigint NOT NULL,
    operation_time timestamptz NOT NULL,
    payload jsonb NOT NULL,
    UNIQUE (quote_id, revision)
);

CREATE TABLE IF NOT EXISTS ficc_demo.sources (
    id text PRIMARY KEY,
    source_type text NOT NULL,
    external_id text NOT NULL,
    payload jsonb NOT NULL,
    UNIQUE (source_type, external_id)
);

CREATE INDEX IF NOT EXISTS quotes_search_idx
    ON ficc_demo.quotes (status, security_id, update_time DESC);

COMMENT ON TABLE ficc_demo.quotes IS '报价当前状态；删除为业务状态，保留记录';
COMMENT ON COLUMN ficc_demo.quotes.payload IS '完整报价 JSON，包含 institutionId、institutionName、contactId、contactName 等业务字段';
COMMENT ON TABLE ficc_demo.history IS '报价连续变更历史；payload 保存动作及变更前后内容';
COMMENT ON TABLE ficc_demo.sources IS '外部来源消息及来源标识';
COMMENT ON TABLE ficc_demo.settings IS '用户关注债券与筛选分组；成员和条件保存在 payload';

-- NotificationSvr：同一 schema；公共表使用 notification_ 前缀，隔离事件序号与幂等状态。

CREATE TABLE IF NOT EXISTS ficc_demo.notification_metadata (
    key text PRIMARY KEY,
    value text NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_demo.notification_events (
    sequence bigserial PRIMARY KEY,
    payload jsonb NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_demo.notification_requests (
    owner_id text,
    topic text,
    request_id text,
    hash text NOT NULL,
    results jsonb NOT NULL,
    PRIMARY KEY (owner_id, topic, request_id)
);

CREATE TABLE IF NOT EXISTS ficc_demo.notification_settings (
    id text PRIMARY KEY,
    owner_id text NOT NULL,
    kind text NOT NULL,
    revision bigint NOT NULL,
    payload jsonb NOT NULL
);

COMMENT ON TABLE ficc_demo.notification_metadata IS '服务元数据；epoch 由服务首次启动时生成';
COMMENT ON TABLE ficc_demo.notification_events IS '已提交的业务事件及服务内序号';
COMMENT ON TABLE ficc_demo.notification_requests IS '请求幂等记录与处理结果';
COMMENT ON TABLE ficc_demo.notification_settings IS '通用设置存储；具体用途由所属服务定义';

CREATE TABLE IF NOT EXISTS ficc_demo.notifications (
    id text PRIMARY KEY,
    user_id text NOT NULL,
    active boolean NOT NULL,
    payload jsonb NOT NULL
);

CREATE INDEX IF NOT EXISTS notification_user_idx
    ON ficc_demo.notifications (user_id, active);

CREATE TABLE IF NOT EXISTS ficc_demo.notification_preferences (
    user_id text PRIMARY KEY,
    payload jsonb NOT NULL
);

CREATE TABLE IF NOT EXISTS ficc_demo.notification_reads (
    id text,
    content_token text,
    read_time text,
    PRIMARY KEY (id, content_token)
);

COMMENT ON TABLE ficc_demo.notifications IS '各用户的业务提醒及当前有效状态';
COMMENT ON TABLE ficc_demo.notification_preferences IS '用户提醒偏好对象';
COMMENT ON TABLE ficc_demo.notification_reads IS '按提醒内容版本保存已读时间';
COMMENT ON COLUMN ficc_demo.notification_reads.read_time IS '与当前代码保持一致，使用文本保存时间';

-- metadata 和 notification_metadata 中的 epoch 由对应服务首次启动时写入。
COMMIT;
