# FICC 第一轮功能平台

已运行并验证：**C# 平台 / 独立 DPS 调试器 → STC GW → STC/BBox QuoteSvr、NotificationSvr → PostgreSQL**。代码位于本目录；用户提供的基础库原仓库未修改，也未回退分支。

日常操作请参阅 [本机启动、测试与停止指南](docs/local-startup.zh-CN.md)，包含可直接复制的 PowerShell 命令。

连接内网 LoginSvr、BaseSvr，以及独立开发 CRM 适配 JAR，参阅 [可切换依赖适配指南](docs/stc-adapters.zh-CN.md)。默认仍使用模拟数据。

前端筛选延迟测试见 [独立性能测试说明及首轮结果](docs/filter-performance.zh-CN.md)。支持手动选择 500 至 50,000 条服务端报价，默认只运行 500 条，使用外部看门狗和虚拟网格。

Java 后端使用独立的 [STC 父 POM](cgws-ficc-saletrading-parent-pom/pom.xml) 和三个 cgws-ficc-saletrading 模块，要求 JDK 8。各项目的 pom.xml / Jenkinsfile 用于正式 STC 构建，pom_compat.xml 用于本机兼容构建。详见 [Maven 与 Jenkins 构建](docs/maven-jenkins.zh-CN.md)。

迁移至 QA 空库时，使用 [建表 SQL](sql/001-create-tables.sql)；执行方法及数据库配置见 [QA 环境数据库初始化](docs/qa-database.zh-CN.md)。

开发约定见 [STC 服务开发原则与实现约定](../docs/stc-development-principles.zh-CN.md)。标准启动、公共服务 Client 及 ZooKeeper 发现已接入，配置与验证见 [本机 ZooKeeper 与 STC 服务发现](docs/zookeeper-discovery.zh-CN.md)。

行情中心内“关注债券”和“筛选分组”已提供创建、编辑、删除及应用行情的管理窗口，详见 [分组管理界面](docs/group-management.zh-CN.md)。当前客户端位于 `client/bin/Debug/net472`，包含 [报价确认时手动选择CRM对手](docs/counterparty-confirmation.zh-CN.md)及独立的 [多用户报价场景](docs/multi-user-scenario.zh-CN.md)。

## STC 项目结构

使用 `sc-app-stc-innersvr` 的框架机制，由 `ServerShell` 启动，资源、处理器和流通过 `control` 装配。

项目按业务服务独立目录与 JAR 组织，采用 `cgws-ficc-saletrading-xxxsvr` 命名：

| 模块 | 职责 | Java 包 |
|---|---|---|
| [cgws-ficc-saletrading-common](cgws-ficc-saletrading-common/README.zh-CN.md) | 共享 JSON/回复、筛选、数据库工具及基础资料模拟器 | `ficc.common` |
| [cgws-ficc-saletrading-quotesvr](cgws-ficc-saletrading-quotesvr/README.zh-CN.md) | 报价生命周期、查询、行情；独立 Handler/Stream/control | `ficc.quote` |
| [cgws-ficc-saletrading-notificationsvr](cgws-ficc-saletrading-notificationsvr/README.zh-CN.md) | 提醒规则、查询、偏好、已读；独立 Handler/Stream/control | `ficc.notification` |

两个业务服务分别依赖 common，不互相加载对方的业务 JAR。每个模块的产物为 `target/<模块名>.jar`；启动脚本显式组装各自的 classpath。此次调整项目目录和产物名，既有 `ficc.*` DPS topic、`QuoteSvr`/`NotificationSvr` 路由名及数据库保持兼容。`GW/` 使用标准 STC GW 分发包，不包含 FICC 自定义类，不参与 Maven 构建。

