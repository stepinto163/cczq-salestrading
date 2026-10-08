// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.cyberark;

public class HexUtil {
    public static String bytesToHex(byte[] bytes) {
        if (bytes == null) {
            return null;
        }
        StringBuilder hexString = new StringBuilder();
        for (byte b : bytes) {
            String hex = Integer.toHexString(0xFF & b);
            if (hex.length() == 1) {
                hexString.append('0');
            }
            hexString.append(hex);
        }
        return hexString.toString();
    }

    public static byte[] hexToBytes(String hexString) {
        if (hexString == null || hexString.length() % 2 != 0) {
            throw new IllegalArgumentException("Invalid hex string");
        }
        hexString = hexString.toLowerCase();
        int len = hexString.length();
        byte[] data = new byte[len / 2];
        for (int i = 0; i < len; i += 2) {
            char highChar = hexString.charAt(i);
            char lowChar = hexString.charAt(i + 1);
            int high = Character.digit(highChar, 16);
            int low = Character.digit(lowChar, 16);
            if (high == -1 || low == -1) {
                throw new IllegalArgumentException("Invalid hex characters");
            }
            data[i / 2] = (byte) ((high << 4) + low);
        }
        return data;
    }

    public static void main(String[] args) {
        byte[] testBytes = new byte[] {86, 52, 86, 120, -85, -17, -17};
        String hex = HexUtil.bytesToHex(testBytes);
        System.out.println("Hex: " + hex);
        byte[] bytes = HexUtil.hexToBytes(hex);
        System.out.print("Bytes: ");
        for (byte b : bytes) {
            System.out.printf("%02x ", b);
        }
    }
}
