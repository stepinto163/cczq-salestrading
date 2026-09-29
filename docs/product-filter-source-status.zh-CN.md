# 产品筛选字段：数据来源与实现状态

核查日期：2026-09-29。范围：当前工作区 Java 实现（含尚未提交的协议模型重构），以 `FilterFields.DEFINITIONS` 的 **79 个条件字段**为清单基准。本文记录代码接入状态，**没有对 QA 每个字段做真实数据覆盖率抽样**。远端自行添加的配置不在本次核查范围内。

字段类型、单位、枚举和允许的操作符统一见[前端筛选协议字典](product-filter-contract.zh-CN.md)，本文不另建一套协议。协议字段存在、UI 控件可选、字典返回 `enabled=true`，都不代表已接入真实数据。

## 1. 总体状态和统计口径

| 分类 | 数量 | 结论 |
|---|---:|---|
| ENTRY：报价属性／基础标识／派生条件 | 22 | 11 个报价属性、2 个债券标识字段、9 个派生条件已有实现；派生条件仍依赖输入完整性 |
| SECURITY：已有映射或派生代码 | 25 | 包含配置缺失、源字段存疑和字典不完整风险，不能称为 25 个真实可用字段 |
| SECURITY：尚无内置真实映射 | 31 | 可由样本填充；部分 STC 原始数据已有，尚未接线或缺业务口径 |
| BOOK：Bid–Ofr | 1 | 未实现实时计算，样本值不代表盘口价差 |
| 合计 | **79** | **31 个资料字段缺内置真实映射，另 1 个盘口计算未完成** |

“缺数据源映射”不等于已经证明供应商没有数据；“候选”仅为后续核查方向。特别是债券余额和中证估值，已在本地 STC 源码找到字段／方法，但本平台尚未接入。

### 来源缩写与代码依据

- **Q**：QuoteSvr 的 `QuoteRecord`，来自人工、解析结果或 QTrade 等报价写入路径。
- **B**：`BaseSvr2Client.getBondInfo(securityId)` 返回的债券资料。
- **I**：按 `B.Issuer_Code` 查询的发行人资料；读取路径见 `StcBaseSvrClientBridge`。
- **V**：`BaseSvr2Client.getChianBondInfo(securityId)` 返回的中债估值资料（保留 STC 方法原拼写）。

| 代码 | 本文核查内容 |
|---|---|
| [FilterFields](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/FilterFields.java) | 79 字段目录、scope、类型和操作符 |
| [StcBaseDataClient](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/client/StcBaseDataClient.java) / [StcBaseSvrClientBridge](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/client/StcBaseSvrClientBridge.java) | B／I／V 取得方式 |
| [StcBaseMapper](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/client/StcBaseMapper.java) | 源字段映射、配置覆盖及模拟填充开关 |
| [BondFilterAttributes](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/client/BondFilterAttributes.java) / [FilterDictionaries](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/FilterDictionaries.java) | 券种分类、浮息类型及枚举规范化 |
| [QuoteFilterValues](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/QuoteFilterValues.java) | 字段读取、单边价格／数量、价差计算及综合搜索 |
| [FilterSampleData](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/FilterSampleData.java) | 缺值样本，`filterDataSources[field]=SIMULATOR` |
| [QuoteMatcher](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/QuoteMatcher.java) / [FilterEngine](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/FilterEngine.java) | 报价和价格共用的匹配入口与比较规则 |

下表“可补样本”专指 `FilterSampleData.fillMissing` 对该字段有补值逻辑，不保证每次都能生成非空值。模拟模式整体使用的资料也不是真实来源；派生条件可能间接受到模拟估值等上游字段影响。

## 2. 报价字段与派生条件（ENTRY，22 项）

