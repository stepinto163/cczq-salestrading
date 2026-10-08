package ats.bbox.framework;

import ats.bbox.framework.inputstream.IInputStream;
import ats.bbox.framework.outputstream.IOutputStream;
import ats.bbox.framework.resource.IResource;
import ats.bbox.framework.stream.IStream;

import cgws.ficc.framework.LifecycleActions;
import cgws.ficc.framework.compat.DeferredStreams;

import org.apache.commons.configuration.Configuration;

import java.util.ArrayList;
import java.util.Collections;
import java.util.IdentityHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

/** Legacy API backed by an explicit lifecycle, without background polling or a DI container. */
public class ServerImpl implements IServer {
    private final Configuration configuration;
    private final StreamEngine engine = new StreamEngine();
    private final List<IResource> resources = new ArrayList<>();
    private final Set<ILifecycle> initialized = Collections.newSetFromMap(new IdentityHashMap<>());
    private volatile STATUS status = STATUS.CREATED;
    private DeferredStreams deferred;
    private Thread keepAlive;

    public ServerImpl(Configuration configuration) {
        this.configuration = configuration;
    }

    public synchronized void init() throws LifecycleException {
        if (status != STATUS.CREATED) {
            throw new LifecycleException("Server has already been initialized");
        }
        try {
            String resourceFile = configuration.getString("resource.file");
            if (resourceFile != null) {
                for (IComponent component :
                        ComponentBuilder.getComponents(resourceFile, null, IResource.class)) {
                    IResource resource = (IResource) component;
                    initialize(resource);
                    transition(resource, "STARTING", "STARTED", resource::start);
                    // Legacy lookup exposes each resource only after its start has completed.
                    resources.add(resource);
                }
            }
            engine.load(configuration.getString("streams.file", "./control/streams.xml"));
            for (ILifecycle stream : streamOrder()) {
                initialize(stream);
            }
            deferred = new DeferredStreams(configuration, engine);
            status = STATUS.INITED;
        } catch (Exception | LinkageError error) {
            cleanupAfterFailure(error);
            throw new LifecycleException(error);
        }
    }

    private void initialize(ILifecycle component) throws Exception {
        // Track before calling init: a partially initialized component also needs cleanup.
        initialized.add(component);
        transition(component, "INITIALIZING", "INITIALIZED", component::init);
    }

    private void transition(
            ILifecycle component, String before, String after, LifecycleActions.Action action)
            throws Exception {
        LifecycleActions.run(
                getServerName(),
                ((IComponent) component).getName(),
                component.getClass().getName(),
                before,
                after,
                action);
    }

    public synchronized void start() throws LifecycleException {
        if (status == STATUS.STARTED) {
            return;
        }
        if (status != STATUS.INITED) {
            throw new LifecycleException("Server must be initialized before start");
        }
        try {
            for (ILifecycle stream : streamOrder()) {
                if (!deferred.contains((IComponent) stream)) {
                    transition(stream, "STARTING", "STARTED", stream::start);
                }
            }
            status = STATUS.STARTED;
            keepAlive =
                    new Thread(
                            () -> {
                                synchronized (ServerImpl.this) {
                                    while (status == STATUS.STARTED) {
                                        try {
                                            ServerImpl.this.wait();
                                        } catch (InterruptedException interrupted) {
                                            Thread.currentThread().interrupt();
                                            return;
                                        }
                                    }
                                }
                            },
                            "ficc-server-lifecycle");
            keepAlive.start();
        } catch (Exception | LinkageError error) {
            cleanupAfterFailure(error);
            throw new LifecycleException(error);
        }
    }

    private List<ILifecycle> streamOrder() {
        List<ILifecycle> order = new ArrayList<>();
        order.addAll(engine.getOutputStreams());
        order.addAll(engine.getNormalStreams());
        order.addAll(engine.getInputStreams());
        return order;
    }

    private List<ILifecycle> shutdownOrder() {
        List<ILifecycle> order = new ArrayList<>();
        order.addAll(engine.getInputStreams());
        order.addAll(engine.getNormalStreams());
        order.addAll(engine.getOutputStreams());
        for (int index = resources.size() - 1; index >= 0; index--) {
            order.add(resources.get(index));
        }
        // Include the resource whose init/start failed before it became available for lookup.
        for (ILifecycle component : initialized) {
            if (!order.contains(component)) {
                order.add(0, component);
            }
        }
        return order;
    }

    private void cleanupAfterFailure(Throwable original) {
        try {
            dispose();
        } catch (LifecycleException cleanup) {
            original.addSuppressed(cleanup);
        }
    }

    public synchronized void stop() throws LifecycleException {
        dispose();
    }

    public synchronized void dispose() throws LifecycleException {
        if (status == STATUS.DISPOSED) {
            return;
        }
        Throwable failure = null;
        List<ILifecycle> order = shutdownOrder();
        for (String phase : new String[] {"stop", "dispose"}) {
            for (ILifecycle component : order) {
                if (!initialized.contains(component)) {
                    continue;
                }
                try {
                    if ("stop".equals(phase)) {
                        transition(component, "STOPPING", "STOPPED", component::stop);
                    } else {
                        transition(component, "DISPOSING", "DISPOSED", component::dispose);
                    }
                } catch (Exception | LinkageError error) {
                    if (failure == null) {
                        failure = error;
                    } else {
                        failure.addSuppressed(error);
                    }
                }
            }
        }
        initialized.clear();
        status = STATUS.DISPOSED;
        notifyAll();
        if (failure != null) {
            throw new LifecycleException(failure);
        }
    }

    public STATUS getStatus() {
        return status;
    }

    public String getServerName() {
        return Consts.SYS_NAME;
    }

    public int getTraceLogLevel() {
        return configuration.getInt("trace.level", -1);
    }

    public IStreamable getStream(String name) {
        return engine.getStream(name);
    }

    public Map<String, IStreamable> getStreams() {
        return engine.getAllStreams();
    }

    public IResource getResource(String name) {
        for (IResource resource : resources) {
            if (resource.getName().equals(name)) {
                return resource;
            }
        }
        return null;
    }

    public List<IResource> getResources() {
        return resources;
    }

    public List<IInputStream> getInputStreams() {
        return engine.getInputStreams();
    }

    public List<IOutputStream> getOutputStreams() {
        return engine.getOutputStreams();
    }

    public List<IStream> getNormalStreams() {
        return engine.getNormalStreams();
    }

    public String[] getUnstartList(String owner) {
        return deferred == null ? new String[0] : deferred.forOwner(owner);
    }
}
