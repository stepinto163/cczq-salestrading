# 销交平台原型 0910 增量对比 → STC.SalesTrading.Plugin 改动清单

> 生成日期：2026-09-10
> 新版原型：`.tmp/prd/销交平台原型0910`（2026-09-10 导出，index.html + _files）
> 对比基线：`.tmp/prd/ficc-client`（0811 原型源码，即 `PROTOTYPE_ANALYSIS.md` 的分析对象）
> 说明：`.tmp/prd/FICC销售智能交易平台.html`（0811 单页导出）比 ficc-client 更早，缺少 `modules/quote-canonical.js`，不作为基线。
> **界面差距另见 `PROTOTYPE_UI_GAP_0910.md`**（只列 UI/交互/布局，含 WPF 文件行号与验收方式）；本文件偏业务规则、接口与数据模型。

---

## 0. 差异总览

| 文件 | 基线 | 0910 | 结论 |
|---|---|---|---|
| app.js | 8808 行 | 9336 行 | **+528 行，主战场** |
| app.css | 671 行 | 705 行 | +34 行 |
| filter-logic.js | 61 行 | 67 行 | +新增 `quotesOppositeForAlert` |
| index.html | 1171 行 | — | 术语改名 + 新元素 + 导入按钮隐藏 |
| requirement-spec-page.js | 429 行 | 431 行 | 规格文案同步（可当权威变更说明） |
| requirements-page.js/css | — | 改名 `requirements-reference-page.*` | 无功能变化 |
| quote-canonical.js / quote-utils.js / api-client.js | — | — | **完全无变化** |
| backend-spec-data.js | — | — | 基本无变化 |
| data-model-spec-data.js | 53 表 / 668 字段 | **62 表 / 767 字段** | 主库新增 9 张表 |
| srs-integrated-data.js | 166 KB | 183 KB | 需求管理页数据 |

新增的数据模型表（9 张）：`alert_read`、`quote_realtime_events`、`sse_tickets`、`login_audit_events`、`schema_migration_audit`、`data_retention_runs`、`recommendation_quote_send_audit`、`recommendation_quote_send_outbox`、`stc_institution_trader_directory`。

---

## 1. 全局术语改名：意向报价 → 模糊报价

原型把行情中心子页「意向报价」整体改名为「模糊报价」，所有文案同步改名（后端 `/api/board-intents` 接口名和 `board_intent` 表名未变）。

改动点：

1) 行情中心子页标签：`意向报价` → `模糊报价`（data-st 仍为 `board-intent`）。
2) 识别/保存/状态/导出全链路提示文案：`意向报价识别失败`、`意向报价保存失败`、`只能更新自己的意向报价`、`请先选择意向报价` 等 → `模糊报价…`。
3) 确认弹窗标题：`意向报价版本冲突` → `模糊报价版本冲突`。
4) 导出：Sheet 名 `意向报价` → `模糊报价`；文件名 `意向报价_*.xlsx` → `模糊报价_*.xlsx`；列头 `意向报价ID` → `模糊报价ID`。
5) 代码注释：`意向报价筛选引擎` / `意向报价左侧面板` → `模糊报价…`。
6) 需求规格/SRS 段落 `意向报价识别` → `模糊报价识别`、接口清单「意向报价」分组 → 「模糊报价」。

插件对应位置：`Modules/MarketCenter/ViewModel/MarketCenterViewModel.cs`（SubTabItem 文案）、`BoardIntentViewModel.cs`（Title、Toast）、`IntentRowViewModel.cs`、`FuzzyQuoteViewModel.cs`、`Abstractions/Models/BoardIntentRow.cs`、`Enums.cs`。

---

## 2. 接口层变更

