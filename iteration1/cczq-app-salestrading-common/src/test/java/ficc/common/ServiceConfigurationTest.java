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
    void blankAdapterValueOverridesInheritedSubsetAndReportsActualSourcesWithoutSecrets() throws Exception {
        Path database = directory.resolve("database.properties");
        Path adapter = directory.resolve("adapters.properties");
        Files.writeString(database, "db.password=PRIVATE-DB-PASSWORD\nbase.securityIds=250001.IB\n");
        Files.writeString(adapter, "identity.mode=stc\nbase.mode=stc\nbase.securityIds=\nidentity.password=PRIVATE-LOGIN\n");
        List<String> logs = new ArrayList<>();
        Properties configuration = ServiceConfiguration.load("QuoteSvr", database.toString(), adapter.toString(), logs::add);
        assertEquals("", configuration.getProperty("base.securityIds"));
        String output = String.join("\n", logs);
        assertTrue(output.contains("base.loadMode=stc-client-on-demand"));
        assertTrue(output.contains("securityIdsCount=0"));
        assertTrue(output.contains(adapter.toAbsolutePath().toString()));
        assertTrue(output.contains("diagnostics=stc-base-config-v2"));
        assertTrue(output.contains("loadedFrom="));
        assertFalse(output.contains("PRIVATE-DB-PASSWORD"));
        assertFalse(output.contains("PRIVATE-LOGIN"));
    }

    @Test
    void adapterOverridesDuplicateKeysAndMissingConfiguredAdapterFails() throws Exception {
        Path database = directory.resolve("database.properties");
        Path adapter = directory.resolve("adapters.properties");
        Files.writeString(database, "db.user=quote_user\nidentity.mode=simulator\ncrm.mode=postgresql\n");
        Files.writeString(adapter, "identity.mode=stc\ncrm.mode=empty\n");
        Properties configuration = ServiceConfiguration.load("QuoteSvr", database.toString(), adapter.toString(), message -> {});
        assertEquals("quote_user", configuration.getProperty("db.user"));
        assertEquals("stc", configuration.getProperty("identity.mode"));
        assertEquals("empty", configuration.getProperty("crm.mode"));
        assertThrows(java.io.IOException.class, () -> ServiceConfiguration.load("QuoteSvr",
                database.toString(), directory.resolve("missing.properties").toString(), message -> {}));
    }

    @Test
    void absentAdapterOrCommentedSettingKeepsInheritedSubsetVisible() throws Exception {
        Path database = directory.resolve("database.properties");
        Path adapter = directory.resolve("adapters.properties");
        Files.writeString(database, "base.mode=stc\nbase.securityIds=250001.IB\n");
        Files.writeString(adapter, "# base.securityIds=\n");
        for (String adapterFile : List.of("", adapter.toString())) {
            List<String> logs = new ArrayList<>();
            Properties configuration = ServiceConfiguration.load("QuoteSvr", database.toString(), adapterFile, logs::add);
            assertEquals("250001.IB", configuration.getProperty("base.securityIds"));
            assertTrue(String.join("\n", logs).contains("base.loadMode=stc-client-on-demand, crm.mode=simulator, securityIdsCount=1"));
        }
    }
}
