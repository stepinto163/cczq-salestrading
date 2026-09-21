# C# 前端开发学习示例

这是面向开发者的最小 WinForms 示例，不是另一套业务客户端或 API 调试工具。复制整个目录即可独立编译，不引用仓库外的源码。`lib` 包含现有客户端使用的 DPS SDK 及运行依赖。

## 启动

要求 Windows x64、.NET Framework 4.7.2 或以上。编译需安装 .NET SDK 和 .NET Framework 4.7.2 开发包；使用已经编译的程序不需要 Visual Studio。

```powershell
cd examples\csharp-frontend
dotnet build FrontendLearning.csproj -m:1
.\bin\Debug\net472\Ficc.FrontendLearning.exe
```

也可运行本目录 `start.ps1`。此脚本只启动学习客户端，不启动或修改任何后台服务。

如果旧程序正在运行、其 EXE 被 Windows 锁定，新构建放在 `bin\Updated\net472`；`start.ps1` 会选择两个目录中时间更新的程序。复制到其他机器仍需复制整个对应输出文件夹。

界面填写 GW 地址、端口、业务账号和密码。密码不写入配置。本机 GW 通常为 `127.0.0.1:18600`；QA 请填写实际 GW。示例依赖运行中的 GW、QuoteSvr；查看提醒还需要 NotificationSvr。真实 STC 模式必须使用能取得有效 sid 的业务账号。

注意这里是 DPS TCP 端口，不是 WebSocket 端口（本机 Web 使用的 `18601`）。只复制可运行程序时，应复制整个 `bin\Debug\net472` 文件夹，不能只复制 EXE。

“自包含”指本示例源码和客户端 DLL 完整，不表示内置服务器、数据库或 STC 基础资料。

## 建议阅读顺序

| 文件 | 学习内容 |
|---|---|
| `src/QuoteExamples.cs` | 请求结构、报价创建/确认/修改/撤销/恢复/删除、CRM、历史和已读操作 |
| `src/FilterExamples.cs` | 价格区间、券种、流动性、信用、条款、区域及组合条件的完整消息结构 |
| `src/LearningForm.cs` | 输入如何形成业务请求；后台请求不阻塞 UI；DPS 推送通过 BeginInvoke 回到 UI 线程 |
| `src/SubscribedPage.cs` | stream.open → subscribe → view.open；分页、序号检查、推送缓存、关闭订阅 |
| `src/ClientViewPage.cs` | upsertIds/removeIds 如何维护当前页；第一页排序补齐，后续页不插入新记录 |
| `src/DpsClient.cs`、`src/MessageLog.cs` | 从现有 C# 客户端复制的传输与日志代码，不重新实现 DPS 协议 |

## 典型操作

1. **登录并订阅报价**：点击登录，填写一个实际存在的债券代码，点击“报价视图”。清空债券代码后打开视图即不限定单只债券。服务器返回 `pushTopic`，客户端直接订阅该值。
2. **创建和确认**：点击创建，得到 `quoteId` 和待确认状态。确认后生效。按需搜索 CRM，手动将返回的机构/联系人 ID 填入输入框，再确认。已录入的名称必须在确认前匹配 ID；本示例创建默认不填写对手方。
3. **修改**：读取报价后修改收益率，再提交。`expectedRevision` 来自界面读取的版本；冲突时显示错误，由用户重新读取，不能自动取最新版本后强行重试。
4. **撤销/恢复/删除**：依次点击撤销、恢复，`quoteId` 不变；有效报价应先撤销再删除。历史按钮查询同一报价连续的变更记录。示例不隐藏业务错误，也不自动替用户决定状态转换。
5. **搜索和翻页**：视图使用筛选条件和固定 20 条页大小；下一页调用 `view.query`，复用原 viewId，替换缓存。返回第一页继续接收实时插入。教学界面用按钮表达分页，业务界面可由滚动事件触发相同调用。
6. **行情**：打开行情视图，得到 `books` 而非 `quotes`。双击一行查看完整 `marketData/quoteRefs`。报价流和价格流不混用。示例一次展示一个业务视图，切换时关闭旧视图和订阅。
7. **提醒**：打开提醒视图，双击记录取得提醒 ID；点击已读，并检查回包 `results` 中每项结果。读取提醒偏好展示完整设置。提醒产生依赖服务端规则和市场事件，打开视图本身不会制造提醒。

## 单条流程与实时排序回归

使用 `Ficc.FrontendLearning.exe --lifecycle-check` 显式启动窗体按钮回归，账号密码读取环境变量 `FICC_USER`、`FICC_PASSWORD`；可选 `FICC_HOST`、`FICC_PORT`、`FICC_SECURITY` 指定测试环境。本检查会创建两条报价，通过真实按钮执行确认、修改、撤销、恢复、删除、历史查询，并验证第一页实时推送后新报价及更新的旧报价置顶，最后删除本次创建的报价。结果写入程序目录 `lifecycle-check.txt`，不是压力测试。仅在测试环境执行，并由独立进程设置超时保护。

报价操作需要先读取当前报价ID的版本；更改ID后操作按钮暂不可用，点击“读取报价/版本”恢复。第一页固定按更新时间倒序，列头不提供其他排序；后续页保持浏览顺序，需要回到第一页查看实时置顶结果。

## 属性筛选示例

