# 历史报价查询优化

## 业务口径

`ficc.quote.history.search` 查询指定日期范围内，每条报价每天最后一次操作后的状态；先选最后版本，再应用筛选条件，不能因旧版本符合条件而返回旧价格。相同时刻以 revision 较大者为准。行 tradeDate 沿用 operationTime 的日期部分，不是报价的交易日期。

普通用户只能查本人历史，管理员及内部用户保留原有权限。债券属性和关注/筛选分组仍由公共 QuoteMatcher 匹配，使用查询时的基础资料。接口字段、客户端调用和权限口径不变。

## 本次实现

- PostgreSQL 按日期和归属人缩小范围，再按操作日期、报价 ID 选最后一版，只向 Java 返回每日快照，不返回全部 before/after 历史。
- 新增 history_owner_time_idx（归属人表达式、操作时间）及 history_time_idx（操作时间）索引。通过 PgStore 初始化自动创建，兼容已有表，无需修改历史数据。
- JDBC 在单条查询的数据库快照中以 128 行批次读取。独立只读事务在成功及失败时均释放；不提交调用方已有事务。
- 业务筛选后才补充返回行的基础资料；超过现有 10,000 条结果上限时立即报错，不继续构造完整结果。
- 页大小在数据库读取前校验；展示顺序沿用每日首次操作时间升序，并增加日期和报价 ID 作为稳定次序。

## 分页与边界

本次保留已有两分钟、用户/会话/接口绑定的快照游标，以及准确 totalCount。后续页不重新扫描历史，查询期间的新操作不会导致翻页重复或漏行。

这不是全部条件下推 SQL，也不是 SQL LIMIT/OFFSET 分页：Java 仍需要匹配全部候选每日快照，最多保留 10,000 条命中结果，随后分页。数据库仍需对范围内历史进行排序/去重，历史查询仍在现有服务串行协调范围内。若实测显示这里成为瓶颈，应继续评估独立只读连接和快照分页；不能用偏移分页悄悄改变一致性。

## 部署与验证

涉及 common 和 QuoteSvr，部署时更新 QuoteSvr 及所带 common JAR；STC 客户端、BaseSvr 不变。启动时建立索引，已有大量历史的环境需要预留索引构建时间；本次没有在运行中的 DEV/QA 数据库建索引或重启服务。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/build-server.ps1 -Module quotesvr
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-history-search.ps1
```

功能测试使用独立随机 schema 和少量历史事件，结束后删除测试 schema；独立进程有 30 秒看门狗。覆盖权限隔离、同日多版本、跨日边界、空快照、相同时刻版本选择、组合筛选、当前基础资料、游标隔离/稳定性及读取失败后的连接恢复。不是性能测试，也不代表桌面端到端验证。
