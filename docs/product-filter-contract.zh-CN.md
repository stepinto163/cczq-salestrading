# 产品筛选接口：前端接入与条件字典

更新时间：2026-09-21。适用于 Web、C# 客户端的报价管理、行情中心和报价提醒。

本文覆盖当前服务端全部 **79 个条件字段**，说明产品控件对应的字段、类型、单位、选项及目标传输值。完整接口结构见 [API 规格](ficc-api-spec.zh-CN.md)。

### 全字段检查范围

本轮按服务端字段目录逐项检查，不限于企业性质。79 个字段分为以下五类；每个字段的具体定义见第 4～8 节。

| 字段类别 | 数量 | 协议规则 |
|---|---|---|
| 分类枚举 | 37 | 完整定义显示名称及字符串代码；复用语义相同的 STC 协议值，没有对应项则使用本平台明确命名的代码 |
| 地区字典 | 4 | region、city、regionCode、localGovernmentRegion；统一发送地区代码，中文名称用于显示 |
| 自由文字 / 业务 ID | 6 | searchText、securityName、issuerName、guarantorName 为文字；securityId、issuerId 为基础资料 ID，不编造枚举 |
| 数值 | 18 | 保持数值筛选，逐项明确单位、范围及计算口径，不将区间改成无意义的枚举 ID |
| 布尔 | 14 | 使用 JSON true/false，不新增 Y/N、0/1 或中文别名 |

总计：37 + 4 + 6 + 18 + 14 = 79。分类字段的“其他”也有明确代码；外部来源未接入不再成为协议值留空的理由，但没有真实资料时仍需明确标记模拟数据。

### 协议约定：采用 STC 的代码与显示名称分离方式

**分类条件传稳定的字符串代码，中文名称仅用于显示。** 数值条件仍传数值，布尔条件仍传 true/false，名称搜索仍传用户输入的文字，不把所有字段都改成枚举。

- 字典统一使用 `{"value":"YQ","label":"央企"}`；前端显示 label，请求发送 value。
- 已核实含义一致的分类优先复用 STC **前后端协议代码**，不直接使用数据库原值或内部标签位编码。
- 保留本平台的 `field/operator/value` 条件结构；对齐 STC 的编码风格，不要求复制旧请求的全部字段名称。
- 后端适配器负责把 STC、DM 等数据源的原值转换为协议代码；前端不处理供应商字段，也不根据中文显示文字推断代码。
- 没有对应 STC 协议的分类，使用本文定义的可读英文代码（大写、下划线分词）；不编造无意义的数字 ID。既有 STC 代码保留原大小写。评级 `AAA` 等本身就是稳定代码，可与显示名称相同。
- 分类字典应通过现有 `dictionary.query` 的 `dictionaries` 返回完整 value/label 列表；`filterFields.examples` 继续只作为样例，不承担枚举定义。
- 分类选项使用 EQ/IN；CONTAINS 用于名称等自由文字搜索，不用于按英文枚举代码的部分字符选择业务类别。

以下代码已从 STC 客户端 `CheckedValue` → `GetFilterDictionary()` → `FuzzyFilter` 请求路径核实，作为本平台对应条件的目标协议：

| 本平台字段 | 显示名称 → 协议 value | STC 请求字段 |
|---|---|---|
| `ownershipType` | 央企 → `YQ`；国企 → `GQ`；民企 → `MQ`；其他 → `Other` | `enterpriseType` |
| `listedMarket` | 银行间 → `CIB`；上交所 → `SSE`；深交所 → `SZSE`；北交所 → `BSE` | `exchangeType` |
| `listingStatus` | 上市 → `SS`；非上市 → `NOTSS` | `onsaleType` |
| `issueMethod` | 公募 → `GM`；私募 → `SM`；小公募 → `XGM` | `scopInvestType` |

例如选择央企和国企，本平台发送：

```json
{"field":"ownershipType","operator":"IN","value":["YQ","GQ"]}
```

对应的目标字典内容：

```json
{
  "ownershipType": [
    {"value":"YQ","label":"央企"},
    {"value":"GQ","label":"国企"},
    {"value":"MQ","label":"民企"},
    {"value":"Other","label":"其他"}
  ]
}
```

**实施状态：Java、Web、C# 主客户端及 C# 学习示例已同步本版代码字典。** 发布时一起更新服务端和客户端；旧客户端仍发送中文分类值，会收到参数校验错误。服务加载已保存筛选组时转换已知旧值，后续保存时持久化新代码；无法识别的旧值保留并记录警告，需用户重新选择，不猜测业务含义。

