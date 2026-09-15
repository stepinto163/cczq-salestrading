package ficc.common;

import ficc.common.client.StcBaseDataClient;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.List;
import java.util.Properties;
import java.util.function.Consumer;

/** Loads deployment properties and reports only the settings needed to diagnose adapter selection. */
public final class ServiceConfiguration {
    private ServiceConfiguration() {}

    public static Properties load(String serviceName, String databaseFile, String adapterFile) throws IOException {
        return load(serviceName, databaseFile, adapterFile, System.out::println);
    }

    static Properties load(String serviceName, String databaseFile, String adapterFile,
            Consumer<String> log) throws IOException {
        Properties configuration = new Properties();
        Path databasePath = Path.of(databaseFile).toAbsolutePath().normalize();
        loadFile(configuration, databasePath);
        Path adapterPath = null;
        if (adapterFile != null && !adapterFile.isBlank()) {
            adapterPath = Path.of(adapterFile).toAbsolutePath().normalize();
            loadFile(configuration, adapterPath);
        }
        log.accept(serviceName + " configuration: workingDirectory=" + Path.of("").toAbsolutePath().normalize());
        log.accept(serviceName + " configuration: quote.properties=" + databasePath
                + ", adapter.properties=" + (adapterPath == null ? "<not configured>" : adapterPath));
        List<String> securityIds = Arrays.stream(configuration.getProperty("base.securityIds", "").split(","))
                .map(String::trim).filter(value -> !value.isEmpty()).distinct().toList();
        String baseMode = configuration.getProperty("base.mode", "simulator");
        String loadMode = "stc".equals(baseMode) ? "stc-client-on-demand" : "simulator";
        // Do not dump properties: the same files can contain database and service credentials.
        log.accept(serviceName + " adapters: identity.mode=" + configuration.getProperty("identity.mode", "simulator")
                + ", base.mode=" + baseMode + ", base.loadMode=" + loadMode
                + ", crm.mode=" + configuration.getProperty("crm.mode", "simulator")
                + ", securityIdsCount=" + securityIds.size() + ", securityIdsSample=" + securityIds.stream().limit(5).toList());
        var source = StcBaseDataClient.class.getProtectionDomain().getCodeSource();
        log.accept(serviceName + " base adapter: diagnostics=stc-base-config-v2, loadedFrom="
                + (source == null ? "<unavailable>" : source.getLocation()));
        return configuration;
    }

    private static void loadFile(Properties configuration, Path path) throws IOException {
        try (var reader = Files.newBufferedReader(path)) {
            configuration.load(reader);
        }
    }
}
