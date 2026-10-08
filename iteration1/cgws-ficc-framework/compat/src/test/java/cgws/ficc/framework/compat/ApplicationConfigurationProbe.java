package cgws.ficc.framework.compat;

import ats.bbox.framework.ComponentBuilder;
import ats.bbox.framework.IComponent;
import ats.bbox.framework.ILifecycle;
import ats.bbox.framework.StreamEngine;
import ats.bbox.framework.resource.IResource;

import org.apache.commons.configuration.PropertiesConfiguration;

import java.util.Collections;
import java.util.List;

/**
 * Constructs the real application's unchanged graph, without init/start or external connections.
 */
public final class ApplicationConfigurationProbe {
    public static void main(String[] args) throws Exception {
        int providers =
                Collections.list(
                                ComponentBuilder.class
                                        .getClassLoader()
                                        .getResources("ats/bbox/framework/Server.class"))
                        .size();
        if (providers != 1) {
            throw new AssertionError("Expected one framework provider, got " + providers);
        }
        PropertiesConfiguration configuration = new PropertiesConfiguration(args[0]);
        List<IComponent> resources =
                ComponentBuilder.getComponents(
                        configuration.getString("resource.file"), null, IResource.class);
        for (IComponent resource : resources) {
            if (((ILifecycle) resource).getStatus() != ILifecycle.STATUS.CREATED) {
                throw new AssertionError("Constructor changed lifecycle: " + resource.getName());
            }
            for (String handler : resource.getConfig().getStringArray("handlerList")) {
                Class<?> type =
                        Class.forName(
                                resource.getConfig().getString("handler." + handler + ".class"));
                // Resolve all handler signatures, including transitive framework types.
                type.getMethods();
                type.getDeclaredConstructor().newInstance();
            }
        }
        StreamEngine streams = new StreamEngine();
        streams.load(configuration.getString("streams.file"));
        System.out.println(
                "CONFIGURATION_GRAPH_OK resources="
                        + resources.size()
                        + " streams="
                        + streams.getAllStreams().size()
                        + " initialized=false");
    }
}
