# 销售交易报价服务

- `src/main/java/ficc/quote/`：报价生命周期、查询、行情簿和模拟登录接口。
- `handler/`、`stream/`：位于上述包内，分别实现 STC 请求处理与实时输出。
- `control/`：本服务独立的 STC/BBox 装配配置。
- `src/test/java/ficc/quote/BusinessSmoke.java`：PostgreSQL 业务功能检查。
- `target/cczq-app-salestrading-quotesvr.jar`：仅包含报价服务代码，依赖公共库及现有 STC/DPS 基础库。

在本目录执行 `powershell -NoProfile -ExecutionPolicy Bypass -File ../scripts/build-server.ps1 -Module quotesvr`；构建前停止两个业务进程。启动脚本为 `../scripts/start-quotesvr.ps1`，DPS 服务名仍为 `QuoteSvr`，端口 `18680`。报价和行情继续使用不同模型及 topic。

`PgStore` 继承公共 `JdbcStore`，自行维护报价、历史及来源消息表。NotificationSvr 通过 DPS 消费报价与行情，不加载本模块 JAR。

入口使用标准 `ServerWrapper.Start`，按 `control/ATSConfig.ini` 注册到本机 ZooKeeper，见 [服务发现说明](../docs/zookeeper-discovery.zh-CN.md)。
