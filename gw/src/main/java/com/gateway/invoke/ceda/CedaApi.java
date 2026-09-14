package com.gateway.invoke.ceda;

import java.io.File;
import java.util.*;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.locks.ReentrantLock;

import ats.app.ym.ex.utils.StringUtils;
import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.io.FileUtil;
import cn.hutool.core.map.multi.RowKeyTable;
import cn.hutool.core.map.multi.Table;
import cn.hutool.core.util.StrUtil;
import cn.hutool.core.util.XmlUtil;
import cn.hutool.json.JSONUtil;

import com.app.common.thread.AsyncCacheThreadGroup;
import com.app.common.thread.AsyncSeqThreadGroup;
import com.app.common.utils.JsonUtils;
import com.app.common.utils.StringUtil;
import org.apache.http.util.TextUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.adaptiveMQ2.event.IMessageListener;

import com.gateway.connector.Session;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.TcpConnector;
import com.gateway.connector.tcp.body.NotifyBody;
import com.gateway.connector.tcp.client.IMessageByte;
import com.gateway.connector.utils.ProtoUtils;
import com.gateway.constant.Constants;
import com.gateway.marketdata.LatestWinsDispatcher;
import com.gateway.marketdata.MarketDataBurstConfig;
import com.gateway.marketdata.MarketDataMessage;
import com.gateway.notify.NotifyProxy;
import com.gw.common.utils.GwClientWrapper;

import ats.app.api.BaseApi.IMessage;
import ats.app.ym.ceda.CedaClientWrapper;
import ats.app.ym.ceda.ICedaConnectionListener;
import ats.foundation.utils.exception.CodeException;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;
import ats.message2.Exception.SerializerException;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import sc.app.stc.net.NetClientWrapper;
import sc.app.stc.utils.Consts;

public class CedaApi {
    //	public static NetClientWrapper cedaClientWrapper = new NetClientWrapper();
    private static ConcurrentHashMap<String, NetClientWrapper> cedaClientWrapperMap = new ConcurrentHashMap<>();
    private static Logger logger = LoggerFactory.getLogger(CedaApi.class);
    public static String sid = "BaseApi";
    public static long timeTime = 10 * 1000;
    public static String NoKnowCode = "1003";
    public static String TimeOut = "1007";
    public static String TimeOutMsg = "网络超时";
    public static String NoOnline = "1015";
    public static String NoOnlineMsg = " is not Online";
    static final String serverKey = "SERVER.";
    private static ConcurrentHashMap<String, ConcurrentHashMap<String, Integer>> nodeServerStatusHm = new ConcurrentHashMap<>();
    public static String requestService = "LoginSvr";
    //    public static String proxyNodes;
    public static boolean usePublicNode = false;
    public static HashMap<String, String> subscribes;
    //	public static IMessage<String> messageListener;
    public static ConcurrentHashMap<String, IMessage<String>> messageListenerMap = new ConcurrentHashMap<>();
    public static ConcurrentHashMap<String, IMessageByte> rawMessageListenerMap = new ConcurrentHashMap<>();
    public static TcpConnector tcpConnector;
    public static NotifyProxy notifyProxy;
    public static CedaProxy cedaProxy;
    public static String nodeIds;
    private static volatile MarketDataBurstConfig marketDataBurstConfig = new MarketDataBurstConfig();
    private static volatile LatestWinsDispatcher<MarketDataMessage> marketDataIngressDispatcher;
    private static final ReentrantLock lock = new ReentrantLock();
    /**
     * R:NodeId
     * C:ServerName
     * V:Map<request/subscribe,Set<Topic>>
     */
    public static Table<String, String, Map<String, Set<String>>> nodeCfgs = new RowKeyTable<>(new ConcurrentHashMap<>(), ConcurrentHashMap::new);

    public static void setMarketDataBurstConfig(MarketDataBurstConfig config) {
        marketDataBurstConfig = config != null ? config : new MarketDataBurstConfig();
        marketDataIngressDispatcher = null;
    }

    private static LatestWinsDispatcher<MarketDataMessage> getMarketDataIngressDispatcher() {
        LatestWinsDispatcher<MarketDataMessage> dispatcher = marketDataIngressDispatcher;
        if (dispatcher != null) {
            return dispatcher;
        }
        synchronized (CedaApi.class) {
            dispatcher = marketDataIngressDispatcher;
            if (dispatcher == null) {
                final MarketDataBurstConfig config = marketDataBurstConfig != null ? marketDataBurstConfig
                        : new MarketDataBurstConfig();
                marketDataBurstConfig = config;
                dispatcher = new LatestWinsDispatcher<MarketDataMessage>("market-data-ingress", "ingress", "ingress",
                        Runtime.getRuntime().availableProcessors() * 2, config.getIngressMaxPendingKeys(), config,
                        new LatestWinsDispatcher.Handler<MarketDataMessage>() {
                            @Override
                            public LatestWinsDispatcher.HandleResult handle(MarketDataMessage value) {
                                if (notifyProxy == null || value == null) {
                                    return LatestWinsDispatcher.HandleResult.DONE;
                                }
                                if (value.rawBody != null) {
                                    notifyProxy.notifyRaw(value.topic, value.rawBody, value.nodeId);
                                } else {
                                    notifyProxy.notify(value.topic, value.content, value.nodeId);
                                }
                                return LatestWinsDispatcher.HandleResult.DONE;
                            }
                        });
                marketDataIngressDispatcher = dispatcher;
            }
        }
        return dispatcher;
    }

