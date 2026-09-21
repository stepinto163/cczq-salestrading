"""Generate Chinese Markdown and correlated metrics without running any load."""
import argparse
import csv
import json
import math
from pathlib import Path


def read_json(path, warnings):
    if not path.exists():
        return {}
    try:
        return json.loads(path.read_text(encoding="utf-8-sig"))
    except (ValueError, OSError) as error:
        warnings.append(f"无法读取 {path.name}: {error}")
        return {}


def percentile(values, quantile):
    values = sorted(value for value in values if isinstance(value, (int, float)))
    return values[max(0, math.ceil(len(values) * quantile) - 1)] if values else None


def number(value):
    return "未采集" if value is None else f"{value:.2f}"


def generate_index(parent):
    lines = ["# 筛选性能测试运行索引", "", "每轮独立生成报告；旧轮次未采集的节点耗时不会补造。", "",
             "| 运行 | 报价数 | 状态 | 场景中最大 P95(ms) | 节点关联 |",
             "|---|---:|---|---:|---|"]
    for directory in sorted(parent.iterdir(), reverse=True):
        if not directory.is_dir() or not (directory / "report.zh-CN.md").exists():
            continue
        summary = read_json(directory / "summary.json", [])
        trace = read_json(directory / "trace-report.json", [])
        values = [scenario["p95Ms"] for scenario in summary.get("scenarios", [])]
        lines.append(f"| [{directory.name}]({directory.name}/report.zh-CN.md) | {summary.get('quoteCount', '未知')} | "
                     f"{summary.get('result', '未知')} | {number(max(values) if values else None)} | "
                     f"{trace.get('completeTraces', 0)}/{trace.get('measuredRequests', 0)} |")
    (parent / "index.zh-CN.md").write_text("\n".join(lines) + "\n", encoding="utf-8")


def traces(directory, warnings):
    result = {}
    for filename in ("gateway.log", "quotesvr.log"):
        path = directory / filename
        if not path.exists():
            continue
        with path.open(encoding="utf-8", errors="replace") as stream:
            for line in stream:
                if "FICC_PERF " not in line:
                    continue
                try:
                    record = json.loads(line.split("FICC_PERF ", 1)[1])
                    key = (record["node"], record["requestId"])
                    if key in result:
                        warnings.append(f"重复服务计时: {key}")
                        result[key] = None
                    else:
                        result[key] = record
                except (ValueError, KeyError):
                    warnings.append(f"{filename} 中有不完整计时记录")
    return result


def correlate(samples, server_traces, warnings):
    requests = []
    for sample_number, sample in enumerate(samples):
        for request in sample.get("requests", []):
            row = dict(request, sample=sample_number, scenario=sample["scenario"], warmup=sample["warmup"])
            request_id = request["requestId"]
            gateway = server_traces.get(("GW", request_id))
            quote = server_traces.get(("QuoteSvr", request_id))
            row["gatewayMs"] = gateway.get("durationMs") if gateway else None
            row["replyBytes"] = gateway.get("replyBytes") if gateway else None
            for output, field in (("quoteSvrMs", "durationMs"), ("searchMs", "searchMs"),
                                  ("searchCalls", "searchCalls"), ("lockWaitMs", "lockWaitMs"),
                                  ("businessMs", "businessMs")):
                row[output] = quote.get(field) if quote else None
            for field, outer, inner in (("clientGatewayResidualMs", "sdkRoundTripMs", "gatewayMs"),
                                         ("gatewayQuoteResidualMs", "gatewayMs", "quoteSvrMs")):
                row[field] = None
                if row.get(outer) is not None and row.get(inner) is not None:
                    difference = row[outer] - row[inner]
                    if difference < 0:
                        warnings.append(f"{request_id}: {field} 为负，边界或关联需检查，未作为有效指标")
                    else:
                        row[field] = difference
            row["traceComplete"] = gateway is not None and quote is not None
            requests.append(row)
    return requests


