# 销售交易业务规则与实现核查

本清单用于约束开发、评审和自动化测试。首批记录报价归属、可见性和操作权限；后续按业务主题追加稳定规则编号，不以现有实现反向定义产品要求。

规则依据优先级：用户明确确认的产品决定 → 产品需求文档 → 产品未规定处参考 SZ 旧实现。代码和 API 文档用于核查实现，不是产品规则的来源。发现规则冲突必须登记，不能自行选择较容易实现的一方。

首次核查日期：2026-10-01，代码基线为 `29d4e03` 所在工作区。下方保留各次历史核查结果；当前可见性以 2026-10-16 决策为准，覆盖此前逐交易员行情及普通报价对手字段脱敏规则。

## 2026-10-16：已确认报价共享，输入仍私有

- QTrade 作为报价来源，外部消息不能自动修改或删除已确认 STC 报价；确认仍由 STC 用户控制。
- 本平台为本机构部署：普通登录用户共享所有来源的 ACTIVE、REFERRED 报价及其机构、交易员、归属人信息。行情只使用 ACTIVE，FULL/BEST/DEPTH 使用同一共享报价池；筛选条件仍可不同。
- PENDING_CONFIRM 及未确认即删除的输入仍只允许所有者、配置管理员和内部服务查看；来源原文查询保持原有归属权限。
- 今日管理共享已确认记录；历史搜索默认 VISIBLE，按每日最后版本判断可见性。已确认后删除的记录凭 confirmTime 共享，缺少确认凭据的删除记录保持私有，不能从旧 ACTIVE 快照推断公开。
- MINE 为主动选择的归属过滤。修改、确认、Refer、Unrefer、删除和逐笔操作审计仍检查所有者/管理员权限；共享读取不授予修改权。
- 提醒按用户自己的关注和筛选设置计算，但候选有效报价池共享。智能推荐沿用独立的产品权限规则，本次不改变其证据脱敏。
- 外部 Refer 指令审核及历史回填不在本次范围；业务键替换遵循下节补充决定。

## 2026-10-16：人工激活时按完整业务键替换

- 业务键：`securityId + side + settlType + ownerUserId + institutionId + contactId`，使用已匹配 ID，不用显示名称。清算字段为 `settlType`，交易日不额外参与此键。
- 人工确认、Unrefer、修改 ACTIVE 报价后，完整键相同的其他 ACTIVE 报价转为 REFERRED。新价与旧价状态、历史、事件共用数据库事务；失败整体回滚。旧记录保留，并记录替换它的内部 quoteId。
- institutionId 或 contactId 任一为空，不替换其他报价；不能把缺失的对手视为同一对手。
- 适用于所有报价来源。QuoteID、NegotiationID、来源账户和来源类型均不参与匹配；不同 NegotiationID 可以对应同一业务键。内部 quoteId 仍独立，不因替换复用。
- 收到 QT 消息、修改待确认提案不触发此规则，只有成功的人工激活/有效报价编辑才改变正式价池。权限仍由 ownerUserId 控制。
- 不启动时批量清洗历史重复数据；下次人工激活同键报价时替换该键的全部旧 ACTIVE。批量操作延续逐条提交语义，同键条目最后成功处理者有效。
- SZ 参考：`ConfirmIntentionQuoteHandler` 使用上述六字段；本平台明确采用更严格的缺失身份边界，不沿用 SZ 把缺失联系人归为 `X` 的处理。

## 产品依据

来源：[FICC 销售智能交易平台](../FICC销售智能交易平台.html)，内嵌《业务需求规格说明书》。主要依据：2.1 用户角色及权限矩阵、4.2/4.3 行情字段、5 报价管理、6.1 权限与可见性、7 账号与权限。

关键原文：

- 待确认报价 / 模糊报价草稿：“本人读写、识别、修订、确认”。
- 正式报价（行情中心）：“所有报价数据可见；报价机构与报价交易员字段仅本人可见；仅可编辑本人报价”。
- 正式报价（报价管理）：“管理本人今日/历史报价”。
- 行情全部/最优：“全部交易员共享报价数据；机构/交易员字段按本人可见规则脱敏”。
- 长城交易员：“报价所有者”；报价管理中“用于‘我的报价’判断”。
- 6.1：“先判断用户是否可见，再展示数据、进行筛选、汇总、详情查看、Copy或导出”；所有者、实际操作人及交易双方身份“不得相互代填”。
- 交易员权限概要：“批量动作混入不可操作对象时整批拒绝”。

