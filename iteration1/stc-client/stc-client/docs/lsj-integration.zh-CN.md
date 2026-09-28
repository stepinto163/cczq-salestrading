# LSJ 客户端合并说明

## 分支与代码来源

工作目录：`iteration1/lsj/stc-client`，独立 Git 仓库。
开发分支：`feature/ficc-lsj-integration`，从我方当前分支的 `336ba1b2c` 创建，再合并同事 `dev_sales_trading_v0` 的 `860421acf`。
原 `iteration1/stc-client/stc-client` 和同事原分支均保留。

保留同事的 MainView、模块划分、MVVM、产品 XAML 和主题，复用我方已验证的 DPS 通信、LivePage、CRM 报价编辑器及本机启动方案。插件默认实例化 `DpsSalesTradingServices`，通过宿主已登录的 ApiManager 连接 QuoteSvr / NotificationSvr，不再默认读取 Mock 报价。注入 Mock 的构造器仅用于隔离测试。

## 已连接的业务

- 报价确认：接收本人待确认报价推送；提交用户编辑后的字段与 expectedRevision；删除也使用所选行的 revision。
- 手工报价：顶栏入口；创建待确认、补全 CRM、确认、修改、Refer、Unrefer、删除、变更历史。修改机构／联系人名称时清除原 ID，必须重新匹配后确认。
- 报价管理：本人有效／撤销报价的查询与实时更新；Refer / Unrefer / 删除；“修改 / 历史”打开复用编辑器；历史查询读取后端。
- 行情中心：信用／利率、全部／最优分别建立 Price 视图；FULL 映射报价条目，BEST 映射债券最佳档。Quote 与 Price 保持独立模型、主题和订阅。Price 查询的 totalCount 是债券数。
- 报价提醒：独立 Notification 订阅、四类原因映射、已读操作。提醒 ID 与报价 ID 不混用，聚合行情不能当作单笔报价直接修改。
- 日志：复用 STC 日志，记录 DPS REQ、REP、SUB、UNSUB、PUSH；默认不输出心跳。

## 当前边界

这次是代码合并与主链路接通，不是全部产品功能验收。同事尚未完成的侧栏复杂筛选、关注债券／筛选分组持久化、通知偏好、翻页／滚动交互、部分右键命令仍需逐项接入。已有相关通信与旧视图代码保留，但不能据此认为新产品控件已经接通。

自然语言识别、模糊报价及智能推荐没有可用的完整服务实现，明确提示未接入，不伪造成功。报价来源／长城交易员专属筛选、仅我的行情／未报价债券等未映射条件也明确提示。当前“今日报价”仍代表当前有效／撤销集合，尚无交易日限定。历史界面目前读取指定日期范围内首批最多100条，尚未接续游标；不能把首批记录当作完整历史。

本机使用模拟的登录／基础资料等依赖，报价和提醒仍调用真实本机服务及数据库。右侧外部 STC 行情保留同事 HostQuoteFeed；本轮没有连接 QA 验证该通道。行情中后端未返回的联系人／机构／成交等资料保持缺失，未编造显示值。

## 构建与启动

在本目录执行，需 Visual Studio Build Tools（WPF）及 .NET Framework 4.7.2 targeting pack。NuGet 依赖从仓库 `nuget/` 还原，构建限制为单进程；输出隔离在 `runtime/native-client`。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\build-native-client.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\start-native-client.ps1 -Mode Local
```

本机后端应已启动：GW 18600、QuoteSvr 18680、NotificationSvr 18690；本机测试账号 trader1 / trader1。真实 STC 环境使用已配置的宿主配置：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\start-native-client.ps1 -Mode Stc -StcConfig 'E:\config\STC.exe.config'
```

## 验证

先关闭 STC 客户端，再执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\test-local-integration.ps1 -Merged
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\test-local-integration.ps1 -NativeUi
```

`-SalesTrading` 是 `-Merged` 的兼容入口。合并功能测试只创建一条报价，验证产品确认页提交编辑字段、修改、Refer、Unrefer、历史和实时列表，最后删除测试报价；独立进程设置45秒看门狗，不运行压力测试。检查本轮 STC 日志没有 ERROR，截图由真实 WPF 视图渲染生成。

报告：`runtime/native-client/merged-salestrading-result.txt`；截图：`merged-management.png`、`merged-market.png`；日志：`runtime/native-client/logs/`。四类提醒原因已映射，本轮未分别制造四类触发场景，不能宣称完成全部提醒回归。
