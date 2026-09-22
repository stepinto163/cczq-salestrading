package ficc.common;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import ficc.common.client.SimulatorBaseDataClient;

import java.util.Map;

/** Bounded, offline contract check, also runnable without the corporate Maven repository. */
public final class FilterDictionaryCheck {
    private static int checks;

    public static void main(String[] args) {
        check(FilterFields.catalog().size() == 79, "all fields");
        check(
                FilterDictionaries.dictionaries().size() == 41,
                "37 enums and four region dictionaries");
        for (Object entry : FilterFields.catalog()) {
            JSONObject field = (JSONObject) entry;
            if (field.getJSONArray("enumValues") == null) {
                continue;
            }
            String name = field.getString("field");
            check(!field.getJSONArray("operators").contains("CONTAINS"), "enum operators " + name);
            for (Object choice : field.getJSONArray("enumValues")) {
                JSONObject option = (JSONObject) choice;
                JSONObject filter = filter(name, "EQ", option.get("value"));
                FilterEngine.validate(filter);
                check(
                        FilterEngine.match(JsonUtils.object(name, option.get("value")), filter),
                        name);
            }
        }
        JSONObject legacy =
                JsonUtils.object(
                        "subgroups",
                        JsonUtils.array(
                                JsonUtils.object(
                                        "conditions",
                                        JsonUtils.array(
                                                condition(
                                                        "ownershipType",
                                                        "IN",
                                                        JsonUtils.array("央企", "国企")),
                                                condition("region", "CONTAINS", "上海")))));
        FilterDictionaries.migrateSavedFilters(legacy);
        JSONArray conditions =
                legacy.getJSONArray("subgroups").getJSONObject(0).getJSONArray("conditions");
        FilterEngine.validate(JsonUtils.object("conditions", conditions));
        check(
                "YQ".equals(conditions.getJSONObject(0).getJSONArray("value").getString(0)),
                "saved labels");
        check(
                "EQ".equals(conditions.getJSONObject(1).getString("operator")),
                "saved region operation");
        check("310000".equals(conditions.getJSONObject(1).getString("value")), "saved region code");

        JSONObject source =
                JsonUtils.object(
                        "ownershipType",
                        "CGE",
                        "couponType",
                        "FRB",
                        "couponFrequency",
                        "A",
                        "region",
                        "广东省",
                        "guaranteeType",
                        "全国性担保");
        FilterDictionaries.normalizeAttributes(source);
        check("YQ".equals(source.getString("ownershipType")), "institution subtype");
        check("FIXED".equals(source.getString("couponType")), "coupon source mapping");
        check("Y1".equals(source.getString("couponFrequency")), "coupon protocol mapping");
        check("440000".equals(source.getString("region")), "province");
        check(FilterEngine.match(source, filter("guaranteeType", "EQ", "YDB")), "guarantee parent");
        source.put("ownershipType", "LTCO");
        FilterDictionaries.normalizeAttributes(source);
        check(!source.containsKey("ownershipType"), "legal form must not become ownership");
        check(
                !FilterEngine.match(source, filter("ownershipType", "EQ", "Other")),
                "unknown not Other");
        JSONObject bond =
                JsonUtils.object(
                        "Bond_ID",
                        "123456.SZ",
                        "Listed_Market",
                        "SZE",
                        "Bond_Subtype",
                        "CMN",
                        "Option_Type",
                        "NON",
                        "Coupon_Type",
                        "FRB",
                        "Coupon_Frequency",
                        "A");
        JSONObject issuer =
                JsonUtils.object(
                        "institution_subtype",
                        "CGE",
                        "org_type",
                        "LTCO",
                        "province",
                        "广东",
                        "city",
                        "深圳市",
                        "city_code",
                        "440300");
        JSONObject mapped =
                new ficc.common.client.StcBaseMapper(new java.util.Properties())
                        .security("123456.SZ", bond, issuer, new JSONObject(), "TEST");
        check(
                "YQ".equals(mapped.getString("ownershipType")),
                "mapper uses institution subtype, not legal form");
        check("SZSE".equals(mapped.getString("listedMarket")), "STC exchange alias");
        check("MTN".equals(mapped.getString("bondSubtype")), "STC bond subtype to frontend code");
        check("Y1".equals(mapped.getString("couponFrequency")), "STC annual frequency");
        check("440300".equals(mapped.getString("city")), "STC city code");
        rejects(filter("ownershipType", "EQ", "央企"));
        rejects(filter("ownershipType", "CONTAINS", "Y"));
        rejects(filter("ownershipType", "EQ", "FAKE"));
        rejects(filter("settlType", "EQ", 1));

        JSONObject moving = JsonUtils.object("ownershipType", "YQ", "price", "2.5");
        JSONObject combined = filter("ownershipType", "IN", JsonUtils.array("YQ", "GQ"));
        combined.getJSONArray("conditions")
                .add(condition("price", "BETWEEN", JsonUtils.array("2", "3")));
        FilterEngine.validate(combined);
        check(FilterEngine.match(moving, combined), "enters combined filter");
        moving.put("ownershipType", "MQ");
        check(!FilterEngine.match(moving, combined), "leaves combined filter");
        for (JSONObject security : new SimulatorBaseDataClient().securities().values()) {
            for (Map.Entry<String, Object> entry : security.entrySet()) {
                if (FilterDictionaries.contains(entry.getKey())) {
                    Object value = entry.getValue();
                    FilterEngine.validate(
                            filter(
                                    entry.getKey(),
                                    value instanceof JSONArray ? "IN" : "EQ",
                                    value));
                    checks++;
                }
            }
        }
        System.out.println("PASS " + checks + " filter contract checks");
    }

    private static JSONObject condition(String field, String operator, Object value) {
        return JsonUtils.object("field", field, "operator", operator, "value", value);
    }

    private static JSONObject filter(String field, String operator, Object value) {
        return JsonUtils.object("conditions", JsonUtils.array(condition(field, operator, value)));
    }

    private static void rejects(JSONObject filter) {
        try {
            FilterEngine.validate(filter);
        } catch (ApiException expected) {
            checks++;
            return;
        }
        throw new AssertionError("Expected validation failure: " + filter);
    }

    private static void check(boolean passed, String description) {
        if (!passed) {
            throw new AssertionError(description);
        }
        checks++;
    }
}
