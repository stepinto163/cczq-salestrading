package ficc.common;

import ficc.common.client.StcBaseDataClient;

import java.io.BufferedReader;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.security.CodeSource;
import java.util.Arrays;
import java.util.List;
import java.util.Properties;
import java.util.function.Consumer;
import java.util.stream.Collectors;

/**
 * Loads deployment properties and reports only the settings needed to diagnose adapter selection.
 */
public final class ServiceConfiguration {
    private ServiceConfiguration() {}

    public static Properties load(String serviceName, String databaseFile, String adapterFile)
            throws IOException {
        Properties context = new Properties();
        context.setProperty("diagnostics.serviceName", serviceName);
        String files =
                "workingDirectory="
                        + Paths.get("").toAbsolutePath().normalize()
                        + ", quote.properties="
                        + databaseFile
                        + ", adapter.properties="
                        + adapterFile;
        try {
            return ComponentLog.initialize(
                    context,
                    "CONFIGURATION",
                    "files",
                    "ServiceConfiguration",
                    files,
                    () ->
                            load(
                                    serviceName,
                                    databaseFile,
                                    adapterFile,
                                    org.apache.log4j.Logger.getLogger("ficc.startup")::info));
        } catch (IOException error) {
            throw error;
        } catch (Exception error) {
            throw new IOException("Cannot load service configuration", error);
        }
    }

    static Properties load(
            String serviceName, String databaseFile, String adapterFile, Consumer<String> log)
            throws IOException {
        Properties configuration = new Properties();
        Path databasePath = Paths.get(databaseFile).toAbsolutePath().normalize();
        loadFile(configuration, databasePath);
        Path adapterPath = null;
        if (adapterFile != null && !adapterFile.trim().isEmpty()) {
            adapterPath = Paths.get(adapterFile).toAbsolutePath().normalize();
            loadFile(configuration, adapterPath);
        }
        log.accept(
                serviceName
                        + " configuration: workingDirectory="
                        + Paths.get("").toAbsolutePath().normalize());
        log.accept(
                serviceName
                        + " configuration: quote.properties="
                        + databasePath
                        + ", adapter.properties="
                        + (adapterPath == null ? "<not configured>" : adapterPath));
        List<String> securityIds =
                Arrays.stream(configuration.getProperty("base.securityIds", "").split(","))
                        .map(String::trim)
                        .filter(value -> !value.isEmpty())
                        .distinct()
                        .collect(Collectors.toList());
        String baseMode = configuration.getProperty("base.mode", "simulator");
        String loadMode = "stc".equals(baseMode) ? "stc-client-on-demand" : "simulator";
        // Do not dump properties: the same files can contain database and service credentials.
        log.accept(
                serviceName
                        + " adapters: identity.mode="
                        + configuration.getProperty("identity.mode", "simulator")
                        + ", base.mode="
                        + baseMode
                        + ", base.loadMode="
                        + loadMode
                        + ", crm.mode="
                        + configuration.getProperty("crm.mode", "simulator")
                        + ", securityIdsCount="
                        + securityIds.size()
                        + ", securityIdsSample="
                        + securityIds.stream().limit(5).collect(Collectors.toList()));
        CodeSource source = StcBaseDataClient.class.getProtectionDomain().getCodeSource();
        log.accept(
                serviceName
                        + " base adapter: diagnostics=stc-base-config-v2, loadedFrom="
                        + (source == null ? "<unavailable>" : source.getLocation()));
        configuration.setProperty("diagnostics.serviceName", serviceName);
        return configuration;
    }

    private static void loadFile(Properties configuration, Path path) throws IOException {
        try (BufferedReader reader = Files.newBufferedReader(path)) {
            configuration.load(reader);
        }
    }
}
