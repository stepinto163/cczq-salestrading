// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import com.alibaba.fastjson.annotation.JSONField;

import java.math.BigDecimal;
import java.sql.Timestamp;

public class BondListInfo {
    @JSONField(name = "ID", serialize = true)
    public String ID = "";

    @JSONField(name = "delflag", serialize = true)
    public String delflag = "";

    @JSONField(name = "create_date", serialize = true)
    public Timestamp create_date;

    @JSONField(name = "modify_date", serialize = true)
    public Timestamp modify_date;

    @JSONField(name = "modify_date2", serialize = true)
    public String modify_date2;

    @JSONField(name = "username", serialize = true)
    public String username;

    @JSONField(name = "inputer", serialize = true)
    public String inputer;

    @JSONField(name = "checker", serialize = true)
    public String checker;

    @JSONField(name = "sts", serialize = true)
    public String sts = "";

    @JSONField(name = "Bond_Key", serialize = true)
    public String Bond_Key;

    @JSONField(name = "Listed_Market", serialize = true)
    public String Listed_Market;

    @JSONField(name = "Bond_ID", serialize = true)
    public String Bond_ID;

    @JSONField(name = "Short_Name", serialize = true)
    public String Short_Name;

    @JSONField(name = "Is_Cross_Mkt", serialize = true)
    public String Is_Cross_Mkt;

    @JSONField(name = "Is_Mortgage", serialize = true)
    public String Is_Mortgage;

    @JSONField(name = "pinyin", serialize = true)
    public String pinyin;

    @JSONField(name = "pinyin_full", serialize = true)
    public String pinyin_full;

    @JSONField(name = "Listed_Date", serialize = true)
    public BigDecimal Listed_Date;

    @JSONField(name = "Delisted_Date", serialize = true)
    public BigDecimal Delisted_Date;

    @JSONField(name = "Mkt_Type", serialize = true)
    public String Mkt_Type;

    @JSONField(name = "Bond_Subtype", serialize = true)
    public String Bond_Subtype;

    public String toString() {
        return "BondListInfo [ID="
                + this.ID
                + ", delflag="
                + this.delflag
                + ", create_date="
                + this.create_date
                + ", modify_date="
                + this.modify_date
                + ", modify_date2="
                + this.modify_date2
                + ", username="
                + this.username
                + ", inputer="
                + this.inputer
                + ", checker="
                + this.checker
                + ", sts="
                + this.sts
                + ", Bond_Key="
                + this.Bond_Key
                + ", Listed_Market="
                + this.Listed_Market
                + ", Bond_ID="
                + this.Bond_ID
                + ", Short_Name="
                + this.Short_Name
                + ", Is_Cross_Mkt="
                + this.Is_Cross_Mkt
                + ", Is_Mortgage="
                + this.Is_Mortgage
                + ", pinyin="
                + this.pinyin
                + ", pinyin_full="
                + this.pinyin_full
                + ", Listed_Date="
                + this.Listed_Date
                + ", Delisted_Date="
                + this.Delisted_Date
                + ", Mkt_Type="
                + this.Mkt_Type
                + ", Bond_Subtype="
                + this.Bond_Subtype
                + "]";
    }

    public BondListInfo(
            String iD,
            String delflag,
            String create_date,
            String modify_date,
            String username,
            String inputer,
            String checker,
            String sts,
            String bond_Key,
            String listed_Market,
            String bond_ID,
            String short_Name,
            String is_Cross_Mkt,
            String is_Mortgage,
            String pinyin,
            String pinyin_full,
            BigDecimal listed_Date,
            BigDecimal delisted_Date,
            String mkt_Type,
            String bond_Subtype) {
        this.ID = iD;
        this.delflag = delflag;
        this.create_date = Timestamp.valueOf(create_date);
        this.modify_date = Timestamp.valueOf(modify_date);
        this.username = username;
        this.inputer = inputer;
        this.checker = checker;
        this.sts = sts;
        this.Bond_Key = bond_Key;
        this.Listed_Market = listed_Market;
        this.Bond_ID = bond_ID;
        this.Short_Name = short_Name;
        this.Is_Cross_Mkt = is_Cross_Mkt;
        this.Is_Mortgage = is_Mortgage;
        this.pinyin = pinyin;
        this.pinyin_full = pinyin_full;
        this.Listed_Date = listed_Date;
        this.Delisted_Date = delisted_Date;
        this.Mkt_Type = mkt_Type;
        this.Bond_Subtype = bond_Subtype;
    }

    public BondListInfo() {}
}
