package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import java.util.Map;

/** Product labels derived from the documented STC bond codes; source values are retained. */
public final class BondFilterAttributes {
    private BondFilterAttributes() {}

    public static void normalize(JSONObject security) {
        String subtype = security.getString("bondSubtype");
        if (subtype != null) {
            security.put("sourceBondSubtype", subtype);
            Map<String, String> groups = Map.ofEntries(
                    Map.entry("国债", "BGB,SGB,EGB"), Map.entry("央票", "SCB"),
                    Map.entry("地方债", "LLB,TLB"), Map.entry("短融", "CCP,LCP,CSP,LSP,SSB"),
                    Map.entry("中票", "CMN,LMN,COB"), Map.entry("企业债", "CEB,LEB,CCB,RAB"),
                    Map.entry("公司债", "DCO,SEB"), Map.entry("金融债", "CBB,CSB,CXB,NCB,SFB,PSB,TET,SES"),
                    Map.entry("ABS", "AMP,CBS,LBS,MBS"), Map.entry("可转债", "CVB,SCV,WBS"),
                    Map.entry("NCD", "MCD,SPD,SHD,CCD,RRD,RTD,FRD,OTD"));
            for (Map.Entry<String, String> group : groups.entrySet()) {
                if (java.util.List.of(group.getValue().split(",")).contains(subtype)) {
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
            for (String prefix : new String[]{"SHIBOR", "DEPO", "LPR"}) {
                if (index.toUpperCase(java.util.Locale.ROOT).startsWith(prefix)) {
                    security.put("rateIndex", prefix);
                }
            }
        }
    }
}