def generate_report(directory):
    directory = Path(directory)
    warnings = []
    summary = read_json(directory / "summary.json", warnings)
    latency = read_json(directory / "latency.json", warnings)
    samples = latency.get("samples", [])
    measured = [sample for sample in samples if not sample.get("warmup", False)]
    requests = correlate(samples, traces(directory, warnings), warnings)
    measured_requests = [request for request in requests if not request["warmup"]]
    opens = [request for request in measured_requests if request["topic"] == "ficc.quote.view.open"]
    complete = sum(request["traceComplete"] for request in measured_requests)
    if measured and not requests:
        warnings.append("旧版本未采集 requestId 对应的服务耗时，无法补算节点指标；未重新运行测试。")
    if complete < len(measured_requests):
        warnings.append(f"服务计时关联不完整: {complete}/{len(measured_requests)} 个已测量请求")
    status = summary.get("result", latency.get("result", "UNKNOWN"))
    lines = ["# 前端筛选性能测试报告", "", f"运行编号：`{directory.name}`", "",
             f"状态：**{status}**；服务端报价数：{summary.get('quoteCount', latency.get('quoteCount', '未知'))}；"
             f"前端加载上限：{latency.get('loadedRowLimit', '未采集')} 行。", "",
             f"测量样本：{len(measured)}；预热样本：{len(samples) - len(measured)}；"
             f"请求的 GW / QuoteSvr 关联完整数：{complete}/{len(measured_requests)}。", "",
             "## 实际调用链", "", "```text",
             "C# 平台 → GW → QuoteSvr → LocalSearchSvrClient（进程内存筛选）",
             "C# 网格 ← GW ← QuoteSvr",
             "```", "",
             "每次切换条件先关闭旧视图，再打开新视图，两次请求分别计时。SearchSvr 尚未独立部署；"
             "PostgreSQL 用于启动时加载本轮数据，本轮实时筛选不执行数据库查询。不要将启动 SQL 耗时计入筛选链路。", "",
             "## 场景结果", "",
             "单位均为 ms；下表的 SDK、GW、QuoteSvr、搜索列只统计打开视图请求。总延迟还包含关闭旧视图和 UI 更新。", "",
             "| 场景 | 样本 | 总 P50 | 总 P95 | SDK 往返 P95 | GW 含下游 P95 | QuoteSvr P95 | 内存搜索 P95 | 网格绘制 P95 |",
             "|---|---:|---:|---:|---:|---:|---:|---:|---:|"]
    for scenario in dict.fromkeys(sample["scenario"] for sample in measured):
        selected = [sample for sample in measured if sample["scenario"] == scenario]
        selected_opens = [request for request in opens if request["scenario"] == scenario]
        metrics = [percentile([sample["totalMs"] for sample in selected], quantile) for quantile in (.5, .95)]
        metrics += [percentile([request.get(field) for request in selected_opens], .95)
                    for field in ("sdkRoundTripMs", "gatewayMs", "quoteSvrMs", "searchMs")]
        metrics += [percentile([sample.get("gridPaintMs") for sample in selected], .95)]
        lines.append(f"| {scenario} | {len(selected)} | " + " | ".join(map(number, metrics)) + " |")
    lines += ["", "P95 采用最近秩；本工具的每场景 1–10 个测量样本下，P95 等于最大值。各列 P95 可能来自不同请求，**不能相加**。"
              "本报告只反映单客户端、同机、静态数据下的短测试，不证明 50,000 条或生产并发的延迟目标。", "",
              "## 分段耗时", "",
              "以下统计打开视图请求；标准 GW 不注入自定义计时类，GW 耗时及依赖它的分段余量将显示缺失，不能视为零耗时。SDK 往返及 QuoteSvr 搜索统计仍可用。", "",
              "| 段 | 有效样本 | P50 | P95 | 最大值 |",
              "|---|---:|---:|---:|---:|"]
    stages = [
        ("客户端发送准备及日志", "sendPreparationMs"),
        ("客户端 DPS SDK 请求往返（含编解码）", "sdkRoundTripMs"),
        ("客户端 ↔ GW 传输/编解码/排队余量（推导）", "clientGatewayResidualMs"),
        ("GW 转发处理至回复准备完成（含下游）", "gatewayMs"),
        ("GW ↔ QuoteSvr 传输/框架/排队余量（推导）", "gatewayQuoteResidualMs"),
        ("QuoteSvr dispatch 处理", "quoteSvrMs"),
        ("QuoteSvr 业务锁等待", "lockWaitMs"),
        ("QuoteSvr 业务处理（含搜索）", "businessMs"),
        ("QuoteSvr 本请求内所有内存搜索之和", "searchMs"),
        ("客户端回复日志及预览提交", "replyLoggingMs"),
    ]
    for label, field in stages:
        values = [request[field] for request in opens if request.get(field) is not None]
        lines.append(f"| {label} | {len(values)} | {number(percentile(values, .5))} | "
                     f"{number(percentile(values, .95))} | {number(max(values) if values else None)} |")
    lines += ["", "关闭旧视图是独立请求，以下单独汇总，原始 requestId 同样保存在 CSV：", "",
              "| 段 | 有效样本 | P50 | P95 |", "|---|---:|---:|---:|"]
    closes = [request for request in measured_requests if request["topic"] == "ficc.quote.view.close"]
    for label, field in (("关闭：客户端 SDK 往返", "sdkRoundTripMs"), ("关闭：GW 含下游", "gatewayMs"),
                         ("关闭：QuoteSvr", "quoteSvrMs")):
        values = [request[field] for request in closes if request.get(field) is not None]
        lines.append(f"| {label} | {len(values)} | {number(percentile(values, .5))} | {number(percentile(values, .95))} |")
    lines += ["", "UI 完整筛选过程（所有场景合计）：", "",
              "| 段 | 有效样本 | P50 | P95 |", "|---|---:|---:|---:|"]
    for label, field in (("关闭旧视图（含 UI 调度）", "closeMs"),
                         ("打开视图请求（含客户端日志）", "openRequestMs"),
                         ("打开请求线程切换与 UI 调度", "openDispatchMs"),
                         ("缓存和行数更新", "applyMs"), ("绘制及等待 UI 调度", "paintAndDispatchMs"),
                         ("其中：网格 WM_PAINT", "gridPaintMs"), ("完整筛选", "totalMs")):
        values = [sample[field] for sample in measured if sample.get(field) is not None]
        lines.append(f"| {label} | {len(values)} | {number(percentile(values, .5))} | {number(percentile(values, .95))} |")
    lines += ["", "服务耗时使用各进程的单调时钟，并按既有 `requestId` 关联，不相减跨机器的绝对时间戳。"
              "两个余量由**同一请求**的嵌套区间相减得到，包含双向传输、序列化、框架排队、计时日志等开销，"
              "不等于单向网络延迟，也不能除以二当作单向延迟。QuoteSvr 计时不包含进入 dispatch 前的框架队列和返回后的回复序列化。", ""]
    if opens:
        slowest = max(opens, key=lambda request: request["clientRequestMs"])
        lines += ["## 一次真实请求示例", "",
                  f"选择客户端打开视图请求耗时最长的测量样本：`{slowest['requestId']}`（{slowest['scenario']}）。", "",
                  "```mermaid", "sequenceDiagram", "    participant C as C# 平台", "    participant G as GW",
                  "    participant Q as QuoteSvr", "    C->>G: ficc.quote.view.open",
                  "    G->>Q: DPS 转发（原异步路径）",
                  f"    Note over Q: dispatch {number(slowest.get('quoteSvrMs'))} ms，搜索 {number(slowest.get('searchMs'))} ms",
                  "    Q-->>G: 回复",
                  f"    Note over G: GW 区间 {number(slowest.get('gatewayMs'))} ms（包含下游）",
                  "    G-->>C: 回复",
                  f"    Note over C: SDK 往返 {number(slowest.get('sdkRoundTripMs'))} ms",
                  "```", "",
                  f"本次搜索调用次数：{slowest.get('searchCalls', '未采集')}；回复正文：{slowest.get('replyBytes', '未采集')} 字节。", ""]
    lines += ["## 运行状态与证据", "",
              f"- 最低可用物理内存：{summary.get('minimumAvailableMemoryMB', '未采集')} MB。",
              f"- 临时 schema 清理：{summary.get('schemaRemoved', '未采集')}。",
              f"- 100ms 目标：{summary.get('targetMet', latency.get('targetMet', '未完成或未采集'))}。",
              "- [逐次客户端数据](latency.json)、[逐请求关联数据](requests.csv)、[关联报告 JSON](trace-report.json)、[运行汇总](summary.json)。",
              "- [GW 日志](gateway.log)、[QuoteSvr 日志](quotesvr.log)。原始消息仍保存在客户端 JSONL 目录。", ""]
    if summary.get("error") or latency.get("error"):
        lines += ["停止原因：" + str(summary.get("error") or latency["error"]), ""]
    metadata = summary.get("build", {})
    if metadata:
        lines += ["构建信息（文件 SHA-256 用于区分未提交改动）：", "", "```json",
                  json.dumps(metadata, ensure_ascii=False, indent=2), "```", ""]
    if warnings:
        lines += ["## 缺失数据与限制", ""] + ["- " + warning for warning in warnings] + [""]
    if not samples:
        lines += ["本轮没有完成前端采样；不生成延迟结论。", ""]
    output = {"runId": directory.name, "status": status, "warnings": warnings,
              "measuredRequests": len(measured_requests), "completeTraces": complete, "requests": requests}
    (directory / "trace-report.json").write_text(json.dumps(output, ensure_ascii=False, indent=2), encoding="utf-8")
    columns = ["sample", "scenario", "warmup", "requestId", "topic", "code", "clientRequestMs",
               "sendPreparationMs", "sdkRoundTripMs", "replyLoggingMs", "gatewayMs", "quoteSvrMs",
               "clientGatewayResidualMs", "gatewayQuoteResidualMs", "lockWaitMs", "businessMs",
               "searchMs", "searchCalls", "replyBytes", "traceComplete"]
    with (directory / "requests.csv").open("w", encoding="utf-8-sig", newline="") as stream:
        writer = csv.DictWriter(stream, fieldnames=columns, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(requests)
    report = directory / "report.zh-CN.md"
    report.write_text("\n".join(lines), encoding="utf-8")
    generate_index(directory.parent)
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directories", nargs="+", type=Path)
    for run_directory in parser.parse_args().directories:
        print(generate_report(run_directory))
