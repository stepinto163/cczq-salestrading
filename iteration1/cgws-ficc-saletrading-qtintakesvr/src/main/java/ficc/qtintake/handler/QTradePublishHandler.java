package ficc.qtintake.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ats.bbox.framework.Server;
import com.adaptiveMQ2.message.Message;
import ficc.qtintake.QtIntakeResource;
import java.util.Map;

/** 只信任网关传入的 session signalID；请求正文不能指定 STC 归属用户。 */
public final class QTradePublishHandler extends ContentHandler {
    @Override
    public Map<String, Object> handle(String topic, Message message, String content,
            Map<String, Object> metadata, boolean fromList) {
        QtIntakeResource resource = (QtIntakeResource) Server.getServer().getResource("QtIntakeResource");
        return resource.publish(message.getSignalID(), content);
    }
}
