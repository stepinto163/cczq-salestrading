package cgws.ficc.framework.dps;

import org.I0Itec.zkclient.IZkChildListener;
import org.I0Itec.zkclient.IZkDataListener;
import org.I0Itec.zkclient.IZkStateListener;
import org.I0Itec.zkclient.ZkClient;
import org.I0Itec.zkclient.ZkConnection;
import org.I0Itec.zkclient.exception.ZkNoNodeException;
import org.I0Itec.zkclient.exception.ZkNodeExistsException;
import org.I0Itec.zkclient.serialize.ZkSerializer;
import org.apache.zookeeper.CreateMode;
import org.apache.zookeeper.Watcher.Event.KeeperState;
import org.apache.zookeeper.data.Stat;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.function.Consumer;

/** Owns one ZooKeeper session. Uses the deployed STC registry path and UTF-8 payload unchanged. */
public final class ZookeeperDiscovery implements AutoCloseable {
    private static final Logger LOGGER = LoggerFactory.getLogger(ZookeeperDiscovery.class);
    private static final String ROOT = "/MDTService";
    private final ZkConnection connection;
    private final ZkClient client;
    private final List<Registration> registrations = new CopyOnWriteArrayList<>();
    private final List<ServiceWatch> watches = new CopyOnWriteArrayList<>();
    private final Consumer<Throwable> failureHandler;
    private volatile boolean closed;
    private volatile boolean connected = true;

    public ZookeeperDiscovery(
            String hosts,
            int sessionMillis,
            int connectMillis,
            String username,
            String password,
            Consumer<Throwable> failureHandler) {
        this.failureHandler = failureHandler;
        connection = new ZkConnection(hosts, sessionMillis);
        client = new ZkClient(connection, connectMillis, new Utf8Serializer(), connectMillis);
        if (username != null && !username.isEmpty()) {
            client.addAuthInfo(
                    "digest", (username + ":" + password).getBytes(StandardCharsets.UTF_8));
        }
        client.subscribeStateChanges(
                new IZkStateListener() {
                    @Override
                    public void handleStateChanged(KeeperState state) {
                        LOGGER.info("DISCOVERY state={}", state);
                        connected = state == KeeperState.SyncConnected;
                        if (state == KeeperState.Disconnected
                                || state == KeeperState.Expired
                                || state == KeeperState.AuthFailed) {
                            // A standby claim is a lease, not evidence that this process may remain
                            // active.
                            for (Registration registration : registrations) {
                                registration.leaseLost();
                            }
                        }
                        if (connected) {
                            guarded(
                                    () -> {
                                        for (Registration registration : registrations) {
                                            registration.restore();
                                        }
                                    });
                        }
                    }

                    @Override
                    public void handleNewSession() {
                        connected = true;
                        guarded(
                                () -> {
                                    for (Registration registration : registrations) {
                                        registration.restore();
                                    }
                                    for (ServiceWatch watch : watches) {
                                        watch.refresh();
                                    }
                                });
                    }

                    @Override
                    public void handleSessionEstablishmentError(Throwable error) {
                        fail(error);
                    }
                });
    }

    public Registration register(
            String service, ServiceEndpoint endpoint, boolean standby, Runnable lostStandbyLease) {
        ensureOpen();
        return register(service, endpoint, standby, false, lostStandbyLease);
    }

    /** Waits without starting application resources while the legacy fixed claim is occupied. */
    public Registration standby(String service, ServiceEndpoint endpoint, Runnable lostLease) {
        return register(service, endpoint, true, true, lostLease);
    }

    private Registration register(
            String service,
            ServiceEndpoint endpoint,
            boolean standby,
            boolean waitForClaim,
            Runnable lostStandbyLease) {
        ensureOpen();
        Registration registration =
                new Registration(service, endpoint, standby, waitForClaim, lostStandbyLease);
        registrations.add(registration);
        try {
            if (waitForClaim) {
                ensurePath(ROOT, "");
                ensurePath(registration.parent, "STANDBY");
                // Subscribe before attempting create: no lost wakeup between exists and watch.
                client.subscribeChildChanges(registration.parent, registration.claimListener);
            }
            registration.restore();
            return registration;
        } catch (RuntimeException error) {
            registration.close();
            throw error;
        }
    }

