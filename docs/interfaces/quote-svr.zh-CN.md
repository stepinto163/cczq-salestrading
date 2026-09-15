# QuoteSvr接口说明（首段评审稿）

> 本文件保留为早期分段评审记录；当前完整接口契约请评审[FICC服务API规范](../ficc-api-spec.zh-CN.md)，不要将两稿混合实现。

日期：2026-09-11。本稿先描述“创建 → 确认 → 推送”及单笔报价变更历史查询。其他报价动作及各服务接口，后续按相同格式补充。示例为新设计，不是已实现的旧接口。

报价字段、类型、单位及枚举统一引用[Quote报价模型与字典](../models/quote-model.zh-CN.md)。本文只描述各动作的请求、应答、推送和执行约定。

2026-09-13职责更新：QuoteSvr同时维护行情条目及价格簿，提供报价和价格查询/订阅，不另部署FICC行情服务。本文为早期接口记录；当前报价及新增行情接口以完整API评审稿为准，模型见[行情模型与STC映射](../models/market-data-model.zh-CN.md)，不将QuoteReport当作价格簿快照。

完整的具体消息见[QuoteSvr消息示例](quote-svr-examples.zh-CN.md)：覆盖create、confirm、update、refer、unrefer、delete、get、history.query及失败应答，使用同一报价示例连续版本变化。

## 1．消息约定

| 项目 | 约定 |
| --- | --- |
| 服务名 | `QuoteSvr` |
| 消息方式 | DPS请求/应答、订阅/发布；JSON业务报文 |
| 本文请求topic | `ficc.quote.create`、`ficc.quote.confirm`、`ficc.quote.history.query` |
| 客户端推送topic | `ficc.quote.report.<sessionId>`，使用服务端分配的实际topic |
| 登录身份 | 现有消息封装携带Sid；服务端解析操作者，不在业务请求中填写userId |
| 批量写入格式 | `quotes`数组，单条写入也使用数组；应答逐条返回`results`。单笔查询直接传quoteId |

沿用STC客户端调用形式，例如：

```csharp
// QuoteOperationReply为本设计的业务应答DTO名称。
var reply = await ClientApi.Instance.RequestMessageAsync<QuoteOperationReply>(
    "QuoteSvr", "ficc.quote.create", requestJson);
```

以下JSON均为业务内容，省略DPS底层消息封装。`requestId`关联一次业务调用；`clientQuoteId`关联本地新建行；`quoteId`是服务端报价的永久标识。

## 2．创建报价

| 项目 | 内容 |
| --- | --- |
| Topic | `ficc.quote.create` |
| 方式 | 请求/应答 |
| 场景 | 客户端提交结构化报价，保存为待确认状态 |
| 状态变化 | 无记录 → `PENDING_CONFIRM` |

### 请求

```json
{
  "requestId": "create-001",
  "quotes": [{
    "clientQuoteId": "client-quote-abc",
    "securityId": "180210.IB", "side": "1",
    "quoteType": "1", "yieldType": "1", "price": "2.6500",
    "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY",
    "institutionId": "INST-01", "contactId": "CONTACT-01",
    "tradeDate": "2026-09-11", "settlType": "2",
    "urgent": true, "recommended": false, "whole": true,
    "oco": false, "packing": false, "remark": null
  }]
}
```

### 应答

```json
{
  "Code": "0000", "Msg": "处理完成",
  "Data": {
    "requestId": "create-001",
    "results": [{
      "clientQuoteId": "client-quote-abc",
      "quoteId": "Q1", "revision": "1", "status": "PENDING_CONFIRM",
      "code": "0000", "message": "创建成功"
    }]
  }
}
```

服务端生成quoteId、revision及创建时间。保存待确认允许业务信息不完整，但不能接受非法字段格式；确认必填项仍按产品最终规则校验。clientQuoteId在创建重试时保持不变，防止重复创建。

创建成功后产生`action=CREATE`的QuoteReport，携带同一requestId、clientQuoteId及已保存报价。创建应答表示已经提交，不表示已经确认发布。

## 3．确认报价

| 项目 | 内容 |
| --- | --- |
| Topic | `ficc.quote.confirm` |
| 方式 | 请求/应答 |
| 场景 | 精确报价结果或QT待确认报价，经人工核对后生效 |
| 状态变化 | `PENDING_CONFIRM` → `ACTIVE`，quoteId不变 |

### 请求

```json
{
  "requestId": "confirm-001",
  "quotes": [{
    "quoteId": "Q1", "expectedRevision": "1",
    "price": "2.6600", "remark": null
  }]
}
```

`expectedRevision`必须等于服务端当前版本。可以直接附带人工修订字段：未传保持原值，null清空允许为空的字段，false取消布尔标记；没有`fields`或`changes`包装层。

### 应答

```json
{
  "Code": "0000", "Msg": "处理完成",
  "Data": {
    "requestId": "confirm-001",
    "results": [{
      "quoteId": "Q1", "revision": "2", "status": "ACTIVE",
      "code": "0000", "message": "确认成功"
    }]
  }
}
```

字段修订、状态变化、审计及待发布事件按单条报价一起提交。失败时该条修订也不保存。多条请求允许部分成功，外层Code为0000不等于全部报价成功，客户端逐条检查results。

## 4．QuoteReport推送

