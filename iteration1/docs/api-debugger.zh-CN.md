# DPS API 调试器

独立 WinForms 程序，使用与平台相同的真实 DPS DLL，经 GW 调试 STC 风格的 request/reply 和 subscribe/publish。可从平台顶部 **API 调试器** 打开，或单独启动：

```powershell
& .\iteration1\api-debugger\bin\Debug\net472\Ficc.DpsApiDebugger.exe
```

## 请求调试

接入真实 STC 的自动测试请使用新增 **QA 自动测试** 页签，填写两个真实账号并选择 BaseSvr 债券，详见 [QA 功能自动测试](qa-autotest.zh-CN.md)。下文的 trader1/trader2 等默认账号仅适用于模拟适配模式。

1. 默认连接 `127.0.0.1:18600`，账户和密码为 `trader1`；需要测试另一用户时使用 `trader2`，内部接口用 `search` 或 `intake`。
2. 选择服务与接口模板，编辑请求 JSON，点击“发送请求”。提供 47 个已注册请求接口模板：32 个报价/行情/分组接口、13 个通知接口、两个基础数据模拟接口。
3. 右侧显示完整回复，包括失败 Code 和错误详情；状态栏显示本次耗时。每次只发一个请求，不自动产生压力。
4. `${quoteId}`、`${revision}` 等在“变量”页修改；成功回复会回填常见 ID 与版本。模板是评审稿的示例，业务标识需按实际回复替换；变量为当前调试上下文，不代表跨接口自动做版本控制。
5. 默认每次生成新的 requestId。测试幂等时，从请求历史恢复原请求，关闭该选项，保留原 requestId 后重发。更改内容但复用 requestId 可检查幂等冲突。

“保存请求／载入请求”保存服务名、topic、业务 JSON 与变量，不保存登录密码。界面保留最近 100 次请求历史；完整收发记录独立写入日志文件。

## 订阅调试

在 **订阅 / 发布** 页签中，“当前服务的订阅 Topic”按上方服务选择器过滤，并显示业务名称、topic 和访问范围。下方黄色说明区解释消息内容、初始化步骤及限制。

1. 选择 QuoteSvr 或 NotificationSvr，再选择对应订阅 topic。
2. 对“当前会话”类型，点击 **获取会话 Topic**：工具调用相应 stream.open，并把返回的 pushTopic 填入“实际 Topic”；请求及回复仍记录在请求历史和消息日志中。
3. 点击 **订阅**，再通过请求区调用对应 view.open 登记筛选条件。也可以手工填写精确 topic。

| 服务 | Topic | 用途及访问范围 |
|---|---|---|
| QuoteSvr | `ficc.quote.report.<sessionId>` | 当前会话报价变化，无订阅快照 |
| QuoteSvr | `ficc.price.report.<sessionId>` | 当前会话行情价格簿，支持订阅快照 |
| QuoteSvr | `ficc.quote.events` | 内部报价事件，当前 GW 禁止 |
| QuoteSvr | `ficc.price.snapshots` | 内部批量价格簿同步，当前 GW 禁止 |
| QuoteSvr | `ficc.price.book.<securityId>` | 内部单债券 STC MarketData，当前 GW 禁止 |
| NotificationSvr | `ficc.notification.report.<sessionId>` | 当前会话提醒、已读及偏好变化，无订阅快照 |
| NotificationSvr | `ficc.notification.events` | 内部通知事件，当前 GW 禁止 |

此目录描述当前代码实际发布的 7 类 topic；内部 topic 也列出供理解服务数据流，不表示能经前端 GW 使用。它是本地维护的目录，不是运行时自动发现。

- 报价、通知使用普通订阅；初始内容通过各自 view.open/query 请求取得。
- 行情可勾选“行情订阅快照”，观察原生快照，再发送 price.view.open 观察增量。
- 推送列表展示时间、kind、topic；选择一条后在“推送内容”查看 JSON。“退订”取消所选 topic。
- GW 拒绝其他会话的私有 topic。调试器不会绕过服务端权限。

### 通配订阅

DPS 的 TopicUtils 支持按点分段的 `*` 和 `**` 匹配；例如 `ficc.price.report.*`、`ficc.price.book.**`。债券代码 `250001.IB` 含点，因此不能用一个 `*` 代表整个 securityId。QuoteSvr 内部行情快照代码明确支持 `ficc.price.book.**`。

但当前项目的 `FiccSecurityCheck` 仅放行本会话的三种精确 report 地址，通配地址与内部地址均返回 `1003`，包括内部账户经此 GW 订阅的情况。因此：**底层支持通配，但当前调试器连接的 FICC GW 不允许用户使用通配订阅**。本轮不调整访问权限。输入框保留手工输入能力，连接其他 GW 时以该 GW 的策略为准。

依据：`dps-api-java/.../utils/TopicUtils.java`、QuoteSvr `ViewService.java`、NotificationSvr `NotificationService.java`、`gateway/src/ficc/gateway/FiccSecurityCheck.java`。

## 日志及构建

与平台共用 UTF-8 JSONL 日志：时间、连接标识、用户、方向、服务名、topic、完整业务消息体；密码脱敏。界面“消息日志”页可打开实际目录，文件不受界面 200 条预览限制。底层 SDK 未暴露的 TCP 帧及原始退订确认不在应用日志范围。

```powershell
dotnet build iteration1/api-debugger/DpsApiDebugger.csproj -m:1
```

源码入口：`api-debugger/src/ApiDebugForm.cs`；订阅界面：`ApiDebugSubscriptions.cs`；请求模板：`api-debugger/requests.json`；订阅目录及中文解释：`api-debugger/subscriptions.json`。

启动参数 `--self-test <结果文件>` 验证服务过滤后的 7 类 topic、会话 topic 获取、QuoteSvr/NotificationSvr 请求、行情快照订阅、退订，以及 GW 拒绝通配和内部订阅。不创建报价。本轮结果为 `runtime/debugger-subscriptions.json`，全部通过。

当前版本已构建到标准输出目录，可直接运行：

```powershell
& .\iteration1\api-debugger\bin\Debug\net472\Ficc.DpsApiDebugger.exe
```

后续构建前关闭占用该文件的调试器窗口。
