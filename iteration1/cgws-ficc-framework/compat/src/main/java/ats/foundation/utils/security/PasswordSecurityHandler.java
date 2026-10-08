// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.security;

import ats.foundation.utils.config.ATSROOTConfig;
import ats.foundation.utils.config.PropertiesHelper;
import ats.foundation.utils.security.impl.IPasswordSecurity;
import ats.foundation.utils.security.impl.PasswordKeyUOB;
import ats.foundation.utils.security.impl.PasswordSecurityAES;

import sun.misc.BASE64Decoder;
import sun.misc.BASE64Encoder;

import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.util.Hashtable;

public class PasswordSecurityHandler {
    private static final String _PasswordCharset = "utf-8";
    private static PasswordSecurityHandler _Handler = null;
    private boolean _PasswordSecurityEnable = false;
    private static String _PasswordSecurityType = "NO";
    private byte[] _PasswordSecurityKey = null;
    private byte[] _MasterKey = null;
    private Hashtable<String, IPasswordSecurity> _asswordSecurityTable = new Hashtable();

    public static synchronized PasswordSecurityHandler getCreateMasterHandler()
            throws SecurityException {
        if (_Handler == null) {
            _Handler = new PasswordSecurityHandler(false);
        }
        return _Handler;
    }

    public static synchronized PasswordSecurityHandler getHandler() throws SecurityException {
        if (_Handler == null) {
            _Handler = new PasswordSecurityHandler();
        }
        return _Handler;
    }

    public static synchronized PasswordSecurityHandler getHandler(String strATSConfig)
            throws SecurityException, IOException {
        if (_Handler == null) {
            _Handler = new PasswordSecurityHandler(strATSConfig);
        }
        return _Handler;
    }

    public void initialization(String key1, String key2, String enMastKey)
            throws SecurityException {
        IPasswordSecurity iPwdSecurity;
        PasswordKeyUOB iPasswordKey = new PasswordKeyUOB(key1, key2);
        this._PasswordSecurityKey = iPasswordKey.getPasswordKey();
        if (enMastKey == null || enMastKey.equals("null")) {
            enMastKey = "";
        }
        if (enMastKey.length() > 0
                && (iPwdSecurity = this.findPasswordSecurity(_PasswordSecurityType)) != null) {
            try {
                byte[] bytesContent = new BASE64Decoder().decodeBuffer(enMastKey);
                byte[] newContent = iPwdSecurity.decrypt(bytesContent, this._PasswordSecurityKey);
                this._MasterKey = new byte[16];
                for (int i = 0; i < 16; ++i) {
                    this._MasterKey[i] =
                            i >= newContent.length ? Byte.valueOf("" + i) : newContent[i];
                }
            } catch (Exception e) {
                throw new SecurityException("Password encrypt faild,", e);
            }
        }
    }

    public String createMaster(String content) throws SecurityException {
        IPasswordSecurity iPwdSecurity = this.findPasswordSecurity(_PasswordSecurityType);
        if (iPwdSecurity != null) {
            try {
                byte[] bmastkey = new byte[16];
                byte[] input = content.getBytes(_PasswordCharset);
                for (int i = 0; i < 16; ++i) {
                    bmastkey[i] = i >= input.length ? Byte.valueOf("" + (i + 30)) : input[i];
                }
                byte[] newContent = iPwdSecurity.encrypt(bmastkey, this._PasswordSecurityKey);
                return new BASE64Encoder().encode(newContent);
            } catch (UnsupportedEncodingException e) {
                throw new SecurityException("Password encrypt faild,", e);
            }
        }
        return content;
    }

    public String encrypt(String content, boolean isEnable) throws SecurityException {
        if (!this._PasswordSecurityEnable || !isEnable) {
            return content;
        }
        IPasswordSecurity iPwdSecurity = this.findPasswordSecurity(_PasswordSecurityType);
        if (iPwdSecurity != null) {
            try {
                byte[] newContent =
                        this._MasterKey == null
                                ? iPwdSecurity.encrypt(
                                        content.getBytes(_PasswordCharset),
                                        this._PasswordSecurityKey)
                                : iPwdSecurity.encrypt(
                                        content.getBytes(_PasswordCharset), this._MasterKey);
                return new BASE64Encoder().encode(newContent);
            } catch (UnsupportedEncodingException e) {
                throw new SecurityException("Password encrypt faild,", e);
            }
        }
        return content;
    }

