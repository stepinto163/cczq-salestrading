// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import ats.app.api.PResult;

public class CalWeekDateResult extends PResult {
    public WeekInfo Data;

    @Override
    public String toString() {
        return "CalWeekDateResult [Data=" + this.Data + "]";
    }
}