| 字段 | 产品含义 | 当前来源／候选来源 | 实现状态 | 可补样本 | 限制及待办 |
|---|---|---|---|---|---|
| `searchText` | 综合搜索 | 报价和基础资料：securityId、securityName、issuerName、institutionName、contactName、ownerId、ownerDisplayName 拼接 | 已计算 | 否 | 忽略大小写的包含匹配；ownerId／ownerDisplayName 是否实际填充须核查，不能据此保证支持按销售姓名搜索。 |
| `bidCleanPriceSpread` | Bid净价-中债净价 | Bid 净价 − V.valuation_net_price | 已计算 | 否 | 仅 quoteType=2；缺净价估值则无值；单位是净价差，不是 BP。 |
| `offerCleanPriceSpread` | 中债净价-Ofr净价 | V.valuation_net_price − Ofr 净价 | 已计算 | 否 | 仅 quoteType=2；缺净价估值则无值；单位是净价差，不是 BP。 |
| `securityId` | 债券代码 | Q.securityId；B.Bond_ID 与 B.Listed_Market 校验／规范化 | 已接入 | 否 | 代码含市场后缀；报价指向已有基础资料。 |
| `securityName` | 债券名称 | B.Short_Name → ReferenceRecord.securityName | 已接入 | 否 | 过滤读取基础资料名称，不能用模拟名称证明真实债券覆盖率。 |
| `side` | 买卖方向 | Q.side | 已接入 | 否 | 报价自身的买卖方向。 |
| `quoteType` | 计价方式 | Q.quoteType | 已接入 | 否 | 报价自身的计价方式；不由估值类型推断。 |
| `yieldType` | 收益率类型 | Q.yieldType | 已接入 | 否 | 报价自身的到期／行权收益率类型。 |
| `price` | 报价价格 | Q.price | 已接入 | 否 | 按 quoteType 解释收益率或价格；不在筛选时自动做估值换算。 |
| `quantity` | 报价数量 | Q.quantity | 已接入 | 否 | 报价数量；按接口规定单位输入，字段匹配本身不换算单位。 |
| `settlType` | 交割方式 | Q.settlType | 已接入 | 否 | 协议交割代码；远期／非远期等 UI 组合须按字典展开，不是额外属性。 |
| `urgent` | 急 | Q.urgent | 已接入 | 否 | 布尔标记；未提供和 false 不等同。 |
| `recommended` | 荐 | Q.recommended | 已接入 | 否 | 布尔标记；未提供和 false 不等同。 |
| `whole` | 整 | Q.whole | 已接入 | 否 | 布尔标记；未提供和 false 不等同。 |
| `oco` | OCO | Q.oco | 已接入 | 否 | 布尔标记；未提供和 false 不等同。 |
| `packing` | 打包 | Q.packing | 已接入 | 否 | 布尔标记；未提供和 false 不等同。 |
| `bidPrice` | Bid | side=1 时取 Q.price | 已计算 | 否 | 单条 Bid 报价的价格；不是自动选择整本最优价。 |
| `offerPrice` | Ofr | 非 Bid 分支取 Q.price | 已计算 | 否 | 有效报价的 Ofr 价格；报价写入校验负责拒绝非法 side。 |
| `bidQuantity` | Vol.Bid | side=1 时取 Q.quantity | 已计算 | 否 | 单条 Bid 数量，不汇总多条报价。 |
| `offerQuantity` | Vol.Ofr | 非 Bid 分支取 Q.quantity | 已计算 | 否 | 单条 Ofr 数量，不汇总多条报价。 |
| `bidValuationSpread` | Bid-中债 | (Bid 收益率 − 中债收益率) × 100 | 已计算 | 否 | quoteType=1，且 yieldType 与 valuationBasis 一致；缺值或口径不一致不计算；单位 BP。 |
| `offerValuationSpread` | 中债-Ofr | (中债收益率 − Ofr 收益率) × 100 | 已计算 | 否 | 同上；中债口径取 V.bondTypeFlag：1=EXERCISE、2=MATURITY，其他值无口径。 |

## 3. 已有基础资料映射（SECURITY，25 项）

| 字段 | 产品含义 | 当前来源／候选来源 | 实现状态 | 可补样本 | 限制及待办 |
|---|---|---|---|---|---|
| `valuationCleanPrice` | 中债净价估值 | V.valuation_net_price | 已映射 | 是 | 核查估值日期、数值有效性及债券覆盖；不保证每券每日有值。 |
| `couponFrequency` | 付息方式 | B.Coupon_Frequency | 已映射 | 是 | 经字典转协议代码，如 A→Y1；未知原值会被移除。 |
| `bondSubtype` | 细分券种 | B.Bond_Subtype；PBB 另结合 I.issuer_name | 已映射 | 是 | BondFilterAttributes 分类后再规范化；未覆盖子类型可能被字典移除。 |
| `remainingYears` | 剩余期限 | V.remaining_year | 已映射 | 否 | 目前依赖估值返回；没有从 Maturity_Date 计算的兜底路径。 |
| `issuerName` | 发行人 | I.issuer_name | 已映射 | 是 | 发行人表无记录时为空；不自动使用债券原始 issuerName 兜底。 |
| `valuationYield` | 中债估值 | V.valuation_yield | 已映射 | 否 | 需同时检查估值日期和到期／行权口径。 |
| `couponRate` | 票面利率 | B.Coupon_Rate_Current | 已映射 | 是 | 当前票面利率；未验证真实样本覆盖率。 |
| `listedMarket` | 交易场所 | B.Listed_Market | 已映射 | 是 | 字典含 SZE→SZSE 转换；未知市场码移除。 |
| `issuerId` | 发行人代码 | B.Issuer_Code | 已映射 | 否 | 发行人关联键，用于读取 I；不同于报价交易对手 institutionId。 |
| `issuerType` | 发行人类型 | I.issuer_type → base.issuerType.<原值> 配置 | 有代码，缺配置核实 | 否 | 本次检查的本地配置未发现有效映射；没有配置即为空。不能计作真实可用。 |
| `industry` | 行业 | I.sw_sector → 产品行业字典 | 有代码，映射不完整风险 | 是 | 如原值“银行”不在当前产品行业字典中，会被移除；需要明确申万行业到产品类别的映射。 |
| `ownershipType` | 企业性质 | I.institution_subtype → 企业性质字典 | 有代码，源字段存疑 | 是 | 所查 TIssuerInfo／PIssuerInfo 未发现该字段；需核实真实响应及正确来源，不能改用 org_type 猜测所有制。 |
| `issuerRating` | 主体评级 | B.Issuer_Rating_Current | 已映射 | 否 | 未使用 Issuer_Rating_Current_NPY 兜底；当前评级字典不是全等级集合，未知等级会被移除。 |
| `bondRating` | 债项评级 | B.Rating_Current | 已映射 | 否 | 未使用 Rating_Current_NPY 兜底；需核查评级字典覆盖。 |
| `optionType` | 含权类型 | B.Option_Type | 已映射 | 是 | 源代码校验及产品字典均需覆盖；SLB／OTH／PRS 等允许进入映射但不在当前产品字典中。 |
| `hasExerciseOption` | 是否含权 | B.Option_Type 分类 | 已计算 | 否 | NON=false；PUT/CNP/CAL/ETS/ASS/DCN/CNV=true；其余认可但未分类值为 null。 |
| `couponType` | 计息方式 | B.Coupon_Type | 已映射 | 是 | 例如 FRB→FIXED、FRN→FLOATING；未知值会被移除。 |
| `rateIndex` | 浮息方式 | B.FRN_Index_ID，结合 B.Coupon_Type | 已映射 | 是 | FRB 映射固息；SHIBOR／DEPO／LPR 按前缀归类后转协议码。 |
| `issueYear` | 发行年份 | B.Issue_Year | 已映射 | 是 | 数值条件；需检查源字段是否缺失，不由日期自动补齐。 |
| `isMunicipal` | 城投 | B.Is_Municipal | 已映射 | 是 | 识别 Y/N、YES/NO、TRUE/FALSE、1/0；不使用 I.is_municipal 兜底。 |
| `region` | 省份 | I.province → 地区字典 | 已映射 | 否 | 省份中文全名／简称转六位码；未知值移除，不能证明所有地区已覆盖。 |
| `city` | 城市 | 优先 I.city_code，否则 I.city | 有代码，字典覆盖风险 | 是 | 接受合法六位城市码；城市名称字典当前只有部分城市；未知供应商代码没有名称回退。 |
| `regionCode` | 省份代码 | I.province_code → 地区字典 | 有代码，供应商码未对齐风险 | 是 | 例如 BJPRN 当前无转换而会被移除；不能因为 region 正常就认定 regionCode 正常。 |
| `isCrossMarket` | 跨市场 | B.Is_Cross_Mkt | 已映射 | 是 | 布尔规范化；无法识别的值保持缺失。 |
| `isMortgage` | 可质押 | B.Is_Mortgage | 已映射 | 是 | 当前沿用该字段解释“可质押”；需由业务／源字典核实语义是否完全一致。 |

