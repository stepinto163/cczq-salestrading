# 组件启动日志约定

重要组件统一通过 Log4j 的 `ficc.startup` 输出到**控制台和服务工作目录的 `logs/startup.log`**。INFO 默认开启；与 `logs/dps-messages.log` 中的请求正文日志独立。启动文件为 UTF-8、5MB 滚动、3 个备份。

固定前缀 `COMPONENT` 后为 JSON，字段固定为 `service / component / state / mode / implementation / target / elapsedMillis`。失败增加 `errorType / reason`，随后输出异常堆栈、原因链及 suppressed 异常；凭据脱敏。

```text
INFO ficc.startup - COMPONENT {"service":"QuoteSvr","component":"CRM","state":"STARTING","mode":"postgresql","implementation":"PostgresCrmClient","target":"127.0.0.1:55432/ficc_ibor schema=public","elapsedMillis":0}
INFO ficc.startup - COMPONENT {"service":"QuoteSvr","component":"CRM","state":"INITIALIZED","mode":"postgresql","implementation":"PostgresCrmClient","target":"127.0.0.1:55432/ficc_ibor schema=public","elapsedMillis":80}
```

状态规则：

- `STARTING`：开始配置、构造或初始化组件。
- `INITIALIZED`：对应初始化步骤成功返回。数据库组件已建立连接；CRM PostgreSQL 初始化还检查了三张表的查询映射。STC 客户端的初始化成功仅表示本地设置和订阅注册完成，远端连接和数据同步仍由连接/同步日志确认。
- `FAILED`：配置或初始化失败，记录 ERROR 和堆栈并继续抛出异常，阻止服务正常启动；失败后不能再记录同一步骤 `INITIALIZED`。JAR 不兼容等 `LinkageError` 同样记录并传播。

本轮覆盖：

| 组件 | QuoteSvr | NotificationSvr |
|---|---|---|
| `CONFIGURATION`：配置来源、工作目录 | 是 | 是 |
| `DATABASE`：报价/通知持久化连接 | 是 | 是 |
| `IDENTITY`：登录适配器 | 是 | 是 |
| `CRM`：机构、联系人适配器 | 是 | 是 |
| `BASE_DATA`：债券资料适配器 | 是 | 是 |
| `QUOTE_SERVICE`：报价业务初始化 | 是 | — |
| `NOTIFICATION_SERVICE`：通知业务初始化 | — | 是 |
| `QUOTE_SOURCE`：内部报价/行情来源客户端初始化 | — | 是 |

基础框架的 GW、ZooKeeper、handler/stream 注册日志仍由 STC 框架输出，本轮不修改上游库。当前基础资料适配器按需查券，不把空缓存称为“全量资料已就绪”。

## 部署与排障

同时更新公共库、两个服务 JAR 及以下配置，然后重启服务：

```text
iteration1/cgws-ficc-saletrading-quotesvr/control/QuoteSvr/log4j.ini
iteration1/cgws-ficc-saletrading-notificationsvr/control/NotificationSvr/log4j.ini
```

搜索 `"component":"CRM"` 即可看到实际 `mode`、实现类、数据库地址/schema 以及初始化结果。`empty` 或 `simulator` 模式也会明确输出，不再依靠是否出现某条 PostgreSQL 专用提示猜测配置。

CRM 查询运行期失败保留 SQLException 为 cause；若发生在启动阶段，统一失败日志显示该原因链。`INITIALIZED` 后断网不属于初始化异常，应查看后续请求和连接日志，不能据启动成功推断一直在线。

## 验证

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-component-logging.ps1
```

脚本分别加载两个服务的实际 Log4j 配置，在独立 JVM 中故意缺少 `crm.jdbcUrl`：断言控制台和文件均有 STARTING、FAILED 和异常堆栈，无 INITIALIZED，进程以非零状态退出。每个进程有 10 秒看门狗，不连接数据库、不创建报价。

单元测试另验证原异常继续抛出、原因链、凭据脱敏及空 CRM 成功初始化。正常启动验证使用本机现有服务与 PostgreSQL，不执行压力测试。
