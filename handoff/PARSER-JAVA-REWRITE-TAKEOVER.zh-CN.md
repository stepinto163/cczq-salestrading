# ParserSvr 重写接手记录

日期：2026-09-29。用户要求核查第三阶段，并由 Codex 帮助或接手剩余实现。

## 当前状态（后续验收更新）

下文保留接手时的阶段记录。当前已部署 Java 默认引擎，Windows/Linux 配置均已修改；原生引擎在隔离子进程中异步比较，业务返回 Java 结果。核心 221 项、服务 12 项和真实 STC 客户端端到端 33 项通过。全量参考快照约 5.6 秒，真实 DLL 双侧完整报文已记录。Linux 运行和全部旧规则等价性未验收，详见 [最终验证记录](../iteration1/cgws-ficc-parser/integration/validation-20260929.zh-CN.md)。

## 评审与接手

第三阶段主要修正比较工具的身份、超时和路径问题，没有新增解析能力。原候选独立构建 32 项测试通过，不足以认定重写完成。现由 Codex 接手核心实现，暂停给 CodeBuddy 追加该任务。

源码导入 iteration1/cgws-ficc-parser，原独立交付目录保留。运行中的 ParserSvr、原生库和配置未改变。本次未提交或推送。

## 本次实际实现

1. 反编译 Parsing JAR 的 Java 包装层，结合 Linux SO 的 MMParser/QuoteServer 符号、正则与调用逻辑核对假设。证据留在运行目录，不称其为完整 C++ 源码。
2. 使用小型合成字典、独立 128 MB / 1 CPU JVM、15 秒进程树看门狗调用 DLL。没有连接数据库或执行全库初始化。
3. 实现价格、期限、数量帮助方法，修复六/九位代码、已验证数量单位、+0/+1、成交词与数值回退。
4. 实现短码规则，通过实际原 Java 方法核对 19 项；测试反射跳过旧构造器的数据库与调度过程，注入候选集和年份。生产代码不使用 Unsafe。
5. 修复显式 SO 文件可绕过 Linux 工作目录检查的问题，增加回归。

## 验证

| 检查 | 结果 |
|---|---|
| JDK 8 Maven package | BUILD SUCCESS；194 项，0 失败/错误/跳过 |
| DLL 固定参考 | 142 项一致，包含拒绝和空结果边界 |
| 原 Java 短码方法 | 19 项一致 |
| 原基准回放 | 16 MATCH，退出码 0 |
| Linux SO 运行比较 | NOT_RUN；无 Linux/WSL 运行环境 |
| 服务/客户端切换 | 未执行，候选不完整 |
| 性能压测 | 未执行 |

日志：iteration1/runtime/parser-takeover/takeover-build.log。
回放：iteration1/runtime/parser-takeover/sample-replay/windows-sample-1.compare.json。
固定参考和采集源码随模块保留。

## 剩余顺序

### 后续进展：复杂消息与服务适配

已增加 `QuoteMessageParser`、`JavaBondQuoteEngine` 和 JDBC 参考数据加载器，多行/分号/标题/双边/中文数量/估值 BP 已有回归。用户明确要求修正旧版的 `2.4` 被拼接成 `2.41000` 及分号丢报价，不复制这两个缺陷。

核心 JDK 8 Maven install：221 项通过；外部 ParserSvr 全主源码 Java 8 编译通过；DPS handler 合同和 CRM 协议：9 项通过；两个真实 PostgreSQL 债券只读验证通过。完整服务 Maven package 因依赖下载超过 50 秒上限中止，不能称其打包通过。运行中的服务及配置没有改变。

外部服务改动已保留为模块内增量补丁；[接入说明](../iteration1/cgws-ficc-parser/integration/README.zh-CN.md) 含构建、引擎选择、可重复验证命令及边界。全量参考数据、完整 NCD/撤销/日期、Linux 同输入对照和 STC 客户端回归仍需验收。以下是接手时的原始顺序，其中多行和服务适配已推进。

1. 中文数值、完整预处理、多行/标题、短码与全码回退。
2. NCD、双边拆分、估值/BP/行权等可达规则，逐字段扩大旧实现对照。
3. 不依赖闭源 Parsing 的服务适配器，保留 DPS/CRM 合同，客户端回归。
4. Linux 同输入对照、完整加载链路验证；另行授权后做有界性能验证。

详见 [能力表](../iteration1/cgws-ficc-parser/coverage.zh-CN.md)。**全量替换和慢初始化问题尚未验收解决。**
