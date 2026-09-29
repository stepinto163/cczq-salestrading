# CodeBuddy：ParserSvr 初始化瓶颈的最小修复评估

状态：首轮已交付；未找到 native 源码，未提供产品补丁，未运行示例或性能测试。后续已升级为[完整 Java 解析器替换](CODEBUDDY-PARSER-JAVA-REWRITE.md)，不要重新执行本文件或重复源码搜索。本文件不属于 INDEX 自动领取队列。

## 可直接执行的任务

你负责评估 ParserSvr 原生字典初始化的最小优化方案，并在证据充分时提供一个隔离的候选补丁。目标是保持识别行为和数据范围，消除已发现的大分组线性查重成本。不要重做整个解析器，不扫描整个项目，不委派其它代理。

先读主仓库 `D:/workspace/greatewall/ficc/ficc/AGENTS.md`、`handoff/README.md` 和本文件。本文件是用户手工指定的任务，不需要去 INDEX 领取其它任务。

讨论使用英文，说明和交付报告使用中文。遵守 Java 8、STC 框架及现有协议，不增加替代服务或修改 STC 客户端。

## 基线与工作位置

- 主仓库：`D:/workspace/greatewall/ficc/ficc`。
- 交接时主仓库分支：`feature/ficc-native-salestrading`；源码参考提交：`c71fc4652d9fafbe4387908bc80a47620c801a4b`。
- **ParserSvr 的实际源文件、JAR 和 DLL 在仓库外的 qa_env 中，上述 Git SHA 不能证明这些文件的版本。** 记录实际读取文件的绝对路径及 SHA-256。
- 在 `D:/workspace/greatewall/ficc/ai-copies/cb-parser-init-20260929` 中工作。目录已存在时保留内容，先核对上次状态，不清空、不覆盖未知改动。
- 只复制用到的源码和二进制分析副本。不要复制数据库配置、密码、许可证、完整运行日志或整个 qa_env。
- 源目录与运行目录只读；不修改现有 JAR/DLL，不提交／推送／合并／部署。

本任务文档不代表 CodeBuddy 已经收到指令；不要启动 CodeBuddy 自动化或联系其他人员。

## 已有证据：不要重复花时间验证已确认事实

最新依据是 `native/README.zh-CN.md`，上一级较早的调查 README 只用于理解调查过程，其“尚未确认根因／尚未启动成功”等结论已被后续证据更新。

1. 启动 SQL 的已有 `EXPLAIN ANALYZE` 耗时约 323.529 ms，连接键已有索引，没有 JOIN 倍增。
2. Java 首次重复加载已经修正。修正后完整原生字典初始化仍耗时 995892 ms，约 16 分 36 秒；附加债券缓存约 830 ms。
3. DLL 副本 SHA-256：`d3df85c550e5ea388b1f112a1e1e2e6a9b73ed047d619b8336896f83e5403559`。其它版本必须重新核对地址，不能套用此地址。
4. `Java_com_sumscope_Parser_setCodeName` 在 `0x180014070`，调用内部函数 `0x180018930`。非空第四参数选择分组，再调用 `0x1800185e0`。
5. `0x1800185e0` 对已有字符串逐一比较，全部不相同时追加。布局与 MSVC x64 `vector<wstring>` 一致；向同一组插入 N 个不同代码会产生约 N×(N−1)/2 次比较。
6. 失效债券使用 `INVALID_CODE` 分组。数据库记录中约 439566 条已到期／摘牌，但前置分支可能影响实际入组数，不能将此数当作实测分组大小。
7. 真实 GW 识别债券及 CRM 机构／联系人曾经通过，不能为了启动快而删除这些能力。

这些是已有报告证据，不是你本轮的新测试结果。DLL 大约 245 KiB，已有反汇编识别 542 个函数块；其中包含模板和运行时代码。不要用估计的源码行数推导重写成本。

## 最小阅读范围

以下路径均在 `D:/workspace/greatewall/ficc/qa_env` 下：

1. `tests/parser-diagnostic/native/README.zh-CN.md`。
2. `tests/parser-diagnostic/README.zh-CN.md`。
3. `sc-app-stc-parsersvr/sc-app-stc-parsersvr/src/main/java/sc/app/stc/parser/utils/BondQuoteHelper.java`。
4. 同一 Java 包树下 `stream/ParserStream.java`、`job/ParserCacheJob.java`、`utils/ParserInitializationLog.java`。
5. `tests/parser-library-bytecode.txt`、`tests/parser-db-bytecode.txt`：只定位 `BondQuoteParser` 构造、`loadData`、`setCodeName` 和清空／释放调用。
6. `tests/parser-diagnostic/native/exports.txt`、`Java_com_sumscope_Parser_setCodeName.asm`、`function_180018930.asm`、`function_1800185e0.asm`。
7. 必要时参考 `tests/inspect_parser_native.py` 和 `tests/ParserNativeProbe.java`；它们是分析与探针，不是生产替代实现。

`native/all.asm` 只作为按地址查找的索引，不全量塞入上下文。JAR 参考位置：`ats_root/stc/ParserSvr/javalib/Parsing-1.8.7-SNAPSHOT.jar`。先校验文件存在和哈希；缺失则写明具体文件，不猜测内容。

