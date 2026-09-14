# DPS API 调试器

独立 WinForms 程序，使用与原型相同的真实 DPS DLL，经 GW 调试 STC 风格的 request/reply 和 subscribe/publish。可从原型顶部 **API 调试器** 打开，或单独启动：

```powershell
& .\iteration1\api-debugger\bin\Debug\net472\Ficc.DpsApiDebugger.exe
```

## 请求调试

1. 默认连接 `127.0.0.1:18600`，账户和密码为 `demo`；需要测试另一用户时使用 `demo2`，内部接口用 `search` 或 `intake`。
2. 选择服务与接口模板，编辑请求 JSON，点击“发送请求”。提供 47 个已注册请求接口模板：32 个报价/行情/分组接口、13 个通知接口、两个基础数据模拟接口。
3. 右侧显示完整回复，包括失败 Code 和错误详情；状态栏显示本次耗时。每次只发一个请求，不自动产生压力。
4. `${quoteId}`、`${revision}` 等在“变量”页修改；成功回复会回填常见 ID 与版本。模板是评审稿的示例，业务标识需按实际回复替换；变量为当前调试上下文，不代表跨接口自动做版本控制。
5. 默认每次生成新的 requestId。测试幂等时，从请求历史恢复原请求，关闭该选项，保留原 requestId 后重发。更改内容但复用 requestId 可检查幂等冲突。

“保存请求／载入请求”保存服务名、topic、业务 JSON 与变量，不保存登录密码。界面保留最近 100 次请求历史；完整收发记录独立写入日志文件。

## 订阅调试

先发送对应 `stream.open`，返回的 pushTopic 会自动填入订阅栏，然后点击“订阅”。

- 报价、通知使用普通订阅；初始内容通过各自 view.open/query 请求取得。
- 行情可勾选“行情订阅快照”，观察原生快照，再发送 price.view.open 观察增量。
- 推送列表展示时间、kind、topic；选择一条后在“推送内容”查看 JSON。“退订”取消所选 topic。
- GW 拒绝其他会话的私有 topic。调试器不会绕过服务端权限。

## 日志及构建

与原型共用 UTF-8 JSONL 日志：时间、连接标识、用户、方向、服务名、topic、完整业务消息体；密码脱敏。界面“消息日志”页可打开实际目录，文件不受界面 200 条预览限制。底层 SDK 未暴露的 TCP 帧及原始退订确认不在应用日志范围。

```powershell
dotnet build iteration1/api-debugger/DpsApiDebugger.csproj -m:1
```

源码入口：`api-debugger/src/ApiDebugForm.cs`；模板：`api-debugger/requests.json`。启动参数 `--self-test <结果文件>` 验证 QuoteSvr 请求、NotificationSvr 请求、行情快照订阅、退订和历史记录；不创建报价。最新结果为 `runtime/debugger-smoke.json`。
