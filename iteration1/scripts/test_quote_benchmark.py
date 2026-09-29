"""Report and watchdog checks only; never launches QuoteSvr or a load test."""
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

import benchmark_quotesvr as benchmark


class QuoteBenchmarkChecks(unittest.TestCase):
    def test_watchdog_terminates_sleeping_child(self):
        process = subprocess.Popen([sys.executable, "-c", "import time; time.sleep(5)"])
        try:
            code, timed_out = benchmark.wait_bounded(process, .05)
            self.assertTrue(timed_out)
            self.assertEqual(124, code)
            self.assertIsNotNone(process.poll())
        finally:
            if process.poll() is None:
                process.kill()
                process.wait(timeout=3)

    def test_no_push_is_not_reported_as_zero_latency(self):
        samples = [{"scenario": "S", "topic": "ficc.quote.view.open", "replyCode": "0000",
                    "durationMs": value, "lockWaitMs": 0, "firstViewPublishMs": None}
                   for value in (10, 20, 150)]
        row = benchmark.summaries(samples)[0]
        self.assertEqual(20, row["durationMs"]["p50"])
        self.assertEqual(150, row["durationMs"]["p95"])
        self.assertFalse(row["filterTargetP95Within100ms"])
        self.assertIsNone(row["firstViewPublishMs"]["p95"])

    def test_incomplete_scenario_cannot_be_marked_measured_or_passed(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = Path(directory)
            (folder / "result.json").write_text(json.dumps({
                "status": "FAILED", "failure": "BENCHMARK_TIME_BUDGET", "rawSamples": [],
                "scenarios": [{"id": "S09-page-and-updates", "status": "MEASURED", "wallMs": 1}]
            }), encoding="utf-8")
            manifest = {"status": "RUNNING", "exitCode": 1, "schema": "ficc_bench_test", "distribution": "spread"}
            benchmark.write_report(folder, manifest)
            saved = json.loads((folder / "report.json").read_text(encoding="utf-8"))
            self.assertEqual("INCOMPLETE_TIME_BUDGET", saved["status"])
            self.assertEqual("NOT_MEASURED", saved["benchmark"]["scenarios"][0]["status"])
            self.assertIn("未完成", (folder / "report.zh-CN.md").read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
