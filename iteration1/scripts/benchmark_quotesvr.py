"""Explicit, single-process QuoteSvr benchmark; fixed authorized 5000/10/4 workload."""
import argparse
import collections
import ctypes
import datetime
import hashlib
import json
import math
import os
from pathlib import Path
import subprocess
import sys
import time
import uuid
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
RUNTIME = ROOT / "iteration1/runtime"
MODULES = [ROOT / "iteration1" / ("cgws-ficc-saletrading-" + name) for name in ("common", "quotesvr")]
JAVA_HOME = RUNTIME / "tools/jdk8/jdk8u504-b01"
MAVEN = RUNTIME / "tools/apache-maven-3.9.16/bin/mvn.cmd"


def fingerprint():
    files = []
    for module in MODULES:
        files.extend(module.glob("src/**/*.java"))
        files.append(module / "pom.xml")
    return {str(file.relative_to(ROOT)).replace("\\", "/"): hashlib.sha256(file.read_bytes()).hexdigest()
            for file in sorted(files)}


def git(*args):
    result = subprocess.run(["git", *args], cwd=ROOT, capture_output=True, text=True, encoding="utf-8")
    return result.stdout.strip() if result.returncode == 0 else "unknown"


def percentile(values, fraction):
    if not values:
        return None
    return sorted(values)[max(0, math.ceil(len(values) * fraction) - 1)]


def summaries(raw):
    groups = collections.defaultdict(list)
    for sample in raw:
        groups[(sample["scenario"], sample["topic"])].append(sample)
    results = []
    metrics = ("durationMs", "lockWaitMs", "businessMs", "responsePreparationMs", "searchMs", "incrementalMatchMs", "bookBuildMs",
               "publishCallbackMs", "firstViewPublishMs", "lastViewPublishMs")
    for (scenario, topic), samples in groups.items():
        row = {"scenario": scenario, "topic": topic, "samples": len(samples),
               "replyErrors": sum(s.get("replyCode") != "0000" or s.get("batchErrors", 0) > 0 for s in samples)}
        for metric in metrics:
            values = [s[metric] for s in samples if s.get(metric) is not None]
            row[metric] = {"p50": percentile(values, .5), "p95": percentile(values, .95),
                           "p99": percentile(values, .99), "max": max(values) if values else None}
        row["filterTargetP95Within100ms"] = (row["durationMs"]["p95"] < 100
                if topic.endswith((".view.open", ".view.query")) else None)
        results.append(row)
    return results