    private static boolean handleMarketDataMessage(String topic, String content, String nodeId) {
        return submitMarketDataMessage(topic, content, null, nodeId);
    }

    private static boolean handleRawNotifyMessage(String topic, byte[] rawBody, String nodeId,
            boolean directMarketDispatch) {
        MarketDataBurstConfig config = marketDataBurstConfig;
        if (notifyProxy == null || config == null || rawBody == null || !config.isRawNotifyEnabled()) {
            return false;
        }
        String dispatcherNodeId = StringUtils.isEmpty(nodeId) ? "*" : nodeId;
        if (config.isMarketDataTopic(topic)) {
            if (directMarketDispatch) {
                try {
                    notifyProxy.notifyRaw(topic, rawBody, nodeId);
                    config.recordRawNotifyIntercept(config.resolveTopicPrefix(topic), dispatcherNodeId);
                } catch (Exception e) {
                    config.recordDrop("ingress", config.resolveTopicPrefix(topic), dispatcherNodeId,
                            "direct_dispatch_error");
                    logger.error("raw market data direct dispatch error nodeId:{} topic:{}", dispatcherNodeId,
                            topic, e);
                }
                return true;
            }
            return submitMarketDataMessage(topic, null, rawBody, nodeId);
        }
        String topicPrefix = config.resolveTopicPrefix(topic);
        IMessageByte rawListener = rawMessageListenerMap.get(nodeId);
        if (rawListener == null) {
            logger.warn("drop raw notify without listener topic:{} nodeId:{} reason:listener_missing", topic,
                    dispatcherNodeId);
            return true;
        }
        try {
            boolean handled = rawListener.onMessage(topic, rawBody);
            if (handled) {
                config.recordRawNotifyIntercept(topicPrefix, dispatcherNodeId);
                return true;
            }
            logger.warn("drop raw notify because listener returned false topic:{} nodeId:{} reason:listener_reject",
                    topic, dispatcherNodeId);
            return true;
        } catch (Exception e) {
            logger.error("drop raw notify because listener error nodeId:{} topic:{}", dispatcherNodeId, topic, e);
            return true;
        }
    }

    private static boolean submitMarketDataMessage(String topic, String content, byte[] rawBody, String nodeId) {
        MarketDataBurstConfig config = marketDataBurstConfig;
        if (notifyProxy == null || config == null || !config.isMarketDataTopic(topic)) {
            return false;
        }
        if (rawBody != null && !config.isRawNotifyEnabled()) {
            return false;
        }
        MarketDataMessage message = new MarketDataMessage();
        message.topic = topic;
        message.content = content;
        message.rawBody = rawBody;
        message.nodeId = nodeId;
        String dispatcherNodeId = StringUtils.isEmpty(nodeId) ? "*" : nodeId;
        String key = dispatcherNodeId + "|" + topic;
        String topicPrefix = config.resolveTopicPrefix(topic);
        boolean submitted = getMarketDataIngressDispatcher().submit(key, message, topicPrefix, dispatcherNodeId);
        if (rawBody != null) {
            if (submitted) {
                config.recordRawNotifyIntercept(topicPrefix, dispatcherNodeId);
                return true;
            }
            logger.warn("drop raw market data without decode fallback topic:{} nodeId:{} reason:ingress_overload",
                    topic, dispatcherNodeId);
            return true;
        }
        return true;
    }


    private static void serverStatusChange(String serverName, int status, String nodeId) {
        ConcurrentHashMap<String, Integer> serverStatusHm = nodeServerStatusHm.get(nodeId);
        if (serverStatusHm == null) {
            serverStatusHm = new ConcurrentHashMap<>();
            nodeServerStatusHm.put(nodeId, serverStatusHm);
        }
        serverStatusHm.put(serverName, status);
        IMessage<String> messageListener = messageListenerMap.get(nodeId);
        if (messageListener != null) {
            try {
                String str = JsonUtils.Serializer(serverStatusHm);
                messageListener.onMessage(Constants.ServerStatusTopic, str);
            } catch (Exception e) {
                logger.error("serverStatusChange error.", e);
            }
        }
    }

    private static ConcurrentHashMap<String, Integer> getServerStatusHm(String sid) {
        Session session = tcpConnector.getTcpSessionManager().getSession(sid);
        if (session != null) {
            ConcurrentHashMap<String, Integer> serverStatusHm = nodeServerStatusHm.get(session.getNodeId());
            if (serverStatusHm != null) {
                return serverStatusHm;
            }
        }
        return nodeServerStatusHm.get("*");
    }

