package sun.misc;

/**
 * Binary-compatibility shim for the ATS foundation on JDK 9+.
 *
 * The JDK-internal decoder was removed in JDK 9; ats.app.ym.utils.encrypt.gx.TripleDes
 * (ats-app-ym-foundation) still calls
 * {@code new sun.misc.BASE64Decoder().decodeBuffer(String)}.
 *
 * Compiled with a JDK 8 javac and injected with
 * {@code --patch-module jdk.unsupported=<dir>}; see build-shim.ps1 and
 * docs/stc-ceda-jdk17.zh-CN.md.
 */
public class BASE64Decoder {
    public byte[] decodeBuffer(String text) throws java.io.IOException {
        return java.util.Base64.getDecoder().decode(text.trim());
    }

    public byte[] decodeBuffer(java.io.InputStream in) throws java.io.IOException {
        return decodeBuffer(readAll(in));
    }

    private static String readAll(java.io.InputStream in) throws java.io.IOException {
        java.io.ByteArrayOutputStream buffer = new java.io.ByteArrayOutputStream();
        byte[] chunk = new byte[4096];
        int read;
        while ((read = in.read(chunk)) > 0) {
            buffer.write(chunk, 0, read);
        }
        return new String(buffer.toByteArray(), "ISO-8859-1");
    }
}
