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
| CRM | `crm.mode=postgresql` | 只读访问独立 PostgreSQL 的机构、联系人和当前关系表 |
| CRM | `crm.mode=custom` | 独立 JAR 实现 `CrmClient` |

三类依赖可以分别切换。真实模式失败时不会回退到模拟数据。业务仍使用 `ReferenceData` 与 `LoginSessionClient`，不直接依赖供应商字段。

### LoginSvrClient 源码复用

已有源码：`D:/workspace/GitHub/sc-app-stc/src/main/java/sc/app/stc/utils/LoginSvrClient.java`。

当前源码使用 `initialize(String, NetClientWrapper)`；平台打包的旧 STC JAR 使用 `initialize(String, CedaClientWrapper)`。因此本轮把已有源码编入 common 模块的 `ficc.common.stc` 包，避免旧 JAR 同名类覆盖，并将请求超时改为可配置。不是重新实现一套用户缓存或登录协议，也没有要求升级全部基础框架。移植文件与差异说明见 [源码说明](../cczq-app-salestrading-common/src/main/java/ficc/common/stc/README.zh-CN.md)。

客户端经 GW 登录真实 LoginSvr；业务服务用请求的 SID 解析 STC `userid`，所有权与审计保存该真实 ID。客户端填写的操作人不能替代已验证会话。管理员和内部服务账号分别通过 `identity.adminUserIds`、`identity.internalUserIds` 明确授权，不按 `S-` 等 ID 前缀推断权限。

注销、用户更新及连接变化会清理登录缓存，并使相关业务订阅失效；客户端重新登录后需重新打开订阅。真实模式下 QuoteSvr 自带的模拟登录入口关闭。

### BaseSvr 基础数据接入

`base.mode=stc` 现在直接使用部署环境的 `sc.app.stc.utils.BaseSvrClient`，设置 `isSubOrReq=R`，由其委托当前工作的 `BaseSvr2Client`。旧 `StcBaseUniverseLoader` 和分页加载路径已删除。

- 报价校验、`ficc.base.get`、历史报价补充属性：按 securityId 调用 `getBondInfo`；首次访问由 STC 查询 BaseSvr，后续使用 STC 缓存。
- 原生接口由 STC 实现调用：债券 `querySecurity`、估值 `queryChianSecurity`；发行人使用 `BaseSvr2Client.getInfoTableVal("p_issuer_info", issuerId)`。
- 缓存、断线清理、基础资料更新通知由原有 STC 客户端负责。FICC 只映射业务字段，不再自行周期性下载三张表。
- `ficc.base.query` 的完整代码查询（例如 `240210.IB`）同样执行按需查询，不要求债券预先存在本地缓存。
- 名称、部分代码或空条件检索由我们自己的适配器扩展：调用 STC 前端使用的 `stc.base.search`，按页读取 BaseSvr 债券字典，再匹配代码、简称和拼音。无需预先访问债券，也不修改上游 `BaseSvrClient` / `BaseSvr2Client` 源码。选择列表不逐条查询发行人和估值，选中后的完整代码查询再补充属性。
- 名称检索每页 500 条，翻页前检查 3 秒预算；单次请求超时由 `base.requestTimeoutMillis` 设置。字典不完整、页总数变化或超时会返回错误，不会伪装成无匹配；超过 10,000 个匹配时要求缩小条件。大字典冷查询的延迟尚需 QA 验证，可优先使用完整代码。
- `base.securityIds`、分页和刷新周期配置已不再控制此路径。遗留非空 securityIds 会提示已忽略，不会成为报价白名单。

启动日志应显示 `base.loadMode=stc-client-on-demand`，随后显示实际 `BaseSvr2Client` 所在 JAR；不会再出现 `cache ready: mode=all, securities=0` 这种误导性全量就绪提示。每次 STC 方法调用记录方法、债券参数、结果是否存在、耗时；DEBUG 记录返回对象，配置见 [DPS 日志](dps-logging.zh-CN.md)。

