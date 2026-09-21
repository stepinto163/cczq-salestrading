package ficc.common;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import java.util.Collection;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.List;

/** Stable frontend codes. Vendor aliases are accepted only at normalization boundaries. */
public final class FilterDictionaries {
    private static final org.apache.log4j.Logger LOGGER = org.apache.log4j.Logger.getLogger(FilterDictionaries.class);
    private static final Map<String, JSONArray> DICTIONARIES = new LinkedHashMap<>();
    private static final Map<String, Map<String, String>> VALUES = new LinkedHashMap<>();

    static {
        register("side", "买入=1,卖出=2");
        register("quoteType", "收益率=1,净价=2,全价=3");
        register("yieldType", "到期=1,行权=2");
        register("bondSubtype", "国债=National,国开=CDB,农发=AGB,口行=NEB,地方债=LGB,短融=CP,中票=MTN,企业债=EnterpriseBond,公司债=CorporatebBonds,PPN=PPN,ABS=ABS,NCD=NCD,其他=Other,金融债=FINANCIAL_BOND,可转债=CONVERTIBLE_BOND");
        register("productType", "利率债=RATE,信用债=CREDIT,NCD=NCD");
        register("hotTags", "商金债=SJZ,银行二级=YHEJ,银行永续=YHYX,政策性次级=POLICY_BANK_SUBORDINATED,保险次级=BXCJ,券商债=SECURITIES_COMPANY_BOND,保险债=INSURANCE_BOND,铁道债=TD,可质押=PLEDGEABLE,跨市场=CROSS_MARKET,城投=CTZ,房地产=DC,产能过剩=OVERCAPACITY,公用事业=UTILITIES,上市公司=LISTED_COMPANY,科创债=KC,TLAC债=TLAC,新上市=NEWLY_LISTED,不含假期=EXCLUDE_HOLIDAYS,绿色债=LZ");
        register("ncdType", "国有=STATE_OWNED,股份=JOINT_STOCK,城商=CITY_COMMERCIAL,农商=RURAL_COMMERCIAL,外资=FOREIGN,其他=OTHER");
        register("listingStatus", "上市=SS,非上市=NOTSS");
        register("issueMethod", "公募=GM,小公募=XGM,私募=SM");
        register("liquidityGrade", "S=S,A=A,B=B,C=C,D=D");
        register("marketMakerTags", "上证信用=SSE_CREDIT,深证信用=SZSE_CREDIT,上证利率=SSE_RATE,深证利率=SZSE_RATE");
        register("listedMarket", "银行间=CIB,上交所=SSE,深交所=SZSE,北交所=BSE");
        register("issuerType", "政府=GOVERNMENT,政策银行=POLICY_BANK,企业=CORPORATE,商业银行=COMMERCIAL_BANK,证券公司=SECURITIES_COMPANY,保险公司=INSURANCE_COMPANY,其他金融机构=OTHER_FINANCIAL,其他=OTHER");
        register("bankType", "股份制=GF,城商行=CS,农商行=NS,国有行=STATE_OWNED,外资行=FOREIGN,其他=OTHER");
        register("industry", "城投=CITY_INVESTMENT,房地产=REAL_ESTATE,煤炭=COAL,钢铁=STEEL,化工=CHEMICAL,建筑=CONSTRUCTION,交运=TRANSPORT,公用事业=UTILITIES,金融=FINANCE,其他=OTHER");
        register("ownershipType", "央企=YQ,国企=GQ,民企=MQ,其他=Other");
        register("guaranteeType", "有担保=YDB,无担保=WDB,全国性担保=NATIONAL_GUARANTEE,地方性担保=LOCAL_GUARANTEE");
        register("issuerRating", "AAA=AAA,AA+=AA+,AA=AA,AA-=AA-,A+=A+,A=A");
        register("issuerCbrRating", "AAA+=AAA+,AAA=AAA,AAA-=AAA-,AA+=AA+");
        register("ratingOutlook", "正面=POSITIVE,稳定=STABLE,观察=WATCH,负面=NEGATIVE");
        register("bondRating", "AAA=AAA,A-1=A-1,AA+=AA+,AA=AA,AA-=AA-");
        register("bondCbrRating", "AAA+=AAA+,AAA=AAA,AAA-=AAA-,AA+=AA+");
        register("yyRating", "投资级=INVESTMENT_GRADE,投机级=SPECULATIVE_GRADE,风险级=HIGH_RISK");
        register("csiRating", "AAA+=AAA+,AAA=AAA,AA+=AA+,AA=AA,AA2=AA2");
        register("optionType", "不含权=NON,回售权=PUT,赎回及回售权=CNP,赎回权=CAL,其他权（ETS）=ETS,其他权（ASS）=ASS,其他权（DCN）=DCN,其他权（CNV）=CNV");
        register("repaymentType", "到期=DQCF,提前还本=TQHB,其他=OTHER");
        register("couponType", "固定利率=FIXED,浮动利率=FLOATING,零息=ZERO_COUPON,其他=OTHER");
        register("couponFrequency", "按月=M1,按季=M3,半年=M6,按年=Y1,其他=Other");
        register("perpetualType", "城投永续=MUNICIPAL_PERPETUAL,产业永续=CORPORATE_PERPETUAL,金融永续=FINANCIAL_PERPETUAL,非永续=NON_PERPETUAL");
        register("rateIndex", "SHIBOR=Shibor,DEPO=Depo,LPR=LPR,固息=Regular,其他=OTHER");
        register("keyProvince", "12重点省=KEY_12,非12重点省=NON_KEY_12");
        register("municipalLevel", "省级=Province,市级=City,区县级=District,其他=Other");
        register("regionalRating", "区域A级=REGION_A,区域B级=REGION_B,区域C级=REGION_C,区域D级=REGION_D");
        register("regionCategory", "好地区=GOOD,中等地区=AVERAGE,弱地区=WEAK,差地区=POOR");
        register("localGovernmentType", "一般债=Normal,专项债=Special");
        register("localGovernmentUse", "棚改=SHANTYTOWN_RENOVATION,土储=LAND_RESERVE,收费公路=TOLL_ROAD,基础设施=INFRASTRUCTURE,其他=OTHER");
        register("region", "北京市=110000,天津市=120000,河北省=130000,山西省=140000,内蒙古自治区=150000,辽宁省=210000,吉林省=220000,黑龙江省=230000,上海市=310000,江苏省=320000,浙江省=330000,安徽省=340000,福建省=350000,江西省=360000,山东省=370000,河南省=410000,湖北省=420000,湖南省=430000,广东省=440000,广西壮族自治区=450000,海南省=460000,重庆市=500000,四川省=510000,贵州省=520000,云南省=530000,西藏自治区=540000,陕西省=610000,甘肃省=620000,青海省=630000,宁夏回族自治区=640000,新疆维吾尔自治区=650000");
        register("regionCode", "北京市=110000,天津市=120000,河北省=130000,山西省=140000,内蒙古自治区=150000,辽宁省=210000,吉林省=220000,黑龙江省=230000,上海市=310000,江苏省=320000,浙江省=330000,安徽省=340000,福建省=350000,江西省=360000,山东省=370000,河南省=410000,湖北省=420000,湖南省=430000,广东省=440000,广西壮族自治区=450000,海南省=460000,重庆市=500000,四川省=510000,贵州省=520000,云南省=530000,西藏自治区=540000,陕西省=610000,甘肃省=620000,青海省=630000,宁夏回族自治区=640000,新疆维吾尔自治区=650000");
        register("localGovernmentRegion", "北京市=110000,天津市=120000,河北省=130000,山西省=140000,内蒙古自治区=150000,辽宁省=210000,吉林省=220000,黑龙江省=230000,上海市=310000,江苏省=320000,浙江省=330000,安徽省=340000,福建省=350000,江西省=360000,山东省=370000,河南省=410000,湖北省=420000,湖南省=430000,广东省=440000,广西壮族自治区=450000,海南省=460000,重庆市=500000,四川省=510000,贵州省=520000,云南省=530000,西藏自治区=540000,陕西省=610000,甘肃省=620000,青海省=630000,宁夏回族自治区=640000,新疆维吾尔自治区=650000");
        register("city", "北京市=110100,天津市=120100,上海市=310100,重庆市=500100,深圳市=440300,厦门市=350200,宁波市=330200,青岛市=370200,大连市=210200");
        register("settlType", "T+0=1,T+1=2,T+2=3,T+3=4,T+4=5,T+5=9,T+6=10,T+7=11,T+8=12,T+9=13,T+10=14,T+11=15,T+12=16");
        aliases("listingStatus", "已上市=SS,未上市=NOTSS");
        aliases("listedMarket", "SZE=SZSE");
        aliases("bankType", "股份行=GF");
        aliases("ownershipType", "CGE=YQ,LGE=GQ,PVE=MQ,OTE=Other");
        aliases("couponFrequency", "M=M1,12=M1,Q=M3,4=M3,S=M6,2=M6,A=Y1,1=Y1,N=Other");
        aliases("couponType", "FRB=FIXED,FIX=FIXED,FRN=FLOATING");
        aliases("rateIndex", "SHIBOR=Shibor,DEPO=Depo");
        aliases("regionalRating", "模拟A级=REGION_A");
        for (String field : List.of("region", "regionCode", "localGovernmentRegion")) {
            for (Object entry : DICTIONARIES.get(field)) {
                JSONObject option = (JSONObject) entry;
                String shortName = option.getString("label")
                        .replaceAll("壮族自治区$|回族自治区$|维吾尔自治区$|自治区$|省$|市$", "");
                VALUES.get(field).put(shortName, option.getString("value"));
            }
        }
    }

