# FICC 协议模型字段表

更新日期：2026-10-12；基线 `cc16316`。配套 [API 文档](ficc-api-spec.zh-CN.md)。

本表逐项列出 Java `ficc.api` 声明的业务字段，按模型分组；继承字段通过父模型链接展开，避免重复复制。类型是 Java 声明类型，不自动表示必填、默认值或实际 JSON 标量类型。必填条件、可用字段、枚举、实际响应扩展及未实现项以主文档和处理器为准。

`String`→字符串，`Boolean`→布尔，`Integer`→整数，`List<T>`→数组，`Map<String,T>`→对象映射，`Object`→条件标量/数组等。null 与省略字段不同。Parser 的 BigDecimal 数值与兼容层保留的原始标量特别见主文档。

模型有字段不代表可写；记录、推送、只读资料字段不能整对象回传给严格写入接口。ReferenceRecord 是多种资料的联合字段集合，不是每种资料都返回全部字段。

## 实际 wire 扩展

| 模型/接口 | 字段与实际类型 | 说明 |
|---|---|---|
| QuoteChangeItem / clone | sourceQuoteId: string；clientQuoteId: string | clone 必填，当前 DTO 未声明 |
| PriceQueryData | prices: PriceBook[] | 实际返回 prices，不是 books |
| BaseData / base.get | missingIds: string[]；referenceVersion: string | 按 ID 读取的实际输出 |
| ParserReply | Warnings: string[]；TotalSize: integer | 服务返回，Java DTO 未声明；C# 有 Warnings |
| ParserQuote | price、qty、chinaBondValuation: number | 实际由 TableIntentionQuote 的 BigDecimal 序列化；DTO 中是 String |
| MarketEntry 双端声明 | PriceType、ExerciseFlag、UserID、MDPriceLevel: string | 2026-10-12 行情身份显示修复已补齐 C# 声明；并新增 InstitutionName、TraderName |

Parser 的完整旧响应对象还包含 intentionId、clordid、各类标志及估值扩展；其字段清单见本文末尾 TableIntentionQuote，不能视为可直接写入 QuoteSvr 的对象。

## 模型索引

