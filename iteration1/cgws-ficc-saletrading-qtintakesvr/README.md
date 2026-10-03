# QTrade 报价接入服务

项目：`cgws-ficc-saletrading-qtintakesvr`；进程名：`QtIntakeSvr`；Java 8。

```text
QTrade FIX → QtIntakeSvr → 本地持久化待发送目录
                         ↓
                  MYSQL0 查询 STC 报价归属人
                         ↓ DPS / STC 服务发现
                  QuoteSvr / ficc.quote.create
                         ↓
                 PostgreSQL + 待确认报价推送
```

## 本版范围

- 使用现有 PRL 接收程序的 QuickFIX/J 连接方式，不向 QTrade 发出报价、订单或撤单。
- 接收 `35=S`、`TransType(18045)=N/H` 且 `Operator(18035)=2` 的对手方报价，每条创建一条待确认报价。本方发送的 `Operator=1` 不进入待确认列表。
- STC 的 resource、stream、handler、control 模式；通过 `SERVER.QuoteSvr` 发现服务，不填写 QuoteSvr IP，不模拟用户登录。
- 复用 `ficc.quote.create` 的校验、写库、历史与事件推送逻辑。接入服务提交带鉴权的 `intake` 来源信息；普通客户端创建接口不变。
- 不自动确认报价，不处理 QTrade 成交、撤销以及其他 FIX 业务报文；这些报文会明确记录为 `IGNORED`。本版不实现旧服务的群组转发、用户意向维护或交易回报同步。


## 原始消息分组（报价确认）

一条原始消息对应一个组，不是一个 QT 聊天群对应一个组。多条 FIX Quote 各自创建报价，但共享 QuoteSvr 返回的 `sourceMessageId`。原文记录持有全部 `quoteIds`；STC 客户端按此字段聚合，支持整组选中、部分确认/删除、最后一条待确认报价移出后删除组头。

- `intake.externalMessageId` / `requestId` 保持单条报价事件的幂等语义。
- 新增可选 `intake.originalMessageId`，由本方账号、对话人、聊天类型/群标识、业务时间、操作类型/操作方和原始语料生成稳定关联键。缺少原文时退回单条事件，不按机构、聊天群或空文本合组。QuoteSvr 再按账号与 STC 归属人隔离来源记录。
- 依据本地接口定义《现券机构逐笔报价解析数据下行 V1.4 (FIX)》：`18026=Content`、`60=TransactTime`、`18058=InterloNickID`、`18136=ChatGroupID`、`18474=ChatGroupNickID`。Quote(S) 未提供保证可用的原始消息唯一 ID；广播的 `18044=MsgID` 及 U49 的 `18336=ContextMsgID` 不能直接套用。
- **推导边界：** 同一对话、同一业务时间的完全相同文本无法可靠区分；同一原文的逐笔下行业务时间不同则不会合组。需真实多券原文 FIX 样本核对。若后续提供原始消息唯一 ID，应替换关联键生成依据，不改变前端分组协议。
- 接口允许 `117=QuoteID=-10000` 表示空，此时用 `18033=NegotiationID` 构造单条事件标识，避免同一消息内多券互相覆盖；缺少该 ID 明确报错。

SZ 参考链路：旧 QtradeSvr 的 `MessageListener` 按用户列表、对手联系人和 remark 复用 `quoteGroupId`；SBPSvr 的 `SbpStream` 复制该字段并通过 `quote_group_id` 持久化；SZ 客户端 `QuotationConfirmViewModel` 按 `QuoteGroupId` 分组，增量加入已有组，移除空组。本实现沿用服务端提供组标识的方式，增加时间及收件人隔离，避免仅按内容长期合组。

升级顺序：先部署 QuoteSvr，再部署 QtIntakeSvr，最后更新 STC 客户端。旧队列及旧来源记录保持原样，新增分组字段不会让已有普通事件重放失败，也不自动重分历史报价。旧版曾以 `-10000` 作为事件标识的数据不能可靠反推 NegotiationID，升级前需核对这类旧记录，不自动重投历史数据。

## 用户与联系人
归属人不再查库：QA 的 `stc_tbook_users` 里**没有 QTrade 账号列**（旧 `UserInfoDao.queryUserByQt`
是 MySQL 写法，`qtAccount` 大小写不敏感，QA 的 MYSQL0 是 PostgreSQL）。归属关系改由运维维护的映射文件给出：

```properties
# $ATS_ROOT/control/QtIntakeOwnerMap.properties
QT-0001=stc_userid_1
QT-0002=stc_userid_2
```

