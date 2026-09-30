# 精确报价接入 ParserSvr

本次使用已合并 LSJ 产品界面的 STC 客户端，不另建客户端或更换页面布局。入口为“报价录入 → 精确报价”。

## 调用流程

1. 用户登录 STC，在现有文本框输入或粘贴报价；Enter 换行，Ctrl+Enter 或“识别报价”按钮执行识别。
2. `StcQuoteParserClient` 复用宿主 DPS 连接，请求 `ParserSvr` 的 `stc.parser.bond`。
3. 识别结果进入现有可编辑预览表。识别本身不写入 QuoteSvr，也不建立订阅。
4. 用户核对后提交：调用 `ficc.quote.create` 创建待确认报价，再调用 `ficc.quote.confirm`。报价与行情刷新仍走已有 Quote/Price 独立订阅。

机构、联系人只有名称而没有 CRM ID 时会显示提示。确认仍遵循 QuoteSvr 的 ID 校验；可在报价确认页打开已有 CRM 编辑器补全，不把名称当作 ID。

## STC 消息

请求中的 `userName` 来自宿主登录用户，不是新增配置项：

```json
{"userName":"当前登录用户名","content":"24国开10 bid 2.5000 1000"}
```

应答结构示例（字段取自现有 ParserSvr 实现，不代表一次真实识别记录）：

```json
{
  "Code":"0000","Msg":"成功","TotalSize":1,
  "Datas":[{
    "securityId":"240210.IB","symbol":"24国开10",
    "side":"1","priceType":"3","price":2.5,"qty":1000,
    "settlType":"2","text":"24国开10 bid 2.5000 1000"
  }]
}
```

ParserSvr 使用 `Code/Msg/Datas`，不使用 FICC 的 `requestId/Data` 应答格式。Parser 的 `priceType=1/2/3/7` 分别映射净价、全价、到期收益率、行权收益率；向 QuoteSvr 提交时转换为它的 `quoteType/yieldType` 字典。`qty` 按万元传递，保留原始小数精度，合法精度由 QuoteSvr 校验。

ParserSvr 的 `settlType` 使用 STC 字典：`1` 当天 +0、`2` 当天 +1、`3` 明天 +0、`4` 明天 +1。客户端将 `3/4` 转换为明天交易日及 FICC 的 `1/2` 清算速度，不能直接解释为 T+2/T+3。

可选 `Warnings` 字符串数组显示在识别结果状态栏。已到期或摘牌债券仍返回预览，并明确提示债券代码和有效期问题；识别不写入 QuoteSvr，也不放宽正式报价校验。例如 `Bid 112508306.IB 25中信银行CD306 2000w 2 到期 明天+0` 返回 Bid、收益率 2、2000 万、明天 T+0，同时提示该债券已到期或摘牌。带单位报量与单个整数价格支持前后换序。普通 `3 1000` 格式也识别整数收益率；真实换行及字面 `\r\n` / `\n` 均拆为独立报价，价格、报量和清算字段不跨行混用。

`institutionId/traderId` 映射机构/联系人 ID；`clordid/status` 不作为 FICC 报价 ID 或生命周期状态。原文保存在报价 `remark`。

## 精确报价批量工具栏

先勾选预览行，再操作工具栏。未勾选时显示提示，不自动修改全部报价。`+1k/-1k` 固定增减 1000 万；报量输入框直接设置所选行数量（万）；收益率以百分数显示，`1 BP` 对应增加/减少 `0.01`。更换所选行后重新选择清算速度或计价方式，避免上一次选择让本次选择失效。

## 错误与重试

- 服务失败时保留原文及已有预览，显示错误码与原因；空结果保留原文。
- ParserSvr 不提供逐行识别失败数，界面仅显示返回条数，要求用户核对原文，不声称全部文本均已识别。
- 已收到创建成功应答时保留报价 ID。后续确认失败重试会复用待确认报价，并提交用户最新编辑字段；已确认行不重复确认。
- 创建请求超时、无法确定服务端是否写入时，需先在报价确认页核查；当前未实现断线后自动恢复未确定的批次。
- 完整 REQ/REP 复用 STC 的 INFO 日志输出，主题为 `stc.parser.bond`。订阅及推送继续使用原有日志。

## 验证与当前边界

从仓库根目录执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/build-native-client.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/test-parser-integration.ps1
```

2026-09-30 客户端构建通过；31 项隔离检查通过，覆盖消息字段、价格字典、明天清算映射、到期警告、CRM、精度、编辑、取消、错误、提交重试和现有页面命令。测试还检查真实 STC 日志文件包含完整识别请求/应答。隔离检查使用协议样本，不访问数据库，子进程有 30 秒看门狗。

本机 ParserSvr 已切换 Java 主引擎并完成服务初始化；原生引擎只用于异步比较。真实 STC UI 套件与最新结果见 [UI 自动化](ui-automation.zh-CN.md)。模糊报价、图片识别及 Linux SO 对照不属于本轮验收。
