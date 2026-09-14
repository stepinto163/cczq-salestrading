package com.gateway.connector.tcp.server;

import com.gateway.connector.tcp.codec.WebSocketProtoCodec;
import com.gateway.connector.tcp.config.ServerTransportConfig;

import io.netty.channel.ChannelHandler;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.socket.SocketChannel;
import io.netty.handler.codec.http.HttpObjectAggregator;
import io.netty.handler.codec.http.HttpServerCodec;
import io.netty.handler.codec.http.cors.CorsConfig;
import io.netty.handler.codec.http.cors.CorsConfigBuilder;
import io.netty.handler.codec.http.cors.CorsHandler;
import io.netty.handler.codec.http.websocketx.WebSocketServerProtocolHandler;
import io.netty.handler.stream.ChunkedWriteHandler;

@ChannelHandler.Sharable
public class WebSocketServerInitializer extends ChannelInitializer<SocketChannel> {

	private ServerTransportConfig config;

	public WebSocketServerInitializer(ServerTransportConfig config) {
		this.config = config;
	}

	@Override
	protected void initChannel(SocketChannel socketChannel) throws Exception {

		ChannelPipeline pipeline = socketChannel.pipeline();
	 
        pipeline.addLast(new HttpServerCodec());
       
        pipeline.addLast(new ChunkedWriteHandler());
       
        pipeline.addLast(new HttpObjectAggregator(64 * 1024));
 
        pipeline.addLast(new WebSocketServerProtocolHandler("/gateway"));
         
		pipeline.addLast(new WebSocketProtoCodec(config.isGzip()));
		pipeline.addLast(new TServerHandler(config));
	}
}