    private FilterDictionaries() {
    }

    private static void register(String field, String entries) {
        JSONArray dictionary = new JSONArray();
        DICTIONARIES.put(field, dictionary);
        VALUES.put(field, new LinkedHashMap<>());
        for (String entry : entries.split(",")) {
            String[] pair = entry.split("=", 2);
            dictionary.add(JsonUtils.object("value", pair[1], "label", pair[0]));
            VALUES.get(field).put(pair[0], pair[1]);
            VALUES.get(field).put(pair[1], pair[1]);
        }
    }

    private static void aliases(String field, String entries) {
        for (String entry : entries.split(",")) {
            String[] pair = entry.split("=", 2);
            VALUES.get(field).put(pair[0], pair[1]);
        }
    }

    public static boolean contains(String field) {
        return DICTIONARIES.containsKey(field);
    }

    public static JSONArray options(String field) {
        JSONArray options = DICTIONARIES.get(field);
        return options == null ? new JSONArray() : JSONArray.parseArray(options.toJSONString());
    }

    public static JSONObject dictionaries() {
        JSONObject result = new JSONObject();
        for (String field : DICTIONARIES.keySet()) {
            result.put(field, options(field));
        }
        return result;
    }

    public static String canonical(String field, Object value) {
        if (value == null) {
            return null;
        }
        Map<String, String> values = VALUES.get(field);
        if (values == null) {
            return value.toString();
        }
        String text = value.toString();
        // City dictionaries can grow with reference data; use normalized six-digit IDs.
        if ("city".equals(field) && text.matches("[0-9]{6}") && !text.endsWith("0000")) {
            return text;
        }
        return values.get(text);
    }

