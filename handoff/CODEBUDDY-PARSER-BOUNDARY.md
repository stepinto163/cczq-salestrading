# CodeBuddy：先核对 Linux ParserSvr 实现，再判断替换边界

状态：已被用户新的完整 Java 替换要求取代，**不要执行本文件**。执行 [完整 Java 解析器替换](CODEBUDDY-PARSER-JAVA-REWRITE.md)。以下内容仅保留为历史调查范围，不再限制新任务。

## 任务前提与目标

**已确认无法提供 ParseLib 的原生源码。不要继续搜索源码／PDB，不要再交付依赖修改供应商 C++ 源码的方案。**

**2026-09-29 调整：最终部署 Linux，以 Linux `.so` 为主。先读 [Linux 检查结论](PARSER-LINUX-INSPECTION.zh-CN.md)。Linux 的 `INVALID_CODE` 使用 `std::set`，不能继续假定 Windows 的 vector 查重热点也存在于 Linux。**

本轮先核对 Linux 的 Java/JNI 字典合同及解析消费者，判断是否还有需要替换的具体问题。没有 Linux 耗时证据时报告 `INSUFFICIENT_EVIDENCE`，允许结论为“暂不重写”；不能为了交付代码硬做 Java 替换。

你负责调查和候选验证，Codex 负责评审。先读主仓库 AGENTS.md、本文件，以及首轮结果摘要；不要扫描全仓，不派生代理，不处理其它协作任务。讨论使用英文，文档使用中文。

## 工作位置

- 主仓库：`D:/workspace/greatewall/ficc/ficc`，只读。
- 首轮结果：`D:/workspace/greatewall/ficc/ai-copies/cb-parser-init-20260929`，只读，保留原始证据。
- 本轮独立目录：`D:/workspace/greatewall/ficc/ai-copies/cb-parser-boundary-20260929`。已存在时先读状态，不覆盖未知改动。
- 运行环境根目录：`D:/workspace/greatewall/ficc/qa_env`，只读。
- 主仓库源码参考提交：`c71fc4652d9fafbe4387908bc80a47620c801a4b`。ParserSvr、Parsing JAR、DLL 在仓库外，须另记实际读取文件的路径与 SHA-256，不用主仓库 SHA 代替。

不要提交、推送、部署、重启服务或修改运行 JAR/DLL，不安装全局工具／编译器，不更改系统环境变量。不复制数据库密码、许可证和完整业务日志到工作目录。

## 已确认事实与首轮评审修正

- Windows 原生分组逐条线性查重是已有热点；Linux 已确认使用不同容器，不能跨平台套用。
- Java 首次重复加载已修正，不得作为本轮新成果。
- 完整字典初始化约 16 分 36 秒、SQL 约 324 ms 是 Windows 本地环境的既有测量，本轮不重跑，不能作为 Linux 数据。
- 首轮查看了 `isCode`，但没有完成 `parseBond` 的消费者追踪，也未展开 Java `BondQuoteParser.loadData()`。因此尚不能断言 Java 适配可行，或完整重写必不可少。
- 首轮“空串不追加”的说明不准确：`0x1800185e0` 在容器为空时直接走追加路径；仅遇到已有空串时返回。回归预期应区分第一次插入和重复插入。
- 首轮 `IndexedGroup` 示例先写 set，再追加 vector，没有实现分配失败时回滚。它未编译、未接入 DLL，不能称为完整等价实现。本轮无需为了运行这个示例安装 C++ 工具链。
- 现有函数地址只适用于 DLL SHA-256 `d3df85c550e5ea388b1f112a1e1e2e6a9b73ed047d619b8336896f83e5403559`。

## 必读文件：按需定位，不全量读反汇编

优先阅读 `PARSER-LINUX-INSPECTION.zh-CN.md`，以及 `qa_env/tests/parser-diagnostic/linux/inspection.json`、`crosschecks.json`、`SetCodeName.asm`、`AddCodeAndSort.asm`、`LoadCodeStart.asm`、`LoadCodeEnd.asm`。Linux 输入文件与哈希以该报告为准；下列 Windows 证据仅用于比较，禁止把 DLL 地址用于 `.so`。

