# FICC服务API规范（实现对照与评审稿）

日期：2026-09-17。本文对照当前仓库的服务注册、请求处理及客户端调用修订；“已注册”表示已有处理入口，不代表全部产品需求或所有部署环境均已验收。

本次更新：补齐CRM机构／联系人搜索、确认时关联ID、内部同步接口；说明viewId、streamId与pushTopic关系；修正Handler名称、会话鉴权及当前限额。Quote、Price、Notification继续使用独立模型与推送频道。

2026-09-17已实现：Quote、Price、Notification三类view统一采用按页查询替代累计extend，推送改为upsertIds／removeIds，由客户端排列第一页。H16/H17、H26/H27、H51/H52、S01/S02/S08及第10节已同步服务、C#客户端、Web客户端和API调试目录。旧view.extend不再注册，旧limit请求及place消费者需同步升级。三类view共享交互规则，模型、topic、缓存和sequence保持独立。体验说明见[产品说明](quote-board-browsing-ux.zh-CN.md)。

实现边界：本轮客户端只保留当前一页，提供滚动换批及上一批／下一批／返回最新入口，不累积全部记录。服务保留过滤匹配指纹以识别进入和退出，仍会重新计算匹配数据；本轮没有完成事件定向索引优化，也没有进行50,000条压力测试，不据此承诺查询延迟。

当前运行服务为QuoteSvr与NotificationSvr。H36—H40、H48—H49及S06—S07是待实现设计；当前搜索在业务服务内完成，不能向部署环境直接调用这些规划中的topic。接口目录明确标出状态，后文保留其设计供评审。

## 1．范围与依据

本稿以[当前架构](ficc-sales-trading-architecture.zh-CN.md)、[核心ER](models/core-data-model-er.zh-CN.md)、[行情模型](models/market-data-model.zh-CN.md)为设计依据，独立列出接口、模型和消息规则。旧API及看板协议不作为补充契约；不要从旧稿复制未在本文列出的topic。

| 服务／组件 | 本版接口范围 |
| --- | --- |
| QuoteSvr | 报价生命周期、查询、历史、来源接入、价格簿、用户分组、报价及行情订阅 |
| NotificationSvr | 四类提醒、开关、已读状态、通知查询、通知筛选及订阅 |
| SearchSvr | 内部DSL搜索、属性读取、处理位置；消费源事件，不维护用户订阅 |
| ParseSvr | 自然语言识别并返回候选Quote，不自动落正式报价 |
| 基础资料层 | 统一资料查询与变化通知；可由现有BaseSvr适配模块承载，不要求新增独立进程 |
| qtrade-intake / ideal-intake | 外部协议转为QuoteSvr统一接入请求，无前端API |
| LoginSvr / CalSvr / STC MarketSvr | 复用既有服务，不重写其对外契约；接入约定见后文 |
| RecommendSvr | 按当前架构留待后续，无本版handler或订阅 |

QuoteSvr内部维护报价、行情条目和价格簿；不再单独部署FICC MarketSvr。基础资料与CRM请求由SERVER.QuoteSvr承接，再调用对应适配器。SearchSvr、ParseSvr及供应商完整接入仍是后续设计，不能将其与已实现入口混淆。

## 2．STC／DPS接入方式

### 2.1 STC的请求、事件订阅与行情订阅

同一进程可以同时处理订单／报价和生成价格簿，但模型与通信路径分别定义。STC InnerSvr的ExecutionReport发布在stc.inner.o.*，行情MarketDataSnapshotFullRefresh发布在stc.inner.p.*；MarketHandler.snapshot仅处理行情初始image。FICC不因共享过滤算法而合并这两类API。

| 通信方式 | 实现与调用 | 本稿用途 |
| --- | --- | --- |
| 请求/应答 | ContentHandler.handle；BaseApi.requestSync | 报价操作、列表查询、登记条件、显式初始化与补事件 |
| 普通订阅/发布 | BaseApi.subscribe；业务Publisher发布 | QuoteReport、QuoteEvent及通知事件；不调用业务snapshot，不自动返回已有报价集合 |
| 带image订阅/发布 | BaseApi.subscribeWithImage；ContentHandler.snapshot | Price频道；先返回当前市场快照，再按券发送完整刷新 |

```java
BaseApi.requestSync("SERVER.QuoteSvr", "ficc.quote.get",
    requestJson, QuoteGetReply.class, false, false, true);
// quoteListener接收JSON字符串，交给独立QuoteStreamBatch DTO解析。
BaseApi.subscribe("SERVER.QuoteSvr", quotePushTopic, quoteListener);
BaseApi.subscribeWithImage("SERVER.QuoteSvr", "QuoteSvr", pricePushTopic,
    priceListener, PriceStreamBatch.class, false);
BaseApi.unSubscribeWithImage("SERVER.QuoteSvr", "QuoteSvr", pricePushTopic, priceListener);
```

示例为共享Java库的真实调用形状；C#客户端经GW的DPS封装须对应普通subscribe和subscribeWithImage两种能力。报价Publisher不是ContentHandler.snapshot的别名，不给纯发布topic虚构snapshot Handler。鉴权应在GW/消息订阅入口完成，不能假设普通subscribe会执行ContentHandler。

本次源码中BaseApi.unSubscribe的底层调用被注释，带Class的unSubscribeWithImage也为空；前者需在客户端适配层补齐实际退订，后者使用四参数重载及原typed listener。不能把方法调用成功当成已退订；引用的CedaClientWrapper实现提供底层退订路径。本文不修改共享库源码。

复杂条件使用业务view.open登记，不将DSL拼入topic。Quote和Price分别stream.open、分别订阅，拥有独立streamId、sequence和客户端缓存。服务仍为QuoteSvr／SERVER.QuoteSvr。框架SIGNAL_SRL等控制字段不进入业务DTO；只有行情snapshot按STC方式将序列化内容放在返回Map的topic键下。

### 2.2 消息封装及身份

```json
{"requestId":"R-001","quoteId":"Q-001"}
```

```json
{"Code":"0000","Msg":"成功","requestId":"R-001","Data":{"quoteId":"Q-001"}}
```

`Code`、`Msg`沿用STC写法；`Data`是本规范统一结果包装，业务字段使用lowerCamelCase，STC行情报文保持原大写字段。`requestId`为业务关联及写入幂等键，DPS自身请求关联仍由其封装处理。本文不用额外batchId。

用户身份从GW/DPS上下文中的SID解析，再通过STC身份适配取得userid。客户端请求不能自行声明operatorUserId；普通创建的ownerUserId取当前用户。登录账号和密码由用户输入，服务间同步不配置业务用户密码。STC的DefaultUser快捷登录可能返回成功但没有有效SID，不能用于这些业务接口；应使用已注册业务账号。内部同步的独立信任边界见H55／H56。

当前实现中，stream.open返回的sessionId及私有topic后缀使用当前GW/DPS登录SID；客户端直接使用返回的pushTopic，不自行拼接。GW只允许订阅当前SID的quote.report、price.report、notification.report三个精确topic，拒绝他人topic、通配订阅和内部频道。DPS底层支持通配符，不等于当前GW授权允许使用。重新登录后应按新SID重新建立订阅。

| Code | error.type | 含义及调用方行为 |
| --- | --- | --- |
| 0000 | 无 | 请求成功；批量操作还需逐项查看结果 |
| 1001 | VALIDATION_FAILED | 字段、格式或状态组合错误，按error.field修正 |
| 1002 | NOT_FOUND | 记录不存在或不可见，不泄露权限外记录 |
| 1003 | FORBIDDEN | 会话缺失、无效、过期，或无操作权限 |
| 1004 | REVISION_CONFLICT | 重新读取后由用户决定，不能盲目覆盖 |
| 1005 | INVALID_STATE | 当前生命周期不允许该动作 |
| 1006 | IDEMPOTENCY_CONFLICT | 同requestId携带不同内容 |
| 1007 | RESET_REQUIRED | 游标、流位置或视图失效，重新取快照 |
| 1008 | RESOURCE_LIMIT | 请求超限，显式缩小范围 |
| 1009 | NOT_READY | 索引、资料或价格簿尚未就绪，可稍后重试 |
| 1010 | UNSUPPORTED | 本版未支持的报价类型、清算等 |
| 1099 | INTERNAL_ERROR | 服务内部错误；当前回复不提供traceId，排查时结合requestId、topic及服务日志 |

### 2.3 写入约束

1. 每个写入携带requestId；去重范围为“服务＋认证主体＋topic＋requestId”。重试同内容返回同一结果，不重复报价或审计；同键不同内容报1006。成功记录与幂等结果在业务事务内保存。stream/view等临时连接操作按领域连接幂等，不沿用业务写入跨连接的持久化重试承诺。
2. 修改已有Quote必须带expectedRevision。它只校验源报价版本；成功后revision加1。新建revision为1；Refer/Unrefer保留quoteId。
3. 批量使用quotes数组，每条独立事务，顺序对应results；顶层0000仅表示批量请求完成。单批不允许重复quoteId/clientQuoteId。失败项不更新版本、不发布成功QuoteReport。
4. update/confirm未传字段保留；null只清除可空字段；false明确关闭标记。不接受客户端修改status、revision和审计时间。
5. 一条操作的Quote、QuoteHistory及待发布事件原子提交；提交后推进本地价格簿。内存更新或网络发布不代替持久化成功。回复可能先于推送到达，也可能晚于推送。
6. 业务超时不是操作失败证明：使用原requestId重试。幂等数据保留期必须覆盖约定重试窗口，配置窗口外不能承诺安全重试。

### 2.4 viewId、streamId与pushTopic

| 字段 | 谁产生 | 作用 |
| --- | --- | --- |
| viewId | 客户端 | 标识一个面板及其过滤条件；三类view均用各自领域的open、query、close定位面板 |
| streamId | 服务端 | 标识当前领域推送流；结合sequence检查重复、缺号及流是否已重建 |
| pushTopic | 服务端返回 | DPS实际订阅地址；客户端向该地址subscribe接收消息 |

同一登录会话下，一个Quote流可以包含多个Quote view，它们共用一个pushTopic和一套sequence。一个view不需要单独创建一个DPS topic。Quote、Price、Notification各有自己的流和topic，不共享业务模型。

```text
当前登录会话 SID-001
├─ pushTopic: ficc.quote.report.SID-001
│  └─ streamId: QE-001
│     ├─ viewId: V-MINE       我的报价
│     └─ viewId: V-BANK       银行类发行人报价
└─ pushTopic: ficc.price.report.SID-001
   └─ streamId: PE-001
      └─ viewId: PV-BEST      最优行情
```

调用示例：先quote.stream.open取得streamId和pushTopic，订阅pushTopic，然后分别quote.view.open两次，传V-MINE和V-BANK及各自条件。Quote新设计中，views指明各面板应插入或更新、移除哪些报价，不发送行位置。同一批中被多个面板引用的报价内容只需发送一份。客户端缓存报价对象，各面板维护自己的显示列表。

同一有效流内重复stream.open返回现有流，不自动建立新view。更换过滤条件时关闭旧view，并以新viewId打开；关闭的viewId在当前流内不复用。重新连接／服务重启后重新调用stream.open，检查返回streamId并按恢复流程重建视图，不假定旧流仍有效。HEARTBEAT也参与流序号处理；客户端关闭心跳显示或日志仅影响可见日志，不应停止接收和处理心跳。

## 3．统一模型与字典

各领域DTO独立定义，不用Object或dataset判别的通用消息承载所有业务。可以共用Code/Msg、基础条件和序列辅助类型，但请求校验与业务载荷分开。

| 领域 | 请求/应答DTO示例 | 发布DTO | 业务载荷 |
| --- | --- | --- | --- |
| Quote | QuoteViewOpenRequest / QuoteViewReply | QuoteStreamBatch、QuoteEventBatch | quotes / reports；QuoteView成员为quoteId |
| Price | PriceViewOpenRequest / PriceViewReply | PriceStreamBatch、PriceFeedBatch | books / prices，内部为PriceResult及STC行情模型；PriceView成员为securityId |
| Notification | NotificationViewOpenRequest / NotificationViewReply | NotificationStreamBatch | notifications / preference；成员为notificationId |

### 3.1 Quote及生命周期

| 字段组 | 字段及约束 |
| --- | --- |
| 标识 | quoteId服务端生成；clientQuoteId客户端创建关联，用户内唯一；sourceMessageId可空；source为来源信息 |
| 身份 | ownerUserId、securityId、institutionId、contactId；机构／联系人可空，确认时债券必须确定 |
| 对手名称 | institutionName、contactName；待确认可仅有名称，确认前由用户从CRM手动选定对应ID |
| 状态 | revision为十进制整数字符串；status取下表 |
| 报价 | side、quoteType、yieldType、price、quantity、quantityUnit、currency、tradeDate、settlType |
| 标记 | urgent、recommended、whole、oco、packing为boolean，默认false；oco与packing互斥 |
| 说明 | remark可空；reason是操作原因，单独写入审计，不是remark别名 |
| 审计 | createdBy、updatedBy、confirmedBy、createTime、updateTime、quoteTime、confirmTime均由服务端维护 |

| 当前状态 | 允许动作 | 结果 |
| --- | --- | --- |
| 无 | create／clone／首次来源接入 | PENDING_CONFIRM |
| PENDING_CONFIRM | update、confirm、delete | 原状态、ACTIVE、DELETED |
| ACTIVE | update、refer | ACTIVE、REFERRED |
| REFERRED | unrefer、delete | ACTIVE、DELETED |
| DELETED | get、history.query | 只读 |

clone是复制输入生成新Quote，不是恢复旧ID。confirm可在同一条请求中提交修正字段；校验合并后的结果再一次性确认。重复requestId幂等；新的requestId重复执行不合法状态动作报1005。

| 字典 | 值 |
| --- | --- |
| side | 1买入，2卖出；报价方视角 |
| quoteType | 1收益率，2净价，3全价 |
| yieldType | 1到期，2行权；quoteType不为1时必须为null |
| quantityUnit / currency | WAN（万单位面值）／CNY |
| settlType | 按 STC SettlTypeEnum：1=T+0、2=T+1、3=T+2、4=T+3、5=T+4、9=T+5、10=T+6，之后至16=T+12；非远期为1/2，远期为其余上述代码。明+0／明+1尚无独立输入语义 |
| source.type（本稿建议） | MANUAL、QT、IDEAL；source含externalQuoteId、externalRevision和sourceTime |

价格和数量使用十进制字符串；收益率为百分数且输入最多4位小数，超过直接拒绝。时间用带时区ISO 8601，日期YYYY-MM-DD。当前确认／恢复有效报价时，securityId、side、quoteType、price、quantity、tradeDate、settlType必填，收益率报价还需yieldType；quantity必须大于0。CalSvr换算及其他精度规则仍待完善。未知正式报价类型不能静默转成收益率。

quoteTime在首次确认前为null，确认、ACTIVE报价修改及unrefer时更新；updateTime反映最近一次事实变更，createTime永不改写。contactId与institutionId同时提供时必须属于同一机构；ownerUserId是我方STC用户，不能用对手联系人ID代替。

QuoteHistory为eventId、quoteId、revision、action、operatorUserId、operationTime、before、after、reason；before/after是完整报价事实，不混入当前评级或估值。create的before=null。系统动作使用独立actorService字段且operatorUserId=null，不伪造STC用户；这是待评审的审计模型补充。

### 3.2 行情消息

每券PriceBook保存该券全部有效MarketDataEntry的买卖侧集合，每条行情条目关联一条Quote。报价入簿、移出及修改由QuoteSvr内部完成；价格簿处理位置与quoteRevision不同。首版按收益率比较，含权债用行权收益率；Bid升序、Ofr降序，同价按Quote.updateTime降序和quoteId升序。

行情返回PriceResult：`securityId、bookRevision、updateTime、marketData、quoteRefs`；updateTime作为面板列表排序依据，定义见3.3。marketData为共享MarketDataSnapshotFullRefresh；quoteRefs按MDEntryID关联quoteId及quoteRevision，不塞入STC Info1等扩展字段。STC直连行情topic只发送marketData；内部S09及独立price.query也可携带该新增时间字段，但不参与面板分页。

| 行情字段 | 本稿约定 |
| --- | --- |
| MarketIndicator | 示例沿用InnerSvr配置的市场代码4，不以securityId的IB后缀代替代码；渠道编码见R6 |
| SecurityID / NoMDEntries | 债券ID / 所选范围内完整条目数组 |
| MDEntryID | 建议FICC:quoteId，保持恢复前后稳定 |
| MDEntryType | 0=Bid、1=Ofr，由Quote.side映射 |
| MDEntryPx | 比较用收益率；PriceType=3，ExerciseFlag=0行权／1到期 |
| Yield / YieldToExecution | 到期收益率／行权收益率，均为百分数 |
| NetPrice / DirtyPrice | 净价／全价，缺失省略，不填0 |
| MDEntrySize | 本稿FICC渠道约定为WAN面值；接入其他STC渠道必须显式核对及转换单位 |
| UserID | FICC渠道内映射ownerUserId；客盘InstitutionID/TraderID另映射 |
| MDEntryDate / MDEntryTime | 报价updateTime在业务时区拆为yyyyMMdd和HH:mm:ss.SSS |
| MDPriceLevel | STC兼容输出仍用每侧条目顺序号；不同价格档先在服务端算完，再按depth截取全部成员 |

mode=BEST取各侧第一价格档；DEPTH需depth正整数；FULL不按档数截断。先过滤再分档，不按quote条数截断同价档。无清算条件时混合T+0/T+1/远期；具体清算代码见第3节字典及运行时dictionary.query。

最优排行所需计算值尚未就绪时，price.query/view.open返回1009，不默默使用错误排序。对价提醒需要无价格依赖的同券反向报价关系，QuoteSvr内部事件流仍提供完整有效Quote成员；不能因价格计算未完成漏掉它们。计算异常处理的最终业务口径仍列入评审项。

### 3.3 查询、筛选与滚动

QuoteFilter与PriceFilter是独立DTO，共享已定义的基础条件类型与过滤库；不存在带dataset判别的通用前端请求。客户端filter使用结构化业务条件：`market、conditions、watchlistIds、filterGroupIds`。conditions每项为field/operator/value；同字段OR、跨字段AND；不同已选filterGroup取并集，再与临时条件及关注债券成员范围相交。空选中列表表示未启用该类限制；显式选中的空关注分组匹配空集。FilterSubgroup仅为编辑器组织结构，查询前拍平。

quote.query、quote.history.search等一次性查询仍使用filter、limit及后续cursor。cursor绑定用户权限、查询条件、数据及基础资料快照；过期1007，不拼接两个时点结果。历史日期范围使用[fromDate,toDate]，不建立订阅。返回nextCursor=null表示结束。它们与下述绑定实时view的quote.view.query不同。

