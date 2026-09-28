# Quote报价模型与字典（讨论稿）

日期：2026-09-11。本文集中定义报价字段及枚举，供[QuoteSvr接口](../ficc-api-spec.zh-CN.md)、SearchSvr和客户端引用。QuoteSvr负责业务字段定义；其他服务补充展示或搜索数据时，不改变这些字段的业务含义。

## 1．基本约定

QuoteSvr统一负责报价与行情；Quote仍为持久化业务事实，MarketDataEntry及PriceBook由同服务内部生成和重建。计算值不覆盖Quote原始输入，行情退出不删除QuoteHistory。两种模型关系见[行情模型与STC映射](market-data-model.zh-CN.md)。

- 一个quoteId标识一条单方向报价，确认、Refer、Unrefer不更换ID；双向报价拆成两条。
- 业务字段平铺在Quote中，不增加fields或changes包装层。引用方式为`quote.quoteType`，不是`quote.fields.quoteType`。
- 本稿字典版本为v1：side、quoteType、yieldType、settlType沿用STC销售报价字典的字符串代码，由FICC固定含义并维护版本；不直接套用其他STC业务的同名字典，也不新增每条消息的字典版本字段。
- 下表描述服务端保存后的完整业务对象。创建请求只提交客户端字段，修改请求只提交本次要改的字段；各接口决定允许编辑的范围。
- 身份、状态、版本及时间由服务端控制，不能通过提交完整Quote任意覆盖。
- 数值使用十进制字符串；时间使用带时区的ISO 8601字符串。null表示缺失，不以空字符串或0代替。
- 收益率输入采用百分数、最多4位小数。超过4位时前端校验失败，服务端采用同一约束，不自动舍入后接受；例如2.50005为非法输入。CalSvr计算结果的舍入方式另行确认，净价/全价输入精度仍待定。

## 2．字段定义

### 身份与来源

| 字段 | JSON类型 | 可空性/来源 | 含义 |
| --- | --- | --- | --- |
| `quoteId` | string | 非空；服务端生成 | 报价唯一标识 |
| `revision` | string | 非空；服务端生成 | 正整数版本字符串，从`"1"`开始，每次成功变更递增；不能按字符串字典顺序比较大小 |
| `status` | string | 非空；服务端维护 | 生命周期状态，见第3节 |
| `clientQuoteId` | string | 可省略；手工新建时客户端生成 | 关联本地新建行与服务端报价；同一创建重试保持不变，按所属用户范围保证唯一 |
| `sourceMessageId` | string | 可省略；服务端关联 | 原始消息标识，供分组与追溯；纯手工结构化报价可没有 |
| `source` | object | 可省略；服务端维护 | 来源类型、外部账号/消息/行标识及原文引用；来源对象子字段后续统一，不由客户端冒充QT来源 |

来源消息ID不参与生命周期动作寻址。一条原始消息可以对应多条报价，一次请求也可处理不同来源的报价，不另设业务批次ID。

### 报价业务字段

