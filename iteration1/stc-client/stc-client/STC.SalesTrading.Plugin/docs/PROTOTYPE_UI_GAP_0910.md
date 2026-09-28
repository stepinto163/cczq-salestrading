# 原型 0910 ↔ 当前 WPF 实现 界面差距清单

> 生成日期：2026-09-10
> 范围：**只列 UI / 交互 / 布局层差距**（文案、控件、列、面板、弹窗、尺寸、样式）。
> 业务规则、接口契约、数据模型、算法类变更见同目录 `PROTOTYPE_DIFF_0910.md`，本文件不重复。
> 新原型：`.tmp/prd/销交平台原型0910`；WPF 现状：`STC.SalesTrading.Plugin` 源码 + `tools/captures/*.png`（1440×900 截图基线）。

---

## A 类：本次原型更新带来的 UI 差距（必须改）

### A1 子页标签与页面文案改名：意向报价 → 模糊报价

| 位置 | 现状 | 应改为 |
|---|---|---|
| `Modules/MarketCenter/ViewModel/MarketCenterViewModel.cs:43` | `new SubTabItem("意向报价", Intent, "intent")` | `模糊报价` |
| `Modules/MarketCenter/ViewModel/BoardIntentViewModel.cs:40` | `Title = "意向报价"` | `模糊报价` |
| `BoardIntentViewModel.cs:145 / 183 / 260` | Toast「意向报价已刷新 / 已Refer N 条意向报价 / 已转 N 条意向报价」 | 模糊报价 |
| `Modules/QuoteEntry/ViewModel/FuzzyQuoteViewModel.cs:260 / 264` | 「已转 N 条意向报价 / 转意向报价失败」 | 模糊报价 |
| `Abstractions/Models/BoardIntentRow.cs`、`Enums.cs:57` | 注释与枚举说明 | 模糊报价 |
| 导出 | Sheet 名 / 文件名 / 列头「意向报价ID」 | 模糊报价 / 模糊报价_*.xlsx / 模糊报价ID |

原型证据：新原型子页 `data-st="board-intent"` 标签已改为「模糊报价」；app.js 全链路 toast、导出、列头均已改名。

### A2 最优页：报价单元格「同档笔数」按钮 + 同档报价浮层（新控件）

原型新增：Bid / Ofr 单元格前插入 20×18 的 `N` 按钮（仅同档 >1 笔时显示），hover 弹出「同档最优报价浮层」——7 列小表：价格 / 折算收益率 / 量 / 报价机构 / 报价交易员 / 长城交易员 / 报价时间；**仅本人报价行可点击**（点击跳转对应市场「全部」页并选中高亮），非本人行的机构/交易员显示 `--`。

WPF 现状：`Modules/MarketCenter/View/BoardBestView.xaml:116 / 127` 的 Bid/Ofr 是普通 `DataGridTemplateColumn`，只有行级 hover tooltip（`BoardBestRow` DataTemplate），没有笔数按钮和浮层。

需要新增：`Shared/Components/` 下的浮层控件（或用 `Popup`）+ `BoardBestRow` 增加 `BidBestCount` / `OfrBestCount` / `BidBestQuotes[]` / `OfrBestQuotes[]` 等展示字段。

### A3 行情中心：顶部「有 N 条新报价，点击刷新」横幅（新控件）

原型新增全宽 30px 横幅 `#boardNewQuoteBanner`（底色 `#2d21..`、底边 `#7c5a00`、居中文字），位于表格上方；手动选中行后新报价不再刷列表，只累计条数并显示该横幅，点击后刷新并解除冻结。

WPF 现状：无此元素（`BoardListView.xaml` / `BoardBestView.xaml` 工具栏下方直接是 DataGrid）。

需要新增：两个看板 Grid 增加一行 `RowDefinition Height="Auto"` + 横幅 `Button`（`Visibility` 绑 `HasPendingNewQuotes`），并配套 `BoardListViewModel` / `BoardBestViewModel` 的横幅命令与可见性属性。

### A4 筛选分组弹窗：子分组卡片式 UI

原型把「筛选分组管理」重做为「左：已选条件子分组 / 右：条件目录」，具体新增 UI：
1) 已选区按子分组渲染为卡片（`.fgm-subgroup`），卡片内条件是可删除的 chip（`.fgm-condition-chip`），卡片右上角是删除子分组按钮（hover 显现、非空二次确认）；
2) 底部固定「+ 新增分组」虚线按钮（`.fgm-add-subgroup`）；
3) 已选区不显示「筛选条件」标题，chip 同行自动换行；
4) 右栏条件目录按市场动态生成，支持 tag 多选、文本输入「添加」、下拉「添加」、区间 min/max + Y/M/D「添加」（`.fbm-custom-condition`）；
5) 不可用条件置灰（`.fbm-check.disabled`、`.fgm-condition-group.disabled`）；
6) 弹窗标题按市场：`信用筛选分组管理` / `利率筛选分组管理`。

WPF 现状：**整个分组管理弹窗尚未实现**——`CreditFilterPanel.xaml:20-23` / `RateFilterPanel.xaml:19-22` / `AlertFilterPanel.xaml:19-22` 的「+」按钮 ToolTip 为「关注债券/筛选分组管理弹窗（待接）」，是占位；`关注债券` 弹窗同样未实现。

结论：A4 是「从零实现 + 直接按新结构做」，不要按旧结构先做再改。

### A5 利率筛选面板删除 4 组

原型 `filters-rate` 现有 5 区：报价筛选 / 债券筛选（细分券种、是否免税、地方债类型、地方债专项、剩余期限、发行人）/ 流动性筛选（债券流动性、上市场所）/ 条款筛选（含权属性、偿还方式、付息方式、利率方式、发行年份）/ 区域筛选（地区等级、地区）。

WPF `Modules/MarketCenter/View/RateFilterPanel.xaml` 多出 4 组，删除行区间：

| 删除项 | 行区间 |
|---|---|
| 地方债地区（标题 + 广东/山东/浙江/江苏/更多） | 106–113 |
| 主体评级（标题 + AAA…更多） | 146–155 |
| 做市标的（标题 + 上证利率/深证利率） | 167–171 |
| 永续债（标题 + 永续/非永续） | 203–207 |

注：`地方债专项`（99–105）在原型利率面板**保留**，不要误删。

### A6 精确/模糊报价工具栏隐藏 3 个按钮

原型给「截图识别 / 导入图片 / 导入表格」三个按钮加了 `style="display:none"`（能力保留、仅隐藏入口），保留「粘贴 / 清空 / 示例 / 识别报价」。

WPF 需隐藏：
- `Modules/QuoteEntry/View/PreciseQuoteView.xaml:67-69`
- `Modules/QuoteEntry/View/FuzzyQuoteView.xaml:53-55`

建议按原型语义注释掉而非删除，保留命令实现。

### A7 报价录入弹窗尺寸

原型 `.quote-entry-shell`：`min(1200px,92vw) × min(720px,84vh,…)` → `min(1970px,96vw) × min(840px,92vh)`（为容纳 min-width 1970 的报价确认表，避免一进来就横向滚动）。

WPF：`Modules/MarketCenter/View/QuoteEntryOverlayWindow.xaml:6` `Width="1200" Height="720"` → 改为 `Width="1970" Height="840"`（并考虑 `MaxWidth/MaxHeight` 按屏幕约束，原型是 96vw/92vh）。

### A8 报价确认工具栏新增「模拟」按钮 —— **确认不需要实现（2026-09-10）**

原型在工具栏最右、`已选 N 条` 之后新增 `模拟` 按钮（`#quoteConfirmMockBtn`，tooltip「模拟 QT 报价下行」，执行中置灰显示「下行中...」），对应接口 `POST /api/quote-confirm/mock`。

**结论：该按钮是原型侧的测试入口，产品侧确认不需要在客户端实现。**
曾按 UI 口径临时加过一版（按钮 + 忙碌态 + 「接口待接」提示），已按确认结论**完整移除**：
`QuoteConfirmView.xaml` 的按钮与 `QuoteConfirmViewModel` 的 `IsMockDownlinking` / `MockQtDownlink` 均不再存在。

→ 后续如再对比原型，**不要**把这一项当成待补差距；`POST /api/quote-confirm/mock` 也不需要纳入客户端接口清单。

### A9 设置窗口新增「弹窗提醒 - 我的最优报价」

WPF `Shell/View/SettingsWindow.xaml:67-72` 目前 3 项（关注分组报价提醒 / 对价提醒 / 筛选分组报价提醒），原型设置里为 4 项，缺「弹窗提醒 - 我的最优报价」（settings key `bestQuote`）。

### A10 右下角提醒弹窗改版

原型弹窗结构：头部 = 提醒类型徽标 + 本次新增数量；正文 = 最多 **3** 条（期限、债券、时间 + Bid/Ofr 两侧交易员、报量、价格类型、价格、标签）；底部两个按钮「查看」「关闭」。

WPF 现状：`Shared/Components/AlertPopupWindow.xaml` 是 **逐条平铺**（每行一条：类型徽标 + 代码 + 简称 + Bid/Ofr + 时间 + 「查看」/「忽略」），无类型/新增数量头部、无 3 条上限、按钮文案是「忽略」而非「关闭」。

需要改：`AlertPopupWindow.xaml` 结构 + `AlertPopupItem`/`AlertPopupService` 的展示字段（类型分组、新增数量、每条的期限/价格类型/标签）。

### A11 列头文案修正

| 文件:行 | 现状 | 原型 |
|---|---|---|
| `Modules/MarketCenter/View/BoardBestView.xaml:108` | `销售.Bid` | `长城交易员.Bid` |
| `Modules/MarketCenter/View/BoardAlertView.xaml:68` | `销售.Bid` | `长城交易员.Bid` |

原型 `board-credit-best` / `board-alert` 定义：Bid 侧 = `长城交易员.Bid`；Ofr 侧最优页 = `长城交易员.ofr`、提醒页 = `销售.Ofr`。
→ `BoardBestView.xaml:145`（`长城交易员.ofr`）和 `BoardAlertView.xaml:105`（`销售.Ofr`）**已与原型一致，不要改**。

### A12 编辑报价弹窗（新报价/编辑报价）与原型差距 —— 新增，见 §D

原型「编辑报价」弹窗（`.modal.modal-wide`，宽 **880px**）结构与 WPF `Modules/MarketCenter/View/QuoteEntryDialog.xaml`（**1200×720**）差距较大，逐项对照见 §D.2。要点：缺债券摘要条（市场最优 Bid/Ofr + 中债估值）、价格/报量缺 ±1bp / ±1k 步进按钮、报量单位应为下拉（万/万面额/手/张）、标签只应保留 急/荐/整、方向应用分段按钮、弹窗宽应为 880px。

---

## B 类：复核时发现的既有 UI 差距（非本次更新引入，建议一并处理）

