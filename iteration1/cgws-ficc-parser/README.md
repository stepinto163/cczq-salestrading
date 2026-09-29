# Java 解析器替换

**Java 已设为 Windows/Linux 默认引擎，本机运行环境已切换。** 2026-09-29 由 Codex 接手 CodeBuddy 的候选实现，导入主仓库本目录。Java 返回业务结果，原生引擎在隔离进程中异步比较并记录完整输入输出。Linux 运行验收尚未执行。

现已支持多行/分号拆分、标题继承、短码协调、双边报价、中文数量、T+0/T+1、估值加减 BP 及行权/价格类型。核心 Maven 构建及 221 项测试通过；服务 12 项、真实 STC 客户端端到端 33 项通过。旧实现 16 个完整消息结果逐字段一致；双边吞数量、分号吞报价按用户决定修正。固定样本通过不代表全部旧规则等价，见 [能力状态](coverage.zh-CN.md)、[服务接入](integration/README.zh-CN.md) 和 [验收记录](integration/validation-20260929.zh-CN.md)。

## 代码入口

| 目录 | 职责 |
|---|---|
| dict/ | 字典快照、索引、加载状态 |
| identify/ | 债券识别、短码解析 |
| parse/ | QuoteMessageParser 消息协调、JavaBondQuoteEngine 原子快照、单行报价及帮助方法 |
| text/ | 已实现的预处理子集 |
| compat/JavaParserFacade.java | 实验性门面，非完整可替换实现 |
| compare/ | 隔离调用、身份校验、比较 |
| src/test/resources/ | 旧实现固定参考与来源 |
| tools/reference/ | 参考采集源码，不参与默认测试 |

## 构建与回放

配置 JDK 8 和 Maven 后，在本目录运行 `mvn package`。测试不加载原生库、不连接数据库。

本机可在仓库根目录使用缓存工具：

```powershell
$env:JAVA_HOME = "$PWD\iteration1\runtime\tools\jdk8\jdk8u504-b01"
$env:JAVA_TOOL_OPTIONS = '-Xmx256m -XX:ActiveProcessorCount=1 -Dfile.encoding=UTF-8'
& .\iteration1\runtime\tools\apache-maven-3.9.16\bin\mvn.cmd `
  -o -s .\iteration1\build\qa-maven\settings-local.xml `
  "-Dmaven.repo.local=$PWD\iteration1\runtime\maven-repository" `
  -f .\iteration1\cgws-ficc-parser\pom.xml package
```

模块目录回放：

```text
java -Xmx128m -XX:ActiveProcessorCount=1 -cp target/classes cgws.ficc.parser.compare.CompareRunner --suite cases/sample-suite.json --baseline baselines/windows-sample-1.baseline.json --out target/comparison
```

返回码：0 一致，1 差异，2 输入或身份错误，3 未执行。离线原生采集应在独立进程中设置进程树看门狗，不默认执行全库初始化。运行服务的双引擎比较按用户要求加载原有字典，使用独立子进程和超时限制。Linux 包装器从工作目录加载 libparser.so；Windows 结果不能代替 Linux 验收。
