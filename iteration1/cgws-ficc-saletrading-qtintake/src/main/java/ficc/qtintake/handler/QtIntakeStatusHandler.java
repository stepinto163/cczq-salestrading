package ficc.qtintake.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ats.bbox.framework.Server;

import com.adaptiveMQ2.message.Message;

import ficc.qtintake.QtIntakeResource;

import org.apache.log4j.Logger;

import java.util.Map;

public final class QtIntakeStatusHandler extends ContentHandler {
    private static final Logger LOGGER = Logger.getLogger("ficc.qtintake.messages");

    @Override
    public Map<String, Object> handle(
            String topic,
            Message message,
            String content,
            Map<String, Object> metadata,
            boolean fromList) {
        LOGGER.debug(
                "DPS REQUEST topic="
                        + topic
                        + " body="
                        + ficc.common.client.DpsMessageLog.body(content));
        Map<String, Object> reply =
                ((QtIntakeResource) Server.getServer().getResource("QtIntakeResource")).status();
        LOGGER.debug("DPS REPLY topic=" + topic + " body=" + ficc.common.JsonUtils.toJson(reply));
        return reply;
    }
}
