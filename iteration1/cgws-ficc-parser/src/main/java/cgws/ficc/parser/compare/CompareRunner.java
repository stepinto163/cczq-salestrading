package cgws.ficc.parser.compare;

import cgws.ficc.parser.compat.JavaParserFacade;
import cgws.ficc.parser.dict.BondDictionary;
import cgws.ficc.parser.model.ParseResponse;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.io.Reader;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;

/**
 * Comparison runner: fixed input and dictionary -> legacy result set + Java result set ->
 * field differences -> per case status, report and exit code.
 *
 * <p>Both sides run through the same boundary: {@code PreProcess} of the whole text, split into
 * lines, one parse per line, and every line result (including empty ones) is compared. Number of
 * results, their order and every field of every result take part in the comparison.
 *
 * <p>Usage:
 * <pre>
 * java cgws.ficc.parser.compare.CompareRunner --suite cases/sample-suite.json \
 *      --legacy-jar &lt;Parsing jar&gt; --native-lib &lt;dir|file&gt; --java &lt;java8&gt; [--work-dir &lt;dir&gt;]
 * java cgws.ficc.parser.compare.CompareRunner --suite cases/sample-suite.json --baseline baselines/x.json
 * java cgws.ficc.parser.compare.CompareRunner --suite cases/sample-suite.json --case C003 --capability bond-identification
 * </pre>
 *
 * <p>Exit codes: {@code 0} all matched, {@code 1} at least one difference or one side errored,
 * {@code 2} input or environment error (unknown case, missing or stale baseline),
 * {@code 3} baseline not executed.
 *
 * <p>The legacy side runs in a separate JVM with a hard timeout; nothing runs in parallel and no
 * retries are attempted. The runner never writes quotes and never calls the live service.
 */
public final class CompareRunner {

    private static final String[] STATUS_ORDER = {
            "MATCH", "DIFF", "BASELINE_ERROR", "CANDIDATE_ERROR", "TIMEOUT", "NOT_RUN"
    };

    public static void main(String[] args) {
        int code = new CompareRunner().run(args);
        System.out.println("exitCode=" + code + " (" + ExitCodes.describe(code) + ")");
        System.exit(code);
    }

    /** Runs the comparison and returns the exit code; usable from tests without System.exit. */
    public int run(String[] args) {
        try {
            return execute(args);
        } catch (IllegalArgumentException e) {
            System.out.println("INPUT_ERROR: " + e.getMessage());
            return ExitCodes.INPUT_ERROR;
        } catch (Exception e) {
            System.out.println("INPUT_ERROR: " + e);
            return ExitCodes.INPUT_ERROR;
        }
    }

