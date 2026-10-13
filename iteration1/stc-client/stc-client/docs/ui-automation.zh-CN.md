# STC UI 端到端测试管理

这是独立的测试管理窗口，业务界面复用正式 STC 客户端。测试连接真实 GW、LoginSvr、BaseSvr、CRM、ParserSvr、QuoteSvr、NotificationSvr 及其数据库；不使用模拟报价服务。

## 启动

与 `client.cmd` 同目录的本机入口：

```powershell
cd D:\workspace\greatewall\ficc\qa_env\ats_root\scripts
.\autotest.cmd start       # 打开测试管理窗口，不自动执行用例
.\autotest.cmd status      # 管理窗口及测试工作进程状态
.\autotest.cmd stop        # 请求停止、清理并关闭窗口
.\autotest.cmd restart     # 清理并关闭后重新打开
.\autotest.cmd build       # 构建测试程序；运行中会拒绝覆盖
.\autotest.cmd report      # 最近一轮报告路径、用例结果及清理状态
```

仓库内也有 `iteration1\scripts\autotest.cmd`，可从仓库根目录直接调用，参数相同。默认使用仓库同级 `qa_env\stc-client\STC.exe.config`，窗口会预填该配置；其他环境可传 `-StcConfig <完整路径>`。启动不自动构建或启动后端，用例在窗口中勾选执行。

重复启动跳过；只匹配本工程测试运行目录中的进程，不关闭日常 STC 客户端。停止最多等待 45 秒，让既有测试清理机制完成；超时保留进程并返回错误，不强制杀进程。命令行 `run-ui-tests.ps1` 已在执行时，`stop/restart` 会拒绝关闭，应等待其报告。`RUNNING` 仅表示进程存在，不能视为测试通过；核对报告的 `status` 与 `cleanupPassed`。

本机 `qa_env` 不随 Git 提交；可直接使用仓库入口，或仿照本机 `client.cmd` 创建同样指向 `manage-ui-tests.ps1` 的快捷入口。

