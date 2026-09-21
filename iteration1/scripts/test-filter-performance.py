"""One bounded frontend filter benchmark. Dataset size never increases automatically."""
import argparse
import ctypes
from ctypes import wintypes
from datetime import datetime
import json
import hashlib
import msvcrt
import os
from pathlib import Path
import re
import shutil
import socket
import subprocess
import time
from urllib.parse import urlparse
import uuid
from filter_performance_report import generate_report

ROOT = Path(__file__).resolve().parents[1]
MB = 1024 * 1024


class MemoryStatus(ctypes.Structure):
    _fields_ = [("length", wintypes.DWORD), ("load", wintypes.DWORD)] + [
        (name, ctypes.c_ulonglong) for name in
        ("totalPhysical", "availablePhysical", "totalPageFile", "availablePageFile", "totalVirtual",
         "availableVirtual", "availableExtendedVirtual")]


class BasicJobLimits(ctypes.Structure):
    _fields_ = [
        ("processTime", ctypes.c_longlong), ("jobTime", ctypes.c_longlong), ("flags", wintypes.DWORD),
        ("minimumWorkingSet", ctypes.c_size_t), ("maximumWorkingSet", ctypes.c_size_t),
        ("activeProcesses", wintypes.DWORD), ("affinity", ctypes.c_size_t),
        ("priority", wintypes.DWORD), ("scheduling", wintypes.DWORD)]


class JobLimits(ctypes.Structure):
    _fields_ = [("basic", BasicJobLimits), ("ioCounters", ctypes.c_ulonglong * 6),
                ("processMemory", ctypes.c_size_t), ("jobMemory", ctypes.c_size_t),
                ("peakProcessMemory", ctypes.c_size_t), ("peakJobMemory", ctypes.c_size_t)]


class Watchdog:
    def __init__(self, directory):
        self.directory = directory
        self.processes = []
        self.logs = []
        self.minimum_available_mb = None
        kernel = ctypes.windll.kernel32
        kernel.CreateJobObjectW.restype = wintypes.HANDLE
        kernel.SetInformationJobObject.argtypes = [wintypes.HANDLE, ctypes.c_int, ctypes.c_void_p, wintypes.DWORD]
        kernel.AssignProcessToJobObject.argtypes = [wintypes.HANDLE, wintypes.HANDLE]
        kernel.CloseHandle.argtypes = [wintypes.HANDLE]
        self.job = kernel.CreateJobObjectW(None, None)
        limits = JobLimits()
        limits.basic.flags = 0x2000 | 0x200  # Kill children if supervisor exits; 1 GB job commit limit.
        limits.jobMemory = 1024 * MB
        if not self.job or not kernel.SetInformationJobObject(self.job, 9, ctypes.byref(limits), ctypes.sizeof(limits)):
            raise RuntimeError("Cannot establish Windows process limits")

    def health(self):
        status = MemoryStatus()
        status.length = ctypes.sizeof(status)
        if not ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(status)):
            raise RuntimeError("Cannot read available memory")
        available = status.availablePhysical // MB
        self.minimum_available_mb = available if self.minimum_available_mb is None else min(
            available, self.minimum_available_mb)
        if available < 2048:
            raise RuntimeError("Stopped: less than 2 GB physical memory available")

    def start(self, name, command, cwd, environment=None, visible=False):
        self.health()
        output = (self.directory / (name + ".log")).open("wb")
        error = (self.directory / (name + ".err")).open("wb")
        self.logs.extend([output, error])
        process = subprocess.Popen(command, cwd=cwd, env=environment, stdout=output, stderr=error,
                                   creationflags=0 if visible else subprocess.CREATE_NO_WINDOW)
        if not ctypes.windll.kernel32.AssignProcessToJobObject(self.job, wintypes.HANDLE(int(process._handle))):
            process.kill()
            process.wait(timeout=3)
            raise RuntimeError("Cannot place test process in watchdog job")
        self.processes.append(process)
        return process

    def wait(self, process, seconds, heartbeat=None):
        start = time.monotonic()
        while process.poll() is None:
            self.health()
            if time.monotonic() - start > seconds:
                raise TimeoutError("Process exceeded " + str(seconds) + " seconds")
            if heartbeat and heartbeat.exists() and time.time() - heartbeat.stat().st_mtime > 2:
                raise TimeoutError("UI heartbeat stalled for more than 2 seconds")
            time.sleep(0.1)
        if process.returncode != 0:
            raise RuntimeError("Test process failed; inspect the run log")

    def stop_children(self):
        for process in reversed(self.processes):
            if process.poll() is None:
                process.terminate()
        for process in self.processes:
            try:
                process.wait(timeout=3)
            except subprocess.TimeoutExpired:
                process.kill()
        self.processes.clear()

    def close(self):
        self.stop_children()
        for stream in self.logs:
            stream.close()
        ctypes.windll.kernel32.CloseHandle(self.job)


