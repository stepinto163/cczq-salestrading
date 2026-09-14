package ficc.gateway;

import com.gateway.connector.tcp.TcpConnector;
import com.gateway.message.MessageWrapper;

public final class MeasuredTcpConnector extends TcpConnector {
    @Override
    public String send(String sessionId, Object message) throws Exception {
        PerformanceTrace.finish(message);
        return super.send(sessionId, message);
    }

    @Override
    public void responseSendMessage(MessageWrapper wrapper) {
        PerformanceTrace.finish(wrapper.getBody());
        super.responseSendMessage(wrapper);
    }
}
