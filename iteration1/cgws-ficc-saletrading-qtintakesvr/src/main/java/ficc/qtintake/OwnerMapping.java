package ficc.qtintake;

import org.apache.log4j.Logger;

import java.io.BufferedReader;
import java.io.StringReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.Set;

/**
 * Maps a QTrade account (FIX OwnQtNO 18004) to the STC userid that owns the quote.
 *
 * <p>STC has no QTrade account column to query, so the mapping is operator-owned: a properties file
 * holding one {@code qtradeAccount=stcUserid} entry per account, plus an optional default for
 * accounts the file does not cover. The file is re-read whenever it changes, so adding a user needs
 * no restart. Missing configuration fails at startup; an unmapped account at runtime leaves the
 * quote in the inbox for a retry.
 */
public final class OwnerMapping implements QuoteForwarder.OwnerResolver {
    private static final Logger LOGGER = Logger.getLogger(OwnerMapping.class);

    private final Path path;
    private final String defaultUserId;
    private Map<String, String> mapping = new LinkedHashMap<>();
    private long modifiedAt = Long.MIN_VALUE;

    private OwnerMapping(Path path, String defaultUserId) {
        this.path = path;
        this.defaultUserId = defaultUserId;
    }

    public static OwnerMapping load(Properties configuration) throws Exception {
        String filename = trim(configuration.getProperty("owner.mapFile"));
        String defaultUserId = trim(configuration.getProperty("owner.defaultUserId"));
        Path path = filename == null ? null : Paths.get(expand(filename));
        if (path == null && defaultUserId == null) {
            throw new IllegalStateException(
                    "Configure owner.mapFile, owner.defaultUserId or both: STC has no QTrade"
                            + " account column, so the quote owner cannot be resolved otherwise");
        }
        OwnerMapping owners = new OwnerMapping(path, defaultUserId);
        owners.reload(true);
        LOGGER.info(
                "OWNER_MAPPING state=LOADED source="
                        + (path == null ? "owner.defaultUserId" : path)
                        + " accounts="
                        + owners.mapping.size()
                        + " defaultUserId="
                        + (defaultUserId == null ? "<none>" : defaultUserId));
        return owners;
    }

    @Override
    public String resolve(String qtradeAccount) throws Exception {
        String account = trim(qtradeAccount);
        reload(false);
        String owner = account == null ? null : mapping.get(account);
        if (owner != null) {
            LOGGER.info("OWNER_RESOLVED sourceUserId=" + account + " ownerUserId=" + owner);
            return owner;
        }
        if (defaultUserId != null) {
            LOGGER.warn(
                    "OWNER_FALLBACK sourceUserId="
                            + account
                            + " ownerUserId="
                            + defaultUserId
                            + " reason=NOT_MAPPED");
            return defaultUserId;
        }
        throw new IllegalStateException(
                "No STC userid mapped for QTrade OwnQtNO="
                        + account
                        + " and owner.defaultUserId is not configured");
    }

    /** Re-reads the file when its timestamp changes; a missing file keeps the last mapping. */
    private synchronized void reload(boolean failWhenMissing) throws Exception {
        if (path == null) {
            return;
        }
        if (!Files.isRegularFile(path)) {
            if (failWhenMissing) {
                throw new IllegalStateException("owner.mapFile does not exist: " + path);
            }
            LOGGER.warn("OWNER_MAPPING state=MISSING source=" + path);
            return;
        }
        long modified = Files.getLastModifiedTime(path).toMillis();
        if (modified == modifiedAt) {
            return;
        }
        List<String> lines = Files.readAllLines(path, StandardCharsets.UTF_8);
        Set<String> seen = new LinkedHashSet<>();
        for (String line : lines) {
            String key = keyOf(line);
            if (key != null && !seen.add(key)) {
                // Properties would silently keep the last value, which can mis-attribute quotes.
                throw new IllegalStateException("Duplicate QTrade account in " + path + ": " + key);
            }
        }
        Properties entries = new Properties();
        try (BufferedReader reader =
                new BufferedReader(new StringReader(String.join("\n", lines)))) {
            entries.load(reader);
        }
        Map<String, String> parsed = new LinkedHashMap<>();
        for (String name : entries.stringPropertyNames()) {
            String account = trim(name);
            String user = trim(entries.getProperty(name));
            if (account == null || user == null) {
                LOGGER.warn("OWNER_MAPPING state=SKIPPED account=" + name + " reason=INCOMPLETE");
                continue;
            }
            parsed.put(account, user);
        }
        mapping = parsed;
        modifiedAt = modified;
        LOGGER.info("OWNER_MAPPING state=RELOADED accounts=" + parsed.size());
    }

    /** Key of a properties line, or null for comments and blank lines. */
    private static String keyOf(String line) {
        String text = line.trim();
        if (text.isEmpty() || text.startsWith("#") || text.startsWith("!")) {
            return null;
        }
        int separator = text.indexOf('=');
        if (separator < 0) {
            separator = text.indexOf(':');
        }
        return trim(separator < 0 ? text : text.substring(0, separator));
    }

    private static String expand(String filename) {
        if (!filename.startsWith("$ATS_ROOT/")) {
            return filename;
        }
        String root = System.getenv("ATS_ROOT");
        if (root == null || root.trim().isEmpty()) {
            throw new IllegalStateException("ATS_ROOT is required to resolve " + filename);
        }
        return Paths.get(root).resolve(filename.substring(10)).toString();
    }

    private static String trim(String value) {
        if (value == null) {
            return null;
        }
        String trimmed = value.trim();
        return trimmed.isEmpty() ? null : trimmed;
    }
}
