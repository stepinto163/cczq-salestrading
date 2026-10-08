// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import ats.app.api.PResult;

import java.util.ArrayList;
import java.util.List;

public class BrokerCrmContactQueryResult extends PResult {
    public int totalSize;
    public int pageSize;
    public List<TBrokerCrmContact> datas = new ArrayList<TBrokerCrmContact>();

    public boolean isOver() {
        return this.totalSize < this.pageSize;
    }
}
