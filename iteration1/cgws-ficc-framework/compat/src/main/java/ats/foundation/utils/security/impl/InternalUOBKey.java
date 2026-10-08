// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.security.impl;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

class InternalUOBKey {
    private byte[] _PasswordKey = new byte[16];

    public byte[] getPasswordKey() {
        return this._PasswordKey;
    }

    public InternalUOBKey(String key1, String key2) throws SecurityException {
        int startIndex;
        int i;
        byte[] key1Bytes = key1.getBytes();
        byte[] key2Bytes = key2.getBytes();
        byte[] useKey1 = new byte[80];
        byte[] useKey2 = new byte[80];
        for (i = startIndex = 4; i < 80 + startIndex; ++i) {
            useKey1[i - startIndex] = i >= key1Bytes.length ? (byte) 1 : key1Bytes[i];
        }
        for (i = startIndex = 23; i < 80 + startIndex; ++i) {
            useKey2[i - startIndex] = i >= key2Bytes.length ? (byte) 1 : key2Bytes[i];
        }
        byte[] useKey = new byte[80];
        for (int i2 = 0; i2 < useKey1.length; ++i2) {
            useKey[i2] = (byte) (useKey1[i2] ^ useKey2[i2]);
        }
        try {
            MessageDigest mdInst = MessageDigest.getInstance("MD5");
            mdInst.update(useKey);
            byte[] md = mdInst.digest();
            for (int i3 = 0; i3 < 16; ++i3) {
                this._PasswordKey[i3] = i3 >= md.length ? 48 : md[i3];
            }
        } catch (NoSuchAlgorithmException e) {
            throw new SecurityException("InternalUOBKey inti faild,", e);
        }
    }
}
