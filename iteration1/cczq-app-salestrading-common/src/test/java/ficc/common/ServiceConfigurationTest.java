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
        assertTrue(output.contains("base.loadMode=all"));
        assertTrue(output.contains("securityIdsCount=0"));
        assertTrue(output.contains(adapter.toAbsolutePath().toString()));
        assertTrue(output.contains("diagnostics=stc-base-config-v2"));
        assertTrue(output.contains("loadedFrom="));
        assertFalse(output.contains("PRIVATE-DB-PASSWORD"));
        assertFalse(output.contains("PRIVATE-LOGIN"));
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
            assertTrue(String.join("\n", logs).contains("base.loadMode=subset, securityIdsCount=1"));
        }
    }
}
