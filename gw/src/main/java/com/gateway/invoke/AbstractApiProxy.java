package com.gateway.invoke;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.alibaba.fastjson.JSON;
import com.gateway.connector.proto.Cmd;
import com.gateway.connector.proto.Format;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.TcpConnector;
import com.gateway.connector.tcp.server.HttpServerHandler.HttpCallBack;
import com.gateway.connector.tcp.server.TopicManager;
import com.gateway.connector.utils.JsonUtils;
import com.gateway.connector.utils.LicenseUtil;
import com.gateway.connector.utils.ProtoUtils;
import com.gateway.connector.utils.ThreadNameFactory;
import com.gateway.constant.Constants;
import com.gateway.invoke.ceda.CedaApi;
import com.gateway.invoke.security.ISecurityCheck;
import com.gateway.invoke.security.SecurityResult;
import com.gateway.message.MessageWrapper;
import com.gateway.message.SystemMessage;

import ats.app.api.BaseApi.IMessage;
import io.netty.handler.codec.http.HttpRequest;
import sc.app.stc.utils.StringUtil;

public abstract class AbstractApiProxy implements ApiProxy {
    protected final static Logger logger = LoggerFactory.getLogger(AbstractApiProxy.class);
    protected ExecutorService executorService = Executors.newCachedThreadPool(new ThreadNameFactory("invoke"));

    private List<ISecurityCheck> securityChecks = new ArrayList<ISecurityCheck>();

    public List<ISecurityCheck> getSecurityChecks() {
        return securityChecks;
    }

    public void setSecurityChecks(List<ISecurityCheck> securityChecks) {
        this.securityChecks = securityChecks;
    }

    public SecurityResult securityCheck(SystemMessage sMsg, Proto message, String serverName, String method,
                                         String content) {
        SecurityResult pr = new SecurityResult();
        for (ISecurityCheck iRule : securityChecks) {
            pr = iRule.check(sMsg, message, serverName, method, content);
            if (!pr.isSuccess()) {
                logger.warn("{}:{} {} {} {} {}", message.getSessionId(), serverName, method, content, pr.Code, pr.Msg);
                break;
            }
        }
        return pr;
    }

    public TcpConnector tcpConnector;

    public void setTcpConnector(TcpConnector tcpConnector) {
        this.tcpConnector = tcpConnector;
    }

    public class ExecutorImpl implements Runnable {
        private SystemMessage sMsg;
        private Proto message;
        private String serverName;
        private String method;
        private String content;

        public ExecutorImpl(SystemMessage sMsg, Proto message, String serverName, String method, String content) {
            this.sMsg = sMsg;
            this.message = message;
            this.serverName = serverName;
            this.method = method;
            this.content = content;
        }

        @Override
        public void run() {

        	try {
            processAppRequestReply(sMsg, message, serverName, method, content);// 处理镜像
        	}
        	catch(Exception e) {
        		logger.warn("{},{},{},{},{}",serverName,method,content,message.getSessionId(),e);
        	}
        }
    }

    public void processSubscribeWithImage(SystemMessage sMsg, Proto message, String serverName, String method,
                                          String content) {
        executorService.execute(new ExecutorImpl(sMsg, message, serverName, method, content));// 处理镜像
    }

    private TopicManager topicManager;

    public TopicManager getTopicManager() {
        return topicManager;
    }

    public void setTopicManager(TopicManager topicManager) {
        this.topicManager = topicManager;
    }

