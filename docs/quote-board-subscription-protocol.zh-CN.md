# 报价看板订阅协议（讨论草案）

> 历史讨论资料：页码、自由排序及旧服务/topic示例不再作为当前实现依据。当前由QuoteSvr统一提供报价及行情API，NotificationSvr提供通知API，SearchSvr仅为内部搜索存储；页面使用滚动。以[现行架构](ficc-sales-trading-architecture.zh-CN.md)与[行情模型](models/market-data-model.zh-CN.md)为准，后续重新编写对应订阅协议。

本文是新方案的讨论稿，不是旧系统接口说明，也不是最终实现规范。

## 1 核心思路

- 每个面板独立订阅：过滤条件、排序、页码、页大小、显示字段。
- 服务端维护完整过滤和排序结果，只向前端发送当前页。
- 同一连接上的面板共享报价缓存，重复出现的报价只传一份。
- 报价数据和页面成员分开传：报价对象描述内容，页面保存有序报价ID列表。
- 新增、修改、确认、Refer、删除仍走独立业务接口，执行结果通过订阅更新页面。

```text
服务端完整报价集合
  → 各面板过滤、排序、分页
  → 按客户端连接合并更新
  → 一份报价数据 + 各面板的ID/位置变化

客户端：
  QuoteCache[quoteId] → 报价数据
  PanelA.rowIds      → [Q1, Q2, Q3]
  PanelB.rowIds      → [Q1, Q4]
```

客户端缓存只需要包含各活动页引用报价的并集，不必持有全部匹配报价。选中状态、编辑内容等保留在各面板，不放进共享报价对象。

## 2 客户端请求

以下是逻辑消息，承载方式沿用旧客户端的DPS请求/应答、订阅/发布模式；本文不另行指定WebSocket。

### 与旧客户端消息层的对应

旧客户端业务入口为`ClientApi`，经`BaseCEDAServer`调用`MQIIIManager`及`com.adaptiveMQ2.message.Message`。源码中使用CEDA/MQIII等名称；这里沿用讨论中的DPS称呼，不据此推断底层网络协议。

| 层次 | 旧客户端做法 | 新方案建议 |
| --- | --- | --- |
| 请求/应答 | `RequestMessage<T>` / `RequestMessageAsync<T>(svrId, topic, JSON)`；封装登录Sid，返回反序列化后的业务对象 | 创建面板、改条件、翻页、关闭、ACK、RESYNC均走此模式 |
| 订阅/发布 | `SubScribeWithImage<T>(topics, svrId, callback)`；服务端向topic发布JSON，客户端按类型回调 | 每个客户端视图会话订阅一个服务端授权的推送流，承载下文`BATCH` |
| 业务应答 | 常见`Code / Msg / Data`或`Datas`，`Code="0000"`表示业务成功 | 沿用`Code / Msg / Data`；消息层错误与业务失败分别处理 |

面板订阅是服务端维护的查询对象，不等于DPS的topic订阅。同一客户端的多个面板共用一个推送流，报价内容仍按批次去重。仅生成独有topic不能代替订阅权限检查。

候选请求topic为`stc.sbp.quote.view.create/changeQuery/changePage/close/ack/resync`（斜线表示分别的末级名称，不是一个topic）；推送topic由服务端分配。名称均为新提议，不是现有接口。

请求应答负责确认命令接收或返回错误；页面快照、增量及关闭结果统一进入有序`BATCH`，以`requestId`关联请求。收到成功应答不表示页面已经刷新；应答与推送可能先后交错。异步执行失败也须发送对应的`ERROR`。

旧代码依据：`STC.Common/Api/ClientApi.cs`、`BaseCEDAServer.cs`、`MessageConsumer.cs`、`STC.Common/Handler/SalesTrading/SalesTradingHandler.cs`。对价板先启动推送订阅，再通过`QueryTodayQuoteInfoAll`逐页查询（每页500条）；这是旧版加载方式，不是新方案的服务端分页订阅。

`SubScribeWithImage`在可见封装中调用`handler.subscribe(topics, svrId, Sid)`；不能仅凭方法名称认定具有原子快照、重放或可靠投递保证。新方案需先确认推送订阅已就绪，再生成首批快照并衔接更新；订阅就绪确认方式需结合底层库进一步确定。下文序号与重建机制仍须保留。

### 逻辑请求字段

| 消息 | 主要字段 | 用途 |
| --- | --- | --- |
| `SUBSCRIBE` | `requestId, panelId, filter, sort, page, pageSize, fields` | 创建面板订阅 |
| `CHANGE_QUERY` | `requestId, subscriptionId, expectedVersion, filter, sort, page, pageSize, fields` | 替换查询定义；改序通常显式回到第一页 |
| `CHANGE_PAGE` | `requestId, subscriptionId, expectedVersion, page, pageSize` | 翻页 |
| `UNSUBSCRIBE` | `requestId, subscriptionId, expectedVersion` | 关闭面板订阅 |
| `ACK` | `streamId, sequence` | 确认已应用的累计批次 |
| `RESYNC` | `streamId, lastAppliedSequence` | 数据缺失或不一致时请求重建 |

过滤条件采用结构化表达式，不直接传SQL。示例：

```json
{
  "type": "SUBSCRIBE",
  "requestId": "req-1",
  "panelId": "bank-quotes",
  "filter": {
    "op": "and",
    "args": [
      {"field": "issuerType", "op": "eq", "value": "BANK"},
      {"field": "yield", "op": "gte", "value": "2.5000"}
    ]
  },
  "sort": [{"field": "updateTime", "direction": "DESC"}],
  "page": 1,
  "pageSize": 50,
  "fields": ["symbol", "yield", "orderQty", "quoteStatus"]
}
```

