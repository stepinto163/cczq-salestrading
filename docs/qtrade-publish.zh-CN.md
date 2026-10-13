# QTrade 意向发布与端到端用例

## 接口与业务边界

服务 `SERVER.QtIntakeSvr`，主题 `ficc.qtrade.quote.publish`；Java/C# 类型均为 `QTradePublishRequest` → `QTradePublishData`。通过现有 FIX 会话发送 U41，以 U29 业务回执判断是否受理。发布不直接调用 QuoteSvr 创建报价；实际接收的 S 报文仍走 inf1 归属映射、待确认、人工确认、正式行情通路。

当前仅新增意向，支持文本和结构化报价、明确的单聊或群聊目标；不包含撤销、修改、议价、成交或群内 @ 成员。现有服务名不改，避免部署兼容性变更。

普通用户通过 DPS 会话识别，senderQtAccount 必须属于本人 inf1。可信内部调用复用现有 intake.token，可代表本机构账户发布；QTrade 最终校验发送账号权限。不要把内部 token 存入用例 JSON。

## 文本请求示例

```json
{
  "requestId": "调用跟踪编号",
  "messageId": "107090227261013220000abcdef123",
  "senderQtAccount": "2881631456",
  "mode": "TEXT",
  "preText": "Bid 240210.IB 2.5000 1w +1",
  "postText": "测试标记",
  "recipients": [{"routingType": 58, "routingId": "3004468043"}]
}
```

群聊改为 routingType=53，routingId 填群号（测试群1125362859）。支持51/52加密QQ/QT、53群、57QQ、58QT、59混合账号。每次1～200个明确收件目标，不支持ALL或标签广播。

STRUCTURED 模式提供 quotes 数组，最多10条。例如：

```json
{
  "clientQuoteId": "client-quote-001",
  "securityId": "240210", "exchange": "XCFE", "symbol": "24国开10",
  "side": "1", "bidPriceType": 1, "bidPrice": "2.5000",
  "bidQuantityYuan": "10000", "bidTradeDate": "2026-10-13",
  "bidSettlementType": "2"
}
```

数量单位是元；清算1=T+0、2=T+1。价格类型1=到期收益率、2=行权收益率、3=净价、4=全价。side=2使用对应offer字段，side=y同时提供两侧。TEXT 不允许携带 quotes。preText/postText各不超过300字符。

## 标识与结果

- requestId：本平台调用跟踪，不作为 QTrade 意向标识。
- messageId：QTrade MsgID(18044)，9～30位协议允许字符；调用方为一次发布固定生成。回执和发送状态以它关联。
- clientQuoteId：结构化明细 ClQuoteID(18031)，批内唯一，最长30字符；不是平台内部 quoteId 或 QTrade QuoteID。
- 新增结构化意向的 QuoteID(117) 发送 -10000，真实 QuoteID/NegotiationID 由后续 QTrade 报文提供。

外层 Code=0000 表示成功返回发布结果，必须继续检查 Data.status：ACKNOWLEDGED=U29全部成功，PARTIAL=部分失败，REJECTED=全部失败或FIX未接受，UNKNOWN=超时/断线导致结果不确定。Data.applicationCode 对应11251；recipients保留逐目标错误，fixSequence用于排查会话拒绝。受理成功不等于接收方已收到 S 或 STC 已显示报价。

同一 messageId、相同内容再次调用只返回持久化回执，不再次发送；内容不同报冲突。超时或断线不自动重发，也不能自动换新 messageId 重试。迟到 U29 会更新回执。发送前持久化记录用于处理进程在发送前后退出的歧义；它不替代 FIX 序号机制。

配置 publish.ackTimeoutMillis=3000，publish.receiptsPath=./data/publish-receipts，源码和 Salt 模板一致。回执目录须持久保留。超频立即拒绝，不入发送队列；本机限制2请求/秒、50请求/分钟。QTrade另有消息总量等机构级配额，多个进程合计及上游权限仍以上游回执为准。

## UI 自动化

用例24：李继存→龚逸单聊结构化；25：同一对手文本；26：销售交易内测群双向两行文本。用例参数可在现有管理窗口插入、编辑、删除。收件人登录使用 adrian；发送人 lijc.inf1=2881631456，收件人 adrian.inf1=3004468043。

执行前设置 STC_UI_USER / STC_UI_PASSWORD 为接收方登录信息，STC_UI_QTRADE_PUBLISH=1，并通过 STC_UI_INTAKE_TOKEN 传入既有内部凭据。操作间隔仍默认为2秒。必须有真实 FIX 在线连接及发布/接收权限；没有这些条件的失败不能算 E2E 通过。

管理窗口仍默认 test2；这些用例要求 receiverLogin=adrian，发送前核对登录名。不改变普通测试的默认账户。环境变量需在启动管理窗口的进程中设置，已有窗口需重新打开后才能继承。使用 `iteration1/scripts/autotest.cmd start` 打开管理窗口，可编辑用例24～26。

每例依次检查：U29受理 → 带本例标记的真实待确认行 → 接收人/双边ID/价格/单位/清算 → UI确认 → 行情可见。群聊同时检查一条原文对应一组。报告保留回执、接收记录及截图。清理仅删除当前用户已收到的本例 STC 报价，不撤回 QT 消息，不清理其他群成员收到的报价；迟到消息可能需要按测试标记另行核对。

依据：本地《标准方案 腾讯企点QTrade API手册_现券_价格广播方案(FIX)V1.0.7》及《接口定义 现券-价格广播FIX V1.9》。SZ旧实现仅作代码参考，不沿用吞异常后返回成功的行为。

用例通过 CRM 工具栏选择指定 contactName/institutionName，再一次性确认本批报价。需要预先存在正确的 CRM 联系人，不能用任意候选代替。当前本机经用户批准添加了李继存测试联系人，归属长城证券股份有限公司，ID为 ui-qtrade-lijc-20261013；不自动修改 QA CRM 数据。

价格参数支持 `${uniquePrice}`，每次生成2～2.9999的四位小数测试价格，避免 QTrade 复用完全相同的既有意向而不再下发；仍有取值碰撞可能，重跑应核对回执及原文。跟踪标记使用纯字母，避免 QT 文本识别把跟踪号数字当作价格。测试量固定1万，不是负载测试。

## 2026-10-13 实测

三条用例在真实 PRL 会话、adrian 桌面登录下全部通过：单聊结构化、单聊文本、群聊双行文本。报告位于客户端 runtime/ui-test-qtrade-publish/ui-test-reports/20261013-225333。分别耗时11.1秒、11.4秒、12.9秒，包含2秒操作间隔，不代表接口延迟。每例清理成功；群内其他收件人的待确认报价保留。

后续日志脱敏与展开截图复核：20261013-225901 群聊用例再次通过，截图显示同组 Bid/Ofr 两条待确认行，客户端报告日志不包含内部凭据。旧测试日志中该凭据已脱敏。

前置问题记录：LoginSvr、BaseSvr、NotificationSvr 进程存在但发现离线，重启恢复；adrian 缺少role，用户授权复制test2本机角色；未匹配 CRM 时确认正确拒绝。FIX 编码发现组18117须以55开头，字段顺序已与 SZ 编译类 QuoteStructured.NoQuotes 对齐，并补上顺序断言及 U41/U29 本机真实 socket 测试。Java QtIntake测试执行39项、跳过10项外部数据库测试；common执行65项、跳过1项。
