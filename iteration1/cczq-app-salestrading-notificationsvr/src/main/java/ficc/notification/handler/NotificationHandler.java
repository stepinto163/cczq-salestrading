package ficc.notification.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ats.bbox.framework.Server;
import com.adaptiveMQ2.message.Message;
import ficc.notification.NotificationResource;
import java.util.Map;

public abstract class NotificationHandler extends ContentHandler {
    @Override public Map<String,Object> handle(String topic,Message message,String content,Map<String,Object> map,boolean fromList){return ((NotificationResource)Server.getServer().getResource("NotificationResource")).dispatch(topic,message.getSignalID(),content);}
}
