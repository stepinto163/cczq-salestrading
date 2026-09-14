package cczq.salestrading.crm;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.client.CrmClient;
import sc.app.stc.net.NetClientWrapper;
import java.util.List;
import java.util.Properties;

/** Internal-team extension point. Empty until the real CRM contract is implemented. */
public final class InternalCrmClient implements CrmClient {
    @Override
    public void initialize(NetClientWrapper network, Properties configuration) {
        // Implement CRM setup here using network and crm.* configuration.
        // NetClientResource owns the shared network: do not dispose it here.
    }

    @Override
    public List<JSONObject> institutions(String userId) {
        return List.of();
    }

    @Override
    public List<JSONObject> contacts(String userId) {
        return List.of();
    }
}
