package com.gateway.connector.tcp.config;

import java.util.Map;
import java.util.Set;

public class NodeServerConfig {

    /**
     * 节点ID（机构ID）
     */
    private String nodeId;

    /**
     * 节点上部署的服务
     */
    private Set<String> servers;

    /**
     * 节点服务请求topic
     */
    private Map<String, Set<String>> requestTopics;

    /**
     * 节点服务订阅topic
     */
    private Map<String, Set<String>> subscribeTopics;

    public String getNodeId() {
        return nodeId;
    }

    public void setNodeId(String nodeId) {
        this.nodeId = nodeId;
    }

    public Set<String> getServers() {
        return servers;
    }

    public void setServers(Set<String> servers) {
        this.servers = servers;
    }

    public Map<String, Set<String>> getRequestTopics() {
        return requestTopics;
    }

    public void setRequestTopics(Map<String, Set<String>> requestTopics) {
        this.requestTopics = requestTopics;
    }

    public Map<String, Set<String>> getSubscribeTopics() {
        return subscribeTopics;
    }

    public void setSubscribeTopics(Map<String, Set<String>> subscribeTopics) {
        this.subscribeTopics = subscribeTopics;
    }
}