```text
cgws-ficc-saletrading-quotesvr/
  control/
    ATSConfig.ini
    QuoteSvr.ini
    QuoteSvr/
      resource.xml           # QuoteResource、STC NetServerResource
      resourceCfg.ini        # 每个具体 Handler 与 topic 注册
      streams.xml            # QuoteStream → STC NetRtOutputStream
      streamCfg.ini          # 推送配置，不合并报价事件
      log4j.ini
  src/main/java/ficc/quote/
    Main.java                # STC/BBox 启动入口
    QuoteResource.java       # 生命周期、数据库、业务服务与模拟会话
    handler/                 # 具体 ContentHandler 子类
    stream/QuoteStream.java  # 业务事件输出、心跳
    QuoteService.java        # 报价生命周期及操作事务
    ViewService.java         # 报价与行情分别管理视图和推送
    PgStore.java             # PostgreSQL 持久化
```

[Handler 示例](cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/handler/QuoteCreateHandler.java)、[公共 Handler](cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/handler/QuoteHandler.java)、[业务 Stream](cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/stream/QuoteStream.java)、[Handler 配置](cgws-ficc-saletrading-quotesvr/control/QuoteSvr/resourceCfg.ini)、[Stream 配置](cgws-ficc-saletrading-quotesvr/control/QuoteSvr/streams.xml)。

注册 32 个 QuoteSvr 请求接口，另有两个基础数据模拟接口、三个行情快照处理器及登录模拟处理器。报价与行情使用不同模型、接口及推送 topic；普通报价订阅不产生快照，行情订阅使用 STC 原生订阅快照机制。

## 本机启动和 UI

