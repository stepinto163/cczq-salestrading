# QTrade 报价接入服务

项目：`cgws-ficc-saletrading-qtintake`；进程名：`QtIntake`；Java 8。

```text
QTrade FIX → QtIntake → 本地持久化待发送目录
                         ↓
                  MYSQL0 查询 STC 报价归属人
                         ↓ DPS / STC 服务发现
                  QuoteSvr / ficc.quote.create
                         ↓
                 PostgreSQL + 待确认报价推送
```

## 本版范围

- 使用现有 PRL 接收程序的 QuickFIX/J 连接方式，不向 QTrade 发出报价、订单或撤单。
- 接收 `35=S`、`TransType(18045)=N/H`，沿用旧销售交易下行的新增意向／对话回价范围，每条创建一条待确认报价。
- STC 的 resource、stream、handler、control 模式；通过 `SERVER.QuoteSvr` 发现服务，不填写 QuoteSvr IP，不模拟用户登录。
- 复用 `ficc.quote.create` 的校验、写库、历史与事件推送逻辑。接入服务按 STC 内网可信服务调用提交 `intake` 来源信息，不使用额外共享令牌；普通客户端创建接口不变。
- 不自动确认报价，不处理 QTrade 成交、撤销以及其他 FIX 业务报文；这些报文会明确记录为 `IGNORED`。本版不实现旧服务的群组转发、用户意向维护或交易回报同步。

## 用户与联系人

归属人遵循旧项目 `UserInfoDao.queryUserByQt`：

```sql
SELECT userid FROM stc_tbook_users WHERE qtAccount = ? AND enable = 1
```

参数是报价的 `OwnQtNO(18004)`，数据库来源为共享 DBPool 配置的 `MYSQL0`。结果必须是唯一有效用户；没有结果、多个结果或数据库不可用时，报价保留在待发送目录并记录错误。用户不需要在线。首次解析的归属人在发送前持久化，超时重试不重新分配归属人。

`InterloNickname(18006)` 是对手联系人名称，`OppInstiFullName(18036)`／`OppInstiShortName(18037)` 是对手机构名称，不能作为报价归属人。接入时不猜测 CRM ID；确认阶段由前端选择正确机构和联系人。

## 字段转换

| QTrade 字段 | QuoteSvr 字段／规则 |
|---|---|
| SecurityID(48)、ExDestination(100) | `securityId`；XCFE→.IB、XSHG→.SH、XSHE→.SZ、BJSE→.BJ、CNBM→.BC；未知市场拒绝，不猜测 |
| Side(54) | `side`：1 买入、2 卖出；不使用旧中间模型的 0/1 编码 |
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

- FIX 回调先写本地文件并刷新内容，再返回。单个转发线程每秒处理最多一条，默认待发送上限 10000，可根据实际流量后续调整；这不是吞吐压测结果。
- `data/inbox/pending`：未完成；`sent`：成功及响应；`rejected`：字段错误或业务拒绝及原因。目录有进程锁；同一目录不可运行两个实例。
- QuoteSvr 离线、调用超时、顶层 `1009/1099` 保留待重试。解析不到归属人的报价轮转等待，不阻塞其他账号。
- 固定 `requestId` 和 `clientQuoteId`，使用 QuoteSvr PostgreSQL 请求回执去重。FIX 重传的序号／发送时间变化不改变业务事件键。相同事件键但内容变化报冲突，不覆盖已确认报价。
- 磁盘异常或容量超限停止 FIX 连接并报错；恢复后需要上游重放。文件内容刷新及原子重命名不等于跨机器持久化，不能删除接入目录后假定未发送数据仍在。成功归档没有自动清理策略，需按保留期限运维归档。
- 日志位于 `$ATS_ROOT/log`：`QtIntake_日期.log`、`QtIntake-startup_日期.log`、`QtIntake-dps_日期.log`。FIX 全文、DPS 请求／响应在消息日志 DEBUG 输出，FIX 登录字段也完整输出。组件初始化与错误在 INFO/ERROR 可见。
- `ficc.qtintake.status.query` 返回 FIX 连接状态、存储故障状态和 QuoteSvr 在线状态；初始化成功不代表远端连接成功。

## 构建与 QA 部署

内部 Maven 环境使用 `mvn clean package`（主 `pom.xml`）；本机兼容环境使用 `mvn -f pom_compat.xml package`。先安装对应 parent/common。主 POM 使用 STC 依赖；不修改旧 STC 项目。

QA Jenkins 使用 `sslib@main`，项目应位于 checkout 根目录。部署目录 `/opt/sumscope/stc/QtIntake`，运行用户 `sumscope`，继续使用全局 `ATS_ROOT`。部署前将 QtIntake 加入已有 start/stop 脚本，并填写服务注册信息。

1. `config/qtrade-fix.cfg` 填写已开通的 FIX SenderCompID、TargetCompID 和地址。示例地址为 PRL，不能直接视为 QA 已配置。
2. 在 QtIntake 的 `config/service.properties` 填写 `fix.username`、`fix.password`。Jenkins QA 包对应模板位于 `salt-formula/cgws-ficc-saletrading-qtintake/files/config/service.properties`，按团队配置管理方式维护。
3. QuoteSvr 与 QtIntake 同时更新，无需额外的服务令牌或认证文件。旧配置中的 `intake.authFile`、`intake.token`、`secrets.file` 可删除；原认证文件中的 FIX 用户／密码迁移到上一步的服务配置。
4. `MYSQL0` 继续指向 STC 数据库；不创建新的用户映射表，不修改 STC 表。
5. 全局 ATSConfig 配置 `SERVER.QtIntake` 和 `SERVER.QuoteSvr` 的服务发现，参照 `deployment/registry.example.ini`。
6. 从服务目录运行 `bash bin/start`，或使用已配置的 STC start 脚本。

`intake` 是可信服务接入报文；QuoteSvr 信任 QtIntake 提交的已解析归属人，仍检查来源必填字段和报价数据。它不进行额外的调用方令牌校验。普通前端不携带 `intake`，继续使用用户会话；报价后续确认、修改、撤销等操作仍遵循现有用户权限。

## 请求示例

```json
{
  "requestId": "QT-稳定来源事件哈希",
  "intake": {
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

本机 Java 8 单元测试覆盖四种价格类型、双方向、数量精度、市场后缀、FIX 重放去重、缓冲恢复、超时重试、嵌套业务错误和 FIX 完整消息日志；本机 FIX socket 功能测试使用一条合成报价。

QuoteSvr 的 `QtIntakeSmoke` 使用本机 PostgreSQL 独立临时 schema，验证两名用户、来源关联、待确认、事件推送以及服务重启后的去重。未连接真实 QTrade、未部署 QA、未进行压力测试。QA 仍需核对真实 FIX 报文与实际 STC 依赖版本。