    public MessageWrapper invoke(SystemMessage sMsg, Proto message) {
        byte[] body = message.getBody();

        String content = body != null ? new String(body) : "";
        try {
        logger.debug("cmd:{},format:{},seq:{},sessionid:{},body:{}", message.getCmd(),
                message.getFormat(), message.getSeq(), message.getSessionId(), content);

        if (message.getCmd() == Cmd.CONNECT) {

            return login(sMsg, message, content);

        } else if (message.getCmd() == Cmd.HEARTBEAT) {
            return new MessageWrapper(MessageWrapper.MessageProtocol.HEART_BEAT, message.getSessionId(), message);

        } else if (message.getCmd() == Cmd.SEND && message.getFormat() == Format.REQUEST) {

            Map<?, ?> hm = JsonUtils.Deserialize(content, Map.class);
            String serverName = hm.get("serverName") + "";
            String method = hm.get("method") + "";
            content = hm.get("content") + "";
            logger.debug("request->severName:{},method:{},content:{}", serverName, method, content);
            SecurityResult pr = securityCheck(sMsg, message, serverName, method, content);
            if (!pr.isSuccess()) {
                return createReply(pr.Code, pr.Msg, message);
            }
            if ("subscribe".equals(method) || "unsubscribe".equals(method)) {// 订阅
                if ((!"gateway".equals(serverName) || Constants.ServerStatusTopic.equals(content))
                        && "subscribe".equals(method)) {
                    processSubscribeWithImage(sMsg, message, serverName, method, content);// 处理镜像
                }
                return processGateWayRequestReply(sMsg, message, serverName, method, content);

            } else {
                try {
                    return processAppRequestReply(sMsg, message, serverName, method, content);
                } catch (Exception e) {
                    logger.error("{},{},{},{}",e.getMessage(),serverName, method, content);
                    return createReply("1003", e.getMessage() + "", message);
                }

            }
        }
        }
        catch(Exception e) {
        	  logger.warn("cmd:{},format:{},seq:{},sessionid:{},body:{}", message.getCmd(),
                      message.getFormat(), message.getSeq(), message.getSessionId(), content);
        }

        return null;
    }

    public MessageWrapper createReply(String code, String msg, Proto message) {
        HashMap<String, String> ehm = new HashMap<>();
        ehm.put("Code", code);
        ehm.put("Msg", msg + "");
        byte[] resultValue = JsonUtils.SerializerBytes(ehm);
        message.setBody(resultValue);
        message.setFormat(Format.REPLY);
        return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
    }

    public MessageWrapper processGateWayRequestReply(SystemMessage sMsg, Proto message, String serverName,
                                                     String method, String topic) {
        String sessionid = message.getSessionId();
        message = ProtoUtils.generateReplySuccess(sessionid, message.getCmd(), message.getSeq());
        logger.debug("processGateWayRequestReply sessionid:{} serverName:{} method:{} topic:{}", sessionid, serverName, method, topic);
        if ("subscribe".equals(method)) {
            topicManager.add(sessionid, topic);
        } else if ("unsubscribe".equals(method)) {
            topicManager.remove(sessionid, topic);
        }
        return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
    }

    public MessageWrapper login(SystemMessage sMsg, Proto message, String content) {
        MessageWrapper mw = null;
        logger.info("login:{}", content);
        SecurityResult pr = securityCheck(sMsg, message, "LoginSvr", "login", content);
        if (!pr.isSuccess()) {
            HashMap<String, String> ehm = new HashMap<>();
            ehm.put("Code", pr.Code);
            ehm.put("Msg", pr.Msg);
            byte[] resultValue = JsonUtils.SerializerBytes(ehm);
            message.setBody(resultValue);
            message.setSessionId("");
            mw = new MessageWrapper(MessageWrapper.MessageProtocol.NO_CONNECT, message.getSessionId(), message);
            return mw;

        }
        HashMap<String, Object> hm = JSON.parseObject(content, HashMap.class);
        String userName = hm.get("userName") + "";
        String clientType = "api";

        Map extendParam = (Map) hm.get("extendParam");
        if (extendParam != null) {
            String tempClientType = (extendParam.containsKey("clientType") ? extendParam.get("clientType") + "" : "").toLowerCase();
            if (StringUtil.isBlank(tempClientType)) {
                if (extendParam.containsKey("param1")) {
                    clientType = "client";
                }
            }
        }
        int count = 1;
        int maxCount = 1;
        if (clientType.contains("api") && sMsg.isLogin()) {
            count = LicenseUtil.getApiMaxSessionCount();
            maxCount = tcpConnector.getTcpSessionManager().getApiSessionCount();

        } else {
            count = LicenseUtil.getClientMaxSessionCount();
            int apiCount = tcpConnector.getTcpSessionManager().getApiSessionCount();
            maxCount = tcpConnector.getTcpSessionManager().getSessionCount() - apiCount;

        }
        if (maxCount >= count) {
            HashMap<String, String> ehm = new HashMap<>();
            ehm.put("Code", "1050");
            ehm.put("Msg", "exceeding the maximum connection limit");
            byte[] resultValue = JsonUtils.SerializerBytes(ehm);
            message.setBody(resultValue);
            message.setSessionId("");
            mw = new MessageWrapper(MessageWrapper.MessageProtocol.NO_CONNECT, message.getSessionId(), message);
            return mw;
        }

        if (!sMsg.isLogin()) {
            mw = defaultLogin(sMsg, message, content);
        } else {
            mw = processLogin(sMsg, message, content);
        }
        mw.setUserName(userName);
        mw.setClientType(clientType);
        return mw;
    }

