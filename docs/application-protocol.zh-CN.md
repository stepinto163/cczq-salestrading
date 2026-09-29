# 应用消息协议代码

销售交易消息采用显式的请求、应答、推送模型。沿用 STC 的简单 Java 数据类风格：一个类一个文件，字段名对应 wire JSON，协议模型放在 common 中供各服务引用。DPS 连接、登录及 ZooKeeper 服务发现仍使用原 STC 库。

## 从哪里开始读

| 代码 | 用途 |
|---|---|
| [Java `ficc.api`](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/) | 后端共享的请求、Data、业务记录及推送定义 |
| [`SalesTradingApi.java`](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/SalesTradingApi.java) | topic → 请求类型 → 成功 Data 类型的目录 |
| [C# `Ficc/Protocol`](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/) | STC 客户端对应模型；JsonProperty 明确指定协议字段名 |
| [`FiccApiClient.cs`](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/FiccApiClient.cs) | 类型化请求、requestId 校验及批量操作结果检查 |
| [`ViewProtocolClient.cs`](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/ViewProtocolClient.cs) | 三类视图的类型化请求、分页及推送读取 |
| [Web `app-protocol.d.ts`](../iteration1/web-client/src/app-protocol.d.ts) | Web 消息字段声明；对应 JS 目录负责 topic 与服务路由 |

## 主要模型

| 场景 | 请求 | 成功 Data / 推送 |
|---|---|---|
| 创建报价 | QuoteCreateRequest → QuoteCreateItem | QuoteWriteData → QuoteOperationResult |
| 确认、修改、Refer、Unrefer、删除 | QuoteChangeRequest → QuoteChangeItem | QuoteWriteData；逐条检查 Code |
| 读取报价 | QuoteGetRequest | QuoteGetData → QuoteRecord |
| 报价视图 | QuoteViewOpenRequest / ViewQueryRequest | QuoteViewData / QuoteStreamBatch |
| 行情视图 | PriceViewOpenRequest / ViewQueryRequest | PriceViewData / PriceStreamBatch → PriceBook |
| 提醒视图 | NotificationViewOpenRequest / ViewQueryRequest | NotificationViewData / NotificationStreamBatch |
| 提醒偏好、已读 | NotificationPreferenceSaveRequest / NotificationReadRequest | NotificationPreferenceData / NotificationReadData |
| 基础资料、CRM | BaseGetRequest / BaseQueryRequest | BaseData → ReferenceRecord |
| CRMSvr 服务调用 | CrmSearchRequest / CrmGetRequest | BaseData / CrmGetData（原 STC 平铺应答） |
| 自然语言识别 | ParserRequest | ParserReply.Datas → ParserQuote |
| QTrade 接入 | QuoteCreateRequest.intake → IntakeIdentity | 同创建报价应答；不伪造用户登录 |
| 通知服务同步报价和行情 | StateQueryRequest / Request | QuoteStateData、QuoteEventBatch、PriceFeedBatch |

Quote、Price、Notification 仍有独立记录及推送模型。ViewPage / ViewDelta 仅共享分页、进入和退出等元数据，不合并三类业务频道。

`QuoteInput` 是可编辑字段；`QuoteCreateItem` 增加 clientQuoteId；`QuoteChangeItem` 增加 quoteId / expectedRevision；`QuoteRecord` 增加持久化身份、状态、版本及审计字段。字段枚举仍以[筛选字典](product-filter-contract.zh-CN.md)和 [API 规格](ficc-api-spec.zh-CN.md)为准；界面中文名称不能代替协议代码。

## 调用示例

Java 服务间调用：

```java
QuoteCreateItem quote = new QuoteCreateItem();
quote.clientQuoteId = "CLIENT-001";
quote.securityId = "240210.IB";
quote.side = "1";
quote.quoteType = "1";
quote.yieldType = "1";
quote.price = "2.5000";
// 其他业务必填字段按 API 规格填写。
QuoteCreateRequest request = new QuoteCreateRequest();
request.requestId = "REQUEST-001";
request.quotes = Collections.singletonList(quote);

JSONObject wireReply = transport.request(
        SalesTradingApi.QuoteCreate.topic, sessionId, request.toJson());
ApiReply<QuoteWriteData> reply = SalesTradingApi.QuoteCreate.reply(wireReply);
// 先检查 reply.Code，再逐条检查 reply.Data.results 的 Code。
```

C# 客户端：

```csharp
QuoteRecord quote = await api.GetQuoteModelAsync(quoteId);
var change = new QuoteChangeItem
{
    QuoteId = quote.QuoteId,
    ExpectedRevision = quote.Revision,
    Price = "2.6000"
};
QuoteWriteData result = await api.ChangeQuotesAsync(
    SalesTradingApi.QuoteUpdate, new[] { change });
FiccApiClient.RequireQuoteResults(result);
```

## 字段存在性和兼容规则

- ID、revision、sequence、价格和数量采用字符串模型，避免大整数或十进制精度损失。已有消息中的数字 token 在未修改时保留原形式，不改变历史请求的幂等摘要。
- `Boolean` / `bool?` 区分未提供与 false；未提供字段不自动补默认值。
- 新建修改请求的 null 字段默认不发送。需要明确清空时调用 `clearField("contactId")` / `ClearField("contactId")`。读入的显式 null 会原样保留。
- ProtocolCodec 保存已读消息的原字段形式和未知字段。业务校验仍负责拒绝非法请求字段、检查必填项、枚举、权限和版本；模型解码不替代这些校验。
- 条件 `value` 按字段与 operator 可以是标量或数组，保留 Object / object / unknown。字典映射、quoteRefs 等使用有明确值类型的 Map / Dictionary。
- 不直接用普通 JSON 序列化器序列化 DTO；统一使用 `toJson()` / `ToJson()`，以保留缺省与清空语义。
- ParserSvr 的 `Datas`、CRM 的 `datas` 与 FICC 的 `Data` 是不同的既有信封，不自动改名。

## 接入边界

Java 服务在请求入口和应答／发布出口应用模型；QuoteSvrClient、StcCrmClient、QTradeQuoteMapper / QuoteForwarder 使用模型描述服务间消息。STC 客户端的报价操作、解析结果提交、视图及订阅通过对应模型调用。

QuoteSvr 的当前报价状态、搜索结果和匹配器使用 QuoteRecord；QuoteMatcher.prepare 在每次查询开始时准备条件，并在该次查询内按债券缓存 ReferenceRecord。逐条 match 只读取所需属性，不逐条执行 JSON→DTO 或 DTO→JSON。下一次查询重新获取基础资料，不引入跨查询的静态资料缓存。

本轮保留数据库 JSON、NotificationService 内部 JSON 状态及 UI 动态行投影。ProtocolBoundary 连接类型化协议与仍采用 JSON 的实现，RequestProjectionAsync 用于尚需 JObject 的网格和条件编辑器。它们不是新的对外协议。通用 DPS 调试器继续接受任意 JSON，便于排障。明确的消息协议不要求所有内部实现使用同一种数据结构；后续转换应根据维护收益和测量结果决定，不能为消除 JSONObject 而增加热路径分配。

小规模对比中，匹配路径减少了逐行复制和重复基础资料补充；50 行响应的模型转换仍有额外开销。详见[协议重构性能对比](protocol-performance-comparison.zh-CN.md)，该结果不代表完整查询或客户端延迟。

## 修改协议及验证

1. 在 Java 和 C# 对应模型中添加字段，明确 wire 名称、类型、是否允许缺省／null；新增接口登记两个 SalesTradingApi 目录。
2. 修改实际生产者与消费者，并更新 API 规格。新业务代码优先使用模型属性，避免重新散布 JSON 键字符串。
3. 运行 `python iteration1/scripts/check-app-protocol.py --write-web` 更新 Web 声明和目录，再运行不带参数的校验。脚本检查 Java/C# 模型字段、继承及 endpoint 映射的一致性。
4. 使用 `build all` 构建 Java 8 服务及 STC 客户端。ProtocolCodecTest 检查 null、false、精度、未知字段及列表重排。
5. BusinessSmoke 和 NotificationProtocolSmoke 在隔离 PostgreSQL schema 中检查业务行为并输出真实协议夹具。C# `test-protocol-contract.ps1` 读取夹具，验证请求、Data 和推送双端兼容；另有解析提交和 QTrade 分组的 WPF 回归脚本。

这些检查不等于实际 QA 网络验收，也不包含性能压力测试。部署新包仍使用现有 build / deploy 流程。
