# 报价持久化与迁移

## 存储边界

`PgStore` 通过 `QuoteColumns` 在数据库列与 `QuoteRecord` 之间映射。当前业务值以列为准，读取时即使 payload 中残留同名键，也不能覆盖列值（包括列为 NULL 的情况）。所有 `QuoteInput` 可编辑字段均有列，外部 JSON 协议不变。

| 协议字段 | quotes 列 | 类型 |
|---|---|---|
| quoteId / ownerUserId / clientQuoteId | id / owner_id / client_id | text，原有列 |
| securityId / status / revision | security_id / status / revision | text / text / bigint，原有列 |
| side / quoteType / yieldType | side / quote_type / yield_type | text |
| price / quantity | price / quantity | numeric，协议仍返回字符串 |
| quantityUnit / currency | quantity_unit / currency | text |
| tradeDate / settlType | trade_date / settl_type | date / text |
| institutionId / institutionName | institution_id / institution_name | text |
| contactId / contactName | contact_id / contact_name | text |
| urgent / recommended / whole | urgent / recommended / whole | boolean，急 / 荐 / 整 |
| oco / packing | oco / packing | boolean，OCO / 打包 |
| remark | remark | text |

五个旗标均为 NOT NULL，默认 false。允许不完整待确认报价的价格、数量、交易日等为 NULL；数据库约束拒绝非正数量、非有限价格及 OCO 与打包同时开启。业务层继续负责确认报价所需字段、枚举及权限校验。

`payload` 保留嵌套来源 `source`、`sourceMessageId`、克隆来源、审计用户/时间及未知扩展字段。审计时间保留协议原文以保留时区与小数精度；已有 `update_time` 列仍用于排序索引。历史 `history`、事件 `events`、幂等回执 `requests` 保留完整 JSON 快照，未改为列存储。

行情簿与条件匹配仍使用内存中的类型化报价。此次不改筛选热路径，也不声称数据库拆列会加速内存筛选。未给低选择性的 Boolean 列增加索引。

## 自动升级

1. 开发阶段按向前升级处理，不提供旧版本回退流程。启动新版 QuoteSvr 后，先获取原有单写入者锁，再执行 `QuoteSchemaMigration`。
2. `metadata` 没有 `quotes.columns.version` 时，同一事务完成加列、旧 payload 回填、设置约束、移除重复业务键、写入版本 `1`。已有六个标识/状态列保留原值。
3. 旧记录缺失旗标时按创建报价的默认值 false 回填；显式 null、非法类型、非法数量等导致迁移失败并完整回滚。失败记录 ERROR 及异常栈，服务启动失败，不丢弃记录或切换模拟数据。
4. 后续启动看到版本 `1` 只验证列可读取，不再次覆盖现有列值；未知版本拒绝启动。

使用既有数据源与 schema 配置，无新增配置项，无需手动运行迁移脚本。应用数据库账号须拥有业务表及修改表结构的权限。空库初始化 SQL 包含新列，约束及版本标记仍由服务启动统一完成。较大数据库的回填耗时未压测，受既有数据库语句超时限制；失败后事务回滚，查看日志原因后处理。

日志组件为 `QUOTE_SCHEMA`，INFO 输出 STARTING/INITIALIZED，失败输出 FAILED 与异常栈；它仅说明本地数据库迁移状态，不代表远端服务已就绪。

## 验证

新增 `QuoteColumnStorageSmoke`：独立 schema 验证字段覆盖、迁移、持久化、重启、列优先级、扩展字段与审计精度、约束、事务回滚，以及非法旧数据导致启动失败且原数据完整保留。通过真实 `QuoteService` 路径验证创建、确认、更新及历史快照。

同时执行既有生命周期、QT intake、议价及增量视图回归。验证使用本机 PostgreSQL 的隔离 schema，不代表已完成 QA TBase 迁移、桌面端到端或压力验证。common 和 C# 协议不需要修改。


## 每日报价历史查询表（2026-11-03）

目的：history为审计保存每次变化，原历史报价查询每次都从操作记录重建每日最后版本，数周后扫描/排序成本高。新增quote_history_daily专供历史报价查询；报价变更历史和推荐证据继续读原history。

| 列 | 类型 | 含义 |
|---|---|---|
| operation_date / quote_id | date / text，联合主键 | 操作日期＋报价ID，不是债券代码 |
| first_operation / last_operation | timestamptz | 当日最早有效快照时间 / 最后版本时间 |
| revision / event_id | bigint / text | 最后版本及来源审计事件 |
| security_id / owner_id | text | 最后版本券代码/归属，支持索引筛选 |
| snapshot | jsonb | 最后完整报价快照，保持原字段及空值语义 |

同一报价同日多次操作只更新一行；同券不同报价分别保存，跨日同报价增加一行。沿用旧接口：只在有操作的日期产生记录，不为无变化日期补行。日期不是报价的tradeDate；服务生成的操作日期必须与Asia/Shanghai时间一致，异常导入数据不静默改变日期或丢弃。保留最后私有/删除状态，再在查询时按原规则判断正式报价，不能复活旧ACTIVE版本。

- 每条history INSERT由AFTER INSERT触发器history_daily_insert更新每日表；二者天然处于同一数据库事务，触发器失败会使审计INSERT失败。整个业务事务回滚时两表同时回滚。比较操作时间、revision后决定最后版本，旧/乱序事件仍参与最早时间但不覆盖最后快照。
- PgStore启动自动执行DailyQuoteHistoryMigration：获得单写入者锁及history表锁；每批2048事件回填，建日期/券/owner/展示顺序索引，安装函数/触发器，写metadata键history.daily.version=1；DDL、回填与版本标记在同一事务。后续启动不全量重建，检查表列及触发器可用。
- 继续使用原数据库连接、schema和部署流水线；无需新增服务、配置项或手动复制SQL。现有业务库部署账号需具备本schema的建表、索引及函数/触发器权限。完整原history保留；直接修改/删除审计事件不是此追加式写入契约，修正历史应另行设计同步重建，不允许手工改审计后假设每日表自动纠正。
- 迁移失败回滚并阻止启动，日志组件DAILY_QUOTE_HISTORY有INFO STARTING/INITIALIZED，失败ERROR与异常栈。回填进度HISTORY_DAILY_BACKFILL可单独检索。禁止手工写版本标记或关闭触发器绕过失败。
- 查询协议不变，客户端、Common和BondSearch Core不需随此变更升级。根债券条件已由原引擎完整解析、关注券集及支持的报价条件可SQL计数分页；综合文本、动态期限、包含报价条件的分组保留原匹配器。SQL计数物化小键，最终页才加载/编码完整JSON。

本机PostgreSQL16.15：百万审计→10万每日记录回填初始化7.75秒；原审计约652.66MiB，每日表含索引约52.05MiB。该耗时含连接与PgStore初始化，不是服务整体启动时间。功能验证188原历史＋486差分＋54每日迁移检查通过（含跨批/重启/回滚/故障/外部连接原子可见性）；QA数据库及真实服务部署尚未执行。规模测试方法及边界见主仓库docs/history-search-optimization.zh-CN.md。
