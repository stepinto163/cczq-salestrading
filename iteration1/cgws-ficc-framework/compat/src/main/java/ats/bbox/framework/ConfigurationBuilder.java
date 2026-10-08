package ats.bbox.framework;

import org.apache.commons.configuration.AbstractConfiguration;
import org.apache.commons.configuration.CompositeConfiguration;
import org.apache.commons.configuration.MapConfiguration;
import org.apache.commons.configuration.PropertiesConfiguration;
import org.apache.commons.configuration.XMLConfiguration;

import java.util.Collections;
import java.util.Map;

/** Keeps Commons Configuration 1.9 semantics, including first-source-wins composition. */
public class ConfigurationBuilder {
    public static final String XML = "xml";
    public static final String INI = "ini";
    public static final String NINI = "nini";
    public static final String ZK = "zk";
    public static final String AQ = "aq";
    public static final String THIS = "this";
    public static final String EMPTY = "null";

    public static AbstractConfiguration getConfiguration(
            String[] types, String[] files, Map parameters) throws EException {
        if (types == null || types.length == 0) {
            throw new EException("Missing config-type");
        }
        try {
            if (types.length > 1) {
                if (files == null || files.length != types.length) {
                    throw new EException("config-type and config-file counts differ");
                }
                CompositeConfiguration result = new CompositeConfiguration();
                for (int index = 0; index < types.length; index++) {
                    result.addConfiguration(
                            getConfiguration(
                                    new String[] {types[index]},
                                    new String[] {files[index]},
                                    parameters));
                }
                return result;
            }
            String type = types[0];
            if (EMPTY.equals(type)) {
                return new MapConfiguration(new java.util.HashMap<>());
            }
            if (THIS.equals(type)) {
                return new MapConfiguration(
                        parameters == null ? Collections.emptyMap() : parameters);
            }
            if (ZK.equals(type) || AQ.equals(type)) {
                throw new EException("Configuration provider unsupported by the baseline: " + type);
            }
            if (INI.equals(type) || XML.equals(type) || NINI.equals(type)) {
                if (files == null || files.length == 0 || files[0] == null) {
                    throw new EException("Missing config-file for " + type);
                }
                if (INI.equals(type)) {
                    return new PropertiesConfiguration(files[0]);
                }
                if (XML.equals(type)) {
                    return new XMLConfiguration(files[0]);
                }
                return NINIConfigurationHelper.getPropertiesConfiguration(
                        (String) parameters.get(ComponentBuilder.ATTRIB_NAME), files[0]);
            }
            AbstractConfigProvider provider =
                    (AbstractConfigProvider)
                            Class.forName(type).getDeclaredConstructor().newInstance();
            provider.setConfigurationClass(parameters);
            return provider.getConfiguration();
        } catch (EException error) {
            throw error;
        } catch (Exception error) {
            throw new EException("Cannot load component configuration", error);
        }
    }
}
