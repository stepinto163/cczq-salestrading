# 候选客户端 UI 自动化验收记录（2026-10-18）

## 2026-10-18 报价变更驱动提醒

- 修复同券聚合行成员变化导致未变报价误弹：新增 `triggers` 独立实例键；WATCHLIST/FILTER_GROUP/MY_BEST 按来源报价版本、COUNTER_QUOTE 按双方版本去重。合并展示行继续保留，不把“当前有效提醒列表”改成历史事件列表。
- “我的最优”要求同方向、同计价/收益率类型、同清算速度存在比较报价；单笔不触发。实例已读落在现有 notification_reads 表中，移除 A 不让已读 B 变回未读；同 ID 的旧已读行自动补齐独立实例记录。
- 客户端初次快照/手动刷新不弹存量；其他页签仍接收新提醒；移除八秒自动消失。弹窗价格、数量和归属用户来自具体触发报价摘要，摘要不含对手机构、联系人或备注。
- 服务端 `NotificationProtocolSmoke` 在独立临时 schema 通过 42 项检查并清理；common Maven 检查通过。客户端受控检查 36 项通过（包括重复事件、首快照、手动刷新、成员移除、已读实例去重）。
- 实际桌面：`14-watchlist-notification-read` 和新增 `62-notification-change-trigger` 均通过，均清理成功。62 经真实精确报价确认两笔同券报价，验证新建 B 不重弹 A、Refer A 不重弹 B、修改 B 只弹一次且价格为 2.5330/数量为 1、手动刷新不弹存量。报告：`qa_env/client-history-ui/reports/notification-final`。
- 本机 NotificationSvr/common 已部署、监听并完成上述真实业务验证；QA 未部署。QA 需配套更新 common + NotificationSvr + 客户端；无需 SQL 表结构迁移。QuoteSvr 业务代码未改。未运行性能测试、未声称全套自动化均重新通过。


## 2026-10-18 报价日志操作人显示登录名

- 操作人列改为显示 STC 登录名（`Username`），按审计 `operatorUserId` 在宿主 AdminSvr 用户缓存中匹配 `Userid`；当前登录会话补齐本人映射。保留原始 ID 用于审计和悬浮提示，不改服务端字段或数据库。
- 查不到映射时显示“未知用户”，不冒用当前查看者，也不把姓名 `Name` 当作登录名。
- 客户端检查 35 项通过，覆盖其他操作人、登录名与姓名区别、本人缓存缺失、未知和空 ID。
- 桌面用例 `61-quote-history-paging` 分别使用 test2、adrian 连接本机真实服务通过，均清理成功；adrian 运行还直接检查已渲染的操作人单元格文本。报告分别位于 `qa_env/client-history-ui/reports/history-operator` 和 `history-operator-adrian`。
- 仅客户端修改；无需部署 Java 服务或迁移数据库。QA 尚未部署。

## 2026-10-18 新增独立历史分页桌面用例

- 新增 `61-quote-history-paging`，测试管理器路径：`03 报价管理 / 03 变更历史`。通过精确报价识别、确认、修改一次生成一笔本例报价；自动清理。
- `history.assert` 以每页 1 条核对真实服务的首、次、末页；`history.dialog` 经行情行的报价日志命令打开真实窗口，对照接口校验记录数及最新版本、单页分页摘要、前后按钮禁用、刷新重新加载和关闭按钮。
- 实际桌面运行通过，耗时 12.19 秒，3 条历史记录，清理通过。报告：`qa_env/client-history-ui/reports/history-small/61-quote-history-paging`。本次验证操作间隔设为 100 ms，用户默认间隔不变。
- 这是小样本回归：真实服务窗口只有一页，没有把禁用按钮检查称为多页 UI 翻页验证；多页窗口导航仍由前述受控传输 WPF 测试覆盖。未修改业务代码，未进行性能测试。

## 2026-10-18 报价变更历史分页