QuoteView新设计：open登记过滤条件并返回第一页，view.query按pageNumber、pageSize返回指定的一页，不创建新订阅、不返回累计前缀。后端查询按`updateTime DESC, quoteId ASC`排序；客户端用相同规则维护第一页，更新后插入或替换并裁剪至pageSize。第一页已加载报价因删除或退出条件被移除时，合并请求一次第一页补位。后续页只更新已有行及移除，不实时插入、重排或补位；滚动由前端触发下一页查询，不必显示页码。

后端维护筛选订阅并推送匹配变化，不持续维护客户端行位置或替换行。普通修改须更新updateTime且不回写为更早值；时间纠正、基础资料批量变化等无法用本地少量记录判断边界的情况重新查询。不同页查询对应各自当前时点，跨页可能重复或跳过记录，不作为完整导出遍历。

PriceView和NotificationView采用相同的open／分页query、upsertIds／removeIds、第一页本地排序及补位、后续页稳定阅读规则。每个view查询默认pageSize=200，上限1000，pageNumber从1开始；查询不创建新订阅，前端对同一view串行请求并按requestId关联。totalCount始终表示完整过滤结果数，不是当前页条数。

| 领域 | 每行标识 | 数据数组 | 查询及第一页排序 |
| --- | --- | --- | --- |
| Quote | quoteId | quotes | updateTime降序、quoteId升序 |
| Price | securityId（缓存键为viewId＋securityId） | books | books.updateTime降序、securityId升序 |
| Notification | notificationId | notifications | updateTime降序、notificationId升序 |

PriceResult顶层`updateTime`表示该view过滤后参与价格簿构建的有效报价中最大的updateTime；BEST/DEPTH取档前计算，查询及推送必须给出相同依据，不能从截断后的NoMDEntries推导。删除最新报价可能使该值下降；第一页收到已加载行排序时间下降时重新查询，因为本地没有下一候选。空簿无此值时为null，并以removeIds移出列表。券内档位及MDPriceLevel仍由后端按价格簿规则生成，不属于前端列表排序。

### 3.4 分组、通知及资料

Watchlist包含watchlistId、name、market、scope、displayOrder、securities[]；成员为securityId、displayOrder。FilterGroup包含filterGroupId、name、market、scope、displayOrder、subgroups[]；子组含subgroupId和conditions[]，条件为conditionId、field、operator、value、displayOrder。保存为完整替换，服务端生成缺省子ID并丢弃空子组；调用者只能改自己的分组。

分组及NotificationPreference已使用revision，save携带expectedRevision以检测多端修改冲突。通知筛选组由NotificationSvr管理，scope=NOTIFICATION；行情及历史分组由QuoteSvr管理，scope=MARKET/HISTORY。分组所有者userId由服务端写入。

Notification包含notificationId、userId、securityId、reasons[]、content、quoteIds[]、readTime、createTime、updateTime。reasons项为type、groupIds、text；type取WATCHLIST、FILTER_GROUP、COUNTER_QUOTE、MY_BEST。非对价同券原因可合并，对价独立；通知拥有唯一用户，不能复用其他用户的已读状态。

个人偏好为watchlistEnabled、filterGroupEnabled、counterQuoteEnabled、myBestEnabled四个开关。关注/筛选提醒匹配用户该市场的全部行情分组，不受面板当前点选影响。我的最优用同侧最优档（含并列）；对价只检查同券、相反方向、本人与他人，不要求价格交叉或清算相同。生成/退出通知由事件驱动，客户端没有notification.create/delete接口。

资料返回稳定FICC模型Security、Issuer、Institution、Contact、DailyValuation，使用统一ID。当前评级与最新可用日估值用于历史报价筛选及显示，原Quote审计不随资料变化改写。未定稿的供应商字段在适配层补齐，不能把供应商整表DTO变成对前端契约。

## 4．接口目录

### 4.0 请求Handler与发布频道

| 编号 | 服务 | topic | Handler | 用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| H01 | QuoteSvr | `ficc.quote.create` | `QuoteCreateHandler` | 创建／批量创建报价 | 已注册 |
| H02 | QuoteSvr | `ficc.quote.confirm` | `QuoteConfirmHandler` | 确认报价并允许同时修正 | 已注册 |
| H03 | QuoteSvr | `ficc.quote.update` | `QuoteUpdateHandler` | 修改报价 | 已注册 |
| H04 | QuoteSvr | `ficc.quote.refer` | `QuoteReferHandler` | 撤销报价 | 已注册 |
| H05 | QuoteSvr | `ficc.quote.unrefer` | `QuoteUnreferHandler` | 恢复报价 | 已注册 |
| H06 | QuoteSvr | `ficc.quote.delete` | `QuoteDeleteHandler` | 删除报价 | 已注册 |
| H07 | QuoteSvr | `ficc.quote.clone` | `QuoteCloneHandler` | 克隆报价 | 已注册 |
| H08 | QuoteSvr | `ficc.quote.get` | `QuoteGetHandler` | 读取单条报价 | 已注册 |
| H09 | QuoteSvr | `ficc.quote.query` | `QuoteQueryHandler` | 当前报价搜索 | 已注册 |
| H10 | QuoteSvr | `ficc.quote.history.query` | `QuoteHistoryQueryHandler` | 单条报价变更历史 | 已注册 |
| H11 | QuoteSvr | `ficc.quote.history.search` | `QuoteHistorySearchHandler` | 跨日期历史报价搜索 | 已注册 |
| H12 | QuoteSvr | `ficc.quote.sourceMessage.query` | `QuoteSourceMessageQueryHandler` | 来源原文及候选关联查询 | 已注册 |
| H13 | QuoteSvr | `ficc.price.query` | `PriceQueryHandler` | 单券／多券行情查询 | 已注册 |
| H14 | QuoteSvr | `ficc.quote.dictionary.query` | `QuoteDictionaryQueryHandler` | 业务字典与可用筛选字段 | 已注册 |
| H15 | QuoteSvr | `ficc.quote.stream.open` | `QuoteStreamOpenHandler` | 取得当前连接的私有推送topic | 已注册 |
| H16 | QuoteSvr | `ficc.quote.view.open` | `QuoteViewOpenHandler` | 登记条件并查询第一页 | 已注册 |
| H17 | QuoteSvr | `ficc.quote.view.query` | `QuoteViewQueryHandler` | 查询指定页／第一页补位 | 已注册 |
| H18 | QuoteSvr | `ficc.quote.view.close` | `QuoteViewCloseHandler` | 关闭实时面板 | 已注册 |
| H19 | QuoteSvr | `ficc.watchlist.query` | `WatchlistQueryHandler` | 读取个人关注分组 | 已注册 |
| H20 | QuoteSvr | `ficc.watchlist.save` | `WatchlistSaveHandler` | 保存关注分组及全部成员 | 已注册 |
| H21 | QuoteSvr | `ficc.watchlist.delete` | `WatchlistDeleteHandler` | 删除关注分组 | 已注册 |
| H22 | QuoteSvr | `ficc.filterGroup.query` | `FilterGroupQueryHandler` | 读取个人筛选分组 | 已注册 |
| H23 | QuoteSvr | `ficc.filterGroup.save` | `FilterGroupSaveHandler` | 保存筛选组及全部条件 | 已注册 |
| H24 | QuoteSvr | `ficc.filterGroup.delete` | `FilterGroupDeleteHandler` | 删除筛选分组 | 已注册 |
| H25 | NotificationSvr | `ficc.notification.stream.open` | `NotificationStreamOpenHandler` | 取得当前连接的私有推送topic | 已注册 |
| H26 | NotificationSvr | `ficc.notification.view.open` | `NotificationViewOpenHandler` | 登记条件并查询第一页 | 已注册 |
| H27 | NotificationSvr | `ficc.notification.view.query` | `NotificationViewQueryHandler` | 查询指定页／第一页补位 | 已注册 |
| H28 | NotificationSvr | `ficc.notification.view.close` | `NotificationViewCloseHandler` | 关闭实时面板 | 已注册 |
| H29 | NotificationSvr | `ficc.notification.query` | `NotificationQueryHandler` | 读取当前通知 | 已注册 |
| H30 | NotificationSvr | `ficc.notification.read` | `NotificationReadHandler` | 标记已读／未读 | 已注册 |
| H31 | NotificationSvr | `ficc.notification.preference.get` | `NotificationPreferenceGetHandler` | 读取个人提醒开关 | 已注册 |
| H32 | NotificationSvr | `ficc.notification.preference.save` | `NotificationPreferenceSaveHandler` | 保存个人提醒开关 | 已注册 |
| H33 | NotificationSvr | `ficc.notification.filterGroup.query` | `NotificationFilterGroupQueryHandler` | 读取个人筛选分组 | 已注册 |
| H34 | NotificationSvr | `ficc.notification.filterGroup.save` | `NotificationFilterGroupSaveHandler` | 保存筛选组及全部条件 | 已注册 |
| H35 | NotificationSvr | `ficc.notification.filterGroup.delete` | `NotificationFilterGroupDeleteHandler` | 删除筛选分组 | 已注册 |
| H36 | SearchSvr | `ficc.search.query` | `SearchQueryHandler` | 内部DSL查询 | 待实现 |
| H37 | SearchSvr | `ficc.search.attributes.get` | `SearchAttributesGetHandler` | 取得完整搜索属性 | 待实现 |
| H38 | SearchSvr | `ficc.search.fields.query` | `SearchFieldsQueryHandler` | 读取模型的筛选字段目录 | 待实现 |
| H39 | SearchSvr | `ficc.search.status.get` | `SearchStatusGetHandler` | 查询索引及持久化处理位置 | 待实现 |
| H40 | ParseSvr | `ficc.quote.recognize` | `QuoteRecognizeHandler` | 自然语言识别为候选报价 | 待实现 |
| H41 | QuoteSvr | `ficc.quote.intake.apply` | `QuoteIntakeApplyHandler` | QT／IDEAL统一来源接入 | 已注册 |
| H42 | QuoteSvr | `ficc.base.get` | `BaseGetHandler` | 按ID批量读取统一基础资料 | 已注册 |
| H43 | QuoteSvr | `ficc.base.query` | `BaseQueryHandler` | 基础资料输入联想与查找 | 已注册 |
| H44 | QuoteSvr | `ficc.quote.state.query` | `QuoteStateQueryHandler` | 显式读取内部初始化记录 | 已注册 |
| H45 | QuoteSvr | `ficc.quote.event.query` | `QuoteEventQueryHandler` | 按位置补取内部事件 | 已注册 |
| H46 | NotificationSvr | `ficc.notification.state.query` | `NotificationStateQueryHandler` | 显式读取内部初始化记录 | 已注册 |
| H47 | NotificationSvr | `ficc.notification.event.query` | `NotificationEventQueryHandler` | 按位置补取内部事件 | 已注册 |
| H48 | BaseSvr适配模块 | `ficc.base.state.query` | `StateQueryHandler` | 显式读取内部初始化记录 | 待实现 |
| H49 | BaseSvr适配模块 | `ficc.base.event.query` | `EventReplayQueryHandler` | 按位置补取内部事件 | 待实现 |
| H50 | QuoteSvr | `ficc.price.stream.open` | `PriceStreamOpenHandler` | 取得独立的行情推送topic | 已注册 |
| H51 | QuoteSvr | `ficc.price.view.open` | `PriceViewOpenHandler` | 登记行情条件并取得第一页价格簿 | 已注册 |
| H52 | QuoteSvr | `ficc.price.view.query` | `PriceViewQueryHandler` | 查询指定行情页／第一页补位 | 已注册 |
| H53 | QuoteSvr | `ficc.price.view.close` | `PriceViewCloseHandler` | 关闭行情view | 已注册 |
| H54 | QuoteSvr | `ficc.price.dictionary.query` | `PriceDictionaryQueryHandler` | 读取行情字典与可用筛选字段 | 已注册 |

| H55 | QuoteSvr | `ficc.quote.internal.state.query` | `QuoteInternalStateQueryHandler` | 服务间报价状态初始化／恢复 | 已注册，仅内部 |
| H56 | QuoteSvr | `ficc.price.internal.snapshot.query` | `PriceInternalSnapshotQueryHandler` | 服务间完整行情初始化／恢复 | 已注册，仅内部 |

| 编号 | 服务 | 订阅topic | 模式及组件 |
| --- | --- | --- | --- |
| S01 | QuoteSvr | `ficc.quote.report.<sessionId>` | 普通subscribe；QuoteReportPublisher；upsertIds协议已实现 |
| S02 | NotificationSvr | `ficc.notification.report.<sessionId>` | 普通subscribe；NotificationReportPublisher；upsertIds协议已实现 |
| S03 | QuoteSvr | `ficc.price.book.<securityId>` | subscribeWithImage；PriceBookSnapshotHandler |
| S04 | QuoteSvr | `ficc.quote.events` | 普通subscribe；QuoteEventPublisher |
| S05 | NotificationSvr | `ficc.notification.events` | 普通subscribe；NotificationEventPublisher |
| S06 | BaseSvr适配模块 | `ficc.base.events` | 普通subscribe；ReferenceEventPublisher |
| S07 | SearchSvr | `ficc.search.attributes.events` | 普通subscribe；SearchAttributesPublisher |
| S08 | QuoteSvr | `ficc.price.report.<sessionId>` | subscribeWithImage；PriceReportSnapshotHandler；分页及upsertIds协议已实现 |
| S09 | QuoteSvr | `ficc.price.snapshots` | subscribeWithImage；PriceFeedSnapshotHandler |

只有Price频道带业务image；其余频道是纯发布。Topic的ACL、路由及Publisher绑定仍需配置，但不为纯发布topic注册不存在的ContentHandler.snapshot。

### 4.1 行情中心与报价管理调用图

| 页面／动作 | 前端调用 | 模型与接收频道 |
| --- | --- | --- |
| 行情中心：信用全部／利率全部 | price.stream.open → DPS带image订阅 → price.view.open，market=CREDIT/RATE、mode=FULL | PriceViewReply／PriceStreamBatch；ficc.price.report.<sessionId> |
| 行情中心：信用最优／利率最优 | 同上，mode=BEST | 同券Bid/Ofr价格簿，保留最优档全部并列报价 |
| 行情中心：滚动／补位／改变条件／离开 | price.view.query／第一页query补位／close后新ID open／price.view.close | 只操作PriceView，pageSize按债券行计数 |
| 报价管理：当前报价 | quote.stream.open → DPS普通订阅 → quote.view.open | QuoteViewReply／QuoteStreamBatch；ficc.quote.report.<sessionId> |
| 报价管理：滚动／第一页补位 | quote.view.query(viewId, pageNumber, pageSize) | 单页结果；复用原view和Quote订阅 |
| 报价管理：历史报价 | quote.history.search，后续cursor续取 | 一次性查询，无订阅 |
| 行情中心：字段、关注债券、筛选组 | price.dictionary.query、watchlist.query、filterGroup.query | 分组保存仍由QuoteSvr管理；不因此混合Quote/Price数据通道 |
| 行情中心：债券详情／报价操作 | base.get/query；quote.get/create/confirm/update/refer | 报价操作应答及报告走Quote协议；行情变化另外走Price协议 |
| 行情中心：报价提醒 | notification.stream.open → 普通订阅 → notification.view.open；read及preference.get/save | 独立Notification频道 |
| 提醒列表：滚动／第一页补位 | notification.view.query(viewId, pageNumber, pageSize) | 单页notifications，复用原订阅 |

正文topic均带ficc前缀。行情中心只浏览行情时无需开Quote流；从行情中心发起报价操作时，请求使用Quote接口，若需实时操作报告再建立Quote订阅。报价写入不以Quote订阅存在为前提。前端不直接调用SearchSvr，也不订阅内部quote.events或price.snapshots。

未报价债券、跨侧价差/偏离条件及模糊报价的完整契约未完成，见R7—R9；上表不宣称覆盖这些功能。Price原始S03是兼容适配入口，不取代前端条件PriceView。

## 5．QuoteSvr请求接口

### H01 创建／批量创建报价

服务：`QuoteSvr`；topic：`ficc.quote.create`；handler：`QuoteCreateHandler.handle`。

quotes必填，1至100条；每条须有clientQuoteId。创建PENDING_CONFIRM，不自动确认。Source及owner由服务端确定；不能假冒QT接入。单条也是quotes数组。

待确认可先保存机构／联系人名称，ID为空；后端不自动按名称匹配。此例先录入名称，H02由用户从CRM列表手动选定记录后，UI补齐ID并提交确认。

请求：

```json
{
  "requestId": "R-001",
  "quotes": [
    {
      "clientQuoteId": "CQ-001",
      "securityId": "250001.IB",
      "institutionId": null,
      "institutionName": "机构简称（待选择CRM记录）",
      "contactId": null,
      "contactName": "联系人姓名（待选择CRM记录）",
      "side": "1",
      "quoteType": "1",
      "yieldType": "1",
      "price": "2.5000",
      "quantity": "1000",
      "quantityUnit": "WAN",
      "currency": "CNY",
      "tradeDate": "2026-09-14",
      "settlType": "2"
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-001",
  "Data": {
    "results": [
      {
        "clientQuoteId": "CQ-001",
        "quoteId": "Q-001",
        "revision": "1",
        "Code": "0000",
        "Msg": "成功"
      }
    ]
  }
}
```

### H02 确认报价并允许同时修正

服务：`QuoteSvr`；topic：`ficc.quote.confirm`；handler：`QuoteConfirmHandler.handle`。

quotes中quoteId、expectedRevision必填；允许附带第3节可编辑字段。确认与修正一次事务，只生成一次CONFIRM审计。

UI让用户从CRM列表选择已有机构及联系人，将institutionId、contactId填入Quote并随本请求提交。后端校验ID和联系人归属，不执行名称猜测或自动消歧；按所选ID返回标准名称。有institutionName却无institutionId，或有contactName却无contactId时拒绝确认：单项Code=1001，error.type=UNRESOLVED_COUNTERPARTY，error.field指出待补齐的ID；状态和revision保持不变。全部对手信息未提供时仍可按原规则确认。原始输入名称保存在报价历史，名称与ID使用相同权限。

请求：

```json
{
  "requestId": "R-002",
  "quotes": [
    {
      "quoteId": "Q-001",
      "expectedRevision": "1",
      "institutionId": "I-001",
      "institutionName": "示例对手机构",
      "contactId": "C-001",
      "contactName": "示例联系人",
      "price": "2.5000"
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-002",
  "Data": {
    "results": [
      {
        "quoteId": "Q-001",
        "revision": "2",
        "Code": "0000",
        "Msg": "成功"
      }
    ]
  }
}
```

### H03 修改报价

服务：`QuoteSvr`；topic：`ficc.quote.update`；handler：`QuoteUpdateHandler.handle`。

quotes中quoteId、expectedRevision必填，其余仅提交要修改的字段；支持PENDING_CONFIRM或ACTIVE。

请求：

