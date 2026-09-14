package com.gateway.connector.tcp.client;

/**
 * Raw-byte notify listener.
 *
 * Restored 2026-09-14: the lite refactor lost this interface while
 * {@code com.gateway.invoke.ceda.CedaApi} still references it directly
 * (rawMessageListenerMap, raw notify dispatch and the Class.forName filter
 * lookup). Contract matches the anonymous implementation in CedaApi#addMessageListener
 * and the dispatch call site: return true when the message was handled.
 *
 * The optional notify-cache methods (isNotifyCacheTopic / getNotifyCacheWorkerCount /
 * getNotifyCacheMaxSize) are intentionally not declared: GwClientWrapper in
 * sc-app-common has no setNotifyByteFilter, so the reflective filter install in
 * CedaApi#trySetNotifyByteFilter stays disabled exactly as in the lite build.
 */
public interface IMessageByte {

    /**
     * @param topic notify topic
     * @param body  raw payload bytes
     * @return true if the message was handled and should not be dispatched further
     */
    boolean onMessage(String topic, byte[] body);
}
