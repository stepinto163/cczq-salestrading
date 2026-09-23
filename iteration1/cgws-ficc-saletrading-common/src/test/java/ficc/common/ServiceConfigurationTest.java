package ficc.common;

import static org.junit.jupiter.api.Assertions.*;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Properties;

final class ServiceConfigurationTest {
    @TempDir Path directory;

    @Test
    void sharedDatabaseFileResolvesUnderAtsRootAndStillUsesNormalLoader() throws Exception {
        Path control = Files.createDirectories(directory.resolve("control"));
        Path database = control.resolve("ficc-database.properties");
        Files.write(database, "db.schema=ficc_salestrading2\n".getBytes(StandardCharsets.UTF_8));
        Path resolved =
                ServiceConfiguration.resolvePath(
                        "$ATS_ROOT/control/ficc-database.properties", directory.toString());
        assertEquals(database.toAbsolutePath(), resolved);
        Properties configuration =
                ServiceConfiguration.load("QuoteSvr", resolved.toString(), "", message -> {});
        assertEquals("ficc_salestrading2", configuration.getProperty("db.schema"));
    }

    @Test
    void missingAtsRootFailsInsteadOfReadingAnUnrelatedLocalFile() {
        for (String atsRoot : Arrays.asList(null, "", " ")) {
            java.io.IOException error =
                    assertThrows(
                            java.io.IOException.class,
                            () ->
                                    ServiceConfiguration.resolvePath(
                                            "$ATS_ROOT/control/ficc-database.properties", atsRoot));
            assertTrue(error.getMessage().contains("ATS_ROOT is required"));
        }
    }

    @Test
    void localConfigurationPathsDoNotRequireAtsRoot() throws Exception {
        assertEquals(
                java.nio.file.Paths.get("config/database.properties").toAbsolutePath().normalize(),
                ServiceConfiguration.resolvePath("./config/database.properties", null));
    }

    @Test
    void blankAdapterValueOverridesInheritedSubsetAndReportsActualSourcesWithoutSecrets()
            throws Exception {
        Path database = directory.resolve("database.properties");
        Path adapter = directory.resolve("adapters.properties");
        Files.write(
                database,
                "db.password=PRIVATE-DB-PASSWORD\nbase.securityIds=250001.IB\n"
                        .getBytes(StandardCharsets.UTF_8));
        Files.write(
                adapter,
                "identity.mode=stc\nbase.mode=stc\nbase.securityIds=\nidentity.password=PRIVATE-LOGIN\n"
                        .getBytes(StandardCharsets.UTF_8));
        List<String> logs = new ArrayList<>();
        Properties configuration =
                ServiceConfiguration.load(
                        "QuoteSvr", database.toString(), adapter.toString(), logs::add);
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
        Files.write(
                database,
                "db.user=quote_user\nidentity.mode=simulator\ncrm.mode=postgresql\n"
                        .getBytes(StandardCharsets.UTF_8));
        Files.write(
                adapter, "identity.mode=stc\ncrm.mode=empty\n".getBytes(StandardCharsets.UTF_8));
        Properties configuration =
                ServiceConfiguration.load(
                        "QuoteSvr", database.toString(), adapter.toString(), message -> {});
        assertEquals("quote_user", configuration.getProperty("db.user"));
        assertEquals("stc", configuration.getProperty("identity.mode"));
        assertEquals("empty", configuration.getProperty("crm.mode"));
        assertThrows(
                java.io.IOException.class,
                () ->
                        ServiceConfiguration.load(
                                "QuoteSvr",
                                database.toString(),
                                directory.resolve("missing.properties").toString(),
                                message -> {}));
    }

    @Test
    void absentAdapterOrCommentedSettingKeepsInheritedSubsetVisible() throws Exception {
        Path database = directory.resolve("database.properties");
        Path adapter = directory.resolve("adapters.properties");
        Files.write(
                database,
                "base.mode=stc\nbase.securityIds=250001.IB\n".getBytes(StandardCharsets.UTF_8));
        Files.write(adapter, "# base.securityIds=\n".getBytes(StandardCharsets.UTF_8));
        for (String adapterFile : Arrays.asList("", adapter.toString())) {
            List<String> logs = new ArrayList<>();
            Properties configuration =
                    ServiceConfiguration.load(
                            "QuoteSvr", database.toString(), adapterFile, logs::add);
            assertEquals("250001.IB", configuration.getProperty("base.securityIds"));
            assertTrue(
                    String.join("\n", logs)
                            .contains(
                                    "base.loadMode=stc-client-on-demand, crm.mode=simulator,"
                                            + " securityIdsCount=1"));
        }
    }
}
