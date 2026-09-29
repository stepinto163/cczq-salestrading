package ficc.common;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.Filter;
import ficc.api.ProtocolCodec;
import ficc.api.QuoteRecord;
import ficc.api.ReferenceRecord;
import org.junit.jupiter.api.Test;
import java.util.Arrays;
import java.util.Collection;
import java.util.Objects;
import static org.junit.jupiter.api.Assertions.assertEquals;

class TypedFilterEquivalenceTest {
    @Test
    void preservesEveryRegisteredAttributeAndOperatorAcrossBondAndQuoteTypes() {
        ReferenceData references = new ReferenceData();
        for (JSONObject security : references.securities().values()) {
            for (String side : Arrays.asList("1", "2")) {
                for (String priceType : Arrays.asList("1", "2")) {
                    JSONObject quote = JsonUtils.object("securityId", security.getString("securityId"),
                            "side", side, "quoteType", priceType, "yieldType", "1", "price", "2.5000",
                            "quantity", "1000", "institutionName", "Example Bank", "contactName", "Trader",
                            "urgent", false, "recommended", true);
                    JSONObject oldRow = references.enrich(quote);
                    QuoteRecord record = ProtocolCodec.decode(quote, QuoteRecord.class);
                    QuoteFilterValues newRow = new QuoteFilterValues(record,
                            ProtocolCodec.decode(security, ReferenceRecord.class));
                    for (String field : FilterFields.DEFINITIONS.keySet()) {
                        Object expected = oldRow.get(field);
                        Object actual = newRow.value(field);
                        assertEquals(Objects.toString(expected, null), Objects.toString(actual, null), field);
                        compare(oldRow, newRow, field, "EXISTS", true);
                        compare(oldRow, newRow, field, "EXISTS", false);
                        Object scalar = expected instanceof Collection
                                ? ((Collection<?>) expected).stream().findFirst().orElse(null) : expected;
                        if (scalar != null) {
                            for (String operator : Arrays.asList("EQ", "GT", "GTE", "LT", "LTE", "CONTAINS")) {
                                compare(oldRow, newRow, field, operator, scalar);
                            }
                            compare(oldRow, newRow, field, "IN", JsonUtils.array(scalar));
                            compare(oldRow, newRow, field, "BETWEEN", JsonUtils.array(scalar, scalar));
                        }
                    }
                }
            }
        }
    }

    private static void compare(JSONObject oldRow, FilterValues newRow, String field, String operator, Object value) {
        JSONObject filter = JsonUtils.object("conditions", JsonUtils.array(
                JsonUtils.object("field", field, "operator", operator, "value", value)));
        assertEquals(FilterEngine.match(oldRow, filter),
                FilterEngine.match(newRow, ProtocolCodec.decode(filter, Filter.class)), field + ":" + operator);
    }
}
