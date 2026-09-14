package ficc.notification.stream;

import ats.bbox.framework.*;
import ats.bbox.framework.stream.StreamAdapter;
import ficc.notification.NotificationResource;
import java.util.*;

public final class NotificationStream extends StreamAdapter {
    @Override public void init()throws LifecycleException{
        ((NotificationResource)Server.getServer().getResource("NotificationResource")).publishThrough((topic,data)->{
            Map<String,Object> message=new HashMap<>();message.put("topic",topic);message.put("data",data);
            try{send(message);}catch(Exception e){throw new IllegalStateException(e);}
        });super.init();
    }
}
