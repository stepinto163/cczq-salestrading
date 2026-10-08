package ats.bbox.framework;

import org.apache.commons.configuration.AbstractConfiguration;

public class SimpleComponent implements IComponent {
    private AbstractConfiguration configuration;
    private String name;

    public AbstractConfiguration getConfig() {
        return configuration;
    }

    public void setConfig(AbstractConfiguration configuration) {
        this.configuration = configuration;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }
}
