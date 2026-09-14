# 关注债券与筛选分组界面

连接 GW 后，点击原型顶部 **关注债券** 或 **筛选分组**，打开独立管理窗口。

## 操作

1. 左侧选择已有分组，或点击“新建”。
2. 填写名称，选择利率债／信用债、行情／历史。
3. 关注债券：勾选需要的债券，取消勾选即可从组中移除。当前债券来自 BaseSvr 模拟接口，共 4 只。
4. 筛选分组：添加／删除条件行，选择字段、比较方式及条件值。子组序号用于组织条件；当前匹配逻辑为同字段 OR、不同字段 AND，并非子组之间 OR。
5. 点击“保存”；更新时携带当前 `expectedRevision`。版本冲突或请求失败保留编辑内容，可刷新后重新编辑。
6. “删除”删除整个分组，需确认。切换分组或关闭窗口时若有未保存修改，也会提示。

“包含任一”和“范围”的值用逗号分隔，例如 `AAA, AA+`、`2, 3`；范围包含边界。方向为 `1` 买入、`2` 卖出；交割为 `1` 当日、`2` 次日；急／荐／整／OCO／打包标签使用 `true` 或 `false`。字段与支持的比较方式来自服务端字典；不支持的组合无法保存。

## 应用到行情

保存行情分组后，点击 **应用到行情**，关闭管理窗口并重新打开行情视图。请求在 `filter.watchlistIds` 或 `filter.filterGroupIds` 中引用保存的组，同时传入市场；报价及行情仍使用各自独立的模型与 topic。

当前界面一次应用一个分组，可与行情模式、深度、债券及交割条件叠加。在行情工具栏点击“清除分组”即可移除分组限制。若在管理窗口删除正在应用的分组，关闭窗口后会自动清除该引用并刷新行情。

行情分组也会由 NotificationSvr 按个人提醒开关参与关注债券／筛选分组提醒，无需先应用到行情视图。历史分组可以保存和维护，但当前原型没有完整的历史分组查询页面。通知收件列表自身的 `scope=NOTIFICATION` 分组编辑不属于这两个窗口。

## 验证

```powershell
cd D:\workspace\greatewall\ficc\ficc
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\test-group-ui.ps1
```

23 项检查通过：两类分组新建、编辑、刷新、删除、空名称校验、无效条件组合校验、关注债券增减、多值与数值范围、行情接口应用。经真实 C# DPS → GW → QuoteSvr → PostgreSQL 执行；创建 2 个临时分组、0 条报价，正常完成后删除测试组。独立进程最多运行 30 秒。

结果为 `runtime/group-ui-smoke.json`，截图为 `runtime/group-ui-smoke-watchlist.png` 和 `runtime/group-ui-smoke-filterGroup.png`。该检查也已加入顶部“自动测试”的第 4 阶段。

源码：[管理窗口](../client/src/GroupManagerForm.cs)、[界面功能检查](../client/src/GroupManagerFormSmoke.cs)。