左边是 QTrade 账号（FIX `OwnQtNO(18004)`），右边是 STC `userid`。文件改动会被自动重新加载，新增账号
不用重启。映射未命中时落到 `owner.defaultUserId`；两者都没配置就**启动失败**；都没命中则该条报价留在
待发送目录重试，不猜归属人。配置项写在 `config/service.properties`：`owner.mapFile`、`owner.defaultUserId`。

参数是报价的 `OwnQtNO(18004)`，数据库来源为共享 DBPool 配置的 `MYSQL0`。结果必须是唯一有效用户；没有结果、多个结果或数据库不可用时，报价保留在待发送目录并记录错误。用户不需要在线。首次解析的归属人在发送前持久化，超时重试不重新分配归属人。

`InterloNickname(18006)` 是对手联系人名称，`OppInstiFullName(18036)`／`OppInstiShortName(18037)` 是对手机构名称，不能作为报价归属人。接入时不猜测 CRM ID；确认阶段由前端选择正确机构和联系人。

这些对手字段在本方发送的报文中也存在，不能据此判断消息发送人。必须先判断 `Operator(18035)`：1 为本方、2 为对方；买卖方向 `Side` 和新增/回价类型 `TransType` 都不能代替发送方向。对方的 Bid/Ofr、N/H 均可进入待确认，本方两种方向都忽略；缺少或未知 Operator 则拒绝并记录原因，不猜测。忽略事件在 INFO 输出 `FIX state=IGNORED reason=SELF_SENT_QUOTE operator=1`，附 quoteId、negotiationId，原始报文仍保留在 DEBUG 消息日志。

2026-09-29 聊天回放回归：对方 Bid 3.126/400万和本方回复 Ofr 3.106/100万共用相同会话及对手信息，只产生前一条待确认报价。旧版本已经入库的错误报价不会自动删除；旧持久化队列没有保存 Operator，不能从对手名称可靠恢复方向，升级前应按原始 FIX 日志核对遗留项。

## 字段转换

| QTrade 字段 | QuoteSvr 字段／规则 |
|---|---|
| SecurityID(48)、ExDestination(100) | `securityId`；XCFE→.IB、XSHG→.SH、XSHE→.SZ、BJSE→.BJ、CNBM→.BC；未知市场拒绝，不猜测 |
| Side(54) | `side`：1 买入、2 卖出；不使用旧中间模型的 0/1 编码 |
| Operator(18035) | 2 对手发送才创建；1 本方发送忽略；缺失或未知值拒绝 |
| BidPx(132)／OfferPx(133) | 按方向取值，写入十进制字符串 `price` |
| BidPxType(18042)／OfferPxType(18043) | 1→收益率/到期；2→收益率/行权；3→净价；4→全价；净价、全价的 yieldType 为空 |
| BidSize(134)／OfferSize(135) | 除以 10000 转为 `quantity`，单位 WAN；保留小数，不截断 |
| BidSettlType(18167)／OfrSettlType(18169)、SettlType(63) | 按方向优先读取买方／卖方字段，缺失时回退到 63；1=T+0、2=T+1，最终由 QuoteSvr 校验；不读取另一侧字段，不从原文猜测 |
| BidTradeDate(18166)／OfrTradeDate(18168)、TradeDate(75)、TransactTime(60) | 按方向优先读取交易日，缺失时回退到 75，再取 60 的日期部分。QTrade 的 60 为业务本地时间，不能按 UTC 再加八小时；不使用报文头 SendingTime(52) 推导交易日 |
| OwnQtNO(18004) | 来源账号，查询 STC userid 后写入 `ownerUserId` |
| Content(18026) | 原文、报价备注 |
| QuoteID(117)、TransactTime(60) | 与 OwnQtNO、TransType 共同构成稳定来源事件键 |

收益率最多四位小数。错误字段写入 `rejected`，记录明确异常，不默默填默认值。

## 重试与可观察性