```json
{
  "requestId": "R-003",
  "quotes": [
    {
      "quoteId": "Q-001",
      "expectedRevision": "2",
      "price": "2.4900",
      "urgent": true
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-003",
  "Data": {
    "results": [
      {
        "quoteId": "Q-001",
        "revision": "3",
        "Code": "0000",
        "Msg": "成功"
      }
    ]
  }
}
```

### H04 撤销报价

服务：`QuoteSvr`；topic：`ficc.quote.refer`；handler：`QuoteReferHandler.handle`。

仅ACTIVE可撤销；移出当前行情，Quote事实及quoteId保留。 quotes中quoteId和expectedRevision必填，reason可选。示例是独立分支，不要求三个例子连续执行。

请求：

```json
{
  "requestId": "R-004",
  "quotes": [
    {
      "quoteId": "Q-001",
      "expectedRevision": "3",
      "reason": "用户操作"
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-004",
  "Data": {
    "results": [
      {
        "quoteId": "Q-001",
        "revision": "4",
        "Code": "0000",
        "Msg": "成功"
      }
    ]
  }
}
```

### H05 恢复报价

服务：`QuoteSvr`；topic：`ficc.quote.unrefer`；handler：`QuoteUnreferHandler.handle`。

仅REFERRED可恢复；重新校验后以同一quoteId入簿，不以今天重解释原日期。 quotes中quoteId和expectedRevision必填，reason可选。示例是独立分支，不要求三个例子连续执行。

请求：

```json
{
  "requestId": "R-005",
  "quotes": [
    {
      "quoteId": "Q-001",
      "expectedRevision": "4",
      "reason": "用户操作"
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-005",
  "Data": {
    "results": [
      {
        "quoteId": "Q-001",
        "revision": "5",
        "Code": "0000",
        "Msg": "成功"
      }
    ]
  }
}
```

### H06 删除报价

服务：`QuoteSvr`；topic：`ficc.quote.delete`；handler：`QuoteDeleteHandler.handle`。

仅PENDING_CONFIRM或REFERRED可逻辑删除；ACTIVE需先refer，DELETED不可恢复。 quotes中quoteId和expectedRevision必填，reason可选。示例是独立分支，不要求三个例子连续执行。

请求：

```json
{
  "requestId": "R-006",
  "quotes": [
    {
      "quoteId": "Q-001",
      "expectedRevision": "4",
      "reason": "用户操作"
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-006",
  "Data": {
    "results": [
      {
        "quoteId": "Q-001",
        "revision": "5",
        "Code": "0000",
        "Msg": "成功"
      }
    ]
  }
}
```

### H07 克隆报价

服务：`QuoteSvr`；topic：`ficc.quote.clone`；handler：`QuoteCloneHandler.handle`。

quotes中sourceQuoteId、clientQuoteId、expectedRevision必填，expectedRevision校验复制来源版本；允许附带可编辑字段覆盖。新记录归当前用户、状态PENDING_CONFIRM、revision=1；来源审计记录clonedFromQuoteId，不沿用旧Source身份。

请求：

```json
{
  "requestId": "R-007",
  "quotes": [
    {
      "sourceQuoteId": "Q-001",
      "expectedRevision": "2",
      "clientQuoteId": "CQ-002",
      "side": "2"
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-007",
  "Data": {
    "results": [
      {
        "clientQuoteId": "CQ-002",
        "quoteId": "Q-002",
        "revision": "1",
        "Code": "0000",
        "Msg": "成功"
      }
    ]
  }
}
```

### H08 读取单条报价

服务：`QuoteSvr`；topic：`ficc.quote.get`；handler：`QuoteGetHandler.handle`。

quoteId必填；直接读报价事实，可读取本用户有权限的逻辑删除记录。返回完整Quote；用于冲突处理与操作后校验，不受搜索索引落后影响。

请求：

```json
{
  "requestId": "R-008",
  "quoteId": "Q-001"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-008",
  "Data": {
    "quote": {
      "quoteId": "Q-001",
      "clientQuoteId": "CQ-001",
      "sourceMessageId": null,
      "source": {
        "type": "MANUAL"
      },
      "ownerUserId": "U-001",
      "securityId": "250001.IB",
      "institutionId": "I-001",
      "contactId": "C-001",
      "revision": "2",
      "status": "ACTIVE",
      "side": "1",
      "quoteType": "1",
      "yieldType": "1",
      "price": "2.5000",
      "quantity": "1000",
      "quantityUnit": "WAN",
      "currency": "CNY",
      "tradeDate": "2026-09-14",
      "settlType": "2",
      "urgent": false,
      "recommended": false,
      "whole": false,
      "oco": false,
      "packing": false,
      "remark": null,
      "createdBy": "U-001",
      "updatedBy": "U-001",
      "confirmedBy": "U-001",
      "createTime": "2026-09-14T09:00:00+08:00",
      "updateTime": "2026-09-14T09:01:00+08:00",
      "quoteTime": "2026-09-14T09:01:00+08:00",
      "confirmTime": "2026-09-14T09:01:00+08:00"
    }
  }
}
```

### H09 当前报价搜索

服务：`QuoteSvr`；topic：`ficc.quote.query`；handler：`QuoteQueryHandler.handle`。

filter可空；status和ownerScope为可选查询约束，ownerScope=MINE或VISIBLE；不允许指定任意他人userid来扩大权限。首请求limit可选，续取只传cursor。默认ACTIVE及当前权限可见范围。

请求：

```json
{
  "requestId": "R-009",
  "filter": {
    "market": "RATE",
    "conditions": [
      {
        "field": "bondRating",
        "operator": "IN",
        "value": [
          "AAA",
          "AA+"
        ]
      }
    ],
    "watchlistIds": [],
    "filterGroupIds": []
  },
  "status": [
    "ACTIVE"
  ],
  "ownerScope": "VISIBLE",
  "limit": 200
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-009",
  "Data": {
    "quotes": [
      {
        "quoteId": "Q-001",
        "clientQuoteId": "CQ-001",
        "sourceMessageId": null,
        "source": {
          "type": "MANUAL"
        },
        "ownerUserId": "U-001",
        "securityId": "250001.IB",
        "institutionId": "I-001",
        "contactId": "C-001",
        "revision": "2",
        "status": "ACTIVE",
        "side": "1",
        "quoteType": "1",
        "yieldType": "1",
        "price": "2.5000",
        "quantity": "1000",
        "quantityUnit": "WAN",
        "currency": "CNY",
        "tradeDate": "2026-09-14",
        "settlType": "2",
        "urgent": false,
        "recommended": false,
        "whole": false,
        "oco": false,
        "packing": false,
        "remark": null,
        "createdBy": "U-001",
        "updatedBy": "U-001",
        "confirmedBy": "U-001",
        "createTime": "2026-09-14T09:00:00+08:00",
        "updateTime": "2026-09-14T09:01:00+08:00",
        "quoteTime": "2026-09-14T09:01:00+08:00",
        "confirmTime": "2026-09-14T09:01:00+08:00"
      }
    ],
    "reference": {
      "250001.IB": {
        "securityName": "示例国债",
        "bondRating": "AAA",
        "referenceVersion": "B-18"
      }
    },
    "totalCount": 1,
    "nextCursor": null
  }
}
```

### H10 单条报价变更历史

服务：`QuoteSvr`；topic：`ficc.quote.history.query`；handler：`QuoteHistoryQueryHandler.handle`。

quoteId必填，limit/cursor用于滚动；按revision升序。用于报价日志，区别于跨日期历史行情搜索。完整before/after按原始事实保存；本例为CREATE记录。

请求：

```json
{
  "requestId": "R-010",
  "quoteId": "Q-001",
  "limit": 100
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-010",
  "Data": {
    "quoteId": "Q-001",
    "records": [
      {
        "eventId": "QE-001",
        "quoteId": "Q-001",
        "revision": "1",
        "action": "CREATE",
        "operatorUserId": "U-001",
        "actorService": null,
        "operationTime": "2026-09-14T09:00:00+08:00",
        "before": null,
        "after": {
          "quoteId": "Q-001",
          "clientQuoteId": "CQ-001",
          "sourceMessageId": null,
          "source": {
            "type": "MANUAL"
          },
          "ownerUserId": "U-001",
          "securityId": "250001.IB",
          "institutionId": "I-001",
          "contactId": "C-001",
          "revision": "1",
          "status": "PENDING_CONFIRM",
          "side": "1",
          "quoteType": "1",
          "yieldType": "1",
          "price": "2.5000",
          "quantity": "1000",
          "quantityUnit": "WAN",
          "currency": "CNY",
          "tradeDate": "2026-09-14",
          "settlType": "2",
          "urgent": false,
          "recommended": false,
          "whole": false,
          "oco": false,
          "packing": false,
          "remark": null,
          "createdBy": "U-001",
          "updatedBy": "U-001",
          "confirmedBy": null,
          "createTime": "2026-09-14T09:00:00+08:00",
          "updateTime": "2026-09-14T09:00:00+08:00",
          "quoteTime": null,
          "confirmTime": null
        },
        "reason": null
      }
    ],
    "nextCursor": null
  }
}
```

### H11 跨日期历史报价搜索

服务：`QuoteSvr`；topic：`ficc.quote.history.search`；handler：`QuoteHistorySearchHandler.handle`。

fromDate、toDate必填，filter/limit可选；无订阅。当前评级和最新日估值参与筛选。rowKey由服务端分配；历史记录粒度见评审R1，本接口不默认使用QuoteHistory所有变更记录。

请求：

```json
{
  "requestId": "R-011",
  "fromDate": "2026-09-01",
  "toDate": "2026-09-14",
  "filter": {
    "conditions": [
      {
        "field": "bondRating",
        "operator": "EQ",
        "value": "AAA"
      }
    ]
  },
  "limit": 200
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-011",
  "Data": {
    "rows": [
      {
        "rowKey": "HQ-001",
        "businessDate": "2026-09-14",
        "quote": {
          "quoteId": "Q-001",
          "clientQuoteId": "CQ-001",
          "sourceMessageId": null,
          "source": {
            "type": "MANUAL"
          },
          "ownerUserId": "U-001",
          "securityId": "250001.IB",
          "institutionId": "I-001",
          "contactId": "C-001",
          "revision": "2",
          "status": "ACTIVE",
          "side": "1",
          "quoteType": "1",
          "yieldType": "1",
          "price": "2.5000",
          "quantity": "1000",
          "quantityUnit": "WAN",
          "currency": "CNY",
          "tradeDate": "2026-09-14",
          "settlType": "2",
          "urgent": false,
          "recommended": false,
          "whole": false,
          "oco": false,
          "packing": false,
          "remark": null,
          "createdBy": "U-001",
          "updatedBy": "U-001",
          "confirmedBy": "U-001",
          "createTime": "2026-09-14T09:00:00+08:00",
          "updateTime": "2026-09-14T09:01:00+08:00",
          "quoteTime": "2026-09-14T09:01:00+08:00",
          "confirmTime": "2026-09-14T09:01:00+08:00"
        },
        "reference": {
          "securityName": "示例国债",
          "bondRating": "AAA",
          "valuationDate": "2026-09-14",
          "valuationSource": "CHINABOND",
          "yield": "2.4800"
        }
      }
    ],
    "nextCursor": null
  }
}
```

### H12 来源原文及候选关联查询

服务：`QuoteSvr`；topic：`ficc.quote.sourceMessage.query`；handler：`QuoteSourceMessageQueryHandler.handle`。

sourceMessageId或筛选条件sourceType/fromTime/toTime至少一种；用于报价确认页定位来源原文及解析候选，不是正式报价搜索。原文仅返回授权用户可见范围。

请求：

```json
{
  "requestId": "R-012",
  "sourceMessageId": "SM-001",
  "limit": 20
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-012",
  "Data": {
    "sourceMessages": [
      {
        "sourceMessageId": "SM-001",
        "sourceType": "QT",
        "externalMessageId": "QT-M-9001",
        "sourceTime": "2026-09-14T08:59:00+08:00",
        "text": "示例国债买入2.50，1000万，今+1",
        "quoteIds": [
          "Q-010"
        ]
      }
    ],
    "nextCursor": null
  }
}
```

### H13 单券／多券行情查询

服务：`QuoteSvr`；topic：`ficc.price.query`；handler：`PriceQueryHandler.handle`。

securityIds必填，最多100券；mode必填BEST/DEPTH/FULL；DEPTH必须有depth。filter用于成员筛选，先过滤后分档。返回每券完整选中范围；空簿用NoMDEntries=[]，不是沿用旧值。

请求：

```json
{
  "requestId": "R-013",
  "securityIds": [
    "250001.IB"
  ],
  "mode": "DEPTH",
  "depth": 2,
  "filter": {
    "conditions": [
      {
        "field": "settlType",
        "operator": "IN",
        "value": [
          "1",
          "2"
        ]
      }
    ]
  }
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-013",
  "Data": {
    "prices": [
      {
        "securityId": "250001.IB",
        "bookRevision": "7",
        "marketData": {
          "SecurityID": "250001.IB",
          "Symbol": "示例国债",
          "MarketIndicator": "4",
          "VenueTypeGW": "FICC",
          "Venues": "FICC",
          "NoMDEntries": [
            {
              "MDEntryID": "FICC:Q-001",
              "MDEntryType": "0",
              "MDEntryPx": "2.5000",
              "MDEntrySize": "1000",
              "Yield": "2.5000",
              "PriceType": "3",
              "ExerciseFlag": "1",
              "SettlType": "2",
              "UserID": "U-001",
              "MDPriceLevel": "1",
              "MDEntryDate": "20260914",
              "MDEntryTime": "09:01:00.000"
            }
          ]
        },
        "quoteRefs": {
          "FICC:Q-001": {
            "quoteId": "Q-001",
            "quoteRevision": "2"
          }
        }
      }
    ]
  }
}
```

### H14 业务字典与可用筛选字段

服务：`QuoteSvr`；topic：`ficc.quote.dictionary.query`；handler：`QuoteDictionaryQueryHandler.handle`。

names可选，缺省返回本版全部业务字典；此接口返回Quote字典及filterFields，Price使用ficc.price.dictionary.query。当前字段目录来自共享FilterEngine，不调用尚未实现的SearchSvr。以下字典应答仅展示dictionaries，完整返回还包含filterFields；字段scope及operators以实际接口为准。

2026-09-21：字段目录为79项，覆盖报价、债券、流动性、信用、条款和区域。目录包含label/group/examples/includesSimulatedData；分类字段另返回enumValues（value/label），支持EQ/IN/EXISTS。dictionaries返回37个分类及4个地区字典，前端显示label、发送value，不发送中文分类名称；自由文字仍支持CONTAINS。客户端不应硬编码旧字段白名单。完整字段、单位、模拟数据开关及组合示例见[完整属性筛选接口](product-filter-contract.zh-CN.md)。查询、历史筛选与订阅采用同一条件语义；未知属性不会自动视为“否”或“其他”。

请求：

```json
{
  "requestId": "R-014",
  "names": [
    "side",
    "quoteType",
    "yieldType"
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-014",
  "Data": {
    "dictionaries": {
      "side": [
        {
          "value": "1",
          "label": "买入"
        },
        {
          "value": "2",
          "label": "卖出"
        }
      ],
      "quoteType": [
        {
          "value": "1",
          "label": "收益率"
        },
        {
          "value": "2",
          "label": "净价"
        },
        {
          "value": "3",
          "label": "全价"
        }
      ],
      "yieldType": [
        {
          "value": "1",
          "label": "到期"
        },
        {
          "value": "2",
          "label": "行权"
        }
      ]
    }
  }
}
```

## 6．QuoteSvr面板与分组接口

### H15 取得当前连接的私有推送topic

服务：`QuoteSvr`；topic：`ficc.quote.stream.open`；handler：`QuoteStreamOpenHandler.handle`。

空业务参数，身份来自可信会话；按用户及客户端连接复用本领域临时流。仅分配ficc.quote.report路由，不执行DPS订阅，不返回已有业务记录。客户端用普通subscribe订阅后，等待带相同streamId的首个HEARTBEAT确认可收到消息，再open view。写入请求不依赖订阅已建立；无推送时仍返回正常应答，可通过get/query读取结果。

请求：

```json
{
  "requestId": "R-015"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-015",
  "Data": {
    "sessionId": "S-001",
    "pushTopic": "ficc.quote.report.S-001",
    "streamId": "E-001"
  }
}
```

### H16 登记条件并查询初始列表

服务：`QuoteSvr`；topic：`ficc.quote.view.open`；handler：`QuoteViewOpenHandler.handle`。

以下为已实现的新协议。viewId由客户端生成且在本领域连接内永不复用；filter、pageSize可选，pageSize默认200、范围1—1000。open固定返回第1页，不接收limit。status及ownerScope与quote.query一致；禁止dataset、mode、depth及行情模型字段。本领域report订阅就绪后调用。服务登记条件并返回第一页及边界sequence；这不是订阅image。按第10节缓冲应答前到达的变化。

请求：

```json
{
  "requestId": "R-016",
  "viewId": "V-001",
  "filter": {
    "conditions": []
  },
  "pageSize": 200
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-016",
  "Data": {
    "streamId": "E-001",
    "sequence": "10",
    "view": {
      "viewId": "V-001",
      "pageNumber": 1,
      "pageSize": 200,
      "totalCount": 1,
      "ids": [
        "Q-001"
      ]
    },
    "quotes": [
      {
        "quoteId": "Q-001",
        "clientQuoteId": "CQ-001",
        "sourceMessageId": null,
        "source": {
          "type": "MANUAL"
        },
        "ownerUserId": "U-001",
        "securityId": "250001.IB",
        "institutionId": "I-001",
        "contactId": "C-001",
        "revision": "2",
        "status": "ACTIVE",
        "side": "1",
        "quoteType": "1",
        "yieldType": "1",
        "price": "2.5000",
        "quantity": "1000",
        "quantityUnit": "WAN",
        "currency": "CNY",
        "tradeDate": "2026-09-14",
        "settlType": "2",
        "urgent": false,
        "recommended": false,
        "whole": false,
        "oco": false,
        "packing": false,
        "remark": null,
        "createdBy": "U-001",
        "updatedBy": "U-001",
        "confirmedBy": "U-001",
        "createTime": "2026-09-14T09:00:00+08:00",
        "updateTime": "2026-09-14T09:01:00+08:00",
        "quoteTime": "2026-09-14T09:01:00+08:00",
        "confirmTime": "2026-09-14T09:01:00+08:00"
      }
    ]
  }
}
```

### H17 查询报价view的指定页（已实现）

服务：`QuoteSvr`；topic：`ficc.quote.view.query`；handler：`QuoteViewQueryHandler.handle`。

viewId、pageNumber、pageSize必填；pageNumber为从1开始的正整数，pageSize范围1—1000。服务复用view登记的条件和当前用户权限，按`offset=(pageNumber-1)*pageSize`查询，返回该时点指定页及sequence，不返回此前各页。超出结果范围时返回空ids／quotes和实际totalCount。

