# 集成客户端 UI 自动化迁移与验收

## 目标与边界

本轮将原销售交易 UI 自动化接到 `integration/ficc-client-convergence` 的候选插件；保留业务场景与断言，适配新控件、ViewModel 和网关结构。**测试工具可编译运行不等于候选产品通过业务验收。失败用例仍是合并门禁。**

原 QA 客户端未替换，同事 checkout 未修改，Java 服务代码与部署配置未改动。测试连接本机真实 GW、LoginSvr、BaseSvr、ParserSvr、CRMSvr、QuoteSvr、NotificationSvr，不使用模拟报价服务。

## 运行方法

在 `iteration1/stc-client/stc-client` 中执行：

```powershell
# 隔离的候选客户端与测试运行目录，避免覆盖当前 QA 安装。
$native = 'D:/workspace/greatewall/ficc/qa_env/client-integration-build'
$tests = 'D:/workspace/greatewall/ficc/qa_env/client-integration-ui'
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-native-client.ps1 -OutputRoot $native
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-native-client.ps1 -Mode Stc -PrepareOnly -OutputRoot $native -StcConfig D:/workspace/greatewall/ficc/qa_env/stc-client/STC.exe.config
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-ui-tests.ps1 -NativeRoot $native -OutputRoot $tests
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-ui-tests.ps1 -RuntimeRoot $tests
```

管理器默认 `test2`，操作间隔 2 秒，支持多选、全选、页面树、浮动速度控制、暂停、单步、当前动作字幕及单元格红框。高级模式在管理器中填写不同的观察账号。

命令行执行指定用例：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/run-ui-tests.ps1 -RuntimeRoot $tests -Cases 01-quote-lifecycle,09-multiline-partial-confirm -OperationIntervalMilliseconds 2000
```

双账号命令行增加 `-Advanced`，并使用现有 `STC_UI_OBSERVER_USER` / `STC_UI_OBSERVER_PASSWORD` 传递观察账号。不要把凭据放进测试报告或命令行参数。外部 FIX 发布用例另需有效 QT 会话、登录用户的 `inf1` 映射及用例中已核实的发送人/测试群。

## 驱动设计

- 产品操作仍经真实 XAML 控件、键盘事件、按钮命令；API 用于独立核对持久化结果、权限和本例数据清理。
- `UiClientAdapter.cs` 封装新客户端结构差异。CRM 定位使用 `LazySearchComboBox`；编辑操作通过今日报价表双击；分组弹窗使用异步派发点击，避免模态 `ShowDialog` 阻塞测试自身。
- `Contracts/` 是来自稳定基线的测试专用 DTO，不恢复旧产品服务，也不编入插件。详见该目录 README。
- 缺少页签、按钮、机构信息或业务效果时失败，不能通过删断言、填假数据或调用 API 代替用户操作放行。
- 仅识别后的“未进入待确认池”校验使用当前用户的真实未过滤待确认视图；结果超过单页上限即失败，避免不完整快照造成假通过。此校验不代表过滤查询已通过。
- `60-precise-crm-dropdown` 单独验证空机构下的交易员候选机构信息与选择回填；不依赖外部 QT 送入待确认。

## 进程与清理

每例独立登录和运行；SuiteRunner 另进程看门狗限制运行时间，保留虚拟化。正常清理仅处理本例记录的报价、测试分组及偏好快照；清理未确认时停止后续用例。

2026-10-18 的首次分组测试因旧点击方式等待模态窗口返回而被看门狗终止。原失败报告保留。已增加报价恢复入口，独立登录并验证每个 ID 属于当前账号，仅处理原 `owned-quotes.json` 中的报价：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/recover-ui-test-quotes.ps1 -TestRoot $tests -CaseReport '<原单例报告目录>'
```

该入口不重跑业务步骤，不覆盖原失败结论，也不声称恢复进程终止时丢失的分组/偏好快照。非默认测试用户使用相同账号运行；恢复时测试运行器必须已退出。

## 本轮修复

- Refer、Unrefer、删除使用专用身份/版本 DTO，避免复用编辑模型发送 `yieldType:null` 等非法字段；新增实际 Gateway 请求序列化检查。
- 未持久化的 Bid 识别行原来因为没有 quoteId 而方向为空，现在根据有效的 Side 显示。
- 今日报价多选同步读取完整 SelectedItems，保留 Ctrl 多选。保存修改后 Refer 使用旧 revision 的问题已被端到端测试复现，仍是产品缺陷；简单重查会命中旧缓存，不能作为修复。
- 红框焦点追踪不再用窗口/整页焦点覆盖当前明确的验证目标。
- 动作等待自检适配重载的 CRM 文本输入方法；最优报量、提醒 ID 和撤销状态断言映射到新模型字段。

## 验证记录

详细结果见 [验收记录](client-convergence-ui-results.zh-CN.md)。桌面报告在 `D:/workspace/greatewall/ficc/qa_env/client-integration-ui/reports/`；每例包含 JSON 结果、步骤日志、截图及客户端日志。

功能检查、桌面真实服务测试、双账号观察、外部 FIX、性能测试分别记录。没有授权或前置条件不满足的测试不能标成通过；本轮不做性能压测，不把旧性能埋点的测试侧数据冒充产品分段延迟。