这组规则已有产品定义，因此不需要由 SZ 旧实现补定。产品中通用批量流程也允许接口明确声明部分成功；它与上述越权整批拒绝之间的关系需在修改批量接口前明确，不能直接沿用当前逐行提交作为依据。

## 规则清单

以下“普通用户”不包含授权管理员及内部服务账号。“本人”按业务所有者判断，不按最后操作人判断。

| 编号 | 必须保持的约束 | 依据与边界 |
|---|---|---|
| VIS-001 | 普通用户只能读取和处理归属于自己的待确认报价；他人的待确认报价不得进入行情。 | 2.1 权限矩阵。管理员按授权例外。 |
| VIS-002 | 非 QT 的有效报价保持产品原有共享行情规则。QT 报价按本地接收交易员隔离，确认后仍不得向无关交易员暴露其双边价格。 | 2026-10-08 用户明确决定优先于原产品文档的通用共享描述；管理员和内部服务仍按显式授权例外。 |
| VIS-003 | 非所有者不能获得报价机构、报价交易员的受限信息；所有者可看完整信息。 | 2.1、4.2。核查时同时覆盖名称和可用于反查身份的 ID。 |
| VIS-004 | 报价管理的今日/历史视图管理本人报价；行情可见不等于有管理权限。 | 2.1、5。普通用户不能修改他人报价。 |
| VIS-005 | 长城交易员表示内部报价所有者；报价交易员表示报价来源联系人。所有者、操作人、对手身份必须分开。 | 1.5、字段表、6.1。不能将外部 QT 联系人直接当作内部所有者。 |
| VIS-006 | 字段权限必须覆盖查询、详情、分页、订阅初始镜像、增量推送、Copy、导出及筛选/聚合。 | 6.1 及跨页面权限规范。禁止通过命中结果、计数或其他字段绕过隐藏规则。 |
| VIS-007 | 本人报价的创建、确认、修改、Refer、Unrefer、删除必须由服务端检查操作者、对象状态和版本。 | 2.1、6.2。按钮禁用不能替代授权。 |
| VIS-008 | 管理员仅按授权范围处理他人报价；保留所有者、实际操作人、原因、时间和结果。 | 2.1、6.1。管理员身份不能与报价所有权合并为一个业务概念。 |
| VIS-009 | 待确认和正式行情的私有推送通道不能被其他会话冒用；重连与增量不能改变上述权限。 | 产品账户隔离与跨端一致性要求。具体 session/topic 是实现方式。 |
| VIS-010 | 普通用户批量操作混入无权处理的报价时，整批拒绝且不得已修改其中部分。 | 2.1 明文规定；与通用“部分成功”合同的适用边界待澄清。 |

### A B C 示例

A 是外部报价人，B/C 是两个普通长城用户；B/C 的 QT 账户分别正确映射到各自 STC 用户。A 把 quote-1 发给 B/C，把 quote-2 只发给 C。

| 时点或页面 | B 的预期 |
|---|---|
| 待确认 | 看到本人收到的 quote-1，看不到归属 C 的 quote-2。 |
| C 确认 QT quote-2 后的行情中心 | B 仍不可见 C 私有接收的 quote-2；确认不会扩大 QT 可见范围。 |
| B 的报价管理 | 不把 C 的 quote-2 当作本人报价。 |
| B 尝试修改 C 的 quote-2 | 服务端拒绝。 |
| 长城交易员列 | quote-2 的所有者为 C，不是外部 A。 |

同一意向发给 B/C 时，各自保留接收记录和双边会话。普通用户行情先按可见范围筛选，因此不会把另一个接收人的副本叠加进本人的价量。管理员/内部全量镜像不代表普通用户行情，也不根据收端缺失的 QuoteID 猜测全局意向去重。

### QT 意向、会话、提案和价格（2026-10-08）

