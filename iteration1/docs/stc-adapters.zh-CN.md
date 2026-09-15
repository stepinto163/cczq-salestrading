# STC 与 CRM 可切换依赖适配

本机开发默认使用模拟数据；内网联调通过配置接入 STC LoginSvr、BaseSvr。QuoteSvr、NotificationSvr 的业务代码使用同一组接口。CRM 可由内网团队独立编译为 JAR，替换配置即可接入。

## 1．实现与边界

| 依赖 | 配置 | 实现 |
|---|---|---|
| 用户与会话 | `identity.mode=simulator` | QuoteSvr 承载 `LocalLoginSimulator`，NotificationSvr 经 STC 客户端调用它 |
| 用户与会话 | `identity.mode=stc` | `StcLoginSessionClient` 调用复用源码的 `LoginSvrClient` |
| 债券、发行人、中债估值 | `base.mode=simulator` | `SimulatorBaseDataClient`，固定样例 |
| 债券、发行人、中债估值 | `base.mode=stc` | `StcBaseDataClient`，经 `NetClientWrapper` 查询并缓存真实数据 |
| CRM | `crm.mode=simulator` | 固定机构、联系人样例 |
| CRM | `crm.mode=empty` | 空列表；按 ID 查询返回空 |
| CRM | `crm.mode=custom` | 独立 JAR 实现 `CrmClient` |

三类依赖可以分别切换。真实模式失败时不会回退到模拟数据。业务仍使用 `ReferenceData` 与 `LoginSessionClient`，不直接依赖供应商字段。

### LoginSvrClient 源码复用

已有源码：`D:/workspace/GitHub/sc-app-stc/src/main/java/sc/app/stc/utils/LoginSvrClient.java`。

当前源码使用 `initialize(String, NetClientWrapper)`；平台打包的旧 STC JAR 使用 `initialize(String, CedaClientWrapper)`。因此本轮把已有源码编入 common 模块的 `ficc.common.stc` 包，避免旧 JAR 同名类覆盖，并将请求超时改为可配置。不是重新实现一套用户缓存或登录协议，也没有要求升级全部基础框架。移植文件与差异说明见 [源码说明](../cczq-app-salestrading-common/src/main/java/ficc/common/stc/README.zh-CN.md)。

客户端经 GW 登录真实 LoginSvr；业务服务用请求的 SID 解析 STC `userid`，所有权与审计保存该真实 ID。客户端填写的操作人不能替代已验证会话。管理员和内部服务账号分别通过 `identity.adminUserIds`、`identity.internalUserIds` 明确授权，不按 `S-` 等 ID 前缀推断权限。

注销、用户更新及连接变化会清理登录缓存，并使相关业务订阅失效；客户端重新登录后需重新打开订阅。真实模式下 QuoteSvr 自带的模拟登录入口关闭。

### BaseSvr 基础数据接入

`base.mode=stc` 且 `base.securityIds` 留空或不配置时，默认加载 BaseSvr 中银行间、上交所、深交所的债券基础数据，不再要求人工列出债券代码。保留已到期债券，供历史报价补充最新属性。数据范围仍取决于上游 BaseSvr 的数据覆盖。

正常模式复用 STC 的 `NetClientWrapper` 服务发现与消息调用，以及 BaseSvr 已有批量接口；并非直接把 `BaseSvrClient` 的静态订阅缓存搬入本项目。

| STC 请求主题 | 请求示例 | 返回及用途 |
|---|---|---|
| `stc.query.page.securityInfo` | `{"pageNum":1,"pageSize":500,"onlyValid":false}` | `Code/Total/Datas`，债券、债项评级、主体评级、条款 |
| `stc.query.page.issuerInfo` | `{"pageNum":1,"pageSize":500,"onlyValid":false}` | `Code/Datas`，发行人、地域、发行人类型 |
| `stc.query.page.cdcValuation` | `{"pageNum":1,"pageSize":500,"onlyValid":false}` | `Code/Datas`，中债日估值 |

