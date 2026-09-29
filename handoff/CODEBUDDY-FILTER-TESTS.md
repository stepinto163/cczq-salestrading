# CodeBuddy 筛选功能测试小试验

状态：**首轮已交付并评审，尚未合并**。Codex 独立使用 Java 8 验证 18/18 通过；发现 enabled=null 被当作禁用且构建成功，以及报告用例定位需改善。后续执行 [第二批任务](CODEBUDDY-FILTER-TESTS-BATCH2.md)，不要重做首轮。本文件不是 INDEX 中的 ready 任务。若改走 Git 交接，仍须先发布依赖源码与任务；本地文件不代表远端已有。

## 目的与成本边界

先用一个小任务判断协作是否值得：CodeBuddy 补固定数据功能测试，Codex 审核预期、差异和结果。不是一次实现 79 字段、测试管理 UI、真实环境验收及性能框架。

本轮最多 18 个业务用例、一个测试类、一份 JSON 夹具、一份简短说明和报告。复用 JUnit 5、Maven、现有 FilterEngine；不加依赖，不改业务实现，不造第二套测试平台。缺依赖／权限或发现业务行为不符时，记录具体问题后交回；最多两轮有依据的局部修正，不反复改提示或全仓搜索。

统计实际人工／Codex 评审与返工时间；如果工具能显示 token、费用则记录，不能显示就写“未知”，不得估算成已节省的金额。功能用例运行时间不是模型推理费用。

## 可直接给 CodeBuddy 的指令

你负责一个范围受限的 Java 筛选功能测试任务。先读取本文件，然后只读取下面列出的文件及必要的直接依赖。不要扫描整个仓库，不派生其他代理，不自动处理其他任务。

### 阅读入口

1. 根 AGENTS.md 和 `iteration1/cgws-ficc-saletrading-common/AGENTS.md`。
2. `iteration1/cgws-ficc-saletrading-common/pom.xml`。
3. `iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/FilterEngine.java`。
4. `iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/FilterFields.java`。
5. `iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/FilterDictionaries.java`（只看涉及的字段和校验规则）。
6. `iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/QuoteFilterValues.java`。
7. `iteration1/cgws-ficc-saletrading-common/src/test/java/ficc/common/TypedFilterEquivalenceTest.java`。
8. `docs/product-filter-source-status.zh-CN.md` 第 6、7 节与第 9 节的 null 差异；`docs/product-filter-contract.zh-CN.md` 相关字段与操作符。

### 工作目录与交付范围

- 使用独立源码副本／checkout，不修改有其他未提交工作的主目录。本机主目录为 `D:/workspace/greatewall/ficc/ficc`；当前模型重构尚可能未提交，单从 HEAD 建 worktree 不一定含这些文件。需要复制时只复制最新 common 模块的 pom.xml、AGENTS.md、src 及本任务所列说明，保留相对路径；不复制运行配置、凭据或 target。
- 无法得到完整最新源码或本地 Maven 依赖时报告阻塞，不回退到旧版模型、不修 POM、不造兼容库。
- 仅新增以下文件，交回补丁供 Codex 评审；不提交／推送、不合并、不部署：
  - `iteration1/cgws-ficc-saletrading-common/src/test/java/ficc/common/ConditionFilterCasesTest.java`
  - `iteration1/cgws-ficc-saletrading-common/src/test/resources/filter-cases/representative.json`
  - `iteration1/cgws-ficc-saletrading-common/src/test/resources/filter-cases/README.zh-CN.md`
  - `handoff/codebuddy-filter-tests-result.md`

### 要实现的测试结构

用 JUnit 5 动态测试或现有参数化能力读取 JSON。每条用例有稳定 id、中文名称、category、enabled、filter 和显式 expectedIds。报告必须展示用例 id／名称与期望、实际 ID；忽略集合顺序，不能忽略缺少／多余记录。业务筛选无排序职责。

JSON 顶层包含 schemaVersion、固定 records 和 cases。测试从夹具建立 QuoteRecord、ReferenceRecord、QuoteFilterValues，调用现有 FilterEngine.validate 与类型化 match。不要依赖 BaseSvr、Wind、数据库、GW、网络、真实时钟或随机数，不调用 FilterSampleData 生成预期。

