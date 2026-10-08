package ats.bbox.framework.stream;

import ats.bbox.framework.EDataException;
import ats.bbox.framework.LifecycleException;

import java.util.Map;

public class StreamAdapter extends AbstractStream {
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

    public void onData(Map<String, Object> data) throws EDataException {
        if (getStatus() == STATUS.STARTED) {
            send(data);
        }
    }
}