- FIX 回调先写本地文件并刷新内容，再唤醒转发线程。单个线程按持久化 arrivalSequence 顺序连续发送，成功后立即处理下一条，空队列阻塞等待；没有每秒一条的限速。默认待发送上限 10000 是积压容量，不是吞吐目标。
- `data/inbox/pending`：未完成；`sent`：成功及响应；`rejected`：明确的报价字段校验拒绝及原因；权限、映射、冲突和未知错误保留 pending。目录有进程锁；同一目录不可运行两个实例。
- QuoteSvr 离线、可识别的通信异常、顶层或条目级 `1009/1099` 保留队首，失败后固定等待 1 秒重试，不设最大重试次数；成功后恢复即时发送。缺失归属人、权限/配置错误、未知错误或不完整应答不丢弃，阻塞队首并每 1 秒复查。后续消息不得越过失败消息。
- 永久拒绝仅识别 `1001 + error.type=VALIDATION_FAILED + 已知报价字段`；机构/联系人关联、owner、未知字段等保守留队。明确拒绝归档后才继续下一条。
- 重启按 arrivalSequence 恢复；旧 pending 文件首次迁移按原修改时间及文件名固定顺序并记录 WARN。旧版本曾轮转时间戳，无法还原真实历史到达顺序；新消息不再修改排序键。禁止运行时手工移动队列文件，修正 rejected 后的重提交需单独处理请求幂等键。
- 固定 `requestId` 和 `clientQuoteId`，使用 QuoteSvr PostgreSQL 请求回执去重。FIX 重传的序号／发送时间变化不改变业务事件键。相同事件键但内容变化报冲突，不覆盖已确认报价。
- 磁盘异常或容量超限停止 FIX 连接并报错；未保存的输入需核对补齐；能否通过 FIX 补发恢复取决于会话状态和 QTrade 支持，不能保证自动恢复。文件内容刷新及原子重命名不等于跨机器持久化，不能删除接入目录后假定未发送数据仍在。成功归档没有自动清理策略，需按保留期限运维归档。
- 日志位于 `$ATS_ROOT/log`：`QtIntakeSvr_日期.log`、`QtIntakeSvr-startup_日期.log`、`QtIntakeSvr-dps_日期.log`。FIX 全文、DPS 请求／响应在消息日志 DEBUG 输出，凭据脱敏。组件初始化与错误在 INFO/ERROR 可见。
- `ficc.qtintake.status.query` 返回 FIX 连接状态、存储故障状态和 QuoteSvr 在线状态；初始化成功不代表远端连接成功。
- FIX 连接前执行 `APPLICATION_PROTOCOL` 组件检查；缺失协议模型类会记录 FAILED 并终止初始化，避免到首条业务消息才暴露旧 common JAR。部署必须同时更新服务 JAR 与匹配的 `javalib`，使用仓库 `deploy qtintakesvr` 包装脚本，不单独复制服务 JAR。

2026-09-29 群聊恢复验证：旧 common JAR 缺少 `ficc.api.QuoteCreateItem`，导致接收方向报价处理异常。修正完整运行包后，按原始 FIX 长度和校验和核对两条报文，保留幂等 ID 写回待发送队列；QuoteSvr 两条均返回成功，运行中的 STC 客户端日志确认收到两次推送。启动依赖检查及 FIX/转发回归共 27 项通过。原始业务报文仅保留在本机运行日志，不提交仓库。

## 构建与 QA 部署

本机与内部 Maven 环境统一使用 `mvn clean package`（主 `pom.xml`）。先安装对应 parent/common。主 POM 使用 STC 依赖；不修改旧 STC 项目。

QA Jenkins 使用 `sslib@main`，项目应位于 checkout 根目录。部署目录 `/opt/sumscope/stc/QtIntakeSvr`，运行用户 `sumscope`，继续使用全局 `ATS_ROOT`。部署前将 QtIntakeSvr 加入已有 start/stop 脚本，并填写服务注册信息。

1. `config/qtrade-fix.cfg` 填写已开通的 FIX SenderCompID、TargetCompID 和地址。示例地址为 PRL，不能直接视为 QA 已配置。
2. 复制 `deployment/QtIntakeSvrAuth.example.properties` 为 `$ATS_ROOT/control/QtIntakeSvrAuth.properties`，填写 FIX 用户／密码及随机 `intake.token`（至少 32 位）。文件仅在服务器保存。
3. QuoteSvr 的 `config/adapters.properties` 增加 `intake.authFile=$ATS_ROOT/control/QtIntakeSvrAuth.properties`，重启更新后的 QuoteSvr。缺少此配置时，服务接入创建默认禁用。
4. `MYSQL0` 继续指向 STC 数据库；不创建新的用户映射表，不修改 STC 表。
5. 全局 ATSConfig 配置 `SERVER.QtIntakeSvr` 和 `SERVER.QuoteSvr` 的服务发现，参照 `deployment/registry.example.ini`。
6. 从服务目录运行 `bash bin/start`，或使用已配置的 STC start 脚本。

`intake.token` 是限定于此接入路径的服务凭据，不是用户密码，也不用于 LoginSvr 登录。普通前端不能通过传入 ownerUserId 代替用户会话；带 intake 的请求必须验证此凭据。

## QA 启动入口（ATS）

QA 的 ATS 管理脚本 `~/scripts/startconsole <Svr>` 决定 JVM 主类的方式是：先看 `~/scripts/serverlist` 里该 server 有没有 `entrance:` 字段，**没有就用 server 名本身当主类**。我们的 serverlist 条目（`stc/QtIntakeSvr|QtIntakeSvr|...`）没有该字段，所以实际执行的是 `java ... QtIntakeSvr`。