- `ficc.quote.history.query` 新增 `pageNumber/pageSize/snapshotRevision`，按版本倒序返回 `records/totalCount/hasMore`。翻页保留版本上界，刷新重新取最新边界；显式 limit/cursor 的旧调用保持兼容。
- STC 报价日志复用 `PaginationBar`，默认每页 20 条；支持前后翻页、刷新、失败重试。请求失败时保留此前成功页；关闭窗口后取消请求。移除只加载前 100 条的截断。
- 验证：common Maven 66 项（65 通过、1 项原有跳过），QuoteSvr Maven 5 项通过；独立 PostgreSQL schema 的历史功能回归 159 项检查通过，覆盖 105 条审计记录、并发新增、首末页、空页、权限、无效参数和旧游标兼容。该测试不属于性能测试。
- 客户端 34 项检查通过，使用实际插件及受控 DPS 应答；其中 WPF 测试创建真实报价日志窗口，操作统一分页条的上一页/下一页并核对显示行，截图位于本机 `qa_env/client-history-ui/bin/quote-history-pagination.png`。
- 实际桌面用例 `01-quote-lifecycle` 通过，连接真实本机 STC/GW/QuoteSvr；`history.assert` 核对首/次/末页接口返回，清理通过。报告位于 `qa_env/client-history-ui/reports/history-paging`。此实际链路用例没有操作历史窗口；窗口控件测试使用受控传输，两者不得混称为历史窗口完整实时端到端验证。
- 发布：本机 QuoteSvr/common 已部署并重启，JAR 哈希与构建产物相同；源码 Debug 的 `client/plugin/STC.SalesTrading.Plugin.dll` 已构建安装。QA 环境未部署；QA 需先更新 common/QuoteSvr，再更新客户端，不涉及数据库结构变更。
- 另一次临时只读 API 调试器探测在登录阶段超时并由 30 秒看门狗终止，不计入通过项；随后采用现有桌面测试器完成上述真实服务验证。


## 第二轮修复与复验（当前结论）

分支：`integration/ficc-client-convergence`。本轮修复客户端和自动化驱动中无需新增业务决策的问题，未修改 Java 服务或对外协议，未合并稳定 QA 分支，未替换现有 QA 客户端。

### 已修复

1. 报价管理修改成功后主动读取最新版本，避免下一次 Refer 使用旧 revision；今日报价使用服务端 `VISIBLE` 权限范围，恢复已确认报价跨账号实时展示；修正查询第二页重复 Skip 导致漏行。
2. 管理侧栏接入筛选字典与服务端组合条件，避免分页后客户端过滤；防抖查询保留 UI 同步上下文；无报价方向的最优报量保持空值。历史查询延迟至首次打开历史页，避免登录时初始化日期触发无意义的历史查询。
3. CRM 联系人搜索按本页去重后的机构 ID 批量补齐机构名称；候选展示姓名与机构，选择后同步 ID 与名称；机构变化清除不相容联系人，输入文字不等同于已选联系人。行与工具栏共用该规则，工具栏只作用于勾选行。Down 键支持重新打开候选，并接收被 WPF TextBox 类处理器处理过的按键；旧搜索结果在输入/机构变化时作废。
4. 批量工具栏修正 BP 换算、直接报量输入及固定 ±1000 万调整；同步选中行的价格类型/清算速度，避免下拉默认值相同而无法应用；精确报价未勾选时提供明确提示。
5. 提醒页提供标记已读，服务器拒绝时不提前把本地状态设成已读。推荐页接回真实证据展开、反馈、刷新及对应字号；反馈仍调用既有服务端接口。
6. 关注/筛选分组回填服务端 ID、保存编辑副本版本，保留条件运算符与数值/布尔/数组类型；删除限定当前市场并传播失败；管理页显示和使用共享账户分组。已验证旧编辑副本产生 1004 冲突而不覆盖最新名称。

### 验证口径

- 实际插件构建通过；确定性检查 **32/32**（原 24 项 + 8 项新回归）。新增覆盖 CRM 机构补齐、选中行范围、分组类型/市场隔离/旧版本冲突、管理查询权限/刷新/分页、通知已读失败。
- 本轮实际执行 **29 个不同桌面用例：28 通过，1 环境前置阻断**。按各例最后一次结果计数，不把重试次数累加为覆盖数；这些结果分批取得，并非一次整套运行。全部 29 例均完成清理。
- 测试通过正式 STC 登录、真实 GW/Java 服务和可见 WPF 控件。18、55—59 使用接入接口夹具，**不是外部 FIX 交付验证**。21 在发布前因 `test2.inf1` 无有效 QT 映射而停止，没有发送外部消息。
- 每例独立进程看门狗，保留虚拟化；组合筛选使用六笔功能夹具。未执行性能压测、未自动增大负载。
- 工具自身的 7 项检查为首轮记录，本轮没有将其冒充重新执行。
- 隔离运行目录：`D:/workspace/greatewall/ficc/qa_env/client-integration-ui`；确定性日志：`integration-tests.log`。桌面复验后仅整理新增方法格式，并重新构建及执行 32 项确定性检查。最新插件 SHA256：`DD1C700DBF05676C8271CD30EBA409904C12AE9B7CA627057007707940CBE625`。