## 4. 尚无内置真实映射（SECURITY，31 项）

| 字段 | 产品含义 | 当前来源／候选来源 | 实现状态 | 可补样本 | 限制及待办 |
|---|---|---|---|---|---|
| `isHighYield` | 高收益债 |未接入；需约定高收益阈值及使用收益率／评级的口径；**SZ：Wind 通道**（详见第 9 节） | 缺业务规则 | 是 | 不能仅凭字段名制定真实高收益债标准。 |
| `isPreListing` | 一级半 |未接入；候选：上市日期、发行状态；**SZ：Wind 通道＋日期规则**（详见第 9 节） | 缺计算规则 | 是 | 须先确认“一级半”的产品定义，不能直接等同未上市。 |
| `outstandingAmount` | 债券余额（亿） |未接入；STC BondInfoView 已有 Outstanding_Amount；**SZ：Wind 通道**（详见第 9 节） | 缺映射 | 是 | 先确认源单位，再换算为协议“亿”；不是需要新采购数据的结论。 |
| `regionCategory` | 地区分类 |未接入；地区分类数据或评定规则待确定；**SZ：本地硬编码＋Wind 地区**（详见第 9 节） | 缺数据源／规则 | 是 | GOOD/AVERAGE/WEAK/POOR 目前只有协议选项和样本值。 |
| `localGovernmentRegion` | 地方债地区 |未接入；候选：地方债发行人地区；**SZ：Wind 部分对应**（详见第 9 节） | 缺派生规则 | 是 | 先识别地方债再取发行地区；不能对所有债券直接复制 region。 |
| `isTaxExempt` | 是否免税 |未接入；税务属性或业务规则待确定；**SZ：Wind 通道**（详见第 9 节） | 缺数据源／规则 | 是 | 不能以利率债／信用债二分推断真实免税资格。 |
| `productType` | 产品分类 |未接入；候选：现有 market、stcGoodsType 与债券分类；**SZ：Wind 部分对应**（详见第 9 节） | 缺分类映射 | 是 | 已有原始分类信息，但还未转换到此产品字段。 |
| `hotTags` | 热门标签 |未接入；候选：STC 债券标签数据，待核实；**SZ：Wind 部分对应**（详见第 9 节） | 缺标签映射 | 是 | 需逐一对应产品热门标签；样本“绿色债／城投”不代表真实标签。 |
| `ncdType` | NCD类型 |未接入；候选：NCD 发行人银行类别；**SZ：Wind 部分对应**（详见第 9 节） | 缺数据源映射 | 是 | 需核实银行类别来源及 NCD 类型分类规则。 |
| `listingStatus` | 债券上市状态 |未接入；候选：Listed_Date、Delisted_Date 及状态信息；**SZ：Wind 部分对应／语义冲突**（详见第 9 节） | 缺派生规则 | 是 | 需确定交易日和上市／未上市／退市边界。 |
| `issueMethod` | 发行方式 |未接入；公募／私募等发行属性来源待核实；**SZ：Wind 通道**（详见第 9 节） | 缺数据源映射 | 是 | 不从券名或单一券种猜测。 |
| `csiValuationYield` | 中证估值 |未接入；STC 已有 BaseSvr2Client.getCsiBondValuation()；**SZ：Wind 通道（模型证据）**（详见第 9 节） | 缺调用及映射 | 是 | 须确认返回值是本接口所需收益率、日期和单位，并验证 QA 有数据；不能仅凭方法存在判定可用。 |
| `liquidityGrade` | 债券流动性 |未接入；外部流动性分级来源待确定；**SZ：未找到证据**（详见第 9 节） | 缺数据源 | 是 | S/A/B/C/D 暂为接口与模拟值。 |
| `marketMakerTags` | 做市标的 |未接入；做市名单来源待确定；**SZ：未找到证据**（详见第 9 节） | 缺数据源 | 是 | 明确名单来源、市场、有效期和更新机制。 |
| `bankType` | 银行业 |未接入；发行人银行分类来源待核实；**SZ：Wind 部分对应**（详见第 9 节） | 缺数据源映射 | 是 | 国有、股份、城商、农商等需稳定代码转换。 |
| `guaranteeType` | 担保 |未接入；候选：Rating_Augment 及担保资料；**SZ：Wind 部分对应**（详见第 9 节） | 缺语义映射 | 是 | 信用增级方式不必然等于产品要求的担保机构类别。 |
| `guarantorName` | 担保人 |未接入；担保关系和担保人名称来源待确定；**SZ：Wind 通道**（详见第 9 节） | 缺数据源映射 | 是 | 确认多担保人的数据结构；目前单字段样本不能证明覆盖。 |
| `issuerCbrRating` | 中债资信 |未接入；中债资信主体评级来源待确定；**SZ：未找到证据**（详见第 9 节） | 缺数据源 | 是 | 不得用普通主体评级替代。 |
| `ratingOutlook` | 评级展望 |未接入；评级展望来源待确定；**SZ：未找到证据**（详见第 9 节） | 缺数据源映射 | 是 | 需绑定评级来源与日期，不能由等级变化推算。 |
| `bondCbrRating` | 中债隐含 |未接入；中债隐含评级来源待确定；**SZ：未找到证据**（详见第 9 节） | 缺数据源 | 是 | 不得用债项公开评级替代。 |
| `yyRating` | YY评分 |未接入；YY 数据来源待确定；**SZ：未找到证据**（详见第 9 节） | 缺数据源／分档规则 | 是 | 当前三档为暂定产品代码，需要与最终评分／等级建立对应。 |
| `csiRating` | 中证隐含 |未接入；中证隐含评级来源待确定；**SZ：未找到证据**（详见第 9 节） | 缺数据源 | 是 | 中证估值接口存在不等于已提供中证评级。 |
| `repaymentType` | 偿还方式 |未接入；偿还计划／方式来源待核实；**SZ：Wind 通道＋字符串规则**（详见第 9 节） | 缺数据源映射 | 是 | 到期一次还本、提前还本等需对应真实条款。 |
| `perpetualType` | 永续债类型 |未接入；永续标识及发行人分类；**SZ：Wind 部分对应**（详见第 9 节） | 缺派生规则 | 是 | 先补真实永续识别，再分类城投／产业／金融永续。 |
| `isPerpetual` | 是否永续 |未接入；永续条款或可靠标识待确定；**SZ：Wind 通道**（详见第 9 节） | 缺数据源映射 | 是 | 不以很长剩余期限代替永续标识。 |
| `isSubordinated` | 次级债 |未接入；候选：清偿顺序／次级条款；**SZ：Wind 通道**（详见第 9 节） | 缺语义映射 | 是 | 需核实源字段编码，不能直接把某券种全判为次级。 |
| `keyProvince` | 重点省份 |未接入；重点省份名单及有效版本；**SZ：未找到证据**（详见第 9 节） | 缺业务规则／名单 | 是 | 六位省码已有接口，名单及变更规则尚未接入。 |
| `municipalLevel` | 城投层级 |未接入；城投行政层级来源待确定；**SZ：Wind 通道**（详见第 9 节） | 缺数据源映射 | 是 | 注册城市不等于省级／市级／区县级城投层级。 |
| `regionalRating` | 区域评级 |未接入；区域评级来源待确定；**SZ：未找到证据**（详见第 9 节） | 缺数据源 | 是 | REGION_A/B/C/D 仍为暂定模拟分档。 |
| `localGovernmentType` | 地方债类型 |未接入；地方债一般／专项属性来源待核实；**SZ：Wind 通道＋组合规则**（详见第 9 节） | 缺数据源映射 | 是 | 需要券级属性，不能按地区判断。 |
| `localGovernmentUse` | 地方债用途 |未接入；地方债资金用途来源待确定；**SZ：未找到证据**（详见第 9 节） | 缺数据源映射 | 是 | 需确认多用途及项目到债券的归集方式。 |