过滤还应支持OR、范围和多选等常见条件，具体字段及操作符白名单后续确定。排序最后增加唯一ID作为稳定的并列排序依据。页码从1开始，页内位置从0开始。

## 3 服务端响应

所有影响报价缓存和页面的数据放入连接级`BATCH`，统一排序、一起应用。

| 字段 | 含义 |
| --- | --- |
| `streamId` | 本次逻辑连接的数据流标识；重建时变化 |
| `sequence` | 连接级连续批次序号 |
| `quotes` | 去重后的报价对象，含`quoteId, revision, fields` |
| `panels` | 各面板的快照或增量 |
| `evictQuoteIds` | 已不被任何活动页引用、可以释放的报价ID |

第一版中，同一报价被多个面板显示时，服务端只发送一份报价数据，其中包含这些面板需要且当前用户有权查看的全部字段。报价发生变化时，重新发送这些字段，不单独发送发生变化的字段。客户端按批次顺序替换缓存中该报价的`fields`，各面板从同一份缓存读取数据。如果面板新增显示字段，服务端重新发送包含新增字段的报价数据；报价本身没有变化时，`revision`可以保持不变。

例如，面板A显示Q1的“债券代码、价格”，面板B显示Q1的“债券代码、价格、机构”。服务端只发送一份Q1，包含这三个字段。价格变化时，仍发送这三个字段，其中价格为新值。

### 页面快照

创建订阅、改过滤、改排序或翻页时返回：

```text
SNAPSHOT {
  requestId, subscriptionId, panelId, version,
  totalCount, rowIds
}
```

修改查询或翻页产生新version；普通报价变化不修改查询version。讨论稿先按客户端提交的查询条件处理，服务端调整条件及回显的细节以后再补充。

快照中的ID必须能在同批次quotes或已有缓存中找到。新快照不意味着UI必须Reset：共有行仍可复用。

### 页面增量

```text
DELTA {
  subscriptionId, version, totalCount,
  operations: [REMOVE / INSERT / MOVE]
}
```

- `REMOVE(quoteId)`：从该页移除，不代表业务删除。
- `INSERT(quoteId, index)`：在指定位置插入。
- `MOVE(quoteId, toIndex)`：先取出该行，再插入目标位置。
- 操作依次执行，索引基于每一步执行时的列表。
- 只改报价值、不改页面成员或位置时，仅发quotes，不必给每个面板重复发UPDATE。
- 只改变结果总数时，可发送空operations及新的totalCount。

关闭订阅返回`CLOSED`，附对应requestId和subscriptionId，并释放不再被其他面板引用的实体。失败返回`ERROR {requestId, code, message}`，保留原有效页面。

## 4 多面板去重示例

假设A页为`[Q1,Q2,Q3]`，B页为`[Q1,Q4]`；两页合计只需要Q1的symbol和yield。Q1价格更新且在A中移动，B的位置不变：

```json
{
  "type": "BATCH",
  "streamId": "stream-1",
  "sequence": "12",
  "quotes": [
    {"quoteId": "Q1", "revision": "9", "fields": {"symbol": "示例债券", "yield": "2.6500"}}
  ],
  "panels": [
    {
      "kind": "DELTA",
      "subscriptionId": "sub-A",
      "version": "3",
      "totalCount": 10000,
      "operations": [{"op": "MOVE", "quoteId": "Q1", "toIndex": 1}]
    }
  ],
  "evictQuoteIds": []
}
```

结果：A变为`[Q2,Q1,Q3]`，B保持`[Q1,Q4]`，两页都显示Q1的新价格，但网络只传了一份Q1。

另一种情况：`[A,B,C]`收到排在最前的新报价X，服务端发送`REMOVE C + INSERT X到0`，得到`[X,A,B]`。C仍被其他面板使用时不能释放。页面补位由服务端从完整排序结果计算。

## 5 必须保留的一致性约定

1. 快照和后续事件必须连续，不能先查快照再开始监听而漏消息；服务端应在一致读取位置构造快照并接续之后的更新。
2. 同一报价变化影响多个面板时，共享实体与相关页面变化必须在一个一致批次中提交。客户端整批应用成功后再ACK，避免出现新价格配旧排序的中间状态。
3. 同一订阅的查询变更串行提交；expectedVersion不符则拒绝。新快照提交后不再发送旧版本增量；客户端也不能让旧版本覆盖新页面。
4. 重复sequence忽略；发现序号缺口或缺失实体时停止应用并RESYNC。实体revision与订阅version不同，前者表示报价数据版本，后者表示查询版本。
5. 首版断线恢复可直接清空缓存、重新订阅；之后再增加有界日志重放。无法恢复时必须明确重建，不假装无缝续传。
6. 同一requestId重试不得重复创建订阅；关闭面板应取消订阅，断线会话应超时释放。
7. 权限、发行人类型、评级等关联资料变化也会影响结果，服务端必须处理；不能只监听报价价格变化。
8. 服务端可以短时间合并事件，只保证最终页面状态，不作为逐事件审计流；发送队列必须有上限，慢客户端超限后重建。

## 6 待讨论的产品选择

- 每页先用50还是100条？最大面板数和发送频率根据小规模验证确定。
- 实时排名导致页面成员频繁变化时，是否需要独立的“暂停排名/固定工作列表”模式？首版先按实时排名处理。
- 当前页因数据减少而超出范围，建议显示空页及正确总数，由用户选择回到有效页，不自动跳转。
- 首版用完整投影加连接去重；仅在带宽测量显示必要时增加字段级补丁。

实现可以先从单机订阅管理、服务端排序分页、连接级去重和断线重建开始，后续再扩展共享查询计算与高可用。
