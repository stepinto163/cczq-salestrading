# Java 独立工程

2026-09-21，按 STC 工程组织方式，将当前 Java 源码提取为以下四个独立本地 Git 项目：

| 路径 | 职责 |
| --- | --- |
| `D:\workspace\GitHub\cgws-app-salestrading-parent-pom` | 发布父 POM，不包含 modules |
| `D:\workspace\GitHub\cgws-app-salestrading-common` | 公共库 |
| `D:\workspace\GitHub\cgws-app-salestrading-quotesvr` | 报价及行情服务 |
| `D:\workspace\GitHub\cgws-app-salestrading-notificationsvr` | 提醒服务 |

每个项目自带 README、Jenkinsfile、标准 `pom.xml` 和本机 `pom_compat.xml`。两个服务均自带 `control`、`build`、`deployment`，打包不依赖本仓库或兄弟目录。

标准父 POM 继承 `sumscope.stc:sc-app-stc-parent-pom:0.1.1-SNAPSHOT`。构建顺序：parent-pom → common → 两个服务。各目录使用 Java 17 执行 `mvn install`；不同 Jenkins 节点之间先将父 POM 和 common 发布到内网 Maven，再构建服务。

本机兼容模式使用 `mvn -o -f pom_compat.xml install`，坐标为 `cgws.salestrading.compat`，与标准 `cgws.salestrading` 隔离。已有外部冻结依赖的 `cczq.salestrading.compat` 坐标保留，仅用于本机，不出现在标准 POM 中。

本次未更改 Java 业务源码、DPS topic、服务名称和数据库 schema，也未调整现有客户端及启动脚本。原目录保留，后续 Java 开发使用独立工程；两份源码不会自动同步。四个新本地仓库尚未创建 GitHub remote，也未提交或推送。

## 验证结果

- 四个工程分别构建，本机兼容模式全部成功。
- common：27 项测试，26 项通过，1 项 PostgreSQL 集成测试因未指定数据库而跳过；无失败。
- 两个服务编译及 ZIP 打包通过。逐项校验 Manifest 的主类和 123 个依赖路径均存在，配置模板与 SQL 完整，不含本机密码文件或许可证。
- 提取的 124 个 Java 文件与原源码逐字节一致；新增 common 单元测试配置，不连接远端服务。
- 标准 POM 完成结构检查；本机离线解析因缺少 STC 父 POM 失败，完整 STC 构建和 Jenkins 共享库执行须在内网验证。
- 本次未启动服务、执行数据库迁移或运行压力测试。
