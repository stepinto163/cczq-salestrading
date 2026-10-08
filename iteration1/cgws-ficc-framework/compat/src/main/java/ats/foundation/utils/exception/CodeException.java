// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.exception;

public class CodeException extends Exception {
    private static final long serialVersionUID = -10000000000000L;
    protected String code = "6003";
    protected String message = "UnKnown";

    public CodeException() {}

    public CodeException(String message) {
        super(message);
        this.message = message;
    }

    public CodeException(String code, String message) {
        super(message);
        this.code = code;
        this.message = message;
    }

    public String code() {
        return this.code;
    }

    @Override
    public String getMessage() {
        return "[" + this.code() + "] " + this.message;
    }

    @Override
    public String toString() {
        StringBuffer msg = new StringBuffer("");
        msg.append(this.getMessage())
                .append(" : \n    Exception by: { \n        by ")
                .append(this.getLocalizedMessage())
                .append("\n");
        StackTraceElement[] se = this.getStackTrace();
        for (int i = 0; i < se.length; ++i) {
            msg.append("        at ").append(se[i].toString()).append("\n");
        }
        msg.append("    }\n");
        return msg.toString();
    }
}
