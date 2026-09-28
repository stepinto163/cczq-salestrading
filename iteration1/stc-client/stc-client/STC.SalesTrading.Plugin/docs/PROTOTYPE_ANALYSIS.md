# FICC 销售交易平台 HTML 原型 → WPF 移植结构化分析报告

> 分析对象：`D:\Work\Coding\cgws-stc-client\.tmp\prd\ficc-client`（单页 Electron 原型，index.html 1171 行、app.css 671 行、app.js 8808 行、backend-spec-data.js 70 个接口、data-model-spec-data.js 53 张表）
> 移植目标：C#/.NET Framework 4.x + CommunityToolkit.Mvvm，单体模块化插件 `STC.SalesTrading.Plugin`，本期范围 = 报价录入、行情中心、报价管理。
> 说明：原型为**暗色主题、硬编码颜色（无 CSS 变量）**，数值/颜色/尺寸均按源码逐一摘录，可直接转成 WPF ResourceDictionary。

---

## 1. 应用整体框架

### 1.1 窗口与外壳（main.js / index.html）

| 项 | 值 |
|---|---|
| 窗口 | 1440×900，最小 1200×700，标题「FICC销售智能交易平台」，背景 `#080a10` |
| 标题栏 | Windows 隐藏标题栏（`titleBarStyle:hidden` + `titleBarOverlay`），overlay 颜色 `#161b22`、符号色 `#e6edf3`、高 48px；菜单栏隐藏 |
| 总体布局 | 顶部 Header（高 48px）＋ 工作区 Content（flex 纵向，占满剩余高度） |
| 页面切换 | 无路由库：`switchPage(page, el)` 切换 `.page` 显隐（`style.display='flex'`），顶部 `.nav-btn` 高亮；子页用 `switchSubTab(el)` 切换 `.subtab`/`.subpage`；记住当前子页（localStorage `ficc_activeSubTabs`） |
| 权限 | 导航按钮 `data-page-roles` 白名单：`accounts/version/requirement-spec→admin`，`requirements→admin/technician`；`canAccessPage()` 校验，无权点击 toast「当前账号无权访问该页面」 |
| 登录 | 独立登录层 `.login-overlay`（星空、光环日食、粒子动画背景），登录成功 `enterApplication(user)` 隐藏登录层、显示 `.app`、默认进入行情中心 |

### 1.2 顶部 Header

- 左：品牌 `FICC销交平台`（蓝色折线 SVG logo）；
- 中：导航按钮 `报价录入(quote) / 行情中心(board) / 报价管理(mgmt)`（本期范围）＋ 管理员专属 `账户管理/版本管理/需求管理/需求规格`；活动态：文字 `#79c0ff` + 底部 2px 蓝边；**角标** `.nav-btn .badge`（红底白字圆角 8px，字号 9px）：quote 角标=待确认报价数，board 角标=未读提醒数；
- 右：用户区（头像 28px 渐变圆 + 姓名 + 箭头），点击展开下拉（个人中心 / 设置 / 退出登录）。

### 1.3 全局状态（app.js 顶层，WPF 建议做成 AppSession/单例 VM）

| 状态 | 说明 |
|---|---|
| `currentUser` | 登录用户：`{username, name, role(admin/technician/trader), roleName, accountId, dept, orgId, email, phone, mustChangePassword}`；`normalizeSessionUser` 补齐缺省 |
| `authToken` | Bearer 会话令牌；`authFetch()` 统一注入头，401 时 toast「登录状态已失效」并登出 |
| `authSessionGeneration` | 会话代数：登出/切换账号时自增，所有异步回调用它丢弃过期响应（竞态防护） |
| `D` | 主数据仓库（等价于各模块 VM 的 ObservableCollection）：`quoteConfirm/quoteConfirmBatches/quoteConfirmStats`、`preciseData/fuzzyData`、`liveQuotes/quoteEvents/quoteStateLoaded`、`boardCreditAll/boardCreditBest/boardRateAll/boardRateBest/boardAlert/boardIntent`、`mgmtToday/mgmtTodayAll/mgmtHistory`、`favBonds/favBondGroups*/filterGroups*`、`marketGroupRevision/marketGroupLoaded/marketGroupReadOnly`、`alertSettings{favorite,filterGroup,priceMatch,bestQuote}` |
| 选区 | `quoteSelected`(Set of recognitionId)、`preciseSelected/fuzzySelected`(index Set)、`boardSelectedQuoteIds`、`intentSelectedIds`、`mgmtTodaySelected/mgmtHistorySelected` |
| 页面控制器 | `FICCPageControllers.register/get`（modules/page-controllers.js），插件式页面注册点 |
| 报价规范 | `FICCQuoteCanonical.normalizeQuoteRow`（modules/quote-canonical.js）：字段别名归一（`bp/op/bv`、`c/n`、`inst/tr`、`pt/cs`），方向判定规则 `directionOf`（显式 d 优先，否则 op 无 bp→ofr，否则 bid） |
| 日期时区 | 全部使用 `Asia/Shanghai` 时区格式化（`FICCQuoteUtils.shanghaiDate/shNow`）；`quoteToday()/quoteNow()` |

### 1.4 主题与样式系统（app.css，全部硬编码色值）

**字体（vendor/fonts.css）**
- 正文：`Inter`（400/500/600/700/800）；数字/代码/价格：`JetBrains Mono`（400/500/600）。WPF 建议打包 woff2 转 ttf 或选相近系统字体（Segoe UI + Cascadia Code/Consolas）。
- 字号：base 15px；表格 td 14px；表头 th 13px；toolbar 按钮 14px；标签/徽标 9–12px；明细区 10–11px。

**色板（按语义归类）**

| 语义 | 色值 |
|---|---|
| 窗口/最暗背景 | `#080a10` |
| 页面背景 | `#0d1117`；board 主区 `#010409` |
| 面板/Header/表头背景 | `#161b22`；hover 背景 `#1c2129`/`#21262d`/`#252d38` |
| 选中行背景 | `#26384a`（hover `#2d4359`），important 级别 |
| 边框 | `#21262d`（细分隔）、`#30363d`（标准）、`#484f58`（强） |
| 主文字 | `#e6edf3`；次文字 `#c9d1d9`；弱化 `#8b949e`；更弱 `#7d8590`；最弱 `#484f58`；标签灰 `#6e7681` |
| 品牌蓝/活动 | `#79c0ff`（活动导航/子页/代码/焦点边）、`#58a6ff`、`#3b82f6`(Ofr)、`#1f6feb`、`#2563eb`(按钮) |
| Bid 琥珀 | `#f59e0b`；`#d29922`（价格字/活动筛选 tag/tag-firm） |
| 涨/急/错误红 | `#ef4444`（主按钮/急）、`#dc2626`、`#f85149`（错误、必填缺失）、`#f87171` |
| 跌/成功绿 | `#3fb950`（成功、TRD）、`#2ea043`、`#22d3ee`（**成交量青色**） |
| 紫 | `#a78bfa`(打包)、`#a855f7`、`#9333ea`(信唐)、`#a371f7`(TKN 成交) |
| 流动性 S/A/B/C/D | S 红 `#ef4444` / A 橙 `#f59e0b` / B 蓝 `#3b82f6` / C 紫 `#a855f7` / D 灰 `#6b7280`（均 15% 透明度底+30% 边框） |
| 机构徽标色 | 国利`#1e40af` 国际`#ef4444` 平安`#f97316` 中诚`#22c55e` 上田`#06b6d4` 信唐`#9333ea` CFETS`#a16207` |

