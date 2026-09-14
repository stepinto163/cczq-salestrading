# QTrade对外通信消息清单

前端关联专题：[QTrade消息与销售交易前端模型映射](qtrade-frontend-model-mapping.zh-CN.md)，覆盖对价板、报价管理及右侧行情面板；特别说明W转输入报价的发布被注释，不能计为已启用链路。

## 1 范围与证据口径

分析仓库：`D:\workspace\greatewall\sc-app-stc-qtradesvr\sc-app-stc-qtradesvr`。

本文记录该服务与外部QTrade之间的FIX及HTTP通信。`stc.qtrade.quote.input`、`stc.qt.up.ack`等内部主题用于下游业务协作，不属于发给QTrade的网络报文。

清单来自应用源码静态分析，并使用仓库内 `lib/quickfixj-messages-fix44.jar` 的消息类常量核对FIX消息类型。未启动服务、连接QTrade或回放业务消息。“存在发送代码”不等于生产环境一定启用；实际行为还取决于配置、入口注册和业务条件。本文不是完整的QTrade协议规范，也未逐字段验证协议合法性。

## 2 通信入口

FIX应用消息的接收路径为 `CFixApi.fromApp → MessageListener.crack → onMessage`。发送通常经过 `QtMessageManagerment.sendQuote → CFixApi.sendQuote → Session.send`，部分代码直接调用 `CFixApi.sendQuote`。

`sendQuote`虽然名为“发送报价”，实际接收通用FIX Message，负责发送报价、询价、成交单据、回执和行情拉取请求等多种报文。

