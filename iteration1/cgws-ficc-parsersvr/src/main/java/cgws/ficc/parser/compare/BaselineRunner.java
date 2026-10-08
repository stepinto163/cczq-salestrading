package cgws.ficc.parser.compare;

import java.io.File;
import java.io.FileOutputStream;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.lang.reflect.Method;
import java.nio.file.Files;
import java.nio.file.StandardCopyOption;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;

/**
 * Runs the <b>working legacy binary</b> on a fixed case set and dumps raw results.
 *
 * <p>Boundary under test (identical on both sides):
 * <ol>
 *   <li>{@code PreProcess(text)} - native on this side, {@code TextNormalizer.preProcess} on the
 *       Java side; the resulting working text is compared as its own field.</li>
 *   <li>the working text is split into lines with the same rule ({@code \r\n|\n|\r}),</li>
 *   <li>every line is passed to {@code ParseBond(line, "", "")} once and every result string is
 *       kept, including empty ones, so the number and the order of results are part of the
 *       comparison.</li>
 * </ol>
 *
 * <p>Launched by {@link CompareRunner} in a separate JVM with a hard timeout. It never touches a
 * database and never constructs {@code BondQuoteParser}, so no full dictionary load is triggered;
 * the dictionary comes from the case set through {@code SetCodeName}.
 *
 * <p>Usage: {@code java -cp <runner classes>:<Parsing jar> [-Djava.library.path=<dir>]
 * cgws.ficc.parser.compare.BaselineRunner --suite <file> --out <file> --run-id <id>
 * [--legacy-jar <jar>] [--native-lib <dir|file>] [--work-dir <dir>]}
 */
public final class BaselineRunner {

    public static void main(String[] args) throws Exception {
        Map<String, String> opts = options(args);
        String suitePath = required(opts, "--suite");
        String outPath = required(opts, "--out");
        String runId = opts.get("--run-id") == null ? UUID.randomUUID().toString() : opts.get("--run-id");

        File suiteFile = new File(suitePath);
        CaseSet set = CaseSet.load(suiteFile);
        File outFile = new File(outPath);

        Class<?> parserClass = Class.forName("com.sumscope.Parser");
        Object parser = parserClass.getMethod("getInstance").invoke(null);
        Method setCodeName = parserClass.getMethod("SetCodeName", String.class, String.class, String.class, String.class);
        Method preProcess = parserClass.getMethod("PreProcess", String.class);
        Method parseBond = parserClass.getMethod("ParseBond", String.class, String.class, String.class);

        Map<String, Object> out = new LinkedHashMap<String, Object>();
        out.put("schema", Fingerprint.SCHEMA);
        out.put("runId", runId);
        out.put("generated", new java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss").format(new java.util.Date()));
        out.put("platform", Fingerprint.platform());
        out.put("jvm", System.getProperty("java.version"));
        out.put("suite", set.id);
        out.put("suiteFingerprint", Fingerprint.ofInputs(set));

        Map<String, Object> artifacts = new LinkedHashMap<String, Object>();
        addArtifact(artifacts, "legacyJar", opts.get("--legacy-jar"));
        addNativeArtifact(artifacts, opts.get("--native-lib"), opts.get("--work-dir"));
        out.put("artifacts", artifacts);

        List<Object> feed = new ArrayList<Object>();
        feed.add(setCodeName.invoke(parser, "start", "start", "", ""));
        for (cgws.ficc.parser.dict.DictionaryRow row : set.dictionary) {
            Object result = setCodeName.invoke(parser, row.code, row.name, "",
                    row.invalid ? "INVALID_CODE" : "");
            Map<String, Object> entry = new LinkedHashMap<String, Object>();
            entry.put("code", row.code);
            entry.put("return", String.valueOf(result));
            feed.add(entry);
        }
        feed.add(setCodeName.invoke(parser, "end", "end", "", ""));
        out.put("dictionaryFeed", feed);

        List<Object> results = new ArrayList<Object>();
        for (CaseSet.Case one : set.cases) {
            Map<String, Object> entry = new LinkedHashMap<String, Object>();
            entry.put("id", one.id);
            entry.put("text", one.text);
            entry.put("businessDate", one.businessDate);
            try {
                String working = String.valueOf(preProcess.invoke(parser, one.text));
                entry.put("preprocessed", working);
                List<Object> lines = new ArrayList<Object>();
                String[] split = working.split("\r\n|\n|\r");
                for (String line : split) {
                    Map<String, Object> lineEntry = new LinkedHashMap<String, Object>();
                    lineEntry.put("text", line);
                    lineEntry.put("raw", String.valueOf(parseBond.invoke(parser, line, "", "")));
                    lines.add(lineEntry);
                }
                entry.put("lineCount", split.length);
                entry.put("lines", lines);
            } catch (Exception e) {
                Throwable cause = e.getCause() == null ? e : e.getCause();
                entry.put("error", String.valueOf(cause));
            }
            results.add(entry);
        }
        out.put("cases", results);

        File parent = outFile.getAbsoluteFile().getParentFile();
        if (parent != null) {
            parent.mkdirs();
        }
        File temp = new File(outFile.getAbsolutePath() + ".tmp-" + runId);
        PrintWriter writer = new PrintWriter(new OutputStreamWriter(new FileOutputStream(temp), "UTF-8"));
        try {
            writer.println(Json.write(out));
        } finally {
            writer.close();
        }
        try {
            Files.move(temp.toPath(), outFile.toPath(), StandardCopyOption.REPLACE_EXISTING,
                    StandardCopyOption.ATOMIC_MOVE);
        } catch (java.nio.file.AtomicMoveNotSupportedException notAtomic) {
            Files.move(temp.toPath(), outFile.toPath(), StandardCopyOption.REPLACE_EXISTING);
        }
        System.out.println("BASELINE_OK runId=" + runId + " out=" + outFile.getAbsolutePath()
                + " fingerprint=" + out.get("suiteFingerprint"));
    }

    /**
     * Records the file the wrapper actually loads, not the path that was passed in. A path that
     * cannot be resolved or hashed aborts the run: a baseline without artifact identity is useless.
     */
    private static void addNativeArtifact(Map<String, Object> artifacts, String nativePath,
                                          String workDir) {
        if (nativePath == null) {
            return;
        }
        NativeArtifact resolved = NativeArtifact.resolve(nativePath, workDir);
        Map<String, Object> entry = new LinkedHashMap<String, Object>();
        entry.put("path", resolved.loadedFile.getAbsolutePath());
        entry.put("sha256", resolved.sha256);
        entry.put("requested", resolved.requested.getAbsolutePath());
        entry.put("loadDirectory", resolved.loadDirectory.getAbsolutePath());
        artifacts.put("nativeLibrary", entry);
    }

    private static void addArtifact(Map<String, Object> artifacts, String key, String path) {
        if (path == null) {
            return;
        }
        File file = new File(path).getAbsoluteFile();
        if (!file.isFile() || !file.canRead()) {
            throw new IllegalArgumentException(key + " is missing or unreadable: " + file);
        }
        String hash = Fingerprint.sha256(file);
        if (hash == null) {
            throw new IllegalArgumentException("cannot hash " + key + " " + file);
        }
        Map<String, Object> entry = new LinkedHashMap<String, Object>();
        entry.put("path", file.getAbsolutePath());
        entry.put("sha256", hash);
        artifacts.put(key, entry);
    }

    private static Map<String, String> options(String[] args) {
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
