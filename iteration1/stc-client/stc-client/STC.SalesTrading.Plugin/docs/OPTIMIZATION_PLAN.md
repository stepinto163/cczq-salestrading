# 销交平台前端优化实施计划

> 更新日期：2026-08-24
> 范围：后端接口与字段尚未确定阶段，优先完成前端可独立验证的架构、状态、交互与业务规则。
> 状态：TODO 未开始，DOING 进行中，DONE 已完成，BLOCKED 等待外部条件。

## 1. 目标与边界

1. ViewModel 只依赖领域服务，不直接引用 ApiManager、HTTP 或 Topic 字符串。
2. 领域服务通过 IGatewayClient 使用 STC 网关；将来可替换为其他通信实现。
3. 行情中心和报价管理以服务端筛选、分页和排序为主，不以 ICollectionView.Filter 作为正式数据路径。
4. 后端契约未确定前不猜测传输 DTO，只定义稳定的 UI 查询语义和统一传输结果。
5. Mock 必须覆盖异步、取消、空结果、失败和状态变化，不能只是静态页面样例。

## 2. 目标架构

    View / ViewModel
        -> IQuoteService / IMarketCenterService / IQuoteMgmtService
            -> IGatewayClient
                -> ApiManagerGatewayClient
                    -> ApiManager / IApi

- IGatewayClient：同步/异步请求、Topic 与连接状态订阅、统一错误、订阅生命周期。
- 领域服务：业务查询、字段映射、Topic 规则、快照与增量合并。
- ViewModel：筛选状态、加载状态、选择状态和 UI 命令，不感知传输协议。

## 3. 实施台账

### P0 基础接缝与可靠性

| 状态 | 项目 | 验收标准 |
|---|---|---|
| DONE | Gateway 抽象 | IGatewayClient 支持同步/异步请求、Topic/连接订阅和 IDisposable 退订 |
| DONE | ApiManager Adapter | ApiManager 仅出现在 Adapter；异常、超时和取消转换为统一语义 |
| DONE | 最新请求保护 | 模糊候选、详情回填、报价确认、精确识别、全部/最优看板和报价管理均接入取消与旧响应保护 |
| DONE | 页面请求状态 | 看板与报价管理统一暴露 Loading / Loaded / Empty / Error / Stale，并提供 Retry |
| DONE | 操作结果模型 | 定义 MutationResult，覆盖成功、失败、部分成功、冲突和字段校验失败语义；具体后端操作待契约到位后接线 |

### P1 服务端查询模型

| 状态 | 项目 | 验收标准 |
|---|---|---|
| DONE | 行情查询模型 | UI 筛选、分页、排序形成稳定 MarketBoardQuery，Mock 模拟服务端筛选 |
| DONE | 报价管理查询模型 | 今日/历史查询统一携带筛选、日期、分页和排序；Mock 在服务层执行筛选 |
| DONE | 查询防抖 | 行情中心与报价管理文本条件采用 300ms 防抖；切换条件取消等待任务和旧请求 |
| DONE | 查询元数据 | 看板与报价管理状态栏使用 TotalCount，并保留 QueryId / IsPartial |
| DOING | 筛选面板接线 | 顶部搜索已写入 Query State，清除按钮同步重置可见标签/输入框；左侧标签、区间字段等待后端 Query 契约 |

### P1 Mock 可演示业务闭环

| 状态 | 项目 | 验收标准 |
|---|---|---|
| DONE | 进程内 Mock Store | 三大模块共享可变数据，确认/删除/转意向后刷新不恢复 |
| DONE | Mock 请求场景 | MockRequestOptions 支持延迟、下一次失败、空结果；查询模型支持分页，部分成功待写操作契约 |
| DOING | Mock 推送 | 新增/修改/撤销报价能经与真实网关相同的订阅语义到达页面；当前已覆盖新增/撤销，编辑 mutation 等待契约 |
| TODO | 原型模式标识 | 未接能力统一禁用或明确提示，不伪装为真实落库成功 |

### P1 前端业务规则

| 状态 | 项目 | 验收标准 |
|---|---|---|
| DONE | 最优报价投影 | 按代码、价格类型、清算速度分组；收益率与价格口径采用正确方向 |
| DONE | 最优页查询开关 | Bid/Ofr、我的、急、荐、整量走领域查询，不做表面状态切换 |
| DOING | 批量编辑校验 | BP、报量已具备非负输入、无选中行和非法行提示；价格类型、必填字段边界校验待字段契约明确 |
| DOING | 状态与统计一致性 | 看板与报价管理、意向和提醒已统一 Loading/Loaded/Empty/Error/Stale 与重试；意向/提醒的分页总数仍待后端 Query 契约 |

### P2 测试与验收

