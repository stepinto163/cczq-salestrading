package ats.bbox.framework;

import org.apache.commons.configuration.ConfigurationException;
import org.apache.commons.configuration.PropertiesConfiguration;

import java.io.IOException;
import java.io.Reader;
import java.io.Writer;

public class NINIConfigurationHelper {
    public static PropertiesConfiguration getPropertiesConfiguration(String name, String file)
            throws ConfigurationException {
        PropertiesConfiguration configuration = new PropertiesConfiguration();
        configuration.setFileName(file);
        configuration.setIOFactory(
                new PropertiesConfiguration.IOFactory() {
                    public PropertiesConfiguration.PropertiesReader createPropertiesReader(
                            Reader reader, char delimiter) {
                        return new SectionReader(reader, delimiter, name);
                    }

                    public PropertiesConfiguration.PropertiesWriter createPropertiesWriter(
                            Writer writer, char delimiter) {
                        throw new UnsupportedOperationException("NINI configuration is read-only");
                    }
                });
        configuration.load();
        return configuration;
    }

    private static final class SectionReader extends PropertiesConfiguration.PropertiesReader {
        private final String section;
        private boolean entered;
        private boolean finished;

        private SectionReader(Reader reader, char delimiter, String name) {
            super(reader, delimiter);
            section = "[" + name + "]";
        }

        public String readProperty() throws IOException {
            if (finished) {
                return null;
            }
            StringBuilder property = new StringBuilder();
            String line;
            while ((line = readLine()) != null) {
                line = line.trim();
                if (!entered) {
                    entered = section.equals(line);
                    continue;
                }
                if (line.startsWith("[")) {
                    finished = true;
                    return null;
                }
                if (line.isEmpty() || line.startsWith("#") || line.startsWith("!")) {
                    continue;
                }
                int slashes = 0;
                for (int index = line.length() - 1;
                        index >= 0 && line.charAt(index) == '\\';
                        index--) {
                    slashes++;
                }
                if (slashes % 2 == 1) {
                    property.append(line, 0, line.length() - 1);
                } else {
                    return property.append(line).toString();
                }
            }
            finished = true;
            return null;
        }
    }
}