    private static NetClientWrapper getNetClientWrapper(String instId, String sid, String serverName, String topic) {
        serverName = StrUtil.removePrefix(serverName, serverKey);

        Session session = tcpConnector.getTcpSessionManager().getSession(sid);
        String nodeId = instId;
        if (session != null) {
            nodeId = session.getNodeId();
        }
        if (!StrUtil.isBlank(nodeId)) {
            NetClientWrapper netClientWrapper = cedaClientWrapperMap.get(nodeId);
            if (netClientWrapper != null && !"*".equals(nodeId)) {
                return getNodeClientWrapper(serverName, topic, nodeId);
            } else if (usePublicNode) {
                //未配置子节点，走公共节点
                return getNodeClientWrapper(serverName, topic, Constants.NODE_PUBLIC);
            } else {
                return cedaClientWrapperMap.get("*");
            }
        }
        return cedaClientWrapperMap.get("*");
    }

    /**
     * 判断该请求是否走子节点
     * 先找私有节点，私有节点没有，走公共节点，公共节点没有，返回主节点
     *
     * @param serverName
     * @param topic
     * @param nodeId
     * @return
     */
    private static NetClientWrapper getNodeClientWrapper(String serverName, String topic, String nodeId) {
        //配置了子节点
        Map<String, Set<String>> topics = nodeCfgs.get(nodeId, serverName);
        if (CollUtil.isEmpty(topics) || !topics.containsKey(Constants.REQUEST_TOPIC)) {
            //子节点没有配置该服务，或者服务没有配置请求topic,走公共服务
            if (Constants.NODE_PUBLIC.equals(nodeId)) {
                //公共服务里也没配置，返回主节点
                return cedaClientWrapperMap.get("*");
            }
            return getNodeClientWrapper(serverName, topic, Constants.NODE_PUBLIC);
        }
        Set<String> reqTopics = topics.get(Constants.REQUEST_TOPIC);
        if (reqTopics.contains("*") || reqTopics.contains(topic) || checkFuzzyMatching(reqTopics, topic)) {
            //服务配置了请求全部topic或者请求的topic
            return cedaClientWrapperMap.get(nodeId);
        }
        if (Constants.NODE_PUBLIC.equals(nodeId)) {
            //公共服务里也没配置，返回主节点
            return cedaClientWrapperMap.get("*");
        }
        //私有节点没有配置，走公共节点
        return getNodeClientWrapper(serverName, topic, Constants.NODE_PUBLIC);
    }

    /**
     * 检查requests中带*的topic是否匹配
     * 1.如果只有一个*，则匹配所有
     * 2.如果最后一个字符是单个*，则必须长度相同，并且开头的字符相同
     * 例如：a.b.* 匹配 a.b.c和a.b.d 不匹配 a.b.c.d
     * 3.如果最后一个字符是多个*，则只需要开头的字符相同
     * 例如：a.b.** 匹配 a.b.c.d和a.b.e.f ，a.b.c 不匹配 a.c.c.d
     *
     * @param reqTopics
     * @param topic
     * @return
     */
    private static boolean checkFuzzyMatching(Set<String> reqTopics, String topic) {
        for (String reqTopic : reqTopics) {
            if (reqTopic.endsWith("*")) {
                String[] t1 = StrUtil.splitToArray(reqTopic, ".");
                if (t1.length == 1) {
                    //如果只有一个*，则匹配所有
                    return true;
                }
                String prefix = StrUtil.sub(reqTopic, 0, reqTopic.length() - t1[t1.length - 1].length());
                if (!StrUtil.startWith(topic, prefix)) {
                    continue;
                }
                String[] t2 = StrUtil.splitToArray(topic, ".");
                if (t1[t1.length - 1].length() == 1) {
                    //如果最后一个字符是单个*，则必须长度相同，并且开头的字符相同
                    if (t1.length == t2.length) {
                        logger.info("fuzzy mached topic:{} reqTopic:{}", topic, reqTopic);
                        return true;
                    }
                    continue;
                } else {
                    logger.info("fuzzy mached topic:{} reqTopic:{}", topic, reqTopic);
                    //如果最后一个字符是多个*，则只需要开头的字符相同
                    return true;
                }
            }
        }
        return false;

    }

    public static void init(ExecutorService executorService, String nodeFilePath) {
        logger.warn("CedaApi init  nodeFilePath:{}", nodeFilePath);
        loadNodeCfgs(nodeFilePath);
        com.gw.common.utils.Consts.ServerName = "gateway";
        for (String nodeId : StrUtil.split(nodeIds, ",")) {
            initNodeClientWrapper(executorService, nodeId, Consts.getNetType());
        }
        logger.warn("cedaClientWrapperMap: {}", JsonUtils.Serializer(cedaClientWrapperMap));
    }

