"""Small correctness tests for trace correlation; no processes or benchmark load."""
import json
from pathlib import Path
import tempfile
import unittest
from filter_performance_report import correlate, generate_report, traces


class ReportTests(unittest.TestCase):
    def test_correlates_same_request_and_computes_nested_durations(self):
        sample = {"scenario": "AAA", "warmup": False, "requests": [
            {"requestId": "R1", "topic": "ficc.quote.view.open", "sdkRoundTripMs": 30}]}
        warnings = []
        rows = correlate([sample], {
            ("GW", "R1"): {"durationMs": 20},
            ("QuoteSvr", "R1"): {"durationMs": 12, "searchMs": 8, "searchCalls": 2},
            ("QuoteSvr", "R2"): {"durationMs": 99},
        }, warnings)
        self.assertEqual(rows[0]["clientGatewayResidualMs"], 10)
        self.assertEqual(rows[0]["gatewayQuoteResidualMs"], 8)
        self.assertEqual(rows[0]["searchCalls"], 2)
        self.assertTrue(rows[0]["traceComplete"])
        self.assertEqual(warnings, [])

    def test_missing_and_negative_durations_are_not_fabricated(self):
        sample = {"scenario": "AAA", "warmup": False, "requests": [
            {"requestId": "R1", "topic": "ficc.quote.view.open", "sdkRoundTripMs": 10}]}
        warnings = []
        rows = correlate([sample], {("GW", "R1"): {"durationMs": 20}}, warnings)
        self.assertIsNone(rows[0]["clientGatewayResidualMs"])
        self.assertIsNone(rows[0]["quoteSvrMs"])
        self.assertFalse(rows[0]["traceComplete"])
        self.assertEqual(len(warnings), 1)

    def test_duplicate_ids_are_not_silently_used(self):
        with tempfile.TemporaryDirectory() as temporary:
            directory = Path(temporary)
            line = 'FICC_PERF {"node":"GW","requestId":"R1","durationMs":2}\n'
            (directory / "gateway.log").write_text(line * 2, encoding="utf-8")
            warnings = []
            self.assertIsNone(traces(directory, warnings)[("GW", "R1")])
            self.assertEqual(len(warnings), 1)

    def test_stopped_run_still_generates_report_and_index(self):
        with tempfile.TemporaryDirectory() as temporary:
            directory = Path(temporary) / "stopped"
            directory.mkdir()
            (directory / "summary.json").write_text(json.dumps({
                "result": "STOPPED", "error": "Startup timeout", "quoteCount": 500,
                "schemaRemoved": True}), encoding="utf-8")
            report = generate_report(directory).read_text(encoding="utf-8")
            self.assertIn("STOPPED", report)
            self.assertIn("没有完成前端采样", report)
            self.assertTrue((directory / "requests.csv").exists())
            self.assertTrue((directory.parent / "index.zh-CN.md").exists())

    def test_legacy_samples_remain_visible_without_server_timings(self):
        with tempfile.TemporaryDirectory() as temporary:
            directory = Path(temporary) / "legacy"
            directory.mkdir()
            (directory / "latency.json").write_text(json.dumps({"samples": [
                {"scenario": "AAA", "warmup": False, "totalMs": 100}]}), encoding="utf-8")
            report = generate_report(directory).read_text(encoding="utf-8")
            self.assertIn("100.00", report)
            self.assertIn("旧版本未采集", report)
            self.assertIn("未采集", report)


if __name__ == "__main__":
    unittest.main()
