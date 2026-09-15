package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.Properties;
import java.util.Set;
import sc.app.stc.api.base.po.Bond;
import sc.app.stc.api.base.po.TSmallView;

/** Based on BondInfoView, TIssuerInfo and TBondValuationView in the supplied STC source. */
public final class StcBaseMapper {
    private final Properties configuration;

    public StcBaseMapper(Properties configuration) {
        this.configuration = configuration;
    }

    public JSONObject security(String securityId, JSONObject bond, JSONObject issuer, JSONObject valuation, String version) {
        if (!securityId.equals(securityId(bond))) {
            throw new IllegalArgumentException("BaseSvr security ID mismatch: " + securityId);
        }
        String goodsType = goodsType(bond);
        String market = configuration.getProperty("base.market." + securityId,
                configuration.getProperty("base.marketBySubtype." + text(bond, "Bond_Subtype")));
        if (market == null) {
            market = switch (goodsType) {
                case "2" -> "RATE";
                case "1" -> "CREDIT";
                default -> null;
            };
        } else if (!Set.of("RATE", "CREDIT").contains(market)) {
            throw new IllegalArgumentException("Invalid base.market mapping for " + securityId);
        }
        String option = text(bond, "Option_Type");
        Set<String> exercise = Set.of("PUT", "CNP", "CAL", "ETS", "ASS", "DCN", "CNV");
        if (option != null && !exercise.contains(option) && !Set.of("NON", "SLB", "OTH", "PRS").contains(option)) {
            throw new IllegalArgumentException("Unmapped STC Option_Type: " + option);
        }
        String issuerType = configuration.getProperty("base.issuerType." + text(issuer, "issuer_type"));
        return JsonUtils.object(
                "securityId", securityId, "securityName", text(bond, "Short_Name"), "market", market,
                "bondSubtype", text(bond, "Bond_Subtype"), "stcGoodsType", goodsType,
                "issuerId", text(bond, "Issuer_Code"), "issuerType", issuerType,
                "sourceIssuerType", text(issuer, "issuer_type"),
                "bondRating", text(bond, "Rating_Current"), "issuerRating", text(bond, "Issuer_Rating_Current"),
                "region", text(issuer, "province"), "remainingYears", text(valuation, "remaining_year"),
                "hasExerciseOption", option != null && exercise.contains(option),
                "optionType", option, "maturityDate", date(text(bond, "Maturity_Date")),
                "referenceVersion", version, "dataVendor", "SUMSCOPE",
                "valuationYield", text(valuation, "valuation_yield"),
                "valuationDate", date(text(valuation, "valuation_date")));
    }

    public static String securityId(JSONObject bond) {
        String rawId = text(bond, "Bond_ID");
        String marketCode = text(bond, "Listed_Market");
        String suffix = switch (marketCode == null ? "" : marketCode) {
            case "CIB" -> ".IB";
            case "SSE" -> ".SH";
            case "SZE" -> ".SZ";
            default -> "";
        };
        if (rawId == null) {
            throw new IllegalArgumentException("BaseSvr bond is missing Bond_ID");
        }
        if (rawId.endsWith(".IB") || rawId.endsWith(".SH") || rawId.endsWith(".SZ")) {
            if (!suffix.isEmpty() && !rawId.endsWith(suffix)) {
                throw new IllegalArgumentException("BaseSvr Listed_Market mismatch: " + rawId);
            }
            return rawId;
        }
        if (rawId.contains(".") || suffix.isEmpty()) {
            return null;
        }
        return rawId + suffix;
    }

    private static String goodsType(JSONObject bond) {
        Bond source = new Bond();
        source.Bond_Subtype = text(bond, "Bond_Subtype");
        source.Ent_Cor = text(bond, "Ent_Cor");
        TSmallView view = new TSmallView();
        view.b = source;
        return view.getGoodsType();
    }

    public JSONObject issuer(JSONObject security, JSONObject raw) {
        return JsonUtils.object("issuerId", security.getString("issuerId"), "issuerName", text(raw, "issuer_name"),
                "issuerType", security.getString("issuerType"), "issuerRating", security.getString("issuerRating"),
                "region", security.getString("region"), "dataVendor", "SUMSCOPE");
    }

    public JSONObject valuation(String securityId, JSONObject raw) {
        if (raw == null || raw.isEmpty()) {
            return null;
        }
        return JsonUtils.object("valuationId", text(raw, "Id"), "securityId", securityId,
                "valuationSource", "CHINABOND", "dataVendor", "SUMSCOPE",
                "valuationDate", date(text(raw, "valuation_date")),
                "valuationBasis", switch (String.valueOf(text(raw, "bondTypeFlag"))) {
                    case "1" -> "EXERCISE";
                    case "2" -> "MATURITY";
                    default -> null;
                }, "yield", text(raw, "valuation_yield"), "cleanPrice", text(raw, "valuation_net_price"));
    }

    public static String text(JSONObject object, String name) {
        if (object == null) {
            return null;
        }
        for (String key : object.keySet()) {
            if (name.equalsIgnoreCase(key)) {
                String value = object.getString(key);
                return value == null || value.isBlank() ? null : value;
            }
        }
        return null;
    }

    private static String date(String value) {
        if (value == null) {
            return null;
        }
        if (value.matches("[0-9]{8}(\\.0+)?")) {
            return LocalDate.parse(value.substring(0, 8), DateTimeFormatter.BASIC_ISO_DATE).toString();
        }
        return LocalDate.parse(value.substring(0, 10)).toString();
    }
}
