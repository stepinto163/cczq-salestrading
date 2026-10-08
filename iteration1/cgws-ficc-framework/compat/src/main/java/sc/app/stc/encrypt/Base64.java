// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.encrypt;

import java.io.UnsupportedEncodingException;

public class Base64 {
    static final int CHUNK_SIZE = 76;
    static final byte[] CHUNK_SEPARATOR;
    static final int BASELENGTH = 255;
    static final int LOOKUPLENGTH = 64;
    static final int EIGHTBIT = 8;
    static final int SIXTEENBIT = 16;
    static final int TWENTYFOURBITGROUP = 24;
    static final int FOURBYTE = 4;
    static final int SIGN = -128;
    static final byte PAD = 61;
    private static byte[] base64Alphabet;
    private static byte[] lookUpBase64Alphabet;

    private static boolean isBase64(byte octect) {
        if (octect == 61) {
            return true;
        }
        return base64Alphabet[octect] != -1;
    }

    public static boolean isArrayByteBase64(byte[] arrayOctect) {
        int length = (arrayOctect = Base64.discardWhitespace(arrayOctect)).length;
        if (length == 0) {
            return true;
        }
        for (int i = 0; i < length; ++i) {
            if (Base64.isBase64(arrayOctect[i])) continue;
            return false;
        }
        return true;
    }

    public static byte[] encodeBase64(byte[] binaryData) {
        return Base64.encodeBase64(binaryData, false);
    }

    public static byte[] encodeBase64Chunked(byte[] binaryData) {
        return Base64.encodeBase64(binaryData, true);
    }

    public static byte[] decode(byte[] pArray) {
        return Base64.decodeBase64(pArray);
    }

