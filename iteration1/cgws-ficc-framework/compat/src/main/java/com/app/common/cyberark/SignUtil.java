// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.cyberark;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Map;

public class SignUtil {
    public static String makeSign(String s) {
        MessageDigest digest = null;
        try {
            digest = MessageDigest.getInstance("SHA-1");
            digest.update(s.getBytes());
            byte[] messageDigest = digest.digest();
            return HexUtil.bytesToHex(messageDigest);
        } catch (NoSuchAlgorithmException e) {
            return null;
        }
    }

    public static String makeSign2(String s) {
        MessageDigest digest = null;
        try {
            digest = MessageDigest.getInstance("SHA-256");
            digest.update(s.getBytes());
            byte[] messageDigest = digest.digest();
            return HexUtil.bytesToHex(messageDigest);
        } catch (NoSuchAlgorithmException e) {
            return null;
        }
    }

    public static String makeSign(Map<String, Object> data, String key) {
        StringBuffer sb = new StringBuffer();
        String appCoe = (String) data.get("appId");
        sb.append(appCoe).append('&').append(key);
        return sb.length() > 0 ? SignUtil.makeSign(sb.toString()) : null;
    }
}
