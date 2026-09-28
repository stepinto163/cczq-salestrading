# cgws-ficc-saletrading-common


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

Jenkins：四个项目统一使用 sslib@main（sslib 仓库没有 0.0.1 标签，写 `@0.0.1` 会报 `No version 0.0.1 found for library sslib`），构建节点标签为 `jdk8`。parent/common 走 buildJavaArtifact，发布到 Nexus 私服（当前 `0.1.0-SNAPSHOT`）；QuoteSvr/NotificationSvr 走 buildJavaService，只配置 QA.APP。详见 [QA Jenkins](../docs/jenkins-qa.zh-CN.md)。

本仓库是 Maven 构件，不参与服务部署：任何分支构建成功都会 deploy 到私服，下游服务加 `-U` 才能拿到最新快照（agent 本地仓库会缓存"找不到"的失败结果）。

## 本机与 QA 统一构建

使用 Java 8 和主 `pom.xml`；依赖来自 QA 原版 JAR。准备方法见 [构建指南](../docs/maven-jenkins.zh-CN.md)。
