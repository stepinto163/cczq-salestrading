# FICC Java 服务框架替换：第一轮约束与设计

## 目标与边界

在 `feature/ficc-lightweight-runtime` 分支实现源码可控、依赖明确的轻量框架，替换现有 STC/BBox 基础设施。第一轮以兼容为目标，不同时重构业务或配置。

迁移范围为所有 FICC Java 进程：QuoteSvr、NotificationSvr、ParserSvr、CRMSvr、QtIntakeSvr。ParserSvr 服务宿主与 `cgws-ficc-parser` 解析库分别核对；仅验证解析库不能算 ParserSvr 迁移完成。已有 GW、LoginSvr、BaseSvr 等 STC 外部服务保持现状，作为互通验证对端。

- FICC 服务 Java 源码、入口、协议模型和业务行为保持不变；通过 Maven 依赖切换完成迁移。
- XML/INI 的结构、语法、类名、键名、默认值、引用、包含规则、相对路径和覆盖优先级保持兼容。
- DPS 底层库及线协议不变；保留 GW/STC 服务互通、会话、请求响应、订阅快照与推送行为。
- 保持 Java 8、Windows/Linux、ATS_ROOT、目录布局、启停命令和 Jenkins 部署约定。
- 不靠打包合并掩盖依赖；逐项说明保留、替换、移除的 JAR，不同时加载新旧同名框架类。
- 历史 ATS 源码仅供参考，当前部署 JAR 和实际配置作为兼容核对基线。
- 任何需要修改业务源码或现有配置的情况，作为兼容缺口记录并与用户讨论，不静默扩大范围。

## 架构

现有服务及配置 → 兼容 API → 配置解析、组件容器、生命周期和流引擎 → DPS/服务发现适配 → 保留的底层库。

| Maven 模块 | 职责 |
| --- | --- |
| cgws-ficc-framework-core | 配置与组件描述、生命周期管理、流连接、统一诊断 |
| cgws-ficc-framework-dps | 现有 DPS 传输和服务发现的薄适配 |
| cgws-ficc-framework-compat | 服务引用的原包名、类名、接口和方法，委托新实现 |

配置声明类的构造允许反射；正常请求路径使用直接调用。保留资源与流的外部模型，不新建通用 DI 系统，不擅自增加队列、重试策略或改变并发/顺序语义。

启动状态区分初始化、监听、注册、业务就绪。启动失败记录异常并清理已创建组件；关闭按依赖逆序释放。会话失效、注册恢复、订阅恢复必须有明确测试。

## 实施与验收

1. 清点服务实际引用的框架/API，以及部署 JAR 和配置，记录版本和 SHA-256。
2. 先实现配置、组件和生命周期兼容层，使用小规模隔离测试核对行为。
3. 接入真实 DPS，选一个服务验证请求、订阅、推送及故障恢复，然后逐个迁移。
4. 对原服务进行不改源码/配置的编译测试、隔离启动和协议测试，再评估部署替换。
5. 对比依赖清单与源码/配置差异；仅测试实际覆盖的行为，不把编译成功称为端到端通过。

性能测试需单独授权；本轮功能测试使用受限内存、超时和隔离端口，不启动 FIX 外部连接，不改变用户正在运行的环境。

## 当前状态（2026-10-08）

工作分支：`feature/ficc-lightweight-runtime`，基于原开发分支提交 `225b6cb`。本分支的父 POM 已默认使用新框架；用户日常运行目录未替换，未发布到 QA。

### 已完成的迁移

- QuoteSvr、NotificationSvr、CRMSvr、QtIntakeSvr 改由父 POM 引入新框架，原业务 Java 源码及 XML/INI 不改。
- ParserSvr 服务宿主纳入 `cgws-ficc-parsersvr`，与 `cgws-ficc-parser` 解析库分开。导入的业务源码、资源和第三方分析器用 `source-baseline.json` 校验；只调整 POM，入口仍为 `ParserSvr`。
- 框架分为 core、dps、compat。保留原包名只是兼容服务引用；这些类已是本仓库源码，不再由 `sc-app-stc` 等旧框架 JAR 提供。
- DPS 的 `gateway-api` 使用部署基线的原始二进制，验证脚本逐服务校验 SHA-256。请求、快照、订阅、推送和 gzip GW 互通使用原 SDK，不重写编解码。
- ZooKeeper 使用原 `/MDTService` 路径、服务名、节点格式。启动先发布 `Stat:0`，应用初始化及监听成功后写 `Stat:1`；测试覆盖 active 节点删除恢复、会话过期恢复、订阅恢复及离线取消订阅。
- INFO 组件启动日志、失败堆栈、异常传播、失败清理、控制台和文件输出均有测试。保留 `log4j.async/thread/size/writeTime` 配置；异步日志队列满时由调用线程写入，避免旧实现静默丢日志。
- `build-server.ps1` 先构建框架；`build/deploy parsersvr` 纳入统一脚本，部署保留环境配置和原生库资源。未实际运行覆盖式部署。

