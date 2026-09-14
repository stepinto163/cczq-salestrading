package ficc.notification;

import ficc.common.*;

import com.alibaba.fastjson2.*;
import java.util.*;
import java.util.function.BiConsumer;
import java.math.BigDecimal;

/** Notification domain. Upstream quote facts and price books are separate inputs. */
final class NotificationService {
    static final List<String> SWITCHES=List.of("watchlistEnabled","filterGroupEnabled","counterQuoteEnabled","myBestEnabled");
    final JdbcStore store;final ReferenceData refs=new ReferenceData();
    final Map<String,JSONObject> quotes=new LinkedHashMap<>(),marketSettings=new LinkedHashMap<>(),books=new LinkedHashMap<>();
    final Map<String,JSONObject> notifications=new LinkedHashMap<>(),preferences=new LinkedHashMap<>(),groups=new LinkedHashMap<>();
    final Map<String,NotifyStream> streams=new LinkedHashMap<>();final Map<String,Page> pages=new LinkedHashMap<>();
    BiConsumer<String,JSONObject> publish=(t,m)->{};
    String quoteEpoch,priceStream;long quoteSequence,priceSequence,lastSequence;boolean ready;
    static class NotifyStream {String user,sid,id;long sequence;Map<String,NotifyView> views=new LinkedHashMap<>();Set<String> used=new HashSet<>();}
    static class NotifyView {String id;JSONObject request;int limit;List<String> ids=new ArrayList<>();Map<String,JSONObject> loaded=new LinkedHashMap<>();}
    record Page(String user,String sid,String topic,String field,List<JSONObject> rows,int offset,int limit,long expires,JSONObject extra){}
    NotificationService(JdbcStore store) throws Exception {
        this.store = store;
        store.exec("CREATE TABLE IF NOT EXISTS notifications"
                + " (id text PRIMARY KEY, user_id text NOT NULL, active boolean NOT NULL, payload jsonb NOT NULL)");
        store.exec("CREATE INDEX IF NOT EXISTS notification_user_idx ON notifications(user_id, active)");
        store.exec("CREATE TABLE IF NOT EXISTS notification_preferences"
                + " (user_id text PRIMARY KEY, payload jsonb NOT NULL)");
        store.exec("CREATE TABLE IF NOT EXISTS notification_reads"
                + " (id text, content_token text, read_time text, PRIMARY KEY(id, content_token))");
        for (JSONObject notification : store.rows("SELECT payload FROM notifications")) {
            notifications.put(notification.getString("notificationId"), notification);
        }
        for (JSONObject preference : store.rows("SELECT payload FROM notification_preferences")) {
            preferences.put(preference.getString("userId"), preference);
        }
        for (JSONObject group : store.rows("SELECT payload FROM " + store.table("settings"))) {
            groups.put(group.getString("filterGroupId"), group);
        }
        lastSequence = store.tail();
    }

