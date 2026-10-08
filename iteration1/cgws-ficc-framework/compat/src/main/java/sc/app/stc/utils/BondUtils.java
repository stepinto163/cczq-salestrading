// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.date.DateTime;
import cn.hutool.core.date.DateUtil;
import cn.hutool.core.lang.Pair;
import cn.hutool.core.text.CharSequenceUtil;
import cn.hutool.core.util.NumberUtil;
import cn.hutool.core.util.StrUtil;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import sc.app.stc.api.base.po.BondInfoView;
import sc.app.stc.enums.SymbolTypeEnum;
import sc.app.stc.tag.enums.QuoteTagEnum;
import sc.app.stc.tag.enums.QuoteTagTypeEnum;

import java.math.BigDecimal;
import java.util.Date;
import java.util.Optional;
import java.util.Set;

public class BondUtils {
    private static final Logger logger = LoggerFactory.getLogger(BondUtils.class);
    private static final Set<String> bondTypeRateSet =
            CollUtil.newHashSet(new String[] {"BGB", "EGB", "SGB", "SCB", "PBB", "TLB", "LLB"});
    private static final Set<String> bondTypeCreditSet =
            CollUtil.newHashSet(
                    new String[] {
                        "CXB", "SPS", "CSP", "SCV", "CRM", "CCP", "LEB", "OTB", "LSP", "SEB", "COB",
                        "SES", "PNN", "LCP", "CCB", "INT", "PSB", "SSB", "PPN", "PDB", "NCB", "CMN",
                        "CSB", "TET", "HJB", "LMN", "RAB", "CVB", "CBB", "CEB", "TLC"
                    });
    private static final Set<String> bondTypeAbsSet =
            CollUtil.newHashSet(new String[] {"LBS", "MBS", "AMP", "CBS"});
    private static final Set<String> bondTypeNcdSet =
            CollUtil.newHashSet(
                    new String[] {"MCD", "SHD", "CCD", "FRD", "RRD", "RTD", "OTD", "SPD"});
    private static final Set<String> equityLinkedBondSet =
            CollUtil.newHashSet(new String[] {"PUT", "CNP", "CAL", "ETS", "ASS", "DCN", "CNV"});

    public static String getSymbolType(BondInfoView bondInfo) {
        if (bondInfo == null) {
            return null;
        }
        String bond_subtype = bondInfo.getBond_Subtype();
        String marketIndicator = bondInfo.getMarketIndicator();
        if ("99999.IB".equals(bondInfo.getBond_ID())) {
            return "VIRTUAL";
        }
        if (bondTypeRateSet.contains(bond_subtype)) {
            if ("4".equals(marketIndicator)) {
                return "BNC";
            }
            if ("BGB".equals(bond_subtype) && "2".equals(marketIndicator)) {
                return "IRS";
            }
        } else {
            if (bondTypeCreditSet.contains(bond_subtype)) {
                return "BCO";
            }
            if (bondTypeAbsSet.contains(bond_subtype)) {
                return "ABS";
            }
            if (bondTypeNcdSet.contains(bond_subtype)) {
                return "NCD";
            }
        }
        return null;
    }

    @Deprecated
    public static String getBondType(BondInfoView bondInfo) {
        String bond_subtype = bondInfo.getBond_Subtype();
        String marketIndicator = bondInfo.getMarketIndicator();
        if ("99999.IB".equals(bondInfo.getBond_ID())) {
            return "VIRTUAL";
        }
        if (bondTypeRateSet.contains(bond_subtype)) {
            if ("4".equals(marketIndicator)) {
                return "RATE";
            }
            if ("BGB".equals(bond_subtype) && "2".equals(marketIndicator)) {
                return "IRS";
            }
        } else {
            if (bondTypeCreditSet.contains(bond_subtype)) {
                return "CREDIT";
            }
            if (bondTypeAbsSet.contains(bond_subtype)) {
                return "ABS";
            }
            if (bondTypeNcdSet.contains(bond_subtype)) {
                return "NCD";
            }
        }
        return null;
    }

