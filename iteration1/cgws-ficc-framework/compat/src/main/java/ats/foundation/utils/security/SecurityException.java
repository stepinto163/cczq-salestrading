// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.security;

public class SecurityException extends Exception {
    private static final long serialVersionUID = -6551957240977915081L;

    public SecurityException() {}

    public SecurityException(Exception e) {
        super(e);
    }

    public SecurityException(String msg, Exception e) {
        super(msg, e);
    }
}