    public MessageWrapper defaultLogin(SystemMessage sMsg, Proto message, String content) {
        String sessionId = message.getSessionId();
        String userName = "";
        if (StringUtils.isEmpty(sessionId)) {
            sessionId = UUID.randomUUID().toString();
            message.setSessionId(sessionId);
            message.setBody(content.getBytes());
            try {
                HashMap<String, String> hm = JSON.parseObject(content, HashMap.class);
                userName = hm.get("userName") + "";
            } catch (Exception e) {
            }
        }

        MessageWrapper mr = new MessageWrapper(MessageWrapper.MessageProtocol.CONNECT, sessionId, message);
        mr.setUserName(userName);
        return mr;

    }

    public MessageWrapper processLogin(SystemMessage sMsg, Proto message, String content) {
        return defaultLogin(sMsg, message, content);
    }

    public abstract MessageWrapper processAppRequestReply(SystemMessage sMsg, Proto message, String serverName,
                                                          String method, String content);

    protected MessageWrapper processMcpRequest(SystemMessage sMsg, Proto message, HttpRequest request, String content) {
        return createReply("1003", "mcp not supported", message);
    }

    public abstract MessageWrapper invokeAsync(SystemMessage sMsg, Proto message, String serverName, String method,
                                               String content, IMessage<String> callback);

    protected boolean isMcpRequest(HttpRequest request) {
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

    public MessageWrapper invokeAsync(SystemMessage sMsg, Proto message, IMessage<String> callback) {
        try {
            byte[] body = message.getBody();

            String content = body != null ? new String(body) : "";
            if (message.getCmd() == Cmd.SEND && message.getFormat() == Format.REQUEST) {

            	HttpCallBack httpCallBack = (HttpCallBack) callback;
				HttpRequest request = httpCallBack.getRequest();
            
 				String apiKey = request.headers().get("apikey");
				sMsg.setApiKey(apiKey);
				sMsg.setApiKeyCheck(true);
			    if (isMcpRequest(request)) {
                    return processMcpRequest(sMsg, message, request, content);
                }
                Map<?, ?> hm = JsonUtils.Deserialize(content, Map.class);
                String serverName = hm.get("serverName") + "";
                String method = hm.get("method") + "";
                content = hm.get("content") + "";
                logger.debug("request->severName:{},method:{},content:{}", serverName, method, content);
                SecurityResult pr = securityCheck(sMsg, message, serverName, method, content);
                if (!pr.isSuccess()) {
                    return createReply(pr.Code, pr.Msg, message);
                }
                return invokeAsync(sMsg, message, serverName, method, content, callback);

            }
            return null;
        } catch (Exception e) {
            String ct = CedaApi.faileMsg("1003", e.getMessage());
            message.setBody(ct.getBytes());
            message.setFormat(Format.REPLY);
            return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
        }
    }
}