| 编号 | 规则 | 接口/模型体现 |
|---|---|---|
| QT-001 | 发送方的 QuoteID 标识意向，一个意向可以派生多个双边会话。 | `QuoteSource.externalQuoteId` 保留 FIX 117；不是 FICC 的 `quoteId`。 |
| QT-002 | 接收方收到的报价带 NegotiationID；它标识双边会话，不标识所有接收人共享的一条报价。 | `QuoteSource.negotiationId` 保留 FIX 18033。接收账号、内部归属与会话分别保留。收端 117=-10000 表示缺失，不能拿这个常量合并记录。 |
| QT-003 | A–B 的回价只能影响 A–B，不修改 A–C 或将意向原价全局改写。 | 关联本地接收账号、ownerUserId、NegotiationID 和报价方向；不得只按 QuoteID、债券或对手机构合并。 |
| QT-004 | 未确认的输入或回价只属于报价确认，不影响正式报价、行情价量、最优价、深度及提醒。 | `PENDING_CONFIRM` 与 `ACTIVE` 分离；价量投影仅使用 ACTIVE。待确认操作可产生本人报价报告，但不能产生用户行情更新。 |
| QT-005 | 会话已有待确认同向提案时更新该提案；已有正式报价时，新输入另存待确认，保留上一笔已确认行情。 | FICC 本地 quoteId 标识提案/正式记录；revision 是本地乐观锁版本，不是 QTrade 会话 ID。 |
| QT-006 | 用户确认新提案后，才替换本人同会话同方向的上一笔正式报价；显式恢复也不能造成两个当前版本。 | 旧记录转 REFERRED，保留历史；新记录 ACTIVE。同一数据库事务保存，两条事件保留各自审计。其他会话和其他接收人不变。 |
| QT-007 | 行情是用户可见正式报价的派生结果，不是原始 FIX 最新消息的缓存。 | FULL/BEST/DEPTH 均先过滤身份及 ACTIVE，再排序取档。quoteRefs 引用 FICC quoteId/revision。内部全量 feed 只能供授权服务，通知服务仍需逐用户过滤。 |

自动回价、成交、撤单联动和完整交易状态机不由这些字段隐式实现；未覆盖的 FIX 事件仍遵守接入端现有事件范围。

回归入口：`python iteration1/scripts/test-qtrade-negotiation.py`。使用六条匿名 FIX 形状经真实 mapper 生成请求，在本机 PostgreSQL 独立临时 schema 中验证三个用户、两个双边会话、待确认更新、确认替换、恢复、重载、通知及旧增量视图；每个测试子进程有 45 秒看门狗。它属于组件集成测试，**不替代真实 QTrade → FIX → DPS/GW → STC 桌面的端到端验收**。输出包含协议报文样本，供 C# `test-protocol-contract.ps1` 验证。

### 2026-10-08 桌面实测

使用正式 STC.App 和 WPF 控件，登录本机 GW，连接实际 QuoteSvr、NotificationSvr、BaseSvr 及 PostgreSQL；独立 UI 子进程带超时看门狗。结果如下：

| 用例 | 实测结果 |
|---|---|
| 18-qtrade-negotiation | 通过：同意向两个会话分别确认；新提案只进入报价确认；更新待确认提案复用本地记录；确认后仅替换本会话旧价，另一会话价格不变。 |
| 01-quote-lifecycle、03-pending-push | 通过：报价生命周期及待确认推送。 |
| 05-settlement-update | 通过：清算修改。 |
| 07-management-filters、08-market-filters | 通过：报价管理及行情侧栏过滤。 |
| 14-watchlist-notification-read | 通过：提醒到达及界面标记已读。 |

首次桌面运行发现 NotificationSvr 无法同步：QuoteSvr 的内部初始行情快照把已失去 BaseSvr 参考数据的待确认债券也纳入证券集合。已修复为仅从 ACTIVE 报价取得证券集合，并补充 PENDING_CONFIRM、REFERRED、DELETED 缺少参考数据时仍可生成初始行情快照的回归断言。重新部署后上述七个桌面用例均通过，全部用例自身夹具清理成功；未清空用户数据。