### 本轮桌面证据

报告根目录：`D:/workspace/greatewall/ficc/qa_env/client-integration-ui/reports/`。

| 用例 | 最后结果 | 报告子目录 |
|---|---|---|
| `01-quote-lifecycle` | 通过 | `repair-core-7/01-quote-lifecycle` |
| `06-invalid-price-recovery` | 通过 | `repair-core-7/06-invalid-price-recovery` |
| `07-management-filters` | 通过 | `repair-core-7/07-management-filters` |
| `08-market-filters` | 通过 | `repair-core-3/08-market-filters` |
| `14-watchlist-notification-read` | 通过 | `repair-core-6/14-watchlist-notification-read` |
| `17-precise-toolbar` | 通过 | `repair-core-6/17-precise-toolbar` |
| `18-qtrade-negotiation` | 通过 | `repair-core-8/18-qtrade-negotiation` |
| `20-smart-recommendation` | 通过 | `repair-core-6/20-smart-recommendation` |
| `21-pending-toolbar` | 前置阻断：test2 缺少 QT 映射（原报告 FAILED） | `repair-core-4/21-pending-toolbar` |
| `22-account-groups` | 通过 | `repair-core-6/22-account-groups` |
| `40-page-history` | 通过 | `repair-core-5/40-page-history` |
| `42-realtime-rateAll` | 通过 | `repair-core-5/42-realtime-rateAll` |
| `43-realtime-rateBest` | 通过 | `repair-core-5/43-realtime-rateBest` |
| `44-realtime-creditAll` | 通过 | `repair-core-5/44-realtime-creditAll` |
| `45-realtime-creditBest` | 通过 | `repair-core-5/45-realtime-creditBest` |
| `46-realtime-alert` | 通过 | `repair-core-5/46-realtime-alert` |
| `47-realtime-today` | 通过 | `repair-core-3/47-realtime-today` |
| `48-realtime-quote-confirm` | 通过 | `repair-core-5/48-realtime-quote-confirm` |
| `49-realtime-quote-precise` | 通过 | `repair-core-5/49-realtime-quote-precise` |
| `51-realtime-history` | 通过 | `repair-core-5/51-realtime-history` |
| `52-filter-combinations-today` | 通过 | `repair-core-5/52-filter-combinations-today` |
| `53-filter-combinations-rateAll` | 通过 | `repair-core-4/53-filter-combinations-rateAll` |
| `54-filter-combinations-rateBest` | 通过 | `repair-core-4/54-filter-combinations-rateBest` |
| `55-realtime-qt-today` | 通过 | `repair-core-6/55-realtime-qt-today` |
| `56-realtime-qt-rateAll` | 通过 | `repair-core-6/56-realtime-qt-rateAll` |
| `57-realtime-qt-rateBest` | 通过 | `repair-core-6/57-realtime-qt-rateBest` |
| `58-realtime-qt-quote-confirm` | 通过 | `repair-core-6/58-realtime-qt-quote-confirm` |
| `59-qtrade-business-key` | 通过 | `repair-core-8/59-qtrade-business-key` |
| `60-precise-crm-dropdown` | 通过 | `repair-core-8/60-precise-crm-dropdown` |

### 未解决与限制

- 模糊报价/意向页仍未恢复（27、32、37、50）；候选代码隐藏入口并声明后端域不支持。不能直接启用伪造预览模型来满足测试，需要明确迁移该域的协议与实现范围。
- 真实外部 FIX 发布/接收、21 外部 FIX 驱动的完整待确认工具栏仍未通过。工具栏本轮有确定性检查及 18/59 的真实桌面 CRM/确认覆盖，不能等同于 21 全流程。
- 首轮 QuoteSvr OOM、本轮 12:30 附近 ParserSvr 定时刷新 OOM 的根因未修复。恢复 ParserSvr 监听后完成本轮功能复验；延迟历史页首次加载只消除多余调用，不能据此宣称 OOM 已解决。没有调整 JVM 参数。
- 本轮修正两处测试适配：反馈状态按实际 `FeedbackText` 绑定定位，避免误选同文字的按钮内容；分组冲突断言接收候选插件的 `GatewayApiException.Code=1004`。未删除业务断言或将失败转为通过；增加 CRM/筛选超时诊断 JSON。

