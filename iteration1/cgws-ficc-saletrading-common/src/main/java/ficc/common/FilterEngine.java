package ficc.common;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.api.Filter;
import ficc.api.FilterCondition;

import java.math.BigDecimal;
import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import java.util.HashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Objects;
import java.util.Set;

/**
 * 条件筛选的公共运算规则：同一字段的多个条件取 OR，不同字段之间取 AND。
 * 上下界必须放在同一个 BETWEEN 条件中，否则两个独立范围会按同字段 OR 处理。
 * validate 校验字段、操作符和字典值；match 只执行匹配，不负责用户权限或数据加载。
 * FilterValues 重载用于高频路径，避免每次匹配都把报价转换成 JSONObject。
 */
public final class FilterEngine {
    public static final Set<String> FIELDS = FilterFields.DEFINITIONS.keySet();
    public static final Set<String> NUMBERS = FilterFields.NUMBERS;
    public static final Set<String> OPS =
            Collections.unmodifiableSet(
                    new LinkedHashSet<>(
                            Arrays.asList(
                                    "EQ",
                                    "IN",
                                    "GT",
                                    "GTE",
                                    "LT",
                                    "LTE",
                                    "BETWEEN",
                                    "EXISTS",
                                    "CONTAINS")));

    public static void validate(JSONObject filter) {
        if (filter == null) return;
        RequestValidator.allowOnlyFields(
                filter, "market", "conditions", "watchlistIds", "filterGroupIds");
        if (filter.containsKey("market")
                && !Arrays.asList("RATE", "CREDIT").contains(filter.getString("market")))
            fail("market", "Invalid market");
        for (JSONObject condition : conditions(filter)) {
            RequestValidator.allowOnlyFields(
                    condition, "field", "operator", "value", "conditionId", "displayOrder");
            String field = RequestValidator.requireString(condition, "field");
            String operator = RequestValidator.requireString(condition, "operator");
            JSONObject definition = FilterFields.DEFINITIONS.get(field);
            if (definition == null || !definition.getJSONArray("operators").contains(operator))
                fail(field, "Unknown field or unsupported operator");
            if (FilterDictionaries.contains(field) && operator.equals("CONTAINS")) {
                fail(field, "Categorical filters require EQ, IN or EXISTS");
            }
            Object value = condition.get("value");
            if (value == null) fail(field, "Filter value required");
            if (operator.equals("EXISTS") || FilterFields.BOOLEANS.contains(field)) {
                if (!(value instanceof Boolean)) fail(field, "Boolean value required");
                continue;
            }
            boolean array = operator.equals("IN") || operator.equals("BETWEEN");
            if (array != (value instanceof List<?>))
                fail(field, "Invalid scalar/array filter value");
            if (array && ((List<?>) value).isEmpty()) fail(field, "Empty filter values");
            if (operator.equals("BETWEEN") && ((List<?>) value).size() != 2)
                fail(field, "BETWEEN requires two bounds");
            List<?> values = array ? (List<?>) value : Collections.singletonList(value);
            for (Object item : values) {
                if (item == null
                        || item.toString().trim().isEmpty()
                        || item instanceof Map<?, ?>
                        || item instanceof Collection<?>)
                    fail(field, "Scalar filter values required");
                if (FilterDictionaries.contains(field)
                        && (!(item instanceof String)
                                || !item.equals(FilterDictionaries.canonical(field, item)))) {
                    fail(field, "Unknown dictionary code");
                }
                if (NUMBERS.contains(field)) {
                    try {
                        new BigDecimal(item.toString());
                    } catch (NumberFormatException error) {
                        fail(field, "Invalid decimal condition");
                    }
                }
            }
            if (operator.equals("BETWEEN") && compare(field, values.get(0), values.get(1)) > 0)
                fail(field, "Range lower bound exceeds upper bound");
        }
    }

    private static void fail(String field, String message) {
        throw ApiException.forField("1001", "VALIDATION_FAILED", message, field);
    }

