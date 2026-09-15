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
import org.apache.log4j.Logger;

/** Cache STC reference data outside query and quote-processing threads. */
public final class StcBaseDataClient implements BaseDataClient {
    private static final Logger LOGGER = Logger.getLogger(StcBaseDataClient.class);
    private final StcTransport transport;
    private final StcBaseMapper mapper;
    private final List<String> securityIds;
    private final int refreshSeconds;
    private final StcBaseUniverseLoader universeLoader;
    private volatile Snapshot snapshot;
    private volatile long generation;
    private volatile boolean invalidated = true;
    private ScheduledExecutorService worker;
    private volatile long nextRefreshNanos;
    private record Snapshot(Map<String, JSONObject> securities, List<JSONObject> issuers,
            List<JSONObject> valuations, String version) {}

    public StcBaseDataClient(StcTransport transport, Properties configuration) {
        this.transport = transport;
        mapper = new StcBaseMapper(configuration);
        securityIds = Arrays.stream(configuration.getProperty("base.securityIds", "").split(","))
                .map(String::trim).filter(value -> !value.isEmpty()).distinct().toList();
        if (securityIds.size() > 1000) {
            throw new IllegalArgumentException("Optional base.securityIds test subset cannot exceed 1000 IDs");
        }
        universeLoader = securityIds.isEmpty() ? new StcBaseUniverseLoader(transport, configuration) : null;
        refreshSeconds = Integer.parseInt(configuration.getProperty("base.refreshSeconds",
                securityIds.isEmpty() ? "3600" : "60"));
        if (refreshSeconds < 10) {
            throw new IllegalArgumentException("base.refreshSeconds must be at least 10");
        }
        transport.connectionChanged(this::invalidate);
        transport.subscribe("stc.base.clear", event -> invalidate());
        transport.subscribe("stc.base.manualClear.**", event -> invalidate());
        transport.subscribe("stc.base.loadBondsInfo", event -> invalidate());
        transport.subscribe("stc.base.bondsinfo", event -> invalidate());
        transport.subscribe("stc.base.chianBond", event -> invalidate());
    }

    private synchronized void invalidate() {
        LOGGER.info("BaseSvr reference cache invalidated; generation=" + (generation + 1));
        generation++;
        invalidated = true;
        nextRefreshNanos = 0;
    }

    @Override
    public synchronized void start() {
        if (worker != null) {
            throw new IllegalStateException("BaseSvr reference worker is already started");
        }
        worker = Executors.newSingleThreadScheduledExecutor(task -> {
            Thread thread = new Thread(task, "stc-reference-refresh");
            thread.setDaemon(true);
            return thread;
        });
        worker.scheduleWithFixedDelay(() -> {
            if (System.nanoTime() < nextRefreshNanos) {
                return;
            }
            try {
                refresh();
            } catch (Exception error) {
                invalidated = true;
                nextRefreshNanos = System.nanoTime() + TimeUnit.SECONDS.toNanos(30);
                LOGGER.warn("BaseSvr reference refresh unavailable; retrySeconds=30: " + error.getMessage());
            }
        }, 0, 1, TimeUnit.SECONDS);
    }

    public void refresh() {
        long started = System.nanoTime();
        LOGGER.info("BaseSvr reference refresh started: mode=" + (universeLoader == null ? "subset" : "all")
                + ", configuredSecurityIds=" + securityIds);
        long startedGeneration = generation;
        Map<String, JSONObject> securities = new LinkedHashMap<>();
        Map<String, JSONObject> issuers = new LinkedHashMap<>();
        Map<String, JSONObject> rawIssuers = new LinkedHashMap<>();
        java.util.ArrayList<JSONObject> valuations = new java.util.ArrayList<>();
        String version = "STC-" + java.time.Instant.now();
        if (universeLoader != null) {
            StcBaseUniverseLoader.Universe universe = universeLoader.load(() -> checkGeneration(startedGeneration));
            for (Map.Entry<String, JSONObject> entry : universe.bonds().entrySet()) {
                JSONObject bond = entry.getValue();
                String issuerId = StcBaseMapper.text(bond, "Issuer_Code");
                addSecurity(entry.getKey(), bond, universe.issuers().get(issuerId),
                        universe.valuations().get(entry.getKey()), version, securities, issuers, valuations);
            }
        } else {
            loadSubset(startedGeneration, version, securities, issuers, rawIssuers, valuations);
        }
        synchronized (this) {
            checkGeneration(startedGeneration);
            snapshot = new Snapshot(java.util.Collections.unmodifiableMap(securities),
                    List.copyOf(issuers.values()), List.copyOf(valuations), version);
            invalidated = false;
            nextRefreshNanos = System.nanoTime() + TimeUnit.SECONDS.toNanos(refreshSeconds);
        }
        LOGGER.info("BaseSvr reference cache ready: mode=" + (universeLoader == null ? "subset" : "all")
                + ", securities=" + securities.size() + ", issuers=" + issuers.size()
                + ", valuations=" + valuations.size() + ", referenceVersion=" + version
                + ", elapsedMillis=" + (System.nanoTime() - started) / 1_000_000);
    }