滚动时请求目标页；第一页因实时移除出现缺口时请求pageNumber=1。每个view同一时间只允许一个在途页查询，前端按requestId关联应答并忽略过期请求；快速滚动合并到最新目标页。此接口不创建新view或订阅，不修改筛选。改变条件先close旧view，再以新viewId open。H09 quote.query仍是独立的一次性搜索，不替代本接口。

迁移说明：`ficc.quote.view.extend`已移除，由本接口替代；旧客户端与服务必须同时升级。示例查询第2页，每页200条，共201条，故只返回1条。

请求：

```json
{
  "requestId": "R-017",
  "viewId": "V-001",
  "pageNumber": 2,
  "pageSize": 200
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-017",
  "Data": {
    "streamId": "E-001",
    "sequence": "15",
    "view": {
      "viewId": "V-001",
      "pageNumber": 2,
      "pageSize": 200,
      "totalCount": 201,
      "ids": [
        "Q-201"
      ]
    },
    "quotes": [
      {
        "quoteId": "Q-201",
        "clientQuoteId": "CQ-201",
        "sourceMessageId": null,
        "source": {
          "type": "MANUAL"
        },
        "ownerUserId": "U-001",
        "securityId": "250001.IB",
        "institutionId": "I-001",
        "contactId": "C-001",
        "revision": "2",
        "status": "ACTIVE",
        "side": "1",
        "quoteType": "1",
        "yieldType": "1",
        "price": "2.5000",
        "quantity": "1000",
        "quantityUnit": "WAN",
        "currency": "CNY",
        "tradeDate": "2026-09-14",
        "settlType": "2",
        "urgent": false,
        "recommended": false,
        "whole": false,
        "oco": false,
        "packing": false,
        "remark": null,
        "createdBy": "U-001",
        "updatedBy": "U-001",
        "confirmedBy": "U-001",
        "createTime": "2026-09-14T09:00:00+08:00",
        "updateTime": "2026-09-14T09:01:00+08:00",
        "quoteTime": "2026-09-14T09:01:00+08:00",
        "confirmTime": "2026-09-14T09:01:00+08:00"
      }
    ]
  }
}
```

### H18 关闭实时面板

服务：`QuoteSvr`；topic：`ficc.quote.view.close`；handler：`QuoteViewCloseHandler.handle`。

viewId必填；已关闭重复调用成功，释放成员和条件缓存。关闭一个view不退订整个连接的report topic。已关闭view的在途消息忽略。

请求：

```json
{
  "requestId": "R-018",
  "viewId": "V-001"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-018",
  "Data": {
    "viewId": "V-001",
    "closed": true
  }
}
```

### H19 读取个人关注分组

服务：`QuoteSvr`；topic：`ficc.watchlist.query`；handler：`WatchlistQueryHandler.handle`。

market/scope可选，watchlistId可选；只读当前用户。每组同时返回完整债券成员。

请求：

```json
{
  "requestId": "R-019",
  "market": "RATE",
  "scope": "MARKET"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-019",
  "Data": {
    "watchlists": [
      {
        "watchlistId": "W-001",
        "userId": "U-001",
        "revision": "1",
        "name": "常看国债",
        "market": "RATE",
        "scope": "MARKET",
        "displayOrder": 0,
        "securities": [
          {
            "securityId": "250001.IB",
            "displayOrder": 0
          }
        ]
      }
    ]
  }
}
```

### H20 保存关注分组及全部成员

服务：`QuoteSvr`；topic：`ficc.watchlist.save`；handler：`WatchlistSaveHandler.handle`。

新建不传watchlistId、expectedRevision；更新两者必填。name、market、scope、securities必填，完整替换；同券去重，用户不能修改userId。

请求：

```json
{
  "requestId": "R-020",
  "name": "常看国债",
  "market": "RATE",
  "scope": "MARKET",
  "displayOrder": 0,
  "securities": [
    {
      "securityId": "250001.IB",
      "displayOrder": 0
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-020",
  "Data": {
    "watchlist": {
      "watchlistId": "W-001",
      "userId": "U-001",
      "revision": "1",
      "name": "常看国债",
      "market": "RATE",
      "scope": "MARKET",
      "displayOrder": 0,
      "securities": [
        {
          "securityId": "250001.IB",
          "displayOrder": 0
        }
      ]
    }
  }
}
```

### H21 删除关注分组

服务：`QuoteSvr`；topic：`ficc.watchlist.delete`；handler：`WatchlistDeleteHandler.handle`。

watchlistId和expectedRevision必填；删除成员关系，不删除债券或Quote。生效后事件通知提醒服务与相关view；引用已删除分组的view进入RESET_REQUIRED，不能自动变成不限制。

请求：

```json
{
  "requestId": "R-021",
  "watchlistId": "W-001",
  "expectedRevision": "1"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-021",
  "Data": {
    "watchlistId": "W-001",
    "deleted": true
  }
}
```

### H22 读取个人筛选分组

服务：`QuoteSvr`；topic：`ficc.filterGroup.query`；handler：`FilterGroupQueryHandler.handle`。

market/scope/filterGroupId可选；返回组、子组及条件，不返回固定命中债券成员。

请求：

```json
{
  "requestId": "R-022",
  "market": "RATE",
  "scope": "MARKET"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-022",
  "Data": {
    "filterGroups": [
      {
        "filterGroupId": "FG-001",
        "userId": "U-001",
        "revision": "1",
        "name": "高评级债券",
        "market": "RATE",
        "scope": "MARKET",
        "displayOrder": 0,
        "subgroups": [
          {
            "subgroupId": "SG-001",
            "displayOrder": 0,
            "conditions": [
              {
                "conditionId": "FC-001",
                "field": "bondRating",
                "operator": "IN",
                "value": [
                  "AAA",
                  "AA+"
                ],
                "displayOrder": 0
              }
            ]
          }
        ]
      }
    ]
  }
}
```

### H23 保存筛选组及全部条件

服务：`QuoteSvr`；topic：`ficc.filterGroup.save`；handler：`FilterGroupSaveHandler.handle`。

新建不传filterGroupId、expectedRevision；更新必填。name、market、scope、subgroups必填；整组替换并校验字段及操作。通知分组仅筛选通知列表，不改变四类提醒的生成条件。

请求：

```json
{
  "requestId": "R-023",
  "name": "高评级债券",
  "market": "RATE",
  "scope": "MARKET",
  "subgroups": [
    {
      "conditions": [
        {
          "field": "bondRating",
          "operator": "IN",
          "value": [
            "AAA",
            "AA+"
          ]
        }
      ]
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-023",
  "Data": {
    "filterGroup": {
      "filterGroupId": "FG-001",
      "userId": "U-001",
      "revision": "1",
      "name": "高评级债券",
      "market": "RATE",
      "scope": "MARKET",
      "displayOrder": 0,
      "subgroups": [
        {
          "subgroupId": "SG-001",
          "displayOrder": 0,
          "conditions": [
            {
              "conditionId": "FC-001",
              "field": "bondRating",
              "operator": "IN",
              "value": [
                "AAA",
                "AA+"
              ],
              "displayOrder": 0
            }
          ]
        }
      ]
    }
  }
}
```

### H24 删除筛选分组

服务：`QuoteSvr`；topic：`ficc.filterGroup.delete`；handler：`FilterGroupDeleteHandler.handle`。

filterGroupId、expectedRevision必填；同时移除子组及条件，不修改已保存Quote。引用已删除组的view失效并明确提示，不放宽查询。

请求：

```json
{
  "requestId": "R-024",
  "filterGroupId": "FG-001",
  "expectedRevision": "1"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-024",
  "Data": {
    "filterGroupId": "FG-001",
    "deleted": true
  }
}
```

### 6.1 独立Price接口

行情接口与报价接口由同一QuoteSvr托管，但使用不同Handler、DTO、topic、缓存和序列。以下编号接续原目录，方便比较评审。

### H50 取得独立的行情推送topic

服务：`QuoteSvr`；topic：`ficc.price.stream.open`；handler：`PriceStreamOpenHandler.handle`。

按认证用户与客户端连接分配独立Price流；不能复用Quote的pushTopic或sequence。客户端通过subscribeWithImage订阅后等待Price初始image，再打开行情view；尚无view时image为views=[]、books=[]。

请求：

```json
{
  "requestId": "R-050"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-050",
  "Data": {
    "sessionId": "S-001",
    "streamId": "PE-001",
    "pushTopic": "ficc.price.report.S-001"
  }
}
```

### H51 登记行情条件并取得当前价格簿

服务：`QuoteSvr`；topic：`ficc.price.view.open`；handler：`PriceViewOpenHandler.handle`。

以下为已实现的新协议。viewId、mode必填；filter和pageSize可选，pageSize默认200、范围1—1000，open返回第一页，不接收limit。mode取BEST/FULL/DEPTH，DEPTH必须有正整数depth，其他mode禁止depth。不接受dataset、Quote状态或ownerScope参数。pageSize按债券行计数；权限与清算等条目条件先过滤，再按价格档计算。返回独立PriceViewReply；禁止quotes/reports字段。跨侧价差等未完成的过滤语义见R8。

请求：

```json
{
  "requestId": "R-051",
  "viewId": "PV-001",
  "filter": {
    "market": "RATE",
    "conditions": [],
    "watchlistIds": [],
    "filterGroupIds": []
  },
  "mode": "BEST",
  "pageSize": 200
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-051",
  "Data": {
    "streamId": "PE-001",
    "sequence": "10",
    "view": {
      "viewId": "PV-001",
      "totalCount": 1,
      "ids": [
        "250001.IB"
      ],
      "pageSize": 200,
      "pageNumber": 1
    },
    "books": [
      {
        "viewId": "PV-001",
        "securityId": "250001.IB",
        "bookRevision": "7",
        "marketData": {
          "SecurityID": "250001.IB",
          "Symbol": "示例国债",
          "MarketIndicator": "4",
          "VenueTypeGW": "FICC",
          "Venues": "FICC",
          "NoMDEntries": [
            {
              "MDEntryID": "FICC:Q-001",
              "MDEntryType": "0",
              "MDEntryPx": "2.5000",
              "MDEntrySize": "1000",
              "Yield": "2.5000",
              "PriceType": "3",
              "ExerciseFlag": "1",
              "SettlType": "2",
              "UserID": "U-001",
              "MDPriceLevel": "1",
              "MDEntryDate": "20260914",
              "MDEntryTime": "09:01:00.000"
            }
          ]
        },
        "quoteRefs": {
          "FICC:Q-001": {
            "quoteId": "Q-001",
            "quoteRevision": "2"
          }
        },
        "updateTime": "2026-09-14T09:01:00+08:00"
      }
    ]
  }
}
```

### H52 查询行情view的指定页（已实现）

服务：`QuoteSvr`；topic：`ficc.price.view.query`；handler：`PriceViewQueryHandler.handle`。

viewId、pageNumber、pageSize必填，校验及查询边界与H17相同；复用mode/depth/filter及原订阅，不重新登记条件。每次只返回指定页的债券及各自完整价格簿，不重发此前各页。客户端按books.updateTime排序第一页；页内补位请求pageNumber=1。改条件使用新viewId重新open。此接口替代旧price.view.extend，已注册。示例查询第2页，每页200条，共201条。

请求：

```json
{
  "requestId": "R-052",
  "viewId": "PV-001",
  "pageSize": 200,
  "pageNumber": 2
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-052",
  "Data": {
    "streamId": "PE-001",
    "sequence": "12",
    "view": {
      "viewId": "PV-001",
      "totalCount": 201,
      "ids": [
        "250001.IB"
      ],
      "pageSize": 200,
      "pageNumber": 2
    },
    "books": [
      {
        "viewId": "PV-001",
        "securityId": "250001.IB",
        "bookRevision": "7",
        "marketData": {
          "SecurityID": "250001.IB",
          "Symbol": "示例国债",
          "MarketIndicator": "4",
          "VenueTypeGW": "FICC",
          "Venues": "FICC",
          "NoMDEntries": [
            {
              "MDEntryID": "FICC:Q-001",
              "MDEntryType": "0",
              "MDEntryPx": "2.5000",
              "MDEntrySize": "1000",
              "Yield": "2.5000",
              "PriceType": "3",
              "ExerciseFlag": "1",
              "SettlType": "2",
              "UserID": "U-001",
              "MDPriceLevel": "1",
              "MDEntryDate": "20260914",
              "MDEntryTime": "09:01:00.000"
            }
          ]
        },
        "quoteRefs": {
          "FICC:Q-001": {
            "quoteId": "Q-001",
            "quoteRevision": "2"
          }
        },
        "updateTime": "2026-09-14T09:01:00+08:00"
      }
    ]
  }
}
```

### H53 关闭行情view

服务：`QuoteSvr`；topic：`ficc.price.view.close`；handler：`PriceViewCloseHandler.handle`。

viewId必填。仅关闭本Price频道的view，已关闭重复请求返回成功；不会关闭同客户端的QuoteView。其他用户的view不得操作。

请求：

```json
{
  "requestId": "R-053",
  "viewId": "PV-001"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-053",
  "Data": {
    "viewId": "PV-001",
    "closed": true
  }
}
```

### H54 读取行情字典与可用筛选字段

服务：`QuoteSvr`；topic：`ficc.price.dictionary.query`；handler：`PriceDictionaryQueryHandler.handle`。

names可选，缺省返回当前已实现且该用户可用的行情字典和filterFields；不使用dataset选择Quote模型。filterFields必须标明条件作用层级：SECURITY债券、ENTRY单条行情、BOOK价格簿。本例只展示已定义字段；跨侧价差/偏离等未定稿字段不得以enabled=true返回。

请求：

```json
{
  "requestId": "R-054"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-054",
  "Data": {
    "dictionaries": {
      "mode": [
        {
          "value": "BEST",
          "label": "最优"
        },
        {
          "value": "FULL",
          "label": "全部"
        },
        {
          "value": "DEPTH",
          "label": "指定档数"
        }
      ],
      "market": [
        {
          "value": "CREDIT",
          "label": "信用"
        },
        {
          "value": "RATE",
          "label": "利率"
        }
      ]
    },
    "filterFields": [
      {
        "field": "securityId",
        "type": "string",
        "operators": [
          "EQ",
          "IN"
        ],
        "scope": "SECURITY",
        "enabled": true
      },
      {
        "field": "settlType",
        "type": "enum",
        "operators": [
          "EQ",
          "IN"
        ],
        "scope": "ENTRY",
        "enabled": true
      }
    ]
  }
}
```

## 7．NotificationSvr请求接口

### H25 取得当前连接的私有推送topic

服务：`NotificationSvr`；topic：`ficc.notification.stream.open`；handler：`NotificationStreamOpenHandler.handle`。

空业务参数，身份来自可信会话；按用户及客户端连接复用本领域临时流。仅分配ficc.notification.report路由，不执行DPS订阅，不返回已有业务记录。客户端用普通subscribe订阅后，等待带相同streamId的首个HEARTBEAT确认可收到消息，再open view。写入请求不依赖订阅已建立；无推送时仍返回正常应答，可通过get/query读取结果。

请求：

```json
{
  "requestId": "R-025"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-025",
  "Data": {
    "sessionId": "S-001",
    "pushTopic": "ficc.notification.report.S-001",
    "streamId": "NE-001"
  }
}
```

### H26 登记条件并查询初始列表

服务：`NotificationSvr`；topic：`ficc.notification.view.open`；handler：`NotificationViewOpenHandler.handle`。

以下为已实现的新协议。viewId由客户端生成且在本领域连接内永不复用；filter、pageSize可选，pageSize默认200、范围1—1000，open返回第一页，不接收limit。可传readState=ALL/READ/UNREAD；禁止dataset和行情参数。本领域report订阅就绪后调用，登记条件并返回第一页及边界sequence；这不是订阅image。按第10节缓冲应答前到达的变化。

请求：

```json
{
  "requestId": "R-026",
  "viewId": "V-001",
  "filter": {
    "conditions": []
  },
  "pageSize": 200
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-026",
  "Data": {
    "streamId": "NE-001",
    "sequence": "10",
    "view": {
      "viewId": "V-001",
      "totalCount": 1,
      "ids": [
        "N-001"
      ],
      "pageSize": 200,
      "pageNumber": 1
    },
    "notifications": [
      {
        "notificationId": "N-001",
        "userId": "U-001",
        "securityId": "250001.IB",
        "reasons": [
          {
            "type": "WATCHLIST",
            "groupIds": [
              "W-001"
            ],
            "text": "命中关注分组：常看国债"
          }
        ],
        "content": "关注债券有新报价",
        "quoteIds": [
          "Q-001"
        ],
        "readTime": null,
        "createTime": "2026-09-14T09:01:00+08:00",
        "updateTime": "2026-09-14T09:01:00+08:00"
      }
    ]
  }
}
```

### H27 查询通知view的指定页（已实现）

服务：`NotificationSvr`；topic：`ficc.notification.view.query`；handler：`NotificationViewQueryHandler.handle`。

viewId、pageNumber、pageSize必填，校验及查询边界与H17相同；复用filter/readState及原订阅。只返回该用户指定页的notifications及sequence，不重发此前各页。第一页移除后用pageNumber=1补位；改条件先close旧view，再用新viewId open。此接口替代旧notification.view.extend，已注册。新设计每次最多1000条，不把旧累计1000条限制当作全部可浏览结果上限。示例查询第2页，每页200条，共201条。

请求：

```json
{
  "requestId": "R-027",
  "viewId": "V-001",
  "pageSize": 200,
  "pageNumber": 2
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-027",
  "Data": {
    "streamId": "NE-001",
    "sequence": "15",
    "view": {
      "viewId": "V-001",
      "totalCount": 201,
      "ids": [
        "N-001"
      ],
      "pageSize": 200,
      "pageNumber": 2
    },
    "notifications": [
      {
        "notificationId": "N-001",
        "userId": "U-001",
        "securityId": "250001.IB",
        "reasons": [
          {
            "type": "WATCHLIST",
            "groupIds": [
              "W-001"
            ],
            "text": "命中关注分组：常看国债"
          }
        ],
        "content": "关注债券有新报价",
        "quoteIds": [
          "Q-001"
        ],
        "readTime": null,
        "createTime": "2026-09-14T09:01:00+08:00",
        "updateTime": "2026-09-14T09:01:00+08:00"
      }
    ]
  }
}
```

### H28 关闭实时面板

服务：`NotificationSvr`；topic：`ficc.notification.view.close`；handler：`NotificationViewCloseHandler.handle`。

viewId必填；已关闭重复调用成功，释放成员和条件缓存。关闭一个view不退订整个连接的report topic。已关闭view的在途消息忽略。

请求：

```json
{
  "requestId": "R-028",
  "viewId": "V-001"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-028",
  "Data": {
    "viewId": "V-001",
    "closed": true
  }
}
```