**标签/徽章配色模式**（WPF 可做成 `Tag` 控件 + 依赖属性）：一律「12% 透明度底色 + 同色文字 + 30% 透明度 1px 边框」，圆角 3px，字号 10px。
- `.tag-bid` 橙 / `.tag-ofr` 蓝 / `.tag-firm` 金 `#d29922` / `.tag-nonfirm` 蓝 `#79c0ff` / `.tag-success` 绿 / `.tag-warning` 金；
- 报价旗标 `.quote-flag-*`：急`#ef4444`、荐`#3fb950`、整`#ec4899`(粉)、OCO`#79c0ff`、打包`#a78bfa`。

**圆角**：按钮 4px；输入 6px；卡片/弹窗/菜单 8px；小标签/徽章 3px；头像/气泡 50%/12px。
**间距**：页面左右 padding 20px；toolbar padding 8px 20px；表头/单元格 padding 7px 10px；区块间距 8–12px。
**滚动条**：5px/7px 细条，thumb `#30363d` hover `#484f58`，track 透明。
**动效**：hover 过渡 120–150ms；toast 从右滑入 `si .3s`；alert 弹窗右滑 `alertIn .25s`；报价保存后行高亮 `quote-row-reveal 2.4s`（前 35% 蓝底 20% 透明度渐隐）；表格行无动画。
**图标**：内联 SVG 线性图标（stroke 1.5），尺寸 14/16/20px（icon-sm/icon/icon-lg）——WPF 可换成 Segoe Fluent/PathGeometry 资源。

---

## 2. 三大模块页面清单

### 2.1 报价录入（page-quote）

入口：顶部导航「报价录入」；也可由行情中心子页工具栏「报价录入」按钮以**弹窗（1200×720，`quote-entry-overlay`）**形式打开同一页面（`openQuoteEntryModal` 把 `#page-quote` 移动到弹窗挂载点）。
子页（subtabs）：`报价确认 / 精确报价 / 模糊报价`。

#### 2.1.1 报价确认（st-quote-confirm）
- **布局**：工具栏（可换行）→ 分组报价表格 → 底部状态栏。
- **工具栏控件**：
  - `确认报价`（主按钮，**红色 `#ef4444`**，全选勾选行确认入库）；`删除`
  - 方向批量：`Bid`（橙底）、`Ofr`（蓝底）——给选中行批量设方向
  - BP 微调：`+`（绿）`-`（红）＋ 输入框（70px，右对齐，`BP` 后缀），键盘 `+`/`-`/`Enter` 可增减（`adjustBP`）；批量价格类型下拉（到期收益率/行权收益率/净价/全价，自定义 `client-select`）、清算速度下拉（T+0/T+1）
  - 报量微调：`+1k`（绿）`-1k`（红）＋ 输入框（`万` 后缀，JetBrains Mono）
  - `报价机构`、`报价交易员` 输入框：聚焦弹出 CRM 联想（机构→交易员两级联动，当前为 MOCK 目录 provider，接口层预留）
  - 旗标按钮：`急` `荐` `整` `OCO` `打包`（点击给选中行批量打标，可再点取消）
  - 右侧 `已选 N 条`
- **表格（quote-confirm-table，min-width 1970px，固定列宽见 CSS col 定义）**：按「机构+来源批次」分组（`batchId||inst|src`），组头行可点击折叠/展开（`.group-header`），组头含来源徽标（`IDEAL` 蓝 / `QT` 绿）、机构、交易员、到报时间、条数、批次状态（待确认/部分识别/识别失败/部分确认）。列：`☑ | 期限 | 代码 | 简称 | 方向 | 报价 | 价格类型 | 报量(万) | 操作(标签) | 报价机构 | 报价交易员 | 交易日期 | 清算速度 | 中债估值 | 中债净价 | 中债偏离 | 中证(行/到) | 文本内容`。
  - 可编辑单元格统一 `field-underline`（无边框输入/下拉，仅下划线，focus 变蓝）：方向/价格类型/清算速度为自定义下拉；报价/报量/机构/交易员/日期为输入框；**价格与报量待补时黄字 `#d29922` 显示「待补」**；必填缺失（债券代码或简称、方向）单元格红色下划线 + 图标。
  - 切换价格类型时按类型缓存各自报价（`_priceValuesByType`，切回保留）。
- **状态栏**：`共 N 组 / M 条报价 | IDEAL: x组 | QT: y组 | Ofr: n | Bid: n | 同步状态`（绿「实时同步正常」/ 红「连接中断，自动重试」）。
- **数据流**：`GET /quote-confirm/batches?status=pending` 拉批次；`GET /quote-confirm/changes?since=` 每 2.5s 轮询增量（end_of_day 事件触发清空+刷新）；新到行**自动勾选**（手工取消过的记住不勾）；确认 `POST /quote-confirm/confirm`（含版本号乐观锁、服务端校验错误回填单元格）；删除 `POST /quote-confirm/delete`。确认成功后 `loadQuoteState({notify,highlightQuoteIds,reveal})` 在行情中心高亮新增行。
- **Enter 键**：子页有文本时按 Enter = 触发精确/模糊识别；无文本但有结果 = 确认。

#### 2.1.2 精确报价（st-quote-precise）
- **布局**：顶部可拖拽调整高度（100–500px）的多行文本输入区（`precise-input-textarea`，底部 6px 拖拽手柄）→ 识别工具行 → 批量工具栏（同 2.1.1 但作用域为精确结果）→ 结果表格 → 状态栏。
- **输入工具行**：`粘贴`（读系统剪贴板，需桌面 API）、`截图识别`（桌面截屏→Tesseract OCR，Tesseract.js 内置中文模型）、`导入图片`、`导入表格`（xlsx/csv，SheetJS）、`清空`、`示例`、`识别后保留文本` 勾选；右侧主按钮 `识别报价`（蓝色）。
- **识别**：`POST /api/parse {text, lookup:true}` → 结构化结果（方向/价格/价格类型/量/机构/交易员/日期/清算速度 + 置信度/完整度/警告/字段缺失/债券候选 + 中债估值记录 `_apiInfo`）；随后 `POST /api/bond-valuation {codes}` 补估值 → 计算中债偏离（收益率口径 `(quote-cbYield)*100`，净价/全价口径价差，行权/到期基准确认）。
- **结果表格（precise-result-table，min-width 1840px，18 列，同 2.1.1 表但第 1 列是「类型」=新增/更新/撤销徽标）**：撤销类（CANCEL）行相关列只读并显示撤销范围；全部单元格可编辑（同上）；支持多选、全选。
- **状态栏**：`精确识别结果 | 共 N 条 | Ofr: n | Bid: n`；识别过程顶部浮动状态条（正在识别…/识别完成·成功 x 条·失败 y 条·耗时 zs）。
- 确认：`executeStableConfirm`（按 `quoteLogicalKey` 去重：账户+来源+代码+方向+机构+交易员+价格类型+日期+清算速度）→ `POST /quotes/state` 落库。