    public static byte[] encodeBase64(byte[] binaryData, boolean isChunked) {
        byte val2;
        byte val1;
        int lengthDataBits = binaryData.length * 8;
        int fewerThan24bits = lengthDataBits % 24;
        int numberTriplets = lengthDataBits / 24;
        byte[] encodedData = null;
        int encodedDataLength = 0;
        int nbrChunks = 0;
        encodedDataLength = fewerThan24bits != 0 ? (numberTriplets + 1) * 4 : numberTriplets * 4;
        if (isChunked) {
            nbrChunks =
                    CHUNK_SEPARATOR.length == 0
                            ? 0
                            : (int) Math.ceil((float) encodedDataLength / 76.0f);
            encodedDataLength += nbrChunks * CHUNK_SEPARATOR.length;
        }
        encodedData = new byte[encodedDataLength];
        byte k = 0;
        byte l = 0;
        byte b1 = 0;
        byte b2 = 0;
        byte b3 = 0;
        int encodedIndex = 0;
        int dataIndex = 0;
        int i = 0;
        int nextSeparatorIndex = 76;
        int chunksSoFar = 0;
        for (i = 0; i < numberTriplets; ++i) {
            dataIndex = i * 3;
            b1 = binaryData[dataIndex];
            b2 = binaryData[dataIndex + 1];
            b3 = binaryData[dataIndex + 2];
            l = (byte) (b2 & 0xF);
            k = (byte) (b1 & 3);
            val1 = (b1 & 0xFFFFFF80) == 0 ? (byte) (b1 >> 2) : (byte) (b1 >> 2 ^ 0xC0);
            val2 = (b2 & 0xFFFFFF80) == 0 ? (byte) (b2 >> 4) : (byte) (b2 >> 4 ^ 0xF0);
            byte val3 = (b3 & 0xFFFFFF80) == 0 ? (byte) (b3 >> 6) : (byte) (b3 >> 6 ^ 0xFC);
            encodedData[encodedIndex] = lookUpBase64Alphabet[val1];
            encodedData[encodedIndex + 1] = lookUpBase64Alphabet[val2 | k << 4];
            encodedData[encodedIndex + 2] = lookUpBase64Alphabet[l << 2 | val3];
            encodedData[encodedIndex + 3] = lookUpBase64Alphabet[b3 & 0x3F];
            if (!isChunked || (encodedIndex += 4) != nextSeparatorIndex) continue;
            System.arraycopy(CHUNK_SEPARATOR, 0, encodedData, encodedIndex, CHUNK_SEPARATOR.length);
            nextSeparatorIndex = 76 * (++chunksSoFar + 1) + chunksSoFar * CHUNK_SEPARATOR.length;
            encodedIndex += CHUNK_SEPARATOR.length;
        }
        dataIndex = i * 3;
        if (fewerThan24bits == 8) {
            b1 = binaryData[dataIndex];
            k = (byte) (b1 & 3);
            val1 = (b1 & 0xFFFFFF80) == 0 ? (byte) (b1 >> 2) : (byte) (b1 >> 2 ^ 0xC0);
            encodedData[encodedIndex] = lookUpBase64Alphabet[val1];
            encodedData[encodedIndex + 1] = lookUpBase64Alphabet[k << 4];
            encodedData[encodedIndex + 2] = 61;
            encodedData[encodedIndex + 3] = 61;
        } else if (fewerThan24bits == 16) {
            b1 = binaryData[dataIndex];
            b2 = binaryData[dataIndex + 1];
            l = (byte) (b2 & 0xF);
            k = (byte) (b1 & 3);
            val1 = (b1 & 0xFFFFFF80) == 0 ? (byte) (b1 >> 2) : (byte) (b1 >> 2 ^ 0xC0);
            val2 = (b2 & 0xFFFFFF80) == 0 ? (byte) (b2 >> 4) : (byte) (b2 >> 4 ^ 0xF0);
            encodedData[encodedIndex] = lookUpBase64Alphabet[val1];
            encodedData[encodedIndex + 1] = lookUpBase64Alphabet[val2 | k << 4];
            encodedData[encodedIndex + 2] = lookUpBase64Alphabet[l << 2];
            encodedData[encodedIndex + 3] = 61;
        }
        if (isChunked && chunksSoFar < nbrChunks) {
            System.arraycopy(
                    CHUNK_SEPARATOR,
                    0,
                    encodedData,
                    encodedDataLength - CHUNK_SEPARATOR.length,
                    CHUNK_SEPARATOR.length);
        }
        return encodedData;
    }

    public static byte[] decodeBase64(byte[] base64Data) {
        if ((base64Data = Base64.discardNonBase64(base64Data)).length == 0) {
            return new byte[0];
        }
        int numberQuadruple = base64Data.length / 4;
        byte[] decodedData = null;
        byte b1 = 0;
        byte b2 = 0;
        byte b3 = 0;
        byte b4 = 0;
        byte marker0 = 0;
        byte marker1 = 0;
        int encodedIndex = 0;
        int dataIndex = 0;
        int lastData = base64Data.length;
        while (base64Data[lastData - 1] == 61) {
            if (--lastData != 0) continue;
            return new byte[0];
        }
        decodedData = new byte[lastData - numberQuadruple];
        for (int i = 0; i < numberQuadruple; ++i) {
            dataIndex = i * 4;
            marker0 = base64Data[dataIndex + 2];
            marker1 = base64Data[dataIndex + 3];
            b1 = base64Alphabet[base64Data[dataIndex]];
            b2 = base64Alphabet[base64Data[dataIndex + 1]];
            if (marker0 != 61 && marker1 != 61) {
                b3 = base64Alphabet[marker0];
                b4 = base64Alphabet[marker1];
                decodedData[encodedIndex] = (byte) (b1 << 2 | b2 >> 4);
                decodedData[encodedIndex + 1] = (byte) ((b2 & 0xF) << 4 | b3 >> 2 & 0xF);
                decodedData[encodedIndex + 2] = (byte) (b3 << 6 | b4);
            } else if (marker0 == 61) {
                decodedData[encodedIndex] = (byte) (b1 << 2 | b2 >> 4);
            } else if (marker1 == 61) {
                b3 = base64Alphabet[marker0];
                decodedData[encodedIndex] = (byte) (b1 << 2 | b2 >> 4);
                decodedData[encodedIndex + 1] = (byte) ((b2 & 0xF) << 4 | b3 >> 2 & 0xF);
            }
            encodedIndex += 3;
        }
        return decodedData;
    }

