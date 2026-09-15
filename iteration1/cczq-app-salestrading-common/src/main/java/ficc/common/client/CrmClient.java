package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import sc.app.stc.net.NetClientWrapper;
import java.util.List;
import java.util.Properties;
import ficc.common.JsonUtils;
import java.util.Locale;

/** Extension contract for an independently built FICC CRM adapter JAR. */
public interface CrmClient extends AutoCloseable {
    default void initialize(NetClientWrapper network, Properties configuration) throws Exception {}

    /** Return only records this authenticated STC user may select. */
    List<JSONObject> institutions(String userId);
    List<JSONObject> contacts(String userId);

    default JSONObject query(String userId, String kind, String text, String institutionId, int offset, int limit) {
        String keyword = text.toLowerCase(Locale.ROOT);
        List<JSONObject> matches = ("INSTITUTION".equals(kind) ? institutions(userId) : contacts(userId)).stream()
                .filter(record -> "INSTITUTION".equals(kind) || institutionId == null || belongsTo(record, institutionId))
                .filter(record -> JsonUtils.toJson(record).toLowerCase(Locale.ROOT).contains(keyword)).toList();
        int end = Math.min(matches.size(), offset + limit);
        return JsonUtils.object("records", matches.subList(Math.min(offset, end), end),
                "hasMore", end < matches.size(), "nextOffset", end < matches.size() ? end : null);
    }

    static boolean belongsTo(JSONObject contact, String institutionId) {
        return institutionId != null && (institutionId.equals(contact.getString("institutionId"))
                || (contact.getJSONArray("institutionIds") != null
                    && contact.getJSONArray("institutionIds").contains(institutionId)));
    }

    default JSONObject institution(String userId, String institutionId) {
        return institutions(userId).stream()
                .filter(record -> institutionId.equals(record.getString("institutionId")))
                .findFirst().orElse(null);
    }

    default JSONObject contact(String userId, String contactId) {
        return contacts(userId).stream()
                .filter(record -> contactId.equals(record.getString("contactId")))
                .findFirst().orElse(null);
    }

    @Override
    default void close() throws Exception {}
}