| 项 | WPF | 原型 | 说明 |
|---|---|---|---|
| B1 | `CreditFilterPanel.xaml` 信用筛选区缺「中证隐含」 | `filters-credit` 信用筛选 = 银行业/行业/企业性质/担保/担保人/主体评级/中债资信/评级展望/债项评级/中债隐含/YY评分/**中证隐含** | 缺最后一项（IntentFilterPanel 与 AlertFilterPanel 都有，只有信用面板漏了） |
| B2 | `AlertFilterPanel.xaml`（报价提醒 + 报价管理共用） | `filters-merged` / `filters-merged-mgmt` | 缺「地方债专项」；区域筛选缺「地区等级」，且第一组用了 intent 版标签「城投属性/全国地区」，原型 merged 用「城投/重点省份/城投层级/地区等级/地区/省份」 |
| B3 | `Shell/SubTabItem.cs` 无角标字段 | 子页 `.cnt` 角标：报价确认 = 待确认数、报价提醒 = 未读数 | 需给 `SubTabItem` 加 `Badge/BadgeVisibility` 并在子页条模板渲染 |
| B4 | `QuoteConfirmView.xaml:64-69` 机构/交易员输入框无占位文案（仅 ToolTip） | 原型为 `placeholder="报价机构"/"报价交易员"` | 可加 `Tag` + 样式实现 Watermark |
| B5 | 关注债券管理弹窗、筛选分组管理弹窗整体未实现 | 两套弹窗齐全 | 与 A4 合并排期 |

---

## C 类：已核对一致，无需改动

1) **列数与列序**：信用/利率全部 20 列、最优 22 列、报价提醒 23 列、今日报价 23 列、历史报价 22 列——与原型 `BOARD_COLUMN_DEFS` 及报价管理列定义完全一致（仅 A11 两处文案）。
2) **报价确认表**：18 列 + 组头通栏行 + 明细无表头（RowDetails）+ 底部状态栏（Grid 固定行）+ DataGrid 自带横向滚动——原型本次把 `.tbl-wrap` 从 `overflow-y` 改回 `overflow:auto` 并把 `.statusbar` 改 `position:static`，WPF 本来就是这两种行为，无需改。
3) **背景色**：`PageBg #0D1117`、`BoardMainBg #010409`、`WindowBg #080A10` 与原型一致（原型本次只是给 `.content`/`.page` 显式补 `background:#0d1117`）。
4) **行情中心子页条尾部**：刷新按钮 + 「报价录入」按钮已实现。
5) **全部页工具栏**：Copy / Refer / 导出 / Bid / Ofr 开关 / 我的报价 / 紧急 / 推荐 / 整量 / 未报价债券 齐全；最优页按原型无「未报价债券」。
6) **报价提醒工具栏**：只有 4 个开关，与原型一致。
7) **意向（模糊）报价面板**（`IntentFilterPanel.xaml`）：4 区 + 全部条件与原型 `filters-intent` 逐项一致。
8) **信用面板其余部分**：报价筛选网格、债券筛选、流动性筛选、条款筛选、区域筛选条件与原型逐项一致（除 B1）。
9) **子页/导航结构**：3 个一级页 + 行情中心 6 子页 + 报价录入 3 子页 + 报价管理 2 子页，与原型一致。

---

## D 类：组件存在性核对（浏览器导出可信度 + 编辑报价弹窗）

> 背景：新版原型是**浏览器「另存为」得到 DOM 序列化导出**，不是原型源码包。本节回答两个问题：
> ① 原型到底有没有某个窗口；② 这版导出丢了什么、哪些结论需要保留。

### D1 导出性质与可信度审计

这版导出的构成：`index.html` 是**运行时 DOM 快照**（标签自闭合被展开、`'` 转成 `&#39;`、JS 动态生成的节点被固化进 HTML、
运行态 `display:none` / `value` / `active` 类被写死）；而 `app.js`(9336 行)、`app.css`(705 行)、
`quote-canonical.js`、`quote-utils.js`、`api-client.js`、`page-controllers.js`、`filter-logic.js` **都是未压缩的完整文件**。

审计方法：把 `app.js` 里所有 `getElementById('x')` / `querySelector('#x')` 字面量、以及 `id="…"` /
`setAttribute('id',…)` / `.id=…` 动态创建集合，与导出 HTML 的 id 集合做三方比对。

| 指标 | 结果 |
|---|---|
| 导出 HTML 中 id 总数 | 549 |
| app.js 动态创建 id 数 | 68 |
| app.js 字面量查询 id 数 | 170（另有 1 处模板拼接无法静态解析） |
| **引用但 HTML 与 app.js 都没有（真缺失）** | **3** |

真缺失的 3 个：`institutionList`、`traderList`、`miniChart`。
→ 这 3 个在**旧基线源码里同样不存在**（`ficc-client/index.html` 与 `app.js` 都是同一情况），
属原型自身的历史遗留死引用（有 null 保护），**不是本次导出丢的**。

**结论：代码层零丢失；DOM 层只丢「保存那一刻未渲染过的运行时节点」。**

### D2 编辑报价弹窗（存在，且本导出完整包含其 DOM）

导出中实测到的完整结构（`.modal-overlay#modalOverlay > .modal.modal-wide`）：

| 区块 | 内容 |
|---|---|
| 头部 | 标题 `#modalTitle`（编辑时「编辑报价」/ 新建时「新报价」）+ 右上 ✕（`closeModal()`） |
| 债券摘要条 `.quote-bond-summary` | 左：债券简称（粗体）+ 代码 + meta `剩余期限 7.65Y / 主体评级 / 债项评级`；右：`.quote-market-metrics` = `市场最优 Bid ‑‑ ｜ Ofr 102.3800` + `中债估值 #quoteChinaBondValuation 1.6880%` |
| 方向 | `.quote-dir-btn`：`Bid 买入` / `Ofr 卖出`，`.active-bid`（橙底）/`.active-ofr`（蓝底），高 36px |
| 表单（12 栅格 `.quote-fields`） | 价格 `span-6`（输入框 + **+1bp/-1bp** 步进 + 价格类型下拉 到期/行权收益率/净价/全价 + 错误位 `#quotePriceError`）；报量 `span-6`（输入框 + **+1k/-1k** 步进 + **报量单位下拉** 万/万面额/手/张 + 错误位 `#quoteVolumeError`）；交易日期 `span-3`（date）；清算速度 `span-2`（请选择/T+0/T+1）；报价机构 `span-4`；报价交易员 `span-3` |
| 标签+备注行 | 报价标签 = **急 / 荐 / 整 三项**（checkbox）；备注 `#quoteNote` textarea，`maxlength=120`，placeholder「补充报价约束、有效期或沟通事项」 |
| 页脚 `#modalFooter` | `<kbd>Enter</kbd> 保存修改` · `<kbd>Esc</kbd> 取消` + 「取消」+「保存修改」`#quoteSaveBtn` |

**与 WPF `Modules/MarketCenter/View/QuoteEntryDialog.xaml` 的逐项差距：**

| # | 差距 | WPF 现状 | 原型 |
|---|---|---|---|
| D2-1 | 窗口尺寸 | `1200×720`（第 4 行） | `.modal-wide` = **880px** 宽，`max-width:90vw`，正文 `max-height:60vh` 内滚 |
| D2-2 | **债券摘要条缺失** | 债券代码/名称/期限放在自绘标题栏（44–49 行），无市场最优、无中债估值 | 独立 `.quote-bond-summary`：市场最优 Bid/Ofr + 中债估值 |
| D2-3 | 方向控件形态 | `RadioButton`（89–92 行） | `.quote-dir-btn` 分段按钮，active 橙底/蓝底 |
| D2-4 | 价格缺步进 | 只有 TextBox + 价格类型（101–109 行） | 输入框右侧 `+1bp` / `-1bp` 按钮 |
| D2-5 | 报量缺步进与单位 | 只有 TextBox，标题写「报量（万）」（113–115 行） | 输入框右侧 `+1k`/`-1k` + 报量单位下拉（万/万面额/手/张） |
| D2-6 | 缺字段错误位 | 无 | 价格/报量下方 `#quotePriceError` / `#quoteVolumeError` 文案位 |
| D2-7 | 标签数量偏多 | 急/荐/整/OCO/打包 5 个（81–85 行） | 弹窗内只 **急/荐/整**（OCO/打包只在工具栏与右键菜单） |
| D2-8 | 备注文案 | 「备注（原始文本）」、MinHeight 120（133–135 行） | 「备注」+ maxlength 120 + placeholder |
| D2-9 | 页脚 | 「取消 / 提交报价」（142–143 行） | kbd 快捷键提示 + 「取消 / **保存修改**」（编辑态） |

### D3 导出中**没有** DOM 快照的组件（必须按 `app.js` 模板 + `app.css` 反推）

以下组件是运行时懒创建/按需渲染，保存那一刻没打开，所以导出 HTML 里出现 0 次。
它们的 DOM 模板在 `app.js` 里是完整的，**不影响实现，但做像素比对时不能以导出为准**：

| 组件 | 导出中 class 计数 | app.js 里的构建入口 |
|---|---|---|
| 关注债券管理弹窗（fbm） | `fbm-layout` = 0 | `showFavBondModal()`，内部节点 `#fbmLeft/#fbmInput/#fbmIdentifyBtn/#fbmNewGroupRow/…` |
| 筛选分组管理弹窗（fgm，含新子分组） | `fgm-subgroup` / `fgm-selected-pane` = 0 | `showFilterGroupModal()`，内部节点 `#fgmLeft/#fgmInput/#fgmNewGroupRow/…`；子分组模板见 app.js 中 `fgm-subgroup-scroll` 字符串 |
| 同档最优报价浮层 | `best-quote-layer` = 0 | `ensureBestQuoteLayer()` + `bestQuoteLayerHtml()`（列头：价格 / 折算收益率 / 量 / 报价机构 / 报价交易员 / 长城交易员 / 报价时间） |
| 报价换算 tooltip | `quote-price-tooltip` = 0 | `ensureQuotePriceTooltip()` + `quotePriceTooltipHtml()` |
| 通用 prompt/confirm 弹窗 | `system-modal` = 0 | 运行时构建，输入框 `#systemModalInput` |
| 债券资料弹窗内容 | `bond-info-popup` = 0（容器 `<div id="bondInfoPopup">` 是静态的、内容为空） | `showBondInfoPopup()` 填充内容 |

→ 也就是说 **A2（浮层）、A4（分组弹窗）、A10（提醒弹窗）、A12（编辑报价）四条结论都已逐条验证过来源**，
不受导出缺失影响：前三条走 app.js 模板 + app.css，A10 走导出里已固化的 `alert-popup` 真实 DOM，A12 走导出里已固化的 modal DOM。

---

## 建议实施顺序

1. **A5 + A6 + A7 + A11 + A9** —— 纯删除/改常量/改文案，零风险，可一批提交。
2. **A1** —— 改名，纯文案，但涉及多文件，建议单独一批（注意导出列头与 Sheet 名）。
3. **A3 + A2** —— 两个新控件，是本次更新的视觉重点；A3 依赖 A2 的「本人报价」判定样式可复用。
4. **A10** —— 提醒弹窗改版，独立控件。
5. **A4 + B5** —— 分组管理弹窗从零实现，工作量最大，建议直接按新（子分组）结构做。
6. **A12** —— 编辑报价弹窗重做（880px + 摘要条 + 步进按钮 + 报量单位），可独立于业务接线先做。
7. **B1 ~ B4** —— 顺手补齐。

## 验收方式

沿用现有 `tools/Capture.ps1` + `tools/PixelCheck.ps1`：补充 `market-信用最优`（笔数按钮/浮层）、`market-报价提醒`（横幅 + 弹窗）、`quote-报价确认`（模拟按钮）三个截图场景；A7 改尺寸后需重新生成 `tools/captures/*.png` 基线，否则像素比对会全部失败。

---

## 实施状态（2026-09-10，未提交）

### 已完成

| 项 | 落地文件 | 说明 |
|---|---|---|
| A1 | `MarketCenterViewModel` / `BoardIntentViewModel` / `FuzzyQuoteViewModel` / `QuoteViewModel` / 模型与枚举注释 | 子页标签与全链路文案改为「模糊报价」；接口名 `board-intents`、表名 `board_intent`、代码标识符 `Intent*` 保持不变 |
| A2 | `BoardBestView.xaml(.cs)` / `BoardBestViewModel` / `MockMarketCenterService.BuildBestRows` / 新增 `Models.BestQuoteDetail` | 笔数按钮 + 7 列浮层 + 仅本人可点击 → 发布 `LocateQuoteInAllMessage` → `MarketCenterViewModel` 切「全部」页并选中该行 |
| A3 | `BoardListView.xaml` / `BoardBestView.xaml` / 两个看板 VM | 30px 横幅（`#2D2100` 底 / `#7C5A00` 底边 / `#FBBF24` 字），选中行时新报价只累计计数 |
| A5 | `RateFilterPanel.xaml` | 删除 4 组（地方债地区 / 主体评级 / 做市标的 / 永续债），保留地方债专项；现与原型 `filters-rate` 逐项一致 |
| A6 | `PreciseQuoteView.xaml` / `FuzzyQuoteView.xaml` | 三按钮 `Visibility="Collapsed"`，命令保留 |
| A7 | `QuoteEntryOverlayWindow.xaml(.cs)` | `1970×840`，并在 `OnLoaded` 按工作区 96vw/92vh 收敛；标题行高 40 → 44 |
| A8 | —— | **不需实现**：原型测试用按钮，已确认移除（见上） |
| A9 | `SettingsWindow.xaml` | 补第 4 个弹窗开关 |
| A10 | `AlertPopupWindow.xaml(.cs)` / `AlertPopupService` / 新增 `AlertPopupGroup` / `AlertPopupItem` 重构 | 按类型分组；头部「类型 (新增数)」+ 查看/关闭；正文最多 3 条；条目两行布局；弹窗宽 400（原型 420） |
| A11 | `BoardBestView.xaml` / `BoardAlertView.xaml` | 「销售.Bid」→「长城交易员.Bid」 |
| A12 | `QuoteEntryDialog.xaml(.cs)` | 880×620 + 摘要条 + 方向分段 + 价格 ±1bp / 报量 ±1k + 报量单位下拉 + 标签 3 项 + 错误位 + Enter/Esc + 页脚 kbd（**第二轮已按 app.css 逐条校准，见文末「A12 复刻校准」**） |
| B1 | `CreditFilterPanel.xaml` | 信用筛选补「中证隐含」 |
| B2 | `AlertFilterPanel.xaml` | 债券筛选补「地方债专项（置灰）」「地方债地区」；区域筛选改为 城投/重点省份/城投层级/地区等级/地区/省份 |
| B3 | `Shell/SubTabItem.cs` + `MarketCenterView.xaml` + `QuoteView.xaml` + `MarketCenterViewModel` + `QuoteViewModel` | `BadgeCount` 属性 + 红底圆角角标；来源复用 `AlertCountChangedMessage` / `PendingQuotesChangedMessage` |
| B4 | `QuoteConfirmView.xaml` / `PreciseQuoteView.xaml` | 机构/交易员输入框加占位文案（DataTrigger 实现，未引入新控件） |

### 有意偏离 / 简化（非缺陷，需业务确认后再对齐）

1. **A4 筛选分组弹窗（含子分组）已实现（2026-09-11 第七轮）** —— 与关注债券弹窗（B5）一起按 app.js 模板还原，
   数据会话内保存（`Shared/Services/FilterProfileService.cs`），后端同步待接；「我的关注」侧栏 chip 仍为静态占位，
   分组查询契约到位后联动。
2. **A8 模拟按钮已移除** —— 产品侧确认为原型测试入口，客户端不实现；`POST /api/quote-confirm/mock` 不纳入接口清单。
3. **A2 的聚合口径未改** —— 原型 0910 把最优聚合键改为「仅债券代码」并用折算收益率统一比价；本次仅补 UI 与同档多笔展示，`BuildBestRows` 仍按 `代码|价格类型|清算速度` 分组，浮层的「折算收益率」列在净价/全价口径下显示 `—`（待中债估值换算服务）。业务口径确认后再改。
4. **A10 浮层交互简化** —— 原型为 hover + 延迟隐藏；WPF 用 `ToggleButton`：鼠标进入按钮即展开，鼠标离开浮层内容或点击外部（`StaysOpen=False`）收起。
5. **A12 交易日期仍为文本框** —— 原型是 `input[type=date]`；WPF 未引入 `DatePicker` 以避免深色模板缺失导致的视觉回退，格式统一为 `yyyy-MM-dd`。
6. **A12 备注为多行文本框** —— 已加 `MaxLength=120`（原型 `maxlength=120`），但未做「超出提示」的额外文案。
7. **B3 只覆盖有角标的两个子页** —— 原型也只有「报价确认」和「报价提醒」带 `.cnt`。

### 尚未验证

本机无 MSBuild / Visual Studio，无法编译。已做的静态校验：

1) 全部 XAML 均可被 XML 解析器解析（0 失败）；
2) 每个 XAML 事件处理器在对应 code-behind 中都有定义（0 缺失）；
3) 引用的 `DynamicResource` 键在 `Themes/` 中均存在（0 缺失）。

**待开发环境完成**：编译 → 运行 → 重跑 `tools/Capture.ps1` 重建 `tools/captures/*.png` 基线 → `PixelCheck.ps1` 比对。

---

## A12 复刻校准（第二轮，按 app.css 逐条核对）

第一版弹窗是「结构对了、观感不对」。把 `app.css` 里所有作用于弹窗子树的规则逐条抽出核对后，找到以下**确定性偏差**（不是主观判断，每条都能对应到具体 CSS 声明）：

| # | 偏差 | 原型 | 第一版 | 已修正为 |
|---|---|---|---|---|
| 1 | **弹窗底色错** | `.modal{background:#161b22}` | `WindowBgBrush` #080A10（近黑） | `PanelBgBrush` #161B22 |
| 2 | 边框色错 | `.modal{border:1px solid #30363d}` | `BorderStrongBrush` #484F58 | `BorderStdBrush` #30363D |
| 3 | 多了投影 | `.modal` 无 box-shadow | `DropShadowEffect` 24/4/0.6 | 移除 |
| 4 | **高度不贴身** | 内容自适应（正文 `max-height:70vh` 滚动） | 固定 620px → 大片留白 | 高度由内容决定，仅受宿主高度-40 约束，超出滚动 |
| 5 | **缺遮罩** | `.modal-overlay{background:rgba(0,0,0,.6)}` | 无（宿主未压暗） | 窗口铺满宿主 + 内部 `#99000000` 遮罩（见下） |
| 6 | 价格/报量字号 | `.quote-control{font-size:12px}`（等宽、常规字重） | 14px SemiBold | 12px 常规 |
| 7 | 备注高度 | `textarea.quote-control{height:34px;padding:7px 10px}` | 72px | 34px |
| 8 | 步进两格无分隔线 | `.quote-step-btn + .quote-step-btn{border-top:1px solid #30363d}` | 只有整组左侧 1px | 下格补上边框 |
| 9 | 输入框右内距 | `.quote-step-wrap .quote-control{padding-right:54px}` | 56px | 54px |
| 10 | 债券名字号 | `.quote-bond-name strong,.quote-bond-code{font-size:16px;color:#e6edf3;font-weight:700}`（名称与代码**同为 16px 粗**） | 14px → 误改为 13px | 16px 粗 #E6EDF3 |
| 11 | 摘要条 meta 结构 | 3 个独立 `span`，`gap:14px` | 单行字符串拼「 / 」 | 3 个 TextBlock，间距 14 |
| 12 | 摘要条下边框 | `.quote-bond-summary` 无边框 | 自加了下边框 | 移除 |
| 13 | 头部/页脚分隔线 | `#30363d` | `BorderSubtleBrush` #21262D | `BorderStdBrush` #30363D |
| 14 | 页脚按钮 | `.tb-btn`：26px 高、14px、取消=灰底 / 保存修改=红底 | 主题默认（28px/13px） | 26px / 14px |
| 15 | 校验红边 | `.quote-control.invalid{border-color:#f85149}` | 只有文案，无红边 | 校验失败时给控件红边，重试时清除 |
| 16 | 聚焦色 | `.quote-control:focus{border-color:#58a6ff}` | AccentBrush | #58A6FF |
| 17 | 方向分段圆角 | 父容器 `border-radius:5px;overflow:hidden` | 子按钮圆角（会溢出父圆角） | 由子按钮内圆角 4px 模拟裁切 |
| 18 | 报价录入浮窗底色/边框/投影 | `.quote-entry-shell{background:#0d1117;border:1px solid #30363d}` 无投影 | `WindowBgBrush` #080A10 + `BorderStrongBrush` + 投影 | `PageBgBrush` #0D1117 + `BorderStdBrush`，移除投影 |
| 19 | 报价录入浮窗遮罩 | `.quote-entry-overlay{background:rgba(0,0,0,.68);padding:20px}` | 无 | 同 #5 方案，`#AD000000`，并保留 20px 内距 |
| 20 | **输入框缺占位文案** | `input placeholder`：价格 `0.0000`、报量 `例如 1000`、报价机构 `机构简称`、报价交易员 `交易员姓名`、备注 `补充报价约束、有效期或沟通事项` | 均无 | 文本框模板内建 placeholder（`Tag` 传占位文案，`Text` 为空时显示） |
| 21 | **备注正文被压住看不见** | 备注是长文本域，内容变长应可见 | `QuoteControlNote` 继承了单行模板：`PART_ContentHost` 被写死 `VerticalAlignment="Center"`，且 `Padding=10,7` 当 Margin 扣掉后只剩 18px 视口 → 一旦内容超过一行，居中的滚动视口会**只露出中间一条**，看起来就是"文字被压没了" | 备注改用**独立的多行模板**：`PART_ContentHost` 不设垂直对齐（铺满、正常从上往下排），并固定 `Height=34`（与原型一致，超出内部滚动） |

### 第三轮：WPF 实测截图 vs 原型截图（像素级对比，2026-09-11）

用户提供了 WPF 实机截图与原型截图，用 `img_diff.py`（主色分布 + 纵向色带剖面）定量比对，结论：

**已一致（无需再改）**：弹窗底色 `#161B22`、内容/输入底色 `#0D1117`、边框 `#30363D`、主按钮 `#EF4444` —— 主色分布
（51.7% / 30.7% / 2.5% / 0.4% vs 50.1% / 32.4% / 2.7% / 0.4%）基本重合；
输入框高 34px、字段行间距、"交易日期"标签到控件的距离（20 vs 21px）均一致；标题字号一致。

**修正项**：

| # | 差异 | 原型 | WPF 实测 | 修正 |
|---|---|---|---|---|
| 22 | **备注框多占一行** | 内部可视 32px（控件 34px，单行） | **内部 48px（控件 50px）** | 固定 `Height=34`；去掉 `MinHeight/MaxHeight` 自动长高 |
| 23 | **复选框观感差异大** | 原生 checkbox：未勾选为**白底方框**（取样 `#FFFFFF` + `#767676` 边） | 主题深色方框（`#0D1117` + `#30363D`），在白底面板上几乎看不见 | `QuoteTagCheck` 改为自绘原生风：13px 白底方框 + `#767676` 边，勾选蓝底 `#3B82F6` + 白勾；不再 `BasedOn` 主题 `StcCheckBox` |
| 24 | 头部高度偏矮 | 58px（`.modal-close{padding:4px}` + 24px 图标 = 32px 点击区） | 52px（关闭按钮 24×24） | 关闭按钮改 32×32，头部回到 60px |
| 25 | **备注文字被压成一条横向细缝** | 34px 单行高 | 34px + 上下 padding 7 → 内容区仅 18px；备注带 `TextWrapping=Wrap` + `VerticalScrollBarVisibility=Auto`，文本换行后溢出 18px 视口触发内滚，滚动偏移正好落在两行之间 → 文字只剩一条缝 | 备注**改为复用单行控件模板**（`BasedOn QuoteControl`：内容区 32px、不换行、无内滚），高度仍 34px；文本恒定可见 |

### 结论：WPF 下「固定 34px + 换行 + 内滚」这个组合不可用

原型备注是 `textarea{height:34px;padding:7px 10px}` —— 内容区正好 = 一行（18px），浏览器里 `overflow` 由浏览器处理，不会出现"滚动错位只露一条缝"。
WPF 的 `TextBox` 是「内容高度 > 视口 → 出现内滚 + 自动滚动到光标」的模型，34px 盒子里换行文本必然溢出，
滚动偏移会落在两行之间，于是文字被压成一条缝（用户实机看到的就是这个）。

因此本项目约定：**34px 的单行高度不要配换行**；要真正的多行备注，就把盒子做高（≥2 行）再开换行。
当前实现取前者（与原型高度一致、文本可见）；若要改成多行备注，把 `Height` 改为 68 并加回 `TextWrapping/AcceptsReturn/VerticalScrollBarVisibility` 即可。

**方法论**：这轮全部结论都来自脚本量测（`runs_in_column` 量控件高度、主色占比量配色），
不再靠肉眼判断——尤其"背景色看起来更暗"这一直觉是**错觉**，实测两者主色完全同色。

---

### 取证方式（避免再靠推测）

本轮不再「读 CSS 猜效果」，而是把导出里的弹窗 DOM 抽出来单独渲染后截图对比：

1) 生成独立预览页 `.tmp/prd/销交平台原型0910/报价弹窗预览.html`（复用导出的 `app.css` / `fonts.css`，只把 `.modal-overlay` 的 `opacity/pointer-events` 打开）；
2) 用本机 Chrome 无头截图拿到原型弹窗的真实渲染：
   ```
   chrome --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=2 \
          --window-size=1000,700 --screenshot=<out.png> <预览页>
   ```
3) 产物：`.tmp/shots/proto-quote-modal.png`（1440×900 全景）、`.tmp/shots/proto-quote-modal-2x.png`（细节放大）。

**建议后续所有「还原度」问题都先跑这一步**——上面 #1（弹窗底色）和 #10（债券名字号）都是靠截图一眼发现的；
#10 的规则是 `.quote-bond-name strong,.quote-bond-code{...}` 这类**分组选择器**，按 `.quote-code` 关键词抓 CSS 会漏掉前半段。

### 遮罩实现方式（新增 `Shared/Services/OverlayBackdrop.cs`）

WPF 的独立 `Window` 无法给宿主窗口叠加半透明层，因此改为：

1) 弹窗窗口在 `ShowDialog` **之前**把自己的 `Left/Top/Width/Height` 对齐到宿主窗口（外扩 8px，掩盖最大化窗口的不可见缩放边框）；
2) 窗口内部分两层：底层是半透明黑 `Border`（编辑报价 60%、报价录入 68%），上层是居中的弹窗/外壳；
3) 点遮罩空白处关闭（对应原型 `onclick="if(event.target===this)closeModal()"`）。

这样既拿到整窗压暗效果，也不改动插件原有的「弹窗即独立 Window」架构。

### 备注（34px 单行盒）的写法约定 —— 2026-09-11 修订

**不要把「34px 固定高」和「换行 + 内滚」放在一起。** 踩过两次坑：

1. 第一版让 `PART_ContentHost` 垂直居中（单行控件的写法）+ 换行 + 内滚 → 内容超出 18px 视口时，
   居中的滚动视口只露出内容中间一条，文字像被压住；
2. 第二版去掉垂直居中、改内容驱动高度（`MinHeight/MaxHeight`）→ 单行文本也被撑成两行高（50px vs 原型 34px）；
3. 第三版回到固定 34px + 换行 + 内滚 → 内容溢出触发自动滚动，滚动偏移落在两行之间，文字被压成**一条横向细缝**。

**结论（本项目约定）**：
- 34px 高度的输入盒 = 单行盒，就**不要换行**（`TextWrapping` 保持默认 NoWrap），让它和价格/报量框走同一套模板；
- 需要真正多行编辑时，盒子高度至少给到 2 行（68px）再开 `TextWrapping=Wrap` + `AcceptsReturn` + `VerticalScrollBarVisibility=Auto`；
- 判断依据可以用 `img_diff.py` 的 `runs_in_column` 量「内容区可视高度 vs 一行文字高度」——
  内容区只剩一行高度时，任何滚动都会导致"只露一条"。

### 仍未对齐 / 待确认

1. **日期控件**：原型是 `input[type=date]`（带原生日期选择器与 `.quote-entry-date{color-scheme:dark}`），WPF 仍是文本框 + `yyyy-MM-dd` 格式。若要对齐需自建深色 `DatePicker` 模板，成本较高，暂留。
2. **下拉与复选框是插件主题控件**：原型 `.quote-control` 的 `<select>` 与 `.quote-tag-check` 的原生 checkbox 被替换为插件的 `StcComboBox` / 自绘原生风 `QuoteTagCheck`。只对齐了高度（34px）、字号（12px）与勾选后的文字色（#c9d1d9）。
3. **字体**：原型是 Inter + JetBrains Mono；插件用系统 Segoe UI + Cascadia/Consolas（与全站一致）。
4. **备注框不支持换行**：当前是 34px 单行盒（与原型的盒高一致）。若产品要求真正的多行备注，需把盒高提到 68px。

---

---

## 行右键菜单复刻（第四轮，2026-09-11 用户实机反馈）

用户反馈两点：① WPF 右键菜单与原型差别大；② **菜单往鼠标左边弹出**。

### 差异与修正

| # | 差异 | 原型 | 修正 |
|---|---|---|---|
| 1 | **菜单项内容完全不同** | `#ctxMenu` 共 8 项：`添加到分组 ▶`（子菜单：暂无关注分组 / + 新建分组）┃ `✚ 新报价` / `编辑报价` / `↩ Refer` ┃ `⬡ 打包` / `◎ OCO` ┃ `报价日志`，带图标与分隔线 | 原 WPF 只有 4 项纯文字（Copy 报价文本 / Refer / 编辑报价 / 导出）。已按原型重建，并抽成三个看板共用字典 `Modules/MarketCenter/View/BoardRowContextMenu.xaml` |
| 2 | **弹出方向朝左** | `m.style.left = Math.min(e.clientX, innerWidth - 200)`（左上角对齐鼠标，越界只左移，**从不翻转**） | `Placement="MousePoint"`（= 左上角对齐鼠标，WPF 默认即原型语义）+ 用项目既有的 `RelativeDropHorizontalOffsetConverter` 补回菜单宽度，抵消系统「菜单左对齐」造成的整体左移。见下方「第三轮：最终结论」 |
| 3 | 菜单外观偏小/偏朴素 | `.ctx-menu{radius:8px;min-width:180px;box-shadow:0 8px 24px rgba(0,0,0,.5)}`、`.ctx-item{padding:8px 14px;font-size:12px;gap:8px}`、`.ctx-divider{height:1px;background:#21262d}`、图标 14px 描边 currentColor、`.active{background:rgba(121,192,255,.1);color:#79c0ff}`、`.disabled{opacity:.42}` | 主题 `StcContextMenu` / `StcMenuItem` / 新增 `StcCtxSeparator`、`StcCtxIcon` 全部按此重写 |

### 第二轮：实机验证后又修的两处（2026-09-11）

**① 菜单跳到鼠标右下方很远（比"往左弹"更严重）**

第一版为了不翻转，用 `PlacementMode.Absolute` + 自己把 `PointToScreen` 的**设备像素**用
`CompositionTarget.TransformFromDevice` 换成 DIP 后当偏移量。实机上偏移量按绝对屏幕坐标成比例放大，
菜单整体跳到鼠标右下方很远 —— 即 Device↔DIP 换算与实际不符（高分屏下 `Absolute` 的偏移量基准与假设不一致）。

**改法：彻底不做 DPI 换算。** 改用 `PlacementMode.Relative`——它的偏移量就是
「PlacementTarget 自身坐标系」，直接取 `Mouse.GetPosition(target)` 即可；
边界收敛也全部在同一坐标系里算（`Window.GetWindow(target).TranslatePoint(new Point(0,0), target)`
拿到宿主窗口原点，再做 `min(clientX, 窗口宽 - 菜单宽)`）。Relative 也不会翻转。

**② 图标不渲染**

第一版把图标放在 `MenuItem.Icon`，模板里用 `ContentPresenter Content="{TemplateBinding Icon}"` 呈现。
实机放大截图后确认：**图标没画出来，而且图标列被压掉了（文字直接从 14px 内距开始，说明 Icon 被判为空而折叠）**。
改法：图标写进 `MenuItem.Header`（`StackPanel{HBox + TextBlock}`），模板只呈现 Header；
同时给 `StcCtxIcon` 的 `Stroke` 绑定加 `FallbackValue=#C9D1D9` —— 即使取不到 `MenuItem` 祖先的前景色，
图标也不会变成不可见。

### 复测数据（从实机截图量取）

- 菜单宽 **178px**（原型 184px）、分隔线位置推得的**条目高 29px**（原型 29.3px）→ 条目内距 `14,6.5` 正确；
- 修完这两处后菜单项内容、顺序、分隔线、子菜单箭头均与原型一致。

### 第三轮：定位反复失败后的最终结论（2026-09-11）—— 改用项目既有做法

第二轮之后又试了两种"自己算位置"的写法（`PlacementMode.Absolute` + 自算屏幕坐标；`Relative` + 附加属性 + 弹出自校正），
实机都不对（先是"跳到鼠标右下方很远"，再是"先出现在右边、闪到左边"）。**这些尝试已全部还原**：

- `Shared/Services/ContextMenuPlacer.cs` 已删除（csproj 里的 Compile 项一并移除）；
- 三个看板的 `Grid_OnContextMenuOpening` 恢复原实现（只注入行 DataContext，见 `git diff` 无改动）。

复盘结论：**"菜单左上角对齐鼠标"在 WPF 里本来就有直接对应写法（`Placement="MousePoint"`），不需要自算坐标**。
真正需要额外处理的只有一件事 —— 系统「菜单左对齐」：

> Windows 的 `SPI_GETMENUDROPALIGNMENT`（WPF 里是 `SystemParameters.MenuDropAlignment`）为 true 时，
> WPF 会把弹出菜单**整体左移自身宽度**，菜单于是出现在鼠标**左边**（鼠标落在菜单右上角）——
> 这正是一开始"菜单往鼠标左边弹"的原因。（第二轮"位置跳到很远"是我自己算的偏移量被叠加到鼠标点上造成的，
> 与系统设置无关；"闪一下"则是那次自校正补偿动作被人眼看到。）

项目里已有现成的处理方式：`STC.Common/Convertor/RelativeDropHorizontalOffsetConverter.cs`
—— 系统右对齐时把值转成 0，左对齐时原样返回（即补回菜单宽度）。STC.Common 各处 ComboBox 的
`PART_Popup` 都这么用（`Popup.HorizontalOffset="{Binding ActualWidth, ElementName=..., Converter={StaticResource rdhoConvertor}}"`）。

**本次落地**（`Modules/MarketCenter/View/BoardRowContextMenu.xaml`）：

```xml
<ContextMenu x:Key="BoardRowMenu" x:Shared="False" Style="{DynamicResource StcContextMenu}"
             MinWidth="184"
             Placement="MousePoint"
             HorizontalOffset="{Binding MinWidth, RelativeSource={RelativeSource Self},
                                Converter={StaticResource RdhoConverter}}">
```

1. `Placement="MousePoint"` —— 菜单左上角对齐鼠标，即原型 `showCtxMenu` 的语义；
2. `HorizontalOffset` 经转换器补回菜单宽度，抵消系统「菜单左对齐」造成的整体左移；系统为右对齐时该值为 0，行为就是默认的鼠标处弹出；
3. 补偿值取 `MinWidth`（184，与原型实测宽度一致）而**不是 `ActualWidth`**：`ActualWidth` 要等 Popup 测量完才有值，
   首次弹出时读到 0 就补不上（会表现为"第一次弹在左边、第二次才正常"）；
4. 保留 `x:Shared="False"`（与定位无关）：同一个 ContextMenu 实例挂到三个 DataGrid 上会报
   "already the logical child of another element"，加此标记后每次 `{StaticResource}` 返回新实例。

### 新增的待接项（右键菜单暴露出来的）

1. **报价日志弹窗（原型 `quote-log-modal`）整体缺失** —— 原型右键菜单「报价日志」会打开一个带表头（时间/操作/报价/操作人…）、空态文案「暂无报价操作记录」的弹窗，插件目前没有该弹窗 → 暂以 toast 待接。**建议纳入后续排期（新增 D 类差距）。**
2. **打包 / OCO 直接改标签** —— 原型 `updateCtxQuoteFlag` 直接更新该笔报价的标签（并显示 `.active`），需要报价编辑接口；插件当前按行上的实际标签显示勾选态，点击提示待接。
3. **添加到分组 / 新建分组** —— 依赖关注分组管理弹窗（与 B5 同一块 TODO）。
4. 原型菜单**不含**「Copy 报价文本 / 导出」两项（工具栏才有）；插件原菜单有，已按原型移除。

---

## 整体页面对比（第五轮，2026-09-11，用户提供两张全页截图）

> 用户给「原型整体界面」与「WPF 实现效果」两张全页截图，问「是不是还有很多细微的差别」。
> 结论：**有，而且大多是系统性的**。大的骨架（三栏、面板宽、配色）已对齐，差别集中在
> **表格对齐方式、列宽、行高、按钮配色、几处字号**——都是"单看每处很小、但整页一起看就明显不同"的类型。

### 0. 截图本身的状态差异（不是实现差距，先排除）

- 原型截图里**报价录入浮窗是打开的**，遮住了右侧详情面板的一大块；WPF 截图是正常的看板态。
  → 该区域无法逐像素比对，本节右侧详情改用 `BondDetailPanel.xaml` + 原型 CSS 对照，不靠这张截图。
- 尺寸：原型 1666×936、WPF 1911×957（WPF 窗口更宽，故表格可见列更多）。

### 1. 已一致（核对过，不要再改）

| 项 | 原型 | WPF | 证据 |
|---|---|---|---|
| 三栏布局 + 左右面板宽 | `.board-sidebar`/`.board-detail` = 384px（折叠 40px） | `PanelExpandedWidth=384` / `PanelCollapsedWidth=40` | `MarketCenterViewModel.cs` |
| 顶栏高度 | `.header{height:48px}` | `RowDefinition Height="48"` | `Shell/View/MainView.xaml:19` |
| 顶栏一级导航字号 / 活动字色 | `.nav-btn{font-size:15px}`、`.active{color:#79c0ff}` | `StcNavItem{FontSize=15}` + `AccentBrush=#79C0FF` | `Themes/Controls.xaml:827`、`Colors.xaml:33` |
| 表格结构 | 20 列 + 前 3 列 `position:sticky` | 20 列 + `FrozenColumnCount="3"` | `app.js BOARD_COLUMN_DEFS` / `BoardListView.xaml:90` |
| 行/表头高度**设定** | `table.dt th,td{padding:7px 10px}` | `RowHeight=32` / `ColumnHeaderHeight=32` | `Controls.xaml:466-467` |
| 横向网格线 / 边框 | `#21262d` | `BorderSubtleBrush=#21262D` | `Colors.xaml` |
| 大面积配色 | `#0D1117` / `#161B22` / `#30363D` / 选中 `#26384a` / 悬停 `#252d38` | 同色 | `Colors.xaml` |
| 标签（filter-tag） | 13px、`padding:2px 8px`、底 `#21262d`、active 金底 `#D29922` | `StcTagBase{FontSize=13;Padding=8,2}`、`StcTagNeutral` active 金底 | `Controls.xaml:603-609` |
| 报价筛选条件集合 / 各区结构 | 清算速度 4 项、计价方式 2 项、`filter-grid` 10 组区间、债券/流动性/信用/条款/区域 | 逐项一致 | 见 §C |

### 2. 差别 A：表格对齐方式是**系统性的**（观感差异最大的一项）

原型把对齐写在列定义上，`table.dt td,th{text-align:center}` 是**默认**，只有 6 列显式 `align:'left'`：

```
{id:'m',  label:'剩余期限', align:'left'}   {id:'c', label:'代码',  align:'left'}
{id:'n',  label:'简称',     align:'left'}   {id:'direction', label:'方向', align:'left'}
{id:'bp', label:'报价',     align:'left'}   {id:'bv', label:'报量(万)', align:'left'}
// 其余 14 列没有 align → 继承 td/th 的 text-align:center
```

对齐在 `app.js` 里对**表头和单元格同时生效**（`applyBoardTableLayout` 第 412-421 行写 `<th style="text-align:left">`；
`boardRowHtml` 第 1373 行写 `text-align:${align} !important`）。所以原型是：

- **左对齐**：剩余期限 / 代码 / 简称 / 方向 / 报价 / 报量（6 列）
- **居中对齐**：报价机构 / 报价交易员 / 长城交易员 / 最新成交价 / 债券库 / 主体评级 / 债券评级 / 中债净价 / 中债偏离 / 中债YTM / 中证净价 / 中证全价 / 中证YTM / 报价时间（**14 列**）

WPF 现状：

- 表头 `StcColumnHeader{HorizontalContentAlignment=Left}` → **20 个表头全部左对齐**；
- 单元格：文本列左对齐（`StcTrimText`）、数值列右对齐（`StcNumberCell`）。

→ 结果：**WPF 表头一律靠左、数字一律靠右；原型表头/内容大面积居中**。这一项比列宽更影响"一眼看上去像不像"。
→ 改法：给 `StcColumnHeader` 加居中的表头样式（或在 `BoardListView` 里按列指定 `HeaderStyle`/`CellStyle`），
把 14 列改成居中，保留前 6 列的左对齐。

### 3. 差别 B：列宽全线偏窄（总宽 −406px，约 −18%）

原型（`app.js BOARD_COLUMN_DEFS['board-credit-all']`，求和 2204；CSS 另写死 `.board-all-table{width:2200px!important}`）
vs WPF（`BoardListView.xaml`）：

| 列 | 原型 | WPF | 差 | | 列 | 原型 | WPF | 差 |
|---|---:|---:|---:|---|---|---:|---:|---:|
| 剩余期限 | 96 | 92 | −4 | | 债券库 | 100 | 80 | −20 |
| 代码 | 132 | 108 | −24 | | 主体评级 | 90 | 80 | −10 |
| 简称 | 190 | 132 | **−58** | | 债券评级 | 90 | 80 | −10 |
| 方向 | 72 | 52 | −20 | | 中债净价 | 105 | 82 | −23 |
| 报价 | 150 | 80 | **−70** | | 中债偏离 | 105 | 78 | **−27** |
| 报量(万) | 82 | 80 | −2 | | 中债YTM(%) | 105 | 92 | −13 |
| 报价机构 | 120 | 110 | −10 | | 中证净价 | 105 | 82 | −23 |
| 报价交易员 | 105 | 110 | +5 | | 中证全价 | 105 | 82 | −23 |
| 长城交易员 | 130 | 110 | −20 | | 中证YTM(%) | 105 | 92 | −13 |
| 最新成交价 | 92 | 92 | 0 | | 报价时间 | 125 | 84 | **−41** |
| | | | | | **合计** | **2204** | **1798** | **−406** |

→ 20 列里 18 列偏窄，只有「最新成交价」相同、「报价交易员」反而宽 5px。
→ 差额最大的 4 列：报价 −70、简称 −58、报价时间 −41、中债偏离 −27。前 3 列是冻结列（左对齐），
再加「方向 −20」，导致 WPF 左侧固定区明显更挤。

### 4. 差别 C：行高差约 2px

| | 原型 | WPF |
|---|---|---|
| 行高 | `td{padding:7px 10px}` + 14px 文本行高（中文回退字体 normal ≈20px）→ **≈33–34px** | `RowHeight=32`（固定） |
| 表头高 | `th{padding:7px 10px}` + 13px → ≈30–32px | `ColumnHeaderHeight=32` |

量测：`cmp-table.png`（1911 宽 WPF 截图的 1:1 裁剪）网格线间距稳定 **32px**，与 `RowHeight=32` 吻合；
两张同行数裁剪图（`zoom-tablerows-proto/wpf`，同高 306px、宽 1080/1440 同内容不同缩放）的网格线间距 62.5 / 58 px，
按宽度归一到同一缩放后 **原型行高 ≈ WPF × 1.078 ≈ 34px**。
→ 单行差 2px 不起眼，但 20 行叠加就是 40px 的"整表高度/疏密"差异。改法：`RowHeight` 32 → 34（或把 cell 上下 padding 由 `8,3` 放宽）。

### 5. 差别 D：顶栏缺「需求管理」

原型 `index.html` 顶栏导航有 **4 项**：报价录入 / 行情中心 / 报价管理 / **需求管理**，
`需求管理` 按角色（`admin` / `technician`）用 `display:flex` 显示。
WPF `Shell/MainViewModel.cs:49-54` 的 `NavItems` **只有前 3 项**。
→ 需补 `需求管理` 导航项 + 对应页面，并按角色控制可见性。

### 6. 差别 E：活动态下划线颜色不一致

| | 原型 | WPF |
|---|---|---|
| 一级导航 active | `.nav-btn.active{border-bottom-color:#79c0ff}` | `StcNavItem` `IsSelected` → `BorderBrush = OfrBrush` = **#3B82F6** |
| 子页 active | `.subtab.active{border-bottom-color:#79c0ff}` | `StcSubTabItem` `IsSelected` → `BorderBrush = OfrBrush` = **#3B82F6** |
| active 文字色 | `#79c0ff` | `AccentBrush` = `#79C0FF` ✓ |

→ **文字色一致，唯独下划线用了 OfrBrush（#3B82F6，偏深）而不是 #79C0FF。** 两处（`Controls.xaml:845`、`798`）各改一个 Setter 即可。

### 7. 差别 F：子页条字号 / 内距

| | 原型 | WPF |
|---|---|---|
| 子页 | `.subtab{font-size:15px;padding:7px 14px}` | `StcSubTabItem{FontSize=14;Padding=12,0}` |
| 一级导航 | `.nav-btn{padding:12px 18px;font-size:15px}` | `StcNavItem{Padding=16,0;FontSize=15}` |

→ 一级导航字号一致（横向内距 16 vs 18，可忽略）；**子页字号 WPF 是 14、原型是 15**，横向内距 12 vs 14。

### 8. 差别 G：「筛选」按钮配色/高度（计价方式行 + 债券筛选剩余期限行）

原型 `filter-btn-primary`：**灰底** `#21262d`、字 `#8b949e`、`border:1px solid #30363d`、`padding:2px 10px`、12px（高≈20px）。
WPF 用的是 `StcButtonPrimary`：**蓝底** `ButtonBlueBrush` = **#2563EB**、`Height=30`、`FontSize=11`、`Padding=8,3`。

出现位置（两处）：`CreditFilterPanel.xaml:51`（计价方式行）、`CreditFilterPanel.xaml:150`（债券筛选剩余期限行）。
→ 结构上原型这两处**确实都有**「筛选」按钮（`index.html` 1243 / 736），不是多做了；**差别只在配色与高度**（灰 20px ↔ 蓝 30px）。
→ 改法：给筛选按钮换一个「次级灰底」样式（`#21262D` 底 + `#8B949E` 字 + `#30363D` 边框 + 高 20–22），不要用主色蓝按钮。

### 9. 差别 H：区间输入行（`FilterRangeRow` ↔ 原型 `.filter-grid-item`）

原型（`app.css:347-351`、`index.html:1244-1253`）：
`.filter-grid{grid-template-columns:1fr 1fr;gap:4px 7px}`、`.filter-grid-item{display:flex;gap:3px}`、
`.filter-label{font-size:12px;width:52px}`、`.filter-input{width:43px;padding:2px 4px;font-size:12px;text-align:right}`、
`.filter-sep{color:#484f58;font-size:11px}`、`.filter-unit{font-size:11px;color:#7d8590;min-width:22px}`。

WPF `Shared/Components/FilterRangeRow.xaml`：

| 项 | 原型 | WPF | 差 |
|---|---|---|---|
| 标签列宽 | 52px（固定） | 64px（`ColumnDefinition Width="64"`） | +12 |
| 标签字号 / 色 | 12px / `#c9d1d9`（报价筛选区内） | 11px（`StcTextFaint`）/ `#7D8590` | 字号 −1、色偏冷 |
| 输入框宽 | 43px（固定） | `*` 占满剩余 | 显著变宽 |
| 输入框字号 | 12px | 11px | −1 |
| 输入框内距 | `2px 4px` | `6,3` | 上下 +4 |
| 输入框对齐 | **右对齐** | 左对齐 | ← |
| 分隔符 | `.filter-sep` 11px `#484f58` | `StcTextFaint` 11px `#7D8590` `Margin=4,0` | 色差 |

→ 后果：原型是"窄输入框 + 右对齐"，WPF 是"宽输入框 + 左对齐"。**这是左侧面板一眼可见的差别**。
→ 注意**例外**：原型 `债券筛选 → 剩余期限` 那一行（`index.html:736`）的输入框带内联 `style="flex:1"`，
即**该行输入框确实是自适应宽的**；只有 `filter-grid` 里的 10 组是 43px。若要做准，`FilterRangeRow` 需要
一个「固定 43px / 自适应」的开关，而不是一律 `*`。

### 10. 差别 I：「计价方式」两个选项的横向内距

原型：`filter-tag` 加内联 `style="padding:2px 28px;font-size:12px"`（收益率 / 净价，两者 `gap:6px`）。
WPF：`ToggleButton ... FontSize="12" Padding="20,2"`（`CreditFilterPanel.xaml:47-50`）。
→ 横向内距 **28 vs 20**；且原型的标签在那一行有独立 `filter-tags{gap:6px}` 容器，WPF 用 `Margin="0,0,6,0"` 等效。

### 11. 本轮结论汇总

| 编号 | 差别 | 原型 | WPF | 影响面 |
|---|---|---|---|---|
| 整-A | 表格对齐方式 | 14 列居中 / 6 列左 | 表头全左、数值右 | **大**（整表观感） |
| 整-B | 列宽 | 合计 2204 | 合计 1798（−18%） | **大** |
| 整-C | 行高 | ≈34px | 32px | 中 |
| 整-D | 顶栏「需求管理」 | 有（按角色） | 无 | 中（功能缺项） |
| 整-E | active 下划线色 | `#79C0FF` | `#3B82F6` | 小但显眼 |
| 整-F | 子页字号 | 15px | 14px | 小 |
| 整-G | 「筛选」按钮 | 灰底 20px | 蓝底 30px | 中 |
| 整-H | 区间输入行 | 52/43px、12px、右对齐 | 64/自适应、11px、左对齐 | 中 |
| 整-I | 计价方式内距 | 28px | 20px | 小 |

**优先级建议**：整-B + 整-A 一起做（都是 `BoardListView` 的列定义/样式，一次改完再重建像素基线）；
整-H 独立（只动 `FilterRangeRow`）；整-G + 整-E + 整-F 是主题样式微调，可一批；整-D 需要新页面，单独排期。

> ⚠️ 整-B / 整-C 会改变表格布局，**改完必须先用 `tools/Capture.ps1` 重建 `tools/captures/*.png` 基线**，否则 `PixelCheck.ps1` 会整体失败。

---

## 左侧筛选栏 + 提醒弹窗观感对齐（第六轮，2026-09-11 用户实机反馈）

用户给「左侧筛选栏」与「右下角提醒弹窗」两张原型/WPF 截图问两件事：
① 左侧筛选栏的"标签-输入框-按钮"布局看着别扭；② 提醒弹窗的每组之间**应该像多个独立窗口叠在一起**，是不是要多个透明 Window。

### 1. 左侧筛选栏：找到的真实 Bug，不是"观感差异"

`CreditFilterPanel.xaml:35-53` / `RateFilterPanel.xaml:34-52` / `AlertFilterPanel.xaml:34-52` 的
**计价方式行**结构是：

```xml
<Grid>      <!-- 外层：label 在 col0，控件区在 col1 -->
    <TextBlock Text="计价方式" Margin="0,0,8,0"/>
    <Grid>  <!-- 内层：原意是想把"收益率/净价/筛选"水平排开 -->
        <ToggleButton Content="收益率"/>
        <ToggleButton Content="净价"/>
        <Button Content="筛选" Style="StcButtonPrimary"/>
    </Grid>
</Grid>
```

两个真问题（同一组 Bug 在 3 个 panel 重复出现）：

| # | Bug | 后果 | 修复 |
|---|---|---|---|
| 1 | **内层 `<Grid>` 没有 `Grid.Column="1"`** —— 内层默认进 col0，与外层 label 挤在同一格，3 个控件垂直堆叠（收益率上 / 净价中 / 蓝"筛选"下），而 `StcButtonPrimary` 是 30px 高的蓝底不透明按钮，正好把它俩盖住 | WPF 截图里"计价方式"行只看得见一个孤零零的蓝按钮，下方两个 tag 完全被覆盖 | 给内层 `Grid` 加 `Grid.Column="1"`，并改成「`* / Auto` 2 列」让 tags 左扩、按钮贴右 |
| 2 | **`筛选`按钮用 `StcButtonPrimary`**（蓝底 30px 字号 11） | 原型 `.filter-btn-primary` 是灰底 `#21262D`/字 `#8B949E`/边框 `#30363D`/12px/高≈22px。整-G 早列过 TODO | 改用主题里新增的 `StcFilterMiniButton`（高 22 / padding 10,0 / 灰底） |

同轮顺手修两个独立的不一致：

| 文件:行 | 现状 | 改 |
|---|---|---|
| `CreditFilterPanel.xaml:150` 债券筛选「剩余期限」行的"筛选"按钮 | `StcButtonPrimary` 蓝 30px | `StcFilterMiniButton` 灰 22px（与原型 `.filter-btn-primary` 一致） |
| `RateFilterPanel.xaml:134` 同上 | 同 | 同 |
| 三个 panel 计价方式 toggle 内距 | `StcTagNeutral` 默认 `Padding=8,2` | `Padding="28,2" FontSize="12"`（原型内联 `padding:2px 28px;font-size:12px`） |

`AlertFilterPanel` 的剩余期限只有 tag 没有 range 行 + 筛选按钮，不需要改。

### 2. 主题新增样式（仅 filter-panel 专用）

`STC.SalesTrading.Plugin/Themes/Controls.xaml` 末尾追加：

| 样式 | 用途 | 关键值 |
|---|---|---|
| `StcFilterLabel` | 原型 `.filter-label` | 13px、`#6e7681`、`MinWidth=70`、`VerticalAlignment=Top` |
| `StcFilterButtonBase` | 灰底实心按钮基类 | 12px SemiBold、灰底 `#21262d`、圆角 3 |
| `StcFilterButton` | 顶栏"清除过滤" | 高 27、padding 14,0（对应原型 `.tb-btn`） |
| `StcFilterMiniButton` | 行内"筛选"按钮（取代 StcButtonPrimary） | 高 22、padding 10,0（对应原型 `.filter-btn-primary`） |
| `StcFilterTag` / `StcFilterTagAdd` | 标签列只读 tag / "+" | 高 22、padding 8,0（原型 `.filter-tag`） |
| `StcFilterInput` | 区间输入框（43×22 / 右对齐 / placeholder via `Tag`） | `TextBox` 完整自定义模板：`#0d1117` 底、聚焦蓝边、占位 TextBlock 通过 `Tag` 触发 |

并把 `StcSectionTitle` 的字号从 11 / 字色淡灰改成 **13 / 600 / `#8b949e`**（与原型 `.filter-section-title` 完全一致，
原文 `.filter-section-title{font-size:13px;font-weight:600;color:#8b949e;margin-bottom:8px;text-transform:uppercase;letter-spacing:.5px}`，WPF 不支持字距则按视觉等效忽略 letter-spacing）。

并给 `StcTextBox`（标准 28px 输入框）加上**占位文案**支持：模板内 `TextBlock x:Name="ph" Text="{TemplateBinding Tag}" Visibility="Collapsed"`，`Text==""` 时显示。
顶上「搜索债券、机构、交易员、发行人」placeholder 走的就是这个，文本框 `Tag="搜索..."` 一行即生效。

### 3. `FilterRangeRow`（区间输入行）新建

把原型 `.filter-grid-item{label 52px + input 43px + sep + input 43px + unit}` 抽成可复用控件：

```
┌─────┬───┬───┬─┬───┐
│ Bid │ 1 │ 2 │ -│ 5%│     ← ShowMax=True（默认）
└─────┴───┴───┴─┴───┘
┌─────┬───┬───┐
│Vol.B│ 1 │ 万│            ← ShowMax=False
└─────┴───┴───┘
```

- `Label / Unit / MinText / MaxText` —— `DependencyProperty`；
- `ShowMax` —— 控制分隔符与大输入框可见性（占位补"最小" / "最大"），对应原型 Vol.Bid/Vol.Ofr 只有"最小"；
- `IsDisabled` —— 整行置灰 0.68 + IsEnabled=False，对应原型 `.filter-page-unavailable`（Bid-Ofr 当前页不可用）。

WPF 第五轮整-H「区间输入行样式」TODO 由此控件 **落地**（52px 标签 / 43px 右对齐输入框 / 11px 间隔符 / 11px 单位，与原型逐项一致）。

> 注：原型 `债券筛选 → 剩余期限` 那一行（`index.html:736`）的输入框带内联 `style="flex:1"`，
> 即**该行输入框确实是自适应宽的**；只有 `.filter-grid` 里的 10 组是 43px。WPF 这一行目前用的是
> 直接写 `<TextBox Width="*">` + `<ComboBox Width="52">`，与原型 flex:1 等效，**不需要改**，仍归当前 row。

### 4. 提醒弹窗："多窗口观感"——不要多个 Window，单窗口 + 多卡片

群里问"是不是要多个透明 Window"，答案是**不用** —— `AlertPopupWindow` 本身就是
`WindowStyle=None / AllowsTransparency=True / Background=Transparent / ShowActivated=False / Topmost=True`
的独立透明 Window，`AlertPopupService.Groups` 是一个 `ObservableCollection<AlertPopupGroup>`，
**一组 = 一个 `AlertPopupGroup`**（按 `TypeLabel` 聚合），所以**早就已经是"一窗多组"的结构**。

之所以看着不像多窗口，是因为：

1) 每组的 `DataTemplate`（`AlertGroupTemplate`）只画了一根 `BorderThickness="0,0,0,1"` 的灰色细线作分隔，
   没有**自己的卡片框**（圆角 / 阴影 / border）；
2) 最外层包了一个**统一的圆角卡片 Border**，把所有组粘在同一张皮上。

原型 `.alert-popup-wrap{gap:10px;flex-direction:column}` 让每组自己是一张卡、组间距 10px：

```css
.alert-popup{background:#161b22;border:1px solid #30363d;border-radius:8px;
             width:420px;box-shadow:0 8px 24px rgba(0,0,0,0.6)}
```

#### 改法（只动 XAML，不改 service / VM）

1) **每组 DataTemplate 升级为独立卡片** —— 把原来那层薄分隔 `Border` 换成

   ```xml
   <Border Background=PanelBgBrush(#161b22)  BorderBrush=BorderStdBrush(#30363d)
           BorderThickness=1  CornerRadius=8  Padding=10>
       <Border.Effect><DropShadowEffect BlurRadius=24 ShadowDepth=4 Opacity=0.55/></Border.Effect>
       <StackPanel>...原 header + ItemsControl...</StackPanel>
   </Border>
   ```

   `StackPanel` 默认无外边距，多卡堆叠时阴影自然向上叠。

2) **最外层换成 `StackPanel` + ItemContainerStyle Margin** —— 默认 `ItemsControl` 的 `ItemsPanel` 就是 `StackPanel`，
   给它一个 `StackPanel.Resources` 里的 implicit Style：

   ```xml
   <StackPanel Margin="12">
       <StackPanel.Resources>
           <Style TargetType="ContentPresenter">
               <Setter Property="Margin" Value="0,0,0,10"/>
           </Style>
       </StackPanel.Resources>
       <ItemsControl ItemsSource="{x:Static ...AlertPopupService.Groups}"
                     ItemTemplate="{StaticResource AlertGroupTemplate}" Width="400"/>
   </StackPanel>
   ```

   `Margin="0,0,0,10"` 替代 `gap:10px`（每张卡底部 10px 间隔，与 CSS `gap` 等效）。

3) **宽度**：原型 420px（含外边距），WPF 实测 400 + 12×2 margin = 424px，与原型 1:1；
   弹窗整体 `SizeToContent=WidthAndHeight`，高度由 stack 出来的所有卡片高度自适应，不用手动算。

#### 为什么不用 N 个透明 Window

| | 单 Window + 多卡片（采用） | N 个透明 Window（否决） |
|---|---|---|
| 定位 | `SizeChanged → Reposition()` 重算总高 | N 个 Window 各自靠右下角，互相错位 + 任务栏避让 |
| 关闭动画 | 可对整组做联动（清空时整体淡出） | 必须每个 Window 单独调度 |
| 性能 | N 组共一个 DWM 句柄 | N 个顶级 Window，DWM 合成开销 × N |
| 拖动/缩放 | 整组一起动 | 「拖一张卡」不是设计目标 |
| Z-order | 单 Window 内置 | N 个 Topmost 互相打架 |

→ 项目一致的设计是「弹窗即独立 Window，`Service.Show` 管理多弹窗」，沿用这个范式即可。

### 6. 追加（同日晚）：全部筛选组改为「label 左列 + 内容右列」

用户确认：**原型每一个筛选组**（清算速度/细分券种/热门标签/发行人/剩余期限…）都是
`.filter-group{display:flex}` = 左侧 70px `.filter-label` + 右侧 `flex:1` 内容（tags / 搜索框 / 下拉都一样）；
只有「报价筛选 / 债券筛选 / …」这种**分区大标题**才是上下结构。WPF 此前把这些组全部做成了"标题在上、选项在下"。

已批量重构四个面板（Credit 29 组 / Rate 11 组 / Alert 35 组 / Intent 29 组）为统一结构：

```xml
<Grid Margin="0,0,0,8">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />   <!-- StcFilterLabel 自带 MinWidth=70 -->
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    <TextBlock Text="热门标签" Style="{DynamicResource StcFilterLabel}" />
    <WrapPanel Grid.Column="1" Margin="3,0,0,0">   <!-- 3px = 原型 flex gap -->
        <ToggleButton ... />
    </WrapPanel>
</Grid>
```

1) tag 间距 `Margin="0,0,6,6"` → `"0,0,3,3"`（原型 `.filter-tags{gap:3px}`，四面板共 493 处）；
2) 发行人 / 担保人 = label + 全宽 `StcTextBox`，并把 ToolTip 占位升级为 `Tag` placeholder（原型有 `placeholder="搜索发行人..."`）；
3) 剩余期限（信用/利率）= label 跨右侧 StackPanel(tags + 全宽输入行)，原型 flex:1 列同构；
4) 既有 Grid 单行下拉（行业/地区等级/地区/省份/地方债专项）label 换 `StcFilterLabel`，列宽统一 70px；
5) 转换脚本：`.tmp/shots/_convert_filter_groups.py`（正则按行块转换 + 缩进重排，模式 A/B/C/D 见脚本头注释）；
6) 静态校验：4 面板 XAML 解析 0 失败、资源键 100% 命中、`StcTextFaint Margin=0,0,0,4` 旧堆叠写法全仓 0 残留。

### 7. 追加（第二轮截图框选）：顶部关注债券/筛选分组 + 区间网格排布

用户截图框出两处不一致：

1) **顶部「关注债券 / 筛选分组」**：原型是**两个独立 .filter-group**（label 70px 列 + 右侧 tags）上下两行，
   「筛选分组」行右侧有已保存分组 chip「我的关注」（`.filter-tag`，数据驱动）+「+」（`.filter-tag-add`）；
   WPF 原实现挤成一行且缺「我的关注」chip。已改为两行结构，「我的关注」静态占位（ToolTip 待接，
   分组管理弹窗 A4/B5 接入后由集合驱动）。

2) **报价筛选区间网格（10 组）**：原型 `.filter-grid{grid-template-columns:1fr 1fr}` 是**两列、行优先**，
   DOM 顺序 = Bid / Ofr / Bid-中债 / 中债-Ofr / **Bid-Ofr(.filter-page-unavailable 置灰)** / 中债估值 / 中证估值 /
   票面利率 / **Vol.Bid / Vol.Ofr（只有"最小"框）**；行间距 4px、列间距 7px。
   WPF 原实现是"两个 StackPanel 按列堆"→ 阅读顺序变成 Bid/Ofr/Bid-中债…|中债估值/中证估值…，与原型逐行对照错位。
   已改 `UniformGrid Columns="2"` 行优先，并补上：
   - Bid-Ofr 整行置灰：信用/利率面板 `IsDisabled="True"`（提醒合并页 filters-merged 原型未置灰，保持可用）；
   - Vol.Bid / Vol.Ofr 只有最小框：`ShowMax="False"`（此前一直没接上）。

脚本取证：`.tmp/shots/_grid_order.py`（从 index.html 抽取 4 个 filter-grid 块的 label 顺序 + unavailable 标记 + max 有无 + 单位，
4 块顺序完全一致，仅 merged 块的 Bid-Ofr 不带置灰）。静态校验通过。

### 8. 追加（第三轮截图框选）：分区分割线 + 单行下拉列错位 + 「+」按钮无响应

1) **分区分割线**：原型 `.filter-section{padding-bottom:8px;border-bottom:1px solid #21262d;margin-bottom:12px}`
   —— 每个分区底部有 1px 分割线。WPF 没有。已加主题样式 `StcFilterSectionDivider`（1px BorderSubtleBrush，
   Margin 0,4,0,12），插入在每个分区标题之前（Credit 6 / Rate 5 / Alert 6 / Intent 4 处）。

2) **单行下拉（地方债专项/行业/地区等级/地区/省份）**：又一个 `Grid.Column` 缺失 Bug（与计价方式同族）——
   这些行的 ComboBox 从未写 `Grid.Column="1"`，全部挤进 col0 与标签叠放；col0 是 Auto 宽，
   ComboBox 只显示自然宽度（窄「不限」悬在标签下方）。已批量补 `Grid.Column="1"`（12 处：
   Credit 4 / Rate 3 / Alert 3 / Intent 2），combo 现在占满右侧内容列，与原型「label + 全宽 select」一致。

3) **「+」按钮无响应**：项目约定是「未实现接口用 `ToastService.Show("xxx 待接…")` 占位，不留裸按钮」，
   之前这三个按钮只有 ToolTip。已接 Click → Toast（三个面板同款）：
   - 关注债券「+」→ `关注债券管理弹窗待接（原型 showFavBondModal）`
   - 筛选分组「+」→ `筛选分组管理弹窗待接（原型 showFilterGroupModal）`
   - 「我的关注」chip → `应用筛选分组待接（依赖分组管理弹窗…）`

### 9. 静态校验（本机无 MSBuild/VS）

通过 `.tmp/shots/_xaml_check.py` / `_check_keys.py` 验证：
- 7 个相关 XAML 解析全 0 失败；
- 6 个面板/窗口文件的 `{DynamicResource X}` / `{StaticResource X}` 引用 100% 命中主题（`Controls.xaml` + `Brushes.xaml` + `DarkTheme.xaml` 共 132 keys）或本地 `Window.Resources`（`AlertPopupWindow.xaml` 自有 3 keys：`AlertBtnBase` / `AlertItemTemplate` / `AlertGroupTemplate`）；
- Grid.Column / Row / RowSpan / ColumnSpan 全为整数（0 个意外值）；
- 无新增事件处理器，所有 `Click="OnGroup..."` 仍在 code-behind 中定义。

**待开发环境**编译 + 重跑 `tools/Capture.ps1` 重建 `tools/captures/*.png` 基线 + `PixelCheck.ps1` 比对。

---

## 关注债券管理 + 筛选分组管理弹窗（第七轮，2026-09-11）

两个弹窗在原型里都是运行时懒创建（导出 HTML 无 DOM），按 `app.js showFavBondModal/_fbmRender` /
`showFilterGroupModal/_fgmRender` 模板 + `app.css` .fbm-*/.fgm-* 规则还原（取证脚本
`.tmp/shots/_extract_modals.py` / `_extract_render.py`）。

