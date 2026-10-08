from pathlib import Path
import subprocess, os, time, socket, json, sys, shutil, re

ROOT = Path.cwd()
TARGET = Path((ROOT / "iteration1/runtime/framework-live-current.txt").read_text())
JAVA = Path(os.environ["FICC_FRAMEWORK_JAVA_HOME"]) / "bin/java.exe"
specs = [
    ("LoginSvr", "legacy", 32931, 192),
    ("CRMSvr", "crmsvr", 32983, 512),
    ("QuoteSvr", "quotesvr", 32980, 384),
    ("NotificationSvr", "notificationsvr", 32981, 256),
    ("ParserSvr", "parsersvr", 32943, 768),
    ("QtIntakeSvr", "qtintakesvr", 32982, 192),
    ("GW", "legacy", 32900, 256),
]
owned = []
outputs = []
report = {
    "status": "RUNNING",
    "processes": [],
    "scope": "DPS/GW protocol integration"
    + (
        " and production WPF desktop tests"
        if os.environ.get("FICC_FRAMEWORK_UI_CASES")
        else ""
    ),
}


def listening(port):
    try:
        with socket.create_connection(("127.0.0.1", port), timeout=0.2):
            return True
    except OSError:
        return False


def validate_takeover():
    """Crash only owned primaries; retain the configured 120-second ZooKeeper session timeout."""
    backups = []
    for name, short, primary_port, heap in specs:
        if name not in ("QuoteSvr", "NotificationSvr"):
            continue
        port = primary_port + 100
        if listening(port):
            raise RuntimeError(f"Backup port {port} is already occupied")
        backup_root = TARGET / (name + "-standby")
        shutil.copytree(TARGET / "control", backup_root / "control")
        service = backup_root / "stc" / name
        shutil.copytree(TARGET / "stc" / name / "control", service / "control")
        shutil.copytree(TARGET / "stc" / name / "config", service / "config")
        (backup_root / "log").mkdir()
        (service / "logs").mkdir()
        (service / "log").mkdir()
        ats_file = backup_root / "control/ATSConfig.ini"
        ats = ats_file.read_text(encoding="utf-8")
        for key, value in (("Host", f"127.0.0.1:{port}"), ("Port", str(port))):
            ats = re.sub(
                r"(?m)^SERVER\." + name + r"\." + key + r"=.*$",
                "SERVER." + name + "." + key + "=" + value,
                ats,
            )
        ats_file.write_text(ats, encoding="utf-8")
        for path in service.rglob("*"):
            if path.is_file():
                content = path.read_text(encoding="utf-8-sig")
                content = content.replace(
                    (TARGET / "log").as_posix(), (backup_root / "log").as_posix()
                )
                path.write_text(content, encoding="utf-8")
        module = (
            ROOT
            / "iteration1"
            / ("cgws-ficc-saletrading-" + short)
            / "target/framework-check"
        )
        cp = os.pathsep.join(
            (
                str(service / "config"),
                str(module / "classes"),
                str(module / "javalib/*"),
            )
        )
        output = (TARGET / "log" / (name + "-standby-console.log")).open(
            "w", encoding="utf-8"
        )
        outputs.append(output)
        process = subprocess.Popen(
            [
                str(JAVA),
                f"-Xmx{heap}m",
                "-XX:ActiveProcessorCount=2",
                "-Dfile.encoding=UTF-8",
                "-cp",
                cp,
                name,
            ],
            cwd=service,
            env=dict(os.environ, ATS_ROOT=str(backup_root)),
            stdout=output,
            stderr=subprocess.STDOUT,
            creationflags=subprocess.CREATE_NO_WINDOW,
        )
        owned.append(process)
        report["processes"].append(
            {"service": name + "-standby", "pid": process.pid, "port": port}
        )
        deadline = time.monotonic() + 30
        log = TARGET / "log" / (name + "-standby-console.log")
        while "STANDBY_WAITING" not in log.read_text(
            encoding="utf-8", errors="replace"
        ):
            if process.poll() is not None or time.monotonic() > deadline:
                raise RuntimeError(name + " backup did not enter standby")
            time.sleep(0.2)
        if listening(port):
            raise RuntimeError(
                name + " backup opened its listener before owning the lease"
            )
        # The process is already owned by this test. The local manager must recognize WAITING,
        # without launching another process or stopping this one for lacking a listener.
        (backup_root / "run").mkdir()
        (backup_root / "scripts").mkdir()
        (backup_root / "run" / (name + ".pid")).write_text(
            str(process.pid), encoding="ascii"
        )
        (backup_root / "scripts/serverlist.csv").write_text(
            f"Service,Enabled,Port,Note\n{name},true,{port},isolated standby test\n",
            encoding="ascii",
        )
        shutil.copy2(log, backup_root / "log" / (name + ".console.log"))
        control = subprocess.run(
            [
                "powershell",
                "-NoProfile",
                "-ExecutionPolicy",
                "Bypass",
                "-File",
                str(ROOT / "iteration1/scripts/manage-services.ps1"),
                "-AtsRoot",
                str(backup_root),
                "-Action",
                "start",
                "-Service",
                name,
            ],
            check=True,
            timeout=15,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
        )
        (TARGET / "log" / (name + "-standby-control.log")).write_bytes(control.stdout)
        if b"STANDBY " not in control.stdout or process.poll() is not None:
            raise RuntimeError(
                name + " service manager did not preserve waiting standby"
            )
        backups.append((name, process, port))
        print(name + " backup WAITING; no business listener", flush=True)
    lost_at = time.monotonic()
    report["takeoverListenerSeconds"] = {}
    for name, process, port in backups:
        primary_pid = next(
            item["pid"] for item in report["processes"] if item["service"] == name
        )
        primary = next(item for item in owned if item.pid == primary_pid)
        primary.kill()
        primary.wait(timeout=10)
    print(
        "Owned primaries terminated. Waiting for the existing ZooKeeper lease timeout; no timeout setting changed.",
        flush=True,
    )
    deadline = time.monotonic() + 155
    for name, process, port in backups:
        while not listening(port):
            if process.poll() is not None or time.monotonic() > deadline:
                raise RuntimeError(name + " backup failed to take over")
            time.sleep(0.2)
        print(name + " backup LISTENING after primary loss", flush=True)
        report["takeoverListenerSeconds"][name] = round(time.monotonic() - lost_at, 2)
    # The original GW stays running; clients must discover the replacement endpoints.
    subprocess.run(
        [sys.executable, str(ROOT / "iteration1/scripts/framework/run-probe.py")],
        check=True,
        timeout=75,
    )
    report["standbyTakeover"] = "PASSED"
    if os.environ.get("FICC_FRAMEWORK_UI_CASES"):
        environment = dict(
            os.environ,
            FICC_FRAMEWORK_UI_CASES="01-quote-lifecycle,14-watchlist-notification-read",
        )
        subprocess.run(
            [
                sys.executable,
                str(ROOT / "iteration1/scripts/framework/run-ui.py"),
                "ui-after-takeover",
            ],
            check=True,
            env=environment,
        )
        report["desktopUiAfterTakeover"] = "PASSED"