枚举含义按字段隔离；不同字段可以复用相同代码。`OTHER`（或 STC 的 `Other`）只表示已明确归类为“其他”，不表示缺失数据。未知/未映射属性保持无值，使用 `EXISTS=false` 查询；未经字典定义的代码应返回参数错误。全部/不限通过省略条件表达。

特别纠正：企业性质不能直接从 `org_type` 推导。STC 的企业类型筛选协议使用 YQ/GQ/MQ/Other；BaseSvr 另一路标签实现读取 `Institution_Subtype` 的 CGE/LGE/PVE/OTE。这些是不同层的代码，不能混为同一个前后端协议字典。

## 1. 接入方式

| 操作 | 接口 / 处理方式 |
|---|---|
| 获取报价条件目录和字典 | `ficc.quote.dictionary.query` |
| 获取行情条件目录和字典 | `ficc.price.dictionary.query` |
| 应用报价管理条件 | `ficc.quote.view.open` 的 `filter` |
| 应用行情中心条件 | `ficc.price.view.open` 的 `filter` |
| 应用报价提醒条件 | `ficc.notification.view.open` 的 `filter`；共用本文字段 |
| 修改筛选条件 | 关闭旧 view，以新 `viewId` 调用对应 `view.open` |
| 滚动加载后续页 | 对应 `view.query`，发送 `viewId`、`pageNumber`、`pageSize`，复用已登记条件 |

首次使用实时视图时，先调用对应业务的 `stream.open`，订阅返回的 `pushTopic`，再调用 `view.open`。筛选条件放在请求体中，不拼进 topic。Quote、Price、Notification 仍使用各自的接口和消息模型。

“全部”“不限”表示**不发送该条件**。不要发送 `"全部"`、空字符串或空 IN 数组。数值框只填下限用 GTE，只填上限用 LTE，同时填写上下限用一条 BETWEEN。

### 示例：信用债行情

发送至 QuoteSvr，topic 为 `ficc.price.view.open`：

```json
{
  "requestId": "REQ-PRICE-001",
  "viewId": "CREDIT-BOARD-001",
  "mode": "FULL",
  "pageSize": 50,
  "filter": {
    "market": "CREDIT",
    "conditions": [
      {"field": "quoteType", "operator": "EQ", "value": "1"},
      {"field": "price", "operator": "BETWEEN", "value": ["2.00", "3.00"]},
      {"field": "settlType", "operator": "IN", "value": ["1", "2"]},
      {"field": "bondRating", "operator": "IN", "value": ["AAA", "AA+"]},
      {"field": "liquidityGrade", "operator": "IN", "value": ["S", "A"]},
      {"field": "isSubordinated", "operator": "EQ", "value": false}
    ]
  }
}
```

含义：信用债、收益率报价 2%～3%、非远期、债项评级 AAA 或 AA+、流动性 S 或 A、非次级债。报价管理使用 Quote 接口及其请求结构，不复用 Price 的 `mode`。

## 2. 条件格式与运算符

```json
{"field": "bondRating", "operator": "IN", "value": ["AAA", "AA+"]}
```

`field` 为本文字段名，`operator` 为大写运算符，`value` 为传输值。`conditionId`、`displayOrder` 可选，用于条件编辑组织，不改变匹配逻辑。

| 运算符 | 含义 | value 示例 |
|---|---|---|
| `EQ` | 等于 | `"AAA"`、`"2.5000"`、`true` |
| `IN` | 命中任意一个选项 | `["AAA","AA+"]` |
| `CONTAINS` | 文本包含，忽略大小写；不是前缀或分词搜索 | `"银行"` |
| `GT` / `GTE` | 大于 / 大于等于 | `"2.5"` |
| `LT` / `LTE` | 小于 / 小于等于 | `"3.0"` |
| `BETWEEN` | 下限至上限，包含两端 | `["2.0","3.0"]` |
| `EXISTS` | 有值 / 无值；不是判断布尔值是否为 true | `true` / `false` |

各条件表按以下类型确定允许运算符：

| 类型 | JSON 值与允许运算符 |
|---|---|
| 自由文字 / ID（string） | 字符串；EQ、IN、CONTAINS、EXISTS |
| 分类 / 地区代码（string） | 字符串；EQ、IN、EXISTS；目录返回 enumValues |
| 数值（decimal） | 建议使用十进制字符串；EQ、IN、GT、GTE、LT、LTE、BETWEEN、EXISTS |
| 布尔（boolean） | JSON true / false，不加引号；EQ、EXISTS |

