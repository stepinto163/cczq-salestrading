// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.encrypt;

import java.io.UnsupportedEncodingException;
import java.security.NoSuchAlgorithmException;
import java.security.SecureRandom;

import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.SecretKeySpec;

public class AES {
    public static byte[] encrypt(byte[] data, byte[] key) {
        CheckUtils.notEmpty(data, "data");
        CheckUtils.notEmpty(key, "key");
        if (key.length != 16) {
            throw new RuntimeException("Invalid AES key length (must be 16 bytes)");
        }
        try {
            SecretKeySpec secretKey = new SecretKeySpec(key, "AES");
            byte[] enCodeFormat = secretKey.getEncoded();
            SecretKeySpec seckey = new SecretKeySpec(enCodeFormat, "AES");
            Cipher cipher = Cipher.getInstance("AES/ECB/PKCS5Padding");
            cipher.init(1, seckey);
            byte[] result = cipher.doFinal(data);
            return result;
        } catch (Exception e) {
            throw new RuntimeException("encrypt fail!", e);
        }
    }

    public static byte[] decrypt(byte[] data, byte[] key) {
        CheckUtils.notEmpty(data, "data");
        CheckUtils.notEmpty(key, "key");
        if (key.length != 16) {
            throw new RuntimeException("Invalid AES key length (must be 16 bytes)");
        }
        try {
            SecretKeySpec secretKey = new SecretKeySpec(key, "AES");
            byte[] enCodeFormat = secretKey.getEncoded();
            SecretKeySpec seckey = new SecretKeySpec(enCodeFormat, "AES");
            Cipher cipher = Cipher.getInstance("AES/ECB/PKCS5Padding");
            cipher.init(2, seckey);
            byte[] result = cipher.doFinal(data);
            return result;
        } catch (Exception e) {
            throw new RuntimeException("decrypt fail!", e);
        }
    }

    public static String encryptToBase64(String data, String key) {
        try {
            byte[] valueByte = AES.encrypt(data.getBytes("UTF-8"), key.getBytes("UTF-8"));
            return new String(Base64.encode(valueByte));
        } catch (UnsupportedEncodingException e) {
            throw new RuntimeException("encrypt fail!", e);
        }
    }

    public static String decryptFromBase64(String data, String key) {
        try {
            byte[] originalData = Base64.decode(data.getBytes());
            byte[] valueByte = AES.decrypt(originalData, key.getBytes("UTF-8"));
            return new String(valueByte, "UTF-8");
        } catch (UnsupportedEncodingException e) {
            throw new RuntimeException("decrypt fail!", e);
        }
    }

    public static String encryptWithKeyBase64(String data, String key) {
        try {
            byte[] valueByte = AES.encrypt(data.getBytes("UTF-8"), Base64.decode(key.getBytes()));
            return new String(Base64.encode(valueByte));
        } catch (UnsupportedEncodingException e) {
            throw new RuntimeException("encrypt fail!", e);
        }
    }

    public static String decryptWithKeyBase64(String data, String key) {
        try {
            byte[] originalData = Base64.decode(data.getBytes());
            byte[] valueByte = AES.decrypt(originalData, Base64.decode(key.getBytes()));
            return new String(valueByte, "UTF-8");
        } catch (UnsupportedEncodingException e) {
            throw new RuntimeException("decrypt fail!", e);
        }
    }

    public static byte[] genarateRandomKey() {
        KeyGenerator keygen = null;
        try {
            keygen = KeyGenerator.getInstance("AES/ECB/PKCS5Padding");
        } catch (NoSuchAlgorithmException e) {
            throw new RuntimeException(" genarateRandomKey fail!", e);
        }
        SecureRandom random = new SecureRandom();
        keygen.init(random);
        SecretKey key = keygen.generateKey();
        return key.getEncoded();
    }

    public static String genarateRandomKeyWithBase64() {
        return new String(Base64.encode(AES.genarateRandomKey()));
    }
}