这里的分批获取仅用于服务间基础数据同步，不涉及前端页面交互。每次只发一个请求；没有 `Total` 的接口读到不足一批为止。有总数时校验总数、实际条数；重复债券/发行人主键、重复整批、条数超限或加载超时均中止本轮，不发布截断数据。默认每批 500 条、每表保护上限 500000 条、整轮请求加载期限 300 秒；上限可通过配置调整，不能把保护上限当成正常数据截断条件。单个请求仍受 `base.requestTimeoutMillis` 限制。

单后台线程完成一轮后整体替换本地缓存；报价处理和筛选只读缓存，不按每条报价发远程请求。正常模式默认每小时刷新；收到 `stc.base.clear`、`stc.base.manualClear.**`、`stc.base.loadBondsInfo`、债券/中债更新通知或连接变化后使缓存失效，由后台线程重新加载。后台检查间隔一秒，失败后等待 30 秒重试。首次未就绪、已失效或后台刷新失败时返回不可用错误，不回退到模拟数据。

STC 分批接口没有跨表一致性版本，当前只能保证本机不发布半套缓存，并检测部分上游变动（总数变化、重复主键及刷新通知），不能宣称取得了上游同一时刻的数据库快照。已打开业务视图尚未实现基础数据变化单独触发全量重算，验证评级/估值变化时应重新查询。

分类直接复用 `sc.app.stc.api.base.po.TSmallView.getGoodsType()`：`2→RATE`、`1→CREDIT`，沿用实际 STC 依赖版本的字典。其他类型仍加载，保留 `bondSubtype/stcGoodsType`，`market` 留空，不擅自把存单或 ABS 归入信用债。可以用 `base.market.<securityId>` 或 `base.marketBySubtype.<子类型>` 明确覆盖。发行人类型通过 `base.issuerType.<原值>` 映射，同时保留 `sourceIssuerType`；债项评级与主体评级分别保存，不互相补值。

中债批量模型为 `TCdcBondValuation`：`bond_id/listed_market` 关联债券，`valuate_date` 为实际估值日期，`val_yield/val_clean_price/remaining_year` 映射收益率、净价及剩余期限。同一债券取最新日期；同日冲突值报错，缺少日期不伪造为今天。该批量模型没有明确的行权/到期估值标识，因此 `valuationBasis` 留空。中证估值和 CalSvr 不在本轮接入范围。

**从旧配置升级：删除 `base.securityIds=250001.IB`，或将值清空。** 建议同时把原来的 `base.refreshSeconds=60` 改为 `3600`，避免频繁整轮加载。成功后日志包含 `BaseSvr reference cache ready: mode=all, securities=..., issuers=..., valuations=...`。

仅在有意限制联调范围时，才填写可选 `base.securityIds`（最多 1000 个）。这种子集模式继续使用以下已有单债请求；未列出的债券不加载，默认刷新间隔为 60 秒：

| STC 请求主题 | 请求示例 | 读取内容 |
|---|---|---|
| `querySecurity` | `{"SecurityID":"250001.IB"}` | `Datas` 中的 `BondInfoView`，包括债项评级、主体评级、发行人、条款类型 |
| `stc.base.info.query` | `{"TableName":"p_issuer_info","KeyValue":"实际发行人代码"}` | `Datas` 中的发行人名称、类型、地域 |
| `queryChianSecurity` | `{"SecurityID":"250001.IB"}` | `Datas` 中的中债估值、估值日期与剩余期限 |

保留 STC 原主题拼写 `queryChianSecurity`。债券代码按 STC 的 `CIB→IB`、`SSE→SH`、`SZE→SZ` 规则转换并校验。

### 启动与接口故障定位

启动时 `ServiceConfiguration` 输出四行诊断：实际工作目录、`quote.properties` 与 `adapter.properties` 的绝对路径、生效的身份/基础数据模式与债券列表数量，以及 `StcBaseDataClient` 实际加载的 JAR/类目录（`loadedFrom`）。标识为 `diagnostics=stc-base-config-v2`。不打印数据库密码或服务账号密码。

