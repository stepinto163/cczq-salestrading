
package com.gateway.connector.tcp.server;

import java.net.InetSocketAddress;
import java.net.SocketAddress;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.proto.Format;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.TcpConnector;
import com.gateway.connector.tcp.config.ServerTransportConfig;
import com.gateway.connector.utils.NetUtils;
import com.gateway.constant.Constants;
import com.gateway.invoke.ApiProxy;
import com.gateway.message.MessageWrapper;
import com.gateway.message.SystemMessage;

import io.netty.channel.ChannelHandler;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;

@ChannelHandler.Sharable
public class TServerHandler extends SimpleChannelInboundHandler<Proto> {

	private final static Logger logger = LoggerFactory.getLogger(TServerHandler.class);

	private TcpConnector tcpConnector = null;
	private ApiProxy proxy = null;
	private boolean login = true;

	public TServerHandler(ServerTransportConfig config) {
		this.tcpConnector = config.getTcpConnector();
		this.proxy = config.getProxy();
		this.login = config.isLogin();
	}

	@Override
	public void channelReadComplete(ChannelHandlerContext ctx) {
		ctx.flush();
	}

	@Override
	protected void channelRead0(ChannelHandlerContext ctx, Proto msg) throws Exception {
		try {

			SystemMessage sMsg = generateSystemMessage(ctx);
			// inbound
			if (msg.getFormat() == Format.REQUEST || msg.getFormat() == Format.SEND) {
				MessageWrapper wrapper = proxy.invoke(sMsg, msg);
				if (wrapper != null)
					receive(ctx, wrapper);
			}

		} catch (Exception e) {
			logger.error("TcpServerHandler handler error.", e);
		}

	}

	public void channelRegistered(ChannelHandlerContext ctx) throws Exception {
		logger.debug("TcpServerHandler Connected from {"
				+ NetUtils.channelToString(ctx.channel().remoteAddress(), ctx.channel().localAddress()) + "}");
	}

	public void channelUnregistered(ChannelHandlerContext ctx) throws Exception {
		logger.debug("TcpServerHandler Disconnected from {"
				+ NetUtils.channelToString(ctx.channel().remoteAddress(), ctx.channel().localAddress()) + "}");
	}

	public void channelActive(ChannelHandlerContext ctx) throws Exception {
		super.channelActive(ctx);
		logger.info("TcpServerHandler channelActive from (" + getRemoteAddress(ctx) + ")");
	}

	public void channelInactive(ChannelHandlerContext ctx) throws Exception {
		super.channelInactive(ctx);
		logger.info("TcpServerHandler channelInactive from (" + getRemoteAddress(ctx) + ")");
		String sessionId0 = getChannelSessionHook(ctx);
		if (StringUtils.isNotBlank(sessionId0)) {
			tcpConnector.close(new MessageWrapper(MessageWrapper.MessageProtocol.CLOSE, sessionId0, null));
			logger.info("TcpServerHandler channelInactive, close channel sessionId0 -> " + sessionId0 + ", ctx -> "
					+ ctx.toString());
		}
	}

	public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) throws Exception {
		logger.info(
				"TcpServerHandler (" + getRemoteAddress(ctx) + ") -> Unexpected exception from downstream." + cause);
		channelInactive(ctx);
	}

	private String getChannelSessionHook(ChannelHandlerContext ctx) {
		return ctx.channel().attr(Constants.SERVER_SESSION_HOOK).get();
	}

	private void setChannelSessionHook(ChannelHandlerContext ctx, String sessionId) {
		ctx.channel().attr(Constants.SERVER_SESSION_HOOK).set(sessionId);
	}

	/**
	 * to send client and receive the message
	 *
	 * @param ctx
	 * @param wrapper
	 */
	private void receive(ChannelHandlerContext ctx, MessageWrapper wrapper) {
		if (wrapper.isConnect()) {
			isConnect0(ctx, wrapper);
		} else if (wrapper.isClose()) {
			tcpConnector.close(wrapper);
		} else if (wrapper.isHeartbeat()) {
			tcpConnector.heartbeatClient(wrapper);
		} else if (wrapper.isRequest()) {
			tcpConnector.responseSendMessage(wrapper);
		} else if (wrapper.isNoKeepAliveMessage()) {
			tcpConnector.responseNoKeepAliveMessage(ctx, wrapper);
		} else if (wrapper.isReply()) {
			tcpConnector.responseSendMessage(wrapper);
		}
	}

	private void isConnect0(ChannelHandlerContext ctx, MessageWrapper wrapper) {
		String sessionId = wrapper.getSessionId();
		String sessionId0 = getChannelSessionHook(ctx);
		if (sessionId.equals(sessionId0)) {
			logger.info("tcpConnector reconnect sessionId -> " + sessionId + ", ctx -> " + ctx.toString());
			tcpConnector.responseSendMessage(wrapper);
		} else {
			logger.info("tcpConnector connect sessionId -> " + sessionId + ", sessionId0 -> " + sessionId0 + ", ctx -> "
					+ ctx.toString());
			tcpConnector.connect(ctx, wrapper);
			setChannelSessionHook(ctx, sessionId);
			logger.info("create channel attr sessionId " + sessionId + " successful, ctx -> " + ctx.toString());
		}
	}

	private SystemMessage generateSystemMessage(ChannelHandlerContext ctx) {
		SystemMessage systemMessage = new SystemMessage();
		systemMessage.setRemoteAddress(getRemoteIp(ctx));
		systemMessage.setLocalAddress(getLocalIp(ctx));
		systemMessage.setLogin(login);
		return systemMessage;
	}
	private String getLocalIp(ChannelHandlerContext ctx) {
		SocketAddress local1 = ctx.channel().localAddress();
		InetSocketAddress local = (InetSocketAddress) local1;
		return NetUtils.toIpString(local);
	}
	private String getRemoteIp(ChannelHandlerContext ctx) {
		SocketAddress remote1 = ctx.channel().remoteAddress();
		InetSocketAddress remote = (InetSocketAddress) remote1;
		return NetUtils.toIpString(remote);
	}
	private String getRemoteAddress(ChannelHandlerContext ctx) {
		SocketAddress remote1 = ctx.channel().remoteAddress();
		InetSocketAddress remote = (InetSocketAddress) remote1;
		return NetUtils.toAddressString(remote);
	}

	private String getLocalAddress(ChannelHandlerContext ctx) {
		SocketAddress local1 = ctx.channel().localAddress();
		InetSocketAddress local = (InetSocketAddress) local1;
		return NetUtils.toAddressString(local);
	}

}