    private int execute(String[] args) throws Exception {
        Map<String, String> opts = parseArgs(args);
        String suitePath = required(opts, "--suite");
        File suiteFile = new File(suitePath);
        if (!suiteFile.isFile()) {
            throw new IllegalArgumentException("suite file does not exist: " + suiteFile.getAbsolutePath());
        }
        CaseSet set = CaseSet.load(suiteFile);

        String caseFilter = opts.get("--case");
        String capabilityFilter = opts.get("--capability");
        String baselinePath = opts.get("--baseline");
        String legacyJar = opts.get("--legacy-jar");
        String nativeLib = opts.get("--native-lib");
        String javaExe = opts.get("--java");
        String workDir = opts.get("--work-dir");
        String reportDir = opts.get("--out") == null ? "reports" : opts.get("--out");
        int timeoutSeconds = opts.get("--timeout") == null ? 30 : Integer.parseInt(opts.get("--timeout"));

        List<CaseSet.Case> selected = selectCases(set, caseFilter, capabilityFilter);
        if (selected.isEmpty()) {
            throw new IllegalArgumentException("no case selected (--case=" + caseFilter
                    + " --capability=" + capabilityFilter + ")");
        }

        NativeArtifact nativeArtifact = NativeArtifact.resolve(nativeLib, workDir);
        Map<String, Object> artifacts = describeArtifacts(legacyJar, nativeArtifact);
        String inputFingerprint = Fingerprint.ofInputs(set);

        Map<String, Object> baseline = null;
        String baselineFileUsed = null;
        boolean baselineTimeout = false;
        String baselineProblem = null;

        if (baselinePath != null) {
            File baselineFile = new File(baselinePath);
            if (!baselineFile.isFile()) {
                throw new IllegalArgumentException("baseline file does not exist: "
                        + baselineFile.getAbsolutePath());
            }
            baseline = readJson(baselineFile);
            baselineProblem = verifyBaseline(baseline, set, inputFingerprint, artifacts);
            if (baselineProblem != null) {
                System.out.println("INVALID_BASELINE: " + baselineProblem);
                return ExitCodes.INPUT_ERROR;
            }
            baselineFileUsed = baselineFile.getPath();
        } else if (legacyJar != null && nativeLib != null) {
            File out = new File(opts.get("--baseline-out") == null
                    ? reportDir + "/" + set.id + ".baseline.json"
                    : opts.get("--baseline-out"));
            File parent = out.getAbsoluteFile().getParentFile();
            if (parent != null) {
                parent.mkdirs();
            }
            String runId = UUID.randomUUID().toString();
            BaselineResult launched = launchBaseline(javaExe, legacyJar, nativeArtifact, suiteFile, out,
                    runId, workDir, timeoutSeconds);
            System.out.println("baseline: " + launched.output);
            if (launched.timeout || launched.exitCode != 0) {
                System.out.println("BASELINE_NOT_RUN: runId=" + runId + " " + launched.output);
                writeNotRunReport(set, reportDir, baselineFileUsed, inputFingerprint, artifacts,
                        launched.output, launched.timeout);
                return ExitCodes.NOT_RUN;
            }
            if (!out.isFile()) {
                System.out.println("BASELINE_NOT_RUN: no baseline file " + out.getAbsolutePath());
                writeNotRunReport(set, reportDir, baselineFileUsed, inputFingerprint, artifacts,
                        "no baseline file produced", false);
                return ExitCodes.NOT_RUN;
            }
            baseline = readJson(out);
            if (!runId.equals(Json.str(baseline, "runId"))) {
                System.out.println("BASELINE_NOT_RUN: file is not from this run (expected runId=" + runId + ")");
                writeNotRunReport(set, reportDir, baselineFileUsed, inputFingerprint, artifacts,
                        "baseline file is not from this run", false);
                return ExitCodes.NOT_RUN;
            }
            baselineProblem = verifyBaseline(baseline, set, inputFingerprint, artifacts);
            if (baselineProblem != null) {
                System.out.println("INVALID_BASELINE: " + baselineProblem);
                return ExitCodes.INPUT_ERROR;
            }
            baselineFileUsed = out.getPath();
        }

        JavaParserFacade facade = JavaParserFacade.getInstance();
        BondDictionary dictionary = facade.loadRows(set.dictionary);

        Report report = new Report(set, inputFingerprint, baselineFileUsed, artifacts, dictionary);
        Map<String, Object> baselineByCase = new LinkedHashMap<String, Object>();
        if (baseline != null) {
            for (Object item : Json.asArray(baseline.get("cases"))) {
                Map<String, Object> entry = Json.asObject(item);
                baselineByCase.put(Json.str(entry, "id"), entry);
            }
        }

        int index = 0;
        for (CaseSet.Case one : selected) {
            index++;
            report.add(compareCase(one, baseline, baselineByCase, facade, baselineTimeout));
            System.out.println("[" + index + "/" + selected.size() + "] " + one.id + " "
                    + report.lastStatus());
        }

        int exitCode = report.exitCode();
        report.exitCode = exitCode;
        File reportFile = report.write(reportDir);
        System.out.println("report: " + reportFile.getPath());
        System.out.println("summary: " + report.summary);
        return exitCode;
    }