如果仍报 `topic=querySecurity ... configured bond`，先检查 `base.loadMode`：`subset` 表示合并后的 `base.securityIds` 仍非空；配置先加载数据库文件，再加载 adapter 文件，删除或注释 adapter 中的键不会清除数据库文件中的同名值，明确写 `base.securityIds=` 才会覆盖。`adapter.properties=<not configured>` 表示未加载单独适配配置。若没有新诊断标识，或 `loadedFrom` 指向旧 JAR，检查 IntelliJ 的运行模块及依赖类路径，不要继续反复修改无效配置。

`QuoteSvr READY` 只表示业务框架已启动，不代表异步基础数据缓存已就绪。需要看到 `BaseSvr reference cache ready` 才表示本轮数据加载成功。`LoginSimulatorHandler` 出现在处理器注册日志也不代表正在使用模拟身份适配器。

真实 `BondQueryHandler`、`ChinaBondQueryHandler` 返回字段为大写 `Datas`。旧适配器错误地读取小写 `datas`，会把成功回复误报为基础数据不可用；已改为优先读取 `Datas`，兼容旧小写回复，测试样例也改用真实大写字段。

错误日志现区分：`OFFLINE`（STC 客户端未在线）、`NO_REPLY`（同步调用未返回内容，不直接断言一定超时）、`INVALID_JSON/NULL_JSON`（回复无法解析）、上游 `Code` 错误、成功回复缺少 `Datas`、配置债券返回空集合，以及加载期间的基础数据失效。请求错误包含服务键、topic、耗时和超时配置，不输出密码、SID、原始请求/回复内容。若 `CONNECTION_CLOSED` 紧随调试进程停止出现，可能是正常清理；仅凭该顺序不能认定为网络故障。

## 2．内网配置步骤

1. 内网使用 `mvn -f iteration1/pom_stc.xml clean verify` 构建，部署 `target-stc` 中的服务包，参阅 [STC 构建说明](maven-stc.zh-CN.md)。本机兼容版本使用 `target-maven`，不要混用。
2. 复制 [内网适配配置模板](../config/adapters.integration.example.properties) 为部署机本地文件，例如 `D:/services/salestrading/adapters.local.properties`。正常模式保持 `base.securityIds` 为空；按需配置字典覆盖及用户权限。
3. 在 QuoteSvr、NotificationSvr 各自 `control/<服务名>/resourceCfg.ini` 中设置：

```ini
adapter.properties=D:/services/salestrading/adapters.local.properties
```

数据库配置仍由 `quote.properties` 指向；先加载它，再用适配文件覆盖同名配置。路径相对进程工作目录解析，部署时建议用绝对路径。配置文件按 UTF-8 读取。

4. 将 [服务消费者配置](../config/stc-consumers.example.ini) 合并到相应服务的 `ATSConfig.ini`，配置内网 ZooKeeper。调用方以服务键寻址，不能把模板的 `127.0.0.1:1` 当成服务监听地址。真实 provider 名称须与内网注册信息一致。
5. GW 的 `SERVER.LoginSvr.ServiceName` 从本机模拟使用的 `QuoteSvr` 改为真实 `LoginSvr`；NotificationSvr 的同项也改为真实服务。GW 仍独立运行，使用其原有登录流程。
6. NotificationSvr 通过 `QuoteSvrClient` 和 `NetClientResource` 直接同步 QuoteSvr，不执行用户登录、不需要配置服务账号密码，也不需要为此增加 `identity.internalUserIds`。旧 `identity.serviceUsername/servicePasswordEnv/serviceClientType/serviceIp`、模拟调用方的 `identity.username/password` 已不再读取，可从本地适配配置删除。两个服务仍保留 `identity.mode=stc`，用于验证前端用户请求的 SID。
7. 重启相应服务，使用真实账号经 GW 登录。可先通过 DPS API 调试器查询基础资料及创建一条报价，检查返回值和数据库中的实际操作人，再验证 NotificationSvr 的内部订阅。