#### 2.1.3 模糊报价（st-quote-fuzzy）
- **布局**：文本输入区（同上，固定不可拖拽）→ 识别工具行（同精确，含批量机构/交易员输入、`删除`、`确认报价`）→ **左右两栏**（左 6 : 右 4，中间 1px 分隔线）→ 状态栏。
- **左栏表格（quote-fuzzy-table）**：`☑ | 报价内容(等宽) | 识别结果(结构化条件标签) | 报价机构(可编辑) | 报价交易员(可编辑)`；行点击切换活动行（蓝左边框 3px + 12% 蓝底高亮）；**点击左侧行 → 右侧联动查询该行识别条件对应的债券列表**。
- **识别**：`POST /api/intent-recognition/parse {items:[{clientItemId,text,institution,trader}]}`，返回每行条件集合（label/value/key/normalizedValue/queryKey/queryValue/queryEnabled/region/range）；条件展示顺序按左侧筛选面板 DOM 顺序（`INTENT_DISPLAY_FIELD_ORDER`）。
- **右栏表格**：`剩余期限 | 债券代码 | 债券简称 | 债券库 | 中债估值`，数据源 `POST /api/bond-filter/query {filters, include_records:true, record_limit:500}`，**每行 50 条分页**（底部「共 N 条 / 第 x/y 页 / 上一页 下一页」），按条件 JSON 缓存，条件变更即失效；AbortController 取消过期请求。
- 确认：`fuzzyConfirmToIntent` 把勾选行转为**意向报价**（`POST /api/board-intents/bulk`），并从列表移除。

### 2.2 行情中心（page-board）

**整体布局**：左「筛选侧栏 384px」（可折叠为 40px，含垂直滑槽按钮）＋ 中「主区」（子页标签条 + 工具栏 + 表格 + 状态栏）＋ 右「债券详情 384px」（可折叠，详情含多档行情/成交/智能推荐）。

**子页标签（subtabs-board）**：`信用全部 / 信用最优 / 利率全部 / 利率最优 / 意向报价 / 报价提醒(角标) / [刷新按钮] / [报价录入按钮]`。
切换子页联动左侧筛选面板：credit* → filters-credit；rate* → filters-rate；intent → filters-intent；alert → filters-merged。

#### 2.2.1 筛选侧栏（行情中心的公共控件）
- **顶部 sticky 区**：`清除过滤` 按钮 + `搜索债券、机构、交易员、发行人` 输入框。
- **关注债券 / 筛选分组**：`+` 添加标签；点击标签 → 按「关注组」/「筛选组」过滤；管理弹窗：
  - 关注债券弹窗（fbm）：左侧分组列表（可新建/删除/重命名、拖拽排序）+ 右侧粘贴债券代码批量识别（`/api/bond-lookup`）+ 命中表（代码/简称/债券库/中债估值）+ 添加；
  - 筛选分组弹窗（fgm）：左侧分组列表 + 右侧粘贴条件文本（`/api/intent-recognition/parse` 复用）自动识别条件、勾选后保存（`PUT /api/market-groups`）。
- **报价筛选（quote-filter-section）**：清算速度（T+0/T+1/远期/非远期）、计价方式（收益率/净价 二选一 + 筛选按钮）、数值区间网格（两列）：
  - Bid / Ofr / 中债估值 / 中证估值 / 票面利率：`价格3位整数+4位小数`（data-rule=price3_4），单位 `%`；
  - Bid-中债 / 中债-Ofr / Bid-Ofr：`6位整数+2位小数`（price6_2，允许负数），单位 `BP`；
  - Vol.Bid / Vol.Ofr：price6_2，单位 `万`。
- **债券筛选**：细分券种（信用：短融/中票/企业债/公司债/金融债/PPN/ABS/可转债/NCD/其他；利率：国债/国开/农发/口行/地方债）、热门标签（20 个，如商金债/银行二级/银行永续/城投/科创债/TLAC债…）、NCD 类型、是否免税、地方债类型/专项/地区、剩余期限（11 档 <3M…＞20Y + 自定义 min-max 带 Y/M/D 单位）、发行人搜索、上市状态、发行方式。
- **流动性筛选**：债券流动性 S/A/B/C/D、做市标的（上证信用/深证信用、上证利率/深证利率）、上市场所（银行间/上交所/深交所/北交所）。
- **信用筛选**：银行业、行业下拉、企业性质、担保、担保人、主体评级/中债资信/债项评级/中债隐含/中证隐含（评级 tag + `更多`）、评级展望、YY评分。
- **条款筛选**：含权属性（不含权/含权+赎回权/回售权/其他权 联动选中）、偿还方式、付息方式、永续债、次级债、利率方式（SHIBOR/DEPO/LPR/固息）、发行年份。
- **区域筛选**：城投/非城投、12重点省、城投层级、地区等级（待确认）、地区（全国省/市/区县三级联动下拉，数据 `GET /api/reference-data/regions`，县区级暂禁用）、省份下拉。
- **交互规则**：同组 tag 多选（组内 OR）；「含权」选中自动带子项、取消联动；输入框输入即过滤数字（规则见上）；筛选变化 → `POST /api/bond-filter/query {filters}` 拿 security_ids → 应用到当前视图（跨服务，失败降级为本地过滤）；Bid-Ofr 区间仅在最优/提醒页可用（`syncBidOfrAvailability` 禁用其他页）。侧栏 filter-group **可拖拽排序**（localStorage `filterOrder`）。
- 数据源状态：`bondFilterRuntime`（bondIds/total/loading/error/requestId/controller/timer），`BOND_FILTER_CACHE_TTL=5min`；能力配置 `GET /api/bond-filter/config`（支持字段白名单，不支持字段置灰 `filter-control-unsupported`）。

#### 2.2.2 信用全部 / 利率全部（st-board-credit-all / rate-all）
- **工具栏**：`Copy`（复制选中报价文本，走 `/api/bond-copy-enrich` 富化，格式「代码 方向 价格 量 + 期限(休N)」）、`Refer`、`导出`（xlsx）；`Bid`/`Ofr` 方向开关 tag（active-bid 橙 / active-ofr 蓝）；复选：`我的报价`、`紧急`、`推荐`、`整量`；`未报价债券`（勾选后切换数据源为 `POST /api/bond-filter/unquoted` 分页列表，每页 200）。
- **表格（min-width 2200px，21 列）**：`剩余期限 | 代码 | 简称 | 方向 | 报价 | 报量(万) | 报价机构 | 报价交易员 | 长城交易员 | 最新成交价 | 债券库 | 主体评级 | 债券评级 | 中债净价 | 中债偏离 | 中债YTM(%) | 中证净价 | 中证全价 | 中证YTM(%) | 报价时间`。
- **状态栏**：`报价总数: n | Bid: n(橙) | Ofr: n(蓝)`。
- 列定义驱动渲染（`BOARD_COLUMN_DEFS['board-credit-all']`）：前 3 列（剩余期限/代码/简称）**sticky 冻结**，其余列**可拖拽换位**（`ficc_board_column_order_v1` 持久化，表头出现拖拽指示线/半透明）；hover 报价可显示换算 tooltip。

