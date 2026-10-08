package cgws.ficc.framework.compat;

import ats.bbox.framework.EDataException;
import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.inputstream.InputStreamAdapter;
import ats.bbox.framework.outputstream.OutputStreamAdapter;
import ats.bbox.framework.resource.ResourceAdapter;
import ats.bbox.framework.stream.StreamAdapter;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/** No JUnit dependency: the same bytecode is also loaded against the deployed legacy framework. */
public final class LifecycleFixtures {
    public static final List<String> EVENTS = new ArrayList<>();
    public static Map<String, Object> received;
    public static Thread deliveryThread;

    public static void reset() {
        EVENTS.clear();
        received = null;
        deliveryThread = null;
    }

    public static class Resource extends ResourceAdapter {
        public void init() throws LifecycleException {
            EVENTS.add(getName() + ":init");
            super.init();
            if (getConfig().getBoolean("failInit", false)) {
                throw new LifecycleException("injected initialization failure");
            }
        }

        public void start() throws LifecycleException {
            EVENTS.add(getName() + ":start");
            super.start();
            if (getConfig().getBoolean("failStart", false)) {
                throw new LifecycleException("injected startup failure");
            }
        }

        public void stop() throws LifecycleException {
            EVENTS.add(getName() + ":stop");
            super.stop();
            if (getConfig().getBoolean("failStop", false)) {
                throw new LifecycleException("injected stop failure");
            }
        }

        public void dispose() throws LifecycleException {
            EVENTS.add(getName() + ":dispose");
            super.dispose();
        }
    }

    public static class Input extends InputStreamAdapter {
        public void init() throws LifecycleException {
            EVENTS.add(getName() + ":init");
            super.init();
        }

        public void start() throws LifecycleException {
            EVENTS.add(getName() + ":start");
            super.start();
        }

        public void stop() throws LifecycleException {
            EVENTS.add(getName() + ":stop");
            super.stop();
        }

        public void dispose() throws LifecycleException {
            EVENTS.add(getName() + ":dispose");
            super.dispose();
        }
    }

    public static class Output extends OutputStreamAdapter {
        public void init() throws LifecycleException {
            EVENTS.add(getName() + ":init");
            super.init();
        }

        public void start() throws LifecycleException {
            EVENTS.add(getName() + ":start");
            super.start();
        }

        public void stop() throws LifecycleException {
            EVENTS.add(getName() + ":stop");
            super.stop();
        }

        public void dispose() throws LifecycleException {
            EVENTS.add(getName() + ":dispose");
            super.dispose();
        }

        public void onData(Map<String, Object> data) throws EDataException {
            received = data;
            deliveryThread = Thread.currentThread();
        }
    }

    public static class Stream extends StreamAdapter {
        public void init() throws LifecycleException {
            EVENTS.add(getName() + ":init");
            super.init();
        }

        public void start() throws LifecycleException {
            EVENTS.add(getName() + ":start");
            super.start();
        }

        public void stop() throws LifecycleException {
            EVENTS.add(getName() + ":stop");
            super.stop();
        }

        public void dispose() throws LifecycleException {
            EVENTS.add(getName() + ":dispose");
            super.dispose();
        }
    }
}
