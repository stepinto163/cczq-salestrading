package com.gateway.connector.tcp.server;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.tcp.config.ServerTransportConfig;
import com.gateway.connector.utils.ThreadNameFactory;
import com.gateway.exception.InitErrorException;

import io.netty.bootstrap.ServerBootstrap;
import io.netty.buffer.PooledByteBufAllocator;
import io.netty.channel.ChannelFuture;
import io.netty.channel.ChannelFutureListener;
import io.netty.channel.ChannelOption;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.WriteBufferWaterMark;
import io.netty.channel.nio.NioEventLoopGroup;
import io.netty.channel.socket.nio.NioServerSocketChannel;

 
public class WebSocketServer  {

	 private final static Logger logger = LoggerFactory.getLogger(TServer.class);

	    private ServerTransportConfig serverConfig;
	    public ServerTransportConfig getServerConfig() {
			return serverConfig;
		}
	    private int port;

	    public int getPort() {
			return port;
		}
	    private String host;
		public String getHost() {
			return host;
		}

		public void setHost(String host) {
			this.host = host;
		}
		private static final int BIZ_GROUP_SIZE = Runtime.getRuntime().availableProcessors() * 2;
	    private static final int BIZ_THREAD_SIZE = Runtime.getRuntime().availableProcessors() *4;

	    private final EventLoopGroup bossGroup = new NioEventLoopGroup(BIZ_GROUP_SIZE,new ThreadNameFactory(this.getClass().getSimpleName()+" Boss"));
	    private final EventLoopGroup workerGroup = new NioEventLoopGroup(BIZ_THREAD_SIZE,new ThreadNameFactory(this.getClass().getSimpleName()+" Worker"));

	    public void init() throws Exception {
	        boolean flag = Boolean.FALSE;
	        logger.info("start websocket server ...");

	        Class clazz = NioServerSocketChannel.class;
	        // Server 服务启动
	        ServerBootstrap bootstrap = new ServerBootstrap();

	        bootstrap.group(bossGroup, workerGroup);
	        bootstrap.channel(clazz);
	        bootstrap.childHandler(new WebSocketServerInitializer(serverConfig));
	        // 可选参数
	        bootstrap  .option(ChannelOption.SO_REUSEADDR, Boolean.TRUE)
	        .childOption(ChannelOption.TCP_NODELAY, true)
	        .childOption(ChannelOption.ALLOCATOR, new PooledByteBufAllocator(false))
	        .childOption(ChannelOption.WRITE_BUFFER_WATER_MARK,
	                new WriteBufferWaterMark(serverConfig.getWriteBufferLowWaterMark(),
	                        serverConfig.getWriteBufferHighWaterMark()));

	        // 绑定接口，同步等待成功
	        ChannelFuture future = StringUtils.isNotBlank(host)?bootstrap.bind(host,port).sync():bootstrap.bind(port).sync();
	        ChannelFuture channelFuture = future.addListener(new ChannelFutureListener() {
	            public void operationComplete(ChannelFuture future) throws Exception {
	                if (future.isSuccess()) {
	        	        logger.warn("start websocket server at port[" + port + "].");
	                } else {
	                    logger.error("websocket server fail bind to " + port);
	                    throw new InitErrorException("websocket server start fail !", future.cause());
	                }
	            }
	        });
	    }

	    public void shutdown() {
	        logger.info("shutdown websocket server ...");
	        // 释放线程池资源
	        bossGroup.shutdownGracefully();
	        workerGroup.shutdownGracefully();
	        logger.info("shutdown websocket server end.");
	    }

	    //------------------ set && get --------------------


	    public void setServerConfig(ServerTransportConfig serverConfig) {
	        this.serverConfig = serverConfig;
	    }

	    public void setPort(int port) {
	        this.port = port;
	    }
}
