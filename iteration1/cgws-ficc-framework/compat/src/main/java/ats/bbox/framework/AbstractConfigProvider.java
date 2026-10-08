package ats.bbox.framework;

import org.apache.commons.configuration.AbstractConfiguration;

import java.util.Map;

public abstract class AbstractConfigProvider {
    private Map<String, String> parameter;

    public void setConfigurationClass(Map<String, String> parameter) {
        this.parameter = parameter;
    }

    public Map<String, String> getParameter() {
        return parameter;
    }

    public abstract AbstractConfiguration getConfiguration() throws EException;
}