    private void loadSubset(long startedGeneration, String version, Map<String, JSONObject> securities,
            Map<String, JSONObject> issuers, Map<String, JSONObject> rawIssuers, List<JSONObject> valuations) {
        for (String securityId : securityIds) {
            checkGeneration(startedGeneration);
            JSONObject request = JsonUtils.object("SecurityID", securityId);
            JSONObject bond = first(transport.request("querySecurity", null, request), "querySecurity", true);
            String issuerId = StcBaseMapper.text(bond, "Issuer_Code");
            JSONObject issuer = new JSONObject();
            if (issuerId != null) {
                issuer = rawIssuers.computeIfAbsent(issuerId, id -> {
                    JSONObject reply = transport.request("stc.base.info.query", null,
                            JsonUtils.object("TableName", "p_issuer_info", "KeyValue", id));
                    if (reply.isEmpty()) {
                        return new JSONObject();
                    }
                    success(reply, "stc.base.info.query");
                    JSONObject data = reply.getJSONObject("Datas");
                    return data == null ? new JSONObject() : data;
                });
            }
            JSONObject valuation = first(transport.request("queryChianSecurity", null, request),
                    "queryChianSecurity", false);
            addSecurity(securityId, bond, issuer, valuation, version, securities, issuers, valuations);
        }
    }

    private void addSecurity(String securityId, JSONObject bond, JSONObject issuer, JSONObject valuation,
            String version, Map<String, JSONObject> securities, Map<String, JSONObject> issuers,
            List<JSONObject> valuations) {
        JSONObject security = mapper.security(securityId, bond, issuer, valuation, version);
        securities.put(securityId, security);
        String issuerId = security.getString("issuerId");
        if (issuerId != null) {
            issuers.put(issuerId, mapper.issuer(security, issuer));
        }
        JSONObject record = mapper.valuation(securityId, valuation);
        if (record != null) {
            valuations.add(record);
        }
    }

    private void checkGeneration(long expectedGeneration) {
        if (!transport.online()) {
            throw refreshFailure("BaseSvr client is offline; check service discovery and connection logs");
        }
        if (Thread.currentThread().isInterrupted()) {
            throw refreshFailure("BaseSvr refresh thread was interrupted");
        }
        if (generation != expectedGeneration) {
            throw refreshFailure("BaseSvr references changed during loading; retrying with a new generation");
        }
    }

    private static void success(JSONObject reply, String topic) {
        if (!"0000".equals(reply.getString("Code"))) {
            throw refreshFailure("BaseSvr topic=" + topic + " returned Code=" + reply.getString("Code"));
        }
    }

    private static JSONObject first(JSONObject reply, String topic, boolean required) {
        success(reply, topic);
        // Supplied BaseSvr handlers return Datas. Retain lowercase compatibility with older replies.
        String rowsField = reply.containsKey("Datas") ? "Datas" : "datas";
        JSONArray rows = reply.getJSONArray(rowsField);
        if (rows == null) {
            throw refreshFailure("BaseSvr topic=" + topic + " returned Code=0000 but no Datas array");
        }
        if (rows.isEmpty()) {
            if (required) {
                throw refreshFailure("BaseSvr topic=" + topic + " returned an empty Datas array for a configured bond");
            }
            return new JSONObject();
        }
        if (rows.size() != 1) {
            throw refreshFailure("BaseSvr topic=" + topic + " returned " + rows.size()
                    + " rows for a single-security request");
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

    private static ApiException refreshFailure(String message) {
        return ApiException.forField("1009", "NOT_READY", message, null);
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
