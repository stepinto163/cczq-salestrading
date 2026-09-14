package com.gateway.connector.tcp.server;

import static io.netty.handler.codec.http.HttpResponseStatus.CONTINUE;
import static io.netty.handler.codec.http.HttpResponseStatus.OK;
import static io.netty.handler.codec.http.HttpVersion.HTTP_1_1;

import java.net.InetSocketAddress;
import java.net.SocketAddress;
import java.util.Set;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.app.common.thread.AsyncSeqThreadGroup;
import com.app.common.utils.Consts;
import com.gateway.connector.proto.Cmd;
import com.gateway.connector.proto.Format;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.config.ServerTransportConfig;
import com.gateway.connector.utils.NetUtils;
import com.gateway.invoke.ApiProxy;
import com.gateway.invoke.ceda.CedaApi;
import com.gateway.message.MessageWrapper;
import com.gateway.message.SystemMessage;

import ats.app.api.BaseApi.IMessage;
import io.netty.buffer.ByteBuf;
import io.netty.buffer.Unpooled;
import io.netty.channel.ChannelFutureListener;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;
import io.netty.handler.codec.http.DefaultFullHttpResponse;
import io.netty.handler.codec.http.FullHttpRequest;
import io.netty.handler.codec.http.FullHttpResponse;
import io.netty.handler.codec.http.HttpContent;
import io.netty.handler.codec.http.HttpHeaderNames;
import io.netty.handler.codec.http.HttpHeaderValues;
import io.netty.handler.codec.http.HttpMethod;
import io.netty.handler.codec.http.HttpRequest;
import io.netty.handler.codec.http.HttpResponse;
import io.netty.handler.codec.http.HttpUtil;
import io.netty.handler.codec.http.cookie.ServerCookieDecoder;
import io.netty.util.CharsetUtil;

public class HttpServerHandler extends SimpleChannelInboundHandler<FullHttpRequest> {
	private HttpRequest request;

	private final static Logger logger = LoggerFactory.getLogger(HttpServerHandler.class);
	private ApiProxy proxy = null;
	private static AsynHttpMsg asynHttpMsg = new AsynHttpMsg(Runtime.getRuntime().availableProcessors() * 4);

	public HttpServerHandler(ServerTransportConfig config) {
		this.proxy = config.getProxy();
	}

	private final static String health = "/health";

	@Override
	protected void channelRead0(ChannelHandlerContext ctx, FullHttpRequest msg) throws Exception {

		if (msg instanceof HttpRequest) {
			// 记录下request
			request = (HttpRequest) msg;
			if (HttpUtil.is100ContinueExpected(request)) {
				send100Continue(ctx);
			}
		}
		SystemMessage sMsg = generateSystemMessage(ctx,request);
		if (msg instanceof HttpContent) {
			HttpContent content = (HttpContent) msg;
			String sessionId = request.headers().get("sessionId");
			String instId = request.headers().get("companyId");
			String url = request.uri().toString();
			if (!url.endsWith(health)) {
				if (StringUtils.isEmpty(sessionId)) {
					sessionId = "";
				}
				ByteBuf buf = content.content();
				if (buf.isReadable()) {
					String body = buf.toString(CharsetUtil.UTF_8);
					logger.info("request:{}->remoteaddress:{},content:{}", sessionId, sMsg.getRemoteAddress(), body);
				}
				if (isMcpRequest(request) && !buf.isReadable()) {
					String emptyReq = "{\"jsonrpc\":\"2.0\",\"id\":null,\"error\":{\"code\":-32600,\"message\":\"empty request body\"}}";
					Proto mcpReply = new Proto();
					mcpReply.setBody(emptyReq.getBytes(CharsetUtil.UTF_8));
					HttpResponse httpResponse = processMessage(mcpReply, request);
					writeResponse(request, httpResponse, sessionId, ctx);
					return;
				}
				Proto message = new Proto();
				int seq = 0;

				message.setCmd(Cmd.SEND);
				message.setSeq(seq);
				message.setFormat(Format.REQUEST);
				message.setSessionId(sessionId);
				message.setCompanyId(instId);
				byte[] body = new byte[buf.readableBytes()];
				MessageWrapper mr = null;
				if (body.length != 0) {
					buf.readBytes(body);
					message.setBody(body);
					HttpCallBack callback = new HttpCallBack(sMsg, message, request);
					mr = proxy.invokeAsync(sMsg, message, callback);
				}
				if (mr != null) {
					HttpResponse httpResponse = processMessage((Proto) mr.getBody(), request);
					writeResponse(request, httpResponse, sessionId, ctx);
				} else {

					allowOrigin(ctx, msg);
				}
			} else {
				HttpResponse httpResponse = processMessage(null, request);
				writeResponse(request, httpResponse, sessionId, ctx);
			}

		}
	}