expectedIds 必须直接写在夹具中，依据以下表格手工确定；不得调用被测过滤器、旧过滤器或旧 enrich 来生成或修正它们。现有 TypedFilterEquivalenceTest 只证明新旧一致，本任务补独立预期。

夹具读取器遇到重复用例 id、未知 expectedIds、缺少预期或非法夹具格式应明确失败；disabled 用例在结果中可辨认，不悄悄消失。这些是加载器检查，不扩展成通用测试框架。

### 三条固定输入

所有数据均为人工测试夹具，无外部供应商真实性声明。字段缺失必须真的省略，不能用 false 或空字符串代替。枚举使用协议代码。

| 记录 ID | region | isTaxExempt | price | side | quoteType | yieldType |
|---|---|---|---|---|---|---|
| Q-A | 440000 | true | 2.5000 | 1 | 1 | 1 |
| Q-B | 320000 | false | 2.7000 | 2 | 1 | 1 |
| Q-C | 缺失 | 缺失 | 2.6000 | 1 | 1 | 1 |

quoteId／price／side／quoteType／yieldType 属于 QuoteRecord；region／isTaxExempt 属于 ReferenceRecord。给三条记录不同的固定 securityId，使测试不会错误共享属性；不读取这些债券的真实基础资料。

### 18 个固定业务用例

| id | 条件 | expectedIds |
|---|---|---|
| TAX-01 | isTaxExempt EQ true | Q-A |
| TAX-02 | isTaxExempt EQ false | Q-B |
| TAX-03 | isTaxExempt EXISTS false | Q-C |
| TAX-04 | isTaxExempt EXISTS true | Q-A、Q-B |
| REGION-01 | region EQ 440000 | Q-A |
| REGION-02 | region IN [440000,320000] | Q-A、Q-B |
| REGION-03 | region EQ 110000 | 空集合 |
| REGION-04 | region EXISTS false | Q-C |
| PRICE-01 | price BETWEEN [2.5000,2.6000] | Q-A、Q-C |
| PRICE-02 | price GT 2.5000 | Q-B、Q-C |
| PRICE-03 | price GTE 2.5000 | Q-A、Q-B、Q-C |
| PRICE-04 | price LT 2.6000 | Q-A |
| PRICE-05 | price LTE 2.6000 | Q-A、Q-C |
| PRICE-06 | price EQ 2.5 | Q-A |
| COMBO-01 | region EQ 440000 AND isTaxExempt EQ false | 空集合 |
| COMBO-02 | 两个同字段条件：region EQ 440000、region EQ 320000 | Q-A、Q-B（同字段 OR） |
| COMBO-03 | region IN [440000,320000] AND price LTE 2.6000 | Q-A |
| EMPTY-01 | 空 filter，conditions=[] | Q-A、Q-B、Q-C |

注意：本产品当前 false 不包含 null；不要为对齐 SZ 旧逻辑把 TAX-02 改成 Q-B、Q-C。多条件的 JSON 结构遵循现有协议，不创建 AND／OR 新字段。

### 验证与报告

仅运行 common 模块的这个测试类，复用现有 Maven／本地依赖，例如：

```powershell
mvn -o -f iteration1/cgws-ficc-saletrading-common/pom.xml -Dtest=ConditionFilterCasesTest test
```

本机 Maven 不在 PATH 时只从现有构建说明定位，不安装工具。依赖缺失则报告具体坐标。不要运行 build all、已有性能探针或数据库测试。测试数据固定三条，无负载扩大。

结果用 Surefire XML／文本报告；另写简短 `handoff/codebuddy-filter-tests-result.md`，包含：

- 完成／阻塞状态，源码副本位置、来源分支／SHA；存在未提交源码则如实注明并记录所读关键文件 SHA-256。
- 修改文件、实际命令、退出码、18 个业务用例各自结果及 Surefire 文件位置。
- 未覆盖：资料映射、真实外部源、QuoteMatcher 分组权限、价格簿聚合、推送、UI、性能；不得声称完整端到端覆盖。
- 发现的行为差异；不得改 expectedIds 或生产代码消除失败。
- 本轮调用次数、工具可见的 token／费用（不可见写未知），耗时和需 Codex 决策的问题。

完成后停下，等待 Codex 评审。后续才考虑其它字段、映射夹具及已有 UI 测试管理窗口的接入。
