package ats.bbox.framework.inputstream;

import ats.bbox.framework.LifecycleException;

public class InputStreamAdapter extends AbstractInputStream {
    public void init() throws LifecycleException {
        getLifecycle().init();
    }

    public void start() throws LifecycleException {
        getLifecycle().start();
    }

    public void stop() throws LifecycleException {
        getLifecycle().stop();
    }

    public void dispose() throws LifecycleException {
        getLifecycle().dispose();
    }
}
