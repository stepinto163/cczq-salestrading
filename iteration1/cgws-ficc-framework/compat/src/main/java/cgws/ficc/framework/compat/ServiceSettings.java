package cgws.ficc.framework.compat;

import ats.foundation.utils.config.ATSROOTConfig;
import ats.foundation.utils.config.PropertiesHelper;
import ats.foundation.utils.config.mqserver.RegisterParams;
import ats.foundation.utils.config.mqserver.ServerParams;

import cgws.ficc.framework.dps.ServiceEndpoint;

import java.util.ArrayList;
import java.util.List;

/** Converts existing ATSConfig.ini values without changing their interpretation. */
public final class ServiceSettings {
    public final String key;
    public final String name;
    public final ServiceEndpoint endpoint;
    public final boolean register;
    public final boolean standby;
    public final String registryHosts;

    private ServiceSettings(String key, ServerParams parameters) {
        this.key = key;
        name = parameters.ServiceName;
        endpoint = new ServiceEndpoint(parameters.Host, parameters.Port, true);
        register = parameters.RegisterEnable;
        standby = parameters.RegType == 2;
        List<String> hosts = new ArrayList<>();
        for (RegisterParams registry : parameters.RegisterConfigs) {
            hosts.add(registry.Host + ":" + registry.Port);
        }
        registryHosts = String.join(",", hosts);
        if (register && registryHosts.isEmpty()) {
            throw new IllegalArgumentException("No registry configured for " + key);
        }
    }

    public static ServiceSettings load(String key) {
        try {
            PropertiesHelper configuration = ATSROOTConfig.getATSROOTConfig();
            return new ServiceSettings(key, configuration.findServerParam(key));
        } catch (Exception error) {
            throw new IllegalStateException("Invalid service configuration: " + key, error);
        }
    }
}