| 状态 | 项目 | 验收标准 |
|---|---|---|
| TODO | 纯业务规则测试 | 覆盖最优投影、筛选 Query 映射、分页、三态选择、批量调整 |
| TODO | 异步竞态测试 | 覆盖快速切换、旧响应晚到、Dispose 后返回和取消 |
| TODO | Mock mutation 测试 | 覆盖成功、失败、部分成功和刷新后一致性 |
| TODO | 验收脚本兼容 | PowerShell 版本/编码明确；路径参数化；失败返回非零退出码 |
| DONE | 现有冒烟基线 | pwsh 下 Smoke、Pixel、EmptyHint 三项通过（使用当前部署 DLL 基线） |

### P0/P1 BondDetailPanel 原型对齐专项

| 状态 | 项目 | 验收标准 |
|---|---|---|
| DONE | 原型主布局还原 | 详情头部按最新成交/昨高开低/久期中债三列呈现；机构标签单行横向滚动；最优报价条固定 44px；多档与成交区域紧凑且不撑破 384px 面板 |
| DONE | 移除额外债券信息区块 | 主布局按原型从市场行情直接进入智能推荐；评级、期限、债券库字段暂保留在 VM，不臆造后端契约 |
| DONE | 智能推荐日期范围 | 保留近 7/30/90 日快捷选项，增加开始日期、结束日期双向绑定；日期变化进入统一刷新入口 |
| DONE | 智能推荐分页 | 增加页大小、当前页、总页数、上一页/下一页命令；分页摘要与列表视图保持一致 |
| DONE | 详情交互 MVVM 化 | 机构标签选中状态、推荐方向/范围、分页、日期状态由 VM 绑定维护，机构筛选不再依赖 Click 事件 |
| DONE | 视觉验收 | 对比原型完成区块顺序、固定高度、颜色、间距和窄窗口滚动行为的自动化布局/像素冒烟验证 |

### P1 原型 0910 整体页面对齐（第五轮，TODO）

> 来源：`PROTOTYPE_UI_GAP_0910.md` §「整体页面对比（第五轮）」。用户提供原型/WPF 两张全页截图后逐项量测得出。

| 状态 | 编号 | 项目 | 验收标准 |
|---|---|---|---|
| DONE | 整-B | 表格列宽对齐原型 | 信用/利率「全部」20 列按 `BOARD_COLUMN_DEFS` 宽度（合计 2204）设置；报价 150、简称 190、报价时间 125、中债偏离 105 等不再偏窄 |
| DONE | 整-A | 表格对齐方式对齐原型 | 表头与单元格：剩余期限/代码/简称/方向/报价/报量 6 列左对齐，其余 14 列居中（数值列不再右对齐） |
| DONE | 整-C | 行高 32 → 34 | `StcDataGrid.RowHeight` 32 → 34（或放宽 cell 上下 padding），与原型 `td padding:7px + 14px 行高` 一致 |
| DONE | 整-H | 区间输入行样式对齐原型 | `FilterRangeRow` 新建完成：标签 52px / 12px、输入框 43px / 12px / padding 4,0 / 右对齐；支持 `ShowMax`（Vol.Bid/Vol.Ofr）与 `IsDisabled`（Bid-Ofr） |
| DONE | 整-G | 「筛选」按钮改次级灰底样式 | 新增主题 `StcFilterMiniButton`（22px / 灰底 `#21262D` / 字 `#8B949E` / 边框 `#30363D`），3 个 filter panel 的 5 处「筛选」全部改完（计价方式行 ×3、剩余期限行 ×2） |
| DONE | 整-E | active 下划线色改 `#79C0FF` | `StcNavItem` / `StcSubTabItem` 选中态下划线由 `OfrBrush(#3B82F6)` 改为 `AccentBrush(#79C0FF)` |
| DONE | 整-F | 子页条字号 14 → 15 | `StcSubTabItem.FontSize` 14 → 15、`Padding` 12,0 → 14,0 |
| DONE | 整-I | 计价方式选项内距 20 → 28 | 3 个 filter panel 的收益率/净价 ToggleButton 改为 `Padding="28,2" FontSize="12"` |
| 不做 | 整-D | 顶栏补「需求管理」导航 | 顶栏新增第 4 个一级导航项「需求管理」，按角色（admin/technician）控制可见性，并接入对应页面。**2026-09-12 产品确认：不做**——原型「需求管理」页（page-requirements）是内嵌的业务需求规格说明书（SRS 文档阅读器），仅产品交付物，非客户端功能页 |

> 第六轮（filter panel Bug + 提醒弹窗观感）详见 `PROTOTYPE_UI_GAP_0910.md` §「左侧筛选栏 + 提醒弹窗观感对齐」。

## 4. 本轮增量记录

