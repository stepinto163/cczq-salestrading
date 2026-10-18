# 候选客户端 UI 自动化验收记录（2026-10-18）

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