### 新增文件

| 文件 | 内容 |
|---|---|
| `Shared/ViewModel/FilterProfileModels.cs` | FavBondGroup/FavBondEntry、FilterNamedGroup/FilterSubgroup/FilterCondition、目录模型（CatalogSection/ConditionGroup/Item） |
| `Shared/Services/FilterProfileService.cs` | 会话内存储（四组 ObservableCollection，原型 favBondGroupsCredit/Rate、filterGroupsCredit/Rate 字段）+ 种子数据（我的关注）+ 条件目录静态数据（取值与筛选面板一致，不可用项置灰；原型是从侧栏 DOM 抽取，WPF 直接静态化） |
| `Modules/MarketCenter/ViewModel/FavBondManagerViewModel.cs` / `FilterGroupManagerViewModel.cs` | 工作副本深拷贝 → 保存整体替换（取消不落库）；分组增删/行内重命名、勾选↔分组、条件 toggle（identity = key|value 去重）、自定义文本/下拉/区间添加、文本识别（MVP：与目录项 label 精确匹配） |
| `Modules/MarketCenter/View/FavBondManagerWindow.xaml(.cs)` / `FilterGroupManagerWindow.xaml(.cs)` | 遮罩弹窗（OverlayBackdrop 铺满宿主 + 点空白关闭），标题「关注债券管理」/「信用·利率筛选分组管理」 |

