// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.tag.enums;

import cn.hutool.core.util.StrUtil;

import java.beans.ConstructorProperties;
import java.util.HashMap;
import java.util.Map;

public enum QuoteTagTypeEnum {
    BOND_TYPE_BAK("BOND_TYPE_BAK", "\u503a\u5238\u7c7b\u578b", 4, 12, 60, true, "sign"),
    TERM("TERM", "\u5269\u4f59\u671f\u9650", 4, 11, 56, true, "sign"),
    FIRM("FIRM", "Firm\u7c7b\u578b", 4, 10, 52, true, "sign"),
    SETTL_TYPE("SETTL_TYPE", "\u6e05\u7b97\u901f\u5ea6", 4, 9, 48, true, "sign"),
    SIDE("SIDE", "\u65b9\u5411", 4, 8, 44, true, "sign"),
    VIP("VIP", "VIP", 4, 7, 40, true, "sign"),
    ORG_TYPE("ORG_TYPE", "\u673a\u6784\u7c7b\u578b", 16, 6, 24, false, "sign"),
    ROLE_TYPE("ROLE_TYPE", "\u89d2\u8272\u7c7b\u578b", 4, 5, 20, true, "sign"),
    FIN_BOND_TYPE("FIN_BOND_TYPE", "\u91d1\u878d\u503a\u7c7b\u578b", 4, 4, 16, true, "sign"),
    DQ_TYPE("DQ_TYPE", "\u5b9a\u5411\u56de\u4ef7\u72b6\u6001", 4, 3, 12, true, "sign"),
    LISTED_MARKET("LISTED_MARKET", "\u6d41\u901a\u573a\u6240", 4, 2, 8, true, "sign"),
    CAL_INTER_TYPE("CAL_INTER_TYPE", "\u8ba1\u606f\u65b9\u5f0f", 4, 1, 4, true, "sign"),
    PERPETUAL_TYPE("PERPETUAL_TYPE", "\u6c38\u7eed\u7c7b\u578b", 4, 0, 0, true, "sign"),
    ISSUE_YEAR("ISSUE_YEAR", "\u5e74\u4efd", 8, 16, 56, false, "sign1"),
    CUM_TYPE("CUM_TYPE", "\u542b\u6743\u7c7b\u578b", 2, 15, 54, true, "sign1"),
    HOLIDAY_TYPE(
            "HOLIDAY_TYPE",
            "\u8282\u5047/\u5de5\u4f5c\u65e5\u7c7b\u578b",
            2,
            14,
            52,
            true,
            "sign1"),
    LOCAL_GOV_BOND_TYPE(
            "LOCAL_GOV_BOND_TYPE", "\u5730\u65b9\u503a\u7c7b\u578b", 4, 13, 48, true, "sign1"),
    QUOTATION_TYPE("QUOTATION_TYPE", "\u62a5\u4ef7\u7c7b\u578b", 4, 12, 44, true, "sign1"),
    BOND_SUB_TYPE("BOND_SUB_TYPE", "\u503a\u5238\u7c7b\u578b", 4, 11, 40, true, "sign1"),
    ENTERPRISE_TYPE("ENTERPRISE_TYPE", "\u4f01\u4e1a\u7c7b\u578b", 4, 10, 36, true, "sign1"),
    ISSUE_TYPE("ISSUE_TYPE", "\u53d1\u884c\u7c7b\u578b", 4, 9, 32, true, "sign1"),
    PRINCIPAL_TYPE("PRINCIPAL_TYPE", "\u4e3b\u4f53\u7c7b\u578b", 4, 8, 28, true, "sign1"),
    RANKING_TYPE("RANKING_TYPE", "\u8bc4\u7ea7\u5206\u7c7b", 5, 7, 23, true, "sign1"),
    IF_MUNICIPAL("IF_MUNICIPAL", "\u57ce\u6295", 2, 6, 21, true, "sign1"),
    IF_PLATFORM("IF_PLATFORM", "\u5e73\u53f0\u503a\u7c7b\u578b", 4, 5, 17, true, "sign1"),
    IF_GUARANTEE("IF_GUARANTEE", "\u62c5\u4fdd\u7c7b\u578b", 4, 4, 13, true, "sign1"),
    CUM_RIGHTS_TYPE("CUM_RIGHTS_TYPE", "\u542b\u6743\u7c7b\u578b", 4, 3, 9, true, "sign1"),
    IF_NEW_LISTING("IF_NEW_LISTING", "\u65b0\u4e0a\u5e02", 2, 2, 7, true, "sign1"),
    IF_PLEDGED("IF_PLEDGED", "\u53ef\u8d28\u62bc", 2, 1, 5, true, "sign1"),
    IF_CROSS_MARKET("IF_CROSS_MARKET", "\u8de8\u5e02\u573a", 2, 0, 3, true, "sign1"),
    INDUSTRY_TYPE("INDUSTRY_TYPE", "\u884c\u4e1a\u5206\u7c7b", 11, 3, 53, false, "sign2"),
    REGION_TYPE("REGION_TYPE", "\u5730\u533a\u5206\u7c7b", 12, 2, 41, false, "sign2"),
    ISSUER("ISSUER", "\u53d1\u884c\u4eba", 20, 1, 21, false, "sign2"),
    GUARANTOR("GUARANTOR", "\u62c5\u4fdd\u4eba", 20, 0, 1, false, "sign2"),
    CDC_IMPLITED_RATING(
            "CDC_IMPLITED_RATING",
            "\u4e2d\u503a\u9690\u542b\u8bc4\u7ea7",
            5,
            12,
            59,
            true,
            "sign3"),
    RATING_CURRENT("RATING_CURRENT", "\u503a\u9879\u8bc4\u7ea7", 5, 11, 54, true, "sign3"),
    ISSUER_RATING_CURRENT_CBR(
            "ISSUER_RATING_CURRENT_CBR",
            "\u4e2d\u503a\u8d44\u4fe1\u8bc4\u7ea7",
            5,
            10,
            49,
            true,
            "sign3"),
    CSI_IMPLIED_RATING(
            "CSI_IMPLIED_RATING", "\u4e2d\u8bc1\u9690\u542b\u8bc4\u7ea7", 5, 9, 44, true, "sign3"),
    IF_CONVERTIBLE("IF_CONVERTIBLE", "\u53ef\u4ea4\u6362", 2, 8, 42, true, "sign3"),
    NCD_TYPE("NCD_TYPE", "NCD\u7c7b\u578b", 4, 7, 38, true, "sign3"),
    FIN_INST_BOND("FIN_INST_BOND", "\u91d1\u878d\u673a\u6784\u503a", 3, 6, 35, true, "sign3"),
    ADMIN_LEVEL("ADMIN_LEVEL", "\u884c\u653f\u7ea7\u522b", 3, 5, 32, true, "sign3"),
    IRS_TYPE("IRS_TYPE", "IRS\u7c7b\u578b", 4, 4, 28, true, "sign3"),
    IRS_PRODUCT_TYPE("IRS_PRODUCT_TYPE", "IRS\u4ea7\u54c1\u7c7b\u578b", 6, 3, 22, false, "sign3"),
    IRS_TERM("IRS_TERM", "IRS\u671f\u9650", 4, 2, 18, true, "sign3"),
    IF_EXERCISE("IF_EXERCISE", "\u662f\u5426\u884c\u6743", 2, 1, 16, true, "sign3"),
    IF_REPAY("IF_REPAY", "\u662f\u5426\u63d0\u524d\u8fd8\u672c", 2, 0, 14, true, "sign3"),
    INNOVATION_TYPE("INNOVATION_TYPE", "\u521b\u65b0\u503a\u7c7b\u578b", 4, 0, 0, true, "sign4"),
    IF_MARKET_MAKING("IF_MARKET_MAKING", "\u662f\u5426\u505a\u5e02", 2, 1, 4, true, "sign4"),
    IF_TIER2_PERP(
            "IF_TIER2_PERP",
            "\u662f\u5426\u4e8c\u7ea7\u8d44\u672c\u6c38\u7eed\u503a",
            2,
            2,
            6,
            true,
            "sign4"),
    BOND_SUB_TYPE2("BOND_SUB_TYPE2", "\u503a\u5238\u7c7b\u578b2", 4, 3, 8, false, "sign4"),
    IF_DEFAULT("IF_DEFAULT", "\u662f\u5426\u8fdd\u7ea6", 2, 4, 12, true, "sign4"),
    IF_GREEN_BOND("IF_GREEN_BOND", "\u662f\u5426\u7eff\u8272\u503a", 2, 5, 14, true, "sign4"),
    IF_TAX_FREE("IF_TAX_FREE", "\u662f\u5426\u514d\u7a0e", 2, 6, 16, true, "sign4");