def write_report(folder, manifest):
    result_file = folder / "result.json"
    result = json.loads(result_file.read_text(encoding="utf-8")) if result_file.exists() else {}
    rows = summaries(result.get("rawSamples", []))
    manifest["metrics"] = rows
    manifest["endToEndAcceptance"] = "NOT_MEASURED"
    manifest["acceptanceTarget"] = {"p95MillisExclusive": 100,
            "scope": "STC client filter action through GW until client rows applied",
            "quotes": 5000, "users": 10, "viewsPerUser": 4}
    manifest["benchmark"] = result
    for scenario in result.get("scenarios", []):
        count = sum(s.get("scenario") == scenario["id"] for s in result.get("rawSamples", []))
        scenario["measuredRequests"] = count
        scenario["throughputRequestsPerSecond"] = count * 1000 / scenario["wallMs"] if scenario.get("wallMs") else None
        if count == 0:
            scenario["status"] = "NOT_MEASURED"
    manifest["heapPoolPeakSumBytes"] = result.get("heapPoolPeakSumBytes", result.get("heapPeakBytes"))
    if result.get("status") == "PASS" and manifest.get("exitCode") == 0:
        manifest["status"] = "PASS"
    elif manifest.get("status") == "RUNNING" and "exitCode" in manifest:
        manifest["status"] = result.get("status", "FAILED")
    if "BENCHMARK_TIME_BUDGET" in result.get("failure", "") and manifest.get("status") != "WATCHDOG_TIMEOUT":
        manifest["status"] = "INCOMPLETE_TIME_BUDGET"
    manifest["schemaCleanup"] = "removed" if result.get("schemaRemoved") else "not confirmed; inspect isolated schema only"
    (folder / "report.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    lines = ["# QuoteSvr 性能基准报告", "", "- 场景执行状态：" + manifest["status"],
             "- 端到端验收：NOT_MEASURED。PASS 仅表示场景执行及正确性检查完成，不表示 100 ms 目标达成。",
             "- 配置：5,000 报价、10 用户、每用户 4 view；分布：" + manifest["distribution"],
             "- 独立 watchdog：15 秒，覆盖 JVM 启动、准备、测量与清理；JVM 堆上限 128 MB。",
             "- 范围：直接调用 handler，包含数据库写入和发布回调编码，不包含 GW、网络或 UI。",
             "- 顺序场景仅 3 次测量，p95/p99 往往等于最大值，只作初步诊断。",
             "- searchMs 包含报价查询过滤和排序；incrementalMatchMs 是单条报价匹配；bookBuildMs 是价格簿重建。",
             "- 阶段可能嵌套，不应相加；锁等待不包含 GW 排队；完整正确性校验不计入请求延迟。场景 CPU／吞吐含测试准备与同步开销。",
             "- heapPoolPeakSumBytes 是各堆内存池历史峰值之和，不是同时占用或进程 RSS，可能大于最大堆。实际采样堆占用见各场景 heapUsedBytes。",
             "- first/lastViewPublishMs 从 handler 调用前至私有 view 回调完成，无推送时为 null。",
             "- 代码：" + manifest.get("commit", "unknown") + "；未提交源文件哈希见 JSON。",
             "- 当前／最后场景：" + result.get("currentScenario", "setup or build"),
             "- 校验断言：" + str(result.get("checks", 0)),
             "- 清理后剩余 view：" + str(result.get("remainingViews", "未完成")),
             "- 隔离 schema：" + manifest["schema"] + "；清理：" + manifest["schemaCleanup"], "",
             "| 场景 | 请求 | 样本 | p50 ms | p95 ms | p99 ms | 最大 ms | 锁等待 p95 ms |",
             "|---|---|---:|---:|---:|---:|---:|---:|"]
    for row in rows:
        d = row["durationMs"]
        lines.append("| {} | {} | {} | {:.2f} | {:.2f} | {:.2f} | {:.2f} | {:.2f} |".format(
            row["scenario"], row["topic"], row["samples"], d["p50"], d["p95"], d["p99"], d["max"], row["lockWaitMs"]["p95"]))
    lines += ["", "## 场景完成状态", ""]
    measured = {item["id"]: item for item in result.get("scenarios", [])}
    for scenario in ("S01-open-broad", "S01-open-narrow", "S01-open-combined", "S02-switch",
                     "S03-matching-update", "S04-enter-leave", "S05-book-move", "S06-concurrent-query",
                     "S07-view-churn-and-updates", "S08-filter-group", "S09-page-and-updates"):
        item = measured.get(scenario, {})
        lines.append("- {}：{}；测量请求 {}。".format(scenario, item.get("status", "NOT_COMPLETED"), item.get("measuredRequests", 0)))
    lines += ["", "CPU、GC、堆占用、原始样本和场景吞吐见 report.json。吞吐不代表最大持续容量。",
              "失败或超时不得视为通过；不自动增加负载或循环重跑。"]
    if result.get("failure"):
        lines += ["", "失败：" + result["failure"]]
    (folder / "report.zh-CN.md").write_text("\n".join(lines) + "\n", encoding="utf-8")


def restrict_cpu(process):
    if os.name != "nt":
        return "not set"
    kernel = ctypes.WinDLL("kernel32", use_last_error=True)
    mask = ctypes.c_size_t()
    system = ctypes.c_size_t()
    kernel.GetProcessAffinityMask.argtypes = [ctypes.c_void_p, ctypes.POINTER(ctypes.c_size_t), ctypes.POINTER(ctypes.c_size_t)]
    kernel.SetProcessAffinityMask.argtypes = [ctypes.c_void_p, ctypes.c_size_t]
    if not kernel.GetProcessAffinityMask(int(process._handle), ctypes.byref(mask), ctypes.byref(system)):
        raise OSError(ctypes.get_last_error(), "Cannot read process CPU affinity")
    bits = [1 << bit for bit in range(ctypes.sizeof(mask) * 8) if mask.value & (1 << bit)]
    chosen = sum(bits[:2])
    if not kernel.SetProcessAffinityMask(int(process._handle), chosen):
        raise OSError(ctypes.get_last_error(), "Cannot restrict benchmark to two logical CPUs")
    return hex(chosen)


