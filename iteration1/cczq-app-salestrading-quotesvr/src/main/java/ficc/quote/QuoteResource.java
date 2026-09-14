package ficc.quote;

import ficc.common.*;

import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.resource.ResourceAdapter;
import com.alibaba.fastjson2.*;
import java.nio.file.*;
import java.util.*;
import java.util.function.BiConsumer;

/** BBox resource: owns business state and the PostgreSQL connection. */
public final class QuoteResource extends ResourceAdapter {
    private PgStore store;
    private QuoteService service;
    private final Map<String,String> sessions=new HashMap<>();
    @Override public void init()throws LifecycleException{
        try{
            Properties properties=new Properties();
            try(var r=Files.newBufferedReader(Path.of(getConfig().getString("quote.properties")))){properties.load(r);}
            store=new PgStore(properties);service=new QuoteService(store,new ReferenceData());
            super.init();
        }catch(Exception error){throw new LifecycleException(error);}
    }
    public void publishThrough(BiConsumer<String,JSONObject> publisher){service.publish=publisher;}
    public void heartbeat(){synchronized(service){service.views.heartbeat();}}
    public synchronized Map<String,Object> login(String content){
        JSONObject request=JsonUtils.parseObject(content);String method=request.getString("method");
        JSONObject parameters=request.getJSONObject("parameter");
        if("userInfo".equals(method)){
            String sid=parameters==null?null:parameters.getString("sid");
            return sessions.containsKey(sid)?loginResult(sid,sessions.get(sid)):JsonUtils.object("Code","1003","Msg","Unknown simulator session");
        }
        String name=parameters==null?request.getString("username"):parameters.getString("userName");
        String password=parameters==null?request.getString("password"):parameters.getString("password");
        String uid=name==null?null:service.refs.user(name);
        if(uid==null||!"demo".equals(password))return JsonUtils.object("Code","1003","Msg","Invalid simulator login");
        String sid=IdGenerator.newId("SIM-");sessions.put(sid,uid);return loginResult(sid,uid);
    }
    private JSONObject loginResult(String sid,String uid){return JsonUtils.object("Code","0001","Msg","Fixed LoginSvr simulator","sid",sid,"userId",uid,"userid",uid,"companyId","FICC-DEMO");}
    public Map<String,Object> dispatch(String topic,String sid,String content){
        String user; synchronized(this){user=sessions.get(sid);}
        JSONObject request;
        try{request=JsonUtils.parseObject(content);}catch(Exception ex){return ApiResponse.failure(null,ApiException.forField("1001","VALIDATION_FAILED","Invalid JSON",null));}
        synchronized(service){
            JSONObject result=service.handle(user,sid,topic,request);
            if(topic.endsWith(".stream.open")&&"0000".equals(result.getString("Code"))){
                // GW owns the physical client subscription. QuoteSvr owns the logical view.
                service.views.authorize(sid,user,result.getJSONObject("Data").getString("pushTopic"));
            }
            return result;
        }
    }
    public Map<String,Object> snapshot(String topic,String sid){
        String user; synchronized(this){user=sessions.get(sid);}
        if(user==null)return JsonUtils.object("Code","1005","Msg","Login required");
        synchronized(service){try{
            service.views.authorize(sid,user,topic);
            Map<String,Object> result=new LinkedHashMap<>();
            service.views.image(topic,sid).forEach((key,value)->result.put(key,JsonUtils.toJson(value)));
            return result;
        }catch(ApiException error){return JsonUtils.object("Code","1005","Msg",error.getMessage());}}
    }
    @Override public void stop()throws LifecycleException{
        if(store!=null)try{store.close();store=null;}catch(Exception error){throw new LifecycleException(error);}
        super.stop();
    }
}
