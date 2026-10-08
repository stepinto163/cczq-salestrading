package cgws.ficc.framework.dps;

import com.gateway.connector.tcp.client.GateWayApi;
import com.gateway.connector.tcp.client.IEventListener;
import com.gateway.connector.tcp.client.IMessage;
import com.gateway.connector.tcp.client.IRspCallBack;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.Executor;
import java.util.function.Consumer;

/** One DPS endpoint. The SDK owns transport reconnect; this class owns subscription intent. */
public final class DpsPeer implements AutoCloseable {
    private static final Logger LOGGER = LoggerFactory.getLogger(DpsPeer.class);
    private final GateWayApi api = new GateWayApi();
    private final String server;
    private final Consumer<Boolean> connectionListener;
    private final Consumer<Throwable> failureListener;
    private final Executor callbackExecutor;
    private final List<Subscription> subscriptions = new ArrayList<>();
    private volatile boolean closed;
    private boolean notifiedOnline;

    public DpsPeer(
            String server,
            Executor callbackExecutor,
            Consumer<Boolean> connectionListener,
            Consumer<Throwable> failureListener) {
        this.server = server;
        this.callbackExecutor = callbackExecutor;
        this.connectionListener = connectionListener;
        this.failureListener = failureListener;
        api.setReconnectFlag(true);
        api.setEventlistener(
                event -> {
                    // Never block the SDK's network callback with subscribe/image requests.
                    synchronized (this) {
                        if (closed) {
                            return;
                        }
                        callbackExecutor.execute(() -> onEvent(event));
                    }
                });
    }

    public String connect(ServiceEndpoint endpoint, String username, String password) {
        ensureOpen();
        return api.connect(endpoint.getHost(), endpoint.getPort(), username, password, false, 1);
    }

    private synchronized void onEvent(int event) {
        if (closed) {
            return;
        }
        if (event == IEventListener.CONNECTION_CLOSED
                || event == IEventListener.CONNECTION_LOGOUT) {
            if (notifiedOnline) {
                notifiedOnline = false;
                connectionListener.accept(false);
            }
        } else if (event == IEventListener.CONNECTION_LOGIN_SUCCESS && api.isLogin()) {
            if (!notifiedOnline) {
                notifiedOnline = true;
                connectionListener.accept(true);
            }
            for (Subscription subscription : new ArrayList<>(subscriptions)) {
                install(subscription);
            }
        }
    }

    public boolean isOnline() {
        return !closed && api.isLogin();
    }

    public String request(String session, String topic, String content, long timeoutMillis)
            throws Exception {
        ensureOpen();
        return api.requestSync(session, server, topic, content, String.class, timeoutMillis);
    }

    public void requestAsync(
            String session, String topic, String content, long timeoutMillis, IRspCallBack callback)
            throws Exception {
        ensureOpen();
        api.requestAsync(server, topic, content, timeoutMillis, callback, session);
    }

    public synchronized void subscribe(
            String topic, String imageServer, IMessage<String> callback) {
        ensureOpen();
        for (Subscription subscription : subscriptions) {
            if (subscription.matches(topic, imageServer, callback)) {
                return;
            }
        }
        Subscription subscription = new Subscription(topic, imageServer, callback);
        subscriptions.add(subscription);
        if (api.isLogin()) {
            install(subscription);
        }
    }

    public synchronized void unsubscribe(
            String topic, String imageServer, IMessage<String> callback) throws Exception {
        ensureOpen();
        for (Subscription subscription : new ArrayList<>(subscriptions)) {
            if (subscription.matches(topic, imageServer, callback)) {
                // The SDK removes its reconnect intent before attempting the network request.
                // Skipping this while offline would resurrect the subscription on reconnect.
                api.unSubscribe(topic, callback, String.class);
                subscriptions.remove(subscription);
            }
        }
    }

    private void install(Subscription subscription) {
        // The unchanged SDK restores installed subscriptions after reconnect. Installing them
        // twice would duplicate snapshots; only offline/new intent needs installation here.
        if (subscription.installed) {
            return;
        }
        try {
            boolean success =
                    subscription.imageServer == null
                            ? api.subscribe(subscription.topic, subscription.callback, String.class)
                            : api.subscribe(
                                    subscription.imageServer,
                                    subscription.topic,
                                    subscription.callback,
                                    String.class);
            if (!success) {
                throw new IllegalStateException("DPS subscription rejected: " + subscription.topic);
            }
            subscription.installed = true;
            LOGGER.info(
                    "DPS operation=SUBSCRIBE server={} topic={} image={}",
                    server,
                    subscription.topic,
                    subscription.imageServer != null);
        } catch (Exception error) {
            LOGGER.error("DPS subscription failed: " + server + "/" + subscription.topic, error);
            failureListener.accept(error);
        }
    }

    private void ensureOpen() {
        if (closed) {
            throw new IllegalStateException("DPS peer is closed");
        }
    }

    @Override
    public synchronized void close() {
        if (closed) {
            return;
        }
        closed = true;
        api.setReconnectFlag(false);
        api.setLogOut(true);
        api.disconnect();
        subscriptions.clear();
        if (notifiedOnline) {
            notifiedOnline = false;
            connectionListener.accept(false);
        }
    }

    private static final class Subscription {
        final String topic;
        final String imageServer;
        final IMessage<String> callback;
        boolean installed;

        Subscription(String topic, String imageServer, IMessage<String> callback) {
            this.topic = topic;
            this.imageServer = imageServer;
            this.callback = callback;
        }

        boolean matches(String topic, String imageServer, IMessage<String> callback) {
            return this.topic.equals(topic)
                    && java.util.Objects.equals(this.imageServer, imageServer)
                    && this.callback == callback;
        }
    }
}
