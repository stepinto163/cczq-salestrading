"""Bounded compatibility verification; never deploys or starts a FICC/FIX service."""

import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
ITERATION = ROOT / "iteration1"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def protected_files():
    result = {}
    for module in ITERATION.glob("cgws-ficc-saletrading-*"):
        for folder in ("src", "control", "config", "salt-formula"):
            source = module / folder
            if source.is_dir():
                for path in source.rglob("*"):
                    if path.is_file() and path.suffix in (
                        ".java",
                        ".xml",
                        ".ini",
                        ".properties",
                    ):
                        result[path.relative_to(ROOT).as_posix()] = digest(path)
    return result


def run(command, log, seconds, environment, working_directory=ROOT):
    print("Running: " + log.stem, flush=True)
    started = time.monotonic()
    with log.open("w", encoding="utf-8") as output:
        process = subprocess.Popen(
            command,
            cwd=working_directory,
            env=environment,
            stdout=output,
            stderr=subprocess.STDOUT,
            creationflags=subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0,
            start_new_session=os.name != "nt",
        )
        try:
            code = process.wait(timeout=seconds)
        except subprocess.TimeoutExpired:
            if os.name == "nt":
                subprocess.run(
                    ["taskkill", "/PID", str(process.pid), "/T", "/F"],
                    stdout=subprocess.DEVNULL,
                    stderr=subprocess.DEVNULL,
                    check=False,
                )
            else:
                import signal

                os.killpg(process.pid, signal.SIGKILL)
            process.wait()
            raise RuntimeError(
                f"Watchdog stopped {log.stem} after {seconds}s; see {log}"
            )
    if code:
        raise RuntimeError(f"{log.stem} failed ({code}); see {log}")
    return {
        "step": log.stem,
        "seconds": round(time.monotonic() - started, 2),
        "exitCode": code,
    }