- 2026-08-22：行情中心与报价管理接入统一查询状态、重试、QueryId/IsPartial 元数据；报价管理筛选迁移到服务端语义。
- 2026-08-22：MockRequestOptions 增加延迟、失败、空结果注入能力；MutationResult 与 QuoteMgmtQuery 已纳入编译项。
- 2026-08-22：行情中心与报价管理文本查询增加 300ms 防抖调度器，开关/模式/状态/日期条件保持即时查询。
- 2026-08-22：报价确认批量 BP/报量调整增加选中行、非负输入和行数据解析校验；非法行跳过并通过 Toast 提示，避免静默按 0 修改。
- 2026-08-22：行情中心清除过滤按钮同时清空 ViewModel 查询文本和当前面板可见 Toggle/TextBox，避免查询条件与视觉状态不一致。
- 2026-08-22：意向报价与报价提醒迁移到统一请求状态和最新请求保护，增加取消、错误提示、空态和重试入口，避免异常被吞掉后伪装为空数据。
- 2026-08-24：三大模块接入共享进程内 Mock Store；待确认报价确认后同步投影到行情和今日报价，删除/转意向/意向状态修改后重新查询保持一致；补充 MockStoreCheck 验收脚本。
- 2026-08-24：Mock Store 通过 IMarketCenterService 的既有 QuoteEvent 订阅语义发布 New/Revoke 事件；全部/最优看板订阅事件并在 UI Dispatcher 上重新查询；编辑事件待报价编辑 mutation 契约到位后补齐。
- 2026-09-10：按原型 0910 对比结论完成界面层对齐（明细见 `PROTOTYPE_UI_GAP_0910.md`）：
  1) 术语改名「意向报价 → 模糊报价」（子页标签、Toast、模型/枚举注释；后端接口名与表名未变）；
  2) 利率筛选面板删除地方债地区 / 主体评级 / 做市标的 / 永续债 4 组；
  3) 精确报价、模糊报价隐藏「截图识别 / 导入图片 / 导入表格」入口（命令保留）；
  4) 报价录入浮窗 1200×720 → min(1970,96vw) × min(840,92vh)；
  5) 设置窗口补「弹窗提醒 - 我的最优报价」；
  6) 最优/提醒页列头「销售.Bid」→「长城交易员.Bid」；
  7) 行情中心全部/最优页新增「有 N 条新报价，点击刷新」横幅（选中行后新报价只计数）；
  8) 最优页新增「同档报价笔数」按钮 + 7 列悬停浮层（仅本人报价可点击跳转全部页定位）；
  9) 右下角提醒弹窗改版：按类型分组、头部「类型 (新增数)」+ 查看/关闭、正文最多 3 条、条目两行布局；
  10) 报价表单弹窗重做：880px 宽 + 债券摘要条（市场最优/中债估值）+ 价格 ±1bp / 报量 ±1k 步进 + 报量单位下拉 + 标签仅急/荐/整 + Enter/Esc 快捷键 + 字段错误位；
  11) 子页角标（报价确认待确认数 / 报价提醒未读数）落地；
  12) 既有差距补齐：信用面板补「中证隐含」、提醒/报价管理共用面板补「地方债专项」「地区等级」并修正区域区标签、报价确认机构/交易员输入框加占位文案。
