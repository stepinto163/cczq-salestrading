# FICC 销交平台 · STC.SalesTrading.Plugin 架构与实施计划（v0）

> 分支：`dev_sales_trading_v0` ｜ 原型：`.tmp/prd/ficc-client`（暗色 Electron 原型）
> 原型深度分析：`docs/PROTOTYPE_ANALYSIS.md`（色板/组件/接口/交互逐条摘录，UI 还原时以此为准）
> 技术路线：WPF + **CommunityToolkit.Mvvm 8.4.0**（离线 NuGet）+ **STC.Common.EventAggregator**（WeakReferenceMessenger 包装）
> 范围：**报价录入 / 行情中心 / 报价管理** 三大模块。后端接口未定 → **先锁数据契约，UI 只依赖接口**。

---

## 一、设计原则

1. **基础架构先行**：宿主契约、MVVM 基础设施、主题 tokens、通用组件、数据接缝先落地；页面级业务 UI 后置。
2. **单体模块化**：单个插件程序集，内部按域切目录（Shell/Modules/Shared/Abstractions/Mock/Themes），模块间**禁止互相引用**，跨模块通信只走 `EventAggregator` + `Shared/Messages` 契约。
3. **契约优先**：后端接口与出参未定 → `Abstractions/` 锁定语义接口 + DTO 字段（canonical 命名对齐原型 §4.4）；开发期 Mock 实现，接网关只换实现、VM 零改动。
4. **高保真还原**：View 不写死颜色/字体，全部走 `Themes/`（Colors → Brushes → Controls 三层，DynamicResource 引用，预留换肤）；组件样式全 keyed、零隐式样式，不污染宿主皮肤。
5. **宿主契约正确性**：沿用 v2 批次 A/B 已验证的结论（IsAlone 接回字段、load() 重入守卫、NotifyCloseTabItem 释放链、TFM 对齐、ProjectReference）。

## 二、目录结构

```
STC.SalesTrading.Plugin/
├─ SalesTradingPlugin.cs            # 插件入口（宿主契约修正 + 释放链 + XAML 诊断钩子）
├─ Shell/                           # 应用壳：顶导航(3页+角标)/用户下拉/页面切换
│  ├─ View/          MainView + OverlayService（IOverlayService 实现）+ UserCenter/Settings 弹窗
│  ├─ MainViewModel.cs  NavItem.cs  SubTabItem.cs  UserProfile.cs
├─ Modules/                         # 三大业务模块（模块间不互相引用）
│  ├─ QuoteEntry/    View/ ViewModel/            # 报价录入（报价确认/精确报价/模糊报价）
│  ├─ MarketCenter/  View/ ViewModel/ Model/     # 行情中心（信用/利率×全部/最优 + 意向 + 提醒；Model=筛选面板枚举）
│  └─ QuoteMgmt/     View/ ViewModel/            # 报价管理（今日 MgmtTodayView / 历史 MgmtHistoryView）
├─ Shared/                          # 跨模块复用
│  ├─ Components/    StatusBar · SearchBox · ToastHost · FilterRangeRow · AlertPopupWindow · KeepAliveContentControl（视图常驻切换宿主）
│  ├─ Converters/    Converters.cs（方向/涨跌/ToastKind 画刷、Bool/Null/Int/Empty、Price4）
│  ├─ Messages/      PendingQuotesChanged · AlertCountChanged · DataSourceReset
│  ├─ Services/      ToastService（UI 线程调度 + 自动消失） · IOverlayService（弹窗接缝，VM 不引用 View） · AlertPopupService · ColumnLayoutManager（列宽持久化）
│  └─ ViewModel/     StatusBarSection · ToastMessage · AlertPopupItem
├─ Abstractions/                    # 数据接缝（后端未定，先锁契约）
│  ├─ IQuoteService.cs  IMarketCenterService.cs  IQuoteMgmtService.cs
│  └─ Models/          Enums + 14 个 DTO（字段名对齐原型 canonical）
├─ Mock/                            # 开发期 Mock 实现（接网关时仅替换实现，VM 零改动）
├─ Themes/                          # DarkTheme(Colors+Brushes+Controls) + ThemeManager
└─ docs/                            # PROTOTYPE_ANALYSIS.md（原型逐条摘录）
```