| 变更 | 接口 | 说明 |
|---|---|---|
| 新增（客户端不实现） | `POST /api/quote-confirm/mock` | QT 模拟下行，**原型侧测试入口**（2026-09-10 确认客户端不需要，不计入接口清单）。入参 `{count:5}`，返回 `{success,batchCount,quoteCount/generatedCount}`；非生产环境随机从 50 条模板中取 5 家不同机构各 1 条，生成 5 个 QT 待确认批次 |
| 新增 | `POST /api/quotes/price-conversions` | 批量价格换算（注意是复数）。入参 `{items:[{clientKey,securityId,quoteValue,priceType,direction,tradeDate,settlementType}]}`；出参 `{success,items:[{clientKey,yieldValue,cleanPrice,fullPrice...}]}` |
| 新增 | `GET/POST/PUT /api/fuzzy-quotes`（含 `/bulk`、`/status`、`/stats`） | 模糊报价新路径。前端 `fuzzyQuoteFetch()` 先打 `/fuzzy-quotes`，返回 404/405/501 时**回退** `/board-intents`（兼容旧后端） |
| 参数新增 | `POST /api/quotes/price-conversion` | 新增 `targetPriceType`，用于「价格类型切换 = 语义换算」 |
| 兼容能力 | `/api/health` | 需求规格要求声明 `board-intents.status.v1`、`quotes.canonical-state.v2`（与基线一致，无新增） |

---

## 3. 行情中心

### 3.1 选中态冻结 + 「有新报价」横幅（新增机制）

1) 新增状态：`boardSelectionMode`（none / auto / manual）、`boardPendingNewQuoteCount`、`boardFrozenProjection`、`boardAutoSelectSuppressed`。
2) 进入页面 / 切换子页 / `renderAll` 后执行 `setBoardAutoSelection()`：自动选中当前视图第一条非「未报价」行（mode=auto），并联动右侧推荐目标。
3) 用户点击行或拖拽框选 → mode=manual，并把当前投影快照冻结到 `boardFrozenProjection`。
4) manual 状态下，全量 `loadQuoteState`、SSE `new_quote`/`edit_quote` 都不再改写列表；新增报价只累计计数并显示横幅 `#boardNewQuoteBanner`「有 N 条新报价，点击刷新」。
5) 点击横幅 → `refreshFrozenBoardProjection()`：解除冻结 + `loadQuoteState({forceBoardRefresh:true})`。
6) 切换市场子页 / 数据源重置时清空该机制状态。
7) 新增 `openBestQuoteInAll(quoteId)`：跳转到对应市场「全部」页、切 manual、选中并高亮该报价。

### 3.2 最优报价算法重写（影响最大）

| 维度 | 基线 | 0910 |
|---|---|---|
| 分组键 | `代码\|价格类型\|清算速度`（quotePairKey） | **仅债券代码** |
| 比价口径 | 按价格类型分别比（收益率 Bid 小优；净价/全价 Bid 大优） | **统一折算为收益率后比**（Bid 小优 / Ofr 大优） |
| 折算来源 | 无 | `POST /api/quotes/price-conversions` 批量换算，结果挂 `quote._bestPriceConversion`，缓存 `bestQuoteConversionCache` |
| 同档多笔 | 每侧只留一条 | 收益率 `toFixed(4)` 相同视为同档，保留**多笔**：`bidBestQuoteIds[]` / `ofrBestQuoteIds[]`，代表行取同档内最新 |
| 时间列 | 沿用原行 | 取同档最新报价时间（`bestQuoteDisplayTime`） |
| 交互 | 仅单元格 hover 换算 tooltip | 报价单元格前新增 `N` 按钮（>1 笔才出现），hover 弹「同档最优报价浮层」：价格 / 折算收益率 / 量 / 报价机构 / 报价交易员 / 长城交易员 / 报价时间 |
| 权限 | — | 浮层中**仅本人报价**可点击（`.best-quote-layer-own`），点击跳「全部行情」并选中高亮；非本人报价的机构/交易员显示 `--` 且不可点击 |
| 刷新 | — | `rebuildBoardProjections` 末尾检测缺失折算 → `refreshBestQuoteConversions`（带序号防竞态）→ 重新投影 + `renderAll` |
| 缓存清理 | — | 换算失败的 `null` 缓存项在下一次 `loadQuoteState` 时清除 |

