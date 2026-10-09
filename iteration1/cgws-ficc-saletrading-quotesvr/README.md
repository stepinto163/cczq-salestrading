# cgws-ficc-saletrading-quotesvr


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

Jenkins：四个项目统一使用 sslib@main（sslib 仓库没有 0.0.1 标签，写 `@0.0.1` 会报 `No version 0.0.1 found for library sslib`），构建节点标签为 `jdk8`。parent/common 走 buildJavaArtifact，发布到 Nexus 私服；QuoteSvr/NotificationSvr 走 buildJavaService，只配置 QA.APP。详见 [QA Jenkins](deployment/QA.zh-CN.md)。

## 分支与发布

sslib 按分支名决定 `buildEnv`：只有 `rel_` 前缀的分支算 QA，其余（含 `main`）算 DEV。本仓库的 `deployInfo` 只定义了 `QA`，因此：

| 分支 | 行为 |
| --- | --- |
| `main` | 编译 + 打包 + 归档 tar.gz，**不部署** |
| `rel_x.y.z` | 编译 + 打包 + **部署到 10.130.21.53**：`~/scripts/stop QuoteSvr` → 上传解压到 `/opt/sumscope/stc` → `~/scripts/start QuoteSvr` |

发版：从 `main` 开出 `rel_x.y.z` 并推送，Jenkins 分支扫描会自动发现并构建。当前已发布 `rel_0.1.0`。

⚠️ sslib 只在「当前分支不是版本号最大的 rel_ 分支」时弹 `Ready to Release?` 人工确认。也就是说**推一条版本号更大的 rel_ 分支会直接自动部署到 QA，没有闸门**；重新构建旧的 rel_ 分支才会弹确认。需要硬闸门请用 GitLab 分支保护。

## QA 启动入口（ATS）

QA 的 ATS 管理脚本 `~/scripts/startconsole <Svr>` 决定 JVM 主类的方式是：先看 `~/scripts/serverlist` 里该 server 有没有 `entrance:` 字段，**没有就用 server 名本身当主类**。我们的 serverlist 条目（`stc/QuoteSvr|QuoteSvr| option: ...`）没有该字段，所以实际执行的是 `java -Xmx2048m ... QuoteSvr`。

因此每个服务必须有一个**默认包、类名与 server 名完全一致**的启动类 —— 这也是 STC 的既有约定（`LoginSvr`、`OHLCSvr`、`BaseSvr` 都在默认包）。本仓库的入口：

- `src/main/java/QuoteSvr.java`（默认包）→ 委托给 `ficc.quote.Main`
- `pom.xml` 的 `mainClass`、`deployment/bin/start` 都指向 `QuoteSvr`

把它改名或挪进包里，QA 会立刻报 `Could not find or load main class QuoteSvr`。替代方案是在 `~/scripts/serverlist` 对应行加 `entrance:ficc.quote.Main`，但该文件由 QA 运维维护，服务包自带启动类更省事。

## 本机与 QA 统一构建

使用 Java 8 和主 `pom.xml`；依赖来自 QA 原版 JAR。准备方法见 [构建指南](../docs/maven-jenkins.zh-CN.md)。

## IntelliJ 本机 Run / Debug

以 Java 8 导入本工程 `pom.xml`，选择已提交的 `.run/* Local.run.xml` 运行配置。源码包含启动配置；共享数据库、注册中心和许可证来自现有 `qa_env/ats_root/control`。具体依赖、调试前停止同名实例及验证方法见 [本机构建与部署](../docs/local-build-deploy.zh-CN.md#intellij-本机-run--debug)。
