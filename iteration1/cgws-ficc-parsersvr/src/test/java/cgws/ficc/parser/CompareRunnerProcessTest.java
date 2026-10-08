package cgws.ficc.parser;

import cgws.ficc.parser.compare.ExitCodes;
import cgws.ficc.parser.compare.Fingerprint;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Launches the runner as an external process and asserts the real exit code, so automation that
 * shells out to it cannot be fooled by a printed message.
 */
class CompareRunnerProcessTest {

    @Test
    void matchReturnsZero() throws Exception {
        File dir = CompareTestSupport.tempDir("process-match");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null);
        int code = launch(dir, "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath());
        assertEquals(ExitCodes.MATCH, code);
    }

    @Test
    void diffReturnsOne() throws Exception {
        File dir = CompareTestSupport.tempDir("process-diff");
        File suite = CompareTestSupport.writeSuite(dir);
        Map<String, List<String>> results = correctResults();
        results.put("C001", Arrays.asList(
                "Code:2400001.ib;Name:24国债03;CodeList:2400001.ib,;Volume:1000;Single:9.99,;"));
        File baseline = CompareTestSupport.writeBaseline(dir, suite, results, null);
        int code = launch(dir, "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath());
        assertEquals(ExitCodes.DIFF, code, "DIFF must leave a non zero exit code");
    }

    @Test
    void staleBaselineReturnsTwo() throws Exception {
        File dir = CompareTestSupport.tempDir("process-stale");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(),
                Fingerprint.ofText("inputs that no longer exist"));
        int code = launch(dir, "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath());
        assertEquals(ExitCodes.INPUT_ERROR, code, "a stale baseline must not exit 0");
    }

    @Test
    void baselineWithoutIdentityReturnsTwo() throws Exception {
        File dir = CompareTestSupport.tempDir("process-no-identity");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null,
                null, CompareTestSupport.defaultArtifacts());
        int code = launch(dir, "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath());
        assertEquals(ExitCodes.INPUT_ERROR, code,
                "a baseline without platform must not exit 0");
    }

    @Test
    void unknownCaseReturnsTwo() throws Exception {
        File dir = CompareTestSupport.tempDir("process-unknown-case");
        File suite = CompareTestSupport.writeSuite(dir);
        File baseline = CompareTestSupport.writeBaseline(dir, suite, correctResults(), null);
        int code = launch(dir, "--suite", suite.getAbsolutePath(),
                "--baseline", baseline.getAbsolutePath(), "--case", "C999");
        assertEquals(ExitCodes.INPUT_ERROR, code, "an unknown case id must not exit 0");
    }

    /** A hung child must be killed at the deadline, not waited for forever. */
    @Test
    void hungChildIsKilledAtTheDeadline() throws Exception {
        File dir = CompareTestSupport.tempDir("process-timeout");
        String javaExecutable = javaExecutable();
        String classPath = parserTestClassPath();
        File output = new File(dir, "hanging.out");
        long start = System.currentTimeMillis();
        CompareTestSupport.ProcessResult result = CompareTestSupport.runProcess(
                Arrays.asList(javaExecutable, "-Dfile.encoding=UTF-8", "-Xmx64m", "-cp", classPath,
                        HangingProcess.class.getName()),
                new File(System.getProperty("user.dir")), output, 5);
        long elapsed = System.currentTimeMillis() - start;
        assertTrue(result.timedOut, "the child never exits on its own, the deadline must fire");
        assertTrue(elapsed < 40000L, "elapsed=" + elapsed + "ms, the deadline must be honoured");
        assertTrue(result.output.contains("HANGING_PROCESS_STARTED"), "output=" + result.output);
    }

    private static Map<String, List<String>> correctResults() {
        Map<String, List<String>> results = new LinkedHashMap<String, List<String>>();
        results.put("C001", Arrays.asList(CompareTestSupport.expectedResult("2400001 2.35 1000")));
        results.put("C002", Arrays.asList(
                CompareTestSupport.expectedResult("2400001 2.35 1000"),
                CompareTestSupport.expectedResult("2400001.ib 3.10 2000")));
        return results;
    }

    private static String javaExecutable() {
        File java = new File(System.getProperty("java.home"), "bin/java");
        if (!java.isFile()) {
            java = new File(System.getProperty("java.home"), "bin/java.exe");
        }
        return java.getAbsolutePath();
    }

    private static int launch(File dir, String... args) throws Exception {
        List<String> command = new ArrayList<String>();
        command.add(javaExecutable());
        command.add("-Dfile.encoding=UTF-8");
        command.add("-Xmx128m");
        command.add("-cp");
        command.add(parserTestClassPath());
        command.add("cgws.ficc.parser.compare.CompareRunner");
        command.addAll(Arrays.asList(args));
        command.add("--out");
        command.add(new File(dir, "reports").getAbsolutePath());
        File output = new File(dir, "runner.out");
        CompareTestSupport.ProcessResult result = CompareTestSupport.runProcess(command,
                new File(System.getProperty("user.dir")), output, 60);
        assertTrue(!result.timedOut, "runner did not finish within 60s: " + result.output);
        int code = result.exitCode;
        assertTrue(result.output.contains("exitCode=" + code),
                "runner must print its exit code: " + result.output);
        return code;
    }

    private static String parserTestClassPath() throws Exception {
        File mainClasses = new File(cgws.ficc.parser.compare.CompareRunner.class
                .getProtectionDomain().getCodeSource().getLocation().toURI());
        File testClasses = new File(HangingProcess.class
                .getProtectionDomain().getCodeSource().getLocation().toURI());
        return mainClasses.getAbsolutePath() + File.pathSeparator + testClasses.getAbsolutePath();
    }
}
