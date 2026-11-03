# 历史报价查询与页码分页

## 业务口径

`ficc.quote.history.search` 查询指定日期范围内，每条报价每天最后一次操作后的状态。先选最后版本，再判断正式报价状态、权限和筛选条件，不能因旧版本符合条件而返回旧价格。相同时刻以 revision 较大者为准。行 tradeDate 取 operationTime 的日期部分，不是 quote.tradeDate。

已确认正式报价按现有共享规则可见；未确认输入及直接丢弃的草稿不进入历史。ownerScope 默认 VISIBLE，MINE 限定本人。债券属性、关注债券和筛选分组使用公共 QuoteMatcher，基础资料采用查询时的值。

## 分页协议

2026-10-21 起，历史报价筛选使用页码分页，不再使用游标：

```json
{"requestId":"history-1","fromDate":"2026-10-01","toDate":"2026-10-20","pageNumber":2,"pageSize":50,"ownerScope":"VISIBLE","filter":{"market":"RATE"}}
```

成功响应 Data：

```json
{"pageNumber":2,"pageSize":50,"totalCount":51,"hasMore":false,"rows":[{"tradeDate":"2026-10-20","quote":{"quoteId":"Q-051","securityId":"240210.IB","status":"ACTIVE"},"reference":{"securityId":"240210.IB","securityName":"24国开10"}}]}
```

上例共 51 条，第 2 页包含最后 1 条；行内 quote/reference 字段为节选。

- pageNumber 默认 1，最小 1；pageSize 默认 50，范围 1–1000。
- 每次请求携带完整条件，可直接跳页。totalCount 是筛选后的全部匹配数量。
- 越界页返回空 rows，保留 totalCount，hasMore=false；参数非法返回 1001。
- 不接受 cursor/limit，不返回 nextCursor；移除原 10,000 条结果缓存上限。
- 客户端增加与今日报价一致的分页控件；日期、文本及侧栏条件变化回到第 1 页。客户端不再对已分页结果二次过滤，Bid/Ofr 计数标明“本页”。

这是无状态页码查询：同一次请求的计数与数据基于同一个数据库查询快照，但不同页之间不冻结历史记录。并发新增/修改历史可能让行跨页移动；需要查看最新结果时重新查询。单笔报价变更历史 history.query 的 snapshotRevision 机制不受影响。

## 服务端处理

1. PostgreSQL 按日期缩小范围，选出每日最后版本，再应用正式状态和权限。已有 history_owner_time_idx、history_time_idx 索引由 PgStore 初始化维护。
2. 仅日期/归属筛选时，SQL 在同一语句中计算总数并 LIMIT/OFFSET，只返回本页。
3. 仅报价自身条件（价格、数量、方向、计价方式、清算速度及标志等支持的运算符）时，将条件下推到每日最后版本之后，在同一SQL中筛选、计数、分页；只向Java返回本页。
4. 市场/DM债券条件复用原引擎的命中券集；关注债券和筛选分组按原来的并集/交集关系形成必要券集。将券集、MINE和可支持的报价条件下推到每日最后版本之后，缩小数据库返回范围；剩余条件继续执行公共QuoteMatcher。空券集直接返回空页；未支持的同字段OR条件整组不下推，避免误排除。
5. 复杂筛选仍以128行批次读取预筛后的快照、准确计数，仅保留本页。综合搜索、当前期限/估值差等依赖当前资料的条件保留原有匹配口径；没有把当前资料改成历史快照资料。
6. 排序沿用每日首次操作时间升序，再按操作日期和 quoteId 排序，保证同一数据集顺序稳定。

数据库仍需按日期选取每日最后版本；非选择性条件、复杂条件、准确计数及深页OFFSET仍有成本。此次主要减少跨数据库传输和Java反序列化，不宣称已消除海量历史的数据库扫描成本。

新增INFO日志HISTORY_QUERY包含user、mode、page、transferredRows、candidateSecurities、totalCount、returnedRows、elapsedMillis。transferredRows是数据库实际返回到服务的快照数，不是数据库内部扫描行数。准备期间资料版本变化返回NOT_READY，避免混用两版候选券集与匹配器。

