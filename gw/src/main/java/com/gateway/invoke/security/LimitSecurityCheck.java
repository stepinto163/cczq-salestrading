package com.gateway.invoke.security;

import org.apache.commons.lang3.StringUtils;

import com.gateway.connector.Session;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.TcpSessionManager;
import com.gateway.message.SystemMessage;

public class LimitSecurityCheck implements ISecurityCheck {
	private static String LimitCheck = "10003";
	private static String LimitCheckMsg = "用户%s超过限流";
	private int limitQps = 100;
	private TcpSessionManager tcpSessionManager;

	public void setTcpSessionManager(TcpSessionManager tcpSessionManager) {
		this.tcpSessionManager = tcpSessionManager;
	}

	public int getLimitQps() {
		return limitQps;
	}

	public void setLimitQps(int limitQps) {
		this.limitQps = limitQps;
	}

	public void init() {

	}

	@Override
	public SecurityResult check(SystemMessage sMsg, Proto message, String serverName, String method, String content) {
		SecurityResult pr = new SecurityResult();
		if (StringUtils.isNoneBlank(message.getSessionId())) {// 限流
			Session session = tcpSessionManager.getSession(message.getSessionId());
			if (session != null) {
				String userName = session.getUserName() + "";
				boolean flag = LimitApi.LimitCheck(userName, limitQps);
				if (!flag) {
					pr.Code = LimitCheck;
					pr.Msg = String.format(LimitCheckMsg, userName);
				}
			}
		}
		return pr;
	}

}
