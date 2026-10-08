package ats.bbox.framework;

public class EException extends Exception {
    public EException() {}

    public EException(String message) {
        super(message);
    }

    public EException(Throwable cause) {
        super(cause);
    }

    public EException(String message, Throwable cause) {
        super(message, cause);
    }
}