本机证据目录：`iteration1/runtime/negotiation-desktop/ui-run-1`（首次失败）、`ui-run-2`（议价通过及三张行情截图）、`ui-regression`（六个既有用例）。Java 组件报告：`iteration1/runtime/negotiation-tests/20261008-160722`，包括议价 80、通知 23、原接入 44、增量视图 375 项断言。

边界：用例 18 通过真实 GW 的 intake 接口准备输入，再通过正式 UI 控件确认并验证推送、行情表和服务端状态。**没有发送外部 QTrade 消息，没有覆盖本次外部 FIX 接收和 QtIntakeSvr 归属解析，也不是三个用户同时登录的桌面测试。** 三用户隔离由组件测试覆盖；外部完整链路仍需单独实测。用例 18 需要测试进程环境变量 `STC_UI_INTAKE_TOKEN`，取本机现有接入配置；不要把值写进用例 JSON。

运行指定桌面用例：先用 `iteration1/stc-client/stc-client/scripts/build-ui-tests.ps1` 编译测试程序，再设置专用测试账号环境变量 `STC_UI_USER`、`STC_UI_PASSWORD`，执行 `run-ui-tests.ps1 -Cases 18-qtrade-negotiation -StcConfig <本机STC.exe.config> -ReportDirectory <报告目录>`。用例 JSON 可在独立测试管理窗口中查看、编辑和执行。

## 2026-10-01 历史源码核查结果

源码目录缩写：Java 服务位于 `iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote`，公共代码位于 `iteration1/cgws-ficc-saletrading-common/src/main/java/ficc`；客户端位于 `iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc`。

| 规则 | 结论 | 实现证据与缺口 |
|---|---|---|
| VIS-001 | 普通用户主要路径源码符合 | `QuoteMatcher.prepareQuery`：非 ACTIVE 要求 `access.owns`；`QuoteService.get` 同样检查。客户端 `LivePage.OwnerScope=MINE`，pending 打开 PENDING_CONFIRM。Qt 接入 `IntakeCreateEndpoint` 按 owner/接收账号限定来源分组。真实多接收人映射未实测。 |
| VIS-002 | 主要行情路径源码符合 | `QuoteMatcher` 的 VISIBLE 放行 ACTIVE；`ViewService.buildBook` 聚合有效报价而不按普通用户所有权隔离；客户端行情使用 price 接口。 |
| VIS-003 | 部分符合，存在旁路 | `QuoteService.visible`、`QuotePageSnapshot.visibleJson` 清除他人机构/联系人四字段；price book 对非所有者不填 InstitutionID/TraderID。但 clone 和原文路径存在下述问题。 |
| VIS-004 | 普通用户主要路径源码符合 | 今日管理使用默认 MINE；`LocalSearchSvrClient.searchHistory` 限定 `access.owns`；单笔 history 限所有者/特权账号。行情查询能看到他人 ACTIVE 本身不违反管理规则。 |
| VIS-005 | 身份概念分开，展示不完整 | Java QuoteRecord 分开 ownerUserId、createdBy、updatedBy、confirmedBy、contactId；客户端 WallTrader 映射 ownerUserName，但 Java 显式模型没有该名称字段。行情 `BookRows` 还未接入返回的 UserID/InstitutionID/TraderID，无法据此显示内部所有者及本人报价来源。 |
| VIS-006 | 不符合 | 过滤器直接读取原始机构/联系人；clone 复制隐藏字段；嵌套 source.text 保留。详见 F-01 至 F-03。Copy/导出与通知所有路径尚未完整验收。 |
| VIS-007 | 单笔变更主要路径源码符合 | `QuoteService.write` 的非 create/clone 分支校验所有权、expectedRevision、状态。现有 BusinessSmoke 包含非所有者 Refer 被拒绝用例，本次未运行。clone 须另行限制。 |
| VIS-008 | 部分符合 | history 记录 owner 所在完整报价、operatorUserId、operationTime、reason；但 reason 未见强制校验，AccessPolicy 将配置中的管理员/内部账号直接视为 owns，没有细分对象/动作授权。管理员字段读取范围也应独立确认。 |
| VIS-009 | 源码存在隔离检查，端到端未验证 | `ViewService.authorize` 检查 session、user、topic；原始 price book、snapshots、quote.events 限 internal。普通行情推送使用按接收人脱敏路径；不能把 internal 原始流当作普通用户漏洞。 |
| VIS-010 | 与具体产品条款不符 | `QuoteService.write` 逐项开始/提交事务，后续越权仅记录该项失败，之前成功项不回滚；与“混入不可操作对象整批拒绝”不一致。修改前需协调 API 部分成功合同。 |

