# STC 与 CRM 可切换依赖适配

本机开发默认使用模拟数据；内网联调通过配置接入 STC LoginSvr、BaseSvr。QuoteSvr、NotificationSvr 的业务代码使用同一组接口。CRM 优先通过 CRMSvr 接入；也保留 PostgreSQL 直连和自定义适配器。

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
| CRM | `crm.mode=stc` | `StcCrmClient` 经 `SERVER.CRMSvr` 查询机构和联系人 |
| CRM | `crm.mode=custom` | 独立 JAR 实现 `CrmClient` |

三类依赖可以分别切换。真实模式失败时不会回退到模拟数据。业务仍使用 `ReferenceData` 与 `LoginSessionClient`，不直接依赖供应商字段。

### LoginSvrClient 源码复用

已有源码：`D:/workspace/GitHub/sc-app-stc/src/main/java/sc/app/stc/utils/LoginSvrClient.java`。

当前源码使用 `initialize(String, NetClientWrapper)`；平台打包的旧 STC JAR 使用 `initialize(String, CedaClientWrapper)`。因此本轮把已有源码编入 common 模块的 `ficc.common.stc` 包，避免旧 JAR 同名类覆盖，并将请求超时改为可配置。不是重新实现一套用户缓存或登录协议，也没有要求升级全部基础框架。移植文件与差异说明见 [源码说明](../cgws-ficc-saletrading-common/src/main/java/ficc/common/stc/README.zh-CN.md)。

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

本机与 QA 均使用 QA 原版 STC 依赖。桥接类绑定部署中的 STC 客户端；缺少所需类或方法时明确失败，不回退到另一套网络协议。

真实数据返回空且 STC 将债券记入 failedBonds 时，才视为未找到；未完成查询或离线报 NOT_READY。正常数据仍通过 StcBaseMapper 映射评级、发行人、条款及 STC 返回的估值，不伪造参考数据。现有业务视图尚未实现基础数据变化单独触发全量重算，验证评级变化时应重新查询。


## 2．内网配置步骤

1. 使用各工程主 `pom.xml`，按 parent-pom → common → 服务的顺序构建，部署 `target` 产物；见[Maven 构建指南](maven-jenkins.zh-CN.md)。本机与 QA 均使用 Java 8 和 QA 原版依赖。
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

桌面业务前端使用 STC 客户端；接口排障可使用[DPS API 调试器](api-debugger.zh-CN.md)。已移除原型的自动测试入口不适用于当前客户端。

### 内部同步与用户登录的边界

| 调用 | 主题/方式 | 用户会话 |
|---|---|---|
| NotificationSvr 初始化/恢复报价及分组 | `ficc.quote.internal.state.query`，请求 `requestId` 与可选 `limit/cursor`，返回 `Code/Msg/Data`，`Data` 含 `records/epoch/sequence/nextCursor/done` | 不发送用户 SID |
| NotificationSvr 初始化/恢复价格簿 | `ficc.price.internal.snapshot.query`，请求 `requestId`，返回 `Code/Msg/Data`，`Data` 为现有价格快照消息 | 不发送用户 SID |
| 后续内部推送 | `ficc.quote.events`、`ficc.price.snapshots` | STC 服务订阅，不执行用户登录 |
| 前端报价/价格/通知业务操作 | 原有业务请求和私有 report 主题 | 用户在 UI 输入账号密码，经 GW 登录；服务通过 LoginSvrClient 解析 SID 和 userid |

QuoteSvr 在 `handler/` 和 `control/QuoteSvr/resourceCfg.ini` 注册两个只读内部 Handler；不会给普通业务接口增加匿名访问。不要将这两个内部请求作为客户端业务入口；标准 GW 的路由与权限以部署配置为准。STC 底层可能把连接标识写入 `signalID`，即使调用方没有发送用户 SID，因此内部 Handler 不把该字段当成已登录用户，也不要求它为空。内部端口按 STC 可信服务网络部署，仅对受信服务开放。连接标识和 ZooKeeper 注册本身均不代表身份认证。

升级时同时更新 common、QuoteSvr、NotificationSvr 和 GW，并合并 QuoteSvr 新增的 Handler 配置。旧服务端没有这些主题，不能只替换 NotificationSvr。同步完成日志为 `NotificationSvr source synchronized at ...`。


## CRM 接入

```properties
crm.mode=stc
crm.serverKey=SERVER.CRMSvr
crm.requestTimeoutMillis=3000
```

CRMSvr 独立访问 CRM 数据库，消费方不需要 CRM 数据库账号。部署及 ParserSvr 镜像约定见[CRMSvr 部署](../cgws-ficc-saletrading-crmsvr/deployment/README.zh-CN.md)。PostgreSQL 直连模式见[CRM 数据与接口](crm-postgresql.zh-CN.md)；扩展其他来源见[自定义适配器](../extensions/crm-adapter/README.zh-CN.md)。

QA 使用服务内 `config/adapters.properties` 和共享 `$ATS_ROOT/control/DBPoolConfig.ini`，具体覆盖关系及部署文件见[QA 配置指南](jenkins-qa.zh-CN.md)。不要用旧 `runtime` 中的本机属性文件覆盖 QA 包。

## 登录排障

业务账号登录必须取得有效 SID，随后能够解析出真实 STC `userid`。只有成功码、但 SID 或 userid 为空的默认用户登录不满足业务操作条件。用户账号密码由客户端输入；NotificationSvr 内部同步不需要用户密码。

以实际收发日志确认 GW、LoginSvr 与业务服务使用同一会话。不要把连接标识当成用户 SID，也不要仅凭初始化成功判断远端服务可用。
