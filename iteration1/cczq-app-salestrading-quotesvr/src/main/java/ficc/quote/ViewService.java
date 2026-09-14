package ficc.quote;

import ficc.common.*;

import com.alibaba.fastjson2.*;
import java.math.*;
import java.time.*;
import java.util.*;

/** Separate QuoteView and PriceView state; shared list-position utility only. */
final class ViewService {
    final QuoteService svc;
    final Map<String,Stream> streams=new LinkedHashMap<>();
    final Map<String,Long> bookRevisions=new HashMap<>();
    final String marketStream=IdGenerator.newId("MDE-");long marketSequence=0;
    static class Stream {String user,session,domain,route,id;long sequence=0;final Map<String,View> views=new LinkedHashMap<>();final Set<String> used=new HashSet<>();boolean subscribed;}
    abstract static class View {String id;JSONObject request;int limit;List<String> ids=new ArrayList<>();Map<String,JSONObject> loaded=new LinkedHashMap<>();}
    static final class QuoteView extends View {}
    static final class PriceView extends View {}
    ViewService(QuoteService svc){this.svc=svc;}
    Stream find(String session,String domain){Stream s=streams.get(session+":"+domain);if(s==null)throw ApiException.forField("1009","NOT_READY","Call stream.open first",null);return s;}
    Object openStream(QuoteService.Call c,String domain){
        RequestValidator.allowOnlyFields(c.request(),"requestId");Stream s=streams.computeIfAbsent(c.session()+":"+domain,k->{Stream x=new Stream();x.user=c.user();x.session=c.session();x.domain=domain;x.route=c.session();x.id=IdGenerator.newId(domain.equals("quote")?"QE-":"PE-");return x;});return JsonUtils.object("sessionId",s.route,"streamId",s.id,"pushTopic",topic(s));
    }
    String topic(Stream s){return "ficc."+s.domain+".report."+s.route;}
    void authorize(String session,String user,String topic){
        if(topic.equals("ficc.quote.events")||topic.equals("ficc.price.snapshots")){if(!svc.access.internal(user))throw ApiException.forField("1003","FORBIDDEN","Internal channel",null);return;}
        if(topic.startsWith("ficc.price.book.")){if(!svc.access.internal(user))throw ApiException.forField("1003","FORBIDDEN","Raw STC channel is internal",null);return;}
        for(Stream s:streams.values())if(topic(s).equals(topic)&&s.session.equals(session)&&s.user.equals(user)){s.subscribed=true;return;}
        throw ApiException.forField("1003","FORBIDDEN","Private topic not owned by connection",null);
    }
    void unsubscribe(String sid,String topic){for(Stream s:streams.values())if(s.session.equals(sid)&&topic(s).equals(topic))s.subscribed=false;}
    Map<String,Object> image(String topic,String sid){
        if(topic.startsWith("ficc.price.report.")){Stream s=find(sid,"price");return Map.of(topic,snapshot(s));}
        if(topic.equals("ficc.price.snapshots"))return Map.of(topic,feed(true,new ArrayList<>(svc.refs.securities().keySet())));
        if(topic.startsWith("ficc.price.book.")){Map<String,Object> m=new LinkedHashMap<>();String id=topic.substring("ficc.price.book.".length());for(String sec:svc.refs.securities().keySet())if(id.equals("**")||id.equals(sec))m.put("ficc.price.book."+sec,book(null,sec,JsonUtils.object(),"FULL",0).get("marketData"));return m;}
        return Map.of(); // Quote ordinary subscription never manufactures an initial collection.
    }
    Object openView(QuoteService.Call c,String domain){
        JSONObject r=c.request();if(domain.equals("quote"))RequestValidator.allowOnlyFields(r,"requestId","viewId","filter","limit","status","ownerScope");else RequestValidator.allowOnlyFields(r,"requestId","viewId","filter","limit","mode","depth");
        Stream s=find(c.session(),domain);if(!s.subscribed)throw ApiException.forField("1009","NOT_READY","Subscribe to this domain's channel first",null);
        String id=RequestValidator.requireString(r,"viewId");if(s.used.contains(id))throw ApiException.forField("1001","VALIDATION_FAILED","viewId cannot be reused", "viewId");if(s.views.size()>=20)throw ApiException.forField("1008","RESOURCE_LIMIT","View limit reached",null);
        View v=domain.equals("quote")?new QuoteView():new PriceView();v.id=id;v.request=JsonUtils.deepCopy(r);v.limit=svc.limit(r,1000);FilterEngine.validate(r.getJSONObject("filter"));if(domain.equals("price"))mode(r);
        refresh(s,v);s.used.add(id);s.views.put(id,v);return result(s,v);
    }
    Object extend(QuoteService.Call c,String domain){RequestValidator.allowOnlyFields(c.request(),"requestId","viewId","limit");Stream s=find(c.session(),domain);View v=s.views.get(RequestValidator.requireString(c.request(),"viewId"));if(v==null)throw ApiException.forField("1002","NOT_FOUND","View not found",null);int n=svc.limit(c.request(),10000);if(n<=v.limit)throw ApiException.forField("1001","VALIDATION_FAILED","limit must increase","limit");v.limit=n;refresh(s,v);return result(s,v);}
    Object close(QuoteService.Call c,String domain){RequestValidator.allowOnlyFields(c.request(),"requestId","viewId");Stream s=find(c.session(),domain);String id=RequestValidator.requireString(c.request(),"viewId");s.views.remove(id);return JsonUtils.object("viewId",id,"closed",true);}
    JSONObject result(Stream s,View v){JSONObject out=JsonUtils.object("streamId",s.id,"sequence",Long.toString(s.sequence),"view",viewRecord(s,v));if(v instanceof QuoteView)out.put("quotes",new ArrayList<>(v.loaded.values()));else out.put("books",books(v));return out;}
    JSONObject viewRecord(Stream s,View v){return JsonUtils.object("viewId",v.id,"limit",v.limit,"totalCount",total(s,v),"ids",v.ids);}
    int total(Stream s,View v){if(v instanceof QuoteView)return svc.selectQuotes(s.user,v.request).size();return priceIds(s.user,v.request).size();}
    void refresh(Stream s,View v){List<JSONObject> rows;if(v instanceof QuoteView)rows=svc.selectQuotes(s.user,v.request).stream().map(q->svc.visible(q,s.user)).toList();else rows=priceIds(s.user,v.request).stream().map(id->book(s.user,id,v.request.getJSONObject("filter"),mode(v.request),v.request.getIntValue("depth"))).toList();v.loaded=new LinkedHashMap<>();for(JSONObject row:rows.stream().limit(v.limit).toList())v.loaded.put(row.getString(v instanceof QuoteView?"quoteId":"securityId"),row);v.ids=new ArrayList<>(v.loaded.keySet());}
    List<JSONObject> books(View v){return v.loaded.values().stream().map(p->{JSONObject b=JsonUtils.deepCopy(p);b.put("viewId",v.id);return b;}).toList();}
    JSONObject snapshot(Stream s){JSONArray views=new JSONArray(),books=new JSONArray();for(View v:s.views.values()){refresh(s,v);views.add(viewRecord(s,v));books.addAll(books(v));}return JsonUtils.object("Code","0000","Msg","成功","kind","SNAPSHOT","streamId",s.id,"sequence",Long.toString(s.sequence),"books",books,"views",views);}
    static String mode(JSONObject r){String mode=RequestValidator.requireString(r,"mode");if(!Set.of("FULL","BEST","DEPTH").contains(mode))throw ApiException.forField("1001","VALIDATION_FAILED","Invalid mode","mode");if(mode.equals("DEPTH")&&(r.getIntValue("depth")<1||r.getIntValue("depth")>1000))throw ApiException.forField("1001","VALIDATION_FAILED","Positive depth required","depth");if(!mode.equals("DEPTH")&&r.containsKey("depth"))throw ApiException.forField("1001","VALIDATION_FAILED","depth only allowed for DEPTH","depth");return mode;}
    List<String> priceIds(String user,JSONObject r){JSONObject f=r.getJSONObject("filter");FilterEngine.validate(f);Map<String,String> latest=new HashMap<>();for(JSONObject q:svc.quotes.values())if(q.getString("status").equals("ACTIVE")&&svc.matches(user,q,f))latest.merge(q.getString("securityId"),q.getString("updateTime"),(a,b)->a.compareTo(b)>=0?a:b);return latest.keySet().stream().sorted(Comparator.<String,String>comparing(latest::get).reversed().thenComparing(x->x)).toList();}
    Object priceQuery(QuoteService.Call c){RequestValidator.allowOnlyFields(c.request(),"requestId","securityIds","filter","mode","depth");JSONArray ids=c.request().getJSONArray("securityIds");if(ids==null||ids.size()>100)throw ApiException.forField("1008","RESOURCE_LIMIT","securityIds required, max 100",null);String mode=mode(c.request());FilterEngine.validate(c.request().getJSONObject("filter"));return JsonUtils.object("prices",ids.toJavaList(String.class).stream().map(id->{svc.refs.require(id);return book(c.user(),id,c.request().getJSONObject("filter"),mode,c.request().getIntValue("depth"));}).toList());}
    JSONObject book(String user,String security,JSONObject filter,String mode,int depth){
        List<JSONObject> qs=svc.quotes.values().stream().filter(q->security.equals(q.getString("securityId"))&&q.getString("status").equals("ACTIVE")&&svc.matches(user,q,filter)).toList();
        for(JSONObject q:qs){boolean exercise=svc.refs.require(security).getBooleanValue("hasExerciseOption");if(!"1".equals(q.getString("quoteType"))||!Objects.equals(q.getString("yieldType"),exercise?"2":"1"))throw ApiException.forField("1009","NOT_READY","CalSvr unavailable: enter the comparable yield for this prototype","price");}
        JSONArray entries=new JSONArray();JSONObject quoteRefs=JsonUtils.object();
        for(String side:List.of("1","2")){
            Comparator<JSONObject> cmp=Comparator.comparing(q->new BigDecimal(q.getString("price")));if(side.equals("2"))cmp=cmp.reversed();cmp=cmp.thenComparing(Comparator.<JSONObject,String>comparing(q->q.getString("updateTime")).reversed()).thenComparing(q->q.getString("quoteId"));
            List<JSONObject> sorted=qs.stream().filter(q->side.equals(q.getString("side"))).sorted(cmp).toList();BigDecimal last=null;int level=0,ordinal=0,max=mode.equals("FULL")?Integer.MAX_VALUE:mode.equals("BEST")?1:depth;
            for(JSONObject q:sorted){BigDecimal value=new BigDecimal(q.getString("price"));if(last==null||last.compareTo(value)!=0){level++;last=value;}if(level>max)break;String entryId="FICC:"+q.getString("quoteId");OffsetDateTime time=OffsetDateTime.parse(q.getString("updateTime"));JSONObject e=JsonUtils.object("MDEntryID",entryId,"MDEntryType",side.equals("1")?"0":"1","MDEntryPx",q.getString("price"),"MDEntrySize",q.getString("quantity"),"PriceType","3","ExerciseFlag",q.getString("yieldType").equals("2")?"0":"1","SettlType",q.getString("settlType"),"UserID",q.getString("ownerUserId"),"MDPriceLevel",Integer.toString(++ordinal),"MDEntryDate",time.format(java.time.format.DateTimeFormatter.ofPattern("yyyyMMdd")),"MDEntryTime",time.format(java.time.format.DateTimeFormatter.ofPattern("HH:mm:ss.SSS")));e.put(q.getString("yieldType").equals("2")?"YieldToExecution":"Yield",q.getString("price"));if(user == null || svc.owns(user,q)){e.put("InstitutionID",q.get("institutionId"));e.put("TraderID",q.get("contactId"));}entries.add(e);quoteRefs.put(entryId,JsonUtils.object("quoteId",q.getString("quoteId"),"quoteRevision",q.getString("revision")));}
        }
        return JsonUtils.object("securityId",security,"bookRevision",Long.toString(bookRevisions.getOrDefault(security,0L)),"marketData",JsonUtils.object("SecurityID",security,"Symbol",svc.refs.require(security).getString("securityName"),"MarketIndicator","4","VenueTypeGW","FICC","Venues","FICC","NoMDEntries",entries),"quoteRefs",quoteRefs);
    }
    JSONObject feed(boolean initial,List<String> ids){return JsonUtils.object("Code","0000","Msg","成功","kind",initial?"SNAPSHOT":"UPDATE","streamId",marketStream,"sequence",Long.toString(marketSequence),"quotePosition",JsonUtils.object("epoch",svc.store.epoch,"sequence",Long.toString(svc.lastSequence)),"prices",ids.stream().map(id->book(null,id,JsonUtils.object(),"FULL",0)).toList());}
    void changed(JSONObject q,JSONObject event,String previousSecurity){
        Set<String> securities=new LinkedHashSet<>();if(q.getString("securityId")!=null)securities.add(q.getString("securityId"));
        // Include the previous security when an active quote changes bond.
        if(previousSecurity!=null)securities.add(previousSecurity);
        for(String id:securities)bookRevisions.merge(id,1L,Long::sum);marketSequence++;
        for(Stream s:streams.values())if(s.subscribed){try{publishChanges(s,q,event,null);}catch(ApiException ex){svc.publish.accept(topic(s),JsonUtils.object("Code",ex.code,"Msg",ex.getMessage(),"kind","RESET_REQUIRED","streamId",s.id,"sequence",Long.toString(++s.sequence)));}}
        try{svc.publish.accept("ficc.price.snapshots",feed(false,new ArrayList<>(securities)));for(String id:securities)svc.publish.accept("ficc.price.book."+id,book(null,id,JsonUtils.object(),"FULL",0).getJSONObject("marketData"));}catch(ApiException e){svc.publish.accept("ficc.price.snapshots",JsonUtils.object("Code","1009","Msg",e.getMessage(),"kind","RESET_REQUIRED","streamId",marketStream,"sequence",Long.toString(marketSequence)));}
    }
    void publishChanges(Stream s,JSONObject changed,JSONObject event,JSONObject setting){
        JSONArray deltas=new JSONArray(),bookUpdates=new JSONArray(),reports=new JSONArray();Map<String,JSONObject> quoteUpdates=new LinkedHashMap<>();
        for(View v:s.views.values()){
            List<String> oldIds=v.ids;Map<String,JSONObject> oldRows=v.loaded;refresh(s,v);
            List<String> remove=oldIds.stream().filter(id->!v.loaded.containsKey(id)).toList();JSONArray place=new JSONArray();List<String> working=new ArrayList<>(oldIds);working.removeAll(remove);
            for(int i=0;i<v.ids.size();i++){String id=v.ids.get(i);if(i>=working.size()||!id.equals(working.get(i))){working.remove(id);working.add(i,id);place.add(JsonUtils.object("id",id,"index",i));}}
            boolean dataChanged=false;for(String id:v.ids)if(!JsonUtils.toJson(v.loaded.get(id)).equals(JsonUtils.toJson(oldRows.get(id)))){dataChanged=true;if(v instanceof QuoteView)quoteUpdates.put(id,v.loaded.get(id));else{JSONObject b=JsonUtils.deepCopy(v.loaded.get(id));b.put("viewId",v.id);bookUpdates.add(b);}}
            if(dataChanged||!remove.isEmpty()||!place.isEmpty()||changed!=null||setting!=null)deltas.add(JsonUtils.object("viewId",v.id,"totalCount",total(s,v),"removeIds",remove,"place",place));
        }
        if(s.domain.equals("quote")&&changed!=null&&svc.owns(s.user,changed)){
            quoteUpdates.put(changed.getString("quoteId"),svc.visible(changed,s.user));reports.add(JsonUtils.object("eventId",event.getString("eventId"),"requestId",event.getString("requestId"),"clientQuoteId",changed.get("clientQuoteId"),"quoteId",changed.getString("quoteId"),"revision",changed.getString("revision"),"action",event.getString("action"),"operatorUserId",event.getString("operatorUserId"),"operationTime",event.getString("operationTime")));
        }
        if(deltas.isEmpty()&&reports.isEmpty()&&setting==null)return;
        JSONObject out=JsonUtils.object("Code","0000","Msg","成功","kind","UPDATE","streamId",s.id,"sequence",Long.toString(++s.sequence),"views",deltas);
        if(s.domain.equals("quote")){out.put("quotes",new ArrayList<>(quoteUpdates.values()));out.put("reports",reports);}else out.put("books",bookUpdates);if(setting!=null)out.put("settingsChanged",JsonUtils.array(setting));svc.publish.accept(topic(s),out);
    }
    void settingsChanged(String user,String kind,String id,String rev,boolean deleted){for(Stream s:streams.values())if(s.user.equals(user)&&s.subscribed){try{publishChanges(s,null,null,JsonUtils.object("type",kind.equals("watchlist")?"WATCHLIST":"FILTER_GROUP","id",id,"revision",rev,"action",deleted?"REMOVE":"UPSERT"));}catch(ApiException e){svc.publish.accept(topic(s),JsonUtils.object("Code","1007","Msg","Selected group changed or was deleted; reopen view","kind","RESET_REQUIRED","streamId",s.id,"sequence",Long.toString(++s.sequence)));}}}
    void heartbeat(){for(Stream s:streams.values())if(s.subscribed)svc.publish.accept(topic(s),JsonUtils.object("Code","0000","Msg","成功","kind","HEARTBEAT","streamId",s.id,"sequence",Long.toString(++s.sequence)));svc.publish.accept("ficc.quote.events",JsonUtils.object("Code","0000","Msg","成功","kind","HEARTBEAT","epoch",svc.store.epoch,"currentSequence",Long.toString(svc.lastSequence)));}
    void disconnected(String sid){streams.values().removeIf(s->s.session.equals(sid));}
}
