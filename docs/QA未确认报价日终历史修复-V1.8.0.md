# V1.8.0 未确认报价日终误入历史修复

## 范围及合并基线

只交付 QuoteSvr 本次增量补丁，以及该组件完整源码供比对。不是全部后端的累计发布；不包含客户端、Common、Notification、BaseSvr、Parser 或 CRM 的源码改动。Common 0.1.1、父 POM 及其他服务沿用 QA 已部署依赖。

上一交付 20261120-R3；QA 回邮确认 QuoteSvr rel_0.1.1 的 c5f18c3 已合并，本方对应源码基线 e10ee3b4。QA QuoteService 保留了五处中文注释，请合并补丁并保留注释，不用完整目录强行覆盖。完整源码不包含 QA 未回传的后续改动。

## 修改行为

- 未确认报价仍按原日终规则变为 REFERRED / PENDING_EXPIRED，保留 quotes 状态和事件，退出报价确认及正式管理列表。
- 日终不再向 history 写入该未确认报价，也不生成 quote_history_daily 记录。
- ACTIVE 为正式报价；REFERRED / DELETED 必须有非空 confirmTime 才属于正式历史。已确认报价正常创建、修改、撤销、删除历史仍保留，quoteTime 语义和排序不变。
- 启动自动升级 history.daily.version 6→7：清理 quote_history_daily 非正式记录，更新触发器和部分索引。旧 history 审计记录不删除，供追溯；重新回填不会恢复这些非正式查询快照。
- 每笔报价唯一键不变，没有增加查询时去重。协议、配置及日终截止时间不变。

## 部署与核对

使用现有 Jenkins/Salt 流程构建和部署 QuoteSvr，无新增配置。先备份相关库表及当前应用，在维护窗口执行；迁移会锁 history 和 quote_history_daily 并重建一个部分索引。QA 大库耗时尚未测试。失败会回滚迁移并传播异常，不能宣称启动成功。旧应用回退后仍可能再次产生缺陷；如需恢复数据库，使用部署前备份，不手工改版本标记。

检查 HISTORY_FORMAL_UPGRADE state=COMMITTED from=6 to=7。以下只读SQL将 <schema> 替换为实际 QuoteSvr schema，结果应为7、0：

```sql
SELECT value FROM <schema>.metadata WHERE key='history.daily.version';
SELECT count(*) FROM <schema>.quote_history_daily
WHERE NOT COALESCE((snapshot->>'status'='ACTIVE'
 OR (snapshot->>'status' IN ('REFERRED','DELETED')
 AND NULLIF(btrim(snapshot->>'confirmTime'),'') IS NOT NULL)),false);
```

QA请用真实STP分别创建未确认、已确认报价，经历日终及停机后启动补处理：前者退出确认且历史搜索无记录，后者历史正常；再次重启无重复记录。确认页面、客户端缓存和接口结果一致。

## 实际验证与限制

- 四项真实PostgreSQL隔离schema功能测试通过：UnconfirmedHistoryMigrationSmoke、QuoteEndOfDaySmoke、DraftLifecycleSmoke、HistoryLatestVersionSmoke。覆盖回滚、升级、回填、日终、启动补处理、幂等；固定时钟/测试资料组件测试不是桌面端到端。
- QuotePricesTest、MissingReferenceQueryTest共8项通过，选定测试构建成功。完整Maven两次未完成：既有RequestParserTest连接QA ZK超时，第二次skipTests受父POM配置影响仍执行。不能称全套通过。
- 本机真实GW→QuoteSvr→PostgreSQL→历史接口：独立schema两笔报价等待00:08定时日终。未确认history=0、daily=0；已确认history=2、daily=1。两笔都REFERRED，未确认reason=PENDING_EXPIRED。仅两笔功能测试，未压测。
- 恢复原配置后正常库v6→v7，181条旧非正式快照（108条PENDING_EXPIRED、73条未确认删除）清理为0，旧审计保留。本机日志38ms不代表QA性能。原schema及22:00截止已恢复，QT Intake仍停止；临时隔离schema保留复核。
- 桌面工具缺失运行模块，未核对实际客户端缓存/页面；QA TBase迁移及桌面端到端待验。本包为QA合并回归候选，未完成全部验收。CalSvr当时离线，样本标记CALCULATOR_OFFLINE，本轮不验价格换算。

## 版本链

用户已确认本次V1.8.0；V1.7.2后十次旧交付保留原名与SHA256，以台账关联，不覆盖或重命名。20261120-R1为未发送草稿，不计入已交付链。
