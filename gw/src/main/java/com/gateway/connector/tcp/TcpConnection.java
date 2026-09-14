
package com.gateway.connector.tcp;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.api.ExchangeConnection;
import com.gateway.constant.Constants;
import com.gateway.exception.LostConnectException;
import com.gateway.exception.PushException;

import io.netty.channel.ChannelHandlerContext;

public class TcpConnection<T> extends ExchangeConnection<T> {

	private final static Logger logger = LoggerFactory.getLogger(TcpConnection.class);

	private ChannelHandlerContext cxt;

	public TcpConnection(ChannelHandlerContext cxt) {
		this.cxt = cxt;
	}

	public void connect() {
	}

	public void close() {
		this.close = true;

		if (cxt != null) {
			cxt.close();
			logger.debug("the connection have been destroyed! ctx -> " + cxt.toString());
		}
	}

	public void send(T message) {
		if (message == null)
			return;

		sendMessage(message);
	}

	public boolean isWritable() {
		return cxt != null && cxt.channel() != null && cxt.channel().isWritable();
	}

	public int getOutboundQueueSize() {
		if (cxt == null || cxt.channel() == null || cxt.channel().unsafe() == null
				|| cxt.channel().unsafe().outboundBuffer() == null) {
			return 0;
		}
		return cxt.channel().unsafe().outboundBuffer().size();
	}

	private void sendMessage(T message) {
		if (isClosed()) {
			return;
		}
		pushMessage0(message);
	}

	private void pushMessage0(T message) {
		try {
			ChannelHandlerContext ctx = cxt;
			if (ctx == null || ctx.channel() == null || !ctx.channel().isActive()) {
				logger.info("skip send on inactive channel, connectionId:{}", connectionId);
				return;
			}
			if (session != null && session.getMaxQueueSize() > 0) {
				int size = getOutboundQueueSize();
				if (size == 0 && ctx.channel().unsafe() != null && ctx.channel().unsafe().outboundBuffer() == null) {
					logger.info("skip send because outboundBuffer is null, connectionId:{}", connectionId);
					return;
				}
				if (size > 100 && size % 100 == 0) {
					logger.warn("{} outbound queue size:{}", session.getUserName(), size);
				}
				if (size > session.getMaxQueueSize()) {
					session.close();
					logger.warn("{} outbound queue size:{},session close", session.getUserName(), size);
					return;
				}
			}
			ctx.writeAndFlush(message);
			if (session != null) {
				session.access();
			}
		} catch (LostConnectException e) {
			logger.warn("TcpConnection pushMessage occur LostConnectException.", e);
			this.fireError(new PushException(e));
		} catch (Exception e) {
			logger.warn("TcpConnection pushMessage occur Exception.", e);
			this.fireError(new PushException("ChannelFuture " + connectionId + " ", e));
		} catch (Throwable e) {
			logger.warn("TcpConnection pushMessage occur Throwable.", e);
			this.fireError(new PushException("Failed to send message, cause: " + e.getMessage(), e));
		}
	}

}
