// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.algo;

public class PResult {
    public String Code = "0000";
    public String Msg = "Success";
    public String Info1 = "";
    public String Info2 = "";
    public String Info3 = "";
    public String Info4 = "";
    public String RequestId;

    public boolean isSuccess() {
        return "0000".equals(this.Code);
    }

    public String toString() {
        return "PResult [Code="
                + this.Code
                + ", Msg="
                + this.Msg
                + ", Info1="
                + this.Info1
                + ", Info2="
                + this.Info2
                + ", Info3="
                + this.Info3
                + ", Info4="
                + this.Info4
                + ", RequestId="
                + this.RequestId
                + "]";
    }
}
