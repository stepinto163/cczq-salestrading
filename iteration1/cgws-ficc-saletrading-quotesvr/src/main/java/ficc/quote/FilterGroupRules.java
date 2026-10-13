package ficc.quote;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

/** 产品 4.8.4：利率分组的历史禁用条件在加载、查询和保存时清理。 */
final class FilterGroupRules {
    private static final Set<String> RATE_EXCLUDED_FIELDS = new HashSet<>(Arrays.asList(
            "localGovernmentRegion", "issuerRating", "marketMakerTags", "perpetualType", "isPerpetual"));

    private FilterGroupRules() {
    }

    static void normalize(JSONObject group) {
        JSONArray subgroups = group.getJSONArray("subgroups");
        if (!"RATE".equals(group.getString("market")) || subgroups == null) {
            return;
        }
        JSONArray retained = new JSONArray();
        for (JSONObject subgroup : subgroups.toJavaList(JSONObject.class)) {
            JSONArray conditions = subgroup.getJSONArray("conditions");
            if (conditions == null) {
                continue;
            }
            JSONArray enabled = new JSONArray();
            for (JSONObject condition : conditions.toJavaList(JSONObject.class)) {
                if (!RATE_EXCLUDED_FIELDS.contains(condition.getString("field"))) {
                    enabled.add(condition);
                }
            }
            if (!enabled.isEmpty()) {
                subgroup.put("conditions", enabled);
                retained.add(subgroup);
            }
        }
        group.put("subgroups", retained);
    }
}
