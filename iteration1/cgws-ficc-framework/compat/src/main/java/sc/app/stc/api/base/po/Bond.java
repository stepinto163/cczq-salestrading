// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import com.alibaba.fastjson.annotation.JSONField;

import java.math.BigDecimal;
import java.sql.Timestamp;

public class Bond {
    public String Institution_Rating = "";

    @JSONField(name = "Issuer_Code", serialize = true)
    public String Issuer_Code;

    @JSONField(name = "Is_Cross_Mkt", serialize = true)
    public String Is_Cross_Mkt;

    @JSONField(name = "Bond_ID", serialize = true)
    public String Bond_ID;

    @JSONField(name = "Bond_Key", serialize = true)
    public String Bond_Key;

    @JSONField(name = "Short_Name", serialize = true)
    public String Short_Name;

    @JSONField(name = "Bond_Subtype", serialize = true)
    public String Bond_Subtype;

    @JSONField(name = "Coupon_Rate_Current", serialize = true)
    public BigDecimal Coupon_Rate_Current;

    @JSONField(name = "Maturity_Date", serialize = true)
    public BigDecimal Maturity_Date;

    @JSONField(name = "Is_Municipal", serialize = true)
    public String Is_Municipal;

    @JSONField(name = "Issue_Year", serialize = true)
    public BigDecimal Issue_Year;

    @JSONField(name = "Issuer_Rating_Current_NPY", serialize = true)
    public String Issuer_Rating_Current_NPY;

    @JSONField(name = "Is_Mortgage", serialize = true)
    public String Is_Mortgage;

    @JSONField(name = "Listed_Date", serialize = true)
    public BigDecimal Listed_Date;

    @JSONField(name = "FRN_Index_ID", serialize = true)
    public String FRN_Index_ID;

    @JSONField(name = "Option_Type", serialize = true)
    public String Option_Type;

    @JSONField(name = "modify_date", serialize = true)
    public Timestamp modify_date;

    @JSONField(name = "modify_date2", serialize = true)
    public String modify_date2;

    @JSONField(name = "Ent_Cor", serialize = true)
    public String Ent_Cor;

    @JSONField(name = "Coupon_Type", serialize = true)
    public String Coupon_Type;

    @JSONField(name = "Bond_Type", serialize = true)
    public String Bond_Type;

    @JSONField(name = "Issuer_Rating_Current", serialize = true)
    public String Issuer_Rating_Current;

    @JSONField(name = "Delisted_Date", serialize = true)
    public BigDecimal Delisted_Date;

    @JSONField(name = "Option_Date", serialize = true)
    public BigDecimal Option_Date;
}
