package ficc.common;

import com.alibaba.fastjson2.*;
import java.math.BigDecimal;
import java.util.*;

/** Shared condition implementation; Quote and Price endpoints still have distinct contracts. */
public final class FilterEngine {
    public static final Set<String> FIELDS=Set.of("securityId","securityName","issuerId","issuerType","bondRating","issuerRating","region","remainingYears","side","quoteType","yieldType","price","quantity","settlType","urgent","recommended","whole","oco","packing");
    public static final Set<String> NUMBERS=Set.of("price","quantity","remainingYears");
    public static final Set<String> OPS=Set.of("EQ","IN","GT","GTE","LT","LTE","BETWEEN","EXISTS");
    public static void validate(JSONObject f){
        if(f==null)return;
        RequestValidator.allowOnlyFields(f,"market","conditions","watchlistIds","filterGroupIds");
        if(f.containsKey("market")&&!Set.of("RATE","CREDIT").contains(f.getString("market")))throw ApiException.forField("1001","VALIDATION_FAILED","Invalid market","market");
        for(JSONObject c:conditions(f)){
            RequestValidator.allowOnlyFields(c,"field","operator","value","conditionId","displayOrder");
            String field=RequestValidator.requireString(c,"field"),op=RequestValidator.requireString(c,"operator");
            if(!FIELDS.contains(field)||!OPS.contains(op))throw ApiException.forField("1001","VALIDATION_FAILED","Unknown filter field/operator",field);
            Object value=c.get("value");
            if(!NUMBERS.contains(field)&&!Set.of("EQ","IN","EXISTS").contains(op))throw ApiException.forField("1001","VALIDATION_FAILED","Operator is not supported for this field",field);
            if(op.equals("EXISTS")&&!(value instanceof Boolean))throw ApiException.forField("1001","VALIDATION_FAILED","EXISTS requires boolean value",field);
            if((op.equals("IN")||op.equals("BETWEEN"))&&!(value instanceof List<?>))throw ApiException.forField("1001","VALIDATION_FAILED","Array value required",field);
            if(op.equals("BETWEEN")&&((List<?>)value).size()!=2)throw ApiException.forField("1001","VALIDATION_FAILED","BETWEEN requires two bounds",field);
            if(NUMBERS.contains(field)&&!op.equals("EXISTS"))try{if(value instanceof List<?> l){for(Object v:l)new BigDecimal(v.toString());}else new BigDecimal(value.toString());}catch(Exception e){throw ApiException.forField("1001","VALIDATION_FAILED","Invalid decimal condition",field);}
        }
    }
    public static List<JSONObject> conditions(JSONObject f){JSONArray a=f==null?null:f.getJSONArray("conditions");return a==null?List.of():a.toJavaList(JSONObject.class);}
    public static boolean match(JSONObject row,JSONObject f){
        if(f==null)return true;
        if(f.getString("market")!=null&&!Objects.equals(f.getString("market"),row.getString("market")))return false;
        Map<String,Boolean> groups=new HashMap<>();
        for(JSONObject c:conditions(f))groups.merge(c.getString("field"),test(row,c),Boolean::logicalOr);
        return groups.values().stream().allMatch(Boolean::booleanValue);
    }
    public static boolean test(JSONObject row,JSONObject c){
        String field=c.getString("field"),op=c.getString("operator");Object got=row.get(field),v=c.get("value");
        if(op.equals("EXISTS"))return (got!=null)==Boolean.parseBoolean(String.valueOf(v));
        if(got==null)return op.equals("EQ")&&v==null;
        if(op.equals("IN"))return ((List<?>)v).stream().anyMatch(x->compare(field,got,x)==0);
        if(op.equals("BETWEEN")){List<?> l=(List<?>)v;return compare(field,got,l.get(0))>=0&&compare(field,got,l.get(1))<=0;}
        int cmp=compare(field,got,v);return switch(op){case "EQ"->cmp==0;case "GT"->cmp>0;case "GTE"->cmp>=0;case "LT"->cmp<0;case "LTE"->cmp<=0;default->false;};
    }
    public static int compare(String field,Object a,Object b){if(b==null)return 1;return NUMBERS.contains(field)?new BigDecimal(a.toString()).compareTo(new BigDecimal(b.toString())):a.toString().compareTo(b.toString());}
    public static JSONArray catalog(boolean price){JSONArray a=new JSONArray();for(String f:new TreeSet<>(FIELDS))a.add(JsonUtils.object("field",f,"type",NUMBERS.contains(f)?"decimal":"string","operators",NUMBERS.contains(f)?JsonUtils.array("EQ","IN","GT","GTE","LT","LTE","BETWEEN","EXISTS"):JsonUtils.array("EQ","IN","EXISTS"),"scope",Set.of("securityId","securityName","issuerId","issuerType","bondRating","issuerRating","region","remainingYears").contains(f)?"SECURITY":"ENTRY","enabled",true));return a;}
}
