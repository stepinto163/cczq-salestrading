package com.gateway.mcp;

import java.io.File;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

public final class McpToolConfigLoader {
    private McpToolConfigLoader() {
    }

    public static Snapshot load(String configPath) throws Exception {
        byte[] bytes = Files.readAllBytes(Paths.get(configPath));
        String rawContent = new String(bytes, StandardCharsets.UTF_8);
        if (rawContent.startsWith("\uFEFF")) {
            rawContent = rawContent.substring(1);
        }
        List<String> lines = new ArrayList<String>();
        String[] parts = rawContent.split("\\r?\\n", -1);
        for (String part : parts) {
            lines.add(part);
        }
        File file = new File(configPath);
        String version = String.valueOf(file.lastModified());
        return new Snapshot(rawContent, lines, version);
    }

    public static final class Snapshot {
        private final String rawContent;
        private final List<String> lines;
        private final String version;

        public Snapshot(String rawContent, List<String> lines, String version) {
            this.rawContent = rawContent;
            this.lines = lines;
            this.version = version;
        }

        public String getRawContent() {
            return rawContent;
        }

        public List<String> getLines() {
            return lines;
        }

        public String getVersion() {
            return version;
        }
    }
}
