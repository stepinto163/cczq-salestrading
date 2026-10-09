# QA 环境数据库初始化

执行 [001-create-tables.sql](../sql/001-create-tables.sql)，创建当前可运行平台实际使用的表。此脚本与当前 Java 持久化代码一致，不是完整生产版 ER 模型的建表脚本，也不包含本机测试数据。

## 执行方式

使用 PostgreSQL 16（本机版本为 16.15）。由 DBA 准备一个 UTF-8 数据库和应用账号，例如数据库与账号均为 `ficc_quote`，并让应用账号拥有该专用数据库。密码在 QA 环境单独设置。

使用该应用账号连接数据库，在 DBeaver 中打开 SQL 文件并执行整个脚本；或者运行以下命令，将 `QA_DB_HOST` 和端口替换为实际值：

```powershell
psql -h QA_DB_HOST -p 5432 -U ficc_quote -d ficc_quote -W -v ON_ERROR_STOP=1 -f iteration1/sql/001-create-tables.sql
```

脚本包含事务，成功时一次提交；执行失败时应回滚后处理错误。脚本可重复执行，但 `IF NOT EXISTS` 不会修复已有表的结构差异，因此适用于空库初始化或同版本重复部署。

## 表的范围

| Schema | 表 | 用途 |
|---|---|---|
| `ficc_salestrading` | `quotes`、`history`、`sources` | 报价当前状态、连续变更历史、来源消息 |
| `ficc_salestrading` | `metadata`、`events`、`requests`、`settings` | 服务元数据、事件、请求幂等、关注债券与筛选分组 |
| `ficc_salestrading` | `notifications`、`notification_preferences`、`notification_reads` | 提醒、用户偏好、已读状态 |
| `ficc_salestrading` | `notification_metadata`、`notification_events`、`notification_requests`、`notification_settings` | NotificationSvr 使用的公共存储结构 |

两个服务共用一个 `ficc_salestrading` schema。业务服务的进程拆分不要求数据库按服务拆分 schema；通知的公共表使用 `notification_` 前缀，防止事件序号、幂等结果和设置互相混用。写入锁按服务区分，允许两个服务同时连接同一 schema。

共 14 张表、2 个事件序列，以及代码要求的主键、唯一约束和查询索引。两个服务首次启动时会分别初始化 `metadata` 和 `notification_metadata` 中的 `epoch`。无需预置报价或提醒数据。

当前报价的稳定业务属性使用类型化列：价格、数量为 `numeric`，交易日为 `date`，`urgent/recommended/whole/oco/packing` 为 Boolean；机构及联系人 ID/名称也有独立列。`quotes.payload` 仅保存来源、审计时间及扩展元数据。待确认报价允许只有名称，确认时由用户选择 CRM 对应 ID，变更前后的完整快照仍保存在 `history.payload`。旧库由新版 QuoteSvr 启动时自动回填并移除 payload 中重复的业务字段，详见[报价持久化与迁移](../cgws-ficc-saletrading-quotesvr/deployment/quote-storage.zh-CN.md)。

不创建本机 `ficc_notification` 中遗留的 `quotes`、`history`、`sources` 表。行情价格簿由报价状态重建，当前实现没有独立行情价格表。用户、债券、机构、联系人等基础资料由配置的 STC/CRM 数据源提供。

## 服务配置

QA 连接配置集中在 `$ATS_ROOT/control/DBPoolConfig.ini`，服务包内的 `config/service.properties` 只选择数据源和 schema：

```properties
dbpool.cfg=$ATS_ROOT/control/DBPoolConfig.ini
db.sourceName=FICC_SALESTRADING
db.schema=ficc_salestrading
```

两个服务使用同一业务 schema，通知表带独立前缀。实际 QA 可以使用其他 schema（如 `ficc_salestrading2`），但建表 SQL、检查 SQL 和服务配置必须一致；脚本默认创建 `ficc_salestrading`，不会创建数据库。共享数据源的追加方法见[QA 配置指南](jenkins-qa.zh-CN.md)，保留 STC 既有 `MYSQL0`。

当前服务启动时执行建表，QuoteSvr 升级还会执行 `ALTER TABLE` 和数据回填，所以仅授予增删改查权限不足以满足启动代码；使用拥有这些对象及专用数据库的应用账号最直接。若由 DBA 代为建表，应使用 `SET ROLE` 切换到应用账号后执行，使对象归属一致。

完成建表后，按 [启动指南](local-startup.zh-CN.md) 的顺序启动服务，同时把 ZooKeeper 注册地址、服务对外地址和客户端 GW 地址改成 QA 对应地址。数据库初始化不替代这些网络配置。

## 检查

```sql
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_schema = 'ficc_salestrading'
  AND table_type = 'BASE TABLE'
ORDER BY table_schema, table_name;

SELECT id,
       institution_name, contact_name,
       urgent, recommended, whole, oco, packing
FROM ficc_salestrading.quotes
ORDER BY update_time DESC;
```

空库应列出 14 张表，报价查询返回零行属于正常情况。本脚本只准备新环境结构；如果还要搬迁已有数据，应另做一致性备份与恢复，包含 JSONB 内容、历史、幂等记录及元数据。

## 已有双 schema 环境升级

全新 QA 数据库只执行 `001-create-tables.sql`。如果要保留旧环境的提醒数据：

1. 停止 QuoteSvr 和 NotificationSvr，备份数据库。
2. 已有报价 schema 尚未改名时，先执行 `003-rename-salestrading-schema.sql`；随后执行 `001-create-tables.sql`，创建同一 schema 下新增的通知表。
3. 执行 [002-migrate-notifications-to-shared-schema.sql](../sql/002-migrate-notifications-to-shared-schema.sql)。它把旧 `ficc_notification` 的 7 张有效表复制到 `ficc_salestrading`，保留通知、已读、偏好、事件、epoch 和幂等状态。目标通知表必须为空，否则整个迁移回滚。
4. 两个服务的 `db.schema` 都设为 `ficc_salestrading`，部署新代码后启动服务。报价原表由 schema 改名保留；旧 `ficc_notification` 仅作为迁移备份保留，新版本不再读写它。

`002` 是一次性迁移，不要在已启动并产生通知数据的新库上执行。它不会删除旧表，也不会搬迁旧通知 schema 中无用的报价表。