    private String code;
    private String name;
    private Integer length;
    private Integer index;
    private Integer start;
    private Boolean ifFixed;
    private String signField;
    private static Map<String, Map<Integer, QuoteTagTypeEnum>> indexMap;
    private static Map<String, QuoteTagTypeEnum> codeMap;

    public static QuoteTagTypeEnum getByIndex(Integer index, String signField) {
        Map<Integer, QuoteTagTypeEnum> map = indexMap.get(signField);
        if (map == null) {
            return null;
        }
        return map.get(index);
    }

    public static QuoteTagTypeEnum getByCode(String code) {
        if (StrUtil.isBlank((CharSequence) code)) {
            return null;
        }
        return codeMap.get(code);
    }

    public static String getDefaultTag(QuoteTagTypeEnum typeEnum) {
        return StrUtil.fill((String) "", (char) '0', (int) typeEnum.getLength(), (boolean) true);
    }

    public static Map<Integer, QuoteTagTypeEnum> getSignMap(String signField) {
        Map<Integer, QuoteTagTypeEnum> map = indexMap.get(signField);
        if (map == null) {
            return new HashMap<Integer, QuoteTagTypeEnum>();
        }
        return map;
    }

    public static Map<String, QuoteTagTypeEnum> getCodeMap() {
        return codeMap;
    }

