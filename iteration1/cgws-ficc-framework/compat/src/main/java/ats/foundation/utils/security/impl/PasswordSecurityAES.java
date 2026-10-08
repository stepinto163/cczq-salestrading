// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.security.impl;

import ats.foundation.utils.security.SecurityException;

import java.security.InvalidAlgorithmParameterException;
import java.security.InvalidKeyException;
import java.security.Key;
import java.security.NoSuchAlgorithmException;

import javax.crypto.BadPaddingException;
import javax.crypto.Cipher;
import javax.crypto.IllegalBlockSizeException;
import javax.crypto.NoSuchPaddingException;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

public class PasswordSecurityAES implements IPasswordSecurity {
    private String _SecurityType = "AES";
    private String _CipherType = "AES/CBC/PKCS5Padding";
    private byte[] _biv = new byte[16];
    private IvParameterSpec _iv;

    public PasswordSecurityAES() {
        for (int i = 0; i < 16; ++i) {
            this._biv[i] = 0;
        }
        this._iv = new IvParameterSpec(this._biv);
    }

    @Override
    public String getSecurityType() {
        return this._SecurityType;
    }

    @Override
    public byte[] decrypt(byte[] content, byte[] password) throws SecurityException {
        try {
            SecretKeySpec key = new SecretKeySpec(password, this._SecurityType);
            Cipher cipher = Cipher.getInstance(this._CipherType);
            cipher.init(2, (Key) key, this._iv);
            byte[] result = cipher.doFinal(content);
            return result;
        } catch (NoSuchAlgorithmException e) {
            throw new SecurityException(
                    "Password-[" + this._SecurityType + "] decrypt faild,NoSuchAlgorithmException-",
                    e);
        } catch (NoSuchPaddingException e) {
            throw new SecurityException(
                    "Password-[" + this._SecurityType + "] decrypt faild,NoSuchPaddingException-",
                    e);
        } catch (InvalidKeyException e) {
            throw new SecurityException(
                    "Password-[" + this._SecurityType + "] decrypt faild,SecurityException-", e);
        } catch (IllegalBlockSizeException e) {
            throw new SecurityException(
                    "Password-["
                            + this._SecurityType
                            + "] decrypt faild,IllegalBlockSizeException-",
                    e);
        } catch (BadPaddingException e) {
            throw new SecurityException(
                    "Password-[" + this._SecurityType + "] decrypt faild,BadPaddingException-", e);
        } catch (InvalidAlgorithmParameterException e) {
            throw new SecurityException(
                    "Password-["
                            + this._SecurityType
                            + "] decrypt faild,InvalidAlgorithmParameterException-",
                    e);
        }
    }

    @Override
    public byte[] encrypt(byte[] byteContent, byte[] password) throws SecurityException {
        try {
            SecretKeySpec key = new SecretKeySpec(password, this._SecurityType);
            Cipher cipher = Cipher.getInstance(this._CipherType);
            cipher.init(1, (Key) key, this._iv);
            byte[] result = cipher.doFinal(byteContent);
            return result;
        } catch (NoSuchAlgorithmException e) {
            throw new SecurityException(
                    "Password-[" + this._SecurityType + "] encrypt faild,NoSuchAlgorithmException-",
                    e);
        } catch (NoSuchPaddingException e) {
            throw new SecurityException(
                    "Password-[" + this._SecurityType + "] encrypt faild,NoSuchPaddingException-",
                    e);
        } catch (InvalidKeyException e) {
            throw new SecurityException(
                    "Password-[" + this._SecurityType + "] encrypt faild,SecurityException-", e);
        } catch (IllegalBlockSizeException e) {
            throw new SecurityException(
                    "Password-["
                            + this._SecurityType
                            + "] encrypt faild,IllegalBlockSizeException-",
                    e);
        } catch (BadPaddingException e) {
            throw new SecurityException(
                    "Password-[" + this._SecurityType + "] encrypt faild,BadPaddingException-", e);
        } catch (InvalidAlgorithmParameterException e) {
            throw new SecurityException(
                    "Password-["
                            + this._SecurityType
                            + "] decrypt faild,InvalidAlgorithmParameterException-",
                    e);
        }
    }
}
