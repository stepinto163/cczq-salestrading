# V1.8.9 报价管理三框独立搜索：QA合并与前端协作

## 范围与业务契约

本包是后端候选源码，前序交付V1.8.8，基线f73177bd，分支codex/qa-return-20261125。未修改或打包客户端、BondSearch Core、BaseSvr。无数据库迁移；包含Common、QuoteSvr、NotificationSvr及通知服务的源码/Salt资源配置。版本、最终提交和文件哈希见manifest.json与回执。

| 页面输入框 | 客户端已有属性 | 新条件field | operator | 实际匹配 |
|---|---|---|---|---|
| 代码/名称 | BondSearchText | bondSearchText | CONTAINS | 当前债券代码或当前名称 |
| 报价机构 | InstitutionSearchText | institutionName | CONTAINS | 报价自身机构名称 |
| 长城交易员 | TraderSearchText | ownerSearchText | CONTAINS | STC用户name或username，允许一次命中多名归属人 |

三个不同字段之间AND；同一字段多个条件仍按原协议OR。空框不发条件。包含匹配不区分大小写，百分号、下划线、单引号按普通文字，不是SQL通配符。不要求用户选一个ownerUserId；归属人的文本匹配由后端转换为ID集合。未命中返回空，不能忽略条件；权限与MINE/VISIBLE约束继续执行。停用账号仍可搜索其历史报价，账号名称变化将在后续新查询刷新目录时生效（目录最长缓存60秒）。已打开视图沿用创建时的匹配集合，重新打开/刷新可使用新目录。

历史展示继续使用已保存的历史资料；代码/名称筛选使用当前基础资料，交易员使用当前用户目录。机构按该报价最终记录的机构名称筛选，机构修改后旧名称不应继续匹配。旧searchText综合搜索保留兼容，但不会自动变成三个独立框。

## 前端同事需要修改

位置：STC.SalesTrading.Plugin/Gateway/GatewayQuoteMgmtService.cs，ToFilter(QuoteMgmtQuery)和AddSearchText。今日和历史共用此路径。

1. 保留原产品筛选条件、关注券/筛选分组、日期和ownerScope。
2. 将BondSearchText、InstitutionSearchText、TraderSearchText分别按上表发送，禁止继续都拼成searchText。保留独立的旧query.SearchText用途，不与三个框重复发同一输入。
3. 输入trim，空字符串不发；三个框一起提交一个filter。输入变化按原逻辑重开视图/历史第1页；建议短延迟合并输入并丢弃过期响应，避免每个字符发一次请求。
4. 长城交易员保持可输入文本，不改成强制选择一个账号。报价交易员contactName与内部归属交易员不是同一概念。

请求示例（放入今日query/view.open或历史history.search的filter；历史另带日期和分页）：

```json
{"conditions":[
  {"field":"bondSearchText","operator":"CONTAINS","value":"250220"},
  {"field":"institutionName","operator":"CONTAINS","value":"证券"},
  {"field":"ownerSearchText","operator":"CONTAINS","value":"张"}
]}
```

## 性能实现与边界

- FiccBaseDataClient在已有加载流程缓存小写债券代码/名称；新搜索只扫描这两项并返回ID，不复制全量基础资料或估值JSON，不远程查每笔报价。
- 最近16个债券搜索结果缓存，每个最多20000个ID；宽泛结果不长期缓存，基础资料版本更新立即失效。
- STC目录只读userid、username、name，首次启动验证真实查询；之后60秒内复用。无逐报价连接/查询，SQL查询超时3秒；失败明确报错，不静默放宽条件。
- 历史代码集合与归属集合以绑定数组下推security_id/owner_id；机构包含用绑定参数strpos，数据库完成计数、排序和LIMIT/OFFSET，仅传本页快照。既有其他未支持SQL下推的复杂条件仍可能走原预筛选路径，本次不宣称所有筛选都全量SQL化。
- 本机小样本真实接口计时不代表QA大规模/并发性能，不新增未经实测必要性的索引；QA按实际日期范围和数据量核对日志HISTORY_QUERY的mode、transferredRows、elapsedMillis。不要将本机数字当作生产承诺。

## 合并与部署

- 应用from-V1.8.8三个组件patch；最终文件同时提供用于审阅。保留QA POM版本和本地已有修复，不整仓覆盖。
- 先构建Common，再构建Quote/Notification；运行目录只能保留一个Common版本。
- NotificationSvr新增DBPoolResource，源码control与Salt files/control两份都必须合并；resourceCfg新增共享$ATS_ROOT/control/DBPoolConfig.ini与MYSQL0引用，和QuoteSvr同源。不创建新凭据文件，不要求手工拷贝example。检查部署渲染结果与实际启动路径。
- 停止并升级两个服务，先启动Quote，再启动Notification完成同步。真实初始化应包含OWNER_SEARCH_DIRECTORY INITIALIZED，Notification随后SOURCE_SYNC READY。
- 不改变前序V1.8.8的原价/计价方式、收益率排序及MDPriceLevel共档约定；前序未验收事项保留。

## 实际验证及待办

单测/集成测试Common168（1项原有跳过）、Quote83（排除原依赖QA连接的RequestParserTest）、Notification12，失败/错误0。真实标准Ubuntu依赖已启动检查；本机Quote/GW/Notification、真实Base/CRM与两个真实测试账号完成专项56项：三个框、AND、同字段多用户OR、大小写、无命中、特殊字符、错框不匹配、分页计数、打开视图的机构修改移除/恢复、历史只按最终机构筛选、真实订阅与通知查询。测试报价通过API删除，未补造DM/基础字段。最终计时与请求关联号见three-box-search-live-20261127.json。

前端同事尚未改映射，客户端缓存/UI没有验收；QA的TBase大数据量、实际部署包、页面交互与权限回归待完成。因此这是供QA合并回归的候选源码，不是已验收版本。请回邮合并提交、部署JAR与配置、三框组合请求/响应、HISTORY_QUERY计时和页面验收结果。

最终本机功能计时：query 4.89–57.58ms（12次），view.open 4.34–11.93ms（13次），history 4.24–11.79ms（41次）。三轮共12笔测试报价独立DB复核均为DELETED。