    public AutoCloseable watch(String service, Consumer<List<ServiceEndpoint>> listener) {
        ensureOpen();
        ServiceWatch watch = new ServiceWatch(service, listener);
        watches.add(watch);
        client.subscribeChildChanges(watch.path, watch.childListener);
        try {
            watch.refresh();
            return watch;
        } catch (RuntimeException error) {
            watch.close();
            throw error;
        }
    }

    private void ensureOpen() {
        if (closed) {
            throw new IllegalStateException("Discovery is closed");
        }
    }

    private static String path(String service) {
        String name = service.startsWith("SERVER.") ? service.substring(7) : service;
        if (name.isEmpty() || name.indexOf('/') >= 0) {
            throw new IllegalArgumentException("Invalid service name");
        }
        return ROOT + "/" + name;
    }

    private void ensurePath(String path, String value) {
        try {
            client.createPersistent(path, value);
        } catch (ZkNodeExistsException exists) {
            /* Another service created the shared parent. */
        }
    }

    private void guarded(Runnable action) {
        if (closed) {
            return;
        }
        try {
            action.run();
        } catch (RuntimeException error) {
            fail(error);
        }
    }

    private void fail(Throwable error) {
        if (!closed) {
            LOGGER.error("DISCOVERY state=FAILED", error);
            failureHandler.accept(error);
        }
    }

    @Override
    public void close() {
        if (closed) {
            return;
        }
        closed = true;
        for (Registration registration : registrations) {
            registration.cancelWait();
        }
        // Session close removes only our ephemeral nodes. Never delete another process's claim.
        client.unsubscribeAll();
        client.close();
        registrations.clear();
        watches.clear();
    }

    public final class Registration implements AutoCloseable {
        private final String parent;
        private final String prefix;
        private ServiceEndpoint endpoint;
        private final boolean standby;
        private final boolean waitForClaim;
        private final IZkChildListener claimListener = (path, children) -> guarded(this::restore);
        private final Runnable lostStandbyLease;
        private volatile String node;
        private boolean stopped;
        private boolean fenced;

        private Registration(
                String service,
                ServiceEndpoint endpoint,
                boolean standby,
                boolean waitForClaim,
                Runnable lostStandbyLease) {
            parent = path(service);
            prefix = parent + parent.substring(parent.lastIndexOf('/'));
            this.endpoint = endpoint;
            this.standby = standby;
            this.waitForClaim = waitForClaim;
            this.lostStandbyLease = lostStandbyLease;
        }

        public String getNodePath() {
            return node;
        }

        public synchronized void awaitOwnership() throws InterruptedException {
            while (node == null && !stopped && !fenced && !closed) {
                wait();
            }
            requireOwnership();
        }

        private void requireOwnership() {
            if (stopped || fenced || closed || node == null || (standby && !connected)) {
                throw new IllegalStateException("Registration no longer owns a service lease");
            }
        }

        private synchronized void cancelWait() {
            stopped = true;
            notifyAll();
        }

        public synchronized void markReady() {
            requireOwnership();
            Stat stat = new Stat();
            client.readData(node, stat);
            if (stat.getEphemeralOwner() != connection.getZookeeper().getSessionId()) {
                throw new IllegalStateException("Service lease belongs to another session");
            }
            ServiceEndpoint ready =
                    new ServiceEndpoint(endpoint.getHost(), endpoint.getPort(), true);
            client.writeData(node, ready.encode(), stat.getVersion());
            endpoint = ready;
        }

        private synchronized void restore() {
            if (closed || stopped || fenced || !connected) {
                return;
            }
            if (standby && node != null) {
                // Never re-acquire a lost claim under a still-running application.
                Stat ownership = new Stat();
                try {
                    client.readData(node, ownership);
                } catch (ZkNoNodeException removed) {
                    leaseLost();
                    return;
                }
                if (ownership.getEphemeralOwner() != connection.getZookeeper().getSessionId()) {
                    leaseLost();
                }
                return;
            }
            ensurePath(ROOT, "");
            ensurePath(parent, standby ? "STANDBY" : "ALL");
            // Resolve an uncertain create outcome using ownership, not just an equal host/port.
            for (String child : client.getChildren(parent)) {
                String candidate = parent + "/" + child;
                Stat stat = new Stat();
                String value;
                try {
                    value = client.readData(candidate, stat);
                } catch (ZkNoNodeException removed) {
                    continue;
                }
                if (stat.getEphemeralOwner() == connection.getZookeeper().getSessionId()
                        && endpoint.encode().equals(value)) {
                    node = candidate;
                    notifyAll();
                    return;
                }
            }
            try {
                node =
                        client.create(
                                prefix,
                                endpoint.encode(),
                                standby ? CreateMode.EPHEMERAL : CreateMode.EPHEMERAL_SEQUENTIAL);
            } catch (ZkNodeExistsException occupied) {
                if (!waitForClaim) {
                    throw occupied;
                }
                return;
            }
            notifyAll();
            client.subscribeDataChanges(
                    node,
                    new IZkDataListener() {
                        @Override
                        public void handleDataChange(String path, Object value) {}

                        @Override
                        public void handleDataDeleted(String path) {
                            if (standby) {
                                leaseLost();
                            } else {
                                guarded(() -> restore());
                            }
                        }
                    });
            LOGGER.info("DISCOVERY state=REGISTERED node={} target={}", node, endpoint);
        }

