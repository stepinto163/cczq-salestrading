> 2026-10-31 架构更新：FICC 参考模块集成至原 BaseSvr，新增 `ficc.reference.sync`；旧 `stc.base.search` 保持原 STC 契约。以下独立服务部署描述是此前方案，已被本节取代。
>
> 当前部署：单个 `SERVER.BaseSvr`，共享 `$ATS_ROOT/control`。FICC 客户端配置 `Ficc.BaseService=BaseSvr`、`Ficc.ReferenceSyncEnabled=true`；后端适配器配置 `base.mode=ficc`、`base.serverKey=SERVER.BaseSvr`，不再自动注册独立服务。FICC 后台模块失败不阻断旧 STC 接口，新接口初始失败返回 1009；已有快照失败返回显式 STALE。源码运行、Jenkins/Salt 和服务控制同步迁移，先确认 BaseSvr 的 FICC 模块 READY 再部署消费者。

# 销售交易基础数据同步

## 服务边界

新增独立服务 `FiccBaseSvr`，BaseSvr 仓库分支为 `feature/ficc-reference-data`。现有 STC BaseSvr 的部署、服务名和旧客户端均不修改。新销售交易客户端通过 `Ficc.BaseService=FiccBaseSvr` 连接新服务。

服务只读 STC 已生成的债券表，不运行原 BaseSvr 的原始数据重建任务，不回写共享债券数据。

## 数据来源

| 内容 | 来源和规则 |
| --- | --- |
| 债券代码、简称、分类、发行人标识、条款 | STC `stc_tbook_bonds`，保留原映射 |
| 中债估值、净价、待偿期、修正久期 | STC 原估值加载与选值规则；区分行权与到期口径 |
| 中证到期及行权收益率、净价、全价 | STC `sdb_csi_bond_valuation` |
| 信用债、ABS、NCD、地方债的债项评级与主体评级 | DM `onshore_bond_filter`；1=AAA、2=AA+、3=AA、4=AA-、5=A+，其他值为空，不解释成有效评级 |
| 上述债券的省、市、区县 | DM `com_basic` / `area_info`，输出行政区划代码 |
| 国债、政策性金融债等其他利率债的评级 | 仍取 STC，不用 DM 覆盖 |
| 债券库 | IBOR `ibor_bond_white` / `ibor_bond_black`，不是 STC 债券子类型 |

同一 DM 债券代码存在多条有效数据时，取 `update_time` 最新、再按 `id` 最大的一条；忽略空代码。选定的 DM 字段为空或记录被删除时清空旧值，不保留上一版本的评级或地区。

IBOR 排除已删除记录。黑名单优先显示“黑名单库”；否则合并白名单 `pool_list` 并去重：321 一级库、323 二级库、324 三级库、325 四级库、541 做市库。未匹配或只有未知枚举时为空。移出库后必须清空客户端旧值。

## 同步流程

1. 服务启动时读取三个来源，全部成功后才发布 FICC 快照。之后每轮完成后等待 60 秒再检查更新。
2. 复用 `stc.base.search`，请求增加 `ReferenceSync=true`，未增加业务接口。原有非同步请求仍按原协议处理。
3. 初次请求不带 `ReferenceVersion`，下载完整快照。后续请求携带上次成功版本，只返回变更记录与 `delflag=1` 的删除记录。
4. `ExtendedModifyDate` 是聚合数据变更时间，不只是 STC 原始修改时间。因此只改 DM、IBOR 或估值也能产生增量。
5. 首页返回 `SnapshotVersion`，后续分页固定该版本，避免下载中途混入另一批数据。当前保留最近四批快照；版本过期时本轮失败，下轮从上次已提交检查点重试。
6. 服务重启产生新的版本前缀，客户端会自动重新下载全量。
7. 基础数据插件在独立 SQLite 事务中同时保存数据与版本；所有分页完成后才替换内存。分页失败不提交半份数据。
8. 缓存按网关地址、端口和基础服务名隔离。启动可先读取本环境上次完整缓存，同时进行服务器同步。

每条同步记录保留原基础插件搜索所需的 STC 字段，并增加完整 `FiccReference` 对象；不重复携带原始 `info` JSON 和其他模块的大量字段。传统非同步债券查询仍可读取完整 STC 债券模型。客户端仅持久化搜索所需字段、版本与 FICC 扩展，避免保存无用途的大对象。

报价录入、行情中心、报价管理、报价提醒统一读取基础数据插件的内存快照。债券显示资料不再按页面请求 `ficc.base.get`；CRM 的机构、联系人查询仍保留原接口。基础资料变化只更新显示字段，不修改报价价格、数量、归属人或生命周期。

## 部署与启动

- 共享连接与许可证仍在 `$ATS_ROOT/control`：`ATSConfig.ini`、`DBPoolConfig.ini`、`server.lic`。
- 默认 STC 数据源为 `MYSQL0`；DM 为 `FICC_DM`；IBOR 为 `FICC_CRM`。本机分别对应现有 PostgreSQL 的 STC、`dm_data`、`ibor_dev_20261013` 数据库。
- IBOR 连接用户必须能读取两张债券库表。本机仅补了读取授权，没有导入、清空或修改这些业务表。
- 新服务身份 `SERVER.FiccBaseSvr` 在进程内注册；不覆盖共享配置中的 `SERVER.BaseSvr`。
- 本地端口 18687；QA 地址由 Salt 环境参数渲染。Jenkins 使用本项目自己的启动脚本和配置。
- IntelliJ 使用提交的 `.run/FiccBaseSvr.run.xml`；源码目录和部署包均读取上述共享文件。
- 新客户端同时构建 `STC.Common`、`common.netapi`、`STC.Data.Plugin` 和 `STC.SalesTrading.Plugin`，不能只替换销售交易 DLL。

