package sc.app.stc.parser.utils;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONArray;
import com.alibaba.fastjson.JSONObject;

import sc.app.stc.api.TBrokerCrmContact;
import sc.app.stc.api.TBrokerCrmInst;

import java.util.Collections;
import java.util.List;

/**
 * Wire format of the CRM topics CRMSvr serves to parsersvr.
 *
 * <p>Replaces {@code sc.app.stc.utils.CRMSvrClient}, which mirrored the whole CRM into this JVM
 * through an image subscription. parsersvr only ever resolves one parsed name or one id, so it now
 * asks CRMSvr for those rows over {@code stc.crm.lookup}, for institutions and traders alike.
 *
 * <p>A reply is {@code {Code, Msg, totalSize, pageSize, datas}}. Anything else — blank body, bad
 * JSON, a {@code Code} other than 0000 — is treated as "no match" so parsing never fails because
 * CRM is unavailable.
 */
public final class FiccCrmProtocol {

    public static final String LOOKUP_TOPIC = "stc.crm.lookup";

    public static final String INSTITUTION = "INSTITUTION";
    public static final String CONTACT = "CONTACT";
    public static final String EXACT = "exact";
    public static final String CONTAINS = "contains";
    public static final String OK = "0000";

    private FiccCrmProtocol() {}

    /** Request body of {@code stc.crm.lookup}; {@code id} wins over {@code name} on the server. */
    public static String lookupRequest(
            String kind, String name, String id, String match, int pageSize) {
        JSONObject request = new JSONObject(true);
        request.put("kind", kind);
        if (name != null) {
            request.put("name", name);
        }
        if (id != null) {
            request.put("id", id);
        }
        request.put("match", match);
        request.put("PageSize", pageSize);
        return request.toJSONString();
    }

    public static List<TBrokerCrmInst> institutions(String reply) {
        JSONArray datas = datas(reply);
        if (datas == null) {
            return Collections.emptyList();
        }
        List<TBrokerCrmInst> rows = JSON.parseArray(datas.toJSONString(), TBrokerCrmInst.class);
        return rows == null ? Collections.<TBrokerCrmInst>emptyList() : rows;
    }

    public static List<TBrokerCrmContact> contacts(String reply) {
        JSONArray datas = datas(reply);
        if (datas == null) {
            return Collections.emptyList();
        }
        List<TBrokerCrmContact> rows =
                JSON.parseArray(datas.toJSONString(), TBrokerCrmContact.class);
        return rows == null ? Collections.<TBrokerCrmContact>emptyList() : rows;
    }

    private static JSONArray datas(String reply) {
        JSONObject body = body(reply);
        return body == null ? null : body.getJSONArray("datas");
    }

    /**
     * Parses a CRMSvr reply, or null when there is nothing usable in it.
     *
     * <p>A missing {@code Code} is not an error.
     */
    private static JSONObject body(String reply) {
        if (reply == null || reply.trim().isEmpty()) {
            return null;
        }
        JSONObject body;
        try {
            body = JSON.parseObject(reply);
        } catch (Exception error) {
            return null;
        }
        if (body == null) {
            return null;
        }
        String code = body.getString("Code");
        if (code != null && !OK.equals(code)) {
            return null;
        }
        return body;
    }
}