- **同字段多个条件取 OR，不同字段取 AND。** 区间不能拆成同字段的 GTE 和 LTE，否则会变成“满足任意一边”。
- 标签数组的 EQ / IN 命中任意元素即可；多个同字段条件不能表达“同时包含全部标签”。
- 缺失、null、空白字符串不匹配普通条件；查询无值使用 EXISTS=false。布尔 false 是有效值，不等于无值。
- 文本 EQ / IN 按实际值精确匹配；名称与代码不可混用。前端选项发送代码，旧值转换仅发生在后端资料适配及已保存条件加载环节。
- 暂无 NOT_IN、通用表达式或任意嵌套 AND/OR。评级高低不能按字符串顺序比较，应枚举所需等级。

## 3. 如何使用字典

请求 `ficc.quote.dictionary.query`：

```json
{"requestId": "REQ-DICT-001"}
```

应答结构示例，仅保留一个业务字典及一个条件字段：

```json
{
  "Code": "0000",
  "Msg": "成功",
  "requestId": "REQ-DICT-001",
  "Data": {
    "dictionaries": {
      "side": [
        {"value": "1", "label": "买入"},
        {"value": "2", "label": "卖出"}
      ]
    },
    "filterFields": [
      {
        "field": "liquidityGrade",
        "label": "债券流动性",
        "group": "流动性",
        "type": "string",
        "scope": "SECURITY",
        "operators": ["EQ", "IN", "EXISTS"],
        "enumValues": [
          {"value":"S","label":"S"}, {"value":"A","label":"A"},
          {"value":"B","label":"B"}, {"value":"C","label":"C"}, {"value":"D","label":"D"}
        ],
        "enabled": true,
        "examples": ["S", "A", "B"],
        "includesSimulatedData": true
      }
    ]
  }
}
```

前端需区分以下三类取值：

| 类别 | 使用规则 |
|---|---|
| 分类字典 | dictionaries 按字段名返回本版 37 个分类及 4 个地区字典的 value/label 列表，包括尚无报价匹配的分类选项；另有 market 及 Price 的 mode。city 在初始列表上补充当前基础资料中已归一化的城市 |
| 产品选项 | 以下表格中“名称=代码”表示 label=value；前端只发送等号右侧的代码。标注“STC”表示复用已核实协议，“新增”表示本平台定义 |
| 动态取值 | 债券/发行人 ID、城市代码通过基础资料字典获取；名称搜索仍使用文字。供应商原值由后端转换，前端不透传源代码。filterFields.examples 仅为缓存采样，**不是完整枚举接口** |

`scope=ENTRY/SECURITY/BOOK` 分别表示报价、债券属性和价格簿相关字段。它是目录说明，不表示所有价格簿组合算法已经实现。`enabled=true` 表示字段允许提交，不保证有真实数据。`includesSimulatedData=true` 表示该字段存在模拟资料，并非每条记录均为模拟值。

## 4. 报价条件字典

“无枚举”表示直接输入相应类型的值；各类型允许的运算符见第 2 节。

