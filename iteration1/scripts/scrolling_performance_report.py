"""A report for each bounded scrolling run, including interrupted runs."""
import json
from filter_performance_report import correlate, traces


def generate_report(directory):
    summary = json.loads((directory / "summary.json").read_text(encoding="utf-8-sig"))
    data_file = directory / "latency.json"
    report = json.loads(data_file.read_text(encoding="utf-8-sig")) if data_file.exists() else {}
    lines = ["# C# 平台滚动测试报告", "",
             f"- 结果：{summary['result']}",
             f"- 服务端报价：{summary['quoteCount']}；客户端已加载：{report.get('loadedRows', '未完成')}",
             f"- 滚动至重绘 p95 / 最大：{report.get('scrollP95Ms', '—')} / {report.get('scrollMaxMs', '—')} ms",
             f"- UI 50ms 定时器最大间隔：{report.get('maximumUiTimerGapMs', '—')} ms",
             f"- 进程峰值工作集：{report.get('peakWorkingSetMB', '—')} MB",
             f"- 测试 schema 已清理：{summary.get('schemaRemoved', False)}", "",
             "程序驱动网格纵向滚动，使用真实 GW / QuoteSvr 和隔离数据库。不是鼠标设备输入延迟或直播更新压力测试。",
             "单个客户端、静态数据；每次查询一页，替换当前页缓存；不自动增加下一轮的数据规模。", "",
             "## 分页请求", "", "| 本页行数 | 请求耗时 ms | 应用数据 ms | 合计 ms |",
             "|---:|---:|---:|---:|"]
    for sample in report.get("extensions", []):
        lines.append(f"| {sample['loadedRows']} | {sample['requestMs']:.2f} | {sample['applyMs']:.2f} | {sample['totalMs']:.2f} |")
    warnings = []
    hops = correlate([{"scenario": "scrolling", "warmup": False, "requests": report.get("requests", [])}], traces(directory, warnings), warnings)
    (directory / "hops.json").write_text(json.dumps(hops, ensure_ascii=False, indent=2), encoding="utf-8")
    lines += ["", "## 同一 requestId 的节点计时", "",
              "| 接口 | 客户端请求 ms | SDK 往返 ms | GW ms | QuoteSvr ms | 回复字节 |",
              "|---|---:|---:|---:|---:|---:|"]
    def value(number):
        return "—" if number is None else f"{number:.2f}"
    for request in hops:
        if not request["topic"].startswith("ficc.quote.view."):
            continue
        lines.append(f"| {request['topic']} | {value(request.get('clientRequestMs'))} | {value(request.get('sdkRoundTripMs'))} | {value(request.get('gatewayMs'))} | {value(request.get('quoteSvrMs'))} | {request.get('replyBytes', '—')} |")
    lines += ["", "原始 samples、requests 和计时在 latency.json；requests 包含 DPS 往返与消息记录耗时。",
              "本报告不将往返耗时推断为单向网络耗时。服务端/GW 原始追踪日志保留于本轮目录。"]
    if report.get("error") or summary.get("error"):
        lines += ["", "停止原因：" + (report.get("error") or summary["error"])]
    if warnings:
        lines += ["", "计时关联说明：" + "；".join(warnings)]
    (directory / "report.zh-CN.md").write_text("\n".join(lines) + "\n", encoding="utf-8")
