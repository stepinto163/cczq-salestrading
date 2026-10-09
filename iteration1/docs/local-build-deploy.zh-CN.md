# 本机构建与部署

在仓库根目录执行。Java 使用各工程的主 `pom.xml` 和 Maven；STC 客户端使用现有 MSBuild 脚本。包装脚本统一命令、构建产物和启停流程，不替代 Jenkins。

```powershell
cd D:\workspace\greatewall\ficc\ficc

.\build quotesvr                 # 编译、运行 Maven 测试、打包；不停止服务
.\deploy quotesvr                # 先构建，成功后替换二进制并重启
.\deploy quotesvr -NoBuild       # 部署最近一次成功打包的产物

.\build parsersvr                # 服务和 Java 引擎统一构建
.\build client                   # 独立目录编译 STC 客户端
.\deploy client -NoBuild         # 替换客户端二进制并启动
.\deploy notificationsvr
.\build all                      # 构建五个业务服务及客户端
.\deploy all -NoBuild            # 依次部署五个业务服务及客户端
.\deploy quotesvr -Plan          # 只查看目标和执行计划
```

入口为 `build.cmd` / `deploy.cmd`，内部调用 `iteration1/scripts/product.ps1`。CMD 入口已设置本次 PowerShell 进程的执行策略，不需要修改系统执行策略。

完整本机环境的 `qa_env\ats_root\scripts` 也提供同名入口，与 `start.cmd`、`show.cmd` 放在一起。它们复用同一构建脚本，并固定选择所在环境的 `ATS_ROOT`，不受当前工作目录或全局 `ATS_ROOT` 影响：

```powershell
cd D:\workspace\greatewall\ficc\qa_env\ats_root\scripts
.\build.cmd quo -Plan
.\deploy.cmd NOT -Plan
.\build.cmd all
.\deploy.cmd all -NoBuild
.\build.cmd client
.\deploy.cmd client -NoBuild
```

目标支持大小写不敏感的唯一前缀，参数及覆盖范围与仓库入口相同。`-Plan` 只检查执行计划，不构建或重启。此处入口已绑定本环境；需要其他环境时使用仓库入口的 `-AtsRoot` 参数。`qa_env` 为本地部署目录，不随仓库提交。

## 目标和依赖

| 目标 | 构建内容 | 部署位置 |
|---|---|---|
| `parent-pom` | 安装父 POM | 无进程，不支持 deploy |
| `common` | parent-pom → common | 无进程；部署使用它的服务 |
| `quotesvr` | parent-pom → common → quotesvr | `ATS_ROOT/stc/QuoteSvr` |
| `notificationsvr` | parent-pom → common → notificationsvr | `ATS_ROOT/stc/NotificationSvr` |
| `crmsvr` | parent-pom → common → crmsvr | `ATS_ROOT/stc/CRMSvr` |
| `qtintakesvr` | parent-pom → common → qtintakesvr | `ATS_ROOT/stc/QtIntakeSvr` |
| `parsersvr` | parent-pom → common → parsersvr（含 Java 引擎） | `ATS_ROOT/stc/ParserSvr` |
| `client` | netapi、Common、Frame、SalesTrading.Plugin、STC | `iteration1/stc-client/stc-client/runtime/native-client` |
| `all` | 五个业务服务及客户端 | 按上表依次部署 |

Java 执行 `mvn clean install`，不跳过测试。先使用 `JAVA_HOME`，未设置时尝试仓库 runtime 中的 JDK 8；主 POM 以 Java 8 为编译目标。Maven 优先使用 PATH，其次使用 runtime 中的 Maven。客户端需要 MSBuild、WPF 和 .NET Framework 4.7.2 开发工具。

本入口不编译或重启外部 STC 底座：GW、LoginSvr、BaseSvr、ZooKeeper、PostgreSQL 等。它们仍由环境启停脚本管理。`deploy all` 会启动 QtIntakeSvr，使用该命令前应确认当前需要建立 QTrade 连接。

## 环境选择

`ATS_ROOT` 的选择顺序：`-AtsRoot` 参数 → 当前环境变量 `ATS_ROOT` → 仓库同级 `qa_env/ats_root`。

```powershell
.\deploy quotesvr -AtsRoot D:\workspace\greatewall\ficc\qa_env\ats_root
.\build quotesvr -SettingsFile C:\path\settings.xml
.\deploy client -NoBuild -ClientConfig C:\path\STC.exe.config
```

默认 Maven settings 为 `iteration1/build/qa-maven/settings-local.xml`，依赖准备见[本机 QA Maven 依赖](../build/qa-maven/README.zh-CN.md)。默认客户端配置为 ATS_ROOT 上级目录的 `stc-client/STC.exe.config`。

服务必须已安装在目标环境并在 `ATS_ROOT/scripts/serverlist.csv` 中启用。部署调用该环境的 `scripts/service.ps1`；客户端调用 `iteration1/scripts/manage-stc-client.ps1`。这是本机部署命令，不通过 SSH 部署远程 QA，也不执行 Maven 的 `deploy` 仓库发布目标。