| 产品条件 | field | 类型 / 单位 | 显示值 → 传输值或输入规则 |
|---|---|---|---|
| 综合搜索 | `searchText` | 文本 | 无枚举；CONTAINS 搜索债券代码/名称、发行人、机构/联系人名称、报价归属用户 ID/显示名 |
| 债券代码 | `securityId` | 文本 | 动态；基础资料完整代码，如 `"250001.IB"` |
| 债券名称 | `securityName` | 文本 | 动态；名称或部分文字，如 `"国债"` |
| 买卖方向 | `side` | 文本 | Bid / 买入 → `"1"`；Ofr / 卖出 → `"2"` |
| 计价方式 | `quoteType` | 文本 | 收益率 → `"1"`；净价 → `"2"`；全价 → `"3"` |
| 收益率类型 | `yieldType` | 文本 | 到期 → `"1"`；行权 → `"2"`；用于收益率报价 |
| 报价价格 | `price` | 数值 | 无枚举；收益率用百分数，`"2.5000"` 表示 2.5%；净价/全价用价格值。应同时限定 quoteType |
| 报价数量 | `quantity` | 数值 / 万 | 无枚举，如 `"1000"` |
| 清算速度 | `settlType` | 文本 | 见第 4.1 节；不要发送 `"T+0"` 等显示文字 |
| 急 | `urgent` | 布尔 | 是 → true；否 → false |
| 荐 | `recommended` | 布尔 | 是 → true；否 → false |
| 整 | `whole` | 布尔 | 是 → true；否 → false |
| OCO | `oco` | 布尔 | 是 → true；否 → false |
| 打包 | `packing` | 布尔 | 是 → true；否 → false |
| Bid | `bidPrice` | 数值 | 无枚举；仅买方报价有值，单位由 quoteType 决定 |
| Ofr | `offerPrice` | 数值 | 无枚举；仅卖方报价有值，单位由 quoteType 决定 |
| Vol.Bid | `bidQuantity` | 数值 / 万 | 无枚举；买方数量，最低数量用 GTE |
| Vol.Ofr | `offerQuantity` | 数值 / 万 | 无枚举；卖方数量，最低数量用 GTE |
| Bid-中债 | `bidValuationSpread` | 数值 / BP | 无枚举；（买方收益率 − 中债估值收益率）×100 |
| 中债-Ofr | `offerValuationSpread` | 数值 / BP | 无枚举；（中债估值收益率 − 卖方收益率）×100 |
| Bid净价-中债净价 | `bidCleanPriceSpread` | 数值 / 价格差 | 无枚举；买方净价 − 中债净价估值，不乘 100 |
| 中债净价-Ofr净价 | `offerCleanPriceSpread` | 数值 / 价格差 | 无枚举；中债净价估值 − 卖方净价，不乘 100 |
| Bid-Ofr | `bidOfferSpread` | 数值 / BP | 无枚举；当前仅有模拟值，真实双边价差算法未实现 |

### 4.1 清算速度

STC 代码均以字符串发送，T+4 后不能按数字顺延推算。

| 显示 | value | 显示 | value | 显示 | value |
|---|---|---|---|---|---|
| T+0 | `"1"` | T+1 | `"2"` | T+2 | `"3"` |
| T+3 | `"4"` | T+4 | `"5"` | T+5 | `"9"` |
| T+6 | `"10"` | T+7 | `"11"` | T+8 | `"12"` |
| T+9 | `"13"` | T+10 | `"14"` | T+11 | `"15"` |
| T+12 | `"16"` | | | | |

| 产品组合选项 | 条件 |
|---|---|
| 非远期 | `settlType IN ["1","2"]` |
| 远期 | `settlType IN ["3","4","5","9","10","11","12","13","14","15","16"]` |

不选清算速度则不限制以上类型。当前没有独立的“明+0 / 明+1”筛选编码。

## 5. 债券与流动性条件字典

本节列出目标协议代码。名称只用于显示，多选发送代码数组。

| 产品条件 | field | 类型 / 单位 | 选项值 / 输入规则 |
|---|---|---|---|
| 细分券种 | `bondSubtype` | 文本 | STC：国债=`National`、国开=`CDB`、农发=`AGB`、口行=`NEB`、地方债=`LGB`、短融=`CP`、中票=`MTN`、企业债=`EnterpriseBond`、公司债=`CorporatebBonds`、PPN=`PPN`、ABS=`ABS`、NCD=`NCD`、其他=`Other`；新增：金融债=`FINANCIAL_BOND`、可转债=`CONVERTIBLE_BOND`。`CorporatebBonds` 保留旧协议拼写；综合金融债不借用旧“政策性金融债”含义 |
| 产品分类 | `productType` | 文本 | 新增：利率债=`RATE`、信用债=`CREDIT`、NCD=`NCD`；与外层 market 仍是独立字段 |
| 热门标签 | `hotTags` | 文本，资料可为数组 | 见第 5.1 节；命中任意已选标签 |
| NCD类型 | `ncdType` | 文本 | 新增：国有=`STATE_OWNED`、股份=`JOINT_STOCK`、城商=`CITY_COMMERCIAL`、农商=`RURAL_COMMERCIAL`、外资=`FOREIGN`、其他=`OTHER` |
| 剩余期限 | `remainingYears` | 数值 / 年 | 无枚举；年直接发送，月÷12，日÷365；预设见第 5.2 节 |
| 发行人 | `issuerName` | 文本 | 动态；名称通常用 CONTAINS |
| 债券上市状态 | `listingStatus` | 文本 | STC：上市=`SS`、非上市=`NOTSS` |
| 发行方式 | `issueMethod` | 文本 | STC：公募=`GM`、小公募=`XGM`、私募=`SM` |
| 中债估值 | `valuationYield` | 数值 / % | 无枚举，如 `"2.45"`；最新日估值 |
| 中证估值 | `csiValuationYield` | 数值 / % | 无枚举，如 `"2.50"`；真实来源尚未全部接入 |
| 中债净价估值 | `valuationCleanPrice` | 数值 / 价格 | 无枚举，如 `"100.00"` |
| 票面利率 | `couponRate` | 数值 / % | 无枚举，如 `"3.00"` |
| 债券余额 | `outstandingAmount` | 数值 / 亿 | 无枚举，如 `"20.00"`；不要按数量字段的“万”传输 |
| 高收益债 | `isHighYield` | 布尔 | 是 → true；否 → false；前端不自行设收益率阈值推断 |
| 一级半 | `isPreListing` | 布尔 | 是 → true；否 → false |
| 是否免税 | `isTaxExempt` | 布尔 | 免税 → true；非免税 → false |
| 债券流动性 | `liquidityGrade` | 文本 | 新增：S=`S`、A=`A`、B=`B`、C=`C`、D=`D`；等级判定由后端数据源提供 |
| 做市标的 | `marketMakerTags` | 文本，资料可为数组 | 新增：上证信用=`SSE_CREDIT`、深证信用=`SZSE_CREDIT`、上证利率=`SSE_RATE`、深证利率=`SZSE_RATE`；资料可包含多个代码 |