在 `D:\workspace\greatewall\ficc\ficc` 执行；已有进程运行时不必重复启动：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/start-postgres.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/start-quotesvr.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/start-notificationsvr.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/start-gateway.ps1
```

`runtime/quotesvr.log` 出现 `QuoteSvr READY`，GW 启动完成后打开 UI：

```powershell
& .\iteration1\client\bin\Debug\net472\Ficc.SalesTradingClient.exe
```

选择 `trader1`，点击“连接 GW”。可试用“新建 → 确认 → 修改 → 撤出 → 恢复”；有效报价需先撤出再删除，待确认报价可直接删除。另有评级与状态筛选、批量新建两条、历史、该债行情。界面通过真实 DPS 推送更新。

上方有 **自动测试** 和 **API 调试器** 按钮。主表分为 **报价管理、行情视图、报价提醒**：行情视图支持最优／全部／指定档数、债券及清算筛选，使用独立行情订阅。报价提醒提供四个开关、未读数、实时弹窗、已读操作和关联报价表。详见 [报价提醒实现](docs/notifications.zh-CN.md)、[DPS API 调试器](docs/api-debugger.zh-CN.md)。

| 进程 | 本地地址 | 说明 |
|---|---|---|
| PostgreSQL 16.15 | 127.0.0.1:55432 | 数据库 `ficc_quote`，平台 schema `ficc_salestrading` |
| QuoteSvr | 127.0.0.1:18680 | 独立 STC/BBox 进程 |
| NotificationSvr | 127.0.0.1:18690 | 独立 STC/BBox 进程，与 QuoteSvr 共用 `ficc_salestrading` schema |
| GW | 127.0.0.1:18600 | 使用提供的 GW 源码，独立进程 |

PostgreSQL 是本项目单独安装的实例，未删除或修改旧实例。数据库凭据保存在被忽略的 `runtime/*.local.properties`。STC 许可证仍由原 SDK 验证，许可证文件不提交。

模拟账户：`trader1` → U-001，`trader2` → U-002，`admin` → U-ADMIN；`intake`、`search` 为内部功能测试身份。模拟密码均为 `trader1`。业务操作者从登录会话取得；GW 拒绝订阅其他会话的私有 topic。

停止时先关闭 C# 窗口，再执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/stop-gateway.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/stop-notificationsvr.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/stop-quotesvr.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/stop-postgres.ps1
```

## 自动测试模式

无参数启动为交互 UI。点击顶部 **自动测试**，依次运行报价接口、提醒规则、报价界面、分组界面和CRM对手确认检查，显示实时进度、结果及日志入口；共创建 8 条测试报价、2 个临时分组，不同时运行测试子进程。每个子进程有 30 秒超时保护，可以点击“停止测试”。测试窗口完成后返回主界面，结果继续保留。

命令行仍支持 `--autotest <结果文件>`、`--autotest-notifications <结果文件>`、`--autotest-ui <结果文件>`。脚本方式：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-csharp-gateway.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-notifications.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-ui.ps1
```

最近结果位于 `runtime/csharp-gateway-smoke.json`、`runtime/ui-smoke.json`；界面捕获为 `runtime/ui-smoke.png`。退出码 0 表示通过，失败返回非零。测试是真实调用，不使用写死的接口回复。

2026-09-14 最近验证结果（基础库及 Java 业务检查沿用前一日结果）：

| 检查 | 结果 | 每次规模及边界 |
|---|---|---|
| Java 业务 + PostgreSQL | 34 项通过 | 5 条报价、独立测试 schema；事务、版本、权限、精度、档位、重建服务、部分 intake 批次恢复 |
| Java DPS → STC Handler/Stream | 16 项通过 | 1 条报价；原始字节回调、正常回调与异常回退，未经过外部 GW |
| C# DPS → GW → QuoteSvr → PostgreSQL | 39 项通过 | 3 条报价；全部 32 个请求 topic、报价推送、行情订阅快照、中文原文、私有 topic 权限 |
| NotificationSvr 经 C# / GW | 20 项通过 | 3 条报价；四类提醒、并列最优、他人报价变化、设置、权限、已读保留、持久事件 |
| WinForms 界面功能 | 18 项通过 | 1 条报价；报价、实时行情、提醒、已读及撤出移除；虚拟表格开启 |
| 独立 DPS 调试器 | 通过 | 3 个请求，0 条新报价；QuoteSvr、NotificationSvr、行情订阅快照与退订；47 个模板 |

测试代码：[业务](cgws-ficc-saletrading-quotesvr/src/test/java/ficc/quote/BusinessSmoke.java)、[Java DPS](compat/tests/StcTransportSmoke.java)、[C# 网关](client/src/GatewaySmoke.cs)、[界面](client/src/QuoteFormSmoke.cs)。这些是小规模功能检查，不代表每种参数组合均已覆盖，也不是性能测试。未运行万条报价或并发压力测试。

## 前端消息日志

### 行情视图的多债券显示（2026-09-14）

“全部债券”表示所有匹配且存在有效报价的债券，不是基础资料中的所有债券。新建报价为 `PENDING_CONFIRM`，必须在报价管理中选中该报价并点击“确认”，成为 `ACTIVE` 后才形成行情；待确认、已撤出和已删除的报价不进入行情。

左侧表格列出匹配债券，右侧展示选中债券的买卖盘。已增加说明及右侧债券代码，并修复切换债券时读取旧行、实时重排后选中债券发生跳变的问题：按债券代码保留选择，使用 `CurrentCellChanged` 更新明细。

专项功能检查 [QuoteFormPriceSmoke.cs](client/src/QuoteFormPriceSmoke.cs) 顺序创建两条临时报价，验证待确认排除、确认后多债券推送、初始查询、切换明细、更新重排、单债与全部债券筛选、撤出及虚拟表格，共 12 项通过；正常完成后删除测试自己创建的报价，不修改用户报价。结果为 `runtime/price-multibond-smoke.json`，截图为同名 PNG。独立进程看护上限 30 秒，不属于压力测试。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-price-ui.ps1
```

当前修复已合入常规构建 `client/bin/Debug/net472`，该版本同时包含分组管理界面。

UI 和自动测试共用 [DpsClient](client/src/DpsClient.cs) 与 [MessageLog](client/src/MessageLog.cs)。每次启动在 `runtime/client-logs/<时间-进程号>/` 创建独立日志目录；UI 的 DPS 消息页及测试结果 JSON 中可找到路径。

- UTF-8 JSONL：每行一个事件，包含时间、顺序号、客户端标识、账户、方向、消息类别、topic 和完整业务消息体。
- 记录请求、回复、推送（含行情快照与心跳）、登录调用/回复、订阅、显式订阅确认、退订、断开连接及异常。请求和回复中的 `requestId` 可用于关联。
- 订阅 DLL 返回布尔值时记作 `LOCAL/SDK_SUBSCRIBE_RESULT`，不会伪装成服务器原始回复；另记录实际 GW 确认消息。底层 SDK 未暴露的 TCP 帧、协议保活及原始退订确认不在此应用日志范围。
- 登录密码及认证口令脱敏；业务字段不截断。每 10 MB 新开一个文件，旧文件保留。每条记录立即刷新，便于异常退出后排查。
- UI 仅保留最近 200 条业务消息预览；文件记录不受这个显示上限影响。日志可能包含报价和联系人等业务数据，目录已在运行目录忽略规则内。

## 构建

本机已构建。停止 NotificationSvr 和 QuoteSvr 后重建三个模块（共享 JAR 在 Windows 下不可被运行中进程占用）：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/build-server.ps1
dotnet build iteration1/client/SalesTradingClient.csproj -m:1
dotnet build iteration1/api-debugger/DpsApiDebugger.csproj -m:1
```

`build-server.ps1 -Module quotesvr` 或 `-Module notificationsvr` 可构建公共库及指定服务；默认按 common → quotesvr → notificationsvr 顺序独立编译、打包。`test-business.ps1` 使用已构建的 QuoteSvr JAR，只编译测试源码，避免通过一次性混编掩盖模块依赖问题。

2026-09-14 模块拆分后，经 C# → GW 的报价/行情 39 项、通知 20 项功能检查通过，各使用 3 条报价顺序执行。数据库业务检查与模块隔离检查结果保存在 `runtime/business-smoke.json` 和 `runtime/module-isolation.json`。

基础库重建前还需停止 GW，以释放 Windows JAR 文件锁。顺序与兼容改动见 [基础库兼容记录](docs/foundation-compatibility.zh-CN.md)。

## 第一轮边界

- [SearchSvrClient](docs/search-dependency.zh-CN.md) 使用本地真实缓存和 PostgreSQL 历史查询；查询与订阅共用筛选逻辑。独立 SearchSvr、分布式扩展、高可用尚未实现。
- LoginSvr、BaseSvr 使用固定数据；筛选字典仅覆盖平台字段。
- CalSvr 尚未接入。行情支持直接可比较的收益率，含权债使用行权收益率；需要净价/全价换算时返回 `NOT_READY`。交割暂支持 T+0、T+1。
- PostgreSQL 保存报价、历史、事件、来源原文、幂等记录和个人分组。行情由已持久化的当前报价重建，暂不单独保存历史行情快照。
- 历史查询按“该日最后一次变更后的报价”返回，不补齐无变更日；基础属性及最新日估值来自当前模拟数据。这是平台限制，不是新的产品约定。
- Intake 暂实现新增及待确认报价更新，没有真实外部 QT 连接。
- UI 最多加载 200 行；QuoteSvr/GW Java 堆分别限制 256/192 MB，各限制两个可用处理器。断线后重新打开客户端；自动补流、服务端会话回收及真实认证需后续完成。
- NotificationSvr Java 堆限制为 160 MB，使用单个工作线程和容量 128 的输入队列；缺号或溢出后重新读取 QuoteSvr 状态及原生行情快照。通知服务是独立进程，未把通知规则塞入 QuoteSvr。

主项目 [API 规格](../docs/ficc-api-spec.zh-CN.md) 仍作为评审依据；此目录说明第一轮实现及其边界。