本机无需修改配置即可继续模拟运行；显式配置见 [本机模板](../config/adapters.local.example.properties)。切回模拟模式还需把 GW、NotificationSvr 的 LoginSvr 服务发现目标改回模拟服务。不要对正在运行的进程热切换模式。

C# 平台的固定样例账号、债券选择尚未完整改造成内网业务界面；本轮内网适配验证可使用支持输入地址和账号的独立 DPS API 调试器。模拟自动测试使用固定账号与数据，不可直接当作真实环境验收脚本。

### 内部同步与用户登录的边界

| 调用 | 主题/方式 | 用户会话 |
|---|---|---|
| NotificationSvr 初始化/恢复报价及分组 | `ficc.quote.internal.state.query`，请求 `requestId` 与可选 `limit/cursor`，返回 `Code/Msg/Data`，`Data` 含 `records/epoch/sequence/nextCursor/done` | 不发送用户 SID |
| NotificationSvr 初始化/恢复价格簿 | `ficc.price.internal.snapshot.query`，请求 `requestId`，返回 `Code/Msg/Data`，`Data` 为现有价格快照消息 | 不发送用户 SID |
| 后续内部推送 | `ficc.quote.events`、`ficc.price.snapshots` | STC 服务订阅，不执行用户登录 |
| 前端报价/价格/通知业务操作 | 原有业务请求和私有 report 主题 | 用户在 UI 输入账号密码，经 GW 登录；服务通过 LoginSvrClient 解析 SID 和 userid |

QuoteSvr 在 `handler/` 和 `control/QuoteSvr/resourceCfg.ini` 注册两个只读内部 Handler；不会给普通业务接口增加匿名访问。GW 拒绝转发这两个内部请求。STC 底层可能把连接标识写入 `signalID`，即使调用方没有发送用户 SID，因此内部 Handler 不把该字段当成已登录用户，也不要求它为空。内部端口按 STC 可信服务网络部署，仅对受信服务开放。连接标识和 ZooKeeper 注册本身均不代表身份认证。

升级时同时更新 common、QuoteSvr、NotificationSvr 和 GW，并合并 QuoteSvr 新增的 Handler 配置。旧服务端没有这些主题，不能只替换 NotificationSvr。同步完成日志为 `NotificationSvr source synchronized at ...`。

### 2026-09-15 登录日志说明

对比日志中，我们发送的是 `test1`，STC 客户端发送的是 `test01`。前者走普通用户查询并返回 `user:[] / Code=5006`；后者进入 `loginDefault`，不是同一认证路径。`LoginService.login` 明确支持 `isEncrypt=N` 与 `Y`；不能把这次失败直接归因于未加密。

`LoginHandler` 根据 `Consts.DefaultUser` 选择特殊登录路径。所给成功回复中 `sid/userid` 均为空，不能仅凭 `Code=0001` 或 `onLine` 判定已经获得 FICC 可用的业务会话。请使用 QA 中实际存在、能够取得有效 SID 和 userid 的 STC 账号；若配置 CRM 登录模式，还应检查其实际用户数据源。不要把特殊默认用户自动当作业务用户，也不在 QuoteSvr 配置用户密码。

C# 平台与 API 调试器共用 `DpsClient`，错误提示统一为 `Login failed: Code=..., Msg=...`，不再把真实 LoginSvr 的错误误标为 Simulator。登录请求仍由现有 DPS SDK 发送。

## 3．CRM 独立实现

空模式没有假机构或假联系人。待确认报价仍能保存手工输入的机构/联系人名称；确认时，已输入名称必须匹配可访问的真实 ID，所以空 CRM 无法完成这类报价的确认。

内网团队从 [CRM 独立工程](../extensions/crm-adapter/README.zh-CN.md) 开始，实现 `CrmClient`，编译 JAR 后配置：