    public static Integer getTermDays(String timeToMaturity) {
        Integer dayCount = 0;
        if (StrUtil.isBlank((CharSequence) timeToMaturity)) {
            return dayCount;
        }
        String s =
                StrUtil.trim(
                        (CharSequence)
                                StrUtil.splitToArray(
                                        (CharSequence) timeToMaturity, (CharSequence) "+")[0]);
        if (s.endsWith("D")) {
            dayCount =
                    dayCount
                            + new BigDecimal(
                                            StrUtil.subBefore(
                                                    (CharSequence) s,
                                                    (CharSequence) "D",
                                                    (boolean) true))
                                    .intValue();
        } else if (s.endsWith("Y")) {
            dayCount =
                    dayCount
                            + NumberUtil.round(
                                            (double)
                                                    NumberUtil.mul(
                                                                    (Number)
                                                                            new BigDecimal(
                                                                                    StrUtil
                                                                                            .subBefore(
                                                                                                    (CharSequence)
                                                                                                            s,
                                                                                                    (CharSequence)
                                                                                                            "Y",
                                                                                                    (boolean)
                                                                                                            true)),
                                                                    (Number) 365)
                                                            .doubleValue(),
                                            (int) 0)
                                    .intValue();
        } else if (s.endsWith("M")) {
            dayCount =
                    dayCount
                            + NumberUtil.round(
                                            (double)
                                                    NumberUtil.mul(
                                                                    (Number)
                                                                            new BigDecimal(
                                                                                    StrUtil
                                                                                            .subBefore(
                                                                                                    (CharSequence)
                                                                                                            s,
                                                                                                    (CharSequence)
                                                                                                            "M",
                                                                                                    (boolean)
                                                                                                            true)),
                                                                    (Number) 30)
                                                            .doubleValue(),
                                            (int) 0)
                                    .intValue();
        }
        return dayCount;
    }

    public static String getAlertMsgName(BondInfoView bondInfo) {
        if (bondInfo == null) {
            return null;
        }
        String bondName = bondInfo.getBond_ID();
        String bondType = BondUtils.getBondType(bondInfo);
        String bondSubtype = bondInfo.getBond_Subtype();
        if ("RATE".equals(bondType)) {
            if (CharSequenceUtil.equalsAny(
                    (CharSequence) bondSubtype,
                    (CharSequence[]) new CharSequence[] {"TLB", "LLB"})) {
                bondName =
                        Optional.ofNullable(bondInfo.getShort_Name()).orElse(bondInfo.getBond_ID());
            }
        } else {
            bondName = Optional.ofNullable(bondInfo.getShort_Name()).orElse(bondInfo.getBond_ID());
        }
        return bondName;
    }

    public static boolean ifCumRights(BondInfoView bondInfo) {
        if (bondInfo == null || StrUtil.isBlank((CharSequence) bondInfo.getOption_Type())) {
            return false;
        }
        return equityLinkedBondSet.contains(bondInfo.getOption_Type());
    }

    public static Boolean ifExercised(BigDecimal optionDate) {
        if (optionDate == null) {
            return true;
        }
        try {
            DateTime dateTime =
                    DateUtil.parse((CharSequence) optionDate.toPlainString(), (String) "yyyyMMdd");
            if (dateTime.isAfterOrEquals((Date) DateUtil.beginOfDay((Date) new Date()))) {
                return false;
            }
            return true;
        } catch (Exception e) {
            logger.error("check ifExercised failed optionDate:{}", (Object) optionDate, (Object) e);
            return false;
        }
    }

    public static String getSymbolTypeKey(BondInfoView bondInfo) {
        SymbolTypeEnum typeEnum = SymbolTypeEnum.getByValue(BondUtils.getSymbolType(bondInfo));
        if (typeEnum != null) {
            return typeEnum.getKey();
        }
        return null;
    }

