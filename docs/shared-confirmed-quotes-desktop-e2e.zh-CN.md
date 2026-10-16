# 已确认报价共享：桌面端到端回归

日期：2026-10-16。分支：`feature/ficc-shared-confirmed-quotes`。业务实现基线：`ef7338f`。

结论：10 个桌面用例全部通过，全部清理成功。其中 9 个是双账号用例，操作方及观察方均通过；另 1 个是双边议价更新的桌面确认回归。未发现本轮断言失败，不代表全部产品功能已验证。

## 环境和边界

- 使用项目的 `SalesTradingUiTests` / `UiScenarioWorker`，真实 STC 登录、WPF 控件、GW、QuoteSvr、NotificationSvr 和 PostgreSQL；不是模拟客户端。
- 操作账号 `test2`，观察账号 `adrian`。每例独立登录、两账号同时运行；用例之间串行。操作/验证展示间隔为 2 秒，管理器使用既有独立进程看门狗。
- 本次已构建、部署并重启本机 QuoteSvr、NotificationSvr；ParserSvr 原本停止，已启动用于精确报价识别。未修改 QA 服务器。
- 普通路径经精确报价识别后在 UI 确认。QT 路径通过真实 GW 的 QuoteSvr 接入接口准备 `PENDING_CONFIRM`，再在报价确认页点击确认；不发送外部 FIX，不把该检查报告为腾讯端到端验证。
- 观察账号始终停留目标页面，不点击刷新。操作账号通过 UI 依次确认、Refer、Unrefer、再次 Refer、删除。
- 每例清理自身创建的报价；提醒用例恢复自身使用的提醒设置。未运行性能压测。

## 断言

1. 精确报价预览不进入其他账号的待确认页。
2. QT 输入只在所属账号的待确认页出现；另一账号逐笔读取返回 `1002`。
3. 确认后，另一账号能在完整行情、最优行情及今日报价中收到更新。
4. Refer 后退出有效行情，但在今日报价管理中仍显示撤销状态；Unrefer 恢复，删除退出今日列表。
5. 共享读取不授予写权限：观察账号通过真实 GW 请求 Refer，逐条返回 `1003`，报价状态和版本不变。该权限断言是 API 检查，不冒称点击了观察方的按钮。
6. 每阶段同时核对页面数据和真实 DataGrid 行，截图保存验证目标；负向断言保留观察等待窗口。

## 用例和证据

初始用例 `47-realtime-today` 的操作方、观察方及清理均通过。报告位于：

`iteration1/runtime/shared-quotes-ui/reports/shared-live-20261016-173312/`

后续套件包括 `42`、`43`、`46`、`48`，以及本次新增的 `55`—`58`：

| 用例 | 业务路径 | 观察页面 |
| --- | --- | --- |
| 42 | 精确报价 | 利率全部 |
| 43 | 精确报价 | 利率最优 |
| 46 | 精确报价 | 报价提醒 |
| 48 | 精确报价 | 报价确认隔离 |
| 55 | QT 接入、UI 确认 | 今日报价 |
| 56 | QT 接入、UI 确认 | 利率全部 |
| 57 | QT 接入、UI 确认 | 利率最优 |
| 58 | QT 接入、UI 确认 | 报价确认隔离 |

套件报告目录：`iteration1/runtime/shared-quotes-ui/reports/shared-suite-20261016-173544/`。8 个用例及其观察方均为 PASSED，`cleanupPassed=true`。证据包括 `summary.json`、每例 `result.json`、`observer/result.json` 及阶段截图。

另运行 `18-qtrade-negotiation`，结果 PASSED，清理成功。确认前新提案及其更新不改变既有正式价格；UI 确认后仅替换匹配会话的旧价，另一会话保持不变。报告目录：`iteration1/runtime/shared-quotes-ui/reports/negotiation-live-20261016-174528/`。

新用例位于 `iteration1/stc-client/stc-client/tests/ui/cases/`。通过 `autotest.cmd start` 打开管理器，启用双账号高级模式，选择“05 高级测试/02 QT确认后共享”。接入凭据自动读取所选环境部署生成的 `QtIntakeSvrAuth.properties`。`realtime.lifecycle` 新增可选参数 `source=QT`；未设置时保留原精确报价路径。

已重新构建正常入口使用的 `runtime/ui-test-client`，新增用例可以直接从管理器选择。服务保留本次部署版本；测试工作进程已退出。

## 已知限制

- 截图中部分其他归属人的“长城交易员”显示原始 userid：宿主用户目录缺少显示名称时采用现有回退行为。共享可见性通过不代表此显示问题已解决。
- 本轮未验证外部 QTrade FIX 收发、全部 CRM 元数据组合、桌面历史查询或性能指标。历史共享及删除前是否确认的隐私规则已有 PostgreSQL 组件回归，不能替代桌面历史查询验证。