- 2026-09-10：产品侧确认原型的「模拟」按钮（`POST /api/quote-confirm/mock`）仅为原型侧测试入口，客户端不实现；此前按 UI 口径临时加的一版（按钮 + 忙碌态）已完整移除，`QuoteConfirmViewModel.IsMockDownlinking` / `MockQtDownlink` 不再存在。该项不再作为待补差距。
- 2026-09-10：报价表单弹窗与报价录入浮窗「观感不对」复刻校准（按 app.css 逐条核对，共 19 项偏差，清单见 `PROTOTYPE_UI_GAP_0910.md` §A12 复刻校准）。主因是弹窗底色/边框/投影用错（#080A10 而非 #161b22、#484F58 而非 #30363d、原型无投影）、固定高度导致留白、以及缺整窗遮罩。新增 `Shared/Services/OverlayBackdrop.cs`：弹窗窗口铺满宿主并在内部用半透明黑 Border 实现 60%/68% 遮罩（WPF 独立 Window 无法直接压暗宿主）。
- 2026-09-11：编辑报价弹窗继续复刻校准（两次实机截图）：备注框多占一行（50→34px）、报价标签复选框改原生风白底方框、弹窗头部高度 52→60、备注不再复用单行垂直居中模板；明细见 `PROTOTYPE_UI_GAP_0910.md` §A12「第三轮」。
- 2026-09-11：行情中心三个看板补齐与原型一致的**行右键菜单**（原型 `#ctxMenu` 8 项 + 子菜单 + 3 条分隔线 + 图标），抽成共用字典 `Modules/MarketCenter/View/BoardRowContextMenu.xaml`（加 `x:Shared="False"`，三个 DataGrid 各用各的实例）；新增 `QuoteFormMode`（新报价 / 编辑报价）与 `QuoteEntryDialog` 的提醒行重载。原型「报价日志」弹窗（`quote-log-modal`）插件缺失，暂以 toast 待接。
- 2026-09-11：右键菜单**定位**改用项目既有做法 —— `Placement="MousePoint"`（= 原型「左上角对齐鼠标」）+ `RelativeDropHorizontalOffsetConverter` 补回菜单宽度，抵消系统 `SystemParameters.MenuDropAlignment`（菜单左对齐）造成的整体左移。此前自研的 `Shared/Services/ContextMenuPlacer.cs`（`Absolute`+自算 DPI / `Relative`+附加属性 / `Opened` 自校正，三版均被实机否决）**已完整删除**，三个看板的 `Grid_OnContextMenuOpening` 还原为原实现（只注入行 DataContext）。
- 2026-09-11：按用户两张全页截图完成**整体页面对比**（原型 1666×936 / WPF 1911×957），逐项量测后新增 9 条界面差距（编号 整-A ~ 整-I，明细见 `PROTOTYPE_UI_GAP_0910.md` §「整体页面对比（第五轮）」）。其中影响最大的是**表格对齐方式**（原型 14 列居中/6 列左，WPF 表头全左、数值右）与**列宽全线偏窄**（合计 1798 vs 原型 2204，−18%），其次是行高（32 vs ≈34）、区间输入行样式（原型 43px/12px/右对齐 vs WPF 自适应/11px/左对齐）、「筛选」按钮配色（原型灰底 20px vs WPF 蓝底 30px）、active 下划线色（原型 #79C0FF vs WPF #3B82F6）、子页字号（13→14 应为 15）、以及**顶栏缺「需求管理」导航项**。以上均为 TODO，尚未实施；整-B/整-C 落地后需重建 `tools/captures/*.png` 基线。
- 2026-09-11：本轮复用像素量测脚本 `~/.workbuddy/skills/prototype-version-diff/scripts/img_diff.py` 扩展版（网格线间距法量行高）：`cmp-table.png`（WPF 1:1 裁剪）间距稳定 32px，与 `RowHeight=32` 吻合；两同行数裁剪图按宽度归一后得原型行高 ≈ 32×1.078 ≈ 34px。
- 2026-09-11：**左侧筛选栏 + 提醒弹窗观感对齐**（第六轮，TODO→DONE 一批；明细见 `PROTOTYPE_UI_GAP_0910.md` §「左侧筛选栏 + 提醒弹窗观感对齐（第六轮）」）：
  1) **找到 2 个真 Bug**（不是观感差异）：3 个 filter panel 的「计价方式」行内层 `<Grid>` 漏了 `Grid.Column="1"`，导致 收益率/净价 toggle 与 30px 高的蓝底 `StcButtonPrimary`「筛选」按钮**全部叠在 col0**——WPF 截图只看见孤零零的蓝按钮，下面两个 tag 被完全覆盖。修复：内层加 `Grid.Column="1"` + 改成「`* / Auto` 2 列」让 tags 左扩、按钮贴右。
  2) 「筛选」按钮在 3 个面板共 5 处（计价方式 ×3 + 剩余期限 ×2）从 `StcButtonPrimary`（蓝 30px）改为新增主题 `StcFilterMiniButton`（灰 22px 与原型 `.filter-btn-primary` 等效）—— 第五轮 整-G 落地。
  3) 新建 `Shared/Components/FilterRangeRow.xaml(.cs)` 区间输入行控件：标签 52px / 输入 43px / 右对齐 / 占位"最小/最大" / 11px 间隔 / 11px 单位 —— 第五轮 整-H 落地；并支持 `ShowMax`（Vol.Bid/Vol.Ofr 只有最小）和 `IsDisabled`（Bid-Ofr 整行置灰对应原型 `.filter-page-unavailable`）。
  4) 主题 `Controls.xaml` 追加 6 个样式：`StcFilterLabel`（70px 列宽 / 13px / `#6e7681`）、`StcFilterButtonBase`、`StcFilterButton`（27px / 顶栏「清除过滤」）、`StcFilterMiniButton`（22px / 行内「筛选」）、`StcFilterTag`/`StcFilterTagAdd`（22px / 行内 tag）；并更新 `StcSectionTitle` 为 13px/600/`#8b949e`；给 `StcTextBox` 模板加上 `Tag` placeholder 支持（搜索框 placeholder 走这条）。
  5) **提醒弹窗「看起来像多个窗口」** —— 答案：本来就是一个独立透明 Window + `ItemsControl` 绑 `AlertPopupService.Groups`，现在把每组的 `DataTemplate` 的薄分隔 `Border` 升级成**独立卡片**：`Background=PanelBgBrush(#161b22) / BorderBrush=BorderStdBrush(#30363d) / CornerRadius=8 / DropShadowEffect blur=24 depth=4 opacity=0.55`，并把外层容器改为 `StackPanel` + `ItemContainerStyle Margin=0,0,0,10`（= 原型 `.alert-popup-wrap{gap:10px;flex-direction:column}`）。Service / VM / code-behind 一行未动。"要不要多个 Window" 实测**否决**（多 DWM 句柄 / 定位 / 关闭动画 / Z-order 都比单窗差），沿用"弹窗即独立 Window, Service 管理多弹窗"的老设计。
  6) 3 个 panel 的「计价方式」toggle `Padding="20,2" → "28,2" FontSize="12"` 与原型内联 `padding:2px 28px;font-size:12px` 一致 —— 第五轮 整-I 落地。
  7) 静态校验：`xaml_check.py` 解析 0 失败、`check_keys.py` 主题 132 keys + 各面板本地资源 100% 命中，事件处理器全在 code-behind 中定义；**待开发环境编译 + 重跑 Capture 重建基线**。
