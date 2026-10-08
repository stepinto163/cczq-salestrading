package cgws.ficc.framework.compat;

import ats.app.ym.ceda.server.ContentHandler;

import org.apache.commons.configuration.Configuration;

import java.util.Hashtable;

/** Preserves legacy exact-topic precedence and wildcard comparison. */
public final class HandlerRegistry {
    private final Hashtable<String, ContentHandler> handlers = new Hashtable<>();

    public void load(Configuration configuration) throws ReflectiveOperationException {
        for (String name : configuration.getStringArray("handlerList")) {
            String prefix = "handler." + name;
            String topic = configuration.getString(prefix + ".topic");
            String className = configuration.getString(prefix + ".class");
            ContentHandler handler =
                    Class.forName(className)
                            .asSubclass(ContentHandler.class)
                            .getDeclaredConstructor()
                            .newInstance();
            handler.setTopic(topic);
            handler.setPoolKey(configuration.getString(prefix + ".pool", ""));
            handlers.put(topic, handler);
        }
    }

    public ContentHandler find(String topic) {
        ContentHandler exact = handlers.get(topic);
        if (exact != null) {
            return exact;
        }
        for (String pattern : handlers.keySet()) {
            if (compareTopic(pattern, topic) >= 0) {
                return handlers.get(pattern);
            }
        }
        return null;
    }

    public static int compareTopic(String source, String destination) {
        if (source.equals(destination)) {
            return 0;
        }
        if ("**".equals(source)) {
            return 1;
        }
        if ("**".equals(destination)) {
            return -1;
        }
        String[] left = source.split("\\.");
        String[] right = destination.split("\\.");
        int result = -2;
        for (int index = 0; index < left.length && index < right.length; index++) {
            result =
                    left[index].equals(right[index])
                            ? 0
                            : wildcard(left[index]) ? 1 : wildcard(right[index]) ? -1 : -2;
            if (result != 0) {
                break;
            }
        }
        if (result == 0) {
            if (left.length > right.length) {
                result = wildcard(left[right.length]) ? 1 : -2;
            } else if (left.length < right.length) {
                result = wildcard(right[left.length]) ? -1 : -2;
            }
        }
        return result;
    }

    private static boolean wildcard(String segment) {
        return "*".equals(segment) || "**".equals(segment);
    }
}