## 部署行为

1. 完成构建和包检查后才停止目标进程；构建失败不影响正在运行的版本。
2. 校验产物清单和 SHA-256，完整替换 `javalib` 或客户端 `bin`，避免旧依赖混入新版本。
3. 保留服务的 `control`、`config`、数据库配置和运行数据；不运行 SQL。客户端启动时应用选定的 `STC.exe.config`。
4. 服务旧二进制保存在目标目录的 `.deploy/<本次编号>/previous`；客户端保存在安装目录外的同级 `native-client-deployment-backups/<本次编号>/previous`，避免 STC 递归扫描到旧插件 DLL。既有客户端 `.deploy` 会在停机后迁出。新版本启动失败时，恢复旧二进制并尝试启动；命令仍返回失败状态。
5. `all` 先完成全部构建和包校验，再逐个部署。某个服务失败时只恢复该服务，已成功部署的服务不会自动回退。

启动检查只确认服务端口由目标进程监听，或客户端进程存活，不代表登录、基础资料或业务端到端检查通过。客户端有未保存内容、无法正常退出时，部署失败，不强制杀进程。不要同时用其他脚本启停同一目标。

## 产物与日志

- 每次执行：`iteration1/runtime/builds/<本次编号>/build.log` 或 `deploy.log`。
- 构建包：同目录下 `<目标>/payload` 和 `package.json`；客户端详细编译日志也在 payload 中。
- 最近成功包指针：`iteration1/runtime/builds/latest-<目标>.txt`。
- 服务运行日志：目标 `ATS_ROOT/log`，沿用环境原来的日志配置。

`-NoBuild` 使用最近成功包，不自动检测源代码是否更新；修改代码后应先 build，或直接 deploy。产物及备份为本机文件，不提交 Git；脚本不会自动清理回退所需文件。

部署回退的隔离检查（需先成功 build quotesvr）：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-product-build.ps1
```

该检查使用临时目录与假的启停脚本，覆盖启动失败恢复、停止失败保护、配置保留和旧依赖替换，不停止真实服务。

## IntelliJ 本机 Run / Debug

五个业务服务都提交了完整的源码启动配置和 `.run/<服务名> Local.run.xml`，不再要求手动复制 example 文件。parent/common 是 Maven 库，不是独立进程。

1. 在 IntelliJ 单独打开所需服务的 `pom.xml`，导入 Maven 依赖，Project SDK 选择 Java 8。
2. 选择 `QuoteSvr Local`、`NotificationSvr Local`、`CRMSvr Local`、`QtIntakeSvr Local` 或 `ParserSvr Local`。
3. 运行配置默认工作目录为工程根目录，`ATS_ROOT=$PROJECT_DIR$/../../../qa_env/ats_root`；若独立 checkout 到别处，只需将该环境变量指向已准备好的本机 STC 根目录。
4. 先停止本机原来运行的**同名服务**，再按 Run / Debug；保留它依赖的 PostgreSQL、ZooKeeper、LoginSvr、BaseSvr、CRMSvr 等服务。端口和数据库锁仍按共享配置使用，不启动第二份同名实例。

| 服务 | 本机业务配置 | 依赖 |
|---|---|---|
| QuoteSvr | `config/service.properties`、`config/adapters.properties` | FICC_SALESTRADING / ficc_local_stc；LoginSvr、BaseSvr、CRMSvr |
| NotificationSvr | 同上 | 同一数据源/schema；QuoteSvr、LoginSvr、BaseSvr、CRMSvr |
| CRMSvr | `config/crmsvr.properties` | FICC_CRM / public |
| QtIntakeSvr | `config/service.properties`、`config/qtrade-fix.cfg` | MYSQL0 用户映射、共享 QtIntakeSvrAuth.properties、QuoteSvr、QTrade FIX 网络 |
| ParserSvr | `control/ParserSvr`、`config` | MYSQL0、BaseSvr、CRMSvr；共享本机 ParserSvr 字典及原生对照库 |

`adapter.properties` 为空表示“不加载第二个配置文件”；例如 CRMSvr 使用单个完整配置文件即可。QuoteSvr/NotificationSvr 需要明确加载 STC adapters，现已填好。

ParserSvr 的运行配置设置 `-Dhanlp.properties=./config/hanlp.properties`，该文件引用已部署的本机字典（无需把约 111 MB 字典复制进 Git）。Java 引擎仍为默认；原生库路径指向本机已部署 ParserSvr 的 javalib。独立 checkout 时相应修改这两个资源路径。入口未设置此参数时维持部署包原有的 classpath 配置方式。

共享的 ATSConfig.ini、DBPoolConfig.ini、server.lic 和 QtIntakeSvrAuth.properties 由本机环境管理。Jenkins 继续使用 Salt 的 QA 配置生成，不使用源码中的本机 schema。ParserSvr 的 `config/database.xml` 仅是保留原生引擎接口所需的本机适配配置，Java 引擎从 MYSQL0 加载。

配置检查（不启动服务、不连接 FIX、不执行业务写入）：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-local-java-config.ps1
```