### F-01 隐藏字段仍参与筛选

优先级：高。`QuoteMatcher.prepare` 以原始 QuoteRecord 构造 `QuoteFilterValues`；后者的 searchText 包含 institutionName/contactName。`ViewService.buildBook` 也先应用该匹配器再隐藏输出字段。

结果：B 对 C 的有效报价搜索某个机构/联系人，仍可能通过命中 quoteId、数量或最优结果推断隐藏身份；JSON 不返回名称不足以满足 VIS-006。

修复方向：过滤取值应带当前用户的字段读取权限；对无权字段按统一不可见语义处理，覆盖组合过滤及聚合。不要为此在匹配热路径逐条序列化 JSON。

### F-02 clone 可复制他人隐藏身份

优先级：高。`QuoteService.write` 的 clone 分支使用 `get(sourceQuoteId, user)`；get 允许读取他人 ACTIVE。随后 `QuoteEdits.copy` 直接复制 institutionId/name、contactId/name 到以当前用户创建的新报价，没有执行源报价所有权校验或字段投影。

结果：有效且通过其他校验的复制请求可把 C 的隐藏来源复制到 B 名下，B 后续读自己的新报价即可获得来源信息。源码路径明确；本次未对运行服务执行此请求。

修复方向：按产品“本人 Copy”限制源对象；即使以后允许复制共享行情，也只能复制当前用户有权读取的字段，不能复制原始隐藏字段。

### F-03 嵌套原文未按字段权限处理

优先级：高，是否实际泄露取决于原文内容。两种 quote 脱敏函数只删除四个顶层字段，保留 `source.text`。QTradeQuoteMapper 将 18026 原文写入 source.text，IntakeCreateEndpoint 将其带入 QuoteRecord.source。

结果：原文若带有机构/姓名，非所有者获取已确认报价时仍可从原文看到它们。sourceMessage.query 的所有者检查并不能保护已经内嵌在 quote 返回里的原文。

修复方向：明确非所有者的 source/remark 输出规则，保证受限身份不会从原文旁路返回；不能简单宣称所有原文必然含敏感字段。

### F-04 行情身份显示链路缺失

原问题：`DpsSalesTradingServices.Market.BookRows` 从行情 entry 构造 quote 时没有拷贝 UserID、InstitutionID、TraderID；`WithReference` 仅补债券资料。`DpsModelMapper` 又依赖 ownerUserName 显示 WallTrader。

2026-10-12 修复：QuoteSvr 在已有身份权限分支内返回 InstitutionName/TraderName；客户端 FULL 和 BEST 明细保留三个身份，按 STC userid 从宿主用户目录读取 name 显示“长城交易员”。本方报价所有者不能用对手 TraderID 或当前查看者代填。未知用户显示原 userid；不会补查服务端已隐藏的对手资料。修复需同时部署 common、QuoteSvr 和客户端。

SZ 依据：`sc-app-stc-sbpsvr` 的 `QuoteInfoAddHandler`、`ConfirmIntentionQuoteHandler` 使用 `userInfo.name` 设置 createUserName；`SbpStream` 的 QT 接入使用 `stcUserInfo.getName()`。本实现沿用该显示语义，并保留 ownerUserId 与 contactId 的区分。

验证：Java PriceSearchSmoke 45 项、客户端协议/投影测试 287 项、WPF 行情组件 24 项通过，覆盖 FULL/BEST/DEPTH、本人/他人、姓名缺失、旧报文兼容及三个实际表格单元格。安装本地 common/QuoteSvr 和客户端插件后，经本地 GW 只读查询已有 240205.IB 报价，确认 price 条目的机构/联系人名称与报价一致，UserID 可在 STC 用户目录解析为 test2测试；未创建或修改报价。不代表 QA 已部署或完整桌面端到端验收完成。上表为原核查记录，VIS-005 的展示缺口以本节修复为准。

