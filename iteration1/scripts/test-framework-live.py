"""Isolated Windows DPS/GW functional test of five FICC services; no external QTrade connection."""

import argparse
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--ats-root",
        type=Path,
        required=True,
        help="Existing local STC deployment, copied read-only",
    )
    parser.add_argument(
        "--java-home",
        type=Path,
        default=ROOT / "iteration1/runtime/tools/jdk8/jdk8u504-b01",
    )
    parser.add_argument("--user", required=True, help="Local STC test account")
    parser.add_argument(
        "--ui-cases", help="Run production WPF cases: all or comma-separated case IDs"
    )
    parser.add_argument(
        "--standby-test",
        action="store_true",
        help="Crash isolated primaries and validate standby takeover",
    )
    parser.add_argument(
        "--password-env",
        default="FICC_TEST_PASSWORD",
        help="Environment variable containing its password",
    )
    args = parser.parse_args()
    if os.name != "nt":
        parser.error("This validation harness currently supports Windows only")
    password = os.environ.get(args.password_env)
    if not password:
        parser.error(f"Set {args.password_env} for this test session")
    source = args.ats_root.resolve()
    if not (source / "control/ATSConfig.ini").is_file():
        parser.error("Missing source ATSConfig.ini")
    for name in ("quotesvr", "notificationsvr", "crmsvr", "qtintakesvr", "parsersvr"):
        module = (
            "cgws-ficc-parsersvr"
            if name == "parsersvr"
            else "cgws-ficc-saletrading-" + name
        )
        if not (
            ROOT / "iteration1" / module / "target/framework-check/javalib"
        ).is_dir():
            parser.error(
                "Run test-framework.py --services first; missing package for " + name
            )
    environment = dict(
        os.environ,
        FICC_FRAMEWORK_ATS_ROOT=str(source),
        FICC_FRAMEWORK_JAVA_HOME=str(args.java_home.resolve()),
        FICC_FRAMEWORK_TEST_USER=args.user,
        FICC_FRAMEWORK_TEST_PASSWORD=password,
        PYTHONIOENCODING="utf-8",
        FICC_FRAMEWORK_UI_CASES=args.ui_cases or "",
        FICC_FRAMEWORK_STANDBY_TEST="1" if args.standby_test else "0",
    )
    lock = ROOT / "iteration1/runtime/framework-live.lock"
    handle = lock.open("x")
    try:
        with handle:
            helper = ROOT / "iteration1/scripts/framework"
            subprocess.run(
                [sys.executable, str(helper / "prepare-live.py")],
                cwd=ROOT,
                env=environment,
                check=True,
                timeout=60,
            )
            # The runner has individual startup timeouts, a 75-second probe watchdog, and a
            # finally block that stops only its own processes. Never taskkill all Java processes.
            subprocess.run(
                [sys.executable, str(helper / "run-live.py")],
                cwd=ROOT,
                env=environment,
                check=True,
            )
    finally:
        handle.close()
        lock.unlink(missing_ok=True)
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except subprocess.CalledProcessError as error:
        sys.exit(error.returncode)
