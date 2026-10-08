"""Run existing production WPF cases against the isolated GW, with screenshots and a watchdog."""

from pathlib import Path
import json
import os
import subprocess
import sys
from xml.dom import minidom

root = Path.cwd()
target = Path(
    (root / "iteration1/runtime/framework-live-current.txt").read_text().strip()
)
client = root / "iteration1/stc-client/stc-client"
runtime = root / "iteration1/runtime/framework-ui-client"
if not (runtime / "SalesTradingUiTests.exe").is_file():
    raise RuntimeError(
        "Build the UI runner with build-ui-tests.ps1 -OutputRoot iteration1/runtime/framework-ui-client"
    )

# Use the established local STC configuration, never the QA endpoint or the user's running config.
config = minidom.parse(str(client / "runtime/ui-test-client/STC.exe.config"))
for server in config.getElementsByTagName("Server"):
    server.setAttribute("Host", "127.0.0.1")
    server.setAttribute("DpsHost", "127.0.0.1")
    server.setAttribute("Port", "32900")
    server.setAttribute("DpsPort", "32900")
for setting in config.getElementsByTagName("appSettings")[0].getElementsByTagName(
    "add"
):
    if setting.getAttribute("key") == "IsSingle":
        setting.setAttribute("value", "false")
    if setting.getAttribute("key") == "Ficc.IntegrationMode":
        setting.setAttribute("value", "stc")
config_path = target / "STC-ui.config"
config_path.write_bytes(config.toxml(encoding="utf-8"))
report = target / (sys.argv[1] if len(sys.argv) > 1 else "ui-reports")
environment = dict(
    os.environ,
    STC_UI_USER=os.environ["FICC_FRAMEWORK_TEST_USER"],
    STC_UI_PASSWORD=os.environ["FICC_FRAMEWORK_TEST_PASSWORD"],
)
with (target / "log" / (report.name + ".log")).open("w", encoding="utf-8") as output:
    process = subprocess.Popen(
        [
            str(runtime / "SalesTradingUiTests.exe"),
            "--cases",
            str(client / "tests/ui/cases"),
            "--run",
            os.environ["FICC_FRAMEWORK_UI_CASES"],
            "--config",
            str(config_path),
            "--report",
            str(report),
        ],
        cwd=runtime,
        env=environment,
        stdout=output,
        stderr=subprocess.STDOUT,
        creationflags=subprocess.CREATE_NO_WINDOW,
    )
    try:
        # Each case already has its own process watchdog and cleanup. This also bounds the suite.
        result = process.wait(timeout=1500)
    except subprocess.TimeoutExpired:
        subprocess.run(
            ["taskkill", "/PID", str(process.pid), "/T", "/F"],
            check=False,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
        process.wait()
        raise
summary = report / "summary.json"
if summary.is_file():
    for case in json.loads(summary.read_text(encoding="utf-8-sig")):
        print(
            case["caseId"],
            case["status"],
            "cleanup=" + str(case["cleanupPassed"]),
            flush=True,
        )
else:
    raise RuntimeError("UI runner did not produce summary.json; inspect " + str(report))
raise SystemExit(result)
