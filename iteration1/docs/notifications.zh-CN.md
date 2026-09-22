# 第一轮报价提醒

## 链路与落库

```mermaid
flowchart LR
  Q[QuoteSvr] -->|ficc.quote.events| N[NotificationSvr]
  Q -->|ficc.price.snapshots| N
  N --> DB[(PostgreSQL ficc_salestrading)]
  C[C# 报价提醒] <-->|独立 notification 请求与推送| GW[STC GW]
  GW <--> N
```

NotificationSvr 独立运行于 `127.0.0.1:18690`。复用现有 STC/BBox `ContentHandler`、`ResourceAdapter`、`StreamAdapter` 和 `NetRtOutputStream`，配置位于 `cgws-ficc-saletrading-notificationsvr/control`。13 个请求 Handler 与独立通知 report topic 已接入 GW。

启动时先订阅 QuoteSvr 两类消息，再用 `ficc.quote.internal.state.query` 加载报价与行情分组，用 `ficc.price.internal.snapshot.query` 获取价格簿。内部调用通过 STC 服务客户端完成，不登录用户账号，也不配置服务账号密码；前端请求仍验证用户会话。报价和价格分别检查处理位置；只有簿成员及引用的 quoteRevision 与当前有效报价一致，才用于判断最优。缺号或容量 128 的队列溢出触发重建，不无限增加内存。内部接口边界及升级步骤见 [STC 适配说明](stc-adapters.zh-CN.md#内部同步与用户登录的边界)。

QuoteSvr 与 NotificationSvr 共用 `ficc_salestrading` schema。通知服务使用 `notifications`、`notification_preferences`、`notification_reads`，公共存储使用 `notification_metadata`、`notification_events`、`notification_requests`、`notification_settings`，避免与报价的事件序号和幂等记录混用。通知与相应事件在同一 PostgreSQL 事务提交；写入锁按服务区分。行情分组仍来自 QuoteSvr 事件。旧双 schema 环境的升级见 [数据库初始化与迁移](qa-database.zh-CN.md)，旧 schema 仅保留作迁移备份。

源码位于 [cgws-ficc-saletrading-notificationsvr/src/main/java/ficc/notification](../cgws-ficc-saletrading-notificationsvr/src/main/java/ficc/notification)：`NotificationMain` 为入口，`NotificationResource` 通过公共 `QuoteSvrClient` 处理上游订阅与业务恢复，`NetClientResource` 负责连接和 ZooKeeper 发现，`NotificationService` 实现规则与通知业务，`handler/` 和 `stream/` 分别处理请求与推送。运行时只加载通知服务 JAR、`cgws-ficc-saletrading-common.jar` 和 STC/DPS 等基础依赖。

## 四类规则

| 类型 | 条件 |
|---|---|
| 关注债券 | 当前账户、对应市场、scope=MARKET 的全部关注分组命中有效报价 |
| 筛选分组 | 当前账户全部行情筛选分组；复用 FilterEngine，同字段 OR、跨字段 AND |
| 对价提醒 | 同券、反方向、一条本人和一条他人有效报价；不要求价格交叉、相同清算或相同计价方式；每一对报价单独提醒 |
| 我的最优 | QuoteSvr 排好序的完整价格簿中，同侧首档属于本人的报价，保留全部并列；他人的操作也触发重新判断 |

非对价提醒按账户、债券合并原因和关联 quoteIds；对价保持独立。通知 ID 包含所关联报价版本的稳定摘要，重复上游消息不会重复生成相同提醒；新版本产生新的当前提醒，旧提醒退出当前列表。

`alertKey` 是本轮客户端去重用的附加字段。已读按通知内容版本保存，因此关闭再开启同一规则不会清空已读；新版本不会因上一条提醒已读而被自动吞掉。客户端 read 请求中的旧 notificationId 也不会误标新版本。

## 客户端操作

连接 GW 后打开 **报价提醒**：

1. 四个复选框分别保存个人开关；失败时恢复界面原值。
2. “关注录入栏债券”将上方所选债券加入本账户的“平台关注债券”行情分组；“添加 AAA 筛选组”保存一个行情评级筛选组。即使没有在行情面板选中这些组，它们也参与提醒。
3. 提醒通过自己的 `ficc.notification.report.<sessionId>` 实时刷新；进入提醒页标记当时已加载列表已读。也可手动标为已读／未读。
4. 新提醒显示角标与弹窗，最多展示三条；弹窗“关闭”不修改已读，“查看”只标记该弹窗的提醒并定位。
5. 双击或“查看相关报价”打开关联报价表，通过 QuoteSvr 读取真实 Quote，按后端权限显示机构与联系人；初版最多展示前 20 条，并标明总数。

已读请求失败时写入本账户本地重试文件，每 5 秒重试；服务器已删除/不再可见的 ID 不继续重试。JSON 中 readTime 缺失或为 null 均视为未读，以兼容 STC 序列化。

## 验证与边界

`client/src/NotificationSmoke.cs` 经 C# → GW → NotificationSvr 验证四类规则、并列最优、他人报价变化、不同清算对价、跨用户拒绝、已读保留、分组以及内部状态/事件，共 20 项，创建 3 条报价。WinForms 检查另覆盖提醒插入、已读回推及撤出移除。

基础数据和账户仍为固定示例；需要 CalSvr 的价格不会被用旧价格簿冒充最优，对价仍可按有效 Quote 的方向关系判断。没有历史提醒收件箱 API，也未进行压力或高可用验证。通知详情 UI 是功能平台，尚未复刻产品的全部基础资料列。