#### 2.2.3 信用最优 / 利率最优（best）
- **工具栏**：同上但无「未报价债券」。
- **表格（min-width 2800px，22 列，按债券聚合双向最优）**：`剩余期限 | 代码 | 简称 | 报价机构.Bid | 长城交易员.Bid | Vol.Bid | Bid(橙底列) | Ofr(蓝底列) | Vol.Ofr | 长城交易员.ofr | 报价机构.Ofr | 最新成交价 | 债券库 | 主体评级 | 债券评级 | 中债净价 | 中债偏离 | 中债YTM(%) | 中证净价 | 中证全价 | 中证YTM(%) | 报价时间`。
- **聚合算法**（`buildBestRows`）：按 `代码|价格类型|清算速度`（quotePairKey）分组；**收益率口径 Bid 越小越优、Ofr 越大越优；净价/全价口径 Bid 越大越优、Ofr 越小越优**（`quotePriceIsBetter`）；记录来源 quoteId。
- **状态栏**：`报价总数 | Bid | Ofr | 对价: n(金 #d29922)`。

#### 2.2.4 意向报价（st-board-intent）
- **工具栏**：`更新报价`、`Copy`、`Refer`、`Unrefer`、`导出`；日期区间 `开始日期 ~ 结束日期`（默认当日，`onIntentDateChange` 刷新）。
- 表格（board-intent-table，1160px，7 列）：`报价内容(可点击行内编辑 textarea，蓝框) | 识别结果 | 报价机构 | 报价交易员 | 长城交易员 | 报价时间 | 状态(有效/撤销)`；未同步行前有红色脉冲圆点 `board-intent-pending`。
- 数据：`GET /api/board-intents?start_date=&end_date=`；状态变更 `POST /api/board-intents/status {items:[{id, expectedDataVersion}], action: refer|unrefer}`（带数据版本乐观锁）；统计条 `GET /api/board-intents/stats`。
- 左侧筛选：filters-intent 面板（细分券种/热门标签/评级/区域…每个 tag 带 `data-field-key/data-key`，**组内 OR、组间 AND**；「仅显示未完整识别」开关；搜索框实时过滤）；本地结构化匹配 `parseIntentRowConditions`（recognitionJson 优先，字符串回退）。

#### 2.2.5 报价提醒（st-board-alert）
- **工具栏**：四个开关 `关注债券 / 筛选分组 / 对价提醒 / 我的最优报价`（`setAlertSetting` 持久化 `D.alertSettings`）。
- **表格（min-width 2800px，23 列）**：`提醒类型 | 剩余期限 | 代码 | 简称 | 报价机构.Bid | 长城交易员.Bid | Vol.Bid | Bid | Ofr | Vol.Ofr | 销售.Ofr | 报价机构.Ofr | 最新成交价 | …(同最优列)`。提醒类型：关注债券(绿)/筛选分组(蓝)/对价提醒(金)/我的最优(紫)。
- **提醒规则**（`refreshAlertsForQuote`）：
  - 关注债券/筛选分组：报价命中收藏组或筛选组；
  - 我的最优：本人报价在最优页且存在同券同方向其他报价（仅一条不触发）；
  - 对价提醒（`appendPriceMatchAlerts`）：同券同价类型同清算速度、他人 Ofr ≤ 本人 Bid 时触发（也可被他人触发），合并展示双方价格；
  - 非对价类多类型提醒合并为一行（`mergeNonPriceMatchAlerts`，类型用 `/` 连接）。
- **已读机制**：未读角标（导航+标签）、`GET /api/alerts/read-state` + `POST /api/alerts/read-state/bulk` 批量上报、本地缓存 `ficc_alert_read_pending_<accountId>`；新提醒右下角弹出 `alert-popup`（类型徽标 + 明细，可「查看/关闭」）。
- 状态栏同最优页（报价总数/Bid/Ofr/对价）。

#### 2.2.6 债券详情右栏（board-detail，本期行情中心重点）
- **债券核心头部**（bond-header）：`最新成交`（大号 20px 加粗，**涨红跌绿**）+ 时间；`昨/高/开/低` 四宫格（高红低绿）；`久期 / 中债 YTM/净价`。
- **机构筛选标签栏**（inst-filter-bar）：全部/国利/国际/平安/中诚/上田/信唐/CFETS（每机构品牌色，`filterInst` 过滤多档行情行）。
- **最优报价汇总条**（best-quote-bar）：`最优 Bid [量 价] | 价差 BP(绿) | [价 量] 最优 Ofr`，点击高亮对应多档行情行。
- **多档行情**（inst-quote-table）：`来源 | Vol.bid | Bid | Ofr | Vol.ofr | 来源`（Bid 橙字/Ofr 蓝字，最优行 8% 底色），来源为机构徽标。
- **成交行情**（market-trade-table）：`方向 | 代码 | 简称 | 成交价 | 成交量 | 时间`，方向 `TRD`(绿)/`TKN`(紫)/`GVN`(金)；当前为 mock（`_mockCapabilitiesCache.market_tape_demo` 控制），接口待接入。
- **智能推荐面板**（rec-panel）：
  - 债券搜索框（代码/简称联想 `GET /api/recommendations/bond-suggestions?q=&limit=10`）+ `说明`按钮（推荐规则弹窗 `GET /api/recommendations/rules`）；
  - `Bid/Ofr` 分段切换（计数角标）→ `POST /api/recommendations/quote-leads {security_id, task(bid/ofr), trigger_quote_id, window, result_scope, page, page_size}`；
  - 范围标签 `全部 / 本券 / 相似券 / 相似机构`；时间窗 `近7日/近30日/近90日` + 自定义起止日期；
  - 结果表（推荐度 / 机构交易员 / 代码简称 / 报价次数 / 最近报价报量 / 长城交易员），行可勾选，`发送报价`（`POST /api/recommendations/quote-send`，可编辑报价文本后发送，带幂等键），`Like/Dislike` 反馈（`POST /api/recommendations/{id}/feedback`）；
  - 分页（每页按钮 + 省略号）、推荐度命中/未命中文本高亮（绿/红波浪线）、证据 tooltip（`GET /api/recommendations/{id}/evidence`）。

### 2.3 报价管理（page-mgmt）

**布局**：左「筛选侧栏（复用 filters-merged-mgmt，内容同行情中心 merged 面板，含报价筛选/债券/流动性/信用/条款/区域）」＋ 右主区（子页 + 工具栏 + 表格 + 状态栏）。

**子页**：`今日报价 / 历史报价`。

#### 2.3.1 今日报价（st-mgmt-today）
- **工具栏**：`Refer`、`Unrefer`、`删除`、`Copy`、`导出`；三个**联想搜索框**（代码/名称、报价机构、长城交易员——输入弹下拉建议 `mgmt-search-suggestions`，含主文本代码蓝字+副文本简称灰字，↑↓+Enter 选择）；下拉：`报价方式`（全部/QT/ideal/手工录入）、`报价状态`（全部/有效/撤销）。
- **表格（min-width 2300px，23 列）**：`报价方式 | 报价状态 | 剩余期限 | 代码 | 简称 | 方向 | 报价(含内联旗标) | 报量(万) | 操作(旗标) | 报价机构 | 报价交易员 | 长城交易员 | 最新成交价 | 债券库 | 主体评级 | 债券评级 | 中债净价 | 中债偏离 | 中债YTM(%) | 中证净价 | 中证全价 | 中证YTM(%) | 报价时间`。
  - 报价方式标签：QT=绿 firm、ideal=蓝 nonfirm、手工录入=橙 tag-bid；报价状态：有效灰白/撤销灰 `#7d8590`；
  - 行选择：整行点击切换选中（浅蓝底）；多选后工具栏批量操作。