    /**
     * 定时任务重载节点配置
     *
     * @param executorService
     * @param nodeFilePath
     */
    public static void reloadNodes(ExecutorService executorService, String nodeFilePath) {
        lock.lock();
        try {
            logger.warn("CedaApi reloadNodes nodeFilePath:{}", nodeFilePath);
            loadNodeCfgs(nodeFilePath);
            for (String nodeId : StrUtil.split(nodeIds, ",")) {
                initNodeClientWrapper(executorService, nodeId, Consts.getNetType());
            }
            logger.warn("cedaClientWrapperMap: {}", JsonUtils.Serializer(cedaClientWrapperMap));
        } catch (Exception e) {
            logger.error("reloadNodes error.", e);
        } finally {
            lock.unlock();
        }
    }

    private static void initNodeClientWrapper(ExecutorService executorService, String nodeId, String netType) {
        logger.info("initNodeClientWrapper:{} netType:{}", nodeId, netType);
        NetClientWrapper cedaClientWrapper = null;
        if ("ceda".equals(netType)) {
            cedaClientWrapper = new NetClientWrapper();
            cedaClientWrapper.setCedaClientWrapper(new CedaClientWrapper());
        } else {
            if (cedaClientWrapperMap.containsKey(nodeId)) {
                logger.info("nodeId {} has been initialized", nodeId);
                return;
            }
            cedaClientWrapper = new NetClientWrapper();
            GwClientWrapper gwClientWrapper = new GwClientWrapper();
            if (!"*".equals(nodeId)) {
                gwClientWrapper.setNodeId(nodeId);
            }
            if (marketDataBurstConfig != null) {
                trySetNotifyByteFilter(gwClientWrapper, nodeId);
            }
            gwClientWrapper.init();
            cedaClientWrapper.setGwClientWrapper(gwClientWrapper);
        }
        cedaClientWrapperMap.put(nodeId, cedaClientWrapper);
        addMessageListener(nodeId);
        Set<String> servers = new HashSet<>();
        if ("*".equals(nodeId)) {
            //主节点使用requestService配置连接的服务
            servers = CollUtil.newHashSet(StrUtil.splitToArray(requestService, ","));
        } else {
            //子节点使用node_server_***.xml配置的服务
            servers = Optional.of(nodeCfgs.getRow(nodeId).keySet()).orElse(new HashSet<>());
        }
        logger.warn("initNodeClientWrapper nodeId:{} servers:{}", nodeId, JSONUtil.toJsonStr(nodeCfgs.getRow(nodeId)));
        for (String serverName : servers) {
            serverStatusChange(serverName, 0, nodeId);
            ICedaConnectionListener cedaClientConnectListener = createCedaConnectionListener(executorService, serverName, nodeId);
            addCedaClientConnectListener(serverKey + serverName, cedaClientWrapper, cedaClientConnectListener);
        }
    }

    private static void trySetNotifyByteFilter(GwClientWrapper gwClientWrapper, String nodeId) {
        try {
            final Class<?> filterInterface = Class.forName("com.gateway.connector.tcp.client.IMessageByte");
            final boolean notifyCacheSupported = hasMethod(filterInterface, "isNotifyCacheTopic")
                    && marketDataBurstConfig != null && marketDataBurstConfig.getNotifyCacheMaxSize() > 0;
            Object filter = java.lang.reflect.Proxy.newProxyInstance(
                    filterInterface.getClassLoader(),
                    new Class<?>[]{filterInterface},
                    (proxy, method, args) -> {
                        if ("onMessage".equals(method.getName()) && args != null && args.length == 2) {
                            String topic = args[0] == null ? null : args[0].toString();
                            byte[] body = args[1] instanceof byte[] ? (byte[]) args[1] : null;
                            return !handleRawNotifyMessage(topic, body, nodeId, notifyCacheSupported);
                        }
                        if ("isNotifyCacheTopic".equals(method.getName()) && args != null && args.length == 1) {
                            String topic = args[0] == null ? null : args[0].toString();
                            return marketDataBurstConfig != null && marketDataBurstConfig.isMarketDataTopic(topic);
                        }
                        if ("getNotifyCacheWorkerCount".equals(method.getName())) {
                            return marketDataBurstConfig != null ? marketDataBurstConfig.getNotifyCacheWorkerCount() : 1;
                        }
                        if ("getNotifyCacheMaxSize".equals(method.getName())) {
                            return marketDataBurstConfig != null ? marketDataBurstConfig.getNotifyCacheMaxSize() : 0;
                        }
                        if ("toString".equals(method.getName())) {
                            return "GwNotifyByteFilter[" + nodeId + "]";
                        }
                        if ("hashCode".equals(method.getName())) {
                            return System.identityHashCode(proxy);
                        }
                        if ("equals".equals(method.getName()) && args != null && args.length == 1) {
                            return proxy == args[0];
                        }
                        return null;
                    });
            gwClientWrapper.getClass().getMethod("setNotifyByteFilter", filterInterface).invoke(gwClientWrapper, filter);
        } catch (ClassNotFoundException e) {
            logger.info("IMessageByte not found, raw notify filter disabled");
        } catch (NoSuchMethodException e) {
            logger.info("GwClientWrapper#setNotifyByteFilter not found, raw notify filter disabled");
        } catch (Exception e) {
            logger.warn("setNotifyByteFilter error", e);
        }
    }