    public static Pair<String, String> getBondSubTypeNew(BondInfoView bondInfo) {
        String bond_subtype = bondInfo.getBond_Subtype();
        String bondInfoType = bondInfo.getBondtype();
        String ent_cor = bondInfo.getEnt_Cor();
        String bondType = BondUtils.getBondType(bondInfo);
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"LSP", "CSP"})) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.CSP_BOND.getCode());
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype,
                (CharSequence[]) new CharSequence[] {"CCP", "LCP", "SSB"})) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.LCP_BOND.getCode());
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"CMN", "LMN"})) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.CMN_BOND.getCode());
        }
        if (CharSequenceUtil.equalsAny(
                        (CharSequence) bond_subtype,
                        (CharSequence[]) new CharSequence[] {"CCB", "COB", "RAB"})
                || CharSequenceUtil.equalsAny(
                                (CharSequence) bond_subtype,
                                (CharSequence[]) new CharSequence[] {"CEB", "LEB"})
                        && CharSequenceUtil.equals((CharSequence) ent_cor, (CharSequence) "ENT")) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.ENT_BOND.getCode());
        }
        if (CharSequenceUtil.equalsAny(
                        (CharSequence) bondInfoType, (CharSequence[]) new CharSequence[] {"COR"})
                || CharSequenceUtil.equalsAny(
                                (CharSequence) bond_subtype,
                                (CharSequence[]) new CharSequence[] {"PPN", "CEB", "LEB"})
                        && CharSequenceUtil.equals((CharSequence) ent_cor, (CharSequence) "COP")) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.COP_BOND.getCode());
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype,
                (CharSequence[]) new CharSequence[] {"CSB", "SES", "SPS", "NCB", "PSB", "TLC"})) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.SUB_BOND.getCode());
        }
        if (CharSequenceUtil.equals((CharSequence) bond_subtype, (CharSequence) "PPN")
                && StrUtil.isBlank((CharSequence) ent_cor)) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.PPN_BOND.getCode());
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype,
                (CharSequence[]) new CharSequence[] {"BGB", "EGB", "SGB"})) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.NATIONAL_DEBT.getCode());
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"SCB"})) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.CENTRAL_BANK_TICKET.getCode());
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"PBB"})) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.FINANCIAL_DEBT.getCode());
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"TLB", "LLB"})) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.LOCAL_DEBT.getCode());
        }
        if (CharSequenceUtil.equals((CharSequence) bondType, (CharSequence) "NCD")) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.NCD_BOND.getCode());
        }
        if (CharSequenceUtil.equals((CharSequence) bondType, (CharSequence) "ABS")) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.ABS_BOND.getCode());
        }
        if (CharSequenceUtil.equals((CharSequence) bond_subtype, (CharSequence) "CBB")) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                    (Object) QuoteTagEnum.COMMERCIAL_BANK_DEBT.getCode());
        }
        if (CharSequenceUtil.equals((CharSequence) bond_subtype, (CharSequence) "CVB")) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE2.getCode(),
                    (Object) QuoteTagEnum.CONVERTIBLE_BOND.getCode());
        }
        if (CharSequenceUtil.equals((CharSequence) bond_subtype, (CharSequence) "SCV")) {
            return new Pair(
                    (Object) QuoteTagTypeEnum.BOND_SUB_TYPE2.getCode(),
                    (Object) QuoteTagEnum.EXCHANGEABLE_BOND.getCode());
        }
        return new Pair(
                (Object) QuoteTagTypeEnum.BOND_SUB_TYPE.getCode(),
                (Object) QuoteTagEnum.OTHER_BOND.getCode());
    }

    public static String getBondSubType(BondInfoView bondInfo) {
        String bond_subtype = bondInfo.getBond_Subtype();
        String bondInfoType = bondInfo.getBondtype();
        String ent_cor = bondInfo.getEnt_Cor();
        String bondType = BondUtils.getBondType(bondInfo);
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"LSP", "CSP"})) {
            return QuoteTagEnum.CSP_BOND.getCode();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype,
                (CharSequence[]) new CharSequence[] {"CCP", "LCP", "SSB"})) {
            return QuoteTagEnum.LCP_BOND.getCode();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"CMN", "LMN"})) {
            return QuoteTagEnum.CMN_BOND.getCode();
        }
        if (CharSequenceUtil.equalsAny(
                        (CharSequence) bond_subtype,
                        (CharSequence[]) new CharSequence[] {"CCB", "COB", "RAB"})
                || CharSequenceUtil.equalsAny(
                                (CharSequence) bond_subtype,
                                (CharSequence[]) new CharSequence[] {"CEB", "LEB"})
                        && CharSequenceUtil.equals((CharSequence) ent_cor, (CharSequence) "ENT")) {
            return QuoteTagEnum.ENT_BOND.getCode();
        }
        if (CharSequenceUtil.equalsAny(
                        (CharSequence) bondInfoType, (CharSequence[]) new CharSequence[] {"COR"})
                || CharSequenceUtil.equalsAny(
                                (CharSequence) bond_subtype,
                                (CharSequence[]) new CharSequence[] {"PPN", "CEB", "LEB"})
                        && CharSequenceUtil.equals((CharSequence) ent_cor, (CharSequence) "COP")) {
            return QuoteTagEnum.COP_BOND.getCode();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype,
                (CharSequence[]) new CharSequence[] {"CSB", "SES", "SPS", "NCB", "PSB", "TLC"})) {
            return QuoteTagEnum.SUB_BOND.getCode();
        }
        if (CharSequenceUtil.equals((CharSequence) bond_subtype, (CharSequence) "PPN")
                && StrUtil.isBlank((CharSequence) ent_cor)) {
            return QuoteTagEnum.PPN_BOND.getCode();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype,
                (CharSequence[]) new CharSequence[] {"BGB", "EGB", "SGB"})) {
            return QuoteTagEnum.NATIONAL_DEBT.getCode();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"SCB"})) {
            return QuoteTagEnum.CENTRAL_BANK_TICKET.getCode();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"PBB"})) {
            return QuoteTagEnum.FINANCIAL_DEBT.getCode();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"TLB", "LLB"})) {
            return QuoteTagEnum.LOCAL_DEBT.getCode();
        }
        if (CharSequenceUtil.equals((CharSequence) bondType, (CharSequence) "NCD")) {
            return QuoteTagEnum.NCD_BOND.getCode();
        }
        if (CharSequenceUtil.equals((CharSequence) bondType, (CharSequence) "ABS")) {
            return QuoteTagEnum.ABS_BOND.getCode();
        }
        if (CharSequenceUtil.equals((CharSequence) bond_subtype, (CharSequence) "CBB")) {
            return QuoteTagEnum.COMMERCIAL_BANK_DEBT.getCode();
        }
        return QuoteTagEnum.OTHER_BOND.getCode();
    }

    public static String getLocalBondType(BondInfoView bondInfo) {
        String bond_subtype = bondInfo.getBond_Subtype();
        String ent_cor = bondInfo.getEnt_Cor();
        if (StrUtil.isBlank((CharSequence) bond_subtype)
                || StrUtil.isBlank((CharSequence) ent_cor)) {
            return null;
        }
        if ("LLB".equals(bond_subtype)) {
            if ("SMB".equals(ent_cor)) {
                return QuoteTagEnum.GENERAL_BOND.getCode();
            }
            if ("NMB".equals(ent_cor)) {
                return QuoteTagEnum.SPECIAL_BOND.getCode();
            }
        }
        return null;
    }

    public static String getBondSubTypeName(BondInfoView bondInfo) {
        String bond_subtype = bondInfo.getBond_Subtype();
        String bondInfoType = bondInfo.getBondtype();
        String ent_cor = bondInfo.getEnt_Cor();
        String bondType = BondUtils.getBondType(bondInfo);
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"LSP", "CSP"})) {
            return QuoteTagEnum.CSP_BOND.getName();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype,
                (CharSequence[]) new CharSequence[] {"CCP", "LCP", "SSB"})) {
            return QuoteTagEnum.LCP_BOND.getName();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"CMN", "LMN"})) {
            return QuoteTagEnum.CMN_BOND.getName();
        }
        if (CharSequenceUtil.equalsAny(
                        (CharSequence) bond_subtype,
                        (CharSequence[]) new CharSequence[] {"CCB", "COB", "RAB"})
                || CharSequenceUtil.equalsAny(
                                (CharSequence) bond_subtype,
                                (CharSequence[]) new CharSequence[] {"CEB", "LEB"})
                        && CharSequenceUtil.equals((CharSequence) ent_cor, (CharSequence) "ENT")) {
            return QuoteTagEnum.ENT_BOND.getName();
        }
        if (CharSequenceUtil.equalsAny(
                        (CharSequence) bondInfoType, (CharSequence[]) new CharSequence[] {"COR"})
                || CharSequenceUtil.equalsAny(
                                (CharSequence) bond_subtype,
                                (CharSequence[]) new CharSequence[] {"PPN", "CEB", "LEB"})
                        && CharSequenceUtil.equals((CharSequence) ent_cor, (CharSequence) "COP")) {
            return QuoteTagEnum.COP_BOND.getName();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype,
                (CharSequence[]) new CharSequence[] {"CSB", "SES", "SPS", "NCB", "PSB", "TLC"})) {
            return QuoteTagEnum.SUB_BOND.getName();
        }
        if (CharSequenceUtil.equals((CharSequence) bond_subtype, (CharSequence) "PPN")
                && StrUtil.isBlank((CharSequence) ent_cor)) {
            return QuoteTagEnum.PPN_BOND.getName();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype,
                (CharSequence[]) new CharSequence[] {"BGB", "EGB", "SGB"})) {
            return QuoteTagEnum.NATIONAL_DEBT.getName();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"SCB"})) {
            return QuoteTagEnum.CENTRAL_BANK_TICKET.getName();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"PBB"})) {
            return QuoteTagEnum.FINANCIAL_DEBT.getName();
        }
        if (CharSequenceUtil.equalsAny(
                (CharSequence) bond_subtype, (CharSequence[]) new CharSequence[] {"TLB", "LLB"})) {
            return QuoteTagEnum.LOCAL_DEBT.getName();
        }
        if (CharSequenceUtil.equals((CharSequence) bondType, (CharSequence) "NCD")) {
            return QuoteTagEnum.NCD_BOND.getName();
        }
        if (CharSequenceUtil.equals((CharSequence) bondType, (CharSequence) "ABS")) {
            return QuoteTagEnum.ABS_BOND.getName();
        }
        if (CharSequenceUtil.equals((CharSequence) bond_subtype, (CharSequence) "CBB")) {
            return QuoteTagEnum.COMMERCIAL_BANK_DEBT.getName();
        }
        return QuoteTagEnum.OTHER_BOND.getName();
    }
}
