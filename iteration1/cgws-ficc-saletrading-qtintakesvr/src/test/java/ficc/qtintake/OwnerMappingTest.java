package ficc.qtintake;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.attribute.FileTime;
import java.util.Properties;

class OwnerMappingTest {
    @TempDir Path directory;

    private static Properties configuration(String... pairs) {
        Properties configuration = new Properties();
        for (int index = 0; index + 1 < pairs.length; index += 2) {
            configuration.setProperty(pairs[index], pairs[index + 1]);
        }
        return configuration;
    }

    private Path mapping(String content) throws Exception {
        Path file = directory.resolve("owner-map.properties");
        Files.write(file, content.getBytes(StandardCharsets.UTF_8));
        return file;
    }

    @Test
    void resolvesMappedAccount() throws Exception {
        Path file = mapping("QT-1=STC-1\nQT-2 = STC-2 \n");
        OwnerMapping owners = OwnerMapping.load(configuration("owner.mapFile", file.toString()));
        assertEquals("STC-1", owners.resolve("QT-1"));
        assertEquals("STC-2", owners.resolve(" QT-2 "));
    }

    @Test
    void unmappedAccountFallsBackToDefault() throws Exception {
        Path file = mapping("QT-1=STC-1\n");
        OwnerMapping owners =
                OwnerMapping.load(
                        configuration(
                                "owner.mapFile",
                                file.toString(),
                                "owner.defaultUserId",
                                "STC-DEFAULT"));
        assertEquals("STC-DEFAULT", owners.resolve("QT-9"));
    }

    @Test
    void unmappedAccountWithoutDefaultIsRejected() throws Exception {
        Path file = mapping("QT-1=STC-1\n");
        OwnerMapping owners = OwnerMapping.load(configuration("owner.mapFile", file.toString()));
        assertThrows(IllegalStateException.class, () -> owners.resolve("QT-9"));
    }

    @Test
    void defaultAloneIsEnough() throws Exception {
        OwnerMapping owners =
                OwnerMapping.load(configuration("owner.defaultUserId", "STC-DEFAULT"));
        assertEquals("STC-DEFAULT", owners.resolve("QT-9"));
    }

    @Test
    void noConfigurationFailsAtStartup() {
        assertThrows(IllegalStateException.class, () -> OwnerMapping.load(configuration()));
    }

    @Test
    void missingFileFailsAtStartup() {
        Path file = directory.resolve("absent.properties");
        assertThrows(
                IllegalStateException.class,
                () -> OwnerMapping.load(configuration("owner.mapFile", file.toString())));
    }

    @Test
    void duplicateAccountIsRejected() throws Exception {
        Path file = mapping("QT-1=STC-1\nQT-1=STC-2\n");
        assertThrows(
                IllegalStateException.class,
                () -> OwnerMapping.load(configuration("owner.mapFile", file.toString())));
    }

    @Test
    void reloadsAfterTheFileChanges() throws Exception {
        Path file = mapping("QT-1=STC-1\n");
        OwnerMapping owners = OwnerMapping.load(configuration("owner.mapFile", file.toString()));
        assertEquals("STC-1", owners.resolve("QT-1"));
        Files.write(file, "QT-1=STC-1\nQT-3=STC-3\n".getBytes(StandardCharsets.UTF_8));
        Files.setLastModifiedTime(file, FileTime.fromMillis(System.currentTimeMillis() + 2000));
        assertEquals("STC-3", owners.resolve("QT-3"));
    }
}
