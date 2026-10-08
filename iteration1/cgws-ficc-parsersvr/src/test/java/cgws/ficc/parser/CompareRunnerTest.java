package cgws.ficc.parser;

import cgws.ficc.parser.compare.CompareRunner;
import cgws.ficc.parser.compare.ExitCodes;
import cgws.ficc.parser.compare.Fingerprint;
import cgws.ficc.parser.compare.Json;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.nio.file.Files;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/** In-process behaviour of the comparison runner, including the negative cases from the review. */
class CompareRunnerTest {

    private static Map<String, List<String>> correctResults() {
        Map<String, List<String>> results = new LinkedHashMap<String, List<String>>();
        results.put("C001", Arrays.asList(CompareTestSupport.expectedResult("2400001 2.35 1000")));
        results.put("C002", Arrays.asList(
                CompareTestSupport.expectedResult("2400001 2.35 1000"),
                CompareTestSupport.expectedResult("2400001.ib 3.10 2000")));
        return results;
    }

    @Test
    void completeResultSetMatches() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-match");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.MATCH, code);
        Map<String, Object> report = CompareTestSupport.readReport(
                new File(dir, "reports/test-suite.compare.json"));
        assertEquals(0, ((Number) Json.asObject(report.get("summary")).get("DIFF")).intValue());
        assertEquals(2, ((Number) Json.asObject(report.get("summary")).get("MATCH")).intValue());
    }

    @Test
    void missingSecondResultIsDiff() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-missing-row");
        File suite = CompareTestSupport.writeSuite(dir);
        Map<String, List<String>> results = correctResults();
        // Review repro: keep the id and the first result, drop the second line result.
        results.put("C002", Arrays.asList(CompareTestSupport.expectedResult("2400001 2.35 1000")));
        File baseline = CompareTestSupport.writeBaseline(dir, suite, results, null);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--case", "C002",
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.DIFF, code, "a dropped second result must not be reported as MATCH");
        Map<String, Object> report = CompareTestSupport.readReport(
                new File(dir, "reports/test-suite.compare.json"));
        List<Object> cases = Json.asArray(report.get("cases"));
        Map<String, Object> first = Json.asObject(cases.get(0));
        assertEquals("DIFF", Json.str(first, "status"));
        boolean countDiff = false;
        for (Object diff : Json.asArray(first.get("diffs"))) {
            if ("resultCount".equals(Json.str(Json.asObject(diff), "field"))) {
                countDiff = true;
            }
        }
        assertTrue(countDiff, "the result count difference must be visible: " + first.get("diffs"));
    }

    @Test
    void changedPriceIsDiff() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-price");
        File suite = CompareTestSupport.writeSuite(dir);
        Map<String, List<String>> results = correctResults();
        results.put("C001", Arrays.asList("Code:2400001.ib;Name:24国债03;CodeList:2400001.ib,;Volume:1000;Single:9.99,;"));
        File baseline = CompareTestSupport.writeBaseline(dir, suite, results, null);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--case", "C001",
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.DIFF, code);
    }

    @Test
    void staleFingerprintIsInputError() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-stale");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(),
                Fingerprint.ofText("inputs that no longer exist"));
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.INPUT_ERROR, code, "a baseline produced for other inputs must fail");
    }

    @Test
    void missingBaselineFileIsInputError() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-nobaseline");
        File suite = CompareTestSupport.writeSuite(dir);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", new File(dir, "does-not-exist.json").getAbsolutePath(),
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.INPUT_ERROR, code);
    }

    @Test
    void unknownCaseIsInputError() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-unknown-case");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--case", "C999",
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.INPUT_ERROR, code, "an unknown case id must not be reported as success");
    }

    @Test
    void emptySelectionIsInputError() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-empty-selection");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--capability", "there-is-no-such-capability",
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.INPUT_ERROR, code, "selecting zero cases must not be reported as success");
    }

    @Test
    void baselineWithoutPlatformIsInputError() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-no-platform");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null,
                null, CompareTestSupport.defaultArtifacts());
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.INPUT_ERROR, code, "a baseline without a platform is not usable");
    }

    @Test
    void baselineWithoutArtifactsIsInputError() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-no-artifacts");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null,
                Fingerprint.platform(), null);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.INPUT_ERROR, code, "a baseline without artifact identity is not usable");
    }

    @Test
    void baselineWithZeroedArtifactHashesIsInputError() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-zeroed-hashes");
        File suite = CompareTestSupport.writeSuite(dir);
        String zeros = new String(new char[64]).replace('\0', '0');
        Map<String, Object> artifacts = new LinkedHashMap<String, Object>();
        artifacts.put("legacyJar", CompareTestSupport.artifact("legacy.jar", zeros));
        artifacts.put("nativeLibrary", CompareTestSupport.artifact("native", zeros));
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null,
                Fingerprint.platform(), artifacts);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.INPUT_ERROR, code,
                "hashes that do not match the declared expectation must be rejected");
    }

    @Test
    void missingLegacyJarPathIsInputError() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-missing-jar-path");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--legacy-jar", new File(dir, "no-such.jar").getAbsolutePath(),
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.INPUT_ERROR, code,
                "an explicitly given jar that does not exist must not be ignored");
    }

    @Test
    void dictionaryKeyChangeInvalidatesTheBaseline() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-dict-key");
        File original = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, original, correctResults(), null);
        String withKey = CompareTestSupport.SUITE_JSON.replace(
                "{ \"code\": \"2400001\", \"name\": \"24国债02\" }",
                "{ \"code\": \"2400001\", \"name\": \"24国债02\", \"key\": \"K-1\" }");
        File changed = new File(dir, "suite-with-key.json");
        Files.write(changed.toPath(), withKey.getBytes(java.nio.charset.StandardCharsets.UTF_8));
        int code = new CompareRunner().run(new String[]{
                "--suite", changed.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(),
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.INPUT_ERROR, code,
                "changing a dictionary key must invalidate the old baseline");
    }

    @Test
    void noBaselineArtifactsIsNotRun() throws Exception {
        File dir = CompareTestSupport.tempDir("compare-notrun");
        File suite = CompareTestSupport.writeSuite(dir);
        int code = new CompareRunner().run(new String[]{
                "--suite", suite.getAbsolutePath(),
                "--out", new File(dir, "reports").getAbsolutePath()});
        assertEquals(ExitCodes.NOT_RUN, code);
        Map<String, Object> report = CompareTestSupport.readReport(
                new File(dir, "reports/test-suite.compare.json"));
        assertEquals(2, ((Number) Json.asObject(report.get("summary")).get("NOT_RUN")).intValue());
        List<Object> cases = new ArrayList<Object>(Json.asArray(report.get("cases")));
        assertEquals("NOT_RUN", Json.str(Json.asObject(cases.get(0)), "status"));
    }
}
