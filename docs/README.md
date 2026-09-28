# 文档目录

当前开发以 [API 规格](ficc-api-spec.zh-CN.md)、[筛选字典](product-filter-contract.zh-CN.md)及对应实现为依据；架构和模型文档中的规划不等于已上线能力。运行步骤以各服务部署说明及本轮实际配置为准。

2026-09-28 清理：移除旧接口草案、重复构建入口、已退役 C# 原型操作说明及一次性测试报告。历史内容可通过 Git 历史查阅，不再维护另一份备份文档。

维护约定：新增或保留的说明应从本目录可达；更新规则直接修改对应文档，避免为同一接口再建“新版／最终版”副本。旧系统分析只作映射依据，不能用于证明当前实现或测试通过。

## 架构、模型与前端协议

- [FICC服务API规范（实现对照与评审稿）](ficc-api-spec.zh-CN.md)
- [FICC销售交易系统架构（讨论稿）](ficc-sales-trading-architecture.zh-CN.md)
- [核心数据模型ER图（讨论草稿）](models/core-data-model-er.zh-CN.md)
- [行情模型与STC映射（讨论稿）](models/market-data-model.zh-CN.md)
- [Quote报价模型与字典（讨论稿）](models/quote-model.zh-CN.md)
- [产品筛选接口：前端接入与条件字典](product-filter-contract.zh-CN.md)
- [实时报价列表的浏览体验（产品讨论稿）](quote-board-browsing-ux.zh-CN.md)
- [报价列表滚动与分页：前后端协作讨论稿](quote-board-paging-collaboration.zh-CN.md)
- [SearchSvr设计简稿](searchsvr-design.zh-CN.md)
- [STC 服务开发原则与实现约定](stc-development-principles.zh-CN.md)

## 构建、部署与排障

- [FICC 销售交易平台开发入口](../iteration1/README.zh-CN.md)
- [服务部署包](../iteration1/build/distribution/README.zh-CN.md)
- [本机 QA Maven 依赖准备](../iteration1/build/qa-maven/README.zh-CN.md)
- [组件启动日志约定](../iteration1/docs/component-logging.zh-CN.md)
- [CRM PostgreSQL 接入](../iteration1/docs/crm-postgresql.zh-CN.md)
- [STC 适配层 DPS 日志](../iteration1/docs/dps-logging.zh-CN.md)
- [ID 生成与 STC 对齐](../iteration1/docs/id-generation.zh-CN.md)
- [QA Jenkins 打包与共享数据库配置](../iteration1/docs/jenkins-qa.zh-CN.md)
- [本机启动与联调](../iteration1/docs/local-startup.zh-CN.md)
- [Maven / Jenkins 构建](../iteration1/docs/maven-jenkins.zh-CN.md)
- [第一轮报价提醒](../iteration1/docs/notifications.zh-CN.md)
- [QA 环境数据库初始化](../iteration1/docs/qa-database.zh-CN.md)
- [第一轮：SearchSvr 尚未实现时的处理](../iteration1/docs/search-dependency.zh-CN.md)
- [STC 与 CRM 可切换依赖适配](../iteration1/docs/stc-adapters.zh-CN.md)
- [STC 服务发现](../iteration1/docs/zookeeper-discovery.zh-CN.md)

## 服务工程及随包说明

deployment 下的说明参与服务打包，保留独立副本供离线部署使用。修改 QA 公共部署规则时，应同步 QuoteSvr 与 NotificationSvr 的随包 QA 说明。

- [common · cgws-ficc-saletrading-common](../iteration1/cgws-ficc-saletrading-common/README.md)
- [common · LoginSvrClient 源码来源](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/stc/README.zh-CN.md)
- [crmsvr · CRMSvr 部署说明](../iteration1/cgws-ficc-saletrading-crmsvr/deployment/README.zh-CN.md)
- [notificationsvr · cgws-ficc-saletrading-notificationsvr](../iteration1/cgws-ficc-saletrading-notificationsvr/README.md)
- [notificationsvr · QA Jenkins 打包与共享数据库配置](../iteration1/cgws-ficc-saletrading-notificationsvr/deployment/QA.zh-CN.md)
- [notificationsvr · NotificationSvr 部署](../iteration1/cgws-ficc-saletrading-notificationsvr/deployment/README.zh-CN.md)
- [parent-pom · cgws-ficc-saletrading-parent-pom](../iteration1/cgws-ficc-saletrading-parent-pom/README.md)
- [qtintakesvr · QTrade 报价接入服务](../iteration1/cgws-ficc-saletrading-qtintakesvr/README.md)
- [qtintakesvr · QTrade 报价接入服务](../iteration1/cgws-ficc-saletrading-qtintakesvr/deployment/QA.zh-CN.md)
- [qtintakesvr · QTrade 报价接入服务](../iteration1/cgws-ficc-saletrading-qtintakesvr/deployment/README.zh-CN.md)
- [quotesvr · cgws-ficc-saletrading-quotesvr](../iteration1/cgws-ficc-saletrading-quotesvr/README.md)
- [quotesvr · QA Jenkins 打包与共享数据库配置](../iteration1/cgws-ficc-saletrading-quotesvr/deployment/QA.zh-CN.md)
- [quotesvr · QuoteSvr 部署](../iteration1/cgws-ficc-saletrading-quotesvr/deployment/README.zh-CN.md)

## 工具与客户端

- [STC 原生销售交易客户端](../iteration1/stc-client/README.zh-CN.md)
- [Python 请求与订阅最小示例](../examples/python/README.zh-CN.md)
- [标准 GW 接入销售交易平台](../iteration1/GW/README.zh-CN.md)
- [标准运行库](../iteration1/GW/javalib/README.zh-CN.md)
- [DPS API 调试器](../iteration1/docs/api-debugger.zh-CN.md)
- [独立 CRM 适配工程](../iteration1/extensions/crm-adapter/README.zh-CN.md)
- [原需求界面的 GW 接入版](../iteration1/web-client/README.zh-CN.md)

## 旧系统参考与交接协议

保留有独立价值的来源分析与外部协议映射。交接协议当前没有待执行任务；旧任务上下文仅供追溯。

- [FICC销售交易旧系统实现与迁移参考](legacy-ficc-findings.zh-CN.md)
- [旧客户端销售交易布局重建](legacy-layout-reconstruction.zh-CN.md)
- [STC InnerSvr行情模型与持久化核查](models/stc-innersvr-price-model-review.zh-CN.md)
- [QTrade消息与销售交易前端模型映射](qtrade-frontend-model-mapping.zh-CN.md)
- [QTrade对外通信消息清单](qtrade-message-inventory.zh-CN.md)
- [当前交接任务](../handoff/INDEX.md)
- [Codex / WorkBuddy 文件交接协议（讨论稿 v1）](../handoff/README.md)
- [codex-20260914-002 · 历史交接上下文（已取消）](../handoff/tasks/codex-20260914-002/context.md)
- [codex-20260915-001 · 历史交接上下文（已取消）](../handoff/tasks/codex-20260915-001/context.md)
