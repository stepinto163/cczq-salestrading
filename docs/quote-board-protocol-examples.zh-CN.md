# 报价看板协议示例（讨论稿）

> 历史讨论资料：页码、自由排序及旧服务/topic示例不再作为当前实现依据。当前由QuoteSvr统一提供报价及行情API，NotificationSvr提供通知API，SearchSvr仅为内部搜索存储；页面使用滚动。以[现行架构](ficc-sales-trading-architecture.zh-CN.md)与[行情模型](models/market-data-model.zh-CN.md)为准，后续重新编写对应订阅协议。

对应[协议草案](quote-board-subscription-protocol.zh-CN.md)。以下topic、状态值、错误码均为新方案示例，不是旧系统已有接口。仅展示消息交互，不包含业务处理实现。

## 1．打开面板A，取得第一页

前提：当前用户的推送topic已由服务端分配并完成订阅，就绪确认方式待DPS底层核实。示例使用`stc.sbp.quote.view.push.session-1`，对应`stream-1`。登录Sid由现有消息封装携带。

客户端 → SBPSvr，请求topic：`stc.sbp.quote.view.create`

```json
{
  "type": "SUBSCRIBE", "requestId": "req-1", "panelId": "A",
  "filter": {"field": "quoteStatus", "op": "eq", "value": "ACTIVE"},
  "sort": [{"field": "price", "direction": "DESC"}],
  "page": 1, "pageSize": 2,
  "fields": ["symbol", "price", "quoteStatus"]
}
```

SBPSvr → 客户端，请求应答（仅表示已受理；实际应答与推送可能交错）：

```json
{"Code": "0000", "Msg": "已受理", "Data": {"requestId": "req-1"}}
```

SBPSvr → 客户端，发布到`stc.sbp.quote.view.push.session-1`（以下推送均使用此topic）：

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "1",
  "quotes": [
    {"quoteId": "Q1", "revision": "1", "fields": {"symbol": "债券甲", "price": "101.00", "quoteStatus": "ACTIVE"}},
    {"quoteId": "Q2", "revision": "1", "fields": {"symbol": "债券乙", "price": "100.00", "quoteStatus": "ACTIVE"}}
  ],
  "panels": [{
    "kind": "SNAPSHOT", "requestId": "req-1", "subscriptionId": "sub-A", "panelId": "A", "version": "1",
    "totalCount": 3, "rowIds": ["Q1", "Q2"]
  }],
  "evictQuoteIds": []
}
```

客户端整批应用成功 → SBPSvr，请求topic：`stc.sbp.quote.view.ack`

```json
{"type": "ACK", "streamId": "stream-1", "sequence": "1"}
```

```json
{"Code": "0000", "Msg": "成功", "Data": {"streamId": "stream-1", "sequence": "1"}}
```

## 2．打开面板B，显示相同报价，但多显示机构

客户端 → SBPSvr，`stc.sbp.quote.view.create`：

```json
{
  "type": "SUBSCRIBE", "requestId": "req-2", "panelId": "B",
  "filter": {"field": "quoteStatus", "op": "eq", "value": "ACTIVE"},
  "sort": [{"field": "price", "direction": "DESC"}],
  "page": 1, "pageSize": 2,
  "fields": ["symbol", "price", "quoteStatus", "institution"]
}
```

```json
{"Code": "0000", "Msg": "已受理", "Data": {"requestId": "req-2"}}
```

服务端推送：补充机构字段，报价本身未变化，`revision`仍为`1`。

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "2",
  "quotes": [
    {"quoteId": "Q1", "revision": "1", "fields": {"symbol": "债券甲", "price": "101.00", "quoteStatus": "ACTIVE", "institution": "机构甲"}},
    {"quoteId": "Q2", "revision": "1", "fields": {"symbol": "债券乙", "price": "100.00", "quoteStatus": "ACTIVE", "institution": "机构乙"}}
  ],
  "panels": [{
    "kind": "SNAPSHOT", "requestId": "req-2", "subscriptionId": "sub-B", "panelId": "B", "version": "1",
    "totalCount": 3, "rowIds": ["Q1", "Q2"]
  }],
  "evictQuoteIds": []
}
```

## 后续示例的统一起点

以下各例独立，从第2例完成后开始，不依次累积；省略每批处理成功后的ACK。

