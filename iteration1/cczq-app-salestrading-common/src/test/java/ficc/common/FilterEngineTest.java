package ficc.common;

import com.alibaba.fastjson2.JSONObject;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

final class FilterEngineTest {
    @Test
    void combinesAlternativesWithinAFieldAndRequiresOtherFields() {
        JSONObject filter = JsonUtils.object("conditions", JsonUtils.array(
                condition("bondRating", "EQ", "AAA"),
                condition("bondRating", "EQ", "AA+"),
                condition("region", "EQ", "上海")));
        FilterEngine.validate(filter);

        assertTrue(FilterEngine.match(JsonUtils.object("bondRating", "AA+", "region", "上海"), filter));
        assertFalse(FilterEngine.match(JsonUtils.object("bondRating", "AAA", "region", "北京"), filter));
        assertFalse(FilterEngine.match(JsonUtils.object("bondRating", "AA", "region", "上海"), filter));
    }

    @Test
    void comparesDecimalsNumericallyIncludingRangeBoundaries() {
        JSONObject filter = JsonUtils.object("conditions", JsonUtils.array(
                condition("price", "BETWEEN", JsonUtils.array("2.5000", "3.0000"))));
        FilterEngine.validate(filter);

        assertTrue(FilterEngine.match(JsonUtils.object("price", "2.5"), filter));
        assertTrue(FilterEngine.match(JsonUtils.object("price", "3.0000"), filter));
        assertFalse(FilterEngine.match(JsonUtils.object("price", "3.0001"), filter));
    }

    @Test
    void rejectsUnsupportedFilterFieldsBeforeQueryExecution() {
        JSONObject filter = JsonUtils.object("conditions", JsonUtils.array(
                condition("unrecognizedField", "EQ", "value")));
        assertThrows(ApiException.class, () -> FilterEngine.validate(filter));
    }

    private static JSONObject condition(String field, String operator, Object value) {
        return JsonUtils.object("field", field, "operator", operator, "value", value);
    }
}
