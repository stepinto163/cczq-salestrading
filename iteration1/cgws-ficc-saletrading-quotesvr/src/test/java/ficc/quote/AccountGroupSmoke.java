package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.Filter;
import ficc.api.ProtocolCodec;
import ficc.api.QuoteRecord;
import ficc.api.SettingRecord;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.function.Predicate;

/** 小样本功能验证，无网络、数据库或压力测试。 */
public final class AccountGroupSmoke {
    private static int checks;

    private static void check(boolean matched, String description) {
        if (!matched) {
            throw new AssertionError(description);
        }
        checks++;
        System.out.println("PASS " + description);
    }

    private static QuoteRecord quote(String securityId, String side) {
        return ProtocolCodec.decode(JsonUtils.object("securityId", securityId, "side", side), QuoteRecord.class);
    }

    private static JSONObject condition(String field, String operator, Object value) {
        return JsonUtils.object("field", field, "operator", operator, "value", value);
    }

    private static Filter filter(Object... entries) {
        return ProtocolCodec.decode(JsonUtils.object(entries), Filter.class);
    }

    public static void main(String[] args) {
        Map<String, SettingRecord> settings = new LinkedHashMap<>();
        JSONObject group = JsonUtils.object("filterGroupId", "FG-1", "userId", "U-1", "market", "RATE",
                "subgroups", JsonUtils.array(JsonUtils.object("conditions", JsonUtils.array(
                        condition("issuerRating", "EQ", "AAA"),
                        condition("remainingYears", "BETWEEN", JsonUtils.array(1, 3)))),
                        JsonUtils.object("conditions", JsonUtils.array(condition("isPerpetual", "EQ", true)))));
        FilterGroupRules.normalize(group);
        check(group.getJSONArray("subgroups").size() == 1, "drop empty legacy subgroup");
        check(group.getJSONArray("subgroups").getJSONObject(0).getJSONArray("conditions").size() == 1,
                "remove prohibited rate condition");
        settings.put("FG-1", ProtocolCodec.decode(group, SettingRecord.class));
        settings.put("W-1", ProtocolCodec.decode(JsonUtils.object("watchlistId", "W-1", "userId", "U-1",
                "market", "RATE", "securities", JsonUtils.array(JsonUtils.object("securityId", "250001.IB"))), SettingRecord.class));
        settings.put("W-2", ProtocolCodec.decode(JsonUtils.object("watchlistId", "W-2", "userId", "U-1",
                "market", "RATE", "securities", JsonUtils.array(JsonUtils.object("securityId", "250002.IB"))), SettingRecord.class));
        QuoteMatcher matcher = new QuoteMatcher(new ReferenceData(), settings);
        Predicate<QuoteRecord> groupOnly = matcher.prepare("U-1", filter("filterGroupIds", JsonUtils.array("FG-1")));
        check(groupOnly.test(quote("250001.IB", "1")), "rate group matches rate quote");
        check(!groupOnly.test(quote("102500001.IB", "1")), "rate group cannot match credit quote in combined view");
        Predicate<QuoteRecord> combined = matcher.prepare("U-1", filter("watchlistIds", JsonUtils.array("W-1", "W-2"),
                "filterGroupIds", JsonUtils.array("FG-1"), "conditions", JsonUtils.array(condition("side", "EQ", "1"))));
        check(combined.test(quote("250001.IB", "1")) && combined.test(quote("250002.IB", "1")), "watchlists use OR");
        check(!combined.test(quote("240210.IB", "1")), "watchlist and filter group use AND");
        check(!combined.test(quote("250001.IB", "2")), "temporary condition also uses AND");
        try {
            matcher.prepare("U-2", filter("filterGroupIds", JsonUtils.array("FG-1")));
            throw new AssertionError("other user read group");
        } catch (ApiException expected) {
            check(true, "account isolation");
        }
        Filter alternatives = filter("conditions", JsonUtils.array(condition("remainingYears", "BETWEEN", JsonUtils.array(0, 1)),
                condition("remainingYears", "BETWEEN", JsonUtils.array(2, 3)), condition("side", "EQ", "1")));
        check(matcher.prepare("U-1", alternatives).test(quote("250001.IB", "1")), "same field OR across conditions");
        check(!matcher.prepare("U-1", alternatives).test(quote("250001.IB", "2")), "different fields AND");
        System.out.println("AccountGroupSmoke PASSED checks=" + checks);
    }
}