2026-11-03验证：Common 103项通过、1项既有跳过；QuoteSvr 16项通过；原历史数据库功能检查188项、新差分检查486项通过。新检查用原路径与优化路径比较原始快照内容、顺序、计数和分页，包含空价量、零值、边界、同字段OR、旧版本不能复活、归属变化、缺资料及真实原版DM引擎匹配。功能样本中选择券集使传输由11条降到4条；简单条件页大小2时只传2条。此为功能数据量检查，不是耗时性能结论。

用户随后明确批准小样本性能测试，结果见下节；未增加数据规模或并发。生产/QA尚未部署。

## 发布与验证

页码协议变更时，common、QuoteSvr、STC客户端需配套升级，旧limit/cursor客户端不兼容。
2026-11-03性能改动只涉及Common与QuoteSvr：先构建安装Common，再构建QuoteSvr；客户端协议、BaseSvr、同事BondSearch Core均不变，不增加数据库表或迁移。历史功能脚本会依次运行原功能回归和新增差分回归，各自有30秒看门狗。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/build-server.ps1 -Module quotesvr
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-history-search.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/test-client-integration.ps1 -BuildRoot <客户端构建目录>
```

数据库功能测试使用独立随机 schema 和少量历史事件，结束后删除测试 schema，独立进程有 30 秒看门狗。覆盖每日版本选择、权限、组合筛选后分页、直接跳页、末页/越界页、参数校验及旧游标拒绝。客户端集成测试覆盖请求字段、行内 reference、分页绑定、筛选/日期复位及失败重试。此类功能验证不等于真实服务桌面端到端验证。

## 2026-11-03授权后的小样本耗时对比

- 固定1000条操作历史＝500条报价各2个版本，100只模拟债券，1个串行客户端，页大小50。真实本机PostgreSQL、原版BondSearch引擎；资料与DM字段为可重复生成样本，不是QA全量资料。
- 基线从Git提交5102821直接提取LocalSearchSvrClient，仅重命名测试类；优化源码为83be097。共用同一数据库、资料和当前依赖，比较本次历史查询实现变更，不是两个完整部署环境比较。
- 每场景/每路径预热3次、测量20次，交替新旧执行顺序。时间覆盖Java searchHistory到本机数据库返回及本页资料填充，不含网关传输、UI绘制、启动装载。P95使用20样本的第19个有序值，仅作小样本描述。

| 条件 | 优化前中位数(ms) | 优化后中位数(ms) | 中位数耗时变化 | 优化前P95(ms) | 优化后P95(ms) |
|---|---:|---:|---:|---:|---:|
| 无筛选（对照） | 4.55 | 4.71 | 增加3.6% | 5.13 | 6.61 |
| Bid价格区间＋数量 | 7.48 | 4.22 | 降低43.6% | 11.29 | 5.71 |
| 利率债市场 | 7.13 | 5.85 | 降低17.9% | 8.14 | 8.09 |
| DM债券评级＋价格 | 7.13 | 4.30 | 降低39.7% | 9.13 | 5.75 |
| 关注债券 | 6.00 | 2.79 | 降低53.5% | 6.75 | 3.23 |
| 综合文本（保留原匹配） | 6.80 | 6.87 | 增加1.1% | 9.90 | 12.99 |

全部138组成对请求（含预热）的完整返回内容一致，包括计数、顺序、报价、reference和页信息。无筛选及文本路径没有改善；不能凭这一轮20个样本把P95上升定性为稳定回归，也不能忽略该结果。主要收益来自支持下推的条件。

进程耗时3.38秒，最大堆256MB、2个处理器、单SQL限时5秒、独立进程60秒看门狗；开始可用内存7446MB。临时schema已清理，日志没有WARN/ERROR；未修改运行服务或业务数据。本结果不能外推到百万历史、并发请求或QA端到端耗时。

复现入口（须另获运行授权，不自动执行）：`python iteration1/scripts/benchmark-history-query.py --run`。固定相同小负载且有单实例锁；先确保Common和QuoteSvr当前源码已编译。原始数值摘要含每次样本：[JSON记录](performance/history-query-1000-20261103.json)。本机原日志位于iteration1/runtime/history-benchmark-20261103-095455，不随文档上传。
