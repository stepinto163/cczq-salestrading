# 报价确认时手动选择CRM对手

## 业务流程

1. 用户或外部来源创建报价时，可以只有 `institutionName`、`contactName`，两个ID为空。状态为待确认，不进入行情。
2. 用户在UI中查看原始名称，手动从CRM列表选择已有机构、联系人。UI将选中的 `institutionId`、`contactId` 和标准名称填入报价模型。
3. `ficc.quote.confirm` 同时提交quoteId、expectedRevision及上述字段。QuoteSvr只验证ID有效性、联系人是否属于机构，不按名称自动猜测ID。
4. 验证通过后，同一quoteId变为有效。原始录入名称保留在历史中。

有名称但没有对应ID时不能确认；失败返回 `UNRESOLVED_COUNTERPARTY`，不推进revision或状态。联系人ID必须与机构ID相符。未提供任何对手信息的报价仍遵循原有可空规则；ACTIVE更新和恢复也不能留下仅有名称、没有ID的对手信息。

## 平台操作

报价编辑区新增“对手机构”和“联系人”。新建时可直接输入自由文本。点击确认时，若名称尚未匹配ID，会打开选择窗口；先选机构，再从下拉列表选择该机构的联系人。同名联系人需通过机构和ID区分。

当前CRM列表使用固定模拟数据，经已有 `ficc.base.query` 的INSTITUTION／CONTACT接口获取；没有连接真实CRM。示例包含两家机构以及两个同名、不同机构的联系人，便于验证人工选择及归属检查。

## 验证

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\test-counterparty-ui.ps1
```

专项检查使用1条临时报价，经真实GW验证名称录入、确认阻止、机构不匹配拒绝、UI选定ID后确认、原名称历史、名称权限及ACTIVE更新校验。结果为 `runtime/counterparty-ui-smoke.json`，完整收发日志在结果引用的messageLogDirectory中。