先启动本机 STC/FICC 后端。从仓库根目录执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/start-ui-tests.ps1 -Build
```

首次需要先构建 STC 客户端（`build-native-client.ps1`）。测试构建输出到 `runtime/ui-test-client`，不会覆盖正在运行的日常客户端。只启动已构建的测试管理窗口时，去掉 `-Build`。

选择环境的 `STC.exe.config`，默认账号/密码为 `test2/123456`，可修改账号、密码及债券代码。此版本要求 `Ficc.IntegrationMode=stc`，不把本机模拟模式的通过结果冒充真实服务验证。密码只传给本轮子进程，不写入用例文件。

## 管理用例

- 新增、编辑、复制、删除用例；用例 ID 保存后固定。
- 编辑器中增加、删除、上下移动步骤，从动作下拉框选择操作，在参数表中填写值，无需编辑 JSON。
- 勾选一个或多个用例后点击“运行勾选”；按列表顺序串行执行。
- 下方实时显示登录、业务步骤、清理及最终结果；选中记录查看完整错误。
- “停止并清理”请求停止当前用例，再执行清理；不会结束用户自己打开的 STC。
- 用例保存在 [`tests/ui/cases`](../tests/ui/cases)，可随 Git 同步、评审；删除用例不删除历史报告。
- “打开本轮报告”和“历史报告”可访问结果、截图、日志及本轮用例快照。

## 当前覆盖（2026-09-30：17 条可管理用例）

本次扩展直接加入现有用例列表。优先覆盖每天频繁使用的报价操作和近期反复出错的绑定/筛选，再覆盖解析边界及提醒闭环；没有按字段数量平均堆叠 UI 用例。优先级写在各用例描述中，单例仍可增删改、勾选运行。

| 用例 | 实际操作及断言 |
| --- | --- |
| 报价完整生命周期 | 精确报价输入并识别、勾选确认；报价管理修改/保存、Refer、Unrefer、删除，核对后端状态/价格及连续历史 |
| 识别机构交易员、勾选及确认报价 | 输入真实 CRM 文本、ParserSvr 识别、核对机构/联系人 ID、勾选/取消/重新勾选、确认，核对服务端有效报价及报价/行情页面推送 |
| 待确认报价实时展示 | API准备待确认夹具（非UI录入），核对报价确认页面收到并显示该报价 |
| `04-offer-t0` 卖出 T+0 | 补齐方向和清算分支，核对服务端数量/方向/清算及管理页 |
| `05-settlement-update` 修改清算 | T+0 改 T+1，同时修改数量和价格，核对实际保存值 |
| `06-invalid-price-recovery` 输入校验 | 非法价格有明确错误，原报价版本不变；修正后保存成功 |
| `07-management-filters` 报价管理筛选 | 自建报价；不匹配范围隐藏、等于边界命中、文本搜索隐藏、清除恢复 |
| `08-market-filters` 行情中心筛选 | 同样通过正式侧栏输入框和清除按钮，核对本例 ID 的显示/消失 |
| `09-multiline-partial-confirm` 分批确认 | 两行识别，先确认一条，保留另一条，再确认；每次都核对持久化 |
| `10-semicolon-quotes` 分号分隔 | 一段原文的两条报价均识别，方向/价格/数量逐行核对 |
| `11-preview-selection-toolbar` 选中范围 | 只选第一条，工具栏 Ofr 只改该行；删除后未选行保留、计数回零 |
| `12-no-selection-confirm` 未勾选确认 | 未选时预览保留且无已提交版本；勾选后同一行正常确认 |
| `13-cd-name-volume-before-yield` 用户真实原文 | `Bid 112508306.IB 25中信银行CD306 2000w 2 到期 明天+0`；期望一条 Bid、2000 万、收益率 2、明天 T+0；到期警告在界面可见 |
| `14-watchlist-notification-read` 提醒已读 | 独立关注分组，UI 创建确认报价，按本例 quoteId 找到提醒，UI 标记已读后查服务端验证 |
| `15-escaped-newline-settlement` 转义换行 | 使用用户截图原文，分别核对两条报价价格、1000/2000 数量与 T+1/T+0 |
| `16-enter-multiline` Enter 输入 | 向正式 TextBox 路由 Enter 键事件，断言真实换行、无自动识别，再识别核对两行 |
| `17-precise-toolbar` 批量编辑 | 无选择提示；选中范围、+1k/-1k、直接设置报量、1BP 调价、清算及计价选择，未选行保持不变 |

CRM 用例默认要求 `长城证券 / 田老哥 / 240210.IB` 存在，文本及期望值均可编辑。`${securityId}` 使用运行窗口填写的债券代码；自然语言用例默认采用已验证的固定文本，切换债券时请同时编辑文本和期望字段。

**覆盖边界：**仍未覆盖完整四类通知、多用户并发、全部条件筛选、滚动浏览和真实 QtIntake 链路。当前新增通知例覆盖关注报价和已读；价格范围与搜索的 UI 回归不等于 79 字段均已验证。未实现的动作不显示为通过；性能/压力测试不包含在本套件中。

新增动作由 `tests/ui/CaseDefinition.cs` 注册，实现在 `UiRegressionActions.cs`，编辑器自动列出动作及必需参数。`sidebar.*` 的 page 为 `mgmt/market`；`visible` 为 `true/false`；`parse.row` 的 row 从 1 开始，side 为 `Bid/Ofr`。市场用例及关注夹具使用利率债 `${securityId}`，默认 `240210.IB`；第 13 例必须保留用户原文，不能替换为默认债券来绕过失败。

### 本轮实际结果

2026-09-30 本机真实 STC 服务、独立账号 `localqa`，修复后整套执行 **14 例全部通过，14 例清理全部确认**。报告目录为 `runtime/ui-test-client/ui-test-reports/20260930-113051-519/`，包含 JUnit、每步截图和请求/应答。没有执行性能测试，不据此声明远程 QA 通过。

| 原失败用例 | 原因和修复 | 实测结果 |
| --- | --- | --- |
| `08-market-filters` | 防抖等待后丢失 WPF 同步上下文，后台线程更新绑定数据；保留 UI 上下文并明确取消源的释放归属 | 行情中心和报价管理价格范围、重置、文本搜索均通过 |
| `13-cd-name-volume-before-yield` | 本地资料中该债券 09-22 摘牌、09-23 到期，被静默排除；另有 `2000w 2` 整数价格歧义和 STC 明天清算字典误映射 | 按用户确认保留过期债券预览并显示警告；原文逐字段及警告可见性断言全部通过，无报价写入 |

另通过解析核心 222 项、服务合同 13 项及本机 JDBC 只读检查、客户端协议/WPF 隔离检查 31 项。ParserSvr 已部署并重启，正常 STC 客户端已重建。首次未修复报告 `20260930-111719-803/` 为 12 通过、2 失败，保留作为缺陷证据。其余通知类型、真实 QTrade、全部筛选字段和并发仍按覆盖边界说明，不计入本轮通过数。

### 2026-09-30 下午：精确报价追加回归

完整 17 例全部通过，清理全部确认；报告 `runtime/ui-test-client/ui-test-reports/20260930-150658-885/`。新增 15–17 例在管理窗口中可独立运行、编辑。解析核心 223 项与客户端隔离检查 31 项通过。ParserSvr 新 JAR 已部署重启，正常客户端已重建，插件 DLL 哈希与测试客户端一致。

截图原文 `199385 3 1000 +1\r\n250210 2 2000 +0` 已验证为两行：收益率 3 / 报量 1000 / T+1，以及收益率 2 / 报量 2000 / T+0。真实 Enter 键事件在正式 TextBox 插入换行，未自动触发识别。批量工具栏无选择时提示；选中行可调整方向、数量、价格、清算和计价，未选行不变。键盘测试为 WPF 路由键事件，未宣称覆盖操作系统输入法或焦点遮挡。

### QTrade 原始消息分组回归

独立的有限功能检查 `scripts/test-qtrade-grouping.ps1` 使用 QuoteSvr 的 `QtIntakeSmoke` 在临时 PostgreSQL schema 中生成的三条报价 JSON，经过正式 DPS 映射器和 WPF 报价确认页面，检查两条同源报价一个组、另一消息单独一组、组复选框、迟到成员、刷新后选中/折叠状态、部分确认及空组移除。测试服务替身记录确认/删除调用，不连接真实 QTrade，也不声称完成真实 FIX 到 UI 的端到端验证。独立进程有 30 秒 watchdog。

先构建 QuoteSvr 和 STC 客户端。运行 `ficc.quote.QtIntakeSmoke <本机数据库配置路径> <输出JSON路径>`，第二个参数生成客户端测试夹具；此程序只使用独立随机 schema，完成后删除该测试 schema。再执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/test-qtrade-grouping.ps1 -OutputRoot <已构建客户端目录> -QuoteFixture <输出JSON路径> -ClientConfig <实际STC.exe.config>
```

