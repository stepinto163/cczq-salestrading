// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import com.alibaba.fastjson.annotation.JSONField;

import java.math.BigDecimal;
import java.sql.Timestamp;

public class TBondValuationView {
    @JSONField(name = "Id", serialize = true)
    public String Id = "";

    @JSONField(name = "Bond_ID", serialize = true)
    public String Bond_ID = "";

    @JSONField(name = "Listed_Market", serialize = true)
    public String Listed_Market = "";

    @JSONField(name = "remaining_year", serialize = true)
    public String remaining_year;

    @JSONField(name = "valuation_yield", serialize = true)
    public String valuation_yield;

    public String valuation_yield2;

    @JSONField(name = "valuation_net_price", serialize = true)
    public String valuation_net_price;

    public String valuation_net_price2;

    @JSONField(name = "valuation_date", serialize = true)
    public String valuation_date;

    @JSONField(name = "status", serialize = true)
    public String status = "";

    @JSONField(name = "modify_time", serialize = true)
    public Timestamp modify_time;

    @JSONField(name = "bond_key_listed_market", serialize = true)
    public String bond_key_listed_market;

    @JSONField(name = "Val_Modified_Duration", serialize = true)
    public BigDecimal Val_Modified_Duration;

    @JSONField(name = "Val_Convexity", serialize = true)
    public BigDecimal Val_Convexity;

    @JSONField(name = "Val_Basis_Point_Value", serialize = true)
    public BigDecimal Val_Basis_Point_Value;

    @JSONField(name = "bondcode", serialize = true)
    public String bondcode;

    @JSONField(name = "bondTypeFlag", serialize = true)
    public String bondTypeFlag;
}
