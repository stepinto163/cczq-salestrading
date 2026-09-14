 
package com.gateway.connector.tcp.server;

import com.gateway.connector.tcp.codec.TcpProtoDecoder;
import com.gateway.connector.tcp.codec.TcpProtoEncoder;
import com.gateway.connector.tcp.config.ServerTransportConfig;

import io.netty.channel.ChannelHandler;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.socket.SocketChannel;
import io.netty.handler.logging.LoggingHandler;

@ChannelHandler.Sharable
public class SChannelInitializer extends ChannelInitializer<SocketChannel> {

	private ServerTransportConfig config;

	public SChannelInitializer(ServerTransportConfig config) {
		this.config = config;
	}

	@Override
	protected void initChannel(SocketChannel socketChannel) throws Exception {

		ChannelPipeline pipeline = socketChannel.pipeline();
		pipeline.addLast(new LoggingHandler());

		pipeline.addLast("decoder", new TcpProtoDecoder(config.isGzip()));
		pipeline.addLast("encoder", new TcpProtoEncoder(config.isGzip()));

//		pipeline.addLast(new TcpProtoCodec());
		pipeline.addLast(new TServerHandler(config));

	}
}