**全部条件入口：**登录后点击“全部产品条件”，直接读取服务端字段目录，编辑任意已定义的产品字段，包括远期、中债偏离、付息方式、免税和外部评级。编辑后点击“应用到当前视图”。下拉预设仅作为易读的代码样例，不再是唯一入口；选取其他预设会退出自定义条件。`src/ProductFilterForm.cs` 包含本目录独立编译所需的编辑器，不引用主客户端源码。

Bid-Ofr 等尚未接入真实计算的字段由服务端提供明确标记的模拟值，不能当作真实市场价差；“全部产品条件”的参考值会注明模拟资料。中债收益率偏离使用 BP，净价偏离使用独立字段和价格单位。

保持原学习界面，仅新增一行“筛选样例”。选择样例后点击“查看条件 JSON”即可查看请求里的 `filter`，不需要登录。清空顶部债券代码表示跨债券筛选；保留代码则在样例条件上叠加该债券条件。

登录后打开报价、行情或提醒视图，或点击“应用到当前视图”。条件变化会关闭旧视图、重新订阅并打开新视图，回到第一页；后续“下一页”仅发送 `viewId/pageNumber/pageSize`，服务端复用原条件。三个领域使用同一条件格式，但仍分别使用自己的 `ficc.quote.*`、`ficc.price.*` 和 `ficc.notification.*` topic 与数据模型。

以下代码独立于窗体，可以直接用于业务调用：

```csharp
var filter = new JObject
{
    ["conditions"] = new JArray(
        FilterExamples.Range("price", "2.0000", "3.0000"),
        FilterExamples.AnyOf("bondRating", "AAA", "AA+"),
        FilterExamples.AnyOf("liquidityGrade", "S", "A"),
        FilterExamples.Condition("region", "EQ", "310000"),
        FilterExamples.Condition("isSubordinated", "EQ", false))
};
await page.Open("quote", filter);
```

不同字段取 AND，相同字段多条件取 OR；上下限用一个 BETWEEN，范围包含边界。布尔值使用 JSON `true/false`，不是字符串。缺失数据不会被当作 false。行情先按条件筛选报价，再生成价格簿。

“读取字段字典”调用 `ficc.quote.dictionary.query`，查看当前服务器支持的字段、运算符、参考值和 `includesSimulatedData` 标记。示例值用于教学，不保证实际环境必有匹配记录；尤其券种等枚举应以当前字典为准。缺失外部属性的模拟值由服务端提供，客户端不伪造查询结果。需要其他字段时，在 `FilterExamples.cs` 中按字典增加条件即可。

## 三条容易出错的规则

- 外层 `Code=0000` 不代表批量报价全部成功。`RequireSuccess` 检查单条教学请求的结果；批量时应遍历每个 `results`。批量创建使用同一个 `quotes` 数组，每条配置独立 `clientQuoteId`。
- 请求回包确认操作结果，视图推送维护列表。不要根据“确认成功”回包无条件向所有列表追加一行。
- 订阅回调和初始页回包可能交错：先缓存推送，以回包 sequence 为基线，再应用较新的消息。重复消息丢弃；断序停止更新并提示重新打开视图。心跳不显示但仍推进 sequence。

## 日志和边界

底部显示 REQ、REP、PUSH；心跳默认隐藏。完整消息文件位置在登录后显示，默认位于 `%LOCALAPPDATA%\FiccSalesTradingClient\logs`，密码字段脱敏。界面日志有长度上限，清屏不删除日志文件。

每条屏幕日志独立成块：分隔线、递增序号、时间和操作/topic 标题，下一行显示缩进后的 JSON，末尾留空行。原始文件日志仍为逐行 JSON 格式。

订阅流程也显示在底部：`SUBSCRIBE` → `SDK_SUBSCRIBE_RESULT` → `SUBSCRIBE_ACK_REQUEST` → `SUBSCRIBE_ACK`；关闭或切换视图显示 `UNSUBSCRIBE` 和 `SDK_UNSUBSCRIBE_COMPLETE`。SDK返回与GW应答分别记录，不能把SDK接受当成服务端订阅成功。取消订阅完成仅表示SDK调用结束，不伪造服务端应答。失败显示 `SUBSCRIBE_FAILED` / `UNSUBSCRIBE_FAILED`。这些日志与文件日志同时保留，不受心跳开关影响。

教学界面一次最多显示 20 行，因此用简单表格重绘突出协议流程；它不包含压力测试、生产级虚拟滚动、自动重连、离线队列或多面板管理。断线后重新启动并登录。所有写操作均需手动点击，不自动创建报价或修改提醒设置。

可选只读验证：设置环境变量 `FICC_USER`、`FICC_PASSWORD`，以及可选 `FICC_HOST`、`FICC_PORT` 后，用 `--check` 参数启动程序。它检查登录、报价的全部筛选样例、行情和提醒的组合条件，以及订阅、查询、关闭流程，每次最多查询一条；结果写入程序目录的 `connection-check.txt`。检查验证服务器接受请求和分页复用，不保证已有数据覆盖每个筛选分支。它不创建或修改业务数据，不是压力测试。

分类筛选使用稳定代码，例如企业性质 YQ/GQ/MQ/Other、付息频率 M1/M3/M6/Y1/Other。完整条件编辑器的“选择”按钮读取服务端 `filterFields.enumValues`，显示中文标签，发送代码；数值和布尔字段保持原有类型。字段字典见 [产品筛选契约](../../docs/product-filter-contract.zh-CN.md)。
