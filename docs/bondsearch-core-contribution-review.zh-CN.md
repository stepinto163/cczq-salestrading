# BondSearch Core 回贡献审阅清单

日期：2026-11-01。本页保留当时的差异盘点和回贡献提案。后续决定已改为：集成目录完整恢复同事原版，不保留任何Core修改（包括性能优化），业务服务适配原接口。恢复及验证结果见同日任务备忘；下文不再是待实施方案。

当时讨论范围：只做最小性能优化；不改变已定义业务逻辑，不删除或改变任何公共接口。以下完整差异清单用于说明历史，不能理解为全部待修改事项。

## 1. 原版与比较范围

- 同事仓库：`D:/workspace/greatewall/ficc/ficc/colleague_20261029/cgws-ficc-saletrading-bondsearch-core`。
- 同事基线：`a1af7eef51e22f803e9fb57ab20b45343b0c58dc`。本地工作区无已跟踪文件改动，`main`和本地`origin/main`引用均包含此提交。本轮未联网确认远端最新状态。
- 我们的集成目录：`D:/workspace/greatewall/ficc/ficc/iteration1/cgws-ficc-saletrading-bondsearch-core`；盘点版本为主仓库`8b5289f`。
- 原版已经有Git历史，不应重新制造一个包含我们修改的“原版导入提交”。正式工作分支应从经核实的同事基线创建，保留其作者和历史；如果另一个交付仓库确实没有原版，先单独提交未经修改的原版，再开始提案。
- 比较原版与集成目录的已跟踪文件，统一换行后有33个文件差异，其中30个位于src；包括新增、未纳入/移除、行为修改以及纯排版。`pom.xml`未变。原版仓库里的文件仍在，不能把“集成目录缺少”描述成同事仓库已被删除。

## 2. 三个历史提交不能直接作为回贡献补丁

| 提交 | 实际范围 | 回贡献处理 |
| --- | --- | --- |
| `afd06b4` | 把原版代码和我们的修改一起导入主仓库；还改动Common及业务服务 | 对照同事基线重新提取小补丁，不能整体cherry-pick |
| `d1c5e64` | 删除API实现、转发、会话接口及专属测试，重写宿主 | 不提交此删除方案；在原版之上保留API和测试 |
| `b2e31c0` | Core增加候选索引和适配入口；同一提交还含QuoteSvr锁优化 | 分开服务优化与库提案，不能把整个提交推到Core仓库 |

## 3. 缩小后的唯一性能提案

减少`src/main/java/ficc/bondsearch/index/RangeIndex.java`构建时的数组扩容复制。原版随ordinal增加反复分配并复制数组，拟改为预留容量，同时单独维护有效长度。

- 保留全部原公共方法、参数、返回类型和异常行为。
- `collect`只访问有效长度，预留空间不产生额外命中；构建中查询、稀疏ordinal、pad扩大/小于已有长度、pad后继续add均需与原版一致。
- 不捆绑现有集成版的NaN填充变更：原版未显式写入槽位的实际行为也保留。明确写入的NaN继续按原版跳过。
- 只改这一实现及必要的等价性测试，新增说明用中文，不重新排版其他文件。
- 功能验证逐操作对比原版结果；性能收益需单独验证，不用未实际运行的数据证明。未经授权不启动性能测试。

原盘点中的缺失数值修正（P1）、BitSet副本（P3）、查询固定快照（P4）、JDBC一致性事务/保存点（P5）全部退出本轮。它们不是这项最小性能优化的前提，也不以“顺手修复”的方式提交。

本轮尚未实现或验证该提案；现有集成版测试不能代替在原版上的兼容性验证。

## 4. 已排除及需讨论的语义/兼容性变化

用户已明确：原版明确定义的布尔缺失按false、market允许NCD两项规则保持不变。回贡献不包含这两类行为变更，不将其标记为原版bug；是否调整由库所有者自行决定。本决定限定本次回贡献，不自动修改当前运行的FICC集成版。