`ClientConfig` 只提供实际客户端的程序集绑定配置，不将账号配置复制进测试程序。日志为输出目录下的 `QTradeGroupingChecks.stdout.log` / `.stderr.log`。该检查独立于管理窗口的三条业务用例，尚未登记到窗口中。

## 隔离、断言与清理

每个用例启动独立的 STC 应用进程，使用独立的客户端状态目录，自动登录。业务写入通过正式界面的输入控件及按钮事件完成；测试不会直接调用 ViewModel 命令替代按钮，也不会直接改 `IsSelected` 来绕开 checkbox 绑定。

工作进程托管正式 `STC.App` 和同一套产品程序集，操作 WPF 控件的选中/点击事件。它验证 UI 绑定、业务处理、DPS 请求/推送及真实服务持久化；不是逐像素鼠标回放，对操作系统焦点、遮挡或拖拽的体验检查不在本轮覆盖范围。

服务端查询用于独立核对写入结果、历史以及清理，不代替业务操作。通知用例另通过 API 准备本例独立关注分组和偏好，保存原偏好到报告，结束删除分组并恢复偏好；报价创建/确认、通知标记已读仍通过正式 UI。数据库证据来自真实服务的持久化读取和历史 API，不额外直连 SQL 验证。

每例自己创建报价，自动记录 `owned-quotes.json`。结束或失败后，仅撤销/删除本例报价，并核对删除状态。软删除历史保留，不清空数据库，不删除其他用户数据；因此断言依据本例报价 ID，不依赖全市场固定总条数。服务拒绝或数据不符合预期即失败，不临时伪造预期。

套件有互斥锁，不允许多个实例同时运行。每个工作进程有独立超时监控；取消后最多等待 30 秒进行清理，超时只结束本套件启动的工作进程。若强制终止、进程崩溃或清理失败，报告标记清理未确认并停止后续用例，需检查本例 ID 和请求日志，不标记为通过。

## 报告

`runtime/ui-test-client/ui-test-reports/<时间>/`：

| 文件 | 内容 |
| --- | --- |
| `summary.json` / `junit.xml` | 套件结果，JUnit 格式可供 Jenkins 归档 |
| `suite-cases.json` | 本轮实际执行的用例快照，后续编辑不改变历史证据 |
| `<caseId>/events.jsonl` | 实时步骤状态、耗时、失败原因及清理结果 |
| `<caseId>/result.json` | 单例结果和本例报价 ID |
| `<caseId>/*.png` | 成功步骤及失败现场的窗口截图 |
| `<caseId>/logs/`、`console.log` | 本轮 STC 日志与标准输出，用于追踪 DPS 输入/输出 |
| `<caseId>/*-quote.json`、`history.json` | 实际服务端读回结果 |
| `<caseId>/*-parse.json`、`*-sidebar.json` | 识别原文及字段、筛选期望和实际 ID/加载状态，失败时也保留 |
| `<caseId>/owned-watchlist.json`、`original-notification-preference.json`、`notification-cleanup.json` | 提醒夹具账本与偏好恢复证据 |