        private void leaseLost() {
            Runnable callback = null;
            synchronized (this) {
                if (standby && node != null && !fenced && !stopped && !closed) {
                    fenced = true;
                    notifyAll();
                    callback = lostStandbyLease;
                }
            }
            if (callback != null) {
                callback.run();
            }
        }

        @Override
        public synchronized void close() {
            if (stopped) {
                return;
            }
            stopped = true;
            notifyAll();
            registrations.remove(this);
            if (waitForClaim && !closed) {
                client.unsubscribeChildChanges(parent, claimListener);
            }
            if (node != null && !closed) {
                Stat stat = new Stat();
                try {
                    client.readData(node, stat);
                    if (stat.getEphemeralOwner() == connection.getZookeeper().getSessionId()) {
                        client.delete(node, stat.getVersion());
                    }
                } catch (ZkNoNodeException removed) {
                    /* Already expired. */
                }
            }
        }
    }

    private final class ServiceWatch implements AutoCloseable {
        private final String path;
        private final Consumer<List<ServiceEndpoint>> listener;
        private final Map<String, IZkDataListener> dataListeners = new LinkedHashMap<>();
        private final IZkChildListener childListener = (path, children) -> guarded(this::refresh);
        private List<ServiceEndpoint> previous;
        private boolean stopped;

        ServiceWatch(String service, Consumer<List<ServiceEndpoint>> listener) {
            this.path = path(service);
            this.listener = listener;
        }

        synchronized void refresh() {
            if (stopped || closed) {
                return;
            }
            List<String> children;
            try {
                children = client.getChildren(path);
            } catch (ZkNoNodeException absent) {
                children = Collections.emptyList();
            }
            Collections.sort(children);
            Map<String, ServiceEndpoint> endpoints = new LinkedHashMap<>();
            for (String child : children) {
                String node = path + "/" + child;
                if (!dataListeners.containsKey(node)) {
                    IZkDataListener dataListener =
                            new IZkDataListener() {
                                @Override
                                public void handleDataChange(String path, Object value) {
                                    guarded(() -> refresh());
                                }

                                @Override
                                public void handleDataDeleted(String path) {
                                    guarded(() -> refresh());
                                }
                            };
                    client.subscribeDataChanges(node, dataListener);
                    dataListeners.put(node, dataListener);
                }
                String data = client.readData(node, true);
                if (data != null) {
                    ServiceEndpoint endpoint = ServiceEndpoint.decode(data);
                    if (endpoint.isReady()) {
                        endpoints.put(endpoint.toString(), endpoint);
                    }
                }
            }
            for (String node : new ArrayList<>(dataListeners.keySet())) {
                if (!children.contains(node.substring(path.length() + 1))) {
                    client.unsubscribeDataChanges(node, dataListeners.remove(node));
                }
            }
            List<ServiceEndpoint> current =
                    Collections.unmodifiableList(new ArrayList<>(endpoints.values()));
            if (!current.equals(previous)) {
                listener.accept(current);
                previous = current;
            }
        }

        @Override
        public synchronized void close() {
            stopped = true;
            watches.remove(this);
            client.unsubscribeChildChanges(path, childListener);
            dataListeners.forEach(client::unsubscribeDataChanges);
            dataListeners.clear();
        }
    }

    static final class Utf8Serializer implements ZkSerializer {
        @Override
        public byte[] serialize(Object value) {
            return ((String) value).getBytes(StandardCharsets.UTF_8);
        }

        @Override
        public Object deserialize(byte[] value) {
            return value == null ? null : new String(value, StandardCharsets.UTF_8);
        }
    }
}
