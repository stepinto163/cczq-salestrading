package ats.bbox.framework.resource;

import ats.bbox.framework.ILifecycle;
import ats.bbox.framework.LifecycleAdapter;
import ats.bbox.framework.SimpleComponent;

public abstract class AbstractResource extends SimpleComponent implements IResource {
    private final ILifecycle lifecycle = new LifecycleAdapter();

    public ILifecycle getLifecycle() {
        return lifecycle;
    }

    public STATUS getStatus() {
        return lifecycle.getStatus();
    }
}
