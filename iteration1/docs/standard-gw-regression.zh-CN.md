# 标准 GW 切换回归（2026-09-21）

## 变更

- 删除旧 `gateway` 源码、两套网关 POM、自定义安全/计时类及兼容 GW 补丁；Maven reactor 只保留 Common、QuoteSvr、NotificationSvr。
- `start-gateway.ps1` 直接启动标准 `GW/javalib`，不运行 javac，不依赖 FICC 扩展 JAR；同步重启脚本、依赖清单和部署说明。
- 旧的本机生成物移入忽略的 `runtime/retired-gateway-builds`，不在运行 classpath 或 Git 中。
- 修复 C# 示例报价操作状态提示及混合 JSON 时间类型造成的排序精度丢失；同步主 C# 客户端排序实现与二进制。

## 结果

| 检查 | 结果 |
|---|---|
| 标准 GW 启动 | TCP 18600、WebSocket 18601，控制台/文件日志正常，无自定义类依赖 |
| Web 功能回归 | 通过：两个账号、独立报价/行情/提醒流、创建/确认/撤销/恢复、筛选进出、分页、提醒已读；测试数据已清理 |
| C# 实际窗体按钮回归 | 通过：创建/确认/修改/撤销/恢复/删除/历史查询、推送置顶及旧报价更新后排序；两条测试报价已清理 |
| 时间精度回归 | 通过：JSON Date 与字符串混用时保留小数秒，后续浏览页保持顺序 |
| Maven | 本机兼容入口 offline validate 通过，仅三个业务模块；STC POM XML检查通过，内网整包构建未在本机执行 |
| Python/XML | 已修改脚本语法与标准GW配置检查通过；没有运行压力测试 |

Web 报告：本机 `runtime/web-functional-1789979572147/report.json`。C# 报告：`examples/csharp-frontend/bin/Debug/net472/lifecycle-check.txt`。报告含测试会话信息，不提交原始消息日志。

## 单独记录的权限限制

标准 GW 接受订阅其他已知 sessionId 的 topic；本次测试明确记录为 `LIMITATION`，没有把它记为隔离检查通过。旧自定义类提供的会话主题归属及内部同步接口拦截已移除。功能通过不等于生产权限验收通过；统一权限能力应由标准网关或业务服务提供。

严格订阅隔离测试保留：`test-web-client.ps1` 不加参数时仍要求拒绝跨会话订阅；标准配置回归使用 `-StandardGw`，结果单独记录能力限制。