    public static Map<String, Map<Integer, QuoteTagTypeEnum>> getIndexMap() {
        return indexMap;
    }

    public String getCode() {
        return this.code;
    }

    public String getName() {
        return this.name;
    }

    public Integer getLength() {
        return this.length;
    }

    public Integer getIndex() {
        return this.index;
    }

    public Integer getStart() {
        return this.start;
    }

    public Boolean getIfFixed() {
        return this.ifFixed;
    }

    public String getSignField() {
        return this.signField;
    }

    @ConstructorProperties(
            value = {"code", "name", "length", "index", "start", "ifFixed", "signField"})
    private QuoteTagTypeEnum(
            String code,
            String name,
            Integer length,
            Integer index,
            Integer start,
            Boolean ifFixed,
            String signField) {
        this.code = code;
        this.name = name;
        this.length = length;
        this.index = index;
        this.start = start;
        this.ifFixed = ifFixed;
        this.signField = signField;
    }

    static {
        indexMap = new HashMap<String, Map<Integer, QuoteTagTypeEnum>>();
        codeMap = new HashMap<String, QuoteTagTypeEnum>();
        for (QuoteTagTypeEnum quoteTagTypeEnum : QuoteTagTypeEnum.values()) {
            String signField1 = quoteTagTypeEnum.getSignField();
            if (StrUtil.isBlank((CharSequence) signField1)) continue;
            Map<Integer, QuoteTagTypeEnum> map = indexMap.get(signField1);
            if (map == null) {
                map = new HashMap<Integer, QuoteTagTypeEnum>();
            }
            map.put(quoteTagTypeEnum.getIndex(), quoteTagTypeEnum);
            indexMap.put(signField1, map);
            codeMap.put(quoteTagTypeEnum.getCode(), quoteTagTypeEnum);
        }
    }
}
