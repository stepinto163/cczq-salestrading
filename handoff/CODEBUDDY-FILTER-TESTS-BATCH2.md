# CodeBuddy 筛选测试第二批：评审修复与引擎行为

状态：仅发布任务定义，待用户交给 CodeBuddy 执行；不在 INDEX 自动领取队列。一次只完成本批，完成后停止并交 Codex 评审。

## 工作位置与范围

继续独立副本 `D:/workspace/greatewall/ficc/ai-copies/wb-filter-tests-20260929`，保留首轮交付。主目录为 `D:/workspace/greatewall/ficc/ficc`，不得在那里修改产品代码。先核对两边 common 的 pom.xml 和 src/main 文件哈希：有差异则报告基线变化，不能用旧源码冒充最新验证。首轮基于 feature/ficc-native-salestrading、HEAD c335f7851f1e96823f96418f3b50efbd0db64fc1 加当时未提交源码，不只是该提交。

阅读首轮任务列出的 AGENTS、FilterEngine、FilterFields、FilterDictionaries、QuoteFilterValues、协议文档，以及你的首轮测试和报告。不要扫描全仓。产品行为预期来自任务和协议，不从被测代码生成。

允许修改首轮测试类和 filter-cases 目录中的说明；保留 representative.json 的原始 18 个用例及预期，新增 engine.json。允许新增一个 `ConditionFilterFixtureValidationTest.java` 测试加载器。需要时把加载器提取为同包的一个测试辅助类，不进入 src/main。报告写 `handoff/codebuddy-filter-tests-batch2-result.md`，保留首轮报告；本任务文件可复制进副本供阅读。

不改 POM、依赖、生产代码、协议、配置或原有其他测试。不启动服务、访问数据库或网络，不测试 UI／性能，不提交、推送或部署。最多两轮有依据的局部修正；超出边界或发现业务差异，记录并交回。

## 先修复两项评审问题

1. `enabled` 必须是 JSON Boolean。缺失、null、字符串、数字都明确失败，不允许 `getBooleanValue` 将非法值转换成禁用。Codex 已复现：首个 enabled 改为 null 后，构建仍成功、Skipped=1。
2. 每个成功、失败、跳过的用例都能按稳定 ID 和中文名称定位。优先使用现有 JUnit/Surefire 能力；不要为 XML 名称另造报告框架或改全局 POM。如 XML testcase 名称仍为索引，至少在每例的 system-out、失败信息、跳过原因中写 ID／名称及状态，并在交付报告给出定位示例。成功／匹配失败仍须显示 expectedIds 和 actualIds。

六项加载器检查（独立于下面 18 个业务用例）：

- 参数化检查非法 enabled：缺失、null、"false"、0，均加载失败。
- enabled=false：用例被保留，运行结果为 skipped，记录其 ID／名称。
- 重复 case ID：加载失败。
- 未知 expectedIds：加载失败。
- 缺失 expectedIds：加载失败；显式空数组仍合法。
- 非法夹具结构，例如 records 不是数组：加载失败。

验证这些检查确实执行，不只写校验代码。不要通过永久禁用原有业务用例证明跳过；使用独立内存夹具或临时测试资源。修复不能改变筛选业务预期。

## 新增 18 个业务用例

engine.json 使用首轮三条固定记录 Q-A/Q-B/Q-C 的相同报价字段和基础属性；只增加 ReferenceRecord.securityName：Q-A=`Alpha国开债`、Q-B=`Beta银行债`、Q-C=`null`。Q-C.region、isTaxExempt 仍省略。与首轮 fixture 分开加载，不让新增数据改变首轮预期。禁止用 FilterSampleData 或外部数据生成夹具。

每条保留 id、name、category、enabled、filter。正常用例用 expectedIds；非法条件用 expectedError（code、type、field），二者必须恰好存在一个。非法条件只调用 validate，断言 ApiException 的结构字段，不匹配异常全文，也不能用 assertThrows(Exception.class) 接受任意崩溃。

