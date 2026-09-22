package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;

import java.util.Arrays;
import java.util.LinkedHashMap;
import java.util.Map;

/** Product labels derived from the documented STC bond codes; source values are retained. */
public final class BondFilterAttributes {
    private BondFilterAttributes() {}

    public static void normalize(JSONObject security) {
        String subtype = security.getString("bondSubtype");
        if (subtype != null) {
            security.put("sourceBondSubtype", subtype);
            Map<String, String> groups = BondFilterAttributes.subtypeGroups();
            for (Map.Entry<String, String> group : groups.entrySet()) {
                if (Arrays.asList(group.getValue().split(",")).contains(subtype)) {
                    security.put("bondSubtype", group.getKey());
                    break;
                }
            }
            if ("PBB".equals(subtype)) {
                String issuer = security.getString("issuerName");
                if (issuer != null) {
                    if (issuer.contains("国家开发")) security.put("bondSubtype", "国开");
                    else if (issuer.contains("农业发展")) security.put("bondSubtype", "农发");
                    else if (issuer.contains("进出口")) security.put("bondSubtype", "口行");
                }
            }
        }
        String couponType = security.getString("couponType");
        String index = security.getString("rateIndex");
        if ("FRB".equals(couponType)) {
            security.put("rateIndex", "固息");
        } else if (index != null) {
            security.put("sourceRateIndex", index);
            for (String prefix : new String[] {"SHIBOR", "DEPO", "LPR"}) {
                if (index.toUpperCase(java.util.Locale.ROOT).startsWith(prefix)) {
                    security.put("rateIndex", prefix);
                }
            }
        }
    }

    private static Map<String, String> subtypeGroups() {
        Map<String, String> groups = new LinkedHashMap<>();
        groups.put("国债", "BGB,SGB,EGB");
        groups.put("央票", "SCB");
        groups.put("地方债", "LLB,TLB");
        groups.put("短融", "CCP,LCP,CSP,LSP,SSB");
        groups.put("中票", "CMN,LMN,COB");
        groups.put("企业债", "CEB,LEB,CCB,RAB");
        groups.put("公司债", "DCO,SEB");
        groups.put("金融债", "CBB,CSB,CXB,NCB,SFB,PSB,TET,SES");
        groups.put("ABS", "AMP,CBS,LBS,MBS");
        groups.put("可转债", "CVB,SCV,WBS");
        groups.put("NCD", "MCD,SPD,SHD,CCD,RRD,RTD,FRD,OTD");
        return groups;
    }
}