- 2026-09-11（晚）：**筛选组布局重构**——原型每个筛选组都是 `.filter-group{flex}` = 左侧 70px label + 右侧 flex:1 内容，WPF 原实现是"标题在上、选项在下"。已把四个面板（信用 29 / 利率 11 / 提醒 35 / 模糊报价 29）共 104 组重构为 `Grid[Auto|*]`：`StcFilterLabel`（MinWidth=70）在左、WrapPanel/输入框/下拉在右（Margin 3,0,0,0 = 原型 flex gap 3px）；tag 间距 `6,6→3,3`（原型 `.filter-tags{gap:3px}`，共 493 处）；发行人/担保人搜索框升级 Tag 占位；剩余期限 label 跨 tags+输入行；单行下拉（行业/地区等）label 统一 70px 列。转换脚本 `.tmp/shots/_convert_filter_groups.py`；静态校验通过。**待开发环境编译 + 重建截图基线。**
- 2026-09-11（晚）：提交 716d8bbf2（左侧筛选栏重构 + 提醒弹窗多卡片 + 分区分割线 + 单行下拉列修复，14 文件）。
- 2026-09-11（晚）：**关注债券管理 + 筛选分组管理弹窗按原型实现**（A4/B5 UI 落地；明细见 `PROTOTYPE_UI_GAP_0910.md` §第七轮）：
  1) 新增 `Shared/Services/FilterProfileService`（会话内存储 + 条件目录静态数据，取值与筛选面板一致）与 `Shared/ViewModel/FilterProfileModels`；
  2) `FavBondManagerWindow`：分组列表（双击重命名 / hover 删除 / + 新建行内输入）+ 选中分组债券列表 + 粘贴代码识别表（勾选即加入/移出当前分组；**债券信息回填待接**，行按 found=false 半透明展示）；
  3) `FilterGroupManagerWindow`：分组列表 + 子分组卡片（条件 chip 可删、非空删除二次确认、+ 新增分组）+ 条件目录（分区多选 toggle、文本/下拉/区间「添加」、不可用项置灰）+ 粘贴条件文本识别（MVP 与目录 label 精确匹配，语义解析待接）；
  4) 三个筛选面板的「关注债券 +」「筛选分组 +」改为打开对应弹窗（信用/利率各一套数据；提醒合并页原型无独立字段，暂按信用口径待确认）；
  5) 保存 = 会话内整体替换 + Toast；**后端同步、侧栏「我的关注」chip 联动、分组应用到看板查询均待契约**。
- 2026-09-11（晚）：**第七轮弹窗实机复核后逐条返工**（明细见 `PROTOTYPE_UI_GAP_0910.md` §第八轮）：
  1) P0 行为对齐原型：fgm 文本识别**自动加入激活子分组**（identity 去重）+「已识别 N 个条件/未匹配到条件」
     toast +「请先选择左侧分组」前置校验；fbm 切换分组/移除债券后**回填结果表勾选态**（`_fbmSyncCheckState`）；
     fbm/fgm 删**非空分组**与 fgm 删**非空子分组**接入新增的 `Shared/Components/ConfirmDialog`
     （复刻原型 `_fbmConfirm` 深色确认框，替代系统 MessageBox）；fbm 识别经新领域接缝
     `IMarketCenterService.LookupBondAsync` 在 Mock 债券库回填 名称/期限/债券库/中债估值/市场
     （真实 /parse 待接，仅换实现），并加**市场校验**（跨市场债券拒绝入组，文案照原型）；
  2) P1/P2 观感：fgm 子分组**卡片样式重做**（#0d1117 底/边框/active 蓝描边/空卡提示/右上 ✕ hover 显示）、
     chip 只显 value（ToolTip 全称）、fbm 表头**全选**、已加入行**绿底**、空结果 **12 行骨架**、
     左下/左下空态文案、头部 border-bottom + ✕ hover、重命名 maxlength 30（新建仍 10）、
     目录「添加」钮改原型红底、自定义条件行整行排列（输入/区间撑满、下拉「请选择」占位）、
     结果表列宽按比例撑满、range M/D 单位折算成年、删空子分组回落空卡；
  3) 模型修复：`FavBondEntry` 实现 INPC（勾选/回填驱动行样式）+ 补 `Market` 字段；
     `FilterNamedGroup.ConditionCount` 订阅集合变化（修复加删条件后分组计数不刷新）；
  4) 静态校验：37 XAML 解析 0 失败、事件处理器全定义、资源键全命中、改动 .cs 括号配平、csproj 良构。
     **待开发环境编译 + 实机按文档清单点检。**
