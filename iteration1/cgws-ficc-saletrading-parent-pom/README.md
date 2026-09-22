# cgws-ficc-saletrading-parent-pom


四个项目分别检出、构建和发布，不使用聚合父工程，也不依赖其他工程的磁盘位置。

| 项目 | 职责 |
| --- | --- |
| cgws-ficc-saletrading-parent-pom | 继承 STC 父 POM，统一 Java 8、版本及构建插件；无 modules |
| cgws-ficc-saletrading-common | 公共业务模型、协议工具、适配器及客户端 |
| cgws-ficc-saletrading-quotesvr | 报价生命周期、查询、行情计算与推送 |
| cgws-ficc-saletrading-notificationsvr | 报价提醒、偏好、查询与推送 |

## STC / Jenkins

`pom.xml` 是标准入口，使用 `cgws.ficc.saletrading` 坐标。父项目继承 `sumscope.stc:sc-app-stc-parent-pom:0.1.1-SNAPSHOT`，从内网 Maven 获取 STC 依赖。各子项目使用 `<relativePath/>`，只从 Maven 仓库解析父 POM。

同一工作站按 parent-pom → common → quotesvr / notificationsvr 的顺序，在各项目目录执行 `mvn -B -ntp install`。服务可单独执行 `mvn -B -ntp verify`，输出 `target/*-dist.zip`。

Jenkinsfile 沿用 STC 的 `sslib@0.0.1` 和 `building.buildJavaArtifact`。为四个独立仓库各建一个 Job，使用 Java 8 的构建节点；内网 settings.xml、共享库及仓库发布权限由 Jenkins 环境提供。不同节点之间须先发布父 POM 和 common（`mvn deploy`），而不是仅 install。没有配置业务服务部署目标。共享库执行细节需在内网 Jenkins 验证。

## 本机兼容构建

`pom_compat.xml` 仅用于当前笔记本的已安装 STC 兼容依赖。按同一顺序执行 `mvn -o -B -ntp -f pom_compat.xml install`，服务输出 `target-compat/*-dist.zip`。

本机项目使用 `cgws.ficc.saletrading.compat` 坐标，与标准构建隔离。运行库改用 `cgws.ficc.saletrading.compat:stc-foundation-runtime-java8:1.0-SNAPSHOT`，其中六个本地 STC 基础库已重新生成 Java 8 字节码，不覆盖原 Java 17 JAR。全新电脑需先准备这些本机依赖；内网直接使用标准 pom.xml。

Java 包名 `ficc.*`、DPS topic、服务发现名称、数据库 schema 均不随工程名称变更。当前仓库只维护 cgws-ficc-saletrading 项目，旧 cczq-app 项目通过 Git 历史查看，不再维护两份源码。

服务自己的 build、deployment、control 目录包含完整打包输入。SQL 和配置模板随两个服务分别发布，公共变更需同步两份。密码、许可证、本机运行配置不入库。IntelliJ 的 Working directory 设置为当前服务项目根目录，配置及运行方法见 deployment/README.zh-CN.md。

## Java 8 构建约定

- Jenkins 使用 JDK 8 和 Maven 3.9.x，保持现有 STC 共享库和独立项目构建顺序。
- 编译插件固定为 3.13.0，目标为 Java 8（class major version 52）。JDK 8 下采用 source/target，较新 JDK 下通过 release 8 检查标准库 API。
- Java 源码采用四空格缩进，字段分别声明，方法和分支展开；格式工具使用 google-java-format 1.22.0 的 `--aosp` 风格，不参与 Jenkins 编译。
- 本机兼容依赖曾使用 Java 17 编译，不能直接用于 Java 8 进程。新的 Java 8 依赖使用独立坐标，正式 Jenkins 仍使用内网 STC 依赖。

如需重新准备本机依赖，可运行 `scripts/prepare-java8-compat.py`，指定已有的暂存源码目录和全新的空输出目录。该工具用 JDK 17 的 `--release 8` 重编译暂存库，不修改 STC 源仓库，也不用于 Jenkins。

```powershell
python scripts/prepare-java8-compat.py --foundation-build <暂存目录> --runtime-pom <原运行库POM> --jdk17 <JDK17目录> --maven <mvn.cmd路径> --output <独立空目录>
```

## 本次验证（2026-09-22）

- 实际使用 Temurin JDK 8u504 和 Maven 3.9.16，四个兼容 POM 均完成 clean install。
- common：31 项测试通过，1 项需要单独 CRM 数据库配置的测试按条件跳过。
- 两个服务均生成完整 dist.zip；各包内 124 个 JAR 的运行时 class 均兼容 Java 8。多版本 JAR 的版本目录及 module-info 不属于 Java 8 加载范围。
- 在独立 PostgreSQL schema 中执行 5 条报价的有界功能回归，50 项检查通过，覆盖生命周期、筛选、发布回调和持久化；不包含 GW 链路测试。
- 标准 pom.xml 的内网依赖解析及 Jenkins 共享流水线仍需在内网验证。
- 本机打包使用 `MAVEN_OPTS=-Xmx256m -XX:ActiveProcessorCount=2` 限制并发。处理器参数适用于本次 8u504，不作为旧版 JDK 8 的必要启动参数。