| 字段 | JSON类型 | 可空性/默认值 | 含义 |
| --- | --- | --- | --- |
| `securityId` | string | 待确认可空；确认前必须确定 | 经BaseSvr解析、校验的债券代码，格式为`债券代码.市场后缀`，例如`180210.IB`；不是另行生成的编号或债券简称 |
| `side` | string | 待确认可空；确认必填 | 报价方买入/卖出方向，见字典 |
| `quoteType` | string | 待确认可空；价格存在时必填 | STC报价类型：收益率、净价、全价，见字典 |
| `yieldType` | string或null | 收益率报价有价格时必填；其他类型为null | STC收益率类型：到期、行权，见字典 |
| `price` | string | 可空；确认必填规则待产品定稿 | 与quoteType及yieldType对应的十进制数值；仍只保存一项用户输入价格 |
| `quantity` | string | 可空；确认必填规则待产品定稿 | 报量，必须连同quantityUnit和currency解释；未提供不等于0 |
| `quantityUnit` | string | quantity存在时必填 | 报量单位，当前使用`WAN` |
| `currency` | string | quantity存在时必填 | 面值币种，当前使用`CNY` |
| `institutionId` | string | 可空 | 报价对手机构的内部主数据ID |
| `institutionName` | string | 可空 | 待确认可先保存手工输入／来源消息中的机构名称；人工从CRM选定ID后显示该机构的标准名称 |
| `contactId` | string | 可空 | 对手联系人的内部主数据ID；填写时必须匹配institutionId |
| `contactName` | string | 可空 | 待确认可先保存联系人姓名，尚未匹配时contactId为空；人工从CRM列表选定联系人 |
| `tradeDate` | string | 待确认可空；确认时校验 | 业务交易日期，格式`YYYY-MM-DD`，不是时间戳 |
| `settlType` | string | 待确认可空；确认时校验 | STC清算类型代码，与tradeDate一起解释；明+0/明+1的日期语义仍待统一 |
| `urgent` | boolean | 非空；创建未传默认false | 急：紧急报价标记 |
| `recommended` | boolean | 非空；创建未传默认false | 荐：用户推荐标记，不是智能推荐评分 |
| `whole` | boolean | 非空；创建未传默认false | 整：整量标记，尚不定义成交执行约束 |
| `oco` | boolean | 非空；创建未传默认false | OCO标记，与packing互斥 |
| `packing` | boolean | 非空；创建未传默认false | 打包标记，与oco互斥 |
| `remark` | string | 可空 | 用户备注，可主动清空 |

### securityId格式约定

本稿依据STC客户端及产品示例采用`<bondCode>.<market>`，例如`180210.IB`（银行间）、`242993.SH`（上海）、`111114.SZ`（深圳）。

- 按字符串保存，保留前导零；代码部分不限定为六位，例如`032280666.IB`。市场后缀使用大写。
- 用户输入简称、无后缀代码或外部来源代码时，先由BaseSvr解析为明确的securityId；匹配不唯一时要求用户选择，不能直接补上默认市场。
- 缓存、查询、报价匹配均使用包含市场后缀的完整值。复制展示可以按需隐藏后缀，不能据此改变身份比较规则。
- 上述格式是根据现有资料确定的本稿约定；完整支持市场、长度及字符范围仍由基础资料字典统一，不把当前示例当作穷尽的校验规则。

产品示例中的`normalizedRecognitionCode()`会移除市场后缀，并用于更新、撤销目标匹配。这与本稿的身份规则不一致，新实现应保留后缀，避免不同市场的同码被当成同一标的。

### 用户与时间

| 字段 | JSON类型 | 可空性/来源 | 含义 |
| --- | --- | --- | --- |
| `ownerUserId` | string | 非空；服务端确定 | 报价所属我方用户，不等于对手联系人或本次操作者 |
| `createdBy` | string | 非空；服务端确定 | 首次创建的执行主体 |
| `updatedBy` | string | 非空；服务端确定 | 最近一次成功修改的执行主体；创建时为创建人 |
| `confirmedBy` | string | 未确认时为null | 首次人工确认人；Unrefer不覆盖 |
| `createTime` | string | 非空；服务端生成 | 首次创建时间，不覆盖 |
| `updateTime` | string | 非空；服务端生成 | 最近一次成功字段/状态变更时间，创建时等于createTime |
| `quoteTime` | string | 首次确认前为null | 最近一次发布/刷新时间；确认、有效报价修改及Unrefer时更新 |
| `confirmTime` | string | 未确认时为null | 首次人工确认时间；Unrefer不覆盖 |

时间示例：`"2026-09-11T10:32:00+08:00"`。人工执行主体从已认证DPS会话解析；系统自动动作记录系统主体，具体编码另行统一。Quote中的updatedBy只保存最近操作者，完整操作历史保存在审计记录中。

### 只读补充数据

| 字段 | JSON类型 | 约定 |
| --- | --- | --- |
| `reference` | object | 可省略，存放债券简称、期限、评级、参考估值和换算价格等只读补充数据；缺失值及原因应可表达。具体子字段在基础资料/搜索模型中补充 |
| `validationIssues` | array | 可省略；每项至少包含`field, message`，用于展示当前缺失或错误项。空数组表示本次检查无提示，不替代执行动作时的校验 |

用户明确输入的价格保留在price；其他换算价格不能伪装成用户输入。SearchSvr重新补充基础资料不应自行增加QuoteSvr的报价revision，需要用自身处理位置或资料版本跟踪变化。

