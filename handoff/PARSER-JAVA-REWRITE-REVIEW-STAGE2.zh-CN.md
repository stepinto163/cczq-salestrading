# Java Parser 第二阶段评审（2026-09-29）

结论：上一轮主要误报已修复，可以继续完整解析能力的开发；仍有基准身份校验、测试超时及 Linux 加载路径问题需修正。完整替换未完成，不合并／部署。本轮交付者也明确标记 IN_PROGRESS，未新增解析能力。

## 独立验证通过

从 `ai-copies/cb-parser-java-rewrite-20260929` 复制 POM 和 src 到独立评审目录，使用 JDK 8、离线 Maven 执行 `package`：**25 tests，0 failures，0 errors，JAR 构建成功**。未依赖交付目录原有 class 文件。

逐进程回放已保存 Windows 基准：16/16 MATCH。本轮未重新运行 Windows DLL 或 Linux SO。

- 修改价格：DIFF，真实退出码 1。
- 修改原用例文本：拒绝过期指纹，退出码 2。
- Maven 测试覆盖缺失第二条结果、未知用例、空选集、缺失基准及 NOT_RUN。
- 未加载字典不再产出报价；未实现入口抛出明确异常，对应 JUnit 测试通过。
- 对照现在比较预处理文本及全部行结果，已经修复仅比较第一条的缺陷。注意这仍是当前自定义的逐行比较边界，不等于已经验证 `BondQuoteParser.parsingAll` 的全部服务行为。

所有评审运行使用有界内存和单逻辑 CPU；Maven 外层 55 秒进程树 watchdog，单次回放 15 秒外部超时。没有运行性能测试、数据库或共享服务。

证据：`D:/workspace/greatewall/ficc/ficc/iteration1/runtime/parser-codebuddy-review-stage2-20260929/` 下 `maven.log`、`build-verification.json`、`replay-verification.json` 及独立变异输入／报告。

## 剩余问题与要求

### 1. P1：基准身份校验仍允许缺失或无法核验的数据通过

位置：`CompareRunner.java:331,344,348`（下文 Java 路径均在 `src/main/java/cgws/ficc/parser/compare/`）。

实际重现，以下情况都返回 16/16 MATCH、退出码 0：

- 删除基准 platform，并将 artifacts 置空。
- 将基准文件记录的 JAR／native 哈希全部改为 64 个 0，保留用例清单原来的预期哈希，离线回放仍通过。
- 显式传入不存在的 `--legacy-jar`，哈希得到 null 后被跳过。

离线回放不要求机器上仍有旧二进制，但必须保留有效身份并与明确的预期基准清单核对。不能因为没有提供实物文件，就把缺失平台／缺失哈希／与用例预期不符也当有效。用户显式提供路径时必须验证文件存在且可读。

另：`Fingerprint.java:24-26` 未包含 `DictionaryRow.key`。实际改变字典 key，旧基准仍可回放成功。当前解析尚未使用它，因此这不是本轮输出错误，但后续使用 bond key 时会再次出现旧输入基准误用；应把完整有效输入做规范序列化后计算指纹，配置、日期／时区等新增输入也同样纳入。

验收：上述身份缺失／不符必须返回非零，离线有效基准仍能正常回放；新增独立负向用例。

### 2. P1：进程级测试的超时可能永远到不了

`src/test/java/cgws/ficc/parser/CompareRunnerProcessTest.java:99` 先同步读 stdout 到 EOF，`:102` 才执行 `waitFor(60, ...)`。若子 JVM 卡住且不关闭 stdout，读操作一直阻塞，60 秒超时逻辑根本不会执行。

本轮正常测试未卡住；这是源码确认的控制流程缺陷，没有故意启动挂起程序。Maven 构建也未自带覆盖这种阻塞的外部 watchdog，本次评审依靠独立外部保护。

要求：输出改为文件重定向或异步消费，启动后立即有独立截止时间；超时终止子进程并等待退出，设置堆上限。增加安全、短时的受控子进程超时测试，不启动 native 或压力测试。

### 3. P2：Linux 支持仍不能只凭新增 --work-dir 宣称完成

`CompareRunner.java:384` 把整个 classpath 当作一个文件转绝对路径；多个相对 classpath 项在更换工作目录后会解析错误。`:394-397` 对子进程传入的 legacy/native 路径也未逐项固定为绝对路径。

更重要的是 Linux 旧包装器实际加载 `work-dir/libparser.so`，而记录哈希的文件来自 `--native-lib`。二者可能是不同文件；`ExitCodes.resolveNativeLibrary` 在目录同时含 DLL/SO 时又优先选择 DLL。当前“实际 SHA”不一定代表真正加载的库。

要求：逐项规范化 classpath；根据目标 OS 选择库；将实际加载文件与记录的哈希绑定，加载目录和参数不一致时拒绝运行。Linux 对照保持 NOT_RUN，直到真的在 Linux 验证。

## 次要交付问题

- `verification.json` 仍含 `PENDING_HASH_ON_NEXT_RUN`，应补实际值或标明未核验。
- 集成补丁仍为示意代码：loader 调用位于 try 之前，`refresh()` 返回 false 未检查；上线前必须用可编译适配测试验证加载异常／空结果的记录与传播。
- 比较器单元测试里用候选解析器生成部分预期，仅适合测试比较机制，不能算旧／新兼容证据；兼容证据仍是来自已固定旧库的真实基准。

## 下一步

修复上述有限的比较工具问题后，继续完整任务，不再仅交比较器。优先完成六位债券代码、真实多行／多报价合同、清算／日期／价格类型和机构交易员上下文，再补齐余下能力。扩大固定输入集并保存旧库输出；Linux、性能和服务端到端验证各自如实记录，不以 16 个 Windows 用例代替完整验收。

没有修改 CodeBuddy 交付源码或在线环境；本文件是评审结果，不代表已经让 CodeBuddy 自动继续。