- **数据**：`GET /api/quotes/state?view=today&scope=all`；行由 `buildMgmtTodayRow` 从 live quote 投影；`mode/status/market` 过滤；联想建议从当前数据提取（`mgmtSuggestionItems`）。
- **状态栏**：`共 N 条 | Bid: n | Ofr: n` 等（`updateMgmtTodayStatusbar`）。

#### 2.3.2 历史报价（st-mgmt-history）
- **工具栏**：无 Refer（历史只做 Unrefer/删除/Copy/导出）；搜索框同今日；外加 `日期 开始 ~ 至 结束`（默认上一工作日，周日回退到上周五；相互约束 min/max）。
- **表格**：同今日但**无「报价状态」列**（其余 22 列）。
- 数据：`GET /api/quotes/state?view=today` 之外的历史由 `syncMgmtHistory` 从 `D.mgmtHistory`（原型内置 mock 历史）与本地过滤驱动——WPF 版按 `/api/quotes/state` 扩展日期参数或新接口 `?view=history&start_date=&end_date=` 规划。

### 2.4 公共弹窗（三大模块共用）
- **报价表单弹窗（新报价/编辑报价）**：债券摘要条（代码/简称 + 市场最优 Bid/Ofr + 中债估值）+ 方向分段按钮（Bid 橙/Ofr 蓝）+ 12 栅格字段（价格/报量带步进 ▲▼、价格类型、报量单位、交易日期、清算速度、报价机构、报价交易员、备注 + 急/荐/整勾选）+ 底部 kbd 提示（`Enter` 保存 / `Esc` 取消）；保存 `saveEditQuote` → 校验 → `POST /quotes/state`（新）或 `POST /quotes/state`（改，记录 quote log）；`报价日志`弹窗（时间/用户/动作/方向/旧价/新价/量/备注，动作徽标 报价绿/修改金/撤销红）。
- **撤销报价弹窗**：`POST /api/quotes/revoke/resolve`（按指令解析候选）→ 弹窗选择 → `POST /api/quotes/revoke/confirm`（带 instruction+quoteId）→ `POST /api/quotes/revoke`；恢复 `POST /api/quotes/restore`（幂等 request_id）。
- **右键菜单（ctxMenu）**：`添加到分组(子菜单)` / `新报价` / `编辑报价` / `Refer` / `打包` / `OCO` / `报价日志`；按行数据动态启用/禁用、旗标项随行状态切换。
- **债券资料弹窗**（bondInfoPopup）：hover/点击代码弹出（发行规模/剩余规模/到期日期/票面利率/主体评级），`POST /api/bond/info`。
- **修改密码弹窗**：`POST /api/auth/change-password`（old/new，登录后 mustChangePassword 强制）。

---

## 3. 通用 / 可复用 UI 组件清单（WPF UserControl/资源建议）

| 组件 | CSS 类 | 视觉特征 | 接口/行为 |
|---|---|---|---|
| 数据表格 | `table.dt` / `board-table` | 表头 sticky（`#161b22`、13px、灰字、`th.s` 可点击）、斑马纹 `tr.odd`(`#1c2129`)、hover `#252d38`、选中 `#26384a`；行高约 32px；空数据补 25 行占位（`fillEmptyRows`） | 渲染由列定义驱动 `{id,label,width,sticky,align,type(code/price/muted/neg/status),render,bg,headerClass}`；支持冻结列（sticky + left 偏移 + 右阴影）、列拖拽换位、行点击/拖选/Shift 多选 |
| 工具栏 | `.toolbar` | 背景 `#161b22`、底边 1px、padding 8px 20px、gap 8px、可换行 | 放按钮/输入/下拉/勾选/分隔（`.tb-sep` 1px）/弹性间距（`.tb-spacer`） |
| 按钮 | `.tb-btn`(+`-primary`红/`-default`灰/`-success`绿/`-bid`蓝) | 默认 14px、圆角 4px、hover 变亮、active 下压 1px；primary=红底白字；default=`#21262d` 底 `#30363d` 边高 26px | 支持图标+文字 |
| 输入框 | `.tb-input` / `.filter-input` / `.quote-control` | 底 `#0d1117`、边 `#30363d`、圆角 6px、focus 蓝边 `#79c0ff`；`.input-with-suffix`（BP/万 后缀） | 数值输入过滤 `[^0-9.]`；步进 `+/-` |
| 下拉 | `.tb-select` / `.filter-select` / `client-select`(自定义) | 自定义下拉：触发条 + 弹出菜单（`#161b22` 底、阴影、max-height 220、选项 hover `#26384a`）；方向下拉 Bid 橙字/Ofr 蓝字 | 键盘 ↑↓/Enter/Esc；点击外部关闭 |
| 多选标签 | `.filter-tag` | 灰底圆角 3px 13px；active 金底 `#d29922`；`+` 添加 `filter-tag-add` | 同组多选；「全部」反选逻辑；含权子项联动；可禁用（能力缺失置灰） |
| 报价旗标 | `.quote-flag-*` / `.tag-interactive` | 12% 底色+30% 边框，10-11px；off 态透明度 0.3，hover 0.6 | 急/荐/整/OCO/打包；OCO 与打包互斥（`toggleExclusiveQuoteFlag`） |
| 状态徽标 | `.tag-*` / `.inst-badge` / `.liquidity-*` / `.alert-type-*` | 见 1.4 色板；机构徽标实底白字 | 报价方式/状态/评级/流动性/提醒类型/来源机构 |
| 价格着色 | `.bid-col`/`.ofr-col`（列级）、`.board-quote-price`、`.price`/`.neg`/`.muted` | Bid 橙 `#f59e0b`、Ofr 蓝 `#3b82f6`、量青 `#22d3ee`、价格金 `#d29922`（JetBrains Mono 加粗）、偏离红（abs>16 才红）、弱化灰 | hover 弹换算 tooltip（收益率↔净/全价） |
| 价格换算 tooltip | `.quote-price-tooltip` | 固定 164px，深底+阴影，两行（收益率%、净/全价） | `POST /api/quotes/price-conversion`，150ms 延迟显示，结果缓存 |
| 闪烁动画 | `@keyframes quote-row-reveal` / `.board-intent-pending` | 保存后行 2.4s 蓝底渐隐；未同步红点脉冲 | 确认/保存后高亮定位 |
| 弹窗 | `.modal-overlay/.modal` | 遮罩 60% 黑 + 600px 弹窗（`modal-wide` 880px），圆角 8px，头部/体(60vh 滚动)/底部按钮区 | `showModal(title, bodyHtml, {wide, footerHtml, initialFocus})`；Esc 关闭；确认/输入两类 |
| 报价录入弹窗 | `.quote-entry-overlay/.quote-entry-shell` | 1200×720 或 92vw×84vh，装载整个报价录入页 | 从行情中心打开/关闭，焦点还原 |
| Toast | `.toast(-success/-error/-info)` | 右上角堆叠，右侧滑入 0.3s，3s 后渐隐，12% 底色+同色边框文字 | `showToast(type,msg)`；类型 success/error/info |
| 提醒弹窗 | `.alert-popup` | 右下角堆叠 420px，类型徽标+条目列表+查看/关闭按钮 | 新提醒到达动画 `alertIn .25s` |
| 右键菜单 | `.ctx-menu/.ctx-item/.ctx-submenu` | 180px 深底菜单，hover 高亮，含分隔线、danger 项、禁用项 | 定位到鼠标；子菜单「添加到分组」动态渲染 |
| 联想下拉 | `.crm-suggest-panel` / `.mgmt-search-suggestions` / `.rec-bond-suggestions` | 输入框下方浮层（深底+阴影），选项 hover `#26384a`，主文本等宽蓝字+副文本灰 | 机构/交易员/债券代码/历史值联想；↑↓/Enter/Esc；120ms 延迟关闭 |
| 内联编辑 | `.field-underline` / `.inline-edit` | 无边框仅下划线，focus 蓝线；必填缺失红线下划线+单元格红色内阴影 | 表格内直接编辑；失焦提交；版本冲突提示 |
| 智能推荐 | `.rec-*` | 详情栏内：分段 Bid/Ofr、范围 tab、时间窗按钮、10px 紧凑表、分页按钮组、反馈 Like/Dislike | 独立状态机 `recommendationState`（side/scope/window/page/selected/feedback），AbortController 防抖 |
| 分组管理 | `.fbm-*` / `.fgm-*` | 左分组列表+右粘贴识别；480px 高度布局；组项 hover 显示删除 | 关注债券组/筛选组；新建≤10 字符；拖拽排序；Enter 确认/Esc 取消 |
| 空状态 | `.empty-state` / 占位行 | 48px 灰描边图标+提示；表格空则 25 行 32px 占位行 | `fillEmptyRows(tbodyId)` 保布局稳定 |
| 状态栏 | `.statusbar` | 底部 11px 灰字，`|` 分隔，含加粗白数字 | 每视图一个：总数/Bid/Ofr/对价/同步状态 |
| 分段切换 | `.filter-toggle-btn` / `.rec-toggle-btn` / `.quote-dir-btn` | 两段式按钮组，active 橙/蓝/金底 | Bid/Ofr、收益率/净价、方向选择 |

