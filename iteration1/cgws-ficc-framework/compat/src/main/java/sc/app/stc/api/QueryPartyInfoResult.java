// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import ats.app.api.PResult;

import sc.app.stc.api.base.po.TInfoGrp;

import java.util.ArrayList;
import java.util.List;

public class QueryPartyInfoResult extends PResult {
    public int totalSize;
    public List<TInfoGrp> datas = new ArrayList<TInfoGrp>();

    @Override
    public String toString() {
        return " [totalSize="
                + this.totalSize
                + ", datas="
                + this.datas
                + ", Code="
                + this.Code
                + ", Msg="
                + this.Msg
                + "]";
    }
}
