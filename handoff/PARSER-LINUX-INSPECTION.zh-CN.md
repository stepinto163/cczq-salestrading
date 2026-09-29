# ParserSvr Linux 原生库静态检查（2026-09-29）

## 结论

**本地 QA 包中的 Linux 库与 Windows DLL 的关键实现不同。不能把 Windows 约 16 分 36 秒的初始化测量，或失效债券分组的线性查重热点，直接当成 Linux 的问题。暂不据此启动 Java 重写。**

本轮完成 ELF、DWARF、反汇编和 Java 字节码检查；未启动 ParserSvr、未执行原生库、未连接数据库、未运行性能测试。尚未核对 Linux 服务器实际部署文件的哈希，也没有 Linux 初始化耗时证据。

## 文件身份与加载方式

- 分析文件：`D:/workspace/greatewall/ficc/qa_env/sc-app-stc-parsersvr/sc-app-stc-parsersvr/salt-formula/sc-app-stc-parsersvr/files/libparser.so`。
- SHA-256：`2a2486a1e468939d3581ab231b8aebc398185742aa6dabe8ad91770a78c245d7`。
- 大小 3,884,886 字节；ELF64、x86-64；含函数符号及 DWARF 调试信息。
- 调试信息记录 GCC 4.9.1、C++11，原构建路径包括 `/mnt/hurui/ParseLib/src/QuoteServer.cpp`、`ParseLib.cpp`、`MMParser.cpp`。这是路径和类型元数据，**不是已经找到原始源码**。
- 已检查本地 `ats_root/stc/ParserSvr/javalib/Parsing-1.8.7-SNAPSHOT.jar` 中 `com.sumscope.Parser`：Linux 分支调用 `System.load(System.getProperty("user.dir") + "/libparser.so")`；Windows 分支调用 `System.loadLibrary("ParseLib")`。因此 Linux 加载位置取决于工作目录，并非直接使用 ATS_ROOT。

## 已确认的实现差异

下列地址均为此 SHA 对应 ELF 虚拟地址，不是运行时绝对地址。

| 检查点 | Linux 证据 | 含义 |
|---|---|---|
| 失效债券写入 | `SetCodeName` 位于 `0x96864`；`0x96968` 比较第四参数与 `INVALID_CODE`，`0x96986` 调用 `std::set<wstring>::insert` | 不走 Windows 已发现的大分组 vector 线性查重路径；该 set 的插入比较次数为 O(log N) |
| 成员身份 | DWARF 标明 `setInvalidCode` 偏移 `0xb08`；与上述插入地址一致 | 不是仅根据函数名猜测容器用途 |
| 代码／名称映射 | `0x96912` 更新 `mapCodeName[code]`；`0x9693e` 读取 `mapNameCode[name]`，`0x96955` 调用 `AddCodeAndSort` | 同名代码候选仍使用 vector，不能宣称 Linux 完全没有线性工作 |
| 同名候选插入 | `AddCodeAndSort` 位于 `0x96690`，先扫描查重，再追加，并按 `GetCodeLevel` 比较交换 | 单次代价与同名候选数有关；若同名组异常大，仍值得检查，尚未测量 |
| 旧查重函数 | `AddIfNotExist` 位于 `0x96474`；扫描符号表中函数的直接 call/jmp，未发现对该函数或其 PLT 的调用 | 函数存在不等于初始化实际使用；未穷尽间接调用 |
| 装载起止 | `start` → `LoadCodeStart`（`0xaedbc`），清空 `newCodeSet`；`end` → `LoadCodeEnd`（`0xaee8a`），遍历代码映射并移除本轮缺失代码及名称候选 | 不是每次 start 都清空全部状态；刷新语义须单独审查 |

Java `BondQuoteParser.loadData()` 的字节码确认：失效分支调用 `SetCodeName(code, name, "", "INVALID_CODE")`。`end` 调用受 `FunctionConfig.CleanDataEnable()` 控制。Linux JNI 包装函数位于 `0x92df8`，转换字符串后调用上述 `QuoteServer::SetCodeName`。

解码 UTF-32LE 常量也确认：`0xf3ba4=start`、`0xf3bbc=end`、`0xf3bd0=INVALID_CODE`。没有把 C++ 隐藏返回值参数当成业务参数。

## 后续判断

1. 先核对 Linux 实际部署的 `.so` 和 Parsing JAR 身份；同名文件不代表同一实现。
2. 有 Linux 启动日志时，分开检查 SQL、Java 行转换、JNI 字典填充和 end 清理阶段。没有阶段计时就标未知，不能用 Windows 耗时代替。
3. 当前更适合审查 Linux 刷新语义和同名候选规模；没有 Linux 性能证据前，不将全量 Java 重写作为既定修复方案。

## 可复核证据

- 静态分析脚本：`D:/workspace/greatewall/ficc/qa_env/tests/inspect_parser_linux.py`。
- 输出目录：`D:/workspace/greatewall/ficc/qa_env/tests/parser-diagnostic/linux/`，含 `inspection.json`、`crosschecks.json` 及按函数分开的 `.asm`。
- Java 装载字节码：`qa_env/tests/parser-library-bytecode.txt`，关注 `loadData()` 中 `SetCodeName` 调用。
- 工具：pyelftools 0.33 和现有 Capstone，仅从 `iteration1/runtime/native-inspection-tools` 加载；不安装全局组件。

以上原生文件及详细证据在仓库外，本说明不表示它们已提交 Git。
