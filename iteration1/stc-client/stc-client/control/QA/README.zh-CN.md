# 销售交易客户端 QA 运行

在仓库根目录执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\package-qa-client.ps1 -Build
```

输出目录为 `runtime\qa-client`。复制整个目录到 QA 电脑，双击其中的 `start-qa.cmd`。不需要 Visual Studio，需要安装 .NET Framework 4.7.2 或兼容版本。使用真实 STC 账号登录；不包含模拟登录或基础资料。

配置来源为本目录 `STC.exe.config`。当前源配置的 GW 地址是 `10.100.5.39:2000`；若新部署地址不同，打包时显式指定：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\package-qa-client.ps1 -GatewayHost '<QA-GW-IP>' -GatewayPort 18600
```

也可修改输出目录 `STC.exe.config` 中 ServerGroupCfg 的 `DpsHost`、`DpsPort` 后重启。重新打包会覆盖输出配置，源配置不会被脚本修改。

打包配置使用 `Ficc.IntegrationMode=stc`、`ApiKey=1`、`StartDps=2`，直接连接远端 GW，不启动本地 dps.exe。`ClientUpdateType=3` 跳过自动更新，避免当前联调版本被旧更新包覆盖。保留本次编译所需的 .NET 版本及程序集绑定设置，默认页面为销交平台。

GW 应提供真实 STC 登录及宿主初始化所需服务，并增加 QuoteSvr / NotificationSvr 请求路由和推送主题。新插件仍受宿主权限与服务配置影响。

本机已完成构建、配置解析及窗口启动检查，日志确认 stc 模式；没有提交 QA 登录，尚未验证远端登录、服务发现与业务操作。日志在包内 `logs` 目录。
