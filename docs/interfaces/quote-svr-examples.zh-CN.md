# QuoteSvr完整消息示例

> 本文件保留为早期消息示例；当前完整接口及示例请评审[FICC服务API规范](../ficc-api-spec.zh-CN.md)，与本文件不一致处以新评审稿为准。

日期：2026-09-11。本文与[QuoteSvr接口说明](quote-svr.zh-CN.md)、[Quote模型与字典](../models/quote-model.zh-CN.md)放在一起，先覆盖一个服务，方便评审格式。

示例为新接口的具体业务报文，不是生产抓包，也没有调用实际服务。180210.IB、INST-01等是假设已存在且匹配的测试主数据。以下JSON不使用占位符或省略号；可选的来源、参考资料和校验提示未出现时表示该示例不携带这些字段。

## 使用约定

本组消息采用FICC字典v1（对齐STC销售报价）：side="1"表示Bid，quoteType="1"、yieldType="1"表示到期收益率，settlType="2"表示T+1。请求、快照、推送及历史中的报价字段使用相同字典；price仍为单一报价值。

- 请求服务器为 **QuoteSvr**，请求/应答沿用DPS。下文JSON是业务Body；已认证会话由现有封装携带，服务端解析为USER-002。
- 推送均使用预先由服务端分配并已完成订阅的 **ficc.quote.report.session-1**，客户端具有相关报价权限。QuoteReport展示逻辑业务内容，连续批次及页面成员变化由SearchSvr/GW传递机制另行定义，不能仅凭eventId认定推送没有缺口。
- 第1～7节按顺序执行，Q1的ID不变，版本递增。第8节是独立失败场景，使用注明的前置状态；第9节查询第7节完成后的完整历史。
- 写请求每条成功结果有一份对应的逻辑报告；应答与推送到达顺序不保证。get及history.query只有应答，不产生新的业务事件。

| 节次 | 请求topic | 结果 |
| --- | --- | --- |
| 1 | ficc.quote.create | 新建待确认报价，revision=1 |
| 2 | ficc.quote.confirm | 确认生效，revision=2 |
| 3 | ficc.quote.update | 修改价格和标记，revision=3 |
| 4 | ficc.quote.refer | 撤销，revision=4 |
| 5 | ficc.quote.unrefer | 恢复，revision=5 |
| 6 | ficc.quote.get | 返回当前完整报价，revision仍为5 |
| 7 | ficc.quote.delete | 先再次Refer到revision=6，再删除到7 |
| 9 | ficc.quote.history.query | 删除后读取七次变更，按revision从7到1返回 |

## 1．创建

创建待确认报价，保留本地clientQuoteId以绑定服务端Q1。

**请求** → `ficc.quote.create`

```json
{
  "requestId": "create-001",
  "quotes": [
    {
      "clientQuoteId": "client-quote-abc", "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1",
      "price": "2.6500", "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
      "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": true,
      "recommended": false, "whole": true, "oco": false, "packing": false, "remark": "电话报价"
    }
  ]
}
```

**应答**

```json
{
  "Code": "0000", "Msg": "处理完成",
  "Data": {
    "requestId": "create-001",
    "results": [
      {
        "clientQuoteId": "client-quote-abc", "quoteId": "Q1", "revision": "1", "status": "PENDING_CONFIRM",
        "code": "0000", "message": "创建成功"
      }
    ]
  }
}
```

**推送** → `ficc.quote.report.session-1`

```json
{
  "type": "QuoteReport", "eventId": "event-001", "requestId": "create-001", "clientQuoteId": "client-quote-abc",
  "action": "CREATE", "operatorUserId": "USER-002", "operationTime": "2026-09-11T10:30:00+08:00",
  "quote": {
    "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "1", "status": "PENDING_CONFIRM",
    "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6500", "quantity": "1000",
    "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01", "contactId": "CONTACT-01",
    "tradeDate": "2026-09-11", "settlType": "2", "urgent": true, "recommended": false, "whole": true,
    "oco": false, "packing": false, "remark": "电话报价", "ownerUserId": "USER-002", "createdBy": "USER-002",
    "updatedBy": "USER-002", "confirmedBy": null, "createTime": "2026-09-11T10:30:00+08:00",
    "updateTime": "2026-09-11T10:30:00+08:00", "quoteTime": null, "confirmTime": null
  }
}
```

## 2．确认

同时修改价格并明确清空备注；报价原ID保持不变。

**请求** → `ficc.quote.confirm`