    static byte[] discardWhitespace(byte[] data) {
        byte[] groomedData = new byte[data.length];
        int bytesCopied = 0;
        block3:
        for (int i = 0; i < data.length; ++i) {
            switch (data[i]) {
                case 9:
                case 10:
                case 13:
                case 32:
                    {
                        continue block3;
                    }
                default:
                    {
                        groomedData[bytesCopied++] = data[i];
                    }
            }
        }
        byte[] packedData = new byte[bytesCopied];
        System.arraycopy(groomedData, 0, packedData, 0, bytesCopied);
        return packedData;
    }

    static byte[] discardNonBase64(byte[] data) {
        byte[] groomedData = new byte[data.length];
        int bytesCopied = 0;
        for (int i = 0; i < data.length; ++i) {
            if (!Base64.isBase64(data[i])) continue;
            groomedData[bytesCopied++] = data[i];
        }
        byte[] packedData = new byte[bytesCopied];
        System.arraycopy(groomedData, 0, packedData, 0, bytesCopied);
        return packedData;
    }

    public static byte[] encode(byte[] pArray) {
        return Base64.encodeBase64(pArray, false);
    }

    public static String encode(String str) throws UnsupportedEncodingException {
        String baseStr = new String(Base64.encode(str.getBytes("UTF-8")));
        String tempStr = Digest.digest(str).toUpperCase();
        String result = tempStr + baseStr;
        return new String(Base64.encode(result.getBytes("UTF-8")));
    }

    public static String decode(String cryptoStr) throws UnsupportedEncodingException {
        if (cryptoStr.length() < 40) {
            return "";
        }
        try {
            String tempStr = new String(Base64.decode(cryptoStr.getBytes("UTF-8")));
            String result = tempStr.substring(40, tempStr.length());
            return new String(Base64.decode(result.getBytes("UTF-8")));
        } catch (ArrayIndexOutOfBoundsException ex) {
            return "";
        }
    }

