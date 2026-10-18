# STC 销售交易客户端集成计划与验收记录

> 2026-10-18 后续更新：UI 驱动已迁移并运行真实桌面；24 项功能检查和 7 项工具自检通过，但业务验收仍有失败。详见 [自动化说明](client-convergence-ui-automation.zh-CN.md) 与 [验收记录](client-convergence-ui-results.zh-CN.md)。以下为初次导入阶段记录，端口未就绪和驱动未适配等描述保留为历史，不代表当前状态。

集成目标是收敛为一套客户端：以同事的 UI 和 MVVM 分层作为候选结构，保留已确认的业务规则、后端协议和可复现的回归场景。当前分支是集成候选，不是 QA 验收版本，也没有替换正在使用的 QA 客户端。

## 基线与分支

- 集成分支：`integration/ficc-client-convergence`。
- 原工作版本：`feature/ficc-shared-confirmed-quotes`，提交 `dc929bd284d9daf95890c7fa61342512ef73e82c`。
- 同事版本：`dev_sales_trading_backend`，提交 `de8da9162725406373212771bb3f56f8ab1af91d`。
- 同事源码位置：`D:/workspace/greatewall/stc-client-colleague/stc-client`。
- 本轮导入范围：同事提交中跟踪的 `STC.SalesTrading.Plugin`，共 325 个文件；以 Git 历史保留原插件实现，移除候选工程中已被替代的旧文件。
- 同事目录中的未跟踪旧 `Ficc/`、Mock、旧测试及运行产物没有作为新实现导入。原工作仓库的测试场景文件保留，驱动尚待适配。
- STC 宿主、STC.Common、STC.Frame、Java 服务、数据库结构和部署配置未改动；同事仓库未修改。

## 开发边界

1. 每个业务工作流只指定一名实现负责人；不要在两套客户端同时开发同一新增功能。
2. 同事负责 UI、XAML 和控件交互；Codex 负责协议兼容、业务规则评审及回归场景。跨边界改动共同评审，不将角色划分当作已派发任务。
3. 从集成分支建立小任务分支。提交说明列出改变的行为、涉及的协议、已运行和未运行的检查。
4. 两套客户端共享场景与期望结果，控件定位和操作封装由各自驱动实现。不要为通过测试修改业务断言。
5. 当前工作版本继续作为 QA 基线；只有完成对应工作流验收后才向其合并。功能测试、桌面端到端测试和性能测试分别记录。

## 阶段与验收门槛

| 阶段 | 工作 | 当前状态与验收门槛 |
|---|---|---|
| 1 基线 | 固定两端提交、限定导入范围、建立隔离构建 | 已完成；同事插件可与现有宿主共同编译 |
| 2 结构 | 统一 DTO、Gateway、服务、ViewModel 边界 | 候选结构已导入；所有功能尚未完成等价性审查 |
| 3 基础缺陷 | 批量回执、快照与推送、订阅释放、资料回填 | 已修复下列缺陷；22 个确定性功能检查通过 |
| 4 报价生命周期 | 精确报价、QT 待确认、CRM、确认/修改/Refer/Unrefer/删除、业务键去重 | 尚未完成真实桌面与双账号验收 |
| 5 行情 | 全部/最优、价格时间排序、归属和对手、组合筛选、实时推送 | 排序和资料映射已有局部测试；全页面和双账号实时验证待做 |
| 6 其余页面 | 关注债券、筛选分组、提醒、推荐、历史查询 | 待业务等价性审查和自动化驱动迁移 |
| 7 QA 收敛 | QA 包、回归、缺陷关闭、主分支合并 | 未开始；不得把候选构建当作已验收交付 |

## 本轮修复

