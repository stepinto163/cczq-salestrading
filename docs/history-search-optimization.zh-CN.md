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

本轮未获得小样本性能测试的单独确认，未运行延迟基准/并发压测，未声称具体提速比例；生产/QA尚未部署。

## 发布与验证

页码协议变更时，common、QuoteSvr、STC客户端需配套升级，旧limit/cursor客户端不兼容。
2026-11-03性能改动只涉及Common与QuoteSvr：先构建安装Common，再构建QuoteSvr；客户端协议、BaseSvr、同事BondSearch Core均不变，不增加数据库表或迁移。历史功能脚本会依次运行原功能回归和新增差分回归，各自有30秒看门狗。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/build-server.ps1 -Module quotesvr
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-history-search.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/test-client-integration.ps1 -BuildRoot <客户端构建目录>
```

数据库功能测试使用独立随机 schema 和少量历史事件，结束后删除测试 schema，独立进程有 30 秒看门狗。覆盖每日版本选择、权限、组合筛选后分页、直接跳页、末页/越界页、参数校验及旧游标拒绝。客户端集成测试覆盖请求字段、行内 reference、分页绑定、筛选/日期复位及失败重试。此类功能验证不等于真实服务桌面端到端验证。
