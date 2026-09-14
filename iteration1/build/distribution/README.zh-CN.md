# 服务部署包

本包包含业务 JAR、保持原有依赖顺序的运行库、STC control 配置及数据库 SQL。许可证和本机数据库密码不随包发布。

1. 保留整个目录结构，将有效许可证放入 `control/server.lic`。
2. 修改 `control/ATSConfig.ini` 中 ZooKeeper、服务监听地址及 GW 配置为目标环境值。
3. QuoteSvr / NotificationSvr：复制 `config/database.properties.example` 为 `config/database.properties`，填写数据库连接；把 `control/<服务名>/resourceCfg.ini` 中 `quote.properties` 改为 `./config/database.properties`。两个服务连接同一数据库和 schema。
4. 在本目录设置 `ATS_ROOT` 为本目录绝对路径，执行 `java -Xms32m -Xmx192m -XX:ActiveProcessorCount=2 -Dfile.encoding=UTF-8 -jar <本目录业务JAR文件名>`。主类和有序依赖路径已写入 JAR Manifest，不使用无序的 `lib/*`。
5. 启动顺序为 PostgreSQL、ZooKeeper、QuoteSvr、NotificationSvr、GW。新 QA 数据库只执行 `sql/001-create-demo-tables.sql`；`002` 仅用于旧双 schema 数据迁移。

配置默认使用模拟数据。内网接入可复制 `config/adapters.integration.example.properties` 为本地配置，并在 `resourceCfg.ini` 设置 `adapter.properties` 路径；按 `config/stc-consumers.example.ini` 配置真实 LoginSvr/BaseSvr 服务发现，GW 的 LoginSvr 目标也须改为真实服务。配置专用服务账号、真实债券及字典映射后再启用。详细步骤见源码仓库 `iteration1/docs/stc-adapters.zh-CN.md`。

部署前需要替换网络地址；Jenkins 构建本身不会启动服务、执行数据库迁移或运行负载测试。
