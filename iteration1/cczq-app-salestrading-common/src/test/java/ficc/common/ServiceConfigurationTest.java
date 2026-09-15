package ficc.common;

import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;
import static org.junit.jupiter.api.Assertions.*;

final class ServiceConfigurationTest {
    @TempDir
    Path directory;

    @Test
    void oneFileContainsDatabaseAndAllAdapterSettingsWithoutLoggingSecrets() throws Exception {
        Path database = directory.resolve("database.properties");
        Path adapter = directory.resolve("adapters.properties");
        Files.writeString(database, "db.password=PRIVATE-DB-PASSWORD\nidentity.mode=stc\nbase.mode=stc\ncrm.mode=postgresql\ncrm.password=PRIVATE-CRM\n");
        Files.writeString(adapter, "crm.mode=empty\n");
        List<String> logs = new ArrayList<>();
        Properties configuration = ServiceConfiguration.load("QuoteSvr", database.toString(), "", logs::add);
        assertEquals("postgresql", configuration.getProperty("crm.mode"));
        assertEquals("stc", configuration.getProperty("identity.mode"));
        String output = String.join("\n", logs);
        assertTrue(output.contains("base.loadMode=stc-client-on-demand"));
        assertTrue(output.contains("crm.mode=postgresql"));
        assertTrue(output.contains(database.toAbsolutePath().toString()));
        assertFalse(output.contains(adapter.toAbsolutePath().toString()));
        assertTrue(output.contains("diagnostics=stc-base-config-v2"));
        assertTrue(output.contains("loadedFrom="));
        assertFalse(output.contains("PRIVATE-DB-PASSWORD"));
        assertFalse(output.contains("PRIVATE-CRM"));
    }

    @Test
    void oldOverrideIsRejectedWithAnActionableErrorAndMissingFileThrows() throws Exception {
        Path database = directory.resolve("database.properties");
        Path adapter = directory.resolve("adapters.properties");
        Files.writeString(database, "base.mode=stc\nbase.securityIds=250001.IB\n");
        Files.writeString(adapter, "# base.securityIds=\n");
        var failure = assertThrows(java.io.IOException.class, () ->
                ServiceConfiguration.load("QuoteSvr", database.toString(), adapter.toString(), message -> {}));
        assertTrue(failure.getMessage().contains("adapter.properties is no longer supported"));
        assertTrue(failure.getMessage().contains(database.toAbsolutePath().toString()));
        assertThrows(java.io.IOException.class, () -> ServiceConfiguration.load("QuoteSvr",
                directory.resolve("missing.properties").toString(), "", message -> {}));
    }
}
