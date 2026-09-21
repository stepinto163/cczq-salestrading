package ficc.common;

import com.alibaba.fastjson2.JSONObject;
import java.util.Map;

/** Deterministic prototype-only values, individually marked for later vendor replacement. */
public final class FilterSampleData {
    private FilterSampleData() {}

    public static void fillMissing(JSONObject security) {
        int variant = Math.floorMod(security.getString("securityId").hashCode(), 3);
        boolean credit = "CREDIT".equals(security.getString("market"));
        JSONObject sources = security.getJSONObject("filterDataSources");
        if (sources == null) sources = new JSONObject();
        JSONObject samples = JsonUtils.object(
                "productType", credit ? "信用债" : "利率债", "bondSubtype", credit ? "中票" : "国债",
                "hotTags", JsonUtils.array(variant == 0 ? "绿色债" : "城投"), "ncdType", "股份",
                "issuerName", security.getString("securityName") + "发行人", "listingStatus", "已上市",
                "issueMethod", "公募", "csiValuationYield", "2.50", "couponRate", "3.00",
                "couponFrequency", "A", "isTaxExempt", !credit, "valuationCleanPrice", "100.00",
                "bidOfferSpread", "5.00", "outstandingAmount", "20.00", "isHighYield", variant == 1,
                "isPreListing", false, "regionCategory", new String[]{"GOOD", "AVERAGE", "WEAK"}[variant],
                "localGovernmentRegion", security.getString("region"),
                "liquidityGrade", new String[]{"S", "A", "B"}[variant],
                "marketMakerTags", JsonUtils.array(credit ? "上证信用" : "上证利率"),
                "listedMarket", "银行间", "bankType", "股份行", "industry", credit ? "城投" : "金融",
                "ownershipType", "国企", "guaranteeType", "无担保", "guarantorName", "模拟担保机构",
                "issuerCbrRating", "AAA", "ratingOutlook", "稳定", "bondCbrRating", "AA+",
                "yyRating", "投资级", "csiRating", "AAA", "optionType", security.getBooleanValue("hasExerciseOption") ? "PUT" : "NON", "repaymentType", "到期",
                "couponType", "FIX", "perpetualType", "非永续", "isPerpetual", false,
                "isSubordinated", variant == 1, "rateIndex", "Regular", "issueYear", "2026",
                "isMunicipal", credit, "city", "310100", "regionCode", security.getString("region"),
                "keyProvince", "非12重点省", "municipalLevel", "市级", "regionalRating", "模拟A级",
                "localGovernmentType", "一般债", "localGovernmentUse", "基础设施",
                "isCrossMarket", variant == 1, "isMortgage", true, "valuationBasis", "MATURITY");
        for (Map.Entry<String, Object> entry : samples.entrySet()) {
            if (security.get(entry.getKey()) == null) {
                security.put(entry.getKey(), entry.getValue());
                sources.put(entry.getKey(), "SIMULATOR");
            }
        }
        security.put("filterDataSources", sources);
        FilterDictionaries.normalizeAttributes(security);
    }
}
