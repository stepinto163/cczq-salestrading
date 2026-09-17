# FICC 销售交易平台

命名及已有数据库升级步骤见 [产品命名更新](iteration1/docs/product-naming.zh-CN.md)。

本仓库保存销售交易服务、C# 桌面平台、DPS API 调试器、数据库 SQL 和设计文档。服务基于既有 STC/BBox/DPS 框架；前端经独立 GW 调用后台。

## 代码

| 目录 | 内容 |
|---|---|
| [iteration1/cczq-app-salestrading-common](iteration1/cczq-app-salestrading-common) | 共享业务工具与服务客户端 |
| [iteration1/cczq-app-salestrading-quotesvr](iteration1/cczq-app-salestrading-quotesvr) | 报价生命周期、查询、行情价格簿 |
| [iteration1/cczq-app-salestrading-notificationsvr](iteration1/cczq-app-salestrading-notificationsvr) | 报价提醒、用户偏好与已读状态 |
| [iteration1/gateway](iteration1/gateway) | STC GW 配置与接入扩展 |
| [iteration1/client](iteration1/client) | C# WinForms 平台与有界功能自动测试 |
| [iteration1/web-client](iteration1/web-client/README.zh-CN.md) | Web 联调平台首轮：GW 登录、报价生命周期、行情与提醒 |
| [iteration1/api-debugger](iteration1/api-debugger) | DPS 请求与订阅调试工具 |
| [iteration1/sql](iteration1/sql) | PostgreSQL 初始化及旧 schema 迁移脚本 |

QuoteSvr 与 NotificationSvr 共用 PostgreSQL 的 `ficc_salestrading` schema。默认使用模拟数据；登录、基础数据可切换至真实 STC 服务，CRM 支持空实现及外部适配 JAR。配置与联调边界见 [STC / CRM 适配指南](iteration1/docs/stc-adapters.zh-CN.md)。

## 构建与运行

Java 入口为 [Maven 父 POM](iteration1/pom.xml)，要求 JDK 17 和 Maven 3.9.x。首次构建前先准备兼容依赖：

```powershell
mvn -B -ntp -f iteration1/pom.xml clean verify
```

详细步骤见 [Maven / Jenkins 构建指南](iteration1/docs/maven-jenkins.zh-CN.md)。[Jenkinsfile](iteration1/Jenkinsfile) 构建 4 个 Java 模块并归档 JAR 与部署 ZIP；Maven 产物位于各模块的 `target-maven`。

内网 STC Maven 环境使用独立入口 `mvn -f iteration1/pom_stc.xml clean verify`，包含 Common、QuoteSvr、NotificationSvr 和 GW，输出到各模块 `target-stc`。原有 `pom.xml` 保留供本机兼容构建使用。详见 [STC Maven 构建](iteration1/docs/maven-stc.zh-CN.md)。

C# 平台与调试器要求 Windows、.NET SDK 和 .NET Framework 4.7.2 开发工具：

```powershell
dotnet build iteration1/client/SalesTradingClient.csproj
dotnet build iteration1/api-debugger/DpsApiDebugger.csproj
```

日常启动、功能测试和配置见 [平台说明](iteration1/README.zh-CN.md)、[启动指南](iteration1/docs/local-startup.zh-CN.md) 和 [QA 数据库指南](iteration1/docs/qa-database.zh-CN.md)。

报价、行情和提醒通过 `view.query` 分页查询，客户端只缓存当前批次，不再使用 `view.extend` 累积扩展。C# 支持滚动换批和上一批／下一批；Web 滚动后进入稳定浏览模式，点击“有 XX 条变更”返回实时更新。详见 [分页查询与滚动浏览](iteration1/docs/quote-scrolling.zh-CN.md) 和 [Web 浏览交互](docs/quote-board-browsing-ux.zh-CN.md)。

服务端 ID 复用 STC 生成器，规则与兼容性见 [ID 生成说明](iteration1/docs/id-generation.zh-CN.md)。

## 文档

当前设计入口：

- [系统架构](docs/ficc-sales-trading-architecture.zh-CN.md)
- [核心数据模型与 ER 图](docs/models/core-data-model-er.zh-CN.md)
- [API 规格](docs/ficc-api-spec.zh-CN.md)
- [STC 开发原则](docs/stc-development-principles.zh-CN.md)

`docs` 内的旧协议草案、历史代码分析和界面复原资料用于参考；实际平台行为及部署步骤以 `iteration1` 下的代码和文档为准。

## 仓库边界

数据库数据、密码、许可证、运行日志、安装程序、编译产物和本机缓存不提交。Java 兼容依赖通过 Maven 本地仓库或公司私服提供，详见构建指南。两个 C# 项目直接使用的 `gateway-api.dll` 随源码保留；其余公开 .NET 依赖通过 NuGet 恢复。

首次克隆后需要在本机准备数据库连接配置、有效许可证和 STC 兼容依赖。原始浏览器页面存档、无关旧数据库脚本和早期网格压力测试工具不属于本仓库发布内容。
