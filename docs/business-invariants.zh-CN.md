# 销售交易业务规则与实现核查

本清单用于约束开发、评审和自动化测试。首批记录报价归属、可见性和操作权限；后续按业务主题追加稳定规则编号，不以现有实现反向定义产品要求。

规则依据优先级：用户明确确认的产品决定 → 产品需求文档 → 产品未规定处参考 SZ 旧实现。代码和 API 文档用于核查实现，不是产品规则的来源。发现规则冲突必须登记，不能自行选择较容易实现的一方。

核查日期：2026-10-01。代码基线：`29d4e03` 所在工作区。此次为源码审查，未运行服务、数据库、双账号或 UI 测试；“源码符合”不代表端到端验收通过。此次只新增文档和入口，没有修复下述代码问题。

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
| VIS-002 | 已确认的有效报价在行情中心共享；即使原始 QT 消息没有发给当前用户，也可见其行情要素。 | 2.1 正式报价、行情全部/最优。不能把私人接收范围误当作正式行情范围。 |
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
| C 确认 quote-2 后的行情中心 | 能看到 quote-2 的债券、方向、价量等行情要素；不能获得其报价机构和报价交易员。 |
| B 的报价管理 | 不把 C 的 quote-2 当作本人报价。 |
| B 尝试修改 C 的 quote-2 | 服务端拒绝。 |
| 长城交易员列 | quote-2 的所有者为 C，不是外部 A。 |

注意：同一外部原始报价被 B/C 接收后，内部记录是否合并、确认后是否去重，是另一条产品规则；本清单不擅自规定。

## 源码核查结果

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

优先级：中。`DpsSalesTradingServices.Market.BookRows` 从行情 entry 构造 quote 时没有拷贝 UserID、InstitutionID、TraderID；`WithReference` 仅补债券资料。`DpsModelMapper` 又依赖 ownerUserName 显示 WallTrader。

结果：源码没有完成“显示长城交易员”和“本人看完整报价来源”的链路。需要将内部用户名称解析和经授权返回的 CRM 身份接入；不能由客户端依据隐藏信息重新猜测他人身份。

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
| VIS-T02 | 002、003 | C 确认后，B 可见行情价量，机构/联系人 ID 和名称不可得；C 可见完整信息。 |
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
