// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import ats.app.api.PResult;

import sc.app.stc.tag.dto.QuoteTagDto;

public class QuoteTagResult extends PResult {
    public int totalSize;
    public QuoteTagDto datas;

    public int getTotalSize() {
        return this.totalSize;
    }

    public QuoteTagDto getDatas() {
        return this.datas;
    }

    public void setTotalSize(int totalSize) {
        this.totalSize = totalSize;
    }

    public void setDatas(QuoteTagDto datas) {
        this.datas = datas;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof QuoteTagResult)) {
            return false;
        }
        QuoteTagResult other = (QuoteTagResult) o;
        if (!other.canEqual(this)) {
            return false;
        }
        if (this.getTotalSize() != other.getTotalSize()) {
            return false;
        }
        QuoteTagDto this$datas = this.getDatas();
        QuoteTagDto other$datas = other.getDatas();
        return !(this$datas == null
                ? other$datas != null
                : !((Object) this$datas).equals(other$datas));
    }

    protected boolean canEqual(Object other) {
        return other instanceof QuoteTagResult;
    }

    public int hashCode() {
        int PRIME = 59;
        int result = 1;
        result = result * 59 + this.getTotalSize();
        QuoteTagDto $datas = this.getDatas();
        result = result * 59 + ($datas == null ? 43 : ((Object) $datas).hashCode());
        return result;
    }

    @Override
    public String toString() {
        return "QuoteTagResult(totalSize="
                + this.getTotalSize()
                + ", datas="
                + this.getDatas()
                + ")";
    }
}