对应插件：`Abstractions/Models/BoardBestRow.cs` 需新增 `BidBestQuoteIds` / `OfrBestQuoteIds` / `BidBestYield` / `OfrBestYield`；`Mock/MockMarketCenterService.BuildBestRows`（现按 `Code,PriceType,Settlement` 分组）需重写；`MarketCenter.Detail` 的报价换算 tooltip 需扩展为浮层。

### 3.3 筛选分组支持「子分组」

1) 数据结构：分组条件变为二维 `subgroups: [[条件…]…]`，同时保留拍平视图 `conditions` 供查询复用；兼容旧数据 `conditions[].subgroup` 数字分组。
2) 查询语义**不变**：仍按「同字段 OR、跨字段 AND」拍平执行，子分组只影响维护界面表达（规格原文：「查询语义继续拍平为同字段OR、跨字段AND」）。
3) UI：左栏「已选条件」区改为多个子分组卡片（`.fgm-subgroup`），chip 可逐个删除，卡片右上角删除子分组（非空需二次确认），底部固定「+ 新增分组」；空子分组保存时自动丢弃。
4) 右栏条件目录改为**从当前筛选面板动态生成**：`availableMarketFilterGroupData(market)` 扫描面板 DOM 产出 `{sec, groups:[{l,key,type,items,hasText,hasRange,enabled,disabledReason}]}`；据此提供 tag 多选、文本输入「添加」、下拉「添加」、区间 min/max（期限带 Y/M/D 单位）「添加」。
5) 按市场收窄目录：
   - credit：债券筛选 / 流动性筛选 / 信用筛选 / 条款筛选 / 区域筛选
   - rate：债券筛选 / 流动性筛选 / 条款筛选 / 区域筛选（排除报价筛选、信用筛选）
6) 目录与当前市场不一致时直接报错并终止打开（`筛选分组目录与当前市场不一致，请刷新页面后重试`）。
7) 弹窗标题：`筛选分组管理` → `信用筛选分组管理` / `利率筛选分组管理`。
8) 加载/保存双向按市场过滤条件：`normalizedRemoteMarketGroup` 与 `marketGroupFilterPayload` 都调用 `marketConditionAllowed(market, condition)`。
9) 缓存：`marketFilterPanelLabelCache` / `marketFilterCatalogCache`，在 filter config 刷新时清空。

### 3.4 利率筛选条件裁剪

1) 利率市场**删除 4 类筛选**：地方债地区（`lg_region`）、主体评级（`issuer_rating`）、做市标的（`market_maker`）、永续债（`perp`）。
2) 保留含权属性等其余条件。
3) 旧利率分组中残留的这 4 个条件，在**加载、查询、保存**三条路径上强制清理，不得参与聚合。
4) 利率全部/最优把「主体评级」作为**展示列**保留。

插件对应：`Modules/MarketCenter/View/RateFilterPanel.xaml`（第 106、146、167、203 行附近仍在面板上）；`Shared/Converters` 与筛选查询映射需同步排除这 4 个 key。

### 3.5 报价提醒

1) **对价提醒规则简化**：只需「同券 + 方向相反 + 一条本人一条非本人」（新增 `quotesOppositeForAlert`，替代 `quotesComparableForPair`）；**不再校验价格关系、价格类型、清算速度、价格或报量**。基线规则「他人 Ofr ≤ 本人 Bid」作废。
2) **我的最优**：需存在同券、同方向、且 `quotePairKey`（价格类型+清算速度）一致的他人报价。
3) 已读机制重做：
   - 新增 `markAlertKeysViewed(keys)`；弹窗按类型记录 `alertPopupState`（keys + items）。
   - 弹窗「查看」只标记**该弹窗的 alertKeys**，并跳转提醒页、选中并高亮对应提醒行（mode=manual），不再整页标记已读。
   - 弹窗「关闭」只关 UI，**不标记已读**。
   - 从弹窗跳转提醒页时用 `suppressNextAlertPageRead` 抑制整页已读；只有用户自己点提醒页签才标记整页已读。
4) 设置窗口新增账号级开关：`弹窗提醒 - 我的最优报价`（settings key `bestQuote`）。提醒页顶部四开关（关注债券 / 筛选分组 / 对价提醒 / 我的最优报价）。
5) 提醒重算触发路径扩展到 SSE 断线后 5 秒轮询兜底。

