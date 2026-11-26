# V1.8.7 行情原价与收益率排序分离：QA合并及前端协作说明

状态：**供QA合并/回归的候选源码，不是已验收版本。** 前序交付V1.8.6（163最新已发邮件UID1321041253）；QA合并/部署待回执。仅后端源码和测试/说明，无客户端修改。

## 问题与处理

原PriceBookService把用于收益率排序的副本直接用于展示，且PriceType固定3。净价101.333因此显示成换算后的行权收益率-1.2294。此次保留收益率排序，只分离“展示值”和“比较值”，不改历史报价、数据库字段、计算器算法、含权判断或QA已确认的行权缺失回退到到期收益率规则。

| 字段 | 本版约定 |
|---|---|
| MDEntryPx | 原报价price字符串，保留输入精度 |
| PriceType | 原quoteType映射：quoteType 1→3（收益率），2→1（净价），3→2（全价） |
| ExerciseFlag | 仅原收益率报价有值：yieldType 1→1（到期），2→0（行权）；净价/全价省略 |
| comparableYield | 新增字符串，收益率比较值，允许负数；Bid升序、Ofr降序及相同收益率并列规则不变 |
| comparableYieldType | 新增字符串，比较值实际口径：1到期、2行权；不是ExerciseFlag编码 |
| Yield / YieldToExecution / NetPrice / DirtyPrice | 分别为到期收益率、行权收益率、净价、全价；保留原输入对应字段，其余来自计算器；缺失不补造 |
| MDPriceLevel | 仍为每侧条目顺序号，并列条目也可不同；不是用于判断并列的唯一分组键 |

覆盖ficc.price.query FULL/BEST/DEPTH、ficc.price.view.open/query、行情流推送及Quote→Notification内部价格簿。原rowShape=QUOTE的comparableYield/comparableYieldType已有字段保持原约定。

NotificationSvr改为用comparableYield判定并列，沿用Quote的收益率排序。原来的可见性、版本一致性、同方向/原计价方式/原收益率类型/清算速度分组和至少两笔的提醒资格规则不变。兼容旧价格簿：只有缺少新字段且PriceType=3时才回退读取MDEntryPx；净价/全价缺比较值不猜测。提醒来源报价快照继续保留原价。

## 前端同事需要修改（本包不含前端源码）

以下路径相对同事客户端工程；以QA当前版本定位方法，不依赖固定行号。

1. `STC.SalesTrading.Plugin/Gateway/Contracts/View/MarketDataEntry.cs`：新增字符串属性，JsonProperty必须精确为`comparableYield`、`comparableYieldType`。修改“MDEntryPx是比较收益率”的旧注释；PriceType有1/2/3，不能固定当3。
2. `STC.SalesTrading.Plugin/Gateway/GatewayMarketCenterService.cs` 的 `AggregateBest`、内部 `BestEntry`：展示Price仍来自MDEntryPx；另存ComparisonYield和ComparisonYieldType。Bid最小/Ofr最大和并列集合用ComparisonYield，禁止把净价、全价、收益率的原数值混排。比较用decimal，允许负数，-1.2294与-1.229400相等。
3. `BuildDetails` / `ApplyLevelRepresentative` / `row.Bid`、`row.Ofr`：最优集合按比较收益率选出，最终展示原Price。既有“同档选报量最大作为代表”可保留，但价格、价格类型、收益率类型、报量、机构、人员及操作对象必须取同一代表条目，不能row.Bid仍填比较收益率而其他字段取另一条。混合净价/全价/收益率同收益率时仍保留全部并列报价。
4. `HoverSourceType`、浮层、报价录入预填/Copy、右键操作、行情重连和增量刷新：沿用条目的原PriceType与ExerciseFlag，不用comparableYield覆盖原报价。净价/全价没有ExerciseFlag是正常情况。比较口径不等于原报价口径。
5. 兼容旧后端：新字段缺失且PriceType=3时允许将旧MDEntryPx作为比较值；PriceType=1/2缺失比较值不能当收益率。优先保留服务端已给出的顺序/集合或明确无法比较，不能用0补值。旧客户端在新后端会把混合类型的原价继续做min/max，**不能只部署新QuoteSvr就宣称UI修复完成**。

原故障的正确字段示例（语义示例，原价按对应报价当前版本精度）：