## 5. 盘口条件（BOOK，1 项）

| 字段 | 产品含义 | 当前来源／候选来源 | 实现状态 | 可补样本 | 限制及待办 |
|---|---|---|---|---|---|
| `bidOfferSpread` | Bid-Ofr |当前只有样本值 5.00；无实时买卖盘计算；**SZ：报价派生，非 Wind 原始属性**（详见第 9 节） | 缺实时计算 | 是 | 应先确定同债券、同计价／收益率／交割口径的 Bid/Ofr 配对和聚合规则，再计算；不能把样本常数用于真实筛选。 |

## 6. 共性限制与配置

1. `base.filter.<field>=bond.<column>`、`issuer.<column>` 或 `valuation.<column>` 可覆盖内置映射，但只读取已经取得的 B／I／V 对象，**不会自动增加一个外部接口或数据库查询**。例如配置某个不存在的供应商字段，仍得不到真实值。本次已检查的本地配置未发现这些有效覆盖。
2. `base.filter.simulateMissing` 默认 `false`。开启时，只对缺值补样本，并标记 `filterDataSources`。部分已有映射字段也可能因为缺值而被模拟补齐，因此不能只按字段名判断来源。
3. 枚举规范化遇到未知值会移除该字段并写 DEBUG 日志；当前尚无逐字段真实覆盖率看板。应检查源值、映射值和字典三者，不能把被移除的字段解释为业务上的“其他”。
4. 实际缺值时，普通 EQ／IN／范围条件不匹配；`EXISTS false` 可以匹配缺值。布尔 false 是有效值，不等于缺失。
5. 行情视图先对 ACTIVE 报价应用同一个 QuoteMatcher，再组成债券价格簿；目前没有独立的跨报价 Bid–Ofr 聚合筛选。因此该字段虽声明为 BOOK，也没有因 scope 自动得到计算实现。
6. CRM 的机构／联系人用于报价交易对手选择和解析，与债券发行人资料不是同一来源。当前未通过 CRM 自动补债券信用、行业或地区分类。

