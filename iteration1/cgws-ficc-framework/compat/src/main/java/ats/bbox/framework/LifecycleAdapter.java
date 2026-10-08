package ats.bbox.framework;

public class LifecycleAdapter implements ILifecycle {
    private volatile STATUS status = STATUS.CREATED;

    public void init() throws LifecycleException {
        status = STATUS.INITED;
    }

    public void start() throws LifecycleException {
        status = STATUS.STARTED;
    }

    public void stop() throws LifecycleException {
        status = STATUS.STOPED;
    }

    public void dispose() throws LifecycleException {
        status = STATUS.DISPOSED;
    }

    public STATUS getStatus() {
        return status;
    }
}
