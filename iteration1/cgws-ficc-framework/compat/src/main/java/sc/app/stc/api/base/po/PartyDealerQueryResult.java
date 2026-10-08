// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import ats.app.api.PResult;

import java.util.ArrayList;
import java.util.List;

public class PartyDealerQueryResult extends PResult {
    public int totalSize;
    public List<TDealerInfoGrp> tDealerInfoGrps = new ArrayList<TDealerInfoGrp>();
    public boolean lastpage;
}