    private static boolean hasMethod(Class<?> type, String methodName) {
        for (java.lang.reflect.Method method : type.getMethods()) {
            if (methodName.equals(method.getName())) {
                return true;
            }
        }
        return false;
    }

    private static ICedaConnectionListener createCedaConnectionListener(ExecutorService executorService, String
            serverName, String nodeId) {
        ICedaConnectionListener cedaClientConnectListener = new ICedaConnectionListener() {
            boolean isFirstConnected = false;

            @Override
            public void onDisconnected(String arg0) {
                serverStatusChange(serverName, 0, nodeId);
            }

            @Override
            public synchronized void onConnected(String arg0) {
                serverStatusChange(serverName, 1, nodeId);
                if (!isFirstConnected) {
                    Set<String> strs = new HashSet<>();
                    if ("*".equals(nodeId)) {
                        String topics = subscribes.get(serverName);
                        if (TextUtils.isBlank(topics)) {
                            topics = "**";
                        }
                        //主节点使用spring-ceda-client.xml中的订阅配置
                        strs = CollUtil.newHashSet(StrUtil.split(topics, "|"));
                    } else {
                        //子节点使用node_server_***.xml中的订阅配置
                        Map<String, Set<String>> topicMap = nodeCfgs.get(nodeId, serverName);
                        if (topicMap != null && topicMap.containsKey(Constants.SUBSCRIBE_TOPIC)) {
                            Set<String> subTopics = topicMap.get(Constants.SUBSCRIBE_TOPIC);
                            if (subTopics.contains("*")) {
                                //订阅全部
                                strs.add("**");
                            } else {
                                strs.addAll(subTopics);
                            }
                        }
                    }
                    Set<String> finalStrs = strs;
                    Runnable command = new Runnable() {

                        @Override
                        public void run() {
                            for (String topic : finalStrs) {
                                subscribe(arg0, topic, nodeId, messageListenerMap.get(nodeId));
                            }
                            logger.warn("{} {} onConnected,subscribe:{}", nodeId, serverName, finalStrs);
                            if (cedaProxy.getSessionService().contains(serverName)) {
                                if (tcpConnector == null || tcpConnector.getTcpSessionManager() == null) {
                                    logger.warn("{} {} onConnected skip publishSession, tcpSessionManager not ready", nodeId, serverName);
                                    return;
                                }
                                Session[] sessions = tcpConnector.getTcpSessionManager().getSessions();
                                List<String> userNames = new ArrayList<String>();
                                for (Session session : sessions) {
                                    if (nodeId.equals(session.getNodeId())) {
                                        userNames.add(session.getUserName());
                                    }
                                }
                                cedaProxy.publishSession(userNames, 1, true);
                            }
                        }
                    };
                    executorService.execute(command);

                    isFirstConnected = true;
                }
            }
        };
        return cedaClientConnectListener;
    }