初次来源加载失败时启动失败，记录异常并退出；运行期间同步失败保留上一份完整 FICC 快照并记录 `STALE`。`READY` 表示本地参考快照就绪，不代表桌面端已连接成功。初始化信息同时输出控制台和启动日志。

## 验证范围

2026-10-29 本次实施验证：

- 服务端 Maven 构建及 11 项单元测试通过，覆盖来源选择、快照不可变、DM/IBOR 独立变更、删除、版本与分页。
- 客户端完整隔离构建通过；64 项集成检查通过，其中包含真实 Java DPS 应答的 C# 反序列化检查。
- 本机真实数据库及 DPS 功能检查下载 112,346 条债券、113 页；其中 19,089 条命中 IBOR 债券库，62,051 条具有 DM 债项评级。随后再次读取三个来源，未变更数据返回零条增量。
- 快照在服务端按不可变序列化记录保存；不同时保留多份庞大的 JSON 对象树。数据库游标按批读取，重复 STC 字符串采用有界共享，不丢弃原始字段。
- 验证了缺失配置时非零退出，控制台与启动日志均包含 ERROR 和异常栈。QA 模板已渲染检查资源路径、共享 DBPool 路径、服务地址与运行依赖。
- 尚未执行 QA 服务器部署、旧 STC 客户端连接回归或真实桌面登录验收；原运行客户端未替换。本次是功能验证，不是压力或容量测试。

自动检查覆盖来源选择、IBOR 枚举、DM 独立变化、删除、分页一致性、服务重启、客户端失败不提交、SQLite 版本持久化，以及列表包装行的参考资料通知。真实数据库启动检查与桌面端登录验证分开记录；功能检查不等同于性能测试。

## QuoteSvr / NotificationSvr 共用适配器

两个服务通过 Common 的 `FiccBaseDataClient` 接入 `SERVER.FiccBaseSvr`，各进程各有一份内存快照，不共享进程内对象。原有 `ReferenceData` 业务入口及 `ficc.base.get` 协议保留；CRM 继续走原有独立适配器。

- 启动必须完整下载成功后才就绪；服务端需提供 `FiccReference.referenceSchemaVersion=2`。旧版本或错误分页会明确导致启动失败，不退回原 BaseSvr 或模拟数据。
- 运行期每轮完成后等待 `base.refreshSeconds`（默认 60 秒），下载增量并一次性替换快照。失败保留上次成功快照，下一轮重试；不发布半份数据。
- `require/security/enrich`、债券搜索、发行人及估值查询均读取本地内存，不在处理报价或提醒时逐券调用 BaseSvr。
- FiccBaseSvr 和服务适配器各有刷新周期，所以源数据库变更并非即时到达消费者；默认通常需要一至两轮刷新，另加加载耗时。报价实时推送机制不因此改变。
- FiccBaseSvr 增补市场分类、发行人名称与属性、票息条款、含权标志、中债/中证估值口径等后端所需字段。STC 发行人来自 `p_issuer_info`，机构所有制来自 `institution`；DM 对约定信用债/地方债的评级、地区字段仍有最终决定权。
- 原 BondSearch Core 保留尚未迁入基础快照的 DM 专用筛选属性；不能覆盖基础快照负责的字段，也不能用旧 DM 值恢复已经清空的评级、地区或债券库。
- 基础资料刷新只重新计算已打开视图的筛选结果。不会确认、Refer、删除报价，也不会生成新的通知事件或弹窗；提醒列表的筛选结果可以随之增删。

源码与 Salt 包使用同一组配置：`base.mode=ficc`、`base.serverKey=SERVER.FiccBaseSvr`、`base.requestTimeoutMillis=10000`、`base.snapshotPageSize=1000`、`base.refreshSeconds=60`。若共享 ATSConfig 尚无新服务，适配器使用已有 `REGISTER.ServerList` 在本进程补齐消费者发现配置；不改写共享文件，不覆盖原 `SERVER.BaseSvr`。已有显式 FiccBaseSvr 配置优先。

部署顺序：先发布并启动新版 FiccBaseSvr，再发布包含新版 Common 的 QuoteSvr、NotificationSvr。无需新增业务接口或手工复制 example 配置。原 STC BaseSvr 与旧 STC 客户端保持原路由。

验证记录：FiccBaseSvr 13 项单元测试、Common 新适配器 6 项测试通过；独立 Java 消费进程通过真实服务发现与 DPS 下载 112,346 条债券，随后确认零变更增量。测试覆盖失败保留旧快照、删除、服务重启全量、来源优先级、日志输出及失败传播。未替换正在使用的桌面端，也未部署 QA；这些结果不代表桌面端或 QA 已验收。

后端构建回归共 146 项测试，145 项通过、1 项原有显式集成测试跳过；另执行临时数据库 schema 的报价视图回归 583 项检查、通知协议回归 55 项检查。通知新增检查实际变更地区属性，验证筛选行移除及恢复、通知事件序号不变、推送仅为列表视图更新。临时 schema 与检查进程均已清理。
