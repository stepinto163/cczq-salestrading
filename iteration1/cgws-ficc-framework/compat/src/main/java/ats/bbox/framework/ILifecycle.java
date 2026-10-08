package ats.bbox.framework;

public interface ILifecycle {
    // Keep the legacy spelling: existing compiled services reference these enum constants.
    enum STATUS {
        CREATED,
        INITED,
        STARTED,
        STOPED,
        DISPOSED
    }

    void init() throws LifecycleException;

    void start() throws LifecycleException;

    void stop() throws LifecycleException;

    void dispose() throws LifecycleException;

    STATUS getStatus();
}