79 个字段不包含顶层 `filter.market`、`watchlistIds`、`filterGroupIds`。它们是组合过滤入口：market 来自 STC 分类或配置，后两者来自用户保存的关注债券／筛选分组。它们内部引用的 condition 仍受本文字段覆盖限制。

## 7. 接入优先级与验收

1. 优先接通已有 STC 数据：债券余额、中证估值；修复发行人类型、企业性质、行业、地区代码等已有映射风险。
2. 明确派生业务口径：产品分类、上市／一级半、重点省份、地方债地区以及 Bid–Ofr 配对规则。
3. 对流动性、做市名单、YY／区域评级等明确来源、版本、有效日期和更新方式；其余缺项逐一核查 STC，不预先断言必须购买新数据。

每个字段改为“已接入”前，应至少保存一条真实源响应（脱敏）、源字段与单位／字典说明、映射后的预期值，以及独立计算预期结果的自动化用例。自动化分为两层：

- **固定数据功能回归**：每字段至少命中、不命中、缺值三类；数值包含上下界，枚举包含多选和未知码，布尔区分 false／缺失；按字段支持的操作符运行。组合条件验证同字段 OR、不同字段 AND，并验证报价／行情／订阅进入退出行为。模拟夹具可以使用，但报告明确标记来源。
- **真实资料接入验收**：关闭缺值模拟，对选定真实债券校验源值、规范化值及筛选结果；数据缺失单独报告为前提未满足，不算筛选用例通过。禁止由当前被测 matcher 生成自己的预期结果。

现有 `TypedFilterEquivalenceTest` 验证新旧读取和匹配结果一致，不能证明两版都符合业务需求，也不能证明真实源完整。上面的全字段独立预期测试是后续建设要求，本次仅完成清单文档，没有新增自动化运行或压力测试。

## 8. 维护约定

新增／删除条件时同时维护 FilterFields、协议字典和本文；一行对应一个精确 wire 字段名，不以中文控件数量代替字段数量。状态变更必须带代码和验证证据，并注明是否仍依赖配置或外部数据。当前各项真实 QA 数据覆盖率均为“未逐字段验证”。

## 9. SZ 旧版来源和硬编码规则核查（31 个缺映射字段＋Bid–Ofr）

### 9.1 “来自 Wind”的证据边界

2026-09-29 检查本地导出目录 `D:/workspace/greatewall/stc-client-sz/stc-client-lite`。该目录无 Git 元数据，以下按所查文件及行号记录。当前应用未接入这些字段的事实不变；本节补充替代数据源和逻辑迁移依据，**没有修改业务代码**。

已确认的客户端链路：`WindHandler.QueryWindBondInfoPage` → 服务 **SBPSvr**、topic **stc.sbp.wind.info** → `WindBondInfoOrigin` → `WindBondInfo` → 本地缓存／筛选。客户端会使用 SQLite 缓存，但缓存不是原始供应商。

本文“Wind 通道”表示**该数据通过 SZ 命名为 Wind 的模型／SBPSvr 接口交付**。没有 SBPSvr 的字段生产代码，不能进一步保证每个 flag 都由 Wind 原始接口直接返回，也可能由 SBPSvr 计算或整合第三方来源。中证／中债估值尤其不能仅因存放在 Wind 模型就改称 Wind 原创数据。

