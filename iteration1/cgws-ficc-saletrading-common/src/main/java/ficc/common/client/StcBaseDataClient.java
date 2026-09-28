package ficc.common.client;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import ficc.common.ApiException;
import ficc.common.JsonUtils;

import org.apache.log4j.Logger;

import sc.app.stc.net.NetClientWrapper;

import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.TimeUnit;

/** Maps STC objects; STC owns loading, caching and invalidation. */
public final class StcBaseDataClient implements BaseDataClient {
    interface Source {
        void start() throws Exception;

        boolean online();

        JSONObject bond(String securityId);

        JSONObject issuer(String issuerId);

        JSONObject valuation(String securityId);

        JSONObject searchPage(int page, int pageSize);
    }

    private static final Logger LOGGER = Logger.getLogger(StcBaseDataClient.class);
    private static final long MAX_SECURITY_CACHE_TTL_MILLIS = 600_000L;
    private final Source source;
    private final StcBaseMapper mapper;
    private final long securityCacheTtlNanos;
    private final Map<String, CachedSecurity> securityCache = new ConcurrentHashMap<>();
    private final Map<String, JSONObject> mappedSecurities = new ConcurrentHashMap<>();
    private final Map<String, JSONObject> mappedIssuers = new ConcurrentHashMap<>();
    private final Map<String, JSONObject> mappedValuations = new ConcurrentHashMap<>();
    private volatile String cachedVersion;

    public StcBaseDataClient(NetClientWrapper network, Properties configuration) {
        this(new StcBaseSvrClientBridge(network, configuration), configuration);
    }

    StcBaseDataClient(Source source, Properties configuration) {
        this.source = source;
        mapper = new StcBaseMapper(configuration);
        long cacheTtlMillis =
                Long.parseLong(configuration.getProperty("base.securityCacheTtlMillis", "0"));
        if (cacheTtlMillis < 0 || cacheTtlMillis > MAX_SECURITY_CACHE_TTL_MILLIS) {
            throw new IllegalArgumentException(
                    "base.securityCacheTtlMillis must be between 0 and "
                            + MAX_SECURITY_CACHE_TTL_MILLIS);
        }
        securityCacheTtlNanos = TimeUnit.MILLISECONDS.toNanos(cacheTtlMillis);
        if (Boolean.parseBoolean(
                configuration.getProperty("base.filter.simulateMissing", "false"))) {
            LOGGER.info(
                    "BaseSvr filter fields: missing vendor attributes use SIMULATOR values;"
                            + " filterDataSources marks each field");
        }
        if (!configuration.getProperty("base.securityIds", "").trim().isEmpty()) {
            LOGGER.warn(
                    "base.securityIds is obsolete and ignored; STC bond lookup is unrestricted");
        }
    }

    @Override
    public void start() throws Exception {
        source.start();
        LOGGER.info(
                "BaseSvr adapter initialized: BaseSvrClient request mode -> BaseSvr2Client;"
                        + " lookup=on-demand; securityCacheTtlMillis="
                        + TimeUnit.NANOSECONDS.toMillis(securityCacheTtlNanos));
    }

    private void ensureOnline() {
        if (!source.online()) {
            throw ApiException.forField("1009", "NOT_READY", "STC BaseSvr is offline", null);
        }
    }

    @Override
    public JSONObject security(String securityId) {
        ensureOnline();
        // STC already memoises the three underlying lookups, but re-deriving the mapped
        // record costs a full JSON round trip plus the record hash on every call. A single
        // client request fans out over every active quote, so the same security is resolved
        // hundreds of times. The memo is opt-in: STC invalidates its own caches, and a TTL
        // would hide those updates, so it stays off unless base.securityCacheTtlMillis is set.
        if (securityCacheTtlNanos == 0) {
            return loadSecurity(securityId);
        }
        long now = System.nanoTime();
        CachedSecurity cached = securityCache.get(securityId);
        if (cached != null && cached.expiresAtNanos - now > 0) {
            return JsonUtils.deepCopy(cached.value);
        }
        JSONObject loaded = loadSecurity(securityId);
        if (loaded == null) {
            securityCache.remove(securityId);
        } else {
            securityCache.put(
                    securityId,
                    new CachedSecurity(now + securityCacheTtlNanos, JsonUtils.deepCopy(loaded)));
        }
        return loaded;
    }

    private JSONObject loadSecurity(String securityId) {
        JSONObject bond = source.bond(securityId);
        if (bond == null) {
            mappedSecurities.remove(securityId);
            mappedValuations.remove(securityId);
            securityCache.remove(securityId);
            cachedVersion = null;
            LOGGER.warn("STC BaseSvr bond not found: securityId=" + JsonUtils.toJson(securityId));
            return null;
        }
        return mapSecurity(securityId, bond);
    }

    private JSONObject mapSecurity(String securityId, JSONObject bond) {
        String issuerId = StcBaseMapper.text(bond, "Issuer_Code");
        JSONObject issuer = issuerId == null ? null : source.issuer(issuerId);
        JSONObject valuation = source.valuation(securityId);
        ensureOnline();
        String recordVersion =
                "STC-"
                        + JsonUtils.canonicalHash(
                                JsonUtils.object(
                                        "bond", bond, "issuer", issuer, "valuation", valuation));
        JSONObject result = mapper.security(securityId, bond, issuer, valuation, recordVersion);
        mappedSecurities.put(securityId, result);
        if (issuerId != null) {
            mappedIssuers.put(issuerId, mapper.issuer(result, issuer));
        }
        JSONObject mappedValuation = mapper.valuation(securityId, valuation);
        if (mappedValuation == null) {
            mappedValuations.remove(securityId);
        } else {
            mappedValuations.put(securityId, mappedValuation);
        }
        cachedVersion = null;
        return result;
    }

