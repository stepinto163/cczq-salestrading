package ats.bbox.framework.outputstream;

import ats.bbox.framework.LifecycleException;

public class OutputStreamAdapter extends AbstractOutputStream {
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