```json
{"MDEntryPx":"101.333","PriceType":"1","comparableYield":"-1.2294","comparableYieldType":"2","Yield":"3.2586","YieldToExecution":"-1.2294","NetPrice":"101.333","DirtyPrice":"103.8934"}
```

实际本机250220.IB回归的净价条目：MDEntryPx=130.000000、PriceType=1、comparableYield=-1.331、comparableYieldType=1、NetPrice=130.000000、DirtyPrice=130.4252。它证明负收益率不会再覆盖原净价。

## 实际验证与未验范围

- Common 164项（1项原条件跳过），Quote 81项，Notification 11项，失败/错误0。Quote排除旧的QA连接依赖RequestParserTest，不冒充该用例通过。
- 更新后的独立专项：PriceSearchSmoke 114断言；NotificationProtocolSmoke 99断言；NotificationScenarioMatrix 324断言。隔离临时PostgreSQL schema均清理。这些是组件/协议验证，不是UI验收。
- 真实Ubuntu依赖通过标准启动脚本检查；本机Quote/Notification更新Common及各自JAR，按标准脚本重启。真实GW→Quote→CalSvr、Quote→Notification：250220.IB的10条实际CRM/债券报价，29项通过，涵盖净价/全价/到期收益率、Bid/Ofr、负比较收益率、并列、FULL/BEST/DEPTH、view.open、quote.get与数据库、实际订阅推送、MY_BEST选档及Refer后的切换。最终报价全部删除，用户提醒偏好已恢复。
- **原故障2228017.IB的真实端到端未完成**：本机CalSvr的sscalc适配器明确拒绝含权债和行权收益率计算（1003），12条尝试报价全部清理。未填造换算结果绕过上游。净价/全价/行权原值和独立比较字段已有组件测试覆盖，但不能替代QA实际CalSvr验证。
- 本机原cutoff=22:00阻止夜间确认；临时改00:00完成测试后已按备份恢复22:00并重启；不交付该临时配置，不修改QA cutoff。测试开发中修正数据库Decimal/规范列存储断言，失败尝试已清理。
- 未操作客户端UI、未验证同事改动后的缓存/页面、未验证QA Jenkins/运行包。QtIntake实盘连接保持停止，没有压力测试。

## QA合并/部署顺序与验收

- 本次三个组件增量：Common、QuoteSvr、NotificationSvr；从V1.8.6后端01e0ae9f基线生成。每个patch从对应组件仓库根目录应用。各目标提交和文件哈希见manifest。保留QA POM/0.1.1和本地业务改动，不全仓覆盖、不降级。
- 新Common先构建，再构建Quote与Notification。保留V1.8.6日志配置；无新数据库迁移、无新增部署配置项。
- 前端先实现兼容新旧协议；服务更新时Notification可先升级（兼容旧价格簿），再切换Quote，刷新内部同步。避免新Quote+旧Notification和新Quote+旧客户端组合。重启后核对Common只剩一个有效版本。
- V1.8.4/V1.8.5/V1.8.6及既有Base/Core依赖继续保留；本包不是全平台累计包。本次无Parser、QtIntake、Base、BondSearch Core新增源码，不撤销前序修改。前序未收到QA回执的事项仍待闭环。
- 在QA真实计算器下创建/检查2228017.IB同价多笔净价报价，接口MDEntryPx必须为原净价、PriceType=1，独立比较收益率可为负数；按对应版本核对数量和时间，不拿CREATE历史快照去匹配后续已修改版本。
- 再覆盖全价、到期/行权收益率、两侧并列、混合计价同收益率、可见性/版本不一致、Refer/修改后实时推送和提醒选档；客户端界面及浮层/Copy/预填均核对原价类型与报量。UI同事负责。
- 接口专项运行：`python market_price_regression.py --config config.local.json --security 2228017.IB --state <本机台账路径> --report <结果路径>`。依赖既有Python3 headless套件/真实桥接器/测试账号及数据库配置；allowWrites=true。会创建并清理最多12条报价，拒绝测试券已有ACTIVE报价，勿在有业务报价时强行绕过检查。支持`--cleanup`清理已有台账；不包含凭据。
- 请回传组件合并提交、部署JAR/版本、原故障实际请求/响应、29项接口专项及前端页面结果。SMTP接受不等于QA合并或验收。
