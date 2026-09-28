# DPS API 调试器

独立 WinForms 程序，使用真实 DPS DLL，经 GW 调试 STC 风格的 request/reply 和 subscribe/publish。代码和 SDK 均位于 `iteration1/api-debugger`，不依赖已移除的独立 C# 原型。构建后单独启动：

```powershell
& .\iteration1\api-debugger\bin\Debug\net472\Ficc.DpsApiDebugger.exe
```

## 请求调试

桌面业务测试以集成后的 `stc-client` 为准，旧原型的自动测试按钮和脚本已退役。下文的 trader1/trader2 等默认账号仅适用于模拟适配模式；连接实际 STC 时使用该环境的账号和地址。

1. 默认连接 `127.0.0.1:18600`，账户和密码为 `trader1`；需要测试另一用户时使用 `trader2`，内部接口用 `search` 或 `intake`。
2. 选择服务与接口模板，编辑请求 JSON，点击“发送请求”。提供 47 个已注册请求接口模板：32 个报价/行情/分组接口、13 个通知接口、两个基础数据模拟接口。
3. 右侧显示完整回复，包括失败 Code 和错误详情；状态栏显示本次耗时。每次只发一个请求，不自动产生压力。
4. `${quoteId}`、`${revision}` 等在“变量”页修改；成功回复会回填常见 ID 与版本。模板是评审稿的示例，业务标识需按实际回复替换；变量为当前调试上下文，不代表跨接口自动做版本控制。
5. 默认每次生成新的 requestId。测试幂等时，从请求历史恢复原请求，关闭该选项，保留原 requestId 后重发。更改内容但复用 requestId 可检查幂等冲突。

“保存请求／载入请求”保存服务名、topic、业务 JSON 与变量，不保存登录密码。界面保留最近 100 次请求历史；完整收发记录独立写入日志文件。

### CRM 请求模板

在 **接口模板** 中选择以 **CRM：** 开头的示例，共六项：搜索机构、查询所选机构联系人、跨机构搜索联系人、按机构 ID 读取、按联系人 ID 读取，以及匹配对手后确认报价。均调用 QuoteSvr，沿用 `ficc.base.query`、`ficc.base.get` 和 `ficc.quote.confirm`；没有新增 CRM 订阅 topic。

先搜索机构，再查询该机构联系人。回复首条记录的 `institutionId`、`contactId` 会自动填入 **变量** 页签；需要其他记录时手工改为其 ID。按 ID 读取及确认模板引用这些变量，确认前还需核对 `quoteId`、`revision`。搜索采用包含匹配；下一批将返回的 `nextOffset` 填入请求的 `offset`。空关键词只返回限定数量的首批记录，QA 可直接输入实际名称或代码。

2026-09-16：通过本机 GW 和 PostgreSQL CRM 验证四个机构/联系人查询及读取模板、变量提取；没有执行确认报价的写模板。报告：`runtime/debugger-crm-templates.json`。

## 订阅调试

在 **订阅 / 发布** 页签中，“当前服务的订阅 Topic”按上方服务选择器过滤，并显示业务名称、topic 和访问范围。下方黄色说明区解释消息内容、初始化步骤及限制。

1. 选择 QuoteSvr 或 NotificationSvr，再选择对应订阅 topic。
2. 对“当前会话”类型，点击 **获取会话 Topic**：工具调用相应 stream.open，并把返回的 pushTopic 填入“实际 Topic”；请求及回复仍记录在请求历史和消息日志中。
3. 点击 **订阅**，再通过请求区调用对应 view.open 登记筛选条件。也可以手工填写精确 topic。
4. 同页左侧选择收到的消息，右侧 **完整推送消息（可复制）** 显示时间、topic 和完整 JSON 正文，不截断消息内容。可拖动分隔条调整宽度；正在查看历史消息时，新消息不会强制跳到最新一条。

