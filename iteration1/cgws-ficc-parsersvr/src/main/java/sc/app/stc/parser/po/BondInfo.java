package sc.app.stc.parser.po;

/**
 * 债券信息
 *
 * @author lemon
 **/
public class BondInfo {
    public String Bond_Id;
    public String Short_Name;
    public String Bond_Key;
    public String listed_market;
    public String Delisted_Date;

    public String getBond_Id() {
        return Bond_Id;
    }

    public void setBond_Id(String bond_Id) {
        Bond_Id = bond_Id;
    }

    public String getShort_Name() {
        return Short_Name;
    }

    public void setShort_Name(String short_Name) {
        Short_Name = short_Name;
    }

    public String getBond_Key() {
        return Bond_Key;
    }

    public void setBond_Key(String bond_Key) {
        Bond_Key = bond_Key;
    }

    public String getListed_market() {
        return listed_market;
    }

    public void setListed_market(String listed_market) {
        this.listed_market = listed_market;
    }

    public String getDelisted_Date() {
        return Delisted_Date;
    }

    public void setDelisted_Date(String delisted_Date) {
        Delisted_Date = delisted_Date;
    }
}

