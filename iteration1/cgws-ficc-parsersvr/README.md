# ParserSvr：服务与 Java 解析引擎

ParserSvr 与 Java 解析器合并为一个 Maven 工程、一个服务 JAR。原 `cgws-ficc-parser` 工程已移除，不再单独编译、安装或部署引擎 JAR。包名保留，外部 DPS 接口 `stc.parser.bond`、XML/INI 结构及 `ParserEngine=java` 配置不变。

## 代码入口

| 位置（相对 `src/main/java`） | 职责 |
|---|---|
| `ParserSvr.java` | 现有 STC 服务入口 |
| `sc/app/stc/parser/handler`、`stream`、`utils` | DPS 请求、服务生命周期、BaseSvr/CRM 接入及双引擎比较 |
| `cgws/ficc/parser/parse` | JavaBondQuoteEngine、QuoteMessageParser、多行协调及字段识别 |
| `cgws/ficc/parser/model` | AllQuote、BondQuote 等解析模型 |
| `cgws/ficc/parser/dict`、`identify`、`text` | 字典、债券识别及文本处理 |
| `cgws/ficc/parser/compare` | 隔离进程比较、固定样本回放 |

Java 引擎与服务接入测试都在 `src/test`；JUnit 5 与原 JUnit 4 合同测试均执行。原生 shadow 保留原 SDK 与平台库依赖，本次合并不重写解析规则。

## 构建与部署

在仓库根目录运行：

```powershell
.\build parsersvr
.\deploy parsersvr -NoBuild
```

构建使用 JDK 8 和当前 FICC 父 POM（仍为 STC 框架），产物为 `target/cgws-ficc-parsersvr-0.1.0-SNAPSHOT.jar`，依赖放在 `target/javalib`。部署脚本替换整个 `javalib`，因此不会残留旧引擎 JAR 或旧服务 JAR；现有 control/config、字典、平台库保留。部署会重启 ParserSvr。

也可直接执行 `mvn -f iteration1/cgws-ficc-parsersvr/pom.xml package`。单元测试不连接生产数据库或加载原生库；所需 STC/原生 SDK Maven 依赖须已准备。

配置仍由现有 `ATS_ROOT` 与服务目录提供。本次纳入仓库的服务源码来自本机 QA 源码，与框架分支已核对；没有引入新框架依赖，未带入本机数据库配置、许可证或旧 API 密钥默认值。

## 回放与已知边界

在本模块目录执行：

```text
java -Xmx128m -cp target/classes cgws.ficc.parser.compare.CompareRunner --suite cases/sample-suite.json --baseline baselines/windows-sample-1.baseline.json --out target/comparison
```

能力范围见 [能力状态](coverage.zh-CN.md)。[历史验收](integration/validation-20260929.zh-CN.md) 记录此前双引擎及 UI 验证，不代表本次重新验收；Linux native shadow 与全部旧规则等价性仍需单独验证。

## IntelliJ 本机 Run / Debug

以 Java 8 导入本工程 `pom.xml`，选择已提交的 `.run/* Local.run.xml` 运行配置。源码包含启动配置；共享数据库、注册中心和许可证来自现有 `qa_env/ats_root/control`。具体依赖、调试前停止同名实例及验证方法见 [本机构建与部署](../docs/local-build-deploy.zh-CN.md#intellij-本机-run--debug)。
