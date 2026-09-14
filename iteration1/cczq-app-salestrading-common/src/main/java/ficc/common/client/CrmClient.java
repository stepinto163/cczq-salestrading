package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import sc.app.stc.net.NetClientWrapper;
import java.util.List;
import java.util.Properties;

/** Extension contract for an independently built FICC CRM adapter JAR. */
public interface CrmClient extends AutoCloseable {
    default void initialize(NetClientWrapper network, Properties configuration) throws Exception {}

    /** Return only records this authenticated STC user may select. */
    List<JSONObject> institutions(String userId);
    List<JSONObject> contacts(String userId);

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