- 2026-09-12：**编译修复（用户报 MC3072 ×2）**：两个管理弹窗头部又在 `<Grid>` 上写了 BorderBrush/BorderThickness
  （与第七轮 ff725da87 同类错误——Border 属性必须写在 Border 上）。已改 Border 包内层 Grid。
  **根因是校验脚本**：第七轮记录称已给 `_xaml_check.py` 加「Panel 族禁用 Border 专属属性」规则，但当前脚本里
  该规则并不存在（规则丢失未被察觉），本轮复扫因此放行。已把规则真正补进 `.tmp/shots/_xaml_check.py`
  （Grid/StackPanel/WrapPanel/UniformGrid/Canvas/DockPanel/VirtualizingStackPanel 禁
  BorderBrush/BorderThickness/CornerRadius），并用负面用例验证检测器生效，全仓 37 XAML 复扫 0 问题。
  **教训：修过的坑必须让校验器真实拦截并验证一次（构造反例跑通），"加了规则"要以脚本内容为准，不以记忆为准。**
- 2026-09-12：**继续编译修复（C# 错误 ×2，并纠正环境结论）**：① ConfirmDialog 缺
  `using STC.SalesTrading.Plugin.Shared.Services`（OverlayBackdrop 所在命名空间，CS0103）；
  ② FavBondManagerViewModel 调 `LookupBondAsync(code)` 漏传 CancellationToken（CS7036，补 None）。
  **重要：本机实测存在可用的 MSBuild**（`D:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`，
  级联编译 Common/Frame，退出码 0、0 错误）——此前"本机无 MSBuild"的记录有误；今后交付前必须本机编译。
  静态校验抓不出 C# 类型/签名错误，只能靠真实编译。
- 2026-09-12：**fbm 结果表重做 + 离屏渲染自查落地**（明细见 `PROTOTYPE_UI_GAP_0910.md` §第八轮追加）：
  1) 用户实机反馈表格丑 —— 根因是 ListView+GridView 只改色不换模板，残留默认表头模板/系统蓝色选中/
     白色原生 CheckBox/无行分隔线。已按 `.fbm-table` 自绘：表头 Border(sticky) + ItemsControl 行
     （分隔线/hover/绿底/半透明/StcCheckBox/固定列宽+简称列撑满），删掉 SizeChanged 列宽补偿；
  2) **新验证手段**：`.tmp/shots/_preview_modals.ps1` 离屏渲染（反射私有构造器 + RenderTargetBitmap），
     不弹真窗出 PNG + 视觉模型逐项核对；
  3) 离屏自查发现并修复**第七轮遗留 Bug**：fgm `SubgroupList` 首次提交起就没绑 `ItemsSource`
     （子分组卡片区一直空白）；补绑定 + `IsSynchronizedWithCurrentItem` + SelectionChanged 忽略 -1；
  4) 验证：MSBuild 0 错误、37 XAML 0 失败、括号配平通过。**后续每轮 UI 改动交付前走
     「编译 → 离屏渲染 → 视觉自查」流程。**

## 5. 暂缓项

- 后端传输 DTO、JSON 字段名和服务 Topic 常量：等待正式契约。
- HTTP、SSE、WebSocket 等具体实现：当前优先适配 STC ApiManager。
- 真实 OCR、Excel 导入和 CRM 联想：等待依赖与接口确认。
- 真实鉴权、账号切换与权限模型：等待宿主和后端契约。

## 6. 更新规则

1. 每完成一个可验证增量，立即更新本文件状态和验收结果。
2. 新发现的问题先写入台账，再实施；不在代码里保留无归属 TODO。
3. 后端契约到位后，新增字段映射计划，不绕过领域服务让 ViewModel 直接访问 Gateway。

## 7. 当前验证记录

