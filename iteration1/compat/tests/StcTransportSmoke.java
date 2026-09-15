import com.gateway.connector.tcp.client.*;
import com.alibaba.fastjson2.*;
import java.util.*;
import java.util.concurrent.*;
import java.nio.file.*;

/** One quote, small real-DPS functional test against the STC-configured QuoteSvr. */
public final class StcTransportSmoke {
    static GateWayApi api=new GateWayApi();static String sid,topic,id;static int checks;
    static LinkedBlockingQueue<JSONObject> ordinary=new LinkedBlockingQueue<>(32),raw=new LinkedBlockingQueue<>(32);
    static JSONObject obj(Object... pairs){JSONObject o=new JSONObject();for(int i=0;i<pairs.length;i+=2)o.put(pairs[i].toString(),pairs[i+1]);return o;}
    static void check(boolean ok,String label){if(!ok)throw new AssertionError(label);checks++;System.out.println("PASS "+label);}
    static JSONObject call(String name,JSONObject r)throws Exception{
        r.put("requestId",UUID.randomUUID().toString());
        JSONObject out=JSON.parseObject(api.requestSync(sid,"QuoteSvr","ficc."+name,r.toJSONString(),String.class,3000));
        if(out==null||!"0000".equals(out.getString("Code")))throw new AssertionError(name+" reply "+out);
        return out.getJSONObject("Data");
    }
    static JSONObject get()throws Exception{return call("quote.get",obj("quoteId",id)).getJSONObject("quote");}
    static void action(String op)throws Exception{
        JSONObject data=call("quote."+op,obj("quotes",List.of(obj("quoteId",id,"expectedRevision",get().getString("revision")))));
        check("0000".equals(data.getJSONArray("results").getJSONObject(0).getString("Code")),op+" through registered handler");
    }
    static JSONObject event(BlockingQueue<JSONObject> q)throws Exception{
        long end=System.nanoTime()+TimeUnit.SECONDS.toNanos(3);
        while(System.nanoTime()<end){JSONObject v=q.poll(100,TimeUnit.MILLISECONDS);if(v!=null&&"UPDATE".equals(v.getString("kind")))return v;}
        throw new AssertionError("Expected quote event within three seconds");
    }
    public static void main(String[] args)throws Exception{
        try{
            api.setReconnectFlag(false);api.connect("127.0.0.1",18680,"gateway","",false,1,null);
            JSONObject login=JSON.parseObject(api.requestSync("LoginSvr","SYS.ATS.LOGIN",obj("method","login","username","trader1","password","trader1").toJSONString(),String.class,3000));
            check(login!=null&&"0001".equals(login.getString("Code")),"STC Login simulator handler");sid=login.getString("sid");
            topic=call("quote.stream.open",obj()).getString("pushTopic");
            IMessage<String> listener=(t,c)->ordinary.offer(JSON.parseObject(c));
            check(api.subscribe(topic,listener,String.class),"ordinary DPS subscription accepted");
            JSONObject view=call("quote.view.open",obj("viewId","test-"+UUID.randomUUID(),"ownerScope","MINE","status",List.of("PENDING_CONFIRM","ACTIVE","REFERRED","DELETED"),"limit",20));
            check(view.containsKey("quotes"),"view handler returns explicit initial records");
            JSONObject data=call("quote.create",obj("quotes",List.of(obj("clientQuoteId","DPS-"+UUID.randomUUID(),"securityId","250001.IB","side","1","quoteType","1","yieldType","1","price","2.5000","quantity","1000","settlType","1","tradeDate",java.time.LocalDate.now().toString()))));
            JSONObject item=data.getJSONArray("results").getJSONObject(0);check("0000".equals(item.getString("Code")),"create through handler and PostgreSQL");id=item.getString("quoteId");
            JSONObject first=event(ordinary);check(first.containsKey("quotes")&&!first.containsKey("books"),"QuoteStream publishes via STC NetRtOutputStream");
            api.setNotifyByteFilter((t,b)->{if(t.equals(topic)){JSONObject envelope=JSON.parseObject(b);check(t.equals(envelope.getString("topic")),"raw callback receives complete DPS envelope");raw.offer(JSON.parseObject(envelope.getString("content")));return false;}return true;});
            action("confirm");JSONObject second=event(raw);check(ordinary.poll(100,TimeUnit.MILLISECONDS)==null,"consumed raw notification is not delivered twice");
            api.setNotifyByteFilter((t,b)->true);action("refer");JSONObject third=event(ordinary);check(third.getLongValue("sequence")>second.getLongValue("sequence"),"raw/ordinary switching preserves sequence");
            api.setNotifyByteFilter((t,b)->{throw new IllegalStateException("expected test exception");});action("unrefer");event(ordinary);check(true,"callback exception falls back to ordinary delivery");
            api.setNotifyByteFilter(null);action("refer");event(ordinary);action("delete");event(ordinary);
            check("DELETED".equals(get().getString("status")),"request/reply remains functional after callback changes");
            check(call("quote.history.query",obj("quoteId",id)).getJSONArray("records").size()==6,"six lifecycle revisions persisted");
            api.unSubscribe(topic,listener,String.class);
            Files.writeString(Path.of(args[0]),obj("result","PASS","checks",checks,"quotesCreated",1,"path","DPS Java client -> STC GwServerWrapper -> handlers -> QuoteStream -> NetRtOutputStream","externalGWTested",false).toJSONString());
            System.out.println("PASS "+checks+" transport checks; one quote; external GW/C# not covered.");
        }finally{api.disconnect();}
    }
}