```json
{"requestId":"confirm-001","quotes":[{"quoteId":"Q1","expectedRevision":"1","price":"2.6600","remark":null}]}
```

**应答**

```json
{
  "Code": "0000", "Msg": "处理完成",
  "Data": {
    "requestId": "confirm-001",
    "results": [{"quoteId":"Q1","revision":"2","status":"ACTIVE","code":"0000","message":"确认成功"}]
  }
}
```

**推送** → `ficc.quote.report.session-1`

```json
{
  "type": "QuoteReport", "eventId": "event-002", "requestId": "confirm-001", "clientQuoteId": "client-quote-abc",
  "action": "CONFIRM", "operatorUserId": "USER-002", "operationTime": "2026-09-11T10:32:00+08:00",
  "quote": {
    "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "2", "status": "ACTIVE",
    "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6600", "quantity": "1000",
    "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01", "contactId": "CONTACT-01",
    "tradeDate": "2026-09-11", "settlType": "2", "urgent": true, "recommended": false, "whole": true,
    "oco": false, "packing": false, "remark": null, "ownerUserId": "USER-002", "createdBy": "USER-002",
    "updatedBy": "USER-002", "confirmedBy": "USER-002", "createTime": "2026-09-11T10:30:00+08:00",
    "updateTime": "2026-09-11T10:32:00+08:00", "quoteTime": "2026-09-11T10:32:00+08:00",
    "confirmTime": "2026-09-11T10:32:00+08:00"
  }
}
```

## 3．修改

改价、取消急、设置荐和OCO。未传的数量等保持原值；推送仍包含完整示例报价。

**请求** → `ficc.quote.update`

```json
{
  "requestId": "update-001",
  "quotes": [
    {
      "quoteId": "Q1", "expectedRevision": "2", "price": "2.6700", "urgent": false, "recommended": true,
      "oco": true, "packing": false
    }
  ]
}
```

**应答**

```json
{
  "Code": "0000", "Msg": "处理完成",
  "Data": {
    "requestId": "update-001",
    "results": [{"quoteId":"Q1","revision":"3","status":"ACTIVE","code":"0000","message":"修改成功"}]
  }
}
```

**推送** → `ficc.quote.report.session-1`

```json
{
  "type": "QuoteReport", "eventId": "event-003", "requestId": "update-001", "clientQuoteId": "client-quote-abc",
  "action": "UPDATE", "operatorUserId": "USER-002", "operationTime": "2026-09-11T10:33:00+08:00",
  "quote": {
    "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "3", "status": "ACTIVE",
    "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700", "quantity": "1000",
    "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01", "contactId": "CONTACT-01",
    "tradeDate": "2026-09-11", "settlType": "2", "urgent": false, "recommended": true, "whole": true,
    "oco": true, "packing": false, "remark": null, "ownerUserId": "USER-002", "createdBy": "USER-002",
    "updatedBy": "USER-002", "confirmedBy": "USER-002", "createTime": "2026-09-11T10:30:00+08:00",
    "updateTime": "2026-09-11T10:33:00+08:00", "quoteTime": "2026-09-11T10:33:00+08:00",
    "confirmTime": "2026-09-11T10:32:00+08:00"
  }
}
```

## 4．Refer

撤销有效报价。更新updateTime，保留上次发布的quoteTime和首次confirmTime；原因写入操作审计。

**请求** → `ficc.quote.refer`

```json
{"requestId":"refer-001","quotes":[{"quoteId":"Q1","expectedRevision":"3","reason":"对手撤回报价"}]}
```

**应答**

```json
{
  "Code": "0000", "Msg": "处理完成",
  "Data": {
    "requestId": "refer-001",
    "results": [{"quoteId":"Q1","revision":"4","status":"REFERRED","code":"0000","message":"撤销成功"}]
  }
}
```

**推送** → `ficc.quote.report.session-1`

```json
{
  "type": "QuoteReport", "eventId": "event-004", "requestId": "refer-001", "clientQuoteId": "client-quote-abc",
  "action": "REFER", "operatorUserId": "USER-002", "operationTime": "2026-09-11T10:34:00+08:00",
  "quote": {
    "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "4", "status": "REFERRED",
    "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700", "quantity": "1000",
    "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01", "contactId": "CONTACT-01",
    "tradeDate": "2026-09-11", "settlType": "2", "urgent": false, "recommended": true, "whole": true,
    "oco": true, "packing": false, "remark": null, "ownerUserId": "USER-002", "createdBy": "USER-002",
    "updatedBy": "USER-002", "confirmedBy": "USER-002", "createTime": "2026-09-11T10:30:00+08:00",
    "updateTime": "2026-09-11T10:34:00+08:00", "quoteTime": "2026-09-11T10:33:00+08:00",
    "confirmTime": "2026-09-11T10:32:00+08:00"
  }
}
```

