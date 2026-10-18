# UI 自动化的独立协议断言

这些 DTO 与 JSON 编解码来自原验收基线 `dc929bd284d9daf95890c7fa61342512ef73e82c`，仅保留 UI 用例实际使用的类型，并将命名空间迁到 `SalesTrading.UiTests.Contracts`。

- 仅编入 `UiScenarioWorker.exe`，不编入产品插件，不恢复旧 `Ficc` 服务实现。
- `TestGatewayTransport` 使用当前产品的真实 ApiManager Gateway 连接。它不是模拟服务，也不绕过登录权限。
- 产品命令走新 Gateway/Service/ViewModel；独立 API 读取用于确认持久化结果、权限拒绝及清理本例数据，避免用产品自己的 DTO 映射错误证明自己正确。
- UI 定位与对象结构变化在测试驱动中适配，不能为了候选版本通过而删除原业务断言。
- 协议真正发生变更时同步更新此目录、用例与 API 文档；不能从候选 DTO 自动生成期望结果。

`ClientPerformanceTrace` 仅供测试侧协议解码使用。候选产品没有继承旧版性能埋点，不得把测试侧采样声称为完整产品 response-to-render 分段测量。
