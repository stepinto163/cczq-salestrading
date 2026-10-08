from pathlib import Path
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")
import subprocess, os

root = Path.cwd()
runtime = root / "iteration1/runtime"
target = Path((runtime / "framework-live-current.txt").read_text().strip())
log_path = target / "log/FrameworkLiveProbe.log"
if log_path.exists():
    log_path = target / "log/FrameworkLiveProbe-after-takeover.log"
java = Path(os.environ["FICC_FRAMEWORK_JAVA_HOME"]) / "bin"
cp = str(
    root
    / "iteration1/cgws-ficc-saletrading-qtintakesvr/target/framework-check/javalib/*"
)
subprocess.run(
    [
        str(java / "javac.exe"),
        "-encoding",
        "UTF-8",
        "-cp",
        cp,
        "-d",
        str(runtime),
        str(root / "iteration1/scripts/framework/FrameworkLiveProbe.java"),
    ],
    check=True,
    timeout=30,
)
with log_path.open("w", encoding="utf-8") as log:
    process = subprocess.Popen(
        [
            str(java / "java.exe"),
            "-Xmx128m",
            "-XX:ActiveProcessorCount=2",
            "-Dfile.encoding=UTF-8",
            "-cp",
            str(runtime) + os.pathsep + cp,
            "FrameworkLiveProbe",
        ],
        stdout=log,
        stderr=subprocess.STDOUT,
        creationflags=subprocess.CREATE_NO_WINDOW,
    )
    try:
        code = process.wait(timeout=60)
    except subprocess.TimeoutExpired:
        process.kill()
        process.wait()
        raise
print(log_path.read_text(encoding="utf-8"))
raise SystemExit(code)
