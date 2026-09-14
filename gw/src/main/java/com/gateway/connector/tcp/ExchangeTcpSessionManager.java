 
package com.gateway.connector.tcp;

import com.gateway.connector.Session;
import com.gateway.connector.api.ExchangeSessionManager;

import io.netty.channel.ChannelHandlerContext;

public abstract class ExchangeTcpSessionManager extends ExchangeSessionManager {

    public abstract Session createSession(String userName,String sessionId, String nodeId, ChannelHandlerContext ctx);

}
