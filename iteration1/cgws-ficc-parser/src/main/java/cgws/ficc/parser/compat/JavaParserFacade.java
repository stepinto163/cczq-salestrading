package cgws.ficc.parser.compat;

import cgws.ficc.parser.dict.BondDictionary;
import cgws.ficc.parser.dict.DictionaryRow;
import cgws.ficc.parser.dict.DictionaryStore;
import cgws.ficc.parser.model.Diagnostic;
import cgws.ficc.parser.model.ParseResponse;
import cgws.ficc.parser.parse.LegacyResultCodec;
import cgws.ficc.parser.parse.ParserCore;
import cgws.ficc.parser.parse.MoneyMarketFields;
import cgws.ficc.parser.text.TextNormalizer;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.atomic.AtomicLong;

/**
 * Experimental facade with the same method names the service uses today
 * ({@code com.sumscope.Parser} / {@code com.sumscope.ParserApi}), backed by pure Java.
 *
 * <p>Switching ParserSvr to this class removes ParseLib/SO loading; the legacy class stays
 * available for the comparison runner and as a rollback path, but nothing in Java mode calls it.
 */
public final class JavaParserFacade {

    public static final String START = "start";
    public static final String END = "end";
    public static final String INVALID_CODE = "INVALID_CODE";

    private static volatile JavaParserFacade instance;

    private final DictionaryStore store = new DictionaryStore();
    private final AtomicLong versionSeq = new AtomicLong();
    private final List<Diagnostic> diagnostics = new ArrayList<Diagnostic>();
    private final Map<String, String> pendingSeen = new HashMap<String, String>();
    private volatile BondDictionary.Builder pending;
    private volatile int pendingRows;

    private JavaParserFacade() {
    }

    public static JavaParserFacade getInstance() {
        if (instance == null) {
            synchronized (JavaParserFacade.class) {
                if (instance == null) {
                    instance = new JavaParserFacade();
                }
            }
        }
        return instance;
    }

    /**
     * Isolated instance with its own dictionary store; used by tests and by callers that must not
     * share the process wide singleton state.
     */
    public static JavaParserFacade newInstance() {
        return new JavaParserFacade();
    }

    public DictionaryStore store() {
        return store;
    }

    // ------------------------------------------------------------------ dictionary lifecycle

    /**
     * Legacy shaped dictionary entry point.
     *
     * @return {@code "code:name"} for a new entry, {@code ""} for an unchanged duplicate,
     *         {@code "start:start"} / {@code "end:end"} for the control markers.
     */
    public synchronized String SetCodeName(String code, String name, String group, String flags) {
        String normalized = BondDictionary.normalize(code);
        if (START.equals(normalized)) {
            pending = new BondDictionary.Builder();
            pendingSeen.clear();
            pendingRows = 0;
            return START + ":" + START;
        }
        if (END.equals(normalized)) {
            if (pending == null) {
                addDiagnostic("DICTIONARY_END_WITHOUT_START", "end marker without start");
                return "";
            }
            BondDictionary snapshot = pending.build("v" + versionSeq.incrementAndGet());
            pending = null;
            String previous = store.publish(snapshot);
            addDiagnostic("DICTIONARY_PUBLISHED",
                    "version=" + snapshot.version() + " rows=" + snapshot.size() + " previous=" + previous);
            return END + ":" + END;
        }
        if (normalized.isEmpty()) {
            return "";
        }
        BondDictionary.Builder target = pending != null ? pending : fallbackBuilder();
        boolean invalid = INVALID_CODE.equals(flags);
        String signature = (name == null ? "" : name) + "|" + invalid;
        String previousSignature = pendingSeen.put(normalized, signature);
        boolean published = store.isReady() && store.current().containsCode(normalized);
        String publishedName = published ? store.current().nameOf(normalized) : null;
        boolean duplicate = (previousSignature != null && previousSignature.equals(signature))
                || (previousSignature == null && published && equals(publishedName, name)
                    && store.current().isInvalid(normalized) == invalid);
        target.put(normalized, name, null, invalid);
        pendingRows++;
        return duplicate ? "" : normalized + ":" + (name == null ? "" : name);
    }

    /** Bulk load used by the service adapter; publishes one snapshot and records the outcome. */
    public synchronized BondDictionary loadRows(List<DictionaryRow> rows) {
        BondDictionary.Builder builder = new BondDictionary.Builder();
        for (DictionaryRow row : rows) {
            builder.put(row.code, row.name, row.key, row.invalid);
        }
        BondDictionary snapshot = builder.build("v" + versionSeq.incrementAndGet());
        store.publish(snapshot);
        addDiagnostic("DICTIONARY_PUBLISHED",
                "version=" + snapshot.version() + " rows=" + snapshot.size());
        return snapshot;
    }