    private Map<String, Object> compareCase(CaseSet.Case one, Map<String, Object> baseline,
                                            Map<String, Object> baselineByCase,
                                            JavaParserFacade facade, boolean baselineTimeout) {
        Map<String, Object> entry = new LinkedHashMap<String, Object>();
        entry.put("id", one.id);
        entry.put("capability", one.capability);
        entry.put("text", one.text);
        entry.put("businessDate", one.businessDate);
        entry.put("expectedSource", one.expectedSource);

        String status;
        String baselinePreprocessed = null;
        String candidatePreprocessed = null;
        List<String> baselineRaws = new ArrayList<String>();
        List<String> candidateRaws = new ArrayList<String>();
        List<ResultDiffer.FieldDiff> diffs = new ArrayList<ResultDiffer.FieldDiff>();

        if (baseline == null || baselineTimeout) {
            status = baselineTimeout ? "TIMEOUT" : "NOT_RUN";
            entry.put("status", status);
            return entry;
        }
        Object storedCase = baselineByCase.get(one.id);
        if (!(storedCase instanceof Map)) {
            status = "NOT_RUN";
            entry.put("status", status);
            entry.put("reason", "baseline has no result for this case id");
            return entry;
        }
        Map<String, Object> base = Json.asObject(storedCase);
        String baseText = Json.str(base, "text");
        if (baseText != null && !baseText.equals(one.text)) {
            status = "BASELINE_ERROR";
            entry.put("status", status);
            entry.put("reason", "baseline text does not match the suite text for this id");
            return entry;
        }
        if (base.get("error") != null) {
            status = "BASELINE_ERROR";
            entry.put("status", status);
            entry.put("baselineError", Json.str(base, "error"));
            return entry;
        }
        baselinePreprocessed = Json.str(base, "preprocessed");
        for (Object item : Json.asArray(base.get("lines"))) {
            Map<String, Object> lineEntry = Json.asObject(item);
            String raw = Json.str(lineEntry, "raw");
            baselineRaws.add(raw == null ? "" : raw);
        }
        try {
            ParseResponse response = facade.Parse(one.text, one.businessDate);
            if (response.failed) {
                status = "CANDIDATE_ERROR";
                entry.put("candidateError", response.diagnosticsAsString());
            } else {
                candidatePreprocessed = response.preprocessed;
                candidateRaws = new ArrayList<String>(response.lineResults);
                diffs = ResultDiffer.diff(baselinePreprocessed, candidatePreprocessed,
                        baselineRaws, candidateRaws);
                status = diffs.isEmpty() ? "MATCH" : "DIFF";
            }
        } catch (RuntimeException e) {
            status = "CANDIDATE_ERROR";
            entry.put("candidateError", String.valueOf(e));
        }

        entry.put("status", status);
        entry.put("baselinePreprocessed", baselinePreprocessed);
        entry.put("candidatePreprocessed", candidatePreprocessed);
        entry.put("baselineResults", baselineRaws);
        entry.put("candidateResults", candidateRaws);
        List<Object> diffList = new ArrayList<Object>();
        for (ResultDiffer.FieldDiff d : diffs) {
            Map<String, Object> m = new LinkedHashMap<String, Object>();
            m.put("field", d.path);
            m.put("baseline", d.baseline);
            m.put("candidate", d.candidate);
            diffList.add(m);
        }
        entry.put("diffs", diffList);
        return entry;
    }

    private List<CaseSet.Case> selectCases(CaseSet set, String caseFilter, String capabilityFilter) {
        List<CaseSet.Case> selected = new ArrayList<CaseSet.Case>();
        List<String> unknown = new ArrayList<String>();
        if (caseFilter != null && !caseFilter.trim().isEmpty()) {
            for (String id : caseFilter.split(",")) {
                CaseSet.Case found = set.find(id.trim());
                if (found == null) {
                    unknown.add(id.trim());
                } else if (!selected.contains(found)) {
                    selected.add(found);
                }
            }
            if (!unknown.isEmpty()) {
                throw new IllegalArgumentException("unknown case id(s): " + unknown);
            }
        } else {
            for (CaseSet.Case c : set.cases) {
                selected.add(c);
            }
        }
        if (capabilityFilter != null) {
            List<CaseSet.Case> filtered = new ArrayList<CaseSet.Case>();
            for (CaseSet.Case c : selected) {
                if (capabilityFilter.equals(c.capability)) {
                    filtered.add(c);
                }
            }
            selected = filtered;
        }
        return selected;
    }