---

## 4. 数据与接口层

### 4.1 服务拓扑（main.js）
- 主后端：FastAPI `127.0.0.1:8080`（`server.py`，SQLite `ficc.db`）；健康 `GET /api/health`，必须声明能力 `board-intents.status.v1 / quotes.canonical-state.v2 / alerts.read-state.v1 / quotes.sse-ticket.v1` 才兼容。
- Parser：Express `127.0.0.1:3000`（报价/意向文本识别）。
- Bond Filter：`127.0.0.1:8600`（SQLite `bond_pool.sqlite3` 物化池筛选）。
- API_BASE 解析：EXE 中央服务 `window.FICC_SERVER_URL` + `/api`；本机 `http://127.0.0.1:8080/api`；局域网同源 `/api`。→ WPF 对应一个 `ApiClient`（HttpClient，Bearer 注入，401 全局登出）。
- 认证：`POST /api/auth/login {username,password}` → `{token, user}`；`GET /api/auth/me`；`POST /api/auth/logout`；`POST /api/auth/change-password`。

### 4.2 接口清单（backend-spec-data.js 70 项 + app.js 补充，按模块）

**报价确认（6）**
| 方法 | 路径 | 入参要点 | 出参要点 |
|---|---|---|---|
| GET | `/api/quote-confirm/batches?status=pending` | status | `{batches:[{batchId,status,customerName,customerTraderName,ownerDisplayName,rawText,quotes:[{recognitionId,version,c,n,d,p,pt,vol,inst,tr,date,cs,oco,packing,urgent,rec,whole,dev,…}]}], stats{batchCount,quoteCount,qtBatchCount,idealBatchCount,bidCount,ofrCount,errorCount}, draftMergeEnabled, lastEventSequence}` |
| GET | `/api/quote-confirm/changes?since={seq}` | 事件序号 | `{events:[{type(quote_batch.*/end_of_day.completed…),…}], lastEventSequence}` |
| POST | `/api/quote-confirm/confirm` | `{items:[{recognitionId,version,securityId,securityName,direction,priceType,quoteValue,volumeWan,tradeDate,settlementType,counterpartyInstitutionName,counterpartyTraderName,oco,packing,urgent,recommend,round}]}` | `{confirmedRecognitionIds, deferredRecognitionIds(重复报价), quoteIds}` |
| POST | `/api/quote-confirm/delete` | `{recognitionIds, reason}` | `{success}` |
| PATCH | `/api/quote-confirm/quotes/{recognition_id}` | 字段修正 | `{success}` |
| GET | `/api/end-of-day/status` | — | 日终状态 |

**报价状态（8）**
| 方法 | 路径 | 入参 | 出参 |
|---|---|---|---|
| GET | `/api/quotes/state?view=board|today&scope=all` | view/scope | `{success, revision, quotes:[canonical quote], events}` |
| POST | `/api/quotes/state` | `{quote_id, action, source_type, market, status, data(整条 canonical quote), event}` | `{success, quoteId, quote(规范化回填), revision}`；按 quoteId 串行队列+重试（3s/10s） |
| POST | `/api/quotes/revoke/resolve` | `{instructions:[{instruction}]}` | 候选列表（弹窗选择） |
| POST | `/api/quotes/revoke/confirm` | `{selections:[{instruction, quoteId}]}` | `{success}` |
| POST | `/api/quotes/revoke` | `{quote_ids}` | `{success}` |
| POST | `/api/quotes/restore` | `{quote_ids, request_id}` | `{success}`（幂等） |
| POST | `/api/quotes/delete` | `{quote_ids}` | `{success}` |
| GET | `/api/quotes/events` | ticket/connection_id（SSE） | 事件流：`new_quote/edit_quote/revoke_quote`，payload 含 `{quote, stateRevision}` |

**识别/债券（9）**
| 方法 | 路径 | 入参 | 出参 |
|---|---|---|---|
| POST | `/api/parse` | `{text, lookup:true, mode:'bond'}` | `{ok, data:[{direction{value}, price{value,type}, price_type{value}, quantity{value}, bond_code{value,tags}, bond_name{value,tag}, institution{value}, trader{value}, trade_date{value}, settlement{value}, quote_action, cancel_scope, confidence, completeness, reason, status, warnings, missing_fields, recognized_fields, field_states, bond_candidates, _apiInfo{security_id,rating,bond_type_desc,remaining_tenor,…}, _valuation}]}` |
| POST | `/api/intent-recognition/parse` | `{items:[{clientItemId,text,institution,trader}]}` | `{ok, data:[{clientItemId, ok, result, inst, salesTr, recognition:{version,status,conditions:[{label,value,key,normalizedValue,queryKey,queryValue,queryEnabled,region,range}],unmatchedTokens,conflicts}}]}` |
| POST | `/api/bond-valuation` | `{codes:[…]}` | `{ok, valuations:{code:{cb_ytm,cb_yte,cb_nptm,cb_npte,cb_net_price,full_price,cb_mdtm,cb_mdte,cs_ytm,cs_yte,cs_nptm,cs_npte,cs_fptm,cs_fpte,cs_full_price,valuation_date,…}}}` |
| POST | `/api/bond-copy-enrich` | `{codes}` | `{success, data:{code:{remainingTenor,bondCategory,…}}}`（Copy 富化） |
| POST | `/api/bond-lookup` | `{codes}` | 代码/名称查询（关注债券批量识别） |
| POST | `/api/bond/info` | `{code, api_path?, params?}` | `{bondCode, success, source, data{…}, message}`（债券详情弹窗） |
| POST | `/api/quotes/price-conversion` | `{securityId, quoteValue, priceType(到期收益率/行权收益率/净价/全价), direction, tradeDate, settlementType}` | `{success, yieldType, yieldLabel, yieldValue, cleanPrice, fullPrice, calculationMode}` |
| POST | `/api/bond-search` | 关键词 | 债券搜索 |
| GET | `/api/bonds/option-map` | — | `{success, callableCodes:[…]}`（价格类型「行/到」标识） |

