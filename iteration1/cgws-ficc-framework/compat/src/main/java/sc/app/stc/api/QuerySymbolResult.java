// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import ats.app.api.PResult;

import sc.app.stc.api.base.po.BondInfoView;

import java.util.ArrayList;
import java.util.List;

public class QuerySymbolResult extends PResult {
    public int totalSize;
    public List<BondInfoView> datas = new ArrayList<BondInfoView>();

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