1. 首轮 `result.zh-CN.md`；`evidence.zh-CN.md` 中的生命周期未知项和消费者判断。
2. `qa_env/tests/parser-diagnostic/native/README.zh-CN.md`。
3. `qa_env/tests/parser-library-bytecode.txt`：定位构造函数和约第 580 行开始的 `loadData()`，继续读到方法结束，不只搜索一个 `INVALID_CODE` 字面量。
4. `qa_env/tests/parser-db-bytecode.txt`：只查看字典记录来源和字段映射。
5. `qa_env/ats_root/stc/ParserSvr/javalib/Parsing-1.8.7-SNAPSHOT.jar`：仅当已有反编译文本不足时，用现有 Java 工具反编译／查看需要的类。
6. `qa_env/tests/parser-diagnostic/native/exports.txt`、`Java_com_sumscope_Parser_parseBond.asm`、`Java_com_sumscope_Parser_setCodeName.asm`、`function_180018930.asm`。
7. `qa_env/tests/ParserNativeProbe.java`：只作直接 JNI 探针参考；它只验证插入和 IsCode，不能替代 parseBond 验证。
8. ParserSvr 源目录 `qa_env/sc-app-stc-parsersvr/sc-app-stc-parsersvr/src/main/java/sc/app/stc/parser/` 下的 `utils/BondQuoteHelper.java`、`stream/ParserStream.java`、`job/ParserCacheJob.java`，只定位调用边界。

`native/all.asm` 用地址和调用点检索，禁止整份反汇编塞入上下文。不要重新分析全部 542 个函数块。

## 第一步：还原 Java/JNI 的完整字典调用合同

追踪 `BondQuoteParser.loadData()` → Java Parser 包装层 → JNI，给出简短表格：

| 事项 | 必须说明 |
|---|---|
| `SetCodeName` 参数 | 每个参数从哪个字段来，编码／大小写／市场后缀如何处理 |
| `start` / `end` | 是普通代码还是控制标记，是否清空／排序／发布字典；无证据则标未知 |
| 有效／失效债券 | 如何分类，`INVALID_CODE` 在哪个阶段产生，进入哪个数据结构 |
| 其它分组 | 是否也走同一慢路径，不能只处理一个分组却忽略另一个 |
| 名称与别名 | 同名不同代码、代码修改、别名覆盖的行为 |
| 刷新 | 重建还是追加，删除／失效／改名如何生效 |
| 构造与静态初始化 | 哪些入口会自动连接数据库或加载完整字典，探针必须避开这些入口 |

重点检查参数数量／位置和 JNI 包装转换。不要将 C++ 隐藏返回值参数或寄存器布局直接当成业务参数。

## 第二步：追踪真正的解析消费者

从 Linux `Java_com_sumscope_Parser_parseBond` 跟进到实际解析函数，追踪 `setInvalidCode` 和 `mapNameCode` 的读者，而不只看 `isCode`。不要预设它们是 Linux 性能热点。

至少回答：

1. 容器是否用于切词、代码／简称识别、失效标记、别名消歧、候选排序？
2. 假如只保留代码名称映射、把失效分组放到 Java，native 会缺少什么信息？
3. 这些信息能否在调用前／调用后由 Java **完整恢复**？是否影响返回候选及其顺序？
4. 不同生命周期中是否还需要 native 内部清空、删除或重建？
5. 机构／交易员的文本提取、CRM ID 匹配分别属于 native 还是 Java 服务层？不要把 CRM 查询与文本解析混为一谈。

最多详细展开 12 个新的直接相关例程；若仍不能定位消费者，列出下一步地址与原因后停止。所有判断分为“确认／推断／未知”，附短源码或地址证据。未发现读者不等于没有读者。