## 首轮历史记录（修复前，保留取证）

以下结果为首轮状态，已修复项目以本页第二轮结果为准。

## 结论

自动化驱动已适配候选插件并可编译、登录、操作真实桌面与核对真实后端。**业务验收未完成，禁止据此合并到稳定 QA 分支。** 本报告保留失败断言，不把隐藏页面、缺失控件或服务离线改成通过。

- 插件确定性检查：24/24 通过；只替换 DPS 传输以控制消息顺序，不代表真实网络验收。
- 自动化工具专项检查：7/7 通过，覆盖操作前等待、红框、当前字幕、速度/暂停/单步、管理器多选、使用须知关闭、虚拟化精确验证目标。
- 桌面按用例汇总见下表；同一例以指定报告中的最新有效记录为准，不将多次运行累加成覆盖数量。
- 外部 FIX 发布未执行；QtIntake 未启动。18、55—59 属于接入/去重链路，本轮未获得成功验收证据。没有执行性能压测。

## 环境与中断

隔离客户端：`D:/workspace/greatewall/ficc/qa_env/client-integration-ui`。原 QA 安装未替换，同事目录未改动。

`migration-acceptance` 19 例均遇到 QuoteSvr 离线，不能视为 19 个 UI 缺陷。`ats_root/log/QuoteSvr.console.log` 末尾记录 `Terminating due to java.lang.OutOfMemoryError: GC overhead limit exceeded`（12:16 附近），此前有历史查询大响应。该现象不构成堆泄漏根因分析；本轮未改 JVM 参数或 Java 代码。恢复 QuoteSvr 监听后仅重跑 01、20、47，仍有下表产品失败；没有再次执行整批历史查询。

首次 22 分组用例因模态点击阻塞被看门狗停止；已修正驱动，并按原清单恢复其唯一报价。恢复报告为 `migration-functional-final/22-account-groups/quote-recovery-20261018-121110`，原超时报告保留。随后 22 的正常运行与清理完成，但服务端分组 ID/版本断言失败。

## 主要合并阻断项

1. 报价修改后选中行仍持旧 revision，Refer 未生效；生命周期请求混入 `yieldType:null` 的独立问题已修复并新增协议检查，不能混为全部生命周期已修复。
2. CRM 候选无所属机构，自动回填尚未通过；已有解析器识别 CRM 用例通过不等于下拉选择通过。
3. 管理/行情筛选未产生预期结果；管理侧栏搜索输入未绑定。最优无报价方向的报量显示 0，与既有空值断言不一致。
4. 模糊报价/意向页签、提醒标记已读、推荐展开等旧验收入口缺失；工具栏无勾选提示缺失；分组保存后 UI 缓存未反映服务端 ID/版本。
5. 账号二的今日报价实时更新超时。双账号剩余动作、真实外部 FIX、编辑框属性/历史等后续断言未完成，不能因前置步骤失败而声称覆盖。

## 桌面结果

汇总：失败 16，通过 21，环境阻断 9，待重测 6。未出现在下表的用例未纳入本轮有效桌面结果。

报告根目录：`D:/workspace/greatewall/ficc/qa_env/client-integration-ui/reports/`。路径列为根目录下的子路径；每例保留截图、客户端日志与 JSON。

