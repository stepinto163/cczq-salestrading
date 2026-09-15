package ficc.quote;

import ficc.common.*;
import ficc.common.client.*;
import ats.bbox.framework.Server;
import sc.app.stc.net.NetClientResource;

import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.resource.ResourceAdapter;
import com.alibaba.fastjson2.*;
import java.util.*;
import java.util.function.BiConsumer;

/** BBox resource: owns business state and the PostgreSQL connection. */
public final class QuoteResource extends ResourceAdapter {
    private PgStore store;
    private QuoteService service;
    private Properties configuration;
    private LoginSessionClient identity;
    private BiConsumer<String, JSONObject> publisher = (topic, value) -> {};

    @Override
    public void init() throws LifecycleException {
        try {
            configuration = ServiceConfiguration.load("QuoteSvr", getConfig().getString("quote.properties"),
                    getConfig().getString("adapter.properties", ""));
            store = new PgStore(configuration);
            super.init();
        } catch (Exception error) {
            throw new LifecycleException(error);
        }
    }

    @Override
    public void start() throws LifecycleException {
        super.start();
        try {
            NetClientResource resource = (NetClientResource) Server.getServer().getResource("NetClientResource");
            var network = resource.getNetClientWrapper();
            identity = AdapterFactory.identity(configuration, network, true);
            ReferenceData references = AdapterFactory.references(configuration, network);
            service = new QuoteService(store, references, new AccessPolicy(configuration));
            service.publish = publisher;
            identity.onInvalidated(sessionId -> {
                synchronized (service) {
                    if (sessionId == null) {
                        service.views.streams.clear();
                        service.cursors.clear();
                    } else {
                        service.views.disconnected(sessionId);
                        service.cursors.values().removeIf(cursor -> sessionId.equals(cursor.session()));
                    }
                }
            });
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

    public void heartbeat() {
        if (service != null) {
            synchronized (service) {
                service.views.heartbeat();
            }
        }
    }

    public Map<String, Object> login(String content) {
        if (!(identity instanceof LocalLoginSimulator simulator)) {
            return JsonUtils.object("Code", "1003", "Msg", "Local login simulator is disabled; use STC LoginSvr");
        }
        return simulator.login(JsonUtils.parseObject(content));
    }

    public Map<String, Object> dispatch(String topic, String sid, String content) {
        QuotePerformanceTrace trace = QuotePerformanceTrace.begin(topic);
        String requestId = null;
        try {
            JSONObject request;
            try {
                request = JsonUtils.parseObject(content);
            } catch (Exception error) {
                return ApiResponse.failure(null,
                        ApiException.forField("1001", "VALIDATION_FAILED", "Invalid JSON", null));
            }
            requestId = request.getString("requestId");
            if (QuoteSvrClient.INTERNAL_STATE_QUERY.equals(topic) || QuoteSvrClient.INTERNAL_PRICE_QUERY.equals(topic)) {
                // STC may put a transport connection ID in signalID even without user login.
                // Internal reads rely on the trusted backend network; GW blocks these topics.
                if (service == null) {
                    throw ApiException.forField("1009", "NOT_READY", "QuoteSvr is starting", null);
                }
                return service.handleInternalRead(topic, request);
            }
            if (identity == null || service == null) {
                throw ApiException.forField("1009", "NOT_READY", "QuoteSvr is starting", null);
            }
            String user = identity.resolveUser(sid);
            if (trace != null) {
                trace.lockRequested = System.nanoTime();
            }
            synchronized (service) {
                if (trace != null) {
                    trace.lockAcquired = System.nanoTime();
                }
                try {
                    JSONObject result = service.handle(user, sid, topic, request);
                    if (topic.endsWith(".stream.open") && "0000".equals(result.getString("Code"))) {
                        // GW owns the physical subscription; QuoteSvr owns the logical view.
                        service.views.authorize(sid, user, result.getJSONObject("Data").getString("pushTopic"));
                    }
                    return result;
                } finally {
                    if (trace != null) {
                        trace.businessFinished = System.nanoTime();
                    }
                }
            }
        } catch (ApiException error) {
            return ApiResponse.failure(requestId, error);
        } catch (Exception error) {
            return ApiResponse.failure(requestId, ApiException.forField("1009", "NOT_READY", "Identity provider is unavailable", null));
        } finally {
            if (trace != null) {
                trace.finish(topic, requestId);
            }
        }
    }
    public Map<String, Object> snapshot(String topic, String sessionId) {
        try {
            if (identity == null || service == null) {
                throw ApiException.forField("1009", "NOT_READY", "QuoteSvr is starting", null);
            }
            String userId = identity.resolveUser(sessionId);
            if (userId == null) {
                return JsonUtils.object("Code", "1005", "Msg", "Login required");
            }
            synchronized (service) {
                service.views.authorize(sessionId, userId, topic);
                Map<String, Object> result = new LinkedHashMap<>();
                service.views.image(topic, sessionId).forEach((key, value) -> result.put(key, JsonUtils.toJson(value)));
                return result;
            }
        } catch (ApiException error) {
            return ApiResponse.failure(null, error);
        } catch (Exception error) {
            return JsonUtils.object("Code", "1009", "Msg", "Identity provider is unavailable");
        }
    }

    @Override
    public void stop() throws LifecycleException {
        try {
            if (service != null) {
                service.refs.close();
            }
        } catch (Exception error) {
            throw new LifecycleException(error);
        } finally {
            if (store != null) {
                try {
                    store.close();
                    store = null;
                } catch (Exception error) {
                    throw new LifecycleException(error);
                }
            }
            super.stop();
        }
    }
}
