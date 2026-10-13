package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.JsonUtils;

import sc.app.stc.api.base.po.Bond;
import sc.app.stc.api.base.po.TSmallView;

import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashSet;
import java.util.Locale;
import java.util.Properties;
import java.util.Set;

/**
 * 将 STC 债券、发行人、估值记录转换为 FICC 标准基础字段和筛选属性。
 * 字段名称兼容逻辑集中在此；市场分类优先使用显式配置，否则按 STC 券种分类推导。
 * base.filter.* 可显式指定来源字段，simulateMissing 必须显式开启；协议支持不等于上游有值。
 * 修改映射时应核对真实源字段及口径，不能为了使某个筛选通过而硬编码某只债券的分类。
 */
public final class StcBaseMapper {
    private final Properties configuration;

    public StcBaseMapper(Properties configuration) {
        this.configuration = configuration;
        for (String key : configuration.stringPropertyNames()) {
            if (!key.startsWith("base.filter.") || key.equals("base.filter.simulateMissing"))
                continue;
            String field = key.substring("base.filter.".length());
            if (!ficc.common.FilterFields.DEFINITIONS.containsKey(field)
                    || !configuration
                            .getProperty(key)
                            .matches("(bond|issuer|valuation)\\.[A-Za-z0-9_]+")) {
                throw new IllegalArgumentException("Invalid reference filter mapping: " + key);
            }
        }
        String simulateMissing = configuration.getProperty("base.filter.simulateMissing", "false");
        if (!Arrays.asList("true", "false").contains(simulateMissing)) {
            throw new IllegalArgumentException("base.filter.simulateMissing must be true or false");
        }
    }

    public JSONObject security(
            String securityId,
            JSONObject bond,
            JSONObject issuer,
            JSONObject valuation,
            String version) {
        return security(securityId, bond, issuer, valuation, null, version);
    }

    public JSONObject security(String securityId, JSONObject bond, JSONObject issuer,
            JSONObject valuation, JSONObject csi, String version) {
        if (!securityId.equals(securityId(bond))) {
            throw new IllegalArgumentException("BaseSvr security ID mismatch: " + securityId);
        }
        String goodsType = goodsType(bond);
        String market =
                configuration.getProperty(
                        "base.market." + securityId,
                        configuration.getProperty(
                                "base.marketBySubtype." + text(bond, "Bond_Subtype")));
        if (market == null) {
            market = marketByGoodsType(goodsType);
        } else if (!Arrays.asList("RATE", "CREDIT").contains(market)) {
            throw new IllegalArgumentException("Invalid base.market mapping for " + securityId);
        }
        String option = text(bond, "Option_Type");
        Set<String> exercise =
                Collections.unmodifiableSet(
                        new HashSet<>(
                                Arrays.asList("PUT", "CNP", "CAL", "ETS", "ASS", "DCN", "CNV")));
        if (option != null
                && !exercise.contains(option)
                && !Arrays.asList("NON", "SLB", "OTH", "PRS").contains(option)) {
            throw new IllegalArgumentException("Unmapped STC Option_Type: " + option);
        }
        String issuerType =
                configuration.getProperty("base.issuerType." + text(issuer, "issuer_type"));
        JSONObject result =
                JsonUtils.object(
                        "securityId",
                        securityId,
                        "securityName",
                        text(bond, "Short_Name"),
                        "market",
                        market,
                        "bondSubtype",
                        text(bond, "Bond_Subtype"),
                        "stcGoodsType",
                        goodsType,
                        "issuerId",
                        text(bond, "Issuer_Code"),
                        "issuerType",
                        issuerType,
                        "sourceIssuerType",
                        text(issuer, "issuer_type"),
                        "bondRating",
                        text(bond, "Rating_Current"),
                        "issuerRating",
                        text(bond, "Issuer_Rating_Current"),
                        "region",
                        text(issuer, "province"),
                        "remainingYears",
                        text(valuation, "remaining_year"),
                        "hasExerciseOption",
                        "NON".equals(option)
                                ? Boolean.FALSE
                                : exercise.contains(option == null ? "" : option)
                                        ? Boolean.TRUE
                                        : null,
                        "optionType",
                        option,
                        "maturityDate",
                        date(text(bond, "Maturity_Date")),
                        "referenceVersion",
                        version,
                        "dataVendor",
                        "SUMSCOPE",
                        "valuationYield",
                        text(valuation, "valuation_yield"),
                        "valuationDate",
                        date(text(valuation, "valuation_date")),
                        "valuationBasis",
                        valuationBasis(String.valueOf(text(valuation, "bondTypeFlag"))));
        result.putAll(
                JsonUtils.object(
                        "issuerName",
                        text(issuer, "issuer_name"),
                        "listedMarket",
                        text(bond, "Listed_Market"),
                        "cityName",
                        text(issuer, "city"),
                        "city",
                        text(issuer, "city_code") == null
                                ? text(issuer, "city")
                                : text(issuer, "city_code"),
                        "regionCode",
                        text(issuer, "province_code"),
                        "industry",
                        text(issuer, "sw_sector"),
                        "ownershipType",
                        text(issuer, "institution_subtype"),
                        "couponRate",
                        text(bond, "Coupon_Rate_Current"),
                        "couponType",
                        text(bond, "Coupon_Type"),
                        "couponFrequency",
                        text(bond, "Coupon_Frequency"),
                        "valuationCleanPrice",
                        text(valuation, "valuation_net_price"),
                        "rateIndex",
                        text(bond, "FRN_Index_ID"),
                        "issueYear",
                        text(bond, "Issue_Year"),
                        "isMunicipal",
                        flag(text(bond, "Is_Municipal")),
                        "isMortgage",
                        flag(text(bond, "Is_Mortgage")),
                        "isCrossMarket",
                        flag(text(bond, "Is_Cross_Mkt"))));
        if (csi != null) {
            result.putAll(csi);
            // 非含权债的 recommendation 按 STC 定义为空，此时估值口径为到期。
            if ("NON".equals(option) && result.get("csiRecommendedBasis") == null
                    && result.get("csiValuationYield") != null) {
                result.put("csiRecommendedBasis", "MATURITY");
            }
        }
        // Explicit vendor mappings allow additional fields without changing business filtering.
        // Example: base.filter.liquidityGrade=bond.<verified vendor column>
        for (String field : ficc.common.FilterFields.DEFINITIONS.keySet()) {
            String mapping = configuration.getProperty("base.filter." + field);
            if (mapping == null) continue;
            String[] parts = mapping.split("\\.", 2);
            if (parts.length != 2)
                throw new IllegalArgumentException("Invalid base.filter mapping: " + field);
            JSONObject source;
            if ("bond".equals(parts[0])) source = bond;
            else if ("issuer".equals(parts[0])) source = issuer;
            else if ("valuation".equals(parts[0])) source = valuation;
            else throw new IllegalArgumentException("Unknown base.filter source: " + field);
            Object value = source == null ? null : source.get(parts[1]);
            if (ficc.common.FilterFields.BOOLEANS.contains(field))
                value = flag(value == null ? null : value.toString());
            result.put(field, value);
        }
        BondFilterAttributes.normalize(result);
        ficc.common.FilterDictionaries.normalizeAttributes(result);
        if (Boolean.parseBoolean(
                configuration.getProperty("base.filter.simulateMissing", "false"))) {
            ficc.common.FilterSampleData.fillMissing(result);
        }
        return result;
    }

