package com.gateway.invoke.security;

import com.gateway.connector.proto.Proto;
import com.gateway.message.SystemMessage;

/**
 * 安全检查接口
 * 
 * @author deshuai.kong
 *
 */
public interface ISecurityCheck {
	SecurityResult check(SystemMessage sMsg, Proto message, String serverName, String method, String content);
}
