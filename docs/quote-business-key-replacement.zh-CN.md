# 报价完整业务键替换：实现与验证

日期：2026-10-16。分支：`feature/ficc-shared-confirmed-quotes`。

## 规则与实现

用户确认的业务键为 `securityId + side + settlType + ownerUserId + institutionId + contactId`。
确认、Unrefer、修改 ACTIVE 报价时，对手两个 ID 都存在才替换同键的其他 ACTIVE 报价。
不同 NegotiationID、外部 QuoteID 或来源类型不阻止替换；任一对手 ID 缺失则保持独立。
交易日不额外参与该键；生效顺序为成功的人工操作顺序，不是外部消息时间。

旧报价保留内部 quoteId，转为 REFERRED，增加 revision，并记录指向新报价的替换原因。
旧报价状态、历史、事件与新报价写入共用事务；提交后才更新内存和推送视图变化。
外部接入和待确认编辑不替换正式报价。启动时不清洗已有重复数据，下次人工激活同键报价时处理。

变更集中于 QuoteSvr 的 `QuoteService.replaceActiveQuotesWithSameBusinessKey`。
没有数据库结构、common 模块、生产 STC 客户端协议或配置变更；客户端修改仅涉及自动化工具和用例。
本地 QuoteSvr 已重新构建部署，自动化工具已重新构建。

## 组件验证

执行：`python iteration1/scripts/test-qtrade-negotiation.py`。
每个子进程有 45 秒看门狗，使用独立 PostgreSQL schema，结束删除测试 schema；不是压力测试。

报告目录：`iteration1/runtime/negotiation-tests/20261016-181123/`。

| 检查 | 结果 |
|---|---|
| QuoteBusinessKeySmoke | 99 项断言通过 |
| QTradeNegotiationSmoke | 86 项断言通过；映射后的新提案改用不同 NegotiationID |
| QTradeNotificationSmoke | 23 项断言通过 |
| QtIntakeSmoke | 44 项检查通过 |
| IncrementalViewSmoke | 385 项检查通过，五笔功能夹具 |

覆盖六个键字段分别变化、未匹配身份保持独立、确认替换、Unrefer 替换、有效报价编辑导致碰撞、
待确认不影响正式价池、过期版本与无效输入拒绝、历史记录及服务重新加载。
数据库触发器在保存新 ACTIVE 报价时故意抛错，验证已执行的旧价替换、事件均回滚，内存和重载状态保持正确。

## 真实桌面验证

真实 STC 登录 `test2`，通过运行中的 GW、QuoteSvr、ParserSvr、CRMSvr，操作正式 WPF 页面。
每次 UI 操作前间隔 2 秒；报价确认通过实际控件，机构和联系人通过 CRM 下拉选择。

| 用例 | 结果 | 主要验证 |
|---|---|---|
| 18-qtrade-negotiation | PASSED，约 111 秒 | 不同清算速度保持独立，同键新提案确认后替换；待确认更新不污染正式价格 |
| 59-qtrade-business-key | PASSED，约 100 秒 | 三个不同 NegotiationID，完整业务键相同，逐次 UI 确认后替换旧报价 |

桌面报告与截图：
`iteration1/stc-client/stc-client/runtime/ui-test-client/ui-test-reports/business-key-20261016-181313/`。
两例清理均通过。测试在确认前检查是否存在非本例的同键有效报价，若存在则中止，不能覆盖业务数据。
自动化工具增加 `qtrade.business-key` 动作，可在用例编辑器复用。

边界：本次桌面用例通过接入 API 准备 QT 提案，未重新发送外部 QTrade FIX 消息；
不能把此结果当作新一轮外部 FIX 交付验证。没有进行性能或大数据量测试。