### 5.1 热门标签

| 显示名称 | value | 来源 |
|---|---|---|
| 商金债 | `SJZ` | STC |
| 银行二级 | `YHEJ` | STC |
| 银行永续 | `YHYX` | STC |
| 政策性次级 | `POLICY_BANK_SUBORDINATED` | 新增 |
| 保险次级 | `BXCJ` | STC |
| 券商债 | `SECURITIES_COMPANY_BOND` | 新增；不等同旧“券商普通” |
| 保险债 | `INSURANCE_BOND` | 新增 |
| 铁道债 | `TD` | STC |
| 可质押 | `PLEDGEABLE` | 新增标签代码 |
| 跨市场 | `CROSS_MARKET` | 新增标签代码 |
| 城投 | `CTZ` | 复用 STC 城投类别码 |
| 房地产 | `DC` | STC |
| 产能过剩 | `OVERCAPACITY` | 新增 |
| 公用事业 | `UTILITIES` | 新增 |
| 上市公司 | `LISTED_COMPANY` | 新增 |
| 科创债 | `KC` | STC |
| TLAC债 | `TLAC` | 新增 |
| 新上市 | `NEWLY_LISTED` | 新增 |
| 不含假期 | `EXCLUDE_HOLIDAYS` | 新增；具体标签判定由后端维护 |
| 绿色债 | `LZ` | STC |

```json
{"field":"hotTags","operator":"IN","value":["LZ","KC"]}
```

标签数组命中任意已选标签即可。`hotTags=CTZ` 与 `isMunicipal=true` 仍是不同字段；后端归一化时应保证相同业务分类一致。

### 5.2 剩余期限预设

| 显示选项 | operator | value（年） |
|---|---|---|
| <3M | LT | `"0.25"` |
| 3-6M / 6-9M / 9-12M | BETWEEN | `["0.25","0.5"]` / `["0.5","0.75"]` / `["0.75","1"]` |
| 1-3Y / 3-5Y / 5-7Y / 7-10Y | BETWEEN | `["1","3"]` / `["3","5"]` / `["5","7"]` / `["7","10"]` |
| 10-15Y / 15-20Y | BETWEEN | `["10","15"]` / `["15","20"]` |
| >20Y | GT | `"20"` |

区间包含两端，相邻预设边界会重叠；多个预设取 OR。自定义上下限也转换成年后发送。

## 6. 信用条件字典

评级代码本身可作为显示名称。下表列出第一版产品选项；目标字典必须返回完整可选列表，不从已到达的报价样本推断。“更多”仅展示字典中已定义的扩展等级。

