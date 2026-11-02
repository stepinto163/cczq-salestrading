# 工作记录统一入口

记录方法见 [README.md](README.md)。先按任务和日期读取相关记录，不需要扫描所有历史。

## 当前交付

- 范围：2026年10月29日至11月1日；本仓库后端及同事 STP 客户端。
- 最新对外说明：[交付变更记录 V1.2](../交付变更记录-20261101-V1.2.txt)，共19项；更正了V1.1的中债偏离方向描述。
- [QA客户端自动化测试说明](../QA客户端自动化测试编译与运行说明.txt)。
- GitHub归档：[codex/work-records](https://github.com/stepinto163/cczq-salestrading/tree/codex/work-records)。这是工作文档分支，不是产品发布分支。

## 每日记录

| 日期 | 主仓库备忘 | 同事客户端备忘归档 |
|---|---|---|
| 2026-10-29 | [基础资料、逐笔行情、筛选及性能](2026-10-29.md) | [识别、参考字段及接口适配](sources/colleague-client/2026-10-29.md) |
| 2026-10-30 | 当日客户端及相关联调见右列 | [独立启动、字段、必填规则、状态筛选、日终处理](sources/colleague-client/2026-10-30.md) |
| 2026-10-31 | [BaseSvr集成、候选下拉及现场调查](2026-10-31.md) | [源码整合、导出、断连恢复、下拉候选](sources/colleague-client/2026-10-31.md) |
| 2026-11-01 | [权限、缓存优化、分组、回归及交付](2026-11-01.md) | 当日同事客户端变更统一记在左列 |
| 2026-11-02 | [原子创建正式报价、价量空值及回归](2026-11-02.md) | [直接提交与新报价窗口空值修复](sources/colleague-client/2026-11-02.md) |

## 来源与读取注意

- 主项目：`D:/workspace/greatewall/ficc/ficc`，当前集成分支 `integration/ficc-client-convergence`。
- 同事客户端原始记录：`D:/workspace/greatewall/ficc/ficc/colleague_20261029/stc-client/docs/task-memos`；本次归档来源提交 `613ee2947`，分支 `dev_sales_trading_backend`。副本原文保留早期方案和当时验证状态，以后续更正及最终代码为准。
- BaseSvr：`D:/workspace/greatewall/ficc/stc-cczq/cgws-app-salestrading-basesvr`，分支 `feature/ficc-reference-data`；本轮交付提交 `9912168`，相关工作记录在主仓库10月29日、31日备忘中。
- 对外说明由这些记录汇总；原始任务备忘不等于整项功能已经部署或验收。当前全部交付的远端QA验收仍须单独核对。
- 较早任务尚未逐项补录到本索引，不宣称这里已覆盖全部历史。继续处理旧任务时补入口，缺证据的内容标为待核实。
