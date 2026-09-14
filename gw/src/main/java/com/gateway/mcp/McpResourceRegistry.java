package com.gateway.mcp;

import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

public class McpResourceRegistry {
    private final Logger logger = LoggerFactory.getLogger(McpResourceRegistry.class);
    private final ConcurrentHashMap<String, McpResourceSpec> resourceSpecs = new ConcurrentHashMap<String, McpResourceSpec>();
    private final ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);

    private String configPath;
    private long period = 1;
    private volatile boolean started;

    public void setConfigPath(String configPath) {
        this.configPath = configPath;
    }

    public void setPeriod(long period) {
        this.period = period;
    }

    public void init() {
        reload();
        if (!started) {
            started = true;
            executor.scheduleAtFixedRate(new Runnable() {
                @Override
                public void run() {
                    reload();
                }
            }, period, period, TimeUnit.MINUTES);
        }
    }

    public List<McpResourceSpec> list() {
        return new ArrayList<McpResourceSpec>(resourceSpecs.values());
    }

    public McpResourceSpec get(String uri) {
        if (uri == null) {
            return null;
        }
        return resourceSpecs.get(uri);
    }

    public McpToolConfigLoader.Snapshot loadSnapshot() throws Exception {
        return McpToolConfigLoader.load(configPath);
    }

    private void reload() {
        try {
            McpToolConfigLoader.Snapshot snapshot = loadSnapshot();
            ConcurrentHashMap<String, McpResourceSpec> next = new ConcurrentHashMap<String, McpResourceSpec>();
            JSONObject root = JSON.parseObject(snapshot.getRawContent());
            if (root == null) {
                logger.warn("ignore empty mcp resource config from {}", configPath);
                return;
            }
            JSONArray resources = root.getJSONArray("resources");
            if (resources != null) {
                for (int i = 0; i < resources.size(); i++) {
                    JSONObject item = resources.getJSONObject(i);
                    if (item == null) {
                        continue;
                    }
                    String uri = item.getString("uri");
                    if (StringUtils.isBlank(uri)) {
                        continue;
                    }
                    String name = item.getString("name");
                    String title = item.getString("title");
                    String description = item.getString("description");
                    String mimeType = item.getString("mimeType");
                    String kind = item.getString("kind");
                    Object contents = item.get("contents");
                    String contentsJson = contents == null ? "" : JSON.toJSONString(contents);
                    McpResourceSpec spec = new McpResourceSpec(uri, name, title, description, mimeType, kind, contentsJson);
                    next.put(uri, spec);
                }
            }
            resourceSpecs.clear();
            resourceSpecs.putAll(next);
            logger.info("loaded {} mcp resources from {}", resourceSpecs.size(), configPath);
        } catch (Exception e) {
            logger.error("reload mcp resources failed", e);
        }
    }
}
