# QuoteSvr 本机运行与部署

源码已包含 `config/service.properties`、`config/adapters.properties` 和完整 `control`。不需要从 example 复制配置。

- IntelliJ：以 Java 8 导入本工程 `pom.xml`，选择 `QuoteSvr Local`，工作目录为工程根目录。
- 共享配置：运行配置的 `ATS_ROOT` 指向本仓库旁的 `qa_env/ats_root`；使用其中的 `control/ATSConfig.ini`、`DBPoolConfig.ini` 和 STC 许可证。
- 本机 QuoteSvr、NotificationSvr 都使用现有 `FICC_SALESTRADING` 数据源和 `ficc_local_stc` schema；身份、基础资料、CRM 使用真实 STC 服务。
- 调试前只停止原来运行的同名服务，保留 PostgreSQL、ZooKeeper、LoginSvr、BaseSvr、CRMSvr 等依赖。不能同时启动两个相同端口/存储的实例。
- Jenkins QA 仍由 Salt 模板生成配置，详见 [QA 部署](QA.zh-CN.md)。本机默认配置不会替代 QA 模板。

详细步骤见仓库 `iteration1/docs/local-build-deploy.zh-CN.md`。
