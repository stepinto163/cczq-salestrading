package ficc.quote;

import ficc.common.*;

import com.alibaba.fastjson2.*;
import java.util.*;
import java.util.function.*;
import java.math.BigDecimal;
import java.time.*;

/** All QuoteSvr request handlers share one ordered standalone dispatcher, not a shared Quote/Price DTO. */
final class QuoteService {
    static final List<String> EDIT=List.of("securityId","institutionId","institutionName","contactId","contactName","side","quoteType","yieldType","price","quantity","quantityUnit","currency","tradeDate","settlType","urgent","recommended","whole","oco","packing","remark");
    static final List<String> FLAGS=List.of("urgent","recommended","whole","oco","packing");
    final PgStore store; final ReferenceData refs;
    final Map<String,JSONObject> quotes=new LinkedHashMap<>(),settings=new LinkedHashMap<>();
    final ViewService views;
    final SearchSvrClient search; final QuoteMatcher matcher;
    BiConsumer<String,JSONObject> publish=(t,v)->{};
    final Map<String,Cursor> cursors=new LinkedHashMap<>();
    final Map<String,Function<Call,Object>> handlers=new LinkedHashMap<>();
    long lastSequence;
    record Call(String user,String session,String topic,JSONObject request){}
    record Cursor(String user,String session,String topic,String field,List<JSONObject> rows,int offset,int limit,long expires,JSONObject extra){}
    QuoteService(PgStore store,ReferenceData refs)throws Exception{
        this.store=store;this.refs=refs;
        for(JSONObject q:store.quotes())quotes.put(q.getString("quoteId"),q);
        for(JSONObject g:store.rows("SELECT payload FROM settings"))settings.put(settingId(g),g);
        matcher=new QuoteMatcher(refs,settings);search=new LocalSearchSvrClient(store,refs,quotes,matcher);
        lastSequence=store.tail();views=new ViewService(this);
        for(String op:List.of("create","confirm","update","refer","unrefer","delete","clone"))handlers.put("ficc.quote."+op,c->unchecked(()->write(c,op)));
        handlers.put("ficc.quote.get",c->JsonUtils.object("quote",visible(get(c.request.getString("quoteId"),c.user),c.user)));
        handlers.put("ficc.quote.query",c->page(c,"quotes",selectQuotes(c.user,c.request).stream().map(q->visible(q,c.user)).toList(),JsonUtils.object("reference",referenceFor(selectQuotes(c.user,c.request)))));
        handlers.put("ficc.quote.history.query",c->unchecked(()->history(c)));
        handlers.put("ficc.quote.history.search",c->unchecked(()->historySearch(c)));
        handlers.put("ficc.quote.sourceMessage.query",c->unchecked(()->sourceQuery(c)));
        handlers.put("ficc.quote.dictionary.query",c->dictionary(false,c.request));
        handlers.put("ficc.price.dictionary.query",c->dictionary(true,c.request));
        handlers.put("ficc.price.query",c->views.priceQuery(c));
        for(String d:List.of("quote","price")){
            handlers.put("ficc."+d+".stream.open",c->views.openStream(c,d));
            handlers.put("ficc."+d+".view.open",c->views.openView(c,d));
            handlers.put("ficc."+d+".view.extend",c->views.extend(c,d));
            handlers.put("ficc."+d+".view.close",c->views.close(c,d));
        }
        for(String kind:List.of("watchlist","filterGroup")){
            handlers.put("ficc."+kind+".query",c->groupQuery(c,kind));
            handlers.put("ficc."+kind+".save",c->unchecked(()->saveGroup(c,kind,false)));
            handlers.put("ficc."+kind+".delete",c->unchecked(()->saveGroup(c,kind,true)));
        }
        handlers.put("ficc.quote.intake.apply",c->unchecked(()->intake(c)));
        handlers.put("ficc.quote.state.query",c->state(c));
        handlers.put("ficc.quote.event.query",c->unchecked(()->events(c)));
        handlers.put("ficc.base.get",c->reference(c,false));handlers.put("ficc.base.query",c->reference(c,true));
    }
    interface Checked {Object get()throws Exception;}
    static Object unchecked(Checked f){try{return f.get();}catch(ApiException e){throw e;}catch(Exception e){throw new RuntimeException(e);}}
    synchronized JSONObject handle(String user,String sid,String topic,JSONObject r){
        String requestId=r.getString("requestId");
        try{
            RequestValidator.requireString(r,"requestId");if(user==null)throw ApiException.forField("1003","FORBIDDEN","Unknown simulator user",null);
            Function<Call,Object> h=handlers.get(topic);if(h==null)throw ApiException.forField("1010","UNSUPPORTED","Unknown interface: "+topic,null);
            Call c=new Call(user,sid,topic,r);
            if(r.getString("cursor")!=null&&!topic.endsWith("state.query"))return ApiResponse.success(requestId,continuePage(c));
            return ApiResponse.success(requestId,h.apply(c));
        }catch(ApiException e){return ApiResponse.failure(requestId,e);}catch(Exception e){store.rollback();System.err.println("Request failed: "+topic+" ("+e.getClass().getSimpleName()+")");return ApiResponse.failure(requestId,ApiException.forField("1099","INTERNAL_ERROR","Internal error; see local server log",null));}
    }
    JSONObject get(String id,String user){JSONObject q=quotes.get(id);if(q==null||(!"ACTIVE".equals(q.getString("status"))&&!owns(user,q)))throw ApiException.forField("1002","NOT_FOUND","Quote not found", "quoteId");return q;}
    static boolean owns(String user,JSONObject q){return user.equals(q.getString("ownerUserId"))||user.equals("U-ADMIN")||user.startsWith("S-");}
    JSONObject visible(JSONObject q,String user){JSONObject v=JsonUtils.deepCopy(q);if(!owns(user,q)){for(String field:List.of("institutionId","institutionName","contactId","contactName"))v.put(field,null);}return v;}
    void revision(JSONObject q,JSONObject in){String expected=RequestValidator.requireString(in,"expectedRevision");if(!expected.equals(q.getString("revision")))throw new ApiException("1004","报价已被更新",JsonUtils.object("type","REVISION_CONFLICT","quoteId",q.getString("quoteId"),"expectedRevision",expected,"currentRevision",q.getString("revision")));}
    Object write(Call c,String op)throws Exception{return write(c,op,null);}
    Object write(Call c,String op,JSONObject source)throws Exception{
        return write(c,op,source,JsonUtils.canonicalHash(c.request));
    }
    Object write(Call c,String op,JSONObject source,String receiptHash)throws Exception{
        RequestValidator.allowOnlyFields(c.request,"requestId","quotes");JSONArray inputs=c.request.getJSONArray("quotes");
        if(inputs==null||inputs.isEmpty()||inputs.size()>100)throw ApiException.forField("1008","RESOURCE_LIMIT","quotes requires 1..100 entries","quotes");
        Set<String> ids=new HashSet<>();for(Object x:inputs){if(!(x instanceof JSONObject in))throw ApiException.forField("1001","VALIDATION_FAILED","Invalid quote object","quotes");String id=in.getString(op.equals("create")||op.equals("clone")?"clientQuoteId":"quoteId");if(id!=null&&!ids.add(id))throw ApiException.forField("1001","VALIDATION_FAILED","Duplicate quote in batch","quotes");}
        JSONArray results=store.receipt(c.user,c.topic,c.request.getString("requestId"),receiptHash);
        for(int i=results.size();i<inputs.size();i++){
            JSONObject in=inputs.getJSONObject(i),q=null,event=null,result;String previousSecurity=null;
            try{
                store.begin();JSONObject before=null;
                boolean create=op.equals("create")||op.equals("clone");
                if(create){
                    String clientId=RequestValidator.requireString(in,"clientQuoteId");
                    if(quotes.values().stream().anyMatch(x->c.user.equals(x.getString("ownerUserId"))&&clientId.equals(x.getString("clientQuoteId"))))throw ApiException.forField("1006","IDEMPOTENCY_CONFLICT","clientQuoteId already exists","clientQuoteId");
                    q=newQuote(c.user,clientId);
                    if(op.equals("clone")){JSONObject src=get(RequestValidator.requireString(in,"sourceQuoteId"),c.user);revision(src,in);for(String k:EDIT)q.put(k,src.get(k));q.put("clonedFromQuoteId",src.getString("quoteId"));}
                }else{
                    before=get(RequestValidator.requireString(in,"quoteId"),c.user);if(!owns(c.user,before))throw ApiException.forField("1003","FORBIDDEN","Only quote owner/admin may change it",null);revision(before,in);q=JsonUtils.deepCopy(before);
                    previousSecurity=before.getString("securityId");
                    String st=q.getString("status");boolean valid=switch(op){case "confirm"->st.equals("PENDING_CONFIRM");case "update"->st.equals("PENDING_CONFIRM")||st.equals("ACTIVE");case "refer"->st.equals("ACTIVE");case "unrefer"->st.equals("REFERRED");case "delete"->st.equals("REFERRED")||st.equals("PENDING_CONFIRM");default->false;};
                    if(!valid)throw ApiException.forField("1005","INVALID_STATE","Action "+op+" is not allowed for "+st,"status");
                    q.put("revision",Long.toString(q.getLongValue("revision")+1));
                }
                Set<String> allowed=new HashSet<>(List.of("quoteId","expectedRevision","clientQuoteId","sourceQuoteId","reason"));
                if(create||op.equals("update")||op.equals("confirm"))allowed.addAll(EDIT);
                for(String k:in.keySet())if(!allowed.contains(k))throw ApiException.forField("1001","VALIDATION_FAILED","Field cannot be changed",k);
                for(String k:EDIT)if(in.containsKey(k))q.put(k,in.get(k));
                switch(op){case "confirm","unrefer"->q.put("status","ACTIVE");case "refer"->q.put("status","REFERRED");case "delete"->q.put("status","DELETED");}
                if(source!=null){q.put("source",source.getJSONObject("source"));q.put("sourceMessageId",source.getString("sourceMessageId"));}
                validateQuote(q,q.getString("status").equals("ACTIVE"));
                String now=BusinessClock.currentTime();q.put("updatedBy",c.user);q.put("updateTime",now);
                if(op.equals("confirm")){q.put("confirmedBy",c.user);q.put("confirmTime",now);}
                if(q.getString("status").equals("ACTIVE")&&!op.equals("refer"))q.put("quoteTime",now);
                store.saveQuote(q);
                event=store.event(JsonUtils.object("type","QUOTE","action",op.toUpperCase(Locale.ROOT),"quoteId",q.getString("quoteId"),"quote",q,"operatorUserId",c.user,"operationTime",now,"requestId",c.request.getString("requestId")));
                store.history(JsonUtils.object("eventId",event.getString("eventId"),"quoteId",q.getString("quoteId"),"revision",q.getString("revision"),"action",op.toUpperCase(Locale.ROOT),"operatorUserId",c.user,"operationTime",now,"before",before,"after",q,"reason",in.get("reason")));
                result=JsonUtils.object("quoteId",q.getString("quoteId"),"revision",q.getString("revision"),"Code","0000","Msg","成功");if(create)result.put("clientQuoteId",q.getString("clientQuoteId"));
                results.add(result);store.receipt(c.user,c.topic,c.request.getString("requestId"),results);store.commit();
            }catch(ApiException e){store.rollback();result=JsonUtils.object("quoteId",in.get("quoteId"),"clientQuoteId",in.get("clientQuoteId"),"Code",e.code,"Msg",e.getMessage(),"error",e.details);results.add(result);store.receipt(c.user,c.topic,c.request.getString("requestId"),results);q=null;event=null;
            }catch(Exception e){store.rollback();throw e;}
            if(q!=null){quotes.put(q.getString("quoteId"),q);committed(event);views.changed(q,event,previousSecurity);}
        }
        return JsonUtils.object("results",results);
    }
    JSONObject newQuote(String user,String clientId){JSONObject q=JsonUtils.object("quoteId",IdGenerator.newId("Q-"),"clientQuoteId",clientId,"sourceMessageId",null,"source",JsonUtils.object("type","MANUAL"),"ownerUserId",user,"revision","1","status","PENDING_CONFIRM","quantityUnit","WAN","currency","CNY","createdBy",user,"updatedBy",user,"confirmedBy",null,"createTime",BusinessClock.currentTime(),"updateTime",BusinessClock.currentTime(),"quoteTime",null,"confirmTime",null);for(String k:EDIT)q.putIfAbsent(k,null);for(String f:FLAGS)q.put(f,false);return q;}
    void validateQuote(JSONObject q,boolean active){
        for(String f:FLAGS)if(!(q.get(f) instanceof Boolean))throw ApiException.forField("1001","VALIDATION_FAILED","Expected boolean",f);
        if(q.getBooleanValue("oco")&&q.getBooleanValue("packing"))throw ApiException.forField("1001","VALIDATION_FAILED","OCO and packing are mutually exclusive","oco");
        if(q.getString("securityId")!=null)refs.require(q.getString("securityId"));
        for(var e:Map.of("side",Set.of("1","2"),"quoteType",Set.of("1","2","3"),"yieldType",Set.of("1","2"),"settlType",Set.of("1","2")).entrySet()){
            if(q.get(e.getKey())!=null&&!e.getValue().contains(q.getString(e.getKey())))throw ApiException.forField(e.getKey().equals("settlType")?"1010":"1001","VALIDATION_FAILED","Unsupported dictionary value",e.getKey());
        }
        if(!"1".equals(q.getString("quoteType"))&&q.get("yieldType")!=null)throw ApiException.forField("1001","VALIDATION_FAILED","Non-yield quote must have null yieldType","yieldType");
        for(String f:List.of("price","quantity"))if(q.get(f)!=null){if(!(q.get(f) instanceof String))throw ApiException.forField("1001","VALIDATION_FAILED","Use decimal string",f);try{BigDecimal n=new BigDecimal(q.getString(f));if(f.equals("price")&&"1".equals(q.getString("quoteType"))&&n.scale()>4)throw new IllegalArgumentException();if(f.equals("quantity")&&n.signum()<=0)throw new IllegalArgumentException();}catch(Exception e){throw ApiException.forField("1001","VALIDATION_FAILED","Invalid decimal / yield precision",f);}}
        if(q.get("tradeDate")!=null)try{LocalDate.parse(q.getString("tradeDate"));}catch(Exception e){throw ApiException.forField("1001","VALIDATION_FAILED","Invalid tradeDate","tradeDate");}
        for(String field:List.of("institutionName","contactName"))if(q.get(field)!=null&&!(q.get(field) instanceof String))throw ApiException.forField("1001","VALIDATION_FAILED","Expected text",field);
        validateCounterparty(q,active);
        if(!"WAN".equals(q.getString("quantityUnit"))||!"CNY".equals(q.getString("currency")))throw ApiException.forField("1010","UNSUPPORTED","Prototype supports CNY / WAN",null);
        if(active){for(String f:List.of("securityId","side","quoteType","price","quantity","tradeDate","settlType"))RequestValidator.requireString(q,f);if("1".equals(q.getString("quoteType")))RequestValidator.requireString(q,"yieldType");}
    }
    void validateCounterparty(JSONObject quote,boolean active){
        for(String field:List.of("institutionId","institutionName","contactId","contactName")){
            Object value=quote.get(field);
            if(value!=null&&!(value instanceof String))throw ApiException.forField("1001","VALIDATION_FAILED","Expected text",field);
            if(value instanceof String text)quote.put(field,text.isBlank()?null:text.trim());
        }
        String institutionId=quote.getString("institutionId"),contactId=quote.getString("contactId");
        if(institutionId!=null){
            JSONObject institution=refs.records("INSTITUTION").stream().filter(record->institutionId.equals(record.getString("institutionId"))).findFirst().orElseThrow(()->ApiException.forField("1001","VALIDATION_FAILED","Unknown institution","institutionId"));
            quote.put("institutionName",institution.getString("institutionName"));
        }
        if(contactId!=null){
            JSONObject contact=refs.records("CONTACT").stream().filter(record->contactId.equals(record.getString("contactId"))).findFirst().orElseThrow(()->ApiException.forField("1001","VALIDATION_FAILED","Unknown contact","contactId"));
            if(!Objects.equals(institutionId,contact.getString("institutionId")))throw ApiException.forField("1001","VALIDATION_FAILED","Contact/institution mismatch","contactId");
            quote.put("contactName",contact.getString("contactName"));
        }
        if(active){
            if(quote.getString("institutionName")!=null&&institutionId==null)throw ApiException.forField("1001","UNRESOLVED_COUNTERPARTY","Select an institution ID before confirmation","institutionId");
            if(quote.getString("contactName")!=null&&contactId==null)throw ApiException.forField("1001","UNRESOLVED_COUNTERPARTY","Select a contact ID before confirmation","contactId");
        }
    }
    void committed(JSONObject event){lastSequence=event.getLongValue("sequence");publish.accept("ficc.quote.events",JsonUtils.object("Code","0000","Msg","成功","kind","EVENTS","epoch",store.epoch,"fromSequence",Long.toString(lastSequence),"toSequence",Long.toString(lastSequence),"events",JsonUtils.array(event)));}
    List<JSONObject> selectQuotes(String user, JSONObject request) {
        long started = QuotePerformanceTrace.searchStart();
        try {
            return search.queryQuotes(user, request);
        } finally {
            QuotePerformanceTrace.searchFinished(started);
        }
    }
    boolean matches(String user,JSONObject q,JSONObject f){return matcher.matches(user,q,f);}
    JSONArray referenceFor(List<JSONObject> qs){JSONArray a=new JSONArray();Set<String> seen=new HashSet<>();for(JSONObject q:qs)if(q.getString("securityId")!=null&&seen.add(q.getString("securityId")))a.add(refs.require(q.getString("securityId")));return a;}
    int limit(JSONObject r,int max){int n=r.getIntValue("limit",200);if(n<1||n>max)throw ApiException.forField("1008","RESOURCE_LIMIT","limit out of range","limit");return n;}
    Object page(Call c,String field,List<JSONObject> rows,JSONObject extra){
        if(rows.size()>10000)throw ApiException.forField("1008","RESOURCE_LIMIT","Prototype result cap: narrow the filter","filter");
        int n=limit(c.request,1000);return pageResult(new Cursor(c.user,c.session,c.topic,field,rows.stream().map(JsonUtils::deepCopy).toList(),0,n,System.currentTimeMillis()+120000,extra));
    }
    Object pageResult(Cursor cur){int end=Math.min(cur.offset+cur.limit,cur.rows.size());JSONObject d=JsonUtils.deepCopy(cur.extra);d.put(cur.field,cur.rows.subList(cur.offset,end));d.put("totalCount",cur.rows.size());String next=null;if(end<cur.rows.size()){cursors.entrySet().removeIf(e->e.getValue().expires<System.currentTimeMillis());if(cursors.size()>=64)throw ApiException.forField("1008","RESOURCE_LIMIT","Too many cursors",null);next=IdGenerator.newId("CUR-");cursors.put(next,new Cursor(cur.user,cur.session,cur.topic,cur.field,cur.rows,end,cur.limit,cur.expires,cur.extra));}d.put("nextCursor",next);return d;}
    Object continuePage(Call c){RequestValidator.allowOnlyFields(c.request,"requestId","cursor");Cursor cur=cursors.get(c.request.getString("cursor"));if(cur==null||cur.expires<System.currentTimeMillis()||!cur.user.equals(c.user)||!cur.session.equals(c.session)||!cur.topic.equals(c.topic))throw ApiException.forField("1007","RESET_REQUIRED","Cursor expired or belongs to a different query", "cursor");return pageResult(cur);}
    Object history(Call c)throws Exception{String id=RequestValidator.requireString(c.request,"quoteId");JSONObject q=get(id,c.user);if(!owns(c.user,q))throw ApiException.forField("1003","FORBIDDEN","History requires ownership",null);return page(c,"records",store.rows("SELECT payload FROM history WHERE quote_id=? ORDER BY revision DESC",id),JsonUtils.object("quoteId",id));}
    Object historySearch(Call c)throws Exception{return page(c,"rows",search.searchHistory(c.user,c.request),JsonUtils.object());}
    Object sourceQuery(Call c)throws Exception{String id=c.request.getString("sourceMessageId");List<JSONObject> a=id==null?store.rows("SELECT payload FROM sources"):store.rows("SELECT payload FROM sources WHERE id=?",id);return page(c,"sourceMessages",a.stream().filter(x->c.user.equals("U-ADMIN")||c.user.startsWith("S-")||c.user.equals(x.getString("ownerUserId"))).toList(),JsonUtils.object());}
    String settingId(JSONObject g){return g.getString(g.containsKey("watchlistId")?"watchlistId":"filterGroupId");}
    Object groupQuery(Call c,String kind){String key=kind+"Id";return JsonUtils.object(kind.equals("watchlist")?"watchlists":"filterGroups",settings.values().stream().filter(g->g.containsKey(key)&&c.user.equals(g.getString("userId"))).filter(g->c.request.getString(key)==null||c.request.getString(key).equals(g.getString(key))).filter(g->c.request.getString("market")==null||c.request.getString("market").equals(g.getString("market"))).filter(g->c.request.getString("scope")==null||c.request.getString("scope").equals(g.getString("scope"))).toList());}
    Object saveGroup(Call c,String kind,boolean delete)throws Exception{
        String rid=RequestValidator.requireString(c.request,"requestId"),key=kind+"Id";JSONArray receipt=store.receipt(c.user,c.topic,rid,JsonUtils.canonicalHash(c.request));if(!receipt.isEmpty())return receipt.getJSONObject(0);
        String id=c.request.getString(key);if(delete)RequestValidator.requireString(c.request,key);JSONObject prev=id==null?null:settings.get(id);
        if(id!=null&&(prev==null||!c.user.equals(prev.getString("userId"))))throw ApiException.forField("1002","NOT_FOUND","Group not found",key);
        if(prev!=null&&!RequestValidator.requireString(c.request,"expectedRevision").equals(prev.getString("revision")))throw ApiException.forField("1004","REVISION_CONFLICT","Group changed","expectedRevision");
        JSONObject g=JsonUtils.deepCopy(c.request);g.remove("requestId");g.remove("expectedRevision");if(id==null)id=IdGenerator.newId(kind.equals("watchlist")?"W-":"FG-");
        g.put(key,id);g.put("userId",c.user);g.put("revision",Long.toString(prev==null?1:prev.getLongValue("revision")+1));g.put("createTime",prev==null?BusinessClock.currentTime():prev.getString("createTime"));g.put("updateTime",BusinessClock.currentTime());
        if(!delete){RequestValidator.requireString(g,"name");if(!Set.of("RATE","CREDIT").contains(RequestValidator.requireString(g,"market"))||!Set.of("MARKET","HISTORY").contains(RequestValidator.requireString(g,"scope")))throw ApiException.forField("1001","VALIDATION_FAILED","Invalid group scope",null);
            if(kind.equals("watchlist")){JSONArray a=g.getJSONArray("securities");if(a==null)throw ApiException.forField("1001","VALIDATION_FAILED","securities required","securities");Set<String> seen=new HashSet<>();for(JSONObject x:a.toJavaList(JSONObject.class)){refs.require(RequestValidator.requireString(x,"securityId"));if(!seen.add(x.getString("securityId")))throw ApiException.forField("1001","VALIDATION_FAILED","Duplicate bond","securityId");x.putIfAbsent("displayOrder",seen.size()-1);}}
            else{JSONArray subs=g.getJSONArray("subgroups");if(subs==null)throw ApiException.forField("1001","VALIDATION_FAILED","subgroups required","subgroups");JSONArray clean=new JSONArray();for(JSONObject sg:subs.toJavaList(JSONObject.class)){JSONArray cs=sg.getJSONArray("conditions");if(cs==null||cs.isEmpty())continue;FilterEngine.validate(JsonUtils.object("conditions",cs));sg.putIfAbsent("subgroupId",IdGenerator.newId("SG-"));int i=0;for(JSONObject cond:cs.toJavaList(JSONObject.class)){cond.putIfAbsent("conditionId",IdGenerator.newId("C-"));cond.putIfAbsent("displayOrder",i++);}clean.add(sg);}g.put("subgroups",clean);}
        }
        JSONObject out=delete?JsonUtils.object(key,id,"deleted",true):JsonUtils.object(kind,g),event;
        try{store.begin();if(delete)store.update("DELETE FROM settings WHERE id=?",id);else store.update("INSERT INTO settings VALUES (?,?,?,?,?::jsonb) ON CONFLICT(id) DO UPDATE SET revision=excluded.revision,payload=excluded.payload",id,c.user,kind,g.getLongValue("revision"),JsonUtils.toJson(g));event=store.event(JsonUtils.object("type",kind.equals("watchlist")?"WATCHLIST":"FILTER_GROUP","action",delete?"REMOVE":"UPSERT","id",id,"record",delete?null:g,"userId",c.user));store.receipt(c.user,c.topic,rid,JsonUtils.array(out));store.commit();}catch(Exception e){store.rollback();throw e;}
        if(delete)settings.remove(id);else settings.put(id,g);committed(event);views.settingsChanged(c.user,kind,id,g.getString("revision"),delete);return out;
    }
    Object state(Call c){internal(c);if(c.request.getString("cursor")!=null){JSONObject d=(JSONObject)continuePage(c);finishState(d);return d;}List<JSONObject> entries=new ArrayList<>();quotes.values().forEach(q->entries.add(JsonUtils.object("collection","quotes","record",q)));settings.values().forEach(g->entries.add(JsonUtils.object("collection",g.containsKey("watchlistId")?"watchlists":"filterGroups","record",g)));JSONObject d=(JSONObject)page(c,"entries",entries,JsonUtils.object("epoch",store.epoch,"sequence",Long.toString(lastSequence)));finishState(d);return d;}
    void finishState(JSONObject d){JSONObject records=JsonUtils.object("quotes",JsonUtils.array(),"watchlists",JsonUtils.array(),"filterGroups",JsonUtils.array());for(Object x:(List<?>)d.remove("entries")){JSONObject e=(JSONObject)x;records.getJSONArray(e.getString("collection")).add(e.get("record"));}d.put("records",records);d.put("done",d.get("nextCursor")==null);}
    Object events(Call c)throws Exception{internal(c);String epoch=RequestValidator.requireString(c.request,"epoch");long after=Long.parseLong(RequestValidator.requireString(c.request,"afterSequence"));if(!epoch.equals(store.epoch)||after<0||after>lastSequence)throw ApiException.forField("1007","RESET_REQUIRED","Event position invalid",null);int n=limit(c.request,1000);List<JSONObject> es=store.rows("SELECT payload FROM events WHERE sequence>? ORDER BY sequence LIMIT ?",after,n);long next=es.isEmpty()?after:es.get(es.size()-1).getLongValue("sequence");return JsonUtils.object("epoch",epoch,"events",es,"nextSequence",Long.toString(next),"seenSequence",Long.toString(lastSequence),"hasMore",next<lastSequence);}
    void internal(Call c){if(!c.user.startsWith("S-"))throw ApiException.forField("1003","FORBIDDEN","Internal service identity required",null);}
    Object reference(Call c,boolean query){String kind=RequestValidator.requireString(c.request,"kind");List<JSONObject> all=refs.records(kind);String key=switch(kind){case "SECURITY","DAILY_VALUATION"->"securityId";case "ISSUER"->"issuerId";case "CONTACT"->"contactId";default->"institutionId";};if(query){String text=JsonUtils.getStringOrDefault(c.request,"text","");return page(c,"records",all.stream().filter(x->JsonUtils.toJson(x).contains(text)).toList(),JsonUtils.object());}JSONArray ids=c.request.getJSONArray("ids");if(ids==null)throw ApiException.forField("1001","VALIDATION_FAILED","ids required","ids");List<JSONObject> records=all.stream().filter(x->ids.contains(x.getString(key))).toList();return JsonUtils.object("records",records,"missingIds",ids.stream().filter(id->records.stream().noneMatch(x->id.equals(x.getString(key)))).toList(),"referenceVersion","SIM-1");}
    Object dictionary(boolean price,JSONObject r){JSONObject d=JsonUtils.object("side",JsonUtils.array(JsonUtils.object("value","1","label","买入"),JsonUtils.object("value","2","label","卖出")),"quoteType",JsonUtils.array(JsonUtils.object("value","1","label","收益率"),JsonUtils.object("value","2","label","净价"),JsonUtils.object("value","3","label","全价")),"yieldType",JsonUtils.array(JsonUtils.object("value","1","label","到期"),JsonUtils.object("value","2","label","行权")),"settlType",JsonUtils.array(JsonUtils.object("value","1","label","T+0"),JsonUtils.object("value","2","label","T+1")),"market",JsonUtils.array(JsonUtils.object("value","RATE","label","利率"),JsonUtils.object("value","CREDIT","label","信用")));if(price)d.put("mode",JsonUtils.array(JsonUtils.object("value","BEST","label","最优"),JsonUtils.object("value","FULL","label","全部"),JsonUtils.object("value","DEPTH","label","档数")));if(r.getJSONArray("names")!=null){Set<String> names=new HashSet<>(r.getJSONArray("names").toJavaList(String.class));d.keySet().retainAll(names);}return JsonUtils.object("dictionaries",d,"filterFields",FilterEngine.catalog(price));}
    Object intake(Call c)throws Exception{return Intake.apply(this,c);}
}
