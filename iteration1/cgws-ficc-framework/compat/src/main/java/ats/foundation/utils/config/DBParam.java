// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.config;

public class DBParam {
    public String Driver = "";
    public String Url = "";
    public String UserName = "";
    public String Passwd = "";

    static enum TYPE {
        UnKnown,
        JDBC {

            public String toString() {
                return "JDBC";
            }
        },
        ServiceName {

            public String toString() {
                return "SERVICENAME";
            }
        };
    }
}
