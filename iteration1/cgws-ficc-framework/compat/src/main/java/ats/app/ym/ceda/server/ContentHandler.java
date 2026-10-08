package ats.app.ym.ceda.server;

import com.adaptiveMQ2.message.Message;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.Map;

/**
 * Preserves handler signatures; requests and snapshots are dispatched directly by the DPS adapter.
 */
public class ContentHandler {
    protected final Logger logger = LoggerFactory.getLogger(getClass());
    protected String matchTopic;
    protected String topicPrefix;
    protected String poolKey;

    public void setTopic(String topic) {
        matchTopic = topic;
        int wildcard = topic.indexOf('*');
        topicPrefix = wildcard < 0 ? topic : topic.substring(0, wildcard);
    }

    public String getPoolKey() {
        return poolKey;
    }

    public void setPoolKey(String value) {
        poolKey = value;
    }

    public String getTopicPrefix() {
        return topicPrefix;
    }

    public String getMatchTopic() {
        return matchTopic;
    }

    public Map<String, Object> snapshot(
            String topic, Message message, Map<String, Object> values, boolean fromList) {
        return values;
    }

    public Map<String, Object> handle(
            String topic,
            Message message,
            String content,
            Map<String, Object> values,
            boolean fromList) {
        return values;
    }
}
