package ficc.notification;

import ats.app.ym.ceda.ICedaConnectionListener;
import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.Server;
import ats.bbox.framework.resource.ResourceAdapter;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import ficc.common.ApiResponse;
import ficc.common.JdbcStore;
import ficc.common.JsonUtils;
import ficc.common.client.LoginSessionClient;
import ficc.common.client.QuoteSvrClient;
import ficc.common.client.AdapterFactory;
import ficc.common.client.AccessPolicy;
import ficc.common.ReferenceData;
import ficc.common.ServiceConfiguration;
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
    private static final int MAX_PENDING_EVENTS = 128;

    private final BlockingQueue<SourceEvent> incoming = new ArrayBlockingQueue<>(MAX_PENDING_EVENTS);
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

    private record SourceEvent(boolean quote, JSONObject message) {}

    @Override
    public void init() throws LifecycleException {
        try {
            configuration = ServiceConfiguration.load("NotificationSvr",
                    getConfig().getString("quote.properties"), getConfig().getString("adapter.properties", ""));
            store = new JdbcStore(configuration, "notification_");
            super.init();
        } catch (Exception error) {
            throw new LifecycleException(error);
        }
    }

    public void publishThrough(BiConsumer<String, JSONObject> publisher) {
        this.publisher = publisher;
        if (service != null) {
            service.publish = publisher;
        }
    }

    @Override
    public void start() throws LifecycleException {
        super.start();
        NetClientResource resource = (NetClientResource) Server.getServer().getResource("NetClientResource");
        NetClientWrapper network = resource.getNetClientWrapper();
        try {
            ReferenceData references = AdapterFactory.references(configuration, network);
            service = new NotificationService(store, references, new AccessPolicy(configuration));
            service.publish = publisher;
            identity = AdapterFactory.identity(configuration, network, false);
            identity.onInvalidated(sessionId -> {
                synchronized (service) {
                    service.streams.values().removeIf(stream -> sessionId == null || sessionId.equals(stream.sid));
                    service.pages.values().removeIf(page -> sessionId == null || sessionId.equals(page.sid()));
                }
            });
        } catch (Exception error) {
            throw new LifecycleException(error);
        }
        long timeoutMillis = getConfig().getInt("source.requestTimeoutMillis", 3000);
        source = new QuoteSvrClient(network, timeoutMillis);
        source.initialize(new ICedaConnectionListener() {
            @Override
            public void onConnected(String serverName) {
                invalidateSource();
            }

            @Override
            public void onDisconnected(String serverName) {
                invalidateSource();
            }
        });
        source.subscribeQuoteEvents(message -> enqueue(true, message));
        source.subscribePrices(message -> enqueue(false, message));
        worker = Executors.newSingleThreadScheduledExecutor(task -> {
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

    private void enqueue(boolean quote, JSONObject message) {
        if (!incoming.offer(new SourceEvent(quote, message))) {
            invalidateSource();
        }
    }

    private void restore() throws Exception {
        long generation = sourceGeneration.get();
        incoming.clear();
        List<JSONObject> quotes = new ArrayList<>();
        List<JSONObject> settings = new ArrayList<>();
        JSONObject state = source.queryState(JsonUtils.object("limit", 1000));
        JSONObject firstPage = state;
        while (true) {
            JSONObject records = state.getJSONObject("records");
            quotes.addAll(records.getJSONArray("quotes").toJavaList(JSONObject.class));
            settings.addAll(records.getJSONArray("watchlists").toJavaList(JSONObject.class));
            settings.addAll(records.getJSONArray("filterGroups").toJavaList(JSONObject.class));
            String cursor = state.getString("nextCursor");
            if (cursor == null) {
                break;
            }
            state = source.queryState(JsonUtils.object("cursor", cursor));
        }
        JSONObject snapshot = source.queryPriceSnapshot();
        synchronized (service) {
            service.replaceQuotes(firstPage, quotes, settings);
            if (!service.priceMessage(snapshot)) {
                throw new IllegalStateException("Initial price snapshot requires resynchronization");
            }
        }
        restoredGeneration = generation;
        ready = generation == sourceGeneration.get() && source.isOnline();
        System.out.println("NotificationSvr source synchronized at " + firstPage.getString("sequence"));
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
                boolean accepted = event.quote()
                        ? service.quoteMessage(event.message())
                        : service.priceMessage(event.message());
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
            System.err.println("Notification source retry: " + error.getClass().getSimpleName()
                    + ": " + error.getMessage());
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
        } catch (Exception error) {
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
