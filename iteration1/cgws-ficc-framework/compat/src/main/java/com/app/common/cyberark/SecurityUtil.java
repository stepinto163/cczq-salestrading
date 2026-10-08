// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.cyberark;

import java.io.UnsupportedEncodingException;
import java.security.InvalidAlgorithmParameterException;
import java.security.InvalidKeyException;
import java.security.Key;
import java.security.NoSuchAlgorithmException;

import javax.crypto.Cipher;
import javax.crypto.NoSuchPaddingException;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

public class SecurityUtil {
    private static final byte[] IV = "pidms20180327!@#".getBytes();

    public static String decrypt20(String sSrc, String sKey)
            throws UnsupportedEncodingException,
                    NoSuchPaddingException,
                    NoSuchAlgorithmException,
                    InvalidAlgorithmParameterException,
                    InvalidKeyException {
        if (sKey == null || sKey == null) {
            throw new IllegalArgumentException();
        }
        byte[] raw = sKey.getBytes("utf-8");
        SecretKeySpec skeySpec = new SecretKeySpec(raw, "AES");
        Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
        cipher.init(2, (Key) skeySpec, new IvParameterSpec(IV));
        try {
            byte[] original = cipher.doFinal(HexUtil.hexToBytes(sSrc));
            String originalString = new String(original);
            return originalString;
        } catch (Exception e) {
            return null;
        }
    }
}