```text
服务端有效报价：Q1=101.00，Q2=100.00，Q3=99.00；revision均为1。
面板A：sub-A，version=1，第一页，[Q1,Q2]，totalCount=3。
面板B：sub-B，version=1，第一页，[Q1,Q2]，totalCount=3。
客户端缓存：Q1、Q2；已应用sequence=2。
```

## 3．Q1价格变化，两个面板的位置都不变

服务端推送：Q1只发一次，无需分别给A、B发送更新消息。

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "3",
  "quotes": [
    {"quoteId": "Q1", "revision": "2", "fields": {"symbol": "债券甲", "price": "101.50", "quoteStatus": "ACTIVE", "institution": "机构甲"}}
  ],
  "panels": [], "evictQuoteIds": []
}
```

## 4．Q2价格上涨，两个面板都需要调整位置

服务端推送：两个面板均从`[Q1,Q2]`变为`[Q2,Q1]`。

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "3",
  "quotes": [
    {"quoteId": "Q2", "revision": "2", "fields": {"symbol": "债券乙", "price": "102.00", "quoteStatus": "ACTIVE", "institution": "机构乙"}}
  ],
  "panels": [
    {"kind": "DELTA", "subscriptionId": "sub-A", "version": "1", "totalCount": 3, "operations": [{"op": "MOVE", "quoteId": "Q2", "toIndex": 0}]},
    {"kind": "DELTA", "subscriptionId": "sub-B", "version": "1", "totalCount": 3, "operations": [{"op": "MOVE", "quoteId": "Q2", "toIndex": 0}]}
  ],
  "evictQuoteIds": []
}
```

## 5．新增Q4，排到第一页最前面

服务端推送：两个面板均变为`[Q4,Q1]`；Q2仍存在于服务端，只是离开当前页。

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "3",
  "quotes": [
    {"quoteId": "Q4", "revision": "1", "fields": {"symbol": "债券丁", "price": "102.00", "quoteStatus": "ACTIVE", "institution": "机构丁"}}
  ],
  "panels": [
    {"kind": "DELTA", "subscriptionId": "sub-A", "version": "1", "totalCount": 4, "operations": [{"op": "REMOVE", "quoteId": "Q2"}, {"op": "INSERT", "quoteId": "Q4", "index": 0}]},
    {"kind": "DELTA", "subscriptionId": "sub-B", "version": "1", "totalCount": 4, "operations": [{"op": "REMOVE", "quoteId": "Q2"}, {"op": "INSERT", "quoteId": "Q4", "index": 0}]}
  ],
  "evictQuoteIds": ["Q2"]
}
```

## 6．Q1被Refer，不再满足有效报价条件；Q3补位

Refer业务操作已通过独立业务接口完成；这里只展示页面推送。删除Q1、或修改后使其不再满足过滤条件时，页面成员变化也可采用此形式。

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "3",
  "quotes": [
    {"quoteId": "Q3", "revision": "1", "fields": {"symbol": "债券丙", "price": "99.00", "quoteStatus": "ACTIVE", "institution": "机构丙"}}
  ],
  "panels": [
    {"kind": "DELTA", "subscriptionId": "sub-A", "version": "1", "totalCount": 2, "operations": [{"op": "REMOVE", "quoteId": "Q1"}, {"op": "INSERT", "quoteId": "Q3", "index": 1}]},
    {"kind": "DELTA", "subscriptionId": "sub-B", "version": "1", "totalCount": 2, "operations": [{"op": "REMOVE", "quoteId": "Q1"}, {"op": "INSERT", "quoteId": "Q3", "index": 1}]}
  ],
  "evictQuoteIds": ["Q1"]
}
```

## 7．面板A翻到第二页，面板B不变

客户端 → SBPSvr，`stc.sbp.quote.view.changePage`：

```json
{"type": "CHANGE_PAGE", "requestId": "req-page", "subscriptionId": "sub-A", "expectedVersion": "1", "page": 2, "pageSize": 2}
```

```json
{"Code": "0000", "Msg": "已受理", "Data": {"requestId": "req-page"}}
```

