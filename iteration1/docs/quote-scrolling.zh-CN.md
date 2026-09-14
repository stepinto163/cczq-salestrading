# C# 报价滚动加载与有界测试

报价管理先通过 `ficc.quote.view.open` 加载 200 条。滚动或键盘移动接近已加载列表末尾时，异步调用 `ficc.quote.view.extend`，每次增加 200 条。`CellValueNeeded` 始终只读本地缓存，不在单元格绘制中发送网络请求。

同一时刻最多一个扩展请求；筛选切换与扩展串行。回复和期间推送按序号衔接，重复序号忽略，缺号或超时要求重新筛选。扩展前后按 quoteId 保留选择和首个可见行。顶部“加载更多 / 重试”可手动加载；如果请求超时，需点“筛选”重建视图，因为服务器可能已执行扩展。

底部显示“已加载 / 总匹配数”。达到末尾不再请求。当前服务端扩展上限为 10,000 条，超出后提示缩小筛选。缓存随加载增长，不做窗口淘汰。现有 extend 返回扩展后的完整集合，并非只返回新增 200 条，因此 50,000 条完整浏览仍需后续调整 API。行情价格簿和提醒网格此次没有增加自动滚动加载。

## 功能检查

```powershell
dotnet build iteration1/client/QuotePrototype.csproj --no-restore -m:1 -p:UseSharedCompilation=false -o iteration1/runtime/scroll-client
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-scrolling-ui.ps1
```

仅用 65 条内存假数据和真实 WinForms 网格，10 项检查覆盖滚动事件、并发去重、选择/位置保持、推送重叠、末尾、筛选竞态与失败恢复。不写业务数据库，外部看门狗 15 秒。

## 滚动压力测试

原型“操作结果 / 开发工具”的自动测试页新增“滚动性能测试(500条)”按钮。或运行：

```powershell
dotnet build iteration1/client/QuotePrototype.csproj --no-restore -m:1 -p:UseSharedCompilation=false
python iteration1/scripts/test-filter-performance.py --scrolling --quotes 500 --client iteration1/client/bin/Debug/net472/Ficc.QuotePrototype.exe
```

复用筛选测试的隔离 GW、QuoteSvr、临时 PostgreSQL schema、互斥锁及进程资源保护。每轮生成独立报告，结束后停止测试服务并删除本轮 schema。当前已有的普通原型服务不受影响。

程序以小步长驱动真实网格滚动到底部，使用真实 GW 请求扩展。报告区分滚动到重绘、网格绘制、请求、应用数据、UI 50ms 定时器最大间隔和进程工作集；按 requestId 关联 GW / QuoteSvr 计时与回复大小。这不是操作系统鼠标输入延迟测试，也不是有实时更新的压力测试。

只运行一个指定规模，不自动加压。可选择 500、2,000、10,000 条；先查看较小规模报告再手工提高。UI 心跳中断 2 秒、低于 2 GB 可用物理内存或测试进程组超过 1 GB 时停止；滚动阶段最多 35 秒，单次加载最多等待 1.5 秒。停止亦生成报告并执行清理。

## 首次结果（2026-09-14，500 条）

- 200 → 400 → 500，完成滚动到底部，保持选择，无重复记录。
- 滚动到重绘 p95 22.31 ms，最大 27.52 ms。
- 扩展至 400 条：请求 70.28 ms，应用 2.16 ms；至 500 条：请求 61.72 ms，应用 2.38 ms。
- UI 50ms 定时器最大间隔 112.13 ms，进程峰值工作集 101.41 MB。
- 临时 schema 已清理；未继续运行更高负载。

报告：`iteration1/runtime/filter-performance/20260914_223956_7773e5c8/report.zh-CN.md`。同时保留 `latency.json`、`hops.json` 和节点日志。此次结果不代表 10,000 条或实时更新场景已通过。
