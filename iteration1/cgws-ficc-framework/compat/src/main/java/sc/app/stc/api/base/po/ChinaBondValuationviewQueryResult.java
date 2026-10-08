// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import ats.app.api.PResult;

import java.util.ArrayList;
import java.util.List;

public class ChinaBondValuationviewQueryResult extends PResult {
    public List<TBondValuationView> Datas = new ArrayList<TBondValuationView>();
    public boolean lastpage;
}
