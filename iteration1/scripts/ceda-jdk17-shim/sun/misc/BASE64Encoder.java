package sun.misc;

/**
 * Binary-compatibility shim for the STC/CEDA stack on JDK 9+.
 *
 * The JDK-internal encoder was removed in JDK 9; com.adaptiveMQ2 (ceda-api) still
 * calls {@code new sun.misc.BASE64Encoder().encode(byte[])} during client start,
 * which fails with NoClassDefFoundError on JDK 17 and prevents the CEDA client
 * from ever connecting. Only encode(byte[]) is used by that stack.
 *
 * Compiled with a JDK 8 javac and injected with
 * {@code --patch-module jdk.unsupported=<dir>}; see build-shim.ps1 and
 * docs/stc-ceda-jdk17.zh-CN.md.
 */
public class BASE64Encoder {
    public String encode(byte[] bytes) {
        return java.util.Base64.getEncoder().encodeToString(bytes);
    }

    public String encodeBuffer(byte[] bytes) {
        return encode(bytes);
    }

    public void encodeBuffer(byte[] bytes, java.io.OutputStream out) throws java.io.IOException {
        out.write(encode(bytes).getBytes("ISO-8859-1"));
    }

    public void encode(byte[] bytes, java.io.OutputStream out) throws java.io.IOException {
        encodeBuffer(bytes, out);
    }
}
