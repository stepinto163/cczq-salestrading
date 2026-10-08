// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import ats.app.api.PResult;

import java.util.ArrayList;
import java.util.List;

public class BondsInfoQueryResult extends PResult {
    public int totalSize;
    public List<BondInfoView> bondInfoViews = new ArrayList<BondInfoView>();
    public boolean lastpage;
    public boolean flag;
}
