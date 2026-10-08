// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.security.impl;

import ats.foundation.utils.security.SecurityException;

public interface IPasswordSecurity {
    public String getSecurityType();

    public byte[] encrypt(byte[] var1, byte[] var2) throws SecurityException;

    public byte[] decrypt(byte[] var1, byte[] var2) throws SecurityException;
}