### H29 读取当前通知

服务：`NotificationSvr`；topic：`ficc.notification.query`；handler：`NotificationQueryHandler.handle`。

filter、readState可选，readState=ALL/READ/UNREAD，默认ALL；limit/cursor用于一次性滚动。用户只能读取自己的通知。本版没有历史通知收件箱API。

请求：

```json
{
  "requestId": "R-029",
  "readState": "UNREAD",
  "limit": 200
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-029",
  "Data": {
    "notifications": [
      {
        "notificationId": "N-001",
        "userId": "U-001",
        "securityId": "250001.IB",
        "reasons": [
          {
            "type": "WATCHLIST",
            "groupIds": [
              "W-001"
            ],
            "text": "命中关注分组：常看国债"
          }
        ],
        "content": "关注债券有新报价",
        "quoteIds": [
          "Q-001"
        ],
        "readTime": null,
        "createTime": "2026-09-14T09:01:00+08:00",
        "updateTime": "2026-09-14T09:01:00+08:00"
      }
    ],
    "nextCursor": null
  }
}
```

### H30 标记已读／未读

服务：`NotificationSvr`；topic：`ficc.notification.read`；handler：`NotificationReadHandler.handle`。

notificationIds、read必填；read=true由服务端写readTime，false清空。显式ID数组避免“全部已读”误覆盖调用后新通知。逐项返回结果，不存在或不可见项报1002。

请求：

```json
{
  "requestId": "R-030",
  "notificationIds": [
    "N-001"
  ],
  "read": true
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-030",
  "Data": {
    "results": [
      {
        "notificationId": "N-001",
        "Code": "0000",
        "Msg": "成功",
        "readTime": "2026-09-14T09:02:00+08:00"
      }
    ]
  }
}
```

### H31 读取个人提醒开关

服务：`NotificationSvr`；topic：`ficc.notification.preference.get`；handler：`NotificationPreferenceGetHandler.handle`。

无业务参数；缺少设置返回默认对象revision=0，默认值建议全部开启，待评审。

请求：

```json
{
  "requestId": "R-031"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-031",
  "Data": {
    "preference": {
      "userId": "U-001",
      "revision": "1",
      "watchlistEnabled": true,
      "filterGroupEnabled": true,
      "counterQuoteEnabled": true,
      "myBestEnabled": true,
      "updateTime": "2026-09-14T09:00:00+08:00"
    }
  }
}
```

### H32 保存个人提醒开关

服务：`NotificationSvr`；topic：`ficc.notification.preference.save`；handler：`NotificationPreferenceSaveHandler.handle`。

expectedRevision及四个boolean必填，完整替换；成功后重评当前提醒并推送。不能因关闭开关清除用户此前已读状态。

请求：

```json
{
  "requestId": "R-032",
  "expectedRevision": "1",
  "watchlistEnabled": true,
  "filterGroupEnabled": true,
  "counterQuoteEnabled": false,
  "myBestEnabled": true
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-032",
  "Data": {
    "preference": {
      "userId": "U-001",
      "revision": "2",
      "watchlistEnabled": true,
      "filterGroupEnabled": true,
      "counterQuoteEnabled": false,
      "myBestEnabled": true,
      "updateTime": "2026-09-14T09:02:00+08:00"
    }
  }
}
```

### H33 读取个人筛选分组

服务：`NotificationSvr`；topic：`ficc.notification.filterGroup.query`；handler：`NotificationFilterGroupQueryHandler.handle`。

market/scope/filterGroupId可选；返回组、子组及条件，不返回固定命中债券成员。

请求：

```json
{
  "requestId": "R-033",
  "market": "RATE",
  "scope": "NOTIFICATION"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-033",
  "Data": {
    "filterGroups": [
      {
        "filterGroupId": "FG-001",
        "userId": "U-001",
        "revision": "1",
        "name": "高评级债券",
        "market": "RATE",
        "scope": "NOTIFICATION",
        "displayOrder": 0,
        "subgroups": [
          {
            "subgroupId": "SG-001",
            "displayOrder": 0,
            "conditions": [
              {
                "conditionId": "FC-001",
                "field": "bondRating",
                "operator": "IN",
                "value": [
                  "AAA",
                  "AA+"
                ],
                "displayOrder": 0
              }
            ]
          }
        ]
      }
    ]
  }
}
```

### H34 保存筛选组及全部条件

服务：`NotificationSvr`；topic：`ficc.notification.filterGroup.save`；handler：`NotificationFilterGroupSaveHandler.handle`。

新建不传filterGroupId、expectedRevision；更新必填。name、market、scope、subgroups必填；整组替换并校验字段及操作。通知分组仅筛选通知列表，不改变四类提醒的生成条件。

请求：

```json
{
  "requestId": "R-034",
  "name": "高评级债券",
  "market": "RATE",
  "scope": "NOTIFICATION",
  "subgroups": [
    {
      "conditions": [
        {
          "field": "bondRating",
          "operator": "IN",
          "value": [
            "AAA",
            "AA+"
          ]
        }
      ]
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-034",
  "Data": {
    "filterGroup": {
      "filterGroupId": "FG-001",
      "userId": "U-001",
      "revision": "1",
      "name": "高评级债券",
      "market": "RATE",
      "scope": "NOTIFICATION",
      "displayOrder": 0,
      "subgroups": [
        {
          "subgroupId": "SG-001",
          "displayOrder": 0,
          "conditions": [
            {
              "conditionId": "FC-001",
              "field": "bondRating",
              "operator": "IN",
              "value": [
                "AAA",
                "AA+"
              ],
              "displayOrder": 0
            }
          ]
        }
      ]
    }
  }
}
```

### H35 删除筛选分组

服务：`NotificationSvr`；topic：`ficc.notification.filterGroup.delete`；handler：`NotificationFilterGroupDeleteHandler.handle`。

filterGroupId、expectedRevision必填；同时移除子组及条件，不修改已保存Quote。引用已删除组的view失效并明确提示，不放宽查询。

请求：

```json
{
  "requestId": "R-035",
  "filterGroupId": "FG-001",
  "expectedRevision": "1"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-035",
  "Data": {
    "filterGroupId": "FG-001",
    "deleted": true
  }
}
```

## 8．内部搜索、资料、解析与接入接口

### H36 内部DSL查询（待实现）

服务：`SearchSvr`；topic：`ficc.search.query`；handler：`SearchQueryHandler.handle`。

仅认证业务服务可用。collection必填QUOTE/MARKET_ENTRY/NOTIFICATION/HISTORY_QUOTE，where为最小DSL，actorUserId为代查用户；调用服务提供经校验的权限约束，SearchSvr再次限制服务可读集合及字段。limit/cursor续取；minPositions可选，未追上返回1009。输出row为该集合完整模型，禁止调用者指定任意数据库表或脚本。此collection为内部存储DSL参数，不是前端Quote/Price接口合并的dataset。MARKET_ENTRY由独立price.snapshots拆入；位置校验分别使用quote的epoch/sequence与price的streamId/sequence。

请求：

```json
{
  "requestId": "R-036",
  "collection": "QUOTE",
  "actorUserId": "U-001",
  "where": {
    "$and": [
      {
        "status": {
          "$eq": "ACTIVE"
        }
      },
      {
        "bondRating": {
          "$in": [
            "AAA",
            "AA+"
          ]
        }
      }
    ]
  },
  "limit": 200,
  "minPositions": {
    "quote": {
      "epoch": "QEPOCH-1",
      "sequence": "20"
    }
  }
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-036",
  "Data": {
    "rows": [
      {
        "quoteId": "Q-001",
        "clientQuoteId": "CQ-001",
        "sourceMessageId": null,
        "source": {
          "type": "MANUAL"
        },
        "ownerUserId": "U-001",
        "securityId": "250001.IB",
        "institutionId": "I-001",
        "contactId": "C-001",
        "revision": "2",
        "status": "ACTIVE",
        "side": "1",
        "quoteType": "1",
        "yieldType": "1",
        "price": "2.5000",
        "quantity": "1000",
        "quantityUnit": "WAN",
        "currency": "CNY",
        "tradeDate": "2026-09-14",
        "settlType": "2",
        "urgent": false,
        "recommended": false,
        "whole": false,
        "oco": false,
        "packing": false,
        "remark": null,
        "createdBy": "U-001",
        "updatedBy": "U-001",
        "confirmedBy": "U-001",
        "createTime": "2026-09-14T09:00:00+08:00",
        "updateTime": "2026-09-14T09:01:00+08:00",
        "quoteTime": "2026-09-14T09:01:00+08:00",
        "confirmTime": "2026-09-14T09:01:00+08:00"
      }
    ],
    "nextCursor": null,
    "positions": {
      "quote": {
        "epoch": "QEPOCH-1",
        "sequence": "20"
      },
      "base": {
        "epoch": "BEPOCH-1",
        "sequence": "18"
      }
    },
    "attributeVersion": "18"
  }
}
```

### H37 取得完整搜索属性（待实现）

服务：`SearchSvr`；topic：`ficc.search.attributes.get`；handler：`SearchAttributesGetHandler.handle`。

securityIds必填；minAttributeVersion可选。返回按券补齐的当前债券、发行人、评级、估值属性；缺失ID在missingIds返回，不能当作空对象参与“不匹配”判断。

请求：

```json
{
  "requestId": "R-037",
  "securityIds": [
    "250001.IB"
  ],
  "minAttributeVersion": "18"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-037",
  "Data": {
    "attributes": [
      {
        "securityId": "250001.IB",
        "securityName": "示例国债",
        "issuerId": "IS-001",
        "issuerType": "GOVERNMENT",
        "bondRating": "AAA",
        "issuerRating": null,
        "province": null,
        "valuationDate": "2026-09-14",
        "valuationSource": "CHINABOND",
        "valuationYield": "2.4800"
      }
    ],
    "missingIds": [],
    "attributeVersion": "18"
  }
}
```

### H38 读取模型的筛选字段目录（待实现）

服务：`SearchSvr`；topic：`ficc.search.fields.query`；handler：`SearchFieldsQueryHandler.handle`。

collection必填；返回field/type/operators及枚举或单位。前端通过对应业务服务dictionary接口获取权限裁剪结果，不直连SearchSvr。

请求：

```json
{
  "requestId": "R-038",
  "collection": "QUOTE"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-038",
  "Data": {
    "fields": [
      {
        "field": "bondRating",
        "type": "string",
        "operators": [
          "EQ",
          "IN"
        ],
        "values": [
          "AAA",
          "AA+"
        ]
      },
      {
        "field": "price",
        "type": "decimal",
        "operators": [
          "EQ",
          "GT",
          "GTE",
          "LT",
          "LTE",
          "BETWEEN"
        ],
        "unit": "由quoteType决定"
      }
    ]
  }
}
```

### H39 查询索引及持久化处理位置（待实现）

服务：`SearchSvr`；topic：`ficc.search.status.get`；handler：`SearchStatusGetHandler.handle`。

仅服务运维或源服务；无业务参数。appliedPositions是可查询位置，persistedPositions是已落搜索库位置，不得混用；异步落库位置只在成功提交后推进。

请求：

```json
{
  "requestId": "R-039"
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-039",
  "Data": {
    "ready": true,
    "appliedPositions": {
      "quote": {
        "epoch": "QEPOCH-1",
        "sequence": "20"
      },
      "price": {
        "streamId": "MDE-001",
        "sequence": "30"
      }
    },
    "persistedPositions": {
      "quote": {
        "epoch": "QEPOCH-1",
        "sequence": "18"
      },
      "price": {
        "streamId": "MDE-001",
        "sequence": "30"
      }
    },
    "attributeVersion": "18"
  }
}
```

### H40 自然语言识别为候选报价（待实现）

服务：`ParseSvr`；topic：`ficc.quote.recognize`；handler：`QuoteRecognizeHandler.handle`。

text必填，默认最大20000字符；sourceMessageId与context可选。只返回候选，不生成正式quoteId，不写QuoteSvr；客户端校正后调用quote.create。不能识别的字段返回issues，不用估值替用户输入价格。

请求：

```json
{
  "requestId": "R-040",
  "text": "示例国债买入2.50，1000万，今+1",
  "context": {
    "tradeDate": "2026-09-14"
  }
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-040",
  "Data": {
    "quotes": [
      {
        "securityId": "250001.IB",
        "side": "1",
        "quoteType": "1",
        "yieldType": "1",
        "price": "2.5000",
        "quantity": "1000",
        "quantityUnit": "WAN",
        "currency": "CNY",
        "tradeDate": "2026-09-14",
        "settlType": "2",
        "candidateId": "PC-001"
      }
    ],
    "issues": [],
    "referenceVersion": "18"
  }
}
```

### H41 QT／IDEAL统一来源接入

服务：`QuoteSvr`；topic：`ficc.quote.intake.apply`；handler：`QuoteIntakeApplyHandler.handle`。

仅对应intake服务可调用；sourceType、externalMessageId、sourceTime、text和quotes必填。按(sourceType,externalMessageId)去重原文，按(sourceType,externalQuoteId)关联Quote；首次创建PENDING_CONFIRM。现有人工确认报价与来源更新冲突见评审R4，不允许直接覆盖。sourceUserId须经服务器映射到ownerUserId，映射失败逐项报错。

请求：

```json
{
  "requestId": "R-041",
  "sourceType": "QT",
  "externalMessageId": "QT-M-9001",
  "sourceTime": "2026-09-14T08:59:00+08:00",
  "sourceUserId": "QT-U-88",
  "text": "示例国债买入2.50，1000万，今+1",
  "quotes": [
    {
      "externalQuoteId": "QT-Q-81",
      "externalRevision": "1",
      "action": "NEW",
      "securityId": "250001.IB",
      "institutionId": "I-001",
      "contactId": "C-001",
      "side": "1",
      "quoteType": "1",
      "yieldType": "1",
      "price": "2.5000",
      "quantity": "1000",
      "quantityUnit": "WAN",
      "currency": "CNY",
      "tradeDate": "2026-09-14",
      "settlType": "2"
    }
  ]
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-041",
  "Data": {
    "sourceMessageId": "SM-001",
    "results": [
      {
        "externalQuoteId": "QT-Q-81",
        "quoteId": "Q-010",
        "revision": "1",
        "Code": "0000",
        "Msg": "成功"
      }
    ]
  }
}
```

### H42 按ID批量读取统一基础资料与CRM记录

服务：`QuoteSvr`；topic：`ficc.base.get`；handler：`BaseGetHandler.handle`；服务键：`SERVER.QuoteSvr`。

kind及ids必填。kind支持SECURITY、ISSUER、DAILY_VALUATION、INSTITUTION、CONTACT；所有ID均为字符串。SECURITY按精确securityId读取，适合报价列表补充债券名称等资料；不要在登录阶段使用全市场模糊搜索。DAILY_VALUATION按securityId读取当前适配器中的最新日估值，当前未实现请求参数valuationSource筛选。

债券资料示例（records仅列常用字段，实际字段取决于适配器）：

```json
{"requestId":"R-042","kind":"SECURITY","ids":["250001.IB"]}
```

```json
{"Code":"0000","Msg":"成功","requestId":"R-042","Data":{"records":[{"securityId":"250001.IB","securityName":"示例国债","issuerId":"IS-001","bondRating":"AAA"}],"missingIds":[],"referenceVersion":"STC-reference-version"}}
```

CRM按ID回填示例：

```json
{"requestId":"R-042-C","kind":"CONTACT","ids":["C-001","C-MISSING"]}
```

```json
{"Code":"0000","Msg":"成功","requestId":"R-042-C","Data":{"records":[{"contactId":"C-001","contactName":"张三","positionName":"交易员","institutionIds":["I-001"],"institutionId":"I-001"}],"missingIds":["C-MISSING"]}}
```

INSTITUTION／CONTACT每次最多200个ID；CRM结果只有records和missingIds，不返回referenceVersion。联系人可能关联多个机构：institutionIds为全部当前有效机构，只有恰好一个关联时institutionId才有值，否则为null；不能把多机构联系人自动归到第一个机构。INSTITUTION返回institutionId、institutionName、institutionShortName。记录顺序不保证与ids一致，客户端按ID关联。示例ID与姓名仅说明结构，调用时替换为目标CRM真实值。

### H43 基础资料联想与CRM包含搜索

服务：`QuoteSvr`；topic：`ficc.base.query`；handler：`BaseQueryHandler.handle`；服务键：`SERVER.QuoteSvr`。

kind必填；text可省略，默认空字符串。SECURITY查询由基础资料适配器执行，可能因搜索时间预算超限返回1009；已知完整securityId时优先H42。SECURITY／ISSUER查询返回records、totalCount、nextCursor；续取使用requestId和cursor，不能套用CRM的offset协议。

```json
{"requestId":"R-043-B","kind":"SECURITY","text":"250001","limit":20}
```

```json
{"Code":"0000","Msg":"成功","requestId":"R-043-B","Data":{"records":[{"securityId":"250001.IB","securityName":"示例国债"}],"totalCount":1,"nextCursor":null}}
```

CRM查询使用以下规则：

- kind=INSTITUTION／CONTACT；text执行不区分大小写的包含搜索，不是仅前缀匹配，当前没有matchMode参数。空text用于浏览列表。
- 机构搜索匹配ID、全称、简称及全称／简称拼音；联系人搜索匹配ID、姓名、CFETS交易员ID和姓名。
- CONTACT可传institutionId限定当前关联机构；省略时跨机构搜索。更换机构或搜索文本后offset重新置0。
- offset默认0，范围0—1000000；limit默认200，范围1—200，界面建议一次50条。
- 返回records、hasMore、nextOffset；不返回totalCount、nextCursor或referenceVersion。有下一批时保留条件并传nextOffset，否则停止加载。这是实时目录查询，不保证跨请求数据库快照一致。

搜索机构：

```json
{"requestId":"R-043-I","kind":"INSTITUTION","text":"银行","offset":0,"limit":50}
```

```json
{"Code":"0000","Msg":"成功","requestId":"R-043-I","Data":{"records":[{"institutionId":"I-001","institutionName":"示例银行股份有限公司","institutionShortName":"示例银行"}],"hasMore":false,"nextOffset":null}}
```

搜索所选机构的联系人：

```json
{"requestId":"R-043-C","kind":"CONTACT","institutionId":"I-001","text":"张","offset":0,"limit":50}
```

```json
{"Code":"0000","Msg":"成功","requestId":"R-043-C","Data":{"records":[{"contactId":"C-001","contactName":"张三","positionName":"交易员","institutionIds":["I-001"],"institutionId":"I-001"}],"hasMore":false,"nextOffset":null}}
```

待确认报价可以只保存institutionName／contactName。用户通过上述搜索选择机构、联系人后，在H02传匹配ID；服务校验关联并填写CRM标准名称，不自动按模糊名称选择ID。已填写的名称在确认前必须匹配ID。

