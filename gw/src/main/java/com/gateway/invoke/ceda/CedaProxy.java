package com.gateway.invoke.ceda;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.nio.charset.StandardCharsets;

import com.alibaba.fastjson.JSON;
import com.gateway.connector.api.listener.SessionEvent;
import com.gateway.connector.api.listener.SessionListener;
import com.gateway.connector.proto.Format;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.utils.JsonUtils;
import com.gateway.invoke.AbstractApiProxy;
import com.gateway.marketdata.MarketDataBurstConfig;
import com.gateway.mcp.McpGatewayAdapter;
import com.gateway.message.MessageWrapper;
import com.gateway.message.SystemMessage;
import com.gateway.notify.NotifyProxy;

import ats.app.api.BaseApi.IMessage;
import ats.app.ym.ex.utils.StringUtils;
import ats.foundation.utils.exception.CodeException;
import io.netty.handler.codec.http.HttpRequest;

public class CedaProxy extends AbstractApiProxy implements SessionListener {
    private  McpGatewayAdapter mcpGatewayAdapter ;

    public McpGatewayAdapter getMcpGatewayAdapter() {
		return mcpGatewayAdapter;
	}

    public void setMcpGatewayAdapter(McpGatewayAdapter mcpGatewayAdapter) {
		this.mcpGatewayAdapter = mcpGatewayAdapter;
	}

	public NotifyProxy notifyProxy;

    public NotifyProxy getNotifyProxy() {
        return notifyProxy;
    }

    public void setNotifyProxy(NotifyProxy notifyProxy) {
        this.notifyProxy = notifyProxy;
    }

    public void setRequestService(String requestService) {
        CedaApi.requestService = requestService;
    }

    public void setSubscribes(HashMap<String, String> hm) {
        CedaApi.subscribes = hm;
    }

    public void setMarketDataBurstConfig(MarketDataBurstConfig marketDataBurstConfig) {
        CedaApi.setMarketDataBurstConfig(marketDataBurstConfig);
    }

    public String nodeFilePath;

    public Boolean usePublicNode;

    public void setNodeFilePath(String nodeFilePath) {
        this.nodeFilePath = nodeFilePath;
    }

//    public void setProxyNodes(String proxyNodes) {
//        CedaApi.proxyNodes = proxyNodes;
//    }

    public void setUsePublicNode(Boolean usePublicNode) {
        this.usePublicNode = usePublicNode;
        CedaApi.usePublicNode = usePublicNode;
    }

    public void init() {
    	if(mcpGatewayAdapter!=null) {
 
        mcpGatewayAdapter.init();
    	}
        CedaApi.tcpConnector = this.tcpConnector;
        CedaApi.notifyProxy = this.notifyProxy;
        CedaApi.cedaProxy = this;
        try {

            CedaApi.init(executorService, nodeFilePath);
        } catch (Exception e) {
            logger.error("CedaApi init error {}", e.getMessage());
        }
    }
    
    public void reloadNodes() {
        CedaApi.reloadNodes(executorService, nodeFilePath);
    }

    @SuppressWarnings("unchecked")
    @Override
    public MessageWrapper processLogin(SystemMessage sMsg, Proto message, String content) {
        try {
            logger.info("processLogin message:{} content:{}", JSON.toJSONString(message), content);
            String sid = message.getSessionId();
            String result = null;
            String userName = null;
            String clientType = "";
            if (StringUtils.isEmpty(content) && !StringUtils.isEmpty(sid)) {
                result = CedaApi.getUserInfo(sid);
            } else {
                HashMap<String, Object> hm = JSON.parseObject(content, HashMap.class);
                userName = hm.get("userName") + "";
                String pwd = hm.get("pwd") + "";
                Map extendParam = (Map) hm.get("extendParam");
                List<Object> arr = null;
                String ip ="";
                if (extendParam != null) {
                    clientType = extendParam.containsKey("clientType") ? extendParam.get("clientType") + "" : "";
                    arr = new ArrayList<>();
                    arr.add(extendParam);
                    ip = extendParam.containsKey("ip")? extendParam.get("ip") + "" : "";
                }
                result = CedaApi.Login(userName, pwd, StringUtils.isEmpty(ip)?sMsg.getRemoteAddress():ip, arr != null ? arr.toArray() : null);
            }
            String sessionId = "";
            String instId = "";
            boolean isSuccess = false;
            if (!StringUtils.isEmpty(result)) {
                HashMap<String, Object> rhm = JSON.parseObject(result, HashMap.class);
                String code = rhm.get("Code") + "";
                if ("0001".equals(code)) {
                    sessionId = rhm.get("sid") + "";
                    instId = rhm.get("companyId") + "";
                    isSuccess = true;
                    logger.warn("{} login success sessionId:{} instId:{}", userName, sessionId, instId);
                }
                message.setBody(result.getBytes());
            }
            message.setSessionId(sessionId);
            MessageWrapper mr = new MessageWrapper(
                    isSuccess ? MessageWrapper.MessageProtocol.CONNECT : MessageWrapper.MessageProtocol.NO_CONNECT,
                    sessionId, message);
            mr.setUserName(userName);
            mr.setClientType(clientType);
            if (CedaApi.nodeCfgs.rowKeySet().contains(instId)) {
                mr.setNodeId(instId);
            } else {
                mr.setNodeId("*");
            }
            return mr;
        } catch (Exception e) {
            logger.error("content:" + content);
            throw e;
        }
    }