### 结构要点（与原型逐条对应）

1) `.fbm-layout{height:480px}`：左列 260px（上半分组列表 + 「+ 新建」虚线按钮；下半选中分组内容）+ 右侧工作区；
2) 分组列表项 = 名称(数量) + hover 删除 ✕ + **双击行内重命名**（maxlength 10，Enter 确认/Esc 取消）+ active 蓝描边；
3) fbm 下半 = 选中分组的债券列表（代码 简称，hover ✕ 移除）；fgm 下半 = **子分组卡片**（.fgm-subgroup：
   条件 chip 蓝描边可单独删除、卡片右上 ✕ 删除（非空需确认）、点击激活蓝描边）+ 底部「+ 新增分组」虚线按钮；
4) fbm 右侧 = 粘贴代码 textarea（150px，清空/识别）+ 结果表（勾选 | 剩余期限 | 债券代码 | 债券简称 | 债券库 |
   中债估值；**勾选即加入/移出当前选中分组**；Found=false 行 0.4 透明）；
5) fgm 右侧 = 粘贴条件文本 textarea + 条件目录（分区标题蓝色小字 + 分组 label + .fbm-check 多选 chip +
   文本/下拉/区间「添加」行；不可用组/项置灰带 reason）；顶部「识别结果」chips 区（有匹配才显示）；