源码：[接收分发](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/client/CFixApi.java#L338)、[统一发送](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/client/CFixApi.java#L476)。文内源码相对链接按当前工作区目录布局编写，部分Markdown阅读器可能不支持行号锚点。

## 3 上行：本服务发送给QTrade

| FIX类型 | 消息类 | 业务用途 | 主要实现与状态 |
| --- | --- | --- | --- |
| `U41` | `QuoteStructured` | 发起询价、撤销询价；TRS询价入口也使用此消息 | `IntentionHandler`、`TrsIntentionHandler`存在发送代码 |
| `S` | `Quote` | 对话回价、回价变更或撤销；“Q一下”回价；向QT发布报价数据 | `IntentionHandler`、`QtCategoryService`、`QtradeStream`，各路径启用情况不同，见第7节 |
| `AE` | `TradeCaptureReport` | 发送协商交易或请求单据 | `QtTradeCaptureReportSendHandler`，部分错误重试路径也会发送 |
| `AR` | `TradeCaptureReportAck` | 对交易单据接受或拒绝；回传成交、撤销等结果 | `QtTradeCaptureReportAckSendHandler`、`QtQOnceRejectHandler`、`QtradeStream`及失败处理路径 |
| `U44` | `MarketCompanyQuoteSummaryUp` | 按债券集合请求本机构单券汇总行情 | `SingleBondMarketDataJob`；按缓存债券列表分组请求，实际分组上限为49条 |
| `R` | `QuoteRequest` | 主动发起“Q一下”询价 | `QoneceHandler`有调用发送的代码，但构造器中包含固定债券、价格、参与方及请求标识等样例值，不能视为完整生产实现 |

同一种FIX消息类型可以承载多个生命周期操作。例如，新增、回价、撤销并不一定分别使用不同消息类，还需要结合 `TransType`、`QuoteType`、`InquiryType` 等字段理解。

主要源码：

- [IntentionHandler](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/handler/IntentionHandler.java#L472)：`S`回价构造位于472行附近，`U41`询价和撤销构造位于577、656行附近。
- [TrsIntentionHandler](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/handler/TrsIntentionHandler.java#L147)：构造`U41`。
- [QtTradeCaptureReportSendHandler](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/handler/QtTradeCaptureReportSendHandler.java#L263)：构造`AE`。
- [QtTradeCaptureReportAckSendHandler](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/handler/QtTradeCaptureReportAckSendHandler.java#L217)：构造`AR`。
- [SingleBondMarketDataJob](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/job/SingleBondMarketDataJob.java#L92)：构造并发送`U44`。
- [QoneceHandler](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/handler/QoneceHandler.java#L73)：发送`R`，需注意其样例字段。

## 4 下行：QTrade发送给本服务

以下入口均位于 [MessageListener.java](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/lisener/MessageListener.java)。

| FIX类型 | 消息类 | 业务含义及本地处理 | 入口行号 |
| --- | --- | --- | --- |
| `R` | `QuoteRequest` | 接收询价，包括“Q一下”业务；解析询价与参与方信息，进入本地询价处理 | 211 |
| `S` | `Quote` | 接收报价、回价及协商更新；部分分支转换成销售交易输入报价并发布内部消息 | 684 |
| `AE` | `TradeCaptureReport` | 接收交易或请求单据，根据`InquiryType`等字段进入不同处理分支 | 1039 |
| `AR` | `TradeCaptureReportAck` | 接收交易单据响应，进入本地交易状态处理 | 1560 |
| `U47` | `Ack` | 通用应用确认或错误；关联原发送消息，部分分支执行失败重试或后续业务动作 | 1931 |
| `U29` | `QuoteAck` | `U41`询价及撤销的确认或错误 | 2100 |
| `U45` | `MarketCompanyQuoteSummaryDown` | 单券机构汇总行情响应；解析后写缓存并通过RabbitMQ向下游推送 | 2124 |
| `W` | `MarketDataSnapshotFullRefresh` | 机构汇总行情；解析后更新缓存并向下游推送 | 2160 |

销售交易报价入口并不是整个QT接入服务的全部职责；该服务还承担询价协商、单据处理、执行反馈、市场数据及客户资料同步。

## 5 回执关联与会话消息

### 5.1 应用回执

普通上行消息在 `CFixApi.toApp` 中按消息类型和发送序号建立关联。收到`U47`后，代码以 `RefMsgType + RefSeqNum` 查找对应的本地意向记录。

`U41`采用另一条关联路径：发送时记录 `U41 + MsgID`，收到`U29`后取回相应记录。两个接收处理器最终可生成内部 `stc.qt.up.ack` 推送。

因此，`U47`通用确认、`U29`询价确认与`AR`交易单据响应不是同一种业务确认。接口收到确认也不能直接等同于正式销售报价已确认或交易已最终完成。

### 5.2 会话层

| FIX类型 | 名称 | 已确认的应用行为 |
| --- | --- | --- |
| `A` | `Logon` | `toAdmin`补充认证字段；`fromAdmin`识别登录并更新本地连接状态 |
| `5` | `Logout` | `fromAdmin`识别退出并更新本地连接状态 |
| `3` | `Reject` | `MessageListener`存在仅打印报文的处理器，但管理消息进入`fromAdmin`，该方法没有转发给此处理器；不能认为已接通业务拒绝处理 |

心跳、测试请求、重传及序号恢复属于QuickFIX引擎会话层能力，不在本文已核对的业务消息清单中。本次未逐项验证引擎内部实现或实际线上报文，不能把协议通常支持的类型全部列为本项目已观测通信。

源码：[管理消息处理](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/client/CFixApi.java#L171)、[回执关联](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/client/CFixApi.java#L223)、[Reject处理器](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/lisener/MessageListener.java#L2535)。

## 6 HTTP接口

以下接口存在请求调用，均使用POST。业务含义依据请求字段及调用代码解释；不替代外部接口文档。

| 路径 | 用途 | 调用实现 |
| --- | --- | --- |
| `/qtrade-rest/v1/quote/secondary/broadcast/account` | 按QT账号发送报价或自然语言文本，请求含`ToQts` | `QuoteBroadcastHttpUtils`，104行附近 |
| `/qtrade-rest/v1/quote/secondary/broadcast/tag` | 按接收方标识发送文本，请求含`ToUserIDs`；不能仅凭路径名称理解为必然按标签广播 | `QuoteBroadcastHttpUtils`，135行附近 |
| `/open_api/user/v1/tag/list_by_qt` | 获取QT用户关联标签 | `QtTableClientInfoJob`，101行附近 |
| `/qtrade-rest/v1/common/secondary/tag` | 获取标签关联的客户或联系人数据 | `QtTableClientInfoJob`，181、210行附近 |

另外，`Consts`定义了 `/qtrade-rest/v1/quote/secondary/broadcast/result` 与 `/open_api/user/v1/tag/list_tag_users`，但本次未找到调用点，应标记为“仅定义，未确认使用”。

`QtTableClientInfoForDemo`也包含标签和联系人请求，其存在不代表正式任务运行了该演示类。正式任务使用的域名来自配置，不能用演示类中的固定地址推断部署目标。

源码：[QuoteBroadcastHttpUtils](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/http/QuoteBroadcastHttpUtils.java)、[QtTableClientInfoJob](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/job/QtTableClientInfoJob.java)、[接口常量](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/stream/Consts.java#L113)。

## 7 已发现的实现边界

1. **行情上行的首次发送与后续更新不一致。** `QtradeStream`在首次行情路径实际发送`S`；价格发生变化后的分支更新缓存、发布内部消息并构造FIX报文，但`sendQuote`被注释。不能据此宣称该路径持续向QT同步价格变化。
2. **撤销路径仍可实际发送。** `QtradeStream.methodA`使用`S`，设置`QuoteType=51`、`TransType=Z`，调用发送接口。因此，不能把上述问题概括为该模块完全不发送FIX。
3. **“主动Q一下”入口存在样例构造。** `QoneceHandler`虽然接收请求对象，但多个关键字段仍为固定值。迁移前应核实入口是否注册、是否投入使用及实际契约。
4. **部分发送调用被注释。** 例如`QtSubScriptHandler`、`QtQOnceRequireHandler`以及部分分类管理分支。仅找到构造器或调用文本，不足以认定功能启用。
5. **业务确认存在多个层次。** FIX接口回执、对手方单据响应、内部输入报价确认以及最终成交是不同事件，设计新系统时应分别建模。

源码：[首次发送及变价分支](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/stream/QtradeStream.java#L565)、[撤销上行](../../../sc-app-stc-qtradesvr/sc-app-stc-qtradesvr/src/main/java/sc/app/stc/qtradesvr/stream/QtradeStream.java#L661)。

## 8 与现有分析的关系

本文补充[旧系统实现与迁移参考](legacy-ficc-findings.zh-CN.md)中的QT接入部分，专门列出外部通信边界。报价模型字段、机构与用户映射、输入报价到正式报价的确认流程，仍以该主文档中的分析为参考。
