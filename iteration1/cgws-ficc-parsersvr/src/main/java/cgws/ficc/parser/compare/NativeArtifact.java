package cgws.ficc.parser.compare;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

/**
 * The native library file the legacy wrapper really loads, together with its identity.
 *
 * <p>The recorded hash must belong to the file that is actually loaded, not to whichever file the
 * caller happened to pass:
 * <ul>
 *   <li>Windows: the wrapper resolves {@code ParseLib.dll} through {@code java.library.path}, so
 *       the directory that is passed to the child JVM decides which file is loaded.</li>
 *   <li>Linux: the wrapper loads {@code <working directory>/libparser.so}, so {@code --native-lib}
 *       and {@code --work-dir} must point at the same directory; otherwise the run is refused.</li>
 * </ul>
 * A directory that holds more than one candidate for the current platform is refused as ambiguous.
 */
public final class NativeArtifact {

    /** Path as requested by the caller (file or directory), absolute. */
    public final File requested;
    /** Directory handed to {@code java.library.path}. */
    public final File loadDirectory;
    /** The single file that will be loaded. */
    public final File loadedFile;
    public final String sha256;

    private NativeArtifact(File requested, File loadDirectory, File loadedFile, String sha256) {
        this.requested = requested;
        this.loadDirectory = loadDirectory;
        this.loadedFile = loadedFile;
        this.sha256 = sha256;
    }

    public static boolean isWindows() {
        return System.getProperty("os.name", "").toLowerCase().contains("win");
    }

    /** File name the legacy wrapper looks for on this platform. */
    public static String expectedLibraryName() {
        return isWindows() ? "ParseLib.dll" : "libparser.so";
    }

    /** Resolves the artifact or fails; never returns an artifact without a hash. */
    public static NativeArtifact resolve(String nativePath, String workDir) {
        if (nativePath == null) {
            return null;
        }
        File requested = new File(nativePath).getAbsoluteFile();
        if (!requested.exists()) {
            throw new IllegalArgumentException("native library path does not exist: " + requested);
        }
        File workDirectory = new File(workDir == null ? "." : workDir).getAbsoluteFile();
        String expectedName = expectedLibraryName();
        File loadDirectory;
        File loadedFile;
        if (requested.isFile()) {
            if (!requested.getName().equals(expectedName)) {
                throw new IllegalArgumentException("native library file " + requested.getName()
                        + " is not what the legacy wrapper loads on this platform (" + expectedName + ")");
            }
            loadedFile = requested;
            loadDirectory = requested.getParentFile();
        } else {
            File candidate = new File(requested, expectedName);
            if (!candidate.isFile()) {
                throw new IllegalArgumentException("no " + expectedName + " under " + requested
                        + " (entries: " + entries(requested) + ")");
            }
            requireUnambiguous(requested, expectedName);
            loadedFile = candidate;
            loadDirectory = requested;
        }
        if (!isWindows() && !samePath(loadDirectory, workDirectory)) {
            throw new IllegalArgumentException("Linux wrapper loads libparser.so from --work-dir; "
                    + "the requested native library must be in that directory");
        }
        String hash = Fingerprint.sha256(loadedFile);
        if (hash == null) {
            throw new IllegalArgumentException("cannot read native library " + loadedFile);
        }
        return new NativeArtifact(requested, loadDirectory, loadedFile, hash);
    }

    private static void requireUnambiguous(File directory, String expectedName) {
        List<String> present = new ArrayList<String>();
        for (String name : candidateNames()) {
            if (new File(directory, name).isFile()) {
                present.add(name);
            }
        }
        if (present.size() > 1) {
            throw new IllegalArgumentException("native library directory " + directory
                    + " holds several candidates for this platform " + present
                    + "; pass the file to load explicitly");
        }
        if (!present.contains(expectedName)) {
            throw new IllegalArgumentException("no " + expectedName + " under " + directory
                    + " (entries: " + entries(directory) + ")");
        }
    }

    private static String[] candidateNames() {
        if (isWindows()) {
            return new String[]{"ParseLib.dll"};
        }
        return new String[]{"libparser.so", "libParseLib.so"};
    }

    private static String entries(File directory) {
        String[] names = directory.list();
        return names == null ? "<unreadable>" : java.util.Arrays.toString(names);
    }

    private static boolean samePath(File left, File right) {
        try {
            return left.getCanonicalFile().equals(right.getCanonicalFile());
        } catch (java.io.IOException e) {
            return left.getAbsoluteFile().equals(right.getAbsoluteFile());
        }
    }
}