### 依赖审计

各服务运行 classpath 均不包含 `sc-app-stc`、`sc-app-common`、`ats-bbox-framework`、`ats-foundation-utilities`、`ats-app-ym-foundation`。

| 服务 | 旧部署 javalib JAR 数 | 新运行依赖 JAR 数 |
| --- | ---: | ---: |
| QuoteSvr | 125 | 39 |
| NotificationSvr | 138 | 39 |
| CRMSvr | 139 | 39 |
| QtIntakeSvr | 127 | 41 |
| ParserSvr | 147 | 57 |

新应用 JAR 单独计算。旧目录可能含历史冗余版本，数量对比用于检查部署内容，不等于性能提升。完整列表见 [dependency-audit.json](../cgws-ficc-framework/dependency-audit.json)。

保留第三方 DPS、Netty、ZooKeeper、数据库驱动/连接池、JSON、日志，以及公开方法签名依赖的 `ats-message` / `ceda-api`；后者仅保留消息对象和序列化兼容，不启用旧 CEDA 传输。ParserSvr 继续保留原 native/shadow SDK，不将解析引擎替换混入框架迁移。

所恢复的 STC 客户端/DTO/工具源码记录原 JAR、类文件和维护后源码 SHA-256，见 [recovered-support.json](../cgws-ficc-framework/recovered-support.json)。它们是兼容实现，不应被误称为重新设计过的业务模型。

### 实测与边界

验证环境：Windows 11、Temurin JDK 8u504。本轮未运行负载或性能实验。

| 检查 | 结果 |
| --- | --- |
| 框架配置/生命周期/日志/DPS/发现测试 | 30 通过，无跳过；包含部署 BBox 二进制对照、实际 loopback TCP、隔离 ZooKeeper 及 standby 接管 |
| common 原测试 | 60 通过、1 跳过 |
| CRMSvr 原测试 | 26 通过、1 跳过 |
| QtIntakeSvr 原测试 | 34 通过 |
| ParserSvr 宿主原测试 | 13 通过 |
| 五个服务编译、打包及依赖检查 | 通过；QuoteSvr/NotificationSvr 没有 JUnit 测试，不能将编译计为业务测试 |
| 隔离真实进程链路 | 标准 GW/LoginSvr 登录；CRM 查询；两行报价解析；GW 路由及订阅；报价创建、确认、业务推送；行情查询；MY_BEST 通知推送/查询；loopback FIX → QtIntakeSvr → QuoteSvr → GW 查询，均通过 |
| 源码和配置保护 | 原服务源码/配置未修改；ParserSvr 导入文件哈希一致；底层 DPS 二进制哈希一致 |

运行报告保存在 `iteration1/runtime/framework-checks/<时间>/report.json` 和 `iteration1/runtime/framework-live/<时间>/`，不提交环境口令、许可证或完整运行日志。命令见 [框架 README](../cgws-ficc-framework/README.zh-CN.md)。

隔离测试复制本机配置，使用独立服务注册名、329xx 端口及新数据库 schema；只关闭测试自己启动的进程。原 LoginSvr/BaseSvr 进程存在但注册丢失，因此测试另起标准 LoginSvr/GW，并让测试配置直连原 BaseSvr 的本机 DPS 端口。没有修改用户原注册目录或服务。

### 尚未作为完成项的内容

- 桌面验收使用 WPF 自动化宿主加载正式 STC 应用及控件，通过 UI 事件执行业务操作；并非逐像素鼠标/键盘驱动。实际测试范围见下文。
- Linux 启动、QA 部署、Jenkins 远程发布及性能没有实测。发布前需先将新框架 reactor 的三个模块发布到内部 Maven 仓库，再构建父 POM/common/服务。
- 支持当前 DPS + ZK active/standby 注册模式。旧 DB cluster、管理 GUI、monitor/encryption 扩展未完成；不能宣称可直接替换任意历史 STC 服务。
- standby 是冷备接管，不是内存状态复制；失去租约的原主实例停止服务，不在同一进程里自动重建所有业务单例。重启后可再次作为备机参与竞争，不抢占当前主机。
- ParserSvr 的 Java 主引擎通过。原生 shadow 分支仍遇到本地数据库缺少 `bond_type_selective`，不计为 native 引擎验证通过；未修改业务源码掩盖该问题。
- 首轮保留的兼容辅助源码需要继续审阅和收敛。不得仅凭本轮检查将所有历史配置语义、未覆盖的客户端方法或故障组合标记为完全等价。