    /**
     * Describes the artifacts of the run and refuses anything whose identity cannot be established:
     * a path the caller provided must exist and be readable, and its hash must be computable.
     */
    private Map<String, Object> describeArtifacts(String legacyJar, NativeArtifact nativeArtifact) {
        Map<String, Object> artifacts = new LinkedHashMap<String, Object>();
        if (legacyJar != null) {
            File jar = new File(legacyJar).getAbsoluteFile();
            if (!jar.isFile() || !jar.canRead()) {
                throw new IllegalArgumentException("legacy jar is missing or unreadable: " + jar);
            }
            String hash = Fingerprint.sha256(jar);
            if (hash == null) {
                throw new IllegalArgumentException("cannot hash legacy jar " + jar);
            }
            Map<String, Object> entry = new LinkedHashMap<String, Object>();
            entry.put("path", jar.getAbsolutePath());
            entry.put("sha256", hash);
            artifacts.put("legacyJar", entry);
        }
        if (nativeArtifact != null) {
            Map<String, Object> entry = new LinkedHashMap<String, Object>();
            entry.put("path", nativeArtifact.loadedFile.getAbsolutePath());
            entry.put("sha256", nativeArtifact.sha256);
            entry.put("requested", nativeArtifact.requested.getAbsolutePath());
            entry.put("loadDirectory", nativeArtifact.loadDirectory.getAbsolutePath());
            artifacts.put("nativeLibrary", entry);
        }
        return artifacts;
    }

    private String verifyBaseline(Map<String, Object> baseline, CaseSet set,
                                  String inputFingerprint, Map<String, Object> currentArtifacts) {
        String schema = Json.str(baseline, "schema");
        if (!Fingerprint.SCHEMA.equals(schema)) {
            return "baseline schema " + schema + " != " + Fingerprint.SCHEMA;
        }
        String suiteId = Json.str(baseline, "suite");
        if (suiteId == null || !suiteId.equals(set.id)) {
            return "baseline suite " + suiteId + " != " + set.id;
        }
        String storedFingerprint = Json.str(baseline, "suiteFingerprint");
        if (!inputFingerprint.equals(storedFingerprint)) {
            return "baseline input fingerprint " + storedFingerprint + " != " + inputFingerprint
                    + " (inputs changed since the baseline was produced)";
        }
        String platform = Json.str(baseline, "platform");
        if (platform == null) {
            return "baseline has no platform, its identity cannot be checked";
        }
        if (!platform.equals(Fingerprint.platform())) {
            return "baseline platform " + platform + " != " + Fingerprint.platform()
                    + " (a Windows baseline cannot be replayed on Linux)";
        }
        return verifyArtifacts(baseline, currentArtifacts, set);
    }

    /**
     * Every artifact identity must be present in the baseline and must be confirmed either against
     * the artifact on this machine or against the expectation the suite declares. A missing, empty
     * or unverifiable identity is never accepted.
     */
    private String verifyArtifacts(Map<String, Object> baseline, Map<String, Object> currentArtifacts,
                                   CaseSet set) {
        Map<String, Object> stored = Json.asObject(baseline.get("artifacts"));
        if (stored.isEmpty()) {
            return "baseline records no artifacts, its identity cannot be checked";
        }
        String[] required = {"legacyJar", "nativeLibrary"};
        Map<String, String> declared = new LinkedHashMap<String, String>();
        declared.put("legacyJar", set.baselineJarSha256);
        declared.put("nativeLibrary", set.baselineNativeSha256);
        for (String key : required) {
            Map<String, Object> storedEntry = Json.asObject(stored.get(key));
            String storedHash = Json.str(storedEntry, "sha256");
            if (storedHash == null || !storedHash.matches("[0-9a-f]{64}")) {
                return "baseline " + key + " has no usable sha256 (value=" + storedHash + ")";
            }
            String declaredHash = declared.get(key);
            if (declaredHash != null && !declaredHash.equals(storedHash)) {
                return "baseline " + key + " sha256 " + storedHash
                        + " != hash the suite declares (" + declaredHash + ")";
            }
            Object given = currentArtifacts.get(key);
            if (given instanceof Map) {
                String currentHash = Json.str(Json.asObject(given), "sha256");
                if (currentHash == null) {
                    return "the " + key + " given for this run cannot be hashed";
                }
                if (!storedHash.equals(currentHash)) {
                    return "baseline " + key + " sha256 " + storedHash + " != current " + currentHash;
                }
            } else if (declaredHash == null) {
                return key + " identity cannot be checked: no artifact given and the suite "
                        + "declares no expected hash";
            }
        }
        return null;
    }

