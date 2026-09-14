 
package com.gateway.message;

import java.io.Serializable;

import io.netty.channel.ChannelHandlerContext;

public class SystemMessage implements Serializable {
	private ChannelHandlerContext channelHandlerContext;
    public ChannelHandlerContext getChannelHandlerContext() {
		return channelHandlerContext;
	}

	public void setChannelHandlerContext(ChannelHandlerContext channelHandlerContext) {
		this.channelHandlerContext = channelHandlerContext;
	}

	private String remoteAddress;
    private String localAddress;
    private boolean login=true;
    public boolean isLogin() {
		return login;
	}

	public void setLogin(boolean login) {
		this.login = login;
	}
	private boolean apiKeyCheck=false;
    public boolean isApiKeyCheck() {
		return apiKeyCheck;
	}

	public void setApiKeyCheck(boolean apiKeyCheck) {
		this.apiKeyCheck = apiKeyCheck;
	}

	private String apiKey;
    public String getApiKey() {
		return apiKey;
	}

	public void setApiKey(String apiKey) {
		this.apiKey = apiKey;
	}

	private boolean syncFlag=false;
    public boolean isSyncFlag() {
		return syncFlag;
	}

	public void setSyncFlag(boolean syncFlag) {
		this.syncFlag = syncFlag;
	}

	public String getRemoteAddress() {
        return remoteAddress;
    }

    public void setRemoteAddress(String remoteAddress) {
        this.remoteAddress = remoteAddress;
    }

    public String getLocalAddress() {
        return localAddress;
    }

    public void setLocalAddress(String localAddress) {
        this.localAddress = localAddress;
    }
}