    /** 中证到期和行权数据分别映射，收益率保留百分数单位；空值不能转换为零。 */
    static JSONObject csiValuation(JSONObject raw) {
        JSONObject result = JsonUtils.object(
                "csiValuationDate", date(text(raw, "valuate_date")),
                "csiRecommendedBasis", valuationBasis(String.valueOf(text(raw, "recommendation"))),
                "csiRating", text(raw, "implied_rating"));
        String[][] fields = {
            {"csiValuationYield", "yld_to_maturity"},
            {"csiValuationCleanPrice", "clean_price_maturity"},
            {"csiValuationFullPrice", "full_price_maturity"},
            {"csiExerciseYield", "yld_to_exercise"},
            {"csiExerciseCleanPrice", "clean_price_exercise"},
            {"csiExerciseFullPrice", "full_price_exercise"}
        };
        for (String[] field : fields) {
            String value = text(raw, field[1]);
            result.put(field[0], value == null ? null : new java.math.BigDecimal(value.trim()).toPlainString());
        }
        return result;
    }

    private static Boolean flag(String value) {
        if (value == null || value.trim().isEmpty()) return null;
        String upper = value.toUpperCase(Locale.ROOT);
        if ("Y".equals(upper) || "YES".equals(upper) || "TRUE".equals(upper) || "1".equals(upper))
            return Boolean.TRUE;
        if ("N".equals(upper) || "NO".equals(upper) || "FALSE".equals(upper) || "0".equals(upper))
            return Boolean.FALSE;
        return null;
    }

    private static String marketByGoodsType(String goodsType) {
        if ("2".equals(goodsType)) return "RATE";
        if ("1".equals(goodsType)) return "CREDIT";
        return null;
    }

    private static String valuationBasis(String bondTypeFlag) {
        if ("1".equals(bondTypeFlag)) return "EXERCISE";
        if ("2".equals(bondTypeFlag)) return "MATURITY";
        return null;
    }

    private static String marketSuffix(String marketCode) {
        if ("CIB".equals(marketCode)) return ".IB";
        if ("SSE".equals(marketCode)) return ".SH";
        if ("SZE".equals(marketCode)) return ".SZ";
        return "";
    }

    public static String securityId(JSONObject bond) {
        String rawId = text(bond, "Bond_ID");
        String marketCode = text(bond, "Listed_Market");
        String suffix = marketSuffix(marketCode == null ? "" : marketCode);
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
        return JsonUtils.object(
                "issuerId",
                security.getString("issuerId"),
                "issuerName",
                text(raw, "issuer_name"),
                "issuerType",
                security.getString("issuerType"),
                "issuerRating",
                security.getString("issuerRating"),
                "region",
                security.getString("region"),
                "dataVendor",
                "SUMSCOPE");
    }

    public JSONObject valuation(String securityId, JSONObject raw) {
        if (raw == null || raw.isEmpty()) {
            return null;
        }
        return JsonUtils.object(
                "valuationId",
                text(raw, "Id"),
                "securityId",
                securityId,
                "valuationSource",
                "CHINABOND",
                "dataVendor",
                "SUMSCOPE",
                "valuationDate",
                date(text(raw, "valuation_date")),
                "valuationBasis",
                valuationBasis(String.valueOf(text(raw, "bondTypeFlag"))),
                "yield",
                text(raw, "valuation_yield"),
                "cleanPrice",
                text(raw, "valuation_net_price"));
    }

    public static String text(JSONObject object, String name) {
        if (object == null) {
            return null;
        }
        for (String key : object.keySet()) {
            if (name.equalsIgnoreCase(key)) {
                String value = object.getString(key);
                return value == null || value.trim().isEmpty() ? null : value;
            }
        }
        return null;
    }

    private static String date(String value) {
        if (value == null) {
            return null;
        }
        if (value.matches("[0-9]{8}(\\.0+)?")) {
            return LocalDate.parse(value.substring(0, 8), DateTimeFormatter.BASIC_ISO_DATE)
                    .toString();
        }
        return LocalDate.parse(value.substring(0, 10)).toString();
    }
}
