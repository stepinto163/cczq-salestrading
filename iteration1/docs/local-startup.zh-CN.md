# 本机启动与联调

当前存在两套部署目录，管理脚本不能混用。原生 STC 客户端替代了独立 C# 原型，旧原型的启动与自动测试入口已移除。

## 仓库内运行目录

以下命令从仓库根目录执行，适用于已经准备好配置、依赖和数据库的 `iteration1` 部署：

```powershell
# 先查看涉及的进程
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\restart-all.ps1 -WhatIf

# 重启 QuoteSvr、NotificationSvr、标准 GW 和 Web
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\restart-all.ps1
```

默认不重启 PostgreSQL 和 ZooKeeper；`-IncludeInfrastructure` 可包含本项目的这两个本机依赖。脚本不管理 LoginSvr、BaseSvr、CRMSvr、ParserSvr、QtIntakeSvr 或原生 STC 客户端，也不自动运行测试。它复用现有配置，不是首次安装脚本；构建产物需先按[构建指南](maven-jenkins.zh-CN.md)准备。

单独启停可使用 `iteration1/scripts/start-quotesvr.ps1`、`stop-quotesvr.ps1` 以及对应的 notificationsvr、gateway、web-client 脚本。服务控制台日志在 `iteration1/runtime` 下；具体文件名与端口以脚本输出、部署配置为准。

## 完整本机 STC / FICC 环境

本机环境在 `D:\workspace\greatewall\ficc\qa_env`，与仓库同级，使用自己的 `ats_root`、服务包和 `service.ps1` 管理脚本。该目录未随本仓库提交，克隆仓库不会自动得到它。应使用该环境的管理脚本和配置，不能用上面的 `restart-all.ps1` 控制其中的服务。

桌面客户端从本地独立 STC 客户端工程构建、启动，并使用相应环境的 GW 地址。本文不提供已移除原型的可执行文件路径。

## API 调试与日志

```powershell
dotnet build iteration1/api-debugger/DpsApiDebugger.csproj
Start-Process .\iteration1\api-debugger\bin\Debug\net472\Ficc.DpsApiDebugger.exe
```

在调试器输入目标 GW 地址和该环境的账号。真实 STC 登录与模拟登录应匹配[适配配置](stc-adapters.zh-CN.md)，不要把固定模拟账号用于 QA。

- [标准 GW 配置](../GW/README.zh-CN.md)
- [API 调试器操作](api-debugger.zh-CN.md)
- [Web 启动](../web-client/README.zh-CN.md)
- [启动日志](component-logging.zh-CN.md)及[DPS 日志](dps-logging.zh-CN.md)
- [QA 部署与日志路径](jenkins-qa.zh-CN.md)

启动完成需同时检查组件初始化、远端连接和数据可用性；仅有进程或监听端口不代表业务已就绪。服务重启后重新登录并重新打开业务订阅。
