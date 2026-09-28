# QA Jenkins 打包与共享数据库配置

本轮只配置 `QA.APP`，没有 DEV 环境。QuoteSvr、NotificationSvr 使用 `sslib@main` 的 `building.buildJavaService`，已核对 QA 库提交 `43d4f0642e2b7d8e473ed78ffebf7f1c607b6e4c`。parent/common 保留原来的 artifact 流程。共享库仍跟随 main，并非锁定该提交。

## Jenkins 打包

每个服务工程应位于 Jenkins 工作区根目录，入口为该工程的 `Jenkinsfile` 和 `pom.xml`。如果从当前总仓库检出，不能仅指定子目录 Jenkinsfile 就直接运行：共享库会清理工作区并重新 `checkout scm`，需要先按独立工程配置检出。

构建顺序：先发布 parent-pom 和 common，再构建两个服务。使用 `jdk8` 节点；内网 Maven 配置由 Jenkins 提供。

Maven 输出 `target/bin`、`target/javalib` 和服务 JAR。Jenkins 读取 `salt-formula/<工程名>/config.yaml`，将 `common` 与 `QA.APP` 合并后渲染 `files`，生成 `*-QA.APP-<分支>.tar.gz`。

QA 的完整服务配置直接保存在 `salt-formula/<工程名>/files/control`：服务入口 INI、`resource.xml`、`resourceCfg.ini`、`streams.xml`、`streamCfg.ini`、`log4j.ini` 均可在源码中查看。打包不再从本机 `control` 目录生成这些文件；本机 control 配置与 QA 模板分别维护。

QA 部署使用 Jenkins 中名为 `10.130.21.53` 的 SSH 配置，登录账号必须为 `sumscope`（账号在 Jenkins 的 SSH 配置中设置，不是 Jenkinsfile 的自定义字段）。解压目标为 `/opt/sumscope/stc`，包内服务目录分别是 `QuoteSvr`、`NotificationSvr`，最终路径如下：

- `/opt/sumscope/stc/QuoteSvr`
- `/opt/sumscope/stc/NotificationSvr`

部署前加载 `~/.bash_profile`，进入 `~/scripts` 执行 `./stop QuoteSvr` 或 `./stop NotificationSvr`；上传解压后执行对应的 `./start`。首次部署前由 QA 将两个服务加入既有管理脚本，确认脚本使用上述工作目录和 JDK 8。部署不会覆盖 `/opt/sumscope/control`，也不会删除现有服务目录、日志或许可证。共享库使用覆盖解压，升级版本时需在服务停止后清理已经不用的旧 JAR，避免新旧版本同时进入 classpath。

QA 共享库不接受应用分支名中的 `/`。需要部署时使用例如 `rel_0.1.0` 的应用分支，它会进入 QA/release 流程并执行 `deployInfo.QA`；其他分支仍会打出 QA.APP 包，但不会部署，因为没有配置 DEV 部署目标。共享库可能要求人工确认。包内环境来自 `QA.APP`，与库自身的 main 分支不是同一概念。

## 目录与配置归属

```text
$ATS_ROOT/control/
  ATSConfig.ini                 # 既有 STC 注册中心与服务配置
  DBPoolConfig.ini              # 所有销售交易进程共享数据库配置

QuoteSvr/                       # NotificationSvr 结构相同
  bin/start
  javalib/*.jar
  control/QuoteSvr.ini
  control/QuoteSvr/...
  config/service.properties
  config/adapters.properties
```

`service.properties` 只选择数据源和业务 schema：

```properties
dbpool.cfg=$ATS_ROOT/control/DBPoolConfig.ini
db.sourceName=FICC_SALESTRADING
db.schema=ficc_salestrading2
```

`adapters.properties` 选择真实 STC 登录、基础资料和 CRM，并指定 `crm.sourceName=FICC_CRM`、`crm.schema=public`。不再包含未使用的 `identity.serviceUsername`、`identity.servicePasswordEnv` 等设置，也不设置已废弃的 `base.securityIds`。

