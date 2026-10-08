package cgws.ficc.parser;

import cgws.ficc.parser.compare.CaseSet;
import cgws.ficc.parser.compare.Fingerprint;
import cgws.ficc.parser.compare.Json;
import cgws.ficc.parser.text.TextNormalizer;

import java.io.File;
import java.io.FileOutputStream;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.TimeUnit;

/** Builds throwaway suites and baselines for the runner tests. */
final class CompareTestSupport {

    /** Hashes the test suite declares as the expected baseline artifacts. */
    static final String JAR_SHA =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    static final String NATIVE_SHA =
            "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    static final String SUITE_JSON = "{\n"
            + "  \"id\": \"test-suite\",\n"
            + "  \"platform\": \"test\",\n"
            + "  \"baselineJarSha256\": \"" + JAR_SHA + "\",\n"
            + "  \"baselineNativeSha256\": \"" + NATIVE_SHA + "\",\n"
            + "  \"dictionary\": [\n"
            + "    { \"code\": \"2400001\", \"name\": \"24国债02\" },\n"
            + "    { \"code\": \"2400001.ib\", \"name\": \"24国债03\" }\n"
            + "  ],\n"
            + "  \"cases\": [\n"
            + "    { \"id\": \"C001\", \"capability\": \"bond-identification\", \"text\": \"2400001 2.35 1000\", \"businessDate\": \"20260929\" },\n"
            + "    { \"id\": \"C002\", \"capability\": \"multi-quote\", \"text\": \"2400001 2.35 1000\\n2400001.ib 3.10 2000\", \"businessDate\": \"20260929\" }\n"
            + "  ]\n"
            + "}\n";

    private CompareTestSupport() {
    }

    static File tempDir(String name) throws Exception {
        return Files.createTempDirectory(name).toFile();
    }

    static File writeSuite(File dir) throws Exception {
        File file = new File(dir, "suite.json");
        write(file, SUITE_JSON);
        return file;
    }

    static String expectedResult(String line) {
        cgws.ficc.parser.dict.BondDictionary.Builder builder =
                new cgws.ficc.parser.dict.BondDictionary.Builder();
        builder.put("2400001", "24国债02", null, false);
        builder.put("2400001.ib", "24国债03", null, false);
        cgws.ficc.parser.dict.BondDictionary dictionary = builder.build("test");
        cgws.ficc.parser.model.ParseResponse response =
                cgws.ficc.parser.parse.ParserCore.parse(line, dictionary, "20260929", true);
        return response.lineResults.isEmpty() ? "" : response.lineResults.get(0);
    }

    static File writeBaseline(File dir, File suiteFile, Map<String, List<String>> resultsPerCase,
                              String fingerprint) throws Exception {
        return writeBaseline(dir, suiteFile, resultsPerCase, fingerprint,
                Fingerprint.platform(), defaultArtifacts());
    }

    /**
     * @param platform  null writes a baseline without platform
     * @param artifacts null writes a baseline without artifacts
     */
    static File writeBaseline(File dir, File suiteFile, Map<String, List<String>> resultsPerCase,
                              String fingerprint, String platform,
                              Map<String, Object> artifacts) throws Exception {
        CaseSet set = CaseSet.load(suiteFile);
        Map<String, Object> root = new LinkedHashMap<String, Object>();
        root.put("schema", Fingerprint.SCHEMA);
        root.put("runId", "test-run");
        if (platform != null) {
            root.put("platform", platform);
        }
        root.put("jvm", System.getProperty("java.version"));
        root.put("suite", set.id);
        root.put("suiteFingerprint", fingerprint == null ? Fingerprint.ofInputs(set) : fingerprint);
        if (artifacts != null) {
            root.put("artifacts", artifacts);
        }
        List<Object> cases = new ArrayList<Object>();
        for (CaseSet.Case one : set.cases) {
            Map<String, Object> entry = new LinkedHashMap<String, Object>();
            entry.put("id", one.id);
            entry.put("text", one.text);
            entry.put("businessDate", one.businessDate);
            String working = TextNormalizer.preProcess(one.text);
            entry.put("preprocessed", working);
            List<Object> lines = new ArrayList<Object>();
            List<String> raws = resultsPerCase.get(one.id);
            String[] split = working.split("\r\n|\n|\r");
            // A baseline that carries fewer results than the text has lines models a dropped
            // result; the comparison must then report the count difference.
            int lineCount = raws == null ? split.length : Math.min(raws.size(), split.length);
            for (int i = 0; i < lineCount; i++) {
                Map<String, Object> lineEntry = new LinkedHashMap<String, Object>();
                lineEntry.put("text", split[i]);
                lineEntry.put("raw", raws.get(i));
                lines.add(lineEntry);
            }
            entry.put("lineCount", lineCount);
            entry.put("lines", lines);
            cases.add(entry);
        }
        root.put("cases", cases);
        File file = new File(dir, "baseline.json");
        write(file, Json.write(root));
        return file;
    }

    static Map<String, Object> defaultArtifacts() {
        Map<String, Object> artifacts = new LinkedHashMap<String, Object>();
        artifacts.put("legacyJar", artifact("legacy.jar", JAR_SHA));
        artifacts.put("nativeLibrary", artifact("native", NATIVE_SHA));
        return artifacts;
    }

    static Map<String, Object> artifact(String path, String sha256) {
        Map<String, Object> entry = new LinkedHashMap<String, Object>();
        entry.put("path", path);
        entry.put("sha256", sha256);
        return entry;
    }

    static Map<String, Object> readReport(File file) throws Exception {
        StringBuilder b = new StringBuilder();
        for (String line : Files.readAllLines(file.toPath(), StandardCharsets.UTF_8)) {
            b.append(line);
        }
        return Json.asObject(Json.parse(b.toString()));
    }

    /**
     * Runs a command with a hard deadline. Output goes to a file, so a child that never closes
     * stdout cannot block the deadline; on timeout the child is destroyed and awaited.
     */
    static final class ProcessResult {

        int exitCode = -1;
        boolean timedOut;
        String output;
    }

    static ProcessResult runProcess(List<String> command, File workingDir, File outputFile,
                                    int timeoutSeconds) throws Exception {
        ProcessBuilder builder = new ProcessBuilder(command);
        if (workingDir != null) {
            builder.directory(workingDir);
        }
        builder.redirectErrorStream(true);
        builder.redirectOutput(outputFile);
        long deadline = System.currentTimeMillis() + timeoutSeconds * 1000L;
        Process process = builder.start();
        ProcessResult result = new ProcessResult();
        while (System.currentTimeMillis() < deadline) {
            try {
                result.exitCode = process.exitValue();
                result.output = read(outputFile);
                return result;
            } catch (IllegalThreadStateException notYet) {
                Thread.sleep(100);
            }
        }
        process.destroy();
        if (!process.waitFor(5, TimeUnit.SECONDS)) {
            process.destroyForcibly();
            process.waitFor(5, TimeUnit.SECONDS);
        }
        result.timedOut = true;
        result.output = read(outputFile);
        return result;
    }

    private static String read(File file) throws Exception {
        if (!file.isFile()) {
            return "";
        }
        StringBuilder b = new StringBuilder();
        for (String line : Files.readAllLines(file.toPath(), StandardCharsets.UTF_8)) {
            b.append(line).append('\n');
        }
        return b.toString();
    }

    private static void write(File file, String content) throws Exception {
        PrintWriter writer = new PrintWriter(new OutputStreamWriter(new FileOutputStream(file), "UTF-8"));
        try {
            writer.print(content);
        } finally {
            writer.close();
        }
    }
}
