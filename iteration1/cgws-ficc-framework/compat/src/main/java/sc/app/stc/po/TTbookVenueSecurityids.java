// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.po;

import com.alibaba.fastjson.annotation.JSONField;

import java.math.BigDecimal;

public class TTbookVenueSecurityids {
    @JSONField(name = "id", serialize = true)
    public String id = "";

    @JSONField(name = "venuetypegw", serialize = true)
    public String venuetypegw;

    @JSONField(name = "venues", serialize = true)
    public String venues;

    @JSONField(name = "securityid", serialize = true)
    public String securityid;

    @JSONField(name = "symbol", serialize = true)
    public String symbol;

    @JSONField(name = "contractsize", serialize = true)
    public BigDecimal contractsize;

    @JSONField(name = "maxsize", serialize = true)
    public BigDecimal maxsize;

    @JSONField(name = "minsize", serialize = true)
    public BigDecimal minsize;

    @JSONField(name = "stepvalue", serialize = true)
    public BigDecimal stepvalue;

    @JSONField(name = "createtime", serialize = true)
    public String createtime;

    @JSONField(name = "updatetime", serialize = true)
    public String updatetime;

    @JSONField(name = "splitordertype", serialize = true)
    public String splitordertype;

    public TTbookVenueSecurityids() {}

    public TTbookVenueSecurityids(
            String id,
            String venuetypegw,
            String venues,
            String securityid,
            String symbol,
            BigDecimal contractsize,
            BigDecimal maxsize,
            BigDecimal minsize,
            BigDecimal stepvalue,
            String splitordertype) {
        this.id = id;
        this.venuetypegw = venuetypegw;
        this.venues = venues;
        this.securityid = securityid;
        this.symbol = symbol;
        this.contractsize = contractsize;
        this.maxsize = maxsize;
        this.minsize = minsize;
        this.stepvalue = stepvalue;
        this.splitordertype = splitordertype;
    }
}
