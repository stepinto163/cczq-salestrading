package com.gateway.notify;

import java.util.HashMap;
import java.util.Map.Entry;
import java.util.concurrent.CopyOnWriteArraySet;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.alibaba.fastjson.JSON;
import com.gateway.connector.Session;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.TcpConnection;
import com.gateway.connector.tcp.TcpConnector;
import com.gateway.connector.tcp.server.TopicManager;
import com.gateway.connector.utils.ProtoUtils;
import com.gateway.connector.utils.TopicUtils;
import com.gateway.constant.Constants;
import com.gateway.marketdata.LatestWinsDispatcher;
import com.gateway.marketdata.MarketDataBurstConfig;

import sc.app.stc.utils.AsyncSeqThreadGroup;

public class NotifyProxy {

    protected Logger logger = LoggerFactory.getLogger(NotifyProxy.class);

    private TopicManager topicManager;

    public TopicManager getTopicManager() {
        return topicManager;
    }

    public void setTopicManager(TopicManager topicManager) {
        this.topicManager = topicManager;
    }

    private TcpConnector tcpConnector;
    private AsynNotify asynNotify = new AsynNotify(Runtime.getRuntime().availableProcessors() * 2);
    private NotifyFilter filterChain;
    private MarketDataBurstConfig marketDataBurstConfig = new MarketDataBurstConfig();
    private volatile LatestWinsDispatcher<NotifyMsg> marketDataSessionDispatcher;

    public void setFilterChain(NotifyFilter filterChain) {
        this.filterChain = filterChain;
    }

    public void setTcpConnector(TcpConnector tcpConnector) {
        this.tcpConnector = tcpConnector;
    }

    public void setMarketDataBurstConfig(MarketDataBurstConfig marketDataBurstConfig) {
        this.marketDataBurstConfig = marketDataBurstConfig != null ? marketDataBurstConfig : new MarketDataBurstConfig();
        this.marketDataSessionDispatcher = null;
    }

    public static class NotifyMsg {
        public String sid;
        public String topic;
        public String content;
        public byte[] rawBody;
        public boolean contentLoaded;
        public boolean contentDirty;
        public String nodeId;
        private transient MarketDataBurstConfig burstConfig;

        public NotifyMsg copy() {
            NotifyProxy.NotifyMsg msg = new NotifyProxy.NotifyMsg();
            msg.sid = sid;
            msg.topic = topic;
            msg.content = content;
            msg.rawBody = rawBody;
            msg.contentLoaded = contentLoaded;
            msg.contentDirty = contentDirty;
            msg.nodeId = nodeId;
            msg.burstConfig = burstConfig;
            return msg;
        }

        public String getContent() {
            if (!contentLoaded && rawBody != null) {
                contentLoaded = true;
                try {
                    com.gateway.connector.tcp.body.NotifyBody notifyBody = ProtoUtils.getNotify(rawBody);
                    content = notifyBody != null ? notifyBody.content : null;
                } catch (Exception e) {
                    content = null;
                }
                if (burstConfig != null) {
                    burstConfig.recordLazyDecode(burstConfig.resolveTopicPrefix(topic), nodeId);
                }
            }
            return content;
        }

        public void setContent(String content) {
            this.content = content;
            this.contentLoaded = true;
            this.contentDirty = true;
        }
    }

    public class AsynNotify extends AsyncSeqThreadGroup<NotifyMsg> {

        public AsynNotify(int asyncBaseNum) {
            super(asyncBaseNum);
        }

        @Override
        protected void process(NotifyMsg msg) {
            xNotify(msg);
        }
    }

    public void notify(String topic, String content, String nodeId) {
        NotifyMsg msg = newMsg(null, topic, content, nodeId);
        if (isMarketDataTopic(topic)) {
            broadcastMarketData(msg);
            return;
        }
        asynNotify.add(topic, msg);
    }

    public void notifyRaw(String topic, byte[] rawBody, String nodeId) {
        notifyRaw(null, topic, rawBody, nodeId);
    }

    public void xNotify(NotifyMsg msg) {
        String sid = msg.sid;
        if (StringUtils.isEmpty(sid)) {
            broadcastNotify(msg);
        } else {
            p2pNotify(msg);
        }
    }

