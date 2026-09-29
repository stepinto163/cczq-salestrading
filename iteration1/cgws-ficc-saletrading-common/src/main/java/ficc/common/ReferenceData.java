package ficc.common;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.ProtocolCodec;
import ficc.api.ReferenceRecord;

import ficc.common.client.BaseDataClient;
import ficc.common.client.CrmClient;
import ficc.common.client.SimulatorBaseDataClient;
import ficc.common.client.SimulatorCrmClient;

import org.apache.log4j.Logger;

import java.util.Arrays;
import java.util.List;
import java.util.Map;

/** Business facade; vendor fields and transport stay inside adapters. */
public final class ReferenceData implements AutoCloseable {
    private static final Logger LOGGER = Logger.getLogger(ReferenceData.class);
    private final BaseDataClient base;
    private final CrmClient crm;

    public ReferenceData() {
        this(new SimulatorBaseDataClient(), new SimulatorCrmClient());
    }

    public ReferenceData(BaseDataClient base, CrmClient crm) {
        this.base = base;
        this.crm = crm;
    }

    public Map<String, JSONObject> securities() {
        return base.securities();
    }

    public String version() {
        return base.version();
    }

    public com.alibaba.fastjson2.JSONArray filterCatalog() {
        com.alibaba.fastjson2.JSONArray fields = FilterFields.catalog();
        Map<String, JSONObject> currentSecurities = securities();
        for (Object item : fields) {
            JSONObject field = (JSONObject) item;
            String name = field.getString("field");
            java.util.Set<String> values = new java.util.TreeSet<>();
            boolean simulated = false;
            for (JSONObject security : currentSecurities.values()) {
                Object value = security.get(name);
                if ("city".equals(name) && value != null) {
                    com.alibaba.fastjson2.JSONArray options = field.getJSONArray("enumValues");
                    boolean known =
                            options.stream()
                                    .anyMatch(
                                            option ->
                                                    value.equals(
                                                            ((JSONObject) option)
                                                                    .getString("value")));
                    if (!known) {
                        options.add(
                                JsonUtils.object(
                                        "value",
                                        value,
                                        "label",
                                        security.getString("cityName") == null
                                                ? value
                                                : security.getString("cityName")));
                    }
                }
                if (values.size() < 100 && value != null) {
                    if (value instanceof java.util.Collection<?>) {
                        java.util.Collection<?> list = (java.util.Collection<?>) value;
                        list.forEach(entry -> values.add(entry.toString()));
                    } else {
                        values.add(value.toString());
                    }
                }
                JSONObject sources = security.getJSONObject("filterDataSources");
                simulated |= sources != null && "SIMULATOR".equals(sources.getString(name));
            }
            field.put("examples", values);
            field.put("includesSimulatedData", simulated);
        }
        return fields;
    }

    public JSONObject require(String id) {
        JSONObject security = base.security(id);
        if (security == null) {
            LOGGER.warn(
                    "Reference lookup missed: securityId="
                            + JsonUtils.toJson(id)
                            + ", adapter="
                            + base.getClass().getSimpleName());
            throw ApiException.forField(
                    "1001",
                    "VALIDATION_FAILED",
                    "Security is not in the configured reference universe",
                    "securityId");
        }
        return security;
    }

    public ReferenceRecord requireRecord(String securityId) {
        return ProtocolCodec.decode(require(securityId), ReferenceRecord.class);
    }

    public JSONObject enrich(JSONObject quote) {
        JSONObject result = JsonUtils.deepCopy(quote);
        String securityId = quote.getString("securityId");
        if (securityId != null) {
            result.putAll(require(securityId));
        }
        boolean bid = "1".equals(quote.getString("side"));
        StringBuilder searchText = new StringBuilder();
        for (String field :
                Arrays.asList(
                        "securityId",
                        "securityName",
                        "issuerName",
                        "institutionName",
                        "contactName",
                        "ownerId",
                        "ownerDisplayName")) {
            if (result.get(field) != null) {
                searchText.append(result.getString(field)).append(' ');
            }
        }
        result.put("searchText", searchText.toString());
        result.put(bid ? "bidPrice" : "offerPrice", quote.get("price"));
        result.put(bid ? "bidQuantity" : "offerQuantity", quote.get("quantity"));
        // Only compare yields with the same maturity/exercise basis; unknown basis stays absent.
        if ("1".equals(quote.getString("quoteType"))
                && result.get("valuationYield") != null
                && quote.get("price") != null
                && result.get("valuationBasis") != null
                && ("2".equals(quote.getString("yieldType"))
                        ? "EXERCISE".equals(result.getString("valuationBasis"))
                        : "MATURITY".equals(result.getString("valuationBasis")))) {
            java.math.BigDecimal spread =
                    quote.getBigDecimal("price")
                            .subtract(result.getBigDecimal("valuationYield"))
                            .multiply(java.math.BigDecimal.valueOf(100));
            result.put(
                    bid ? "bidValuationSpread" : "offerValuationSpread",
                    bid ? spread : spread.negate());
        }
        // Clean-price differences have price units; never mix them into the BP fields.
        if ("2".equals(quote.getString("quoteType"))
                && result.get("valuationCleanPrice") != null
                && quote.get("price") != null) {
            java.math.BigDecimal spread =
                    quote.getBigDecimal("price")
                            .subtract(result.getBigDecimal("valuationCleanPrice"));
            result.put(
                    bid ? "bidCleanPriceSpread" : "offerCleanPriceSpread",
                    bid ? spread : spread.negate());
        }
        return result;
    }

    public List<JSONObject> records(String kind) {
        return records(kind, null);
    }

    public List<JSONObject> records(String kind, String userId) {
        if ("INSTITUTION".equals(kind)) return crm.institutions(userId);
        if ("CONTACT".equals(kind)) return crm.contacts(userId);
        return base.records(kind);
    }

    public JSONObject institution(String userId, String institutionId) {
        return crm.institution(userId, institutionId);
    }

    public JSONObject queryCrm(
            String userId, String kind, String text, String institutionId, int offset, int limit) {
        return crm.query(userId, kind, text, institutionId, offset, limit);
    }

    public JSONObject security(String securityId) {
        return base.security(securityId);
    }

    public List<JSONObject> querySecurities(String text) {
        return base.querySecurities(text);
    }

    public JSONObject contact(String userId, String contactId) {
        return crm.contact(userId, contactId);
    }

    @Override
    public void close() throws Exception {
        try {
            base.close();
        } finally {
            crm.close();
        }
    }
}
