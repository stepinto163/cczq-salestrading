package ficc.common;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.client.SimulatorBaseDataClient;

import java.util.Collection;

/** Small deterministic check; no services, database, or performance load. */
public final class FilterContractCheck {
    public static void main(String[] args) throws Exception {
        int checks = 0;
        try (SimulatorBaseDataClient base = new SimulatorBaseDataClient()) {
            for (JSONObject security : base.securities().values()) {
                for (JSONObject field : FilterFields.DEFINITIONS.values()) {
                    String name = field.getString("field");
                    Object value = security.get(name);
                    if (value == null) continue;
                    if (value instanceof Collection<?>)
                        value = ((Collection<?>) value).iterator().next();
                    JSONObject filter =
                            JsonUtils.object(
                                    "conditions",
                                    JsonUtils.array(
                                            JsonUtils.object(
                                                    "field",
                                                    name,
                                                    "operator",
                                                    "EQ",
                                                    "value",
                                                    value)));
                    FilterEngine.validate(filter);
                    if (!FilterEngine.match(security, filter)) throw new AssertionError(name);
                    JSONObject missing = JsonUtils.deepCopy(security);
                    missing.remove(name);
                    if (FilterEngine.match(missing, filter))
                        throw new AssertionError("Unknown matched " + name);
                    checks += 2;
                }
            }
            JSONObject range =
                    JsonUtils.object(
                            "conditions",
                            JsonUtils.array(
                                    JsonUtils.object(
                                            "field",
                                            "price",
                                            "operator",
                                            "BETWEEN",
                                            "value",
                                            JsonUtils.array("2.5", "3"))));
            FilterEngine.validate(range);
            if (!FilterEngine.match(JsonUtils.object("price", "2.5000"), range))
                throw new AssertionError("Boundary");
            if (FilterEngine.match(JsonUtils.object("price", "3.0001"), range))
                throw new AssertionError("Outside");
            try (ReferenceData refs = new ReferenceData()) {
                JSONObject security = refs.require("250001.IB");
                security.put("valuationYield", "2.40");
                security.put("valuationBasis", "MATURITY");
                security.put("valuationCleanPrice", "100.00");
                JSONObject quote =
                        JsonUtils.object(
                                "securityId",
                                "250001.IB",
                                "side",
                                "1",
                                "quoteType",
                                "1",
                                "yieldType",
                                "1",
                                "price",
                                "2.50");
                checkDecimal(refs.enrich(quote), "bidValuationSpread", "10");
                quote.put("side", "2");
                checkDecimal(refs.enrich(quote), "offerValuationSpread", "-10");
                quote.put("yieldType", "2");
                if (refs.enrich(quote).containsKey("offerValuationSpread"))
                    throw new AssertionError("Yield basis mismatch");
                quote.put("quoteType", "2");
                quote.put("price", "99.80");
                checkDecimal(refs.enrich(quote), "offerCleanPriceSpread", "0.20");
                quote.put("side", "1");
                checkDecimal(refs.enrich(quote), "bidCleanPriceSpread", "-0.20");
                if (refs.enrich(quote).containsKey("bidValuationSpread"))
                    throw new AssertionError("Clean price leaked into BP");
                checks += 6;
            }
            JSONObject rawBond = JsonUtils.object("bondSubtype", "CMN", "couponType", "FRB");
            ficc.common.client.BondFilterAttributes.normalize(rawBond);
            if (!"中票".equals(rawBond.getString("bondSubtype"))
                    || !"固息".equals(rawBond.getString("rateIndex"))) {
                throw new AssertionError("STC product classification");
            }
            if (!SettlementTypes.SUPPORTED.contains("9")
                    || SettlementTypes.SUPPORTED.contains("6")) {
                throw new AssertionError("STC settlement codes");
            }
            checks += 2;
            System.out.println(
                    "PASS "
                            + (checks + 2)
                            + " filter checks; fields="
                            + FilterFields.DEFINITIONS.size());
        }
    }

    private static void checkDecimal(JSONObject row, String field, String expected) {
        if (row.getBigDecimal(field).compareTo(new java.math.BigDecimal(expected)) != 0) {
            throw new AssertionError(field);
        }
    }
}
