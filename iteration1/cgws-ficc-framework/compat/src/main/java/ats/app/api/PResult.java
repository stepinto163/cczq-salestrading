// Recovered compatibility source; provenance: recovered-support.json.
package ats.app.api;

public class PResult {
    public String Code = "0000";
    public String Msg = "Success";
    public String cid;
    public String data;
    public String encryptkey;

    public boolean isSuccess() {
        return "0000".equals(this.Code);
    }

    public String toString() {
        return "[Code=" + this.Code + ", Msg=" + this.Msg + "]";
    }
}
