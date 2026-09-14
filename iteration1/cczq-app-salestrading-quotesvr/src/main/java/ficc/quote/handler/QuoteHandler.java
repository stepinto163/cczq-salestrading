package ficc.quote.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ats.bbox.framework.Server;
import com.adaptiveMQ2.message.Message;
import ficc.quote.QuoteResource;
import java.util.Map;

/** STC handler adapter. Session identity is supplied by GW, not request JSON. */
public abstract class QuoteHandler extends ContentHandler {
    protected QuoteResource resource(){return (QuoteResource)Server.getServer().getResource("QuoteResource");}
    @Override public Map<String,Object> handle(String topic,Message message,String content,Map<String,Object> map,boolean fromList){
        return resource().dispatch(topic,message.getSignalID(),content);
    }
    @Override public Map<String,Object> snapshot(String topic,Message message,Map<String,Object> map,boolean fromList){
        return resource().snapshot(topic,message.getSignalID());
    }
}