    private static void addMessageListener(String nodeId) {
        rawMessageListenerMap.put(nodeId, new IMessageByte() {
            private final String nodeId1 = nodeId;
            private AsynOnRawMessage asynOnRawMessage = new AsynOnRawMessage(Runtime.getRuntime().availableProcessors() * 2);
            private AsynCacheOnRawMessage asynCacheOnRawMessage = new AsynCacheOnRawMessage(Runtime.getRuntime().availableProcessors() * 2);

            @Override
            public boolean onMessage(String topic, byte[] body) {
                if (body == null) {
                    return false;
                }
                try {
        			if(notifyProxy.isMarketDataTopic(topic)) {
        				asynCacheOnRawMessage.add(topic, new Object[]{topic, body});
        			}
        			else {
        				asynOnRawMessage.add(topic, new Object[]{topic, body});
        			}
                    return true;
                } catch (Exception e) {
                    logger.error("raw onMessage enqueue error nodeId:{} topic:{}", nodeId1, topic, e);
                    return false;
                }
            }

            class AsynOnRawMessage extends AsyncSeqThreadGroup<Object[]> {

                public AsynOnRawMessage(int asyncBaseNum) {
                    super(asyncBaseNum);
                }

                @Override
                protected void process(Object[] t) {
                    processNotify(t);
                }

			
            }
            class AsynCacheOnRawMessage extends AsyncCacheThreadGroup<Object[]> {

                public AsynCacheOnRawMessage(int asyncBaseNum) {
                    super(asyncBaseNum);
                }

                @Override
                protected void process(Object[] t) {
                    processNotify(t);
                }

			
            }
        	private void processNotify(Object[] t) {
				try {
                    String topic = (String) t[0];
                    byte[] rawBody = (byte[]) t[1];
                    if (logger.isDebugEnabled()) {
                        logger.debug("on raw message nodeId:{} topic:{}", nodeId1, topic);
                    }
                    String sid = null;
                    if ("SYS.ATS.LOGIN".equals(topic)) {
                        NotifyBody notifyBody = ProtoUtils.getNotify(rawBody);
                        String content = notifyBody != null ? notifyBody.content : null;
                        if (StringUtil.isBlank(content)) {
                            return;
                        }
                        HashMap<?, ?> hm = JsonUtils.Deserialize(content, HashMap.class);
                        Object tsid = hm.get("sid");
                        String type = hm.get("type") + "";
                        if ("logout".equals(type)) {
                            if (tsid != null) {
                                sid = tsid + "";
                            }
                        } else {
                            return;
                        }
                    }
                    if (notifyProxy != null) {
                        if (StringUtils.isEmpty(sid)) {
                            notifyProxy.notifyRaw(topic, rawBody, nodeId1);
                        } else {
                            notifyProxy.notifyRaw(sid, topic, rawBody, nodeId1);
                        }
                    }
                } catch (Exception e) {
                    logger.error("", e);
                }
			}
        });
        messageListenerMap.put(nodeId, new IMessage<String>() {
            private final String nodeId1 = nodeId;
            private AsynOnMessage asynOnMessage = new AsynOnMessage(Runtime.getRuntime().availableProcessors() * 2);

            @Override
            public void onMessage(String topic, String content) {
                if (handleMarketDataMessage(topic, content, nodeId1)) {
                    return;
                }
                asynOnMessage.add(topic, new String[]{topic, content});

            }

            class AsynOnMessage extends AsyncSeqThreadGroup<String[]> {

                public AsynOnMessage(int asyncBaseNum) {
                    super(asyncBaseNum);
                }

                @Override
                protected void process(String[] t) {
                    try {
                        String topic = t[0];
                        String content = t[1];
                        if (logger.isDebugEnabled()) {
                            logger.debug("on message nodeId:{} topic:{},content: {}", nodeId1, topic, content);
                        }
                        String sid = null;
                        if ("SYS.ATS.LOGIN".equals(topic)) {
                            HashMap<?, ?> hm = JsonUtils.Deserialize(content, HashMap.class);
                            Object tsid = hm.get("sid");
                            String type = hm.get("type") + "";
                            if ("logout".equals(type)) {
                                if (tsid != null) {
                                    sid = tsid + "";
                                }
                            } else {
                                return;
                            }
                        }
                        if (notifyProxy != null) {
                            if (StringUtils.isEmpty(sid)) {
                                notifyProxy.notify(topic, content, nodeId1);
                            } else {
                                notifyProxy.notify(sid, topic, content, nodeId1);

                            }
                        }
                    } catch (Exception e) {
                        logger.error("", e);
                    }

                }
            }
        });
    }

    public static boolean isOnline(NetClientWrapper cedaClientWrapper, String serverName) {

        cedaClientWrapper.init(serverName, "gateway");

        boolean flag = cedaClientWrapper.getOnlineStatus().isOnline(serverName);
        if (flag) {
            logger.debug("{} {} is online:{}", Optional.ofNullable(cedaClientWrapper.getNodeId()).orElse("*"), serverName, flag);
        } else {
            logger.warn("{} {} is online:{}", Optional.ofNullable(cedaClientWrapper.getNodeId()).orElse("*"), serverName, flag);
        }

        return flag;
    }

    public static void addCedaClientConnectListener(String serviceKey, NetClientWrapper cedaClientWrapper,
                                                    ICedaConnectionListener cedaClientConnectListener) {
        cedaClientWrapper.addCedaClientConnectListener(serviceKey, cedaClientConnectListener);
    }

    public static String faileMsg(String code, String msg) {
        HashMap<String, String> hm = new HashMap<String, String>();
        hm.put("Code", NoKnowCode);
        hm.put("Msg", msg);

        String content = null;
        try {
            content = MsgHelper.Serializer(hm, CharsetConvertType.None);
        } catch (SerializerException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        }
        return content;
    }

    @SuppressWarnings("unchecked")
    public static String Login(String userName, String pwd, String ip, Object... extendsParam) {
        String serverName = "LoginSvr";
        String topic = "SYS.ATS.LOGIN";
        HashMap<String, Object> hm = new HashMap<>();
        hm.put("method", "login");
        hm.put("isEncrypt", "N");
        hm.put("isManage", "N");
        HashMap<String, Object> phm = new HashMap<>();
        phm.put("userName", userName);
        phm.put("password", pwd);

        if (extendsParam != null && extendsParam.length > 0 && extendsParam[0] instanceof Map) {
            Map<String, Object> map = (Map) extendsParam[0];
            for (Map.Entry<String, Object> kv : map.entrySet()) {

                if (kv.getKey().equals("isEncrypt")) {
                    hm.put("isEncrypt", kv.getValue());

                } else if (kv.getKey().equals("isManage"))
                    hm.put("isManage", kv.getValue());

                phm.put(kv.getKey(), kv.getValue());
            }
        }
        phm.put("ip", ip);
        hm.put("parameter", phm);
        String content = JsonUtils.Serializer(hm);
        return requestSync(serverName, topic, content, "", "");
    }

