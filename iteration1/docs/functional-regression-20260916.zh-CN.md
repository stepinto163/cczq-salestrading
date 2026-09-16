# 本机功能回归记录：2026-09-16

环境：本机 PostgreSQL、ZooKeeper、GW、QuoteSvr、NotificationSvr；模拟用户及债券资料，CRM 使用独立 PostgreSQL 样例库。没有连接远程 STC QA，也没有执行性能或压力测试。各功能场景串行运行，客户端进程有外部超时保护。

## 发现与处理

1. 服务重启后，原 GW 仍报告 QuoteSvr 不在线。重启本项目 GW 后请求恢复；未修改 STC/GW 遗留代码，也未宣称解决其长期重连行为。
2. NotificationSvr 的本机模拟登录查询与报价同步为同一服务建立了两个逻辑客户端。模拟身份默认改为复用 `SERVER.QuoteSvr`；真实 STC 仍调用 `SERVER.LoginSvr`。模拟身份消息复用已有带日志的传输适配器，通知请求意外异常增加错误堆栈日志。
3. C# 生命周期测试从当前选中报价继承了未匹配的对手名称，导致确认弹窗等待人工操作。测试创建前清空对手输入；自动测试遇到未匹配名称时直接报错，不打开人工匹配窗口。超时遗留的一条待确认报价已按本次日志中的准确 ID 删除。
4. 多用户测试写死 `I-001/C-001` 等模拟 CRM ID，切到 PostgreSQL CRM 后创建失败。改为在测试前查询有机构关系的联系人，保持后续所有权、价格排序、幂等和清理断言。
5. 已打开的客户端占用可执行文件。测试脚本新增可选 `-ExecutablePath`，允许使用独立构建目录。最终可运行目录 `client/bin/Debug/net472` 已更新。

## 最终结果

| 测试 | 结果 |
|---|---|
| Java 构建及测试（包含 PostgreSQL CRM、配置覆盖） | 26 项通过，无跳过 |
| C# GW 请求接口 | 40 项通过，覆盖 32 个 QuoteSvr 请求主题 |
| C# 报价生命周期界面 | 18 项通过 |
| 四类通知 | 20 项通过 |
| CRM 查询、同名联系人选择、确认及权限 | 15 项通过 |
| 多债券行情界面 | 12 项通过 |
| 关注债券及筛选分组界面 | 23 项通过 |
| 两用户报价场景 | 293 项通过，4 条报价，完成清理 |
| Web 单元测试 | 13 项通过 |
| Web 双用户 GW 集成 | 27 项检查通过，2 条报价，完成清理 |
| 原需求页面 DOM、精确报价、CRM、通知 | 16 项检查通过，1 条报价，完成清理 |

原页面检查使用 DOM 自动化，不代表完整浏览器视觉验收。CRM 结论仅针对本机样例数据，不替代 QA 真实数据验收。

详细消息和报告保存在被 Git 忽略的 `iteration1/runtime/`：`autotest-rebuild.log`、`csharp-gateway-smoke.json`、`ui-smoke.json`、`notification-smoke.json`、`counterparty-ui-smoke.json`、`price-multibond-smoke.json`、`group-ui-smoke.json`、`requirements-dom-report.json`，以及 `multi-user-runs/`、`web-functional-*/` 和 `client-logs/`。

本轮保留 `adapter.properties` 的覆盖机制：先加载 `quote.properties`，再加载可选的适配器文件；后者同名键优先。Web 登录按需查询债券的修复保留。
