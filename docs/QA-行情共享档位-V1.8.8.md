# V1.8.8 行情共享档位：替代V1.8.7比较字段方案

**本版取代V1.8.7关于价格簿新增比较字段及前端再按收益率排档的设计。** 用户确认原STC中同收益率报价共享MDPriceLevel。本包为QA合并/回归候选；原含权故障券和UI尚待QA验收，不是已验收发布。

## 最终接口约定

- 删除V1.8.7新增到`MarketEntry / NoMDEntries[]`的`comparableYield`和`comparableYieldType`。
- `MDEntryPx`保持原报价，`PriceType`保持原计价方式：原quoteType 1→3收益率，2→1净价，3→2全价。`ExerciseFlag`仅原收益率报价有值：1到期、0行权；净价/全价省略。
- `Yield`仍为到期收益率，`YieldToExecution`仍为行权收益率；NetPrice/DirtyPrice和价格计算逻辑不变。
- 后端内部仍按收益率比较：Bid由低到高、Ofr由高到低。每侧`MDPriceLevel`从1开始；相同比较收益率共享档位，例如1、1、2，而非1、2、3。BEST保留第一档全部报价，DEPTH按不同收益率档数截取。
- 同档条目按真实更新时间倒序，再按quoteId稳定排序；timestamp只决定同档显示先后，不取消其他报价的并列最优资格。用户原STC约定是本次修正依据；既有FICC错误的逐条序号已改为档位。
- NotificationSvr直接取每个既有可比较/可见分组中的最小正整数MDPriceLevel，同档全部命中。原至少两条、方向/原计价方式/原收益率类型/清算速度分组、可见性和版本一致性规则不变。缺失/非正档位跳过，不用价格或收益率猜测；也不需要重新匹配参考数据计算排序。
- **删除范围仅限本次新增价格簿字段**：此前普通行情报价行`rowShape=QUOTE`中的comparableYield/comparableYieldType仍保留，已有客户端/筛选契约不变。不要全仓搜索删除同名字段。

## 前端同事需要调整（不含前端源码）

路径相对客户端工程：`STC.SalesTrading.Plugin/Gateway/GatewayMarketCenterService.cs` 的 `AggregateBest` 当前仍对MDEntryPx做min/max，必须改为读取已有MDPriceLevel（按整数，不能按字符串排序）。每侧选最小档位并保留该档全部条目；按服务端同档顺序展示，也可按MDEntryDate/MDEntryTime倒序复现时间顺序。**不引入比较收益率字段、不重新按Yield/原价计算名次。**

`Gateway/Contracts/View/MarketDataEntry.cs`已有MDPriceLevel，无需新增DTO字段；若尚未执行V1.8.7前端要求，不再执行；若已新增，其价格簿比较字段及使用应撤销。普通QUOTE行原有比较字段不撤销。

`BuildDetails` / `ApplyLevelRepresentative` 与row.Bid/row.Ofr：显示原MDEntryPx和PriceType；既有同档最大报量代表规则可保留，但价格、类型、收益率类型、数量、机构和操作对象必须来自同一代表条目。浮层、Copy、录入预填、增量更新和导出也遵循原价/原类型。无需改造页面布局。

例：两笔净价101.333、101.334经换算比较收益率均为-1.2294，则两笔均可MDPriceLevel=1，仍各自显示101.333、101.334；第三笔较差收益率MDPriceLevel=2。新报价在同档中排前不影响另一笔仍属第一档。

## 合并及部署

- 前序已发送V1.8.7（163邮件UID1321041255），本次唯一版本V1.8.8，旧包/哈希保留，不覆盖旧件。
- 包含Common、QuoteSvr、NotificationSvr最终相关源码及测试。主patch以V1.8.7后端a3b3fa84为基线；另提供`patches/from-V1.8.6/`累积路径供尚未合并V1.8.7的QA使用，二选一，不能重复套用。V1.8.6下Common没有新增字段，无需该组件累积patch。
- 保留QA现有POM/0.1.1和其他改动；从各组件根目录应用对应patch或方法级核对最终源码，不整仓覆盖。提交/范围/哈希见manifest。
- 先构建Common，再构建Quote/Notification；**暂停并一起更新Quote和Notification，然后启动Quote，再启动Notification使其全量同步新价格簿**。V1.8.7的唯一序号和本版共享档位语义不同，不支持混合版本滚动运行。不能仅先换Notification而继续使用旧Quote的逐条序号。
- 前端需同步最优聚合修改；旧FICC客户端仍对原价min/max，会误排。后端切换前协调同事改完，不宣称仅后端更新就解决UI。
- 无新数据库迁移/新部署配置；保持V1.8.6日志配置及前序Base/Core/Parser/QtIntake和V1.8.4/V1.8.5修复。未改前端源码、未连接外部QT。

## 实际验证

- Common164项（1项既有条件跳过），Quote82项（排除原QA连接依赖RequestParserTest），Notification12项，0失败/错误。
- 独立PriceSearch114、NotificationProtocol99、NotificationScenarioMatrix324断言通过。新增覆盖同收益率不同价格共享档位、两侧顺序、时间含不同offset的真实时刻比较、相同时间ID稳定排序、2/10数字档位、缺档位不猜、不可见报价及版本不一致。
- 标准Ubuntu启动脚本确认依赖在运行。本机两服务/新Common已部署并重启；250220.IB十条真实报价经GW→Quote→实际CalSvr→Notification，**36项通过**，含FULL/BEST/DEPTH、view.open、字段删除、同收益率同档、同档最新优先、负收益率下原净价、数据库、实际行情PUSH、提醒及Refer后选档。
- 十条报价全部通过业务接口删除，数据库独立复查全部DELETED，提醒偏好已恢复。本机原22:00 cutoff夜间测试临时00:00，测试结束已恢复原文件/哈希及重启；该临时配置不入包。
- **2228017.IB原故障端到端仍未完成**：上一轮实际发现本机sscalc适配器拒绝含权债/行权计算1003，本次未改变计算器也未注入假值，不能以普通券通过代替原券验收。QA请用实际计算器核对净价101.333/全价/到期与行权报价、混合计价并列、两侧、更新/Refer、前端浮层/Copy/预填。UI、QA部署及原券验收待回邮。
- 专项：`python market_price_regression.py --config config.local.json --security 2228017.IB --state <本机台账> --report <结果>`，依赖既有headless套件与真实测试配置，allowWrites=true；测试券已有ACTIVE时脚本主动拒绝避免影响业务。缺上游能力会明确失败，不能手工补计算字段。凭据不在包内。

请回传各组件合并提交、实际JAR/版本、原含权接口和客户端结果。SMTP接受仅证明邮件发送，不等于QA合并、部署或验收。