```properties
crm.mode=custom
crm.adapterClass=cczq.salestrading.crm.InternalCrmClient
crm.adapterJar=D:/services/salestrading/extensions/cczq-salestrading-crm-adapter-0.1.0-SNAPSHOT.jar
crm.serverKey=SERVER.CrmSvr
```

适配器接收现有 `NetClientWrapper` 和配置，沿用 STC 服务发现。需要时自行增加 CRM 的消费者配置。主服务不硬编码 CRM 消息格式。更换 JAR 后重启服务，无需修改报价生命周期代码。

## 4．验证记录

2026-09-15 内部调用修复：本机兼容 Maven 五个模块 `verify` 通过，26 项单元/契约测试通过；独立 PostgreSQL schema 中以 5 条报价完成 50 项功能检查，包括内部读取、分批完整性、普通业务仍需登录和拒绝内部写操作。另启动两个隔离服务，均使用 `identity.mode=stc`，LoginSvr 故意指向不可用地址，基础数据使用模拟适配；NotificationSvr 通过真实 DPS 连接完成 QuoteSvr 状态及价格同步，无服务账号配置。该链路测试为零报价，25 秒期限，结束后停止测试进程并删除临时 schema。C# 平台与 API 调试器均编译通过。没有执行压力测试，也尚未连接 QA；内网 Maven/STC 实际运行仍需在 QA 验证。

记录：`iteration1/runtime/internal-sync-verify.log`、`internal-sync-business.json`、`internal-sync-20260915140311/`。

2026-09-15：取消正常模式的人工债券清单限制。兼容 Maven 全部五个模块 `verify` 通过，17 个单元/契约测试通过，其中新增 7 个批量同步测试，使用最多 5 只债券的内存样例，覆盖多批读取、历史债券、STC 分类、最新估值、评级不混用、新增债券、断线恢复、刷新期间失效、失败不发布半套数据、重复/不完整批次、保护上限、缺失日期及同日估值冲突。没有连接 QA、没有执行压力测试；内网 STC 依赖版本及真实接口仍需在目标环境验证。日志：`iteration1/runtime/base-universe-verify.log`。

接口依据：`sc-app-stc-basesvr` 的 `SecurityInfoQueryByPageHandler`、`IssuerInfoQueryByPageHandler`、`CdcValuationQueryByPageHandler`、`QueryByPageParam`、`TCdcBondValuation`；分类直接调用 `sc-app-stc` 的 `TSmallView`。实现入口为 `StcBaseDataClient`、`StcBaseUniverseLoader` 和 `StcBaseMapper`。

同日后续修复：按真实 STC 单债接口读取 `Datas`，保留小写兼容，补充故障分类及敏感信息不泄漏测试。五个模块再次 `verify` 通过，测试总数 20，失败/错误均为 0；日志 `iteration1/runtime/base-diagnostics-verify.log`。尚未在 QA 复验，不能据此宣称远端连接问题全部解决。

2026-09-14 的适配验证记录：

- Maven 全模块构建通过；9 个单元/契约测试通过，包括已有筛选测试、复用 LoginSvrClient 源码的请求与缓存测试、基础资料映射和失效、真实用户权限，以及外部 CRM JAR 的编译与加载。
- CRM 独立工程构建通过；C# 平台构建通过。
- 仅使用 3 条报价，经真实 C# DPS → GW → QuoteSvr → PostgreSQL 完成 40 项功能检查；独立临时 schema 已删除。没有执行压力测试。
- 本轮未连接内网真实 LoginSvr/BaseSvr/CRM；真实返回数据、字典、权限和网络重连需要内网集成验证。NotificationSvr 已接入适配接口，并按实际用户集合生成提醒；本轮三条报价的端到端测试未启动 NotificationSvr。

本机功能记录：`iteration1/runtime/adapter-smoke/20260914_162657_5f448878/functional.json`。运行日志和凭据不进入版本库。
