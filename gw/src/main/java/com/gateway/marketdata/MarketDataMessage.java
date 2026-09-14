package com.gateway.marketdata;

public class MarketDataMessage {
    public String sid;
    public String topic;
    public String content;
    public byte[] rawBody;
    public String nodeId;

    public MarketDataMessage copy() {
        MarketDataMessage message = new MarketDataMessage();
        message.sid = sid;
        message.topic = topic;
        message.content = content;
        message.rawBody = rawBody;
        message.nodeId = nodeId;
        return message;
    }
}
