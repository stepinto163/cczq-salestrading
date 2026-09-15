package ficc.common;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.client.BaseDataClient;
import ficc.common.client.CrmClient;
import ficc.common.client.SimulatorBaseDataClient;
import ficc.common.client.SimulatorCrmClient;
import java.util.List;
import java.util.Map;
import org.apache.log4j.Logger;

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

    public JSONObject require(String id) {
        Map<String, JSONObject> loaded = securities();
        JSONObject security = loaded.get(id);
        if (security == null) {
            LOGGER.warn("Reference lookup missed: securityId=" + JsonUtils.toJson(id)
                    + ", loadedSecurities=" + loaded.size() + ", adapter=" + base.getClass().getSimpleName());
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Security is not in the configured reference universe", "securityId");
        }
        return security;
    }

    public JSONObject enrich(JSONObject quote) {
        JSONObject result = JsonUtils.deepCopy(quote);
        String securityId = quote.getString("securityId");
        if (securityId != null) {
            result.putAll(require(securityId));
        }
        return result;
    }

    public List<JSONObject> records(String kind) {
        return records(kind, null);
    }

    public List<JSONObject> records(String kind, String userId) {
        return switch (kind) {
            case "INSTITUTION" -> crm.institutions(userId);
            case "CONTACT" -> crm.contacts(userId);
            default -> base.records(kind);
        };
    }

    public JSONObject institution(String userId, String institutionId) {
        return crm.institution(userId, institutionId);
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
