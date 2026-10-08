"""Bounded component regression. No external FIX session, service restart, or UI actions."""
import argparse
import os
from pathlib import Path
import subprocess
import time


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--skip-build", action="store_true")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    output = root / "runtime" / "negotiation-tests" / time.strftime("%Y%m%d-%H%M%S")
    output.mkdir(parents=True)
    java_home = root / "runtime/tools/jdk8/jdk8u504-b01"
    java = java_home / "bin/java.exe"
    maven = root / "runtime/tools/apache-maven-3.9.16/bin/mvn.cmd"
    environment = os.environ.copy()
    environment["JAVA_HOME"] = str(java_home)
    environment["MAVEN_OPTS"] = "-Xmx256m -XX:ActiveProcessorCount=2"
    environment["ATS_ROOT"] = str(root / "cgws-ficc-saletrading-quotesvr")
    config = root / "runtime/quotesvr.local.properties"
    if "db.url=jdbc:postgresql://127.0.0.1:55432/ficc_quote" not in config.read_text():
        raise RuntimeError("Only the local test PostgreSQL database is allowed")

    def run(name, command, timeout):
        log = output / (name + ".log")
        print("START", name, flush=True)
        with log.open("w", encoding="utf-8") as stream:
            process = subprocess.Popen([str(arg) for arg in command], cwd=root.parent,
                                       env=environment, stdout=stream, stderr=subprocess.STDOUT)
            try:
                code = process.wait(timeout=timeout)
            except subprocess.TimeoutExpired:
                subprocess.run(["taskkill", "/PID", str(process.pid), "/T", "/F"],
                               capture_output=True, timeout=10)
                raise RuntimeError(name + " exceeded watchdog; see " + str(log))
        if code:
            print(log.read_text(encoding="utf-8", errors="replace")[-6000:])
            raise RuntimeError(name + " failed; see " + str(log))
        print("PASS", name, flush=True)

    def target(module):
        return root / ("cgws-ficc-saletrading-" + module) / "target/negotiation-check"

    modules = ("common", "qtintakesvr", "quotesvr", "notificationsvr")
    if not args.skip_build:
        for module in modules:
            run("build-" + module, [maven, "-B", "-ntp", "-f",
                root / ("cgws-ficc-saletrading-" + module) / "pom.xml",
                "-Dmaven.build.directory=target/negotiation-check", "install"], 150)

    def smoke(name, module, class_name, *arguments):
        folder = target(module)
        classpath = os.pathsep.join(str(folder / part) for part in ("test-classes", "classes", "javalib/*"))
        run(name, [java, "-Xms32m", "-Xmx192m", "-XX:ActiveProcessorCount=2",
            "-Dfile.encoding=UTF-8", "-cp", classpath, class_name, *arguments], 45)

    requests = output / "mapped-fix.json"
    snapshots = output / "quote-snapshots.json"
    smoke("map-fix", "qtintakesvr", "ficc.qtintake.QTradeNegotiationFixtures", requests)
    smoke("quote-negotiation", "quotesvr", "ficc.quote.QTradeNegotiationSmoke", config, requests, snapshots,
          output / "quote-protocol.json")
    smoke("notification-negotiation", "notificationsvr", "ficc.notification.QTradeNotificationSmoke", config, snapshots)
    smoke("existing-intake", "quotesvr", "ficc.quote.QtIntakeSmoke", config)
    smoke("existing-incremental-views", "quotesvr", "ficc.quote.IncrementalViewSmoke", config,
          output / "incremental-view-result.json")
    print("Reports:", output)
    print("Component integration passed. External FIX, GW and desktop E2E are NOT covered.")


if __name__ == "__main__":
    main()
