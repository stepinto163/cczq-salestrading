package cgws.ficc.parser.model;

/** Structured problem report; never swallowed, always attached to the response. */
public class Diagnostic {

    public final String code;
    public final String message;

    public Diagnostic(String code, String message) {
        this.code = code;
        this.message = message;
    }

    @Override
    public String toString() {
        return code + ": " + message;
    }
}
