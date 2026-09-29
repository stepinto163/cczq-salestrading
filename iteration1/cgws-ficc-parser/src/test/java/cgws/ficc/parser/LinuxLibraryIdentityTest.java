package cgws.ficc.parser;

import cgws.ficc.parser.compare.NativeArtifact;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.nio.file.Files;
import java.nio.file.Path;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;

class LinuxLibraryIdentityTest {
    @TempDir
    Path directory;

    @Test
    void explicitLibraryFileMustAlsoMatchWorkingDirectory() throws Exception {
        Path library = Files.write(directory.resolve("libparser.so"), new byte[] {1, 2, 3});
        Path differentDirectory = Files.createDirectory(directory.resolve("other"));
        String originalPlatform = System.getProperty("os.name");
        try {
            System.setProperty("os.name", "Linux");
            assertThrows(IllegalArgumentException.class,
                    () -> NativeArtifact.resolve(library.toString(), differentDirectory.toString()));
            assertEquals(library.toFile().getCanonicalFile(), NativeArtifact
                    .resolve(library.toString(), directory.toString()).loadedFile.getCanonicalFile());
        } finally {
            System.setProperty("os.name", originalPlatform);
        }
    }
}
