// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.config.mqserver;

public class MQServer {

    public static enum TYPE {
        UnKnown,
        Server {

            public String toString() {
                return "SERVER";
            }
        },
        Client {

            public String toString() {
                return "CLIENT";
            }
        },
        Regiser {

            public String toString() {
                return "REGISTER";
            }
        };
    }
}
