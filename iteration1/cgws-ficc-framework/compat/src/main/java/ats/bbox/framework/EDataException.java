package ats.bbox.framework;

public class EDataException extends EException {
    private IComponent component;
    private int code;

    public EDataException(Throwable cause, IComponent component) {
        super(cause);
        this.component = component;
    }

    public EDataException(Throwable cause) {
        super(cause);
    }

    public EDataException(String message) {
        super(message);
    }

    public EDataException(String message, Throwable cause) {
        super(message, cause);
    }

    public EDataException(IComponent component) {
        this.component = component;
    }

    public EDataException(String message, IComponent component) {
        super(message);
        this.component = component;
    }

    public void setCode(int code) {
        this.code = code;
    }

    public int getCode() {
        return code;
    }

    public IComponent getComponent() {
        return component;
    }
}
