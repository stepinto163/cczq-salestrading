# STC 适配层 DPS 日志

QuoteSvr 与 NotificationSvr 的 `control/<服务名>/log4j.ini` 已为 `ficc.common` 配置 INFO 级别滚动日志。部署时需同时更新公共库和这两个配置文件，重启服务后生效。

组件启动、配置失败、初始化异常统一在控制台及 `logs/startup.log` 可见，不依赖 DPS DEBUG 日志。规则见 [组件启动日志](component-logging.zh-CN.md)。

请求摘要、订阅和连接事件为 INFO，完整脱敏消息体为 DEBUG，故障为 WARN。当前排障配置单独启用 `log4j.logger.ficc.common.client.NetClientTransport=DEBUG`；改为 INFO 可关闭消息体，保留摘要与故障信息。未启用 DEBUG 时不执行消息体解析和脱敏，减少开销。

本机 control 配置的日志路径为各服务工作目录下的 `logs/dps-messages.log`，UTF-8；单文件 10MB，保留 3 个备份。

- `REQUEST / REPLY`：服务键、topic、同一次调用的 callId、完整 JSON 业务内容、超时设置、耗时；业务错误 Code 也保留在回复中。
- `ERROR`：离线、无回复、传输异常、非法 JSON 等原因；传输异常记录类型，不将可能含凭据的底层异常文本直接写入。
- `SUBSCRIBE / SUBSCRIBE_REGISTERED / PUSH`：订阅调用、底层方法返回、实际收到的 topic 与内容。REGISTERED 只表示本地调用返回，不代表收到远端订阅确认。
- `CONNECTED / DISCONNECTED`：连接状态。
- 基础资料：现使用 STC 客户端按需查券，记录方法、参数、返回对象是否存在和耗时；DEBUG 记录对象内容。
- `Reference lookup missed`：查不到的 securityId 和适配器类型，便于区分模拟数据和 STC 数据。

密码、会话 SID、token 等字段递归脱敏，包含字符串内嵌 JSON；原请求不修改。不截断正常 JSON 业务内容。无法解析的 JSON 仅记录字符数，避免无法可靠脱敏时泄露凭据。

本日志覆盖新写的 `NetClientTransport` 及基础资料适配代码，不等于抓取底层所有 DPS 帧。现有 STC 客户端自身调用仍由 STC 的日志配置负责。日志的 callId 用于本进程请求/回复关联，不会修改业务协议，也不是跨服务链路追踪 ID。

2026-09-15：3 项传输测试通过，检查请求、回复、推送、错误记录、关联标识和凭据脱敏。真实 QA 的接口可用性仍需通过目标环境日志确认。

QA 使用服务命名的每日文件，路径及路由见[QA 日志](jenkins-qa.zh-CN.md#qa-日志)。服务端业务收发另由 `ficc.quote.dps` / `ficc.notification.dps` 记录；是否输出控制台取决于对应 appender，不能仅查看控制台判断有没有记录。