| 项目 | 内容 |
| --- | --- |
| 客户端订阅topic | **`ficc.quote.report.<sessionId>`** |
| 实际topic示例 | `ficc.quote.report.session-1` |
| 消息业务名 | `QuoteReport` |
| 业务事件产生方 | QuoteSvr，在提交成功后产生 |
| 用途 | 直接更新客户端报价缓存及相关UI，不在每次通知后重新查询 |

下面是第3节确认成功对应的逻辑报告；报价对象的附加参考字段省略：

```json
{
  "type": "QuoteReport",
  "eventId": "event-002",
  "requestId": "confirm-001",
  "clientQuoteId": "client-quote-abc",
  "action": "CONFIRM",
  "operatorUserId": "USER-002",
  "operationTime": "2026-09-11T10:32:00+08:00",
  "quote": {
    "quoteId": "Q1", "revision": "2", "status": "ACTIVE",
    "securityId": "180210.IB", "side": "1",
    "quoteType": "1", "yieldType": "1", "price": "2.6600",
    "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY",
    "institutionId": "INST-01", "contactId": "CONTACT-01",
    "tradeDate": "2026-09-11", "settlType": "2",
    "urgent": true, "recommended": false, "whole": true,
    "oco": false, "packing": false, "remark": null,
    "ownerUserId": "USER-002", "createdBy": "USER-002",
    "updatedBy": "USER-002", "confirmedBy": "USER-002",
    "createTime": "2026-09-11T10:30:00+08:00",
    "updateTime": "2026-09-11T10:32:00+08:00",
    "quoteTime": "2026-09-11T10:32:00+08:00",
    "confirmTime": "2026-09-11T10:32:00+08:00"
  }
}
```

一个客户端会话共用此topic接收各报价动作；客户端通过quoteId定位已有对象，首次创建时可通过clientQuoteId绑定本地行。推送和请求应答可能交错，不能重复插入行。操作者与operationTime由服务端填写。

按[当前架构](../ficc-sales-trading-architecture.zh-CN.md)，SearchSvr消费业务事件后维护订阅结果，经GW统一分发报价及结果变化。上例定义业务报告内容，**不代表QuoteSvr和SearchSvr向客户端重复发送两条更新流**；底层发布路由、会话握手及连续批次封装留在后续SearchSvr接口段落确定。

报价列表滚动浏览，固定按updateTime从新到旧显示；不提供页码或用户自选顺序参数。正常变更走推送，初次加载及缺口恢复走快照。

## 5．查询单笔报价变更历史

| 项目 | 内容 |
| --- | --- |
| Topic | `ficc.quote.history.query` |
| 服务与方式 | QuoteSvr，DPS请求/应答 |
| 产品场景 | 报价右键菜单“报价日志”，查看谁在何时修改价格、报量或执行确认、撤销等动作 |
| 请求 | `quoteId`：必填，报价永久标识 |
| 应答 | `Data.quoteId`及`Data.records`；每条记录使用[QuoteHistoryRecord模型](../models/quote-model.zh-CN.md#6quotehistoryrecord报价变更记录) |

**请求**

```json
{"quoteId":"Q1"}
```

成功应答使用`Code="0000"`、`Msg="成功"`。完整应答见[消息示例第9节](quote-svr-examples.zh-CN.md#9查询报价日志)。本接口只读，不增加revision，也不产生QuoteReport；请求关联使用DPS现有请求/应答机制，无需另传requestId或expectedRevision。

- 返回查询时已提交的全部变更记录，按revision数值从大到小排列，同一次应答读取一致的历史范围。
- refer和unrefer始终使用同一quoteId，历史连续保留；每次成功撤销或恢复都产生新的revision和审计记录。unrefer不会创建新报价，也不会清空此前历史，多次撤销/恢复仍属于同一条报价的生命周期。
- 每次成功改变报价并生成新revision时，同步保存一条审计记录，与报价及待发布事件一起提交。失败操作和幂等重试不新增报价变更记录。
- 历史来源是QuoteSvr持久保存的审计数据；SearchSvr的跨日期历史报价搜索是另一用途，不承担本接口的审计职责。
- 逻辑删除只改变当前报价状态，不删除历史；有权限的用户仍可查询。服务端通过Sid校验历史访问权限，不能仅因报价已离开当前面板而丢失日志。
- 查询错误通过外层Code返回（如`FORBIDDEN`、`NOT_FOUND`），Data为null；无权限时不返回历史内容。

前端用before/after展示价格变化、报量、方向和标记变化，用action、operatorUserId、operationTime及reason展示操作记录。基础资料名称可用于显示，历史业务值不能被当前值覆盖。

## 6．失败与重试

| 情况 | 处理 |
| --- | --- |
| 会话无效或整份报文格式错误 | 外层Code返回错误，不执行报价动作 |
| 某条报价不允许操作 | 对应result.code返回`FORBIDDEN`或`INVALID_STATE`，其他合法项可成功 |
| 版本已变化 | 返回`REVISION_CONFLICT`及最新revision，回读后由用户决定 |
| 字段校验失败 | 返回`VALIDATION_FAILED`及fieldErrors，定位到字段 |
| 请求超时 | 使用原requestId和原内容重试，服务端返回原执行结果，不重复执行 |

失败项不产生成功的状态变更报告。风险偏离提示、其他生命周期动作及完整错误码下一段再补充。字段口径见[Quote报价模型与字典](../models/quote-model.zh-CN.md)，整体讨论背景见[报价API讨论稿](../quote-entry-api-design-discussion.zh-CN.md)。本轮优先评审“topic、请求、应答、推送、异常”的文档格式。
