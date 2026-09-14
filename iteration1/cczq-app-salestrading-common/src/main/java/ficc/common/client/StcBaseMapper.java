package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.Properties;
import java.util.Set;

/** Based on BondInfoView, TIssuerInfo and TBondValuationView in the supplied STC source. */
public final class StcBaseMapper {
    private final Properties configuration;

    public StcBaseMapper(Properties configuration) {
        this.configuration = configuration;
    }

    public JSONObject security(String securityId, JSONObject bond, JSONObject issuer, JSONObject valuation, String version) {
        String rawId = text(bond, "Bond_ID");
        String marketCode = text(bond, "Listed_Market");
        String suffix = switch (marketCode == null ? "" : marketCode) {
            case "CIB" -> ".IB";
            case "SSE" -> ".SH";
            case "SZE" -> ".SZ";
            default -> "";
        };
        String normalized = rawId == null ? null : rawId.contains(".") ? rawId : rawId + suffix;
        if (!securityId.equals(normalized)) {
            throw new IllegalArgumentException("BaseSvr security ID mismatch: " + securityId);
        }
        String market = configuration.getProperty("base.market." + securityId,
                configuration.getProperty("base.marketBySubtype." + text(bond, "Bond_Subtype")));
        if (!Set.of("RATE", "CREDIT").contains(market == null ? "" : market)) {
            throw new IllegalArgumentException("Configure base.market." + securityId + " or base.marketBySubtype");
        }
        String option = text(bond, "Option_Type");
        Set<String> exercise = Set.of("PUT", "CNP", "CAL", "ETS", "ASS", "DCN", "CNV");
        if (option != null && !exercise.contains(option) && !Set.of("NON", "SLB", "OTH", "PRS").contains(option)) {
            throw new IllegalArgumentException("Unmapped STC Option_Type: " + option);
        }
        String issuerType = configuration.getProperty("base.issuerType." + text(issuer, "issuer_type"));
        // 债项评级优先；非公开（PPN）/私募债的 Rating_Current 常为 NULL，此时回退到发行人评级，
        // 并用 bondRatingSource 显式标注口径来源，避免消费方把主体评级误当成债项评级。
        // 真实 BaseSvr 应答的字段名实测：信用债会同时返回 Rating_Current 与 Rating_Current_NPY
        // （实测 133179.SZ 债项 AA+、197522.SH 债项 AAA，两者取值相同），两种写法都要认；
        // 利率债（国债/国开债）不带任何评级字段，此时评级为 null 是正常业务结果，不是字段缺失。
        // 债项评级与主体评级可能不同（133179.SZ 债项 AA+ / 主体 AA），故不能互相替代。
        String issueRating = firstText(bond, "Rating_Current", "Rating_Current_NPY");
        String issuerRating = firstText(bond, "Issuer_Rating_Current", "Issuer_Rating_Current_NPY");
        JSONObject result = JsonUtils.object(
                "securityId", securityId, "securityName", text(bond, "Short_Name"), "market", market,
                "issuerId", text(bond, "Issuer_Code"), "issuerType", issuerType,
                "bondRating", issueRating != null ? issueRating : issuerRating,
                "bondRatingSource", issueRating != null ? "ISSUE" : issuerRating != null ? "ISSUER" : null,
                "issuerRating", issuerRating,
                "region", text(issuer, "province"), "remainingYears", text(valuation, "remaining_year"),
                "hasExerciseOption", option != null && exercise.contains(option),
                "optionType", option, "maturityDate", date(text(bond, "Maturity_Date")),
                "referenceVersion", version, "dataVendor", "SUMSCOPE",
                // 中债估值收益率可能是 "行权|到期" 双值，取主值（[0]，与 BaseSvr getCbv() 一致）。
                "valuationYield", primary(text(valuation, "valuation_yield")),
                "valuationDate", date(text(valuation, "valuation_date")));
        return result;
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
        // 中债对含权债同时发布「行权」与「到期」两个口径，落库为 "行权|到期"（如 "100.0136|100.0136"、
        // "2.0035|2.8155"）；非含权债只有一个值。STC 客户端按 '|' 拆分，第 1 段即行权、
        // 第 2 段即到期，单值则属于到期口径（见 stc-client InnerHistoryRequireViewModel
        // ChinaBondExercise/ChinaBondDue）；BaseSvr 的 getCbv() 也取第 1 段作为主值。
        // 因此估值口径由「是否存在双值」判定，而不是由 bondTypeFlag 判定——后者描述的是
        // 「含权债是否已行权」，是另一件事（STC CommonDataManager.initChinaBondsinfo）。
        String yield = text(raw, "valuation_yield");
        String cleanPrice = text(raw, "valuation_net_price");
        String dueYield = secondary(yield);
        String dueCleanPrice = secondary(cleanPrice);
        boolean dual = dueYield != null || dueCleanPrice != null;
        return JsonUtils.object("valuationId", text(raw, "Id"), "securityId", securityId,
                "valuationSource", "CHINABOND", "dataVendor", "SUMSCOPE",
                "valuationDate", date(text(raw, "valuation_date")),
                "valuationBasis", dual ? "EXERCISE" : "MATURITY",
                "yield", primary(yield), "cleanPrice", primary(cleanPrice),
                "exerciseYield", dueYield == null ? null : primary(yield),
                "dueYield", dueYield == null ? primary(yield) : dueYield,
                "exerciseCleanPrice", dueCleanPrice == null ? null : primary(cleanPrice),
                "dueCleanPrice", dueCleanPrice == null ? primary(cleanPrice) : dueCleanPrice,
                // bondTypeFlag: 1=含权债未行权 2=含权债已行权 空=非含权债。
                "optionState", switch (String.valueOf(text(raw, "bondTypeFlag"))) {
                    case "1" -> "NOT_EXERCISED";
                    case "2" -> "EXERCISED";
                    default -> null;
                });
    }

    /** 取中债 "行权|到期" 双值的第 1 段（行权口径）；单值时原样返回。 */
    private static String primary(String value) {
        if (value == null) {
            return null;
        }
        int separator = value.indexOf('|');
        String head = (separator < 0 ? value : value.substring(0, separator)).trim();
        return head.isEmpty() ? null : head;
    }

    /** 取中债 "行权|到期" 双值的第 2 段（到期口径）；单值时返回 null，用于判定是否双值。 */
    private static String secondary(String value) {
        if (value == null) {
            return null;
        }
        int separator = value.indexOf('|');
        if (separator < 0) {
            return null;
        }
        String tail = value.substring(separator + 1).trim();
        return tail.isEmpty() ? null : tail;
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

    /** 依次尝试多个字段名，返回第一个非空值：兼容 STC 应答中带/不带 _NPY 后缀的评级字段。 */
    private static String firstText(JSONObject object, String... names) {
        for (String name : names) {
            String value = text(object, name);
            if (value != null) {
                return value;
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