## 执行顺序

### 1. 查找源代码，限制搜索范围

先只在 ParserSvr 源目录和 `qa_env/sources` 中查找 `ParseLib`、`Parsing`、`setCodeName` 相关 C/C++ 文件或 PDB 的文件名／符号。DLL 内已有路径线索：`stc-lite/ipservice/Parsing/ParseLib/prj_win/x64/Release/ParseLib.pdb`。

不要扫描整个磁盘或遍历所有 Git 仓库。一次有范围的搜索后，若没有匹配源码，就记录“未找到”，进入静态分析。发现候选源码不等于与当前 DLL 匹配；说明版本依据及未确认之处。

### 2. 查清分组的读写语义

以已定位函数为起点，追踪直接调用关系；本轮最多详细分析 8 个相关例程，超过则列出尚需追踪的地址，交回评审。

回答：

- 分组是否参与识别、失效标记、候选消歧或仅用作查询索引？
- 读取方是否依赖插入顺序、唯一性、字符串原始大小写、市场后缀或编码？
- 初始化、再次加载、单条修改、删除、清空、release 分别如何维护分组？
- 是否共享原生 singleton？有哪些实际线程安全依据？不以“Java 方法 synchronized”推导所有 native 入口都安全。
- Java/JNI 边界是否允许替换该分组管理？如果下游 native 识别仍读取其内部容器，必须明确指出不能只在 Java 端加 HashSet 就解决问题。

每个关键判断列出源码行号或地址／短指令证据，标为“确认”“推断”或“未知”。反编译结果只能称伪代码，不能称恢复了完整源码。

### 3. 提出一个最小候选方案

有匹配的原生源码时，优先评估“保留有序容器＋新增哈希集合做存在性判断”。同时考虑删除、清空、刷新、字符串相等语义、异常后的双结构一致性与内存开销；不能只优化 add 而留下失效索引。

只有源码和语义足够明确时，才在独立副本制作候选补丁，可进行局部编译及小型功能单元测试。不要改变导出函数、JNI 签名、字典范围、解析结果顺序或生产构建配置。

没有匹配源码时，提交带证据的方案和阻塞点即可。可以写独立的容器算法示例来验证语义，但必须标明“示例、未接入 ParseLib、不能证明生产加速”。**不补丁修改 DLL 指令，不全面反编译／重写成 Java。** 如建议 Java 替换，明确替换边界、仍依赖的 native 功能和行为等价验证成本，等待下一轮决定。

### 4. 准备兼容性验证

复用现有测试方式，不创建新测试平台。小型容器功能测试使用最多 20 个合成代码，无数据库、网络或性能计时。至少覆盖：同组重复、不同组同代码、顺序保持、大小写和市场后缀、中文／UTF-16、清空后重建、重复刷新及可用的删除／修改操作。未知旧行为先标待确认，不能自己发明预期。

识别层准备以下回归清单：代码和简称、有效与失效债券、歧义名称、方向／价格／数量／清算速度、多条报价、机构／交易员、非法文本、字典刷新。只使用已有证据确认过的输入／输出作为旧版本对照；新输入的预期没有确认就标记未验证，不声称能够真实识别。

## 本轮不能做的事

- 不启动或重启 ParserSvr、STC 客户端及其它服务，不附加 Java agent 到运行进程。
- 不执行全量初始化、性能基准、压力测试或百万级循环，不增加线程并发调用 native 库。
- 不查询／修改共享数据库、不减少历史债券、不切换模拟数据、不改 SQL 过滤范围或延长超时后声称加速。
- 不增加 Maven 依赖，不替换 SDK、JDK、CRM 或报文协议；不提交二进制、真实业务数据和凭据。

性能测试需用户另行明确授权。可以写下一轮测量方案：相同数据、独立进程、CPU／堆限制、外部 watchdog、初始化各阶段耗时、峰值内存、正确性对照及清理方式；本轮不执行。无需重新跑已有的 16 分钟基线。

## 交付与停止条件

在独立工作目录中输出：

- `result.zh-CN.md`：摘要不超过 10 行，明确能否最小修复、是否需要源码、下一步建议。
- `evidence.zh-CN.md`：文件哈希、已确认调用链、消费者与生命周期表、关键证据及未知项；建议不超过 150 行。
- `candidate.patch`：仅有实际源码修改时提供，基于已记录哈希的复制文件；没有修改则不创建空补丁。
- `tests/`：实际新增的少量功能测试（如有），附准确命令、退出码、结果；编译通过与原生运行通过分开写。
- 下一轮性能验证计划，注明 `NOT_RUN`，以及未覆盖的业务行为。

不能把算法复杂度估算写成实测加速倍数；不能把 Java 容器示例通过写成 ParseLib 已修复。记录已有发现与本轮新增发现，避免 Codex 重复阅读长日志。

最多两轮有明确依据的局部构建／功能修正尝试。源码缺失、语义无法证明、需要改变业务行为或超出 8 个例程分析范围时，交回具体阻塞点，不扩大任务。完成后告诉用户结果目录，等待 Codex 评审；不自行继续下一阶段。