    public static byte[] decode2(String encoded) {
        int i;
        if (encoded == null) {
            return null;
        }
        char[] base64Data = encoded.toCharArray();
        int len = Base64.removeWhiteSpace(base64Data);
        if (len % 4 != 0) {
            return null;
        }
        int numberQuadruple = len / 4;
        if (numberQuadruple == 0) {
            return new byte[0];
        }
        byte[] decodedData = null;
        byte b1 = 0;
        byte b2 = 0;
        byte b3 = 0;
        byte b4 = 0;
        char d1 = '\u0000';
        char d2 = '\u0000';
        char d3 = '\u0000';
        char d4 = '\u0000';
        int encodedIndex = 0;
        int dataIndex = 0;
        decodedData = new byte[numberQuadruple * 3];
        for (i = 0; i < numberQuadruple - 1; ++i) {
            if (!(Base64.isData(d1 = base64Data[dataIndex++])
                    && Base64.isData(d2 = base64Data[dataIndex++])
                    && Base64.isData(d3 = base64Data[dataIndex++])
                    && Base64.isData(d4 = base64Data[dataIndex++]))) {
                return null;
            }
            b1 = base64Alphabet[d1];
            b2 = base64Alphabet[d2];
            b3 = base64Alphabet[d3];
            b4 = base64Alphabet[d4];
            decodedData[encodedIndex++] = (byte) (b1 << 2 | b2 >> 4);
            decodedData[encodedIndex++] = (byte) ((b2 & 0xF) << 4 | b3 >> 2 & 0xF);
            decodedData[encodedIndex++] = (byte) (b3 << 6 | b4);
        }
        if (!Base64.isData(d1 = base64Data[dataIndex++])
                || !Base64.isData(d2 = base64Data[dataIndex++])) {
            return null;
        }
        b1 = base64Alphabet[d1];
        b2 = base64Alphabet[d2];
        d3 = base64Data[dataIndex++];
        d4 = base64Data[dataIndex++];
        if (!Base64.isData(d3) || !Base64.isData(d4)) {
            if (Base64.isPad(d3) && Base64.isPad(d4)) {
                if ((b2 & 0xF) != 0) {
                    return null;
                }
                byte[] tmp = new byte[i * 3 + 1];
                System.arraycopy(decodedData, 0, tmp, 0, i * 3);
                tmp[encodedIndex] = (byte) (b1 << 2 | b2 >> 4);
                return tmp;
            }
            if (!Base64.isPad(d3) && Base64.isPad(d4)) {
                b3 = base64Alphabet[d3];
                if ((b3 & 3) != 0) {
                    return null;
                }
                byte[] tmp = new byte[i * 3 + 2];
                System.arraycopy(decodedData, 0, tmp, 0, i * 3);
                tmp[encodedIndex++] = (byte) (b1 << 2 | b2 >> 4);
                tmp[encodedIndex] = (byte) ((b2 & 0xF) << 4 | b3 >> 2 & 0xF);
                return tmp;
            }
            return null;
        }
        b3 = base64Alphabet[d3];
        b4 = base64Alphabet[d4];
        decodedData[encodedIndex++] = (byte) (b1 << 2 | b2 >> 4);
        decodedData[encodedIndex++] = (byte) ((b2 & 0xF) << 4 | b3 >> 2 & 0xF);
        decodedData[encodedIndex++] = (byte) (b3 << 6 | b4);
        return decodedData;
    }

    private static boolean isWhiteSpace(char octect) {
        return octect == ' ' || octect == '\r' || octect == '\n' || octect == '\t';
    }

    private static boolean isData(char octect) {
        return octect < '\u00ff' && base64Alphabet[octect] != -1;
    }

    private static boolean isPad(char octect) {
        return octect == '=';
    }

    private static int removeWhiteSpace(char[] data) {
        if (data == null) {
            return 0;
        }
        int newSize = 0;
        int len = data.length;
        for (int i = 0; i < len; ++i) {
            if (Base64.isWhiteSpace(data[i])) continue;
            data[newSize++] = data[i];
        }
        return newSize;
    }

    static {
        int i;
        CHUNK_SEPARATOR = "\r\n".getBytes();
        base64Alphabet = new byte[255];
        lookUpBase64Alphabet = new byte[64];
        for (i = 0; i < 255; ++i) {
            Base64.base64Alphabet[i] = -1;
        }
        for (i = 90; i >= 65; --i) {
            Base64.base64Alphabet[i] = (byte) (i - 65);
        }
        for (i = 122; i >= 97; --i) {
            Base64.base64Alphabet[i] = (byte) (i - 97 + 26);
        }
        for (i = 57; i >= 48; --i) {
            Base64.base64Alphabet[i] = (byte) (i - 48 + 52);
        }
        Base64.base64Alphabet[43] = 62;
        Base64.base64Alphabet[47] = 63;
        for (i = 0; i <= 25; ++i) {
            Base64.lookUpBase64Alphabet[i] = (byte) (65 + i);
        }
        i = 26;
        int j = 0;
        while (i <= 51) {
            Base64.lookUpBase64Alphabet[i] = (byte) (97 + j);
            ++i;
            ++j;
        }
        i = 52;
        j = 0;
        while (i <= 61) {
            Base64.lookUpBase64Alphabet[i] = (byte) (48 + j);
            ++i;
            ++j;
        }
        Base64.lookUpBase64Alphabet[62] = 43;
        Base64.lookUpBase64Alphabet[63] = 47;
    }
}