	private void allowOrigin(ChannelHandlerContext ctx, FullHttpRequest request) {

		//浏览器跨域测试消息
		if (request.method().equals(HttpMethod.OPTIONS)) {

			//允许跨域访问
			String origin = request.headers().get(HttpHeaderNames.ORIGIN);
			if (StringUtils.isBlank(origin)) {
				origin = request.headers().get("Referer");
			}
			origin = origin == null ? "" : origin;
			
			logger.warn("allowOrigin request:{}, method:{}", origin, request.method());

			FullHttpResponse response = null;
			try {
				response = new DefaultFullHttpResponse(HTTP_1_1, OK);

				//返回是字符串
				//response.headers().set(CONTENT_TYPE, "text/plain;charset=UTF-8");

				//返回值是json
				response.headers().set(HttpHeaderNames.CONTENT_TYPE, "application/json;charset=UTF-8");

				origin = origin.endsWith("/") ? origin.substring(0, origin.length() - 1) : origin;
				response.headers().set(HttpHeaderNames.ACCESS_CONTROL_ALLOW_ORIGIN, origin);//"http://localhost:8090");
				response.headers().set(HttpHeaderNames.ACCESS_CONTROL_ALLOW_HEADERS, "Origin, X-Requested-With, Content-Type, Accept, sessionId, apikey");
				response.headers().set(HttpHeaderNames.ACCESS_CONTROL_ALLOW_METHODS, "GET, POST, PUT,DELETE,OPTIONS");
				response.headers().set(HttpHeaderNames.ACCESS_CONTROL_ALLOW_CREDENTIALS, "true");

				response.headers().set(HttpHeaderNames.CONTENT_LENGTH, response.content().readableBytes());
				if (HttpUtil.isKeepAlive(request)) {
					response.headers().set(HttpHeaderNames.CONNECTION, HttpHeaderValues.KEEP_ALIVE);
				}
				ctx.write(response);
				ctx.flush();

			} catch (Exception e) {
				logger.error("Exception:{}", e);
			}
		}
	}

	public static class AsynHttpMsg extends AsyncSeqThreadGroup<HttpMsg> {

		public AsynHttpMsg(int asyncBaseNum) {
			super(asyncBaseNum);
		}

		@Override
		protected void process(HttpMsg msg) {
			HttpRequest request = msg.request;
			String content = msg.content;
			SystemMessage systemMessage = msg.systemMessage;
			Proto message = msg.message;
			if (StringUtils.isEmpty(content)) {
				content = CedaApi.faileMsg(Consts.NoKnowCode, "timeout");
			}
			logger.info("response:{}->remoteaddress:{},content:{}", message.getSessionId(),
					systemMessage.getRemoteAddress(), content);

			message.setBody(content.getBytes());
			message.setFormat(Format.REPLY);
			HttpResponse httpResponse = processMessage(message, request);
			writeResponse(msg.request, httpResponse, message.getSessionId(), systemMessage.getChannelHandlerContext());

		}
	}

