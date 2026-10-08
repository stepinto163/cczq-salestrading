// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import ats.app.api.PResult;

import java.util.ArrayList;
import java.util.List;

public class BrokerCrmAccountQueryResult extends PResult {
    public int totalSize;
    public int pageSize;
    public List<TBrokerCrmAccount> datas = new ArrayList<TBrokerCrmAccount>();

    public boolean isOver() {
        return this.totalSize < this.pageSize;
    }
}
