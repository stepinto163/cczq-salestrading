package com.gateway.mcp;

import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import com.gateway.connector.proto.Format;
import com.gateway.connector.proto.Proto;
import com.gateway.invoke.AbstractApiProxy;
import com.gateway.invoke.ceda.CedaApi;
import com.gateway.invoke.security.SecurityResult;
import com.gateway.message.MessageWrapper;
import com.gateway.message.SystemMessage;
import io.netty.handler.codec.http.HttpRequest;
import org.apache.commons.lang3.StringUtils;

import java.io.File;
import java.math.BigDecimal;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class McpGatewayAdapter {
    private static final Pattern TEMPLATE_PATTERN = Pattern.compile("\\$\\{([^{}]+)\\}");
    private static final String DEFAULT_PROTOCOL_VERSION = "2024-11-05";
    private static final Set<String> SUPPORTED_PROTOCOL_VERSIONS = new HashSet<String>(
            Arrays.asList("2024-11-05", "2025-03-26", "2025-06-18"));
    private final McpToolRegistry registry = new McpToolRegistry();
    private final McpResourceRegistry resourceRegistry = new McpResourceRegistry();

    public void setConfigPath(String configPath) {
        registry.setConfigPath(configPath);
        resourceRegistry.setConfigPath(deriveResourceConfigPath(configPath));
    }

    public void setPeriod(long period) {
        registry.setPeriod(period);
        resourceRegistry.setPeriod(period);
    }

    public void init() {
        registry.init();
        resourceRegistry.init();
    }

    public McpToolConfigLoader.Snapshot loadConfigSnapshot() throws Exception {
        return registry.loadSnapshot();
    }

    public McpToolConfigLoader.Snapshot loadResourceConfigSnapshot() throws Exception {
        return resourceRegistry.loadSnapshot();
    }

    public MessageWrapper handle(AbstractApiProxy proxy,SystemMessage sMsg, Proto message, HttpRequest request, String content) {
        Object id = null;
        try {
            JSONObject rpc = JSON.parseObject(content);
            if (rpc == null) {
                return error(message, null, -32700, "invalid json");
            }
            id = rpc.get("id");
            String method = rpc.getString("method");
            JSONObject params = rpc.getJSONObject("params");
            if (StringUtils.isBlank(method)) {
                return error(message, id, -32600, "method is required");
            }
            if ("initialize".equals(method)) {
                Map<String, Object> result = new LinkedHashMap<String, Object>();
                result.put("protocolVersion", negotiateProtocolVersion(params));
                result.put("capabilities",
                        mapOf("tools", new LinkedHashMap<String, Object>(), "resources", new LinkedHashMap<String, Object>()));
                result.put("serverInfo", mapOf("name", "stc-mcp", "version", "1.0.0"));
                return success(message, id, result);
            }
            if ("notifications/initialized".equals(method)) {
                return empty(message);
            }
            if ("ping".equals(method)) {
                return success(message, id, new LinkedHashMap<String, Object>());
            }
            if ("tools/list".equals(method)) {
                Map<String, Object> result = new LinkedHashMap<String, Object>();
                List<Map<String, Object>> tools = buildAuthorizedTools(proxy, sMsg, message);
                if (tools.isEmpty()) {
                    return error(message, id, -32600, "没有权限访问,请检查apikey是否正确");
                }
                result.put("tools", tools);
                return success(message, id, result);
            }
            if ("resources/list".equals(method)) {
                MessageWrapper authFailure = authorizeMcpMetadata(proxy, sMsg, message, id);
                if (authFailure != null) {
                    return authFailure;
                }
                return success(message, id, mapOf("resources", buildResources()));
            }
            if ("resources/templates/list".equals(method)) {
                MessageWrapper authFailure = authorizeMcpMetadata(proxy, sMsg, message, id);
                if (authFailure != null) {
                    return authFailure;
                }
                return success(message, id, mapOf("resourceTemplates", new ArrayList<Object>()));
            }
            if ("resources/read".equals(method)) {
                MessageWrapper authFailure = authorizeMcpMetadata(proxy, sMsg, message, id);
                if (authFailure != null) {
                    return authFailure;
                }
                return readResource(message, id, params);
            }
            if ("tools/call".equals(method)) {
                return callTool(proxy,sMsg,message, request, id, params);
            }
            return error(message, id, -32601, "method not found");
        } catch (Exception e) {
            return error(message, id, -32603, e.getMessage());
        }
    }

    private MessageWrapper authorizeMcpMetadata(AbstractApiProxy proxy, SystemMessage sMsg, Proto message, Object id) {
        if (!hasAnyAuthorizedTool(proxy, sMsg, message)) {
            return error(message, id, -32600, "没有权限访问,请检查apikey是否正确");
        }
        return null;
    }

    private boolean hasAnyAuthorizedTool(AbstractApiProxy proxy, SystemMessage sMsg, Proto message) {
        for (McpToolSpec toolSpec : registry.list()) {
            SecurityResult pr = proxy.securityCheck(sMsg, message, toolSpec.getServerName(), toolSpec.getMethod(), "{}");
            if (pr.isSuccess()) {
                return true;
            }
        }
        return false;
    }

    private List<Map<String, Object>> buildAuthorizedTools(AbstractApiProxy proxy, SystemMessage sMsg, Proto message) {
        List<Map<String, Object>> tools = new ArrayList<Map<String, Object>>();
        for (McpToolSpec toolSpec : registry.list()) {
            SecurityResult pr = proxy.securityCheck(sMsg, message, toolSpec.getServerName(), toolSpec.getMethod(), "{}");
            if (!pr.isSuccess()) {
                continue;
            }
            Map<String, Object> tool = new LinkedHashMap<String, Object>();
            tool.put("name", toolSpec.getName());
            tool.put("description", toolSpec.getDescription());
            tool.put("inputSchema", buildInputSchema(toolSpec));
            Map<String, Object> outputSchema = buildOutputSchema(toolSpec);
            if (!outputSchema.isEmpty()) {
                tool.put("outputSchema", outputSchema);
            }
            Map<String, Object> annotations = buildAnnotations(toolSpec);
            if (!annotations.isEmpty()) {
                tool.put("annotations", annotations);
            }
            tools.add(tool);
        }
        return tools;
    }

    private MessageWrapper callTool(AbstractApiProxy proxy,SystemMessage sMsg, Proto message, HttpRequest request, Object id, JSONObject params) {
        if (params == null) {
            return error(message, id, -32602, "params is required");
        }
        String toolName = params.getString("name");
        JSONObject arguments = params.getJSONObject("arguments");
        if (StringUtils.isBlank(toolName)) {
            return error(message, id, -32602, "tool name is required");
        }
        McpToolSpec toolSpec = registry.get(toolName);
        if (toolSpec == null) {
            return error(message, id, -32602, "unknown tool: " + toolName);
        }

        JSONObject nestedPayload = arguments == null ? null : arguments.getJSONObject("payload");
        JSONObject payload = nestedPayload != null ? nestedPayload : arguments;
        if (payload == null && arguments != null) {
            Object payloadObj = arguments.get("payload");
            if (payloadObj instanceof JSONArray) {
                payload = new JSONObject();
                payload.put("items", payloadObj);
            }
        }

        Map<String, Object> templateValues = buildTemplateValues(arguments, payload);
        String method = resolveTemplate(
                StringUtils.isBlank(toolSpec.getMethodTemplate()) ? toolSpec.getMethod() : toolSpec.getMethodTemplate(),
                templateValues);
        String payloadJson = buildPayloadJson(toolSpec, payload, templateValues);
        String sessionId = request.headers().get("sessionId");
        String companyId = request.headers().get("companyId");
        
        SecurityResult pr = proxy.securityCheck(sMsg, message, toolSpec.getServerName(), method, payloadJson);
        if (!pr.isSuccess()) {
        	 return error(message, id, -32600, pr.Msg);           
        }

        String result = CedaApi.requestSync(toolSpec.getServerName(), method, payloadJson,
                StringUtils.isBlank(sessionId) ? "" : sessionId,
                StringUtils.isBlank(companyId) ? "" : companyId);
        String normalizedResult = normalizeToolResult(toolSpec, method, result);
        ProjectedToolResult projectedResult = projectToolResult(toolSpec, normalizedResult);

        Map<String, Object> toolResult = new LinkedHashMap<String, Object>();
        toolResult.put("content", buildToolContent(projectedResult.text));
        toolResult.put("isError", projectedResult.error);
        if (projectedResult.structuredContent != null) {
            toolResult.put("structuredContent", projectedResult.structuredContent);
        }
        return success(message, id, toolResult);
    }

    private List<Map<String, Object>> buildTools() {
        List<Map<String, Object>> tools = new ArrayList<Map<String, Object>>();
        for (McpToolSpec toolSpec : registry.list()) {
            Map<String, Object> tool = new LinkedHashMap<String, Object>();
            tool.put("name", toolSpec.getName());
            tool.put("description", toolSpec.getDescription());
            tool.put("inputSchema", buildInputSchema(toolSpec));
            Map<String, Object> outputSchema = buildOutputSchema(toolSpec);
            if (!outputSchema.isEmpty()) {
                tool.put("outputSchema", outputSchema);
            }
            Map<String, Object> annotations = buildAnnotations(toolSpec);
            if (!annotations.isEmpty()) {
                tool.put("annotations", annotations);
            }
            tools.add(tool);
        }
        return tools;
    }

    private MessageWrapper readResource(Proto message, Object id, JSONObject params) {
        if (params == null) {
            return error(message, id, -32602, "params is required");
        }
        String uri = params.getString("uri");
        if (StringUtils.isBlank(uri)) {
            return error(message, id, -32602, "resource uri is required");
        }
        McpResourceSpec spec = resourceRegistry.get(uri);
        if (spec == null) {
            return error(message, id, -32602, "unknown resource: " + uri);
        }

        List<Map<String, Object>> contents = new ArrayList<Map<String, Object>>();
        Map<String, Object> item = new LinkedHashMap<String, Object>();
        item.put("uri", spec.getUri());
        item.put("mimeType", StringUtils.defaultIfBlank(spec.getMimeType(), "application/json"));
        item.put("text", spec.getContentsJson());
        contents.add(item);
        return success(message, id, mapOf("contents", contents));
    }

    private List<Map<String, Object>> buildResources() {
        List<Map<String, Object>> resources = new ArrayList<Map<String, Object>>();
        for (McpResourceSpec resourceSpec : resourceRegistry.list()) {
            Map<String, Object> resource = new LinkedHashMap<String, Object>();
            resource.put("uri", resourceSpec.getUri());
            resource.put("name", resourceSpec.getName());
            if (StringUtils.isNotBlank(resourceSpec.getTitle())) {
                resource.put("title", resourceSpec.getTitle());
            }
            if (StringUtils.isNotBlank(resourceSpec.getDescription())) {
                resource.put("description", resourceSpec.getDescription());
            }
            if (StringUtils.isNotBlank(resourceSpec.getMimeType())) {
                resource.put("mimeType", resourceSpec.getMimeType());
            }
            resources.add(resource);
        }
        return resources;
    }

    private Map<String, Object> buildAnnotations(McpToolSpec toolSpec) {
        Map<String, Object> annotations = new LinkedHashMap<String, Object>();
        if (StringUtils.isNotBlank(toolSpec.getExampleJson())) {
            annotations.put("example", JSON.parse(toolSpec.getExampleJson()));
        }
        Map<String, Object> outputSchema = buildOutputSchema(toolSpec);
        if (!outputSchema.isEmpty()) {
            annotations.put("outputSchema", outputSchema);
        }
        annotations.put("serverName", toolSpec.getServerName());
        annotations.put("method", toolSpec.getMethod());
        return annotations;
    }

    private Map<String, Object> buildInputSchema(McpToolSpec toolSpec) {
        if (StringUtils.isNotBlank(toolSpec.getInputSchemaJson())) {
            Object schema = JSON.parse(toolSpec.getInputSchemaJson());
            if (schema instanceof JSONObject) {
                return new LinkedHashMap<String, Object>(((JSONObject) schema));
            }
        }
        Map<String, Object> schema = new LinkedHashMap<String, Object>();
        schema.put("type", "object");
        Map<String, Object> properties = new LinkedHashMap<String, Object>();
        properties.put("cid", mapOf("type", "string", "description", "Client request id used by downstream services"));
        properties.put("payload", mapOf("type", "object",
                "description", "Optional nested payload. If omitted, the full arguments object is forwarded."));
        schema.put("properties", properties);
        schema.put("required", new ArrayList<String>());
        schema.put("additionalProperties", true);
        return schema;
    }

    private Map<String, Object> buildOutputSchema(McpToolSpec toolSpec) {
        if (StringUtils.isNotBlank(toolSpec.getOutputSchemaJson())) {
            Object schema = JSON.parse(toolSpec.getOutputSchemaJson());
            if (schema instanceof JSONObject) {
                return new LinkedHashMap<String, Object>(((JSONObject) schema));
            }
        }
        return new LinkedHashMap<String, Object>();
    }

    private List<Map<String, Object>> buildToolContent(String result) {
        List<Map<String, Object>> content = new ArrayList<Map<String, Object>>();
        content.add(mapOf("type", "text", "text", result));
        return content;
    }

    private ProjectedToolResult projectToolResult(McpToolSpec toolSpec, String normalizedResult) {
        boolean error = isErrorResult(normalizedResult);
        Object parsed = tryParseJson(normalizedResult);
        if (parsed == null) {
            return new ProjectedToolResult(normalizedResult, null, error);
        }
        if (error) {
            return new ProjectedToolResult(JSON.toJSONString(parsed), parsed, true);
        }
        Map<String, Object> outputSchema = buildOutputSchema(toolSpec);
        if (outputSchema.isEmpty()) {
            return new ProjectedToolResult(JSON.toJSONString(parsed), parsed, false);
        }
        Object projected = projectBySchema(parsed, outputSchema);
        return new ProjectedToolResult(JSON.toJSONString(projected), projected, false);
    }

    private boolean isErrorResult(String result) {
        try {
            JSONObject json = JSON.parseObject(result);
            if (json == null || !json.containsKey("Code")) {
                return false;
            }
            return !"0000".equals(json.getString("Code")) && !"0".equals(json.getString("Code"));
        } catch (Exception e) {
            return false;
        }
    }

    private Object tryParseJson(String result) {
        try {
            return JSON.parse(result);
        } catch (Exception e) {
            return null;
        }
    }

    private Object projectBySchema(Object source, Map<String, Object> schema) {
        if (schema == null || schema.isEmpty()) {
            return source;
        }
        Object resolvedSource = resolveRootPath(source, schema.get("x-root-path"));
        String type = asString(schema.get("type"));
        if ("object".equalsIgnoreCase(type)) {
            return projectObject(resolvedSource, schema);
        }
        if ("array".equalsIgnoreCase(type)) {
            return projectArray(resolvedSource, schema);
        }
        return coercePrimitive(resolvedSource, type);
    }

    private JSONObject projectObject(Object source, Map<String, Object> schema) {
        JSONObject target = new JSONObject(new LinkedHashMap<String, Object>());
        Map<?, ?> sourceMap = asMap(source);
        Object propertiesObj = schema.get("properties");
        if (!(propertiesObj instanceof Map)) {
            return target;
        }
        Map<?, ?> properties = (Map<?, ?>) propertiesObj;
        for (Map.Entry<?, ?> entry : properties.entrySet()) {
            String fieldName = String.valueOf(entry.getKey());
            if (!(entry.getValue() instanceof Map)) {
                target.put(fieldName, null);
                continue;
            }
            Map<String, Object> childSchema = castMap(entry.getValue());
            Object childSource = sourceMap == null ? null : lookupValue(sourceMap, fieldName, childSchema);
            target.put(fieldName, projectBySchema(childSource, childSchema));
        }
        return target;
    }

    private JSONArray projectArray(Object source, Map<String, Object> schema) {
        if (!(source instanceof Collection) && !(source instanceof JSONArray)) {
            return null;
        }
        Object itemSchemaObj = schema.get("items");
        Map<String, Object> itemSchema = itemSchemaObj instanceof Map ? castMap(itemSchemaObj) : new LinkedHashMap<String, Object>();
        JSONArray target = new JSONArray();
        Iterable<?> iterable = source instanceof JSONArray ? (JSONArray) source : (Collection<?>) source;
        String itemRootPath = asString(schema.get("x-item-root-path"));
        for (Object item : iterable) {
            Object itemSource = resolveRootPath(item, itemRootPath);
            target.add(projectBySchema(itemSource, itemSchema));
        }
        return target;
    }

    private Object coercePrimitive(Object value, String type) {
        if (value == null) {
            return null;
        }
        if (StringUtils.isBlank(type)) {
            return value;
        }
        if ("string".equalsIgnoreCase(type)) {
            return String.valueOf(value);
        }
        if ("integer".equalsIgnoreCase(type)) {
            try {
                if (value instanceof Number) {
                    return ((Number) value).longValue();
                }
                return Long.parseLong(String.valueOf(value));
            } catch (Exception e) {
                return value;
            }
        }
        if ("number".equalsIgnoreCase(type)) {
            try {
                if (value instanceof Number) {
                    return value;
                }
                return new BigDecimal(String.valueOf(value));
            } catch (Exception e) {
                return value;
            }
        }
        if ("boolean".equalsIgnoreCase(type)) {
            if (value instanceof Boolean) {
                return value;
            }
            String text = String.valueOf(value);
            if ("true".equalsIgnoreCase(text) || "1".equals(text)) {
                return Boolean.TRUE;
            }
            if ("false".equalsIgnoreCase(text) || "0".equals(text)) {
                return Boolean.FALSE;
            }
            return value;
        }
        return value;
    }

    private Object lookupValue(Map<?, ?> sourceMap, String fieldName, Map<String, Object> schema) {
        String sourceField = asString(schema.get("x-source"));
        Object value = getByPath(sourceMap, StringUtils.defaultIfBlank(sourceField, fieldName));
        if (value == null) {
            value = sourceMap.get(fieldName);
        }
        return value;
    }

    private Object resolveRootPath(Object source, Object rootPathObj) {
        String rootPath = asString(rootPathObj);
        if (StringUtils.isBlank(rootPath)) {
            return source;
        }
        return getByPath(source, rootPath);
    }

    private Object getByPath(Object source, String path) {
        if (source == null || StringUtils.isBlank(path)) {
            return source;
        }
        String[] segments = path.split("\\.");
        Object current = source;
        for (String segment : segments) {
            if (current == null) {
                return null;
            }
            if (current instanceof Map) {
                current = ((Map<?, ?>) current).get(segment);
            } else if (current instanceof JSONObject) {
                current = ((JSONObject) current).get(segment);
            } else {
                return null;
            }
        }
        return current;
    }

    @SuppressWarnings("unchecked")
    private Map<String, Object> castMap(Object value) {
        return value == null ? new LinkedHashMap<String, Object>() : (Map<String, Object>) value;
    }

    private Map<?, ?> asMap(Object value) {
        if (value instanceof Map) {
            return (Map<?, ?>) value;
        }
        return null;
    }

    private String asString(Object value) {
        return value == null ? "" : String.valueOf(value);
    }

    private String deriveResourceConfigPath(String toolConfigPath) {
        if (StringUtils.isBlank(toolConfigPath)) {
            return "./config/mcpResources.json";
        }
        File toolFile = new File(toolConfigPath);
        File parent = toolFile.getParentFile();
        if (parent == null) {
            return "mcpResources.json";
        }
        return new File(parent, "mcpResources.json").getPath();
    }

    private String negotiateProtocolVersion(JSONObject params) {
        if (params == null) {
            return DEFAULT_PROTOCOL_VERSION;
        }
        String clientVersion = params.getString("protocolVersion");
        if (StringUtils.isBlank(clientVersion)) {
            return DEFAULT_PROTOCOL_VERSION;
        }
        if (SUPPORTED_PROTOCOL_VERSIONS.contains(clientVersion)) {
            return clientVersion;
        }
        return DEFAULT_PROTOCOL_VERSION;
    }

    private String normalizeToolResult(McpToolSpec toolSpec, String method, String result) {
        if (StringUtils.isBlank(result) || !toolSpec.isTemplateTool()) {
            return result;
        }
        try {
            JSONObject outer = JSON.parseObject(result);
            if (outer == null || outer.isEmpty()) {
                return result;
            }
            outer.remove("SRL");
            Object inner = outer.get(method);
            if (inner == null && outer.size() == 1) {
                inner = outer.values().iterator().next();
            }
            if (inner == null) {
                return result;
            }
            if (inner instanceof String) {
                String innerText = (String) inner;
                Object parsedInner = tryParseJson(innerText);
                return parsedInner != null ? JSON.toJSONString(parsedInner) : innerText;
            }
            return JSON.toJSONString(inner);
        } catch (Exception e) {
            return result;
        }
    }

    private MessageWrapper success(Proto message, Object id, Object result) {
        Map<String, Object> body = new LinkedHashMap<String, Object>();
        body.put("jsonrpc", "2.0");
        body.put("id", id);
        body.put("result", result);
        return reply(message, body);
    }

    private MessageWrapper error(Proto message, Object id, int code, String errorMessage) {
        Map<String, Object> error = new LinkedHashMap<String, Object>();
        error.put("code", code);
        error.put("message", errorMessage);
        Map<String, Object> body = new LinkedHashMap<String, Object>();
        body.put("jsonrpc", "2.0");
        body.put("id", id);
        body.put("error", error);
        return reply(message, body);
    }

    private MessageWrapper reply(Proto message, Map<String, Object> body) {
        message.setBody(JSON.toJSONString(body).getBytes(StandardCharsets.UTF_8));
        message.setFormat(Format.REPLY);
        return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
    }

    private MessageWrapper empty(Proto message) {
        message.setBody(new byte[0]);
        message.setFormat(Format.REPLY);
        return new MessageWrapper(MessageWrapper.MessageProtocol.REPLY, message.getSessionId(), message);
    }

    private String buildPayloadJson(McpToolSpec toolSpec, JSONObject payload, Map<String, Object> templateValues) {
        String contentTemplate = toolSpec.getContentTemplate();
        if ("\"\"".equals(contentTemplate)) {
            return "";
        }
        if (StringUtils.isNotBlank(contentTemplate)) {
            return resolveTemplate(contentTemplate, templateValues);
        }
        return payload == null ? "{}" : JSON.toJSONString(payload);
    }

    private Map<String, Object> buildTemplateValues(JSONObject arguments, JSONObject payload) {
        Map<String, Object> values = new HashMap<String, Object>();
        if (arguments != null) {
            for (Map.Entry<String, Object> entry : arguments.entrySet()) {
                values.put(entry.getKey(), entry.getValue());
            }
        }
        if (payload != null) {
            for (Map.Entry<String, Object> entry : payload.entrySet()) {
                values.put(entry.getKey(), entry.getValue());
                values.put("payload." + entry.getKey(), entry.getValue());
            }
        }
        return values;
    }

    private String resolveTemplate(String template, Map<String, Object> values) {
        if (StringUtils.isBlank(template)) {
            return "";
        }
        Matcher matcher = TEMPLATE_PATTERN.matcher(template);
        StringBuffer buffer = new StringBuffer();
        while (matcher.find()) {
            String replacement = applyTemplateExpression(matcher.group(1), values);
            matcher.appendReplacement(buffer, Matcher.quoteReplacement(replacement));
        }
        matcher.appendTail(buffer);
        return buffer.toString();
    }

    private String applyTemplateExpression(String expression, Map<String, Object> values) {
        String[] parts = expression.split("\\|");
        String key = parts[0].trim();
        Object value = values.get(key);
        String text = value == null ? "" : String.valueOf(value);
        for (int i = 1; i < parts.length; i++) {
            String op = parts[i].trim();
            if ("nodot".equalsIgnoreCase(op)) {
                text = text.replace(".", "");
            } else if ("trim".equalsIgnoreCase(op)) {
                text = text.trim();
            } else if (op.startsWith("default:")) {
                if (text == null || text.trim().isEmpty()) {
                    text = op.substring("default:".length());
                }
            } else if ("lower".equalsIgnoreCase(op)) {
                text = text.toLowerCase();
            } else if ("upper".equalsIgnoreCase(op)) {
                text = text.toUpperCase();
            } else if (op.startsWith("replace:")) {
                String arg = op.substring("replace:".length());
                String[] replaceArgs = arg.split(":", 2);
                String from = replaceArgs.length > 0 ? replaceArgs[0] : "";
                String to = replaceArgs.length > 1 ? replaceArgs[1] : "";
                text = text.replace(from, to);
            }
        }
        return text;
    }

    private Map<String, Object> mapOf(Object... kvs) {
        Map<String, Object> map = new LinkedHashMap<String, Object>();
        for (int i = 0; i < kvs.length; i += 2) {
            map.put(String.valueOf(kvs[i]), kvs[i + 1]);
        }
        return map;
    }

    private static final class ProjectedToolResult {
        private final String text;
        private final Object structuredContent;
        private final boolean error;

        private ProjectedToolResult(String text, Object structuredContent, boolean error) {
            this.text = text;
            this.structuredContent = structuredContent;
            this.error = error;
        }
    }
}