	public class HttpMsg {
		public String sid;
		public SystemMessage systemMessage;
		public Proto message;
		public String content;
		public HttpRequest request;
	}

	public class HttpCallBack implements IMessage<String> {
		private SystemMessage systemMessage;
		private Proto message;
		private HttpRequest request;

		public HttpRequest getRequest() {
			return request;
		}

		public HttpCallBack(SystemMessage systemMessage, Proto proto, HttpRequest request) {
			this.message = proto;
			this.systemMessage = systemMessage;
			this.request = request;
		}

		@Override
		public void onMessage(String sid, String content) {
			HttpMsg msg = new HttpMsg();
			msg.request = request;
			msg.content = content;
			msg.sid = sid;
			msg.systemMessage = systemMessage;
			msg.message = message;
			asynHttpMsg.add(sid + "", msg);
		}

		public SystemMessage getSystemMessage() {
			return systemMessage;
		}

		public void setSystemMessage(SystemMessage systemMessage) {
			this.systemMessage = systemMessage;
		}
	}

	private SystemMessage generateSystemMessage(ChannelHandlerContext ctx,HttpRequest request) {
		SystemMessage systemMessage = new SystemMessage();
		systemMessage.setChannelHandlerContext(ctx);
		systemMessage.setRemoteAddress(getIpAddr(ctx,request));
		systemMessage.setLocalAddress(getLocalAddress(ctx));
		systemMessage.setSyncFlag(true);
		return systemMessage;
	}
	public String getIpAddr(ChannelHandlerContext ctx,HttpRequest request) {



		String ip = request.headers().get("x-forwarded-for");

		if(StringUtils.isNoneBlank(ip)&&!"unknown".equalsIgnoreCase(ip)) {
			String[] ips=ip.split(",");
			if(ips.length>0) {
				ip=ips[0];
			}
		}

		logger.debug("x-forwarded-for:{}",ip);
		if (StringUtils.isBlank(ip) || "unknown".equalsIgnoreCase(ip)) {
			ip = request.headers().get("Proxy-Client-IP");
		}
		if (StringUtils.isBlank(ip)|| "unknown".equalsIgnoreCase(ip)) {
			ip = request.headers().get("WL-Proxy-Client-IP");
		}
		if (StringUtils.isBlank(ip)|| "unknown".equalsIgnoreCase(ip)) {
			ip=getRemoteAddress(ctx);
		}

		return ip;
	}

	private boolean isMcpRequest(HttpRequest request) {
		if (request == null) {
			return false;
		}
		String uri = request.uri();
		if (StringUtils.isBlank(uri)) {
			return false;
		}
		String path = uri;
		int schemeIdx = path.indexOf("://");
		if (schemeIdx >= 0) {
			int slashIdx = path.indexOf('/', schemeIdx + 3);
			path = slashIdx >= 0 ? path.substring(slashIdx) : "/";
		}
		int queryIdx = path.indexOf('?');
		if (queryIdx >= 0) {
			path = path.substring(0, queryIdx);
		}
		path = path.trim();
		return "/mcp".equals(path) || path.startsWith("/mcp/");
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

	public static HttpResponse processMessage(Proto proto, HttpRequest request) {

		FullHttpResponse httpResponse = null;
		byte[] bt = null;
		boolean mcpRequest = isMcpRequestStatic(request);

		if (proto != null) {

			bt = proto.getBody();
		} else {
			bt = OK.toString().getBytes();
		}
		try {
			boolean emptyBody = bt == null || bt.length == 0;
			httpResponse = new DefaultFullHttpResponse(HTTP_1_1,
					(mcpRequest && emptyBody) ? io.netty.handler.codec.http.HttpResponseStatus.ACCEPTED : OK,
					Unpooled.copiedBuffer(bt == null ? new byte[0] : bt));
			httpResponse.headers().set(HttpHeaderNames.CONTENT_TYPE, "application/json;charset=UTF-8");
		} catch (Exception e) {
			e.printStackTrace();
		}

		return httpResponse;
	}

	@Override
	public void channelReadComplete(ChannelHandlerContext ctx) {
		ctx.flush();
	}

	private static void send100Continue(ChannelHandlerContext ctx) {
		FullHttpResponse response = new DefaultFullHttpResponse(HTTP_1_1, CONTINUE);
		ctx.write(response);
	}

	@Override
	public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) {
		cause.printStackTrace();
		ctx.close();
	}

