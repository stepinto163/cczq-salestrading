package cgws.ficc.parser.compare;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.security.MessageDigest;

/**
 * Fingerprint of everything a baseline depends on: the inputs, the platform and the actual bytes
 * of the legacy artifacts. A baseline is only usable when this fingerprint matches byte for byte.
 */
public final class Fingerprint {

    public static final String SCHEMA = "parser-baseline/2";

    private Fingerprint() {
    }

    /**
     * Input fingerprint: the canonical serialization of the whole input set - suite id and
     * declaration, every dictionary field (code, name, key, invalid) and every case field
     * (id, text, business date, capability, expectation source). No clock, no paths.
     */
    public static String ofInputs(CaseSet set) {
        return sha256(Json.write(set.canonical()));
    }

    public static String ofText(String text) {
        return sha256(text == null ? "" : text);
    }

    public static String platform() {
        return System.getProperty("os.name") + "/" + System.getProperty("os.arch");
    }

    /** SHA-256 of a file; null when the file does not exist. */
    public static String sha256(File file) {
        if (file == null || !file.isFile()) {
            return null;
        }
        MessageDigest digest = newDigest();
        byte[] buffer = new byte[65536];
        FileInputStream in = null;
        try {
            in = new FileInputStream(file);
            int read;
            while ((read = in.read(buffer)) > 0) {
                digest.update(buffer, 0, read);
            }
        } catch (IOException e) {
            throw new IllegalStateException("cannot hash " + file + ": " + e.getMessage(), e);
        } finally {
            close(in);
        }
        return hex(digest.digest());
    }

    public static String sha256(String text) {
        return hex(newDigest().digest(bytes(text)));
    }

    public static String hex(byte[] data) {
        StringBuilder b = new StringBuilder(data.length * 2);
        for (byte one : data) {
            String hex = Integer.toHexString(one & 0xFF);
            if (hex.length() == 1) {
                b.append('0');
            }
            b.append(hex);
        }
        return b.toString();
    }

    private static byte[] bytes(String text) {
        try {
            return text.getBytes("UTF-8");
        } catch (IOException e) {
            throw new IllegalStateException(e);
        }
    }

    private static MessageDigest newDigest() {
        try {
            return MessageDigest.getInstance("SHA-256");
        } catch (java.security.NoSuchAlgorithmException e) {
            throw new IllegalStateException(e);
        }
    }

    private static void close(FileInputStream in) {
        if (in != null) {
            try {
                in.close();
            } catch (IOException ignored) {
                // nothing useful to do while hashing
            }
        }
    }
}