6) 页脚 取消 / 保存（蓝底）；保存 Toast「会话内生效，后端同步待接」。

### 待接 / 待确认

1) **债券信息回填**（名称/期限/债券库/中债估值）：原型走债券查询接口，插件待接——当前解析出的代码按
   Found=false 展示（可勾选入组）；
2) **筛选条件文本语义解析**：MVP 仅与目录项 label 精确匹配（原型 bondFilterCapabilities 全能力）；
3) **后端同步**：保存为会话内存储；侧栏「我的关注」chip 及分组应用到看板查询，均待分组查询契约；
4) 提醒合并页（filters-merged）的分组/关注数据在原型无独立字段，暂按信用口径（待业务确认）。

---

## 第七轮弹窗实机复核 + 逐条返工（第八轮，2026-09-11）

用户反馈两个管理弹窗「没做好」。本轮把 `app.js` 的全部行为函数逐个提取核对
（`_fbmIdentify` / `_fgmMatch` / `_fgmCondListHtml` / `_fbmAutoAdd` / `_fbmToggleAll` / `_fbmSave` /
`_fgmSave` / `_fgmToggleCatalog` / `_fgmAddCatalogValue/Range` / `_fbmConfirm` / `showModal` 的 wide+fill 语义），
对照后修复以下差距（证据：`.tmp/shots/_modal_proto.txt` / `_modal_render.txt` + 本轮新增提取）。