API 调试器在 **订阅 / 发布** 的订阅、退订按钮旁提供 **显示 HEARTBEAT**，同样默认关闭。该选项即时控制后续心跳是否进入订阅列表、消息日志及 JSONL 文件，无需重新连接；已显示或写入的历史记录保留。开启后可选中心跳查看完整正文，关闭也不会改变订阅或消息处理。

| 服务 | Topic | 用途及访问范围 |
|---|---|---|
| QuoteSvr | `ficc.quote.report.<sessionId>` | 当前会话报价变化，无订阅快照 |
| QuoteSvr | `ficc.price.report.<sessionId>` | 当前会话行情价格簿，支持订阅快照 |
| QuoteSvr | `ficc.quote.events` | 内部报价事件，内部服务用途；取决于 GW 路由及权限配置 |
| QuoteSvr | `ficc.price.snapshots` | 内部批量价格簿同步，内部服务用途；取决于 GW 路由及权限配置 |
| QuoteSvr | `ficc.price.book.<securityId>` | 内部单债券 STC MarketData，内部服务用途；取决于 GW 路由及权限配置 |
| NotificationSvr | `ficc.notification.report.<sessionId>` | 当前会话提醒、已读及偏好变化，无订阅快照 |
| NotificationSvr | `ficc.notification.events` | 内部通知事件，内部服务用途；取决于 GW 路由及权限配置 |

此目录描述当前代码实际发布的 7 类 topic；内部 topic 也列出供理解服务数据流，不表示能经前端 GW 使用。它是本地维护的目录，不是运行时自动发现。

- 报价、通知使用普通订阅；初始内容通过各自 view.open/query 请求取得。
- 行情可勾选“行情订阅快照”，观察原生快照，再发送 price.view.open 观察增量。
- 推送列表展示时间、kind、topic；选择一条后在“推送内容”查看 JSON。“退订”取消所选 topic。
- 私有推送使用当前会话的 stream.open 返回值；标准 GW 的实际会话隔离边界见下文，不假定调试工具替代服务端鉴权。

### 通配订阅

DPS 的 TopicUtils 支持按点分段的 `*` 和 `**` 匹配；例如 `ficc.price.report.*`、`ficc.price.book.**`。债券代码 `250001.IB` 含点，因此不能用一个 `*` 代表整个 securityId。QuoteSvr 内部行情快照代码明确支持 `ficc.price.book.**`。

当前使用标准 GW，不再包含自定义会话主题检查。底层通配订阅能力由标准 GW 提供；本配置不验证订阅主题后缀与当前 sessionId 的归属。用户应使用 stream.open 返回的 pushTopic，生产会话隔离仍需标准网关/服务端统一实现，详见 `GW/README.zh-CN.md`。

依据：`dps-api-java/.../utils/TopicUtils.java`、QuoteSvr `ViewService.java`、NotificationSvr `NotificationService.java`；标准 GW 的权限能力以 `GW/README.zh-CN.md` 为准。

## 日志及构建

调试器使用自身的 UTF-8 JSONL 日志：时间、连接标识、用户、方向、服务名、topic、完整业务消息体；密码脱敏。界面“消息日志”页可打开实际目录，文件不受界面 200 条预览限制。底层 SDK 未暴露的 TCP 帧及原始退订确认不在应用日志范围。

```powershell
dotnet build iteration1/api-debugger/DpsApiDebugger.csproj -m:1
```

源码入口：`api-debugger/src/ApiDebugForm.cs`；订阅界面：`ApiDebugSubscriptions.cs`；请求模板：`api-debugger/requests.json`；订阅目录及中文解释：`api-debugger/subscriptions.json`。

早期订阅自测包含已移除自定义 GW 的权限假设，不作为标准 GW 的当前验收结果。联调时逐项检查目标服务的请求、推送及实际权限配置。

当前版本已构建到标准输出目录，可直接运行：

```powershell
& .\iteration1\api-debugger\bin\Debug\net472\Ficc.DpsApiDebugger.exe
```

后续构建前关闭占用该文件的调试器窗口。
