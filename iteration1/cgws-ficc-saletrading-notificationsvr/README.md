# cgws-ficc-saletrading-notificationsvr


四个项目分别检出、构建和发布，不使用聚合父工程，也不依赖其他工程的磁盘位置。

| 项目 | 职责 |
| --- | --- |
| cgws-ficc-saletrading-parent-pom | 继承 STC 父 POM，统一 Java 8、版本及构建插件；无 modules |
| cgws-ficc-saletrading-common | 公共业务模型、协议工具、适配器及客户端 |
| cgws-ficc-saletrading-quotesvr | 报价生命周期、查询、行情计算与推送 |
| cgws-ficc-saletrading-notificationsvr | 报价提醒、偏好、查询与推送 |

## STC / Jenkins

`pom.xml` 是标准入口，使用 `cgws.ficc.saletrading` 坐标。父项目继承 `sumscope.stc:sc-app-stc-parent-pom:0.1.1-SNAPSHOT`，从内网 Maven 获取 STC 依赖。各子项目使用 `<relativePath/>`，只从 Maven 仓库解析父 POM。

同一工作站按 parent-pom → common → quotesvr / notificationsvr 的顺序，在各项目目录执行 `mvn -B -ntp install`。服务可单独执行 `mvn -B -ntp verify`，输出 `target/bin、target/javalib 和业务 JAR，供 QA Jenkins 打成 tar.gz`。

Jenkins：parent/common 使用 sslib@0.0.1 和 buildJavaArtifact；QuoteSvr/NotificationSvr 使用 sslib@main 和 buildJavaService，仅配置 QA.APP。详见 [QA Jenkins](deployment/QA.zh-CN.md)。

## 本机兼容构建

`pom_compat.xml` 仅用于当前笔记本的已安装 STC 兼容依赖。按同一顺序执行 `mvn -o -B -ntp -f pom_compat.xml install`，服务输出 `target-compat/*-dist.zip`。

本机项目使用 `cgws.ficc.saletrading.compat` 坐标，与标准构建隔离。运行库改用 `cgws.ficc.saletrading.compat:stc-foundation-runtime-java8:1.0-SNAPSHOT`，其中六个本地 STC 基础库已重新生成 Java 8 字节码，不覆盖原 Java 17 JAR。全新电脑需先准备这些本机依赖；内网直接使用标准 pom.xml。

Java 包名 `ficc.*`、DPS topic、服务发现名称、数据库 schema 均不随工程名称变更。当前仓库只维护 cgws-ficc-saletrading 项目，旧 cczq-app 项目通过 Git 历史查看，不再维护两份源码。

服务自己的 build、deployment、control 目录包含完整打包输入。SQL 和配置模板随两个服务分别发布，公共变更需同步两份。密码、许可证、本机运行配置不入库。IntelliJ 的 Working directory 设置为当前服务项目根目录，配置及运行方法见 deployment/README.zh-CN.md。