    private void writeNotRunReport(CaseSet set, String reportDir, String baselineFileUsed,
                                   String inputFingerprint, Map<String, Object> artifacts,
                                   String reason, boolean timeout) throws Exception {
        Report report = new Report(set, inputFingerprint, baselineFileUsed, artifacts, null);
        for (CaseSet.Case one : set.cases) {
            Map<String, Object> entry = new LinkedHashMap<String, Object>();
            entry.put("id", one.id);
            entry.put("capability", one.capability);
            entry.put("text", one.text);
            entry.put("status", timeout ? "TIMEOUT" : "NOT_RUN");
            entry.put("reason", reason);
            report.add(entry);
        }
        report.exitCode = ExitCodes.NOT_RUN;
        File file = report.write(reportDir);
        System.out.println("report: " + file.getPath());
    }

    private static final class BaselineResult {

        boolean timeout;
        int exitCode = -1;
        String output;
    }

    private static BaselineResult launchBaseline(String javaExe, String legacyJar,
                                                 NativeArtifact nativeArtifact,
                                                 File suiteFile, File out, String runId,
                                                 String workDir, int timeoutSeconds) throws Exception {
        String executable = javaExe == null ? "java" : new File(javaExe).getAbsolutePath();
        File jarFile = new File(legacyJar).getAbsoluteFile();
        File workingDirectory = new File(workDir == null ? System.getProperty("user.dir") : workDir)
                .getAbsoluteFile();
        List<String> command = new ArrayList<String>(Arrays.asList(
                executable, "-Xmx128m", "-cp", absoluteClassPath(jarFile),
                "-Djava.library.path=" + nativeArtifact.loadDirectory.getAbsolutePath(),
                BaselineRunner.class.getName(),
                "--suite", suiteFile.getAbsolutePath(),
                "--out", out.getAbsolutePath(),
                "--run-id", runId,
                "--legacy-jar", jarFile.getAbsolutePath(),
                "--native-lib", nativeArtifact.loadedFile.getAbsolutePath(),
                "--work-dir", workingDirectory.getAbsolutePath()));
        ProcessBuilder builder = new ProcessBuilder(command);
        // Linux legacy wrapper loads <working directory>/libparser.so, so it is explicit.
        builder.directory(workingDirectory);
        return runWithDeadline(builder, timeoutSeconds);
    }

    /** Class path for the child JVM: every entry absolutized, nothing relative. */
    private static String absoluteClassPath(File extraJar) {
        StringBuilder b = new StringBuilder();
        String separator = java.util.regex.Pattern.quote(File.pathSeparator);
        for (String entry : System.getProperty("java.class.path").split(separator)) {
            if (entry.trim().isEmpty()) {
                continue;
            }
            if (b.length() > 0) {
                b.append(File.pathSeparator);
            }
            b.append(new File(entry).getAbsolutePath());
        }
        if (b.length() > 0) {
            b.append(File.pathSeparator);
        }
        b.append(extraJar.getAbsolutePath());
        return b.toString();
    }

    private static BaselineResult runWithDeadline(ProcessBuilder builder, int timeoutSeconds)
            throws Exception {
        builder.redirectErrorStream(true);
        long deadline = System.currentTimeMillis() + timeoutSeconds * 1000L;
        final Process process = builder.start();
        final StringBuilder captured = new StringBuilder();
        Thread reader = new Thread(new Runnable() {
            @Override
            public void run() {
                byte[] buffer = new byte[4096];
                try {
                    int read;
                    while ((read = process.getInputStream().read(buffer)) > 0) {
                        captured.append(new String(buffer, 0, read, "UTF-8"));
                    }
                } catch (Exception ignored) {
                    // stream closed when the process ends or is killed
                }
            }
        });
        reader.setDaemon(true);
        reader.start();

        BaselineResult result = new BaselineResult();
        while (System.currentTimeMillis() < deadline) {
            try {
                result.exitCode = process.exitValue();
                reader.join(2000);
                result.output = "exit=" + result.exitCode + " " + captured.toString().trim();
                return result;
            } catch (IllegalThreadStateException notYet) {
                Thread.sleep(200);
            }
        }
        process.destroy();
        if (!process.waitFor(5, java.util.concurrent.TimeUnit.SECONDS)) {
            process.destroyForcibly();
            process.waitFor(5, java.util.concurrent.TimeUnit.SECONDS);
        }
        result.timeout = true;
        result.output = "timeout after " + timeoutSeconds + "s";
        return result;
    }