def test_totals(folder):
    totals = dict(tests=0, failures=0, errors=0, skipped=0)
    for path in folder.glob("TEST-*.xml"):
        suite = ET.parse(path).getroot()
        for key in totals:
            totals[key] += int(suite.get(key, 0))
    return totals


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--legacy-jars", type=Path, required=True)
    parser.add_argument(
        "--maven",
        type=Path,
        default=ITERATION / "runtime/tools/apache-maven-3.9.16/bin/mvn.cmd",
    )
    parser.add_argument(
        "--services",
        action="store_true",
        help="Also compile and run existing unit tests of unchanged services",
    )
    parser.add_argument(
        "--java-home", type=Path, default=ITERATION / "runtime/tools/jdk8/jdk8u504-b01"
    )
    parser.add_argument(
        "--license",
        type=Path,
        required=True,
        help="Existing DPS license for loopback transport tests",
    )
    parser.add_argument(
        "--timeout", type=int, default=120, help="Per-process watchdog seconds"
    )
    args = parser.parse_args()
    baseline = args.legacy_jars.resolve()
    if not (baseline / "ats-bbox-framework-1.2.3.jar").is_file():
        parser.error(
            "Expected the deployed ats-bbox-framework-1.2.3.jar in --legacy-jars"
        )
    output = ITERATION / "runtime/framework-checks" / time.strftime("%Y%m%d-%H%M%S")
    output.mkdir(parents=True, exist_ok=False)
    environment = dict(os.environ, MAVEN_OPTS="-Xmx256m -XX:ActiveProcessorCount=2")
    environment["JAVA_HOME"] = str(args.java_home.resolve())
    environment["PATH"] = (
        str(args.java_home.resolve() / "bin") + os.pathsep + environment.get("PATH", "")
    )
    before = protected_files()
    report = {
        "status": "RUNNING",
        "steps": [],
        "baseline": {},
        "tests": {},
        "limits": {
            "mavenHeapMb": 256,
            "forkHeapMb": 128,
            "timeoutSeconds": args.timeout,
        },
    }
    for jar in sorted(baseline.glob("*.jar")):
        report["baseline"][jar.name] = {
            "sha256": digest(jar),
            "bytes": jar.stat().st_size,
        }

    def maven(name, pom, options):
        command = [
            str(args.maven.resolve()),
            "-B",
            "-ntp",
            "-o",
            "-f",
            str(pom),
        ] + options
        report["steps"].append(
            run(command, output / (name + ".log"), args.timeout, environment)
        )

    try:
        framework = ITERATION / "cgws-ficc-framework"
        maven(
            "framework",
            framework / "pom.xml",
            [
                f"-Dlegacy.framework.directory={baseline}",
                f"-Ddps.licensePath={args.license.resolve()}",
                "clean",
                "install",
            ],
        )
        for component in ("core", "dps", "compat"):
            report["tests"][component] = test_totals(
                framework / component / "target/surefire-reports"
            )
            if report["tests"][component]["skipped"]:
                raise RuntimeError(
                    "Framework verification must not skip transport or baseline tests"
                )
        if args.services:
            parent = ITERATION / "cgws-ficc-saletrading-parent-pom/pom.xml"
            maven("parent", parent, ["install"])
            maven("parser-engine", ITERATION / "cgws-ficc-parser/pom.xml", ["install"])
            for name in (
                "common",
                "quotesvr",
                "notificationsvr",
                "crmsvr",
                "qtintakesvr",
                "parsersvr",
            ):
                module = ITERATION / (
                    "cgws-ficc-parsersvr"
                    if name == "parsersvr"
                    else "cgws-ficc-saletrading-" + name
                )
                options = ["-Dmaven.build.directory=target/framework-check"]
                # No benchmark or standalone database smoke program is invoked by these goals.
                goals = (
                    ["clean", "install"] if name == "common" else ["clean", "package"]
                )
                maven(name, module / "pom.xml", options + goals)
                report["tests"][name] = test_totals(
                    module / "target/framework-check/surefire-reports"
                )
                maven(
                    name + "-dependencies",
                    module / "pom.xml",
                    options
                    + [
                        "org.apache.maven.plugins:maven-dependency-plugin:3.8.1:build-classpath",
                        "-Dmdep.outputFile=" + str(output / (name + "-classpath.txt")),
                    ],
                )
                classpath = (output / (name + "-classpath.txt")).read_text(
                    encoding="utf-8"
                )
                for legacy in (
                    "ats-bbox-framework-",
                    "sc-app-stc-",
                    "sc-app-common-",
                    "ats-foundation-utilities-",
                    "ats-app-ym-foundation-",
                ):
                    if legacy in classpath:
                        raise RuntimeError(f"{name} still contains {legacy}")
                if "cgws-ficc-framework-compat-" not in classpath:
                    raise RuntimeError(f"{name} does not contain the replacement")
                sdk = [
                    Path(item)
                    for item in classpath.strip().split(os.pathsep)
                    if Path(item).name == "gateway-api-0.1.1-SNAPSHOT.jar"
                ]
                if (
                    len(sdk) != 1
                    or digest(sdk[0])
                    != report["baseline"]["gateway-api-0.1.1-SNAPSHOT.jar"]["sha256"]
                ):
                    raise RuntimeError(f"{name} changed the underlying DPS binary")
                if name not in ("common", "parsersvr"):
                    ini_names = dict(
                        quotesvr="QuoteSvr",
                        notificationsvr="NotificationSvr",
                        crmsvr="CRMSvr",
                        qtintakesvr="QtIntakeSvr",
                    )
                    probe_classpath = os.pathsep.join(
                        [
                            str(framework / "compat/target/test-classes"),
                            str(module / "target/framework-check/classes"),
                            classpath.strip(),
                        ]
                    )
                    report["steps"].append(
                        run(
                            [
                                str(args.java_home.resolve() / "bin/java"),
                                "-Xmx128m",
                                "-XX:ActiveProcessorCount=2",
                                "-cp",
                                probe_classpath,
                                "cgws.ficc.framework.compat.ApplicationConfigurationProbe",
                                "./control/" + ini_names[name] + ".ini",
                            ],
                            output / (name + "-configuration.log"),
                            20,
                            environment,
                            module,
                        )
                    )
            parser_host = ITERATION / "cgws-ficc-parsersvr"
            imported = json.loads(
                (parser_host / "source-baseline.json").read_text(encoding="utf-8")
            )
            if any(
                digest(parser_host / name) != value
                for name, value in imported["files"].items()
            ):
                raise RuntimeError("ParserSvr imported application source changed")
        report["protectedSourceAndConfigUnchanged"] = before == protected_files()
        if not report["protectedSourceAndConfigUnchanged"]:
            raise RuntimeError(
                "Application source/configuration changed during verification"
            )
        report["status"] = "PASSED"
    except Exception as error:
        report["status"] = "FAILED"
        report["reason"] = str(error)
    finally:
        (output / "report.json").write_text(
            json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8"
        )
    print(report["status"] + ": " + str(output / "report.json"), flush=True)
    if report["status"] != "PASSED":
        print(report["reason"], file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
