package com.gateway.mcp;

public class McpResourceSpec {
    private final String uri;
    private final String name;
    private final String title;
    private final String description;
    private final String mimeType;
    private final String kind;
    private final String contentsJson;

    public McpResourceSpec(String uri, String name, String title, String description, String mimeType, String kind,
                           String contentsJson) {
        this.uri = uri;
        this.name = name;
        this.title = title;
        this.description = description;
        this.mimeType = mimeType;
        this.kind = kind;
        this.contentsJson = contentsJson;
    }

    public String getUri() {
        return uri;
    }

    public String getName() {
        return name;
    }

    public String getTitle() {
        return title;
    }

    public String getDescription() {
        return description;
    }

    public String getMimeType() {
        return mimeType;
    }

    public String getKind() {
        return kind;
    }

    public String getContentsJson() {
        return contentsJson;
    }
}