try:
    for name, short, port, heap in specs:
        if listening(port):
            raise RuntimeError(f"{port} already in use; not changing existing listener")
        module = (
            "cgws-ficc-parsersvr"
            if short == "parsersvr"
            else "cgws-ficc-saletrading-" + short
        )
        if short == "legacy":
            cp = (
                str(TARGET / "stc" / name / "config")
                + os.pathsep
                + str(
                    Path(os.environ["FICC_FRAMEWORK_ATS_ROOT"])
                    / "stc"
                    / name
                    / "javalib/*"
                )
            )
        else:
            cp = (
                str(TARGET / "stc" / name / "config")
                + os.pathsep
                + str(ROOT / "iteration1" / module / "target/framework-check/classes")
                + os.pathsep
                + str(ROOT / "iteration1" / module / "target/framework-check/javalib/*")
            )
        output = (TARGET / "log" / (name + "-console.log")).open("w", encoding="utf-8")
        outputs.append(output)
        process = subprocess.Popen(
            [
                str(JAVA),
                f"-Xmx{heap}m",
                "-XX:ActiveProcessorCount=2",
                "-Dfile.encoding=UTF-8",
                "-cp",
                cp,
                name,
            ],
            cwd=TARGET / "stc" / name,
            env=dict(os.environ, ATS_ROOT=str(TARGET)),
            stdout=output,
            stderr=subprocess.STDOUT,
            creationflags=subprocess.CREATE_NO_WINDOW,
        )
        owned.append(process)
        report["processes"].append(
            {"service": name, "pid": process.pid, "port": port, "heapMb": heap}
        )
        print(f"{name} pid={process.pid}", flush=True)
        limit = time.monotonic() + 60
        while not listening(port):
            if process.poll() is not None:
                raise RuntimeError(f"{name} exited {process.returncode}")
            if time.monotonic() > limit:
                raise RuntimeError(f"{name} did not listen within 60 seconds")
            time.sleep(0.2)
        print(
            f"{name} LISTENING; application readiness still requires requests",
            flush=True,
        )
    (TARGET / "ready").write_text("listening")
    subprocess.run(
        [sys.executable, str(ROOT / "iteration1/scripts/framework/run-probe.py")],
        check=True,
        timeout=75,
    )
    report["status"] = "PASSED"
    if os.environ.get("FICC_FRAMEWORK_UI_CASES"):
        subprocess.run(
            [sys.executable, str(ROOT / "iteration1/scripts/framework/run-ui.py")],
            check=True,
        )
        report["desktopUi"] = "PASSED"
    if os.environ.get("FICC_FRAMEWORK_STANDBY_TEST") == "1":
        validate_takeover()
    report["status"] = "PASSED"
except Exception as error:
    report["status"] = "FAILED"
    report["reason"] = str(error)
    raise
finally:
    for process in reversed(owned):
        if process.poll() is None:
            process.terminate()
            process.wait(timeout=10)
    for output in outputs:
        output.close()
    (TARGET / "report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(
        "Owned test processes stopped; existing services were not changed.", flush=True
    )