def wait_bounded(process, seconds):
    try:
        return process.wait(timeout=seconds), False
    except subprocess.TimeoutExpired:
        process.kill()
        process.wait(timeout=3)
        return 124, True


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--config", type=Path, default=RUNTIME / "quotesvr.local.properties")
    parser.add_argument("--distribution", choices=("spread", "concentrated"), default="spread")
    parser.add_argument("--build-only", action="store_true")
    parser.add_argument("--skip-build", action="store_true")
    args = parser.parse_args()
    parent = RUNTIME / "quote-benchmarks"
    parent.mkdir(parents=True, exist_ok=True)
    # OS lock is released even if the launcher exits; no stale-PID override or parallel runs.
    lock = (parent / "active.lock").open("a+b")
    lock.seek(0)
    lock.write(b"0")
    lock.flush()
    lock.seek(0)
    import msvcrt
    try:
        msvcrt.locking(lock.fileno(), msvcrt.LK_NBLCK, 1)
    except OSError:
        raise SystemExit("Another benchmark launcher is running; no process started")
    folder = parent / (datetime.datetime.now().strftime("%Y%m%d-%H%M%S") + "-" + uuid.uuid4().hex[:6])
    folder.mkdir()
    schema = "ficc_bench_" + uuid.uuid4().hex[:12]
    manifest = {"status": "BUILDING", "schema": schema, "distribution": args.distribution,
                "commit": git("rev-parse", "HEAD"), "branch": git("branch", "--show-current"),
                "dirty": bool(git("status", "--porcelain")), "sourceHashes": fingerprint(),
                "watchdogSeconds": 15, "maxHeapMB": 128, "startedUtc": datetime.datetime.now(datetime.timezone.utc).isoformat()}
    env = os.environ.copy()
    env["JAVA_HOME"] = str(JAVA_HOME)
    env["MAVEN_OPTS"] = "-Xmx512m -Dfile.encoding=UTF-8"
    stamp = parent / "build-fingerprint.json"
    if args.skip_build:
        if not stamp.exists() or json.loads(stamp.read_text()) != manifest["sourceHashes"]:
            raise SystemExit("Sources changed or no verified build. Run without --skip-build")
    else:
        reactor = folder / "reactor.xml"
        modules = "".join("<module>" + os.path.relpath(module, folder).replace("\\", "/") + "</module>" for module in MODULES)
        reactor.write_text('<project xmlns="http://maven.apache.org/POM/4.0.0"><modelVersion>4.0.0</modelVersion><groupId>cgws.local</groupId><artifactId>quote-benchmark-build</artifactId><version>1</version><packaging>pom</packaging><modules>' + modules + '</modules></project>', encoding="utf-8")
        command = [str(MAVEN), "-B", "-ntp", "-o", "-f", str(reactor), "-Dtest=IncrementalViewPageTest", "-Dsurefire.failIfNoSpecifiedTests=false", "test"]
        manifest["buildCommand"] = command
        with (folder / "build.log").open("wb") as log:
            built = subprocess.run(command, env=env, cwd=ROOT, stdout=log, stderr=subprocess.STDOUT, timeout=120)
        if built.returncode:
            manifest.update(status="BUILD_FAILED", exitCode=built.returncode)
            write_report(folder, manifest)
            print(folder / "report.zh-CN.md")
            return built.returncode
        stamp.write_text(json.dumps(manifest["sourceHashes"]), encoding="utf-8")
    if args.build_only:
        manifest["status"] = "BUILD_ONLY"
        write_report(folder, manifest)
        print("Build passed; benchmark was not started:", folder)
        return 0
    config = args.config.resolve()
    lines = config.read_text(encoding="utf-8-sig").splitlines()
    url = next((line.split("=", 1)[1].strip() for line in lines if line.startswith("db.url=")), "")
    if not url.startswith(("jdbc:postgresql://127.0.0.1:", "jdbc:postgresql://localhost:")):
        raise SystemExit("This launcher only permits a local PostgreSQL connection")
    xml = ET.parse(MODULES[0] / "target/surefire-reports/TEST-ficc.common.IncrementalViewPageTest.xml")
    cp = next(item.attrib["value"] for item in xml.findall("properties/property") if item.attrib["name"] == "java.class.path")
    cp = ";".join([str(MODULES[1] / "target/test-classes"), str(MODULES[1] / "target/classes"), cp])
    env["ATS_ROOT"] = str(MODULES[0] / "target/test-classes")
    command = [str(JAVA_HOME / "bin/java.exe"), "-Xms32m", "-Xmx128m", "-XX:ParallelGCThreads=2", "-XX:CICompilerCount=2",
               "-Dfile.encoding=UTF-8", "-Dficc.benchmark.authorized=true", "-Dficc.performance.trace=true", "-cp", cp,
               "ficc.quote.QuoteViewBenchmark", str(config), str(folder / "result.json"), schema, args.distribution]
    manifest.update(status="RUNNING", javaCommand=command)
    write_report(folder, manifest)
    started = time.monotonic()
    with (folder / "console.log").open("wb") as log:
        process = subprocess.Popen(command, env=env, cwd=ROOT, stdout=log, stderr=subprocess.STDOUT)
        try:
            manifest["cpuAffinity"] = restrict_cpu(process)
            manifest["exitCode"], timed_out = wait_bounded(process, max(.1, 15 - (time.monotonic() - started)))
            if timed_out:
                manifest["status"] = "WATCHDOG_TIMEOUT"
        except BaseException:
            process.kill()
            process.wait(timeout=3)
            raise
    manifest["processWallSeconds"] = time.monotonic() - started
    write_report(folder, manifest)
    print("Status:", manifest["status"])
    print("Report:", folder / "report.zh-CN.md")
    print("JSON:", folder / "report.json")
    return manifest.get("exitCode", 1)


if __name__ == "__main__":
    sys.exit(main())
