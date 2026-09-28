# NotificationSvr 部署

1. 解压完整 ZIP，保留业务 JAR、lib、control、config、sql 的相对位置。部署包不包含密码和许可证。
2. 复制 `config/database.properties.example` 为 `config/database.properties`，填写 PostgreSQL 连接。两个服务使用同一数据库和 `ficc_salestrading` schema。
3. 修改 `control/ATSConfig.ini` 的注册中心、NetType、监听地址及服务发现配置。示例默认是本机配置，不能直接作为 QA 配置。
4. 本机模拟模式复制 `config/adapters.local.example.properties` 为 `config/adapters.properties`；STC 模式使用 `adapters.integration.example.properties`。在 `control/NotificationSvr/resourceCfg.ini` 设置 `adapter.properties=./config/adapters.properties`。后者覆盖数据库配置文件中的同名项。CRM 由 CRMSvr 通过总线提供（`crm.mode=stc`、`crm.serverKey=SERVER.CRMSvr`）；回退的直连库配置参见 `crm.postgresql.example.properties`。
5. STC 模式按 `config/stc-consumers.example.ini` 合并 LoginSvr/BaseSvr 发现配置，保留本服务及 NotificationSvr 对 QuoteSvr 的配置。登录账号和密码由客户端用户提供，无需服务账号。
6. 按所用 STC 运行库要求放置许可证 `control/server.lic`。新库执行 `sql/001-create-tables.sql`；002、003 仅用于对应旧版本迁移，不自动执行。
7. 设置 Java 8，工作目录切换至解压后的服务根目录，设置 `ATS_ROOT` 为该目录后启动：

```powershell
$env:ATS_ROOT = (Get-Location).Path
java -Xms32m -Xmx192m -XX:ParallelGCThreads=2 -XX:CICompilerCount=2 -Dfile.encoding=UTF-8 -jar cgws-ficc-saletrading-notificationsvr-0.1.0-SNAPSHOT.jar
```

依赖路径来自 JAR Manifest。启动顺序：PostgreSQL、ZooKeeper、QuoteSvr、NotificationSvr、标准 GW、客户端。Jenkins 构建不启动服务、不迁移数据库、不运行负载测试。
