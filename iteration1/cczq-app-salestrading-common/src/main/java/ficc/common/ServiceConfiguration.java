package ficc.common;

import ficc.common.client.StcBaseDataClient;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Properties;
import java.util.function.Consumer;

/** Loads one deployment properties file. Adapter settings are in that same file. */
public final class ServiceConfiguration {
    private ServiceConfiguration() {}

    public static Properties load(String serviceName, String databaseFile, String adapterFile) throws IOException {
        Properties context = new Properties();
        context.setProperty("diagnostics.serviceName", serviceName);
        String files = "workingDirectory=" + Path.of("").toAbsolutePath().normalize()
                + ", quote.properties=" + databaseFile;
        try {
            return ComponentLog.initialize(context, "CONFIGURATION", "single-file", "ServiceConfiguration", files,
                    () -> load(serviceName, databaseFile, adapterFile, org.apache.log4j.Logger.getLogger("ficc.startup")::info));
        } catch (IOException error) {
            throw error;
        } catch (Exception error) {
            throw new IOException("Cannot load service configuration", error);
        }
    }

    static Properties load(String serviceName, String databaseFile, String adapterFile,
            Consumer<String> log) throws IOException {
        Properties configuration = new Properties();
        Path databasePath = Path.of(databaseFile).toAbsolutePath().normalize();
        // Reject the old override explicitly so an upgrade cannot silently switch STC to simulator mode.
        if (adapterFile != null && !adapterFile.isBlank()) {
            throw new IOException("adapter.properties is no longer supported. Move its identity.*, base.* and crm.* settings into "
                    + databasePath + " and remove adapter.properties from resourceCfg.ini. Only quote.properties is loaded.");
        }
        loadFile(configuration, databasePath);
        log.accept(serviceName + " configuration: workingDirectory=" + Path.of("").toAbsolutePath().normalize());
        log.accept(serviceName + " configuration: quote.properties=" + databasePath);
        String baseMode = configuration.getProperty("base.mode", "simulator");
        String loadMode = "stc".equals(baseMode) ? "stc-client-on-demand" : "simulator";
        // Do not dump properties: the same files can contain database and service credentials.
        log.accept(serviceName + " adapters: identity.mode=" + configuration.getProperty("identity.mode", "simulator")
                + ", base.mode=" + baseMode + ", base.loadMode=" + loadMode
                + ", crm.mode=" + configuration.getProperty("crm.mode", "simulator"));
        var source = StcBaseDataClient.class.getProtectionDomain().getCodeSource();
        log.accept(serviceName + " base adapter: diagnostics=stc-base-config-v2, loadedFrom="
                + (source == null ? "<unavailable>" : source.getLocation()));
        configuration.setProperty("diagnostics.serviceName", serviceName);
        return configuration;
    }

    private static void loadFile(Properties configuration, Path path) throws IOException {
        try (var reader = Files.newBufferedReader(path)) {
            configuration.load(reader);
        }
    }
}