    public void broadcastNotify(NotifyMsg msg) {
        String topic = msg.topic;
        String nodeId = msg.nodeId;

        CopyOnWriteArraySet<String> cas2 = topicManager.getTopics2().get(topic);
        if (cas2 != null) {
            for (String sid : cas2) {
            try {
                NotifyMsg current = msg.copy();
                current.sid = sid;
                current = applyFilter(current);
                if (current == null) {
                    continue;
                }
                Session session = getSession(sid);
                if (isBroadcastNodeMatched(nodeId, session)) {
                    sendNow(current);
                }
            } catch (Exception e) {
                logger.debug("notify ", e);
            }
        }
        }

        for (Entry<String, CopyOnWriteArraySet<String>> entry : topicManager.getTopicsWildCard().entrySet()) {
            String sid = entry.getKey();
            CopyOnWriteArraySet<String> cas = entry.getValue();
            for (String stopic : cas) {
                if (!TopicUtils.matchTopic(stopic, topic)) {
                    continue;
                }
                NotifyMsg current = msg.copy();
                current.sid = sid;
                current = applyFilter(current);
                if (current == null) {
                    break;
                }
                try {
                    Session session = getSession(sid);
                    if (isBroadcastNodeMatched(nodeId, session)) {
                        sendNow(current);
                    }
                } catch (Exception e) {
                    logger.error("notify error", e);
                }
                break;
            }
        }
    }

    public void p2pNotify(NotifyMsg msg) {
        NotifyMsg current = applyFilter(msg);
        if (current == null) {
            return;
        }
        sendNow(current);
    }

    public void notify(String sid, String topic, String content, String nodeId) {
        if (StringUtils.isEmpty(sid)) {
            notify(topic, content, nodeId);
            return;
        }
        NotifyMsg msg = newMsg(sid, topic, content, nodeId);
        if (isMarketDataTopic(topic)) {
            submitMarketData(msg);
            return;
        }
        asynNotify.add(topic, msg);
    }

    public void notifyRaw(String sid, String topic, byte[] rawBody, String nodeId) {
        if (StringUtils.isEmpty(sid)) {
            NotifyMsg msg = newRawMsg(null, topic, rawBody, nodeId);
            if (isMarketDataTopic(topic)) {
                broadcastMarketData(msg);
                return;
            }
            asynNotify.add(topic, msg);
            return;
        }
        NotifyMsg msg = newRawMsg(sid, topic, rawBody, nodeId);
        if (isMarketDataTopic(topic)) {
            submitMarketData(msg);
            return;
        }
        asynNotify.add(topic, msg);
    }

    private NotifyMsg newMsg(String sid, String topic, String content, String nodeId) {
        NotifyMsg msg = new NotifyMsg();
        msg.sid = sid;
        msg.topic = topic;
        msg.content = content;
        msg.contentLoaded = true;
        msg.nodeId = nodeId;
        msg.burstConfig = marketDataBurstConfig;
        return msg;
    }

    private NotifyMsg newRawMsg(String sid, String topic, byte[] rawBody, String nodeId) {
        NotifyMsg msg = new NotifyMsg();
        msg.sid = sid;
        msg.topic = topic;
        msg.rawBody = rawBody;
        msg.contentLoaded = false;
        msg.contentDirty = false;
        msg.nodeId = nodeId;
        msg.burstConfig = marketDataBurstConfig;
        return msg;
    }

    private void broadcastMarketData(NotifyMsg msg) {
        String topic = msg.topic;
        CopyOnWriteArraySet<String> exactSessions = topicManager.getTopics2().get(topic);
        if (exactSessions != null) {
            for (String sid : exactSessions) {
                NotifyMsg current = msg.copy();
                current.sid = sid;
                submitMarketData(current);
            }
        }
        for (Entry<String, CopyOnWriteArraySet<String>> entry : topicManager.getTopicsWildCard().entrySet()) {
            for (String stopic : entry.getValue()) {
                if (TopicUtils.matchTopic(stopic, topic)) {
                    NotifyMsg current = msg.copy();
                    current.sid = entry.getKey();
                    submitMarketData(current);
                    break;
                }
            }
        }
    }

    private void submitMarketData(NotifyMsg msg) {
        if (msg == null || StringUtils.isEmpty(msg.sid)) {
            return;
        }
        Session session = getSession(msg.sid);
        if (session == null) {
            return;
        }
        if (!isBroadcastNodeMatched(msg.nodeId, session)) {
            return;
        }
        NotifyMsg filtered = applyFilter(msg);
        if (filtered == null) {
            return;
        }
        String topicPrefix = marketDataBurstConfig.resolveTopicPrefix(filtered.topic);
        getMarketDataSessionDispatcher().submit(filtered.sid + "|" + filtered.topic, filtered.copy(), topicPrefix,
                filtered.nodeId);
    }