- 20 个缺映射字段找到 Wind 通道字段或部分对应依据；其中存在枚举与语义不完全一致的情况。
- `regionCategory` 是本地硬编码地区名单，匹配用的地区属性来自 Wind 通道。
- 另外 10 个缺映射字段未找到明确来源证据；不能推断它们也来自 Wind。
- `bidOfferSpread` 另列为报价聚合问题，未找到同等 SZ 条件实现。

“硬编码规则”包含常量名单、日期比较、字符串分类、flag 组合和 null 处理。简单的字段映射与规则算法分开标记；未找到规则的项不补写推测算法。

证据缩写，路径均相对上述 SZ 根目录：

| 缩写 | 文件 |
|---|---|
| W | `STC.Common/Model/Wind/WindBondInfoOrigin.cs` |
| M | `STC.Common/Model/Wind/WindBondInfo.cs` |
| F | `STC.SalesTrading.Plugin/View/Component/BondFilter/BondFilterViewModel.cs` |
| A | `STC.SalesTrading.Plugin/View/Component/BondFilter/BondFilterView.xaml.cs` |
| H | `STC.Common/Handler/Wind/WindHandler.cs`：169 行请求 SBPSvr；825 行担保人列表 |

### 9.2 逐字段来源和规则标记

| 当前字段 | SZ 来源判断 | SZ 数据字段 | 硬编码／派生规则 | 证据与迁移注意事项 |
|---|---|---|---|---|
| `isHighYield` | Wind 通道 | highYieldBondFlag；IsHighYield | R01：只判断 flag==true；没有本地收益率阈值 | W:581，M:1102，F:2797；保留真实标记来源，不能把模拟阈值当作 SZ 算法。 |
| `isPreListing` | Wind 通道＋日期规则 | bInfoListdate | R02：有上市日期且晚于 DateTime.Today | W:353，F:2801/2829；SZ 注释明确“待上市（一级半）”，迁移需固定业务日期测试边界。 |
| `outstandingAmount` | Wind 通道 | bInfoOutstandingbalance | R03：下界含、上界不含 | W:569，F:4909；当前 BETWEEN 上下界均包含，需明确兼容策略；单位仍需验证。 |
| `regionCategory` | 本地硬编码＋Wind 地区 | 本地好／中／弱／差地区名单；province/city/district | R04：硬编码名单及宁波／大连例外 | A:92，F:2057；不是供应商提供的评级列，须迁移名单与省市层次语义。 |
| `localGovernmentRegion` | Wind 部分对应 | localGovernmentDebtFlag ＋ province/city/district | R05：地方债识别与通用地区匹配分别存在 | W:448/51/56/61，F:2057；没有同名地方债地区字段，组合为新字段需明确规则。 |
| `isTaxExempt` | Wind 通道 | taxFreeFlag → IsTaxFree | R01：true 为免税；旧非免税判断把 null 也归入 | W:611，M:1042，F:3170/3174；另有税标展示日期规则 R06，不是筛选来源。 |
| `productType` | Wind 部分对应 | bondType、ncdFlag | R07：bondType==1 为利率债，旧 IsCredit 取其反值；NCD 独立 flag | W:424/229，M:957/962/720；当前 RATE/CREDIT/NCD 单选需要定义 NCD 优先级，不能直接一列映射。 |
| `hotTags` | Wind 部分对应 | commercialFinanceBondFlag、bankTier2CapitalFlag、bankPerpetualBondFlag、insuranceSubordinateBondFlag、cityinvestmentbond、realEstateIndustryFlag、railTransportIndustryFlag、stiBondFlag、greenBondFlag 等 | R08：多个标记派生标签；同组勾选 OR | W 对应属性，F:3568；只找到部分标签，TLAC、产能过剩等不能据此标记已具备。 |
| `ncdType` | Wind 部分对应 | ncdFlag ＋ stateOwnedCommercialBankFlag／jointStockCommercialBankFlag／cityCommercialBankFlag／ruralCommercialBankFlag／foreignBankFlag | R09：有 NCD 与银行标记，无当前六类 NCD 的完整专用组合实现 | W:229/518/524/530/536/259；须确定互斥、优先级及 OTHER 的缺值行为。 |
| `listingStatus` | Wind 部分对应／语义冲突 | SZ OnsaleType 用 isInMarket（主体上市）；另有 isListedFlag（债券上市） | R10：SS 判 isInMarket==true；NOTSS 判 !=true | W:419/587，F:3887，M:952/1082；当前字段说明是债券上市，不能直接迁移主体上市逻辑。 |
| `issueMethod` | Wind 通道 | publicOfferingBondFlag、privatePlacementBondFlag、miniPublicOfferingBondFlag | R11：标记分别对应 GM／SM／XGM；旧组内 OR | W:301/307/313，F:4369；若多个标记同时为真，不能擅自压成一个标量。 |
| `csiValuationYield` | Wind 通道（模型证据） | bAnalYieldCsi（到期），bAnalYtmIfexe（行权）；bAnalNetCsi 是净价 | R12：口径必须区分；未找到同名条件的独立筛选实现 | W:403/408/413，M:361/366/371；SZ 报价模型也带这些值，不能把价格当收益率；不证明全部值直接来自 Wind 商业接口。 |
| `liquidityGrade` | 未找到证据 | SZ 所查模型／筛选中无对应 S/A/B/C/D 来源 | 未知 | 不能标记为 Wind；仍需寻找供应商或规则。 |
| `marketMakerTags` | 未找到证据 | 未找到四类做市标的名单对应来源 | 未知 | 做市机构／桥的交易字段不是做市标的筛选数据。 |
| `bankType` | Wind 部分对应 | bigBankFlag、jointStockCommercialBankFlag、cityCommercialBankFlag、ruralCommercialBankFlag；模型另有 stateOwnedCommercialBankFlag／foreignBankFlag | R09：旧控件 DH/GF/CS/NS；DH=大行，不是当前国有行的同义值 | W 对应属性，F:4020；按当前 STATE_OWNED 等字典重新明确对应关系。 |
| `guaranteeType` | Wind 部分对应 | guaranteeFlag | R01：有担保==true，无担保取反 | W:46，M:851，F:4603；不能覆盖当前全国性／地方性担保子类。 |
| `guarantorName` | Wind 通道 | guarantor | R13：与选定担保人名称做精确相等比较 | W:142，F:1993；旧候选列表来自 WindHandler.GetGuarantors。 |
| `issuerCbrRating` | 未找到证据 | 只有 subjectrating 不能证明是中债资信主体评级 | 未知 | 不得把普通主体评级直接当此字段。 |
| `ratingOutlook` | 未找到证据 | 未找到评级展望字段及同等过滤分支 | 未知 | 需外部来源或继续核查 SBPSvr。 |
| `bondCbrRating` | 未找到证据 | bondrating 是普通债券评级；未找到中债隐含专属字段 | 未知 | 不得按相似等级字符串强行对应。 |
| `yyRating` | 未找到证据 | 未找到 YY 来源；finalResult 注释为德勤评级 | 未知 | finalResult 不能冒充 YY 评分。 |
| `csiRating` | 未找到证据 | 中证估值字段存在，但未找到中证隐含评级来源 | 未知 | 估值不是评级。 |
| `repaymentType` | Wind 通道＋字符串规则 | repaymentmethod | R14：等于“到期偿付”为到期；其他值（含 null）旧逻辑归提前还本 | W:93，M:623/628，F:4933；当前还有 OTHER，需决定缺值及未知方式如何迁移。 |
| `perpetualType` | Wind 部分对应 | perpetualBondFlag、bankPerpetualBondFlag、insurancePerpetualBondFlag、cityinvestmentbond 等 | R15：旧有永续及银行／保险永续布尔规则，未找到当前城投／产业／金融三分法 | W:478/253/289/319，F:5093；三分法仍需组合规则，不是一个既有 Wind 字段。 |
| `isPerpetual` | Wind 通道 | perpetualBondFlag | R01：true 为永续；旧非永续取反 | W:478，M:1012，F:5120/5123；null 的旧行为需要显式决定。 |
| `isSubordinated` | Wind 通道 | subordinateBondFlag | R01：flag==true | W:211，M:705，F:3399；不需要用券名猜测，但银行二级等其他标签是否并入须另定。 |
| `keyProvince` | 未找到证据 | 未找到“12重点省”名单或对应规则 | 未知 | SZ 好／中／弱／差地区名单不是重点省名单。 |
| `municipalLevel` | Wind 通道 | provinceFlag、cityFlag、districtFlag | R16：布尔标记决定省／市／区级；不是地址层级推断 | W:115/121/127，M:633/638/643，F:5010；旧 OtherArea 分支不完整，不能直接复制。 |
| `regionalRating` | 未找到证据 | 未找到 REGION_A/B/C/D 对应评级列 | 未知 | 不能将本地地区名单当作评级来源。 |
| `localGovernmentType` | Wind 通道＋组合规则 | localGovernmentDebtFlag、specialBondFlag | R17：地方债且 special==true 为专项；地方债且 special!=true 为一般 | W:448/472，M:1002/1007，F:3156；未知专项标记被旧逻辑并入一般，迁移要明确。 |
| `localGovernmentUse` | 未找到证据 | 未找到棚改／土储／收费公路等当前用途对应链路 | 未知 | M 尾部 windPlateName 专项板块处理已注释，不能当作可运行的数据来源。 |
| `bidOfferSpread` | 报价派生，非 Wind 原始属性 | 报价两侧数据；未找到与当前条件等价的 SZ 聚合筛选 | 待补配对／计算规则 | 不属于替换 Wind 即可解决的问题；不要拿估值差或图表 Bid/Ofr 坐标冒充该条件。 |

