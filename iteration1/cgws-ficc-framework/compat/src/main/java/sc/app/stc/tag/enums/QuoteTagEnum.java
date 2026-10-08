// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.tag.enums;

import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.util.StrUtil;

import java.beans.ConstructorProperties;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public enum QuoteTagEnum {
    BOND_TYPE_BAK_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.BOND_TYPE_BAK.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.BOND_TYPE_BAK),
    TERM_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.TERM.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.TERM),
    FIRM_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.FIRM.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.FIRM),
    SETTL_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.SETTL_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.SETTL_TYPE),
    SIDE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.SIDE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.SIDE),
    VIP_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.VIP.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.VIP),
    ORG_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.ORG_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.ORG_TYPE),
    ORG_TYPE_OTHERS(
            StrUtil.fill(
                            (String) "",
                            (char) '0',
                            (int) (QuoteTagTypeEnum.ORG_TYPE.getLength() - 1),
                            (boolean) true)
                    + "1",
            "\u5176\u4ed6",
            QuoteTagTypeEnum.ORG_TYPE),
    ROLE_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.ROLE_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.ROLE_TYPE),
    FIN_BOND_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.FIN_BOND_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.FIN_BOND_TYPE),
    DQ_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.DQ_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.DQ_TYPE),
    LISTED_MARKET_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.LISTED_MARKET.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.LISTED_MARKET),
    CAL_INTER_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.CAL_INTER_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.CAL_INTER_TYPE),
    PERPETUAL_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.PERPETUAL_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.PERPETUAL_TYPE),
    ISSUE_YEAR_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.ISSUE_YEAR.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.ISSUE_YEAR),
    CUM_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.CUM_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.CUM_TYPE),
    HOLIDAY_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.HOLIDAY_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.HOLIDAY_TYPE),
    LOCAL_GOV_BOND_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.LOCAL_GOV_BOND_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.LOCAL_GOV_BOND_TYPE),
    QUOTATION_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.QUOTATION_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.QUOTATION_TYPE),
    BOND_SUB_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.BOND_SUB_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.BOND_SUB_TYPE),
    ENTERPRISE_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.ENTERPRISE_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.ENTERPRISE_TYPE),
    ISSUE_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.ISSUE_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.ISSUE_TYPE),
    PRINCIPAL_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.PRINCIPAL_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.PRINCIPAL_TYPE),
    RANKING_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.RANKING_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.RANKING_TYPE),
    IF_MUNICIPAL_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_MUNICIPAL.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_MUNICIPAL),
    IF_PLATFORM_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_PLATFORM.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_PLATFORM),
    IF_GUARANTEE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_GUARANTEE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_GUARANTEE),
    CUM_RIGHTS_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.CUM_RIGHTS_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.CUM_RIGHTS_TYPE),
    IF_NEW_LISTING_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_NEW_LISTING.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_NEW_LISTING),
    IF_PLEDGED_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_PLEDGED.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_PLEDGED),
    IF_CROSS_MARKET_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_CROSS_MARKET.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_CROSS_MARKET),
    INDUSTRY_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.INDUSTRY_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.INDUSTRY_TYPE),
    INDUSTRY_TYPE_OTHER(
            StrUtil.fill(
                            (String) "",
                            (char) '0',
                            (int) (QuoteTagTypeEnum.INDUSTRY_TYPE.getLength() - 1),
                            (boolean) true)
                    + "1",
            "\u5176\u4ed6",
            QuoteTagTypeEnum.INDUSTRY_TYPE),
    REGION_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.REGION_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.REGION_TYPE),
    ISSUER_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.ISSUER.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.ISSUER),
    GUARANTOR_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.GUARANTOR.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.GUARANTOR),
    CDC_IMPLITED_RATING_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.CDC_IMPLITED_RATING.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    RATING_CURRENT_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.RATING_CURRENT.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.RATING_CURRENT),
    ISSUER_RATING_CURRENT_CBR_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.ISSUER_RATING_CURRENT_CBR.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.ISSUER_RATING_CURRENT_CBR),
    CSI_IMPLIED_RATING_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.CSI_IMPLIED_RATING.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.CSI_IMPLIED_RATING),
    IF_CONVERTIBLE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_CONVERTIBLE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_CONVERTIBLE),
    NCD_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.NCD_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.NCD_TYPE),
    FIN_INST_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.FIN_INST_BOND.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.FIN_INST_BOND),
    IRS_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IRS_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IRS_TYPE),
    IRS_PRODUCT_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IRS_PRODUCT_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IRS_PRODUCT_TYPE),
    IRS_TERM_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IRS_TERM.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IRS_TERM),
    IF_EXERCISE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_EXERCISE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_EXERCISE),
    IF_REPAY_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_REPAY.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_REPAY),
    ADMIN_LEVEL_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.ADMIN_LEVEL.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.ADMIN_LEVEL),
    INNOVATION_TYPE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.INNOVATION_TYPE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.INNOVATION_TYPE),
    IF_MARKET_MAKING_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_MARKET_MAKING.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_MARKET_MAKING),
    IF_TIER2_PERP_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_TIER2_PERP.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_TIER2_PERP),
    BOND_SUB_TYPE2_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.BOND_SUB_TYPE2.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.BOND_SUB_TYPE2),
    IF_DEFAULT_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_DEFAULT.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_DEFAULT),
    IF_GREEN_BOND_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_GREEN_BOND.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_GREEN_BOND),
    IF_TAX_FREE_DEFAULT(
            StrUtil.fill(
                    (String) "",
                    (char) '0',
                    (int) QuoteTagTypeEnum.IF_TAX_FREE.getLength(),
                    (boolean) true),
            "\u65e0",
            QuoteTagTypeEnum.IF_TAX_FREE),
    LESS_THAN_3M("0001", "<3M", QuoteTagTypeEnum.TERM),
    THREE_TO_SIX_MONTHS("0010", "3M-6M", QuoteTagTypeEnum.TERM),
    SIX_TO_NINE_MONTHS("0011", "6M-9M", QuoteTagTypeEnum.TERM),
    NINE_TO_TWELVE_MONTHS("0100", "9M-12M", QuoteTagTypeEnum.TERM),
    ONE_TO_THREE_YEARS("0101", "1Y-3Y", QuoteTagTypeEnum.TERM),
    THREE_TO_FIVE_YEARS("0110", "3Y-5Y", QuoteTagTypeEnum.TERM),
    FIVE_TO_TEN_YEARS("0111", "5Y-10Y", QuoteTagTypeEnum.TERM),
    MORE_THAN_TEN_YEARS("1000", ">10Y", QuoteTagTypeEnum.TERM),
    FIVE_TO_SEVEN_YEARS("1001", "5Y-7Y", QuoteTagTypeEnum.TERM),
    SEVEN_TO_TEN_YEARS("1010", "7Y-10Y", QuoteTagTypeEnum.TERM),
    TEN_TO_TWENTY_YEARS("1011", "10Y-20Y", QuoteTagTypeEnum.TERM),
    TWENTY_TO_THIRTY_YEARS("1100", "20Y-30Y", QuoteTagTypeEnum.TERM),
    MORE_THAN_THIRTY_YEARS("1101", ">30Y", QuoteTagTypeEnum.TERM),
    FIRM("0001", "Firm", QuoteTagTypeEnum.FIRM),
    NON_FIRM("0010", "NonFirm", QuoteTagTypeEnum.FIRM),
    DEMAND("0011", "\u9700\u6c42", QuoteTagTypeEnum.FIRM),
    T0("0001", "T+0", QuoteTagTypeEnum.SETTL_TYPE),
    T1("0010", "T+1", QuoteTagTypeEnum.SETTL_TYPE),
    N0("0011", "\u660e+0", QuoteTagTypeEnum.SETTL_TYPE),
    N1("0100", "\u660e+1", QuoteTagTypeEnum.SETTL_TYPE),
    FUTURE("0101", "\u8fdc\u671f", QuoteTagTypeEnum.SETTL_TYPE),
    BUY("0001", "\u4e70\u5165", QuoteTagTypeEnum.SIDE),
    SELL("0010", "\u5356\u51fa", QuoteTagTypeEnum.SIDE),
    BOTHSIDE("0011", "\u4e70\u5165&\u5356\u51fa", QuoteTagTypeEnum.SIDE),
    VIP("0001", "VIP", QuoteTagTypeEnum.VIP),
    NON_VIP("0010", "\u975eVIP", QuoteTagTypeEnum.VIP),
    SALE("0001", "\u9500\u4ea4", QuoteTagTypeEnum.ROLE_TYPE),
    MARKET_MAKER("0010", "\u505a\u5e02", QuoteTagTypeEnum.ROLE_TYPE),
    CDB("0001", "\u56fd\u5f00", QuoteTagTypeEnum.FIN_BOND_TYPE),
    CEXIM("0010", "\u53e3\u884c", QuoteTagTypeEnum.FIN_BOND_TYPE),
    ADBC("0011", "\u519c\u53d1", QuoteTagTypeEnum.FIN_BOND_TYPE),
    DQ_REPLIED("0001", "\u505a\u5e02\u5df2\u56de", QuoteTagTypeEnum.DQ_TYPE),
    DQ_UNREPLIED("0010", "\u505a\u5e02\u672a\u56de", QuoteTagTypeEnum.DQ_TYPE),
    SSE("0001", "\u4e0a\u4ea4", QuoteTagTypeEnum.LISTED_MARKET),
    SZE("0010", "\u6df1\u4ea4", QuoteTagTypeEnum.LISTED_MARKET),
    CIB("0011", "\u94f6\u884c\u95f4", QuoteTagTypeEnum.LISTED_MARKET),
    SHIBOR("0001", "SHIBOR", QuoteTagTypeEnum.CAL_INTER_TYPE),
    DEPO("0010", "DEPO", QuoteTagTypeEnum.CAL_INTER_TYPE),
    FRB("0011", "\u56fa\u606f", QuoteTagTypeEnum.CAL_INTER_TYPE),
    LPR("0100", "LPR", QuoteTagTypeEnum.CAL_INTER_TYPE),
    DR("0101", "DR", QuoteTagTypeEnum.CAL_INTER_TYPE),
    PERPETUAL_SUB("0001", "\u6c38\u7eed\u6b21\u7ea7", QuoteTagTypeEnum.PERPETUAL_TYPE),
    PERPETUAL_UN_SUB("0010", "\u6c38\u7eed\u975e\u6b21\u7ea7", QuoteTagTypeEnum.PERPETUAL_TYPE),
    NON_PERPETUAL("0011", "\u975e\u6c38\u7eed", QuoteTagTypeEnum.PERPETUAL_TYPE),
    CUM("01", "\u542b\u6743", QuoteTagTypeEnum.CUM_TYPE),
    NON_CUM("10", "\u4e0d\u542b\u6743", QuoteTagTypeEnum.CUM_TYPE),
    EXERCISED("01", "\u5df2\u884c\u6743", QuoteTagTypeEnum.IF_EXERCISE),
    WAIT_CUM("10", "\u5f85\u884c\u6743", QuoteTagTypeEnum.IF_EXERCISE),
    WORK_DAY("01", "\u5de5\u4f5c\u65e5", QuoteTagTypeEnum.HOLIDAY_TYPE),
    HOLIDAY("10", "\u8282\u5047\u65e5", QuoteTagTypeEnum.HOLIDAY_TYPE),
    GENERAL_BOND("0001", "\u4e00\u822c\u503a", QuoteTagTypeEnum.LOCAL_GOV_BOND_TYPE),
    SPECIAL_BOND("0010", "\u4e13\u9879\u503a", QuoteTagTypeEnum.LOCAL_GOV_BOND_TYPE),
    INTENTION("0001", "\u610f\u5411\u62a5\u4ef7", QuoteTagTypeEnum.QUOTATION_TYPE),
    SECONDARY("0010", "\u4e8c\u7ea7\u62a5\u4ef7", QuoteTagTypeEnum.QUOTATION_TYPE),
    CSP_BOND("0001", "\u8d85\u77ed\u878d", QuoteTagTypeEnum.BOND_SUB_TYPE),
    LCP_BOND("0010", "\u77ed\u878d", QuoteTagTypeEnum.BOND_SUB_TYPE),
    CMN_BOND("0011", "\u4e2d\u7968", QuoteTagTypeEnum.BOND_SUB_TYPE),
    ENT_BOND("0100", "\u4f01\u4e1a\u503a", QuoteTagTypeEnum.BOND_SUB_TYPE),
    COP_BOND("0101", "\u516c\u53f8\u503a", QuoteTagTypeEnum.BOND_SUB_TYPE),
    SUB_BOND("0110", "\u6b21\u7ea7\u503a", QuoteTagTypeEnum.BOND_SUB_TYPE),
    PPN_BOND("0111", "PPN", QuoteTagTypeEnum.BOND_SUB_TYPE),
    OTHER_BOND("1000", "\u5176\u4ed6", QuoteTagTypeEnum.BOND_SUB_TYPE),
    NCD_BOND("1001", "NCD", QuoteTagTypeEnum.BOND_SUB_TYPE),
    ABS_BOND("1010", "ABS", QuoteTagTypeEnum.BOND_SUB_TYPE),
    NATIONAL_DEBT("1011", "\u56fd\u503a", QuoteTagTypeEnum.BOND_SUB_TYPE),
    CENTRAL_BANK_TICKET("1100", "\u592e\u7968", QuoteTagTypeEnum.BOND_SUB_TYPE),
    FINANCIAL_DEBT("1101", "\u91d1\u878d\u503a", QuoteTagTypeEnum.BOND_SUB_TYPE),
    LOCAL_DEBT("1110", "\u5730\u65b9\u503a", QuoteTagTypeEnum.BOND_SUB_TYPE),
    COMMERCIAL_BANK_DEBT("1111", "\u5546\u4e1a\u94f6\u884c\u503a", QuoteTagTypeEnum.BOND_SUB_TYPE),
    CGE("0001", "\u592e\u4f01", QuoteTagTypeEnum.ENTERPRISE_TYPE),
    LGE("0010", "\u56fd\u4f01", QuoteTagTypeEnum.ENTERPRISE_TYPE),
    PVE("0011", "\u6c11\u4f01", QuoteTagTypeEnum.ENTERPRISE_TYPE),
    OTE("0100", "\u5176\u4ed6\u4e3b\u4f53", QuoteTagTypeEnum.ENTERPRISE_TYPE),
    BPB("0001", "\u516c\u52df", QuoteTagTypeEnum.ISSUE_TYPE),
    PUB("0010", "\u5c0f\u516c\u52df", QuoteTagTypeEnum.ISSUE_TYPE),
    PRI("0011", "\u79c1\u52df", QuoteTagTypeEnum.ISSUE_TYPE),
    MCB_PRINCIPAL("0001", "\u5927\u884c", QuoteTagTypeEnum.PRINCIPAL_TYPE),
    SHB_PRINCIPAL("0010", "\u80a1\u4efd\u5236", QuoteTagTypeEnum.PRINCIPAL_TYPE),
    SPB_PRINCIPAL("0011", "\u653f\u7b56\u6027", QuoteTagTypeEnum.PRINCIPAL_TYPE),
    CCB_PRINCIPAL("0100", "\u57ce\u5546\u884c", QuoteTagTypeEnum.PRINCIPAL_TYPE),
    FRB_PRINCIPAL("0101", "\u5916\u8d44\u884c", QuoteTagTypeEnum.PRINCIPAL_TYPE),
    RRB_PRINCIPAL("0110", "\u519c\u4fe1", QuoteTagTypeEnum.PRINCIPAL_TYPE),
    RTB_PRINCIPAL("0111", "\u6751\u9547", QuoteTagTypeEnum.PRINCIPAL_TYPE),
    OTHER_PRINCIPAL("1000", "\u5176\u4ed6\u4e3b\u4f53", QuoteTagTypeEnum.PRINCIPAL_TYPE),
    AAA_PLUS("00001", "AAA+", QuoteTagTypeEnum.RANKING_TYPE),
    AAA("00010", "AAA", QuoteTagTypeEnum.RANKING_TYPE),
    AA_PLUS("00011", "AA+", QuoteTagTypeEnum.RANKING_TYPE),
    AA("00100", "AA", QuoteTagTypeEnum.RANKING_TYPE),
    AA_MINUS("00101", "AA-", QuoteTagTypeEnum.RANKING_TYPE),
    A_PLUS("00110", "A+", QuoteTagTypeEnum.RANKING_TYPE),
    A("00111", "A", QuoteTagTypeEnum.RANKING_TYPE),
    A_MINUS("01000", "A-", QuoteTagTypeEnum.RANKING_TYPE),
    A1("01001", "A1", QuoteTagTypeEnum.RANKING_TYPE),
    BBB_PLUS("01010", "BBB+", QuoteTagTypeEnum.RANKING_TYPE),
    BBB("01011", "BBB", QuoteTagTypeEnum.RANKING_TYPE),
    BBB_MINUS("01100", "BBB-", QuoteTagTypeEnum.RANKING_TYPE),
    BB_PLUS("01101", "BB+", QuoteTagTypeEnum.RANKING_TYPE),
    BB("01110", "BB", QuoteTagTypeEnum.RANKING_TYPE),
    BB_MINUS("01111", "BB-", QuoteTagTypeEnum.RANKING_TYPE),
    B_PLUS("10000", "B+", QuoteTagTypeEnum.RANKING_TYPE),
    B("10001", "B", QuoteTagTypeEnum.RANKING_TYPE),
    B_MINUS("10010", "B-", QuoteTagTypeEnum.RANKING_TYPE),
    CCL("10011", "CCL", QuoteTagTypeEnum.RANKING_TYPE),
    CC("10100", "CC", QuoteTagTypeEnum.RANKING_TYPE),
    C("10101", "C", QuoteTagTypeEnum.RANKING_TYPE),
    MUNICIPAL("01", "\u57ce\u6295\u503a", QuoteTagTypeEnum.IF_MUNICIPAL),
    NON_MUNICIPAL("10", "\u975e\u57ce\u6295\u503a", QuoteTagTypeEnum.IF_MUNICIPAL),
    PLATFORM("0001", "\u5e73\u53f0\u503a", QuoteTagTypeEnum.IF_PLATFORM),
    NON_PLATFORM("0010", "\u975e\u5e73\u53f0\u503a", QuoteTagTypeEnum.IF_PLATFORM),
    GUARANTEE("0001", "\u6709\u62c5\u4fdd", QuoteTagTypeEnum.IF_GUARANTEE),
    NON_GUARANTEE("0010", "\u65e0\u62c5\u4fdd", QuoteTagTypeEnum.IF_GUARANTEE),
    CUM_RIGHTS("0001", "\u542b\u6743", QuoteTagTypeEnum.CUM_RIGHTS_TYPE),
    NON_CUM_RIGHTS("0010", "\u975e\u542b\u6743", QuoteTagTypeEnum.CUM_RIGHTS_TYPE),
    NEW_LISTING("01", "\u65b0\u4e0a\u5e02", QuoteTagTypeEnum.IF_NEW_LISTING),
    NON_NEW_LISTING("10", "\u975e\u65b0\u4e0a\u5e02", QuoteTagTypeEnum.IF_NEW_LISTING),
    PLEDGED("01", "\u53ef\u8d28\u62bc", QuoteTagTypeEnum.IF_PLEDGED),
    NON_PLEDGED("10", "\u4e0d\u53ef\u8d28\u62bc", QuoteTagTypeEnum.IF_PLEDGED),
    CROSS_MARKET("01", "\u8de8\u5e02\u573a", QuoteTagTypeEnum.IF_CROSS_MARKET),
    NON_CROSS_MARKET("10", "\u5355\u5e02\u573a", QuoteTagTypeEnum.IF_CROSS_MARKET),
    CDC_IMPLITED_RATING_AAA_PLUS("00001", "AAA+", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_AAA("00010", "AAA", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_AAA_MINUS("00011", "AAA-", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_AA_PLUS("00100", "AA+", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_AA("00101", "AA", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_AA2("00110", "AA(2)", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_AA_MINUS("01000", "AA-", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_OTHER("01001", "\u5176\u4ed6", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_A_PLUS("01010", "A+", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_A("01011", "A", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_A_MINUS("01100", "A-", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_BBB_PLUS("01101", "BBB+", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_BBB("01110", "BBB", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_BBB_MINUS("01111", "BBB-", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_BB_PLUS("10000", "BB+", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_BB("10001", "BB", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_BB_MINUS("10010", "BB-", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_B("10011", "B", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_CCC("10100", "CCC", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_CC("10101", "CC", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    CDC_IMPLITED_RATING_C("10110", "C", QuoteTagTypeEnum.CDC_IMPLITED_RATING),
    RATING_CURRENT_AAA("00001", "AAA", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_A1("00010", "A-1", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_AA_PLUS("00011", "AA+", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_AA("00100", "AA", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_AA_MINUS("00101", "AA-", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_A_PLUS("00110", "A+", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_A("00111", "A", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_A_MINUS("01000", "A-", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_BBB_PLUS("01001", "BBB+", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_BBB("01010", "BBB", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_BBB_MINUS("01011", "BBB-", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_BB_PLUS("01100", "BB+", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_BB("01101", "BB", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_BB_MINUS("01110", "BB-", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_B_PLUS("01111", "B+", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_B("10000", "B", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_B_MINUS("10001", "B-", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_CCL("10010", "CCL", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_CCC("10011", "CCC", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_CC("10100", "CC", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_C("10101", "C", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_AAA_MINUS("10110", "AAA-", QuoteTagTypeEnum.RATING_CURRENT),
    RATING_CURRENT_AAA_PLUS("10111", "AAA+", QuoteTagTypeEnum.RATING_CURRENT),
    ISSUER_RATING_CURRENT_CBR_AAA_PLUS("00001", "AAA+", QuoteTagTypeEnum.ISSUER_RATING_CURRENT_CBR),
    ISSUER_RATING_CURRENT_CBR_AAA("00010", "AAA", QuoteTagTypeEnum.ISSUER_RATING_CURRENT_CBR),
    ISSUER_RATING_CURRENT_CBR_AAA_MINUS(
            "00011", "AAA-", QuoteTagTypeEnum.ISSUER_RATING_CURRENT_CBR),
    ISSUER_RATING_CURRENT_CBR_AA_PLUS("00100", "AA+", QuoteTagTypeEnum.ISSUER_RATING_CURRENT_CBR),
    ISSUER_RATING_CURRENT_CBR_AA("00101", "AA", QuoteTagTypeEnum.ISSUER_RATING_CURRENT_CBR),
    ISSUER_RATING_CURRENT_CBR_OTHER(
            "01000", "\u5176\u4ed6", QuoteTagTypeEnum.ISSUER_RATING_CURRENT_CBR),
    CSI_IMPLIED_RATING_AAA_PLUS("00001", "AAA+", QuoteTagTypeEnum.CSI_IMPLIED_RATING),
    CSI_IMPLIED_RATING_AAA("00010", "AAA", QuoteTagTypeEnum.CSI_IMPLIED_RATING),
    CSI_IMPLIED_RATING_AA_PLUS("00011", "AA+", QuoteTagTypeEnum.CSI_IMPLIED_RATING),
    CSI_IMPLIED_RATING_AA("00100", "AA", QuoteTagTypeEnum.CSI_IMPLIED_RATING),
    CSI_IMPLIED_RATING_AA2("00101", "AA2", QuoteTagTypeEnum.CSI_IMPLIED_RATING),
    CSI_IMPLIED_RATING_AA_MINUS("00110", "AA-", QuoteTagTypeEnum.CSI_IMPLIED_RATING),
    CSI_IMPLIED_RATING_OTHER("01000", "\u5176\u4ed6", QuoteTagTypeEnum.CSI_IMPLIED_RATING),
    NON_CONVERTIBLE("01", "\u4e0d\u53ef\u4ea4\u6362", QuoteTagTypeEnum.IF_CONVERTIBLE),
    CONVERTIBLE("10", "\u53ef\u4ea4\u6362", QuoteTagTypeEnum.IF_CONVERTIBLE),
    NCD_MCB("0001", "\u5927\u884c", QuoteTagTypeEnum.NCD_TYPE),
    NCD_SHB("0010", "\u80a1\u4efd\u5236", QuoteTagTypeEnum.NCD_TYPE),
    NCD_OTHERS("0011", "\u5176\u4ed6", QuoteTagTypeEnum.NCD_TYPE),
    FIN_INST_BANK("001", "\u94f6\u884c", QuoteTagTypeEnum.FIN_INST_BOND),
    FIN_INST_SECURITIES("010", "\u5238\u5546", QuoteTagTypeEnum.FIN_INST_BOND),
    FIN_INST_INSURANCE("011", "\u4fdd\u9669", QuoteTagTypeEnum.FIN_INST_BOND),
    FIN_INST_GN("100", "GN", QuoteTagTypeEnum.FIN_INST_BOND),
    ADMIN_LEVEL_PROVINCE("001", "\u7701\u7ea7", QuoteTagTypeEnum.ADMIN_LEVEL),
    ADMIN_LEVEL_CITY("010", "\u5e02\u7ea7", QuoteTagTypeEnum.ADMIN_LEVEL),
    ADMIN_LEVEL_COUNTY("011", "\u533a\u53bf\u7ea7", QuoteTagTypeEnum.ADMIN_LEVEL),
    ADMIN_LEVEL_OTHER("100", "\u5176\u4ed6", QuoteTagTypeEnum.ADMIN_LEVEL),
    IRS_TYPE_OUTRIGHT("0001", "Outright", QuoteTagTypeEnum.IRS_TYPE),
    IRS_TYPE_SPREAD("0010", "Spread", QuoteTagTypeEnum.IRS_TYPE),
    IRS_TYPE_BASIS("0011", "Basis", QuoteTagTypeEnum.IRS_TYPE),
    IRS_TERM_1M("0001", "1M", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_3M("0010", "3M", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_6M("0011", "6M", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_9M("0100", "9M", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_1Y("0101", "1Y", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_2Y("0110", "2Y", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_3Y("0111", "3Y", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_4Y("1000", "4Y", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_5Y("1001", "5Y", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_7Y("1010", "7Y", QuoteTagTypeEnum.IRS_TERM),
    IRS_TERM_10Y("1011", "10Y", QuoteTagTypeEnum.IRS_TERM),
    MATURITY_REPAY("01", "\u5230\u671f\u4e00\u6b21\u6027\u8fd8\u672c", QuoteTagTypeEnum.IF_REPAY),
    ADVANCE_REPAY("10", "\u63d0\u524d\u8fd8\u672c", QuoteTagTypeEnum.IF_REPAY),
    INNOVATION_KECHUANG("0001", "\u79d1\u521b\u503a", QuoteTagTypeEnum.INNOVATION_TYPE),
    INNOVATION_SHUANGCHUANG("0010", "\u53cc\u521b\u503a", QuoteTagTypeEnum.INNOVATION_TYPE),
    NON_MARKET_MAKING("01", "\u975e\u505a\u5e02\u503a", QuoteTagTypeEnum.IF_MARKET_MAKING),
    MARKET_MAKING_BOND("10", "\u505a\u5e02\u503a", QuoteTagTypeEnum.IF_MARKET_MAKING),
    NON_TIER2_PERP("01", "\u975e\u4e8c\u6c38", QuoteTagTypeEnum.IF_TIER2_PERP),
    TIER2_PERP("10", "\u4e8c\u6c38", QuoteTagTypeEnum.IF_TIER2_PERP),
    CONVERTIBLE_BOND("0001", "\u53ef\u8f6c\u503a", QuoteTagTypeEnum.BOND_SUB_TYPE2),
    EXCHANGEABLE_BOND("0010", "\u53ef\u4ea4\u6362\u503a", QuoteTagTypeEnum.BOND_SUB_TYPE2),
    NON_DEFAULT("01", "\u975e\u8fdd\u7ea6", QuoteTagTypeEnum.IF_DEFAULT),
    IS_DEFAULT("10", "\u8fdd\u7ea6", QuoteTagTypeEnum.IF_DEFAULT),
    GREEN_BOND("01", "\u7eff\u8272\u503a", QuoteTagTypeEnum.IF_GREEN_BOND),
    NON_GREEN_BOND("10", "\u975e\u7eff\u8272\u503a", QuoteTagTypeEnum.IF_GREEN_BOND),
    FREE_TAX("01", "\u514d\u7a0e", QuoteTagTypeEnum.IF_TAX_FREE),
    NON_FREE_TAX("10", "\u975e\u514d\u7a0e", QuoteTagTypeEnum.IF_TAX_FREE);

    private String code;
    private String name;
    private QuoteTagTypeEnum type;
    private static Map<String, Map<String, QuoteTagEnum>> enumMap;

    public static List<QuoteTagEnum> getAllTypeTag(String type) {
        ArrayList<QuoteTagEnum> list = new ArrayList<QuoteTagEnum>();
        Map<String, QuoteTagEnum> tagEnumMap = enumMap.get(type);
        if (tagEnumMap != null) {
            list.addAll(tagEnumMap.values());
        }
        return list;
    }

    public static QuoteTagEnum getEnumByName(String tagType, String tagName) {
        Map<String, QuoteTagEnum> tagEnumMap = enumMap.get(tagType);
        if (CollUtil.isNotEmpty(tagEnumMap)) {
            for (Map.Entry<String, QuoteTagEnum> entry : tagEnumMap.entrySet()) {
                if (!entry.getValue().getName().equals(tagName)) continue;
                return entry.getValue();
            }
        }
        return null;
    }

    public String getCode() {
        return this.code;
    }

    public String getName() {
        return this.name;
    }

    public QuoteTagTypeEnum getType() {
        return this.type;
    }

    @ConstructorProperties(value = {"code", "name", "type"})
    private QuoteTagEnum(String code, String name, QuoteTagTypeEnum type) {
        this.code = code;
        this.name = name;
        this.type = type;
    }

    static {
        enumMap = new HashMap<String, Map<String, QuoteTagEnum>>();
        for (QuoteTagEnum quoteTagEnum : QuoteTagEnum.values()) {
            Map<String, QuoteTagEnum> tagEnumMap = enumMap.get(quoteTagEnum.getType().getCode());
            if (tagEnumMap == null) {
                tagEnumMap = new HashMap<String, QuoteTagEnum>();
            }
            tagEnumMap.put(quoteTagEnum.getCode(), quoteTagEnum);
            enumMap.put(quoteTagEnum.getType().getCode(), tagEnumMap);
        }
    }
}