```json
{"requestId":"R-CONFIRM-CRM","quotes":[{"quoteId":"Q-001","expectedRevision":"1","institutionId":"I-001","contactId":"C-001"}]}
```

```json
{"Code":"0000","Msg":"成功","requestId":"R-CONFIRM-CRM","Data":{"results":[{"quoteId":"Q-001","revision":"2","Code":"0000","Msg":"成功"}]}}
```

上述确认示例假设报价其他必填字段已录入。CRM不增加独立订阅topic，确认后的报价变化仍通过ficc.quote.report发送。

PostgreSQL模式读取独立CRM数据库的crm_org、crm_contact、crm_contact_org_rel。当前仅返回deleted=false记录，机构关联要求is_current=true且关联和机构均为deleted=false；尚未按owner_user_ids、部门或status增加权限过滤。CRM分支不依赖债券基础资料缓存已就绪；crm.mode=empty返回空目录，postgresql模式初始化或查询失败必须在服务日志中体现。

### H44 显式读取内部初始化记录

服务：`QuoteSvr`；topic：`ficc.quote.state.query`；handler：`QuoteStateQueryHandler.handle`。

仅内部消费者。首次请求cursor=null，由本次请求建立一致读取边界epoch/sequence；后续只用返回的nextCursor续取该边界的记录。无stateToken，不依赖订阅image；这是单独的数据加载请求，不改变事件订阅语义。done=true才完成初始化。游标过期返回1007并重新开始；块中未出现的集合不代表删除其他块记录。

请求：

```json
{
  "requestId": "R-044",
  "cursor": null,
  "limit": 500
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-044",
  "Data": {
    "epoch": "QEPOCH-1",
    "sequence": "20",
    "records": {
      "quotes": [
        {
          "quoteId": "Q-001",
          "clientQuoteId": "CQ-001",
          "sourceMessageId": null,
          "source": {
            "type": "MANUAL"
          },
          "ownerUserId": "U-001",
          "securityId": "250001.IB",
          "institutionId": "I-001",
          "contactId": "C-001",
          "revision": "2",
          "status": "ACTIVE",
          "side": "1",
          "quoteType": "1",
          "yieldType": "1",
          "price": "2.5000",
          "quantity": "1000",
          "quantityUnit": "WAN",
          "currency": "CNY",
          "tradeDate": "2026-09-14",
          "settlType": "2",
          "urgent": false,
          "recommended": false,
          "whole": false,
          "oco": false,
          "packing": false,
          "remark": null,
          "createdBy": "U-001",
          "updatedBy": "U-001",
          "confirmedBy": "U-001",
          "createTime": "2026-09-14T09:00:00+08:00",
          "updateTime": "2026-09-14T09:01:00+08:00",
          "quoteTime": "2026-09-14T09:01:00+08:00",
          "confirmTime": "2026-09-14T09:01:00+08:00"
        }
      ],
      "watchlists": [],
      "filterGroups": []
    },
    "nextCursor": null,
    "done": true
  }
}
```

### H45 按位置补取内部事件

服务：`QuoteSvr`；topic：`ficc.quote.event.query`；handler：`QuoteEventQueryHandler.handle`。

仅内部消费者；epoch、afterSequence必填，limit可选，返回严格大于该位置的事件。无缺口保留区间才返回0000，否则1007并重新调用对应state.query初始化。seenSequence是可用尾位置；空数组不代表可以跳过任何缺失位置。

请求：

```json
{
  "requestId": "R-045",
  "epoch": "QEPOCH-1",
  "afterSequence": "20",
  "limit": 500
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-045",
  "Data": {
    "epoch": "QEPOCH-1",
    "events": [],
    "nextSequence": "20",
    "seenSequence": "20",
    "hasMore": false
  }
}
```

### H46 显式读取内部初始化记录

服务：`NotificationSvr`；topic：`ficc.notification.state.query`；handler：`NotificationStateQueryHandler.handle`。

仅内部消费者。首次请求cursor=null，由本次请求建立一致读取边界epoch/sequence；后续只用返回的nextCursor续取该边界的记录。无stateToken，不依赖订阅image；这是单独的数据加载请求，不改变事件订阅语义。done=true才完成初始化。游标过期返回1007并重新开始；块中未出现的集合不代表删除其他块记录。

请求：

```json
{
  "requestId": "R-046",
  "cursor": null,
  "limit": 500
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-046",
  "Data": {
    "epoch": "NEPOCH-1",
    "sequence": "9",
    "records": {
      "notifications": [
        {
          "notificationId": "N-001",
          "userId": "U-001",
          "securityId": "250001.IB",
          "reasons": [
            {
              "type": "WATCHLIST",
              "groupIds": [
                "W-001"
              ],
              "text": "命中关注分组：常看国债"
            }
          ],
          "content": "关注债券有新报价",
          "quoteIds": [
            "Q-001"
          ],
          "readTime": null,
          "createTime": "2026-09-14T09:01:00+08:00",
          "updateTime": "2026-09-14T09:01:00+08:00"
        }
      ],
      "preferences": [],
      "filterGroups": []
    },
    "nextCursor": null,
    "done": true
  }
}
```

### H47 按位置补取内部事件

服务：`NotificationSvr`；topic：`ficc.notification.event.query`；handler：`NotificationEventQueryHandler.handle`。

仅内部消费者；epoch、afterSequence必填，limit可选，返回严格大于该位置的事件。无缺口保留区间才返回0000，否则1007并重新调用对应state.query初始化。seenSequence是可用尾位置；空数组不代表可以跳过任何缺失位置。

请求：

```json
{
  "requestId": "R-047",
  "epoch": "NEPOCH-1",
  "afterSequence": "9",
  "limit": 500
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-047",
  "Data": {
    "epoch": "NEPOCH-1",
    "events": [],
    "nextSequence": "9",
    "seenSequence": "9",
    "hasMore": false
  }
}
```

### H48 显式读取内部初始化记录（待实现）

服务：`BaseSvr适配模块`；topic：`ficc.base.state.query`；handler：`StateQueryHandler.handle`。

仅内部消费者。首次请求cursor=null，由本次请求建立一致读取边界epoch/sequence；后续只用返回的nextCursor续取该边界的记录。无stateToken，不依赖订阅image；这是单独的数据加载请求，不改变事件订阅语义。done=true才完成初始化。游标过期返回1007并重新开始；块中未出现的集合不代表删除其他块记录。

请求：

```json
{
  "requestId": "R-048",
  "cursor": null,
  "limit": 500
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-048",
  "Data": {
    "epoch": "BEPOCH-1",
    "sequence": "18",
    "records": {
      "securities": [
        {
          "securityId": "250001.IB",
          "securityName": "示例国债",
          "issuerId": "IS-001",
          "bondRating": "AAA"
        }
      ],
      "issuers": [],
      "valuations": []
    },
    "nextCursor": null,
    "done": true
  }
}
```

### H49 按位置补取内部事件（待实现）

服务：`BaseSvr适配模块`；topic：`ficc.base.event.query`；handler：`EventReplayQueryHandler.handle`。

仅内部消费者；epoch、afterSequence必填，limit可选，返回严格大于该位置的事件。无缺口保留区间才返回0000，否则1007并重新调用对应state.query初始化。seenSequence是可用尾位置；空数组不代表可以跳过任何缺失位置。

请求：

```json
{
  "requestId": "R-049",
  "epoch": "BEPOCH-1",
  "afterSequence": "18",
  "limit": 500
}
```

应答：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "R-049",
  "Data": {
    "epoch": "BEPOCH-1",
    "events": [],
    "nextSequence": "18",
    "seenSequence": "18",
    "hasMore": false
  }
}
```

### H55 服务间报价状态初始化／恢复

服务：`QuoteSvr`；topic：`ficc.quote.internal.state.query`；handler：`QuoteInternalStateQueryHandler.handle`。

NotificationSvr通过QuoteSvrClient和STC服务发现直接调用，不执行业务用户登录。仅接受requestId、limit、cursor。limit默认200、最大1000；后续请求只需新requestId和nextCursor。返回结构与H44一致，分页固定同一epoch／sequence，records包含quotes、watchlists、filterGroups；done=true表示读取完毕。

```json
{"requestId":"NS-STATE-1","limit":1000}
```

```json
{"Code":"0000","Msg":"成功","requestId":"NS-STATE-1","Data":{"epoch":"EP-001","sequence":"0","records":{"quotes":[],"watchlists":[],"filterGroups":[]},"totalCount":0,"nextCursor":null,"done":true}}
```

### H56 服务间完整行情初始化／恢复

服务：`QuoteSvr`；topic：`ficc.price.internal.snapshot.query`；handler：`PriceInternalSnapshotQueryHandler.handle`。

只接受requestId；返回当前完整内部行情SNAPSHOT，结构与S09一致，使用独立行情streamId／sequence，不提供cursor。下面以无报价状态说明外层结构，有报价时prices为S09的PriceResult数组。

```json
{"requestId":"NS-PRICE-1"}
```

```json
{"Code":"0000","Msg":"成功","requestId":"NS-PRICE-1","Data":{"Code":"0000","Msg":"成功","kind":"SNAPSHOT","streamId":"MDE-001","sequence":"0","quotePosition":{"epoch":"EP-001","sequence":"0"},"prices":[]}}
```

H55／H56只读，但当前实现跳过用户SID解析，依赖可信服务网络边界，不是已完成的服务身份认证。前端GW明确禁止调用；部署必须限制服务端口访问。H44／H45与H46／H47仍校验内部角色，不能当作普通前端接口或无登录调用的替代。NotificationSvr当前使用H55／H56恢复，不要求配置identity.serviceUsername。

## 9．订阅及发布接口

### S01 `ficc.quote.report.<sessionId>`

发布服务：QuoteSvr；发布组件：`QuoteReportPublisher`，普通subscribe，无业务snapshot Handler。

仅绑定用户与连接可订阅；只承载报价操作报告及QuoteView变化，绝不包含PriceResult、books或marketData。初始及翻页报价由quote.view.open/query应答提供；quote.query仍可独立搜索。quotes是本批完整报价对象并集，每个quoteId只发送一次；reports记录操作结果、操作人、时间、requestId和revision，不是字段差异。当前实现仅向报价所有者附带操作报告。失败操作通过请求应答返回，不冒充成功事件。

以下views格式为已实现的新协议：`upsertIds`为该view匹配的新建／更新报价ID，`removeIds`为删除或从匹配变为不匹配的ID；不发送place/index。upsertIds引用的完整报价必须在本批quotes中提供，同一view两个ID集合不能重叠。totalCount是该view全部匹配结果数，不是当前页行数。更新前不匹配、更新后匹配则upsert；前后都匹配也upsert；从匹配变为不匹配则remove。不能只过滤更新后的数据而漏发退出通知。

推送覆盖view过滤范围内的相关变化，不以当前页排名截断。是否显示在第一页由客户端排序决定；后续页忽略未加载ID的upsert，仅更新已加载行。报价因为本地裁剪退出第一页，不是removeIds语义。即使某报价不属于当前view，quotes也可能因其他view或操作报告包含它，客户端须按views处理成员关系。

发布示例：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "UPDATE",
  "streamId": "E-001",
  "sequence": "11",
  "quotes": [
    {
      "quoteId": "Q-001",
      "clientQuoteId": "CQ-001",
      "sourceMessageId": null,
      "source": {
        "type": "MANUAL"
      },
      "ownerUserId": "U-001",
      "securityId": "250001.IB",
      "institutionId": "I-001",
      "contactId": "C-001",
      "revision": "3",
      "status": "ACTIVE",
      "side": "1",
      "quoteType": "1",
      "yieldType": "1",
      "price": "2.4900",
      "quantity": "1000",
      "quantityUnit": "WAN",
      "currency": "CNY",
      "tradeDate": "2026-09-14",
      "settlType": "2",
      "urgent": false,
      "recommended": false,
      "whole": false,
      "oco": false,
      "packing": false,
      "remark": null,
      "createdBy": "U-001",
      "updatedBy": "U-001",
      "confirmedBy": "U-001",
      "createTime": "2026-09-14T09:00:00+08:00",
      "updateTime": "2026-09-14T09:03:00+08:00",
      "quoteTime": "2026-09-14T09:03:00+08:00",
      "confirmTime": "2026-09-14T09:01:00+08:00"
    }
  ],
  "reports": [
    {
      "eventId": "QE-021",
      "requestId": "R-WRITE-21",
      "clientQuoteId": "CQ-001",
      "quoteId": "Q-001",
      "revision": "3",
      "action": "UPDATE",
      "operatorUserId": "U-001",
      "operationTime": "2026-09-14T09:03:00+08:00"
    }
  ],
  "views": [
    {
      "viewId": "V-001",
      "totalCount": 1,
      "removeIds": [],
      "upsertIds": ["Q-001"]
    }
  ]
}
```

### S02 `ficc.notification.report.<sessionId>`

以下views示例采用已实现的upsertIds／removeIds，不含place/index。upsertIds引用本批完整notifications中的notificationId；removeIds表示该view不再匹配，不能据此推断通知全局删除。第一页按updateTime降序、notificationId升序排列并裁剪，移除已加载行后查询第一页补位；后续页只更新或移除已有行。匹配前后比较、总数及查询缓冲规则与Quote相同，使用独立Notification序列。

发布服务：NotificationSvr；发布组件：`NotificationReportPublisher`，普通subscribe，无业务snapshot Handler。

只承载本人的通知、偏好及通知view变化。初始列表使用notification.view.open/query，初始偏好使用preference.get。read操作可使通知退出UNREAD列表；退出不等于已读。preference变化发送完整对象，通知频道不携带报价或行情模型。

发布示例：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "UPDATE",
  "streamId": "NE-001",
  "sequence": "9",
  "notifications": [
    {
      "notificationId": "N-001",
      "userId": "U-001",
      "securityId": "250001.IB",
      "reasons": [
        {
          "type": "WATCHLIST",
          "groupIds": [
            "W-001"
          ],
          "text": "命中关注分组：常看国债"
        }
      ],
      "content": "关注债券有新报价",
      "quoteIds": [
        "Q-001"
      ],
      "readTime": "2026-09-14T09:02:00+08:00",
      "createTime": "2026-09-14T09:01:00+08:00",
      "updateTime": "2026-09-14T09:02:00+08:00"
    }
  ],
  "views": [
    {
      "viewId": "NV-001",
      "totalCount": 0,
      "removeIds": [
        "N-001"
      ],
      "upsertIds": []
    }
  ]
}
```

### S03 `ficc.price.book.<securityId>`

发布服务：QuoteSvr；`PriceBookSnapshotHandler.snapshot`提供初始image，后续由业务事件处理器发布。

STC兼容的原始整券行情topic，仅开放给被授权读取该完整FICC渠道的STC适配服务；前端有权限及条件过滤时走price.query/view，不直接订阅此原始topic。示例实际topic为ficc.price.book.250001.IB。每次NoMDEntries均完整替换该券前值，空簿必须发布空数组。此兼容流不承担可靠事件日志用途；丢失后重新subscribeWithImage，不能凭MDPriceLevel推断消息连续性。

初始image：

```json
{
  "SecurityID": "250001.IB",
  "Symbol": "示例国债",
  "MarketIndicator": "4",
  "VenueTypeGW": "FICC",
  "Venues": "FICC",
  "NoMDEntries": [
    {
      "MDEntryID": "FICC:Q-001",
      "MDEntryType": "0",
      "MDEntryPx": "2.5000",
      "MDEntrySize": "1000",
      "Yield": "2.5000",
      "PriceType": "3",
      "ExerciseFlag": "1",
      "SettlType": "2",
      "UserID": "U-001",
      "MDPriceLevel": "1",
      "MDEntryDate": "20260914",
      "MDEntryTime": "09:01:00.000"
    }
  ]
}
```

后续发布：

```json
{
  "SecurityID": "250001.IB",
  "Symbol": "示例国债",
  "MarketIndicator": "4",
  "VenueTypeGW": "FICC",
  "Venues": "FICC",
  "NoMDEntries": []
}
```

### S04 `ficc.quote.events`

发布服务：QuoteSvr；发布组件：`QuoteEventPublisher`，普通subscribe，无业务snapshot Handler。

内部报价事实事件，不自动返回已有报价、不返回初始化游标。消费者自行调用quote.state.query加载记录，再用quote.event.query补齐并衔接事件。type只含QUOTE、WATCHLIST、FILTER_GROUP；不含MARKET_ENTRY或价格快照。已有的QuoteEventHandler.snapshot设计撤销，纯发布不需要该Handler。

发布示例：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "EVENTS",
  "epoch": "QEPOCH-1",
  "fromSequence": "21",
  "toSequence": "21",
  "events": [
    {
      "sequence": "21",
      "eventId": "QEPOCH-1-EV-21",
      "type": "QUOTE",
      "action": "UPDATE",
      "quoteId": "Q-001",
      "quote": {
        "quoteId": "Q-001",
        "clientQuoteId": "CQ-001",
        "sourceMessageId": null,
        "source": {
          "type": "MANUAL"
        },
        "ownerUserId": "U-001",
        "securityId": "250001.IB",
        "institutionId": "I-001",
        "contactId": "C-001",
        "revision": "3",
        "status": "ACTIVE",
        "side": "1",
        "quoteType": "1",
        "yieldType": "1",
        "price": "2.4900",
        "quantity": "1000",
        "quantityUnit": "WAN",
        "currency": "CNY",
        "tradeDate": "2026-09-14",
        "settlType": "2",
        "urgent": false,
        "recommended": false,
        "whole": false,
        "oco": false,
        "packing": false,
        "remark": null,
        "createdBy": "U-001",
        "updatedBy": "U-001",
        "confirmedBy": "U-001",
        "createTime": "2026-09-14T09:00:00+08:00",
        "updateTime": "2026-09-14T09:03:00+08:00",
        "quoteTime": "2026-09-14T09:03:00+08:00",
        "confirmTime": "2026-09-14T09:01:00+08:00"
      },
      "operatorUserId": "U-001",
      "operationTime": "2026-09-14T09:03:00+08:00"
    }
  ]
}
```

### S05 `ficc.notification.events`

发布服务：NotificationSvr；发布组件：`NotificationEventPublisher`，普通subscribe，无业务snapshot Handler。

内部通知及偏好/通知分组变更；无初始image。初始化与缺口恢复分别调用notification.state.query和notification.event.query。