### P0 行为不一致（语义错误，已修）

1. **fgm 文本识别不自动入组**：原型 `_fgmMatch` 把命中条件**直接 push 进当前激活子分组**（identity 去重）
   并 toast「已识别 N 个条件 / 未匹配到条件」；WPF 原来只把 token 摆在「识别结果」区等手点。已改为
   自动入组 + 同款 toast；未选分组时 toast「请先选择左侧分组」（原型同文案）。
2. **fbm 切换分组后勾选态不刷新**：原型切分组走 `_fbmSyncCheckState`；WPF 只在识别时设初值。
   已在 `SelectedGroup` 变化与左下移除债券后回填结果表勾选框。
3. **删除缺二次确认**：原型 fbm/fgm 删**非空分组**都弹 `_fbmConfirm`（360px 深色对话框、确认键红底
   #ef4444、点遮罩取消）；WPF 原来两处直接删、fgm 删子分组还是系统白 MessageBox。已新增
   `Shared/Components/ConfirmDialog`（复刻 _fbmConfirm），三处（fbm 删分组 / fgm 删分组 / fgm 删子分组）
   统一接入，文案照原型。
4. **fbm 识别回填**：原型 `_fbmIdentify` 走 `POST /parse` 回填 名称/期限/估值/命中态。本轮加领域接缝
   `IMarketCenterService.LookupBondAsync(code)`（Mock 在进程内行情集合中按 全代码/去后缀/尾部短码 查找），
   面板 code-behind 经 `MarketCenterViewModel.MarketCenterService` 传入弹窗。命中行回填并正常显示；
   未命中维持 Found=false 半透明。**真实 /parse 仍待接**（只换服务实现即可）。
