package com.gateway.mcp;

import org.apache.commons.lang3.StringUtils;

public class McpToolSpec {
    private final String name;
    private final String serverName;
    private final String method;
    private final String description;
    private final String inputSchemaJson;
    private final String outputSchemaJson;
    private final String exampleJson;
    private final String methodTemplate;
    private final String contentTemplate;

    public McpToolSpec(String name, String serverName, String method, String description, String inputSchemaJson,
                       String outputSchemaJson, String exampleJson, String methodTemplate, String contentTemplate) {
        this.name = name;
        this.serverName = serverName;
        this.method = method;
        this.description = description;
        this.inputSchemaJson = inputSchemaJson;
        this.outputSchemaJson = outputSchemaJson;
        this.exampleJson = exampleJson;
        this.methodTemplate = methodTemplate;
        this.contentTemplate = contentTemplate;
    }

    public String getName() {
        return name;
    }

    public String getServerName() {
        return serverName;
    }

    public String getMethod() {
        return method;
    }

    public String getDescription() {
        return description;
    }

    public String getInputSchemaJson() {
        return inputSchemaJson;
    }

    public String getOutputSchemaJson() {
        return outputSchemaJson;
    }

    public String getExampleJson() {
        return exampleJson;
    }

    public String getMethodTemplate() {
        return methodTemplate;
    }

    public String getContentTemplate() {
        return contentTemplate;
    }

    public boolean isTemplateTool() {
        return StringUtils.isNotBlank(methodTemplate) || StringUtils.isNotBlank(contentTemplate);
    }
}
