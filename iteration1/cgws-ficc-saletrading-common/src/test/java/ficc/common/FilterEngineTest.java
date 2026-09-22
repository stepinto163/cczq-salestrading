package ficc.common;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.alibaba.fastjson2.JSONObject;

import org.junit.jupiter.api.Test;

final class FilterEngineTest {
    @Test
    void combinesAlternativesWithinAFieldAndRequiresOtherFields() {
        JSONObject filter =
                JsonUtils.object(
                        "conditions",
                        JsonUtils.array(
                                condition("bondRating", "EQ", "AAA"),
                                condition("bondRating", "EQ", "AA+"),
                                condition("region", "EQ", "310000")));
        FilterEngine.validate(filter);

        assertTrue(
                FilterEngine.match(
                        JsonUtils.object("bondRating", "AA+", "region", "310000"), filter));
        assertFalse(
                FilterEngine.match(
                        JsonUtils.object("bondRating", "AAA", "region", "110000"), filter));
        assertFalse(
                FilterEngine.match(
                        JsonUtils.object("bondRating", "AA", "region", "310000"), filter));
    }

    @Test
    void comparesDecimalsNumericallyIncludingRangeBoundaries() {
        JSONObject filter =
                JsonUtils.object(
                        "conditions",
                        JsonUtils.array(
                                condition(
                                        "price", "BETWEEN", JsonUtils.array("2.5000", "3.0000"))));
        FilterEngine.validate(filter);

        assertTrue(FilterEngine.match(JsonUtils.object("price", "2.5"), filter));
        assertTrue(FilterEngine.match(JsonUtils.object("price", "3.0000"), filter));
        assertFalse(FilterEngine.match(JsonUtils.object("price", "3.0001"), filter));
    }

    @Test
    void rejectsUnsupportedFilterFieldsBeforeQueryExecution() {
        JSONObject filter =
                JsonUtils.object(
                        "conditions",
                        JsonUtils.array(condition("unrecognizedField", "EQ", "value")));
        assertThrows(ApiException.class, () -> FilterEngine.validate(filter));
    }

    private static JSONObject condition(String field, String operator, Object value) {
        return JsonUtils.object("field", field, "operator", operator, "value", value);
    }

    @Test
    void supportsCreditClausesLiquidityAndMissingValues() {
        JSONObject filter =
                JsonUtils.object(
                        "conditions",
                        JsonUtils.array(
                                condition("liquidityGrade", "IN", JsonUtils.array("S", "A")),
                                condition("isSubordinated", "EQ", false),
                                condition("industry", "EQ", "FINANCE"),
                                condition("couponRate", "BETWEEN", JsonUtils.array("2", "4")),
                                condition("hotTags", "IN", JsonUtils.array("LZ", "CTZ"))));
        FilterEngine.validate(filter);
        JSONObject row =
                JsonUtils.object(
                        "liquidityGrade",
                        "A",
                        "isSubordinated",
                        false,
                        "industry",
                        "FINANCE",
                        "couponRate",
                        "3.0000",
                        "hotTags",
                        JsonUtils.array("LZ"));
        assertTrue(FilterEngine.match(row, filter));
        row.remove("isSubordinated");
        assertFalse(FilterEngine.match(row, filter));
        assertThrows(
                ApiException.class,
                () ->
                        FilterEngine.validate(
                                JsonUtils.object(
                                        "conditions",
                                        JsonUtils.array(
                                                condition(
                                                        "couponRate",
                                                        "BETWEEN",
                                                        JsonUtils.array("4", "2"))))));
    }
}
