# ParserSvr Java 引擎接入

## 代码与选择

核心入口为 `QuoteMessageParser`，服务边界为 `BondQuoteEngine`。现有 ParserSvr 的 `BondQuoteHelper` 调用该边界，`ParserManager`、DPS handler 使用明确的 `AllQuote` / `BondQuote` 模型。`stc.parser.bond` 请求与应答字段保持原样，CRM 后处理继续使用原有 CRMSvr 链路。

外部服务源码位置：`qa_env/sc-app-stc-parsersvr/sc-app-stc-parsersvr`。本次修改已在该目录实现；`parsersvr-java-engine.patch` 只保留本轮增量，基线是本机已接入 FICC CRM 的版本，不是原始上游 HEAD。应用前在服务仓库运行 `git apply --check --ignore-space-change <补丁路径>`，确认通过后 `git apply --ignore-space-change <补丁路径>`，兼容 Windows 换行。已应用的本机目录不要重复应用。补丁不含数据库配置、密码或完整反编译厂商源码。

服务的 `control/ParserSvr/streamCfg.ini` 中设置：

```ini
ParserEngine=java
```

Windows 配置、Linux salt 模板及缺省值均为 `java`，本机运行包已部署并重启。Java 初始化失败会抛出异常，不会切回 native。`ficc.startup` 的 `COMPONENT` 日志包含服务、组件、状态、模式、实现、数据源名和耗时；`INITIALIZED` 仅表示本地参考数据快照初始化成功。

Java 模式将同一输入同时交给 Java 主引擎和异步原生比较队列，业务回复始终采用 Java 结果。原生引擎在独立 JVM 中初始化和执行，避免 DLL/SO 崩溃拖垮服务；Java 请求不等待原生初始化。显式 `ParserEngine=native` 仍可用于人工回退。

`ficc.parser.compare` 在 INFO 输出完整输入、两侧原始结果和 comparisonId，事件前缀为 `PARSER_COMPARE`，状态包括 INPUT、OUTPUT、MATCH、DIFFERENT、FAILED。Java 输出在 CRM 后处理前拍快照。主日志位于 `ATS_ROOT/log/ParserSvr.log`，原生子进程诊断位于 `ATS_ROOT/log/ParserSvr-native-shadow.log`。

比较队列容量为 1024，单线程串行执行；溢出明确记录 ERROR/QUEUE_FULL_OR_CLOSED，输入仍留在日志，但不会自动重放。该队列不持久化，重启会丢失待比较任务；它不是业务消费队列。原生启动等待上限 1200 秒、单次解析 15 秒，失败不改变 Java 回复。父进程停止后子进程退出。

Java 引擎通过服务已有的 STC DBPool 读取 `bond_list_info`、`bond`、`china_bond_valuation`，不增加凭据文件。BNC 使用旧解析器默认子类集合 BGB/SGB/LLB/EGB/TLB/SCB/PBB/PSB；短码候选仅 BGB/PBB/PSB，两者不可混用。估值按日期读取最新非空值；尚未增加交易日有效期判断。刷新完成后一次替换快照，失败保留旧快照并向调用者传播异常。

## 构建

1. 用 JDK 8 对本模块执行 `mvn install`。
2. 对外部 ParserSvr 执行 `mvn package`，其 POM 已增加 `cgws.ficc:cgws-ficc-parser:1.0.0-SNAPSHOT`。
3. 包内 `javalib` 必须包含新解析器 JAR。原 `Parsing` JAR 暂时保留，native 模式与其他旧 Java 工具仍依赖它。

本机 `qa_env/build-parser-local.py` 已补充先构建并复制 Java 引擎再编译 ParserSvr；对应增量见 `local-builder.patch`（在 qa_env 目录应用）。这个脚本会更新运行包，不是只读检查。本轮使用其 ParserSvr 构建步骤完成本机 JAR 部署。

## 可复现验证

仓库根目录运行，所有参数可替换成实际路径：

```powershell
python iteration1/cgws-ficc-parser/tools/check-service.py `
  --service ../qa_env/sc-app-stc-parsersvr/sc-app-stc-parsersvr `
  --runtime ../qa_env/ats_root/stc/ParserSvr `
  --java-home iteration1/runtime/tools/jdk8/jdk8u504-b01 `
  --local-database-config ../qa_env/ats_root/stc/ParserSvr/config/database.xml
```

先按模块 README 构建核心 JAR。脚本用 QA JAR 编译服务全部主源码和测试，执行现有 DPS handler 合同、CRM 协议测试及可选 JDBC 只读检查；不部署、不启动服务。JVM 限制 192/256 MB、1 CPU，单进程看门狗 20/45 秒。数据库检查只允许本机，只查 240210、240215 两个债券，不代表全库加载验收。报告保存在 `target/service-check/<时间>/`。

本轮结果：核心 221 项、服务 12 项均通过，两个真实债券 JDBC 检查通过；实际 STC 客户端端到端 33 项通过。完整服务 Maven 打包尝试因依赖下载超过 50 秒看门狗而中止；Java 8 全源码编译和本地 JAR 部署已通过，不能将其写成 Maven 发布包通过。详见 [验收记录](validation-20260929.zh-CN.md)。

## 行为与剩余验收

```text
240210 2.5/2.4 1000;240215 2.3 2000 bid
```

应返回三条：240210 Bid 2.5 / 1000、240210 Ofr 2.4 / 1000、240215 Bid 2.3 / 2000。分号后的报价不丢失，数量不拼进卖价。多行标题、机构/交易员标题、中文数量等见 `QuoteMessageParserTest`；25 个旧包装层消息参考样本中，16 个作为一致性断言，其他行为单独测试，不声称全部等价。

已完成本机全量参考数据初始化、实际 GW→ParserSvr→CRMSvr→STC 客户端回归及真实 DLL 双引擎日志对照。尚未完成 Linux SO 运行验收、完整 NCD/撤销/复杂日期及全部旧词典规则。整个服务尚未移除旧 Parsing JAR。没有执行性能压测。
