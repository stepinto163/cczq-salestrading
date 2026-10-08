// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.encrypt;

import java.math.BigInteger;
import java.security.KeyFactory;
import java.security.KeyPair;
import java.security.KeyPairGenerator;
import java.security.PrivateKey;
import java.security.PublicKey;
import java.security.SecureRandom;
import java.security.Signature;
import java.security.interfaces.RSAPublicKey;
import java.security.spec.PKCS8EncodedKeySpec;
import java.security.spec.X509EncodedKeySpec;
import java.util.HashMap;
import java.util.Map;

import javax.crypto.Cipher;

public class RSA {
    private static int KEYSIZE = 1024;

    public static Map<String, String> generateKeyPair() throws Exception {
        SecureRandom sr = new SecureRandom();
        KeyPairGenerator kpg = KeyPairGenerator.getInstance("RSA");
        kpg.initialize(KEYSIZE, sr);
        KeyPair kp = kpg.generateKeyPair();
        PublicKey publicKey = kp.getPublic();
        byte[] publicKeyBytes = publicKey.getEncoded();
        String pub = new String(Base64.encodeBase64(publicKeyBytes), "UTF-8");
        PrivateKey privateKey = kp.getPrivate();
        byte[] privateKeyBytes = privateKey.getEncoded();
        String pri = new String(Base64.encodeBase64(privateKeyBytes), "UTF-8");
        HashMap<String, String> map = new HashMap<String, String>();
        map.put("publicKey", pub);
        map.put("privateKey", pri);
        RSAPublicKey rsp = (RSAPublicKey) kp.getPublic();
        BigInteger bint = rsp.getModulus();
        byte[] b = bint.toByteArray();
        byte[] deBase64Value = Base64.encodeBase64(b);
        String retValue = new String(deBase64Value);
        map.put("modulus", retValue);
        return map;
    }

    public static String encrypt(String source, String publicKey) throws Exception {
        PublicKey key = RSA.getPublicKey(publicKey);
        Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");
        cipher.init(1, key);
        byte[] b = source.getBytes();
        byte[] b1 = cipher.doFinal(b);
        return new String(Base64.encodeBase64(b1), "UTF-8");
    }

    public static String decrypt(String cryptograph, String privateKey) throws Exception {
        PrivateKey key = RSA.getPrivateKey(privateKey);
        Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");
        cipher.init(2, key);
        byte[] b1 = Base64.decodeBase64(cryptograph.getBytes());
        byte[] b = cipher.doFinal(b1);
        return new String(b);
    }

    public static PublicKey getPublicKey(String key) throws Exception {
        X509EncodedKeySpec keySpec = new X509EncodedKeySpec(Base64.decodeBase64(key.getBytes()));
        KeyFactory keyFactory = KeyFactory.getInstance("RSA");
        PublicKey publicKey = keyFactory.generatePublic(keySpec);
        return publicKey;
    }

    public static PrivateKey getPrivateKey(String key) throws Exception {
        PKCS8EncodedKeySpec keySpec = new PKCS8EncodedKeySpec(Base64.decodeBase64(key.getBytes()));
        KeyFactory keyFactory = KeyFactory.getInstance("RSA");
        PrivateKey privateKey = keyFactory.generatePrivate(keySpec);
        return privateKey;
    }

    public static String sign(String content, String privateKey) {
        String charset = "UTF-8";
        try {
            PKCS8EncodedKeySpec priPKCS8 =
                    new PKCS8EncodedKeySpec(Base64.decodeBase64(privateKey.getBytes()));
            KeyFactory keyf = KeyFactory.getInstance("RSA");
            PrivateKey priKey = keyf.generatePrivate(priPKCS8);
            Signature signature = Signature.getInstance("SHA1WithRSA");
            signature.initSign(priKey);
            signature.update(content.getBytes(charset));
            byte[] signed = signature.sign();
            return new String(Base64.encodeBase64(signed));
        } catch (Exception exception) {
            return null;
        }
    }

    public static boolean checkSign(String content, String sign, String publicKey) {
        try {
            KeyFactory keyFactory = KeyFactory.getInstance("RSA");
            byte[] encodedKey = Base64.decode2(publicKey);
            PublicKey pubKey = keyFactory.generatePublic(new X509EncodedKeySpec(encodedKey));
            Signature signature = Signature.getInstance("SHA1WithRSA");
            signature.initVerify(pubKey);
            signature.update(content.getBytes("utf-8"));
            boolean bverify = signature.verify(Base64.decode2(sign));
            return bverify;
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
    }
}
