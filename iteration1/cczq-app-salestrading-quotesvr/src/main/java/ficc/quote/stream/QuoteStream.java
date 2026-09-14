package ficc.quote.stream;

import ats.bbox.framework.*;
import ats.bbox.framework.stream.StreamAdapter;
import ficc.quote.QuoteResource;
import java.util.*;
import java.util.concurrent.*;

/** Business events -> configured RtOutputStream -> existing STC DPS publisher. */
public final class QuoteStream extends StreamAdapter {
    private QuoteResource resource;
    private ScheduledExecutorService heartbeat;
    @Override public void init()throws LifecycleException{
        resource=(QuoteResource)Server.getServer().getResource("QuoteResource");
        resource.publishThrough((topic,value)->{
            Map<String,Object> message=new HashMap<>();message.put("topic",topic);message.put("data",value);
            try{send(message);}catch(Exception error){throw new IllegalStateException("STC output stream failed",error);}
        });
        super.init();
    }
    @Override public void start()throws LifecycleException{
        super.start();
        heartbeat=Executors.newSingleThreadScheduledExecutor(r->{Thread t=new Thread(r,"quote-heartbeat");t.setDaemon(true);return t;});
        heartbeat.scheduleAtFixedRate(()->{try{resource.heartbeat();}catch(Exception error){System.err.println("Quote heartbeat failed: "+error.getClass().getSimpleName());}},5,5,TimeUnit.SECONDS);
    }
    @Override public void stop()throws LifecycleException{
        if(heartbeat!=null)heartbeat.shutdownNow();super.stop();
    }
}
