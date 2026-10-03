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

在 `qa_env\ats_root\scripts` 使用 `client.cmd start|stop|restart|status` 管理日常客户端，使用 `autotest.cmd start|stop|restart|status|build|report` 管理 UI 自动化测试窗口。仓库内也可执行 `iteration1\scripts\autotest.cmd`；启动窗口后选择用例运行，不会自动发起测试。详见 [UI 自动化测试管理](../stc-client/stc-client/docs/ui-automation.zh-CN.md)。

## API 调试与日志

### 命令前缀匹配

本机 `start/stop/restart/show/log.cmd` 的服务参数、`build/deploy.cmd` 的目标，以及 `client/autotest.cmd` 的操作参数，均支持**大小写不敏感的唯一前缀**。`log` 的 `-Kind` 也支持前缀。完整名称优先匹配；多个候选时列出候选并报错，不执行任何操作。

```powershell
.\show.cmd QUO                 # QuoteSvr
.\start.cmd qt                 # QtIntakeSvr
.\log.cmd not -Kind APP        # NotificationSvr 应用日志
.\log.cmd CL                   # client
.\client.cmd STAT              # status
.\autotest.cmd REP             # report
# 在仓库根目录：
.\build.cmd quo -Plan
.\deploy.cmd NOT -Plan
```

`q` 同时匹配 QuoteSvr / QtIntakeSvr，因此必须至少写 `quo` / `qt`；`client st` 匹配 start / stop / status，因此使用 `start` / `sto` / `stat`。`autotest re` 匹配 restart / report，使用 `res` / `rep`。前缀是字面匹配，不支持通配符。仍兼容 `Registry`、`stc-client` 和 `stc/QuoteSvr/QuoteSvr` 等已有名称形式。脚本文件名不支持缩写，缩写仅适用于参数。

共享匹配逻辑位于 `iteration1/scripts/resolve-command-name.ps1`；本机 `qa_env/ats_root/scripts/service.ps1` 也加载该文件。验证：`powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/tests/test-command-prefixes.ps1`。

在 `qa_env\ats_root\scripts` 执行以下命令，每次都会打开独立日志控制台；也可使用仓库内的 `iteration1\scripts\log.cmd`：

```powershell
.\log.cmd QuoteSvr                  # 控制台日志，最近 80 行并持续跟随
.\log.cmd ParserSvr
.\log.cmd QtIntakeSvr
.\log.cmd client                    # 日常 STC 客户端当前最新日志
.\log.cmd QuoteSvr -Kind app         # 应用日志 QuoteSvr.log
.\log.cmd QuoteSvr -Kind error       # 标准错误日志
.\log.cmd NotificationSvr -Lines 200
```

服务名来自 `ATS_ROOT/scripts/serverlist.csv`。客户端路径读取 `runtime/native-client/STC.exe.config` 的 `LogPath`。关闭日志窗口不会停止服务或客户端；Ctrl+C 停止跟随。日志轮转或客户端跨日生成新文件后，重新执行命令打开新日志。`-Here` 在当前控制台跟随，`-Once` 只输出最近几行，便于脚本检查。仓库入口环境优先级为 `-AtsRoot` → `ATS_ROOT` 环境变量 → 同级 `qa_env/ats_root`；本机环境入口固定指向自己的 ATS_ROOT。

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
