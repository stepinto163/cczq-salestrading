// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import ats.app.api.PResult;

import java.util.ArrayList;
import java.util.List;

public class BrokerCrmInstQueryResult extends PResult {
    public int pageSize;
    public int totalSize;
    public List<TBrokerCrmInst> datas = new ArrayList<TBrokerCrmInst>();

    public boolean isOver() {
        return this.totalSize < this.pageSize;
    }
}
