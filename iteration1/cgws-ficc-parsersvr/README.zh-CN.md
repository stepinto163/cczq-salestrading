# ParserSvr 服务宿主

本项目纳入 `feature/ficc-lightweight-runtime` 的全进程迁移范围。解析引擎仍在相邻的 `cgws-ficc-parser`；这里是原 ParserSvr 的 handler、stream、CRM/BaseSvr 调用及引擎集成代码。

- 包名和入口保持原样；导入时仅清空旧源码内嵌的图片解析 API 密钥默认值，仍由原配置字段提供。`source-baseline.json` 记录导入文件 SHA-256，并单独记录该清理项的原始哈希，用于检查框架迁移是否误改业务行为。
- 未导入运行日志、构建结果、数据库口令或许可证。现有部署的 XML/INI 不修改。
- Native 引擎及 shadow 对比仍需要原 Parsing/msgexpress/JNA SDK；它们是解析引擎依赖，不是新框架实现。
- 已通过 JDK 8 编译、13 项宿主测试和新框架下的真实 DPS 多行解析。Java 主引擎通过不代表 native shadow 或完整 STC UI 均通过。进度见 [框架替换设计](../docs/framework-replacement.zh-CN.md)。
