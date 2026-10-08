// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.security.impl;

import ats.foundation.utils.config.PropertiesHelper;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;

public class PasswordKeyUOB implements IPasswordKey {
    private InternalUOBKey _InternalUOBKey = null;

    public PasswordKeyUOB(String param1, String param2) throws SecurityException {
        this._InternalUOBKey = new InternalUOBKey(this.getParams(param1), this.getParams(param2));
    }

    private String getParams(String params) throws SecurityException {
        if (params.startsWith("FILE:")) {
            String filePath = params.substring(5);
            BufferedReader fileReader = null;
            try {
                fileReader =
                        new BufferedReader(
                                new FileReader(
                                        new File(
                                                PropertiesHelper.getFullPath(
                                                        PropertiesHelper.getATS_ROOT(),
                                                        filePath))));
                StringBuilder buff = new StringBuilder();
                String linevalue = null;
                while ((linevalue = fileReader.readLine()) != null) {
                    buff.append(linevalue);
                }
                String string = buff.toString();
                return string;
            } catch (FileNotFoundException e) {
                throw new SecurityException("PasswordKeyUOB init params[" + params + "] faild.", e);
            } catch (IOException e) {
                throw new SecurityException("PasswordKeyUOB read params[" + params + "] faild.", e);
            } finally {
                if (fileReader != null) {
                    try {
                        fileReader.close();
                    } catch (IOException e) {
                    }
                }
            }
        }
        if (params.startsWith("STRING:")) {
            return params.substring(7);
        }
        throw new SecurityException("unsupport params[" + params + "].");
    }

    @Override
    public byte[] getPasswordKey() {
        return this._InternalUOBKey.getPasswordKey();
    }
}
