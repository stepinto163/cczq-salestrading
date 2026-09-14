package ficc.gateway;

import com.gateway.connector.proto.Proto;
import com.gateway.invoke.ceda.CedaProxy;
import com.gateway.message.MessageWrapper;
import com.gateway.message.SystemMessage;

/** Uses the original asynchronous STC forwarding implementation. */
public final class MeasuredCedaProxy extends CedaProxy {
    @Override
    public MessageWrapper processAppRequestReply(SystemMessage systemMessage, Proto message,
            String serverName, String topic, String content) {
        PerformanceTrace.begin(message, topic, content);
        return super.processAppRequestReply(systemMessage, message, serverName, topic, content);
    }
}