| 产品条件 | field | 类型 | 选项值 / 输入规则 |
|---|---|---|---|
| 上市场所 | `listedMarket` | 文本 | STC：银行间=`CIB`、上交所=`SSE`、深交所=`SZSE`、北交所=`BSE` |
| 发行人代码 | `issuerId` | 文本 | 动态；基础资料发行人 ID，不是 CRM 机构 ID |
| 发行人类型 | `issuerType` | 文本 | 沿用本平台已有码：政府=`GOVERNMENT`、政策银行=`POLICY_BANK`、企业=`CORPORATE`；新增：商业银行=`COMMERCIAL_BANK`、证券公司=`SECURITIES_COMPANY`、保险公司=`INSURANCE_COMPANY`、其他金融机构=`OTHER_FINANCIAL`、其他=`OTHER`。这是机构类别，不是企业所有制 |
| 银行业 | `bankType` | 文本 | STC：股份制=`GF`、城商行=`CS`、农商行=`NS`；新增：国有行=`STATE_OWNED`、外资行=`FOREIGN`、其他=`OTHER`。旧“大行=DH”与“国有行”并非必然同一集合，不强行复用 |
| 行业 | `industry` | 文本 | 新增：城投=`CITY_INVESTMENT`、房地产=`REAL_ESTATE`、煤炭=`COAL`、钢铁=`STEEL`、化工=`CHEMICAL`、建筑=`CONSTRUCTION`、交运=`TRANSPORT`、公用事业=`UTILITIES`、金融=`FINANCE`、其他=`OTHER`；为本产品筛选分类，不声称等同完整申万行业字典 |
| 企业性质 | `ownershipType` | 文本枚举 | STC：央企=`YQ`、国企=`GQ`、民企=`MQ`、其他=`Other`；不再透传 org_type |
| 担保 | `guaranteeType` | 文本 | STC：有担保=`YDB`、无担保=`WDB`；新增：全国性担保=`NATIONAL_GUARANTEE`、地方性担保=`LOCAL_GUARANTEE`。归一化属性使用标签数组：全国性担保同时包含 YDB 和 NATIONAL_GUARANTEE，地方性同理 |
| 担保人 | `guarantorName` | 文本 | 动态；名称或部分文字，用 CONTAINS |
| 主体评级 | `issuerRating` | 文本 | 产品值：AAA、AA+、AA、AA-、A+、A |
| 中债资信 | `issuerCbrRating` | 文本 | 产品值：AAA+、AAA、AAA-、AA+ |
| 评级展望 | `ratingOutlook` | 文本 | 新增：正面=`POSITIVE`、稳定=`STABLE`、观察=`WATCH`、负面=`NEGATIVE` |
| 债项评级 | `bondRating` | 文本 | 产品值：AAA、A-1、AA+、AA、AA- |
| 中债隐含 | `bondCbrRating` | 文本 | 产品值：AAA+、AAA、AAA-、AA+ |
| YY评分 | `yyRating` | 文本 | 新增：投资级=`INVESTMENT_GRADE`、投机级=`SPECULATIVE_GRADE`、风险级=`HIGH_RISK`；本平台分类，外部原始分数如何归类由后端映射 |
| 中证隐含 | `csiRating` | 文本 | 产品值：AAA+、AAA、AA+、AA、AA2 |

## 7. 条款条件字典

| 产品条件 | field | 类型 | 选项值 / 输入规则 |
|---|---|---|---|
| 含权属性 | `optionType` | 文本 | 第 7.1 节；沿用本平台已使用的明细代码，不能与旧客户端 HQ/FHQ 的汇总开关混用 |
| 是否含权 | `hasExerciseOption` | 布尔 | 含权 → true；不含权 → false；当前 STC 转换范围见第 7.1 节 |
| 偿还方式 | `repaymentType` | 文本 | STC：到期=`DQCF`、提前还本=`TQHB`；新增：其他=`OTHER` |
| 计息方式 | `couponType` | 文本 | 新增：固定利率=`FIXED`、浮动利率=`FLOATING`、零息=`ZERO_COUPON`、其他=`OTHER`；FRB、FRN、FIX 等源值由后端映射，不作为新协议值 |
| 付息方式 | `couponFrequency` | 文本 | 第 7.2 节；不要用 couponType 代替 |
| 永续债类型 | `perpetualType` | 文本 | 新增：城投永续=`MUNICIPAL_PERPETUAL`、产业永续=`CORPORATE_PERPETUAL`、金融永续=`FINANCIAL_PERPETUAL`、非永续=`NON_PERPETUAL` |
| 是否永续 | `isPerpetual` | 布尔 | 永续 → true；非永续 → false |
| 次级债 | `isSubordinated` | 布尔 | 次级 → true；非次级 → false |
| 利率方式 | `rateIndex` | 文本 | STC：SHIBOR=`Shibor`、DEPO=`Depo`、LPR=`LPR`、固息=`Regular`；新增：其他=`OTHER`。注意大小写，不发送 SHIBOR 或 DEPO 代替协议值 |
| 发行年份 | `issueYear` | 数值 / 年份 | 无固定枚举；产品按钮为 `"2026"`、`"2025"`、`"2024"`、`"2023"`，也可提交其他年份 |

