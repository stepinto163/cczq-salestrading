// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.po;

import java.util.ArrayList;
import java.util.List;

public class FinqData {
    private String id;
    private String code;
    private String name;
    private String side;
    private String join;
    private String price;
    private List<List<String>> vol_delivery = new ArrayList<List<String>>();

    public String getId() {
        return this.id;
    }

    public String getCode() {
        return this.code;
    }

    public String getName() {
        return this.name;
    }

    public String getSide() {
        return this.side;
    }

    public String getJoin() {
        return this.join;
    }

    public String getPrice() {
        return this.price;
    }

    public List<List<String>> getVol_delivery() {
        return this.vol_delivery;
    }

    public void setId(String id) {
        this.id = id;
    }

    public void setCode(String code) {
        this.code = code;
    }

    public void setName(String name) {
        this.name = name;
    }

    public void setSide(String side) {
        this.side = side;
    }

    public void setJoin(String join) {
        this.join = join;
    }

    public void setPrice(String price) {
        this.price = price;
    }

    public void setVol_delivery(List<List<String>> vol_delivery) {
        this.vol_delivery = vol_delivery;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof FinqData)) {
            return false;
        }
        FinqData other = (FinqData) o;
        if (!other.canEqual(this)) {
            return false;
        }
        String this$id = this.getId();
        String other$id = other.getId();
        if (this$id == null ? other$id != null : !this$id.equals(other$id)) {
            return false;
        }
        String this$code = this.getCode();
        String other$code = other.getCode();
        if (this$code == null ? other$code != null : !this$code.equals(other$code)) {
            return false;
        }
        String this$name = this.getName();
        String other$name = other.getName();
        if (this$name == null ? other$name != null : !this$name.equals(other$name)) {
            return false;
        }
        String this$side = this.getSide();
        String other$side = other.getSide();
        if (this$side == null ? other$side != null : !this$side.equals(other$side)) {
            return false;
        }
        String this$join = this.getJoin();
        String other$join = other.getJoin();
        if (this$join == null ? other$join != null : !this$join.equals(other$join)) {
            return false;
        }
        String this$price = this.getPrice();
        String other$price = other.getPrice();
        if (this$price == null ? other$price != null : !this$price.equals(other$price)) {
            return false;
        }
        List<List<String>> this$vol_delivery = this.getVol_delivery();
        List<List<String>> other$vol_delivery = other.getVol_delivery();
        return !(this$vol_delivery == null
                ? other$vol_delivery != null
                : !((Object) this$vol_delivery).equals(other$vol_delivery));
    }

    protected boolean canEqual(Object other) {
        return other instanceof FinqData;
    }

    public int hashCode() {
        int PRIME = 59;
        int result = 1;
        String $id = this.getId();
        result = result * 59 + ($id == null ? 43 : $id.hashCode());
        String $code = this.getCode();
        result = result * 59 + ($code == null ? 43 : $code.hashCode());
        String $name = this.getName();
        result = result * 59 + ($name == null ? 43 : $name.hashCode());
        String $side = this.getSide();
        result = result * 59 + ($side == null ? 43 : $side.hashCode());
        String $join = this.getJoin();
        result = result * 59 + ($join == null ? 43 : $join.hashCode());
        String $price = this.getPrice();
        result = result * 59 + ($price == null ? 43 : $price.hashCode());
        List<List<String>> $vol_delivery = this.getVol_delivery();
        result = result * 59 + ($vol_delivery == null ? 43 : ((Object) $vol_delivery).hashCode());
        return result;
    }

    public String toString() {
        return "FinqData(id="
                + this.getId()
                + ", code="
                + this.getCode()
                + ", name="
                + this.getName()
                + ", side="
                + this.getSide()
                + ", join="
                + this.getJoin()
                + ", price="
                + this.getPrice()
                + ", vol_delivery="
                + this.getVol_delivery()
                + ")";
    }
}
