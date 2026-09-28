# STC Java 独立工程构建

当前源码以 `iteration1/cgws-ficc-saletrading-*` 为准，各工程使用主 `pom.xml`；父 POM 不含 modules。

本机与 QA/Jenkins 统一使用 Java 8，按 parent-pom → common → 各服务顺序构建，输出 `target`。跨 Jenkins 节点先发布父 POM 和 common。

旧拆分过程和兼容构建记录保留在 Git 历史，不再作为当前入口。详见 [构建指南](maven-jenkins.zh-CN.md)。