    @Override
    public Map<String, JSONObject> securities() {
        ensureOnline();
        Map<String, JSONObject> current = new LinkedHashMap<>();
        for (String id : new ArrayList<>(mappedSecurities.keySet())) {
            JSONObject security = security(id);
            if (security != null) {
                current.put(id, security);
            }
        }
        return current;
    }

    @Override
    public List<JSONObject> querySecurities(String text) {
        if (text != null && text.matches("[^\\s]+\\.(IB|SH|SZ)")) {
            JSONObject security = security(text);
            return security == null ? Collections.emptyList() : Collections.singletonList(security);
        }
        ensureOnline();
        String keyword = text == null ? "" : text.toLowerCase(java.util.Locale.ROOT);
        List<JSONObject> matches = new ArrayList<>();
        int expectedTotal = -1;
        int received = 0;
        long deadline = System.nanoTime() + java.util.concurrent.TimeUnit.SECONDS.toNanos(3);
        java.util.Set<String> seen = new java.util.HashSet<>();
        for (int page = 1; ; page++) {
            if (System.nanoTime() > deadline) {
                throw ApiException.forField(
                        "1009",
                        "NOT_READY",
                        "Bond search exceeded its time budget; use an exact securityId",
                        null);
            }
            JSONObject reply = source.searchPage(page, 500);
            if (reply == null) {
                throw ApiException.forField(
                        "1009", "NOT_READY", "BaseSvr bond search returned no reply", null);
            }
            JSONArray rows = reply.getJSONArray("Datas");
            Integer total = reply.getInteger("Total");
            if (!"0000".equals(reply.getString("Code"))
                    || rows == null
                    || rows.size() > 500
                    || total == null
                    || total < 0
                    || total > 500000
                    || (expectedTotal >= 0 && total != expectedTotal)) {
                throw ApiException.forField(
                        "1009",
                        "NOT_READY",
                        "BaseSvr bond search returned an invalid or changing page",
                        null);
            }
            expectedTotal = total;
            for (Object item : rows) {
                JSONObject bond = (JSONObject) item;
                String id = StcBaseMapper.securityId(bond);
                if (id == null) {
                    continue;
                }
                if (!seen.add(id)) {
                    throw ApiException.forField(
                            "1009",
                            "NOT_READY",
                            "BaseSvr bond search returned duplicate securities",
                            null);
                }
                String searchable =
                        id
                                + " "
                                + StcBaseMapper.text(bond, "Short_Name")
                                + " "
                                + StcBaseMapper.text(bond, "pinyin")
                                + " "
                                + StcBaseMapper.text(bond, "pinyin_full");
                if (searchable.toLowerCase(java.util.Locale.ROOT).contains(keyword)) {
                    matches.add(bond);
                    if (matches.size() > 10000) {
                        throw ApiException.forField(
                                "1001",
                                "VALIDATION_FAILED",
                                "Too many matching bonds; narrow the search",
                                "text");
                    }
                }
            }
            received += rows.size();
            if (received == expectedTotal) {
                break;
            }
            if (received > expectedTotal || rows.size() < 500) {
                throw ApiException.forField(
                        "1009",
                        "NOT_READY",
                        "BaseSvr bond search returned an incomplete dictionary",
                        null);
            }
        }
        // A selector needs bond fields, not a separate issuer/valuation RPC per row.
        List<JSONObject> results = new ArrayList<>();
        for (JSONObject bond : matches) {
            String id = StcBaseMapper.securityId(bond);
            results.add(
                    mapper.security(id, bond, null, null, "STC-" + JsonUtils.canonicalHash(bond)));
        }
        return results;
    }

    @Override
    public List<JSONObject> records(String kind) {
        Map<String, JSONObject> current = securities();
        if ("SECURITY".equals(kind)) {
            return Collections.unmodifiableList(new ArrayList<>(current.values()));
        }
        if ("ISSUER".equals(kind)) {
            return Collections.unmodifiableList(new ArrayList<>(mappedIssuers.values()));
        }
        if ("DAILY_VALUATION".equals(kind)) {
            return Collections.unmodifiableList(new ArrayList<>(mappedValuations.values()));
        }
        throw ApiException.forField("1001", "VALIDATION_FAILED", "Unknown reference kind", "kind");
    }

    @Override
    public String version() {
        ensureOnline();
        // Hashes every mapped record and every issuer and valuation under it, so it is only
        // recomputed when one of those maps actually changed.
        String cached = cachedVersion;
        if (cached == null) {
            cached = "STC-" + JsonUtils.canonicalHash(mappedSecurities);
            cachedVersion = cached;
        }
        return cached;
    }

    /** Memo of a mapped record; never handed out directly so callers cannot mutate it. */
    private static final class CachedSecurity {
        final long expiresAtNanos;
        final JSONObject value;

        CachedSecurity(long expiresAtNanos, JSONObject value) {
            this.expiresAtNanos = expiresAtNanos;
            this.value = value;
        }
    }
}
