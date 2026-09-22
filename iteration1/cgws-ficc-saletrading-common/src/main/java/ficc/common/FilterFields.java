package ficc.common;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import java.util.Arrays;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.Map;
import java.util.Set;

/** Shared filter contract for requests, subscriptions and client editors. */
public final class FilterFields {
    private FilterFields() {}

    public static final Map<String, JSONObject> DEFINITIONS = new LinkedHashMap<>();
    public static final Set<String> NUMBERS =
            Collections.unmodifiableSet(
                    new LinkedHashSet<>(
                            Arrays.asList(
                                    "price",
                                    "quantity",
                                    "remainingYears",
                                    "couponRate",
                                    "valuationYield",
                                    "csiValuationYield",
                                    "issueYear",
                                    "valuationCleanPrice",
                                    "bidCleanPriceSpread",
                                    "offerCleanPriceSpread",
                                    "bidOfferSpread",
                                    "outstandingAmount",
                                    "bidPrice",
                                    "offerPrice",
                                    "bidQuantity",
                                    "offerQuantity",
                                    "bidValuationSpread",
                                    "offerValuationSpread")));
    public static final Set<String> BOOLEANS =
            Collections.unmodifiableSet(
                    new LinkedHashSet<>(
                            Arrays.asList(
                                    "urgent",
                                    "recommended",
                                    "whole",
                                    "oco",
                                    "packing",
                                    "hasExerciseOption",
                                    "isMunicipal",
                                    "isCrossMarket",
                                    "isMortgage",
                                    "isPerpetual",
                                    "isSubordinated",
                                    "isTaxExempt",
                                    "isHighYield",
                                    "isPreListing")));

    static {
        add("报价", "BOOK", "bidOfferSpread:Bid-Ofr");
        add(
                "债券",
                "SECURITY",
                "isHighYield:高收益债,isPreListing:一级半,outstandingAmount:债券余额（亿）,regionCategory:地区分类,localGovernmentRegion:地方债地区");
        add("报价", "ENTRY", "searchText:综合搜索");
        add("报价", "ENTRY", "bidCleanPriceSpread:Bid净价-中债净价,offerCleanPriceSpread:中债净价-Ofr净价");
        add("债券", "SECURITY", "valuationCleanPrice:中债净价估值,isTaxExempt:是否免税");
        add("条款", "SECURITY", "couponFrequency:付息方式");
        add(
                "报价",
                "ENTRY",
                "securityId:债券代码,securityName:债券名称,side:买卖方向,quoteType:计价方式,yieldType:收益率类型,price:报价价格,quantity:报价数量,settlType:交割方式,urgent:急,recommended:荐,whole:整,oco:OCO,packing:打包,bidPrice:Bid,offerPrice:Ofr,bidQuantity:Vol.Bid,offerQuantity:Vol.Ofr,bidValuationSpread:Bid-中债,offerValuationSpread:中债-Ofr");
        add(
                "债券",
                "SECURITY",
                "bondSubtype:细分券种,productType:产品分类,hotTags:热门标签,ncdType:NCD类型,remainingYears:剩余期限,issuerName:发行人,listingStatus:债券上市状态,issueMethod:发行方式,valuationYield:中债估值,csiValuationYield:中证估值,couponRate:票面利率");
        add("流动性", "SECURITY", "liquidityGrade:债券流动性,marketMakerTags:做市标的");
        add(
                "信用",
                "SECURITY",
                "listedMarket:交易场所,issuerId:发行人代码,issuerType:发行人类型,bankType:银行业,industry:行业,ownershipType:企业性质,guaranteeType:担保,guarantorName:担保人,issuerRating:主体评级,issuerCbrRating:中债资信,ratingOutlook:评级展望,bondRating:债项评级,bondCbrRating:中债隐含,yyRating:YY评分,csiRating:中证隐含");
        add(
                "条款",
                "SECURITY",
                "optionType:含权类型,hasExerciseOption:是否含权,repaymentType:偿还方式,couponType:计息方式,perpetualType:永续债类型,isPerpetual:是否永续,isSubordinated:次级债,rateIndex:浮息方式,issueYear:发行年份");
        add(
                "区域",
                "SECURITY",
                "isMunicipal:城投,region:省份,city:城市,regionCode:省份代码,keyProvince:重点省份,municipalLevel:城投层级,regionalRating:区域评级,localGovernmentType:地方债类型,localGovernmentUse:地方债用途,isCrossMarket:跨市场,isMortgage:可质押");
    }

    private static void add(String group, String scope, String fields) {
        for (String item : fields.split(",")) {
            String[] pair = item.split(":");
            String field = pair[0];
            String type =
                    NUMBERS.contains(field)
                            ? "decimal"
                            : BOOLEANS.contains(field) ? "boolean" : "string";
            JSONArray operators =
                    NUMBERS.contains(field)
                            ? JsonUtils.array(
                                    "EQ", "IN", "GT", "GTE", "LT", "LTE", "BETWEEN", "EXISTS")
                            : BOOLEANS.contains(field)
                                    ? JsonUtils.array("EQ", "EXISTS")
                                    : JsonUtils.array("EQ", "IN", "CONTAINS", "EXISTS");
            DEFINITIONS.put(
                    field,
                    JsonUtils.object(
                            "field",
                            field,
                            "label",
                            pair[1],
                            "group",
                            group,
                            "type",
                            type,
                            "scope",
                            scope,
                            "operators",
                            operators,
                            "enabled",
                            true));
        }
    }

    public static JSONArray catalog() {
        JSONArray result = new JSONArray();
        for (JSONObject field : DEFINITIONS.values()) {
            JSONObject definition = JsonUtils.deepCopy(field);
            String name = definition.getString("field");
            if (FilterDictionaries.contains(name)) {
                definition.put("enumValues", FilterDictionaries.options(name));
                definition.put("operators", JsonUtils.array("EQ", "IN", "EXISTS"));
            }
            result.add(definition);
        }
        return result;
    }
}