数据库 URL、账号、密码只写入共享 `DBPoolConfig.ini`，不放进 Git、Jenkins 环境模板或业务服务包。`FICC_SALESTRADING` 指向 `ficc_salestrading` 数据库；`FICC_CRM` 指向 `ficc_ibor` 数据库。两个业务服务通过不同表前缀共用 `ficc_salestrading2` schema。

## 对既有 STC 的影响边界

保留 `MYSQL0` 和全部既有数据源定义，不修改它们的 URL、账号、密码、池参数或 STC 各服务的默认数据源。

已确认 QA 原配置只有索引 0 的 `MYSQL0`，原来的 `DBPOOL.DBCount=4` 与实际条目不一致。保留 `MYSQL0`，在索引 1、2 分别追加 `FICC_SALESTRADING`、`FICC_CRM`，将 `DBPOOL.DBCount` 设为 3。**示例片段不是完整配置文件，禁止直接覆盖原文件。** 先备份，共享文件继续由 QA 运维维护；服务升级不覆盖它。

数据源填写参考放在源码 `deployment/DBPoolConfig.salestrading.example.ini`，只供首次配置时查阅，不会被程序读取，也不打入服务包。运行时唯一数据库配置来源是共享 `$ATS_ROOT/control/DBPoolConfig.ini`。

当前改造统一的是连接配置。销售交易仍保留现有 JDBC 连接生命周期，不迁移整个业务存储到连接池；尤其保留用于 PostgreSQL 会话锁的独立长连接。读取配置复用 STC `PropertiesHelper`，并显式关闭文件流。缺少 ATS_ROOT、文件、指定数据源或存在重名时启动失败，不回退到 MYSQL0 或模拟环境。日志记录配置路径和数据源名，不记录密码。

## 启动

注册中心、服务地址和端口由 QA 运维在全局 `$ATS_ROOT/control/ATSConfig.ini` 中维护；服务包不包含 `ATSConfig.ini` 或 `qa-services.ini`，`config.yaml` 也不再重复定义这些字段。保持原有 `SERVER.LoginSvr`、`SERVER.BaseSvr` 等条目，并由 QA 配置新增的 `SERVER.QuoteSvr`、`SERVER.NotificationSvr`。

```bash
export ATS_ROOT=/opt/sumscope
export JAVA_HOME=/path/to/jdk8
cd /opt/sumscope/stc/QuoteSvr
mkdir -p "$ATS_ROOT/log"
nohup bash bin/start > "$ATS_ROOT/log/QuoteSvr-console.out" 2>&1 &
```

NotificationSvr 在自己的目录执行同样命令。启动脚本保留共享 ATS_ROOT，不将其改成服务目录。先启动 QuoteSvr，再启动 NotificationSvr。确认 CONFIGURATION、DATABASE、IDENTITY、BASE_DATA、CRM 等日志；远端连接和业务可用性仍需 QA 实测。

## QA 日志

沿用 STC 的 `MDTDailyRollingFileAppender`，在标准工作目录 `$ATS_ROOT/stc/<服务名>` 下使用 `../../log`。例如：

| 用途 | QuoteSvr 文件 | NotificationSvr 文件 |
|---|---|---|
| 运行日志 | QuoteSvr_yyyyMMdd.log | NotificationSvr_yyyyMMdd.log |
| 启动及组件初始化 | QuoteSvr-startup_yyyyMMdd.log | NotificationSvr-startup_yyyyMMdd.log |
| DPS / 适配器调用诊断 | QuoteSvr-dps_yyyyMMdd.log | NotificationSvr-dps_yyyyMMdd.log |

启动 INFO、错误及异常栈同时写控制台与运行日志；初始化日志另外写 startup 文件。客户端适配诊断类别保持 DEBUG，同时挂载 stdout、运行文件和 dps appender；控制台 appender 阈值为 INFO。业务 DPS 类别单独写 dps 文件。应按实际类别与 appender 检查正文去向，不能认为所有详细消息都显示在控制台。切换 DEBUG 可在各类别的 `log4j.logger` 中调整。此变更只调整日志配置及路由，不增加新的 DPS 消息采集点。

手工启动 NotificationSvr 时，控制台重定向文件改为 `NotificationSvr-console.out`。控制台重定向文件由启动脚本或运维管理，不属于 Log4j 每日滚动文件。本机 control 配置使用服务内 logs 目录。