## 3．数据字典

### status：报价状态

| 值 | 中文 | 主要允许动作 |
| --- | --- | --- |
| `PENDING_CONFIRM` | 待确认 | update、confirm、delete |
| `ACTIVE` | 有效 | update、refer |
| `REFERRED` | 已撤销 | unrefer、delete |
| `DELETED` | 已删除 | 终态；保留审计，不允许unrefer |

create生成PENDING_CONFIRM；confirm变为ACTIVE；refer变为REFERRED；unrefer恢复ACTIVE；delete逻辑删除。状态由动作改变，不接受客户端直接修改status。具体权限及校验见接口文档。

对手信息允许分阶段补齐：create／待确认update可只传institutionName、contactName，不要求已有ID。用户在确认阶段从CRM机构／联系人列表手动选择记录，由UI把institutionId、contactId及名称填入Quote并随confirm提交；不通过名称自动推断ID。只要填写了某个名称，确认前就必须匹配其ID；contactId必须属于所选institutionId。机构及联系人信息全部未提供的报价仍按原有可空规则处理。

后端验证ID有效性和机构归属，按所选ID返回标准名称；原始名称保留在创建／修改历史中。已有名称未匹配时返回UNRESOLVED_COUNTERPARTY，不改变报价状态、revision或历史。ACTIVE更新及unrefer也遵循此规则。institutionName、contactName与对应ID使用相同可见权限。

### side：报价方向

| 值 | 中文 | 视角 |
| --- | --- | --- |
| `"1"` | Bid／买入 | 报价方愿意买入 |
| `"2"` | Ofr／卖出 | 报价方愿意卖出 |

本表使用销售报价side字典；STC其他上下文（例如exeside的0/1）必须在接入时显式转换。正式Quote不接受Both；双向识别结果拆成两条单方向报价。

### quoteType / yieldType：价格口径

| quoteType | yieldType | 含义 | price单位/示例 |
| --- | --- | --- | --- |
| `"1"` | `"1"` | 到期收益率 | 百分数；`"2.6500"`表示2.65%，不是0.0265 |
| `"1"` | `"2"` | 行权收益率 | 百分数；具体行权依据由计算服务确定 |
| `"2"` | null | 净价 | 每百元面值价格；例如`"100.25"` |
| `"3"` | null | 全价 | 每百元面值价格；例如`"102.30"` |

quoteType沿用STC的1=收益率、2=净价、3=全价；yieldType沿用1=到期、2=行权。两字段替代旧稿的priceType，业务动作仍使用action，不借用quoteType表达新增/修改/撤销。

仅对齐字典，仍保留统一price字段。接入旧报价时，按quoteType从yield、netPrice或fullPrice取主报价，其余换算值属于只读补充数据；不能把净价数值直接重新解释成收益率。产品识别中的SPREAD（利差）、PAR（面值）暂不作为本版正式报价类型，保留候选信息供修正或后续扩展，不能静默改为到期收益率。

### quantityUnit / currency：报量口径

| 字段 | 本轮值 | 含义 |
| --- | --- | --- |
| `quantityUnit` | `WAN` | 万单位面值 |
| `currency` | `CNY` | 人民币 |

例如quantity=`"1000"`、quantityUnit=`"WAN"`、currency=`"CNY"`，表示1000万元人民币面值。其他单位及币种以后显式扩展，不由适配器猜测。

### settlType：清算约定

| STC代码 | 字典含义 |
| --- | --- |
| `"1"` | T+0 |
| `"2"` | T+1（今+1） |
| `"3"` | 明天+0 |
| `"4"` | 明天+1 |

字段名沿用STC的settlType，替代旧稿的settlement。代码是枚举，不是偏移天数：例如`"3"`不是T+2；某些旧筛选字典的`"2,3"`也不是一笔报价的清算类型。

