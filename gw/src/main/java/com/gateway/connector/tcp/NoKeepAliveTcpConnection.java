
package com.gateway.connector.tcp;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.exception.LostConnectException;

import io.netty.channel.ChannelHandlerContext;

public class NoKeepAliveTcpConnection<T> {

	private final static Logger logger = LoggerFactory.getLogger(NoKeepAliveTcpConnection.class);

	private ChannelHandlerContext cxt;

	public NoKeepAliveTcpConnection(ChannelHandlerContext cxt) {
		this.cxt = cxt;
	}

	public void close() {
		cxt.close();
		logger.info("the connection have been destroyed!");
	}

	public void send(T message) {
		sendMessage(message);
	}

	private void sendMessage(T message) {
		pushMessage(message);
	}

	private void pushMessage(T message) {

		try {
			cxt.writeAndFlush(message);
		} catch (LostConnectException e) {
			logger.error("TcpConnection pushMessage occur LostConnectException.", e);

		} catch (Exception e) {
			logger.error("TcpConnection pushMessage occur Exception.", e);

		} catch (Throwable e) {
			logger.error("TcpConnection pushMessage occur Throwable.", e);

		}

	}
}
