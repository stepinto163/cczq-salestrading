package cgws.ficc.framework.compat;

import ats.bbox.framework.IComponent;
import ats.bbox.framework.StreamEngine;

import org.apache.commons.configuration.Configuration;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/** Interprets the existing unstart.mode/unstart.list vocabulary. */
public final class DeferredStreams {
    private final Set<String> deferred = new LinkedHashSet<>();
    private final Map<String, List<String>> groups = new LinkedHashMap<>();
    private final List<String> defaults = new ArrayList<>();

    public DeferredStreams(Configuration configuration, StreamEngine engine) {
        String mode =
                configuration.getString(
                        "unstart.mode",
                        configuration.getBoolean("startOAByOther", false) ? "oa" : "");
        if (mode.matches("(?i)OAs?|IOAs?")) {
            engine.getOutputStreams().forEach(stream -> defaults.add(stream.getName()));
        }
        if (mode.matches("(?i)IAs?|IOAs?")) {
            engine.getInputStreams().forEach(stream -> defaults.add(stream.getName()));
        }
        if ("cust".equalsIgnoreCase(mode) || "custom".equalsIgnoreCase(mode)) {
            String text = String.join(",", configuration.getStringArray("unstart.list"));
            Pattern token =
                    Pattern.compile("\\s*([^,{}=]+)\\s*(?:=\\s*\\{([^{}]*)\\})?\\s*(?:,|$)");
            Matcher matcher = token.matcher(text);
            int position = 0;
            while (matcher.find()) {
                if (matcher.start() != position) {
                    throw new IllegalArgumentException("Invalid unstart.list syntax");
                }
                String group = matcher.group(1).trim();
                if (matcher.group(2) == null) {
                    defaults.add(group);
                } else {
                    List<String> members = new ArrayList<>();
                    for (String member : matcher.group(2).split(",")) {
                        if (!member.trim().isEmpty()) {
                            members.add(member.trim());
                        }
                    }
                    groups.put(group, members);
                    deferred.addAll(members);
                }
                position = matcher.end();
            }
            if (position != text.length()) {
                throw new IllegalArgumentException("Invalid unstart.list syntax");
            }
        }
        deferred.addAll(defaults);
    }

    public boolean contains(IComponent component) {
        return deferred.contains(component.getName());
    }

    public String[] forOwner(String owner) {
        List<String> selected = groups.getOrDefault(owner, defaults);
        return selected.toArray(new String[0]);
    }
}