### 9.3 必须显式迁移的地区规则 R04

SZ 在 A:92–119 和 `LikeBondFilter.xaml.cs` 中重复定义名单。按钮展开为地区树勾选，不是发出一个供应商 `regionCategory` 值：

| 分类 | 硬编码名单／例外 |
|---|---|
| 好地区 | 江苏、浙江、上海、北京、广东；浙江下排除宁波 |
| 中等地区 | 福建、湖北、湖南、四川、河北、河南、山东、安徽、山西、江西、陕西、重庆、青岛、厦门；另加入浙江下宁波 |
| 弱地区 | 甘肃、青海、广西、宁夏、新疆、西藏、海南、天津；另加入辽宁下大连 |
| 差地区 | 云南、贵州、辽宁、吉林、黑龙江、内蒙古；辽宁下排除大连 |

源码使用 `Text.Contains` 和地区树 `CheckedItems` 的父子筛选，而非六位码集合。迁移应保留省市例外的意图并以规范地区码和测试样本验证展开结果，不能只复制字符串数组。当前 condition 引擎不支持跨字段 OR，所以也不能简单展开成“省 IN 列表 AND 市 IN 列表”；可在 Java 资料派生阶段集中计算 `regionCategory`，然后沿用当前单字段匹配。

这些是**旧版本业务名单**，不是当前权威地区评级；迁移时需要记录名单版本。宁波／大连、省份已知但城市缺失、跨字段多选应有固定预期用例。

