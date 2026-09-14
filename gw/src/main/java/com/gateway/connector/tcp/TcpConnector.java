
package com.gateway.connector.tcp;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.Session;
import com.gateway.connector.tcp.listener.TcpHeartbeatListener;
import com.gateway.message.MessageWrapper;

import io.netty.channel.ChannelHandlerContext;

public class TcpConnector extends ExchangeTcpConnector<Object> {

	private final static Logger logger = LoggerFactory.getLogger(TcpConnector.class);

	private TcpHeartbeatListener tcpHeartbeatListener = null;

	public void init() {
		tcpHeartbeatListener = new TcpHeartbeatListener(tcpSessionManager);

		Thread heartbeatThread = new Thread(tcpHeartbeatListener, "tcpHeartbeatListener");
		heartbeatThread.setDaemon(true);
		heartbeatThread.start();
		logger.info("TcpConnector init");
	}

	public void destroy() {
		tcpHeartbeatListener.stop();

		for (Session session : tcpSessionManager.getSessions()) {
			session.close();
		}
		tcpSessionManager = null;
		logger.info("TcpConnector destroy");
	}

	@Override
	public void connect(ChannelHandlerContext ctx, MessageWrapper wrapper) {
		try {
			Session session = tcpSessionManager.createSession(wrapper.getUserName(), wrapper.getSessionId(), wrapper.getNodeId(), ctx);
			session.setClientType(wrapper.getClientType());
			session.setInfo(wrapper.getExtra());
			session.addSessionListener(tcpHeartbeatListener);
			session.connect();
			logger.warn("{} {} connect", session.getUserName(), session.getSessionId());
			tcpSessionManager.addSession(session);
			/** send **/
			session.getConnection().send(wrapper.getBody());
		} catch (Exception e) {
			logger.error("TcpConnector connect occur Exception.", e);
		}
	}

	@Override
	public void close(MessageWrapper wrapper) {
		Session session = tcpSessionManager.getSession(wrapper.getSessionId());
		if (session != null) {
			logger.warn("{} {} close", session.getUserName(), session.getSessionId());
			session.getConnection().send(wrapper.getBody());
			session.close();

		}
	}

	@Override
	public void heartbeatClient(MessageWrapper wrapper) {
		try {
			Session session = tcpSessionManager.getSession(wrapper.getSessionId());
			if (session != null) {
				session.access();
				session.getConnection().send(wrapper.getBody());
			}
		} catch (Exception e) {
			logger.error("TcpConnector heartbeatClient occur Exception.", e);
		}
	}

	@Override
	public void responseSendMessage(MessageWrapper wrapper) {
		try {
			Session session = tcpSessionManager.getSession(wrapper.getSessionId());
			if (session != null) {
				session.getConnection().send(wrapper.getBody());
			} else {
				logger.info("{} is not exist.", wrapper.getSessionId());
			}
		} catch (Exception e) {
			logger.error("TcpConnector responseSendMessage occur Exception.", e);
		}
	}

	@Override
	public void responseNoKeepAliveMessage(ChannelHandlerContext ctx, MessageWrapper wrapper) {
		try {
			NoKeepAliveTcpConnection connection = new NoKeepAliveTcpConnection(ctx);
			connection.send(wrapper.getBody());
		} catch (Exception e) {
			logger.error("TcpConnector responseNoKeepAliveMessage occur Exception.", e);
		}
	}
}
