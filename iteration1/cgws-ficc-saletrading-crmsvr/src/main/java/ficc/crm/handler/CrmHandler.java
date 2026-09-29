package ficc.crm.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ficc.api.ProtocolBoundary;
import ats.bbox.framework.Server;

import com.adaptiveMQ2.message.Message;

import ficc.common.JsonUtils;
import ficc.common.client.DpsMessageLog;
import ficc.crm.CrmResource;

import org.apache.log4j.Logger;

import java.util.Map;

/**
 * STC handler adapter for CRMSvr.
 *
 * <p>CRMSvrClient subscribes with an image, so {@code snapshot} is as important as {@code handle}:
 * the same topic carries the full table for subscribers and a paged reply for request/reply
 * callers.
 */
public abstract class CrmHandler extends ContentHandler {
    private static final Logger LOGGER = Logger.getLogger("ficc.crm.dps");

    protected CrmResource resource() {
        return (CrmResource) Server.getServer().getResource("CrmResource");
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
                "DPS REPLY topic=" + topic + " body=" + DpsMessageLog.body(JsonUtils.toJson(reply)));
        return ProtocolBoundary.reply(topic, reply);
    }

    @Override
    public Map<String, Object> snapshot(
            String topic, Message message, Map<String, Object> map, boolean fromList) {
        LOGGER.info("DPS IMAGE topic=" + topic);
        Map<String, Object> reply = resource().image(topic, message.getSignalID(), map);
        LOGGER.debug(
                "DPS IMAGE REPLY topic="
                        + topic
                        + " body="
                        + DpsMessageLog.body(JsonUtils.toJson(reply)));
        return reply;
    }
}
