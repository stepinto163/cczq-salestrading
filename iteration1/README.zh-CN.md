# FICC 销售交易平台开发入口

桌面业务前端已整合到 `stc-client`，不再维护独立 C# WinForms 原型或学习示例。STC 客户端源码和本机 STC 环境保留；API 调试器用于独立验证请求、订阅和报文。

## Java 服务

`cgws-ficc-saletrading-parent-pom` → `common` → `quotesvr`、`notificationsvr`、`crmsvr`、`qtintakesvr`。
统一使用 Java 8 和主 `pom.xml`，直接使用 QA 的 STC 基础库；GW 使用 QA 预编译包。
构建、依赖导入、打包检查见 [Maven / Jenkins](docs/maven-jenkins.zh-CN.md)。

## 客户端与调试

- 桌面业务入口：`stc-client`，按该项目自己的脚本构建和启动。
- [DPS API 调试器](docs/api-debugger.zh-CN.md)：代码与 SDK 位于 `api-debugger`，不依赖已移除的原型项目。
- [Web 客户端](web-client/README.zh-CN.md)：仍保留现有代码，本轮未删除。
- [Python DPS 示例](../examples/python/README.zh-CN.md)：请求与订阅用法。

```powershell
dotnet build iteration1/api-debugger/DpsApiDebugger.csproj -m:1
```

`scripts/restart-all.ps1` 只控制其声明的 Java/GW/Web 进程，不再启动旧 C# 原型。
当前本机完整 STC/FICC 环境位于 `../qa_env`（相对仓库根目录），由其中的 `ats_root/scripts` 统一管理。

## 配置和验证

- 共享数据库：`ATS_ROOT/control/DBPoolConfig.ini`。
- 服务发现与地址：`ATS_ROOT/control/ATSConfig.ini` 及服务的 control 配置。
- [STC / CRM 适配指南](docs/stc-adapters.zh-CN.md)、[QA 数据库](docs/qa-database.zh-CN.md)、[API 规格](../docs/ficc-api-spec.zh-CN.md)。
- Java 单元测试留在各模块 `src/test/java`；旧 C# 原型专用 UI、自动回归和压力脚本随原型移除。
- 历史测试报告仅作当时版本记录，不代表当前 STC 客户端的自动化覆盖。性能测试必须另行授权，从小规模开始。
