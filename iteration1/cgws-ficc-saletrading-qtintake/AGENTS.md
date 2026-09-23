# 开发约定

- 项目及新增 Maven 坐标使用 cgws；保留 STC 外部依赖坐标及现有 ficc 协议。
- 使用 Java 8，遵循 STC 的 handler、stream、resource、control 结构。
- 使用清晰命名、常规缩进和每行一条语句。
- 讨论使用英文，文档使用中文。
- 未经用户明确授权，不运行性能或压力测试。
- 重要组件初始化必须输出统一 INFO 日志；失败记录异常并向上抛出，不静默切换模拟实现。
- 用户已明确授权：QTrade FIX 账号和密码保存在 Git 管理的 config/service.properties 及对应 Jenkins 模板中；FIX 消息日志保留完整字段，包括登录字段。不要自行增加脱敏或独立认证文件。
- 不提交许可证、运行日志或其他本机配置。
