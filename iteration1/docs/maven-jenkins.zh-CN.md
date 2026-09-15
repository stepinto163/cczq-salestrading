# Maven 与 Jenkins 构建

## 项目结构

使用独立父 POM 统一 Java 17、依赖版本与插件版本，复用 STC/BBox 框架。入口为 [iteration1/pom.xml](../pom.xml)，IDE 可直接导入。

| 模块 | 产物 |
|---|---|
| `cczq-app-salestrading-common` | 共享业务库 JAR |
| `cczq-app-salestrading-quotesvr` | QuoteSvr JAR、部署 ZIP |
| `cczq-app-salestrading-notificationsvr` | NotificationSvr JAR、部署 ZIP |
| `gateway` | `cczq-app-salestrading-gw` 扩展 JAR、包含既有 GW 的部署 ZIP |

版本当前统一为 `0.1.0-SNAPSHOT`。C# 平台与 API 调试器继续使用 .NET 构建，不属于 Java Maven reactor。

STC 的 innersvr 通过父 POM 管理公共依赖。本项目采用相同的 Maven 分层方式，但未直接继承其私有父 POM，以免隐式引入与当前兼容补丁不一致的基础库版本。

## 构建命令

要求 JDK 17、Maven 3.9.x。基础依赖已入库后，在 `iteration1` 目录运行：

```powershell
mvn -B -ntp clean verify
```

只构建 QuoteSvr 及其依赖：

```powershell
mvn -B -ntp -pl cczq-app-salestrading-quotesvr -am clean verify
```

产物位于各模块的 `target-maven`，部署包名为 `<artifactId>-0.1.0-SNAPSHOT-dist.zip`。这是有意与旧 PowerShell 构建的 `target` 分开，避免清理或覆盖正在运行的示例服务 JAR。

`verify` 执行快速内存单元测试，生成 Surefire XML 报告。不会自动运行 `BusinessSmoke`、`SharedSchemaSmoke`、C# 自动测试，也不会连接 PostgreSQL、ZooKeeper 或启动服务。它们是需要独立环境的功能检查。

## 首次准备 STC 兼容依赖

当前示例依赖本地构建过的 STC/DPS 兼容版本，部分基础类来自旧产品 JAR。不能直接把原版 Maven 坐标替换进来，也不能依赖某台电脑上的绝对路径。

本轮将现有 **123 个 JAR** 原样登记为 `cczq.salestrading.compat` 下的普通 Maven 依赖，版本含 SHA-256 摘要。清单见 [foundation-artifacts.json](../build/foundation-artifacts.json)。[stc-foundation-runtime.pom.xml](../build/stc-foundation-runtime.pom.xml) 按已验证的顺序声明 122 个服务基础依赖；GW 额外先加载其自身兼容 JAR。每个导入 JAR 使用明确的 POM，不继承其内嵌旧 POM 的额外依赖。

这是一份冻结的兼容依赖集合，保留了当前运行环境中旧库与补丁库的覆盖关系。本轮没有清理全部旧依赖，也没有把所有 STC 原始仓库改成新的 Maven 工程。后续基础库正式发布后，再单独收敛依赖及坐标。源代码及补丁记录见 [基础库兼容说明](foundation-compatibility.zh-CN.md)。

一次性安装到当前账号默认 Maven 本地仓库：

```powershell
python iteration1/scripts/bootstrap-maven-foundations.py --lib iteration1/runtime/foundation-build/lib
```

脚本先校验全部 JAR，再通过 Maven 官方 `install-file` 登记，不使用 `systemPath`。新开发机可取得同一份依赖目录后执行。Python 要求 3.11 或更新版本。若 Maven 不在 PATH，传入 `--maven <mvn.cmd或mvn路径>`。

本机已安装的 Maven 位于 `iteration1/runtime/tools/apache-maven-3.9.16`。本次验证使用独立缓存，项目根目录的完整命令为：

```powershell
python iteration1/scripts/bootstrap-maven-foundations.py --maven iteration1/runtime/tools/apache-maven-3.9.16/bin/mvn.cmd --local-repository iteration1/runtime/maven-repository

$repository = Join-Path (Get-Location) 'iteration1/runtime/maven-repository'
& iteration1/runtime/tools/apache-maven-3.9.16/bin/mvn.cmd -B -ntp -f iteration1/pom.xml "-Dmaven.repo.local=$repository" clean verify
```

