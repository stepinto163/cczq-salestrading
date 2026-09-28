# STC 原生客户端本机联调

本轮目标是让当前 STC 客户端在不连接内网 STC 的情况下完成登录和主窗口启动，为销售交易插件开发提供入口。销售交易插件已接入首轮报价生命周期及实时列表，范围见[原生销售交易接入进度](sales-trading-integration.zh-CN.md)；启动成功不代表产品功能已全部实现。

## 依赖处理

| 依赖 | 本机 Local 模式 | 内网 Stc 模式 |
| --- | --- | --- |
| 自动更新服务 | 跳过初始化及更新检查 | 保留原启动流程 |
| GW | 连接本机标准 GW，默认 TCP 18600 | 使用环境配置 |
| LoginSvr | GW 调用 QuoteSvr 已有的本机登录模拟器，使用其真实返回的会话 | 原 STC 登录流程 |
| AdminSvr | 明确列出的用户、角色、分组、系统参数响应 | 原服务请求 |
| 客户端 BaseSvr 启动标签 | 固定标签响应 | 原服务请求 |
| QuoteSvr、NotificationSvr | 真实本机服务及 PostgreSQL | 环境中的服务 |
| 其他插件、TBook 组合与连接状态订阅 | 不加载或不启动 | 保留原行为 |

客户端固定数据在 `control/LOCAL/local-stc-fixtures.json`。它只满足宿主启动，不代替报价搜索、报价操作或通知接口。QuoteSvr 自身的基础资料、CRM 模式仍由服务端配置决定。

本机模式只允许连接回环 GW。未知模拟请求和订阅记录 `LOCAL_UNSUPPORTED` 并抛出异常，不统一返回成功。启动时输出 `COMPONENT` 日志；初始化完成只说明本地配置已加载，不代表远程服务可用。

## 编译及启动

在本客户端仓库目录执行，先确保本机 PostgreSQL、ZooKeeper、QuoteSvr、NotificationSvr、标准 GW 已启动：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\build-native-client.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\start-native-client.ps1 -Mode Local
```

本机账号 `trader1`、`trader2`，模拟器密码均为 `trader1`。本机模式不保存密码，也不自动登录。编译和生成的运行配置位于 `runtime/native-client`，不覆盖原 `client` 目录。

本机脚本使用已安装的 .NET Framework 4.7.2 targeting pack 编译，未改写源码项目原有的 4.6.1 目标。脚本需要 Visual Studio/MSBuild 的 WPF 编译支持。内网切换入口如下；内网依赖及正常登录仍需在 QA 环境验证：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\start-native-client.ps1 -Mode Stc -StcConfig 'E:\config\STC.exe.config'
```

该命令采用提供的环境配置并设置 `Ficc.IntegrationMode=stc`，不使用本机响应数据。其他环境专属文件仍按 STC 部署要求提供。

## 验证

先关闭原生客户端，再依次运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\test-local-integration.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\test-local-integration.ps1 -NativeUi
```

第一项检查模式隔离、会话完整性以及两个账号通过真实 GW 查询 QuoteSvr、NotificationSvr。第二项驱动实际登录窗口并检查主窗口打开，同时检查本轮启动日志是否有 ERROR 或未实现依赖调用。测试有独立进程 45 秒看门狗，不执行压力测试。

2026-09-21 验证：23 项 API/配置检查通过；原生登录及主窗口启动检查通过。日志位于 `runtime/native-client/logs`，测试输出位于同一运行目录。

本次排除的启动问题包括：原 STC 初始化会重新设置压缩标志，本机 GW 登录需要在连接前明确关闭压缩；首次启动布局历史可能为空；图标资源应指定所属程序集。模拟登录返回的 `ServerTime` 使用 STC 客户端要求的秒时间戳。
