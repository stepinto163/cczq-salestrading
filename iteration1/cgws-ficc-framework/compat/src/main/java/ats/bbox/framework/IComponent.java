package ats.bbox.framework;

import org.apache.commons.configuration.AbstractConfiguration;

public interface IComponent {
    void setConfig(AbstractConfiguration configuration);

    AbstractConfiguration getConfig();

    String getName();

    void setName(String name);
}