无需 GUI 的执行入口（默认 `test2/123456`、操作间隔 2000ms；可用当前进程环境变量覆盖账户密码）：

```powershell
$env:STC_UI_USER = '专用测试账号'
$env:STC_UI_PASSWORD = '测试密码'
$env:STC_UI_SECURITY = '240210.IB'
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/run-ui-tests.ps1 -Cases all
```

`-Cases 01-quote-lifecycle` 运行单例；多个 ID 以逗号分隔。退出码 0 表示全部通过，1 表示用例失败/取消，2 表示配置或运行器错误。新增现有动作组合无需编译；新增动作种类需修改测试代码并构建，不向正式产品添加业务绕过路径。

## 组合筛选延迟测量

独立性能用例、执行命令及 500 报价首轮测量结果见[报价组合筛选端到端延迟](filter-latency.zh-CN.md)。不在默认功能套件中执行；每次需用户明确授权。


## 报价确认工具栏回归

管理器用例 `21-pending-toolbar` 创建两条独立待确认报价，依次验证删除、Bid/Ofr、价格调整、价格类型、清算速度、报量、机构/交易员搜索及五个旗标。通过真实控件操作、服务端读回和未选报价完整对比，核对编辑范围与确认后的持久化；失败同样清理本例报价。`pending-toolbar-checks.json` 保存逐项检查结果，CRM失败额外保存搜索状态。

报价确认工具栏的数值语义与产品原型一致：按钮每次调整 ±1BP / ±1000万；输入框可输入有符号增量，按 Enter 应用。收益率按百分数展示，1BP对应0.01；净价/全价按原型使用价格单位增量。换一组勾选行后可重新应用同一个下拉选项；混合旗标统一开启，再次点击统一关闭；OCO与打包互斥。只对勾选报价生效。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/run-ui-tests.ps1 -Cases 21-pending-toolbar
```

2026-10-13 已在本地真实 STC/GW/QuoteSvr/CRM 链路验证；本轮范围为报价确认工具栏，不代表所有页面或 QA 环境已完成回归。

## 用例多选与运行范围

用例列表支持 Ctrl/Shift 多行高亮，按空格批量勾选或取消这些行；也可逐个点击「运行」列复选框，勾选立即提交。「全选」同时勾选并高亮全部用例，「取消全选」清除勾选与高亮。列表上方显示「已勾选 N / 总数」；「运行勾选」以复选框为准，不以编辑时的行高亮为准。未勾选任何用例时只提示，不启动测试。运行中锁定用例选择，保持本轮用例快照不变。

可运行 `scripts/test-ui-runner-selection.ps1` 检查真实管理窗口的全选/取消、多行勾选、空格切换、复选框内容点击即时提交以及空选择保护。该检查不启动业务测试，有 15 秒看门狗。更新管理器程序后须重新打开管理窗口；已运行窗口继续使用原版本。

## 运行中调整操作间隔

管理窗口顶部新增「操作间隔（秒）」，范围 0–60，精度 0.1 秒，默认 2 秒（可改为 0 取消额外等待）。运行中仍可调整；调小或调大都会影响当前等待，约 100ms 内读取新值，已经等待的时间不会清零。点击「停止并清理」会中断等待并进入原有清理流程。

间隔应用于每个用例中相邻的 JSON `steps` 动作之间；首步、登录和清理不额外等待。`groups.manage`、`pending.toolbar` 等复合动作内部的多个点击仍作为一个动作执行，不逐次插入间隔。网络超时、页面就绪轮询不随此设置改变；性能计时在动作开始后启动，不包含动作前的人工间隔。

执行日志会记录 `INTERVAL_WAIT` / `INTERVAL_DONE` 和实际 `waitMillis`；每例报告目录的 `operation-interval-ms.txt` 是窗口与工作进程之间的实时控制文件。看门狗只扣除这段有界的人工间隔，业务执行超时和停止后 30 秒清理宽限期保持不变。关闭并重新打开管理窗口时恢复默认 2 秒。

命令行可指定初始间隔，例如：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/run-ui-tests.ps1 -Cases 22-account-groups -OperationIntervalMilliseconds 1000
```

