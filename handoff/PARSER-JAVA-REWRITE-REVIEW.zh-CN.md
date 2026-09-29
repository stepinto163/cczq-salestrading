# CodeBuddy Java Parser 第一阶段评审（2026-09-29）

结论：**可保留为第一阶段候选，未达到完整替换验收，不合并／部署。先修对照工具，再继续功能实现。**

交付目录：`D:/workspace/greatewall/ficc/ai-copies/cb-parser-java-rewrite-20260929`。交付者自己的 `result.zh-CN.md` 标为 `IN_PROGRESS`，并没有声称已完成整个重写。

## 独立验证

使用本地 JDK 8，重新编译全部主代码和测试代码到独立评审目录；逐个子进程运行，128 MB 堆、Java 运行时单 CPU，每个进程由外部 Python `subprocess.run(timeout=40)` 监督。均正常结束。

| 检查 | 实测 |
|---|---|
| javac 8 编译 | 通过 |
| 原自测 | 41 个断言通过（不是 41 个独立业务场景） |
| 已保存 Windows 基准回放 | 16/16 MATCH；本轮没有重新执行旧二进制 |
| 修改 C001 基准价格为 9.99 | 报告 DIFF，但进程退出码仍为 0 |
| C001 保留原 ID，增加第二行报价 | Java 实际返回 2 条，但比较器仍报 MATCH |
| 指定不存在的基准文件 | NOT_RUN，退出码 0 |
| 指定不存在的用例 ID | 执行 0 条用例，退出码 0 |
| 无字典解析报价 | 返回 1 条报价并附 DICTIONARY_EMPTY，未阻止继续处理 |

证据在 `D:/workspace/greatewall/ficc/ficc/iteration1/runtime/parser-codebuddy-review-20260929/`：`verification.json`、每次运行日志、修改后的测试输入与报告、`ReviewProbe.java` 和 `extra-probe.log`。没有修改 CodeBuddy 源码、原始基准或在线目录；没有启动 native、数据库、共享服务或性能测试。

## 必须修正的问题

### P1：只比较第一条报价，会漏掉多余／缺失结果

`src/main/java/cgws/ficc/parser/compare/CompareRunner.java:150` 只取 `response.quotes.get(0)`，其余报价和诊断未参与比较。

重现：在 C001 原文本后加换行和 `2400001.ib 3.10 2000`，独立探针确认 Java 返回 2 条，但运行器仍用第一条与旧的一条结果比较并报 MATCH。

要求：对齐旧／新调用层，比较完整结果集合、数量、顺序、候选、字段和错误。单行 native 字符串与服务层多行结果不能直接混作同一合同。加入“第一条相同、第二条增删或变化”的比较器负向测试。

### P1：缓存基准只按 ID 查找，不校验对应输入及环境

`CompareRunner.java:110` 仅按 ID 建索引；不会检查基准文本与本轮文本、字典、日期、配置和平台是否一致。上面的双行重现也证明旧输入的结果会被错误用于新输入。报告里的 native/JAR 哈希在 `:96` 等处直接取自用例配置，并非实际核验文件。

另外 `:81` 只判断子进程未超时且文件存在，不要求子进程成功或文件为本次生成；基准生成失败时可能读到上一次的文件（此项静态发现，未启动 native 重现）。

要求：保存并核验输入／环境指纹，实际计算二进制哈希；基准使用独立运行 ID／临时文件，检查退出码后原子发布。过期／错平台／缺失基准必须明确失败，不能 MATCH。

### P1：失败未传递给自动化调用方

`CompareRunner.java:192-194` 只打印 `exit=1`，未设置非零进程退出码。NOT_RUN 不计入失败，空选集也被当成功。

实测 DIFF 时控制台写 `exit=1`，Python 捕获实际退出码为 0。缺失基准及不存在用例同样返回 0。

要求：真实返回非零退出码；区分差异、输入错误和未执行。增加从外部进程启动并断言退出码的测试。

### P1：未就绪／不支持能力可能被当作正常结果

`parse/ParserCore.java:31` 添加 DICTIONARY_EMPTY 后继续解析；实测无字典仍产生报价。`compat/JavaParserFacade.java:206` 对未实现方法返回空串，只在内存保存诊断，调用方容易将其解释为“未识别到”。

`docs/integration-patch.zh-CN.md:57-58` 的刷新示例捕获异常后只调用返回 false 的方法，不重新抛出或记录异常栈；外层初始化日志可能因此把失败算成功。

要求：首次未就绪、未实现入口显式失败；刷新可保留旧快照，但必须明确传播失败状态并记录原始异常栈。接入补丁不能吞异常。

### P2：比较边界和 Linux 执行仍未就绪

`compare/BaselineRunner.java:75-76` 保存 PreProcess 输出，却未与候选预处理结果比较；ParseBond 直接接收原文本，候选走 ParserCore 的预处理／逐行处理。当前 16 个简单用例未覆盖这种边界差异。

Linux 旧 Java 包装器从 `user.dir/libparser.so` 加载，而 `CompareRunner.java:225` 未设置工作目录，仅设置 `java.library.path`。因此不能假定把当前 Windows 命令换成 Linux 路径就能运行。

要求：定义相同的被测边界；单独验证预处理，明确业务日期／配置如何传递。Linux 启动器处理库加载目录和绝对 classpath，基准结果明确绑定平台。未运行时保留 NOT_RUN。

### P2：Maven 测试没有接入构建生命周期

当前 `ParserSelfTests` 是 main 方法断言程序，POM 只配置编译插件，没有 JUnit 测试或绑定执行该 main 的测试阶段。现有自测通过不代表 Jenkins 的 `mvn test/package` 会执行这些检查。

要求：接入 Maven test 阶段并验证失败能使构建失败；提供可直接执行的 Windows 构建脚本。README 的 `(dir /s /b ...)` 不是有效的 cmd 参数展开写法。

## 完整目标仍缺什么

报告已列出未完成项：六位短码、NCD、期限／清算速度／日期／价格类型／行权、多报价拆分、表头继承对照、刷新删除改名、ParsePrice/ParseTenor/ParseAmount、表格输入、外部请求响应合同，以及 Linux 和端到端验证。

CRM 查询实现可复用现有服务，但 CRM 上下文提取和最终结果对照仍属于整体验收，不能以“核心库外”为由省略。接入文件目前是设计示例，含尚不存在的 loader/helper，不能称为可编译接入补丁。

## 建议下一轮顺序

1. 修复比较器的全结果比较、基准身份校验、退出码和失败状态；把本报告重现纳入自动化回归。
2. 先对齐现有 `BondQuoteParser.parsingAll`／服务层合同，再继续补真实业务常用的六位债券代码、多行、多报价及清算字段，避免对底层 16 条样本过拟合。
3. 补 Maven 测试与可编译服务适配，继续完成全部覆盖清单。独立样本和 Linux 对照缺口分别列出，不能把第一阶段直接发布为完整替换。

尚未重新调用 CodeBuddy；本文件只提供评审证据与续作顺序。