**筛选 / 市场组（10）**
| 方法 | 路径 | 入参 | 出参 |
|---|---|---|---|
| POST | `/api/bond-filter/query` | `{filters, include_records, record_limit, signal}` | `{success, security_ids}` 或 `{records:[{security_id,remaining_tenor,sec_short_name,bond_type_desc,cb_ytm,…}]}` |
| POST | `/api/bond-filter/unquoted` | `{market, page, pageSize}` | 未报价债券分页 |
| GET | `/api/bond-filter/config` | — | 支持字段/选项能力（禁用列表） |
| GET | `/api/bond-filter/health` | — | 健康 |
| POST | `/api/bond-filter` | `{filters}` | 兼容入口 |
| GET | `/api/bonds/option-map` | — | 见上 |
| GET | `/api/reference-data/regions` | — | 全国省/市/区县树（区县级 enabled:false） |
| GET | `/api/market-groups` | — | `{groups:[{id,kind(favorite|filter),market,name,items,…}]}` |
| PUT | `/api/market-groups` | `{groups}` | `{success, revision}` |
| GET | `/api/bond-search` | — | 见上 |

**意向报价（7）**：`GET/POST/PUT /api/board-intents`、`POST /api/board-intents/bulk`、`POST /api/board-intents/migrate`、`POST /api/board-intents/status {items:[{id,expectedDataVersion}], action}`、`GET /api/board-intents/stats`。

**推荐（8）**：`POST /api/recommendations/quote-leads`、`GET /api/securities/{id}/similar`、`GET /api/institutions/{id}/quote-profile`、`GET /api/institutions/{id}/similar`、`GET /api/recommendations/{id}/evidence`、`POST /api/recommendations/{id}/feedback {action,reason}`、`GET /api/recommendations/rules`、`GET /api/recommendations/health`。app.js 补充：`GET /api/recommendations/bond-suggestions?q=&limit=10`、`POST /api/recommendations/quote-send`（`{idempotency_key, recommendation_ids, quotes, message_text}`）与 `GET /api/recommendations/quote-send/{batchId}`（结果轮询）。

**提醒（app.js 补充，未在 spec）**：`GET /api/alerts/read-state`、`POST /api/alerts/read-state/bulk {alertKeys}`。

**认证/账户（6）**：login/logout/me/change-password、`GET/POST /api/accounts`、`PATCH /api/accounts/{id}`。

**系统/版本（7）**：`GET /api/health`、`GET /`、`GET /api/git/log?count=`、`POST /api/git/commit {message,body}`、`POST /api/git/backup`、`POST /api/git/revert {hash}`。

**Bond Filter 独立服务（8）**：`GET /api/bonds/config|mapping|pool`、`POST /api/bonds/pool/build|refresh|hot-pool|filter`、`GET /api/health`。

### 4.3 推送 / 轮询通道

| 通道 | 机制 | 用途 |
|---|---|---|
| 报价 SSE | `POST /api/quotes/events/ticket {connectionId}` 换 ticket → `EventSource /api/quotes/events?ticket=&connection_id=`；事件 `new_quote/edit_quote/revoke_quote` | 实时行情刷新；按 `stateRevision` 递增合并，缺口>1 触发全量 `loadQuoteState`；**断线 5s 轮询兜底后重连** |
| 待确认轮询 | `GET /api/quote-confirm/changes?since=` 每 2500ms | 批次增量（识别结果推送）、日终事件 |
| 状态全量 | `GET /api/quotes/state?view=board` | 登录、SSE 缺口、刷新按钮 |

→ WPF 建议：`SignalR`/`WebSocket`/`HttpClient SSE` 二选一，至少实现「全量快照 + 增量事件 + 定时兜底」等价语义。

### 4.4 数据模型清单（data-model-spec-data.js：2 库 / 53 表 / 668 字段）

**主库 ficc.db**（按组）：
- 账户权限：`accounts`、`auth_sessions`、`account_audit_events`
- 报价录入：`quote_confirm`（批次暂存+日终状态）、`quote_precise`、`quote_fuzzy`
- 正式报价：`live_quotes`（当前状态+JSON 载荷）、`quote_events`（变更事件+快照）
- 行情投影：`board_credit_all / board_credit_best / board_rate_all / board_rate_best / board_alert`（字段如 `source_quote_id,c,bp,op,inst_bid,bv_bid,inst_ofr`）
- 意向报价：`board_intent`（trader/content/result/inst/status）、`board_intent_audit`
- 市场分组：`market_groups`、`market_group_items`、`account_market_settings`
- 聊天集成：`inbound_chat_messages`、`inbound_quote_batches`、`pending_quotes`、`quote_inbound_links`、`integration_events`
- 日终：`end_of_day_runs`
- 推荐：`recommendation_request / recommendation / recommendation_evidence / recommendation_feedback / recommendation_audit_event / recommendation_rule_config`、`bond_similarity_snapshot`、`institution_master`、`institution_alias`、`institution_similarity_snapshot`、`quote_behavior_snapshot`

**Bond Filter 库 bond_pool.sqlite3**：`bonds`（宽表缓存）、`bond_master`、`company_master`、`dm_raw_snapshot`、`bond_filter_profile`、`bond_rating_current / bond_liquidity_current / bond_terms_current / bond_status_current`、`region_metric`、`filter_capability / filter_evidence / bond_code_alias`、`metadata / schema_migrations / update_runs / sync_checkpoint / refresh_queue`。

**关键字段命名（报价 canonical）**：`quoteId/sourceQuoteId/sourceType(status=active|revoked)/ownerAccountId/ownerUsername/ownerDisplayName/counterpartyInstitution(inst)/counterpartyTrader(trader)/direction/bp(报价Bid)/op(报价Ofr)/bv/bvBid/bvOfr/instBid/instOfr/trBid/trOfr/m(期限)/c(代码)/n(简称)/priceType/cs(清算速度)/tradeDate/market(credit|rate)/mode(QT|ideal|手工录入)/oco/packing/urgent/recommend/round/lastPrice/bondLib/r/br/nzNet/nzDev/nzYTM/csNet/csFull/csYTM/t(报价时间)/quotedAt/updatedAt/valuationStatus`。

**枚举（从代码提取）**：方向 Bid/Ofr；价格类型 到期收益率(YTM)/行权收益率(YTC)/净价(CLEAN_PRICE)/全价(DIRTY_PRICE)/利差(SPREAD)/面值(PAR)；清算速度 T+0/T+1/T+2/T+3/远期；报价方式 QT/ideal/手工录入；报价状态 active/revoked；批次状态 pending_review/partial_failed/recognition_failed/partially_confirmed；意向状态 有效/撤销；提醒类型 关注债券/筛选分组/对价提醒/我的最优；成交方向 TRD/TKN/GVN；角色 admin/technician/trader。

---

## 5. 交互细节清单（UI 还原重点）

