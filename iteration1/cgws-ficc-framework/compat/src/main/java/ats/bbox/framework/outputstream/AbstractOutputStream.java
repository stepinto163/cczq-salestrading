package ats.bbox.framework.outputstream;

import ats.bbox.framework.*;

import java.util.Map;

public abstract class AbstractOutputStream extends SimpleComponent implements IOutputStream {
    private final ILifecycle lifecycle = new LifecycleAdapter();

    public ILifecycle getLifecycle() {
        return lifecycle;
    }

    public STATUS getStatus() {
        return lifecycle.getStatus();
    }

    public void onData(Map<String, Object> data) throws EDataException {}
}
