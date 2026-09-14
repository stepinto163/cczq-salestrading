package com.gateway.mcp;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

public class McpToolRegistry {
    private final Logger logger = LoggerFactory.getLogger(McpToolRegistry.class);
    private final ConcurrentHashMap<String, McpToolSpec> toolSpecs = new ConcurrentHashMap<String, McpToolSpec>();
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

    public List<McpToolSpec> list() {
        return new ArrayList<McpToolSpec>(toolSpecs.values());
    }

    public McpToolSpec get(String toolName) {
        if (toolName == null) {
            return null;
        }
        return toolSpecs.get(toolName.toLowerCase());
    }

    public McpToolConfigLoader.Snapshot loadSnapshot() throws Exception {
        return McpToolConfigLoader.load(configPath);
    }

    private void reload() {
        try {
            McpToolConfigLoader.Snapshot snapshot = loadSnapshot();
            ConcurrentHashMap<String, McpToolSpec> next = new ConcurrentHashMap<String, McpToolSpec>();
            for (String line : snapshot.getLines()) {
                String trimmed = line == null ? "" : line.trim();
                if (!trimmed.isEmpty() && trimmed.charAt(0) == '\uFEFF') {
                    trimmed = trimmed.substring(1).trim();
                }
                if (trimmed.isEmpty() || trimmed.startsWith("#")) {
                    continue;
                }
                String[] parts = trimmed.split("\t", 9);
                if (parts.length < 4) {
                    logger.warn("ignore invalid mcp tool config line: {}", trimmed);
                    continue;
                }
                String toolName = parts[0].trim();
                String serverName = parts[1].trim();
                String method = parts[2].trim();
                String description = parts[3].trim();
                String inputSchemaJson = parts.length >= 5 ? parts[4].trim() : "";
                String outputSchemaJson = parts.length >= 6 ? parts[5].trim() : "";
                String exampleJson = parts.length >= 7 ? parts[6].trim() : "";
                String methodTemplate = parts.length >= 8 ? parts[7].trim() : "";
                String contentTemplate = parts.length >= 9 ? parts[8].trim() : "";
                if (StringUtils.isBlank(toolName) || StringUtils.isBlank(serverName) || StringUtils.isBlank(method)) {
                    logger.warn("ignore invalid mcp tool config line: {}", trimmed);
                    continue;
                }
                McpToolSpec spec = new McpToolSpec(toolName, serverName, method, description, inputSchemaJson,
                        outputSchemaJson, exampleJson, methodTemplate, contentTemplate);
                next.put(toolName.toLowerCase(), spec);
            }
            toolSpecs.clear();
            toolSpecs.putAll(next);
            logger.info("loaded {} mcp tool definitions from {}", toolSpecs.size(), configPath);
        } catch (Exception e) {
            logger.error("reload mcp tools failed", e);
        }
    }
}
