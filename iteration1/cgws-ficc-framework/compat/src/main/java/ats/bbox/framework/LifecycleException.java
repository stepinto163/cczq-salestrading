package ats.bbox.framework;

public class LifecycleException extends EException {
    public LifecycleException() {}

    public LifecycleException(String message) {
        super(message);
    }

    public LifecycleException(Throwable cause) {
        super(cause);
    }
}