发布示例：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "EVENTS",
  "epoch": "NEPOCH-1",
  "fromSequence": "9",
  "toSequence": "9",
  "events": [
    {
      "sequence": "9",
      "eventId": "NEPOCH-1-EV-9",
      "type": "NOTIFICATION",
      "action": "UPSERT",
      "notification": {
        "notificationId": "N-001",
        "userId": "U-001",
        "securityId": "250001.IB",
        "reasons": [
          {
            "type": "WATCHLIST",
            "groupIds": [
              "W-001"
            ],
            "text": "命中关注分组：常看国债"
          }
        ],
        "content": "关注债券有新报价",
        "quoteIds": [
          "Q-001"
        ],
        "readTime": null,
        "createTime": "2026-09-14T09:01:00+08:00",
        "updateTime": "2026-09-14T09:01:00+08:00"
      }
    }
  ]
}
```

### S06 `ficc.base.events`（待实现）

发布服务：BaseSvr适配模块；发布组件：`ReferenceEventPublisher`，普通subscribe，无业务snapshot Handler。

统一基础资料变更；无初始image。初始化及补事件使用base.state.query和base.event.query，供应商DTO不进入本topic。

发布示例：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "EVENTS",
  "epoch": "BEPOCH-1",
  "fromSequence": "18",
  "toSequence": "18",
  "events": [
    {
      "sequence": "18",
      "eventId": "BEPOCH-1-EV-18",
      "type": "SECURITY",
      "action": "UPSERT",
      "record": {
        "securityId": "250001.IB",
        "securityName": "示例国债",
        "issuerId": "IS-001",
        "bondRating": "AAA"
      }
    }
  ]
}
```

### S07 `ficc.search.attributes.events`（待实现）

发布服务：SearchSvr；发布组件：`SearchAttributesPublisher`，普通subscribe，无业务snapshot Handler。

仅广播属性失效。首次订阅或重连时消费者主动清空旧缓存并读取search.status.get/attributes.get，不等待image。INVALIDATE列出影响securityIds；无法列全时发布INVALIDATE_ALL。发布版本必须已能由attributes.get读取；定期HEARTBEAT携带当前attributeVersion，检测漏掉的尾部变化。

发布示例：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "INVALIDATE",
  "streamId": "AE-001",
  "sequence": "2",
  "attributeVersion": "19",
  "securityIds": [
    "250001.IB"
  ]
}
```

### S08 `ficc.price.report.<sessionId>`

发布服务：QuoteSvr；`PriceReportSnapshotHandler.snapshot`提供Price初始image；`PriceReportPublisher`发布后续整券刷新。此频道只允许PriceStreamBatch DTO，不允许QuoteStreamBatch、quotes或报价操作reports。

通过subscribeWithImage建立订阅。首次尚未open任何view时返回views=[]、books=[]；以下为已实现的新协议，展示已有view重订阅时的单页快照。服务仅记住各view最后一次成功查询的pageNumber/pageSize，重订阅时重新查询该页，不恢复累计前缀，也不持续维护行位置。客户端丢弃旧相邻页缓存；若用户已选择其他页，再调用price.view.query。每条books以viewId＋securityId为键，表示该view条件下该券的完整PriceResult；同券不同条件的结果不混在一起。

UPDATE使用upsertIds／removeIds，不含place/index。upsertIds引用本批该view下的完整books；成员变化以securityId为单位，不能将单条报价退出等同于整券退出。books.updateTime用于债券行排序，券内价格档及MDPriceLevel保持后端生成。行情缓存、序列、重连和退订与Quote频道独立。

初始image／重订阅快照：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "SNAPSHOT",
  "streamId": "PE-001",
  "sequence": "10",
  "books": [
    {
      "viewId": "PV-001",
      "securityId": "250001.IB",
      "bookRevision": "7",
      "marketData": {
        "SecurityID": "250001.IB",
        "Symbol": "示例国债",
        "MarketIndicator": "4",
        "VenueTypeGW": "FICC",
        "Venues": "FICC",
        "NoMDEntries": [
          {
            "MDEntryID": "FICC:Q-001",
            "MDEntryType": "0",
            "MDEntryPx": "2.5000",
            "MDEntrySize": "1000",
            "Yield": "2.5000",
            "PriceType": "3",
            "ExerciseFlag": "1",
            "SettlType": "2",
            "UserID": "U-001",
            "MDPriceLevel": "1",
            "MDEntryDate": "20260914",
            "MDEntryTime": "09:01:00.000"
          }
        ]
      },
      "quoteRefs": {
        "FICC:Q-001": {
          "quoteId": "Q-001",
          "quoteRevision": "2"
        }
      },
      "updateTime": "2026-09-14T09:01:00+08:00"
    }
  ],
  "views": [
    {
      "viewId": "PV-001",
      "totalCount": 1,
      "ids": [
        "250001.IB"
      ],
      "pageSize": 200,
      "pageNumber": 1
    }
  ]
}
```

后续发布（UPDATE表示批次发生变化，其中每券仍为完整快照）：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "UPDATE",
  "streamId": "PE-001",
  "sequence": "11",
  "books": [
    {
      "viewId": "PV-001",
      "securityId": "250001.IB",
      "bookRevision": "8",
      "marketData": {
        "SecurityID": "250001.IB",
        "Symbol": "示例国债",
        "MarketIndicator": "4",
        "VenueTypeGW": "FICC",
        "Venues": "FICC",
        "NoMDEntries": [
          {
            "MDEntryID": "FICC:Q-001",
            "MDEntryType": "0",
            "MDEntryPx": "2.4900",
            "MDEntrySize": "1000",
            "Yield": "2.4900",
            "PriceType": "3",
            "ExerciseFlag": "1",
            "SettlType": "2",
            "UserID": "U-001",
            "MDPriceLevel": "1",
            "MDEntryDate": "20260914",
            "MDEntryTime": "09:03:00.000"
          }
        ]
      },
      "quoteRefs": {
        "FICC:Q-001": {
          "quoteId": "Q-001",
          "quoteRevision": "3"
        }
      },
      "updateTime": "2026-09-14T09:03:00+08:00"
    }
  ],
  "views": [
    {
      "viewId": "PV-001",
      "totalCount": 1,
      "removeIds": [],
      "upsertIds": [
        "250001.IB"
      ]
    }
  ]
}
```

空簿用NoMDEntries=[]清除原条目；券退出筛选时由views.removeIds移除，本地因pageSize裁剪不属于服务端移除。无其他引用后回收对应缓存。多Price面板的不同过滤结果暂不承诺跨面板去重；不影响Quote频道的同批报价去重。

### S09 `ficc.price.snapshots`

发布服务：QuoteSvr；`PriceFeedSnapshotHandler.snapshot`提供内部全渠道当前行情，`PriceFeedPublisher`发布后续整券快照。仅SearchSvr、NotificationSvr等受信服务使用；前端无权限直接订阅。

使用subscribeWithImage；prices为完整PriceResult数组，不含Quote事实事件。初始image结束后以其券集合替换旧行情集合；之后按securityId替换变化券，空NoMDEntries必须清空该券。quotePosition表示整个行情状态已处理到的报价事实事件位置，供跨频道关联；它不是行情sequence，报价与行情两个位置不互相比较数值大小。QuoteSvr须按确定顺序处理报价事件并保持该水位真实，计算未完成时不能宣称已追平。

初始image：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "SNAPSHOT",
  "streamId": "MDE-001",
  "sequence": "30",
  "quotePosition": {
    "epoch": "QEPOCH-1",
    "sequence": "20"
  },
  "prices": [
    {
      "securityId": "250001.IB",
      "bookRevision": "7",
      "marketData": {
        "SecurityID": "250001.IB",
        "Symbol": "示例国债",
        "MarketIndicator": "4",
        "VenueTypeGW": "FICC",
        "Venues": "FICC",
        "NoMDEntries": [
          {
            "MDEntryID": "FICC:Q-001",
            "MDEntryType": "0",
            "MDEntryPx": "2.5000",
            "MDEntrySize": "1000",
            "Yield": "2.5000",
            "PriceType": "3",
            "ExerciseFlag": "1",
            "SettlType": "2",
            "UserID": "U-001",
            "MDPriceLevel": "1",
            "MDEntryDate": "20260914",
            "MDEntryTime": "09:01:00.000"
          }
        ]
      },
      "quoteRefs": {
        "FICC:Q-001": {
          "quoteId": "Q-001",
          "quoteRevision": "2"
        }
      }
    }
  ]
}
```

后续发布：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "kind": "UPDATE",
  "streamId": "MDE-001",
  "sequence": "31",
  "quotePosition": {
    "epoch": "QEPOCH-1",
    "sequence": "21"
  },
  "prices": [
    {
      "securityId": "250001.IB",
      "bookRevision": "8",
      "marketData": {
        "SecurityID": "250001.IB",
        "Symbol": "示例国债",
        "MarketIndicator": "4",
        "VenueTypeGW": "FICC",
        "Venues": "FICC",
        "NoMDEntries": [
          {
            "MDEntryID": "FICC:Q-001",
            "MDEntryType": "0",
            "MDEntryPx": "2.4900",
            "MDEntrySize": "1000",
            "Yield": "2.4900",
            "PriceType": "3",
            "ExerciseFlag": "1",
            "SettlType": "2",
            "UserID": "U-001",
            "MDPriceLevel": "1",
            "MDEntryDate": "20260914",
            "MDEntryTime": "09:03:00.000"
          }
        ]
      },
      "quoteRefs": {
        "FICC:Q-001": {
          "quoteId": "Q-001",
          "quoteRevision": "3"
        }
      }
    }
  ]
}
```

价格簿是可重建的当前状态，此流不承担Quote历史或报价事件重放。丢号、换streamId或重启时重新取Price image，SearchSvr重建当前行情索引及其持久化副本；Quote历史仍由quote.events维护。单条NoMDEntries缺失与空数组含义不同，禁止省略数组表示删除。

全渠道image可能超过单条消息上限；必须实现有开始/结束边界的分块image后才能用于大市场，不能截断或将未完成image当成功。完整分块线格式仍列为R6评审项，本例只示例一个债券。原生STC适配仍使用S03，不将本包装冒充共享FIX模型。

## 10．独立频道的初始化、更新与恢复

### 10.1 Quote频道：普通订阅与显式查询

本节及10.2为已实现的分页及客户端排序协议，取代旧版extend／place流程。

```mermaid
sequenceDiagram
    participant C as Client
    participant Q as QuoteSvr报价模块
    C->>Q: ficc.quote.stream.open（request/reply）
    Q-->>C: quote pushTopic、streamId
    C->>Q: DPS subscribe(quote pushTopic)
    Note over C: 缓冲本流推送；HEARTBEAT不含已有报价
    C->>Q: ficc.quote.view.open(filter, viewId, pageSize)
    Note over Q: 在边界S登记条件并读取报价列表
    Q-->>C: QuoteViewReply(quotes, view, sequence=S)
    Q-->>C: QuoteStreamBatch(quote changes, sequence>S)
    C->>Q: ficc.quote.view.query(viewId, pageNumber, pageSize)
    Q-->>C: 指定页quotes、ids、totalCount、sequence
    Note over C: 第一页本地排序；后续页保留阅读顺序
    C->>Q: ficc.quote.view.close
```

先建立DPS订阅并开始缓冲，再调用view.open；当前服务不要求等待首个HEARTBEAT才建立view。HEARTBEAT用于检查流存活及序号，不是报价snapshot，也不能替代订阅成功确认。报价操作应答与quote.get可用于核实结果；断线后先恢复连接。心跳周期及超时判断以部署配置和客户端实现为准。

客户端发Quote view.open/query前按viewId缓冲消息。首次查询与登记必须在同一边界S完成，不能先查询再无边界地开始推送；后续页查询复用订阅，并返回本次读取边界。SearchSvr落后时应追平或用保留的报价事件补齐；不能让查询结果和后续事件中间出现空档。

### 10.2 QuoteView变化与恢复

- QuoteStreamBatch仅包含quotes、reports、views及可选settingsChanged。每批quoteId只出现一次；多个QuoteView引用同一对象。不能出现books或MarketDataSnapshotFullRefresh。
- UPDATE先按revision接收完整Quote对象，同revision显示字段刷新按流序列处理；再对目标view执行removeIds和upsertIds。第一页按quoteId插入或替换，按`updateTime DESC, quoteId ASC`排序，只保留pageSize条。后端不发送位置，也不因挤出第一页而发removeIds。
- 第一页因removeIds移除已加载行时，合并调用view.query(pageNumber=1)补位；移除未加载ID不触发。查询本身不足pageSize直接接受，不循环补位。查询期间再次发生移除，接续缓冲后检查是否仍需补位。普通更新或新报价插入后裁剪尾行，不触发补位查询。
- 后续页只更新已经加载的upsertIds，保持阅读顺序；忽略未加载的upsertIds。removeIds仍移除对应行，但不自动补位。翻页、刷新或返回第一页时重新查询，接受跨页重复或遗漏的取舍。
- view.open/query应答只替换目标页的列表，以sequence=S为读取边界；较新的>S对象不能被旧应答覆盖。按序接续该view缓冲的>S变化；应答不倒退整个Quote流位置。每个view串行查询，以requestId防止旧应答覆盖较新的页选择或筛选。
- 每个Quote流有独立streamId和严格递增sequence；相同代次重复序号丢弃。缺号或换代次时停止应用不连续变化，重新调用quote.stream.open并确认订阅有效，关闭仍存在的旧view，以新viewId逐个open重读列表。不存在的旧view可幂等关闭；不能用subscribeWithImage恢复Quote。
- 恢复时只保留本地未提交编辑和请求关联，旧服务器缓存暂不作为最新状态显示。已经提交但结果不明的操作按原requestId重试，或quote.get/history.query核实；重新查询列表不保证重放错过的操作报告。
- 某Quote退出一个view不代表事实被删除，也不代表可以清掉其他view仍引用的对象。DELETED报告可用于操作反馈；当前有效列表移除该记录。没有view或编辑引用时才回收缓存。
- 关闭后的viewId在该领域连接内不复用；忽略其在途成员变化。改变条件采用新viewId，避免旧查询覆盖新结果。服务重启不要求恢复临时view，持久化分组仍保留。

Quote心跳示例（无quotes数组，无初始报价集合）：

```json
{"Code":"0000","Msg":"成功","kind":"HEARTBEAT","streamId":"E-001","sequence":"12"}
```

### 10.3 Price频道：价格簿快照

```mermaid
sequenceDiagram
    participant C as Client
    participant P as QuoteSvr行情模块
    C->>P: ficc.price.stream.open（request/reply）
    P-->>C: price pushTopic、独立streamId
    C->>P: DPS subscribeWithImage(price pushTopic)
    P-->>C: Price SNAPSHOT（首次无view时为空）
    C->>P: ficc.price.view.open(filter, mode, viewId, pageSize)
    P-->>C: PriceViewReply(books, view, sequence=S)
    P-->>C: PriceStreamBatch(变化券的完整快照, sequence>S)
    C->>P: ficc.price.view.query(viewId, pageNumber, pageSize)
    P-->>C: 指定页books、ids、totalCount、sequence
    C->>P: ficc.price.view.close
