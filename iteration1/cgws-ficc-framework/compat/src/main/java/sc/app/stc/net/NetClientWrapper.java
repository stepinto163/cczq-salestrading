package sc.app.stc.net;

import ats.app.api.BaseApi.IMessage;
import ats.app.ym.ceda.ICedaConnectionListener;
import ats.app.ym.ceda.OnlineStatus;
import ats.foundation.utils.config.ATSROOTConfig;
import ats.foundation.utils.exception.CodeException;

import cgws.ficc.framework.compat.ServiceSettings;
import cgws.ficc.framework.dps.DpsPeer;
import cgws.ficc.framework.dps.ServiceEndpoint;
import cgws.ficc.framework.dps.ZookeeperDiscovery;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.atomic.AtomicInteger;

/** Existing service-facing API, backed by the unchanged DPS SDK and the owned registry adapter. */
public class NetClientWrapper {
    public static String defaultClientName = "Server";
    private static final Logger LOGGER = LoggerFactory.getLogger(NetClientWrapper.class);
    private final Map<String, Channel> channels = new ConcurrentHashMap<>();
    private final Map<String, ZookeeperDiscovery> registries = new ConcurrentHashMap<>();
    private final OnlineStatus onlineStatus = new OnlineStatus();
    private final ExecutorService connections =
            Executors.newSingleThreadExecutor(
                    task -> {
                        Thread thread = new Thread(task, "ficc-dps-connections");
                        thread.setDaemon(true);
                        return thread;
                    });
    private final ExecutorService notifications =
            Executors.newSingleThreadExecutor(
                    task -> {
                        Thread thread = new Thread(task, "ficc-dps-notifications");
                        thread.setDaemon(true);
                        return thread;
                    });
    private volatile boolean closed;
    public boolean asycNotifyThreadFlag;

    public OnlineStatus getOnlineStatus() {
        return onlineStatus;
    }

    public String getNodeId() {
        return null;
    }

    public boolean isOnline(String server) {
        Channel channel = channels.get(server);
        return channel != null && channel.peers.values().stream().anyMatch(DpsPeer::isOnline);
    }

    public void init(String server, String... names) {
        channel(server, names);
    }

    public void init(String server, IMessage<String> callback) {
        init(server);
    }

    public void addCedaClientConnectListener(String server, ICedaConnectionListener listener) {
        channel(server).listeners.add(listener);
    }

    public String requestSync(
            String server, String session, String topic, String content, long timeout)
            throws Exception {
        return channel(server).active().request(session, topic, content, timeout);
    }

    public void requestAsync(
            String server, String topic, String content, String session, IMessage<String> callback)
            throws Exception {
        channel(server)
                .active()
                .requestAsync(
                        session,
                        topic,
                        content,
                        60000,
                        reply -> callback.onMessage(session, reply));
    }

    public void subscribe(String server, String topic, IMessage<String> callback) {
        channel(server).subscribe(topic, null, callback);
    }

    public void subscribeWithImage(
            String server,
            String topic,
            String imageServer,
            String session,
            IMessage<String> callback) {
        channel(server).subscribe(topic, imageServer, callback);
    }

    public void subscribeWithImage(
            String server,
            List<String> topics,
            String imageServer,
            String session,
            IMessage<String> callback) {
        for (String topic : topics) {
            subscribeWithImage(server, topic, imageServer, session, callback);
        }
    }

    public void unSubscribe(String server, String topic, IMessage<String> callback) {
        channel(server).unsubscribe(topic, null, callback);
    }

    public void unSubscribeWithImage(
            String server,
            String topic,
            String imageServer,
            String session,
            IMessage<String> callback) {
        channel(server).unsubscribe(topic, imageServer, callback);
    }

    private synchronized Channel channel(String server, String... names) {
        if (closed) {
            throw new IllegalStateException("Network client is closed");
        }
        Channel channel = channels.get(server);
        if (channel == null) {
            channel =
                    new Channel(
                            ServiceSettings.load(server),
                            names.length > 0 ? names[0] : defaultClientName);
            channels.put(server, channel);
            channel.start();
        }
        return channel;
    }

    private synchronized ZookeeperDiscovery registry(ServiceSettings settings) {
        return registries.computeIfAbsent(
                settings.registryHosts,
                hosts ->
                        new ZookeeperDiscovery(
                                hosts,
                                120000,
                                60000,
                                ATSROOTConfig.getATSROOTConfig().getProperty("REGISTER.UserName"),
                                ATSROOTConfig.getATSROOTConfig().getProperty("REGISTER.Password"),
                                error -> LOGGER.error("DPS discovery failed", error)));
    }

