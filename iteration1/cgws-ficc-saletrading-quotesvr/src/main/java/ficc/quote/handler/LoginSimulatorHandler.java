package ficc.quote.handler;

import com.adaptiveMQ2.message.Message;

import java.util.Map;

/** Client-only LoginSvr operation, hosted in the standalone service process. */
public final class LoginSimulatorHandler extends QuoteHandler {
    @Override
    public Map<String, Object> handle(
            String topic,
            Message message,
            String content,
            Map<String, Object> map,
            boolean fromList) {
        return resource().login(content);
    }
}
