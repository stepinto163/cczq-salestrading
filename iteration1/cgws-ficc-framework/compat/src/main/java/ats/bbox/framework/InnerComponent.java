package ats.bbox.framework;

import org.apache.commons.configuration.Configuration;

import java.util.Iterator;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

public class InnerComponent {
    private static final Map<String, Class<?>> CLASSES = new ConcurrentHashMap<>();

    public static Class<?> get(String name) {
        return CLASSES.get(name);
    }

    public static void load(Configuration configuration) {
        Iterator<String> keys = configuration.getKeys();
        while (keys.hasNext()) {
            String name = keys.next();
            try {
                CLASSES.put(name, Class.forName(configuration.getString(name)));
            } catch (ClassNotFoundException error) {
                throw new IllegalStateException("Cannot resolve inner component " + name, error);
            }
        }
    }
}