    public static void normalizeAttributes(JSONObject attributes) {
        for (String field : DICTIONARIES.keySet()) {
            Object original = attributes.get(field);
            if (original == null) {
                continue;
            }
            if (original instanceof Collection<?> entries) {
                JSONArray normalized = new JSONArray();
                for (Object entry : entries) {
                    String code = canonical(field, entry);
                    if (code != null && !normalized.contains(code)) {
                        normalized.add(code);
                    }
                }
                if (normalized.isEmpty()) {
                    attributes.remove(field);
                } else {
                    attributes.put(field, normalized);
                }
            } else {
                String code = canonical(field, original);
                if (code == null) {
                    LOGGER.debug("Unmapped reference enum: field=" + field + ", value=" + original);
                    attributes.remove(field);
                } else {
                    attributes.put(field, code);
                }
            }
        }
        Object guarantee = attributes.get("guaranteeType");
        if ("NATIONAL_GUARANTEE".equals(guarantee) || "LOCAL_GUARANTEE".equals(guarantee)) {
            attributes.put("guaranteeType", JsonUtils.array("YDB", guarantee));
        }
    }

    /** Upgrade known labels in persisted filters without rewriting their revisions. */
    public static void migrateSavedFilters(Object node) {
        if (node instanceof JSONObject object) {
            if (object.get("field") instanceof String field && contains(field)
                    && !"EXISTS".equals(object.getString("operator"))) {
                Object value = object.get("value");
                if (value instanceof JSONArray array) {
                    for (int index = 0; index < array.size(); index++) {
                        String code = canonical(field, array.get(index));
                        if (code != null) {
                            array.set(index, code);
                        }
                    }
                } else {
                    String code = canonical(field, value);
                    if (code == null) {
                        LOGGER.warn("Saved filter needs review: field=" + field + ", value=" + value);
                    }
                    if (code != null) {
                        object.put("value", code);
                        if ("CONTAINS".equals(object.getString("operator"))) {
                            object.put("operator", "EQ");
                        }
                    }
                }
            }
            for (Object value : object.values()) {
                migrateSavedFilters(value);
            }
        } else if (node instanceof Collection<?> values) {
            for (Object value : values) {
                migrateSavedFilters(value);
            }
        }
    }
}
