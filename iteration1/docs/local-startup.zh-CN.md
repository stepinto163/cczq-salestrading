# 本机启动、测试与停止指南

适用于当前电脑上已配置完成的第一轮平台。使用 **PowerShell**，所有命令均从以下目录执行：

```powershell
cd D:\workspace\greatewall\ficc\ficc
```

## 1. 启动后台进程

### 一条命令重启应用

已有本机部署可运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\restart-all.ps1
```

脚本关闭本项目C#客户端和Web服务，依次停止GW、NotificationSvr、QuoteSvr，再按相反顺序启动Java服务。确认服务监听、READY日志及通知源同步后启动Web，验证HTTP 200，最后打开C#客户端。浏览器刷新后重新登录；不自动运行测试。若客户端有未处理弹窗而无法正常关闭，脚本会停止，请先保存编辑并关闭窗口。

默认不重启PostgreSQL和ZooKeeper，需保证配置中的依赖已可用。若也要重启**本项目本机**数据库和注册中心：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\restart-all.ps1 -IncludeInfrastructure
```

查看操作范围而不执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\restart-all.ps1 -WhatIf
```

脚本复用现有start/stop脚本及当前control、properties配置，不切换STC／模拟模式，不修改数据库或重建数据，不重编业务服务。GW启动脚本使用 `iteration1/GW` 中的标准运行库，不编译任何网关或扩展代码，详见 [标准GW配置](../GW/README.zh-CN.md)。适用于现有本机构建目录；Maven产物尚未部署到该目录、仅在IntelliJ启动的远程QA环境不能直接视为同一部署。API调试器不自动重开。失败时停止后续启动并指出日志位置，已启动的服务保留供排查。

### 分步启动

按 PostgreSQL → ZooKeeper → QuoteSvr → NotificationSvr → GW 的顺序逐条执行。ZooKeeper 已安装；安装目录和注册机制见 [服务发现说明](zookeeper-discovery.zh-CN.md)。

```powershell
# 1）数据库
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\start-postgres.ps1

# 2）本机注册中心
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\start-zookeeper.ps1

# 3）报价与行情服务
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\start-quotesvr.ps1

# 4）报价提醒服务
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\start-notificationsvr.ps1

# 5）客户端接入网关
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\start-gateway.ps1
```

脚本会在后台启动进程。显示 PID 表示已发起启动，仍需查看日志确认服务就绪。若提示 `process is still running`，表示已有记录中的进程正在运行，不需要重复启动。

| 进程 | 本机地址 | 就绪检查 |
|---|---|---|
| PostgreSQL | `127.0.0.1:55432` | 启动脚本成功完成，或提示已运行 |
| ZooKeeper | `127.0.0.1:12181` | 启动脚本返回 `ZooKeeper READY`，`ruok` 返回 `imok` |
| QuoteSvr | `127.0.0.1:18680` | `quotesvr.log` 出现 `QuoteSvr READY` |
| NotificationSvr | `127.0.0.1:18690` | `notificationsvr.log` 出现 `NotificationSvr READY`，随后出现 `source synchronized` |
| GW | `127.0.0.1:18600` | 端口已监听，客户端能够连接 |

查看端口：

```powershell
Get-NetTCPConnection -State Listen -LocalPort 55432,12181,18680,18690,18600 |
    Select-Object LocalAddress,LocalPort,OwningProcess
```

## 2. 打开客户端

当前版本包含布局调整、多债券行情、分组管理、CRM对手确认、多用户测试及只读服务发现检查。当前构建位于 Debug/net472 目录：

```powershell
Start-Process .\iteration1\client\bin\Debug\net472\Ficc.SalesTradingClient.exe
```

选择 `trader1`，点击 **连接 GW**。账户及基础资料使用固定模拟数据。

- **报价管理**：新建 → 选中待确认报价 → 确认 → 修改 → 撤出 → 恢复或删除。
- **行情中心**：只显示有效报价形成的行情。新建但未确认的报价不进入行情；左侧列出匹配债券，右侧显示选中债券的买卖盘。
- **报价提醒**：查看关注债券、筛选分组、我的最优报价和对价提醒，设置开关及已读状态。
- 行情中心内 **关注债券**／**筛选分组**：新建、编辑、删除分组，并应用到行情；详见 [分组管理说明](group-management.zh-CN.md)。
- 对手机构／联系人可先只录名称；确认时用户从模拟CRM列表选定ID，详见 [对手确认说明](counterparty-confirmation.zh-CN.md)。

操作结果和 DPS 日志在底部可收起面板查看，默认收起；报价操作返回结果时自动展开。报价录入及生命周期按钮只在“报价管理”页显示。

后台服务重启后，请关闭旧客户端窗口，重新打开并连接。当前模拟会话保存在 QuoteSvr 内存中，重启后旧会话失效。

独立 API 调试器：

```powershell
Start-Process .\iteration1\api-debugger\bin\Debug\net472\Ficc.DpsApiDebugger.exe
```

连接地址为 `127.0.0.1:18600`，模拟用户名和密码均为 `trader1`。可编辑请求 JSON，查看回复，并订阅推送；详见 [API 调试器说明](api-debugger.zh-CN.md)。

## 3. 运行自动测试

“操作结果 / 开发工具” → “自动测试”页内的 **多用户场景** 单独运行2用户、2只债券、4条报价的固定15步测试。每步显示预期与实际结果，详见 [多用户场景说明](multi-user-scenario.zh-CN.md)。它不会自动加入下面的常规测试。

先启动全部后台进程，然后在客户端打开 **操作结果 / 开发工具**，切换到 **自动测试** 页，再点击 **自动测试**。报价／行情接口、四类提醒、报价界面、分组界面、CRM对手确认依次执行，共创建 8 条测试报价、2 个临时分组；每个测试子进程最多运行 30 秒，可点击 **停止测试**。

也可以单独执行提醒功能检查：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\test-notifications.ps1
```

多债券行情界面专项检查（2 条临时报价）：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\test-price-ui.ps1
```

这些测试会通过真实 GW 调用接口并写入本地数据库，属于小规模功能检查。逐项执行，不同时运行多个测试。测试被强制中断时可能留下测试记录，可根据消息日志识别。

## 4. 查看日志

```powershell
# 最近 30 行
Get-Content .\iteration1\runtime\zookeeper.log -Encoding UTF8 -Tail 30
Get-Content .\iteration1\runtime\quotesvr.log -Encoding UTF8 -Tail 30
Get-Content .\iteration1\runtime\notificationsvr.log -Encoding UTF8 -Tail 30
Get-Content .\iteration1\runtime\gateway.log -Encoding UTF8 -Tail 30

# 持续查看 QuoteSvr 日志；Ctrl+C 仅停止查看，不停止服务
Get-Content .\iteration1\runtime\quotesvr.log -Encoding UTF8 -Tail 30 -Wait
```

启动失败时，同时查看对应的 `.err` 文件，例如 `runtime/quotesvr.err`。数据库日志为 `runtime/postgres.log`。

客户端及调试器完整收发消息保存在 `iteration1/runtime/client-logs/<时间-进程号>/`。自动测试界面提供 **打开测试记录** 按钮；提醒脚本的结果为 `runtime/notification-smoke.json`，多债券检查结果为 `runtime/price-multibond-smoke.json`。

## 5. 停止后台进程

先关闭客户端和调试器，再按以下顺序执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\stop-gateway.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\stop-notificationsvr.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\stop-quotesvr.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\stop-zookeeper.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\stop-postgres.ps1
```

停止进程不会删除数据库中的业务数据。项目结构、构建命令及实现边界见 [平台总说明](../README.zh-CN.md)。