    public static String getUserInfo(String sid) {
        String serverName = "LoginSvr";
        String topic = "SYS.ATS.LOGIN";
        HashMap<String, Object> hm = new HashMap<String, Object>();
        hm.put("method", "userInfo");
        HashMap<String, String> args = new HashMap<String, String>();
        args.put("sid", sid);
        hm.put("parameter", args);

        String content = JsonUtils.Serializer(hm);
        return requestSync(serverName, topic, content, "", "");
    }

    public static IMessageListener rpmessageListener;

    public static String requestSync(String serverName, String topic, String content, String sid, String instId) {
        return requestSync(serverName, topic, content, true, sid, instId);
    }

    public static String requestSync(String serverName, String topic, String content, boolean isZip, String sid, String instId) {
        try {
            NetClientWrapper cedaClientWrapper = getNetClientWrapper(instId, sid, serverName, topic);
            serverName = serverKey + serverName;
            logger.info("requestSync sid:{} request server:{} topic:{} wrapper nodeId:{}", sid, serverName, topic, cedaClientWrapper.getNodeId());
            if (isOnline(cedaClientWrapper, serverName)) {

                try {
                    content = cedaClientWrapper.requestSync(serverName, sid, topic, content, timeTime);
                } catch (Exception e) {
                    throw new CodeException(NoKnowCode, e.getMessage());
                }
                if (content == null) {
                    logger.warn(String.format("Timeout, request to svr %s time out", serverName));
                    throw new CodeException(TimeOut, TimeOutMsg);
                }

            } else {
                throw new CodeException(NoOnline, serverName + NoOnlineMsg);
            }
        } catch (CodeException e) {
            content = faileMsg(e.code(), e.getMessage());
        }
        logger.info("{}[{}]->response content:{}", serverName, topic, content);

        return content;

    }

    private static String createServerStatusMessage(String topic, boolean isZip, String sid) throws CodeException {
        HashMap<String, Object> rhm = new HashMap<>();
        rhm.put(topic, getServerStatusHm(sid));
        return JsonUtils.Serializer(rhm);
    }

    public static void requestAsync(String instId, String serverName, String topic, String content, boolean isZip, String sid,
                                    IMessage<String> callback) throws CodeException {
        if (serverName.equals("gateway") && Constants.ServerStatusTopic.equals(topic)) {
            String message = createServerStatusMessage(topic, isZip, sid);
            new SubscribeRspCallBack(notifyProxy).onMessage(sid, message);
            return;
        }

        NetClientWrapper cedaClientWrapper = getNetClientWrapper(instId, sid, serverName, topic);
        serverName = serverKey + serverName;
        if (isOnline(cedaClientWrapper, serverName)) {

            try {
                cedaClientWrapper.requestAsync(serverName, topic, content, sid, callback);
            } catch (Exception e) {

                throw new CodeException(NoKnowCode, e.getMessage());
            }

        }
    }

    public static void requestAsync(String serverName, String topic, String content, boolean isZip, String sid, String instId)
            throws CodeException {
        requestAsync(instId, serverName, topic, content, isZip, sid, new SubscribeRspCallBack(notifyProxy));
    }

    public static void requestAsync(Proto proto, String serverName, String topic, String content, boolean isZip,
                                    String sid, boolean ischeckOnLine, String instId) throws CodeException {

        NetClientWrapper cedaClientWrapper = getNetClientWrapper(instId, sid, serverName, topic);
        serverName = serverKey + serverName;
        logger.info("requestAsync sid:{} request server:{} topic:{} wrapper nodeId:{}[{}]", sid, serverName, topic, cedaClientWrapper.getNodeId(), proto != null ? proto.getSeq() : "0");

        if (isOnline(cedaClientWrapper, serverName)) {

            try {
                cedaClientWrapper.requestAsync(serverName, topic, content, sid,
                        new RspCallBack(proto, serverName, topic, tcpConnector));
            } catch (Exception e) {

                throw new CodeException(NoKnowCode, e.getMessage());
            }
        } else {
            if (ischeckOnLine) {
                throw new CodeException(NoOnline, serverName + NoOnlineMsg);
            }
        }

    }

    public static void subscribeWithImage(String serverName, String SvrId, String topic, String sid,
                                          IMessage<String> messageListener) {
        NetClientWrapper cedaClientWrapper = getNetClientWrapper("", sid, serverName, topic);
        cedaClientWrapper.subscribeWithImage(serverName, topic, SvrId, sid, messageListener);
    }

    public static void subscribe(String serverName, String topic, String nodeId, IMessage<String> messageListener) {
        cedaClientWrapperMap.get(nodeId).subscribe(serverName, topic, messageListener);
    }

