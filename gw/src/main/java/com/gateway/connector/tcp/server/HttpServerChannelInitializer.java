package com.gateway.connector.tcp.server;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.tcp.config.ServerTransportConfig;

import io.netty.channel.ChannelHandler;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.socket.SocketChannel;
import io.netty.handler.codec.http.HttpObjectAggregator;
import io.netty.handler.codec.http.HttpServerCodec;

@ChannelHandler.Sharable
public class HttpServerChannelInitializer extends ChannelInitializer<SocketChannel> {
	private ServerTransportConfig config;
	private final static Logger _logger = LoggerFactory.getLogger(HttpServerChannelInitializer.class);
	 
	public HttpServerChannelInitializer(ServerTransportConfig config) {
		this.config = config;
	}

	 

 

	@Override
	protected void initChannel(SocketChannel socketChannel) throws Exception {

		 

		ChannelPipeline pipeline = socketChannel.pipeline();

		 
		 pipeline.addLast(new HttpServerCodec());// http 编解码
	        pipeline.addLast("httpAggregator",new HttpObjectAggregator(512*1024)); // http 消息聚合器
 
		pipeline.addLast(new HttpServerHandler(config));
	}

}
