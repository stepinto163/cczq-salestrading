# STC Maven 构建

**正式 STC 构建使用各 cgws-ficc-saletrading-* 项目中的 pom.xml。** 不再使用旧根 pom_stc.xml。

| 环境 | 项目内入口 | 坐标 | 输出 |
|---|---|---|---|
| 内网 STC / Jenkins | pom.xml | cgws.ficc.saletrading | target |
| 本机兼容环境 | pom_compat.xml | cgws.ficc.saletrading.compat | target-compat |

两个入口编译同一份 Java 8 源码。按 parent-pom → common → quotesvr / notificationsvr 的顺序构建。

先在父项目和 common 项目中执行 `mvn -B -ntp install`，然后在服务项目中执行：

```powershell
mvn -v
mvn -B -ntp clean verify
```

确认 Maven 使用 JDK 8；内网仓库不在默认配置时，加 -s 指定 settings.xml。跨 Jenkins 节点需先 deploy 父 POM 和 common。

完整说明见 [Maven / Jenkins 构建](maven-jenkins.zh-CN.md)。

QA 环境打包及共享数据库配置见 [QA Jenkins](jenkins-qa.zh-CN.md)。
