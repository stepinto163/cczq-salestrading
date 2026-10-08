# ParserSvr Java 引擎接入

服务代码和 Java 引擎现在均位于上级 `cgws-ficc-parsersvr` 工程，构建、部署和代码入口见 [工程说明](../README.md)。不再先构建独立引擎，也不再对 `qa_env/sc-app-stc-parsersvr` 应用接入补丁。

服务的 `control/ParserSvr/streamCfg.ini` 保持原有 `ParserEngine=java` 配置。Java 返回业务结果，原生比较使用现有配置；接口、CRM 后处理和比较日志格式不变。

本目录的 `.patch` 文件及日期验收记录仅保留历史来源证据，不是当前安装步骤，请勿应用到合并后的工程。

默认 `mvn package` 同时执行引擎及宿主合同测试。需要用已部署 QA JAR 做额外源码兼容检查时，可运行 `tools/check-service.py --runtime <ParserSvr目录> --java-home <JDK8目录>`；可选 `--local-database-config` 进行只读数据库检查。该工具不部署或启动服务。
