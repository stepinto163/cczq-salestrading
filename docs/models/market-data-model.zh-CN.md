# 行情模型与STC映射（讨论稿）

> 查询、整券行情订阅、条件面板及恢复消息见[FICC服务API规范（v2：报价与行情分离）](../ficc-api-spec.zh-CN.md)，其中STC字段映射已结合共享源码补充核对。

日期：2026-09-13。QuoteSvr已合并FICC行情服务职责，内部保留报价生命周期、价格簿和STC报文转换模块。本文定义模型边界，不新增撮合或成交执行要求。

## 1．模型与存储

| 模型 | 含义 | 存储依据 |
| --- | --- | --- |
| Quote | 原始输入、用户归属、报价生命周期及版本 | FICC报价业务表，变更历史另存QuoteHistory |
| MarketDataEntry | 一条报价对应的当前行情条目，承接输入及所需计算结果 | QuoteSvr内存衍生数据，可由报价状态及计算服务重建 |
| PriceBook | 同券MarketDataEntry的买卖侧集合 | QuoteSvr内存；可选恢复检查点，不是第二份价格业务事实 |
| MarketDataSnapshot | 一个债券及相应范围的行情条目列表 | 消息结构，兼容STC MarketDataSnapshotFullRefresh |
| PriceBookView / PriceLevel | 过滤后的结果及不同价格档 | 查询及输出概念，不要求建表 |

```mermaid
flowchart LR
    DB[("Quote / QuoteHistory")] -->|恢复| Quote["QuoteSvr报价模块"]
    Quote --> Entry["MarketDataEntry<br/>对应GNoMDEntries"]
    Entry --> Book["PriceBook<br/>securityId → 买侧／卖侧集合"]
    Book --> View["先权限及条件过滤<br/>再最优／N档／完整结果"]
    View --> Msg["MarketDataSnapshotFullRefresh<br/>NoMDEntries逐条保留报价身份"]
```

一条Quote为单方向报价，至多对应一个当前MarketDataEntry。确认生效后入簿，修改更新条目，Refer/Delete移出当前簿，Unrefer按同一quoteId重新入簿。退出行情不删除报价审计历史。entryId为稳定标识，可采用带来源命名空间的quoteId映射；不另造独立行情生命周期。

行情条目包含quoteId、securityId、quoteRevision、side、price、yieldType、yield、exerciseYield、cleanPrice、fullPrice、quantity、quantityUnit、settlType、ownerUserId、updateTime；机构、联系人、渠道及业务标记从关联Quote补充输出。缺失计算值可空，计算细节仍待后续确认，不能用0或展示占位符代替业务值。

price为首版入簿比较收益率：普通债到期收益率、含权债行权收益率；Quote.price仍保留用户输入。updateTime沿用报价更新时间，计算结果刷新不改写报价时间或revision；行情事件的处理位置单独管理，不能只按quoteRevision丢弃计算引起的条目变化。

## 2．STC兼容边界

对外行情优先复用共享sc-app-stc的MarketDataSnapshotFullRefresh/GNoMDEntries结构。下表是FICC逻辑字段到STC的集中适配约定，独立topic和线格式见API评审稿，未定项在该稿列明；内部模型不要求采用STC大写字段名。

| FICC逻辑字段 | STC行情字段 | 约定 |
| --- | --- | --- |
| securityId及债券资料 | SecurityID、Symbol、MarketIndicator | 消息头描述债券 |
| entryId，关联quoteId | MDEntryID | 稳定映射；来源命名空间避免与其他STC渠道碰撞 |
| side | MDEntryType | FICC买入1→行情Bid 0；卖出2→行情Ofr 1 |
| price及其口径 | MDEntryPx、PriceType、ExerciseFlag | 明确标识输出值的含义，不能直接复制Quote.quoteType代码 |
| quantity、quantityUnit | MDEntrySize及双方约定单位 | 显式转换单位；当前FICC不引入InnerSvr的成交量扣减逻辑 |
| yield、cleanPrice、fullPrice、exerciseYield | Yield、NetPrice等共享模型对应字段 | 按可用字段及计算口径适配，完整字段落点后续核对，不擅自占用扩展字段 |
| settlType | SettlType | 沿用已定业务口径，通过适配器处理组合/远期字典 |
| ownerUserId及对手资料 | 我方归属、InstitutionID、TraderID等 | 用户归属与客盘机构/联系人分开映射，不直接等同 |
| updateTime | MDEntryDate、MDEntryTime等 | 明确为报价更新时间，不能继承InnerSvr使用createtime的含义 |

STC InnerSvr的PriceType为1净价/2全价/3收益率，FICC Quote.quoteType为1收益率/2净价/3全价。ExerciseFlag为0行权/1或空到期，FICC yieldType为1到期/2行权。应集中转换，不能直接透传同名代码。

STC原实现MDPriceLevel常为逐条顺序号；FICC depth=N仍按N个不同价格档，保留选中档的所有报价。兼容输出中的档号编码需显式约定，不能直接照搬marketNum按条数截断。首版Bid收益率升序、Ofr降序，同价updateTime降序的规则继续生效。

QuoteReport用于报价操作和生命周期；行情消息用于价格簿显示及NotificationSvr的我的最优报价、对价提醒。两类消息来自同一QuoteSvr，但分别使用ficc.quote.*与ficc.price.*接口、DTO及频道，不以dataset合并。Quote订阅只发事件，初始Quote集合通过显式查询获得；Price订阅提供按券快照。二者可关联同一quoteId，但不共用客户端流sequence。行情中心全部/最优分别使用price.view.open的FULL/BEST模式；当前报价管理使用quote.view.open。接续协议见API评审稿，分块等缺口未完成。

依据：[InnerSvr核查](stc-innersvr-price-model-review.zh-CN.md)、[核心ER](core-data-model-er.zh-CN.md)、[架构](../ficc-sales-trading-architecture.zh-CN.md)。
