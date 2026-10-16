# FICC 前后端 API 文档（按当前代码整理）

更新日期：2026-10-13。代码基线：`fix/csi-valuations`，`23cf916`。

本文描述此基线的实际接口，不以旧设计稿推断已实现能力。示例中的 ID、时间、机构和用户均为说明值，实际调用必须使用当前环境返回的值；示例是结构示例，不是本次执行得到的服务器应答。业务规则仍以产品需求和已确认的业务决定为准，代码与需求的差距不能由本文反向合法化。

完整字段、继承关系及 Java/C# 源码入口见 **[协议模型字段表](api-models.zh-CN.md)**。本文按模型介绍业务流程，再逐项列出请求、成功输出及约束。除特别说明，接口输出表中的 JSON 均为成功应答的 `Data` 内容。

## 目录

新增：[QTrade 意向发布 API、回执与真实 UI 用例](qtrade-publish.zh-CN.md)。

- [1. 接入与公共契约](#transport)
- [2. Quote：报价与来源](#quote)
- [3. Parser：识别预览](#parser)
- [4. Price：按用户视角生成的价格簿](#price)
- [4.1 智能推荐](#recommendation)
- [5. Filter、Watchlist、FilterGroup：筛选及用户配置](#filter)
- [6. Base、CRM：基础资料与联系人](#crm)
- [7. Notification：提醒](#notification)
- [8. Stream、View：订阅及分页](#views)
- [9. 前端接口逐项输入输出](#endpoints)
- [10. 内部、兼容及诊断接口](#internal)
- [11. 已发现的契约差异与验证边界](#gaps)

<a id="transport"></a>
## 1. 接入与公共契约

### 1.1 路由及应答封装

通过 STC 网关的 DPS 请求/应答调用，不是 HTTP REST。登录沿用现有 STC 登录流程；本文件不重新定义 LoginSvr、网关二进制帧或 FIX 协议。

| 服务键 | 本文涉及的 topic | 用途 |
|---|---|---|
| `SERVER.QuoteSvr` | `ficc.quote.*`、`ficc.price.*`、`ficc.base.*`、`ficc.watchlist.*`、`ficc.filterGroup.*`、`ficc.recommendation.*` | 报价、行情、资料、关注、筛选组及推荐 |
| `SERVER.NotificationSvr` | `ficc.notification.*` | 提醒、偏好与独立筛选组 |
| `SERVER.ParserSvr` | `stc.parser.bond` | 文本识别，返回预览 |
| `SERVER.CRMSvr` | `stc.crm.*` | 主要供 QuoteSvr、ParserSvr 使用的资料服务 |
| `SERVER.QtIntakeSvr` | `ficc.qtintake.status.query` | 接入状态诊断，不是报价页面查询 |

客户端 `IFiccTransport` 调用使用 `QuoteSvr` 等短服务名，实际网关路由对应 `SERVER.QuoteSvr`。topic、JSON 字段名大小写必须保持一致。

普通 FICC 请求：

```json
{"requestId":"R-001","quoteId":"Q-001"}
```

普通成功应答：

```json
{"Code":"0000","Msg":"成功","requestId":"R-001","Data":{"quote":{"quoteId":"Q-001","revision":"2","status":"ACTIVE"}}}
```

以上 `quote` 为节选；完整字段见 QuoteRecord。错误应答：

```json
{"Code":"1004","Msg":"报价已被更新","requestId":"R-002","error":{"type":"REVISION_CONFLICT","quoteId":"Q-001","expectedRevision":"1","currentRevision":"2"}}
```

三种封装不可混用：FICC 使用 `Code/Msg/requestId/Data`；Parser 使用 `Code/Msg/Datas`；CRM 的 search/get 成功返回平铺对象，旧 CRM 查询返回 `Code/Msg/datas`。推送也不是 `ApiReply<Data>`，直接使用对应 StreamBatch。

### 1.2 类型、缺省及幂等

| 项目 | 规则 |
|---|---|
| `requestId` | 普通 FICC 请求必填字符串；关联请求/应答。支持回执的写接口按用户、topic、requestId 和内容摘要识别重试。读取也必须携带它，但不因此成为持久化写回执 |
| 重试同一次写入 | 保留原 requestId 和完全相同的业务内容；内容改变必须使用新 requestId。同 ID 不同内容可能返回 1006；失败项目已写入回执时，改正后也应使用新请求 |
| `expectedRevision` | 修改、确认、撤销、恢复、删除等针对已有记录的乐观锁版本；必须使用最近读取的 revision，不能固定写 `1` |
| 数字 | 报价 price、quantity、revision、sequence 使用字符串；pageSize、limit、totalCount 使用 JSON 整数；属性开关使用真正的 Boolean |
| 日期 | Quote.tradeDate 为 `yyyy-MM-dd`；时间字段按服务返回的原始格式保留；Parser 的金额实际为 JSON number，不能套用 Quote 的 decimal-string 写入约束 |
| 未提供 vs null | 修改请求中省略字段表示保留；显式 null 表示清空，是否允许清空再由业务校验决定。Java 新模型用 `clearField` 表达显式清空 |
| 未知字段 | 编解码器保留未知字段，不代表处理器接受它们；使用 allowOnlyFields 的入口会拒绝多余字段 |

### 1.3 错误处理

| Code | 常见 error.type | 前端处理 |
|---|---|---|
| `0000` | 无 | 批量写入仍须检查每条 results 的 Code |
| `1001` | VALIDATION_FAILED / UNRESOLVED_COUNTERPARTY | 展示字段错误；后者表示机构/联系人只有文本而未选中 ID |
| `1002` | NOT_FOUND | 记录或视图不存在/不可访问，重新读取 |
| `1003` | FORBIDDEN | 会话无效或权限不足 |
| `1004` | REVISION_CONFLICT | 刷新并让用户处理冲突，不覆盖最新记录 |
| `1005` | INVALID_STATE | 当前状态不允许此动作 |
| `1006` | IDEMPOTENCY_CONFLICT | 请求内容冲突或 clientQuoteId 已存在 |
| `1007` | RESET_REQUIRED | 游标/事件位置失效；重新建立快照 |
| `1008` | RESOURCE_LIMIT | 批量、分页、视图或结果数量超限 |
| `1009` | NOT_READY | 依赖未就绪或正在同步；展示真实失败原因 |
| `1010` | UNSUPPORTED，部分入口为 VALIDATION_FAILED | 不支持的接口、清算类型等，以 Code 和具体消息共同判断 |
| `1099` | INTERNAL_ERROR | 内部错误；按 requestId、topic 和时间排查 |

这些是 FICC 常见业务错误，不覆盖所有 GW/旧 STC 错误。GW 可在请求到达业务服务前返回 OFFLINE/无应答；不能当作空列表或成功。

<a id="quote"></a>
## 2. Quote：报价与来源

### 2.1 模型关系

`QuoteInput` 是可编辑内容；`QuoteCreateItem` 增加 clientQuoteId；`QuoteChangeItem` 增加 quoteId、expectedRevision、reason；`QuoteRecord` 增加状态、版本、来源及审计信息。

| 字段 | 含义及约束 |
|---|---|
| securityId | 债券代码；填写后必须可在基础资料中查询 |
| institutionId / institutionName | 对手机构的 CRM ID 与名称；有 ID 时后端按 CRM 规范名称回填 |
| contactId / contactName | 对手交易员的 CRM 联系人 ID 与名称；后端验证其属于所选机构 |
| side | `1`=Bid，`2`=Ofr |
| quoteType / yieldType | quoteType：`1`收益率、`2`净价、`3`全价；收益率时 yieldType：`1`到期、`2`行权；非收益率必须清空 yieldType |
| price | 十进制字符串；收益率最多 4 位小数 |
| quantity / quantityUnit | 数量必须为正；当前单位固定 `WAN`（万），不是元 |
| currency | 当前仅 `CNY` |
| tradeDate / settlType | 交易日与清算速度共同表达交割安排；Quote 的 `1`=T+0、`2`=T+1、`3`=T+2、`4`=T+3、`5`=T+4，之后 `9`到`16`依次为 T+5 到 T+12 |
| urgent / recommended / whole | 急、荐、整，Boolean |
| oco / packing | OCO、打包，Boolean；两者不能同时为 true |
| remark | 备注 |

创建允许保存待补全字段；成为 ACTIVE 时 securityId、side、quoteType、price、quantity、tradeDate、settlType 必须完整，收益率还必须有 yieldType。机构/联系人可以同时未填写；已经输入名称时，确认必须匹配对应 ID，不能只提交名称冒充匹配成功。

### 2.2 状态与写入用例

| 动作 | 前置状态 | 成功后状态 | 请求中的业务字段 |
|---|---|---|---|
| create | 新记录 | PENDING_CONFIRM | clientQuoteId + QuoteInput |
| confirm | PENDING_CONFIRM | ACTIVE | quoteId + expectedRevision，可一并修订 QuoteInput |
| update | PENDING_CONFIRM / ACTIVE | 保持状态 | quoteId + expectedRevision + 要变更的 QuoteInput 字段 |
| refer | ACTIVE | REFERRED | quoteId + expectedRevision，可带 reason |
| unrefer | REFERRED | ACTIVE | quoteId + expectedRevision；恢复时重新验证完整性 |
| delete | PENDING_CONFIRM / REFERRED | DELETED | quoteId + expectedRevision；不是物理删除 |
| clone | 可读取的来源报价 | 新 PENDING_CONFIRM | sourceQuoteId + 来源 expectedRevision + 新 clientQuoteId，可覆盖 QuoteInput |

ACTIVE 不能直接 delete，应先 refer。报价写入每批 1–100 条，**按行提交，允许部分成功**。成功行返回 quoteId、clientQuoteId、revision、Code；失败行返回 Code、Msg、error。外层 Code=0000 不代表每行成功。前端必须保留已成功行的 ID 和版本，不能整批再次 create。

部分成功的完整信封示例：

```json
{"Code":"0000","Msg":"成功","requestId":"R-BATCH","Data":{"results":[{"quoteId":"Q-001","clientQuoteId":"C-001","revision":"2","Code":"0000","Msg":"成功"},{"quoteId":"Q-002","Code":"1004","Msg":"报价已被更新","error":{"type":"REVISION_CONFLICT","quoteId":"Q-002","expectedRevision":"1","currentRevision":"3"}}]}}
```

待确认报价不进入有效价格簿。精确报价的“识别”只生成本地预览；当前“确认报价”实现依次调用 create、get、confirm，尚不是原子创建并确认，因此中间可能短暂出现 PENDING_CONFIRM；confirm 失败时已创建记录也可能保留为待确认。确认成功后应按视图移除事件移出待确认列表。

### 2.3 ID 的不同职责

| 字段 | 含义 |
|---|---|
| quoteId | QuoteSvr 生成的内部 `Q-...` 主键，不以 NegotiationID 或外部 QuoteID 代替 |
| clientQuoteId | 调用方为创建项生成的标识，在所属用户下防止重复创建 |
| sourceMessageId | 内部 `SM-...` 原文分组 ID；一个原文可关联多条报价 |
| source.originalMessageId | 接入侧提供/推导的原文分组键，不等于聊天群 ID |
| source.externalMessageId | 接入事件标识；当前 QT 接入使用稳定来源事件哈希，不是 FIX MsgSeqNum |
| source.externalQuoteId | QT QuoteID(117)，发送方意向标识；接收侧无效值如 -10000 不能当真实主键 |
| source.negotiationId | QT NegotiationID(18033)，双边会话；一个意向可产生多个会话，一个会话可有多个提案 |
| source.sourceUserId / ownerUserId | OwnQtNO 对应的渠道账户 / 映射得到的 STC userid |
| source.counterpartyId | InterloNickID 原始对手标识；不是 CRM contactId，也不能假设是明文 QT 账户 |
| source.chatGroupId | 聊天群标识，不表示“一个原文组” |
| revision / source.externalRevision | 本地记录版本 / 上游版本；各自独立 |

可见性按当前 AccessPolicy：归属人、配置的管理员和内部用户可访问归属报价；普通用户可看非 QT 的 ACTIVE 报价；QT 即使已确认仍按所属交易员隔离。MINE 进一步要求 ownerUserId 等于登录用户。价格簿在生成前应用同一可见性，不能跨用户复用同一份行情结果。

### 2.4 查询、历史与原文

quote.query 默认 `ownerScope=VISIBLE`、`status=["ACTIVE"]`。报价确认页面显式指定 MINE + PENDING_CONFIRM；报价管理按页面指定 ACTIVE/REFERRED。按 updateTime 倒序、quoteId 升序。

history.query 是一条报价的操作历史（before/after），要求归属权限，按 revision 倒序。history.search 是日期范围内每天每条报价的最后状态；fromDate/toDate 必填、包含两端，跨度不超过 366 天。PostgreSQL 按日期和归属权限选取每天每条报价的最后版本，JDBC 分批读取每日快照；Java 复用公共业务筛选器并保留快照游标分页，并非全部条件下推 SQL。相同操作时刻以 revision 较大者为准。详见 [历史查询优化](history-search-optimization.zh-CN.md)。其行 tradeDate 当前取操作日期，不应误读为 quote.tradeDate。

sourceMessage.query 实际按 sourceMessageId 和归属权限筛选；模型虽有 quoteId，当前处理器没有使用它。应先 get 报价取得 sourceMessageId 再查原文。

<a id="parser"></a>
## 3. Parser：识别预览

服务 `SERVER.ParserSvr`，topic `stc.parser.bond`。输入：

```json
{"userName":"test2","content":"Ofr 199385.IB 25山东债91 1000w 2 到期 明天+0\nBid 112508306.IB 25中信银行CD306 2000w 2 到期 明天+0"}
```

成功应答结构示例（仅展示消费字段，债券名称、期限与警告由实际资料决定）：

```json
{"Code":"0000","Msg":"成功","TotalSize":2,"Datas":[{"securityId":"199385.IB","symbol":"25山东债91","side":"2","priceType":"3","price":2,"qty":1000,"settlType":"3"},{"securityId":"112508306.IB","symbol":"25中信银行CD306","side":"1","priceType":"3","price":2,"qty":2000,"settlType":"3"}],"Warnings":[]}
```

Parser 的 priceType：`1`净价、`2`全价、`3`到期收益率、`7`行权收益率；必须转换后写入 Quote.quoteType/yieldType。Parser 的 settlType `3/4` 在当前 STC 客户端分别按“明天+0/+1”处理，转换为次日 tradeDate 加 Quote.settlType `1/2`。**不能直接复制这两个清算编码。** 当前客户端用本机 DateTime.Today.AddDays(1)，不代表已实现交易日历的“下一工作日”。

traderId 映射为 Quote.contactId；institutionId 映射为 Quote.institutionId。返回 Warnings 时仍可保留识别预览，例如过期债券警告；确认是否允许另由 QuoteSvr 校验。Parser 返回的 intentionId、clordid、status 不是内部 quoteId/revision/status。

错误码：2001 用户名为空，2002 文本为空，2003 未识别到报价，2004 超过配置的识别条数，3999 异常。条数阈值读取 IntentionQuoteSize，错误枚举的文案仍写 200。不存在“每个输入行都有独立错误”的契约。

Parser 的旧服务还保留 image、deal、IRS 等处理器；当前销交客户端的自然语言报价链路只调用 stc.parser.bond，本文不把其他处理器列作已接入的产品功能。

<a id="price"></a>
## 4. Price：按用户视角生成的价格簿

`PriceBook` = securityId、可选 viewId、updateTime、bookRevision、marketData、quoteRefs、可选 calculationIssues。每个 view 的同一只债券可能有不同结果，缓存键应包含 viewId 和 securityId。

`marketData` 保留 STC 大写字段：SecurityID、Symbol、MarketIndicator、VenueTypeGW、Venues、NoMDEntries。条目包含 MDEntryID、MDEntryType、MDEntryPx、MDEntrySize、PriceType、ExerciseFlag、UserID、SettlType 等；quoteRefs 以条目标识关联 quoteId/quoteRevision。不能把行情条目 ID 当 quoteId，或把 Quote.side 数字直接套用 MDEntryType。

仅 ACTIVE、用户可见、符合条件的报价参与投影。FULL 返回全部，BEST 返回每侧最佳价档，DEPTH 返回每侧前 depth 个不同价档；同价档成员不会按条数截断。DEPTH 的 depth 范围 1–1000，其他 mode 不能传 depth（包括显式 null）。

当前投影要求可比收益率：有行权属性的债券要求 yieldType=2，否则要求 yieldType=1。输入价格口径不一致时，QuoteSvr 异步调用 `SERVER.CalSvr/stc.broker.calbond`，不在查询线程等待。等待、离线或换算失败的报价暂不参与行情排序，保留原报价，并通过每本价格簿的 `calculationIssues:[{quoteId,code}]` 返回问题；同页其他报价正常返回，不能将部分结果当作完整行情。换算成功后推送该券行情更新，不改变报价版本和确认状态。当前转换支持 T+0/T+1，其他清算代码暂返回 `CALCULATION_INVALID_INPUT`，不能将 STC CalSvr 的“明天+0”代码误当 FICC 的 T+2。收益率 Bid 按低到高、Ofr 按高到低排序，同价按 updateTime 倒序和 quoteId 排序。当前 MDEntryType 的 `0`=Bid、`1`=Ofr，PriceType 固定 `3`（收益率），ExerciseFlag 的 `0`=行权、`1`=到期。MDPriceLevel 当前按同侧条目序号填写，不等同于不同价格档序号。

换算问题码包括 `CALCULATION_PENDING`、`CALCULATOR_OFFLINE`、`CALCULATOR_BUSY`、`CALCULATION_FAILED`、`CALCULATION_INVALID_INPUT`、`CALCULATOR_NOT_CONFIGURED` 和 `CALCULATOR_STOPPED`。成功结果按报价输入、目标口径和资料版本复用；连接变化及 `stc.cal.clear` 使缓存失效。失败结果不会定时无限重试；新开行情流会允许重试失败项。`cal.requestTimeoutMillis=3000` 限制单次后台 DPS 请求，单线程后台队列由 `cal.queueCapacity=256` 限制；队列满只影响新待算报价，不阻塞查询。协议/容错已用替身验证，真实 CalSvr 换算须在 QA 验证。

行情条目的 `UserID` 是本方报价归属人的 STC userid，用用户目录解析为“长城交易员”；`InstitutionID/InstitutionName`、`TraderID/TraderName` 是对手机构与联系人。最优价页同价多笔时，汇总报量保留同价成员合计，机构、交易员等展示信息采用最新报价；时间取条目的 MDEntryDate/MDEntryTime，不能取整本价格簿 updateTime。

单次 `ficc.price.query` 必填 securityIds（最多 100）和 mode，实际输出 **`Data.prices`**。视图接口输出 **`Data.books`**，两个名称目前不一致。虽然 PriceQueryRequest 继承 PageRequest，这个单次接口不接受 limit/cursor。

<a id="recommendation"></a>
### 4.1 智能推荐（产品需求 4.9.2）

`ficc.recommendation.query` 与 `ficc.recommendation.feedback` 均由 QuoteSvr 处理，使用网关登录会话确定查看者，客户端不能传入或替换用户身份。

查询请求示例：

```json
{"requestId":"REC-QUERY-1","securityId":"2600004.IB","startDate":"2026-07-16","endDate":"2026-10-13"}
```

成功应答 `Data` 为 `RecommendationQueryData`：`securityId`、`startDate`、`endDate`、`scoringVersion`、`recommendations`。空证据返回空数组，不生成兜底候选。日期为业务时区 Asia/Shanghai 的自然日，首尾均包含，最多365天；无效窗口返回1001。

当前已确认的第一版规则 `quote-evidence-v1`：

- 只取所选债券的正式报价历史快照；包括所有用户、所有正式报价来源及已撤销/已过期报价此前的正式证据。待确认记录和从未确认便删除的报价不参与。
- 每个 quoteId 在所选窗口内取最新 ACTIVE 快照，更新不重复增加次数。
- 按债券、Bid/Ofr、本方归属用户、对手机构及联系人分组。相同对手但不同内部归属人不合并，以免混淆证据权限。
- 每条证据权重为 `0.5^(距当前时间的天数/30)`；未来时间的年龄按0计。得分 `100 × (1 - exp(-权重合计/5))`，保留2位小数，按得分降序、稳定条目标识排序。方向分别展示，不混算。
- 最新价格保留原价格类型，不把净价当成收益率。无真实用户归属成交存储，因此不使用成交数据。
- 本人证据返回机构/联系人姓名，其他用户的这两个字段为空；列表与展开证据执行相同规则。内部归属 userid 正常返回，客户端通过 STC 用户目录显示姓名。
- 推荐条目标识按分组稳定生成；反馈按“查看用户 + 推荐条目”唯一，切换日期窗口不会重置反馈。

反馈请求在查询字段基础上增加 `recommendationId` 和 `feedback`（`USEFUL`、`NOT_USEFUL`、`NOT_APPLICABLE`）。返回 `RecommendationFeedbackData`：`recommendationId`、`feedback`。重复提交返回第一次保存的结果，不覆盖、不改变推荐排序；不存在的候选返回1002。反馈写入 `recommendation_feedback`，表和历史查询索引由 QuoteSvr 启动自动创建，Jenkins 包无需手工补配置。

客户端选中债券后自动加载，默认 Bid；支持近7/30/90天、自定义日期、刷新、逐行展开证据和一次性反馈。切换日期或债券时取消旧请求的展示，迟到应答不能覆盖新选择。该模块不提供外发动作。产品4.9.2未定义相似券/相似机构算法，旧占位控件已移除。

部署需同步 common、QuoteSvr（含资源注册配置）和 STC 插件；两个接口的请求与响应样例见第 9 节。

<a id="filter"></a>
## 5. Filter、Watchlist、FilterGroup：筛选及用户配置

Filter = market + conditions + watchlistIds + filterGroupIds，均可省略；提供 market 时只能 RATE/CREDIT。

```json
{"market":"RATE","conditions":[{"field":"side","operator":"EQ","value":"1"},{"field":"remainingYears","operator":"BETWEEN","value":["1","10"]},{"field":"settlType","operator":"IN","value":["1","2"]}]}
```

同一字段的条件取 OR，不同字段取 AND；范围上下界放入一个 BETWEEN，不能拆成同字段 GTE 与 LTE。IN/BETWEEN 的 value 是数组，BETWEEN 必须两个有序边界；EXISTS 和布尔字段用 Boolean；枚举传协议值，不能传 UI 标签。

多个关注组之间 OR，多个筛选组之间 OR，这两类集合与临时条件再 AND。当前实现把筛选组的各 subgroup.conditions 合并后按字段运算；不要将 subgroup 名称理解成任意嵌套逻辑表达式。

通过 quote.dictionary.query / price.dictionary.query 读取 dictionaries、filterFields，字段的 operators、enumValues、enabled、simulated、source 是构建筛选控件的依据。**声明字段不等于外部数据源已填充**，缺数据的限制见[筛选字段来源与实现状态](product-filter-source-status.zh-CN.md)；运算细节见[筛选字典](product-filter-contract.zh-CN.md)。

Watchlist 包含 securities（securityId、displayOrder）；FilterGroup 包含 subgroups（subgroupId、name、displayOrder、conditions）。新增 save 省略组 ID；修改/删除必须传已返回 ID 和 expectedRevision。QuoteSvr 的 scope 为 MARKET/HISTORY、market 为 RATE/CREDIT。NotificationSvr 的筛选组是独立集合，不能混用 QuoteSvr 的 ID。

### 5.1 桌面分组管理与请求流程

关注分组和筛选分组均可新增、改名和删除，不是固定预置列表。配置按登录用户持久化到 QuoteSvr；行情中心与报价管理共享当前账户的配置。桌面当前加载 `scope=MARKET` 的组，不表示服务端不支持 HISTORY。

1. 通过 query 读取组 ID、revision 和完整内容，编辑器保留打开时的原始快照。
2. 新增省略组 ID；编辑提交完整组与原始 expectedRevision；删除提交组 ID 和 expectedRevision。删除组不删除债券或报价。取消编辑不写服务端。
3. 保存多个组时分别调用 save/delete，并非整个编辑器的一次原子事务。发生部分失败或 1004 冲突时重新读取，显示失败，不覆盖其他用户操作/其他窗口保存的版本。
4. 关注组添加债券先用 `ficc.base.get` 按明确代码读取资料；移除债券后保存完整 securities 列表。筛选组保存协议字段、操作符和类型正确的 value，不存控件显示标签。
5. 选择组后把 ID 放入查询或 view.open 的 filter，由服务端在分页前筛选。新增/删除后刷新组列表；不得仅过滤客户端已经加载的当前页。

服务端按每个筛选组自身的 market 限定匹配，避免合并行情中利率组命中信用报价。RATE 组加载/保存时清除 `localGovernmentRegion`、`issuerRating`、`marketMakerTags`、`perpetualType`、`isPerpetual` 历史禁用条件，并移除清理后的空子组。客户端可编辑条件还需服从字典 enabled 状态和产品允许范围。

组合请求中的 filter 示例（两个关注组 OR，再与一个筛选组及临时报价方向 AND）：

```json
{"market":"RATE","watchlistIds":["W-001","W-002"],"filterGroupIds":["FG-001"],"conditions":[{"field":"side","operator":"EQ","value":"1"}]}
```

改名仍需发送完整组内容，例如：

```json
{"requestId":"GROUP-RENAME-1","watchlistId":"W-001","expectedRevision":"1","name":"重点国开债","market":"RATE","scope":"MARKET","securities":[{"securityId":"240210.IB","displayOrder":0}]}
```

成功后使用响应的新 revision；不能接着用旧 revision 删除。新增、查询、保存和删除的应答结构见第 9 节。

<a id="crm"></a>
## 6. Base、CRM：基础资料与联系人

前端统一经 QuoteSvr 的 ficc.base.query/get；ReferenceRecord 根据 kind 返回证券、估值、发行人、机构或联系人字段。其他类型字段可为空，不应要求每条 ReferenceRecord 都包含全部字段。

机构搜索：kind=INSTITUTION、text；交易员搜索：kind=CONTACT、text，机构为空时省略 institutionId，跨机构匹配；选定机构时传 institutionId 限定范围。CRM 查询使用 offset/limit 分页，不使用普通 cursor；QuoteSvr 校验 offset 为 0–1000000、limit 为 1–200。

CONTACT 结果应读取 contactId、contactName、institutionId、institutionName、institutionIds 等。下拉显示“姓名 + 机构”；用户选中后同步保存联系人和机构的 ID/名称。输入文本但未选择不算匹配；换机构后清除不属于该机构的联系人。报价行和批量工具栏共用这条契约，工具栏只修改勾选行。多机构关系用返回的 institutionIds 判断，不以展示文本猜测归属。

base.get 的 ids 必填；CRM 批量最多 200。实际额外返回 missingIds、referenceVersion，Java BaseData 尚未声明这两个字段，但兼容编解码器会保留。未找到 ID 不一定导致整体错误，应检查 missingIds。

CRM 内部接口的 search/get 平铺返回 records/record，不套 Data；账号 image 订阅与 request/reply 是不同能力，详见内部接口表。

### 6.1 中证估值字段

中证资料由 BaseSvr 的估值资料经 QuoteSvr/common 映射到 ReferenceRecord，与中债字段分开：

| 源字段 | FICC 字段 | 口径 |
|---|---|---|
| yld_to_maturity | csiValuationYield | 到期收益率，百分数 |
| clean_price_maturity / full_price_maturity | csiValuationCleanPrice / csiValuationFullPrice | 到期净价 / 全价 |
| yld_to_exercise | csiExerciseYield | 行权收益率，百分数 |
| clean_price_exercise / full_price_exercise | csiExerciseCleanPrice / csiExerciseFullPrice | 行权净价 / 全价 |
| valuate_date | csiValuationDate | yyyy-MM-dd |
| recommendation | csiRecommendedBasis | 1→EXERCISE，2→MATURITY；未知为空 |
| implied_rating | csiRating | 中证隐含评级 |

数值按十进制字符串返回；缺值保留为空，不能补零或以中债替代中证。`csiValuationStatus` 为 LOADING、READY、FAILED，缓存加载成功但该券无记录时为 MISSING。无行权债券在有到期估值且推荐口径为空时可推导 MATURITY。当前 C# 强类型声明尚未覆盖全部中证字段，差异见第 11 节及字段表。

<a id="notification"></a>
## 7. Notification：提醒

NotificationRecord 是用户提醒，不是 QuoteRecord：notificationId、userId、securityId、active、content、contentToken、alertKey、revision、createTime/updateTime/readTime、quoteIds、reasons。

reasons 含类型、关联组/报价、ownQuoteId/otherQuoteId 等原因。readTime 为 null 表示未读。preference 四个开关分别为 watchlistEnabled、filterGroupEnabled、counterQuoteEnabled、myBestEnabled；保存时四个 Boolean 和 expectedRevision 都必须提供。

read 一批 1–200 个 notificationIds，read=true 标记已读，false 恢复未读；逐项检查 results。query/view 按当前用户、active 和 readState=ALL/READ/UNREAD 筛选；默认 ALL。来源尚未同步就绪时返回 1009，不能显示为“暂无数据”。

模型中有 types，但当前 NotificationService.select 没有应用它；view.open 的字段白名单还会拒绝 types。不要据模型声明提供“types 已生效”的保证。

### 7.1 关联报价、提醒原因与桌面行数

`quoteIds` 是该提醒关联的报价集合，不是“本次发生更新的报价 ID 列表”。当前实现按用户和债券重算匹配集合，一条报价更新后仍可能产生关联多笔报价的提醒。

| reason.type | 单个原因关联多笔报价的场景 |
|---|---|
| WATCHLIST | 关注债券下多笔可见有效报价 |
| FILTER_GROUP | 同券多笔报价符合所选筛选组 |
| MY_BEST | 本人多笔报价并列最优，或买卖两侧分别最优 |
| COUNTER_QUOTE | 本方一笔与另一归属人同券反方向一笔；关联两条 ID，当前不要求价格交叉 |

普通原因可合并成一个通知，其 quoteIds 是关联集合的并集。WATCHLIST/FILTER_GROUP 原因携带 groupIds，MY_BEST 原因携带 quoteIds，对价原因用 ownQuoteId/otherQuoteId。内部 contentToken 由关联 quoteId/revision 集合计算；关联版本变化可生成新的 notificationId。对外可见记录会去掉内部 active/contentToken 字段，不能因 DTO 声明就依赖其必定返回。

桌面报价提醒页按 **reasons 展开行**，不是按 quoteIds 展开；例如 3 个关联 quoteId、2 个 reason 会产生 2 行，共享 notificationId 和已读状态。弹窗按提醒类型合并，也不是一条 NotificationRecord 固定对应一个弹窗；当前每轮最多取 2 个新原因行。弹窗“查看”定位仍为占位提示，不能视为完整导航已实现。

当前没有单独的“触发本次提醒的报价 ID”字段。触发事件与关联上下文的进一步拆分仍属待改进项，本文不将它描述为已实现的业务规则。

<a id="views"></a>
## 8. Stream、View：订阅及分页

Quote、Price、Notification 三套频道独立，不能共用 sequence。推荐调用顺序：

1. stream.open 获取 sessionId、streamId、pushTopic，直接使用返回的 topic。
2. 建立订阅后再 view.open；应答前到达的推送先缓冲。
3. view.open 返回第一页、sequence 和相应记录；只处理该边界之后的增量。
4. view.query 传 viewId、pageNumber、pageSize，只返回指定页，不累积旧页。
5. 改条件：close 旧 view，用新 viewId open；离开页面 close，并释放无引用的订阅。
6. 重连、RESET_REQUIRED 或序列缺口：废弃旧视图/位置并重新建快照。

Quote/Notification 使用普通订阅；Price 的适配使用带 image 的订阅。Price image 可能没有任何 view，不能用空 image 认定市场无报价。请求应答没有固定优先于推送的时序。

服务端 pageSize 默认 200、范围 1–1000，pageNumber 从 1 开始；当前桌面 LivePage 另将页面尺寸限制在最多 100。totalCount 是全部匹配数量，ids 是当前页 ID。Quote/Price 每个领域 stream 最多 20 个活动 view，Notification 同样最多 20；同一 stream 生命周期中不要复用已使用的 viewId。

游标式 query 是另一种分页：limit 默认 200、通常 1–1000；返回 nextCursor 后，下一次只传 requestId、cursor，保持原 topic 和会话。QuoteSvr 普通结果上限 10000，游标有效期 120 秒；这些限制不能套用为所有 view 的总容量。

UPDATE 推送结构示例（记录数组省略具体对象，示例仅强调退出视图）：

```json
{"Code":"0000","Msg":"成功","kind":"UPDATE","streamId":"STREAM-Q","sequence":"8","quotes":[],"reports":[],"reference":[],"views":[{"viewId":"V-PENDING","totalCount":0,"upsertIds":[],"removeIds":["Q-001"]}]}
```

QuoteStreamBatch 使用 quotes/reports/reference；PriceStreamBatch 使用 books；NotificationStreamBatch 使用 notifications，可带 preference。三者的 views 均为 ViewDelta：viewId、totalCount、upsertIds、removeIds；不是整页 ids。报价版本保护不能取代流 sequence 校验。重复序列可忽略，缺口应重新同步；HEARTBEAT 不等于有新报价；RESET_REQUIRED 的 reason 用于排查。

第一页由客户端按服务排序合并、裁剪；移除已加载行后补查第一页。后续页保持稳定阅读，不将所有新记录插入当前页。确认一条报价后，即使用户仍勾选其他行，也必须处理该条的 removeIds，不能因选择状态跳过整个刷新。

<a id="endpoints"></a>
## 9. 前端接口逐项输入输出

以下逐项给出模型链接、最小/典型输入和成功 Data。列表示例使用空数组只表示结构，不代表线上无数据；写入示例需要先创建/读取实际记录，不能将示例 ID 直接用于生产。每个新的业务请求使用新的 requestId，重试同一次写入除外。

| 业务组 | 接口数量（不含推送） |
|---|---|
| 本节 FICC 前端请求 | 44 |
| Parser 文本识别（第 3 节） | 1 |
| 第 10 节内部/兼容/诊断 | 14 |

### 接口速查

| 用例 | topic | 输入模型 | 输出模型（实际差异见各项） |
|---|---|---|---|
| [创建待确认报价](#ficc.quote.create) | `ficc.quote.create` | [QuoteCreateRequest](api-models.zh-CN.md#quotecreaterequest) | [QuoteWriteData](api-models.zh-CN.md#quotewritedata) |
| [确认报价](#ficc.quote.confirm) | `ficc.quote.confirm` | [QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest) | [QuoteWriteData](api-models.zh-CN.md#quotewritedata) |
| [修改报价](#ficc.quote.update) | `ficc.quote.update` | [QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest) | [QuoteWriteData](api-models.zh-CN.md#quotewritedata) |
| [撤销有效报价](#ficc.quote.refer) | `ficc.quote.refer` | [QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest) | [QuoteWriteData](api-models.zh-CN.md#quotewritedata) |
| [恢复报价](#ficc.quote.unrefer) | `ficc.quote.unrefer` | [QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest) | [QuoteWriteData](api-models.zh-CN.md#quotewritedata) |
| [删除报价](#ficc.quote.delete) | `ficc.quote.delete` | [QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest) | [QuoteWriteData](api-models.zh-CN.md#quotewritedata) |
| [复制报价](#ficc.quote.clone) | `ficc.quote.clone` | [QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest) | [QuoteWriteData](api-models.zh-CN.md#quotewritedata) |
| [读取一条报价](#ficc.quote.get) | `ficc.quote.get` | [QuoteGetRequest](api-models.zh-CN.md#quotegetrequest) | [QuoteGetData](api-models.zh-CN.md#quotegetdata) |
| [按条件查询报价](#ficc.quote.query) | `ficc.quote.query` | [QuoteQueryRequest](api-models.zh-CN.md#quotequeryrequest) | [QuoteQueryData](api-models.zh-CN.md#quotequerydata) |
| [一条报价的变更历史](#ficc.quote.history.query) | `ficc.quote.history.query` | [QuoteHistoryRequest](api-models.zh-CN.md#quotehistoryrequest) | [QuoteHistoryData](api-models.zh-CN.md#quotehistorydata) |
| [历史报价筛选](#ficc.quote.history.search) | `ficc.quote.history.search` | [QuoteHistorySearchRequest](api-models.zh-CN.md#quotehistorysearchrequest) | [QuoteHistorySearchData](api-models.zh-CN.md#quotehistorysearchdata) |
| [查询来源原文组](#ficc.quote.sourceMessage.query) | `ficc.quote.sourceMessage.query` | [SourceMessageRequest](api-models.zh-CN.md#sourcemessagerequest) | [SourceMessageData](api-models.zh-CN.md#sourcemessagedata) |
| [打开报价流](#ficc.quote.stream.open) | `ficc.quote.stream.open` | [Request](api-models.zh-CN.md#request) | [StreamOpenData](api-models.zh-CN.md#streamopendata) |
| [登记quote视图](#ficc.quote.view.open) | `ficc.quote.view.open` | [QuoteViewOpenRequest](api-models.zh-CN.md#quoteviewopenrequest) | [QuoteViewData](api-models.zh-CN.md#quoteviewdata) |
| [查询quote视图指定页](#ficc.quote.view.query) | `ficc.quote.view.query` | [ViewQueryRequest](api-models.zh-CN.md#viewqueryrequest) | [QuoteViewData](api-models.zh-CN.md#quoteviewdata) |
| [关闭quote视图](#ficc.quote.view.close) | `ficc.quote.view.close` | [ViewCloseRequest](api-models.zh-CN.md#viewcloserequest) | [ViewCloseData](api-models.zh-CN.md#viewclosedata) |
| [打开行情流](#ficc.price.stream.open) | `ficc.price.stream.open` | [Request](api-models.zh-CN.md#request) | [StreamOpenData](api-models.zh-CN.md#streamopendata) |
| [登记price视图](#ficc.price.view.open) | `ficc.price.view.open` | [PriceViewOpenRequest](api-models.zh-CN.md#priceviewopenrequest) | [PriceViewData](api-models.zh-CN.md#priceviewdata) |
| [查询price视图指定页](#ficc.price.view.query) | `ficc.price.view.query` | [ViewQueryRequest](api-models.zh-CN.md#viewqueryrequest) | [PriceViewData](api-models.zh-CN.md#priceviewdata) |
| [关闭price视图](#ficc.price.view.close) | `ficc.price.view.close` | [ViewCloseRequest](api-models.zh-CN.md#viewcloserequest) | [ViewCloseData](api-models.zh-CN.md#viewclosedata) |
| [打开提醒流](#ficc.notification.stream.open) | `ficc.notification.stream.open` | [Request](api-models.zh-CN.md#request) | [StreamOpenData](api-models.zh-CN.md#streamopendata) |
| [登记notification视图](#ficc.notification.view.open) | `ficc.notification.view.open` | [NotificationViewOpenRequest](api-models.zh-CN.md#notificationviewopenrequest) | [NotificationViewData](api-models.zh-CN.md#notificationviewdata) |
| [查询notification视图指定页](#ficc.notification.view.query) | `ficc.notification.view.query` | [ViewQueryRequest](api-models.zh-CN.md#viewqueryrequest) | [NotificationViewData](api-models.zh-CN.md#notificationviewdata) |
| [关闭notification视图](#ficc.notification.view.close) | `ficc.notification.view.close` | [ViewCloseRequest](api-models.zh-CN.md#viewcloserequest) | [ViewCloseData](api-models.zh-CN.md#viewclosedata) |
| [按债券读取价格簿](#ficc.price.query) | `ficc.price.query` | [PriceQueryRequest](api-models.zh-CN.md#pricequeryrequest) | [PriceQueryData](api-models.zh-CN.md#pricequerydata) |
| [读取quote字典和可筛选字段](#ficc.quote.dictionary.query) | `ficc.quote.dictionary.query` | [DictionaryRequest](api-models.zh-CN.md#dictionaryrequest) | [DictionaryData](api-models.zh-CN.md#dictionarydata) |
| [读取price字典和可筛选字段](#ficc.price.dictionary.query) | `ficc.price.dictionary.query` | [DictionaryRequest](api-models.zh-CN.md#dictionaryrequest) | [DictionaryData](api-models.zh-CN.md#dictionarydata) |
| [查询watchlist](#ficc.watchlist.query) | `ficc.watchlist.query` | [WatchlistQueryRequest](api-models.zh-CN.md#watchlistqueryrequest) | [WatchlistData](api-models.zh-CN.md#watchlistdata) |
| [新增或修改watchlist](#ficc.watchlist.save) | `ficc.watchlist.save` | [WatchlistSaveRequest](api-models.zh-CN.md#watchlistsaverequest) | [WatchlistData](api-models.zh-CN.md#watchlistdata) |
| [删除watchlist](#ficc.watchlist.delete) | `ficc.watchlist.delete` | [WatchlistDeleteRequest](api-models.zh-CN.md#watchlistdeleterequest) | [WatchlistData](api-models.zh-CN.md#watchlistdata) |
| [查询filterGroup](#ficc.filterGroup.query) | `ficc.filterGroup.query` | [FilterGroupQueryRequest](api-models.zh-CN.md#filtergroupqueryrequest) | [FilterGroupData](api-models.zh-CN.md#filtergroupdata) |
| [新增或修改filterGroup](#ficc.filterGroup.save) | `ficc.filterGroup.save` | [FilterGroupSaveRequest](api-models.zh-CN.md#filtergroupsaverequest) | [FilterGroupData](api-models.zh-CN.md#filtergroupdata) |
| [删除filterGroup](#ficc.filterGroup.delete) | `ficc.filterGroup.delete` | [FilterGroupDeleteRequest](api-models.zh-CN.md#filtergroupdeleterequest) | [FilterGroupData](api-models.zh-CN.md#filtergroupdata) |
| [查询提醒filterGroup](#ficc.notification.filterGroup.query) | `ficc.notification.filterGroup.query` | [FilterGroupQueryRequest](api-models.zh-CN.md#filtergroupqueryrequest) | [FilterGroupData](api-models.zh-CN.md#filtergroupdata) |
| [新增或修改提醒filterGroup](#ficc.notification.filterGroup.save) | `ficc.notification.filterGroup.save` | [FilterGroupSaveRequest](api-models.zh-CN.md#filtergroupsaverequest) | [FilterGroupData](api-models.zh-CN.md#filtergroupdata) |
| [删除提醒filterGroup](#ficc.notification.filterGroup.delete) | `ficc.notification.filterGroup.delete` | [FilterGroupDeleteRequest](api-models.zh-CN.md#filtergroupdeleterequest) | [FilterGroupData](api-models.zh-CN.md#filtergroupdata) |
| [查询基础资料或 CRM](#ficc.base.query) | `ficc.base.query` | [BaseQueryRequest](api-models.zh-CN.md#basequeryrequest) | [BaseData](api-models.zh-CN.md#basedata) |
| [按 ID 读取资料](#ficc.base.get) | `ficc.base.get` | [BaseGetRequest](api-models.zh-CN.md#basegetrequest) | [BaseData](api-models.zh-CN.md#basedata) |
| [筛选提醒](#ficc.notification.query) | `ficc.notification.query` | [NotificationQueryRequest](api-models.zh-CN.md#notificationqueryrequest) | [NotificationQueryData](api-models.zh-CN.md#notificationquerydata) |
| [标记已读/未读](#ficc.notification.read) | `ficc.notification.read` | [NotificationReadRequest](api-models.zh-CN.md#notificationreadrequest) | [NotificationReadData](api-models.zh-CN.md#notificationreaddata) |
| [读取提醒偏好](#ficc.notification.preference.get) | `ficc.notification.preference.get` | [Request](api-models.zh-CN.md#request) | [NotificationPreferenceData](api-models.zh-CN.md#notificationpreferencedata) |
| [保存四类提醒开关](#ficc.notification.preference.save) | `ficc.notification.preference.save` | [NotificationPreferenceSaveRequest](api-models.zh-CN.md#notificationpreferencesaverequest) | [NotificationPreferenceData](api-models.zh-CN.md#notificationpreferencedata) |
| [查询智能推荐](#ficc.recommendation.query) | `ficc.recommendation.query` | [RecommendationQueryRequest](api-models.zh-CN.md#recommendationqueryrequest) | [RecommendationQueryData](api-models.zh-CN.md#recommendationquerydata) |
| [保存推荐反馈](#ficc.recommendation.feedback) | `ficc.recommendation.feedback` | [RecommendationFeedbackRequest](api-models.zh-CN.md#recommendationfeedbackrequest) | [RecommendationFeedbackData](api-models.zh-CN.md#recommendationfeedbackdata) |

<a id="ficc.quote.create"></a>
### 1. 创建待确认报价

`SERVER.QuoteSvr` · `ficc.quote.create`

输入模型：[QuoteCreateRequest](api-models.zh-CN.md#quotecreaterequest)；目录声明的输出模型：[QuoteWriteData](api-models.zh-CN.md#quotewritedata)。

普通前端不传 intake；返回的状态由 get/推送读取，为 PENDING_CONFIRM。创建不会直接进入价格簿。

输入：

```json
{"requestId":"R-001","quotes":[{"clientQuoteId":"C-001","securityId":"240210.IB","side":"1","quoteType":"1","yieldType":"1","price":"2.5000","quantity":"1000","quantityUnit":"WAN","currency":"CNY","tradeDate":"2026-10-12","settlType":"2","oco":false,"packing":false}]}
```

成功 Data：

```json
{"results":[{"quoteId":"Q-001","clientQuoteId":"C-001","revision":"1","Code":"0000","Msg":"成功"}]}
```

<a id="ficc.quote.confirm"></a>
### 2. 确认报价

`SERVER.QuoteSvr` · `ficc.quote.confirm`

输入模型：[QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest)；目录声明的输出模型：[QuoteWriteData](api-models.zh-CN.md#quotewritedata)。

仅 PENDING_CONFIRM；成功后 ACTIVE，可同时修改输入字段。

输入：

```json
{"requestId":"R-002","quotes":[{"quoteId":"Q-001","expectedRevision":"1","price":"2.5000"}]}
```

成功 Data：

```json
{"results":[{"quoteId":"Q-001","clientQuoteId":"C-001","revision":"2","Code":"0000","Msg":"成功"}]}
```

<a id="ficc.quote.update"></a>
### 3. 修改报价

`SERVER.QuoteSvr` · `ficc.quote.update`

输入模型：[QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest)；目录声明的输出模型：[QuoteWriteData](api-models.zh-CN.md#quotewritedata)。

仅 PENDING_CONFIRM/ACTIVE；省略的字段保留，显式 null 清空并重新校验。

输入：

```json
{"requestId":"R-003","quotes":[{"quoteId":"Q-001","expectedRevision":"1","price":"2.6000"}]}
```

成功 Data：

```json
{"results":[{"quoteId":"Q-001","clientQuoteId":"C-001","revision":"2","Code":"0000","Msg":"成功"}]}
```

<a id="ficc.quote.refer"></a>
### 4. 撤销有效报价

`SERVER.QuoteSvr` · `ficc.quote.refer`

输入模型：[QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest)；目录声明的输出模型：[QuoteWriteData](api-models.zh-CN.md#quotewritedata)。

仅 ACTIVE；变为 REFERRED，退出有效价格簿。

输入：

```json
{"requestId":"R-004","quotes":[{"quoteId":"Q-001","expectedRevision":"1","reason":"用户撤销"}]}
```

成功 Data：

```json
{"results":[{"quoteId":"Q-001","clientQuoteId":"C-001","revision":"2","Code":"0000","Msg":"成功"}]}
```

<a id="ficc.quote.unrefer"></a>
### 5. 恢复报价

`SERVER.QuoteSvr` · `ficc.quote.unrefer`

输入模型：[QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest)；目录声明的输出模型：[QuoteWriteData](api-models.zh-CN.md#quotewritedata)。

仅 REFERRED；恢复 ACTIVE 并校验必填字段。

输入：

```json
{"requestId":"R-005","quotes":[{"quoteId":"Q-001","expectedRevision":"1"}]}
```

成功 Data：

```json
{"results":[{"quoteId":"Q-001","clientQuoteId":"C-001","revision":"2","Code":"0000","Msg":"成功"}]}
```

<a id="ficc.quote.delete"></a>
### 6. 删除报价

`SERVER.QuoteSvr` · `ficc.quote.delete`

输入模型：[QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest)；目录声明的输出模型：[QuoteWriteData](api-models.zh-CN.md#quotewritedata)。

仅 PENDING_CONFIRM/REFERRED；变为 DELETED。

输入：

```json
{"requestId":"R-006","quotes":[{"quoteId":"Q-001","expectedRevision":"1","reason":"删除待确认"}]}
```

成功 Data：

```json
{"results":[{"quoteId":"Q-001","clientQuoteId":"C-001","revision":"2","Code":"0000","Msg":"成功"}]}
```

<a id="ficc.quote.clone"></a>
### 7. 复制报价

`SERVER.QuoteSvr` · `ficc.quote.clone`

输入模型：[QuoteChangeRequest](api-models.zh-CN.md#quotechangerequest)；目录声明的输出模型：[QuoteWriteData](api-models.zh-CN.md#quotewritedata)。

sourceQuoteId 和 clientQuoteId 是实际必填扩展字段，QuoteChangeItem 未显式声明；使用 JSON/扩展字段保留它们。来源 expectedRevision 必须匹配；新记录待确认。

输入：

```json
{"requestId":"R-007","quotes":[{"sourceQuoteId":"Q-001","expectedRevision":"2","clientQuoteId":"C-002"}]}
```

成功 Data：

```json
{"results":[{"quoteId":"Q-002","clientQuoteId":"C-002","revision":"1","Code":"0000","Msg":"成功"}]}
```

<a id="ficc.quote.get"></a>
### 8. 读取一条报价

`SERVER.QuoteSvr` · `ficc.quote.get`

输入模型：[QuoteGetRequest](api-models.zh-CN.md#quotegetrequest)；目录声明的输出模型：[QuoteGetData](api-models.zh-CN.md#quotegetdata)。

示例 QuoteRecord 节选；含完整审计字段。非归属用户能读取的公开 ACTIVE 报价会隐藏对手字段。

输入：

```json
{"requestId":"R-008","quoteId":"Q-001"}
```

成功 Data：

```json
{"quote":{"quoteId":"Q-001","clientQuoteId":"C-001","ownerUserId":"U-001","status":"ACTIVE","revision":"2","source":{"type":"MANUAL"},"securityId":"240210.IB","side":"1","quoteType":"1","yieldType":"1","price":"2.5000","quantity":"1000","quantityUnit":"WAN","currency":"CNY","tradeDate":"2026-10-12","settlType":"2","oco":false,"packing":false}}
```

<a id="ficc.quote.query"></a>
### 9. 按条件查询报价

`SERVER.QuoteSvr` · `ficc.quote.query`

输入模型：[QuoteQueryRequest](api-models.zh-CN.md#quotequeryrequest)；目录声明的输出模型：[QuoteQueryData](api-models.zh-CN.md#quotequerydata)。

limit 默认 200，上限 1000；状态默认 ACTIVE，范围默认 VISIBLE。reference 当前按整次查询的债券集合补充，并非只裁剪当前页，不能当另一批报价。

输入：

```json
{"requestId":"R-009","ownerScope":"MINE","status":["PENDING_CONFIRM"],"limit":50,"filter":{"conditions":[{"field":"side","operator":"EQ","value":"1"}]}}
```

成功 Data：

```json
{"quotes":[],"reference":[],"totalCount":0,"nextCursor":null}
```

<a id="ficc.quote.history.query"></a>
### 10. 一条报价的变更历史

`SERVER.QuoteSvr` · `ficc.quote.history.query`

输入模型：[QuoteHistoryRequest](api-models.zh-CN.md#quotehistoryrequest)；目录声明的输出模型：[QuoteHistoryData](api-models.zh-CN.md#quotehistorydata)。

records 元素是 QuoteHistoryRecord，含 before/after、action、操作人、操作时间、reason。要求归属权限；示例 before/after 为节选。

输入：

```json
{"requestId":"R-010","quoteId":"Q-001","limit":50}
```

成功 Data：

```json
{"totalCount":1,"nextCursor":null,"quoteId":"Q-001","records":[{"eventId":"EV-001","quoteId":"Q-001","revision":"2","action":"CONFIRM","operatorUserId":"U-001","operationTime":"2026-10-12T10:00:00+08:00","before":{"quoteId":"Q-001","revision":"1","status":"PENDING_CONFIRM"},"after":{"quoteId":"Q-001","revision":"2","status":"ACTIVE"}}]}
```

<a id="ficc.quote.history.search"></a>
### 11. 历史报价筛选

`SERVER.QuoteSvr` · `ficc.quote.history.search`

输入模型：[QuoteHistorySearchRequest](api-models.zh-CN.md#quotehistorysearchrequest)；目录声明的输出模型：[QuoteHistorySearchData](api-models.zh-CN.md#quotehistorysearchdata)。

rows 元素是 QuoteHistoryRow，含操作日期 tradeDate、quote、reference（示例节选）。ownerScope 虽声明但当前查询固定使用归属权限。

输入：

```json
{"requestId":"R-011","fromDate":"2026-10-01","toDate":"2026-10-12","limit":50,"filter":{"market":"RATE"}}
```

成功 Data：

```json
{"totalCount":1,"nextCursor":null,"rows":[{"tradeDate":"2026-10-12","quote":{"quoteId":"Q-001","securityId":"240210.IB","status":"ACTIVE","revision":"2"},"reference":{"securityId":"240210.IB","securityName":"24国开10"}}]}
```

<a id="ficc.quote.sourceMessage.query"></a>
### 12. 查询来源原文组

`SERVER.QuoteSvr` · `ficc.quote.sourceMessage.query`

输入模型：[SourceMessageRequest](api-models.zh-CN.md#sourcemessagerequest)；目录声明的输出模型：[SourceMessageData](api-models.zh-CN.md#sourcemessagedata)。

按 sourceMessageId 查询；不传则列出有权访问的原文。quoteId 字段当前不参与筛选；来源字段示例为节选。

输入：

```json
{"requestId":"R-012","sourceMessageId":"SM-001","limit":50}
```

成功 Data：

```json
{"totalCount":1,"nextCursor":null,"sourceMessages":[{"sourceMessageId":"SM-001","sourceType":"QT","ownerUserId":"U-001","text":"240210 2.5 1000 bid +1","quoteIds":["Q-001"]}]}
```

<a id="ficc.quote.stream.open"></a>
### 13. 打开报价流

`SERVER.QuoteSvr` · `ficc.quote.stream.open`

输入模型：[Request](api-models.zh-CN.md#request)；目录声明的输出模型：[StreamOpenData](api-models.zh-CN.md#streamopendata)。

会话级流；先取得 topic 并订阅，再登记 view。重新登录须重新建立。

输入：

```json
{"requestId":"R-013"}
```

成功 Data：

```json
{"sessionId":"SID-EXAMPLE","streamId":"STREAM-quote","pushTopic":"ficc.quote.report.SID-EXAMPLE"}
```

<a id="ficc.quote.view.open"></a>
### 14. 登记quote视图

`SERVER.QuoteSvr` · `ficc.quote.view.open`

输入模型：[QuoteViewOpenRequest](api-models.zh-CN.md#quoteviewopenrequest)；目录声明的输出模型：[QuoteViewData](api-models.zh-CN.md#quoteviewdata)。

返回第一页及边界 sequence；详见 Stream/View。

输入：

```json
{"requestId":"R-014","viewId":"V-quote","pageSize":50,"filter":{},"ownerScope":"MINE","status":["PENDING_CONFIRM"]}
```

成功 Data：

```json
{"streamId":"STREAM-quote","sequence":"0","view":{"viewId":"V-quote","pageNumber":1,"pageSize":50,"totalCount":0,"ids":[]},"quotes":[]}
```

<a id="ficc.quote.view.query"></a>
### 15. 查询quote视图指定页

`SERVER.QuoteSvr` · `ficc.quote.view.query`

输入模型：[ViewQueryRequest](api-models.zh-CN.md#viewqueryrequest)；目录声明的输出模型：[QuoteViewData](api-models.zh-CN.md#quoteviewdata)。

三个字段必填；复用登记的条件，不接受在此修改 filter。超范围返回空页和真实 totalCount。

输入：

```json
{"requestId":"R-015","viewId":"V-quote","pageNumber":1,"pageSize":50}
```

成功 Data：

```json
{"streamId":"STREAM-quote","sequence":"0","view":{"viewId":"V-quote","pageNumber":1,"pageSize":50,"totalCount":0,"ids":[]},"quotes":[]}
```

<a id="ficc.quote.view.close"></a>
### 16. 关闭quote视图

`SERVER.QuoteSvr` · `ficc.quote.view.close`

输入模型：[ViewCloseRequest](api-models.zh-CN.md#viewcloserequest)；目录声明的输出模型：[ViewCloseData](api-models.zh-CN.md#viewclosedata)。

只关闭逻辑视图，不代表物理订阅已经取消；客户端另释放订阅。

输入：

```json
{"requestId":"R-016","viewId":"V-quote"}
```

成功 Data：

```json
{"viewId":"V-quote","closed":true}
```

<a id="ficc.price.stream.open"></a>
### 17. 打开行情流

`SERVER.QuoteSvr` · `ficc.price.stream.open`

输入模型：[Request](api-models.zh-CN.md#request)；目录声明的输出模型：[StreamOpenData](api-models.zh-CN.md#streamopendata)。

会话级流；先取得 topic 并订阅，再登记 view。重新登录须重新建立。

输入：

```json
{"requestId":"R-017"}
```

成功 Data：

```json
{"sessionId":"SID-EXAMPLE","streamId":"STREAM-price","pushTopic":"ficc.price.report.SID-EXAMPLE"}
```

<a id="ficc.price.view.open"></a>
### 18. 登记price视图

`SERVER.QuoteSvr` · `ficc.price.view.open`

输入模型：[PriceViewOpenRequest](api-models.zh-CN.md#priceviewopenrequest)；目录声明的输出模型：[PriceViewData](api-models.zh-CN.md#priceviewdata)。

返回第一页及边界 sequence；详见 Stream/View。mode 必填；DEPTH 需 depth。

输入：

```json
{"requestId":"R-018","viewId":"V-price","pageSize":50,"filter":{},"mode":"BEST"}
```

成功 Data：

```json
{"streamId":"STREAM-price","sequence":"0","view":{"viewId":"V-price","pageNumber":1,"pageSize":50,"totalCount":0,"ids":[]},"books":[]}
```

<a id="ficc.price.view.query"></a>
### 19. 查询price视图指定页

`SERVER.QuoteSvr` · `ficc.price.view.query`

输入模型：[ViewQueryRequest](api-models.zh-CN.md#viewqueryrequest)；目录声明的输出模型：[PriceViewData](api-models.zh-CN.md#priceviewdata)。

三个字段必填；复用登记的条件，不接受在此修改 filter。超范围返回空页和真实 totalCount。

输入：

```json
{"requestId":"R-019","viewId":"V-price","pageNumber":1,"pageSize":50}
```

成功 Data：

```json
{"streamId":"STREAM-price","sequence":"0","view":{"viewId":"V-price","pageNumber":1,"pageSize":50,"totalCount":0,"ids":[]},"books":[]}
```

<a id="ficc.price.view.close"></a>
### 20. 关闭price视图

`SERVER.QuoteSvr` · `ficc.price.view.close`

输入模型：[ViewCloseRequest](api-models.zh-CN.md#viewcloserequest)；目录声明的输出模型：[ViewCloseData](api-models.zh-CN.md#viewclosedata)。

只关闭逻辑视图，不代表物理订阅已经取消；客户端另释放订阅。

输入：

```json
{"requestId":"R-020","viewId":"V-price"}
```

成功 Data：

```json
{"viewId":"V-price","closed":true}
```

<a id="ficc.notification.stream.open"></a>
### 21. 打开提醒流

`SERVER.NotificationSvr` · `ficc.notification.stream.open`

输入模型：[Request](api-models.zh-CN.md#request)；目录声明的输出模型：[StreamOpenData](api-models.zh-CN.md#streamopendata)。

会话级流；先取得 topic 并订阅，再登记 view。重新登录须重新建立。

输入：

```json
{"requestId":"R-021"}
```

成功 Data：

```json
{"sessionId":"SID-EXAMPLE","streamId":"STREAM-notification","pushTopic":"ficc.notification.report.SID-EXAMPLE"}
```

<a id="ficc.notification.view.open"></a>
### 22. 登记notification视图

`SERVER.NotificationSvr` · `ficc.notification.view.open`

输入模型：[NotificationViewOpenRequest](api-models.zh-CN.md#notificationviewopenrequest)；目录声明的输出模型：[NotificationViewData](api-models.zh-CN.md#notificationviewdata)。

返回第一页及边界 sequence；详见 Stream/View。

输入：

```json
{"requestId":"R-022","viewId":"V-notification","pageSize":50,"filter":{},"readState":"UNREAD"}
```

成功 Data：

```json
{"streamId":"STREAM-notification","sequence":"0","view":{"viewId":"V-notification","pageNumber":1,"pageSize":50,"totalCount":0,"ids":[]},"notifications":[]}
```

<a id="ficc.notification.view.query"></a>
### 23. 查询notification视图指定页

`SERVER.NotificationSvr` · `ficc.notification.view.query`

输入模型：[ViewQueryRequest](api-models.zh-CN.md#viewqueryrequest)；目录声明的输出模型：[NotificationViewData](api-models.zh-CN.md#notificationviewdata)。

三个字段必填；复用登记的条件，不接受在此修改 filter。超范围返回空页和真实 totalCount。

输入：

```json
{"requestId":"R-023","viewId":"V-notification","pageNumber":1,"pageSize":50}
```

成功 Data：

```json
{"streamId":"STREAM-notification","sequence":"0","view":{"viewId":"V-notification","pageNumber":1,"pageSize":50,"totalCount":0,"ids":[]},"notifications":[]}
```

<a id="ficc.notification.view.close"></a>
### 24. 关闭notification视图

`SERVER.NotificationSvr` · `ficc.notification.view.close`

输入模型：[ViewCloseRequest](api-models.zh-CN.md#viewcloserequest)；目录声明的输出模型：[ViewCloseData](api-models.zh-CN.md#viewclosedata)。

只关闭逻辑视图，不代表物理订阅已经取消；客户端另释放订阅。

输入：

```json
{"requestId":"R-024","viewId":"V-notification"}
```

成功 Data：

```json
{"viewId":"V-notification","closed":true}
```

<a id="ficc.price.query"></a>
### 25. 按债券读取价格簿

`SERVER.QuoteSvr` · `ficc.price.query`

输入模型：[PriceQueryRequest](api-models.zh-CN.md#pricequeryrequest)；目录声明的输出模型：[PriceQueryData](api-models.zh-CN.md#pricequerydata)。

返回价格簿节选。实际字段 prices，非模型声明的 books；无 limit/cursor。无有效报价时 NoMDEntries=[]，用于清除旧价格。

输入：

```json
{"requestId":"R-025","securityIds":["240210.IB"],"mode":"BEST","filter":{}}
```

成功 Data：

```json
{"prices":[{"securityId":"240210.IB","bookRevision":"1","marketData":{"SecurityID":"240210.IB","NoMDEntries":[{"MDEntryID":"FICC:Q-001","MDEntryType":"0","MDEntryPx":"2.5000","MDEntrySize":"1000","PriceType":"3","ExerciseFlag":"1","SettlType":"2","UserID":"U-001"}]},"quoteRefs":{"FICC:Q-001":{"quoteId":"Q-001","quoteRevision":"2"}}}]}
```

<a id="ficc.quote.dictionary.query"></a>
### 26. 读取quote字典和可筛选字段

`SERVER.QuoteSvr` · `ficc.quote.dictionary.query`

输入模型：[DictionaryRequest](api-models.zh-CN.md#dictionaryrequest)；目录声明的输出模型：[DictionaryData](api-models.zh-CN.md#dictionarydata)。

示例标签仅说明值/名称结构；以实际 label 为准。names 只裁剪 dictionaries，filterFields 仍为完整目录；省略 names 返回全部字典。

输入：

```json
{"requestId":"R-026","names":["side"]}
```

成功 Data：

```json
{"dictionaries":{"side":[{"value":"1","label":"Bid"},{"value":"2","label":"Ofr"}]},"filterFields":[]}
```

<a id="ficc.price.dictionary.query"></a>
### 27. 读取price字典和可筛选字段

`SERVER.QuoteSvr` · `ficc.price.dictionary.query`

输入模型：[DictionaryRequest](api-models.zh-CN.md#dictionaryrequest)；目录声明的输出模型：[DictionaryData](api-models.zh-CN.md#dictionarydata)。

示例标签仅说明值/名称结构；以实际 label 为准。names 只裁剪 dictionaries，filterFields 仍为完整目录；省略 names 返回全部字典。

输入：

```json
{"requestId":"R-027","names":["side"]}
```

成功 Data：

```json
{"dictionaries":{"side":[{"value":"1","label":"Bid"},{"value":"2","label":"Ofr"}]},"filterFields":[]}
```

<a id="ficc.watchlist.query"></a>
### 28. 查询watchlist

`SERVER.QuoteSvr` · `ficc.watchlist.query`

输入模型：[WatchlistQueryRequest](api-models.zh-CN.md#watchlistqueryrequest)；目录声明的输出模型：[WatchlistData](api-models.zh-CN.md#watchlistdata)。

仅当前用户。可选组 ID 查询单个（输出仍为复数数组），market/scope 缩小范围。

输入：

```json
{"requestId":"R-028"}
```

成功 Data：

```json
{"watchlists":[]}
```

<a id="ficc.watchlist.save"></a>
### 29. 新增或修改watchlist

`SERVER.QuoteSvr` · `ficc.watchlist.save`

输入模型：[WatchlistSaveRequest](api-models.zh-CN.md#watchlistsaverequest)；目录声明的输出模型：[WatchlistData](api-models.zh-CN.md#watchlistdata)。

示例为新增；修改须另传 watchlistId 和 expectedRevision，提交完整组内容。服务补齐 ID、排序及时间。

输入：

```json
{"requestId":"R-029","name":"我的测试组","market":"RATE","scope":"MARKET","securities":[{"securityId":"240210.IB","displayOrder":0}]}
```

成功 Data：

```json
{"watchlist":{"name":"我的测试组","market":"RATE","scope":"MARKET","securities":[{"securityId":"240210.IB","displayOrder":0}],"watchlistId":"W-001","revision":"1","userId":"U-001"}}
```

<a id="ficc.watchlist.delete"></a>
### 30. 删除watchlist

`SERVER.QuoteSvr` · `ficc.watchlist.delete`

输入模型：[WatchlistDeleteRequest](api-models.zh-CN.md#watchlistdeleterequest)；目录声明的输出模型：[WatchlistData](api-models.zh-CN.md#watchlistdata)。

只删除用户配置组，不删除组内报价或债券。

输入：

```json
{"requestId":"R-030","watchlistId":"W-001","expectedRevision":"1"}
```

成功 Data：

```json
{"watchlistId":"W-001","deleted":true}
```

<a id="ficc.filterGroup.query"></a>
### 31. 查询filterGroup

`SERVER.QuoteSvr` · `ficc.filterGroup.query`

输入模型：[FilterGroupQueryRequest](api-models.zh-CN.md#filtergroupqueryrequest)；目录声明的输出模型：[FilterGroupData](api-models.zh-CN.md#filtergroupdata)。

仅当前用户。可选组 ID 查询单个（输出仍为复数数组），market/scope 缩小范围。

输入：

```json
{"requestId":"R-031"}
```

成功 Data：

```json
{"filterGroups":[]}
```

<a id="ficc.filterGroup.save"></a>
### 32. 新增或修改filterGroup

`SERVER.QuoteSvr` · `ficc.filterGroup.save`

输入模型：[FilterGroupSaveRequest](api-models.zh-CN.md#filtergroupsaverequest)；目录声明的输出模型：[FilterGroupData](api-models.zh-CN.md#filtergroupdata)。

示例为新增；修改须另传 filterGroupId 和 expectedRevision，提交完整组内容。服务补齐 ID、排序及时间；空条件子组会被去除。

输入：

```json
{"requestId":"R-032","name":"我的测试组","market":"RATE","scope":"MARKET","subgroups":[{"name":"期限","conditions":[{"field":"remainingYears","operator":"BETWEEN","value":["1","10"]}]}]}
```

成功 Data：

```json
{"filterGroup":{"name":"我的测试组","market":"RATE","scope":"MARKET","subgroups":[{"name":"期限","conditions":[{"field":"remainingYears","operator":"BETWEEN","value":["1","10"]}]}],"filterGroupId":"FG-001","revision":"1","userId":"U-001"}}
```

<a id="ficc.filterGroup.delete"></a>
### 33. 删除filterGroup

`SERVER.QuoteSvr` · `ficc.filterGroup.delete`

输入模型：[FilterGroupDeleteRequest](api-models.zh-CN.md#filtergroupdeleterequest)；目录声明的输出模型：[FilterGroupData](api-models.zh-CN.md#filtergroupdata)。

只删除用户配置组，不删除组内报价或债券。

输入：

```json
{"requestId":"R-033","filterGroupId":"FG-001","expectedRevision":"1"}
```

成功 Data：

```json
{"filterGroupId":"FG-001","deleted":true}
```

<a id="ficc.notification.filterGroup.query"></a>
### 34. 查询提醒filterGroup

`SERVER.NotificationSvr` · `ficc.notification.filterGroup.query`

输入模型：[FilterGroupQueryRequest](api-models.zh-CN.md#filtergroupqueryrequest)；目录声明的输出模型：[FilterGroupData](api-models.zh-CN.md#filtergroupdata)。

仅当前用户。当前实现返回用户全部组，忽略请求中的组 ID/market/scope。

输入：

```json
{"requestId":"R-034"}
```

成功 Data：

```json
{"filterGroups":[]}
```

<a id="ficc.notification.filterGroup.save"></a>
### 35. 新增或修改提醒filterGroup

`SERVER.NotificationSvr` · `ficc.notification.filterGroup.save`

输入模型：[FilterGroupSaveRequest](api-models.zh-CN.md#filtergroupsaverequest)；目录声明的输出模型：[FilterGroupData](api-models.zh-CN.md#filtergroupdata)。

示例为新增；修改须另传 filterGroupId 和 expectedRevision，提交完整组内容。服务补齐 ID、排序及时间；空条件子组会被去除。

输入：

```json
{"requestId":"R-035","name":"我的测试组","market":"RATE","scope":"NOTIFICATION","subgroups":[{"name":"期限","conditions":[{"field":"remainingYears","operator":"BETWEEN","value":["1","10"]}]}]}
```

成功 Data：

```json
{"filterGroup":{"name":"我的测试组","market":"RATE","scope":"NOTIFICATION","subgroups":[{"name":"期限","conditions":[{"field":"remainingYears","operator":"BETWEEN","value":["1","10"]}]}],"filterGroupId":"FG-001","revision":"1","userId":"U-001"}}
```

<a id="ficc.notification.filterGroup.delete"></a>
### 36. 删除提醒filterGroup

`SERVER.NotificationSvr` · `ficc.notification.filterGroup.delete`

输入模型：[FilterGroupDeleteRequest](api-models.zh-CN.md#filtergroupdeleterequest)；目录声明的输出模型：[FilterGroupData](api-models.zh-CN.md#filtergroupdata)。

只删除用户配置组，不删除组内报价或债券。

输入：

```json
{"requestId":"R-036","filterGroupId":"FG-001","expectedRevision":"1"}
```

成功 Data：

```json
{"filterGroupId":"FG-001","deleted":true}
```

<a id="ficc.base.query"></a>
### 37. 查询基础资料或 CRM

`SERVER.QuoteSvr` · `ficc.base.query`

输入模型：[BaseQueryRequest](api-models.zh-CN.md#basequeryrequest)；目录声明的输出模型：[BaseData](api-models.zh-CN.md#basedata)。

机构为空的跨机构交易员示例；所选机构不为空时增加 institutionId。证券查询用 kind=SECURITY，可用普通游标分页；CRM 用 offset 分页，不返回总数，结束时 nextOffset=null。

输入：

```json
{"requestId":"R-037","kind":"CONTACT","text":"李","limit":30,"offset":0}
```

成功 Data：

```json
{"records":[{"contactId":"CT-001","contactName":"李某","institutionId":"I-001","institutionName":"示例机构","institutionIds":["I-001"]}],"hasMore":false,"nextOffset":null}
```

<a id="ficc.base.get"></a>
### 38. 按 ID 读取资料

`SERVER.QuoteSvr` · `ficc.base.get`

输入模型：[BaseGetRequest](api-models.zh-CN.md#basegetrequest)；目录声明的输出模型：[BaseData](api-models.zh-CN.md#basedata)。

kind 可用于 SECURITY、DAILY_VALUATION、ISSUER、INSTITUTION、CONTACT；实际可用数据由对应适配器提供。

输入：

```json
{"requestId":"R-038","kind":"INSTITUTION","ids":["I-001"]}
```

成功 Data：

```json
{"records":[{"institutionId":"I-001","institutionName":"示例机构"}],"missingIds":[],"referenceVersion":"EXAMPLE-VERSION"}
```

<a id="ficc.notification.query"></a>
### 39. 筛选提醒

`SERVER.NotificationSvr` · `ficc.notification.query`

输入模型：[NotificationQueryRequest](api-models.zh-CN.md#notificationqueryrequest)；目录声明的输出模型：[NotificationQueryData](api-models.zh-CN.md#notificationquerydata)。

当前仅活动提醒；types 声明存在但未实现过滤；来源同步时可返回 1009。

输入：

```json
{"requestId":"R-039","readState":"UNREAD","filter":{},"limit":50}
```

成功 Data：

```json
{"notifications":[],"totalCount":0,"nextCursor":null}
```

<a id="ficc.notification.read"></a>
### 40. 标记已读/未读

`SERVER.NotificationSvr` · `ficc.notification.read`

输入模型：[NotificationReadRequest](api-models.zh-CN.md#notificationreadrequest)；目录声明的输出模型：[NotificationReadData](api-models.zh-CN.md#notificationreaddata)。

每批 1–200；read=false 时 readTime 清空；可能部分记录 1002。

输入：

```json
{"requestId":"R-040","notificationIds":["N-001"],"read":true}
```

成功 Data：

```json
{"results":[{"notificationId":"N-001","Code":"0000","Msg":"成功","readTime":"2026-10-12T10:00:00+08:00"}]}
```

<a id="ficc.notification.preference.get"></a>
### 41. 读取提醒偏好

`SERVER.NotificationSvr` · `ficc.notification.preference.get`

输入模型：[Request](api-models.zh-CN.md#request)；目录声明的输出模型：[NotificationPreferenceData](api-models.zh-CN.md#notificationpreferencedata)。

首次使用默认值由服务返回；保存前必须先取得 revision，不硬编码版本。

输入：

```json
{"requestId":"R-041"}
```

成功 Data：

```json
{"preference":{"revision":"1","watchlistEnabled":true,"filterGroupEnabled":true,"counterQuoteEnabled":true,"myBestEnabled":true}}
```

<a id="ficc.notification.preference.save"></a>
### 42. 保存四类提醒开关

`SERVER.NotificationSvr` · `ficc.notification.preference.save`

输入模型：[NotificationPreferenceSaveRequest](api-models.zh-CN.md#notificationpreferencesaverequest)；目录声明的输出模型：[NotificationPreferenceData](api-models.zh-CN.md#notificationpreferencedata)。

四个 Boolean 必须全部提供；版本不匹配返回 1004。

输入：

```json
{"requestId":"R-042","expectedRevision":"1","watchlistEnabled":true,"filterGroupEnabled":true,"counterQuoteEnabled":true,"myBestEnabled":true}
```

成功 Data：

```json
{"preference":{"revision":"2","watchlistEnabled":true,"filterGroupEnabled":true,"counterQuoteEnabled":true,"myBestEnabled":true}}
```

<a id="ficc.recommendation.query"></a>
### 43. 查询智能推荐

`SERVER.QuoteSvr` · `ficc.recommendation.query`

输入模型：[RecommendationQueryRequest](api-models.zh-CN.md#recommendationqueryrequest)；输出模型：[RecommendationQueryData](api-models.zh-CN.md#recommendationquerydata)。规则见[智能推荐](#recommendation)。

输入：

```json
{"requestId":"REC-QUERY-1","securityId":"2600004.IB","startDate":"2026-10-07","endDate":"2026-10-13"}
```

成功 Data：

```json
{
  "securityId": "2600004.IB",
  "startDate": "2026-10-07",
  "endDate": "2026-10-13",
  "scoringVersion": "quote-evidence-v1",
  "recommendations": [
    {
      "recommendationId": "REC-EXAMPLE",
      "securityId": "2600004.IB",
      "securityName": "26超长期特别国债04",
      "side": "1",
      "score": 18.13,
      "quoteCount": 1,
      "institutionName": "示例机构",
      "contactName": "示例联系人",
      "ownerUserId": "U-001",
      "latestPrice": "2.0000",
      "quoteType": "1",
      "yieldType": "1",
      "latestQuantity": "1000",
      "latestTime": "2026-10-13T10:00:00+08:00",
      "feedback": null,
      "evidence": [
        {
          "quoteId": "Q-001",
          "ownerUserId": "U-001",
          "securityId": "2600004.IB",
          "securityName": "26超长期特别国债04",
          "side": "1",
          "price": "2.0000",
          "quoteType": "1",
          "yieldType": "1",
          "quantity": "1000",
          "updateTime": "2026-10-13T10:00:00+08:00",
          "institutionName": "示例机构",
          "contactName": "示例联系人"
        }
      ]
    }
  ]
}
```

该样例假设查看者为 U-001，计分时刻与证据更新时间相同；无证据时 recommendations 返回空数组。其他用户证据的 institutionName/contactName 为 null。

<a id="ficc.recommendation.feedback"></a>
### 44. 保存推荐反馈

`SERVER.QuoteSvr` · `ficc.recommendation.feedback`

输入模型：[RecommendationFeedbackRequest](api-models.zh-CN.md#recommendationfeedbackrequest)；输出模型：[RecommendationFeedbackData](api-models.zh-CN.md#recommendationfeedbackdata)。规则见[智能推荐](#recommendation)。

输入：

```json
{"requestId":"REC-FEEDBACK-1","securityId":"2600004.IB","startDate":"2026-10-07","endDate":"2026-10-13","recommendationId":"REC-EXAMPLE","feedback":"USEFUL"}
```

成功 Data：

```json
{"recommendationId":"REC-EXAMPLE","feedback":"USEFUL"}
```

推荐查询不接受分页参数，返回全部候选与 evidence，桌面自行分页。反馈必须使用查询实际返回的 recommendationId；请求中的日期窗口仍须有效且能查询到该条目。

<a id="internal"></a>
## 10. 内部、兼容及诊断接口

本节补齐 SalesTradingApi 注册目录，便于查代码；不代表这些入口都允许桌面普通用户访问。state/event 用于状态同步；内部无登录读入口依赖后端网络边界，不是前端绕过登录的方式。

当前真实 QT 接入使用 `ficc.quote.create` 的 intake 分支（同一 topic，不另立前端 API）：请求携带 QuoteCreateRequest.intake，由 IntakeCreateEndpoint 在普通登录分派前处理，来源身份、提案复用、分组和撤回由该处理器校验。IntakeIdentity 继承 QuoteSource；这是内部输入，前端 create 不应携带。`ficc.quote.intake.apply` 则是另一个仍存在的模拟入口，**不能当作真实 QT 接入实现**。

<a id="ficc.quote.state.query"></a>
### 45. quote全量状态同步

`SERVER.QuoteSvr` · `ficc.quote.state.query`

输入模型：[StateQueryRequest](api-models.zh-CN.md#statequeryrequest)；目录声明的输出模型：[QuoteStateData](api-models.zh-CN.md#quotestatedata)。

内部身份限定；后续使用 cursor。不要将 collections 的声明理解为已经支持按集合裁剪，当前状态构建未使用它。

输入：

```json
{"requestId":"R-043","limit":200}
```

成功 Data：

```json
{"epoch":"E-001","sequence":"0","totalCount":0,"records":{"quotes":[],"watchlists":[],"filterGroups":[]},"nextCursor":null,"done":true}
```

<a id="ficc.quote.event.query"></a>
### 46. quote事件补齐

`SERVER.QuoteSvr` · `ficc.quote.event.query`

输入模型：[EventQueryRequest](api-models.zh-CN.md#eventqueryrequest)；目录声明的输出模型：[QuoteEventBatch](api-models.zh-CN.md#quoteeventbatch)。

内部身份限定；epoch 不匹配或位置无效返回 1007，重新取状态。sequence 是持久化事件位置，不是页面 stream.sequence。

输入：

```json
{"requestId":"R-044","epoch":"E-001","afterSequence":"0","limit":200}
```

成功 Data：

```json
{"epoch":"E-001","events":[],"nextSequence":"0","seenSequence":"0","hasMore":false}
```

<a id="ficc.notification.state.query"></a>
### 47. notification全量状态同步

`SERVER.NotificationSvr` · `ficc.notification.state.query`

输入模型：[StateQueryRequest](api-models.zh-CN.md#statequeryrequest)；目录声明的输出模型：[NotificationStateData](api-models.zh-CN.md#notificationstatedata)。

内部身份限定；后续使用 cursor。不要将 collections 的声明理解为已经支持按集合裁剪，当前状态构建未使用它。

输入：

```json
{"requestId":"R-045","limit":200}
```

成功 Data：

```json
{"epoch":"E-001","sequence":"0","totalCount":0,"records":{"notifications":[],"preferences":[],"filterGroups":[]},"nextCursor":null,"done":true}
```

<a id="ficc.notification.event.query"></a>
### 48. notification事件补齐

`SERVER.NotificationSvr` · `ficc.notification.event.query`

输入模型：[EventQueryRequest](api-models.zh-CN.md#eventqueryrequest)；目录声明的输出模型：[NotificationEventBatch](api-models.zh-CN.md#notificationeventbatch)。

内部身份限定；epoch 不匹配或位置无效返回 1007，重新取状态。sequence 是持久化事件位置，不是页面 stream.sequence。

输入：

```json
{"requestId":"R-046","epoch":"E-001","afterSequence":"0","limit":200}
```

成功 Data：

```json
{"epoch":"E-001","events":[],"nextSequence":"0","seenSequence":"0","hasMore":false}
```

<a id="ficc.quote.internal.state.query"></a>
### 49. 后端专用报价快照

`SERVER.QuoteSvr` · `ficc.quote.internal.state.query`

输入模型：[StateQueryRequest](api-models.zh-CN.md#statequeryrequest)；目录声明的输出模型：[QuoteStateData](api-models.zh-CN.md#quotestatedata)。

QuoteResource 专用内部读路径；NotificationSvr 启动同步使用。

输入：

```json
{"requestId":"R-047","limit":200}
```

成功 Data：

```json
{"epoch":"E-001","sequence":"0","records":{"quotes":[],"watchlists":[],"filterGroups":[]},"done":true,"nextCursor":null,"totalCount":0}
```

<a id="ficc.price.internal.snapshot.query"></a>
### 50. 后端专用行情快照

`SERVER.QuoteSvr` · `ficc.price.internal.snapshot.query`

输入模型：[Request](api-models.zh-CN.md#request)；目录声明的输出模型：[PriceFeedBatch](api-models.zh-CN.md#pricefeedbatch)。

PriceFeedBatch；服务间价格源，不代表某个普通用户的可见行情。epoch 在 quotePosition 中，勿用模型中未赋值的顶层 epoch。

输入：

```json
{"requestId":"R-048"}
```

成功 Data：

```json
{"Code":"0000","Msg":"成功","kind":"SNAPSHOT","streamId":"INTERNAL-PRICE","sequence":"0","quotePosition":{"epoch":"E-001","sequence":"0"},"prices":[]}
```

<a id="ficc.quote.intake.apply"></a>
### 51. 历史模拟接入入口

`SERVER.QuoteSvr` · `ficc.quote.intake.apply`

输入模型：[IntakeApplyRequest](api-models.zh-CN.md#intakeapplyrequest)；目录声明的输出模型：[IntakeApplyData](api-models.zh-CN.md#intakeapplydata)。

仅 S-INTAKE，固定 QT-U-88→U-001；仅 NEW 和已有待确认 UPDATE；不用于部署后的真实 QT 业务。

输入：

```json
{"requestId":"R-049","sourceType":"QT","externalMessageId":"EV-001","sourceUserId":"QT-U-88","sourceTime":"2026-10-12T10:00:00+08:00","text":"模拟报价","quotes":[{"externalQuoteId":"EXT-001","externalRevision":"1","action":"NEW","securityId":"240210.IB","side":"1","quoteType":"1","yieldType":"1","price":"2.5000","quantity":"1000","quantityUnit":"WAN","currency":"CNY","tradeDate":"2026-10-12","settlType":"2","oco":false,"packing":false}]}
```

成功 Data：

```json
{"sourceMessageId":"SM-001","results":[{"quoteId":"Q-001","clientQuoteId":"C-001","revision":"1","Code":"0000","Msg":"成功"}]}
```

<a id="ficc.qtintake.status.query"></a>
### 52. FIX 接入状态诊断

`SERVER.QtIntakeSvr` · `ficc.qtintake.status.query`

输入模型：[Request](api-models.zh-CN.md#request)；目录声明的输出模型：[QtIntakeStatusData](api-models.zh-CN.md#qtintakestatusdata)。

这些布尔状态不是某条报价成功入库/确认的证明。

输入：

```json
{"requestId":"R-050"}
```

成功 Data：

```json
{"fixConnected":true,"storageFailed":false,"quoteSvrOnline":true}
```

<a id="stc.crm.search"></a>
### 53. CRM 内部搜索

`SERVER.CRMSvr` · `stc.crm.search`

输入模型：[CrmSearchRequest](api-models.zh-CN.md#crmsearchrequest)；目录声明的输出模型：[BaseData](api-models.zh-CN.md#basedata)。

成功平铺返回，不套 Data，不返回 totalCount；不要求普通 FICC requestId。机构限定与跨机构搜索同前文。

输入：

```json
{"kind":"CONTACT","text":"李","offset":0,"limit":20}
```

成功应答（平铺）：

```json
{"records":[],"hasMore":false,"nextOffset":null}
```

<a id="stc.crm.get"></a>
### 54. CRM 内部单条读取

`SERVER.CRMSvr` · `stc.crm.get`

输入模型：[CrmGetRequest](api-models.zh-CN.md#crmgetrequest)；目录声明的输出模型：[CrmGetData](api-models.zh-CN.md#crmgetdata)。

成功平铺返回 record，未找到可以为 null。

输入：

```json
{"kind":"CONTACT","id":"CT-001"}
```

成功应答（平铺）：

```json
{"record":null}
```

<a id="stc.crm.lookup"></a>
### 55. 旧 CRM 名称/ID 查找

`SERVER.CRMSvr` · `stc.crm.lookup`

输入模型：[CrmLookupRequest](api-models.zh-CN.md#crmlookuprequest)；目录声明的输出模型：[CrmLegacyData](api-models.zh-CN.md#crmlegacydata)。

kind 为 INSTITUTION/CONTACT；id 优先于 name；match=exact 优先精确比较简称、再全称，其余值走忽略大小写的包含搜索。旧输出字段为 namecn、instid、instname 等。

输入：

```json
{"kind":"CONTACT","name":"李","match":"contains","PageSize":20}
```

成功应答（平铺）：

```json
{"Code":"0000","Msg":"","totalSize":0,"pageSize":0,"datas":[]}
```

<a id="stc.crm.inst.query"></a>
### 56. 旧 CRM inst查询/镜像

`SERVER.CRMSvr` · `stc.crm.inst.query`

输入模型：[CrmLookupRequest](api-models.zh-CN.md#crmlookuprequest)；目录声明的输出模型：[CrmLegacyData](api-models.zh-CN.md#crmlegacydata)。

inst/contact 支持请求及 image；account 当前仅 image，普通 request 返回下述错误。账户 image 由 CrmDirectory.accountImage 返回 BrokerCrmAccountQueryResult，当前为空目录，不套普通 FICC Data。

输入：

```json
{"dealerStr":"U-001","PageSize":200}
```

成功应答（平铺）：

```json
{"Code":"0000","Msg":"","totalSize":0,"pageSize":0,"datas":[]}
```

<a id="stc.crm.contact.query"></a>
### 57. 旧 CRM contact查询/镜像

`SERVER.CRMSvr` · `stc.crm.contact.query`

输入模型：[CrmLookupRequest](api-models.zh-CN.md#crmlookuprequest)；目录声明的输出模型：[CrmLegacyData](api-models.zh-CN.md#crmlegacydata)。

inst/contact 支持请求及 image；account 当前仅 image，普通 request 返回下述错误。账户 image 由 CrmDirectory.accountImage 返回 BrokerCrmAccountQueryResult，当前为空目录，不套普通 FICC Data。

输入：

```json
{"dealerStr":"U-001","PageSize":200}
```

成功应答（平铺）：

```json
{"Code":"0000","Msg":"","totalSize":0,"pageSize":0,"datas":[]}
```

<a id="stc.crm.account.query"></a>
### 58. 旧 CRM account查询/镜像

`SERVER.CRMSvr` · `stc.crm.account.query`

输入模型：[CrmLookupRequest](api-models.zh-CN.md#crmlookuprequest)；目录声明的输出模型：[CrmLegacyData](api-models.zh-CN.md#crmlegacydata)。

inst/contact 支持请求及 image；account 当前仅 image，普通 request 返回下述错误。账户 image 由 CrmDirectory.accountImage 返回 BrokerCrmAccountQueryResult，当前为空目录，不套普通 FICC Data。

输入：

```json
{"dealerStr":"U-001","PageSize":200}
```

实际错误应答：

```json
{"Code":"1002","Msg":"Unsupported topic: stc.crm.account.query","error":{"type":"UNSUPPORTED_TOPIC","field":"topic"}}
```

<a id="gaps"></a>
## 11. 已发现的契约差异与验证边界

| 位置 | 代码现状及调用影响 |
|---|---|
| PriceQueryData | 模型声明 books；处理器实际返回 prices。模型继承分页字段，但入口拒绝 limit/cursor |
| QuoteChangeItem / clone | 未声明 sourceQuoteId/clientQuoteId，clone 却要求它们；须经 JSON 扩展字段发送 |
| BaseData | missingIds/referenceVersion 是实际输出扩展，模型未显式声明 |
| ParserReply | Java DTO 未声明 Warnings/TotalSize；C# 声明 Warnings，服务实际可返回两者 |
| ReferenceRecord | 八个新增中证字段仅 Java 声明，详见字段表；C# 兼容扩展读取不等于强类型属性已齐全 |
| Java/C# MarketEntry | 2026-10-12 行情身份修复已对齐 PriceType、ExerciseFlag、UserID、MDPriceLevel，并增加 InstitutionName、TraderName；详见模型字段表 |
| ParserQuote | 只是桌面消费字段子集；服务实际序列化 TableIntentionQuote，价格/数量为 BigDecimal 数值 |
| Notification 的 types | query 忽略它，view.open 白名单拒绝它；不能宣称支持按 types 筛选 |
| ViewPage.hasMore | 当前三类 view 应答未写此字段，应依据 pageNumber/pageSize/totalCount 判断后续页 |
| PageData.hasMore | 普通游标查询未写此字段，应以 nextCursor 是否为 null 判断；CRM 和事件查询另有真实 hasMore |
| QuoteViewData.reference / NotificationViewData.preference | 当前视图快照未填这两个声明字段；资料另查 base，偏好另查 preference.get |
| SourceMessageRequest.quoteId | 当前 sourceMessage.query 未使用；请使用 sourceMessageId |
| QuoteHistorySearchRequest.ownerScope | 当前历史固定按 access.owns 处理，不使用该参数扩展可见范围 |
| NotificationFilterGroupQueryRequest | 复用通用请求模型，但处理器只按用户取全部组，未按 ID/market/scope 筛选 |
| StateQueryRequest.collections | 声明存在，不等于当前状态接口支持集合裁剪 |
| stc.crm.account.query | 注册和 handler 存在，request 分派无相应分支，image 路径有实现 |
| 精确报价提交 | create → get → confirm，仍有待确认中间态；不能承诺识别确认是一次原子写入 |

以上差异本次只记录，不修改业务代码。ProtocolCodec 能保留未知 wire 字段，因此 DTO 缺字段不等于线上一定丢字段，也不代表使用强类型属性就能直接读到它。

### 源码依据

- [推荐与反馈](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/RecommendationService.java)、[分组条件清理](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/FilterGroupRules.java)、[桌面分组持久化](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/DpsGroupStore.cs)、[中证映射](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/client/StcBaseMapper.java)。

- [Java 接口目录](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/SalesTradingApi.java)、[协议转换边界](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ProtocolBoundary.java)、[C# 接口目录](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/SalesTradingApi.cs)。
- [QuoteService](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/QuoteService.java)、[QuoteResource](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/QuoteResource.java)、[PriceBookService](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/PriceBookService.java)、[ViewService](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/ViewService.java)。
- [QuoteMatcher](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/QuoteMatcher.java)、[FilterEngine](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/FilterEngine.java)、[AccessPolicy](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/client/AccessPolicy.java)、[LocalSearchSvrClient](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/LocalSearchSvrClient.java)。
- [NotificationService](../iteration1/cgws-ficc-saletrading-notificationsvr/src/main/java/ficc/notification/NotificationService.java)、[CrmResource](../iteration1/cgws-ficc-saletrading-crmsvr/src/main/java/ficc/crm/CrmResource.java)、[BondParserHandler](../iteration1/cgws-ficc-parsersvr/src/main/java/sc/app/stc/parser/handler/BondParserHandler.java)。
- [解析结果映射](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/StcQuoteParserClient.cs)、[精确报价提交](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/ParsedQuoteSubmitter.cs)、[客户端分页和推送](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/LivePage.cs)。

本次验证范围：静态核对服务目录、处理器、客户端调用和模型，并检查文档接口覆盖、JSON 语法、模型字段及本地链接。Java/C# topic 目录均为 58 项，另有 Parser 文本识别入口。现有 `python iteration1/scripts/check-app-protocol.py` 在 ParserReply.Warnings 双端不一致处失败；进一步字段比对发现 ReferenceRecord 的八个中证字段尚未声明于 C#。检查脚本本身也尚未配置 RecommendationRecord.score 的 Double 类型映射。不能据此声称全量协议检查通过。本次只更新文档，未启动服务、发送业务请求或重跑桌面 E2E；不保证 QA 部署版本已同步。
