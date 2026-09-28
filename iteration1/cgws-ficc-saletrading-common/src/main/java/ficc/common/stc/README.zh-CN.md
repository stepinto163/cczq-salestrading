# LoginSvrClient 源码来源

源文件：`D:/workspace/GitHub/sc-app-stc/src/main/java/sc/app/stc/utils/LoginSvrClient.java`，本轮读取日期 2026-09-14。

移植差异：

1. 包名改为 `ficc.common.stc`，与基础库中的同名类隔离；当前构建已统一使用 QA 原版依赖。
2. 请求超时由固定 30 秒改为 `requestTimeoutMillis`，默认 3 秒，由外层适配器设置。
3. 增加来源说明；保留原有用户模型、SID 缓存、消息处理和用户查询代码。

外层 `StcLoginSessionClient` 负责 FICC 配置、用户会话查询、断线检查与订阅失效通知；内部服务同步不执行用户登录。未修改用户提供的 sc-app-stc 仓库。未来升级基础库时应重新比较此副本，避免两份源码长期漂移。