5. **市场校验**（原型 `_fbmAutoAdd`）：命中的债券市场与当前弹窗（信用/利率）不符时拒绝入组并
   toast「该债券不属于当前信用债/利率债分组」。

### P1 结构 / 观感（已修）

6. **fgm 子分组卡片重做**：原型是独立卡片（bg #0d1117 / 边框 #21262d / 圆角 6 / margin 6·8；active =
   蓝描边 rgba(59,130,246,.5) + 微蓝底；hover 未选中 = 边框 #30363d），WPF 原来复用分组列表项样式。
   空卡片补提示文案「暂无条件，点击右侧目录或识别文本添加」；右上 ✕ 改 hover 卡片才显示
   （top/right -7 悬浮角标）。
7. **条件 chip 只显示 value**（title 才是 label:value 全称，原型 `marketGroupConditionValueDisplay` 语义）。
8. **fbm 结果表**：补表头**全选 checkbox**（`_fbmToggleAll`：取消全选 = 从分组移除所有结果行代码）；
   已加入行**绿底** rgba(63,185,80,0.06)；无结果时 **12 行 32px 空骨架**占位。
9. **空态文案**：fbm 左下「暂无关注债券 / 请先选择分组」、fgm 左下「请先选择分组」。
10. **头部**：两窗补 `.modal-hdr` 的 `border-bottom #30363d` 与 ✕ hover 态。
11. **重命名长度**：原型新建 maxlength=10、重命名 maxlength=**30**（WPF 原来都 10）。
12. **目录「添加」按钮**：原型 `tb-btn-primary` **红底 #ef4444**（hover #dc2626）；WPF 原来灰底。
13. **自定义条件行整行排列**（原型 `.fbm-custom-condition{width:100%}`）：文本输入 flex:1 撑满、
    下拉组只有下拉+添加（原型 select 组不渲染候选 chips）、区间 min/max 等分撑满；
    下拉组首项补「请选择」占位。

### P2 细节（已修）

14. 结果表列宽按比例撑满右栏（原型 table width:100%；SizeChanged 分配）。
15. 清空/识别、取消/保存按钮改自适应 padding（原型 4px/5px ×14px）。
16. range 单位折算按原型（M=1/12、D=1/365 → 年）；空/非法范围 toast 文案照原型。
17. 删空子分组自动回落一张空卡（原型 normalize 语义 `[[]]`）；选中分组子分组为空也补空卡。
18. 左下债券行补 `border-bottom #21262d` 分隔线与 ✕ hover 红态。
19. **修复分组名后条件计数不刷新**：`FilterNamedGroup` 订阅 Subgroups/Conditions 集合变化通知
    `ConditionCount`（WPF 原来是纯计算属性，加删条件后计数不动——原型每次重渲染天然刷新）。
20. `FavBondEntry` 实现 INPC（异步回填与勾选态要驱动行半透明/绿底样式）；模型补 `Market` 字段。

### 本轮仍待接

- `/parse` 真实识别接口（当前为 Mock 本地债券库查找，接口到位后仅替换 `LookupBondAsync` 实现）；
- `_smartMatchConditions` 全能力语义解析（MVP 仍为目录 label 精确匹配）；
- 后端同步落库、侧栏「我的关注」chip 数据驱动、分组应用到看板查询；
- 提醒合并页口径（沿用信用，待业务确认）。

### 第八轮追加（2026-09-12）：结果表重做 + 离屏渲染自查

用户实机反馈「表格很丑」。根因：上一版结果表用 `ListView+GridView` 只改了颜色属性没换模板，
保留了 WPF 默认皮肤四件套——默认表头模板（列分隔装饰）、**系统蓝色选中高亮**、**白色原生 CheckBox**、
行间无分隔线。已按 `.fbm-table` CSS 自绘重做：

1) 表头 = `Border`（bg #161b22 + 底部 1px #30363d）置于滚动区外（= 原型 `position:sticky`），
   th 内距 8,6 / 11px 600 #7d8590；复选框列 36px 居中；
2) 表体 = `ItemsControl`：行 `Border`（底部 1px #21262d / MinHeight 29）+ 与表头同一套固定列定义
   （36/80/120/`*`/100/100，简称列撑满 = 原型 `table{width:100%}`）；
   触发器顺序 Found 0.4 透明 → IsChecked 绿底 → **hover #21262d 最后**（CSS 里 `tr:hover td` 背景盖过
   行内绿底，同语义）；文本列 12px #c9d1d9，代码列 #79c0ff；
3) 复选框全部换主题 `StcCheckBox`（含表头全选）；
4) 删除了上一版「SizeChanged 按比例分配列宽」的代码补偿。

**新验证手段（离屏渲染）**：`.tmp/shots/_preview_modals.ps1` 复用 `tools/Capture.ps1` 装配方式
（AssemblyResolve + 反射私有构造器 + RenderTargetBitmap），不弹真窗出 PNG，配合视觉模型逐项核对。
本次用它发现并修复 **第七轮遗留 Bug**：`FilterGroupManagerWindow` 的 `SubgroupList`
**从首次提交起就没绑 `ItemsSource`**（code-behind 也没有），子分组卡片区一直是空的。
已补 `ItemsSource="{Binding SelectedGroup.Subgroups}"` + `IsSynchronizedWithCurrentItem="True"`
（切分组自动选中第一张卡），并在 `OnSubgroupSelectionChanged` 里忽略 ItemsSource 重建时的 -1。

自查结果（fbm-preview.png / fgm-preview.png）：表头分隔线 ✓、全码/短码命中行正常、
第 3 行绿底（勾选）✓、第 4 行未命中半透明 ✓、复选框深色样式 ✓；子分组卡片蓝色选中描边 +
城投/AAA/T+0/1-3Y 四个标签（各带 ✕）✓、「+ 新增分组」虚线按钮 ✓（卡片右上 ✕ 为 hover 显示，
离屏无悬停不显示，符合设计）。MSBuild 编译 0 错误。

---

## 第五轮整体对齐落地（2026-09-12，整-A/B/C/E/F → 已修，整-D 产品确认不做）