    private LatestWinsDispatcher<NotifyMsg> getMarketDataSessionDispatcher() {
        LatestWinsDispatcher<NotifyMsg> dispatcher = marketDataSessionDispatcher;
        if (dispatcher != null) {
            return dispatcher;
        }
        synchronized (this) {
            dispatcher = marketDataSessionDispatcher;
            if (dispatcher == null) {
                final MarketDataBurstConfig config = marketDataBurstConfig != null ? marketDataBurstConfig
                        : new MarketDataBurstConfig();
                marketDataBurstConfig = config;
                dispatcher = new LatestWinsDispatcher<NotifyMsg>("market-data-session", "egress", "egress",
                        Runtime.getRuntime().availableProcessors() * 2, config.getEgressMaxPendingKeys(), config,
                        new LatestWinsDispatcher.Handler<NotifyMsg>() {
                            @Override
                            public LatestWinsDispatcher.HandleResult handle(NotifyMsg value) {
                                return sendMarketData(value);
                            }
                        });
                marketDataSessionDispatcher = dispatcher;
            }
        }
        return dispatcher;
    }

    private LatestWinsDispatcher.HandleResult sendMarketData(NotifyMsg msg) {
        if (msg == null || StringUtils.isEmpty(msg.sid)) {
            return LatestWinsDispatcher.HandleResult.DONE;
        }
        Session session = getSession(msg.sid);
        if (session == null) {
            return LatestWinsDispatcher.HandleResult.DONE;
        }
        if (!isBroadcastNodeMatched(msg.nodeId, session)) {
            return LatestWinsDispatcher.HandleResult.DONE;
        }
        if (shouldRetry(msg, session)) {
            marketDataBurstConfig.recordNonWritable("egress", marketDataBurstConfig.resolveTopicPrefix(msg.topic),
                    msg.nodeId);
            return LatestWinsDispatcher.HandleResult.RETRY;
        }
        sendNow(msg);
        return LatestWinsDispatcher.HandleResult.DONE;
    }

    private boolean isBroadcastNodeMatched(String nodeId, Session session) {
        if (session == null) {
            return false;
        }
        if (StringUtils.isBlank(nodeId)) {
            return true;
        }
        if ("*".equals(nodeId) || Constants.NODE_PUBLIC.equals(nodeId)) {
            return true;
        }
        return StringUtils.equals(nodeId, session.getNodeId());
    }

    private boolean shouldRetry(NotifyMsg msg, Session session) {
        if (msg == null || session == null || marketDataBurstConfig == null
                || !marketDataBurstConfig.isDropWhenNotWritable()) {
            return false;
        }
        try {
            if (!session.getConnection().isWritable()) {
                return true;
            }
        } catch (Exception e) {
            logger.debug("check writable error sid={}", msg.sid, e);
            return true;
        }
        int outboundQueueSize = getOutboundQueueSize(session);
        return outboundQueueSize > marketDataBurstConfig.getOutboundSoftLimit();
    }

    private int getOutboundQueueSize(Session session) {
        if (session == null || session.getConnection() == null) {
            return 0;
        }
        if (session.getConnection() instanceof TcpConnection) {
            return ((TcpConnection<?>) session.getConnection()).getOutboundQueueSize();
        }
        return 0;
    }

    private NotifyMsg applyFilter(NotifyMsg msg) {
        if (msg == null) {
            return null;
        }
        NotifyMsg current = msg.copy();
        if (filterChain != null) {
            current = filterChain.doFilter(current);
        }
        return current;
    }

    private Session getSession(String sid) {
        if (StringUtils.isEmpty(sid) || tcpConnector == null || tcpConnector.getTcpSessionManager() == null) {
            return null;
        }
        return tcpConnector.getTcpSessionManager().getSession(sid);
    }

    public boolean isMarketDataTopic(String topic) {
        return marketDataBurstConfig != null && marketDataBurstConfig.isMarketDataTopic(topic);
    }

    private void sendNow(NotifyMsg msg) {
        String sid = msg.sid;
        String topic = msg.topic;
        boolean rawDirect = msg.rawBody != null && !msg.contentDirty;
        String content = rawDirect ? msg.content : msg.getContent();
        byte[] body = rawDirect ? msg.rawBody : getBody(topic, content);
        Proto proto = ProtoUtils.generateNotify(sid, body);
        try {
            if (tcpConnector.exist(sid)) {
                String userName = tcpConnector.send(sid, proto);
                if (rawDirect && marketDataBurstConfig != null) {
                    marketDataBurstConfig.recordRawDirectSend(marketDataBurstConfig.resolveTopicPrefix(topic),
                            msg.nodeId);
                }
                logger.debug("{},{},{},{}", sid, userName, topic, rawDirect ? "<raw>" : content);
            }
        } catch (Exception e) {
            logger.warn("notify ", e);
        }
    }

    private byte[] getBody(String topic, String content) {
        HashMap<String, String> hm = new HashMap<String, String>();
        hm.put("topic", topic);
        hm.put("content", content);
        return JSON.toJSONBytes(hm);
    }
}
