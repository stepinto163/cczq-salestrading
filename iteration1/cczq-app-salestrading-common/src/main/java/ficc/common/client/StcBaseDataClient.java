package ficc.common.client;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import java.util.Arrays;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

/** Refresh a configured QA universe outside query/quote-processing threads. */
public final class StcBaseDataClient implements BaseDataClient {
    private final StcTransport transport;
    private final StcBaseMapper mapper;
    private final List<String> securityIds;
    private final int refreshSeconds;
    private volatile Snapshot snapshot;
    private volatile long generation;
    private volatile boolean invalidated = true;
    private ScheduledExecutorService worker;
    private record Snapshot(Map<String, JSONObject> securities, List<JSONObject> issuers,
            List<JSONObject> valuations, String version) {}

    public StcBaseDataClient(StcTransport transport, Properties configuration) {
        this.transport = transport;
        mapper = new StcBaseMapper(configuration);
        securityIds = Arrays.stream(configuration.getProperty("base.securityIds", "").split(","))
                .map(String::trim).filter(value -> !value.isEmpty()).distinct().toList();
        if (securityIds.isEmpty() || securityIds.size() > 1000) {
            throw new IllegalArgumentException("base.securityIds must contain 1..1000 explicit QA security IDs");
        }
        refreshSeconds = Integer.parseInt(configuration.getProperty("base.refreshSeconds", "60"));
        if (refreshSeconds < 10) {
            throw new IllegalArgumentException("base.refreshSeconds must be at least 10");
        }
        transport.connectionChanged(this::invalidate);
        transport.subscribe("stc.base.clear", event -> invalidate());
        transport.subscribe("stc.base.manualClear.**", event -> invalidate());
    }

    private synchronized void invalidate() {
        generation++;
        invalidated = true;
    }

    @Override
    public void start() {
        worker = Executors.newSingleThreadScheduledExecutor(task -> {
            Thread thread = new Thread(task, "stc-reference-refresh");
            thread.setDaemon(true);
            return thread;
        });
        worker.scheduleWithFixedDelay(() -> {
            try {
                refresh();
            } catch (Exception error) {
                invalidated = true;
                System.err.println("BaseSvr reference refresh unavailable: " + error.getMessage());
            }
        }, 0, refreshSeconds, TimeUnit.SECONDS);
    }

    public void refresh() {
        long startedGeneration = generation;
        Map<String, JSONObject> securities = new LinkedHashMap<>();
        Map<String, JSONObject> issuers = new LinkedHashMap<>();
        Map<String, JSONObject> rawIssuers = new LinkedHashMap<>();
        java.util.ArrayList<JSONObject> valuations = new java.util.ArrayList<>();
        String version = "STC-" + java.time.Instant.now();
        for (String securityId : securityIds) {
            JSONObject request = JsonUtils.object("SecurityID", securityId);
            JSONObject bond = first(transport.request("querySecurity", null, request), true);
            String issuerId = StcBaseMapper.text(bond, "Issuer_Code");
            JSONObject issuer = new JSONObject();
            if (issuerId != null) {
                issuer = rawIssuers.computeIfAbsent(issuerId, id -> {
                    JSONObject reply = transport.request("stc.base.info.query", null,
                            JsonUtils.object("TableName", "p_issuer_info", "KeyValue", id));
                    if (reply.isEmpty()) {
                        return new JSONObject();
                    }
                    success(reply);
                    JSONObject data = reply.getJSONObject("Datas");
                    return data == null ? new JSONObject() : data;
                });
            }
            JSONObject valuation = first(transport.request("queryChianSecurity", null, request), false);
            JSONObject security = mapper.security(securityId, bond, issuer, valuation, version);
            securities.put(securityId, security);
            if (issuerId != null) {
                issuers.put(issuerId, mapper.issuer(security, issuer));
            }
            JSONObject record = mapper.valuation(securityId, valuation);
            if (record != null) {
                valuations.add(record);
            }
        }
        synchronized (this) {
            if (generation != startedGeneration || !transport.online()) {
                throw unavailable();
            }
            snapshot = new Snapshot(java.util.Collections.unmodifiableMap(securities),
                    List.copyOf(issuers.values()), List.copyOf(valuations), version);
            invalidated = false;
        }
    }

    private static void success(JSONObject reply) {
        if (!"0000".equals(reply.getString("Code"))) {
            throw unavailable();
        }
    }

    private static JSONObject first(JSONObject reply, boolean required) {
        success(reply);
        JSONArray rows = reply.getJSONArray("datas");
        if (rows == null) {
            throw unavailable();
        }
        if (rows.isEmpty()) {
            if (required) {
                throw ApiException.forField("1009", "NOT_READY", "BaseSvr did not return a configured bond", null);
            }
            return new JSONObject();
        }
        if (rows.size() != 1) {
            throw ApiException.forField("1009", "NOT_READY", "Ambiguous BaseSvr single-security reply", null);
        }
        return rows.getJSONObject(0);
    }

    private Snapshot current() {
        Snapshot current = snapshot;
        if (invalidated || current == null || !transport.online()) {
            throw unavailable();
        }
        return current;
    }

    private static ApiException unavailable() {
        return ApiException.forField("1009", "NOT_READY", "STC base data is unavailable or refreshing", null);
    }

    @Override
    public Map<String, JSONObject> securities() {
        return current().securities();
    }

    @Override
    public String version() {
        return current().version();
    }

    @Override
    public List<JSONObject> records(String kind) {
        Snapshot current = current();
        return switch (kind) {
            case "SECURITY" -> List.copyOf(current.securities().values());
            case "ISSUER" -> current.issuers();
            case "DAILY_VALUATION" -> current.valuations();
            default -> throw ApiException.forField("1001", "VALIDATION_FAILED", "Unknown reference kind", "kind");
        };
    }

    @Override
    public void close() throws InterruptedException {
        if (worker != null) {
            worker.shutdownNow();
            worker.awaitTermination(4, TimeUnit.SECONDS);
        }
    }
}