- 2026-08-22：git diff --check 通过。
- 2026-08-22：PowerShell 7 下 Smoke/Pixel/EmptyHint 通过。
- 2026-08-24：独立输出目录编译、MockStoreCheck、SmokeTest 均通过；未产生新的编译错误，保留项目既有警告。
- 2026-08-24：Mock 推送接线后再次通过独立编译、MockStoreCheck、SmokeTest；MockStoreCheck 已覆盖 New/Revoke QuoteEvent 及订阅退订，编辑事件保持 DOING。
- 2026-08-24：用户确认 BondDetailPanel 对齐原型；同意移除独立债券信息区块，并补齐智能推荐自定义日期范围与分页控件，专项进入 DOING。
- 2026-08-24：BondDetailPanel 完成原型结构对齐；移除独立债券信息区块，增加推荐日期范围、分页集合与上一页/下一页命令，机构 pill 改为命令绑定。
- 2026-08-22：用户实际开发环境已成功编译并运行；独立命令行 dotnet msbuild 在当前 shell 下存在 NuGet/源生成器加载差异，不作为项目构建阻塞。
- 2026-09-10：原型 0910 界面对齐改动完成（43 个文件，未提交）。当前环境无 MSBuild/VS，已用脚本做静态校验：全部 XAML 均可被 XML 解析器解析（0 失败）、XAML 中每个事件处理器在对应 code-behind 中都有定义（0 缺失）、引用的 DynamicResource 键在 Themes 中均存在。**需要在开发环境编译并重跑 Capture/PixelCheck 方可视为验收通过。**
- 2026-09-10：注意 `tools/captures/*.png` 基线因报价录入弹窗尺寸变更已失效，比对前必须先用 `tools/Capture.ps1` 重建基线。
- 2026-09-11：本次改动静态校验（本机无 MSBuild/VS）：全项目 XAML 解析 0 失败；`RelativeDropHorizontalOffsetConverter` 的命名空间/资源键引用齐全；`ContextMenuPlacer` 全仓无残留（含 csproj 编译项）；三个看板 code-behind 与 HEAD 一致。**待开发环境编译 + 实机右键点一次确认落点。**
- 2026-09-11：左侧筛选栏 + 提醒弹窗观感对齐（第六轮）静态校验：7 个相关 XAML 解析 0 失败；6 个面板/窗口文件 DynamicResource/StaticResource 引用 100% 命中主题（Controls+Brushes+DarkTheme 共 132 keys）或本地 `Window.Resources`（AlertPopupWindow 自有 3 keys）；Grid 属性全为整数；事件处理器无新增。**待开发环境编译 + 重跑 `tools/Capture.ps1` 重建 `tools/captures/*.png` 基线**。

- 2026-09-12：**第五轮整体对齐落地（整-A/B/C/E/F DONE、整-D 确认不做）+ 方向/报价颜色修正**：
  1) **方向/报价颜色**（用户实机反馈）：原型里 `.board-direction-value` 是普通文字色 #c9d1d9 + 600 字重
     （不按方向着色），`renderQuotePriceSpan` 才是 bid #f59e0b / ofr #3b82f6 + 700 粗体——
     WPF 原实现把 `DirBrush` 转换器用在了方向列、报价列却是白色。已互换：方向列普通色，
     报价列按 Direction 绑定橙/蓝 + Bold（`StcNumberCellLeft` 保持等宽）；
  2) **整-A/B**：三个看板列宽全部按 `BOARD_COLUMN_DEFS`（全部 20 列合计 2204 / 最优 22 列 / 提醒 23 列）；
     对齐 = 表头与单元格「剩余期限/代码/简称/方向/报价/报量」（最优/提醒页为 Vol.Bid/Bid/Ofr/Vol.Ofr 等）左对齐、
     其余居中。新增主题 key：`StcColumnHeaderLeft` / `StcTextCellCenter` / `StcTrimTextCenter` /
     `StcNumberCellCenter` / `StcNumberCellLeft`（只供看板引用，其它模块表格不动）；
  3) **整-C**：`StcDataGrid.RowHeight` 32→34；**补看板斑马纹**（原型 `table.dt tr.odd td{background:#1c2129}`，
     仅 boardRowHtml 输出的看板表有）——`StcRow` 加 `AlternationIndex=1 → #1C2129` 触发器（放 hover/selected 前），
     三个看板 DataGrid 加 `AlternationCount="2"`，其它模块不设 AlternationCount 不受影响；
  4) **整-E/F**：`StcNavItem`/`StcSubTabItem` 激活下划线 `OfrBrush(#3B82F6)` → `AccentBrush(#79C0FF)` +
     激活字重 SemiBold（原型 .nav-btn.active/.subtab.active font-weight:600）；子页 15px / Padding 14,0；
     一级导航 Padding 18,0 + 基础字重 Medium（原型 .nav-btn{font-weight:500}）；
  5) **踩坑**：新样式块插到了 `StcTrimText` 定义之前，`BasedOn="{StaticResource ...}"` 前向引用运行期
     XamlParseException（静态校验/XML 良构都查不出，编译也过）——Controls.xaml 内基于同文件样式派生新样式时，
     定义顺序必须"被引用者在前"（与既有 DynamicResource ColumnHeaderStyle 注释同源）；
  6) 验证：MSBuild 0 错误；`tools/Capture.ps1` 重建 11 张基线截图（整-B/C 改版后必做）；
     视觉自查通过（方向普通色 / 报价橙蓝粗体 / 前 6 列左其余居中 / 斑马纹 / 数值列居中 /
     最优页 Bid/Ofr 价格左排 / 导航激活 #79C0FF）。
- 2026-09-12：**修复管理弹窗“点新建后失焦整窗被关”**（用户实机反馈）：两个管理弹窗把
  `MouseDown=OnBackdropMouseDown` 挂在**根 Grid** 上，而弹窗卡片是其子元素——MouseDown 冒泡，
  点击弹窗内任意非交互区域（列表空白/文字）都会冒泡到根触发 Close。已改为**独立遮罩 Border** 挂载
  （与 QuoteEntryDialog/ConfirmDialog 同款：卡片是遮罩的兄弟节点，内容事件不进遮罩）。
  教训：**遮罩点击关闭必须判断命中目标**（挂独立遮罩层，或 handler 里校验 e.OriginalSource），
  不能挂公共祖先节点吃冒泡事件。
