package cgws.ficc.parser.model;

import java.util.ArrayList;
import java.util.List;

/** Result of one {@code ParserCore.parse} call: quotes, diagnostics and the raw legacy strings. */
public class ParseResponse {

    public final List<QuoteInfo> quotes = new ArrayList<QuoteInfo>();
    public final List<Diagnostic> diagnostics = new ArrayList<Diagnostic>();
    /** Working text after preprocessing; part of the comparison boundary. */
    public String preprocessed;
    /** Legacy format string per line of the working text; empty string when a line yields nothing. */
    public final List<String> lineResults = new ArrayList<String>();
    /** Dictionary version the quotes were produced with (for logs and comparison). */
    public String dictionaryVersion;
    /**
     * True when the parse could not be performed: no usable dictionary, empty input.
     * A failed response carries no quotes, so callers cannot mistake it for "nothing recognised".
     */
    public boolean failed;
    /** Raw legacy-format strings, one per recognised quote (subset of {@link #lineResults}). */
    public final List<String> rawResults = new ArrayList<String>();

    public void addDiagnostic(String code, String message) {
        diagnostics.add(new Diagnostic(code, message));
    }

    public boolean hasDiagnostics() {
        return !diagnostics.isEmpty();
    }

    public void fail(String code, String message) {
        failed = true;
        addDiagnostic(code, message);
    }

    public String diagnosticsAsString() {
        StringBuilder b = new StringBuilder();
        for (Diagnostic d : diagnostics) {
            if (b.length() > 0) {
                b.append(" | ");
            }
            b.append(d.toString());
        }
        return b.toString();
    }
}