def properties(path):
    return dict(line.split("=", 1) for line in path.read_text(encoding="utf-8-sig").splitlines()
                if "=" in line and not line.lstrip().startswith("#"))


def write_properties(path, values):
    path.write_text("".join(key + "=" + value + "\n" for key, value in values.items()), encoding="utf-8")


def build_metadata(paths):
    result = {"files": {str(path.relative_to(ROOT)) if path.is_relative_to(ROOT) else path.name:
                        hashlib.sha256(path.read_bytes()).hexdigest() for path in paths}}
    try:
        result["gitCommit"] = subprocess.check_output(
            ["git", "rev-parse", "HEAD"], cwd=ROOT, text=True, timeout=3).strip()
        result["workingTreeModified"] = bool(subprocess.check_output(
            ["git", "status", "--porcelain"], cwd=ROOT, text=True, timeout=3).strip())
    except (subprocess.SubprocessError, OSError):
        result["gitCommit"] = "unavailable"
    return result


def cases(count):
    definitions = [
        ("全部有效报价", [], lambda index: True),
        ("AAA 债项评级", [("bondRating", "EQ", "AAA")], lambda index: index % 4 != 2),
        ("AA+ 债项评级", [("bondRating", "EQ", "AA+")], lambda index: index % 4 == 2),
        ("企业债 + 买方 + 收益率区间",
         [("issuerType", "EQ", "CORPORATE"), ("side", "EQ", "1"),
          ("price", "BETWEEN", ["2.0100", "2.0300"])],
         lambda index: index % 4 >= 2 and (index // 4) % 2 == 0 and 100 <= index % 500 <= 300),
        ("无匹配结果", [("bondRating", "EQ", "AA")], lambda index: False),
    ]
    result = []
    for name, conditions, matches in definitions:
        indices = [index for index in range(count - 1, -1, -1) if matches(index)]
        result.append({
            "name": name,
            "filter": {"conditions": [{"field": field, "operator": operator, "value": value}
                                      for field, operator, value in conditions]},
            "expectedCount": len(indices),
            "expectedIds": ["PERF-" + str(index).zfill(6) for index in indices[:200]],
        })
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--quotes", type=int, choices=[500, 2000, 10000, 50000], default=500)
    parser.add_argument("--repeats", type=int, choices=range(1, 11), default=6)
    parser.add_argument("--scrolling", action="store_true", help="Exercise incremental loading and grid scrolling, at most 10,000 quotes")
    parser.add_argument("--client", type=Path, default=ROOT / "client/bin/FilterPerformance/Ficc.SalesTradingClient.exe")
    parser.add_argument("--functional-smoke", action="store_true",
                        help="Reuse isolation for the three-quote C# functional test; no performance fixture")
    arguments = parser.parse_args()
    if arguments.scrolling and (arguments.quotes > 10000 or arguments.functional_smoke):
        raise SystemExit("Scrolling supports at most 10,000 quotes and cannot be combined with functional-smoke")
    if os.name != "nt":
        raise SystemExit("This WinForms benchmark requires Windows")
    parent = ROOT / ("runtime/adapter-smoke" if arguments.functional_smoke else "runtime/filter-performance")
    parent.mkdir(parents=True, exist_ok=True)
    lock = (parent / "runner.lock").open("a+b")
    lock.seek(0)
    if not lock.read(1):
        lock.write(b"1")
        lock.flush()
    lock.seek(0)
    try:
        msvcrt.locking(lock.fileno(), msvcrt.LK_NBLCK, 1)
    except OSError:
        raise SystemExit("Another filter performance test is already running")
    run_id = datetime.now().strftime("%Y%m%d_%H%M%S") + "_" + uuid.uuid4().hex[:8]
    directory = parent / run_id
    directory.mkdir()
    schema = "ficc_perf_" + run_id
    assert re.fullmatch(r"ficc_perf_[0-9_]+[a-f0-9]{8}", schema)
    watchdog = Watchdog(directory)
    schema_created = False
    summary = {"result": "RUNNING", "quoteCount": arguments.quotes, "schema": schema,
               "runDirectory": str(directory), "watchdogPid": os.getpid()}
    if arguments.scrolling:
        summary["test"] = "scrolling"
    if arguments.functional_smoke:
        summary["quoteCount"] = 0
        summary["test"] = "three-quote-functional-smoke"
    base = properties(ROOT / "runtime/quotesvr.local.properties")
    database = urlparse(base["db.url"].removeprefix("jdbc:"))
    if database.hostname not in ("127.0.0.1", "localhost"):
        raise RuntimeError("Benchmark requires the project localhost database")
    psql = ROOT / "runtime/tools/postgresql/pgsql/bin/psql.exe"
    pg_environment = dict(os.environ, PGPASSWORD=base["db.password"], PGAPPNAME="FiccFilterPerformance")
    pg_command = [str(psql), "-X", "-q", "-h", database.hostname, "-p", str(database.port or 5432),
                  "-U", base["db.user"], "-d", database.path.lstrip("/"), "-v", "ON_ERROR_STOP=1"]

    def sql(name, text):
        path = directory / (name + ".sql")
        path.write_text(text, encoding="utf-8")
        process = watchdog.start(name, pg_command + ["-f", str(path)], ROOT, pg_environment)
        watchdog.wait(process, 15)

    try:
        for port in (18700, 18701, 18780):
            with socket.socket() as probe:
                probe.bind(("127.0.0.1", port))
        if not arguments.client.resolve().is_file():
            raise RuntimeError("Build the FilterPerformance client first")
        sql("create", "CREATE SCHEMA " + schema + ";")
        schema_created = True
        initialization = (ROOT / "sql/001-create-tables.sql").read_text(encoding="utf-8")
        initialization = initialization.replace("ficc_salestrading", schema)
        # Keep the old-installation guard inside the isolated fixture namespace too.
        initialization = initialization.replace("ficc_demo", schema + "_previous")
        sql("tables", initialization)
        if not arguments.functional_smoke:
            fixture = (ROOT / "scripts/filter-performance-fixture.sql").read_text(encoding="utf-8")
            sql("fixture", fixture.replace("PERFORMANCE_SCHEMA", schema).replace("QUOTE_COUNT", str(arguments.quotes)))
        quote_directory = directory / "quotesvr"
        gateway_directory = directory / "gateway"
        shutil.copytree(ROOT / "cczq-app-salestrading-quotesvr/control", quote_directory / "control")
        shutil.copytree(ROOT / "GW/control", gateway_directory / "control")
        shutil.copytree(ROOT / "GW/config", gateway_directory / "config")
        for service_directory in (quote_directory, gateway_directory):
            config = properties(service_directory / "control/ATSConfig.ini")
            config = {key: value for key, value in config.items() if not key.startswith("REGISTER.")}
            for key in list(config):
                if key.endswith(".RegisterEnable"):
                    config[key] = "0"
                if key.endswith(".RegisterServerList"):
                    config[key] = ""
                if key.startswith("SERVER.NotificationSvr"):
                    del config[key]
            config["SERVER.QuoteSvr.Host"] = "127.0.0.1:18780"
            if service_directory == gateway_directory:
                config["SERVER.LoginSvr.Host"] = "127.0.0.1:18780"
            write_properties(service_directory / "control/ATSConfig.ini", config)
        base["db.schema"] = schema
        database_file = directory / "database.local.properties"
        write_properties(database_file, base)
        resource = quote_directory / "control/QuoteSvr/resourceCfg.ini"
        values = properties(resource)
        values["quote.properties"] = database_file.as_posix()
        write_properties(resource, values)
        for spring in (gateway_directory / "config").glob("*.xml"):
            content = spring.read_text(encoding="utf-8").replace('value="18600"', 'value="18700"')
            content = content.replace('value="18601"', 'value="18701"')
            content = content.replace("LoginSvr,QuoteSvr,NotificationSvr", "LoginSvr,QuoteSvr")
            content = content.replace('<entry key="NotificationSvr" value="ficc.notification.report.**" />', "")
            spring.write_text(content, encoding="utf-8")
        foundation = subprocess.check_output([
            "powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
            str(ROOT / "scripts/foundation-classpath.ps1")], text=True, timeout=10).strip()
        quote_jar = ROOT / "cczq-app-salestrading-quotesvr/target-maven/cczq-app-salestrading-quotesvr-0.1.0-SNAPSHOT.jar"
        common_jar = ROOT / "cczq-app-salestrading-common/target-maven/cczq-app-salestrading-common-0.1.0-SNAPSHOT.jar"
        gateway_jar = ROOT / "GW/javalib/gw-0.1.1-SNAPSHOT.jar"
        if not all(path.is_file() for path in (quote_jar, common_jar, gateway_jar)):
            raise RuntimeError("Run the Maven build first")
        summary["build"] = build_metadata([quote_jar, common_jar, gateway_jar, arguments.client.resolve()])
        java = shutil.which("java")
        if not java:
            raise RuntimeError("Java 17 is required")
        quote_process = watchdog.start("quotesvr", [
            java, "-Xms32m", "-Xmx256m", "-XX:ActiveProcessorCount=2", "-Dfile.encoding=UTF-8",
            "-Dficc.performance.trace=" + str(not arguments.functional_smoke).lower(),
            "-cp", str(quote_jar) + ";" + str(common_jar) + ";" + foundation, "ficc.quote.Main"],
            quote_directory, dict(os.environ, ATS_ROOT=str(quote_directory)))
        gateway_process = watchdog.start("gateway", [
            java, "-Xms32m", "-Xmx160m", "-XX:ActiveProcessorCount=2", "-Dfile.encoding=UTF-8",
            "-Dficc.performance.trace=" + str(not arguments.functional_smoke).lower(),
            "-cp", str(ROOT / "GW/javalib/*"), "GW"], gateway_directory, dict(os.environ, ATS_ROOT=str(gateway_directory)))
        start = time.monotonic()
        while True:
            watchdog.health()
            if quote_process.poll() is not None or gateway_process.poll() is not None:
                raise RuntimeError("An isolated service exited during startup")
            quote_log = (directory / "quotesvr.log").read_text(encoding="utf-8", errors="replace")
            gateway_log = (directory / "gateway.log").read_text(encoding="utf-8", errors="replace")
            if ("QuoteSvr READY" in quote_log
                    and "QuoteSvr onConnected" in gateway_log and "LoginSvr onConnected" in gateway_log):
                break
            if time.monotonic() - start > 25:
                raise TimeoutError("Isolated service startup exceeded 25 seconds")
            time.sleep(0.2)
        configuration = {
            "quoteCount": arguments.quotes, "repeats": arguments.repeats, "gatewayPort": 18700,
            "resultFile": str(directory / "latency.json"), "heartbeatFile": str(directory / "ui.heartbeat"),
            "scenarios": cases(arguments.quotes),
        }
        config_file = directory / "benchmark.json"
        config_file.write_text(json.dumps(configuration, ensure_ascii=False, indent=2), encoding="utf-8")
        print("Running:", "three-quote functional smoke" if arguments.functional_smoke else str(arguments.quotes) + " quotes", directory, flush=True)
        client_arguments = (["--smoke", str(directory / "functional.json"), "18700"] if arguments.functional_smoke
                            else ["--scroll-performance" if arguments.scrolling else "--filter-performance", str(config_file)])
        ui = watchdog.start("client", [str(arguments.client.resolve())] + client_arguments,
                            arguments.client.resolve().parent, visible=not arguments.functional_smoke)
        watchdog.wait(ui, 50, None if arguments.functional_smoke else directory / "ui.heartbeat")
        if arguments.functional_smoke:
            report = json.loads((directory / "functional.json").read_text(encoding="utf-8-sig"))
            summary["result"] = "COMPLETED" if report["result"] == "PASS" else "STOPPED"
            summary["checks"] = report.get("checks", 0)
            summary["quoteCount"] = report.get("quotesCreated", 0)
        else:
            report = json.loads((directory / "latency.json").read_text(encoding="utf-8-sig"))
            summary["result"] = report["result"]
            if arguments.scrolling:
                summary["scrollP95Ms"] = report.get("scrollP95Ms")
                summary["loadedRows"] = report.get("loadedRows")
            else:
                summary["targetMet"] = report.get("targetMet", False)
            summary["scenarios"] = report["scenarios"]
    except Exception as error:
        summary["result"] = "STOPPED"
        summary["error"] = str(error)
    finally:
        watchdog.stop_children()
        if schema_created:
            try:
                sql("cleanup", "SET lock_timeout='3s'; DROP SCHEMA " + schema + " CASCADE;")
                summary["schemaRemoved"] = True
            except Exception:
                summary["schemaRemoved"] = False
        summary["minimumAvailableMemoryMB"] = watchdog.minimum_available_mb
        watchdog.close()
        summary["reportFile"] = str(directory / ("functional.json" if arguments.functional_smoke else "report.zh-CN.md"))
        (directory / "summary.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
        lock.close()
        if arguments.scrolling:
            from scrolling_performance_report import generate_report as generate_scrolling_report
            generate_scrolling_report(directory)
        elif not arguments.functional_smoke:
            generate_report(directory)
        print(json.dumps(summary, ensure_ascii=False, indent=2))
    if summary["result"] != "COMPLETED":
        raise SystemExit(1)


if __name__ == "__main__":
    main()
