package ficc.common;

import ficc.api.QuoteRecord;
import ficc.api.ReferenceRecord;
import java.lang.reflect.Field;
import java.math.BigDecimal;
import java.util.HashMap;
import java.util.Map;

/** Reads only requested filter attributes, without serializing or copying each quote. */
public final class QuoteFilterValues implements FilterValues {
    private static final Map<String, Field> QUOTE_FIELDS = fields(QuoteRecord.class);
    private static final Map<String, Field> REFERENCE_FIELDS = fields(ReferenceRecord.class);
    private final QuoteRecord quote;
    private final ReferenceRecord reference;

    public QuoteFilterValues(QuoteRecord quote, ReferenceRecord reference) {
        this.quote = quote;
        this.reference = reference;
    }

    @Override
    public Object value(String field) {
        boolean bid = "1".equals(quote.side);
        switch (field) {
            case "bidPrice": return bid ? quote.price : null;
            case "offerPrice": return bid ? null : quote.price;
            case "bidQuantity": return bid ? quote.quantity : null;
            case "offerQuantity": return bid ? null : quote.quantity;
            case "bidValuationSpread": return bid ? yieldSpread(true) : null;
            case "offerValuationSpread": return bid ? null : yieldSpread(false);
            case "bidCleanPriceSpread": return bid ? cleanSpread(true) : null;
            case "offerCleanPriceSpread": return bid ? null : cleanSpread(false);
            case "searchText":
                StringBuilder text = new StringBuilder();
                for (String name : new String[] {"securityId", "securityName", "issuerName",
                        "institutionName", "contactName", "ownerId", "ownerDisplayName"}) {
                    Object item = raw(name);
                    if (item != null) {
                        text.append(item).append(' ');
                    }
                }
                return text.toString();
            default:
                return raw(field);
        }
    }

    private Object raw(String name) {
        try {
            Field securityField = REFERENCE_FIELDS.get(name);
            if (securityField != null && reference != null) {
                Object value = securityField.get(reference);
                if (value != null) {
                    return value;
                }
            }
            Field quoteField = QUOTE_FIELDS.get(name);
            return quoteField == null ? null : quoteField.get(quote);
        } catch (IllegalAccessException error) {
            throw new IllegalStateException("Cannot read filter field " + name, error);
        }
    }

    private BigDecimal yieldSpread(boolean bid) {
        if (reference == null || quote.price == null || reference.valuationYield == null
                || !"1".equals(quote.quoteType)
                || !("2".equals(quote.yieldType) ? "EXERCISE" : "MATURITY").equals(reference.valuationBasis)) {
            return null;
        }
        BigDecimal spread = new BigDecimal(quote.price).subtract(new BigDecimal(reference.valuationYield))
                .multiply(BigDecimal.valueOf(100));
        return bid ? spread : spread.negate();
    }

    private BigDecimal cleanSpread(boolean bid) {
        if (reference == null || quote.price == null || reference.valuationCleanPrice == null
                || !"2".equals(quote.quoteType)) {
            return null;
        }
        BigDecimal spread = new BigDecimal(quote.price).subtract(new BigDecimal(reference.valuationCleanPrice));
        return bid ? spread : spread.negate();
    }

    private static Map<String, Field> fields(Class<?> type) {
        Map<String, Field> fields = new HashMap<>();
        for (Field field : type.getFields()) {
            fields.put(field.getName(), field);
        }
        return fields;
    }
}
