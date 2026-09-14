package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import java.util.List;

/** Integration placeholder: no CRM records and no invented identities. */
public final class EmptyCrmClient implements CrmClient {
    @Override
    public List<JSONObject> institutions(String userId) {
        return List.of();
    }

    @Override
    public List<JSONObject> contacts(String userId) {
        return List.of();
    }
}