### 7.1 含权属性

| 显示 | 条件值 |
|---|---|
| 不含权 | `EQ "NON"` |
| 含权 | `IN ["PUT","CNP","CAL","ETS","ASS","DCN","CNV"]` |
| 赎回权 | `IN ["CAL","CNP"]` |
| 回售权 | `IN ["PUT","CNP"]` |
| 其他权 | `IN ["ETS","ASS","DCN","CNV"]` |

CNP 同时属于赎回/回售组合。本字段暂沿用已经定义的 NON/PUT/CNP/CAL/ETS/ASS/DCN/CNV 明细范围，不另造一套重复代码。SLB、OTH、PRS 暂不进入可选字典；后端不能将未知明细静默归为不含权，新增选项须明确含义后扩展。

### 7.2 付息方式

| 显示 | 协议 value | 来源 |
|---|---|---|
| 按月 | `M1` | STC |
| 按季 | `M3` | STC |
| 半年 | `M6` | STC |
| 按年 | `Y1` | STC |
| 其他 | `Other` | STC |

前端不再发送 A/M/Q/S/N 或次数别名。后端负责转换已核实的付息频率；缺失值保持缺失，不归入 Other。其他明确但不属于四种常规频率的值才归入 Other。

## 8. 区域与其他属性条件字典

| 产品条件 | field | 类型 | 选项值 / 输入规则 |
|---|---|---|---|
| 城投 | `isMunicipal` | 布尔 | 城投 → true；非城投 → false |
| 地区 / 省份 | `region` | 文本 | 目标：省级地区代码，见第 8.1 节；UI 显示中文名称，发送代码，不发送名称别名 |
| 城市 | `city` | 文本 | 动态城市字典：value 为本平台规范地区代码、label 为城市名，例如上海市=`310100`；无映射时不透传供应商代码。完整城市列表由基础资料提供 |
| 省份代码 | `regionCode` | 文本 | 省级地区代码，与 region 使用同一代码字典；保留现有字段作为代码筛选入口。前端通常只需提交 region，避免重复条件 |
| 重点省份 | `keyProvince` | 文本 | 新增：12重点省=`KEY_12`、非12重点省=`NON_KEY_12`；名单及适用日期由后端资料维护 |
| 城投层级 | `municipalLevel` | 文本 | STC：省级=`Province`、市级=`City`、区县级=`District`、其他=`Other` |
| 地区等级 / 区域评级 | `regionalRating` | 文本 | 新增暂定分档：区域A级=`REGION_A`、区域B级=`REGION_B`、区域C级=`REGION_C`、区域D级=`REGION_D`。先用于模拟联调；非供应商正式评级，不据此制定真实评级阈值 |
| 地区分类 | `regionCategory` | 文本 | 新增：好地区=`GOOD`、中等地区=`AVERAGE`、弱地区=`WEAK`、差地区=`POOR`；旧模拟值“中部地区”作废，不按地理方位推断等级 |
| 地方债地区 | `localGovernmentRegion` | 文本 | 与 region 使用同一地区字典，例如广东省=`440000`、山东省=`370000`、浙江省=`330000`、江苏省=`320000` |
| 地方债类型 | `localGovernmentType` | 文本 | STC：一般债=`Normal`、专项债=`Special` |
| 地方债专项 / 用途 | `localGovernmentUse` | 文本 | 新增：棚改=`SHANTYTOWN_RENOVATION`、土储=`LAND_RESERVE`、收费公路=`TOLL_ROAD`、基础设施=`INFRASTRUCTURE`、其他=`OTHER` |
| 跨市场 | `isCrossMarket` | 布尔 | 是 → true；否 → false |
| 可质押 | `isMortgage` | 布尔 | 是 → true；否 → false |

### 8.1 地区名称

地区属于代码字典，不使用城市或省份名称作为协议值。以下覆盖当前产品地区选项，代码按字符串发送；本项目采用六位地区码，后端负责供应商转换及后续字典维护。

