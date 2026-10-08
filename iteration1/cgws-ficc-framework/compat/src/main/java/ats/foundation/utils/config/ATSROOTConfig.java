// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.config;

import java.io.IOException;

public class ATSROOTConfig {
    private static PropertiesHelper ATSROOT = null;

    public static void main(String[] args) {
        PropertiesHelper temp = ATSROOTConfig.getATSROOTConfig();
        try {
            System.out.println(
                    PropertiesHelper.printParmas(
                            temp.findClientParam("SERVER.AMQIIIServerAdapter")));
        } catch (IOException e) {
            throw new IllegalStateException("Cannot load ATS_ROOT/control/ATSConfig.ini", e);
        }
    }

    public static PropertiesHelper getATSROOTConfig() {
        return ATSROOT;
    }

    static {
        ATSROOT = new PropertiesHelper();
        try {
            ATSROOT.load(
                    PropertiesHelper.getFullPath(
                            PropertiesHelper.getATS_ROOT(), "/control/ATSConfig.ini"));
        } catch (IOException e) {
            throw new IllegalStateException("Cannot load ATS_ROOT/control/ATSConfig.ini", e);
        }
    }
}