因此每个服务必须有一个**默认包、类名与 server 名完全一致**的启动类 —— 这也是 STC 的既有约定（`LoginSvr`、`OHLCSvr`、`BaseSvr`、`QuoteSvr` 都在默认包）。本服务的入口：

- `src/main/java/QtIntakeSvr.java`（默认包）→ 委托给 `ficc.qtintake.Main`
- `pom.xml` 的 `mainClass`、`deployment/bin/start` 都指向 `QtIntakeSvr`

把它改名或挪进包里，QA 会立刻报 `Could not find or load main class QtIntakeSvr`。替代方案是在 `~/scripts/serverlist` 对应行加 `entrance:ficc.qtintake.Main`，但该文件由 QA 运维维护，服务包自带启动类更省事。

### 接入 `~/scripts` 的操作步骤

`serverlist` 每行是 `路径|服务名|选项`（照现有格式）。新增一行：

```
stc/QtIntakeSvr|QtIntakeSvr| option: <照抄 QuoteSvr 那一行 option: 之后的内容，保持 JVM 参数一致>
```

**这行不要写 `entrance:`** —— 留空时 `startconsole` 才会拿第 2 列的 `QtIntakeSvr` 当主类，
正好命中默认包里的入口类。路径 `stc/QtIntakeSvr` 对应部署目录 `/opt/sumscope/stc/QtIntakeSvr`。

在 QA 上先确认这批脚本是通用的还是按服务分支的：

```bash
grep -n "QuoteSvr\|QtIntakeSvr" ~/scripts/start ~/scripts/stop ~/scripts/serverlist
```

- 只有 `serverlist` 命中 `QuoteSvr` ⇒ 脚本按条目通用遍历，加完这一行就够了。
- `start` / `stop` 也命中 ⇒ 它们有按服务名的 `case` 分支，照 `QuoteSvr` 那条补一条 `QtIntakeSvr`。

改完**先手工验证再走 Jenkins**：`~/scripts/start QtIntakeSvr` 看能否起来、
`~/scripts/stop QtIntakeSvr` 能否停干净 —— Jenkins 的 `preCommands/postCommands` 调用的就是这两个脚本，
它们出问题会让 rel 分支部署卡在半途。

## 请求示例

```json
{
  "requestId": "QT-稳定来源事件哈希",
  "intake": {
    "token": "服务凭据，日志脱敏",
    "sourceUserId": "QTrade账号",
    "ownerUserId": "根据stc_tbook_users解析的userid",
    "externalMessageId": "稳定来源事件哈希",
    "externalQuoteId": "QTrade-QuoteID",
    "sourceTime": "20260922-01:02:03.123",
    "transType": "N",
    "text": "报价原文"
  },
  "quotes": [{
    "clientQuoteId": "QT-稳定来源事件哈希",
    "securityId": "250001.IB",
    "side": "1",
    "quoteType": "1",
    "yieldType": "1",
    "price": "2.5",
    "quantity": "1000",
    "quantityUnit": "WAN",
    "currency": "CNY",
    "tradeDate": "2026-09-22",
    "settlType": "2",
    "institutionName": "待匹配机构",
    "contactName": "待匹配联系人"
  }]
}
```

响应仍为现有创建接口格式，需要检查外层 Code 和 `Data.results` 中每条 Code。报价随后由普通确认接口转为有效，行情价格和报价提醒继续由现有服务处理。

## 验证范围

本机 Java 8 单元测试覆盖四种价格类型、双方向、数量精度、市场后缀、FIX 重放去重、缓冲恢复、超时重试、嵌套业务错误和凭据脱敏；本机 FIX socket 功能测试使用一条合成报价。

QuoteSvr 的 `QtIntakeSmoke` 使用本机 PostgreSQL 独立临时 schema，验证两名用户、来源关联、待确认、事件推送、临时依赖故障重试以及服务重启后的去重。

2026-09-29 已将实际收到的两笔 QTrade 报价送入本机 STC 环境：240210.IB 与 250210.IB 共用一个 sourceMessageId，均为待确认且未重复。Java 单元测试 22 项、PostgreSQL 检查 33 项、WPF 分组检查 9 项通过；WPF 检查使用数据库导出的测试数据，不等同于真实 FIX 到已登录桌面的全链路 UI 验证。未部署远端 QA，未进行压力测试。

外层或条目级 1009/1099 均保留在 pending 重试；QuoteSvr 不将临时依赖失败写成最终幂等结果。

2026-10-02 转发恢复核查：移除正常发送的一秒等待；增加持久化顺序、失败队首保留、保守错误分类及 pendingCount。新增功能用例覆盖超时后重启顺序、权限/未知错误保留、明确拒绝、旧文件迁移、空闲唤醒和两条请求连续转发。仅本机合成数据验证，不代表真实 QTrade → STC 延迟验收。
