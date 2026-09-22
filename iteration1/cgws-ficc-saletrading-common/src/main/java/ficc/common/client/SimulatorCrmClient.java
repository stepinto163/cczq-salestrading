package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.JsonUtils;

import java.util.Arrays;
import java.util.List;

public final class SimulatorCrmClient implements CrmClient {
    @Override
    public List<JSONObject> institutions(String userId) {
        return Arrays.asList(
                JsonUtils.object("institutionId", "I-001", "institutionName", "示例对手机构"),
                JsonUtils.object("institutionId", "I-002", "institutionName", "示例证券公司"));
    }

    @Override
    public List<JSONObject> contacts(String userId) {
        return Arrays.asList(
                JsonUtils.object(
                        "contactId", "C-001", "institutionId", "I-001", "contactName", "示例联系人"),
                JsonUtils.object(
                        "contactId", "C-002", "institutionId", "I-002", "contactName", "示例联系人"));
    }
}
