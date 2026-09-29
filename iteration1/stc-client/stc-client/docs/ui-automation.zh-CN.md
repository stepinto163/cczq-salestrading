# STC UI 端到端测试管理

这是独立的测试管理窗口，业务界面复用正式 STC 客户端。测试连接真实 GW、LoginSvr、BaseSvr、CRM、ParserSvr、QuoteSvr、NotificationSvr 及其数据库；不使用模拟报价服务。

## 启动

先启动本机 STC/FICC 后端。从仓库根目录执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/start-ui-tests.ps1 -Build
```

首次需要先构建 STC 客户端（`build-native-client.ps1`）。测试构建输出到 `runtime/ui-test-client`，不会覆盖正在运行的日常客户端。只启动已构建的测试管理窗口时，去掉 `-Build`。

选择环境的 `STC.exe.config`，输入**专用测试账号**、密码及债券代码。此版本要求 `Ficc.IntegrationMode=stc`，不把本机模拟模式的通过结果冒充真实服务验证。密码只传给本轮子进程，不写入用例文件。

## 管理用例

- 新增、编辑、复制、删除用例；用例 ID 保存后固定。
- 编辑器中增加、删除、上下移动步骤，从动作下拉框选择操作，在参数表中填写值，无需编辑 JSON。
- 勾选一个或多个用例后点击“运行选中”；按列表顺序串行执行。
- 下方实时显示登录、业务步骤、清理及最终结果；选中记录查看完整错误。
- “停止并清理”请求停止当前用例，再执行清理；不会结束用户自己打开的 STC。
- 用例保存在 [`tests/ui/cases`](../tests/ui/cases)，可随 Git 同步、评审；删除用例不删除历史报告。
- “打开本轮报告”和“历史报告”可访问结果、截图、日志及本轮用例快照。

## 当前覆盖

| 用例 | 实际操作及断言 |
| --- | --- |
| 手工报价完整生命周期 | 登录、打开手工报价、填写控件、保存待确认、确认、修改、Refer、Unrefer、删除，核对后端状态/价格及连续历史 |
| 识别机构交易员、勾选及确认报价 | 输入真实 CRM 文本、ParserSvr 识别、核对机构/联系人 ID、勾选/取消/重新勾选、确认，核对服务端有效报价及报价/行情页面推送 |
| 待确认报价实时展示 | 手工创建待确认报价，核对报价确认页面收到并显示该报价 |

CRM 用例默认要求 `长城证券 / 田老哥 / 240210.IB` 存在，文本及期望值均可编辑。`${securityId}` 使用运行窗口填写的债券代码；自然语言用例默认采用已验证的固定文本，切换债券时请同时编辑文本和期望字段。

**覆盖边界：**这是第一组真实端到端回归，不代表所有产品功能都已覆盖。四类通知、多用户并发、完整条件筛选、滚动浏览以及 QtIntake 后续分别添加用例。未实现的动作不会显示为已通过；性能/压力测试不包含在本套件中。

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

服务端查询只用于独立核对写入结果、历史以及清理，不代替业务操作。数据库证据来自真实 QuoteSvr 的持久化读取和历史 API；此版本不额外直连 SQL 验证。

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

无需 GUI 的执行入口（账户密码通过当前进程环境变量传入）：

```powershell
$env:STC_UI_USER = '专用测试账号'
$env:STC_UI_PASSWORD = '测试密码'
$env:STC_UI_SECURITY = '240210.IB'
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/run-ui-tests.ps1 -Cases all
```

`-Cases 01-quote-lifecycle` 运行单例；多个 ID 以逗号分隔。退出码 0 表示全部通过，1 表示用例失败/取消，2 表示配置或运行器错误。新增现有动作组合无需编译；新增动作种类需修改测试代码并构建，不向正式产品添加业务绕过路径。
