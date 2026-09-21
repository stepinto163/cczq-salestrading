# 第一轮原型：属性筛选接口与覆盖缺口

更新时间：2026-09-21。

> 当前交付口径：先完成条件接口和明确标记的模拟资料，不代表全部外部源与真实业务算法已接入。原型四个筛选面板的每个属性控件均已对应 API 字段；关注债券/筛选分组已有独立接口，未纳入属性字段数。服务端目录现在有 79 个字段，C# 学习示例新增“全部产品条件”，直接读取该目录。

本轮已补：中债资信/隐含、YY评分、中证隐含绑定；`couponFrequency`、`isTaxExempt`、`localGovernmentRegion`；STC 远期代码；券种与固息归一化；独立净价偏离字段；综合文本查询。收益率 BP 偏离和净价差使用真实算术；**`bidOfferSpread` 当前使用标记为 SIMULATOR 的固定样例，并非实时双边价差计算**。外部评级、免税等未知源同样使用逐字段模拟标记。集成环境需显式开启 `base.filter.simulateMissing=true` 才填补缺失值。

补充接口：`isHighYield`（布尔）、`isPreListing`（布尔）、`outstandingAmount`（数值，亿）、`regionCategory`（字符串）、`bidOfferSpread`（数值，BP）、`valuationCleanPrice` / `bidCleanPriceSpread` / `offerCleanPriceSpread`（数值，价格单位），均列在运行时字典中。未报价债券查询、模糊语句解析、保存分组等是独立业务能力，不能通过伪造一条报价来替代。

## 修正前的产品逐项复核记录（2026-09-21，修正状态以上文为准）

以仓库根目录 `FICC销售智能交易平台.html` 的筛选面板为产品界面基准，对照 `iteration1/web-client/index.html`、Web 适配层、Java 字段目录及 C# 学习示例。另查阅 `D:\workspace\greatewall\qtrade\销交平台需求文档.md` 的 5.2.5、5.2.8、5.2.9、F.2.2～F.2.4；该文档的新增业务不能未经确认直接覆盖本项目后续已经明确的设计决策。

| 产品条件 / 行为 | 当前缺口 |
|---|---|
| 中债资信、中债隐含、YY评分、中证隐含 | Java 有对应字段，但 Web 使用按名称匹配的适配方式，未匹配这四个实际产品标签，因此没有绑定条件。现有 Java 名称也不能替代业务含义核实。 |
| 付息方式：按月、按季、半年、按年、其他 | 缺少付息频率字段；`couponType` 是计息方式，不能代替付息频率。STC `BondInfoView.Coupon_Frequency` 可作为后续映射依据。 |
| 是否免税：免税、非免税 | Java 字典和 Web 适配缺失；应独立表达，不能借用债券类型或随意推断税务规则。暂无确认来源时使用显式模拟属性。 |
| 地方债地区 | Web 实际标签没有接入；此前只处理了名字不同的“地方债区域”。需确认其条件与普通省份筛选如何组合。 |
| 国债、国开、农发、口行、中票、短融等券种 | 有 `bondSubtype` 不代表标签可用。STC 返回 BGB、PBB、CMN 等代码，Web 多数发送中文标签，缺少归一化；政策行还需要发行人分类区分。 |
| 各类热门标签 | `hotTags` 可筛数组，但本机只提供少量模拟值，STC 没有完成所有标签的分类映射与逐项验证。 |
| 利率方式：SHIBOR、DEPO、LPR、固息 | 固息应根据 `Coupon_Type`，不能在 `FRN_Index_ID` 中查“固息”；浮息指数还可能带期限后缀。当前适配不完整。 |
| 上市场所、银行业、发行人类型等枚举 | 当前源代码值、中文标签及模拟值尚未统一；显示了控件不等于实际匹配正确。 |
| 清算速度：远期 | Web 当前明确报不支持；T+0/T+1 已绑定，不能把整个清算速度条件记作完成。 |
| 地区、省份 | 当前将“地区”简化为省份，缺少市区层级；仅对少数省市名配置别名，尚无完整地区字典。 |
| Bid-Ofr | 需要双边报价组合计算；当前未实现服务端聚合条件。 |
| 同时筛 Bid/Ofr 或双边数量 | 当前引擎针对单条单边报价取交集，不能表达价格簿两边分别满足条件；需要独立的价格簿条件语义。 |
| 搜索债券、机构、交易员、发行人 | Web 搜索框未统一接入服务端跨属性搜索；不能以当前页本地搜索替代完整结果搜索。 |
| 关注债券、筛选分组 | 后端已有独立接口；Web 仍配置 `marketGroupReadOnly`，未完整连通保存/选择/应用流程。 |
| C# 学习示例 | 只提供七个教学预设，不是产品所有条件的清单。C# 主客户端虽按服务器目录提供编辑器，也继承了字段缺失和源枚举问题。 |

此前需求文档还列有 **一级半、高收益债、未报价债券、科创票据、地区反选、好/中/弱/差地区分类、债券余额区间、模糊文本识别并取相反方向**。这些不能仅靠现有 67 字段目录声称已支持；部分需要新的数据属性，部分需要查询对象或交互逻辑变化。需与当前产品范围逐项确认，尤其未报价债券不属于已有报价记录集合。

下一轮验收应以“产品控件及每个选项 → API 条件 → 数据来源/模拟标识 → 查询和实时进入/退出结果”为单位；不能只统计字段数或请求成功数。保持原 Web 布局。

