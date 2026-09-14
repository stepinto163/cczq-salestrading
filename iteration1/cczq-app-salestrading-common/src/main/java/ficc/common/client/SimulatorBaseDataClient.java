package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import java.time.LocalDate;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

public final class SimulatorBaseDataClient implements BaseDataClient {
    private final Map<String, JSONObject> securities = new LinkedHashMap<>();

    public SimulatorBaseDataClient() {
        add("250001.IB", "示例国债", "RATE", "GOVERNMENT", "AAA", "北京", false);
        add("250002.IB", "示例政策银行债", "RATE", "POLICY_BANK", "AAA", "北京", false);
        add("102500001.IB", "示例企业债", "CREDIT", "CORPORATE", "AA+", "上海", false);
        add("102500002.IB", "示例含权企业债", "CREDIT", "CORPORATE", "AAA", "广东", true);
    }

    private void add(String id, String name, String market, String issuerType, String rating,
            String region, boolean option) {
        securities.put(id, JsonUtils.object("securityId", id, "securityName", name, "market", market,
                "issuerId", "IS-" + id, "issuerType", issuerType, "bondRating", rating,
                "issuerRating", rating, "region", region, "remainingYears", "2.5",
                "hasExerciseOption", option, "valuationYield", "2.4500",
                "valuationDate", LocalDate.now().toString(), "referenceVersion", version()));
    }

    @Override
    public Map<String, JSONObject> securities() {
        return java.util.Collections.unmodifiableMap(securities);
    }

    @Override
    public String version() {
        return "SIM-1";
    }

    @Override
    public List<JSONObject> records(String kind) {
        return switch (kind) {
            case "SECURITY" -> List.copyOf(securities.values());
            case "ISSUER" -> securities.values().stream().map(value -> JsonUtils.object(
                    "issuerId", value.getString("issuerId"), "issuerName", value.getString("securityName") + "发行人",
                    "issuerType", value.getString("issuerType"), "issuerRating", value.getString("issuerRating"),
                    "region", value.getString("region"))).toList();
            case "DAILY_VALUATION" -> securities.values().stream().map(value -> JsonUtils.object(
                    "valuationId", "SIM-" + value.getString("securityId"), "securityId", value.getString("securityId"),
                    "valuationDate", value.getString("valuationDate"), "valuationSource", "CHINABOND",
                    "dataVendor", "SIMULATOR", "valuationBasis", "MATURITY", "yield", value.getString("valuationYield"))).toList();
            default -> throw ApiException.forField("1001", "VALIDATION_FAILED", "Unknown reference kind", "kind");
        };
    }
}
