package ficc.quote;

import ficc.common.*;

import com.alibaba.fastjson2.*;
import java.util.*;

final class Intake {
    static Object apply(QuoteService svc,QuoteService.Call c)throws Exception{
        if(!c.user().equals("S-INTAKE"))throw ApiException.forField("1003","FORBIDDEN","Intake simulator identity required",null);
        JSONObject r=c.request();String type=RequestValidator.requireString(r,"sourceType"),ext=RequestValidator.requireString(r,"externalMessageId");
        if(!Set.of("QT","IDEAL").contains(type))throw ApiException.forField("1001","VALIDATION_FAILED","Unknown sourceType","sourceType");
        if(!"QT-U-88".equals(RequestValidator.requireString(r,"sourceUserId")))throw ApiException.forField("1001","VALIDATION_FAILED","Simulator maps QT-U-88 to U-001 only","sourceUserId");
        String owner="U-001",rid=RequestValidator.requireString(r,"requestId"),hash=JsonUtils.canonicalHash(r);JSONArray saved=svc.store.receipt(c.user(),c.topic(),rid,hash);
        if(!saved.isEmpty())return saved.getJSONObject(0);
        JSONArray inputs=r.getJSONArray("quotes");if(inputs==null||inputs.isEmpty()||inputs.size()>100)throw ApiException.forField("1008","RESOURCE_LIMIT","1..100 intake quotes required",null);
        String sourceId=svc.store.scalar("SELECT id FROM sources WHERE source_type=? AND external_id=?",type,ext);
        JSONObject raw;
        if(sourceId==null){sourceId=IdGenerator.newId("SM-");raw=JsonUtils.object("sourceMessageId",sourceId,"sourceType",type,"externalMessageId",ext,"sourceTime",RequestValidator.requireString(r,"sourceTime"),"text",JsonUtils.getStringOrDefault(r,"text",""),"ownerUserId",owner,"quoteIds",JsonUtils.array());svc.store.update("INSERT INTO sources VALUES (?,?,?,?::jsonb)",sourceId,type,ext,JsonUtils.toJson(raw));}
        else raw=svc.store.rows("SELECT payload FROM sources WHERE id=?",sourceId).get(0);
        JSONArray results=new JSONArray();int i=0;
        for(JSONObject in:inputs.toJavaList(JSONObject.class)){
            String extQuote=in.getString("externalQuoteId");JSONObject result;
            String itemTopic=c.topic()+"/"+i,itemHash=JsonUtils.canonicalHash(JsonUtils.object("requestHash",hash,"index",i));
            JSONArray itemReceipt=svc.store.receipt(owner,itemTopic,rid,itemHash);
            try{
                if(!itemReceipt.isEmpty())result=itemReceipt.getJSONObject(0);
                else {
                RequestValidator.requireString(in,"externalQuoteId");String action=RequestValidator.requireString(in,"action"),version=RequestValidator.requireString(in,"externalRevision");
                long rev;try{rev=Long.parseLong(version);}catch(NumberFormatException e){throw ApiException.forField("1001","VALIDATION_FAILED","Positive externalRevision required","externalRevision");}if(rev<1)throw ApiException.forField("1001","VALIDATION_FAILED","Positive externalRevision required","externalRevision");
                JSONObject existing=svc.quotes.values().stream().filter(q->{JSONObject src=q.getJSONObject("source");return src!=null&&type.equals(src.getString("type"))&&extQuote.equals(src.getString("externalQuoteId"));}).findFirst().orElse(null);
                JSONObject item=new JSONObject();for(String k:QuoteService.EDIT)if(in.containsKey(k))item.put(k,in.get(k));String op;
                if(action.equals("NEW")&&existing==null){op="create";item.put("clientQuoteId",type+":"+extQuote);}
                else if(action.equals("UPDATE")&&existing!=null&&existing.getString("status").equals("PENDING_CONFIRM")){
                    if(rev<=existing.getJSONObject("source").getLongValue("externalRevision"))throw ApiException.forField("1004","REVISION_CONFLICT","External revision is not newer",null);
                    op="update";item.put("quoteId",existing.getString("quoteId"));item.put("expectedRevision",existing.getString("revision"));
                }else if(action.equals("NEW")&&existing!=null)throw ApiException.forField("1006","IDEMPOTENCY_CONFLICT","External quote already exists",null);
                else throw ApiException.forField("1010","UNSUPPORTED","Only NEW and pending-quote UPDATE are enabled",null);
                JSONObject source=JsonUtils.object("sourceMessageId",sourceId,"source",JsonUtils.object("type",type,"externalQuoteId",extQuote,"externalRevision",version,"sourceTime",r.getString("sourceTime")));
                // Check the original input receipt before deriving state-dependent expectedRevision.
                // This receipt is committed atomically with the quote by write().
                QuoteService.Call inner=new QuoteService.Call(owner,c.session(),itemTopic,JsonUtils.object("requestId",rid,"quotes",JsonUtils.array(item)));
                JSONObject out=(JSONObject)svc.write(inner,op,source,itemHash);result=out.getJSONArray("results").getJSONObject(0);
                }
            }catch(ApiException e){result=JsonUtils.object("Code",e.code,"Msg",e.getMessage(),"error",e.details);}
            svc.store.receipt(owner,itemTopic,rid,JsonUtils.array(result));
            result.put("externalQuoteId",extQuote);results.add(result);if(result.getString("quoteId")!=null&&!raw.getJSONArray("quoteIds").contains(result.getString("quoteId")))raw.getJSONArray("quoteIds").add(result.getString("quoteId"));i++;
        }
        JSONObject out=JsonUtils.object("sourceMessageId",sourceId,"results",results);
        svc.store.begin();try{svc.store.update("UPDATE sources SET payload=?::jsonb WHERE id=?",JsonUtils.toJson(raw),sourceId);svc.store.receipt(c.user(),c.topic(),rid,JsonUtils.array(out));svc.store.commit();}catch(Exception e){svc.store.rollback();throw e;}return out;
    }
}