## 接口不变

报价、行情、提醒各自的 `view.open` 接收 `filter.conditions`；之后 `view.query` 复用原视图条件。当前报价查询、历史报价搜索、筛选分组和实时匹配共用 `FilterEngine`。不是在当前已加载的一页上做本地过滤。

`ficc.quote.dictionary.query` 和 `ficc.price.dictionary.query` 的 `filterFields` 返回字段、中文名称、分组、类型、允许的运算符、参考值及是否包含模拟资料。前端按目录提供条件编辑；后端拒绝未知字段和不合类型的值。

## 条件范围

| 分组 | 字段 |
|---|---|
| 报价 | securityId、securityName、side、quoteType、yieldType、price、quantity、settlType、urgent、recommended、whole、oco、packing、bidPrice、offerPrice、bidQuantity、offerQuantity、bidValuationSpread、offerValuationSpread |
| 债券 | bondSubtype、productType、hotTags、ncdType、remainingYears、issuerName、listingStatus、issueMethod、valuationYield、csiValuationYield、couponRate |
| 流动性 | liquidityGrade、marketMakerTags |
| 信用 | listedMarket、issuerId、issuerType、bankType、industry、ownershipType、guaranteeType、guarantorName、issuerRating、issuerCbrRating、ratingOutlook、bondRating、bondCbrRating、yyRating、csiRating |
| 条款 | optionType、hasExerciseOption、repaymentType、couponType、perpetualType、isPerpetual、isSubordinated、rateIndex、issueYear |
| 区域 | isMunicipal、region、city、regionCode、keyProvince、municipalLevel、regionalRating、localGovernmentType、localGovernmentUse、isCrossMarket、isMortgage |

字符串支持 EQ、IN、CONTAINS、EXISTS；数值支持 EQ、IN、GT、GTE、LT、LTE、BETWEEN、EXISTS；布尔支持 EQ、EXISTS。相同字段多条件取 OR，不同字段取 AND。价格上下限应合成一个 BETWEEN，不要拆成同字段 GTE 和 LTE。范围包含边界；缺失数据不等于 false 或“其他”，只有 EXISTS=false 可查缺失。

收益率、收益率估值和票息单位为百分数；收益率偏离单位为 BP，净价与净价差为价格单位，数量沿用万，剩余期限单位为年。Bid/Ofr 条件作用于对应方向的报价；同时要求 Bid 和 Ofr 的逐笔条件不会匹配同一条单边报价。行情先筛选报价，再计算最优和档位。Bid-Ofr 已定义接口并提供显式模拟值，其真实双边聚合算法仍待实现。

```json
{
  "viewId": "credit-view-1",
  "mode": "FULL",
  "pageSize": 50,
  "filter": {
    "market": "CREDIT",
    "conditions": [
      {"field": "price", "operator": "BETWEEN", "value": ["2.0", "3.0"]},
      {"field": "bondRating", "operator": "IN", "value": ["AAA", "AA+"]},
      {"field": "liquidityGrade", "operator": "IN", "value": ["S", "A"]},
      {"field": "isSubordinated", "operator": "EQ", "value": false},
      {"field": "region", "operator": "CONTAINS", "value": "上海"}
    ]
  }
}
```

## 当前数据和后续替换

- STC 已明确的债券/发行人字段由 `StcBaseMapper` 映射，包括评级、地域、行业、企业性质、票息、含权类型、发行年份等。已核实的券种与浮息指数转换为产品标签，原值保留为 sourceBondSubtype/sourceRateIndex；未核实的源枚举保持原值，不猜测。
- 本机模拟器补充确定性样例，覆盖流动性、外部评级、条款等暂缺字段。每个补充字段在基础资料的 `filterDataSources` 中标记 `SIMULATOR`；不能据此进行真实信用或交易判断。
- STC 联调若也需要补齐模拟值，在原 `adapters.properties` 中显式设置 `base.filter.simulateMissing=true`。默认 false，不静默覆盖真实资料。启动日志会说明该模式。
- 后续可配置 `base.filter.<字段>=bond.<已确认列名>`，或 `issuer.<列名>`、`valuation.<列名>`；真实源值优先。新增供应商服务时仍由 BaseDataClient 提供同名业务属性，查询和订阅协议不变。不能将配置映射当作已有数据源接入完成。
- 中债偏离只在报价收益率与估值的到期/行权口径一致且估值非空时计算，不能把未知估值当作0。

## 客户端入口与验证

C# 的报价、行情和提醒增加“完整条件”编辑器；筛选分组使用同一字段目录。Web 保留原产品页面的标签、区间输入框、下拉框、分组和布局，仅在适配层将现有控件转换成 DPS 条件，不替换为通用字段表单。标签选择、输入和原“筛选”按钮请求服务端重建视图；原“清除过滤”按钮清空条件。切换市场保存各市场的筛选，管理条件也传入历史搜索。尚无对应契约的原控件保留原有不可用状态，不用含义不同的字段替代。

已完成：Java 全模块构建、C# 编译、402项小规模条件检查、Web条件值单测、原页面DOM检查，以及真实GW下一条报价的组合条件进入/退出报价和行情视图验证。测试报价已删除。提醒验证覆盖接受相同条件契约和分页请求；并未声称对全部提醒业务组合做了穷举测试。未运行压力测试。
