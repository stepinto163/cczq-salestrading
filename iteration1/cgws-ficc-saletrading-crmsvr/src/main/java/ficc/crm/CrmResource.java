package ficc.crm;

import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.resource.ResourceAdapter;

import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;

import com.adaptiveMQ2.utils.ConstsMessage;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import ficc.api.ProtocolBoundary;

import ficc.common.ApiException;
import ficc.common.ApiResponse;
import ficc.common.ComponentLog;
import ficc.common.JsonUtils;
import ficc.common.ServiceConfiguration;
import ficc.common.client.StcCrmClient;

import org.apache.log4j.Logger;

import java.util.List;
import java.util.Map;
import java.util.Properties;

/**
 * BBox resource: owns the CRM snapshot and answers every CRM topic.
 *
 * <p>{@code stc.crm.search} / {@code stc.crm.get} serve the FICC adapter; {@code stc.crm.inst.query},
 * {@code stc.crm.contact.query} and {@code stc.crm.account.query} serve the STC CRM image consumed
 * by CRMSvrClient, and {@code stc.crm.lookup} serves the per-name lookups parsersvr does.
 *
 * <p>The snapshot is loaded once at startup, so a CRM change reaches consumers when CRMSvr is
 * restarted; that matches how the deployment refreshes the legacy CRM image today.
 */
public final class CrmResource extends ResourceAdapter {
    private static final Logger LOGGER = Logger.getLogger(CrmResource.class);
    private Properties configuration;
    private CrmDirectory directory;

    @Override
    public void init() throws LifecycleException {
        try {
            configuration =
                    ServiceConfiguration.load(
                            "CRMSvr",
                            getConfig().getString("crm.properties"),
                            getConfig().getString("adapter.properties", ""));
            try (CrmStore store = new CrmStore(configuration)) {
                directory =
                        ComponentLog.initialize(
                                configuration,
                                "CRM_DIRECTORY",
                                "postgresql",
                                "CrmDirectory",
                                ComponentLog.databaseTarget(
                                        configuration, "crm.jdbcUrl", "crm.schema"),
                                () -> new CrmDirectory(store.load(), configuration));
            }
            super.init();
        } catch (Exception error) {
            throw new LifecycleException(error);
        }
    }

    public Map<String, Object> dispatch(String topic, String sid, String content) {
        String requestId = null;
        try {
            JSONObject request;
            try {
                request = ProtocolBoundary.request(topic, JsonUtils.parseObject(content));
            } catch (Exception error) {
                return ApiResponse.failure(
                        null, ApiException.forField("1001", "VALIDATION_FAILED", "Invalid JSON", null));
            }
            requestId = request.getString("requestId");
            if (directory == null) {
                throw ApiException.forField("1009", "NOT_READY", "CRMSvr is starting", null);
            }
            if (StcCrmClient.SEARCH_TOPIC.equals(topic)) {
                return directory.page(
                        request.getString("kind"),
                        request.getString("text"),
                        request.getString("institutionId"),
                        intValue(request, "offset", 0),
                        intValue(request, "limit", 20));
            }
            if (StcCrmClient.GET_TOPIC.equals(topic)) {
                return JsonUtils.object(
                        "record",
                        directory.find(request.getString("kind"), request.getString("id")));
            }
            if (CrmDirectory.LOOKUP_TOPIC.equals(topic)) {
                return page(
                        directory.lookup(
                                request.getString("kind"),
                                request.getString("name"),
                                request.getString("id"),
                                request.getString("match"),
                                intValue(request, "PageSize", 200)));
            }
            if (CrmDirectory.INST_QUERY_TOPIC.equals(topic)) {
                List<JSONObject> datas =
                        directory.institutionsByDealer(
                                request.getString("dealerStr"), intValue(request, "PageSize", 5000));
                return page(datas);
            }
            if (CrmDirectory.CONTACT_QUERY_TOPIC.equals(topic)) {
                List<JSONObject> datas =
                        directory.contactsByDealer(
                                request.getString("dealerStr"), intValue(request, "PageSize", 5000));
                return page(datas);
            }
            throw ApiException.forField(
                    "1002", "UNSUPPORTED_TOPIC", "Unsupported topic: " + topic, "topic");
        } catch (ApiException error) {
            return ApiResponse.failure(requestId, error);
        } catch (Exception error) {
            LOGGER.warn(
                    "CRM request failed: topic=" + topic + ", errorType=" + error.getClass().getName());
            return ApiResponse.failure(
                    requestId,
                    ApiException.forField("1009", "NOT_READY", "CRMSvr request failed", null));
        }
    }

    /** Snapshot reply for an image subscription: the whole CRM table of the topic. */
    public Map<String, Object> image(String topic, String sid, Map<String, Object> map) {
        map.put(ConstsMessage.SIGNAL_SRL, "0");
        try {
            if (directory == null) {
                throw ApiException.forField("1009", "NOT_READY", "CRMSvr is starting", null);
            }
            Object body;
            if (CrmDirectory.INST_QUERY_TOPIC.equals(topic)) {
                body = directory.institutionImage();
            } else if (CrmDirectory.CONTACT_QUERY_TOPIC.equals(topic)) {
                body = directory.contactImage();
            } else if (CrmDirectory.ACCOUNT_QUERY_TOPIC.equals(topic)) {
                body = directory.accountImage();
            } else {
                throw ApiException.forField(
                        "1002", "UNSUPPORTED_TOPIC", "Unsupported image topic: " + topic, "topic");
            }
            long started = System.nanoTime();
            map.put(topic, MsgHelper.Serializer(body, CharsetConvertType.None));
            LOGGER.info(
                    "CRM image: topic="
                            + topic
                            + ", sid="
                            + sid
                            + ", elapsedMillis="
                            + (System.nanoTime() - started) / 1000000);
        } catch (ApiException error) {
            LOGGER.warn("CRM image rejected: topic=" + topic + ", code=" + error.code);
        } catch (Exception error) {
            LOGGER.warn(
                    "CRM image failed: topic="
                            + topic
                            + ", errorType="
                            + error.getClass().getName());
        }
        return map;
    }

    private static JSONObject page(List<JSONObject> datas) {
        return JsonUtils.object(
                "Code",
                "0000",
                "Msg",
                "",
                "totalSize",
                datas.size(),
                "pageSize",
                datas.size(),
                "datas",
                array(datas));
    }

    private static JSONArray array(List<JSONObject> datas) {
        JSONArray records = new JSONArray();
        records.addAll(datas);
        return records;
    }

    private static int intValue(JSONObject request, String key, int fallback) {
        Integer value = request.getInteger(key);
        return value == null ? fallback : value.intValue();
    }
}
