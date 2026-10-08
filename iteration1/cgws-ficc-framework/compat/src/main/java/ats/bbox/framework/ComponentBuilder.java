package ats.bbox.framework;

import org.apache.commons.configuration.AbstractConfiguration;
import org.apache.commons.configuration.HierarchicalConfiguration;
import org.apache.commons.configuration.XMLConfiguration;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

public class ComponentBuilder {
    public static final String NODE_NAME = "component";
    public static final String ATTRIB_NAME = "name";
    public static final String ATTRIB_TYPE = "config-type";
    public static final String ATTRIB_FILE = "config-file";
    public static final String MULTI_FLAG = ",";

    public static IComponent getComponent(
            String name,
            String[] types,
            String[] files,
            Map<String, String> attributes,
            String className)
            throws EException {
        AbstractConfiguration configuration =
                ConfigurationBuilder.getConfiguration(types, files, attributes);
        try {
            IComponent component =
                    (IComponent) Class.forName(className).getDeclaredConstructor().newInstance();
            component.setName(name);
            component.setConfig(configuration);
            return component;
        } catch (Exception error) {
            throw new EException(
                    "Cannot construct component " + name + " (" + className + ")", error);
        }
    }

    public static List<IComponent> getComponents(String file, String section, Class checker)
            throws EException {
        try {
            return getComponents(new XMLConfiguration(file), section, checker);
        } catch (Exception error) {
            throw new EException("Cannot load components from " + file, error);
        }
    }

    public static List<IComponent> getComponents(
            XMLConfiguration configuration, String section, Class checker) throws EException {
        String path = section == null ? NODE_NAME : section + "." + NODE_NAME;
        List<IComponent> result = new ArrayList<>();
        Set<String> names = new HashSet<>();
        for (HierarchicalConfiguration entry : configuration.configurationsAt(path)) {
            String name = entry.getString("[@name]");
            if (name == null || name.trim().isEmpty() || !names.add(name)) {
                throw new EException("Missing or duplicate component name: " + name);
            }
            String className = entry.getString("");
            if (className == null) {
                throw new EException("Missing class for " + name);
            }
            className = className.trim();
            if (!className.contains(".")) {
                Class<?> alias = InnerComponent.get(className);
                if (alias == null) {
                    throw new EException("Unknown component alias: " + className);
                }
                className = alias.getName();
            }
            Map<String, String> attributes = new LinkedHashMap<>();
            Iterator<String> keys = entry.getKeys();
            while (keys.hasNext()) {
                String key = keys.next();
                if (key.startsWith("[@") && key.endsWith("]")) {
                    String attribute = key.substring(2, key.length() - 1);
                    if (!ATTRIB_NAME.equals(attribute)
                            && !ATTRIB_FILE.equals(attribute)
                            && !ATTRIB_TYPE.equals(attribute)) {
                        attributes.put(attribute, String.join(",", entry.getStringArray(key)));
                    }
                }
            }
            IComponent component =
                    getComponent(
                            name,
                            entry.getStringArray("[@config-type]"),
                            entry.getStringArray("[@config-file]"),
                            attributes,
                            className);
            if (checker != null && !checker.isInstance(component)) {
                throw new EException("Component " + name + " must implement " + checker.getName());
            }
            result.add(component);
        }
        return result;
    }
}
