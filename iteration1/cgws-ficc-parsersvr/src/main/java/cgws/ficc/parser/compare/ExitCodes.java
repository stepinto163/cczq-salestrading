package cgws.ficc.parser.compare;

import java.io.File;

/** Exit codes of {@link CompareRunner}; automation must be able to distinguish them. */
public final class ExitCodes {

    /** Every selected case matched the baseline. */
    public static final int MATCH = 0;
    /** At least one case differs from the baseline, or one side errored. */
    public static final int DIFF = 1;
    /** Input or environment problem: unknown case, stale baseline, unusable options. */
    public static final int INPUT_ERROR = 2;
    /** Baseline was not executed: missing artifact, timeout, no baseline produced. */
    public static final int NOT_RUN = 3;

    private ExitCodes() {
    }

    public static String describe(int code) {
        switch (code) {
            case MATCH:
                return "MATCH";
            case DIFF:
                return "DIFF";
            case INPUT_ERROR:
                return "INPUT_ERROR";
            case NOT_RUN:
                return "NOT_RUN";
            default:
                return "UNKNOWN(" + code + ")";
        }
    }
}
