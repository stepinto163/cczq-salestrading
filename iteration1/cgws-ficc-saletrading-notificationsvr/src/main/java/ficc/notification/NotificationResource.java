package ficc.notification;

import ats.app.ym.ceda.ICedaConnectionListener;
import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.Server;
import ats.bbox.framework.resource.ResourceAdapter;

import com.alibaba.fastjson2.JSONObject;

import ficc.api.*;

import ficc.common.ApiException;
import ficc.common.ApiResponse;
import ficc.common.ComponentLog;
import ficc.common.JdbcStore;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import ficc.common.ServiceConfiguration;
import ficc.common.client.AccessPolicy;
import ficc.common.client.AdapterFactory;
import ficc.common.client.LoginSessionClient;
import ficc.common.client.QuoteSvrClient;

import sc.app.stc.net.NetClientResource;
import sc.app.stc.net.NetClientWrapper;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicLong;
import java.util.function.BiConsumer;

/** Owns notification business state; STC resources own outbound connections. */
public final class NotificationResource extends ResourceAdapter {
    private static final org.apache.log4j.Logger LOGGER =
            org.apache.log4j.Logger.getLogger(NotificationResource.class);
    private static final int MAX_PENDING_EVENTS = 128;

    private final BlockingQueue<SourceEvent> incoming =
            new ArrayBlockingQueue<>(MAX_PENDING_EVENTS);
    private final AtomicLong sourceGeneration = new AtomicLong();
    private Properties configuration;
    private BiConsumer<String, JSONObject> publisher = (topic, value) -> {};
    private JdbcStore store;
    private NotificationService service;
    private QuoteSvrClient source;
    private LoginSessionClient identity;
    private ScheduledExecutorService worker;
    private long restoredGeneration = -1;
    private long retryAt;
    private long heartbeatAt;
    private volatile boolean ready;

    private static final class SourceEvent {
        final boolean quote;
        final ProtocolModel message;

        SourceEvent(boolean quote, ProtocolModel message) {
            this.quote = quote;
            this.message = message;
        }

        boolean quote() {
            return quote;
        }

        ProtocolModel message() {
            return message;
        }
    }

    @Override
    public void init() throws LifecycleException {
        try {
            configuration =
                    ServiceConfiguration.load(
                            "NotificationSvr",
                            getConfig().getString("quote.properties"),
                            getConfig().getString("adapter.properties", ""));
            store =
                    ComponentLog.initialize(
                            configuration,
                            "DATABASE",
                            "postgresql",
                            "JdbcStore",
                            ComponentLog.databaseTarget(configuration, "db.url", "db.schema"),
                            () -> new JdbcStore(configuration, "notification_"));
            super.init();
        } catch (Exception error) {
            throw new LifecycleException(error);
        }
    }

    public void publishThrough(BiConsumer<String, JSONObject> publisher) {
        this.publisher = (topic, value) -> publisher.accept(topic, ProtocolBoundary.push(topic, value));
        if (service != null) {
            service.publish = this.publisher;
        }
    }

    @Override
    public void start() throws LifecycleException {
        super.start();
        NetClientResource resource =
                (NetClientResource) Server.getServer().getResource("NetClientResource");
        NetClientWrapper network = resource.getNetClientWrapper();
        try {
            ReferenceData references = AdapterFactory.references(configuration, network);
            service =
                    ComponentLog.initialize(
                            configuration,
                            "NOTIFICATION_SERVICE",
                            "local",
                            "NotificationService",
                            "NotificationSvr",
                            () ->
                                    new NotificationService(
                                            store, references, new AccessPolicy(configuration)));
            service.publish = this.publisher;
            identity = AdapterFactory.identity(configuration, network, false);
            identity.onInvalidated(
                    sessionId -> {
                        synchronized (service) {
                            service.streams
                                    .values()
                                    .removeIf(
                                            stream ->
                                                    sessionId == null
                                                            || sessionId.equals(stream.sid));
                            service.pages
                                    .values()
                                    .removeIf(
                                            page ->
                                                    sessionId == null
                                                            || sessionId.equals(page.sid()));
                        }
                    });
        } catch (Exception error) {
            throw new LifecycleException(error);
        }
        try {
            source =
                    ComponentLog.initialize(
                            configuration,
                            "QUOTE_SOURCE",
                            "stc",
                            "QuoteSvrClient",
                            "SERVER.QuoteSvr",
                            () -> {
                                long timeoutMillis =
                                        getConfig().getInt("source.requestTimeoutMillis", 3000);
                                QuoteSvrClient client = new QuoteSvrClient(network, timeoutMillis);
                                client.initialize(
                                        new ICedaConnectionListener() {
                                            @Override
                                            public void onConnected(String serverName) {
                                                invalidateSource();
                                            }

                                            @Override
                                            public void onDisconnected(String serverName) {
                                                invalidateSource();
                                            }
                                        });
                                client.subscribeQuoteEvents(message -> enqueue(true, message));
                                client.subscribePrices(message -> enqueue(false, message));
                                return client;
                            });
        } catch (Exception error) {
            throw new LifecycleException(error);
        }
        worker =
                Executors.newSingleThreadScheduledExecutor(
                        task -> {
                            Thread thread = new Thread(task, "notification-source");
                            thread.setDaemon(true);
                            return thread;
                        });
        worker.scheduleWithFixedDelay(this::tick, 0, 50, TimeUnit.MILLISECONDS);
    }

