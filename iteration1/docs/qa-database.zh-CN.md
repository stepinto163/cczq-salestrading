# QA 环境数据库初始化

执行 [001-create-demo-tables.sql](../sql/001-create-demo-tables.sql)，创建当前可运行原型实际使用的表。此脚本与当前 Java 持久化代码一致，不是完整生产版 ER 模型的建表脚本，也不包含本机测试数据。

## 执行方式

使用 PostgreSQL 16（本机版本为 16.15）。由 DBA 准备一个 UTF-8 数据库和应用账号，例如数据库与账号均为 `ficc_quote`，并让应用账号拥有该专用数据库。密码在 QA 环境单独设置。

使用该应用账号连接数据库，在 DBeaver 中打开 SQL 文件并执行整个脚本；或者运行以下命令，将 `QA_DB_HOST` 和端口替换为实际值：

```powershell
psql -h QA_DB_HOST -p 5432 -U ficc_quote -d ficc_quote -W -v ON_ERROR_STOP=1 -f iteration1/sql/001-create-demo-tables.sql
```

脚本包含事务，成功时一次提交；执行失败时应回滚后处理错误。脚本可重复执行，但 `IF NOT EXISTS` 不会修复已有表的结构差异，因此适用于空库初始化或同版本重复部署。

## 表的范围

| Schema | 表 | 用途 |
|---|---|---|
| `ficc_demo` | `quotes`、`history`、`sources` | 报价当前状态、连续变更历史、来源消息 |
| `ficc_demo` | `metadata`、`events`、`requests`、`settings` | 服务元数据、事件、请求幂等、关注债券与筛选分组 |
| `ficc_demo` | `notifications`、`notification_preferences`、`notification_reads` | 提醒、用户偏好、已读状态 |
| `ficc_demo` | `notification_metadata`、`notification_events`、`notification_requests`、`notification_settings` | NotificationSvr 使用的公共存储结构 |

两个服务共用一个 `ficc_demo` schema。业务服务的进程拆分不要求数据库按服务拆分 schema；通知的公共表使用 `notification_` 前缀，防止事件序号、幂等结果和设置互相混用。写入锁按服务区分，允许两个服务同时连接同一 schema。

共 14 张表、2 个事件序列，以及代码要求的主键、唯一约束和查询索引。两个服务首次启动时会分别初始化 `metadata` 和 `notification_metadata` 中的 `epoch`。无需预置报价或提醒数据。

机构名称、联系人名称与其他完整报价字段存于 `ficc_demo.quotes.payload` JSONB。待确认报价允许只有名称，确认时由用户选择 CRM 对应 ID，变更前后的内容保存在 `history.payload`。脚本保留现有存储方式。

不创建本机 `ficc_notification` 中遗留的 `quotes`、`history`、`sources` 表。行情价格簿由报价状态重建，当前实现没有独立行情价格表。用户、债券、机构、联系人等基础资料仍由当前模拟器提供。

## 服务配置

修改 QA 副本中的 `iteration1/runtime/quotesvr.local.properties`，保留其他配置项，更新以下数据库属性：

```properties
db.url=jdbc:postgresql://QA_DB_HOST:5432/ficc_quote
db.user=ficc_quote
db.password=填写QA密码
db.schema=ficc_demo
```

QuoteSvr 和 NotificationSvr 的 `control/<服务名>/resourceCfg.ini` 中，`quote.properties` 必须指向 QA 上的实际配置文件。两个服务读取同一份连接属性和 `db.schema`，不再为 NotificationSvr 强制指定另一个 schema。必须部署本次更新后的 common 和 NotificationSvr JAR，旧二进制仍使用旧 schema。

当前服务启动时仍执行 `CREATE ... IF NOT EXISTS`，所以仅授予增删改查权限不足以满足现有启动代码；使用拥有这些对象及专用数据库的应用账号最直接。若由 DBA 代为建表，应使用 `SET ROLE` 切换到应用账号后执行，使对象归属一致。

完成建表后，按 [启动指南](local-startup.zh-CN.md) 的顺序启动服务，同时把 ZooKeeper 注册地址、服务对外地址和客户端 GW 地址改成 QA 对应地址。数据库初始化不替代这些网络配置。

## 检查

```sql
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_schema = 'ficc_demo'
  AND table_type = 'BASE TABLE'
ORDER BY table_schema, table_name;

SELECT id,
       payload->>'institutionName' AS institution_name,
       payload->>'contactName' AS contact_name
FROM ficc_demo.quotes
ORDER BY update_time DESC;
```

空库应列出 14 张表，报价查询返回零行属于正常情况。本脚本只准备新环境结构；如果还要搬迁已有数据，应另做一致性备份与恢复，包含 JSONB 内容、历史、幂等记录及元数据。

## 已有双 schema 环境升级

全新 QA 数据库只执行 `001-create-demo-tables.sql`。如果要保留旧环境的提醒数据：

1. 停止 QuoteSvr 和 NotificationSvr，备份数据库。
2. 执行 `001-create-demo-tables.sql`，创建同一 schema 下新增的通知表。
3. 执行 [002-migrate-notifications-to-shared-schema.sql](../sql/002-migrate-notifications-to-shared-schema.sql)。它把旧 `ficc_notification` 的 7 张有效表复制到 `ficc_demo`，保留通知、已读、偏好、事件、epoch 和幂等状态。目标通知表必须为空，否则整个迁移回滚。
4. 部署新代码后启动服务。报价原表无需搬迁；旧 `ficc_notification` 仅作为迁移备份保留，新版本不再读写它。

`002` 是一次性迁移，不要在已启动并产生通知数据的新库上执行。它不会删除旧表，也不会搬迁旧通知 schema 中无用的报价表。

## 本机验证（2026-09-14）

建表脚本在临时 schema 的事务中连续执行两次，确认 14 张表后回滚。common、QuoteSvr、NotificationSvr 编译通过；共享 schema 测试确认不同服务可同时连接、同一服务的重复写入者被拒绝，事件与幂等结果互不干扰，测试写入全部回滚。

本机迁移前备份为 `iteration1/runtime/before-shared-schema-20260914-113125.dump`。迁移后、重启前逐行比较 7 张通知表，内容完全一致。服务已改为使用 `ficc_demo`；旧 `ficc_notification` 保留作备份。网关在本次维护后未自动恢复 QuoteSvr 连接，重启 GW 后，C# 连接检查 3 项及提醒功能检查 20 项通过，功能检查仅创建 3 条测试报价。结果见 `runtime/shared-schema-client.json`、`runtime/shared-schema-notifications.json`；未执行压力测试。
