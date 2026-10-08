// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import ats.app.api.PResult;

import java.util.ArrayList;
import java.util.List;

public class BrokerHolidayQueryResult extends PResult {
    public int totalSize;
    public List<Holiday> Datas = new ArrayList<Holiday>();
}
