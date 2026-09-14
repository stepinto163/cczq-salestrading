 
package com.gateway.connector.tcp;

import com.gateway.connector.Session;
import com.gateway.connector.api.ExchangeConnector;
import com.gateway.message.MessageWrapper;

import io.netty.channel.ChannelHandlerContext;

public abstract class ExchangeTcpConnector<T> extends ExchangeConnector<T> {

   
    public abstract void connect(ChannelHandlerContext ctx, MessageWrapper wrapper);

    public abstract void close(MessageWrapper wrapper);
    /**
     * 会话心跳
     *
     * @param wrapper
     */
    public abstract void heartbeatClient(MessageWrapper wrapper);

    /**
     * 接收客户端消息通知响应
     *
     * @param wrapper
     */
    public abstract void responseSendMessage(MessageWrapper wrapper);

    public abstract void responseNoKeepAliveMessage(ChannelHandlerContext ctx, MessageWrapper wrapper);
    protected TcpSessionManager tcpSessionManager = null;

    public String send(String sessionId, T message) throws Exception {
        return super.send(tcpSessionManager, sessionId, message);
    }
    public boolean isWritable(String sessionId) throws Exception {
        return super.isWritable(tcpSessionManager, sessionId);
    }
    public boolean exist(String sessionId) throws Exception {
        Session session = tcpSessionManager.getSession(sessionId);
        return session != null ? true : false;
    }

    public void setTcpSessionManager(TcpSessionManager tcpSessionManager) {
        this.tcpSessionManager = tcpSessionManager;
    }

	public TcpSessionManager getTcpSessionManager() {
		return tcpSessionManager;
	}
}