## Jenkins

[Jenkinsfile](../Jenkinsfile) 提供检出后的 Maven 构建、JUnit 报告和 JAR/ZIP 归档。流水线脚本路径配置为 `iteration1/Jenkinsfile`，默认检出整个仓库，节点 PATH 中应有 JDK 17 与 Maven。Windows 使用 `bat`，Linux 使用 `sh`。限制并发构建与构建超时，不启动压力测试。

Jenkins 节点需要能解析上面的私有兼容依赖，可选：

- 初期：在节点上执行一次依赖安装脚本，填充该构建账号使用的 Maven 本地仓库。
- 团队使用：把兼容依赖发布到公司 Maven 私服；后续干净节点只需源码、JDK/Maven 和私服配置。

首次向私服发布时，由有权限的维护者执行以下命令；`--deploy-url` 必须指向可写的 hosted/release 仓库，不能使用只读聚合仓库地址：

```powershell
python iteration1/scripts/bootstrap-maven-foundations.py --lib iteration1/runtime/foundation-build/lib --settings PATH_TO_SETTINGS_XML --deploy-url https://YOUR_MAVEN_HOST/repository/ficc-releases/ --repository-id ficc-internal
```

此命令会上传依赖，仅在明确要发布时执行。本次未向任何远程仓库上传文件，也未配置真实 Jenkins 实例。

参考 [settings.xml.example](../build/settings.xml.example)，在节点的 `~/.m2/settings.xml` 或 Jenkins 受控配置中设置私服镜像与凭据。镜像地址应包含本项目私有库和 Maven Central 代理；凭据来自环境或 Jenkins 凭据管理。POM 中没有公司地址、个人目录或密码。

## 部署包

每个服务 ZIP 包含：

```text
<服务artifactId>/
  <服务artifactId>-0.1.0-SNAPSHOT.jar
  lib/                         # 原样保存运行依赖
  control/                     # STC Handler / Stream / Resource 配置
  config/                      # GW配置或数据库配置示例
  sql/
  README.zh-CN.md
```

JAR Manifest 指定入口类和有序 `lib/` 依赖。保留整个目录后，可以使用 `java -jar` 启动，避免使用顺序未定义的 `lib/*` 替代原有覆盖关系。主类仍为 `ficc.quote.Main`、`ficc.notification.NotificationMain`、`GW`。

包内不包含 `server.lic`、本机数据库配置、测试数据或运行日志。将有效许可证及 QA 配置通过部署步骤放入；详细步骤见包内说明及 [QA 数据库指南](qa-database.zh-CN.md)。包内 control 仍以示例环境为模板，部署时需要替换网络地址和 `quote.properties` 文件位置。

## 本机验证（2026-09-14）

- 从独立 Maven 本地仓库完成首次 `clean verify`，父工程与 4 个 Java 模块全部成功。
- 最终依赖顺序校正后，离线 `clean verify` 成功，耗时约 17 秒；3 个内存单元测试通过。
- `python iteration1/scripts/verify-maven-packages.py` 检查 3 个部署 ZIP，确认入口类、control、SQL、依赖文件及全部 JAR 校验值，有序 classpath 与现有示例环境一致。未启动服务或生成报价。
- 兼容依赖也已安装到当前用户默认 `~/.m2/repository`，IDE 可解析父 POM。
- 日志：`runtime/maven-verify.log`、`runtime/maven-verify-offline.log`；包检查结果：`runtime/maven-package-check.json`。

Jenkinsfile 尚未在真实 Jenkins 实例执行；当前完成的是本机 Maven 构建与产物验证，私服地址、凭据和节点工具配置由实际 CI 环境提供。

## 官方参考

- [Maven 官方：安装第三方 JAR](https://maven.apache.org/guides/mini/guide-3rd-party-jars-local.html)
- [Maven 官方：发行版本](https://maven.apache.org/download.cgi)
- [Jenkins 官方：Declarative Pipeline 语法](https://www.jenkins.io/doc/book/pipeline/syntax/)