    private void invalidateSource() {
        ready = false;
        sourceGeneration.incrementAndGet();
    }

    private void enqueue(boolean quote, ProtocolModel message) {
        if (!incoming.offer(new SourceEvent(quote, message))) {
            invalidateSource();
        }
    }

    private void restore() throws Exception {
        long generation = sourceGeneration.get();
        incoming.clear();
        List<JSONObject> quotes = new ArrayList<>();
        List<JSONObject> settings = new ArrayList<>();
        StateQueryRequest request = new StateQueryRequest();
        request.limit = 1000;
        QuoteStateData state = source.queryState(request);
        QuoteStateData firstPage = state;
        while (true) {
            state.records.quotes.forEach(quote -> quotes.add(quote.toJson()));
            state.records.watchlists.forEach(group -> settings.add(group.toJson()));
            state.records.filterGroups.forEach(group -> settings.add(group.toJson()));
            String cursor = state.nextCursor;
            if (cursor == null) {
                break;
            }
            request = new StateQueryRequest();
            request.cursor = cursor;
            state = source.queryState(request);
        }
        PriceFeedBatch snapshot = source.queryPriceFeed();
        synchronized (service) {
            service.replaceQuotes(firstPage.toJson(), quotes, settings);
            if (!service.priceMessage(snapshot.toJson())) {
                throw new IllegalStateException(
                        "Initial price snapshot requires resynchronization");
            }
        }
        restoredGeneration = generation;
        ready = generation == sourceGeneration.get() && source.isOnline();
        System.out.println(
                "NotificationSvr source synchronized at " + firstPage.sequence);
    }

    private void tick() {
        try {
            if (System.currentTimeMillis() < retryAt) {
                return;
            }
            if (!source.isOnline()) {
                ready = false;
                return;
            }
            if (!ready || restoredGeneration != sourceGeneration.get()) {
                restore();
            }
            if (!ready) {
                return;
            }
            for (int index = 0; index < MAX_PENDING_EVENTS; index++) {
                SourceEvent event = incoming.poll();
                if (event == null) {
                    break;
                }
                boolean accepted =
                        event.quote()
                                ? service.quoteMessage(event.message().toJson())
                                : service.priceMessage(event.message().toJson());
                if (!accepted) {
                    invalidateSource();
                    break;
                }
            }
            if (ready && System.currentTimeMillis() - heartbeatAt >= 5000) {
                service.heartbeat();
                heartbeatAt = System.currentTimeMillis();
            }
        } catch (Exception error) {
            invalidateSource();
            retryAt = System.currentTimeMillis() + 2000;
            synchronized (service) {
                service.ready = false;
            }
            System.err.println(
                    "Notification source retry: "
                            + error.getClass().getSimpleName()
                            + ": "
                            + error.getMessage());
        }
    }

    public Map<String, Object> dispatch(String topic, String sessionId, String content) {
        try {
            if (sessionId == null || !ready) {
                return unavailable("Notification service is synchronizing with QuoteSvr");
            }
            String userId = identity.resolveUser(sessionId);
            if (!ready) {
                return unavailable("Quote source changed during session lookup");
            }
            return service.handle(userId, sessionId, topic, JsonUtils.parseObject(content));
        } catch (ApiException error) {
            return ApiResponse.failure(null, error);
        } catch (Exception error) {
            LOGGER.error("Notification dispatch failed: topic=" + topic, error);
            return unavailable("Notification source/session lookup unavailable");
        }
    }

    private Map<String, Object> unavailable(String message) {
        return ApiResponse.failure(null, ApiException.forField("1009", "NOT_READY", message, null));
    }

    @Override
    public void stop() throws LifecycleException {
        ready = false;
        if (worker != null) {
            worker.shutdownNow();
            try {
                worker.awaitTermination(4, TimeUnit.SECONDS);
            } catch (InterruptedException error) {
                Thread.currentThread().interrupt();
            }
        }
        if (service != null) {
            try {
                service.refs.close();
            } catch (Exception error) {
                throw new LifecycleException(error);
            }
        }
        if (store != null) {
            try {
                store.close();
            } catch (Exception error) {
                throw new LifecycleException(error);
            }
        }
        // NetClientResource releases the shared client when the framework stops.
        super.stop();
    }
}