    public String decrypt(String content, boolean isEnable) throws SecurityException {
        if (!this._PasswordSecurityEnable || !isEnable) {
            return content;
        }
        try {
            byte[] bytesContent = new BASE64Decoder().decodeBuffer(content);
            IPasswordSecurity iPwdSecurity = this.findPasswordSecurity(_PasswordSecurityType);
            if (iPwdSecurity != null) {
                byte[] newContent =
                        this._MasterKey == null
                                ? iPwdSecurity.decrypt(bytesContent, this._PasswordSecurityKey)
                                : iPwdSecurity.decrypt(bytesContent, this._MasterKey);
                return new String(newContent, _PasswordCharset);
            }
        } catch (UnsupportedEncodingException e) {
            throw new SecurityException("Password encrypt faild,", e);
        } catch (IOException e) {
            throw new SecurityException("String to Byte fail,", e);
        }
        return content;
    }

    private PasswordSecurityHandler(boolean createMastKey) throws SecurityException {
        this.Register(new PasswordSecurityAES());
        String tmpEnable = ATSROOTConfig.getATSROOTConfig().get("PasswordSecurityEnable") + "";
        if (tmpEnable.equals("1") || tmpEnable.equalsIgnoreCase("true")) {
            this._PasswordSecurityEnable = true;
            String passwordSecurityKey1 =
                    ATSROOTConfig.getATSROOTConfig().get("PasswordSecurityKey1") + "";
            String passwordSecurityKey2 =
                    ATSROOTConfig.getATSROOTConfig().get("PasswordSecurityKey2") + "";
            String EnMasterKey = "";
            this.initialization(passwordSecurityKey1, passwordSecurityKey2, EnMasterKey);
        }
    }

    private PasswordSecurityHandler() throws SecurityException {
        this.Register(new PasswordSecurityAES());
        String tmpEnable = ATSROOTConfig.getATSROOTConfig().get("PasswordSecurityEnable") + "";
        if (tmpEnable.equals("1") || tmpEnable.equalsIgnoreCase("true")) {
            this._PasswordSecurityEnable = true;
            String passwordSecurityKey1 =
                    ATSROOTConfig.getATSROOTConfig().get("PasswordSecurityKey1") + "";
            String passwordSecurityKey2 =
                    ATSROOTConfig.getATSROOTConfig().get("PasswordSecurityKey2") + "";
            String EnMasterKey = ATSROOTConfig.getATSROOTConfig().get("EncryptedMastKey") + "";
            this.initialization(passwordSecurityKey1, passwordSecurityKey2, EnMasterKey);
        }
    }

    private PasswordSecurityHandler(String strATSConfig) throws SecurityException, IOException {
        this.Register(new PasswordSecurityAES());
        PropertiesHelper propertes = new PropertiesHelper();
        propertes.load(strATSConfig);
        String tmpEnable = propertes.get("PasswordSecurityEnable") + "";
        if (tmpEnable.equals("1") || tmpEnable.equalsIgnoreCase("true")) {
            this._PasswordSecurityEnable = true;
            String passwordSecurityKey1 = propertes.get("PasswordSecurityKey1") + "";
            String passwordSecurityKey2 = propertes.get("PasswordSecurityKey2") + "";
            String EnMasterKey = ATSROOTConfig.getATSROOTConfig().get("EncryptedMastKey") + "";
            this.initialization(passwordSecurityKey1, passwordSecurityKey2, EnMasterKey);
        }
    }

    private synchronized void Register(IPasswordSecurity pwdSecurity) {
        if (pwdSecurity != null) {
            this._asswordSecurityTable.put(pwdSecurity.getSecurityType(), pwdSecurity);
        }
    }

    private IPasswordSecurity findPasswordSecurity(String securityType) {
        if (this._asswordSecurityTable.containsKey(securityType)) {
            return this._asswordSecurityTable.get(securityType);
        }
        return null;
    }

    static {
        _PasswordSecurityType = "AES";
    }
}