- 本稿示例使用tradeDate及代码1/2，分别表示该交易日期的T+0/T+1；实际交收日期按业务日历和市场规则计算，不按自然日直接相加。
- 代码3/4保留STC字典含义，但其与tradeDate的关系须在实现前定稿：tradeDate表示原始基准日还是已换算交易日、由哪一步解析“明”，必须唯一，避免重复顺延。
- 确认时应确定并保存可重现的日期依据。查询历史或unrefer不能因“今天”改变而自动重解释“明”；具体日期落库字段随上述规则一起确定。
- 产品存在T+2筛选及T+3识别值，本版尚无对应代码，是否支持及映射另行确认。产品将明+0/明+1直接映射成T+0/T+1，以及编辑时将其他值归为T+1的行为不能直接沿用。

## 4．写入规则

- 修改已有报价时，**未传保留原值，null清空允许为空的字段，false取消布尔标记**。不能把未编辑字段自动序列化成null或false。
- 五个布尔标记不接受null；oco与packing不能同时为true。切换时显式提交一开一关，服务端校验合并后的结果。
- 价格、报量等字段的格式与关联口径须校验。清空可选输入后，不能用识别或估值数据自动填回。
- quoteType为2/3时yieldType必须为null；从收益率切换到净价/全价时，请求显式提交yieldType=null及对应口径的price（或允许的null），不能携带旧价格换个类型。切换到收益率时提交明确的yieldType及对应价格。服务端校验合并后的完整组合。
- 报价列表固定按updateTime从新到旧，同时间用quoteId保持稳定顺序。字段变更包括急、荐等标记的修改，也会更新updateTime。

## 5．Quote与消息字段的区别

| 字段 | 所属位置 | 含义 |
| --- | --- | --- |
| `requestId` | 请求、应答、报告 | 一次业务调用，不是报价身份 |
| `expectedRevision` | 已有报价的操作请求 | 客户端预期的报价版本，不是Quote的另一个持久化版本字段 |
| `action` | QuoteReport及审计 | `CREATE / UPDATE / CONFIRM / REFER / UNREFER / DELETE` |
| `eventId` | QuoteReport及审计 | 一次业务事件标识 |
| `operatorUserId, operationTime` | QuoteReport及审计 | 本次实际操作者及执行时间 |
| `receiveTime` | 来源消息 | 原文到达平台的时间，不等于报价createTime |

clientQuoteId可作为报价关联信息保存，也在创建应答及QuoteReport外层回传，方便绑定本地行。QuoteReport中的`quote`使用本文业务字段；数据模型不规定消息如何按会话合并或重放。

## 6．QuoteHistoryRecord：报价变更记录

供`ficc.quote.history.query`返回，表示一次已提交的报价变更；不另建一套报价字段字典。

| 字段 | JSON类型 | 业务含义 |
| --- | --- | --- |
| `eventId` | string | 对应QuoteReport的同一业务事件标识，不能为查询重新生成 |
| `revision` | string | 操作完成后的报价版本，等于after.revision |
| `action` | string | `CREATE / UPDATE / CONFIRM / REFER / UNREFER / DELETE`，分别为创建、修改、确认、撤销、恢复、删除 |
| `operatorUserId` | string | 实际操作主体，人工操作由服务端从认证会话解析；系统动作使用系统主体 |
| `operationTime` | string | 服务端记录的操作时间，格式与Quote时间字段一致 |
| `before` | Quote或null | 本次操作前保存的完整报价业务快照；CREATE时为null |
| `after` | Quote | 本次操作后保存的完整报价业务快照；DELETE时仍保留对象，status为DELETED |
| `reason` | string或null | 本次操作填写的原因；未填写时为null，与报价本身的remark区分 |

before/after使用本文的Quote业务字段，保存当时值，不只保存变化字段。SearchSvr实时补充的reference和validationIssues不属于这里的历史业务快照；查询时也不使用当前基础资料重新计算历史值。同一报价的后续记录before应与上一条记录after一致。

返回数组名为`records`，因为每项是一次变更记录；写入请求中的`quotes`仍表示待操作的报价。quoteId可从查询应答外层和before/after获取，无需额外增加historyId或batchId。

## 7．待产品确认

当前产品材料对“确认时价格、报量是否必填”存在冲突。收益率输入最多4位小数、超精度拒绝已确认；数值范围、其他数值精度、计算结果舍入及部分清算值仍待统一。本文保留可表达空值的待确认模型，不将尚未确认的规则写成最终接口约束。OCO、打包、整量先表示属性，联动撤销和成交执行行为另行讨论。