当前笔记本兼容运行库过旧，不含 BaseSvr2Client。桥接类通过运行时绑定调用 QA 的现有 STC 类，以便模拟模式仍能在本机编译、运行。若错误地用旧兼容运行库启动 STC 模式，会明确启动失败，不再退回自写协议。该桥接没有重新实现 STC 网络协议。

真实数据返回空且 STC 将债券记入 failedBonds 时，才视为未找到；未完成查询或离线报 NOT_READY。正常数据仍通过 StcBaseMapper 映射评级、发行人、条款及 STC 返回的估值，不伪造参考数据。现有业务视图尚未实现基础数据变化单独触发全量重算，验证评级变化时应重新查询。

本轮本机 22 项单元/契约测试通过，两个服务构建成功。验证覆盖空缓存首次查券、不受旧清单限制、精确代码查询、名称检索、参考数据变更、离线与未找到区分。真实 QA 的 STC 类调用及远端数据仍需在目标环境验证；本机测试使用 Source 替身，不能视为 QA 已通过。

## 2．内网配置步骤

1. 内网使用 `mvn -f iteration1/pom_stc.xml clean verify` 构建，部署 `target-stc` 中的服务包，参阅 [STC 构建说明](maven-stc.zh-CN.md)。本机兼容版本使用 `target-maven`，不要混用。
2. 复制 [内网适配配置模板](../config/adapters.integration.example.properties) 为部署机本地文件，例如 `D:/services/salestrading/adapters.local.properties`。不再配置债券白名单；按需配置字典覆盖及用户权限。
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

14:23 的后续日志已确认同一问题：`test01` 返回 `Code=0001`、`sid=null`、`userid=null`；随后 `userInfo` 收到字符串 `"null"`，返回 `Code=5001`。原 GW `CedaProxy.processLogin` 只检查成功码，并通过 `rhm.get("sid") + ""` 把缺失 SID 转成了这个字符串。不是报价创建字段校验失败，也不是已经证明存在有效用户会话。

当前 C# 平台和 API 调试器在登录阶段检查 SID：缺失、空白、JSON null 或字符串 `"null"` 均立即报错并断开连接，不显示已连接。共同身份适配器也拒绝这些 SID，不再向 LoginSvr 查询 `sid="null"`；有效 SID 缺少真实 `userid` 时返回明确权限错误。NotificationSvr 保留此权限错误，不再统一改写成同步未就绪。

QA 处理方式：优先使用一个正常 STC 业务账号。若必须使用 `test01`，先通过 STC 用户管理确认该用户在实际认证数据源中存在、启用且具有真实 `userid`；再从 LoginSvr 的 `control/LoginSvr/streamCfg.ini` 中 `DefaultUser` 列表移除它，保留其他配置项，重启 LoginSvr 并重新登录。所提供源码的该列表为 `gwcust;test01`，部署机以实际文件为准。仅添加 `DefaultUserSession` 不足以保证真实 `userid`：该特殊路径没有普通用户的数据库查询。本项目未擅自修改 QA LoginSvr、创建用户或分配用户 ID。

普通 STC 用户表模式下，可只读核对 `SELECT userid, username, enable FROM stc_tbook_users WHERE username = 'test01';`。CRM 登录模式则检查其实际认证数据源。验收应同时看到登录返回有效 SID，以及同一 SID 的 `userInfo` 返回 `Code=0001` 与真实 `userid`。

## 3．CRM 独立实现

现已提供内置 PostgreSQL 实现，可直接配置接入 `crm_org`、`crm_contact`、`crm_contact_org_rel`，无需另写插件。配置、接口样例和本机验证见 [CRM PostgreSQL 接入](crm-postgresql.zh-CN.md)。下面的独立 JAR 方案仍适用于以后其他来源的扩展。

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

真实 STC 环境可使用 C# 客户端的 **自动测试 → STC QA** 模式，选择 BaseSvr 支持且符合前置条件的债券及两个真实账号；每轮三条报价，覆盖四类提醒。配置、操作及边界见 [QA 功能自动测试](qa-autotest.zh-CN.md)。原有模拟账号测试不应直接用于 QA。

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