- 精确报价空机构/联系人统一为空值；不再因空字符串跳过确认后仍提示成功。仅文字未选 CRM ID 的行在创建前报错。
- 批量创建/确认逐条核对身份、状态和新版本。成功行保留 ID，失败行留在预览中；重试不重新创建已成功的行。
- 通信结果未知时保留原请求内容和 requestId，用户再次提交先获取同一操作的回执。未增加后台自动重试次数或间隔。
- Quote、Price、Notification 三域在快照请求期间缓冲本视图推送，按应答 sequence 回放。旧快照显式失败，不覆盖较新的状态；关闭后迟到的应答不能重新填充视图。
- 缓冲推送回放不再次经过流去重；缺号使订阅失效并允许重建；释放中的 stream.open 不再留下新订阅。
- 参考资料缓存保存记录本身，每个新行均回填；跨全部/最优页复用，日期切换清空。超过 100 个证券分批查询，缺失资料不会被误记为已取得。
- 行情全部保留后端的券内方向、可比收益率和时间顺序；最优同价明细按最新报价排序，使用各条报价自己的对手机构、交易员及时间。
- Enter 保留多行输入，Ctrl+Enter 或识别按钮触发识别。ParserSvr 失败直接呈现错误，不再退回本地简析结果。

## 首个端到端里程碑

账号 A 输入精确报价 → 识别 → 选择 CRM 交易员并回填机构 → 确认 → 报价管理出现 ACTIVE → 账号 B 在行情页不刷新收到更新。随后 A 执行 Refer、Unrefer、删除，B 核对对应实时变化。

必须覆盖：空对手、未解析对手、同批部分失败、请求超时后重试、首次加载和翻页期间推送、重连、同价不同时间、不同 NegotiationID 相同完整业务键、任一对手 ID 缺失时不合并。

### 不能跳过的剩余门槛

- 现有后端协议仍为 create 后 confirm 两次操作。用户点击确认前仅有本地预览；但 create 成功而 confirm 失败或进程退出时，服务端仍可能留下 PENDING_CONFIRM。本轮没有用 UI 隐藏该事实，也没有宣称实现原子创建并确认。严格保证“不进入待确认池”需要单独完成后端事务语义及兼容性设计。
- 原 UI 自动化工作进程依赖旧 `Ficc` 类型和控件结构，不能直接证明新插件通过全部旧场景。下一项实现是按业务动作抽取适配器，先迁移精确报价/确认/双账号观察场景，再迁移其余页面。
- 同事实现中的模糊报价等未支持路径、报价录入弹窗及全部页面功能，需要逐项与产品需求及当前版本对照；候选架构导入不代表这些行为已被接受。
- 资料缓存目前按服务实例和日期复用；盘中基础资料主动更新及重连刷新仍需结合真实资料推送验证。

## 实际验证

2026-10-18 在本机执行：

- .NET Framework 4.7.2 完整源码构建通过：common.netapi、STC.Common、STC.Frame、STC.SalesTrading.Plugin、STC。
- 22 项功能检查通过，使用本轮编译的实际插件 DLL，只替换 DPS 传输以精确控制响应次序。包含实际精确报价 ViewModel 命令；不是桌面登录或网络端到端测试。
- 功能检查由独立进程运行，30 秒看门狗；没有执行性能压测。
- 构建和日志目录：`D:/workspace/greatewall/ficc/qa_env/client-integration-build`。
- 本机真实服务未就绪：检查时 GW/QuoteSvr/NotificationSvr/CRMSvr 等预期端口未监听；`show all` 因 GW PID 文件中的 35396 实际属于 OneDrive.Sync.Service.exe 而拒绝继续。没有停止该无关进程，也没有修改 PID 文件。
- 因此真实 STC 登录、CRM 联动、双账号实时更新、外部 FIX 和 QA 验收均未通过本轮验证。

从客户端源码根目录运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-native-client.ps1 -OutputRoot D:/workspace/greatewall/ficc/qa_env/client-integration-build
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-client-integration.ps1 -BuildRoot D:/workspace/greatewall/ficc/qa_env/client-integration-build
```

`tests/integration/Program.cs` 是新增检查入口；每个检查打印 PASS/FAIL，最后汇总数量，任何断言失败或看门狗超时使命令失败。测试不连接真实数据库，不发送 QT 报价，不修改其他人的报价。
