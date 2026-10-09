# 报价组合筛选端到端延迟

## 2026-10-09 首轮结果

结论：**结果正确，但未达到此前 p95 < 100 ms 的目标**。从真实 STC 报价管理侧栏，经本机 GW、QuoteSvr，到 WPF 列表绑定与下一次渲染回调计时。不是仅调用 matcher，也不以请求返回冒充界面完成。

| 交互 | 有效样本 | p50 | p95 | 最大 |
|---|---:|---:|---:|---:|
| 点击“筛选”到 WPF 渲染回调 | 20 | 332.56 ms | 394.55 ms | 427.29 ms |
| 修改条件后自动筛选到 WPF 渲染回调 | 20 | 616.11 ms | 678.09 ms | 700.84 ms |
| 点击路径的 view.open 请求/应答日志间隔 | 20 | 12 ms | 26 ms | 27 ms |
| 自动路径的 view.open 请求/应答日志间隔 | 20 | 11 ms | 25 ms | 26 ms |

首次筛选单独记录为 348.23 ms，不纳入上述预热分位数。这里的首次不是冷 JVM 启动。分位数使用 nearest-rank，20 样本仅适合首轮排查，不作为高负载 SLA 结论；各阶段的 p95 不能直接相加。

自动筛选包含侧栏 300 ms 防抖：实测输入到 Loading 的 p95 为 328.81 ms。点击路径从 Loading 到结果绑定 p95 为 261.33 ms，绑定后到渲染回调 p95 为 176.22 ms。请求应答只有约 25 ms，提示主要剩余成本在客户端处理、绑定和布局/渲染，尚未对这些内部步骤进一步剖析。没有修改生产实现来美化数据。

后续已完成[客户端应答到渲染路径剖析](client-response-profile.zh-CN.md)，定位协议快照维护与 WPF Render 阶段的主要成本。

## 数据与边界

- 单一 STC 测试客户端，`test2`，本机 `127.0.0.1:28600` GW；其他本机服务保持运行。
- 新建 500 条手工 Bid 有效报价，债券 `240210.IB`。原管理池另有 3 条报价；全库历史/已删除报价未清空，因此不声称整个进程只缓存 500 条。
- 使用特殊测试收益率 12.001–12.010 隔离现有数据，启动前要求该区间无既存有效/撤销报价。报价仅用于本机测试，不是市场行情。
- 两组均为 AND：计价方式=收益率、Bid 12.005–12.009、Vol.Bid≥400万；清算分别 T+0/T+1。独立按夹具生成规则核对完整报价 ID 集合和总数，分别得到 60/40 条。
- 服务分页上限 100；两组匹配结果都在一页内。虚拟化开启，每次仅生成 19 个行容器。500 是测试报价数，40/60 是列表项数，19 是实际生成的行容器数。
- 测量终点为 WPF 布局及 `CompositionTarget.Rendering` 回调，未测量显示器实际像素呈现延迟；不含用户移动鼠标及操作系统输入注入延迟。
- QuoteSvr 使用本机已部署的 2026-10-08 16:11:39 JAR，384 MB 最大堆、2 个逻辑处理器设置；没有部署刚完成的报价列存储变更。客户端为当前源码构建。
- 仅覆盖报价管理、单债券与两组组合条件。未覆盖行情中心、多债券基础数据冷加载、远程 QA、多用户并发或其他复杂条件。

原始 41 个计时样本、条件、服务包 SHA-256 及汇总见[结果 JSON](../tests/ui/benchmarks/results/20261009-filter-latency-500.json)。截图和完整进程日志在仓库根目录 `iteration1/runtime/filter-latency/20261009-500-measured/`，未提交日志。

## 准备过程与清理

首次尝试同时保留全部实时 UI 视图并逐批创建报价，批次耗时持续增长，在 200 条时收到停止请求；145 秒 watchdog 结束进程，留下部分夹具待清理。后续按 ID 清单及 `FILTER-LATENCY-` 备注校验，仅清理该测试数据。

随后将夹具准备/清理与 UI 实时推送分开：准备前关闭该测试客户端的视图，测量时由正式筛选控件重新打开视图。每 25 条准备约 0.4 秒，500 条上限不变。一次计时器误把订阅主题 QueryId 当成每次变化的查询 ID，虽收到正确 60 条仍超时；已改为 Loading 边界和完整结果集判断，该次不计入结果。

最终测量成功，500 条夹具的 Refer/Delete 返回成功，清理通过，剩余清单为空。业务删除仍保留审计与历史；未清空数据库或改动其他用户报价。

## 再次执行

每次性能运行必须获得用户明确授权；不得自动增大规模。性能用例在 `tests/ui/benchmarks`，不属于默认 `tests/ui/cases` 功能套件。

在客户端源码目录，用 PowerShell 执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-ui-tests.ps1
$env:STC_UI_USER = 'test2'
$env:STC_UI_PASSWORD = '<本机测试密码>'
$env:STC_UI_SECURITY = '240210.IB'
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/measure-filter-latency.ps1 `
  -StcConfig 'D:\workspace\greatewall\ficc\qa_env\stc-client\STC.exe.config' `
  -ReportDirectory 'D:\workspace\greatewall\ficc\ficc\iteration1\runtime\filter-latency\new-run'
```

测试硬限制 500 报价、每种交互最多 20 个样本；单批准备超过 3 秒即停止加压并清理。独立 runner 对 worker 有 145 秒限时，外层 watchdog 限时 180 秒、工作集 768 MB，只能终止本次创建的测试进程。若进程被终止，清理状态必须按失败处理，检查 `owned-quotes.json`。不得把测量成功状态 PASSED 理解为性能目标达标，要同时看 `referenceTargetMet`。
