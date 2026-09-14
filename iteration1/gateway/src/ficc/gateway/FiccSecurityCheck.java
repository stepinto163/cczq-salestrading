package ficc.gateway;

import com.gateway.invoke.security.*;
import com.gateway.message.SystemMessage;
import com.gateway.connector.proto.Proto;

/** FICC private stream authorization through the existing GW security extension. */
public final class FiccSecurityCheck implements ISecurityCheck {
    public SecurityResult check(SystemMessage sys,Proto message,String server,String method,String content){
        SecurityResult result=new SecurityResult();
        if("subscribe".equals(method)||"unsubscribe".equals(method)){
            String sid=message.getSessionId();
            if(sid==null||sid.isEmpty()||!(content.equals("ficc.quote.report."+sid)||content.equals("ficc.price.report."+sid)||content.equals("ficc.notification.report."+sid))){
                result.Code="1003";result.Msg="Only this session's FICC report topics may be subscribed";
            }
        }
        return result;
    }
}