依赖规则：`Modules → Shared/Abstractions`；`Shell → Modules`（组合根，注入 Mock 服务 + IOverlayService）；
`Mock → Abstractions`。ViewModel 不引用 View（弹窗走 IOverlayService 接缝，实现在 Shell/View）；
View 经隐式 DataTemplate 挂 VM，子页/页面常驻切换统一用 KeepAliveContentControl（按 Content 实例缓存，切换不重建控件树）。

## 三、基础设施决策（本分支已完成）

| 项 | 决策 |
|---|---|
| TFM | 全仓库 41 个 csproj 统一 **v4.7.2**（含 8 个原 v4.5.2），App.config supportedRuntime 同步 |
| CommunityToolkit.Mvvm | 8.4.0，离线 NuGet（`nuget/` 包体 + 根 `NuGet.config` + `packages\gpf` 还原目录） |
| 构建接入 | `build/Mvvm.targets`（源生成器去重 / WPF 临时工程补分析器 / 门面裁剪），STC.Common 与插件均 Import |
| 落盘策略 | 仅 STC.Common 设 `MvvmRuntimeDll` 白名单落 `client\bin`（4.7.2 实测 6 个：CommunityToolkit.Mvvm / System.Memory / System.Buffers / System.Numerics.Vectors / System.Runtime.CompilerServices.Unsafe / Microsoft.Bcl.AsyncInterfaces；RuntimeInformation 4.7.1+ 内置不再落盘）；插件复用 bin 下这份 |
| EventAggregator | `STC.Common.Utils.Events.EventAggregator`（静态门面包装 WeakReferenceMessenger.Default 独立实例：Publish/PublishOnUi/Subscribe(token)/RegisterAll/Unsubscribe；弱引用防泄漏）。老 `EventHub/EventType` 未动（宿主 tab 关闭机制在用） |
| 消息域 | 插件内跨模块联动走 EventAggregator；宿主广播（STC_Clear 等）仍走 `onNotify`，在入口桥接为 `DataSourceResetMessage` |
| 构建注意 | 删除 `obj\` 后必须带 `/restore` 重建（`/t:Build /restore`）：Mvvm.targets 的源生成器依赖 restore 产物，否则报大量假 CS0246（找不到 RelayCommand/ObservableProperty/IRecipient） |

## 四、功能模块拆解（详见 docs/PROTOTYPE_ANALYSIS.md §2）

### 4.1 报价录入（3 子页）
- **报价确认**：分批分组折叠主从表；18 列行内编辑；批量工具栏（确认/删除/Bid/Ofr/±BP/价格类型/清算/±报量/机构/交易员/急荐整OCO打包/已选计数）；2.5s 轮询增量；新到行自动勾选；状态栏来源/方向统计
- **精确报价**：可拖拽高度输入区（100–500px）；粘贴/截图 OCR/导入图片/导入表格；识别预览表 18 列可编辑；批量工具栏；Enter 识别/确认
- **模糊报价**：自由文本识别；左右两栏联动（左识别 6 : 右找券 4）；候选 50 条/页分页；命中/未命中高亮；确认转意向报价

### 4.2 行情中心（6 子页 + 左筛选侧栏 384px + 右债券详情 384px）
- 信用全部/利率全部（20 列，冻结前 3 列，列拖拽，工具栏开关：我的报价/紧急/推荐/整量/未报价债券）
- 信用最优/利率最优（22 列 Bid/Ofr 双侧聚合；收益率 Bid 低优/Ofr 高优，净价全价相反）
- 意向报价（7 列可编辑；Refer/Unrefer 乐观锁；未同步红色脉冲圆点）
- 报价提醒（23 列；4 开关；右下角弹窗推送；已读状态）
- 详情面板 7 区块：头/最新成交条(昨高低开+久期)/机构筛选 8 pill/最优报价条(Bid+Vol/价差/Ofr+Vol)/多档行情+成交行情(TRD·TKN·GVN)/估值网格/智能推荐(搜索+4 scope tab+结果表+分页)

### 4.3 报价管理（2 子页）
- 今日报价（23 列表格；工具栏：报价方式/状态下拉、日期区间、债券/机构/交易员三联想搜索、Refer/Unrefer/删除/Copy/导出）
- 历史报价（按日终 eod-YYYY-MM-DD 分组 + 日期区间）

## 五、实施阶段

> 状态：`⬜未开始` `🔵进行中` `✅完成`

- ✅ **G0 基础设施**：全仓库 TFM→4.7.2（构建验证通过）；离线 NuGet；Mvvm.targets（4.7.2 适配，落盘白名单实测）；EventAggregator 落 STC.Common（编译通过）
- ✅ **G1 插件工程修正**：TFM/ProjectReference/宿主契约（IsAlone 接字段、load 守卫、NotifyCloseTabItem 释放链、XAML 诊断钩子）
- ✅ **G2 骨架**：Shell 导航壳 + 三大模块占位（子页结构按原型）+ Abstractions（3 服务接口 + 14 DTO）+ Mock 空实现
- ✅ **G3 主题与通用组件**：Colors/Brushes/Controls 三层 tokens（原型色板逐条摘录）+ StatusBar/SearchBox/ToastHost/FilterRangeRow + 转换器集
- 🔵 **G4 报价录入 UI 还原**（主体完成，待实机验证）：
  - ✅ 报价确认：分批分组折叠主从表（组头：全选三态/IDEAL蓝·QT绿徽标/机构/交易员/到报时间/N条·状态，点击组头折叠）+ 18 列行内编辑（方向/价格类型/清算下拉着色，报价/报量等宽右对齐）+ 批量工具栏（确认/删除/Bid/Ofr/±BP/价格类型/清算/±报量/机构/交易员/急荐整OCO打包/已选N）+ 状态栏统计（Ofr蓝/Bid橙）+ 确认后 nav 角标联动
  - ✅ 精确报价：可拖拽输入区（100–500px 手柄）+ 粘贴/示例/清空 + 识别预览表（类型+18 列可编辑）+ 同款批量工具栏 + Enter 识别/Shift+Enter 换行 + 极简行解析 Mock
  - ✅ 模糊报价：左识别结果（☑/等宽内容/条件标签/机构/交易员可编辑）6:4 右找券候选（50 条/页分页条）联动 + 批量机构/交易员 + 确认转意向
  - ⏸ 桩位（接网关时替换）：截图 OCR / 图片识别 / Excel 导入（占位 toast）；CRM 机构/交易员联想；2.5s 批次轮询（当前静态 Mock）；真实识别引擎（parse/intent-recognition）；"待补"黄色占位符；18 列定义暂在确认/精确两页重复，G8 抽共享列资源
  - ✅ 样式对齐（实机反馈后）：工具栏控件按原型尺寸（tb-btn padding 5×14/font 14 中粗、tb-input 4×10、下拉高 26）；行内单元格下拉改下划线式 StcUnderlineComboBox（原型 .field-underline .client-select）；深色复选模板（14px 圆角框/蓝底白勾/半选横杠）；DataGrid 单元格模板透明底（选中色失焦不退灰）+ 上下居中 + 左右 8px 间隙；表头模板（#161b22 底/hover #1c2129）；选中悬停 #2d4359；行高统一 32
  - ⬇ **降优先级**：报价确认主从表的组头形态重做——参考 `D:\Work\Coding\stc-client-lite\STC.SalesTrading.Plugin\View\QuotationEntry\QuotationConfirmView.xaml` 的主从实现（当前 DataGrid RowDetails 方案可用，先不动）
- 🔵 **G5 行情中心 UI 还原**（代码完成，待 STC 关闭后最终构建+冒烟+实机验证）：
  - ✅ 信用/利率全部看板：20 列宽表（冻结前 3 列）+ 工具栏（Copy/Refer/导出/Bid/Ofr 开关/我的报价/紧急/推荐/整量/未报价债券）+ 状态栏统计（Bid 橙/Ofr 蓝）
  - ✅ 信用/利率最优看板：22 列 Bid/Ofr 双侧聚合（Bid 橙底列/Ofr 蓝底列）+ 对价计数；与全部行同源聚合（收益率 Bid 低优/Ofr 高优）
  - ✅ 意向报价：7 列（报价内容行内编辑/未同步红点/状态着色）+ 工具栏（更新报价/Copy/Refer/Unrefer/导出/日期区间）
  - ✅ 报价提醒：提醒类型着色（绿/蓝/金/紫）+ 23 列 + 4 开关过滤 + 状态栏对价
  - ✅ 右侧详情面板 7 区块：头/最新成交条（涨红跌绿+昨高低开）/久期+中债/机构 8 pill/最优报价条（Bid 橙+价差 BP+Ofr 蓝）/多档行情+成交行情（TRD绿·TKN紫·GVN金）/债券信息网格/智能推荐占位；选中行驱动（全部/最优/提醒表 SelectedRow → 服务异步回填）
  - ✅ Mock 数据：14 信用 + 12 利率行 + 最优聚合 + 5 意向 + 3 提醒 + 详情（5 档多档/5 成交）
  - ✅ 详情面板按比例布局：头部/成交条/机构pill/最优报价条自然高度，多档+成交(3*)与智能推荐(2*)按比例分配，整体 MinHeight 780，不足时外层滚动条出现
  - ✅ 左侧筛选面板完整化并按子页切换（原型 filters-credit/rate/intent/merged）：清除过滤(一键复位)+搜索占位、关注债券/筛选分组(+按钮)、报价筛选(清算速度/计价方式+10 项 min-max 区间网格 %/BP/万)、债券筛选(细分券种/热门标签20/NCD/剩余期限11档+自定义/发行人/上市状态/发行方式)、流动性筛选(S/A/B/C/D 有色)、信用筛选、条款筛选、区域筛选(省份 34 项下拉)；意向面板含「仅显示未完整识别」
  - ✅ 实机反馈修复：左右面板 384↔40px 折叠手柄（箭头翻转）；列拖拽/拉宽恢复（样式曾误锁 CanUserReorderColumns）；报价弹窗改无边框自绘窗体（标题栏拖拽/最小化/关闭 + 任务栏可见）；右键菜单修复（MenuItem 模板 Header 绑定 + 菜单移至行级，表头不再弹出）；提醒弹窗改独立无边框窗口定位鼠标所在屏幕右下角（System.Windows.Forms Screen）；详情面板空态不再整体消失（常显+置顶提示）
  - ✅ 二轮实机反馈修复：①两侧收起按钮移入各面板内部（筛选面板头部右上/详情面板头部右上），折叠 40px 时内容整体隐藏只留竖条展开按钮，不再出现"收不干净"；②双击行打不开报价窗体 → 根因：独立 Window 未合并主题字典，DynamicResource 全部解析失败导致窗体透明不可见，已自合并 DarkTheme（同根因修复提醒弹窗透明）；ShowForRow 加 try/catch toast 兜底，表头双击不再误触发；③右键菜单仅限行（ContextMenuOpening 对表头/空白区 e.Handled 取消）+ Placement=MousePoint 靠鼠标弹出，行对象注入菜单 DataContext 供 CommandParameter；
  - ✅ 交互深化：列拖拽/宽度持久化（ColumnLayoutManager 落盘 %LOCALAPPDATA%，四表通用）；双击行/右键「报价录入」→ 1200×720 报价弹窗（预填+提交 toast）；行右键菜单（Copy 报价文本/Refer/报价录入/导出）；债券代码 hover 换算 tooltip（代码/简称/期限/评级/中债+最新/换算说明）；报价提醒右下角弹窗（AlertPopupHost，8s 自动消失，nav 角标联动）；详情面板机构 pill 实际过滤多档行情
  - ⏸ 筛选交互桩位（接数据时替换）：标签不驱动表格过滤、搜索框未接、机构 pill 过滤未接、区间输入未参与过滤
  - ⏸ 桩位：列拖拽换位持久化、双击行编辑弹窗、右键菜单、报价换算 tooltip、右下角提醒弹窗推送、机构筛选 pill 实际过滤、推荐区真实数据、未报价债券数据源
- ✅ **G6 报价管理 UI 还原**（今日/历史 + 三联想搜索 + 导出）
  - ✅ 今日报价：23 列宽表（报价方式 QT绿/ideal蓝/手工橙、报价状态撤销灰、方向 Bid橙/Ofr蓝、旗标文本、前 3 列冻结）+ 工具栏（Refer/Unrefer/删除/Copy/导出 + 报价方式/报价状态两下拉 + 代码·简称/报价机构/长城交易员三联想搜索）+ 状态栏统计（共 N 条/Bid/Ofr）
  - ✅ 历史报价：22 列（无报价状态列）+ 工具栏（无 Refer + 日期区间 DatePicker）+ 同款三联想搜索
  - ✅ Mock 数据：今日 15 行（QT/ideal/手工三 mode、有效/撤销、急/荐/整/OCO/打包旗标组合）+ 历史按日终日期分布 24 行
  - ✅ 新增转换器 ModeBrushConverter/StatusBrushConverter；MgmtQuoteRow 增 ModeText/FlagsText 只读展示属性
  - ✅ 左侧筛选栏：复用 filters-merged（AlertFilterPanel）+ 顶部清除过滤/搜索 + 384↔40 可折叠竖条（新建 MgmtFilterPanel，QuoteMgmtViewModel 增 IsFilterExpanded/FilterPanelWidth/ToggleFilterCommand）
  - ⏸ 桩位：Refer/Unrefer/导出接服务、删除/Copy 仅本 Mock 行生效、三联想候选从当前数据提取（接网关后由服务提供）、分页未做、筛选标签不驱动表格过滤
- ⬜ **G7 数据接入**：新增 Gateway 服务实现替换 Mock；SSE 推送 + 轮询兜底；权限/账号切换复位

## 六、验收标准

- 编译：`MSBuild STC.SalesTrading.Plugin.csproj /t:Build` 0 error（TFM v4.7.2 全链路）
- 运行：启动 STC → 菜单打开"销交平台" → 单 tab；三大页切换正常；深色主题生效；筛选侧栏标签可点选/清除/搜索；Toast 弹出自动消失；关闭 tab 释放 VM
- 落盘：`client\bin` 新增 dll 仅白名单 6 个；`client\plugin` 仅 STC.SalesTrading.Plugin.dll

## 七、变更日志

| 日期 | 内容 |
|---|---|
| 2026-08-21 | 架构重构落地：删除死代码链（FilterSidebar 链 / 三模块子页枚举 / 徽标双写，净 −998 行）；IOverlayService 解 VM→View 耦合；KeepAliveContentControl 统一视图常驻切换（替代 14 处 Visibility 长绑定样板）；筛选面板参数改枚举 x:Static 类型安全；QuoteMgmt 拆分 MgmtToday/MgmtHistory 子视图；PLAN.md 同步实际结构 |
| 2026-08-14 | v0 分支建立。全仓库 TFM→v4.7.2；离线 NuGet + Mvvm.targets（4.7.2 适配）；EventAggregator 落 STC.Common；插件单体模块化骨架 + 主题 tokens + 通用组件（本文件初版） |