**不得未经证明把第四参数改为空、拆分 `INVALID_CODE` 分组、跳过到期债券或仅加载测试债券作为产品修复。** 可以把这些写成待验证假设，但不能据此修改服务。

## 第三步：给出明确决策

选择下列状态之一，并给出理由：

- `ADAPTER_FEASIBLE`：证据支持一个具体 Java/native 边界；说明 Java 接管什么、native 保留什么、每条调用如何变化。静态推断与实际验证分开列，不能仅凭小样本宣称全量业务等价。
- `JAVA_REPLACEMENT_NEEDED`：对所考察的边界，native 内部消费者使仅移走字典管理不能保持行为；给出最小可独立替换切片，例如“代码／名称识别及失效标记”，以及它依赖的输入／输出合同。不要笼统断言任何无源码方案都不可能。
- `INSUFFICIENT_EVIDENCE`：仍不能决定；列出至多 3 个具体缺口及下一步最低成本验证。不要编造肯定结论。

这一轮不实现完整 Java parser，不建立第二套微服务，不修改现有报文。若有充分证据且不需要改 DLL，可以在独立目录编写一个小型候选适配器；Java 8、无新增依赖，必须保留 native 路径用于对照。只有算法示例时注明未接入产品。

## 本轮执行限制：只做静态分析

本轮不执行 DLL／SO，不启动服务、JVM 功能探针或性能测试。下列探针条件仅保留为后续建议，不构成本轮运行授权；用户另行确认 Linux 验证环境和范围后再执行。Windows 探针不能证明 Linux 正确性或性能。

静态调查能回答问题就不运行探针。如必须验证某一明确假设，先确认入口不会隐式连库／全量加载，且已有工具可直接使用。确认不了则标记 `NOT_RUN`，不得冒险启动。

可选探针仅限：

- 最多 20 条合成债券、10 条合成文本，无真实 CRM 数据和数据库连接。
- 最多两个**顺序**启动的独立 JVM（旧库对照／候选各一个），禁止并行，禁止自动放大／重试。
- Java 堆上限 128 MB；单逻辑 CPU；外部监督进程在 10 秒内终止探针，必须覆盖 native 卡住的情况。无法落实保护则不执行。
- 仅调用已核对的直接 Parser/JNI 入口，禁止调用会触发全量加载的 `BondQuoteParser` 构造器；不附加运行服务、不污染它的 singleton。
- 测试有效／失效、代码／名称、歧义、重复插入、空串及可确认的刷新行为。按场景拆清输入和预期，不把 IsCode 成功当作 ParseBond 正确。
- 每项保存完整合成输入、原始返回值和字段差异，区分“与旧版一致”和“符合业务预期”。旧库也可能有缺陷，不自动修改预期使测试通过。
- 不测耗时、不循环测速，不将功能探针称为性能证明；失败不得自动追加第三次运行。

全量启动、真实性能测试、真实 GW／数据库／客户端联调仍需用户另行授权，本轮不执行。

## 交付内容

在本轮独立目录输出：

1. `result.zh-CN.md`：10 行以内；决策状态、最小替换边界、实际做了什么、哪些未测、下一步。
2. `boundary.zh-CN.md`：Java→JNI→解析消费者调用图；参数合同、分组读写及生命周期表；确认／推断／未知和证据位置。建议不超过 180 行。
3. `verification.json`：实际读取文件哈希，以及每项检查的名称、命令、退出码、`PASS/FAIL/NOT_RUN`、证据文件。已有历史测量明确标记为引用。
4. `tests/` 或 `candidate/`：仅实际制作的探针／候选代码；不存在就不造空文件。记录复现命令，不要求安装新依赖。
5. 若必须 Java 重写：列出第一切片的行为范围、最多 10 个优先兼容用例、仍需 native 的能力和验收方法。不要基于估计行数承诺工期或正确率。

禁止把下一轮计划写成已经完成的修复。完成后只通知结果目录，等待 Codex 评审；不修改第一轮证据，不提交／推送，不自行继续完整重写。
