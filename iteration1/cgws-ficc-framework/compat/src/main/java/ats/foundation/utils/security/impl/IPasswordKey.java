// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.security.impl;

import ats.foundation.utils.security.SecurityException;

public interface IPasswordKey {
    public byte[] getPasswordKey() throws SecurityException;
}
