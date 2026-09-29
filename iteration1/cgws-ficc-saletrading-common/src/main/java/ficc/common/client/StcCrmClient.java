package ficc.common.client;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.api.BaseData;
import ficc.api.CrmGetData;
import ficc.api.CrmGetRequest;
import ficc.api.CrmSearchRequest;
import ficc.api.ProtocolCodec;
import ficc.api.SalesTradingApi;

import ficc.common.ApiException;
import ficc.common.JsonUtils;

import org.apache.log4j.Logger;

import sc.app.stc.net.NetClientWrapper;

import java.util.List;
import java.util.Properties;

/**
 * CRM integration served by CRMSvr over the STC bus.
 *
 * <p>The caller keeps the exact contract of {@link PostgresCrmClient}: same record fields, same
 * {@code records/hasMore/nextOffset} page shape, same bounded-query restriction. Only the
 * transport changes — this client owns no database connection and no CRM table knowledge, so the
 * CRM schema stays behind CRMSvr and can evolve without redeploying every consumer.
 *
 * <p>Topics {@code stc.crm.search} and {@code stc.crm.get} are additional capabilities of the FICC
 * adapter, in the same way {@code stc.base.search} extends BaseSvr; the upstream STC protocol is
 * untouched.
 */
public final class StcCrmClient implements CrmClient {
    public static final String SEARCH_TOPIC = SalesTradingApi.CrmSearch.topic;
    public static final String GET_TOPIC = SalesTradingApi.CrmGet.topic;

    private static final Logger LOGGER = Logger.getLogger(StcCrmClient.class);

    private StcTransport transport;

    public StcCrmClient() {}

    /** Test seam: bypasses the STC network wrapper with an already configured transport. */
    StcCrmClient(StcTransport transport) {
        this.transport = transport;
    }

    @Override
    public void initialize(NetClientWrapper network, Properties configuration) {
        String serverKey = configuration.getProperty("crm.serverKey", "SERVER.CRMSvr");
        long timeoutMillis =
                Long.parseLong(configuration.getProperty("crm.requestTimeoutMillis", "3000"));
        if (timeoutMillis < 100 || timeoutMillis > 10000) {
            throw new IllegalArgumentException(
                    "crm.requestTimeoutMillis must be between 100 and 10000");
        }
        transport = new NetClientTransport(network, serverKey, timeoutMillis);
        LOGGER.info(
                "CRM STC client initialized; remote readiness checked on request: serverKey="
                        + serverKey
                        + ", timeoutMillis="
                        + timeoutMillis
                        + ", topics="
                        + SEARCH_TOPIC
                        + "/"
                        + GET_TOPIC);
    }

    @Override
    public JSONObject query(
            String userId, String kind, String text, String institutionId, int offset, int limit) {
        validateBounds(offset, limit);
        CrmSearchRequest query = new CrmSearchRequest();
        query.kind = kind;
        query.text = text == null ? "" : text;
        query.institutionId = institutionId;
        query.offset = offset;
        query.limit = limit;
        if (institutionId == null) {
            query.clearField("institutionId");
        }
        BaseData data = ProtocolCodec.decode(request(SEARCH_TOPIC, query.toJson()), BaseData.class);
        JSONObject reply = data.toJson();
        JSONArray records = reply.getJSONArray("records");
        boolean hasMore = Boolean.TRUE.equals(reply.getBoolean("hasMore"));
        Integer nextOffset = reply.getInteger("nextOffset");
        return JsonUtils.object(
                "records",
                records == null ? new JSONArray() : records,
                "hasMore",
                hasMore,
                "nextOffset",
                hasMore
                        ? (nextOffset == null ? Integer.valueOf(offset + limit) : nextOffset)
                        : null);
    }

    @Override
    public List<JSONObject> institutions(String userId) {
        throw new UnsupportedOperationException("Use bounded CRM query or lookup by institutionId");
    }

    @Override
    public List<JSONObject> contacts(String userId) {
        throw new UnsupportedOperationException("Use bounded CRM query or lookup by contactId");
    }

    @Override
    public JSONObject institution(String userId, String institutionId) {
        return find("INSTITUTION", institutionId);
    }

    @Override
    public JSONObject contact(String userId, String contactId) {
        return find("CONTACT", contactId);
    }

    private JSONObject find(String kind, String id) {
        if (id == null || id.trim().isEmpty()) {
            return null;
        }
        CrmGetRequest query = new CrmGetRequest();
        query.kind = kind;
        query.id = id.trim();
        CrmGetData data = ProtocolCodec.decode(request(GET_TOPIC, query.toJson()), CrmGetData.class);
        JSONObject record = data.record == null ? null : data.record.toJson();
        return record == null || record.isEmpty() ? null : record;
    }

    private JSONObject request(String topic, JSONObject payload) {
        if (transport == null) {
            throw ApiException.forField(
                    "1009", "NOT_READY", "CRM client is not initialized", null);
        }
        long started = System.nanoTime();
        JSONObject reply;
        try {
            reply = transport.request(topic, null, payload);
        } catch (ApiException error) {
            LOGGER.warn(
                    "CRM request failed: topic="
                            + topic
                            + ", code="
                            + error.code
                            + ", elapsedMillis="
                            + (System.nanoTime() - started) / 1_000_000);
            throw error;
        }
        if (reply == null) {
            throw ApiException.forField("1009", "NOT_READY", "CRMSvr returned no reply", null);
        }
        String code = reply.getString("Code");
        if (code != null && !"0000".equals(code)) {
            throw new ApiException(code, reply.getString("Msg"), reply.getJSONObject("details"));
        }
        LOGGER.debug("CRM request completed: topic=" + topic + ", payload=" + JsonUtils.toJson(payload));
        return reply;
    }

    private static void validateBounds(int offset, int limit) {
        if (offset < 0 || offset > 1000000 || limit < 1 || limit > 200) {
            throw ApiException.forField(
                    "1001", "VALIDATION_FAILED", "CRM offset/limit out of range", "limit");
        }
    }
}