| ID | 条件 | 手工预期 |
|---|---|---|
| TEXT-01 | securityName CONTAINS "ALPHA" | Q-A（忽略大小写） |
| TEXT-02 | securityName CONTAINS "银行" | Q-B（文本包含） |
| TEXT-03 | securityName EQ "Alpha国开债" | Q-A |
| TEXT-04 | securityName IN ["Alpha国开债","Beta银行债"] | Q-A、Q-B |
| NULL-01 | securityName EXISTS false | Q-C（显式 null） |
| NULL-02 | securityName EXISTS true | Q-A、Q-B |
| NUM-01 | price IN ["2.5","2.7000"] | Q-A、Q-B |
| NUM-02 | price BETWEEN ["2.6000","2.6000"] | Q-C（上下界相等合法） |
| INVALID-01 | unknownField EQ "x" | 1001 / VALIDATION_FAILED / unknownField |
| INVALID-02 | price CONTAINS "2" | 1001 / VALIDATION_FAILED / price |
| INVALID-03 | region EQ "NOT_A_REGION" | 1001 / VALIDATION_FAILED / region |
| INVALID-04 | region IN [] | 1001 / VALIDATION_FAILED / region |
| INVALID-05 | region IN "440000" | 1001 / VALIDATION_FAILED / region |
| INVALID-06 | price BETWEEN ["3","2"] | 1001 / VALIDATION_FAILED / price |
| INVALID-07 | price BETWEEN ["2"] | 1001 / VALIDATION_FAILED / price |
| INVALID-08 | price EQ "not-a-number" | 1001 / VALIDATION_FAILED / price |
| INVALID-09 | isTaxExempt EQ "false"（字符串） | 1001 / VALIDATION_FAILED / isTaxExempt |
| INVALID-10 | region EXISTS "true"（字符串） | 1001 / VALIDATION_FAILED / region |

沿用类型化 QuoteRecord／ReferenceRecord／QuoteFilterValues → FilterEngine.match；正常用例先 validate。集合比较忽略顺序，不忽略多余或缺少的 ID。不要为通过测试修改预期或生产实现；有冲突必须保留失败并报告。

## 验证与交付

仅使用现有 Maven 离线运行本任务测试类，以 Java 8 运行。示例（副本根目录）：

```powershell
$env:JAVA_HOME='D:/workspace/greatewall/ficc/ficc/iteration1/runtime/tools/jdk8/jdk8u504-b01'
$env:MAVEN_OPTS='-Xmx512m -Dfile.encoding=UTF-8'
& 'D:/workspace/greatewall/ficc/ficc/iteration1/runtime/tools/apache-maven-3.9.16/bin/mvn.cmd' `
  -B -ntp -o -f iteration1/cgws-ficc-saletrading-common/pom.xml `
  '-Dtest=ConditionFilterCasesTest,ConditionFilterFixtureValidationTest' test
```

Codex 首轮复核使用上面的 Java 8／MAVEN_OPTS 设置时，控制台中文正常；先检查实际 XML 和控制台编码，不自行改全局 POM。依赖缺失则报告坐标，不安装／替换依赖。若需要从头编译，只清理独立副本的 Maven target，不触碰主目录或运行中的文件。

报告包含：

- 基线与文件哈希、修改文件、实际命令、JDK、退出码和 Surefire 路径。
- 原有 18 个、新增 18 个业务用例和加载器检查分别统计，列 ID、预期、实际及结果；不能把加载器检查算成业务覆盖。
- 两项评审问题的复现／修复证据，成功、失败、跳过的 ID 如何定位；必要的临时验证改动须恢复。
- 简短覆盖表：本批覆盖内容、未覆盖内容。仍未覆盖所有 79 字段、真实数据映射、QuoteMatcher 权限／分组、价格簿、推送、UI、性能。
- 差异／阻塞；工具可见的 token／费用，不可见写未知。不要估算节省金额。

完成后停止。第三批逐字段、第四批数据映射、后续服务集成均不在本次授权范围内。
