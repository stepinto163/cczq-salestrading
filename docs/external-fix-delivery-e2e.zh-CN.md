# 外部 QTrade FIX 投递端到端验证

日期：2026-10-16。分支：`feature/ficc-shared-confirmed-quotes`，测试基线 `12e77ac`。

## 结果

恢复本机服务发现后，4 个外部 FIX 用例全部通过，接收账号的本例 STC 报价全部清理成功。

| 用例 | 发布方式 | U41 发送序号 | U29 接收序号 | 接收报价数 | 结果 |
| --- | --- | --- | --- | --- | --- |
| 24-qtrade-direct-structured | 单聊结构化 | 6 | 6 | 1 | PASSED |
| 25-qtrade-direct-text | 单聊文本 | 7 | 10 | 1 | PASSED |
| 26-qtrade-group-text | 群聊文本，Bid/Ofr 两行 | 8 | 15 | 2 | PASSED |
| 41-qtrade-group-structured | 群聊结构化 | 9 | 27 | 1 | PASSED |

发送和接收是两个独立的 FIX 序号序列，不能把同一行两列序号的差值当作丢包或延迟。

实际链路：自动化工具登录 STC → GW 调用 QtIntakeSvr 发布 API → 向外部 QTrade 发送 `35=U41` → 收到 `35=U29` 业务回执 → 收到 `35=S` → QtIntakeSvr 转发 QuoteSvr → STC 报价确认页收到推送 → UI 选择 CRM 联系人并确认 → 行情中心显示正式报价。

4 个回执均为 `ACKNOWLEDGED`、`applicationCode=0`。接收方关联证据包含 5 条 `35=S`，不是仅检查发布 API 返回成功。本轮未通过接入 API 注入接收报价。

## 环境与业务断言

- 发送人：李继存，QT `2881631456`。
- 接收人：龚逸，STC 登录 `adrian`，通过用户目录 `inf1` 解析 QT `3004468043`。
- 群聊目标：销售交易内测群，发布群号 `1125362859`。
- 使用 `240210.IB`，每条报价量为 1 万，每次生成不同测试价格，避免 QTrade 复用旧意向影响关联。
- 单聊和群聊都验证债券、价格、数量、清算速度、接收账号、归属人、`NegotiationID`。群聊另验证来源群标识，两行文本属于同一接收批次。
- 未确认报价不进入正式行情；通过真实 CRM 控件选择李继存／长城证券股份有限公司后，在 UI 确认，检查 ACTIVE 状态、退出待确认及真实行情 DataGrid。
- 用例串行，操作间隔 2 秒，使用现有进程看门狗。不是性能测试，不根据两端 FIX 时间戳计算网络延迟。

## 初次失败与恢复

首次运行目录 `external-fix-20261016-174905`：4 个用例均在读取 STC 用户目录时失败，尚未发送发布消息。GW 返回 `1003 / [1015] SERVER.AdminSvr is not Online`，AdminSvr 进程虽仍监听但不能据此判定业务就绪。

日志显示 AdminSvr 曾向 ZooKeeper 注册旧 Wi-Fi 地址 `10.99.200.219`；当前网卡为 `10.132.199.12`。重启 AdminSvr 后新地址已注册，但 GW 仍重试旧地址。随后重启本机 GW，17:51:18 记录到新 AdminSvr 地址的 `CONNECTION_SUCCESS` 和 `CONNECTION_LOGIN_SUCCESS`，重新执行测试通过。

这是服务重启后的运行恢复，不是修改旧框架以永久修复网卡变更／服务发现问题。此前桌面检查中部分归属人显示 userid，也伴随该用户目录不可用现象。

## 证据与清理边界

最终报告根目录：

`iteration1/stc-client/stc-client/runtime/ui-test-client/ui-test-reports/external-fix-20261016-175131/`

- `summary.json`：4 个用例状态和清理结果。
- 每例 `qtrade-test-context.json`、`qtrade-publish-ack.json`、`qtrade-received-quotes.json`：发送／接收身份、业务回执及接收报价关联。
- `fix-wire-evidence.json`：从真实 QtIntakeSvr 日志按 MessageID／NegotiationID 提取的 4 条 U41、4 条 U29、5 条 S 的必要字段，不复制登录凭据。
- 每例 `events.jsonl`、`result.json`、`qtrade-real-pending.png`、`qtrade-counterparty-selected.png`、`qtrade-real-confirmed-market.png`：UI 操作和显示证据。
- 原始 FIX 记录位于本机 `qa_env/ats_root/log/QtIntakeSvr.console.log`。

清理仅撤销／删除当前接收账号的本例 STC 报价，不删除外部 QTrade 聊天消息，也不清理其他收件人的报价。不声称验证外部议价、撤销、超时恢复或所有群成员的显示。
