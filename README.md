# FICC 销售交易平台

基于 STC / BBox / DPS 的销售交易系统，包含报价生命周期、行情价格簿、报价提醒、CRM 和 QTrade 接入。桌面业务前端已整合到 STC 客户端；Web 联调页面与独立 DPS API 调试器保留。

## 文档入口

**[文档目录](docs/README.md)** 汇总当前设计、开发、部署及排障文档。

- 了解系统：[架构](docs/ficc-sales-trading-architecture.zh-CN.md)、[核心模型](docs/models/core-data-model-er.zh-CN.md)。
- 开发接入：[API 规格](docs/ficc-api-spec.zh-CN.md)、[筛选字段与枚举](docs/product-filter-contract.zh-CN.md)、[STC 开发约定](docs/stc-development-principles.zh-CN.md)。
- 构建部署：[Maven 构建](iteration1/docs/maven-jenkins.zh-CN.md)、[QA Jenkins](iteration1/docs/jenkins-qa.zh-CN.md)、[数据库初始化与迁移](iteration1/docs/qa-database.zh-CN.md)。
- 运行联调：[本机启动](iteration1/docs/local-startup.zh-CN.md)、[依赖适配](iteration1/docs/stc-adapters.zh-CN.md)、[API 调试器](iteration1/docs/api-debugger.zh-CN.md)。

## 代码

| 目录 | 职责 |
|---|---|
| [parent-pom](iteration1/cgws-ficc-saletrading-parent-pom) | 独立 STC 父 POM、Java 8 构建约定 |
| [common](iteration1/cgws-ficc-saletrading-common) | 共享业务工具、筛选、存储与服务客户端 |
| [quotesvr](iteration1/cgws-ficc-saletrading-quotesvr) | 报价生命周期、查询、行情价格簿 |
| [notificationsvr](iteration1/cgws-ficc-saletrading-notificationsvr) | 报价提醒、用户偏好与已读状态 |
| [crmsvr](iteration1/cgws-ficc-saletrading-crmsvr) | CRM 查询与 STC 镜像 |
| [qtintakesvr](iteration1/cgws-ficc-saletrading-qtintakesvr) | QTrade FIX 接入及报价转发 |
| [GW](iteration1/GW/README.zh-CN.md) | 标准 STC GW 配置，使用 QA 运行库 |
| [Web](iteration1/web-client/README.zh-CN.md) | 原需求界面的 GW 接入版 |
| [API 调试器](iteration1/docs/api-debugger.zh-CN.md) | DPS 请求与订阅调试 |
| [SQL](iteration1/sql) | PostgreSQL 初始化、迁移及测试夹具 |

原生桌面前端源码已纳入本仓库，见 [STC 客户端](iteration1/stc-client/README.zh-CN.md)。使用在 `feature/ficc-native-salestrading` 基础上合并产品 UI 的 LSJ 版本。独立 C# 学习示例和 WinForms 原型已移除。

## 构建与运行

Java 使用 JDK 8、Maven 3.9.x 和各工程主 `pom.xml`，不再维护 `pom_compat.xml`。父 POM 不含 modules；先构建 parent-pom 和 common，再构建各服务。依赖准备、构建命令及 Jenkins 差异见[构建指南](iteration1/docs/maven-jenkins.zh-CN.md)。

API 调试器需要 Windows、.NET SDK 和 .NET Framework 4.7.2 开发工具：

```powershell
dotnet build iteration1/api-debugger/DpsApiDebugger.csproj
```

QuoteSvr 与 NotificationSvr 共用业务 schema。用户及债券资料可接真实 STC 服务；CRM 支持通过 CRMSvr 访问、直接只读访问 PostgreSQL，或显式选择模拟实现。QA 配置和本机配置分别维护，不应互相覆盖。

报价、行情和提醒各有独立模型及推送频道，使用 `view.open` / `view.query` 和客户端分页排序；交互约定见[前后端协作说明](docs/quote-board-paging-collaboration.zh-CN.md)。

## 仓库边界

数据库数据、密码、许可证、运行日志、安装程序和本机缓存不提交。Java 依赖来自公司 Maven 仓库或本机导入的 QA 原版依赖；API 调试器所需的 DPS SDK DLL 随源码保留。

旧接口讨论稿、已移除客户端的操作指南及一次性测试记录已从当前文档树移除，可从 Git 历史追溯。保留的旧系统分析单独列为参考，不作为当前 API 契约。
