// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

public class WeekInfo {
    public String settlementType;
    public String baseDate;
    public String settlementType2;
    public String dealDate;
    public String settlementDate;

    public String toString() {
        return "WeekInfo [settlementType="
                + this.settlementType
                + ", baseDate="
                + this.baseDate
                + ", settlementType2="
                + this.settlementType2
                + ", dealDate="
                + this.dealDate
                + ", settlementDate="
                + this.settlementDate
                + "]";
    }
}
