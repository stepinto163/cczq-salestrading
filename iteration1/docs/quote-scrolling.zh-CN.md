# 分页查询、滚动浏览与功能检查

更新日期：2026-09-17。当前实现已用 `view.query` 替代 `view.extend`。报价、行情和提醒分别使用 `ficc.quote.view.query`、`ficc.price.view.query`、`ficc.notification.view.query`，不混用数据模型或推送通道。

先用 `view.open` 建立筛选视图、取得第一批，再用原 `viewId`、`pageNumber`、`pageSize` 查询指定页。例如第1页200条，下一页仍查询200条并替换当前缓存，而不是返回前400条。页大小最多1000条；旧文中的10000条扩展限制不再适用，测试规模限制也不等于业务查询总数限制。

## C# 客户端

报价管理默认每页200条，接近当前批次末尾时异步查询下一页；同时提供“上一批／下一批／返回最新”。行情和提醒同样使用独立的分页查询。`CellValueNeeded` 始终只读本地缓存，不在绘制过程中发送网络请求；保留网格虚拟化。

筛选与分页请求串行，加载期间缓存推送，并按回复序号衔接；重复消息忽略，断序或失败提示重新同步。第一页实时插入、排序和裁剪，移除记录后按需重新查询补齐；后续页不插入新记录或重排已有记录，移除记录仍可离开列表。翻页替换当前批次，不累计缓存全部匹配记录。

## Web 客户端

滚动到批次边界查询下一批或上一批，不显示上一批／下一批按钮。用户滚动后进入浏览模式，保持行的位置，不自动插入或重排；变更按不同记录计数，按钮显示“有 XX 条变更”，无变更时显示“浏览中 · 返回实时”。点击后保留筛选，重新查询第一批并恢复实时更新。

浏览时仍只缓存当前批次，不是将所有批次无限追加到页面。详情见 [Web 浏览交互](../../docs/quote-board-browsing-ux.zh-CN.md)；协议见 [API 规范](../../docs/ficc-api-spec.zh-CN.md)。实时数据使用偏移分页时，跨批次浏览仍可能重复或遗漏记录，不适合作为完整导出。

## 功能检查

```powershell
dotnet build iteration1/client/SalesTradingClient.csproj --no-restore -m:1 -p:UseSharedCompilation=false
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-scrolling-ui.ps1
```

仅用65条内存假数据和真实 WinForms 网格，检查分页替换、滚动触发、并发去重、末页、返回第一页，以及三个领域的推送处理。不写业务数据库，外部看门狗15秒。功能检查通过不代表大数据量性能已达标。

## 滚动压力测试

平台“操作结果 / 开发工具”的自动测试页新增“滚动性能测试(500条)”按钮。或运行：

```powershell
dotnet build iteration1/client/SalesTradingClient.csproj --no-restore -m:1 -p:UseSharedCompilation=false
python iteration1/scripts/test-filter-performance.py --scrolling --quotes 500 --client iteration1/client/bin/Debug/net472/Ficc.SalesTradingClient.exe
```

复用筛选测试的隔离 GW、QuoteSvr、临时 PostgreSQL schema、互斥锁及进程资源保护。每轮生成独立报告，结束后停止测试服务并删除本轮 schema。当前已有的普通平台服务不受影响。

程序以小步长驱动真实网格滚动，使用真实 GW 分页查询。报告区分滚动到重绘、网格绘制、请求、应用数据、UI 50ms 定时器最大间隔和进程工作集；按 requestId 关联 GW / QuoteSvr 计时与回复大小。这不是操作系统鼠标输入延迟测试，也不是有实时更新的压力测试。压力测试须经用户明确授权，从最小规模开始，本次文档更新不运行测试。

只运行一个指定规模，不自动加压。可选择 500、2,000、10,000 条；先查看较小规模报告再手工提高。UI 心跳中断 2 秒、低于 2 GB 可用物理内存或测试进程组超过 1 GB 时停止；滚动阶段最多 35 秒，单次加载最多等待 1.5 秒。停止亦生成报告并执行清理。

## 历史结果：旧扩展实现（2026-09-14，500 条）

以下仅保留历史测试证据，使用的是已移除的扩展接口，不能作为当前分页实现的性能结论。当前分页及Web浏览模式尚无对应的大规模性能验收结果。

- 200 → 400 → 500，完成滚动到底部，保持选择，无重复记录。
- 滚动到重绘 p95 22.31 ms，最大 27.52 ms。
- 扩展至 400 条：请求 70.28 ms，应用 2.16 ms；至 500 条：请求 61.72 ms，应用 2.38 ms。
- UI 50ms 定时器最大间隔 112.13 ms，进程峰值工作集 101.41 MB。
- 临时 schema 已清理；未继续运行更高负载。

报告：`iteration1/runtime/filter-performance/20260914_223956_7773e5c8/report.zh-CN.md`。同时保留 `latency.json`、`hops.json` 和节点日志。此次结果不代表 10,000 条或实时更新场景已通过。