    public void dispose() {
        synchronized (this) {
            if (closed) {
                return;
            }
            closed = true;
        }
        channels.values().forEach(Channel::close);
        registries.values().forEach(ZookeeperDiscovery::close);
        connections.shutdownNow();
        notifications.shutdownNow();
        channels.clear();
        registries.clear();
    }

    private final class Channel {
        private final ServiceSettings settings;
        private final String username;
        private final Map<ServiceEndpoint, DpsPeer> peers = new ConcurrentHashMap<>();
        private final List<Subscription> subscriptions = new CopyOnWriteArrayList<>();
        private final List<ICedaConnectionListener> listeners = new CopyOnWriteArrayList<>();
        private final AtomicInteger index = new AtomicInteger();

        Channel(ServiceSettings settings, String username) {
            this.settings = settings;
            this.username = username;
        }

        void start() {
            if (settings.register) {
                registry(settings)
                        .watch(
                                settings.name,
                                endpoints -> connections.execute(() -> update(endpoints)));
            } else {
                connections.execute(() -> update(Collections.singletonList(settings.endpoint)));
            }
        }

        synchronized void update(List<ServiceEndpoint> endpoints) {
            if (closed) {
                return;
            }
            for (ServiceEndpoint old : new ArrayList<>(peers.keySet())) {
                if (!endpoints.contains(old)) {
                    peers.remove(old).close();
                }
            }
            for (ServiceEndpoint endpoint : endpoints) {
                if (peers.containsKey(endpoint)) {
                    continue;
                }
                DpsPeer peer =
                        new DpsPeer(
                                settings.key,
                                connections,
                                online -> {
                                    if (online) {
                                        onlineStatus.onConnected(settings.key);
                                    } else {
                                        onlineStatus.onDisconnected(settings.key);
                                    }
                                    for (ICedaConnectionListener listener : listeners) {
                                        if (online) {
                                            listener.onConnected(settings.key);
                                        } else {
                                            listener.onDisconnected(settings.key);
                                        }
                                    }
                                },
                                error -> LOGGER.error("DPS peer failed: " + settings.key, error));
                peers.put(endpoint, peer);
                for (Subscription subscription : subscriptions) {
                    subscription.install(peer);
                }
                peer.connect(endpoint, username, username);
            }
        }

        synchronized void close() {
            peers.values().forEach(DpsPeer::close);
            peers.clear();
            subscriptions.clear();
        }

        DpsPeer active() throws CodeException {
            List<DpsPeer> available = new ArrayList<>();
            for (DpsPeer peer : peers.values()) {
                if (peer.isOnline()) {
                    available.add(peer);
                }
            }
            if (available.isEmpty()) {
                throw new CodeException("1015", settings.key + " is not Online");
            }
            return available.get(Math.floorMod(index.getAndIncrement(), available.size()));
        }

        synchronized void subscribe(String topic, String imageServer, IMessage<String> callback) {
            for (Subscription subscription : subscriptions) {
                if (subscription.matches(topic, imageServer, callback)) {
                    return;
                }
            }
            Subscription subscription = new Subscription(topic, imageServer, callback);
            subscriptions.add(subscription);
            peers.values().forEach(subscription::install);
        }

        synchronized void unsubscribe(String topic, String imageServer, IMessage<String> callback) {
            for (Subscription subscription : subscriptions) {
                if (!subscription.matches(topic, imageServer, callback)) {
                    continue;
                }
                subscriptions.remove(subscription);
                for (DpsPeer peer : peers.values()) {
                    try {
                        peer.unsubscribe(topic, imageServer, subscription.adapter);
                    } catch (Exception error) {
                        throw new IllegalStateException("DPS unsubscribe failed: " + topic, error);
                    }
                }
            }
        }
    }

    private final class Subscription {
        final String topic;
        final String imageServer;
        final IMessage<String> callback;
        final com.gateway.connector.tcp.client.IMessage<String> adapter;

        Subscription(String topic, String imageServer, IMessage<String> callback) {
            this.topic = topic;
            this.imageServer = imageServer;
            this.callback = callback;
            adapter =
                    (receivedTopic, content) -> {
                        if (asycNotifyThreadFlag) {
                            notifications.execute(() -> callback.onMessage(receivedTopic, content));
                        } else {
                            callback.onMessage(receivedTopic, content);
                        }
                    };
        }

        void install(DpsPeer peer) {
            peer.subscribe(topic, imageServer, adapter);
        }

        boolean matches(String topic, String imageServer, IMessage<String> callback) {
            return this.topic.equals(topic)
                    && java.util.Objects.equals(this.imageServer, imageServer)
                    && this.callback == callback;
        }
    }
}