## 5．Unrefer

同一业务日恢复报价，刷新quoteTime，不更换ID或首次确认信息。

**请求** → `ficc.quote.unrefer`

```json
{"requestId":"unrefer-001","quotes":[{"quoteId":"Q1","expectedRevision":"4"}]}
```

**应答**

```json
{
  "Code": "0000", "Msg": "处理完成",
  "Data": {
    "requestId": "unrefer-001",
    "results": [{"quoteId":"Q1","revision":"5","status":"ACTIVE","code":"0000","message":"恢复成功"}]
  }
}
```

**推送** → `ficc.quote.report.session-1`

```json
{
  "type": "QuoteReport", "eventId": "event-005", "requestId": "unrefer-001", "clientQuoteId": "client-quote-abc",
  "action": "UNREFER", "operatorUserId": "USER-002", "operationTime": "2026-09-11T10:35:00+08:00",
  "quote": {
    "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "5", "status": "ACTIVE",
    "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700", "quantity": "1000",
    "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01", "contactId": "CONTACT-01",
    "tradeDate": "2026-09-11", "settlType": "2", "urgent": false, "recommended": true, "whole": true,
    "oco": true, "packing": false, "remark": null, "ownerUserId": "USER-002", "createdBy": "USER-002",
    "updatedBy": "USER-002", "confirmedBy": "USER-002", "createTime": "2026-09-11T10:30:00+08:00",
    "updateTime": "2026-09-11T10:35:00+08:00", "quoteTime": "2026-09-11T10:35:00+08:00",
    "confirmTime": "2026-09-11T10:32:00+08:00"
  }
}
```

## 6．读取最新报价

读取Q1的最新状态，无状态修改、无新报告。

**请求** → `ficc.quote.get`

```json
{"quoteId":"Q1"}
```

**应答**

```json
{
  "Code": "0000", "Msg": "成功",
  "Data": {
    "quote": {
      "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "5", "status": "ACTIVE",
      "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700", "quantity": "1000",
      "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01", "contactId": "CONTACT-01",
      "tradeDate": "2026-09-11", "settlType": "2", "urgent": false, "recommended": true, "whole": true,
      "oco": true, "packing": false, "remark": null, "ownerUserId": "USER-002", "createdBy": "USER-002",
      "updatedBy": "USER-002", "confirmedBy": "USER-002", "createTime": "2026-09-11T10:30:00+08:00",
      "updateTime": "2026-09-11T10:35:00+08:00", "quoteTime": "2026-09-11T10:35:00+08:00",
      "confirmTime": "2026-09-11T10:32:00+08:00"
    }
  }
}
```

## 7．删除：先再次撤销

延续第5节的有效报价。删除前必须先撤销。

**请求** → `ficc.quote.refer`

```json
{"requestId":"refer-002","quotes":[{"quoteId":"Q1","expectedRevision":"5","reason":"撤回并删除"}]}
```

**应答**

```json
{
  "Code": "0000", "Msg": "处理完成",
  "Data": {
    "requestId": "refer-002",
    "results": [{"quoteId":"Q1","revision":"6","status":"REFERRED","code":"0000","message":"撤销成功"}]
  }
}
```

**推送** → `ficc.quote.report.session-1`

```json
{
  "type": "QuoteReport", "eventId": "event-006", "requestId": "refer-002", "clientQuoteId": "client-quote-abc",
  "action": "REFER", "operatorUserId": "USER-002", "operationTime": "2026-09-11T10:36:00+08:00",
  "quote": {
    "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "6", "status": "REFERRED",
    "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700", "quantity": "1000",
    "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01", "contactId": "CONTACT-01",
    "tradeDate": "2026-09-11", "settlType": "2", "urgent": false, "recommended": true, "whole": true,
    "oco": true, "packing": false, "remark": null, "ownerUserId": "USER-002", "createdBy": "USER-002",
    "updatedBy": "USER-002", "confirmedBy": "USER-002", "createTime": "2026-09-11T10:30:00+08:00",
    "updateTime": "2026-09-11T10:36:00+08:00", "quoteTime": "2026-09-11T10:35:00+08:00",
    "confirmTime": "2026-09-11T10:32:00+08:00"
  }
}
```

### 执行删除

**请求** → `ficc.quote.delete`

