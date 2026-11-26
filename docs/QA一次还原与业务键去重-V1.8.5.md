# 一次还原与六字段去重 V1.8.5

日期：2026-11-26。前序交付：V1.8.4。范围：Common、QuoteSvr增量源码、自动迁移、数据库用例和Python真实接口专项；无前端修改。用户明确本次无需UI测试。

## 最终业务规则

1. 每条REFERRED原报价只能成功Unrefer一次。Unrefer创建新ACTIVE报价，原报价及历史快照保持不变。
2. A还原得到B后，A不能再次还原；B以后被Refer，可以独立还原得到C。删除B也不会恢复A的次数。
3. 同一个成功请求重试返回原成功结果，不创建新报价、不重新激活已撤销的结果，不再执行去重。
4. 新报价激活时，自动Refer全部同键旧ACTIVE报价。业务键严格保持：归属人、方向、券、清算速度settlType、对手机构ID、联系人ID。价格、数量、报价来源不在业务键中。
5. 对手机构ID或联系人ID缺失时，保持原有不参与去重规则；不按展示名称猜测相同机构/交易员。
6. 重复还原的新请求返回1005/INVALID_STATE，用户提示“该报价已还原，不能重复还原”，不展示内部ID。

## 源码事实与处理

本机基线1e889b8d已经在Unrefer激活阶段调用replaceActiveQuotesWithSameBusinessKey，并非完全缺少调用。但原实现仅遍历内存quotes.values()；缓存缺项时可能漏掉数据库有效价。现改为按六字段查询并锁定数据库中的匹配ACTIVE记录，使用quotes_active_business_key_idx部分索引，确认/修改有效价/还原继续复用同一方法。

新增quote_unrefer_records：original_quote_id主键、restored_quote_id、created_at。原报价到新报价的关系与旧价Refer、新价保存、历史、事件、请求回执在同一事务内；失败整体回滚，不消耗次数。不改变已有报价协议字段，不新增客户端必须提交的参数。

并发控制保留当前QuoteSvr单写入者锁及服务内串行写入；还原关系另有数据库主键保护。没有宣称新增多主并发写入能力。

## 自动迁移与部署

- 新迁移QuoteUnreferMigration在QuoteSchema/DailyHistory迁移后运行；组件启动日志为QUOTE_UNREFER。
- metadata的quotes.unrefer.version=1记录迁移完成。创建新表、索引、回填和版本标记同事务；失败阻止启动，不静默跳过。
- 初次读取history中action=UNREFER且after.clonedFromQuoteId非空的成功记录，登记每个原报价最早一次成功还原。历史上重复产生的报价不删除；普通CLONE不消耗还原次数。后续启动只检查版本和表，不反复扫描全部历史。
- 无需手工SQL或配置补值。先构建Common，再构建并部署QuoteSvr；保留QA的POM版本和现有Core/Base/Notification修改，核对Common JAR唯一性。
- 本次增量基于后端1e889b8d，分支codex/qa-return-20261125；QA须先合入V1.8.4或方法级处理依赖，不能整文件覆盖QA新增代码。完整QA前置源码仍待回传。
- 回退旧QuoteSvr会失去这项业务限制，因此不能把单纯回退JAR当作保留该规则的回退方案；还原关系及既有历史应保留。
- 本机自动迁移已执行并重启复查。QA TBase实际部署尚待QA执行验证，不把本机PostgreSQL结果写成QA已验收。

## 实际测试

| 范围 | 实际结果 |
|---|---|
| Common | 161项，1跳过，0失败/错误 |
| QuoteSvr JUnit（隔离PostgreSQL） | 80项，0失败/错误/跳过；排除连接旧QA地址的RequestParserTest |
| 新QuoteUnreferOnceTest | 7个测试覆盖一次限制、六字段比较、缓存缺项、事务失败回滚、两个并发请求、重启/回放、历史回填及普通CLONE排除 |
| 既有UnreferCloneSmoke | 新规则调整后101项检查通过，保留权限、批量部分成功、历史、推送及回滚场景 |
| 既有QuoteBusinessKeySmoke | 99项检查通过，含六字段各自变化、缺失对手方不合并、确认/修改/还原及重启 |
| 真实网关专项 | 15项通过：真实CRM/基础资料→报价创建确认→Refer→Unrefer替换同键有效价、订阅移除/加入、重复拒绝、重放、A→B→C、数据库状态、实际重启后持久限制和幂等 |
| 清理 | 4条真实测试报价经Refer/Delete接口清理，未物理删除审计或历史 |
| UI | 按用户要求不执行，不声称通过 |

真实验收证据：docs/test-results/unrefer-live-20261126.json。专项脚本iteration1/headless-tests/unrefer_regression.py基于已交付V1.8.3的Python3套件，使用既有config.local.json连接配置，不含凭据。

运行顺序：before-restart阶段、用既有服务脚本重启QuoteSvr、after-restart阶段；必须使用同一个本机state文件。cleanup阶段可单独清理台账。用法：

```text
python unrefer_regression.py --config config.local.json --state <本机状态文件> --report <结果文件> --phase before-restart
# 用既有标准服务脚本重启QuoteSvr，等待服务和资料就绪
python unrefer_regression.py --config config.local.json --state <同一状态文件> --report <结果文件> --phase after-restart
```

不是压力测试，仅极小功能样本。测试的数据库访问只读，业务写入均走真实网关；未手工补入任何上游字段。

## 尚未确认的QA现场根因

本次真实本机链路已证明六字段齐全时Unrefer替换生效，并用隔离数据库明确复现及覆盖“数据库有ACTIVE但内存缺项”。不能据此直接断言QA原现象必然由缓存缺项导致。请QA在原场景核对六字段实际值、两条报价状态、运行包版本及客户端收到的增量；缺ID、不同ID与界面没刷新需分别判断。

请QA回传Common/Quote合并提交、迁移版本/关系回填数、重复还原与同键替换结果、真实推送、重启重试结果。前序筛选失败、完整源码基线等待办不在本次冒充修复。
