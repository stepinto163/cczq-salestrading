package ficc.common;

import com.alibaba.fastjson2.*;
import java.util.*;

/** Explicit LoginSvr/BaseSvr simulators. Fixed examples, never vendor production data. */
public final class ReferenceData {
    public final Map<String,JSONObject> securities=new LinkedHashMap<>();
    public ReferenceData(){
        add("250001.IB","示例国债","RATE","GOVERNMENT","AAA","北京",false);
        add("250002.IB","示例政策银行债","RATE","POLICY_BANK","AAA","北京",false);
        add("102500001.IB","示例企业债","CREDIT","CORPORATE","AA+","上海",false);
        add("102500002.IB","示例含权企业债","CREDIT","CORPORATE","AAA","广东",true);
    }
    void add(String id,String name,String market,String issuerType,String rating,String region,boolean option){securities.put(id,JsonUtils.object("securityId",id,"securityName",name,"market",market,"issuerId","IS-"+id,"issuerType",issuerType,"bondRating",rating,"issuerRating",rating,"region",region,"remainingYears","2.5","hasExerciseOption",option,"valuationYield","2.4500","valuationDate",java.time.LocalDate.now().toString(),"referenceVersion","SIM-1"));}
    public String user(String login){return switch(login){case "demo"->"U-001";case "demo2"->"U-002";case "admin"->"U-ADMIN";case "intake"->"S-INTAKE";case "search"->"S-SEARCH";default->null;};}
    public JSONObject require(String id){JSONObject s=securities.get(id);if(s==null)throw ApiException.forField("1001","VALIDATION_FAILED","Unknown simulated security","securityId");return s;}
    public JSONObject enrich(JSONObject q){JSONObject o=JsonUtils.deepCopy(q),r=securities.get(q.getString("securityId"));if(r!=null)o.putAll(r);return o;}
    public List<JSONObject> records(String kind){return switch(kind){case "SECURITY"->new ArrayList<>(securities.values());case "INSTITUTION"->List.of(JsonUtils.object("institutionId","I-001","institutionName","示例对手机构"),JsonUtils.object("institutionId","I-002","institutionName","示例证券公司"));case "CONTACT"->List.of(JsonUtils.object("contactId","C-001","institutionId","I-001","contactName","示例联系人"),JsonUtils.object("contactId","C-002","institutionId","I-002","contactName","示例联系人"));case "ISSUER"->securities.values().stream().map(x->JsonUtils.object("issuerId",x.getString("issuerId"),"issuerName",x.getString("securityName")+"发行人","issuerType",x.getString("issuerType"),"issuerRating",x.getString("issuerRating"),"region",x.getString("region"))).toList();case "DAILY_VALUATION"->securities.values().stream().map(x->JsonUtils.object("valuationId","SIM-"+x.getString("securityId"),"securityId",x.getString("securityId"),"valuationDate",x.getString("valuationDate"),"valuationSource","CHINABOND","dataVendor","SIMULATOR","valuationBasis","MATURITY","yield",x.getString("valuationYield"))).toList();default->throw ApiException.forField("1001","VALIDATION_FAILED","Unknown reference kind","kind");};}
}