```json
{"requestId":"delete-001","quotes":[{"quoteId":"Q1","expectedRevision":"6","reason":"录入已作废"}]}
```

**应答**

```json
{
  "Code": "0000", "Msg": "处理完成",
  "Data": {
    "requestId": "delete-001",
    "results": [{"quoteId":"Q1","revision":"7","status":"DELETED","code":"0000","message":"删除成功"}]
  }
}
```

**推送** → `ficc.quote.report.session-1`

```json
{
  "type": "QuoteReport", "eventId": "event-007", "requestId": "delete-001", "clientQuoteId": "client-quote-abc",
  "action": "DELETE", "operatorUserId": "USER-002", "operationTime": "2026-09-11T10:37:00+08:00",
  "quote": {
    "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "7", "status": "DELETED",
    "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700", "quantity": "1000",
    "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01", "contactId": "CONTACT-01",
    "tradeDate": "2026-09-11", "settlType": "2", "urgent": false, "recommended": true, "whole": true,
    "oco": true, "packing": false, "remark": null, "ownerUserId": "USER-002", "createdBy": "USER-002",
    "updatedBy": "USER-002", "confirmedBy": "USER-002", "createTime": "2026-09-11T10:30:00+08:00",
    "updateTime": "2026-09-11T10:37:00+08:00", "quoteTime": "2026-09-11T10:35:00+08:00",
    "confirmTime": "2026-09-11T10:32:00+08:00"
  }
}
```

客户端依据DELETED及订阅成员变化移除显示；服务端保留审计。删除报告中的createTime、quoteTime、confirmTime仍保留原业务时间。

## 8．失败消息示例（独立场景）

以下均假设Q1处于第3节完成后的ACTIVE、revision=3；失败不改变版本，也不产生成功的QuoteReport。

### 8.1 使用过旧版本修改

**请求** → `ficc.quote.update`

```json
{"requestId":"update-stale-001","quotes":[{"quoteId":"Q1","expectedRevision":"2","quantity":"1500"}]}
```

**应答**

```json
{
  "Code": "0000", "Msg": "处理完成，存在失败项",
  "Data": {
    "requestId": "update-stale-001",
    "results": [{"quoteId":"Q1","revision":"3","status":"ACTIVE","code":"REVISION_CONFLICT","message":"报价已更新，请读取最新数据后重试"}]
  }
}
```

### 8.2 OCO和打包同时为true

**请求** → `ficc.quote.update`

```json
{"requestId":"update-invalid-001","quotes":[{"quoteId":"Q1","expectedRevision":"3","oco":true,"packing":true}]}
```

**应答**

```json
{
  "Code": "0000", "Msg": "处理完成，存在失败项",
  "Data": {
    "requestId": "update-invalid-001",
    "results": [
      {
        "quoteId": "Q1", "revision": "3", "status": "ACTIVE", "code": "VALIDATION_FAILED",
        "message": "OCO与打包不能同时启用",
        "fieldErrors": [{"field":"oco","message":"不能与packing同时为true"},{"field":"packing","message":"不能与oco同时为true"}]
      }
    ]
  }
}
```

外层Code=0000表示已完成逐条处理，不代表每条报价都成功。超时重试保持原requestId和原内容；修改内容后重新提交使用新requestId。

## 9．查询报价日志

接第7节删除完成后，Q1已是DELETED、revision=7。用户打开“报价日志”，仍能读取创建至删除的七次变更。第8节为独立失败场景，不计入本记录。

**请求** → QuoteSvr / `ficc.quote.history.query`

```json
{"quoteId":"Q1"}
```

**应答**：records按revision数值倒序，before/after均为当时保存的完整报价业务值。