- 公共封装与分页：[ProtocolModel](#protocolmodel)、[Request](#request)、[Reply](#reply)、[ApiReply](#apireply)、[EmptyData](#emptydata)、[ErrorDetail](#errordetail)、[PageRequest](#pagerequest)、[PageData](#pagedata)、[EventPosition](#eventposition)
- 报价及来源：[QuoteChangeItem](#quotechangeitem)、[QuoteChangeRequest](#quotechangerequest)、[QuoteCreateItem](#quotecreateitem)、[QuoteCreateRequest](#quotecreaterequest)、[QuoteGetData](#quotegetdata)、[QuoteGetRequest](#quotegetrequest)、[QuoteHistoryData](#quotehistorydata)、[QuoteHistoryRecord](#quotehistoryrecord)、[QuoteHistoryRequest](#quotehistoryrequest)、[QuoteHistoryRow](#quotehistoryrow)、[QuoteHistorySearchData](#quotehistorysearchdata)、[QuoteHistorySearchRequest](#quotehistorysearchrequest)、[QuoteInput](#quoteinput)、[QuoteOperationResult](#quoteoperationresult)、[QuoteQueryData](#quotequerydata)、[QuoteQueryRequest](#quotequeryrequest)、[QuoteRecord](#quoterecord)、[QuoteReference](#quotereference)、[QuoteReport](#quotereport)、[QuoteSource](#quotesource)、[QuoteWriteData](#quotewritedata)、[SourceMessage](#sourcemessage)、[SourceMessageRequest](#sourcemessagerequest)、[SourceMessageData](#sourcemessagedata)
- 行情：[PriceBook](#pricebook)、[MarketData](#marketdata)、[MarketEntry](#marketentry)、[PriceQueryRequest](#pricequeryrequest)、[PriceQueryData](#pricequerydata)
- 筛选、关注与字典：[DictionaryData](#dictionarydata)、[DictionaryOption](#dictionaryoption)、[DictionaryRequest](#dictionaryrequest)、[Filter](#filter)、[FilterCondition](#filtercondition)、[FilterField](#filterfield)、[FilterGroup](#filtergroup)、[FilterGroupData](#filtergroupdata)、[FilterGroupDeleteRequest](#filtergroupdeleterequest)、[FilterGroupQueryRequest](#filtergroupqueryrequest)、[FilterGroupSaveRequest](#filtergroupsaverequest)、[FilterSubgroup](#filtersubgroup)、[Watchlist](#watchlist)、[WatchlistData](#watchlistdata)、[WatchlistDeleteRequest](#watchlistdeleterequest)、[WatchlistQueryRequest](#watchlistqueryrequest)、[WatchlistSaveRequest](#watchlistsaverequest)、[WatchlistSecurity](#watchlistsecurity)
- 资料与 CRM：[BaseData](#basedata)、[BaseGetRequest](#basegetrequest)、[BaseQueryRequest](#basequeryrequest)、[CrmGetData](#crmgetdata)、[CrmGetRequest](#crmgetrequest)、[CrmLegacyData](#crmlegacydata)、[CrmLegacyRecord](#crmlegacyrecord)、[CrmLookupRequest](#crmlookuprequest)、[CrmSearchRequest](#crmsearchrequest)、[ReferenceRecord](#referencerecord)
- 识别：[ParserRequest](#parserrequest)、[ParserReply](#parserreply)、[ParserQuote](#parserquote)
- 提醒：[NotificationPreference](#notificationpreference)、[NotificationPreferenceData](#notificationpreferencedata)、[NotificationPreferenceSaveRequest](#notificationpreferencesaverequest)、[NotificationQueryData](#notificationquerydata)、[NotificationQueryRequest](#notificationqueryrequest)、[NotificationReadData](#notificationreaddata)、[NotificationReadRequest](#notificationreadrequest)、[NotificationReadResult](#notificationreadresult)、[NotificationReason](#notificationreason)、[NotificationRecord](#notificationrecord)
- 视图与推送：[NotificationStreamBatch](#notificationstreambatch)、[NotificationViewData](#notificationviewdata)、[NotificationViewOpenRequest](#notificationviewopenrequest)、[PriceStreamBatch](#pricestreambatch)、[PriceViewData](#priceviewdata)、[PriceViewOpenRequest](#priceviewopenrequest)、[QuoteStreamBatch](#quotestreambatch)、[QuoteViewData](#quoteviewdata)、[QuoteViewOpenRequest](#quoteviewopenrequest)、[StreamMessage](#streammessage)、[StreamOpenData](#streamopendata)、[ViewCloseData](#viewclosedata)、[ViewCloseRequest](#viewcloserequest)、[ViewDelta](#viewdelta)、[ViewPage](#viewpage)、[ViewQueryRequest](#viewqueryrequest)
- 内部状态、事件及接入：[EventQueryRequest](#eventqueryrequest)、[IntakeApplyData](#intakeapplydata)、[IntakeApplyRequest](#intakeapplyrequest)、[IntakeIdentity](#intakeidentity)、[IntakeQuoteItem](#intakequoteitem)、[NotificationEvent](#notificationevent)、[NotificationEventBatch](#notificationeventbatch)、[NotificationEventRecord](#notificationeventrecord)、[NotificationStateData](#notificationstatedata)、[NotificationStateRecords](#notificationstaterecords)、[QtIntakeStatusData](#qtintakestatusdata)、[QuoteEvent](#quoteevent)、[QuoteEventBatch](#quoteeventbatch)、[QuoteStateData](#quotestatedata)、[QuoteStateRecords](#quotestaterecords)、[StateQueryRequest](#statequeryrequest)、[SettingRecord](#settingrecord)、[PriceFeedBatch](#pricefeedbatch)

## 公共封装与分页

<a id="protocolmodel"></a>
### ProtocolModel

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ProtocolModel.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ProtocolModel.cs)

无直接声明的业务字段；ProtocolModel 的兼容元数据不发送到网络。
<a id="request"></a>
### Request

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/Request.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/Request.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `requestId` | `String` | 请求关联标识；写回执用途见主文档 |
<a id="reply"></a>
### Reply

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/Reply.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/Reply.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `Code` | `String` | 业务返回码，字符串 |
| `Msg` | `String` | 服务端说明 |
| `requestId` | `String` | 请求关联标识；写回执用途见主文档 |
| `error` | `ErrorDetail` | 错误详情 |
<a id="apireply"></a>
### ApiReply

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ApiReply.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ApiReply.cs)；继承 [Reply](#reply) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `Data` | `T` | 成功数据，类型由 topic 决定 |
<a id="emptydata"></a>
### EmptyData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/EmptyData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/EmptyData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

无直接声明的业务字段；ProtocolModel 的兼容元数据不发送到网络。
<a id="errordetail"></a>
### ErrorDetail

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ErrorDetail.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ErrorDetail.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `type` | `String` | 类型；依所属模型区分 |
| `field` | `String` | 协议字段名 |
| `quoteId` | `String` | 内部报价主键 |
| `expectedRevision` | `String` | 调用方预期的当前版本 |
| `currentRevision` | `String` | 服务端当前版本，用于冲突提示 |
<a id="pagerequest"></a>
### PageRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PageRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PageRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `cursor` | `String` | 不透明游标，不能自行拼接 |
| `limit` | `Integer` | 游标或搜索单批大小；有效范围由入口决定 |
<a id="pagedata"></a>
### PageData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PageData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PageData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `nextCursor` | `String` | 下一页游标；无后续时 null |
| `hasMore` | `Boolean` | 是否还有后续结果 |
| `totalCount` | `Integer` | 完整匹配结果数量 |
<a id="eventposition"></a>
### EventPosition

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/EventPosition.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/EventPosition.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `epoch` | `String` | 持久化事件代次 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |

## 报价及来源

<a id="quotechangeitem"></a>
### QuoteChangeItem

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteChangeItem.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteChangeItem.cs)；继承 [QuoteInput](#quoteinput) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quoteId` | `String` | 内部报价主键 |
| `expectedRevision` | `String` | 调用方预期的当前版本 |
| `reason` | `String` | 动作原因或重置原因 |
<a id="quotechangerequest"></a>
### QuoteChangeRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteChangeRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteChangeRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quotes` | `List<QuoteChangeItem>` | 报价数组 |
<a id="quotecreateitem"></a>
### QuoteCreateItem

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteCreateItem.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteCreateItem.cs)；继承 [QuoteInput](#quoteinput) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `clientQuoteId` | `String` | 调用方创建项标识 |
<a id="quotecreaterequest"></a>
### QuoteCreateRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteCreateRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteCreateRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quotes` | `List<QuoteCreateItem>` | 报价数组 |
| `intake` | `IntakeIdentity` | 内部来源身份及事件信息，普通前端不传 |
<a id="quotegetdata"></a>
### QuoteGetData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteGetData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteGetData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quote` | `QuoteRecord` | 报价对象 |
<a id="quotegetrequest"></a>
### QuoteGetRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteGetRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteGetRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quoteId` | `String` | 内部报价主键 |
<a id="quotehistorydata"></a>
### QuoteHistoryData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteHistoryData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteHistoryData.cs)；继承 [PageData](#pagedata) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quoteId` | `String` | 内部报价主键 |
| `records` | `List<QuoteHistoryRecord>` | 记录集合；结构依所属模型而定 |
<a id="quotehistoryrecord"></a>
### QuoteHistoryRecord

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteHistoryRecord.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteHistoryRecord.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `eventId` | `String` | 事件标识 |
| `quoteId` | `String` | 内部报价主键 |
| `revision` | `String` | 当前记录版本，十进制字符串 |
| `action` | `String` | 业务动作 |
| `operatorUserId` | `String` | 操作人 STC userid |
| `operationTime` | `String` | 操作时间 |
| `before` | `QuoteRecord` | 操作前报价快照 |
| `after` | `QuoteRecord` | 操作后报价快照 |
| `reason` | `String` | 动作原因或重置原因 |
<a id="quotehistoryrequest"></a>
### QuoteHistoryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteHistoryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteHistoryRequest.cs)；继承 [PageRequest](#pagerequest) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quoteId` | `String` | 内部报价主键 |
<a id="quotehistoryrow"></a>
### QuoteHistoryRow

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteHistoryRow.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteHistoryRow.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `tradeDate` | `String` | 交易日期；历史行该字段实际为操作日期 |
| `quote` | `QuoteRecord` | 报价对象 |
| `reference` | `ReferenceRecord` | 关联基础资料 |
<a id="quotehistorysearchdata"></a>
### QuoteHistorySearchData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteHistorySearchData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteHistorySearchData.cs)；继承 [PageData](#pagedata) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `rows` | `List<QuoteHistoryRow>` | 历史报价与资料的组合行 |
<a id="quotehistorysearchrequest"></a>
### QuoteHistorySearchRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteHistorySearchRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteHistorySearchRequest.cs)；继承 [PageRequest](#pagerequest) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `fromDate` | `String` | 历史范围开始日 |
| `toDate` | `String` | 历史范围结束日 |
| `ownerScope` | `String` | VISIBLE / MINE；部分历史入口未使用 |
| `filter` | `Filter` | 组合筛选条件 |
<a id="quoteinput"></a>
### QuoteInput

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteInput.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteInput.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `securityId` | `String` | 债券代码 |
| `institutionId` | `String` | CRM 机构 ID |
| `institutionName` | `String` | 机构名称 |
| `contactId` | `String` | CRM 联系人 ID |
| `contactName` | `String` | 联系人姓名 |
| `side` | `String` | 报价方向：1 Bid / 2 Ofr |
| `quoteType` | `String` | Quote 计价类型；不要直接复制 Parser 编码 |
| `yieldType` | `String` | Quote 收益率类型：1 到期 / 2 行权 |
| `price` | `String` | 价格或收益率，具体单位随计价类型 |
| `quantity` | `String` | 报价量，当前 WAN（万） |
| `quantityUnit` | `String` | 数量单位，当前 WAN |
| `currency` | `String` | 币种，当前 CNY |
| `tradeDate` | `String` | 交易日期；历史行该字段实际为操作日期 |
| `settlType` | `String` | 清算编码；Parser 与 Quote 语义不同，见主文档 |
| `urgent` | `Boolean` | 急 |
| `recommended` | `Boolean` | 荐 |
| `whole` | `Boolean` | 整量 |
| `oco` | `Boolean` | OCO 标记 |
| `packing` | `Boolean` | 打包标记，与 oco 互斥 |
| `remark` | `String` | 备注 |
<a id="quoteoperationresult"></a>
### QuoteOperationResult

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteOperationResult.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteOperationResult.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quoteId` | `String` | 内部报价主键 |
| `clientQuoteId` | `String` | 调用方创建项标识 |
| `revision` | `String` | 当前记录版本，十进制字符串 |
| `Code` | `String` | 业务返回码，字符串 |
| `Msg` | `String` | 服务端说明 |
| `error` | `ErrorDetail` | 错误详情 |
<a id="quotequerydata"></a>
### QuoteQueryData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteQueryData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteQueryData.cs)；继承 [PageData](#pagedata) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quotes` | `List<QuoteRecord>` | 报价数组 |
| `reference` | `List<ReferenceRecord>` | 关联基础资料 |
<a id="quotequeryrequest"></a>
### QuoteQueryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteQueryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteQueryRequest.cs)；继承 [PageRequest](#pagerequest) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `ownerScope` | `String` | VISIBLE / MINE；部分历史入口未使用 |
| `status` | `List<String>` | 业务状态；不同模型不得混用状态枚举 |
| `filter` | `Filter` | 组合筛选条件 |
<a id="quoterecord"></a>
### QuoteRecord

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteRecord.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteRecord.cs)；继承 [QuoteInput](#quoteinput) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quoteId` | `String` | 内部报价主键 |
| `clonedFromQuoteId` | `String` | 复制来源的内部报价 ID |
| `clientQuoteId` | `String` | 调用方创建项标识 |
| `sourceMessageId` | `String` | 内部来源原文组 ID |
| `source` | `QuoteSource` | 报价来源 |
| `ownerUserId` | `String` | 报价所属 STC userid |
| `status` | `String` | 业务状态；不同模型不得混用状态枚举 |
| `revision` | `String` | 当前记录版本，十进制字符串 |
| `createdBy` | `String` | 创建操作人 |
| `updatedBy` | `String` | 最后修改操作人 |
| `confirmedBy` | `String` | 确认操作人 |
| `createTime` | `String` | 创建时间 |
| `updateTime` | `String` | 最后更新时间 |
| `quoteTime` | `String` | 有效报价时间 |
| `confirmTime` | `String` | 确认时间 |
<a id="quotereference"></a>
### QuoteReference

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteReference.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteReference.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quoteId` | `String` | 内部报价主键 |
| `quoteRevision` | `String` | 关联报价的 revision |
<a id="quotereport"></a>
### QuoteReport

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteReport.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteReport.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quoteId` | `String` | 内部报价主键 |
| `revision` | `String` | 当前记录版本，十进制字符串 |
| `action` | `String` | 业务动作 |
| `requestId` | `String` | 请求关联标识；写回执用途见主文档 |
| `clientQuoteId` | `String` | 调用方创建项标识 |
| `quote` | `QuoteRecord` | 报价对象 |
| `eventId` | `String` | 事件标识 |
| `operatorUserId` | `String` | 操作人 STC userid |
| `operationTime` | `String` | 操作时间 |
<a id="quotesource"></a>
### QuoteSource

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteSource.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteSource.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `type` | `String` | 类型；依所属模型区分 |
| `originService` | `String` | 来源服务 |
| `sourceUserId` | `String` | 来源渠道账户，QT 为 OwnQtNO |
| `ownerUserId` | `String` | 报价所属 STC userid |
| `externalMessageId` | `String` | 来源事件标识 |
| `originalMessageId` | `String` | 原文分组键 |
| `externalQuoteId` | `String` | 上游意向 QuoteID |
| `negotiationId` | `String` | 双边会话 NegotiationID |
| `counterpartyId` | `String` | 上游对手标识，非 CRM contactId |
| `chatType` | `String` | 聊天类型 |
| `chatGroupId` | `String` | 聊天群标识 |
| `externalRevision` | `String` | 上游版本，不等于本地 revision |
| `sourceTime` | `String` | 上游事件时间 |
| `transType` | `String` | QT TransType 原值 |
| `text` | `String` | 搜索关键词/来源原文，依模型 |
<a id="quotewritedata"></a>
### QuoteWriteData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteWriteData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteWriteData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `results` | `List<QuoteOperationResult>` | 逐项结果，必须逐条检查 Code |
<a id="sourcemessage"></a>
### SourceMessage

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/SourceMessage.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/SourceMessage.cs)；继承 [QuoteSource](#quotesource) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `sourceMessageId` | `String` | 内部来源原文组 ID |
| `sourceType` | `String` | 来源类型 |
| `quoteIds` | `List<String>` | 关联报价 ID |
<a id="sourcemessagerequest"></a>
### SourceMessageRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/SourceMessageRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/SourceMessageRequest.cs)；继承 [PageRequest](#pagerequest) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `sourceMessageId` | `String` | 内部来源原文组 ID |
| `quoteId` | `String` | 内部报价主键 |
<a id="sourcemessagedata"></a>
### SourceMessageData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/SourceMessageData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/SourceMessageData.cs)；继承 [PageData](#pagedata) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `sourceMessages` | `List<SourceMessage>` | 原文组记录数组 |

## 行情

<a id="pricebook"></a>
### PriceBook

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PriceBook.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PriceBook.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `securityId` | `String` | 债券代码 |
| `viewId` | `String` | 客户端生成的视图标识，生命周期内不复用 |
| `updateTime` | `String` | 最后更新时间 |
| `bookRevision` | `String` | 债券价格簿版本 |
| `marketData` | `MarketData` | STC 风格行情内容 |
| `quoteRefs` | `Map<String,QuoteReference>` | 行情条目 ID 到报价 ID/版本的映射 |
| `calculationIssues` | `List<PriceCalculationIssue>` | 未参与排序的报价及原因；无问题时省略，前端提示部分行情 |

<a id="pricecalculationissue"></a>
### PriceCalculationIssue

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PriceCalculationIssue.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PriceCalculationIssue.cs)；继承 [ProtocolModel](#protocolmodel)。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quoteId` | `String` | 当前用户可见、满足筛选，但尚无可比收益率的原始报价 ID |
| `code` | `String` | 等待换算、离线、超时/计算失败、队列满或输入不支持等原因，见 API 文档 |

<a id="marketdata"></a>
### MarketData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/MarketData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/MarketData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `SecurityID` | `String` | 债券代码（STC 大写字段） |
| `Symbol` | `String` | 债券简称 |
| `MarketIndicator` | `String` | 当前 FICC 投影写 4 |
| `VenueTypeGW` | `String` | 当前写 FICC |
| `Venues` | `String` | 当前写 FICC |
| `NoMDEntries` | `List<MarketEntry>` | 行情条目数组；空数组清空条目 |
<a id="marketentry"></a>
### MarketEntry

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/MarketEntry.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/MarketEntry.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `MDEntryID` | `String` | 行情条目标识，当前 FICC: 加内部 quoteId |
| `MDEntryType` | `String` | 0 Bid / 1 Ofr，与 Quote.side 不同 |
| `MDEntryPx` | `String` | 报价收益率 |
| `MDEntrySize` | `String` | 报价量，万 |
| `PriceType` | `String` | 当前投影为 3（收益率） |
| `ExerciseFlag` | `String` | 0 行权 / 1 到期 |
| `UserID` | `String` | 报价归属 STC userid |
| `MDPriceLevel` | `String` | 当前同侧条目顺序，从 1 开始，非不同价档序号 |
| `SettlType` | `String` | Quote 清算编码 |
| `MDEntryPositionNo` | `String` | 保留的 STC 条目位置字段；当前投影未赋值 |
| `MDEntryDate` | `String` | 报价更新时间日期，yyyyMMdd |
| `MDEntryTime` | `String` | 报价更新时间，HH:mm:ss.SSS |
| `Yield` | `String` | 到期收益率 |
| `YieldToExecution` | `String` | 行权收益率 |
| `InstitutionID` | `String` | CRM 机构 ID，依权限返回 |
| `InstitutionName` | `String` | 报价中已保存的对手机构名称，与 InstitutionID 同权限返回 |
| `TraderID` | `String` | CRM 联系人 ID，依权限返回 |
| `TraderName` | `String` | 报价中已保存的对手联系人姓名，与 TraderID 同权限返回 |

客户端将 `UserID` 按 STC 用户目录的 `userid` 解析为 `name`，显示在“长城交易员”；`name` 为空时使用该用户的 `username`。目录使用宿主登录时加载的 AdminSvr 缓存，本人还可使用登录资料 `FullName`；未解析到的用户显示原 userid，不冒用当前登录人或对手姓名。每页构建内存索引，不逐条发起远程查询。FULL 行和 BEST 明细共用映射；对手资料被服务端省略时，客户端不补查隐藏身份。
<a id="pricequeryrequest"></a>
### PriceQueryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PriceQueryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PriceQueryRequest.cs)；继承 [PageRequest](#pagerequest) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `securityIds` | `List<String>` | 债券代码数组 |
| `mode` | `String` | BEST / FULL / DEPTH |
| `depth` | `Integer` | 每侧价档数，仅 DEPTH 使用 |
| `filter` | `Filter` | 组合筛选条件 |
<a id="pricequerydata"></a>
### PriceQueryData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PriceQueryData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PriceQueryData.cs)；继承 [PageData](#pagedata) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `books` | `List<PriceBook>` | 视图价格簿数组；price.query 实际返回 prices |

## 筛选、关注与字典

<a id="dictionarydata"></a>
### DictionaryData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/DictionaryData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/DictionaryData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `dictionaries` | `Map<String,List<DictionaryOption>>` | 字典名到选项数组 |
| `filterFields` | `List<FilterField>` | 筛选字段目录 |
<a id="dictionaryoption"></a>
### DictionaryOption

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/DictionaryOption.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/DictionaryOption.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `value` | `String` | 枚举/标量/数组，按字段与操作符决定 |
| `label` | `String` | 展示名称 |
<a id="dictionaryrequest"></a>
### DictionaryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/DictionaryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/DictionaryRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `names` | `List<String>` | 所需字典名称数组 |
<a id="filter"></a>
### Filter

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/Filter.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/Filter.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `conditions` | `List<FilterCondition>` | 条件集合：同字段 OR、不同字段 AND |
| `market` | `String` | RATE / CREDIT |
| `watchlistIds` | `List<String>` | 引用的关注组 ID |
| `filterGroupIds` | `List<String>` | 引用的筛选组 ID |
<a id="filtercondition"></a>
### FilterCondition

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/FilterCondition.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/FilterCondition.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `conditionId` | `String` | 条件条目标识 |
| `field` | `String` | 协议字段名 |
| `operator` | `String` | 条件操作符 |
| `value` | `Object` | 枚举/标量/数组，按字段与操作符决定 |
| `displayOrder` | `Integer` | 展示顺序 |
<a id="filterfield"></a>
### FilterField

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/FilterField.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/FilterField.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `field` | `String` | 协议字段名 |
| `label` | `String` | 展示名称 |
| `type` | `String` | 类型；依所属模型区分 |
| `operators` | `List<String>` | 允许的操作符 |
| `enumValues` | `List<DictionaryOption>` | 枚举代码与标签 |
| `unit` | `String` | 单位 |
| `source` | `String` | 报价来源 |
| `group` | `String` | 字段分组 |
| `scope` | `String` | 配置作用域，由服务入口约束 |
| `enabled` | `Boolean` | 字段是否启用 |
| `simulated` | `Boolean` | 是否模拟资料 |
<a id="filtergroup"></a>
### FilterGroup

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/FilterGroup.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/FilterGroup.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `filterGroupId` | `String` | 筛选组 ID |
| `name` | `String` | 名称 |
| `market` | `String` | RATE / CREDIT |
| `scope` | `String` | 配置作用域，由服务入口约束 |
| `revision` | `String` | 当前记录版本，十进制字符串 |
| `userId` | `String` | STC 用户 ID |
| `createTime` | `String` | 创建时间 |
| `updateTime` | `String` | 最后更新时间 |
| `subgroups` | `List<FilterSubgroup>` | 子组数组 |
<a id="filtergroupdata"></a>
### FilterGroupData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/FilterGroupData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/FilterGroupData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `filterGroup` | `FilterGroup` | 单个筛选组 |
| `filterGroups` | `List<FilterGroup>` | 筛选组数组 |
| `filterGroupId` | `String` | 筛选组 ID |
| `deleted` | `Boolean` | 是否删除成功 |
<a id="filtergroupdeleterequest"></a>
### FilterGroupDeleteRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/FilterGroupDeleteRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/FilterGroupDeleteRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `filterGroupId` | `String` | 筛选组 ID |
| `expectedRevision` | `String` | 调用方预期的当前版本 |
<a id="filtergroupqueryrequest"></a>
### FilterGroupQueryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/FilterGroupQueryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/FilterGroupQueryRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `filterGroupId` | `String` | 筛选组 ID |
| `market` | `String` | RATE / CREDIT |
| `scope` | `String` | 配置作用域，由服务入口约束 |
<a id="filtergroupsaverequest"></a>
### FilterGroupSaveRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/FilterGroupSaveRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/FilterGroupSaveRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `filterGroupId` | `String` | 筛选组 ID |
| `expectedRevision` | `String` | 调用方预期的当前版本 |
| `name` | `String` | 名称 |
| `market` | `String` | RATE / CREDIT |
| `scope` | `String` | 配置作用域，由服务入口约束 |
| `subgroups` | `List<FilterSubgroup>` | 子组数组 |
<a id="filtersubgroup"></a>
### FilterSubgroup

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/FilterSubgroup.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/FilterSubgroup.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `subgroupId` | `String` | 子组 ID |
| `name` | `String` | 名称 |
| `displayOrder` | `Integer` | 展示顺序 |
| `conditions` | `List<FilterCondition>` | 条件集合：同字段 OR、不同字段 AND |
<a id="watchlist"></a>
### Watchlist

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/Watchlist.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/Watchlist.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `watchlistId` | `String` | 关注组 ID |
| `name` | `String` | 名称 |
| `market` | `String` | RATE / CREDIT |
| `scope` | `String` | 配置作用域，由服务入口约束 |
| `revision` | `String` | 当前记录版本，十进制字符串 |
| `userId` | `String` | STC 用户 ID |
| `createTime` | `String` | 创建时间 |
| `updateTime` | `String` | 最后更新时间 |
| `securities` | `List<WatchlistSecurity>` | 组内债券及顺序 |
<a id="watchlistdata"></a>
### WatchlistData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/WatchlistData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/WatchlistData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `watchlist` | `Watchlist` | 单个关注组 |
| `watchlists` | `List<Watchlist>` | 关注组数组 |
| `watchlistId` | `String` | 关注组 ID |
| `deleted` | `Boolean` | 是否删除成功 |
<a id="watchlistdeleterequest"></a>
### WatchlistDeleteRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/WatchlistDeleteRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/WatchlistDeleteRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `watchlistId` | `String` | 关注组 ID |
| `expectedRevision` | `String` | 调用方预期的当前版本 |
<a id="watchlistqueryrequest"></a>
### WatchlistQueryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/WatchlistQueryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/WatchlistQueryRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `watchlistId` | `String` | 关注组 ID |
| `market` | `String` | RATE / CREDIT |
| `scope` | `String` | 配置作用域，由服务入口约束 |
<a id="watchlistsaverequest"></a>
### WatchlistSaveRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/WatchlistSaveRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/WatchlistSaveRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `watchlistId` | `String` | 关注组 ID |
| `name` | `String` | 名称 |
| `market` | `String` | RATE / CREDIT |
| `scope` | `String` | 配置作用域，由服务入口约束 |
| `expectedRevision` | `String` | 调用方预期的当前版本 |
| `securities` | `List<WatchlistSecurity>` | 组内债券及顺序 |
<a id="watchlistsecurity"></a>
### WatchlistSecurity

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/WatchlistSecurity.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/WatchlistSecurity.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `securityId` | `String` | 债券代码 |
| `displayOrder` | `Integer` | 展示顺序 |

## 资料与 CRM

<a id="basedata"></a>
### BaseData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/BaseData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/BaseData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `records` | `List<ReferenceRecord>` | 记录集合；结构依所属模型而定 |
| `hasMore` | `Boolean` | 是否还有后续结果 |
| `nextOffset` | `Integer` | CRM 下一批偏移量 |
| `totalCount` | `Integer` | 完整匹配结果数量 |
| `nextCursor` | `String` | 下一页游标；无后续时 null |
<a id="basegetrequest"></a>
### BaseGetRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/BaseGetRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/BaseGetRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `ids` | `List<String>` | 当前页 ID 或按 ID 查询的集合 |
<a id="basequeryrequest"></a>
### BaseQueryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/BaseQueryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/BaseQueryRequest.cs)；继承 [PageRequest](#pagerequest) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `text` | `String` | 搜索关键词/来源原文，依模型 |
| `institutionId` | `String` | CRM 机构 ID |
| `offset` | `Integer` | CRM 偏移量 |
<a id="crmgetdata"></a>
### CrmGetData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/CrmGetData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/CrmGetData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `record` | `ReferenceRecord` | 单条资料/配置记录 |
<a id="crmgetrequest"></a>
### CrmGetRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/CrmGetRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/CrmGetRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `id` | `String` | 按 kind 解释的资料 ID |
<a id="crmlegacydata"></a>
### CrmLegacyData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/CrmLegacyData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/CrmLegacyData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `Code` | `String` | 业务返回码，字符串 |
| `Msg` | `String` | 服务端说明 |
| `totalSize` | `Integer` | 旧 CRM 结果条数 |
| `pageSize` | `Integer` | 每页条数 |
| `datas` | `List<CrmLegacyRecord>` | 旧 CRM 结果数组，小写 d |
<a id="crmlegacyrecord"></a>
### CrmLegacyRecord

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/CrmLegacyRecord.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/CrmLegacyRecord.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `idbkey` | `String` | 旧 CRM 标识 |
| `crmid` | `String` | CRM 原始记录标识 |
| `id` | `String` | 按 kind 解释的资料 ID |
| `shortnamecn` | `String` | 机构中文简称 |
| `fullnamecn` | `String` | 机构中文全称 |
| `pinyin` | `String` | 拼音简写 |
| `pinyinfull` | `String` | 全拼 |
| `instcode` | `String` | 机构编码 |
| `insttypevalue` | `String` | 机构类型值 |
| `taglist` | `String` | 标签列表原始表示 |
| `contactcount` | `Integer` | 联系人数量 |
| `namecn` | `String` | 联系人中文姓名 |
| `instid` | `String` | 所属机构 ID |
| `instname` | `String` | 所属机构名称 |
| `instfullnamecn` | `String` | 所属机构全称 |
| `ctctposition` | `String` | 联系人职务 |
| `biztype` | `String` | 业务类型 |
| `commonuse` | `String` | 旧 CRM 常用标记原值 |
| `isonboard` | `String` | 旧 CRM 在职标记原值 |
<a id="crmlookuprequest"></a>
### CrmLookupRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/CrmLookupRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/CrmLookupRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `name` | `String` | 名称 |
| `id` | `String` | 按 kind 解释的资料 ID |
| `match` | `String` | 旧 CRM 匹配方式 |
| `dealerStr` | `String` | 旧 CRM 交易员列表条件 |
| `PageSize` | `Integer` | 旧 CRM 请求上限，注意大写 P/S |
<a id="crmsearchrequest"></a>
### CrmSearchRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/CrmSearchRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/CrmSearchRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `text` | `String` | 搜索关键词/来源原文，依模型 |
| `institutionId` | `String` | CRM 机构 ID |
| `offset` | `Integer` | CRM 偏移量 |
| `limit` | `Integer` | 游标或搜索单批大小；有效范围由入口决定 |
<a id="referencerecord"></a>
### ReferenceRecord

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ReferenceRecord.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ReferenceRecord.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `bidOfferSpread` | `String` | 买卖收益率价差 |
| `market` | `String` | RATE / CREDIT |
| `valuationBasis` | `String` | 估值口径 |
| `id` | `String` | 按 kind 解释的资料 ID |
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `securityId` | `String` | 债券代码 |
| `securityName` | `String` | 债券名称 |
| `institutionId` | `String` | CRM 机构 ID |
| `institutionName` | `String` | 机构名称 |
| `institutionShortName` | `String` | 机构简称 |
| `contactId` | `String` | CRM 联系人 ID |
| `contactName` | `String` | 联系人姓名 |
| `positionName` | `String` | 联系人职务 |
| `institutionIds` | `List<String>` | 联系人关联机构 ID 集合 |
| `name` | `String` | 名称 |
| `shortName` | `String` | 简称 |
| `remainingYears` | `String` | 剩余期限（年） |
| `bondType` | `String` | 债券类型 |
| `bondSubtype` | `String` | 债券子类型 |
| `valuationYield` | `String` | 中债估值收益率 |
| `valuationCleanPrice` | `String` | 中债估值净价 |
| `couponRate` | `String` | 票面利率 |
| `issuerId` | `String` | 发行人 ID |
| `issuerName` | `String` | 发行人名称 |
| `province` | `String` | 省份 |
| `city` | `String` | 城市 |
| `ownershipType` | `String` | 企业性质 |
| `rating` | `String` | 评级 |
| `isHighYield` | `Boolean` | 是否高收益 |
| `isPreListing` | `Boolean` | 是否上市前 |
| `outstandingAmount` | `String` | 存量余额；单位以来源及筛选字典为准 |
| `regionCategory` | `String` | 区域分类 |
| `localGovernmentRegion` | `String` | 地方债地区 |
| `isTaxExempt` | `Boolean` | 是否免税 |
| `couponFrequency` | `String` | 付息频率 |
| `productType` | `String` | 产品分类 |
| `hotTags` | `List<String>` | 热门标签集合 |
| `ncdType` | `String` | 同业存单类型 |
| `listingStatus` | `String` | 上市状态 |
| `issueMethod` | `String` | 发行方式 |
| `csiValuationYield` | `String` | 中证到期收益率，百分数（1.7050 表示 1.7050%） |
| `csiValuationCleanPrice` / `csiValuationFullPrice` | `String` | 中证到期净价／全价 |
| `csiExerciseYield` | `String` | 中证行权收益率，百分数；不替代到期收益率 |
| `csiExerciseCleanPrice` / `csiExerciseFullPrice` | `String` | 中证行权净价／全价 |
| `csiValuationDate` | `String` | 来源估值日期 YYYY-MM-DD；可能不是当日 |
| `csiRecommendedBasis` | `String` | recommendation：1→EXERCISE，2→MATURITY；非含权债且有到期收益率时为 MATURITY，其余未提供则空 |
| `csiValuationStatus` | `String` | LOADING／READY／MISSING／FAILED；只在完整加载成功且该券无记录时返回 MISSING |
| `liquidityGrade` | `String` | 流动性等级 |
| `marketMakerTags` | `List<String>` | 做市商标签 |
| `listedMarket` | `String` | 上市市场 |
| `issuerType` | `String` | 发行人类型 |
| `bankType` | `String` | 银行类型 |
| `industry` | `String` | 行业 |
| `guaranteeType` | `String` | 担保类型 |
| `guarantorName` | `String` | 担保人名称 |
| `issuerRating` | `String` | 主体评级 |
| `issuerCbrRating` | `String` | 主体中债评级 |
| `ratingOutlook` | `String` | 评级展望 |
| `bondRating` | `String` | 债项评级 |
| `bondCbrRating` | `String` | 债项中债评级 |
| `yyRating` | `String` | YY 评级 |
| `csiRating` | `String` | 中证评级 |
| `optionType` | `String` | 含权类型 |
| `hasExerciseOption` | `Boolean` | 是否存在行权属性 |
| `repaymentType` | `String` | 还本方式 |
| `couponType` | `String` | 利率类型 |
| `perpetualType` | `String` | 永续类型 |
| `isPerpetual` | `Boolean` | 是否永续 |
| `isSubordinated` | `Boolean` | 是否次级 |
| `rateIndex` | `String` | 浮息基准 |
| `issueYear` | `String` | 发行年份 |
| `isMunicipal` | `Boolean` | 是否城投 |
| `region` | `String` | 区域 |
| `regionCode` | `String` | 区域编码 |
| `keyProvince` | `String` | 重点省份 |
| `municipalLevel` | `String` | 城投行政级别 |
| `regionalRating` | `String` | 区域评级 |
| `localGovernmentType` | `String` | 地方债类型 |
| `localGovernmentUse` | `String` | 地方债用途 |
| `isCrossMarket` | `Boolean` | 是否跨市场 |
| `isMortgage` | `Boolean` | 是否可质押 |

## 识别

<a id="parserrequest"></a>
### ParserRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ParserRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ParserRequest.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `userName` | `String` | STC 登录名 |
| `content` | `String` | 识别输入或提醒文本，依模型 |
<a id="parserreply"></a>
### ParserReply

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ParserReply.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ParserReply.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `Code` | `String` | 业务返回码，字符串 |
| `Msg` | `String` | 服务端说明 |
| `Datas` | `List<ParserQuote>` | Parser 识别结果数组，注意大写 D |
<a id="parserquote"></a>
### ParserQuote

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ParserQuote.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ParserQuote.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `chinaBondValuation` | `String` | 中债估值 |
| `institutionId` | `String` | CRM 机构 ID |
| `institutionName` | `String` | 机构名称 |
| `price` | `String` | 价格或收益率，具体单位随计价类型 |
| `priceType` | `String` | Parser/行情使用 STC 计价编码，区别于 Quote.quoteType |
| `qty` | `String` | Parser 报价量，实际 JSON 为 number |
| `securityId` | `String` | 债券代码 |
| `settlType` | `String` | 清算编码；Parser 与 Quote 语义不同，见主文档 |
| `side` | `String` | 报价方向：1 Bid / 2 Ofr |
| `symbol` | `String` | 债券简称 |
| `text` | `String` | 搜索关键词/来源原文，依模型 |
| `timeToMaturity` | `String` | 剩余期限展示文本 |
| `traderId` | `String` | Parser 交易员 ID，映射 Quote.contactId |
| `traderName` | `String` | Parser 交易员姓名 |

## 提醒

<a id="notificationpreference"></a>
### NotificationPreference

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationPreference.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationPreference.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `revision` | `String` | 当前记录版本，十进制字符串 |
| `watchlistEnabled` | `Boolean` | 关注债券提醒开关 |
| `filterGroupEnabled` | `Boolean` | 筛选组提醒开关 |
| `counterQuoteEnabled` | `Boolean` | 对价提醒开关 |
| `myBestEnabled` | `Boolean` | 我的最优报价提醒开关 |
| `userId` | `String` | STC 用户 ID |
| `updateTime` | `String` | 最后更新时间 |
<a id="notificationpreferencedata"></a>
### NotificationPreferenceData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationPreferenceData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationPreferenceData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `preference` | `NotificationPreference` | 提醒偏好 |
<a id="notificationpreferencesaverequest"></a>
### NotificationPreferenceSaveRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationPreferenceSaveRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationPreferenceSaveRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `expectedRevision` | `String` | 调用方预期的当前版本 |
| `watchlistEnabled` | `Boolean` | 关注债券提醒开关 |
| `filterGroupEnabled` | `Boolean` | 筛选组提醒开关 |
| `counterQuoteEnabled` | `Boolean` | 对价提醒开关 |
| `myBestEnabled` | `Boolean` | 我的最优报价提醒开关 |
<a id="notificationquerydata"></a>
### NotificationQueryData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationQueryData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationQueryData.cs)；继承 [PageData](#pagedata) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `notifications` | `List<NotificationRecord>` | 提醒数组 |
<a id="notificationqueryrequest"></a>
### NotificationQueryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationQueryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationQueryRequest.cs)；继承 [PageRequest](#pagerequest) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `readState` | `String` | ALL / READ / UNREAD |
| `types` | `List<String>` | 类型数组；当前提醒入口未实现此筛选 |
| `filter` | `Filter` | 组合筛选条件 |
<a id="notificationreaddata"></a>
### NotificationReadData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationReadData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationReadData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `results` | `List<NotificationReadResult>` | 逐项结果，必须逐条检查 Code |
<a id="notificationreadrequest"></a>
### NotificationReadRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationReadRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationReadRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `notificationIds` | `List<String>` | 待处理提醒 ID |
| `read` | `Boolean` | true 标已读；false 恢复未读 |
<a id="notificationreadresult"></a>
### NotificationReadResult

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationReadResult.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationReadResult.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `notificationId` | `String` | 提醒主键 |
| `Code` | `String` | 业务返回码，字符串 |
| `Msg` | `String` | 服务端说明 |
| `readTime` | `String` | 已读时间，未读为 null |
<a id="notificationreason"></a>
### NotificationReason

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationReason.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationReason.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `type` | `String` | 类型；依所属模型区分 |
| `groupIds` | `List<String>` | 关联组 ID |
| `quoteIds` | `List<String>` | 关联报价 ID |
| `ownQuoteId` | `String` | 我方报价 ID |
| `otherQuoteId` | `String` | 对方报价 ID |
| `text` | `String` | 搜索关键词/来源原文，依模型 |
<a id="notificationrecord"></a>
### NotificationRecord

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationRecord.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationRecord.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `notificationId` | `String` | 提醒主键 |
| `userId` | `String` | STC 用户 ID |
| `securityId` | `String` | 债券代码 |
| `content` | `String` | 识别输入或提醒文本，依模型 |
| `contentToken` | `String` | 提醒内容版本关联标识 |
| `alertKey` | `String` | 提醒聚合标识 |
| `active` | `Boolean` | 提醒是否仍有效 |
| `revision` | `String` | 当前记录版本，十进制字符串 |
| `createTime` | `String` | 创建时间 |
| `updateTime` | `String` | 最后更新时间 |
| `readTime` | `String` | 已读时间，未读为 null |
| `reasons` | `List<NotificationReason>` | 提醒原因数组 |
| `quoteIds` | `List<String>` | 关联报价 ID |

## 视图与推送

<a id="notificationstreambatch"></a>
### NotificationStreamBatch

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationStreamBatch.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationStreamBatch.cs)；继承 [StreamMessage](#streammessage) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `notifications` | `List<NotificationRecord>` | 提醒数组 |
| `views` | `List<ViewDelta>` | 视图变化列表 |
| `preference` | `NotificationPreference` | 提醒偏好 |
<a id="notificationviewdata"></a>
### NotificationViewData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationViewData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationViewData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `streamId` | `String` | 服务端流实例标识 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `view` | `ViewPage` | 单页视图元数据 |
| `notifications` | `List<NotificationRecord>` | 提醒数组 |
| `preference` | `NotificationPreference` | 提醒偏好 |
<a id="notificationviewopenrequest"></a>
### NotificationViewOpenRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationViewOpenRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationViewOpenRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `viewId` | `String` | 客户端生成的视图标识，生命周期内不复用 |
| `pageSize` | `Integer` | 每页条数 |
| `readState` | `String` | ALL / READ / UNREAD |
| `types` | `List<String>` | 类型数组；当前提醒入口未实现此筛选 |
| `filter` | `Filter` | 组合筛选条件 |
<a id="pricestreambatch"></a>
### PriceStreamBatch

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PriceStreamBatch.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PriceStreamBatch.cs)；继承 [StreamMessage](#streammessage) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `books` | `List<PriceBook>` | 视图价格簿数组；price.query 实际返回 prices |
| `views` | `List<ViewDelta>` | 视图变化列表 |
<a id="priceviewdata"></a>
### PriceViewData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PriceViewData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PriceViewData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `streamId` | `String` | 服务端流实例标识 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `view` | `ViewPage` | 单页视图元数据 |
| `books` | `List<PriceBook>` | 视图价格簿数组；price.query 实际返回 prices |
<a id="priceviewopenrequest"></a>
### PriceViewOpenRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PriceViewOpenRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PriceViewOpenRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `viewId` | `String` | 客户端生成的视图标识，生命周期内不复用 |
| `pageSize` | `Integer` | 每页条数 |
| `mode` | `String` | BEST / FULL / DEPTH |
| `depth` | `Integer` | 每侧价档数，仅 DEPTH 使用 |
| `filter` | `Filter` | 组合筛选条件 |
<a id="quotestreambatch"></a>
### QuoteStreamBatch

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteStreamBatch.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteStreamBatch.cs)；继承 [StreamMessage](#streammessage) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quotes` | `List<QuoteRecord>` | 报价数组 |
| `reports` | `List<QuoteReport>` | 报价操作报告数组 |
| `views` | `List<ViewDelta>` | 视图变化列表 |
| `reference` | `List<ReferenceRecord>` | 关联基础资料 |
<a id="quoteviewdata"></a>
### QuoteViewData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteViewData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteViewData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `streamId` | `String` | 服务端流实例标识 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `view` | `ViewPage` | 单页视图元数据 |
| `quotes` | `List<QuoteRecord>` | 报价数组 |
| `reference` | `List<ReferenceRecord>` | 关联基础资料 |
<a id="quoteviewopenrequest"></a>
### QuoteViewOpenRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteViewOpenRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteViewOpenRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `viewId` | `String` | 客户端生成的视图标识，生命周期内不复用 |
| `pageSize` | `Integer` | 每页条数 |
| `ownerScope` | `String` | VISIBLE / MINE；部分历史入口未使用 |
| `status` | `List<String>` | 业务状态；不同模型不得混用状态枚举 |
| `filter` | `Filter` | 组合筛选条件 |
<a id="streammessage"></a>
### StreamMessage

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/StreamMessage.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/StreamMessage.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `Code` | `String` | 业务返回码，字符串 |
| `Msg` | `String` | 服务端说明 |
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `streamId` | `String` | 服务端流实例标识 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `reason` | `String` | 动作原因或重置原因 |
<a id="streamopendata"></a>
### StreamOpenData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/StreamOpenData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/StreamOpenData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `sessionId` | `String` | 当前会话标识 |
| `streamId` | `String` | 服务端流实例标识 |
| `pushTopic` | `String` | 服务返回的订阅 topic |
<a id="viewclosedata"></a>
### ViewCloseData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ViewCloseData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ViewCloseData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `viewId` | `String` | 客户端生成的视图标识，生命周期内不复用 |
| `closed` | `Boolean` | 逻辑视图已关闭 |
<a id="viewcloserequest"></a>
### ViewCloseRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ViewCloseRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ViewCloseRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `viewId` | `String` | 客户端生成的视图标识，生命周期内不复用 |
<a id="viewdelta"></a>
### ViewDelta

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ViewDelta.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ViewDelta.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `viewId` | `String` | 客户端生成的视图标识，生命周期内不复用 |
| `totalCount` | `Integer` | 完整匹配结果数量 |
| `upsertIds` | `List<String>` | 进入/更新的记录 ID |
| `removeIds` | `List<String>` | 退出该视图的记录 ID |
<a id="viewpage"></a>
### ViewPage

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ViewPage.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ViewPage.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `viewId` | `String` | 客户端生成的视图标识，生命周期内不复用 |
| `pageNumber` | `Integer` | 页码，从 1 开始 |
| `pageSize` | `Integer` | 每页条数 |
| `totalCount` | `Integer` | 完整匹配结果数量 |
| `ids` | `List<String>` | 当前页 ID 或按 ID 查询的集合 |
| `hasMore` | `Boolean` | 是否还有后续结果 |
<a id="viewqueryrequest"></a>
### ViewQueryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/ViewQueryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/ViewQueryRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `viewId` | `String` | 客户端生成的视图标识，生命周期内不复用 |
| `pageNumber` | `Integer` | 页码，从 1 开始 |
| `pageSize` | `Integer` | 每页条数 |

## 内部状态、事件及接入

<a id="eventqueryrequest"></a>
### EventQueryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/EventQueryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/EventQueryRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `epoch` | `String` | 持久化事件代次 |
| `afterSequence` | `String` | 从该事件位置之后读取 |
| `limit` | `Integer` | 游标或搜索单批大小；有效范围由入口决定 |
<a id="intakeapplydata"></a>
### IntakeApplyData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/IntakeApplyData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/IntakeApplyData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `sourceMessageId` | `String` | 内部来源原文组 ID |
| `results` | `List<QuoteOperationResult>` | 逐项结果，必须逐条检查 Code |
<a id="intakeapplyrequest"></a>
### IntakeApplyRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/IntakeApplyRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/IntakeApplyRequest.cs)；继承 [Request](#request) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `sourceType` | `String` | 来源类型 |
| `externalMessageId` | `String` | 来源事件标识 |
| `sourceUserId` | `String` | 来源渠道账户，QT 为 OwnQtNO |
| `sourceTime` | `String` | 上游事件时间 |
| `text` | `String` | 搜索关键词/来源原文，依模型 |
| `quotes` | `List<IntakeQuoteItem>` | 报价数组 |
<a id="intakeidentity"></a>
### IntakeIdentity

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/IntakeIdentity.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/IntakeIdentity.cs)；继承 [QuoteSource](#quotesource) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `token` | `String` | 历史内部身份字段；不属于普通前端请求 |
<a id="intakequoteitem"></a>
### IntakeQuoteItem

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/IntakeQuoteItem.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/IntakeQuoteItem.cs)；继承 [QuoteInput](#quoteinput) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `externalQuoteId` | `String` | 上游意向 QuoteID |
| `externalRevision` | `String` | 上游版本，不等于本地 revision |
| `action` | `String` | 业务动作 |
<a id="notificationevent"></a>
### NotificationEvent

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationEvent.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationEvent.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `eventId` | `String` | 事件标识 |
| `type` | `String` | 类型；依所属模型区分 |
| `action` | `String` | 业务动作 |
| `notificationId` | `String` | 提醒主键 |
| `record` | `NotificationEventRecord` | 单条资料/配置记录 |
<a id="notificationeventbatch"></a>
### NotificationEventBatch

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationEventBatch.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationEventBatch.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `Code` | `String` | 业务返回码，字符串 |
| `Msg` | `String` | 服务端说明 |
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `epoch` | `String` | 持久化事件代次 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `currentSequence` | `String` | 当前末尾位置 |
| `fromSequence` | `String` | 本批起始事件位置 |
| `toSequence` | `String` | 本批结束事件位置 |
| `nextSequence` | `String` | 下一次请求的已读位置 |
| `seenSequence` | `String` | 服务观察到的末尾位置 |
| `hasMore` | `Boolean` | 是否还有后续结果 |
| `events` | `List<NotificationEvent>` | 事件数组 |
<a id="notificationeventrecord"></a>
### NotificationEventRecord

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationEventRecord.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationEventRecord.cs)；继承 [NotificationRecord](#notificationrecord) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `filterGroupId` | `String` | 筛选组 ID |
| `name` | `String` | 名称 |
| `market` | `String` | RATE / CREDIT |
| `scope` | `String` | 配置作用域，由服务入口约束 |
| `subgroups` | `List<FilterSubgroup>` | 子组数组 |
| `watchlistEnabled` | `Boolean` | 关注债券提醒开关 |
| `filterGroupEnabled` | `Boolean` | 筛选组提醒开关 |
| `counterQuoteEnabled` | `Boolean` | 对价提醒开关 |
| `myBestEnabled` | `Boolean` | 我的最优报价提醒开关 |
<a id="notificationstatedata"></a>
### NotificationStateData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationStateData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationStateData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `epoch` | `String` | 持久化事件代次 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `totalCount` | `Integer` | 完整匹配结果数量 |
| `records` | `NotificationStateRecords` | 记录集合；结构依所属模型而定 |
| `nextCursor` | `String` | 下一页游标；无后续时 null |
| `done` | `Boolean` | 状态快照是否已读完 |
<a id="notificationstaterecords"></a>
### NotificationStateRecords

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/NotificationStateRecords.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/NotificationStateRecords.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `notifications` | `List<NotificationRecord>` | 提醒数组 |
| `preferences` | `List<NotificationPreference>` | 用户偏好集合 |
| `filterGroups` | `List<FilterGroup>` | 筛选组数组 |
<a id="qtintakestatusdata"></a>
### QtIntakeStatusData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QtIntakeStatusData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QtIntakeStatusData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `fixConnected` | `Boolean` | FIX 是否连接 |
| `storageFailed` | `Boolean` | 本地存储是否故障 |
| `quoteSvrOnline` | `Boolean` | QuoteSvr 在线状态 |
<a id="quoteevent"></a>
### QuoteEvent

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteEvent.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteEvent.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `eventId` | `String` | 事件标识 |
| `type` | `String` | 类型；依所属模型区分 |
| `action` | `String` | 业务动作 |
| `quoteId` | `String` | 内部报价主键 |
| `quote` | `QuoteRecord` | 报价对象 |
| `record` | `SettingRecord` | 单条资料/配置记录 |
| `requestId` | `String` | 请求关联标识；写回执用途见主文档 |
<a id="quoteeventbatch"></a>
### QuoteEventBatch

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteEventBatch.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteEventBatch.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `Code` | `String` | 业务返回码，字符串 |
| `Msg` | `String` | 服务端说明 |
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `epoch` | `String` | 持久化事件代次 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `currentSequence` | `String` | 当前末尾位置 |
| `fromSequence` | `String` | 本批起始事件位置 |
| `toSequence` | `String` | 本批结束事件位置 |
| `nextSequence` | `String` | 下一次请求的已读位置 |
| `seenSequence` | `String` | 服务观察到的末尾位置 |
| `hasMore` | `Boolean` | 是否还有后续结果 |
| `events` | `List<QuoteEvent>` | 事件数组 |
<a id="quotestatedata"></a>
### QuoteStateData

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteStateData.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteStateData.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `epoch` | `String` | 持久化事件代次 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `totalCount` | `Integer` | 完整匹配结果数量 |
| `records` | `QuoteStateRecords` | 记录集合；结构依所属模型而定 |
| `nextCursor` | `String` | 下一页游标；无后续时 null |
| `done` | `Boolean` | 状态快照是否已读完 |
<a id="quotestaterecords"></a>
### QuoteStateRecords

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/QuoteStateRecords.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/QuoteStateRecords.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `quotes` | `List<QuoteRecord>` | 报价数组 |
| `watchlists` | `List<Watchlist>` | 关注组数组 |
| `filterGroups` | `List<FilterGroup>` | 筛选组数组 |
<a id="statequeryrequest"></a>
### StateQueryRequest

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/StateQueryRequest.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/StateQueryRequest.cs)；继承 [PageRequest](#pagerequest) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `collections` | `List<String>` | 集合名选择声明；实际支持情况见主文档 |
<a id="settingrecord"></a>
### SettingRecord

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/SettingRecord.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/SettingRecord.cs)；继承 [FilterGroup](#filtergroup) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `watchlistId` | `String` | 关注组 ID |
| `securities` | `List<WatchlistSecurity>` | 组内债券及顺序 |
<a id="pricefeedbatch"></a>
### PriceFeedBatch

[Java 定义](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api/PriceFeedBatch.java) · [C# 定义](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol/PriceFeedBatch.cs)；继承 [ProtocolModel](#protocolmodel) 的字段。

| 字段 | 声明类型 | 说明 |
|---|---|---|
| `Code` | `String` | 业务返回码，字符串 |
| `Msg` | `String` | 服务端说明 |
| `kind` | `String` | 消息种类/资料类型，取值依接口 |
| `epoch` | `String` | 持久化事件代次 |
| `sequence` | `String` | 流或事件序号，按模型区分，不可跨流混用 |
| `currentSequence` | `String` | 当前末尾位置 |
| `streamId` | `String` | 服务端流实例标识 |
| `quotePosition` | `EventPosition` | 关联的报价事件位置 |
| `prices` | `List<PriceBook>` | 内部行情快照中的价格簿数组 |

## Parser 旧响应完整字段：TableIntentionQuote

[服务端实际响应类](../iteration1/cgws-ficc-parsersvr/src/main/java/sc/app/stc/parser/po/TableIntentionQuote.java)。以下是服务可序列化字段，不代表每次识别均能填充；客户端目前仅消费 ParserQuote 的子集。

| 字段 | Java 类型 |
|---|---|
| `intentionId` | `String` |
| `institutionId` | `String` |
| `institutionName` | `String` |
| `traderId` | `String` |
| `traderName` | `String` |
| `userId` | `String` |
| `userName` | `String` |
| `qqNumber` | `String` |
| `securityId` | `String` |
| `symbol` | `String` |
| `side` | `String` |
| `price` | `BigDecimal` |
| `cleanPrice` | `BigDecimal` |
| `fullPrice` | `BigDecimal` |
| `qty` | `BigDecimal` |
| `settlType` | `String` |
| `text` | `String` |
| `status` | `String` |
| `type` | `String` |
| `orderId` | `String` |
| `createTime` | `String` |
| `updateTime` | `String` |
| `closeBy` | `String` |
| `wholeflag` | `String` |
| `roleid` | `String` |
| `rolename` | `String` |
| `partysubname` | `String` |
| `parseTraderIdbkey` | `String` |
| `parseCompanyIdbkey` | `String` |
| `accountID` | `String` |
| `accountName` | `String` |
| `vipCode` | `String` |
| `orgType` | `String` |
| `ifSubBond` | `String` |
| `validuntitime` | `String` |
| `inf1` | `String` |
| `sign` | `Long` |
| `sign1` | `Long` |
| `sign2` | `Long` |
| `sign3` | `Long` |
| `reftype` | `String` |
| `roletype` | `String` |
| `termDays` | `BigDecimal` |
| `optionType` | `String` |
| `optionDate` | `String` |
| `couponRateCurrent` | `BigDecimal` |
| `issuerCode` | `String` |
| `issuerName` | `String` |
| `warranterCode` | `String` |
| `warranterName` | `String` |
| `chinaBondValuation` | `BigDecimal` |
| `chinaBondValuationFull` | `String` |
| `macaulayDuration` | `String` |
| `modifiedDuration` | `String` |
| `pvbp` | `String` |
| `convexity` | `String` |
| `dayOffStr` | `String` |
| `issueAmount` | `BigDecimal` |
| `firmflag` | `String` |
| `marketindicator` | `String` |
| `executionqty` | `BigDecimal` |
| `clordid` | `String` |
| `csiBondValuation` | `BigDecimal` |
| `csiBondValuationFull` | `String` |
| `tenor` | `String` |
| `handlinst` | `String` |
| `priceType` | `String` |
| `timeToMaturity` | `String` |
| `timeToMaturityToWorkDay` | `String` |
| `actionType` | `String` |
| `quoteType` | `String` |
| `name` | `String` |
| `closeByName` | `String` |
| `failMessage` | `String` |
| `isInternal` | `Boolean` |
| `channel` | `String` |
| `info2` | `String` |
| `info3` | `String` |
| `info4` | `String` |
| `info5` | `String` |
| `info6` | `String` |
| `hurryFlag` | `String` |
| `suggestFlag` | `String` |
| `fireFlag` | `String` |
| `floorPrice` | `String` |
| `customerPrice` | `BigDecimal` |
| `customerSpread` | `BigDecimal` |
| `publicRemarks` | `String` |
| `personalRemarks` | `String` |
| `ocoflag` | `String` |
| `biztype` | `String` |
| `biztypeid` | `String` |
| `inf3` | `String` |


## 智能推荐模型

Java 类型位于 common 的 `ficc.api`，C# 对应类型位于插件 `Ficc/Protocol`，通过两侧 `SalesTradingApi` 绑定 topic。

| 模型 | 字段 | 含义 |
| --- | --- | --- |
| RecommendationQueryRequest | requestId, securityId, startDate, endDate | 债券及1–365天自然日窗口 |
| RecommendationQueryData | securityId, startDate, endDate, scoringVersion, recommendations | 查询范围、算法版本与结果 |
| RecommendationRecord | recommendationId, securityId, securityName, side | 稳定条目标识、债券、方向（1 Bid/2 Ofr） |
| RecommendationRecord | score, quoteCount | 衰减评分、窗口内不同正式报价数 |
| RecommendationRecord | institutionName, contactName, ownerUserId | 本人可见的对手姓名字段及可见的内部归属 |
| RecommendationRecord | latestPrice, quoteType, yieldType, latestQuantity, latestTime | 最新证据的原价格/价格类型/收益率类型/报量（万）/时间 |
| RecommendationRecord | feedback, evidence | 当前查看者的反馈状态、脱敏证据列表 |
| RecommendationEvidence | quoteId, ownerUserId, securityId, securityName, side | 证据来源报价及内部归属 |
| RecommendationEvidence | price, quoteType, yieldType, quantity, updateTime | 历史正式快照的报价要素 |
| RecommendationEvidence | institutionName, contactName | 按该证据归属逐条脱敏，无原始payload、备注或渠道账户透传 |
| RecommendationFeedbackRequest | 查询字段 + recommendationId, feedback | 只允许三种反馈；不能传反馈归属用户 |
| RecommendationFeedbackData | recommendationId, feedback | 已持久化的首次反馈结果 |

验证入口：Java `RecommendationSmoke` 使用独立 PostgreSQL schema（运行完清理），输出跨语言协议样本；客户端 `scripts/test-recommendations.ps1` 验证真实 XAML、证据展开、反馈按钮、日期校验与迟到应答隔离。`-Live` 额外通过本地 GW/QuoteSvr 读取历史证据，账户从测试进程 `FICC_TEST_USER`、`FICC_TEST_PASSWORD` 读取；不向 QT 外发消息。脚本使用30秒独立进程看门狗，不是性能测试。
