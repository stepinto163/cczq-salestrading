# 标准 GW 接入销售交易平台

使用标准 `javalib` 运行库，仅调整 XML/INI 配置。不包含任何 FICC 自定义 Java 类、扩展 JAR、Maven 网关模块或网关编译步骤。

## 配置

| 文件 | 当前设置 |
|---|---|
| `config/spring-tcp-server.xml` | TCP `127.0.0.1:18600`、WebSocket `127.0.0.1:18601`；两者均要求登录，关闭 gzip；不启用 HTTP 接口 |
| `config/spring-ceda-client.xml` | 请求路由为 LoginSvr、QuoteSvr、NotificationSvr；订阅登录事件及三个领域的 `report.**` |
| `control/ATSConfig.ini` | `NetType=gw`，通过本机 ZooKeeper `127.0.0.1:12181` 发现服务；本机模拟登录映射至 QuoteSvr |
| `config/log4j.ini` | INFO 输出至控制台与 `logs/GW_日期.log` |

Web 保持 `ws://127.0.0.1:18601/gateway`，C# 保持 TCP 18600。`spring-ceda-client.xml` 是标准类名/文件名，并不表示当前使用 CEDA 传输，实际由 `NetType=gw` 选择 DPS。

真实 STC 环境：修改注册中心地址，并将 `SERVER.LoginSvr.ServiceName=QuoteSvr` 改为 `LoginSvr`，同时按现有部署配置 QuoteSvr/NotificationSvr 注册中心。浏览器在其他机器运行时，需要调整监听地址与 Web 的 GW 地址。用户凭证由登录界面输入，不写入此配置。

## 启动

先启动 ZooKeeper、QuoteSvr、NotificationSvr，再启动 GW。当前标准依赖在本机模拟登录与 QuoteSvr 共用注册服务名时，服务重启后可能只更新其中一路连接；此时重启 GW。此次未修改标准库以修正该行为。

仓库根目录执行（只启动 Java，不编译）：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\stop-gateway.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\start-gateway.ps1
```

也可直接前台启动：

```powershell
cd "D:\workspace\greatewall\ficc\ficc\iteration1\GW"
$env:ATS_ROOT = (Get-Location).Path
java -Xms32m -Xmx192m -XX:ActiveProcessorCount=2 "-Dfile.encoding=UTF-8" -cp "javalib/*" GW
```

必须保留 `config`、`control`、`javalib` 目录。`control/server.lic` 使用现有有效许可证，不提交 Git。不要同时启动旧 `gateway` 和标准 `GW`，两者监听相同端口。

## 已验证

2026-09-21，本机标准 GW 通过 Web 功能回归：双账号登录、请求回复、报价/行情/提醒独立订阅、创建与确认、撤销与恢复、筛选进出、分页、提醒已读及测试报价清理。真实 STC QA 环境需另行验证。

## 权限边界

标准 GW 保留登录验证，业务请求仍由各服务验证会话和业务权限。它不会按当前用户校验订阅 topic 后缀是否属于自己的 sessionId；已知其他会话的 topic 时可以发起订阅。这与旧自定义检查不同，功能回归必须单独记录此限制，不能视为已通过会话隔离验收。内部同步接口的网关专项拦截也随自定义类移除；生产权限加固应由标准网关或服务端统一提供。

回归命令：powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-web-client.ps1 -StandardGw。默认不带参数的严格测试仍要求拒绝跨会话订阅。