```

PriceView每行是一只债券，ids为securityId；books以viewId＋securityId为键。pageSize是债券行数，mode/depth决定每券价格档：FULL全部、BEST每侧最佳档、DEPTH每侧N个不同价格档；同价档成员不能按条数截掉。

PriceViewReply只含Price领域数据；PriceStreamBatch也不含quotes、QuoteReport或报价操作确认。价格条目使用STC MarketDataSnapshotFullRefresh/GNoMDEntries，quoteRefs用于关联Quote身份及版本，不把Quote事实DTO嵌入行情。不同view的过滤结果可以不同，独立缓存；内部可共享计算与条目对象，但线上契约分开。

Price UPDATE携带变化券的完整簿，客户端替换该view下该券前值；按removeIds／upsertIds维护列表。第一页按books.updateTime降序、securityId升序排列并裁剪，移除已加载行或其排序时间下降时合并query补位；后续页只更新和移除已有行，不插入、不重排、不补位。Price SNAPSHOT替换本Price频道各view最后查询页及其books，不恢复旧相邻页缓存；重新安装后只接续边界之后连续更新。image与在途发布交错时先缓冲；view.open/query同样使用回复边界衔接，不覆盖其他view或较新数据。

Price流独立sequence，不与Quote sequence合并；连接缺号重订阅Price image即可。服务重启后image若views=[]，客户端按本地行情条件重新price.view.open。Price流也周期发布独立HEARTBEAT以发现尾部缺口，心跳不改变任何价格簿。空NoMDEntries表示清空该券；券离开当前筛选范围另由view成员移除。完整image分块规则未完成时，不允许截断后宣称恢复成功，见R6。

### 10.4 内部事件与价格数据分别恢复

**当前NotificationSvr路径**：普通订阅ficc.quote.events和ficc.price.snapshots并缓冲；用H55读完报价状态，再用H56读取完整价格快照，然后接续缓冲事件。断线、缺号或缓冲溢出时重新执行这两个读取，不调用用户登录，也不通过H45补事件。实现见NotificationResource.restore／tick。

以下表格同时保留后续SearchSvr的事件补取设计；SearchSvr及基础资料事件流尚未实现，不应把该恢复方案当成当前NotificationSvr调用链。

| 频道 | 数据 | 初始化／恢复 | 消费者 |
| --- | --- | --- | --- |
| ficc.quote.events | QUOTE、WATCHLIST、FILTER_GROUP事件；绝无MARKET_ENTRY | NotificationSvr用H55加载；规划SearchSvr用H44／H45 | NotificationSvr；规划SearchSvr |
| ficc.price.snapshots | PriceResult当前整券快照；独立行情流 | NotificationSvr普通订阅并用H56恢复；S09也提供image入口 | NotificationSvr；规划SearchSvr |
| ficc.notification.events | NOTIFICATION、PREFERENCE、FILTER_GROUP事件 | 普通subscribe；notification.state.query、notification.event.query | SearchSvr |
| ficc.base.events | SECURITY、ISSUER、INSTITUTION、CONTACT、DAILY_VALUATION事件 | 普通subscribe；base.state.query、base.event.query | SearchSvr及授权资料缓存 |

事件流初始化：先普通subscribe并缓冲，再显式state.query取得边界S和记录；通过nextCursor读完同一边界的数据，然后event.query补齐>S事件，去重并接续现场消息。订阅过程中可能漏掉的消息由事件日志覆盖，不能只假设subscribe已经生效。done=true及追平前不对外宣布ready。断点仍在日志保留范围内时直接补事件；1007时重新显式加载。上述读取一致性是存储实现保证，不是给事件订阅增加snapshot。

事件消费者以epoch＋sequence去重，fromSequence至toSequence必须连续；不关心的type也推进位置。事件心跳只报告currentSequence，不产生虚构持久化记录；消费者发现尾部落后也需补取。Quote事实、审计及对应事件原子提交。日志和读取边界由FICC实现，不假设DPS自动可靠重放；同一逻辑事件流需要统一的序列分配者，副本不能独立从1计数。

SearchSvr分别保存报价、通知、基础资料的已应用与已持久化事件位置；重启从已落库位置补事件。价格来源单独保存streamId/sequence及当前整券状态，失效后使用S09重建行情索引，再异步覆盖其搜索存储副本。S09的NoMDEntries转为内部MarketDataEntry索引时，按整券替换，删除该券不再出现的旧条目；不会把同券报价历史覆盖成一个价格快照。

Quote与Price频道无跨频道到达顺序保证。NotificationSvr分别维护报价状态与行情状态；以S09的quotePosition及条目quoteRefs确认已对齐的报价版本后评估我的最优。价格计算未就绪时不使用旧簿冒充最新最优。对价提醒依据同券、反方向、本人/他人的有效Quote关系，不要求价格交叉、同清算或只看最佳档；不因价格计算延迟漏掉这些关系。通知结果及对应事件原子提交。

SearchSvr属性失效流使用普通subscribe；建立或重连时主动清缓存并读当前attributeVersion，后续失效/心跳通知发现落后则刷新。attributeVersion为持久化递增资料版本，不能被旧查询响应覆盖；minAttributeVersion要求尚未满足时返回1009。相关资料变化后必须重评view成员与显示数据，不能只改文本。不同模型仍复用FilterEngine代码，不复用业务频道。

### 10.5 个人设置及资源释放

Watchlist/FilterGroup变更用settingsChanged通知该用户的在线频道；只开Price频道的行情中心也必须能收到。可在QuoteStreamBatch或PriceStreamBatch中携带同一设置失效元数据，但不能把另一领域的业务数据混入。字段type=WATCHLIST/FILTER_GROUP、action=UPSERT/REMOVE、id、revision；收到后重查对应分组query。首次进入及恢复时主动查询分组，查询期间缓冲失效通知。分组改变引起的匹配结果分别由QuoteView与PriceView计算和推送。

```json
{"Code":"0000","Msg":"成功","kind":"UPDATE","streamId":"PE-001","sequence":"13","books":[],"views":[],"settingsChanged":[{"type":"WATCHLIST","action":"UPSERT","id":"W-001","revision":"2"}]}
```

NotificationPreference仅走Notification频道，变化发送完整preference；初始/重连通过preference.get读取，并按revision防止旧应答覆盖已收到的新值。NotificationView普通订阅和显式分页加载沿用Quote的流程，调用notification.view.open/query，不使用image；列表以notificationId为键，按updateTime降序、notificationId升序排列第一页。读通知导致退出UNREAD时remove并按需补位；ALL列表中仍匹配则upsert。断线或缺号后重建通知view并查询，使用独立DTO、topic及sequence，不能用Quote流的边界去重。

各领域的view.close只释放对应view。所有view关闭后是否保留Quote操作报告订阅由客户端决定；注销时客户端应分别关闭view并退订Quote、Price、Notification。当前领域实现没有可配置TTL回收承诺，断线遗留流的自动清理仍需完善；不能仅凭无view立即停掉仍需报价操作报告的流。退订适配的源码问题见第2.1节。

## 11．字段补充、校验和错误例子

### 11.1 最小DSL（SearchSvr待实现设计）

SearchSvr where支持$and/$or，字段操作$eq/$in/$gt/$gte/$lt/$lte/$exists。客户端EQ/IN等由业务服务转换；BETWEEN转换为同一字段的闭区间$gte和$lte，不能因为跨条件拍平而变成上下界OR。区间条件本身是一个谓词，同字段多个完整谓词之间OR。空where表示授权范围内全部。

```json
{"$and":[{"issuerType":{"$eq":"BANK"}},{"remainingYears":{"$gte":"1","$lte":"3"}}]}
```

字段值必须匹配目录类型；decimal用十进制数值比较，禁止字符串字典序。未知字段、未知操作、$where/脚本、任意正则、跨集合JOIN均报1001/1010。敏感字段即使只用于过滤也需授权，不能允许借筛选探测不可见资料。客户端filter不直接透传为数据库SQL。

索引记录由源数据与当前参考属性组装。SearchSvr回复positions＋attributeVersion固定了初始结果依据；SearchSvrClient不得用混合资料版本静默接续。无法保留一致快照时返回1009或重新建view，不返回看似成功的部分结果。

### 11.2 请求字段通则

- 每项示例请求所示业务字段按该接口说明判定必填/可选；不接受未列出的写入字段。cursor查询续取只需requestId、cursor，不重复传条件；CRM搜索使用offset，续取时必须保留kind、text和institutionId等条件。
- Quote可编辑字段为securityId、institutionId、institutionName、contactId、contactName、side、quoteType、yieldType、price、quantity、quantityUnit、currency、tradeDate、settlType、五个标记及remark；来源信息及审计字段由服务端维护。
- 分组查询返回userId、revision、createTime、updateTime；示例省略的时间字段由服务端实际生成。成员及子组displayOrder缺省按请求数组顺序生成；新子ID由服务端生成，更新保留已有ID。
- Reference.get输出records按kind使用对应模型；记录顺序不保证与ids相同，按ID关联。资料缺失在missingIds返回；不是业务不存在的Quote，也不应映射成价格0。
- DAILY_VALUATION至少包含valuationId、securityId、valuationDate、valuationSource、dataVendor、valuationBasis以及可用yield/cleanPrice/fullPrice。valuationSource提案CHINABOND/CSI，dataVendor提案SUMSCOPE/DM；两者互不替代。
- 来源原文记录最小模型为sourceMessageId、sourceType、externalMessageId、sourceTime、text、ownerUserId、quoteIds；Quote.source为sourceType对应来源及外部报价标识。原文去重与Quote匹配使用不同键，不以requestId当永久来源标识。
- QuoteIntake首版NEW创建待确认，UPDATE仅更新同来源的待确认报价且检查externalRevision较新。外部引用序号如何规范化由intake处理；对于ACTIVE/REFERRED报价的外部UPDATE/REFER/UNREFER/DELETE，在R4确认前返回1010并保留原文待处理，不自动覆盖人工操作。这是一项显式待评审限制。

### 11.3 版本冲突与部分失败

```json
{"Code":"0000","Msg":"批量处理完成","requestId":"R-CONFLICT","Data":{"results":[{"quoteId":"Q-001","Code":"1004","Msg":"报价已被更新，请重新读取","error":{"type":"REVISION_CONFLICT","quoteId":"Q-001","expectedRevision":"2","currentRevision":"3"}}]}}
```

```json
{"Code":"0000","Msg":"批量处理完成","requestId":"R-BATCH","Data":{"results":[{"clientQuoteId":"CQ-A","quoteId":"Q-101","revision":"1","Code":"0000","Msg":"成功"},{"clientQuoteId":"CQ-B","Code":"1001","Msg":"收益率最多4位小数","error":{"type":"VALIDATION_FAILED","field":"price"}}]}}
```

无权限时不返回当前报价版本或字段值。requestId相同的批量重试返回原完整results；只重试失败项时使用新的requestId。

### 11.4 当前限额与未实现的部署建议

| 项目 | 当前默认／上限或明确标注的建议 | 超限行为 |
| --- | --- | --- |
| 批量报价写入 | 100条 | 1008；不拆成不可见的多批事务 |
| 一次性query | 默认200、最大1000条 | 使用nextCursor续取 |
| price.query | 最大100券 | 显式缩小securityIds |
| 三类view初始及分页查询 | pageSize默认200、最大1000；pageNumber从1开始 | 每次只返回一页；pageSize超限报1008，非法页码报1001，超出结果范围返回空页 |
| view客户端缓存 | 当前一页；Quote／Price旧累计10000、Notification旧累计1000不再限制浏览总量 | 翻页替换缓存，不加载累计前缀 |
| 同会话、同领域的view数 | 20；Quote／Price／Notification分别计数 | 1008 |
| 已实现state/event批量 | 默认200、最大1000 | 按cursor或sequence继续 |
| CRM查询／按ID读取 | 查询默认200、最大200；get最多200个ID | 查询以offset续取；get拆批 |
| 消息字节上限／慢消费者 | 尚未形成统一字节限额契约；1 MiB仅为待验证建议 | 需完善显式拒绝或恢复机制，当前不承诺统一执行 |
| requestId记录保留 | 建议至少一个交易日并覆盖客户端重试窗口 | 窗口外先查询状态，不能声称无限幂等 |

除明确标注的部署建议外，表中限额来自当前实现。不能据此推断响应时间；字节限制及幂等保留期仍为设计建议。恢复快照需按记录大小控制块容量；某单券完整行情超出消息上限时需在开发前确定DPS分片方案，不能切掉同价成员。行情模式FULL的压力应逐步验证。

## 12．既有STC接入及Handler注册

### 12.1 外部服务与共享库

| 依赖 | 复用点 | 本版边界 |
| --- | --- | --- |
| LoginSvr | LoginSvrClient.get(sid)，SYS.ATS.LOGIN登录/登出/更新 | 不新建FICC登录、密码、会话主库；实际登录handler沿用STC部署 |
| CalSvr | CalSvrClient.calBond，stc.broker.calbond请求/应答，stc.cal.clear失效通知 | 待接入；当前不能据此承诺净价／全价自动换算及计算缓存失效处理 |
| STC BaseSvr | 现有BaseSvr2Client、stc.base.info.query等 | FICC BaseSvrClient对外返回统一模型；新ficc.base.*为适配入口，不宣称现有BaseSvr已支持 |
| STC MarketSvr | 既有外部行情订阅路径 | 继续由GW/现有适配接入，不自动纳入FICC最优比较 |
| QT／IDEAL | intake内部读取供应商消息 | 统一转为quote.intake.apply；不是前端订阅topic |

当前ficc.base.get/query注册在QuoteSvr。债券使用STC基础资料适配器，复用BaseSvrClient／BaseSvr2Client路径；CRM可选empty、simulator或postgresql模式，通过独立连接查询机构／联系人，不用债券发行人代替CRM机构。配置和启动日志见[STC适配说明](../iteration1/docs/stc-adapters.zh-CN.md)。

本规范不新增前端创建通知、修改行情条目、写SearchSvr任意集合、执行成交或管理STC用户的接口。SearchSvr数据写入来自受信发布者的领域事件及独立行情快照；其消费入口是订阅回调，不是另一个暴露给前端的save handler。

### 12.2 Handler注册与纯发布路由

```ini
[CedaServerResouce]
serverKey=SERVER.QuoteSvr
handlerList=QuoteGetHandler,QuoteViewOpenHandler,PriceViewOpenHandler,PriceBookSnapshotHandler,PriceReportSnapshotHandler,PriceFeedSnapshotHandler
handler.QuoteGetHandler.topic=ficc.quote.get
handler.QuoteGetHandler.class=ficc.quote.handler.QuoteGetHandler
handler.QuoteViewOpenHandler.topic=ficc.quote.view.open
handler.QuoteViewOpenHandler.class=ficc.quote.handler.QuoteViewOpenHandler
handler.PriceViewOpenHandler.topic=ficc.price.view.open
handler.PriceViewOpenHandler.class=ficc.quote.handler.PriceViewOpenHandler
handler.PriceBookSnapshotHandler.topic=ficc.price.book.**
handler.PriceBookSnapshotHandler.class=ficc.quote.handler.PriceBookSnapshotHandler
handler.PriceReportSnapshotHandler.topic=ficc.price.report.**
handler.PriceReportSnapshotHandler.class=ficc.quote.handler.PriceReportSnapshotHandler
handler.PriceFeedSnapshotHandler.topic=ficc.price.snapshots
handler.PriceFeedSnapshotHandler.class=ficc.quote.handler.PriceFeedSnapshotHandler
```

这是注册形式示例，不是完整可启动配置。第4节列56个请求接口（49个已注册、7个待实现）与9个订阅topic（7个已有实现、2个待实现）；3个Price订阅入口提供业务snapshot Handler，其余为纯发布设计。纯发布topic仍需要正确的DPS路由及ACL配置，但不为QuoteEventPublisher或QuoteReportPublisher注册虚构的snapshot方法。

普通Quote订阅与Price带image订阅、鉴权、连接关联及实际退订必须在目标STC/GW适配中分别验证。价格与报价的topic前缀互不覆盖；ficc.price.book.**不匹配ficc.price.query请求。

### 12.3 源码依据

本次实现核对：

- [QuoteSvr注册](../iteration1/cczq-app-salestrading-quotesvr/control/QuoteSvr/resourceCfg.ini)、[NotificationSvr注册](../iteration1/cczq-app-salestrading-notificationsvr/control/NotificationSvr/resourceCfg.ini)：实际topic与Handler名称。
- [QuoteService](../iteration1/cczq-app-salestrading-quotesvr/src/main/java/ficc/quote/QuoteService.java)、[ViewService](../iteration1/cczq-app-salestrading-quotesvr/src/main/java/ficc/quote/ViewService.java)：请求、会话流、view及序号。
- [PostgresCrmClient](../iteration1/cczq-app-salestrading-common/src/main/java/ficc/common/client/PostgresCrmClient.java)：CRM匹配字段、关联及offset分页。
- [NotificationResource](../iteration1/cczq-app-salestrading-notificationsvr/src/main/java/ficc/notification/NotificationResource.java)、[QuoteSvrClient](../iteration1/cczq-app-salestrading-common/src/main/java/ficc/common/client/QuoteSvrClient.java)：服务间订阅与恢复。
- [FiccSecurityCheck](../iteration1/gateway/src/ficc/gateway/FiccSecurityCheck.java)：GW私有topic授权与内部接口拦截。

既有STC设计依据：

- [ContentHandler](D:/workspace/GitHub/ats-app-ym-foundation/src/main/java/ats/app/ym/ceda/server/ContentHandler.java:60)：有内容进入handle，无内容进入snapshot。
- [InnerSvr QueryOrderHandler](D:/workspace/GitHub/sc-app-stc-innersvr/src/main/java/sc/app/stc/inner/handler/QueryOrderHandler.java:16)：Code/Msg式请求应答。
- [InnerSvr MarketHandler](D:/workspace/GitHub/sc-app-stc-innersvr/src/main/java/sc/app/stc/inner/handler/MarketHandler.java:34)：订阅初始image按topic返回行情。
- [InnerSvr sendAllMD](D:/workspace/GitHub/sc-app-stc-innersvr/src/main/java/sc/app/stc/inner/service/impl/OrderResource.java:221)：MarketDataSnapshotFullRefresh/NoMDEntries发布。
- [BaseApi普通订阅与退订](D:/workspace/GitHub/sc-app-stc/src/main/java/ats/app/api/BaseApi.java:270)：普通subscribe已实现，unSubscribe底层调用被注释；不能只使用带image订阅。
- [BaseApi行情订阅](D:/workspace/GitHub/sc-app-stc/src/main/java/ats/app/api/BaseApi.java:251)：subscribeWithImage与退订重载；带Class退订方法为空。
- [底层退订实现](D:/workspace/GitHub/ats-app-ym-foundation/src/main/java/ats/app/ym/ceda/CedaClientWrapper.java:59)：底层CedaClientWrapper的unSubscribe会调用连接客户端退订。
- [InnerSvr订单报告发布](D:/workspace/GitHub/sc-app-stc-innersvr/src/main/java/sc/app/stc/inner/service/impl/OrderResource.java:404)：ExecutionReport发布到stc.inner.o.*，与行情p.*分离。
- [InnerSvr市场代码](D:/workspace/GitHub/sc-app-stc-innersvr/src/main/java/sc/app/stc/inner/common/Consts.java:11)：MarketIndicator使用4。
- [共享行情模型](D:/workspace/GitHub/sc-app-stc/src/main/java/sc/app/stc/fix/message/MarketDataSnapshotFullRefresh.java:388)：YieldToExecution、DirtyPrice、UserID等实际已有字段。
- [LoginSvrClient](D:/workspace/GitHub/sc-app-stc/src/main/java/sc/app/stc/utils/LoginSvrClient.java:121)、[CalSvrClient](D:/workspace/GitHub/sc-app-stc/src/main/java/sc/app/stc/utils/CalSvrClient.java:47)：身份及计算服务复用。

STC源码证明通信形式、订单/行情分离及可复用模型，不证明新topic、恢复日志、权限校验或本稿业务语义已经实现。

## 13．最终评审项及实现边界

| 编号 | 需确认内容 | 影响接口／可先实现的部分 |
| --- | --- | --- |
| R1 | 历史报价按每日终态还是任意时点记录检索 | history.search及历史库入库规则；不影响单Quote的history.query |
| R2 | 确认时价格/报量是否可空、CalSvr计算输出的精度及失败处理 | 当前确认要求price／quantity等必要字段；CalSvr精度及失败处理仍待实现，不应将草稿可空等同于确认可空 |
| R3 | 明+0/明+1及远期的日期基准、存储字段、合法代码 | confirm/unrefer/历史显示；先实现已确认的T+0/T+1 |
| R4 | QT/IDEAL对已人工确认报价的更新、撤销和恢复优先级；外部用户映射 | intake.apply；待确认报价NEW/UPDATE可先实现，其余显式拒绝并保留原文 |
| R5 | 角色对报价、客盘机构/联系人、内部归属的可见及操作权限；基础资料来源完整映射 | 报价已有身份及归属校验；CRM当前仅按软删除／有效关联过滤，正式部署需确认其目录可见范围 |
| R6 | FICC渠道及单位映射；单券/多券image的分块、开始/结束及超限恢复 | S03/S08/S09；当前单券例子不能证明大规模初始化可用 |
| R7 | 未报价债券视图契约缺失 | 当前PriceView只列有匹配条目的债券，需基于完整债券池补充未报价查询及后续变化；不能用空NoMDEntries代替全券池搜索 |
| R8 | Bid/Ofr、估值偏离、跨侧价差条件的字段及计算层级未完整定义 | PriceFilter须明确SECURITY/ENTRY/BOOK作用域、先后顺序和空值口径；未定义字段不得返回enabled=true |
| R9 | 模糊报价的模型、录入、查询及事件接口未完成 | 不能把quoteType=UNKNOWN或price=null当作已经实现该功能 |
| R10 | 目标C#客户端/GW普通订阅、行情image、ACL、连接身份及实际退订 | Java源码已确认模式；本机已有联调，目标STC环境仍需逐项验收，不能由本机结果替代 |

通知规则还需确认退出后再次进入时是否沿用notificationId和readTime；当前API允许服务端明确发布REMOVE后再UPSERT，不让客户端自行猜测。

本稿新增的技术约定一并评审：Code/Msg/Data封装、分组及偏好revision、actorService系统审计字段、source最小模型、view前缀加载、Price面板整券替换、独立内部价格快照频道与quotePosition、内部epoch/sequence日志、初始限额。其中多数已有实现；actorService系统审计字段、SearchSvr恢复及消息字节限制仍不能作为已实现能力使用。

第4.1节页面映射覆盖已定义的当前报价和有报价行情主流程，不宣称上述缺口已解决。建议先评审模型与topic边界，再确定缺口契约；不能仅凭JSON可解析就批准整页开发。

后续重点为目标STC环境联调、CRM权限范围、CalSvr与完整供应商接入、SearchSvr和ParseSvr。当前实现与剩余提案以本稿状态标记区分；本次文档核对未启动服务或运行压力测试。