    public static List<JSONObject> conditions(JSONObject filter) {
        JSONArray conditions = filter == null ? null : filter.getJSONArray("conditions");
        return conditions == null
                ? Collections.emptyList()
                : conditions.toJavaList(JSONObject.class);
    }

    public static boolean match(JSONObject row, JSONObject filter) {
        if (filter == null) return true;
        if (filter.getString("market") != null
                && !Objects.equals(filter.getString("market"), row.getString("market")))
            return false;
        Map<String, Boolean> matches = new HashMap<>();
        for (JSONObject condition : conditions(filter))
            matches.merge(condition.getString("field"), test(row, condition), Boolean::logicalOr);
        return matches.values().stream().allMatch(Boolean::booleanValue);
    }

    public static boolean match(FilterValues row, Filter filter) {
        if (filter == null) {
            return true;
        }
        if (filter.market != null && !Objects.equals(filter.market, row.value("market"))) {
            return false;
        }
        Map<String, Boolean> matches = new HashMap<>();
        if (filter.conditions != null) {
            for (FilterCondition condition : filter.conditions) {
                matches.merge(condition.field, test(row, condition), Boolean::logicalOr);
            }
        }
        return matches.values().stream().allMatch(Boolean::booleanValue);
    }

    private static boolean test(FilterValues row, FilterCondition condition) {
        Object actual = row.value(condition.field);
        boolean present = actual != null && !actual.toString().trim().isEmpty();
        if ("EXISTS".equals(condition.operator)) {
            return present == Boolean.TRUE.equals(condition.value);
        }
        if (!present) {
            return false;
        }
        if (actual instanceof Collection<?>) {
            for (Object value : (Collection<?>) actual) {
                if (value != null && scalarMatch(condition.field, condition.operator, value, condition.value)) {
                    return true;
                }
            }
            return false;
        }
        return scalarMatch(condition.field, condition.operator, actual, condition.value);
    }

    public static boolean test(JSONObject row, JSONObject condition) {
        String field = condition.getString("field");
        String operator = condition.getString("operator");
        Object actual = row.get(field);
        Object expected = condition.get("value");
        boolean present = actual != null && !actual.toString().trim().isEmpty();
        if (operator.equals("EXISTS")) return present == Boolean.TRUE.equals(expected);
        if (!present) return false;
        if (actual instanceof Collection<?>) {
            Collection<?> values = (Collection<?>) actual;
            return values.stream()
                    .filter(Objects::nonNull)
                    .anyMatch(value -> scalarMatch(field, operator, value, expected));
        }
        return scalarMatch(field, operator, actual, expected);
    }

    private static boolean scalarMatch(
            String field, String operator, Object actual, Object expected) {
        if (operator.equals("CONTAINS"))
            return actual.toString()
                    .toLowerCase(Locale.ROOT)
                    .contains(expected.toString().toLowerCase(Locale.ROOT));
        if (operator.equals("IN"))
            return ((List<?>) expected)
                    .stream().anyMatch(value -> compare(field, actual, value) == 0);
        if (operator.equals("BETWEEN")) {
            List<?> range = (List<?>) expected;
            return compare(field, actual, range.get(0)) >= 0
                    && compare(field, actual, range.get(1)) <= 0;
        }
        int comparison = compare(field, actual, expected);
        if ("EQ".equals(operator)) return comparison == 0;
        if ("GT".equals(operator)) return comparison > 0;
        if ("GTE".equals(operator)) return comparison >= 0;
        if ("LT".equals(operator)) return comparison < 0;
        if ("LTE".equals(operator)) return comparison <= 0;
        return false;
    }

    public static int compare(String field, Object left, Object right) {
        return NUMBERS.contains(field)
                ? new BigDecimal(left.toString()).compareTo(new BigDecimal(right.toString()))
                : left.toString().compareTo(right.toString());
    }

    public static JSONArray catalog(boolean price) {
        return FilterFields.catalog();
    }
}