| 显示名称 | value | 显示名称 | value |
|---|---|---|---|
| 北京市 | `110000` | 天津市 | `120000` |
| 河北省 | `130000` | 山西省 | `140000` |
| 内蒙古自治区 | `150000` | 辽宁省 | `210000` |
| 吉林省 | `220000` | 黑龙江省 | `230000` |
| 上海市 | `310000` | 江苏省 | `320000` |
| 浙江省 | `330000` | 安徽省 | `340000` |
| 福建省 | `350000` | 江西省 | `360000` |
| 山东省 | `370000` | 河南省 | `410000` |
| 湖北省 | `420000` | 湖南省 | `430000` |
| 广东省 | `440000` | 广西壮族自治区 | `450000` |
| 海南省 | `460000` | 重庆市 | `500000` |
| 四川省 | `510000` | 贵州省 | `520000` |
| 云南省 | `530000` | 西藏自治区 | `540000` |
| 陕西省 | `610000` | 甘肃省 | `620000` |
| 青海省 | `630000` | 宁夏回族自治区 | `640000` |
| 新疆维吾尔自治区 | `650000` | 深圳市 | `440300` |
| 厦门市 | `350200` | 宁波市 | `330200` |
| 青岛市 | `370200` | 大连市 | `210200` |

省级选项提交 region（地方债地区提交 localGovernmentRegion）；单独选择深圳等城市时提交 city，不把城市代码作为省份值。多个同级选项用 IN。当前引擎不支持“广东省 OR 大连市”这类跨字段 OR；前端暂不能在一个组合里混选省级与城市级选项。

```json
{"field":"region","operator":"IN","value":["440000","320000"]}
```

全国/不限通过省略地区条件表达；城市字典应包含所属省份关系，避免前端自行推断。此表是本产品初始选项，不声称涵盖全部行政区划。

## 9. 不属于 conditions 的范围条件

| 属性 | 位置 | 取值与说明 |
|---|---|---|
| 利率 / 信用 | `filter.market` | `"RATE"` / `"CREDIT"`；省略表示不限制 |
| 关注债券分组 | `filter.watchlistIds` | 已保存分组 ID 数组，不是债券代码数组 |
| 筛选分组 | `filter.filterGroupIds` | 已保存筛选组 ID 数组，不是字段名数组 |

```json
{
  "market": "CREDIT",
  "watchlistIds": ["WATCH-001"],
  "filterGroupIds": ["GROUP-001"],
  "conditions": [
    {"field": "quantity", "operator": "GTE", "value": "1000"}
  ]
}
```

多个关注组取成员并集，多个筛选组取匹配并集，再与临时条件取交集。空 ID 数组表示不启用限制；选中没有成员的关注组则匹配空集。报价归属、报价状态、提醒类型等按各业务 API 放在对应位置，不擅自增加为 conditions.field。

## 10. 当前实现边界

1. **条件可提交，不等于真实数据齐全。** 流动性、YY/区域评级、做市标的等尚待外部资料接入。模拟值按字段标记 `filterDataSources.<field>=SIMULATOR`；STC 环境仅在显式启用 `base.filter.simulateMissing=true` 时补充缺失值，不覆盖真实资料。
2. **代码字典完成不等于真实数据源齐全。** 新增分类使用明确标记的模拟资料；供应商接入后转换为同一代码，前端无需改值。REGION_A～REGION_D 等缺少来源的分档仅用于联调，不据此推断真实评级阈值。尚未接入的城市/供应商分类仍需要基础资料映射，不将未知值归为“其他”。
3. **价格单位不可混用。** 收益率偏离要求到期/行权口径一致，缺估值或口径不一致时无值；净价偏离使用独立字段。当前不自动将净价换算为收益率，也不自动调整交割日口径。历史报价使用最新日估值及当前参考属性。
4. **行情当前先筛单条报价，再生成价格簿。** 同时限定 Bid 和 Ofr 的单边字段，会要求同一条报价具有两边数据，当前不能表达“两边分别满足条件”。bidOfferSpread 目前是固定模拟样例 5 BP，不是真实双边价差。行情排序仍要求可比较的收益率，净价筛选字段存在不代表行情净价换算已接入。

前端可直接使用 filterFields.enumValues 构建选项控件；与 dictionaries 中同名字段的代码一致。原 Web 产品布局保留，C# 完整条件编辑器通过“选择”按钮显示中文名称并填入协议代码。