| 用例 | 状态 | 结果说明 | 报告路径 |
|---|---|---|---|
| `01-quote-lifecycle` 报价完整生命周期 | 失败 | 报价管理操作未生效：ACTIVE | `migration-targeted-final/01-quote-lifecycle` |
| `02-precise-crm` 识别机构交易员、勾选及确认报价 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/02-precise-crm` |
| `04-offer-t0` 卖出 T+0 报价确认及持久化 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/04-offer-t0` |
| `05-settlement-update` 清算速度修改后保存 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final2/05-settlement-update` |
| `06-invalid-price-recovery` 非法价格拒绝后修正重试 | 失败 | 修改拒绝提示 | `migration-functional-final2/06-invalid-price-recovery` |
| `07-management-filters` 报价管理侧栏过滤与重置 | 失败 | mgmt 筛选后本例报价应消失 | `migration-functional-final2/07-management-filters` |
| `08-market-filters` 行情中心侧栏过滤与重置 | 失败 | market 筛选后本例报价应消失 | `migration-functional-final/08-market-filters` |
| `09-multiline-partial-confirm` 多行识别分两次确认 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/09-multiline-partial-confirm` |
| `10-semicolon-quotes` 分号分隔报价不丢第二条 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/10-semicolon-quotes` |
| `11-preview-selection-toolbar` 批量工具栏只修改勾选报价 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/11-preview-selection-toolbar` |
| `12-no-selection-confirm` 未勾选不能确认报价 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/12-no-selection-confirm` |
| `13-cd-name-volume-before-yield` 用户原文：存单简称、报量在前、明天清算 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/13-cd-name-volume-before-yield` |
| `14-watchlist-notification-read` 关注报价提醒到达并标记已读 | 失败 | 界面缺少可见按钮：标记已读，页面：BoardAlertView | `migration-functional-final2/14-watchlist-notification-read` |
| `15-escaped-newline-settlement` 字面转义换行：各行清算报量独立 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/15-escaped-newline-settlement` |
| `16-enter-multiline` Enter 换行及多行识别 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/16-enter-multiline` |
| `17-precise-toolbar` 精确报价工具栏选择范围与数量操作 | 失败 | 验证目标缺失：识别警告 请先勾选 | `migration-functional-final/17-precise-toolbar` |
| `18-qtrade-negotiation` QT 双边议价待确认与正式行情隔离 | 环境阻断 | QuoteSvr 离线，未验证业务步骤。 | `migration-acceptance/18-qtrade-negotiation` |
| `20-smart-recommendation` 智能推荐：证据、反馈与字号回归 | 失败 | 界面缺少可见按钮：展开，页面：DataGridRow | `migration-targeted-final/20-smart-recommendation` |
| `22-account-groups` 关注债券与筛选分组：账户持久化和过滤 | 失败 | 关注分组采用服务端ID和版本 | `migration-functional-final2/22-account-groups` |
| `23-complex-parser` 双边价格、分号、方向标题与中文报量 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/23-complex-parser` |
| `27-fuzzy-preview` 模糊报价：预览、选择与批量范围 | 失败 | 集成客户端缺少要求的报价录入页签：quote-fuzzy | `migration-functional-final/27-fuzzy-preview` |
| `28-precise-input` 精确报价：示例、清空、保留原文与切页 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/28-precise-input` |
| `30-page-quote-confirm` 01 报价录入/01 报价确认：页面与查询就绪 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/30-page-quote-confirm` |
| `31-page-quote-precise` 01 报价录入/02 精确报价：页面与查询就绪 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/31-page-quote-precise` |
| `32-page-quote-fuzzy` 01 报价录入/03 模糊报价：页面与查询就绪 | 失败 | 集成客户端缺少要求的报价录入页签：quote-fuzzy | `migration-functional-final/32-page-quote-fuzzy` |
| `33-page-creditAll` 02 行情中心/01 信用全部：页面与查询就绪 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/33-page-creditAll` |
| `34-page-creditBest` 02 行情中心/02 信用最优：页面与查询就绪 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/34-page-creditBest` |
| `35-page-rateAll` 02 行情中心/03 利率全部：页面与查询就绪 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/35-page-rateAll` |
| `36-page-rateBest` 02 行情中心/04 利率最优：页面与查询就绪 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/36-page-rateBest` |
| `37-page-intent` 02 行情中心/05 模糊报价：页面与查询就绪 | 失败 | 集成客户端缺少要求的行情页签：intent | `migration-functional-final/37-page-intent` |
| `38-page-alert` 02 行情中心/06 报价提醒：页面与查询就绪 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final/38-page-alert` |
| `39-page-today` 03 报价管理/01 今日报价：页面与查询就绪 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final2/39-page-today` |
| `40-page-history` 03 报价管理/02 历史报价：页面与查询就绪 | 通过 | 业务步骤与本例清理通过。 | `migration-functional-final2/40-page-history` |
| `42-realtime-rateAll` 双账号实时：利率全部 | 待重测 | 报价管理操作未生效：ACTIVE；本次记录早于生命周期 DTO 修复，修复后重跑遇服务离线，不能作为最新通过证据。 | `migration-realtime/42-realtime-rateAll` |
| `43-realtime-rateBest` 双账号实时：利率最优 | 待重测 | 报价管理操作未生效：ACTIVE；本次记录早于生命周期 DTO 修复，修复后重跑遇服务离线，不能作为最新通过证据。 | `migration-realtime/43-realtime-rateBest` |
| `44-realtime-creditAll` 双账号实时：信用全部 | 待重测 | 报价管理操作未生效：ACTIVE；本次记录早于生命周期 DTO 修复，修复后重跑遇服务离线，不能作为最新通过证据。 | `migration-realtime/44-realtime-creditAll` |
| `45-realtime-creditBest` 双账号实时：信用最优 | 待重测 | 报价管理操作未生效：ACTIVE；本次记录早于生命周期 DTO 修复，修复后重跑遇服务离线，不能作为最新通过证据。 | `migration-realtime/45-realtime-creditBest` |
| `46-realtime-alert` 双账号实时：报价提醒 | 待重测 | 报价管理操作未生效：ACTIVE；本次记录早于生命周期 DTO 修复，修复后重跑遇服务离线，不能作为最新通过证据。 | `migration-realtime/46-realtime-alert` |
| `47-realtime-today` 双账号实时：今日报价 | 失败 | 观察账号 today/active 未实时更新；预览隔离已通过，后续阶段未完成。 | `migration-realtime-final/47-realtime-today` |
| `48-realtime-quote-confirm` 双账号实时：报价确认隔离 | 环境阻断 | gateway: 网关请求失败（ficc.base.get）：服务未返回应答 | `migration-realtime/48-realtime-quote-confirm` |
| `49-realtime-quote-precise` 双账号实时：精确报价预览隔离 | 环境阻断 | 1003: [1015] SERVER.QuoteSvr is not Online | `migration-realtime/49-realtime-quote-precise` |
| `50-realtime-quote-fuzzy` 双账号实时：模糊报价预览隔离 | 环境阻断 | 1003: [1015] SERVER.QuoteSvr is not Online | `migration-realtime/50-realtime-quote-fuzzy` |
| `51-realtime-history` 双账号实时：历史报价隔离 | 待重测 | 报价管理操作未生效：ACTIVE；本次记录早于生命周期 DTO 修复，修复后重跑遇服务离线，不能作为最新通过证据。 | `migration-realtime/51-realtime-history` |
| `52-filter-combinations-today` 今日报价：多条件组合与边界 | 失败 | 侧栏找不到绑定 FilterSearchText 的输入；定位错误已改为明确提示，产品输入仍未绑定。 | `migration-functional-final2/52-filter-combinations-today` |
| `53-filter-combinations-rateAll` 利率全部：多条件组合与边界 | 失败 | 本次组合筛选查询完成 | `migration-functional-final/53-filter-combinations-rateAll` |
| `54-filter-combinations-rateBest` 利率最优：多条件组合与边界 | 失败 | 最优报量错误：Ofr，预期  | `migration-functional-final2/54-filter-combinations-rateBest` |
| `55-realtime-qt-today` 双账号 QT 共享：今日报价 | 环境阻断 | QuoteSvr 离线，未验证业务步骤。 | `migration-acceptance/55-realtime-qt-today` |
| `56-realtime-qt-rateAll` 双账号 QT 共享：利率全部 | 环境阻断 | QuoteSvr 离线，未验证业务步骤。 | `migration-acceptance/56-realtime-qt-rateAll` |
| `57-realtime-qt-rateBest` 双账号 QT 共享：利率最优 | 环境阻断 | QuoteSvr 离线，未验证业务步骤。 | `migration-acceptance/57-realtime-qt-rateBest` |
| `58-realtime-qt-quote-confirm` 双账号 QT 共享：报价确认 | 环境阻断 | QuoteSvr 离线，未验证业务步骤。 | `migration-acceptance/58-realtime-qt-quote-confirm` |
| `59-qtrade-business-key` QT 不同 NegotiationID 按完整业务键确认替换 | 环境阻断 | QuoteSvr 离线，未验证业务步骤。 | `migration-acceptance/59-qtrade-business-key` |
| `60-precise-crm-dropdown` 精确报价行跨机构交易员搜索与回填 | 失败 | CRM 候选缺少机构信息 | `migration-functional-final2/60-precise-crm-dropdown` |

## 下一轮执行次序

先修复单账号生命周期、CRM 与管理筛选；逐例通过后再验证双账号实时变化。隐藏页面与缺失操作须由产品/开发负责人明确恢复或批准需求变更，不能由测试驱动擅自豁免。历史查询引发的服务内存问题单独定位后，再恢复批量跑测；任何性能实验另行授权。
