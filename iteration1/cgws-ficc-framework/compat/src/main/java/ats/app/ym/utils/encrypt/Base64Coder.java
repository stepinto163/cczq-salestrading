// Recovered compatibility source; provenance: recovered-support.json.
package ats.app.ym.utils.encrypt;

public class Base64Coder {
    private static final String systemLineSeparator = "\n";
    private static char[] map1 = new char[64];
    private static byte[] map2;

    static {
        int i = 0;
        int c = 65;
        while (c <= 90) {
            Base64Coder.map1[i++] = (char) c;
            c = (char) (c + 1);
        }
        c = 97;
        while (c <= 122) {
            Base64Coder.map1[i++] = (char) c;
            c = (char) (c + 1);
        }
        c = 48;
        while (c <= 57) {
            Base64Coder.map1[i++] = (char) c;
            c = (char) (c + 1);
        }
        Base64Coder.map1[i++] = 43;
        Base64Coder.map1[i++] = 47;
        map2 = new byte[128];
        i = 0;
        while (i < map2.length) {
            Base64Coder.map2[i] = -1;
            ++i;
        }
        i = 0;
        while (i < 64) {
            Base64Coder.map2[Base64Coder.map1[i]] = (byte) i;
            ++i;
        }
    }

    public static String encodeString(String s) {
        return new String(Base64Coder.encode(s.getBytes()));
    }

    public static String encodeLines(byte[] in) {
        return Base64Coder.encodeLines(in, 0, in.length, 76, systemLineSeparator);
    }

    public static String encodeLines(
            byte[] in, int iOff, int iLen, int lineLen, String lineSeparator) {
        int blockLen = lineLen * 3 / 4;
        if (blockLen <= 0) {
            throw new IllegalArgumentException();
        }
        int lines = (iLen + blockLen - 1) / blockLen;
        int bufLen = (iLen + 2) / 3 * 4 + lines * lineSeparator.length();
        StringBuilder buf = new StringBuilder(bufLen);
        int ip = 0;
        while (ip < iLen) {
            int l = Math.min(iLen - ip, blockLen);
            buf.append(Base64Coder.encode(in, iOff + ip, l));
            buf.append(lineSeparator);
            ip += l;
        }
        return buf.toString();
    }

    public static char[] encode(byte[] in) {
        return Base64Coder.encode(in, 0, in.length);
    }

    public static char[] encode(byte[] in, int iLen) {
        return Base64Coder.encode(in, 0, iLen);
    }

    public static char[] encode(byte[] in, int iOff, int iLen) {
        int oDataLen = (iLen * 4 + 2) / 3;
        int oLen = (iLen + 2) / 3 * 4;
        char[] out = new char[oLen];
        int ip = iOff;
        int iEnd = iOff + iLen;
        int op = 0;
        while (ip < iEnd) {
            int i0 = in[ip++] & 0xFF;
            int i1 = ip < iEnd ? in[ip++] & 0xFF : 0;
            int i2 = ip < iEnd ? in[ip++] & 0xFF : 0;
            int o0 = i0 >>> 2;
            int o1 = (i0 & 3) << 4 | i1 >>> 4;
            int o2 = (i1 & 0xF) << 2 | i2 >>> 6;
            int o3 = i2 & 0x3F;
            out[op++] = map1[o0];
            out[op++] = map1[o1];
            out[op] = op < oDataLen ? map1[o2] : 61;
            int n = ++op < oDataLen ? map1[o3] : 61;
            out[op] = (char) n;
            ++op;
        }
        return out;
    }

    public static String decodeString(String s) {
        return new String(Base64Coder.decode(s));
    }

    public static byte[] decodeLines(String s) {
        char[] buf = new char[s.length()];
        int p = 0;
        int ip = 0;
        while (ip < s.length()) {
            char c = s.charAt(ip);
            if (c != ' ' && c != '\r' && c != '\n' && c != '\t') {
                buf[p++] = c;
            }
            ++ip;
        }
        return Base64Coder.decode(buf, 0, p);
    }

    public static byte[] decode(String s) {
        return Base64Coder.decode(s.toCharArray());
    }

    public static byte[] decode(char[] in) {
        return Base64Coder.decode(in, 0, in.length);
    }

    public static byte[] decode(char[] in, int iOff, int iLen) {
        if (iLen % 4 != 0) {
            throw new IllegalArgumentException(
                    "Length of Base64 encoded input string is not a multiple of 4.");
        }
        while (iLen > 0 && in[iOff + iLen - 1] == '=') {
            --iLen;
        }
        int outputLength = iLen * 3 / 4;
        byte[] output = new byte[outputLength];
        int inputPosition = iOff;
        int inputEnd = iOff + iLen;
        int outputPosition = 0;
        while (inputPosition < inputEnd) {
            int first = in[inputPosition++];
            int second = in[inputPosition++];
            int third = inputPosition < inputEnd ? in[inputPosition++] : 65;
            int fourth = inputPosition < inputEnd ? in[inputPosition++] : 65;
            if (first > 127
                    || second > 127
                    || third > 127
                    || fourth > 127
                    || map2[first] < 0
                    || map2[second] < 0
                    || map2[third] < 0
                    || map2[fourth] < 0) {
                throw new IllegalArgumentException("Illegal character in Base64 encoded data.");
            }
            int b0 = map2[first];
            int b1 = map2[second];
            int b2 = map2[third];
            int b3 = map2[fourth];
            output[outputPosition++] = (byte) (b0 << 2 | b1 >>> 4);
            if (outputPosition < outputLength) {
                output[outputPosition++] = (byte) ((b1 & 15) << 4 | b2 >>> 2);
            }
            if (outputPosition < outputLength) {
                output[outputPosition++] = (byte) ((b2 & 3) << 6 | b3);
            }
        }
        return output;
    }

    private Base64Coder() {}
}
