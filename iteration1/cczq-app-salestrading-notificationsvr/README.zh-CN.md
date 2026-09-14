# 销售交易提醒服务

- `src/main/java/ficc/notification/NotificationMain.java`：独立进程入口。
- `NotificationResource.java`：业务初始化、通过公共 QuoteSvrClient 订阅与恢复；连接和发现由 NetClientResource 管理。
- `NotificationService.java`：四类提醒、用户偏好、已读状态、查询与视图。
- `handler/`、`stream/`：位于上述包内，分别实现 STC 请求处理与通知输出。
- `control/`：本服务独立的 STC/BBox 装配配置。
- `target/cczq-app-salestrading-notificationsvr.jar`：仅包含通知服务代码。

在本目录执行 `powershell -NoProfile -ExecutionPolicy Bypass -File ../scripts/build-server.ps1 -Module notificationsvr`；构建前停止两个业务进程。启动脚本为 `../scripts/start-notificationsvr.ps1`，DPS 服务名仍为 `NotificationSvr`，端口 `18690`。

编译和运行只依赖公共库与现有 STC/DPS 基础库，不依赖 QuoteSvr JAR。报价、行情及模拟会话通过 DPS 获取。实现细节见 [报价提醒](../docs/notifications.zh-CN.md)。

使用标准 `ServerWrapper.Start` 注册服务，启动前先运行本机 ZooKeeper，见 [服务发现说明](../docs/zookeeper-discovery.zh-CN.md)。