单动作复合用例没有相邻步骤，因此不会增加等待；观察逐步流程时应选择多步骤用例。无业务写入的间隔控制回归检查：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/test-ui-operation-interval.ps1
```

## 自动处理用户使用须知

自动化工作进程每 200ms 检查自身 WPF 窗口，只处理 `STC.Frame.CustomeUI.UserNoticeView`：点击「已完成阅读，并确认接受用户使用须知(条款)」后点击「确定」，在事件报告记录 `用户使用须知 / DISMISSED`。直接点 X 会触发 STC 主窗口退出，因此测试使用正常确认流程。普通客户端未修改；其他确认、错误提示及同标题窗口不自动关闭。控件缺失或确认失败会记录 FAILED 并让用例失败。

处理器在登录前启动，覆盖迟到弹窗和测试期间再次出现的须知。它不受操作间隔影响。二进制替换只对随后启动的工作进程生效，已经运行的用例仍使用加载时版本。

可运行 `scripts/test-ui-user-notice.ps1` 检查真实 WPF 须知弹窗：连续两次模态弹窗均确认关闭、同标题的普通窗口保留；独立进程有 15 秒看门狗。此检查不登录或提交业务报价，不等于整套业务 E2E 回归。

## 演示字幕

2026-10-13 移除客户端顶栏「手工报价」及临时 `QuoteOperationWindow`。自动化不再提供 `manual.*` 动作。新增报价统一通过精确报价的文本识别、预览勾选及「确认报价」，识别阶段不落入待确认池；修改从报价管理「修改 / 历史」打开产品编辑表单，保存成功关闭；Refer、Unrefer、删除通过报价管理工具栏。

`fixture.pending` 只用于准备待确认推送/工具栏测试数据，字幕和报告标记「API准备待确认夹具（非UI录入）」，不声称完成 FIX 接入或UI创建。编辑表单之外的后续操作先关闭编辑表单；正常等待、截图、业务核对与清理机制保留。管理器新增 `management.edit/save/reject/operate` 动作，可按正常工作流组合用例。旧 `test-local-integration.ps1 -Merged/-SalesTrading` 和 `test-parser-live.ps1` 已改为调用管理套件，要求 STC 模式配置；结果改用套件 JSON/JUnit 报告，独立旧检查源码不再保留。旧解析检查的双边价格、分号、方向标题和中文报量迁入 `23-complex-parser`。

自动化工作进程在窗口下方显示中文字幕：用例名称、步骤序号、当前动作和下一动作。操作间隔内显示「下一步」，动作开始时显示「正在执行」，登录和清理也有提示。字幕不接收鼠标、不抢键盘焦点，随普通测试窗口移动；仅自动化工作进程启用，正式客户端不变。

字幕取自用例 JSON `steps[].name`，可在管理器的用例编辑功能中修改。每个配置步骤对应一条字幕；`pending.toolbar`、`groups.manage` 等复合动作内部的点击目前共用该步骤字幕。演示时建议把「操作间隔（秒）」设为 1–2 秒，运行中可调整；默认为 2 秒。

字幕在动作计时开始前完成渲染。`filter.latency` 测量动作期间隐藏字幕并停止位置跟踪定时器，避免增加测量期间的渲染工作；普通用例截图只捕获 STC 窗口，不包含独立字幕窗口。

可运行 `scripts/test-ui-demo-caption.ps1`：使用独立 WPF 窗口验证字幕内容、焦点保留、鼠标穿透窗口样式、跟随移动及隐藏恢复，并生成 `runtime/demo-caption-check/demo-caption.png`。检查带 15 秒看门狗，不登录业务服务，不代表完整业务 E2E 已通过。更新工作进程后，随后启动的用例使用新字幕，已运行的用例保持旧版本。

## 2026-10-13 产品工作流迁移验证

本机真实 STC/GW/ParserSvr/QuoteSvr/CRM/NotificationSvr 链路共验证 12 个不同用例：01、03–08、14、20–23，全部通过并确认清理；21 额外复跑，验证产品编辑表单保留 CRM ID/报价旗标、OCO互斥拒绝以及变更历史。管理器默认值/多选专项检查通过。没有执行性能测试；18 双边议价用例本轮未运行（进程未提供 intake 测试 token），不据此声明 QA 环境或完整 FIX 链路通过。

报告位于 `runtime/ui-test-normal-workflow/ui-test-reports/`：`20261013-183849`（生命周期）、`20261013-184004`（10例）、`20261013-184430`（编辑表单扩展核对）、`20261013-184650`（复杂解析）。
