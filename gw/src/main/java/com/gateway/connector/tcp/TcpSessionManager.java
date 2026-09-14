 
package com.gateway.connector.tcp;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.Connection;
import com.gateway.connector.Session;
import com.gateway.connector.api.ExchangeSession;
import com.gateway.connector.api.listener.SessionListener;

import io.netty.channel.ChannelHandlerContext;

public class TcpSessionManager extends ExchangeTcpSessionManager {
	

    private final static Logger logger = LoggerFactory.getLogger(TcpSessionManager.class);
	private int maxQueueSize=0;
	public int getMaxQueueSize() {
		return maxQueueSize;
	}
	public void setMaxQueueSize(int maxQueueSize) {
		this.maxQueueSize = maxQueueSize;
	}
    @Override
    public synchronized Session createSession(String userName, String sessionId, String nodeId, ChannelHandlerContext ctx) {
        Session session = sessions.get(sessionId);
        if (session != null) {
            logger.info("session " + sessionId + " exist!");
            /**
             * 如果在已经建立Connection(1)的Channel上，再建立Connection(2)
             * session.close会将ctx关闭， Connection(2)和Connection(1)的Channel都将会关闭
             * 断线之后再建立连接Connection(3)，由于Session是有一点延迟
             * Connection(3)和Connection(1/2)的Channel不是同一个
             * **/
            // 如果session已经存在则销毁session
            session.close();
            logger.info("session " + sessionId + " have been closed!");
        }
        logger.info("create new session " + sessionId + ", ctx -> " + ctx.toString());

        session = new ExchangeSession();
        session.setMaxQueueSize(maxQueueSize);
        session.setSessionId(sessionId);
        session.setValid(true);
        session.setMaxInactiveInterval(this.getMaxInactiveInterval());
        session.setCreationTime(System.currentTimeMillis());
        session.setLastAccessedTime(System.currentTimeMillis());
        session.setSessionManager(this);
        session.setUserName(userName);
        session.setNodeId(nodeId);
        session.setConnection(createTcpConnection(session, ctx));
        logger.info("create new session " + sessionId + " successful!");

        for (SessionListener listener : sessionListeners) {
            session.addSessionListener(listener);
        }
        logger.debug("add listeners to session " + sessionId + " successful! " + sessionListeners);

        return session;
    }

    protected Connection createTcpConnection(Session session, ChannelHandlerContext ctx) {
        Connection conn = new TcpConnection(ctx);
        conn.setConnectionId(session.getSessionId());
        conn.setSession(session);
        return conn;
    } 

}