### 3.6 智能推荐（说明文案 + 字段脱敏）

1) 推荐说明弹窗文案重写：`使用所有用户已确认的正式报价…；QT 模拟下行只有人工确认后才会进入推荐`；新增「字段权限」段；`自定义窗口最长 365 日`；删除「模拟数据补位」段。
2) 规则：所有账户已确认正式报价均参与计算；**推荐线索中的「报价机构 / 报价交易员」只在证据属于当前用户时显示，非本人证据对应字段留空**；评分、债券、次数、价格、数量、长城交易员正常展示。

---

## 4. 报价录入

### 4.1 批量旗标语义变更

基线：逐行取反（混合状态会翻出「半开半关」）。
0910：`next = rows.some(row => !row[field])`，然后把**所有选中行统一设为 next**（即「只要有一行没打标就全部打上，否则全部取消」）。影响 `quoteBatchTag` 与 `preciseBatchTag`，OCO/打包仍走互斥逻辑。

插件对应：`QuoteConfirmViewModel.ToggleFlag`（第 524 行）、`PreciseQuoteViewModel.ToggleFlag`（第 359 行）——当前都是逐行 `!row.X`。

### 4.2 BP / 报量 ± 按钮改为累计步进

1) 新增 `advanceBatchButtonInput(input, action, step)`：连续点 `+` 时输入框值按 1、2、3… 累加（`-` 同理累负值）；切换方向则从 1 重新开始。
2) 通过 `input.dataset.batchLastAction` / `batchButtonMode` 记录状态；`sanitizeBatchNumericInput(input, resetState=true)` 新增 resetState 参数，手动输入时重置步进态。
3) `batchNumericDelta` 仅用于 `action==='auto'` 的手动输入场景。

### 4.3 价格类型切换 = 语义换算（不再只是缓存切换）

新流程：切类型 → 立即清空目标价格 → 若该类型有缓存则用缓存 → 否则调 `POST /api/quotes/price-conversion`（带 `targetPriceType`）换算 → 回填 4 位小数；失败则清空并 Toast（不残留旧值）。

### 4.4 剩余期限回填

1) `applyValuation` 新增：从估值记录 `record.remaining_tenor` 回填 `target.m`（避免 DM 查询暂时失败后期限永久丢失）。
2) 识别结果 `term` 不再用 `bond_type_desc` 兜底（债券分类 ≠ 期限），缺省留空等主数据回填。

### 4.5 估值字段映射修正

`nzNet: r.nzPrice || r.nzNet`；`nzYTM: r.cbYtm || r.nzYTM`（基线取 `r.nzVal` / `r.csVal`）。

### 4.6 识别工具行入口隐藏

精确报价 / 模糊报价工具行中 `截图识别`、`导入图片`、`导入表格` 三个按钮 `display:none`（能力保留，仅隐藏入口）；保留 `粘贴`、`清空`、`示例`、`识别报价`。

插件对应：`PreciseQuoteView.xaml` 第 67-69 行、`FuzzyQuoteView.xaml` 第 53-55 行 —— 需隐藏（建议注释保留而非删除，与原型「能力保留」一致）。

### 4.7 报价确认新增「模拟」按钮 —— **客户端不实现（2026-09-10 确认）**

原型工具栏最右新增 `模拟` 按钮（`#quoteConfirmMockBtn`，标题「模拟 QT 报价下行」）→ `mockQtQuoteDownlink()` → `POST /api/quote-confirm/mock {count:5}`；执行期间按钮置灰显示「下行中...」，成功后刷新待确认批次并 Toast「QT已下行 N 个机构 / M 条报价」。规格限定仅非生产环境。

**产品侧确认该按钮是原型自身的测试入口，客户端不需要实现，`POST /api/quote-confirm/mock` 也不纳入客户端接口清单。**
本项仅作为原型行为记录保留；对应差距项见 `PROTOTYPE_UI_GAP_0910.md` §A8（状态：不需实现）。