	private static boolean writeResponse(HttpRequest request, HttpResponse response, String sessionId,
			ChannelHandlerContext ctx) {
		boolean keepAlive = HttpUtil.isKeepAlive(request);
		boolean mcpRequest = isMcpRequestStatic(request);
		if (mcpRequest) {
			keepAlive = true;
		}
		try {

			FullHttpResponse fullHttpResponse = (FullHttpResponse) response;

			response.headers().setInt(HttpHeaderNames.CONTENT_LENGTH, fullHttpResponse.content().readableBytes());
			// 允许跨域访问
			String origin = request.headers().get(HttpHeaderNames.ORIGIN);
			if (origin != null) {
				response.headers().set(HttpHeaderNames.ACCESS_CONTROL_ALLOW_ORIGIN, origin);
			}
			response.headers().set(HttpHeaderNames.ACCESS_CONTROL_ALLOW_HEADERS,
					"Content-Type,Access-Token,Appid,Secret,Authorization,sessionId");// 允许headers自定义
			response.headers().set(HttpHeaderNames.ACCESS_CONTROL_ALLOW_METHODS, "GET,POST,PUT,DELETE,OPTIONS");
			response.headers().set(HttpHeaderNames.ACCESS_CONTROL_ALLOW_CREDENTIALS, "true");
			response.headers().set(HttpHeaderNames.CACHE_CONTROL, "expires");
			response.headers().set(HttpHeaderNames.ACCESS_CONTROL_MAX_AGE, "3600");

			if (keepAlive) {

				response.headers().set(HttpHeaderNames.CONNECTION, HttpHeaderValues.KEEP_ALIVE);
			}

			String cookieString = request.headers().get(HttpHeaderNames.COOKIE);
			if (cookieString != null) {
				Set<io.netty.handler.codec.http.cookie.Cookie> cookies = ServerCookieDecoder.STRICT
						.decode(cookieString);
				if (!cookies.isEmpty()) {
					for (io.netty.handler.codec.http.cookie.Cookie cookie : cookies) {
						response.headers().add(HttpHeaderNames.SET_COOKIE,
								io.netty.handler.codec.http.cookie.ServerCookieEncoder.STRICT.encode(cookie));
					}
				}
			} else {
				if (!StringUtils.isEmpty(sessionId)) {

					response.headers().add(HttpHeaderNames.SET_COOKIE,
							io.netty.handler.codec.http.cookie.ServerCookieEncoder.STRICT.encode("sessionId",
									sessionId));
				}
			}

			if (!keepAlive) {
				ctx.writeAndFlush(response).addListener(ChannelFutureListener.CLOSE);
			} else {
				ctx.writeAndFlush(response);
			}

		} catch (Exception e) {
			logger.debug("", e);
		}
		return keepAlive;
	}

	private static boolean isMcpRequestStatic(HttpRequest request) {
		if (request == null) {
			return false;
		}
		String uri = request.uri();
		if (StringUtils.isBlank(uri)) {
			return false;
		}
		String path = uri;
		int schemeIdx = path.indexOf("://");
		if (schemeIdx >= 0) {
			int slashIdx = path.indexOf('/', schemeIdx + 3);
			path = slashIdx >= 0 ? path.substring(slashIdx) : "/";
		}
		int queryIdx = path.indexOf('?');
		if (queryIdx >= 0) {
			path = path.substring(0, queryIdx);
		}
		path = path.trim();
		return "/mcp".equals(path) || path.startsWith("/mcp/");
	}
}