    /**
     * 通过配置文件加载节点配置
     *
     * @param nodeFilePath
     */
    private static void loadNodeCfgs(String nodeFilePath) {

        Table<String, String, Map<String, Set<String>>> tempNodeCfgs = new RowKeyTable<>(new ConcurrentHashMap<>(), ConcurrentHashMap::new);
        if (nodeFilePath != null) {
            File dir = new File(nodeFilePath);
            File[] files = dir.listFiles((d, name) -> name.startsWith("node_server_") && name.endsWith(".xml"));
            if (files != null) {
                //从配置文件中加载节点配置
                for (File file : files) {
                    Document document = XmlUtil.readXML(file);
                    Element rootElement = XmlUtil.getRootElement(document);
                    Element node = XmlUtil.getElement(rootElement, "Node");
                    if (node != null) {
                        String nodeId = XmlUtil.getElement(node, "NodeId").getTextContent();
                        Element servers = XmlUtil.getElement(rootElement, "Servers");
                        if (servers != null) {
                            List<Element> serverList = XmlUtil.getElements(servers, "Server");
                            for (Element server : serverList) {
                                String serverName = XmlUtil.getElement(server, "ServerName").getTextContent();
                                List<Element> reqElements = XmlUtil.getElements(XmlUtil.getElement(server, "RequestTopics"), "Topic");
                                List<Element> subElements = XmlUtil.getElements(XmlUtil.getElement(server, "SubscribeTopics"), "Topic");
                                Set<String> reqTopics = new HashSet<>();
                                Set<String> subTopics = new HashSet<>();
                                if (CollUtil.isNotEmpty(reqElements)) {
                                    for (Element topic : reqElements) {
                                        reqTopics.add(topic.getTextContent());
                                    }
                                }
                                if (CollUtil.isNotEmpty(subElements)) {
                                    for (Element topic : subElements) {
                                        subTopics.add(topic.getTextContent());
                                    }
                                }
                                Map<String, Set<String>> topicMap = new HashMap<>();
                                topicMap.put(Constants.REQUEST_TOPIC, reqTopics);
                                topicMap.put(Constants.SUBSCRIBE_TOPIC, subTopics);
                                tempNodeCfgs.put(nodeId, serverName, topicMap);
                                logger.info("loadNodeCfgs nodeId:{} serverName:{} reqTopics:{} subTopics:{}",
                                        nodeId, serverName, reqTopics, subTopics);
                            }
                        }
                    }
                }
            }
        }

        //更新节点配置
        for (String nodeId : nodeCfgs.rowKeySet()) {
            if ("*".equals(nodeId)) {
                continue;
            }
            if (!tempNodeCfgs.rowKeySet().contains(nodeId)) {
                //新节点配置中不包含原有节点配置，移除
                if (cedaClientWrapperMap.containsKey(nodeId)) {
                    cedaClientWrapperMap.get(nodeId).dispose();
                    cedaClientWrapperMap.remove(nodeId);
                }
            } else {
                Map<String, Map<String, Set<String>>> newCfg = tempNodeCfgs.getRow(nodeId);
                Map<String, Map<String, Set<String>>> oldCfg = nodeCfgs.getRow(nodeId);
                int flag = 0;
                //是否有新增配置
                flag += compareCfgMap(newCfg, oldCfg);
                //是否有删减配置
                flag += compareCfgMap(oldCfg, newCfg);
                if (flag > 0) {
                    //新配置有更新
                    cedaClientWrapperMap.get(nodeId).dispose();
                    cedaClientWrapperMap.remove(nodeId);
                }
            }
        }
        nodeCfgs = tempNodeCfgs;
        nodeIds = String.join(",", nodeCfgs.rowKeySet());
        if (StringUtil.isBlank(nodeIds)) {
            nodeIds = "*";
        } else {
            if (!StrUtil.contains(nodeIds, "*")) {
                nodeIds = "*," + nodeIds;
            }
        }
        logger.info("CedaApi.nodeIds update {}", nodeIds);
        logger.warn("loadNodeCfgs nodeCfgs:{}", JsonUtils.Serializer(nodeCfgs));
    }

    /**
     * 比较 cfg1 中key-value是否多于 cfg2
     *
     * @param cfg1
     * @param cfg2
     * @return
     */
    private static int compareCfgMap
    (Map<String, Map<String, Set<String>>> cfg1, Map<String, Map<String, Set<String>>> cfg2) {
        int flag = 0;
        for (String serverName : cfg1.keySet()) {
            if (!cfg2.containsKey(serverName)) {
                //node新增server
                flag++;
                continue;
            }
            Map<String, Set<String>> topics1 = cfg1.get(serverName);
            Map<String, Set<String>> topics2 = cfg2.get(serverName);
            for (String topic : topics1.keySet()) {
                if (!topics2.containsKey(topic)) {
                    //server新增topic类型
                    flag++;
                } else {
                    Set<String> newTopicSet = topics1.get(topic);
                    Set<String> oldTopicSet = topics2.get(topic);
                    if (!CollUtil.containsAll(oldTopicSet, newTopicSet)) {
                        //newTopicSet 大于 oldTopicSet 新增topic
                        flag++;
                    }
                }
            }
        }
        return flag;
    }

}