### 4.8 报价表单 BP 步进以中债估值为基准

价格为空时按 1bp 步进不再按 0 起算，改为取该债券中债估值（行权收益率取 `cbYte`，其余取 `cbYtm`）作为基准；若估值未取到则 Toast「中债估值尚未获取，无法按1bp调整」并中止。报价弹窗状态新增 `valuation` 缓存。

### 4.9 其他

1) `ensureBatchNumericInputValue` 调用 `sanitizeBatchNumericInput(input,false)`，避免清空步进态。
2) 登录成功后新增 `refreshMarketGroupMatches(true)`（分组匹配立即刷新）。

---

## 5. 报价管理

**无功能变更**。app.js 中报价管理区块只出现全局改名之外的上下文行，`BOARD_COLUMN_DEFS` / 今日/历史列定义完全未变。

---

## 6. 样式与布局

| 项 | 变更 |
|---|---|
| 报价录入弹窗外壳 | `.quote-entry-shell`：`min(1200px,92vw) × min(720px,84vh,…)` → **`min(1970px,96vw) × min(840px,92vh)`**（容纳 min-width 1970 的报价确认表）；窄屏媒体查询同步改 `90vh` |
| 报价确认表 | `.quote-confirm-wrap .tbl-wrap` `overflow-y:auto` → `overflow:auto`（恢复横向滚动）；`.statusbar` 改 `position:static` |
| 新增 | `.best-quote-count`、`.best-quote-layer*`（浮层，max-height 320 / min-width 780，sticky 表头） |
| 新增 | `.fgm-subgroup*`、`.fgm-selected-pane`、`.fgm-condition-chip`、`.fgm-add-subgroup`、`.fgm-condition-group(.disabled)`、`.fbm-check.disabled`、`.fbm-custom-condition` |
| 背景 | `.content` 与 `#page-quote/#page-board/#page-mgmt/#page-version` 显式 `background:#0d1117` |
| 新元素 | `#boardNewQuoteBanner`（高 30px、底部 `#7c5a00`、底色 `#2d21…`、全宽按钮） |

---

## 7. 需求规格 / SRS 层面的新增约定（可作验收依据）

1) 报价确认：非生产环境可点击「模拟」随机下行 5 条来源为 QT 的报价；模拟下行失败不影响已有待确认数据。**（该条为原型自身测试行为，客户端不实现。）**
2) 精确/模糊报价：截图 OCR、导入图片、导入 Excel 入口隐藏但能力保留。
3) 利率面板：按债券/流动性/条款/区域筛选组织，删除地方债地区、主体评级、做市标的、永续债。
4) 报价提醒：对价提醒只校验同券、方向相反、本人与非本人；弹窗最多展示最近 3 条；「查看」只标记本弹窗并定位，「关闭」不标记已读。
5) 推荐：全部账户正式报价参与计算；证据非本人时机构/交易员返回空值；自定义窗口最长 365 日。
6) 筛选分组子分组：已选区不显示筛选条件标题只显示具体项；同一行展示多项自动换行；新增分组固定在底部；不限制子分组数量；空子分组保存自动丢弃。
7) 数据模型验收：表 62、字段 767、显式索引 46、物理外键 13、`foreign_key_check` 返回 0 行。

---

## 8. 插件改动清单（按文件）

### P0 — 规则/语义类，必须改

| 文件 | 改动 |
|---|---|
| `Mock/MockMarketCenterService.BuildBestRows` | 分组键改 `Code`；引入收益率折算比价；同档多笔 |
| `Abstractions/Models/BoardBestRow.cs` | 新增 `BidBestQuoteIds` / `OfrBestQuoteIds` / `BidBestYield` / `OfrBestYield` |
| `Modules/MarketCenter/ViewModel/BoardAlertViewModel.cs` | 对价提醒改「同券+反向+本人对方」；我的最优加 `quotePairKey` 条件；状态栏对价计数去掉 `Bid>=Ofr` 判定 |
| `Modules/QuoteEntry/ViewModel/QuoteConfirmViewModel.cs` | `ToggleFlag` 改统一置位语义；BP/报量改为累计步进 |
| `Modules/QuoteEntry/ViewModel/PreciseQuoteViewModel.cs` | 同上 `ToggleFlag` 与步进 |
| `Modules/MarketCenter/View/RateFilterPanel.xaml` | 删除地方债地区、主体评级、做市标的、永续债 4 组；筛选映射同步排除 4 个 key |
| 全模块文案 | 意向报价 → 模糊报价（子页标签、Toast、导出 Sheet/文件名/列头） |