用户实机反馈：① 全部页「方向/报价」颜色不对；② 按文档把其余差异项一并处理。

### 方向/报价颜色（用户指出的）

原型证据：`.board-direction-value{color:#c9d1d9;font-weight:600}`（方向 = 普通文字色）；
`renderQuotePriceSpan` 内联 `color: #f59e0b(bid)/#3b82f6(ofr); font-weight:700`（报价 = 按方向着色粗体）。
WPF 原实现把 `DirBrush` 转换器用在**方向**列、**报价**列用 TextPrimaryBrush（白）——正好装反。
已互换：方向列 `TextSecondaryBrush`；报价列 Foreground 绑 Direction×DirBrush + FontWeight Bold。

### 整-A/B（对齐 + 列宽）

三看板列宽全部对齐 `BOARD_COLUMN_DEFS`（全部合计 1798→2204）；对齐规则 = 定义里 `align:'left'`
的列左对齐（全部页 6 列 / 最优 7 列 / 提醒 8 列），其余表头+单元格居中（数值列不再右对齐）。
新增主题 key：`StcColumnHeaderLeft` / `StcTextCellCenter` / `StcTrimTextCenter` /
`StcNumberCellCenter` / `StcNumberCellLeft`——只供看板引用；`StcNumberCell` 本身未动
（报价确认/管理等 7 个视图共用，其原型对齐口径未核对，不连带改）。
最优/提醒页 Bid/Ofr 模板列内部 StackPanel/TextBlock 的 Right 对齐改 Left（原型 align:'left'）。

### 整-C（行高 + 斑马纹）

RowHeight 32→34；补斑马纹 `table.dt tr.odd td{background:#1c2129}`（第五轮没记到的一项）——
`StcRow` 触发器 `AlternationIndex=1 → #1C2129`（置于 hover/selected 之前），看板 DataGrid 加
`AlternationCount="2"`。斑马纹只出现在 `boardRowHtml`（看板表），报价确认/管理等表原型无 odd 类，
它们不设 AlternationCount、触发器恒不命中，天然隔离。

### 整-E/F（导航样式）

激活下划线与文字 #79C0FF + 字重 600；子页 15px / Padding 14,0；一级导航 Padding 18,0、
基础字重 500、激活 600（原型 `.nav-btn` / `.subtab` 全套）。

### 整-D：不做（产品确认）

原型「需求管理」页 = 内嵌的《业务需求规格说明书》文档阅读器（SRS），是产品交付物不是客户端功能；
顶栏不补该导航项。

### 踩坑（新）

派生样式（`BasedOn="{StaticResource X}"`）插在被引用样式定义**之前** → 运行期
XamlParseException「无法找到名为 X 的资源」——XML 良构校验和 MSBuild 编译都发现不了，
只有真渲染才炸。Controls.xaml 里加派生样式必须排在基样式之后（本轮 Capture.ps1 首跑抓到，已修）。

### 验证

`tools/Capture.ps1` 重建 11 张基线（整-B/C 改版后必须重建，PixelCheck 才有效）；
视觉自查：信用全部页（方向普通色 / 报价橙蓝粗体 / 前 6 列左其余居中 / 斑马纹 / 数值列居中）、
信用最优页（Bid/Ofr 价格左排、Vol 列左对齐）、导航激活 #79C0FF——全部通过。

---

## fgm 条件目录全量重对齐（2026-09-12，用户实机反馈「债券筛选项不对」）

第七轮的静态目录是"按印象写的"，与原型 DOM（availableMarketFilterGroupData 的数据源）有系统性偏差。
本轮直接解析原型 `index.html` 的 `#filters-credit` / `#filters-rate`（提取脚本 `.tmp/shots/_extract_catalog2.py`，
产物 `_catalog_proto2.txt`），逐分区/分组/条目重对齐 `FilterProfileService.GetCatalog`：

| 差异 | 原型 | 修复 |
|---|---|---|
| 目录含「报价筛选」整区 | `MARKET_FILTER_GROUP_SECTIONS` 只放行 债券/流动性/信用/条款/区域 | 移除 |
| key 不一致 | data-bond-filter-key：type/tag/rt/liq/mkt/city/ind/ent/gur/cir/csir/opt/repay/interest_pay/perp/sub/rate… | 全部改用原型 key（原 liquidity/market_place/chengtou/interest/subordinate/rate_mode 等为臆造） |
| 置灰大面积缺失 | 单项置灰（filter-control-unsupported）：热门标签 5 项、上市状态/发行方式/担保/含权/永续各若干；整组置灰（filter-group-unsupported）：NCD类型、银行业、主体评级、中债资信、评级展望、债项评级、YY评分、发行年份、城投层级、地区等级、省份 | `TagSpec` 的 "!" 前缀按 DOM 原位标记单项置灰；`DisabledGroup`/`DisabledSelect` 整组置灰 |
| 剩余期限只有区间 | 11 档标签（<3M…>20Y）+ 自定义区间 + Y/M/D 单位 | Tags + `HasRange` + `HasUnitSelect` 混排；目录组渲染改为标签/文本/下拉/区间可叠加 |
| 缺组缺项 | 信用筛选缺「行业」下拉；细分券种缺「其他」；企业性质缺「其他」；付息方式缺「其他」；永续债取值应为 城投永续/产业永续/金融永续/非永续；城投层级缺「其他」；中债/中证隐含缺「更多」 | 全部补齐按 DOM 顺序 |
| 区域筛选 | 城投层级为置灰组；「地区」= region 级联下拉（3807 项）；「省份」本身是置灰组 | 城投层级/省份置灰；地区取 [省级] 34 项（市/区县级联待接） |
| 上市状态/发行方式顺序 | 上市→非上市(灰)；公募→小公募(灰)→私募 | 顺序与置灰对齐 |
| 种子条件 key | — | 我的关注种子改用目录 key（tag/ent/rt/mkt），勾选态可正确回显 |

离屏渲染 + 视觉自查通过：无报价筛选区、热门标签 5 项置灰、剩余期限标签+区间混排（Y/M/D+红添加钮）、
NCD 类型整组置灰。`CatalogConditionGroup` 补 `HasRange` 属性；fgm 目录组渲染从互斥分支改为可叠加。

---

## 关注分组闭环（批次 A，2026-09-14）

「零后端依赖」清单的第一批：把已实现的管理弹窗接入侧栏与右键菜单，分组功能端到端可用。
原型语义来源：`renderMarketGroupTags`(6810) / `toggleMarketGroup`(952) / `rowMatchesActiveMarketGroups`(936) /
`addToContextFavorite`(8890) / `showNewGroupModal`+`confirmNewGroup`(8899-8925)。

### 落地内容

| 项 | 实现 |
|---|---|
| A1 右键「添加到分组」动态子菜单 | `BoardRowGroupMenu.Populate`（三看板 `ContextMenuOpening` 时装配）：列当前市场关注分组（点击入组、去重静默）、分隔线、「+ 新建分组」；「默认」空组不列 |
| A2 新建分组弹窗 | 新增 `Shared/Components/PromptDialog`（360px 深色输入框，复刻 showNewGroupModal：取消/Esc=取消，Enter/确定=确认；重名 toast「同名分组已存在」）；建组即把当前行债券放入 |
| A3 侧栏 chip 数据驱动 | `MarketCenterViewModel` 暴露 Credit/Rate/Merged 三组 × 关注/筛选 两类 chip 集合（`MarketGroupChip`：title=「名称 · N只债券/N个条件」）；三个 FilterPanel 改 ItemsControl + `StcFilterTagToggle`（激活=金底 rgba(210,153,34,.2)+金字 #D29922，同 `.filter-tag.active`）；管理弹窗保存/右键建组 → `FilterProfileService.DataChanged` → chip 重建 |
| A4 chip 点击过滤看板 | `MarketGroupSelectionService`（激活集 + 原型 AND/OR 语义：关注组类内任一命中 + 筛选组类内任一命中；激活组在另一市场时该项跳过；分组被删自动修剪激活 id）；BoardList/BoardBest 的 `QueryAsync` 结果上做行过滤，激活变化触发重查；状态栏「报价总数」改用过滤后行数。**提醒看板 BoardAlertRow 无市场字段，暂不过滤（待字段补齐）** |

筛选分组条件求值：`MarketGroupRowMatcher`（key 分桶，key 内 OR/key 间 AND）——**客户端近似**：
可求值 key = rt（剩余期限档 `<3M..>20Y` / 自定义区间 min-max，M/D 折算年）+ mkt（代码后缀→银行间/上交所/深交所/北交所）；
其余 key（tag/ent/type/city…）看板行无对应属性，**忽略（视为通过）**。后端 `bond-filter/query` 契约到位后整体替换为服务端命中集比对（原型 marketGroupMatchIds 方案）。

模型配套：`FavBondGroup`/`FilterNamedGroup` 加稳定 `Id`（激活集定位；管理弹窗工作副本同名沿用旧 id）。

### 验证（MSBuild 0 错误 + 离屏渲染）

- 状态断言：信用 15 行 → 激活「我的关注」筛选 chip（城投/国企/1-3Y/银行间 → 生效条件 1-3Y+银行间）→ **4 行** → 取消 → 15 行恢复；
- 像素断言：激活后 chip 区域出现金底（默认态 0 金色像素）；可视化树断言：y=99 关注债券 chip 未选中灰底、y=132 筛选分组 chip 选中金底（绑定未接反）；
- 右键子菜单装配断言：`我的关注 | + 新建分组`；
- 提醒合并面板 chip：fav 1 + filter 1（信用+利率合并）。
- 截图：`.tmp/shots/chips-credit-default.png` / `chips-credit-filtered.png` / `chips-alert-merged.png`。

## 附：本次核对用到的原型证据位置

| 内容 | 位置 |
|---|---|
| 子页标签改名 | 新原型 `index.html` 子页条 `data-st="board-intent"` |
| 最优浮层 / 笔数按钮样式 | 新原型 `app.css` `.best-quote-count` / `.best-quote-layer*` |
| 有新报价横幅 | 新原型 `index.html` `#boardNewQuoteBanner`（含内联样式） |
| 子分组 UI 样式 | 新原型 `app.css` `.fgm-subgroup*` / `.fgm-condition-chip` / `.fgm-add-subgroup` / `.fbm-custom-condition` |
| 弹窗尺寸 | 新原型 `app.css` `.quote-entry-shell` |
| 列头定义（权威） | 新原型 `app.js` `BOARD_COLUMN_DEFS` |
| 表格对齐规则（权威） | 新原型 `app.js` `applyBoardTableLayout`(412-421 写 th) / `boardRowHtml`(1373 写 td) + `app.css` `table.dt td,th{text-align:center}` |
| 一行区间输入的规格 | 新原型 `app.css` `.filter-grid` / `.filter-grid-item` / `.filter-label` / `.filter-input` / `.filter-sep` / `.filter-unit`（`index.html:1244-1253` 为 10 组实例） |
| 「筛选」按钮规格 | 新原型 `app.css` `.filter-btn-primary`（灰底），实例见 `index.html:1243`（计价方式行）、`index.html:736`（剩余期限行） |
| 顶栏导航项与角色控制 | 新原型 `index.html` 顶栏 `.nav-btn`（含「需求管理」） |
| 筛选面板结构（权威） | 新原型 `index.html` `#filters-credit` / `#filters-rate` / `#filters-intent` / `#filters-merged` / `#filters-merged-mgmt` |
| 规格层描述（可作验收依据） | 新原型 `requirement-spec-page.js`（3.2.x / 4.x / 5.x 条款） |
| 编辑报价弹窗 DOM（权威） | 新原型 `index.html` 中 `#modalOverlay > .modal.modal-wide`（导出已固化） |
| 提醒弹窗 DOM（权威） | 新原型 `index.html` 尾部 `#alert-对价提醒`（`.alert-popup.alert-match-popup`） |
| 弹窗尺寸/表单栅格样式 | 新原型 `app.css` `.modal` / `.modal-wide` / `.quote-fields` / `.quote-dir-btn` / `.quote-bond-summary` |
| 无 DOM 快照组件的模板 | 新原型 `app.js`：`showFavBondModal` / `showFilterGroupModal` / `ensureBestQuoteLayer` / `ensureQuotePriceTooltip` / `showBondInfoPopup` |
| 提醒弹窗多卡片观感 | 新原型 `app.css` `.alert-popup-wrap{gap:10px}` + `.alert-popup{border-radius:8px; box-shadow 0 8px 24px rgba(0,0,0,.6)}` |
| 左侧筛选栏标签列宽（70px / 13px / #6e7681） | 新原型 `app.css` `.filter-label{font-size:13px;color:#6e7681;min-width:70px}` |
| 左侧筛选栏区间（43×22 / 12px / 右对齐 / #0d1117） | 新原型 `app.css` `.filter-input{width:43px;padding:2px 4px;font-size:12px;text-align:right;background:#0d1117}` |
| 导出完整性审计脚本 | `.tmp/prd/_new0910/_diff/audit_dom.py`（id 三方比对） |