    private static final class Report {

        final Map<String, Object> root = new LinkedHashMap<String, Object>();
        final List<Object> cases = new ArrayList<Object>();
        final Map<String, Integer> summary = new LinkedHashMap<String, Integer>();
        int exitCode = ExitCodes.MATCH;

        Report(CaseSet set, String inputFingerprint, String baselineFile,
               Map<String, Object> artifacts, BondDictionary dictionary) {
            root.put("suite", set.id);
            root.put("platform", Fingerprint.platform());
            root.put("generated", new java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss")
                    .format(new java.util.Date()));
            root.put("inputFingerprint", inputFingerprint);
            root.put("baselineFile", baselineFile);
            root.put("artifacts", artifacts);
            Map<String, Object> environment = new LinkedHashMap<String, Object>();
            environment.put("os", System.getProperty("os.name"));
            environment.put("arch", System.getProperty("os.arch"));
            environment.put("jvm", System.getProperty("java.version"));
            environment.put("dictionaryVersion", dictionary == null ? null : dictionary.version());
            environment.put("dictionaryCodes", dictionary == null ? 0 : dictionary.size());
            root.put("environment", environment);
            for (String status : STATUS_ORDER) {
                summary.put(status, 0);
            }
        }

        void add(Map<String, Object> entry) {
            cases.add(entry);
            String status = Json.str(entry, "status");
            Integer count = summary.get(status);
            summary.put(status, count == null ? 1 : count + 1);
        }

        String lastStatus() {
            return Json.str(Json.asObject(cases.get(cases.size() - 1)), "status");
        }

        int exitCode() {
            if (summary.get("DIFF") > 0 || summary.get("BASELINE_ERROR") > 0
                    || summary.get("CANDIDATE_ERROR") > 0) {
                return ExitCodes.DIFF;
            }
            if (summary.get("TIMEOUT") > 0 || summary.get("NOT_RUN") > 0) {
                return ExitCodes.NOT_RUN;
            }
            return ExitCodes.MATCH;
        }

        File write(String reportDir) throws Exception {
            root.put("summary", summary);
            root.put("cases", cases);
            root.put("exitCode", exitCode);
            root.put("exitCodeMeaning", ExitCodes.describe(exitCode));
            new File(reportDir).mkdirs();
            File file = new File(reportDir, Json.str(root, "suite") + ".compare.json");
            PrintWriter writer = new PrintWriter(new OutputStreamWriter(new FileOutputStream(file), "UTF-8"));
            try {
                writer.println(Json.write(root));
            } finally {
                writer.close();
            }
            return file;
        }
    }

    private static Map<String, Object> readJson(File file) throws Exception {
        Reader reader = new InputStreamReader(new FileInputStream(file), "UTF-8");
        StringBuilder b = new StringBuilder();
        try {
            char[] buffer = new char[8192];
            int read;
            while ((read = reader.read(buffer)) > 0) {
                b.append(buffer, 0, read);
            }
        } finally {
            reader.close();
        }
        return Json.asObject(Json.parse(b.toString()));
    }

    private static Map<String, String> parseArgs(String[] args) {
        Map<String, String> opts = new LinkedHashMap<String, String>();
        for (int i = 0; i + 1 < args.length; i++) {
            if (args[i].startsWith("--")) {
                opts.put(args[i], args[i + 1]);
            }
        }
        return opts;
    }

    private static String required(Map<String, String> opts, String key) {
        String value = opts.get(key);
        if (value == null) {
            throw new IllegalArgumentException("missing required option " + key);
        }
        return value;
    }
}