### P1 — 交互与界面类

| 文件 | 改动 |
|---|---|
| `Modules/MarketCenter/ViewModel/BoardListViewModel.cs` / `BoardBestViewModel.cs` | 选中态 auto/manual 模型；manual 冻结投影 + 新报价计数 + 横幅命令 |
| `Modules/MarketCenter/View/BoardListView.xaml` / `BoardBestView.xaml` | 顶部 `有新报价` 横幅；报价单元格前「同档笔数」按钮 |
| 新增组件（`Modules/MarketCenter/View/`） | 同档最优报价浮层（7 列小表，仅本人行可点击跳转） |
| `Modules/MarketCenter/View/BoardAlertView.xaml` + VM | 已读按 alertKey 粒度；弹窗「查看」定位高亮；补 SSE 兜底轮询语义 |
| `Shell/View/SettingsWindow.xaml` | 新增「弹窗提醒 - 我的最优报价」 |
| `Modules/MarketCenter/View/QuoteEntryDialog.xaml` + VM | BP 步进以中债估值为基准；估值缓存 |
| `Modules/QuoteEntry/View/PreciseQuoteView.xaml` / `FuzzyQuoteView.xaml` | 隐藏截图/导入图片/导入表格 |
| `Modules/QuoteEntry/ViewModel/QuoteConfirmViewModel.cs` | ~~工具栏新增「模拟」命令（`/quote-confirm/mock`）~~ —— 2026-09-10 确认不需要，已移除 |
| 价格类型切换（精确/确认/表单三处） | 加 `targetPriceType` 换算请求 + 失败清空 |
| `Themes/Controls.xaml` / `Colors.xaml` | 新增浮层、子分组 chip、禁用态 tag 样式；报价录入弹窗尺寸对齐（1970×840） |

### P2 — 接口/契约类

| 项 | 改动 |
|---|---|
| `Abstractions/IMarketCenterService.cs` + `IQuoteService.cs` | 补 `InsertMockQtQuotesAsync`、`ConvertPriceBatchAsync`、模糊报价路径回退语义 |
| `Abstractions/Gateway/*` | `/fuzzy-quotes` 与 `/board-intents` 的 404/405/501 回退策略 |
| Mock 数据 | 报价提醒四类提醒样例改为新规则；最优样例补同档多笔 |
| 数据模型 | 主库 +9 表（`alert_read`、`quote_realtime_events`、`sse_tickets`、`login_audit_events`、`stc_institution_trader_directory` 等），与插件相关性排序：`alert_read`、`stc_institution_trader_directory`、`quote_realtime_events` 优先 |

---

## 9. 待确认

1) `/api/fuzzy-quotes` 是后端**已上线**还是仅前端预埋（原型的 `fuzzyQuoteFetch` 带 `/board-intents` 回退，规格页接口清单里仍写 `/board-intents`）？插件侧是否也做双路径回退？
2) 最优报价改成「仅按债券代码聚合」后，同一债券的**不同价格类型**如何在一行展示？原型 22 列结构未变，但代表行的价格类型会随「同档最新」漂移——需要确认展示字段口径。
3) 推荐字段脱敏（非本人证据机构/交易员留空）是**前端置空**还是后端返回空值？原型同时在说明文案与数据两层做了约束。
4) 报价提醒「对价提醒」不再判断价格关系，是否会影响现有 `Bid >= Ofr` 的状态栏计数（原型仍保留该计数，语义已与控制规则脱钩）。
5) 利率 4 项被删筛选在旧分组数据中的清理是「静默丢弃」还是需要提示用户？