    /** Refresh that keeps the previous snapshot when the new one cannot be built. */
    public synchronized boolean refresh(List<DictionaryRow> rows, String failureMessage) {
        if (rows == null || failureMessage != null) {
            store.recordFailure(failureMessage == null ? "refresh returned no rows" : failureMessage);
            addDiagnostic("DICTIONARY_REFRESH_FAILED", store.lastLoadError());
            return false;
        }
        loadRows(rows);
        return true;
    }

    public boolean isReady() {
        return store.isReady();
    }

    public boolean IsCode(String code) {
        BondDictionary dict = store.current();
        return dict != null && dict.containsCode(code);
    }

    /** No-op kept for source compatibility: the Java path uses an immutable snapshot instead. */
    public void PrepareParse() {
        // intentionally empty
    }

    public String PreProcess(String text) {
        return TextNormalizer.preProcess(text);
    }

    /**
     * Returns the legacy format string for one line, {@code ""} when nothing is recognised.
     * Without a usable dictionary it throws, so a missing dictionary cannot be reported as a miss.
     */
    public String ParseBond(String text, String unused1, String unused2) {
        ParseResponse response = Parse(text, null);
        if (response.failed) {
            throw new IllegalStateException(response.diagnosticsAsString());
        }
        if (response.quotes.isEmpty()) {
            return "";
        }
        return response.rawResults.get(0);
    }

    /**
     * Structured variant used by the service adapter and by the comparison runner.
     * A missing dictionary is a failure, never an empty result that could be read as
     * "nothing recognised".
     */
    public ParseResponse Parse(String text, String businessDate) {
        if (!isReady()) {
            ParseResponse failed = new ParseResponse();
            failed.fail("PARSER_NOT_READY",
                    "no usable dictionary published" + (store.lastLoadError() == null ? ""
                            : ": " + store.lastLoadError()));
            addDiagnostic("PARSER_NOT_READY", failed.diagnosticsAsString());
            return failed;
        }
        return ParserCore.parse(text, store.current(), businessDate, true);
    }

    // ------------------------------------------------------- legacy helpers still in the API

    /** Legacy money-market helper; distinct from a bond quote numeric price. */
    public String ParsePrice(String text) {
        return MoneyMarketFields.price(text);
    }

    /** Returns legacy tenor endpoints, including the 9999/10000 sentinels. */
    public String ParseTenor(String text) {
        return MoneyMarketFields.tenor(text);
    }

    /** Returns legacy amount endpoints in ten-thousand-yuan units. */
    public String ParseAmount(String text) {
        return MoneyMarketFields.amount(text);
    }

    /** Chinese numeral conversion: confirmed against the binary for 三千 -> 3000. */
    public String ReplaceNumber(String text) {
        if (text == null || text.isEmpty()) {
            return text;
        }
        boolean allNumerals = true;
        for (int i = 0; i < text.length(); i++) {
            if (ChineseNumbers.digitOf(text.charAt(i)) < 0 && ChineseNumbers.unitOf(text.charAt(i)) <= 0) {
                allNumerals = false;
                break;
            }
        }
        if (!allNumerals) {
            return text;
        }
        return String.valueOf(ChineseNumbers.toLong(text));
    }

    public List<Diagnostic> drainDiagnostics() {
        synchronized (this) {
            List<Diagnostic> copy = new ArrayList<Diagnostic>(diagnostics);
            diagnostics.clear();
            return copy;
        }
    }

    /** Compares our own rendering of a quote with a legacy string; used by the runner. */
    public static boolean sameLegacyString(String legacy, String candidate) {
        if (legacy == null) {
            return candidate == null || candidate.isEmpty();
        }
        return legacy.equals(candidate == null ? "" : candidate);
    }

    private void addDiagnostic(String code, String message) {
        synchronized (this) {
            diagnostics.add(new Diagnostic(code, message));
        }
    }

    private boolean equals(String a, String b) {
        return a == null ? b == null : a.equals(b);
    }

    private BondDictionary.Builder fallbackBuilder() {
        // SetCodeName without a start marker: keep working on the published snapshot so the
        // behaviour matches the legacy "append" path instead of throwing.
        BondDictionary.Builder builder = new BondDictionary.Builder();
        BondDictionary current = store.current();
        if (current != null) {
            for (String code : current.codes()) {
                builder.put(code, current.nameOf(code), current.keyOf(code), current.isInvalid(code));
            }
        }
        pending = builder;
        return builder;
    }

    /** Status line for service startup logs. */
    public String statusLine() {
        BondDictionary dict = store.current();
        return "JavaParserFacade status: ready=" + store.isReady()
                + " version=" + (dict == null ? "none" : dict.version())
                + " codes=" + (dict == null ? 0 : dict.size())
                + " header=" + LegacyResultCodec.HEADER
                + " pendingRows=" + pendingRows;
    }

    /** Diagnostics of the store itself (not ready / refresh failed) for startup logging. */
    public List<Diagnostic> storeDiagnostics() {
        return new ArrayList<Diagnostic>(store.statusDiagnostics());
    }
}
