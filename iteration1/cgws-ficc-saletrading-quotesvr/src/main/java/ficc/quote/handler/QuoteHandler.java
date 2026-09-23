package ficc.quote.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ats.bbox.framework.Server;

import com.adaptiveMQ2.message.Message;

import ficc.common.JsonUtils;
import ficc.common.client.DpsMessageLog;
import ficc.quote.QuoteResource;

import org.apache.log4j.Logger;

import java.util.Map;

/** STC handler adapter. Session identity is supplied by GW, not request JSON. */
public abstract class QuoteHandler extends ContentHandler {
    private static final Logger LOGGER = Logger.getLogger("ficc.quote.dps");

    protected QuoteResource resource() {
        return (QuoteResource) Server.getServer().getResource("QuoteResource");
    }

    @Override
    public Map<String, Object> handle(
            String topic,
            Message message,
            String content,
            Map<String, Object> map,
            boolean fromList) {
        LOGGER.debug("DPS REQUEST topic=" + topic + " body=" + DpsMessageLog.body(content));
        Map<String, Object> reply = resource().dispatch(topic, message.getSignalID(), content);
        LOGGER.debug(
                "DPS REPLY topic="
                        + topic
                        + " body="
                        + DpsMessageLog.body(JsonUtils.toJson(reply)));
        return reply;
    }

    @Override
    public Map<String, Object> snapshot(
            String topic, Message message, Map<String, Object> map, boolean fromList) {
        return resource().snapshot(topic, message.getSignalID());
    }
}