```json
{
  "Code": "0000", "Msg": "成功",
  "Data": {
    "quoteId": "Q1",
    "records": [
      {
        "eventId": "event-007", "revision": "7", "action": "DELETE", "operatorUserId": "USER-002",
        "operationTime": "2026-09-11T10:37:00+08:00",
        "before": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "6", "status": "REFERRED",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": false,
          "recommended": true, "whole": true, "oco": true, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:36:00+08:00",
          "quoteTime": "2026-09-11T10:35:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "after": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "7", "status": "DELETED",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": false,
          "recommended": true, "whole": true, "oco": true, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:37:00+08:00",
          "quoteTime": "2026-09-11T10:35:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "reason": "录入已作废"
      },
      {
        "eventId": "event-006", "revision": "6", "action": "REFER", "operatorUserId": "USER-002",
        "operationTime": "2026-09-11T10:36:00+08:00",
        "before": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "5", "status": "ACTIVE",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": false,
          "recommended": true, "whole": true, "oco": true, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:35:00+08:00",
          "quoteTime": "2026-09-11T10:35:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "after": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "6", "status": "REFERRED",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": false,
          "recommended": true, "whole": true, "oco": true, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:36:00+08:00",
          "quoteTime": "2026-09-11T10:35:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "reason": "撤回并删除"
      },
      {
        "eventId": "event-005", "revision": "5", "action": "UNREFER", "operatorUserId": "USER-002",
        "operationTime": "2026-09-11T10:35:00+08:00",
        "before": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "4", "status": "REFERRED",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": false,
          "recommended": true, "whole": true, "oco": true, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:34:00+08:00",
          "quoteTime": "2026-09-11T10:33:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "after": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "5", "status": "ACTIVE",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": false,
          "recommended": true, "whole": true, "oco": true, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:35:00+08:00",
          "quoteTime": "2026-09-11T10:35:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "reason": null
      },
      {
        "eventId": "event-004", "revision": "4", "action": "REFER", "operatorUserId": "USER-002",
        "operationTime": "2026-09-11T10:34:00+08:00",
        "before": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "3", "status": "ACTIVE",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": false,
          "recommended": true, "whole": true, "oco": true, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:33:00+08:00",
          "quoteTime": "2026-09-11T10:33:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "after": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "4", "status": "REFERRED",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": false,
          "recommended": true, "whole": true, "oco": true, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:34:00+08:00",
          "quoteTime": "2026-09-11T10:33:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "reason": "对手撤回报价"
      },
      {
        "eventId": "event-003", "revision": "3", "action": "UPDATE", "operatorUserId": "USER-002",
        "operationTime": "2026-09-11T10:33:00+08:00",
        "before": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "2", "status": "ACTIVE",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6600",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": true,
          "recommended": false, "whole": true, "oco": false, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:32:00+08:00",
          "quoteTime": "2026-09-11T10:32:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "after": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "3", "status": "ACTIVE",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6700",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": false,
          "recommended": true, "whole": true, "oco": true, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:33:00+08:00",
          "quoteTime": "2026-09-11T10:33:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "reason": null
      },
      {
        "eventId": "event-002", "revision": "2", "action": "CONFIRM", "operatorUserId": "USER-002",
        "operationTime": "2026-09-11T10:32:00+08:00",
        "before": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "1", "status": "PENDING_CONFIRM",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6500",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": true,
          "recommended": false, "whole": true, "oco": false, "packing": false, "remark": "电话报价",
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": null,
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:30:00+08:00", "quoteTime": null,
          "confirmTime": null
        },
        "after": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "2", "status": "ACTIVE",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6600",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": true,
          "recommended": false, "whole": true, "oco": false, "packing": false, "remark": null,
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": "USER-002",
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:32:00+08:00",
          "quoteTime": "2026-09-11T10:32:00+08:00", "confirmTime": "2026-09-11T10:32:00+08:00"
        },
        "reason": null
      },
      {
        "eventId": "event-001", "revision": "1", "action": "CREATE", "operatorUserId": "USER-002",
        "operationTime": "2026-09-11T10:30:00+08:00", "before": null,
        "after": {
          "quoteId": "Q1", "clientQuoteId": "client-quote-abc", "revision": "1", "status": "PENDING_CONFIRM",
          "securityId": "180210.IB", "side": "1", "quoteType": "1", "yieldType": "1", "price": "2.6500",
          "quantity": "1000", "quantityUnit": "WAN", "currency": "CNY", "institutionId": "INST-01",
          "contactId": "CONTACT-01", "tradeDate": "2026-09-11", "settlType": "2", "urgent": true,
          "recommended": false, "whole": true, "oco": false, "packing": false, "remark": "电话报价",
          "ownerUserId": "USER-002", "createdBy": "USER-002", "updatedBy": "USER-002", "confirmedBy": null,
          "createTime": "2026-09-11T10:30:00+08:00", "updateTime": "2026-09-11T10:30:00+08:00", "quoteTime": null,
          "confirmTime": null
        },
        "reason": null
      }
    ]
  }
}
```

CREATE的before为null；DELETE的after仍是完整报价，状态为DELETED。两次REFER及DELETE的reason来自对应操作请求。其余未填写原因的操作返回null；不会把报价remark当成操作原因。查询本身不产生QuoteReport。

其他服务的查询和订阅实例待接口职责确认后另建对应文档；本文不沿用旧稿的分页或自选顺序参数。