服务端推送：A变为`[Q3]`；Q1、Q2仍被B使用，不能释放。

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "3",
  "quotes": [{"quoteId": "Q3", "revision": "1", "fields": {"symbol": "债券丙", "price": "99.00", "quoteStatus": "ACTIVE"}}],
  "panels": [{
    "kind": "SNAPSHOT", "requestId": "req-page", "subscriptionId": "sub-A", "panelId": "A", "version": "2",
    "totalCount": 3, "rowIds": ["Q3"]
  }],
  "evictQuoteIds": []
}
```

## 8．面板A修改过滤条件，同时改为价格升序

客户端 → SBPSvr，`stc.sbp.quote.view.changeQuery`：

```json
{
  "type": "CHANGE_QUERY", "requestId": "req-query", "subscriptionId": "sub-A", "expectedVersion": "1",
  "filter": {"op": "and", "args": [
    {"field": "quoteStatus", "op": "eq", "value": "ACTIVE"},
    {"field": "price", "op": "gte", "value": "100.00"}
  ]},
  "sort": [{"field": "price", "direction": "ASC"}],
  "page": 1, "pageSize": 2, "fields": ["symbol", "price", "quoteStatus"]
}
```

```json
{"Code": "0000", "Msg": "已受理", "Data": {"requestId": "req-query"}}
```

服务端推送：A变为`[Q2,Q1]`；所需报价已在缓存中，无需重发。

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "3", "quotes": [],
  "panels": [{
    "kind": "SNAPSHOT", "requestId": "req-query", "subscriptionId": "sub-A", "panelId": "A", "version": "2",
    "totalCount": 2, "rowIds": ["Q2", "Q1"]
  }],
  "evictQuoteIds": []
}
```

## 9．新增报价未进入当前页，仅总数变化

新增Q4价格为98.00，按价格降序排在第四位；两个面板的当前页均不包含Q4。

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "3", "quotes": [],
  "panels": [
    {"kind": "DELTA", "subscriptionId": "sub-A", "version": "1", "totalCount": 4, "operations": []},
    {"kind": "DELTA", "subscriptionId": "sub-B", "version": "1", "totalCount": 4, "operations": []}
  ],
  "evictQuoteIds": []
}
```

## 10．关闭面板A，面板B继续使用报价

客户端 → SBPSvr，`stc.sbp.quote.view.close`：

```json
{"type": "UNSUBSCRIBE", "requestId": "req-close", "subscriptionId": "sub-A", "expectedVersion": "1"}
```

```json
{"Code": "0000", "Msg": "已受理", "Data": {"requestId": "req-close"}}
```

```json
{
  "type": "BATCH", "streamId": "stream-1", "sequence": "3", "quotes": [],
  "panels": [{"kind": "CLOSED", "requestId": "req-close", "subscriptionId": "sub-A"}],
  "evictQuoteIds": []
}
```

## 11．查询版本不符，请求被拒绝

客户端 → SBPSvr，`stc.sbp.quote.view.changePage`：

```json
{"type": "CHANGE_PAGE", "requestId": "req-old", "subscriptionId": "sub-A", "expectedVersion": "0", "page": 2, "pageSize": 2}
```

服务端应答；不产生页面变化批次，保留原页面：

```json
{"Code": "VERSION_CONFLICT", "Msg": "查询版本不符", "Data": {"requestId": "req-old", "subscriptionId": "sub-A", "currentVersion": "1"}}
```

## 12．漏消息或断线，第一版直接重建

```text
客户端已应用：stream-1，sequence=2
客户端收到：  stream-1，sequence=4
客户端动作：  停止应用该批次，不ACK 4，发送以下请求。
```

客户端 → SBPSvr，`stc.sbp.quote.view.resync`：

```json
{"type": "RESYNC", "streamId": "stream-1", "lastAppliedSequence": "2"}
```

服务端应答（示例补充：用Data明确要求重建，具体会话建立接口待定）：

```json
{"Code": "0000", "Msg": "请重建视图会话", "Data": {"action": "RECREATE"}}
```

```text
客户端：保留各面板的过滤/排序/页码设置，清空旧报价缓存和页面ID列表。
服务端：停止并释放旧视图会话，为新会话分配推送流，例如stream-2。
客户端：完成新推送流订阅后，重新发送各面板的SUBSCRIBE请求。
服务端：从sequence=1开始发送新BATCH；首批页面快照必须携带所需报价数据。
客户端：忽略旧stream-1的迟到消息。

断线重连：连接及登录恢复后，采用同样的重建流程，不沿用旧缓存直接接增量。
```