### 9.4 税字段有两套不同逻辑 R01／R06

- **筛选**：M:1042 的 `IsTaxFree = Origin?.taxFreeFlag == true`。F 的“非免税”分支取 `!=true` 或 `!IsTaxFree`，因此一个已存在的债券模型中 null 也进入旧“非免税”分支。当前 `isTaxExempt EQ false` 不匹配 null，这个差异必须显式选择，不能混入数据映射悄悄改变。
- **展示**：M:1106 起另外定义 `IsShowNoTax`／`IsShowTax`。先判断国债／地方政府债／政策性银行债，再按 `ParticularDate` 判断：免税展示区间 `[2025-01-01, 2025-08-08)`，征税展示区间 `[2025-08-08, 2026-01-01)`。日期来自 `particularyear` 按 `yyyyMMdd` 解析。
- 这两个展示属性不是 `taxFreeFlag` 的生产代码，也不是 filter 的替代来源。日期常量具有历史时效，不能为了补字段直接移植成当前税务规则。本节只记录源码行为，不解释为现行税务政策。

### 9.5 逻辑迁移清单与测试要求

| 规则 | 建议落点／处理 | 必测场景 |
|---|---|---|
| R01、R11、R13 | 资料适配层保留原始 nullable 值、名单／名称；显式规范化为协议值 | true/false/null、多标记同时为真、担保人精确匹配 |
| R02 | Java 资料派生层使用可固定的业务日期；先确认是否沿用客户端当天日期 | 上市前一天、当天、后一天、日期缺失 |
| R03 | 比较新旧范围边界；由协议决定兼容，不能改现有 BETWEEN 而影响其他字段 | 正好等于下界／上界、缺值、单位转换 |
| R04、R05 | 集中维护地区分类规则及地方债识别，供报价和行情共同使用 | 宁波／大连例外、非地方债、城市缺失、不同地区代码来源 |
| R06 | 单独的展示规则，暂不当作免税筛选规则迁移 | 四个日期边界及适用券种；确认规则有效期 |
| R07、R08、R09、R15 | 只移植已证实的标记逻辑；补齐当前分类优先级及旧版缺少的标签 | NCD 与信用债重叠、大行≠国有行、多个永续／热门标签、未知标记 |
| R10 | 先区分主体上市与债券上市；当前字段继续按产品定义，禁止偷换源字段 | 主体上市但债券未上市、反向场景、两标记缺失 |
| R12 | 中证到期／行权／净价分别接入；目前只确认资料可承载，未确认对应条件完整实现 | 估值口径、日期和缺值 |
| R14 | 先确定 OTHER 与 null 的分类，不能默认沿用“非到期偿付即提前还本” | 到期偿付、提前还本、其他字符串、空值 |
| R16 | 迁移明确的省／市／区 flag；旧 OtherArea 分支不能直接复用 | 无模型、有模型无层级、多层级同时为真；防止空引用 |
| R17 | 地方债识别后区分一般／专项；明确专项标记 null 的政策 | 非地方债、true/false/null 的专项标记 |

迁移目标是让 Java 统一产生资料属性与执行共享匹配，STC 客户端只提交条件；不用替换数据源的原始字段名污染业务协议。先构造独立预期夹具，再实现派生规则；不能仅用新旧代码互相比较证明业务正确。

### 9.6 本次来源文件指纹

用于确认后续拿到的 SZ 版本是否与本次相同；不是供应商来源证明。

| 文件 | SHA-256 |
|---|---|
| `STC.Common/Model/Wind/WindBondInfoOrigin.cs` | `8046b695a9208bbb4f0e7c11c9310da49248a8b3f1e19b1dbcc8fcd63d0d88e9` |
| `STC.Common/Model/Wind/WindBondInfo.cs` | `b4a7655da4743f45c92435545328580d954a8777c45b0b7fc132bee6b0719d6c` |
| `STC.SalesTrading.Plugin/View/Component/BondFilter/BondFilterViewModel.cs` | `7843b2539e2984d3565cba29e8fffa6e4df020f7e80ee2f4c867986cfe27f8c1` |
| `STC.SalesTrading.Plugin/View/Component/BondFilter/BondFilterView.xaml.cs` | `162f788258b0a57b480c1c3ab6e19db18cdf299ac6001437912c4fbb524b556e` |
| `STC.Common/Handler/Wind/WindHandler.cs` | `16584058b5df532255acddb1150eeaed4068115e19f2973f02a67f7448dbdcc8` |