**颜色方向（中国习惯，与 A 股相反）**
- 上涨/最高/买入价相关红：`#ef4444`；下跌/最低绿：`#3fb950`（债券详情头部「高/低」、最新成交）。
- **Bid = 琥珀橙 `#f59e0b`，Ofr = 蓝 `#3b82f6`**——全站统一（方向 tag、列底色、报价文字、状态栏计数、tooltip 值、Alert 条目、方向下拉）。Bid 列底 `rgba(245,158,11,0.2)`，Ofr 列底 `rgba(59,130,246,0.2)`。
- 成交量/报量一律青色 `#22d3ee` 加粗等宽。
- 成交方向：TRD 绿、TKN 紫 `#a371f7`、GVN 金 `#d29922`。

**价格精度与格式化**
- 收益率报价 4 位小数（`2.9150`）；估值 `toFixed(4)`；中债偏离 BP 2 位小数 + `bp` 后缀（净价口径为价格点）；偏离 |绝对值|>16 显示红色。
- 区间输入规则：`price3_4`=3 整数+4 小数；`price6_2`=6 整数+2 小数；Bid-中债/中债-Ofr/Bid-Ofr 允许负数；输入时实时过滤非法字符。
- 价格类型非默认口径加标识：净价「净」/全价「全」/非含权债标行权「行」/含权债标到期「到」（依据 `/bonds/option-map` 的 callable 集合）。
- 交易日期：`min=当日`，早于当日自动纠正并 toast 提示；默认当日（上海时区）。
- 报量单位「万」（交易所券为「万面额」）；BP 输入框 70px 右对齐。

**表格行为**
- 冻结前 3 列（剩余期限/代码/简称）；列拖拽换位持久化；hover 报价单元格显示「收益率↔净价/全价」换算 tooltip（150ms 延迟、Esc/滚动/离开关闭、结果缓存）。
- 行选择：单击切换；**鼠标拖拽跨行批量选择**；Shift/Ctrl 多选；选中行浅蓝底；多选后工具栏批量操作。
- 双击行 → 编辑报价弹窗；右键 → 上下文菜单（添加到分组/新报价/编辑报价/Refer/打包/OCO/报价日志，动态启用）。
- 报价保存后：自动定位到所在视图（清筛选/切 tab）、行 `scrollIntoView` + 2.4s 蓝色 reveal 动画。
- 空数据：表格补 25 行占位（32px/行）保持高度稳定，滚动条位置不动。

**键盘**
- 精确/模糊页：输入区非空时 `Enter`=识别；结果非空时 `Enter`=确认；`Shift+Enter` 换行。
- BP/报量输入：`+` 加、`-` 减、`Enter` 提交当前值。
- 全局 `Esc`：关闭通用弹窗 → 报价录入弹窗 → 行内编辑 → tooltip；`Enter` 提交 system modal（prompt/confirm），推荐发送编辑器内除外。
- 下拉/联想：`↑↓` 循环、`Enter` 选中、`Esc` 关闭。
- 报价表单：`Enter` 保存、`Esc` 取消（kbd 提示展示）。
- 意向报价编辑：`Esc` 结束编辑。

**拖拽**
- 精确输入区底部手柄纵向拉伸（100–500px，`ns-resize` 光标）。
- 表格列头拖拽换列（冻结列除外）。
- 筛选侧栏 filter-group 拖拽排序（持久化 `filterOrder`）。
- 关注债券/筛选分组列表内分组项拖拽排序（弹窗内）。

**数据刷新与一致性**
- 登录后并行加载：市场分组、筛选配置、option-map、地区字典、能力标记 → 报价状态、意向、待确认批次、提醒已读 → SSE 连接 + 2.5s 批次轮询。
- 行情视图由 `live_quotes` 统一投影（`rebuildBoardProjections`）：活跃报价按市场分类 → 范围筛选（bond filter + 本地 tag）→ 全部列表 / 最优聚合 / 提醒生成；任何报价事件触发全量重投影。
- 报价写入按 quoteId 串行队列（防乱序），失败 3s/10s 重试，`syncStatus` 标记。
- 会话/账号切换 `resetAccountState()`：清空全部缓存数据与选区、请求代数自增使在途请求失效。
- 只读降级：后端不可用仍可登录查看（认证前只读），写操作 toast「应急模式为只读」。

**本地存储（localStorage/desktop storage）键清单**：`ficc_remember_user`（仅用户名，密码禁止落盘）、`ficc_activeSubTabs`、`ficc_board_column_order_v1`、`filterOrder`、`ficc_favBondGroupsCredit/Rate`、`ficc_filterGroupsCredit/Rate`（旧版分组，登录后可迁移导入）、`ficc_marketGroupMigrationDismissed:<accountId>`、`ficc_alert_read_pending_<accountId>`（已读提醒离线缓冲，成功后清空）。→ WPF 对应 `%AppData%` 下 JSON 配置或 Settings。

**特殊业务细节**
- 报价去重：同代码同方向更新合并（`dedupAndUpdate`）；确认时按 `quoteLogicalKey`（账户|来源|代码|方向|机构|交易员|价格类型|日期|清算速度）判定重复，重复项返回 `deferredRecognitionIds` 保留在待确认页。
- 最优报价：收益率 Bid 低优/Ofr 高优；净价/全价相反；「我的最优」提醒需存在同券同方向他人报价。
- 对价提醒：他人 Ofr ≤ 本人 Bid（同券/同价类型/同清算速度）触发，可撮合成交提示（`⚠️ 当Bid报价≥Ofr报价时触发对价提醒`）。
- 意向报价 Refer 后仍保留记录并刷新报价时间；Unrefer 仅恢复撤销项；均带 `expectedDataVersion` 乐观锁。
- Copy 文本格式：`代码 方向 价格 报量` 等宽格式 + 富化字段（期限带 `(休1)/(休2)` 标注）。
- 导出：SheetJS 生成 xlsx → 桌面 API 保存对话框（`save-xlsx-file`，默认名「行情中心导出.xlsx」）。
- 登录记住账号：仅回填用户名；历史 `ficc_remember_pass` 键启动即清除（安全约束）。
- 页面主题无切换入口：固定暗色，无亮色主题。

---

## 附：WPF 拆目录建议（对应本节内容的落地映射）

```
STC.SalesTrading.Plugin/
  Themes/            # ResourceDictionary：Colors.xaml（1.4 色板）、Brushes.xaml、Styles.xaml（按钮/输入/表格/标签/弹窗/Toast）
  Controls/          # DataGrid 扩展（冻结列/列拖拽/斑马纹/占位行）、Tag 控件、ClientSelect、SuggestPopup、QuoteFlag、PriceTooltip、StatusBar
  Modules/
    QuoteEntry/      # 报价录入：Confirm/Precise/Fuzzy 三个 UserControl + ViewModel + ParserApi
    MarketCenter/    # 行情中心：侧栏筛选（FilterPanel）、BoardTable（列定义驱动）、DetailPanel（多档/成交/推荐）
    QuoteMgmt/       # 报价管理：Today/History + 搜索联想
  Services/          # ApiClient（Bearer/401）、QuoteSseClient（或轮询）、QuoteStore（live_quotes 投影）、AlertService、GroupService、Clipboard/Export
  Models/            # QuoteCanonical（字段归一）、BondInfo、Valuation、IntentRecognition、Recommendation、MarketGroup、Alert
  Common/            # Converters（涨跌/方向/状态→Brush、精度格式化）、Behaviors（拖拽列、拖选行）、Messenger 事件（新报价/提醒角标）
```