- 2026-09-12：**fgm 条件目录全量重对齐**（用户实机反馈债券筛选项不对；明细见 GAP 文档 §fgm 目录重对齐）：
  直接解析原型面板 DOM 重建静态目录 —— 移除多余的「报价筛选」区；key 全部改用原型 data-bond-filter-key；
  单项/整组置灰按 DOM 原位补齐（此前大面积缺失）；剩余期限改为 11 档标签+区间+Y/M/D 混排
  （目录组渲染从互斥分支改为可叠加，`CatalogConditionGroup` 补 `HasRange`）；补齐缺组缺项
  （行业下拉/其他/更多/永续债取值等）；区域筛选三处置灰组 + 地区[省级]34项（级联待接）；
  种子条件 key 对齐目录。MSBuild 0 错误 + 离屏渲染视觉自查通过。
  **教训：静态数据别凭印象写——先用脚本把原型 DOM 提取成对照表（_extract_catalog2.py），再照表落码。**
- 2026-09-14：**修复管理弹窗目录区滚动问题**（用户实机反馈「滚动条往下滚，内容没往上滚」）：
  1) 排查过程（本机可复现路径全部正常）：离屏/屏幕内实测——目录 SV 几何正常（Viewport 319/Extent 2164）、
     程序化 offset 内容正常跟随（截图对比 0/600px）、模态下滚轮正常；合成滚轮/拖拽测试因
     WM_MOUSEWHEEL 发给焦点窗口、console 抢前台而不可信，未能定位用户环境的精确根因；
  2) 落地修复（双管齐下）：目录/结果表 SV 从默认 Aero 滚动条换成主题 `StcPanelScrollViewer`
     （深色滚动条，与原型一致；换模板顺带排除默认模板在用户环境/高 DPI 下的异常）；
     两弹窗加 `PreviewMouseWheel` 兜底——光标下无可滚的内部 SV（textarea 内容不满、悬停 ComboBox
     会被吃轮改选中）时，把滚轮转给主滚动区并标记已处理；
  3) 验证：MSBuild 0 错误；离屏渲染确认深色滚动条生效、offset 0→600 内容正常滚动。
  **待用户实机复测**：若仍出现「thumb 动内容不动」，下一步排查方向为高 DPI +
  AllowsTransparency 渲染脏区（离屏渲染无法复现，需要实机录屏取证）。
- 2026-09-14：**修复目录勾选联动与控件样式**（用户实机反馈三条）：
  1) **根因（真 Bug）**：上轮重构 `BuildConditionGroup` 支持混排时删了各分支 return，但末尾仍
     `return new CatalogGroupView{...}`（新建空实例）——收集了目录按钮引用的 `view` 被丢弃，
     `_sectionViews` 里所有组 CheckButtons 为空，`RefreshChecks` 无从刷新 → 表现为"目录勾选与左侧
     子分组无联动、无选中样式"。已改 `return view`。离屏断言验证：Show 后热门标签"城投"chip
     背景=#143B82F6（原型 rgba(59,130,246,.08)）、`IsConditionActive=True`、Toggle 新增条件正常入卡；
  2) **选中色值修正**：`FromRgb(0x14,0x38,0x8B)`（实色深藏青，视觉上与未选中无差）→
     `FromArgb(0x14,0x3B,0x82,0xF6)`（原型 8% 透明蓝）+ 描边 40% 蓝（RefreshChecks/RefreshMatchedChecks 两处）；
  3) **输入框/下拉框套主题**：目录输入行从 code-behind 裸建（默认浅色 ComboBox 极扎眼）改为
     BasedOn `StcTextBox`（深色 + Tag 占位，覆盖 11px/22px）与 `StcComboBox`（深色模板，覆盖
     11px/22px/MinWidth96/单位 52px）；删除叠层占位方案（AttachPlaceholder/转换器）；
     踩坑：`TextAlignmentProperty` 定义在 Control 上（TextBoxBase 上没有，编译报 CS0117）；
  4) 编译 0 错误（obj DLL 验证；client\plugin 被用户进程锁住复制为环境现象非代码问题）。
- 2026-09-14：**修复粘贴框占位提示不隐藏**（用户反馈）：两弹窗 textarea 的占位显隐由
  `DataTrigger Binding={Binding ConditionInput/BondInput} Value=""` 驱动，但这两个 VM 属性是
  **无通知的自动属性**——输入写回 VM 后不发 PropertyChanged，触发器停在旧值"空"上，占位永不隐藏。
  已改为完整 INPC 属性（`FavBondManagerViewModel.BondInput` / `FilterGroupManagerViewModel.ConditionInput`）。
  离屏模拟用户输入验证：空=显示 / 输入=隐藏 / 清空=恢复，VM 值同步正确。
  **教训：凡被 DataTrigger/样式触发器绑定的 VM 属性必须走 INPC，自动属性只适合"一次性读"场景。**