## 桌面 UI 与 standby 补充验收（2026-10-08）

### 主备行为

沿用 `SERVER.<服务>.RegType=2`，以及原 `/MDTService/<ServiceName>/<ServiceName>` 固定临时节点。两个实例使用相同 ServiceName、注册中心和持久化数据，各自使用自己的监听地址。没有新增选举节点格式、配置键、轮询间隔或应用重试次数。

1. 已有主实例时，备实例输出 `STANDBY_WAITING`，只等待 ZooKeeper 子节点变化；不初始化业务资源、不打开 DPS 监听、不占用数据库写入锁、不连接 QTrade FIX。
2. 固定临时节点释放后，等待实例竞争原子创建；只有一个成功，其他实例继续等待。等待期间自身 ZooKeeper 会话过期后可以恢复等待。
3. 获得节点后先发布 `Stat:0`，初始化资源并监听，成功后才写 `Stat:1`。初始化失败清理资源并释放自己的节点，下一备机才能继续竞争。
4. 服务中的主实例发生 ZooKeeper 断连、会话过期或节点丢失时，被标记为不可继续使用原租约，并在独立生命周期线程停止业务、关闭注册会话。ZooKeeper 事件线程不运行长时间的业务启停。
5. 正常停机先停止业务再关闭注册会话；进程被强杀时，备机必须等待 ZooKeeper 判定原会话过期。保留原会话超时设置，未将其擅自缩短。GW 收到 `Stat:1` 后还有自己的重连过程；监听成功不等于 GW 路由已经可用。

这是基于 ZooKeeper 租约的进程切换，未增加写入端 fencing token。进程长时间暂停/网络分区的严格外部写入互斥，仍取决于业务存储锁和外部服务自身约束；不把冷备机制描述为所有故障下的无缝切换。

### 验证结果

- 注册层 8 项测试覆盖 active 节点恢复、就绪发布、standby 占用不抢占、主机退出后接管、多备机竞争、等待备机会话过期、主机失租约停止、关闭等待实例解除阻塞。
- 桌面 17 个现有用例全部通过，逐例清理通过，保存步骤截图、事件、JUnit 和 JSON 报告。范围包括手工报价生命周期、精确识别及 CRM、报价推送、T+0/T+1 修改、错误恢复、报价/行情筛选、多行和分号解析、选择及工具栏、过期存单警告、提醒已读。
- 强杀隔离 QuoteSvr 和 NotificationSvr 主进程后，两个备机自动接管；原 GW 不重启，重新发现后报价、行情、通知及 FIX intake 协议链路通过。
- 接管后重新登录桌面，重跑报价完整生命周期及提醒已读，两例通过。未验证一个始终打开的旧桌面会话自动恢复所有内存视图；不将重新登录后的验证等同于无感恢复。
- 桌面截图仍能看到原有“模糊报价服务尚未接入”提示，来源是现有客户端明确抛出的未实现分支，不属于新框架回归，也不计入已支持的业务。

完整桌面及接管报告：`iteration1/runtime/framework-live/20261008-000752/`，其中 `ui-reports/summary.json` 为 17 例，`ui-after-takeover/summary.json` 为 2 例。所有测试使用独立注册名、端口和 schema，未替换用户日常服务。

控制脚本及持久化恢复补验报告：`iteration1/runtime/framework-live/20261008-001404/report.json`。两个真实等待备进程均被 start 识别为 `STANDBY`；强杀主进程后，备机恢复的已确认报价 ID、revision、status、price、quantity 与切换前一致。此次从杀主到备机监听，QuoteSvr 约 35 秒、NotificationSvr 约 37 秒，包含现有 ZooKeeper 会话判定时间；GW 路由可用另行用实际只读请求确认，未将监听耗时当作端到端切换耗时。

### 本机控制脚本

原 `qa_env/ats_root/scripts/service.ps1` 已转发到仓库内 `iteration1/scripts/manage-services.ps1`，原 start/stop/show 命令不变。识别到本次进程的新鲜等待日志时，start 返回 `STANDBY`、show 显示 `STANDBY / WAITING`，不再因为没有监听端口而在 90 秒后杀掉健康备机。旧日志、已开始接管或失败日志不会当作健康待机。

`test-service-standby-state.ps1` 覆盖等待、旧日志、接管及失败状态。`test-framework-live.py --standby-test` 还会针对真实备进程调用该管理脚本，确认它不会启动重复实例或杀掉等待实例。