| 项目 | 原版约定 | 我们的集成变化 | 建议 |
| --- | --- | --- | --- |
| 布尔缺失值 | `BondRow`注释明确“缺失按false”，字段及getter使用boolean | 改为Boolean/null；修改装配、presence索引及EQ/EXISTS | 已排除：保留原版。公共字段/getter改型还有二进制兼容风险；不作为P1附带提交 |
| NCD市场 | `market`支持RATE/CREDIT/NCD | NCD的market变为CREDIT，productType仍为NCD | 已排除：保留原版NCD市场；FICC自身口径如需转换应独立设计适配，不改变共享库 |
| 属性适配 | 原版无`BondReferenceAttributes` | 增加对FICC Common的SecurityAttributes/ReferenceData等契约依赖 | 优先留在我们自己的服务或适配模块；若要进入Core须与所有者确认扩展契约 |
| 候选索引 | 原版无`CanonicalBondCandidateIndex` | 从最终合并资料建枚举/布尔候选集；只预筛支持的条件，完整匹配仍在服务端 | 优先作为FICC适配代码复用原索引API；不要直接把应用依赖带回共享Core |
| 宿主装配及刷新通知 | 原版宿主同时支持请求分发和独立服务使用 | `attach/attributes/onReload`替代原宿主入口 | 应在FICC另建装配层，或与所有者设计向后兼容扩展；不能通过删除旧能力实现 |
| 启动/刷新日志 | 原版已有启动和刷新失败行为 | 改动初始化包装、失败清理及日志文字 | 本轮排除，不顺便改日志；获准性能修改新增说明才使用中文 |

## 5. 必须保留的原版能力

- 保留`BondSearchService`、`BondSearchForwarder`、`SessionResolver`、`SyncRequestSender`。
- 保留`BondSearchHost`的原公共构造函数、`matchBondScope`、`bondRows`、`dispatch`、`snapshot`和原有生命周期约定。
- 原版已有`ficc.bondsearch.query`、`ficc.bondsearch.catalog`、`ficc.bondsearch.facet`，并有调用`ficc.quote.hitset.query`的配套逻辑。FICC不使用这些接口不能作为删除依据。
- 保留原API、分页、转发、拆分、命中集测试，以及`BondSnapshotScopeAndFacetTest`中被移除的securityConditions检查。
- 保留原`benchmark/BondSearchBenchmark.java`和`.gitignore`。我们不执行压测、不在自己的包里使用某工具，不代表可以从同事仓库删除它。
- 不带入纯getter重排、整文件格式化。`BondSearchResult.java`本次差异为排版，无需回贡献。
- 我们新增的`Jenkinsfile`和README包含FICC自己的构建/部署说明，不覆盖同事仓库交付方式；先保留在FICC仓库。

## 6. 中文注释与消息要求

核实到`BondSearchHost.java`原有中文类说明、启动/停止/接口说明被缩减，类说明改成英文；“债券搜索宿主启动失败”也被改为英文。后续提案必须保留原中文说明；必要修订仅针对已同意的行为，不能静默删除。

新增或修改的注释、警告、错误描述及测试断言说明使用中文。协议topic、字段名、错误码、配置键和日志检索键不翻译。原版就存在的英文文案不进行未经同意的全库替换。

拟用中文消息示例（本轮仅列方案，未改代码）：

| 场景 | 中文描述 |
| --- | --- |
| DM读取失败 | DM债券快照加载失败 |
| 保存点创建失败 | 无法创建评级分片查询保存点 |
| 分片回退事务恢复失败 | 评级分片缺失后恢复事务失败 |
| 配置值非法 | bondsearch.mode仅支持embedded或disabled |
| 字段映射未配置 | 未配置该DM字段映射或分类规则 |
| 后台刷新失败 | DM筛选快照刷新失败，继续保留上一版快照 |

异常原因、堆栈及原错误码继续保留；日志检索键不因中文化而失效。原注释中有关线程复用/返回值的约定需要结合P3准确补充，不能仅翻译一句话而丢掉使用限制。

## 7. 文件级差异清单

以下路径相对于两份Core工程根目录；用于定位，不是待整体应用的文件清单。

