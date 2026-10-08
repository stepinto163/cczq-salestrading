// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.encrypt;

import ats.foundation.utils.exception.CodeException;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import sc.app.stc.utils.Consts;
import sc.app.stc.utils.RandomUtil;

import java.security.MessageDigest;
import java.util.HashMap;
import java.util.Map;

public class EncryptionUtil {
    private static Logger logger = LoggerFactory.getLogger(EncryptionUtil.class);

    public static String DecryptData(boolean isencry, String content, String privatekey)
            throws CodeException {
        String str = content;
        if (isencry) {
            try {
                EncryData ed =
                        (EncryData)
                                MsgHelper.Deserialize(
                                        (String) content,
                                        EncryData.class,
                                        (CharsetConvertType) CharsetConvertType.None);
                str = EncryptionUtil.DecryptData(isencry, ed.data, ed.encryptkey, privatekey);
            } catch (Exception e) {
                throw new CodeException(Consts.NoKnowCode, e.getMessage());
            }
        }
        return str;
    }

    public static String DecryptData(
            boolean isencry, String data, String encryptkey, String privatekey)
            throws CodeException {
        String str = data;
        if (isencry) {
            try {
                String decryptData;
                String AESKey = RSA.decrypt(encryptkey, privatekey);
                str = decryptData = AES.decryptFromBase64(data, AESKey);
            } catch (Exception e) {
                throw new CodeException(Consts.NoKnowCode, e.getMessage());
            }
        }
        return str;
    }

    public static String EncryData(boolean isencry, String content, String publickey)
            throws CodeException {
        String str = content;
        if (isencry) {
            try {
                EncryData ed = new EncryData();
                String AESKey = RandomUtil.getRandom(16);
                ed.data = AES.encryptToBase64(content, AESKey);
                ed.encryptkey = RSA.encrypt(AESKey, publickey);
                str =
                        MsgHelper.Serializer(
                                (Object) ed, (CharsetConvertType) CharsetConvertType.None);
            } catch (Exception e) {
                throw new CodeException(Consts.NoKnowCode, e.getMessage());
            }
        }
        return str;
    }

    public static Object EncryData(boolean isencry, Object o, String publickey)
            throws CodeException {
        Object obj = o;
        if (isencry) {
            try {
                String content =
                        MsgHelper.Serializer(
                                (Object) o, (CharsetConvertType) CharsetConvertType.None);
                EncryData ed = new EncryData();
                String AESKey = RandomUtil.getRandom(16);
                ed.data = AES.encryptToBase64(content, AESKey);
                ed.encryptkey = RSA.encrypt(AESKey, publickey);
                obj = ed;
            } catch (Exception e) {
                throw new CodeException(Consts.NoKnowCode, e.getMessage());
            }
        }
        return obj;
    }

    public static Map<String, Object> EncryDataToMap(boolean isencry, Object o, String publickey) {
        HashMap<String, Object> map = new HashMap<String, Object>();
        if (isencry) {
            try {
                String content =
                        MsgHelper.Serializer(
                                (Object) o, (CharsetConvertType) CharsetConvertType.None);
                EncryData ed = new EncryData();
                String AESKey = RandomUtil.getRandom(16);
                ed.data = AES.encryptToBase64(content, AESKey);
                ed.encryptkey = RSA.encrypt(AESKey, publickey);
                map.put("data", ed.data);
                map.put("encryptkey", ed.encryptkey);
            } catch (Exception e) {
                logger.error("", (Throwable) e);
            }
        }
        return map;
    }

    public static String md5(String str) {
        try {
            MessageDigest md = MessageDigest.getInstance("MD5");
            md.update(str.getBytes());
            byte[] b = md.digest();
            StringBuffer sb = new StringBuffer();
            for (int i = 0; i < b.length; ++i) {
                int v = b[i];
                v = v < 0 ? 256 + v : v;
                String cc = Integer.toHexString(v);
                if (cc.length() == 1) {
                    sb.append('0');
                }
                sb.append(cc);
            }
            return sb.toString();
        } catch (Exception exception) {
            return "";
        }
    }

    public static String sha256(String str) {
        try {
            MessageDigest md = MessageDigest.getInstance("SHA-256");
            md.update(str.getBytes());
            byte[] b = md.digest();
            StringBuffer sb = new StringBuffer();
            for (int i = 0; i < b.length; ++i) {
                int v = b[i];
                v = v < 0 ? 256 + v : v;
                String cc = Integer.toHexString(v);
                if (cc.length() == 1) {
                    sb.append('0');
                }
                sb.append(cc);
            }
            return sb.toString();
        } catch (Exception exception) {
            return "";
        }
    }

    public static String sha1(String str) {
        try {
            MessageDigest md = MessageDigest.getInstance("SHA-1");
            md.update(str.getBytes());
            byte[] b = md.digest();
            StringBuffer sb = new StringBuffer();
            for (int i = 0; i < b.length; ++i) {
                int v = b[i];
                v = v < 0 ? 256 + v : v;
                String cc = Integer.toHexString(v);
                if (cc.length() == 1) {
                    sb.append('0');
                }
                sb.append(cc);
            }
            return sb.toString();
        } catch (Exception exception) {
            return "";
        }
    }
}