    @Override
    public MessageWrapper processAppRequestReply(SystemMessage sMsg, Proto message, String serverName, String method,
                                                 String content) {
        message.setFormat(Format.REPLY);
        String sid = message.getSessionId();
        String nodeId = tcpConnector.getTcpSessionManager().getSession(sid).getNodeId();
        if ("subscribe".equals(method)) {
            try {
                CedaApi.requestAsync(serverName, content, "", false, sid,nodeId);
            } catch (Exception e) {
                logger.error("processAppRequestReply error", e);
            }
            return null;
        }
        if (serverName.equals("gateway") && "getMcpToolsConfig".equals(method)) {
            return processGetMcpToolsConfig(message);
        }
        if (serverName.equals("gateway") && "getMcpResourcesConfig".equals(method)) {
            return processGetMcpResourcesConfig(message);
        }
        if (serverName.equals("gateway") && "notify".equals(method)) {
            Map<?, ?> map = JsonUtils.Deserialize(content, Map.class);
            String topic = map.get("topic") + "";
            String content2 = map.get("content") + "";
            notifyProxy.notify(topic, content2, nodeId);
            return createReply("0000", "Success", message);
        } else {
            try {
                if (sMsg.isSyncFlag()) {
                    String body = CedaApi.requestSync(serverName, method, content, sid,nodeId);
                    message.setBody(body.getBytes());
                    logger.info("{}:{} result:{}", serverName, method, body);
                    return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
                } else {
                    CedaApi.requestAsync(message, serverName, method, content, false, sid, true,nodeId);
                }
            } catch (CodeException e) {
                String ct = CedaApi.faileMsg(e.code(), e.getMessage());
                message.setBody(ct.getBytes());
                message.setFormat(Format.REPLY);
                return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
            }
            return null;

        }
    }

    private MessageWrapper processGetMcpToolsConfig(Proto message) {
        if (mcpGatewayAdapter == null) {
            return createReply("1003", "mcp not supported", message);
        }
        try {
            com.gateway.mcp.McpToolConfigLoader.Snapshot snapshot = mcpGatewayAdapter.loadConfigSnapshot();
            Map<String, Object> result = new LinkedHashMap<String, Object>();
            result.put("Code", "0000");
            result.put("Msg", "Success");
            result.put("Content", snapshot.getRawContent());
            result.put("Version", snapshot.getVersion());
            message.setBody(JSON.toJSONString(result).getBytes(StandardCharsets.UTF_8));
            message.setFormat(Format.REPLY);
            return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
        } catch (Exception e) {
            logger.error("getMcpToolsConfig error", e);
            return createReply("1003", e.getMessage(), message);
        }
    }

    private MessageWrapper processGetMcpResourcesConfig(Proto message) {
        if (mcpGatewayAdapter == null) {
            return createReply("1003", "mcp not supported", message);
        }
        try {
            com.gateway.mcp.McpToolConfigLoader.Snapshot snapshot = mcpGatewayAdapter.loadResourceConfigSnapshot();
            Map<String, Object> result = new LinkedHashMap<String, Object>();
            result.put("Code", "0000");
            result.put("Msg", "Success");
            result.put("Content", snapshot.getRawContent());
            result.put("Version", snapshot.getVersion());
            message.setBody(JSON.toJSONString(result).getBytes(StandardCharsets.UTF_8));
            message.setFormat(Format.REPLY);
            return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
        } catch (Exception e) {
            logger.error("getMcpResourcesConfig error", e);
            return createReply("1003", e.getMessage(), message);
        }
    }

    private List<String> sessionServiceLt = new ArrayList<String>();

    public List<String> getSessionService() {
        return sessionServiceLt;
    }

    public void setSessionService(String sessionService) {
        String[] strs = sessionService.split(",");
        for (String str : strs) {
            if (!StringUtils.isEmpty(str)) {
                sessionServiceLt.add(str);
            }
        }
    }

    public void sessionCreated(SessionEvent se) {
        publishSession(se.getSession().getUserName(), 1);
    }

    public void sessionDestroyed(SessionEvent se) {
        publishSession(se.getSession().getUserName(), 0);
    }

    private void publishSession(String userName, int status) {
        if (!StringUtils.isEmpty(userName)) {
            List<String> userNames = new ArrayList<String>();
            userNames.add(userName);
            publishSession(userNames, status, false);
        }
    }

    public void publishSession(List<String> userNames, int status, boolean isall) {
        HashMap<String, Object> hm = new HashMap<String, Object>();
        hm.put("userNames", userNames);
        hm.put("status", status);
        hm.put("all", isall);
        String content = JSON.toJSONString(hm);
        for (String serverName : sessionServiceLt) {
            try {
                CedaApi.requestAsync(null, serverName, "SessionStatus", content, false, null, false,"");
            } catch (CodeException e) {
                logger.error("publishSession {} error", serverName, e);
            }
        }
    }

    @Override
    public MessageWrapper invokeAsync(SystemMessage sMsg, Proto message, String serverName, String method,
                                      String content, IMessage<String> callback) {
        try {
            String sid = message.getSessionId();
            String instId = message.getCompanyId();
            CedaApi.requestAsync(instId,serverName, method, content, true, sid, callback);
        } catch (CodeException e) {
            String ct = CedaApi.faileMsg(e.code(), e.getMessage());
            message.setBody(ct.getBytes());
            message.setFormat(Format.REPLY);
            return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
        }
        return null;

    }

    @Override
    protected MessageWrapper processMcpRequest(SystemMessage sMsg, Proto message, HttpRequest request, String content) {
        return mcpGatewayAdapter.handle(this,sMsg, message, request, content);
    }

}