    synchronized void replaceQuotes(JSONObject state,List<JSONObject> allQuotes,List<JSONObject> settings){
        quotes.clear();marketSettings.clear();for(JSONObject q:allQuotes)quotes.put(q.getString("quoteId"),q);for(JSONObject g:settings)marketSettings.put(g.getString(g.containsKey("watchlistId")?"watchlistId":"filterGroupId"),g);
        quoteEpoch=state.getString("epoch");quoteSequence=state.getLongValue("sequence");ready=true;
    }
    synchronized boolean quoteMessage(JSONObject message)throws Exception{
        if(!Objects.equals(quoteEpoch,message.getString("epoch")))return false;
        if("HEARTBEAT".equals(message.getString("kind")))return message.getLongValue("currentSequence")<=quoteSequence;
        for(JSONObject e:message.getJSONArray("events").toJavaList(JSONObject.class)){
            long seq=e.getLongValue("sequence");if(seq<=quoteSequence)continue;if(seq!=quoteSequence+1)return false;
            if("QUOTE".equals(e.getString("type")))quotes.put(e.getString("quoteId"),e.getJSONObject("quote"));
            else {String id=e.getString("id");if("REMOVE".equals(e.getString("action")))marketSettings.remove(id);else marketSettings.put(id,e.getJSONObject("record"));}
            quoteSequence=seq;
        }
        recompute();return true;
    }
    synchronized boolean priceMessage(JSONObject message)throws Exception{
        String kind=message.getString("kind");if("RESET_REQUIRED".equals(kind)){books.clear();recompute();return false;}
        boolean initial="SNAPSHOT".equals(kind);String stream=message.getString("streamId");long seq=message.getLongValue("sequence");
        if(!initial&&(!Objects.equals(stream,priceStream)||seq>priceSequence+1))return false;
        if(!initial&&seq<=priceSequence)return true;
        if(initial)books.clear();priceStream=stream;priceSequence=seq;
        for(JSONObject b:message.getJSONArray("prices").toJavaList(JSONObject.class))books.put(b.getString("securityId"),b);
        recompute();return true;
    }
    JSONObject preference(String user){return preferences.getOrDefault(user,JsonUtils.object("userId",user,"revision","0","watchlistEnabled",true,"filterGroupEnabled",true,"counterQuoteEnabled",true,"myBestEnabled",true,"updateTime",null));}
    boolean bookMatchesQuotes(String sec,List<JSONObject> active){
        JSONObject book=books.get(sec);if(book==null)return false;JSONObject refs=book.getJSONObject("quoteRefs");if(refs==null||refs.size()!=active.size())return false;
        for(JSONObject q:active){JSONObject link=refs.getJSONObject("FICC:"+q.getString("quoteId"));if(link==null||!q.getString("revision").equals(link.getString("quoteRevision")))return false;}return true;
    }
    Set<String> bestIds(String sec,List<JSONObject> active){
        Set<String> ids=new LinkedHashSet<>();if(!bookMatchesQuotes(sec,active))return ids;
        Map<String,BigDecimal> first=new HashMap<>();
        // QuoteSvr already sorted the complete book. Preserve its side order and all ties.
        for(JSONObject entry:books.get(sec).getJSONObject("marketData").getJSONArray("NoMDEntries").toJavaList(JSONObject.class)){
            String side=entry.getString("MDEntryType");BigDecimal px=new BigDecimal(entry.getString("MDEntryPx"));first.putIfAbsent(side,px);
            if(first.get(side).compareTo(px)==0)ids.add(entry.getString("MDEntryID").substring("FICC:".length()));
        }return ids;
    }
    synchronized void recompute()throws Exception{
        if(!ready)return;
        Map<String,List<JSONObject>> byBond=new TreeMap<>();for(JSONObject q:quotes.values())if("ACTIVE".equals(q.getString("status")))byBond.computeIfAbsent(q.getString("securityId"),k->new ArrayList<>()).add(q);
        Map<String,JSONObject> desired=new LinkedHashMap<>();
        for(String user:List.of("U-001","U-002","U-ADMIN")){
            JSONObject pref=preference(user);
            for(var bond:byBond.entrySet()){
                String sec=bond.getKey();List<JSONObject> active=bond.getValue();JSONArray reasons=new JSONArray();Set<String> links=new TreeSet<>();
                for(String kind:List.of("watchlist","filterGroup"))if(pref.getBooleanValue(kind+"Enabled")){
                    JSONArray matched=new JSONArray();
                    for(JSONObject group:marketSettings.values()){
                        String key=kind+"Id";if(!group.containsKey(key)||!user.equals(group.getString("userId"))||!"MARKET".equals(group.getString("scope"))||!Objects.equals(group.getString("market"),refs.require(sec).getString("market")))continue;
                        JSONArray cs=new JSONArray();if(kind.equals("filterGroup"))for(JSONObject sub:group.getJSONArray("subgroups").toJavaList(JSONObject.class))cs.addAll(sub.getJSONArray("conditions"));
                        boolean hit=false;for(JSONObject q:active){boolean match=kind.equals("watchlist")?group.getJSONArray("securities").stream().anyMatch(x->sec.equals(((JSONObject)x).getString("securityId"))):FilterEngine.match(refs.enrich(q),JsonUtils.object("conditions",cs));if(match){links.add(q.getString("quoteId"));hit=true;}}
                        if(hit)matched.add(group.getString(key));
                    }
                    if(!matched.isEmpty())reasons.add(JsonUtils.object("type",kind.equals("watchlist")?"WATCHLIST":"FILTER_GROUP","groupIds",matched,"text",kind.equals("watchlist")?"命中关注债券分组":"命中行情筛选分组"));
                }
                if(pref.getBooleanValue("myBestEnabled")){
                    Set<String> best=bestIds(sec,active);List<String> mine=active.stream().filter(q->user.equals(q.getString("ownerUserId"))&&best.contains(q.getString("quoteId"))).map(q->q.getString("quoteId")).sorted().toList();
                    if(!mine.isEmpty()){links.addAll(mine);reasons.add(JsonUtils.object("type","MY_BEST","quoteIds",mine,"text","我的报价位于同方向最优档（含并列）"));}
                }
                if(!reasons.isEmpty())putDesired(desired,user,sec,"NORMAL",reasons,links);
                if(pref.getBooleanValue("counterQuoteEnabled"))for(JSONObject mine:active)if(user.equals(mine.getString("ownerUserId")))for(JSONObject other:active)if(!user.equals(other.getString("ownerUserId"))&&!Objects.equals(mine.getString("side"),other.getString("side"))){
                    String a=mine.getString("quoteId"),b=other.getString("quoteId");putDesired(desired,user,sec,"COUNTER:"+a+":"+b,JsonUtils.array(JsonUtils.object("type","COUNTER_QUOTE","ownQuoteId",a,"otherQuoteId",b,"text","同券、反方向的本人及他人有效报价")),new TreeSet<>(List.of(a,b)));
                }
            }
        }
        List<JSONObject> events=new ArrayList<>();Map<String,JSONObject> changes=new LinkedHashMap<>();
        try{store.begin();
            for(var entry:desired.entrySet()){
                JSONObject candidate=entry.getValue(),prev=notifications.get(entry.getKey());
                if(prev!=null&&prev.getBooleanValue("active")&&Objects.equals(candidate.getString("contentToken"),prev.getString("contentToken"))&&JsonUtils.canonicalHash(candidate.get("reasons")).equals(JsonUtils.canonicalHash(prev.get("reasons"))))continue;
                candidate.put("createTime",prev==null?BusinessClock.currentTime():prev.getString("createTime"));candidate.put("updateTime",BusinessClock.currentTime());candidate.put("active",true);
                candidate.put("readTime",store.scalar("SELECT read_time FROM notification_reads WHERE id=? AND content_token=?",entry.getKey(),candidate.getString("contentToken")));
                save(candidate);changes.put(entry.getKey(),candidate);events.add(event("NOTIFICATION","UPSERT",entry.getKey(),visible(candidate)));
            }
            for(JSONObject prev:notifications.values())if(prev.getBooleanValue("active")&&!desired.containsKey(prev.getString("notificationId"))){JSONObject n=JsonUtils.deepCopy(prev);n.put("active",false);save(n);changes.put(n.getString("notificationId"),n);events.add(event("NOTIFICATION","REMOVE",n.getString("notificationId"),null));}
            store.commit();
        }catch(Exception e){store.rollback();throw e;}
        notifications.putAll(changes);if(!events.isEmpty()){committed(events);pushViews(null);}
    }
    void putDesired(Map<String,JSONObject> desired,String user,String sec,String key,JSONArray reasons,Set<String> ids){
        Map<String,String> versions=new TreeMap<>();for(String q:ids)versions.put(q,quotes.get(q).getString("revision"));String token=JsonUtils.canonicalHash(versions);String id="N-"+JsonUtils.canonicalHash(List.of(user,sec,key,token)).substring(0,32);
        desired.put(id,JsonUtils.object("notificationId",id,"userId",user,"securityId",sec,"reasons",reasons,"content",String.join("；",reasons.toJavaList(JSONObject.class).stream().map(r->r.getString("text")).toList()),"quoteIds",new ArrayList<>(ids),"contentToken",token,"alertKey",id+":"+token));
    }
    JSONObject visible(JSONObject n){JSONObject out=JsonUtils.deepCopy(n);out.remove("active");out.remove("contentToken");return out;}
    void save(JSONObject n)throws Exception{store.update("INSERT INTO notifications VALUES (?,?,?,?::jsonb) ON CONFLICT(id) DO UPDATE SET active=excluded.active,payload=excluded.payload",n.getString("notificationId"),n.getString("userId"),n.getBooleanValue("active"),JsonUtils.toJson(n));}
    JSONObject event(String type,String action,String id,JSONObject record)throws Exception{return store.event(JsonUtils.object("type",type,"action",action,"id",id,"record",record));}
    void committed(List<JSONObject> events){if(events.isEmpty())return;long first=events.get(0).getLongValue("sequence");lastSequence=events.get(events.size()-1).getLongValue("sequence");publish.accept("ficc.notification.events",JsonUtils.object("Code","0000","Msg","成功","kind","EVENTS","epoch",store.epoch,"fromSequence",Long.toString(first),"toSequence",Long.toString(lastSequence),"events",events));}
    synchronized JSONObject handle(String user,String sid,String topic,JSONObject request){
        String rid=request.getString("requestId");try{
            RequestValidator.requireString(request,"requestId");if(user==null)throw ApiException.forField("1003","FORBIDDEN","Unknown login session",null);
            String op=topic.replace("ficc.notification.","");Object data;
            if(request.getString("cursor")!=null)data=continuePage(user,sid,topic,request.getString("cursor"));
            else data=switch(op){
                case "preference.get"->JsonUtils.object("preference",preference(user));
                case "preference.save"->savePreference(user,topic,request);
                case "read"->read(user,topic,request);
                case "query"->page(user,sid,topic,"notifications",select(user,request),request,JsonUtils.object());
                case "stream.open"->{NotifyStream s=stream(user,sid);yield JsonUtils.object("sessionId",sid,"streamId",s.id,"pushTopic","ficc.notification.report."+sid);}
                case "view.open"->openView(user,sid,request);
                case "view.extend"->extendView(user,sid,request);
                case "view.close"->{NotifyStream s=stream(user,sid);String id=RequestValidator.requireString(request,"viewId");s.views.remove(id);yield JsonUtils.object("viewId",id,"closed",true);}
                case "filterGroup.query"->JsonUtils.object("filterGroups",groups.values().stream().filter(g->user.equals(g.getString("userId"))).toList());
                case "filterGroup.save","filterGroup.delete"->saveGroup(user,topic,request,op.endsWith("delete"));
                case "state.query"->state(user,sid,topic,request);
                case "event.query"->events(user,request);
                default->throw ApiException.forField("1010","UNSUPPORTED","Unknown notification interface",null);
            };return ApiResponse.success(rid,data);
        }catch(ApiException e){return ApiResponse.failure(rid,e);}catch(Exception e){store.rollback();System.err.println("Notification request failed: "+topic+" "+e);return ApiResponse.failure(rid,ApiException.forField("1099","INTERNAL_ERROR","Notification operation failed",null));}
    }
    List<JSONObject> select(String user,JSONObject r){
        if(!ready)throw ApiException.forField("1009","NOT_READY","Quote source is synchronizing",null);
        String read=JsonUtils.getStringOrDefault(r,"readState","ALL");if(!Set.of("ALL","READ","UNREAD").contains(read))throw ApiException.forField("1001","VALIDATION_FAILED","Invalid readState",null);JSONObject filter=r.getJSONObject("filter");FilterEngine.validate(filter);
        return notifications.values().stream().filter(n->n.getBooleanValue("active")&&user.equals(n.getString("userId"))).filter(n->read.equals("ALL")||(read.equals("READ")==(n.get("readTime")!=null))).filter(n->matches(user,n,filter)).sorted(Comparator.<JSONObject,String>comparing(n->n.getString("updateTime")).reversed().thenComparing(n->n.getString("notificationId"))).map(this::visible).toList();
    }
    boolean matches(String user,JSONObject n,JSONObject f){
        List<JSONObject> related=n.getJSONArray("quoteIds").toJavaList(String.class).stream().map(quotes::get).filter(Objects::nonNull).toList();
        if(related.stream().noneMatch(q->FilterEngine.match(refs.enrich(q),f)))return false;
        if(f!=null&&f.getJSONArray("filterGroupIds")!=null&&!f.getJSONArray("filterGroupIds").isEmpty()){
            boolean any=false;for(String id:f.getJSONArray("filterGroupIds").toJavaList(String.class)){JSONObject g=groups.get(id);if(g==null||!user.equals(g.getString("userId")))throw ApiException.forField("1002","NOT_FOUND","Notification filter group not found",null);JSONArray cs=new JSONArray();for(JSONObject sub:g.getJSONArray("subgroups").toJavaList(JSONObject.class))cs.addAll(sub.getJSONArray("conditions"));any|=related.stream().anyMatch(q->FilterEngine.match(refs.enrich(q),JsonUtils.object("market",g.getString("market"),"conditions",cs)));}return any;
        }return true;
    }
    Object savePreference(String user,String topic,JSONObject r)throws Exception{
        RequestValidator.allowOnlyFields(r,"requestId","expectedRevision","watchlistEnabled","filterGroupEnabled","counterQuoteEnabled","myBestEnabled");JSONArray prior=store.receipt(user,topic,r.getString("requestId"),JsonUtils.canonicalHash(r));if(!prior.isEmpty())return prior.get(0);
        JSONObject prev=preference(user);if(!RequestValidator.requireString(r,"expectedRevision").equals(prev.getString("revision")))throw ApiException.forField("1004","REVISION_CONFLICT","Preferences changed",null);
        JSONObject p=JsonUtils.object("userId",user,"revision",Long.toString(prev.getLongValue("revision")+1),"updateTime",BusinessClock.currentTime());for(String key:SWITCHES){if(!(r.get(key) instanceof Boolean))throw ApiException.forField("1001","VALIDATION_FAILED","Boolean required",key);p.put(key,r.get(key));}
        JSONObject out=JsonUtils.object("preference",p),event;try{store.begin();store.update("INSERT INTO notification_preferences VALUES (?,?::jsonb) ON CONFLICT(user_id) DO UPDATE SET payload=excluded.payload",user,JsonUtils.toJson(p));event=event("PREFERENCE","UPSERT",user,p);store.receipt(user,topic,r.getString("requestId"),JsonUtils.array(out));store.commit();}catch(Exception e){store.rollback();throw e;}
        preferences.put(user,p);committed(List.of(event));recompute();pushViews(p);return out;
    }
    Object read(String user,String topic,JSONObject r)throws Exception{
        RequestValidator.allowOnlyFields(r,"requestId","notificationIds","read");if(!(r.get("read") instanceof Boolean))throw ApiException.forField("1001","VALIDATION_FAILED","read must be boolean",null);JSONArray ids=r.getJSONArray("notificationIds");if(ids==null||ids.isEmpty()||ids.size()>200)throw ApiException.forField("1008","RESOURCE_LIMIT","1..200 notification IDs required",null);
        JSONArray prior=store.receipt(user,topic,r.getString("requestId"),JsonUtils.canonicalHash(r));if(!prior.isEmpty())return prior.get(0);
        JSONArray results=new JSONArray();Map<String,JSONObject> changed=new LinkedHashMap<>();List<JSONObject> events=new ArrayList<>();
        try{store.begin();for(String id:ids.toJavaList(String.class)){
            JSONObject before=notifications.get(id);if(before==null||!user.equals(before.getString("userId"))||!before.getBooleanValue("active")){results.add(JsonUtils.object("notificationId",id,"Code","1002","Msg","通知不存在"));continue;}
            JSONObject n=JsonUtils.deepCopy(before);String time=r.getBooleanValue("read")?BusinessClock.currentTime():null;n.put("readTime",time);n.put("updateTime",BusinessClock.currentTime());save(n);
            store.update("INSERT INTO notification_reads VALUES (?,?,?) ON CONFLICT(id,content_token) DO UPDATE SET read_time=excluded.read_time",id,n.getString("contentToken"),time);
            changed.put(id,n);events.add(event("NOTIFICATION","UPSERT",id,visible(n)));results.add(JsonUtils.object("notificationId",id,"Code","0000","Msg","成功","readTime",time));
        }JSONObject out=JsonUtils.object("results",results);store.receipt(user,topic,r.getString("requestId"),JsonUtils.array(out));store.commit();notifications.putAll(changed);committed(events);pushViews(null);return out;
        }catch(Exception e){store.rollback();throw e;}
    }
    Object saveGroup(String user, String topic, JSONObject request, boolean delete) throws Exception {
        JSONArray prior = store.receipt(user, topic, request.getString("requestId"), JsonUtils.canonicalHash(request));
        if (!prior.isEmpty()) {
            return prior.get(0);
        }
        String id = request.getString("filterGroupId");
        if (delete) {
            RequestValidator.requireString(request, "filterGroupId");
        }
        JSONObject previous = id == null ? null : groups.get(id);
        if (id != null && (previous == null || !user.equals(previous.getString("userId")))) {
            throw ApiException.forField("1002", "NOT_FOUND", "Filter group not found", null);
        }
        if (previous != null
                && !RequestValidator.requireString(request, "expectedRevision").equals(previous.getString("revision"))) {
            throw ApiException.forField("1004", "REVISION_CONFLICT", "Filter group changed", null);
        }

        JSONObject group = JsonUtils.deepCopy(request);
        group.remove("requestId");
        group.remove("expectedRevision");
        if (id == null) {
            id = IdGenerator.newId("NFG-");
        }
        group.put("filterGroupId", id);
        group.put("userId", user);
        group.put("scope", "NOTIFICATION");
        group.put("revision", Long.toString(previous == null ? 1 : previous.getLongValue("revision") + 1));
        group.put("createTime", previous == null ? BusinessClock.currentTime() : previous.getString("createTime"));
        group.put("updateTime", BusinessClock.currentTime());
        if (!delete) {
            RequestValidator.requireString(group, "name");
            if (!Set.of("RATE", "CREDIT").contains(RequestValidator.requireString(group, "market"))) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid market", null);
            }
            JSONArray subgroups = group.getJSONArray("subgroups");
            if (subgroups == null) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "subgroups required", null);
            }
            JSONArray cleanSubgroups = new JSONArray();
            for (JSONObject subgroup : subgroups.toJavaList(JSONObject.class)) {
                JSONArray conditions = subgroup.getJSONArray("conditions");
                if (conditions == null || conditions.isEmpty()) {
                    continue;
                }
                FilterEngine.validate(JsonUtils.object("conditions", conditions));
                subgroup.putIfAbsent("subgroupId", IdGenerator.newId("NSG-"));
                cleanSubgroups.add(subgroup);
            }
            group.put("subgroups", cleanSubgroups);
        }
        JSONObject result = delete
                ? JsonUtils.object("filterGroupId", id, "deleted", true)
                : JsonUtils.object("filterGroup", group);
        JSONObject changeEvent;
        try {
            store.begin();
            if (delete) {
                store.update("DELETE FROM " + store.table("settings") + " WHERE id=?", id);
            } else {
                store.update("INSERT INTO " + store.table("settings")
                                + " VALUES (?, ?, ?, ?, ?::jsonb) ON CONFLICT(id)"
                                + " DO UPDATE SET revision=excluded.revision, payload=excluded.payload",
                        id, user, "filterGroup", group.getLongValue("revision"), JsonUtils.toJson(group));
            }
            changeEvent = event("FILTER_GROUP", delete ? "REMOVE" : "UPSERT", id, delete ? null : group);
            store.receipt(user, topic, request.getString("requestId"), JsonUtils.array(result));
            store.commit();
        } catch (Exception error) {
            store.rollback();
            throw error;
        }
        if (delete) {
            groups.remove(id);
        } else {
            groups.put(id, group);
        }
        committed(List.of(changeEvent));
        pushViews(null);
        return result;
    }

    NotifyStream stream(String user,String sid){return streams.computeIfAbsent(sid,k->{NotifyStream s=new NotifyStream();s.user=user;s.sid=sid;s.id=IdGenerator.newId("NE-");return s;});}
    int limit(JSONObject r){int n=r.getIntValue("limit",200);if(n<1||n>1000)throw ApiException.forField("1008","RESOURCE_LIMIT","limit must be 1..1000",null);return n;}
    Object openView(String user,String sid,JSONObject r){RequestValidator.allowOnlyFields(r,"requestId","viewId","readState","filter","limit");NotifyStream s=stream(user,sid);String id=RequestValidator.requireString(r,"viewId");if(s.used.contains(id)||s.views.size()>=20)throw ApiException.forField("1008","RESOURCE_LIMIT","View exists or limit reached",null);NotifyView v=new NotifyView();v.id=id;v.request=JsonUtils.deepCopy(r);v.limit=limit(r);refresh(s,v);s.used.add(id);s.views.put(id,v);return viewResult(s,v);}
    Object extendView(String user,String sid,JSONObject r){NotifyStream s=stream(user,sid);NotifyView v=s.views.get(RequestValidator.requireString(r,"viewId"));if(v==null)throw ApiException.forField("1002","NOT_FOUND","View not found",null);int n=limit(r);if(n<=v.limit)throw ApiException.forField("1001","VALIDATION_FAILED","limit must increase",null);v.limit=n;refresh(s,v);return viewResult(s,v);}
    void refresh(NotifyStream s,NotifyView v){v.loaded=new LinkedHashMap<>();for(JSONObject n:select(s.user,v.request).stream().limit(v.limit).toList())v.loaded.put(n.getString("notificationId"),n);v.ids=new ArrayList<>(v.loaded.keySet());}
    JSONObject viewResult(NotifyStream s,NotifyView v){return JsonUtils.object("streamId",s.id,"sequence",Long.toString(s.sequence),"view",JsonUtils.object("viewId",v.id,"limit",v.limit,"totalCount",select(s.user,v.request).size(),"ids",v.ids),"notifications",new ArrayList<>(v.loaded.values()));}
    void pushViews(JSONObject preference){for(NotifyStream s:streams.values()){
        JSONArray views=new JSONArray();Map<String,JSONObject> changed=new LinkedHashMap<>();
        for(NotifyView v:s.views.values())try{
            List<String> old=v.ids;Map<String,JSONObject> prev=v.loaded;refresh(s,v);List<String> removed=old.stream().filter(id->!v.loaded.containsKey(id)).toList();List<String> working=new ArrayList<>(old);working.removeAll(removed);JSONArray place=new JSONArray();
            for(int i=0;i<v.ids.size();i++){String id=v.ids.get(i);if(i>=working.size()||!id.equals(working.get(i))){working.remove(id);working.add(i,id);place.add(JsonUtils.object("id",id,"index",i));}if(!JsonUtils.canonicalHash(v.loaded.get(id)).equals(JsonUtils.canonicalHash(prev.get(id))))changed.put(id,v.loaded.get(id));}
            views.add(JsonUtils.object("viewId",v.id,"totalCount",select(s.user,v.request).size(),"removeIds",removed,"place",place));
        }catch(ApiException e){publish.accept("ficc.notification.report."+s.sid,JsonUtils.object("Code","1007","kind","RESET_REQUIRED","streamId",s.id,"sequence",Long.toString(++s.sequence)));}
        if(views.isEmpty()&&(preference==null||!s.user.equals(preference.getString("userId"))))continue;
        JSONObject out=JsonUtils.object("Code","0000","Msg","成功","kind","UPDATE","streamId",s.id,"sequence",Long.toString(++s.sequence),"notifications",new ArrayList<>(changed.values()),"views",views);
        if(preference!=null&&s.user.equals(preference.getString("userId")))out.put("preference",preference);publish.accept("ficc.notification.report."+s.sid,out);
    }}
    synchronized void heartbeat(){for(NotifyStream s:streams.values())publish.accept("ficc.notification.report."+s.sid,JsonUtils.object("Code","0000","kind","HEARTBEAT","streamId",s.id,"sequence",Long.toString(++s.sequence)));}
    Object page(String user,String sid,String topic,String field,List<JSONObject> rows,JSONObject r,JSONObject extra){return pageResult(new Page(user,sid,topic,field,rows.stream().map(JsonUtils::deepCopy).toList(),0,limit(r),System.currentTimeMillis()+120000,extra));}
    Object pageResult(Page p){int end=Math.min(p.rows.size(),p.offset+p.limit);JSONObject result=JsonUtils.deepCopy(p.extra);result.put(p.field,p.rows.subList(p.offset,end));String cursor=null;if(end<p.rows.size()){pages.entrySet().removeIf(e->e.getValue().expires<System.currentTimeMillis());if(pages.size()>=64)throw ApiException.forField("1008","RESOURCE_LIMIT","Too many cursors",null);cursor=IdGenerator.newId("NC-");pages.put(cursor,new Page(p.user,p.sid,p.topic,p.field,p.rows,end,p.limit,p.expires,p.extra));}result.put("nextCursor",cursor);result.put("totalCount",p.rows.size());if(p.topic.endsWith("state.query"))finishState(result);return result;}
    Object continuePage(String user,String sid,String topic,String id){Page p=pages.get(id);if(p==null||!p.user.equals(user)||!p.sid.equals(sid)||!p.topic.equals(topic)||p.expires<System.currentTimeMillis())throw ApiException.forField("1007","RESET_REQUIRED","Cursor expired",null);return pageResult(p);}
    Object state(String user,String sid,String topic,JSONObject r){internal(user);List<JSONObject> rows=new ArrayList<>();notifications.values().stream().filter(n->n.getBooleanValue("active")).forEach(n->rows.add(JsonUtils.object("collection","notifications","record",visible(n))));preferences.values().forEach(p->rows.add(JsonUtils.object("collection","preferences","record",p)));groups.values().forEach(g->rows.add(JsonUtils.object("collection","filterGroups","record",g)));return page(user,sid,topic,"entries",rows,r,JsonUtils.object("epoch",store.epoch,"sequence",Long.toString(lastSequence)));}
    void finishState(JSONObject result){JSONObject records=JsonUtils.object("notifications",JsonUtils.array(),"preferences",JsonUtils.array(),"filterGroups",JsonUtils.array());for(Object x:(List<?>)result.remove("entries")){JSONObject row=(JSONObject)x;records.getJSONArray(row.getString("collection")).add(row.get("record"));}result.put("records",records);result.put("done",result.get("nextCursor")==null);}
    Object events(String user, JSONObject request) throws Exception {
        internal(user);
        long after;
        try {
            after = Long.parseLong(RequestValidator.requireString(request, "afterSequence"));
        } catch (NumberFormatException error) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid afterSequence", null);
        }
        if (!Objects.equals(store.epoch, request.getString("epoch")) || after < 0 || after > lastSequence) {
            throw ApiException.forField("1007", "RESET_REQUIRED", "Invalid event position", null);
        }
        List<JSONObject> events = store.rows("SELECT payload FROM " + store.table("events")
                + " WHERE sequence>? ORDER BY sequence LIMIT ?", after, limit(request));
        long nextSequence = events.isEmpty() ? after : events.get(events.size() - 1).getLongValue("sequence");
        return JsonUtils.object("epoch", store.epoch, "events", events,
                "nextSequence", Long.toString(nextSequence),
                "seenSequence", Long.toString(lastSequence), "hasMore", nextSequence < lastSequence);
    }

    void internal(String user){if(!user.startsWith("S-"))throw ApiException.forField("1003","FORBIDDEN","Internal service required",null);}
}