| 文件 | 当前差异 | 拟处理 |
| --- | --- | --- |
| `.gitignore` | 集成目录未纳入 | 保留同事原版 |
| `Jenkinsfile` | 新增 | FICC构建配置，不默认回贡献 |
| `README.md` | 新增 | FICC整合说明，不替代原库说明 |
| `src/main/java/ficc/bondsearch/BondReferenceAttributes.java` | 新增 | FICC适配层候选 |
| `src/main/java/ficc/bondsearch/BondRow.java` | 修改 | 保留原布尔契约/NCD说明；排除无关排版 |
| `src/main/java/ficc/bondsearch/BondRowView.java` | 修改 | 保留原布尔公共返回类型；排除无关排版 |
| `src/main/java/ficc/bondsearch/BondSearchForwarder.java` | 移除 | 保留原版 |
| `src/main/java/ficc/bondsearch/BondSearchHost.java` | 改写 | 保留原API/中文注释，FICC装配另行设计 |
| `src/main/java/ficc/bondsearch/BondSearchService.java` | 移除 | 保留原版 |
| `src/main/java/ficc/bondsearch/CanonicalBondCandidateIndex.java` | 新增 | FICC适配层候选 |
| `src/main/java/ficc/bondsearch/SessionResolver.java` | 移除 | 保留原版 |
| `src/main/java/ficc/bondsearch/SyncRequestSender.java` | 移除 | 保留原版 |
| `src/main/java/ficc/bondsearch/benchmark/BondSearchBenchmark.java` | 未纳入 | 保留原版，不自动运行 |
| `src/main/java/ficc/bondsearch/engine/BondSearchEngine.java` | 修改 | 保留原版，本轮不回贡献P4 |
| `src/main/java/ficc/bondsearch/engine/BondSearchResult.java` | 仅排版 | 不回贡献 |
| `src/main/java/ficc/bondsearch/engine/BondSnapshot.java` | 修改 | 保留原版，本轮不回贡献P3 |
| `src/main/java/ficc/bondsearch/index/RangeIndex.java` | 修改 | 仅提取扩容性能方向，重新实现保持原行为的最小补丁；不直接复制集成版 |
| `src/main/java/ficc/bondsearch/load/BondSnapshotBuilder.java` | 修改 | 布尔presence变更不回贡献 |
| `src/main/java/ficc/bondsearch/load/OnshoreFilterTableLoader.java` | 修改 | 保留原版，本轮不回贡献P5 |
| `src/main/java/ficc/bondsearch/load/OnshoreRowAssembler.java` | 修改 | 保留原布尔、含权缺失和NCD规则 |
| `src/main/java/ficc/bondsearch/query/BondQueryExecutor.java` | 修改 | 布尔EQ/EXISTS变更不回贡献 |
| `src/test/java/ficc/bondsearch/BondSearchCriteriaTest.java` | 移除 | 保留原版 |
| `src/test/java/ficc/bondsearch/BondSearchForwarderTest.java` | 移除 | 保留原版 |
| `src/test/java/ficc/bondsearch/BondSearchHitSetTest.java` | 移除 | 保留原版 |
| `src/test/java/ficc/bondsearch/BondSearchHostTest.java` | 改写 | 保留原API覆盖，新扩展另加测试 |
| `src/test/java/ficc/bondsearch/BondSearchPagingTest.java` | 移除 | 保留原版 |
| `src/test/java/ficc/bondsearch/BondSearchSplitTest.java` | 移除 | 保留原版 |
| `src/test/java/ficc/bondsearch/BondSearchStartupLoggingTest.java` | 新增 | 本轮不回贡献 |
| `src/test/java/ficc/bondsearch/BondSnapshotScopeAndFacetTest.java` | 删除一个测试 | 保留原版检查 |
| `src/test/java/ficc/bondsearch/CanonicalBondCandidateIndexTest.java` | 新增 | 跟随候选索引归属 |
| `src/test/java/ficc/bondsearch/OnshoreLoaderSmoke.java` | 新增 | 本轮不回贡献、不运行 |
| `src/test/java/ficc/bondsearch/OnshoreRowAssemblerTest.java` | 修改一处预期 | 保留原NCD预期 |
| `src/test/java/ficc/bondsearch/SnapshotIntegrityTest.java` | 新增 | 不直接回贡献；为最小扩容补丁另写等价性测试 |

## 8. 依赖与后续步骤

1. 用户审阅差异及提案，明确哪些可提交给所有者，哪些留在FICC。
2. 再核实GitLab远端基线，在同事仓库的独立工作分支实现已同意的小补丁；不直接修改main、不重写原版历史。
3. Core原有API依赖同事Common的协议模型；我们的Common曾移除相关契约，因此不能只复制回Core几个类就声称恢复兼容。选定匹配的Common基线，并单独盘点需要恢复/保留的契约；不顺手修改另一个所有者的库。
4. 每个提交保留原API/中文注释，补充独立测试；需要FICC的适配变化在FICC仓库独立提交。
5. 推送提案分支并创建MR须以确认后的范围和目标仓库为准，由库所有者审阅。批准后再重新构建QuoteSvr/NotificationSvr并验收。

本轮实际验证仅为：原版Git基线/工作区检查、33项文件差异及关键行为静态核查。未修改产品代码、未恢复API、未执行新补丁测试、未确认GitLab远端最新状态、未推送MR。差异原始材料位于`.tmp/bond-core-review/runtime/`，仅为比较证据，不是可直接应用的补丁。