### F-05 批量越权与管理员授权边界

优先级：中。先解决 VIS-010 的产品/API 冲突，再补越权整批预校验。管理员的操作权、字段读取权、所有权应分开表达；保留管理员例外，但不能用一个 owns 同时代表三者。上述为整改方向，本次没有修改协议或业务实现。

### 代码证据入口

- [QuoteMatcher](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/QuoteMatcher.java)：对象范围及原始值匹配。
- [QuoteFilterValues](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/QuoteFilterValues.java)：searchText 包含机构和联系人。
- [QuoteService](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/QuoteService.java)：get、visible、write、history、sourceQuery。
- [QuoteEdits](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/QuoteEdits.java)：copy 的字段范围。
- [QuotePageSnapshot](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/QuotePageSnapshot.java)：分页和增量的字段脱敏。
- [ViewService](../iteration1/cgws-ficc-saletrading-quotesvr/src/main/java/ficc/quote/ViewService.java)：authorize、buildBook、publishChanges。
- [AccessPolicy](../iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/common/client/AccessPolicy.java)：管理员、内部账号与 owns。
- [STC 行情映射](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/DpsSalesTradingServices.Market.cs)：BookRows 与最优行情。
- [STC 报价管理](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/DpsSalesTradingServices.Quotes.cs)、[LivePage](../iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/LivePage.cs)：管理和待确认的请求范围。

## 验收用例清单

固定三个角色：外部 A、普通用户 B/C。给 C 的机构名和联系人名使用唯一测试标记，避免恰好出现在债券名中。每例独立准备和清理自己的报价，不依赖上一例的执行结果。只在隔离测试环境构造数据。

| 用例 | 覆盖规则 | 核心断言 |
|---|---|---|
| VIS-T01 | 001、009 | C 的 pending 在 B 的 query/get/view.open/view.query/推送中均不可得；C 自己可读。 |
| VIS-T02 | 002、003 | QT：C 确认后 B 仍不可见，C 可见完整信息。非 QT：保留共享价量及机构/联系人脱敏的原产品规则。 |
| VIS-T03 | 004、007 | B 今日/历史管理不出现 C 的报价；逐一尝试更新、确认、Refer、Unrefer、删除均不能改变 C 数据。 |
| VIS-T04 | 006 | B 使用仅存在于 C 报价中的机构/联系人标记搜索，不能因隐藏值而得到命中或计数差异；覆盖 price BEST/FULL 与组合条件。 |
| VIS-T05 | 003、006 | B clone C 的报价被拒绝；不能产生包含 C 私有来源的新报价。 |
| VIS-T06 | 003、006 | C 原文含测试机构/联系人，B 的 quote详情、分页和增量完整 JSON 中均不泄露该标记。 |
| VIS-T07 | 005 | STC 行情长城交易员显示 C；报价来源字段只在 C 登录时完整显示。切换用户后不可复用前一用户缓存。 |
| VIS-T08 | 009 | B 不能订阅 C 的私有 topic，也不能订阅 internal 原始行情；重连后同样成立。 |
| VIS-T09 | 010 | B 提交“本人报价 + C 报价”的变更批次，越权整批拒绝，所有版本与状态不变；最终预期以冲突澄清后的产品决定锁定。 |
| VIS-T10 | 008 | 管理员授权范围、原因必填、owner/operator 区分及审计完整性；先明确具体授权矩阵再落测试。 |
| VIS-T11 | 006 | 通知、详情、Copy、导出沿用相同字段权限；验证原始协议数据，不只验证界面隐藏。 |

已有测试线索：`BusinessSmoke` 的非所有者操作拒绝、`IncrementalViewSmoke` 的分页四字段脱敏。它们不是完整规则覆盖，且本次没有重新执行。新用例应以规则编号命名/标注，并记录运行报告；不得将本清单当作测试通过报告。

## 维护方法

新增规则须包含编号、产品来源、角色、前置状态和可验证结果。实现改变后更新核查结论与证据，不改写规则迁就实现。产品决定变化时保留旧编号并注明替代关系；没有依据的建议必须标为待确认，不进入强制验收。
