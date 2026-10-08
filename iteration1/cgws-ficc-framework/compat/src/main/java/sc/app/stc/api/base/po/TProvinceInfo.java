// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import com.alibaba.fastjson.annotation.JSONField;

import java.sql.Timestamp;

public class TProvinceInfo {
    @JSONField(name = "ID", serialize = true)
    public String ID = "";

    @JSONField(name = "delflag", serialize = true)
    public String delflag = "";

    @JSONField(name = "create_date", serialize = true)
    public Timestamp create_date;

    @JSONField(name = "modify_date", serialize = true)
    public Timestamp modify_date = new Timestamp(System.currentTimeMillis());

    @JSONField(name = "username", serialize = true)
    public String username;

    @JSONField(name = "inputer", serialize = true)
    public String inputer;

    @JSONField(name = "checker", serialize = true)
    public String checker;

    @JSONField(name = "sts", serialize = true)
    public String sts = "";

    @JSONField(name = "Area_Code", serialize = true)
    public String Area_Code;

    @JSONField(name = "Area_English_Name", serialize = true)
    public String Area_English_Name;

    @JSONField(name = "Area_Name", serialize = true)
    public String Area_Name;

    @JSONField(name = "Area_Level", serialize = true)
    public String Area_Level;

    @JSONField(name = "Area_Upper_Grade_Code", serialize = true)
    public String Area_Upper_Grade_Code;

    public String getID() {
        return this.ID;
    }

    public String getDelflag() {
        return this.delflag;
    }

    public Timestamp getCreate_date() {
        return this.create_date;
    }

    public Timestamp getModify_date() {
        return this.modify_date;
    }

    public String getUsername() {
        return this.username;
    }

    public String getInputer() {
        return this.inputer;
    }

    public String getChecker() {
        return this.checker;
    }

    public String getSts() {
        return this.sts;
    }

    public String getArea_Code() {
        return this.Area_Code;
    }

    public String getArea_English_Name() {
        return this.Area_English_Name;
    }

    public String getArea_Name() {
        return this.Area_Name;
    }

    public String getArea_Level() {
        return this.Area_Level;
    }

    public String getArea_Upper_Grade_Code() {
        return this.Area_Upper_Grade_Code;
    }

    public void setID(String ID) {
        this.ID = ID;
    }

    public void setDelflag(String delflag) {
        this.delflag = delflag;
    }

    public void setCreate_date(Timestamp create_date) {
        this.create_date = create_date;
    }

    public void setModify_date(Timestamp modify_date) {
        this.modify_date = modify_date;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public void setInputer(String inputer) {
        this.inputer = inputer;
    }

    public void setChecker(String checker) {
        this.checker = checker;
    }

    public void setSts(String sts) {
        this.sts = sts;
    }

    public void setArea_Code(String Area_Code) {
        this.Area_Code = Area_Code;
    }

    public void setArea_English_Name(String Area_English_Name) {
        this.Area_English_Name = Area_English_Name;
    }

    public void setArea_Name(String Area_Name) {
        this.Area_Name = Area_Name;
    }

    public void setArea_Level(String Area_Level) {
        this.Area_Level = Area_Level;
    }

    public void setArea_Upper_Grade_Code(String Area_Upper_Grade_Code) {
        this.Area_Upper_Grade_Code = Area_Upper_Grade_Code;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof TProvinceInfo)) {
            return false;
        }
        TProvinceInfo other = (TProvinceInfo) o;
        if (!other.canEqual(this)) {
            return false;
        }
        String this$ID = this.getID();
        String other$ID = other.getID();
        if (this$ID == null ? other$ID != null : !this$ID.equals(other$ID)) {
            return false;
        }
        String this$delflag = this.getDelflag();
        String other$delflag = other.getDelflag();
        if (this$delflag == null ? other$delflag != null : !this$delflag.equals(other$delflag)) {
            return false;
        }
        Timestamp this$create_date = this.getCreate_date();
        Timestamp other$create_date = other.getCreate_date();
        if (this$create_date == null
                ? other$create_date != null
                : !((Object) this$create_date).equals(other$create_date)) {
            return false;
        }
        Timestamp this$modify_date = this.getModify_date();
        Timestamp other$modify_date = other.getModify_date();
        if (this$modify_date == null
                ? other$modify_date != null
                : !((Object) this$modify_date).equals(other$modify_date)) {
            return false;
        }
        String this$username = this.getUsername();
        String other$username = other.getUsername();
        if (this$username == null
                ? other$username != null
                : !this$username.equals(other$username)) {
            return false;
        }
        String this$inputer = this.getInputer();
        String other$inputer = other.getInputer();
        if (this$inputer == null ? other$inputer != null : !this$inputer.equals(other$inputer)) {
            return false;
        }
        String this$checker = this.getChecker();
        String other$checker = other.getChecker();
        if (this$checker == null ? other$checker != null : !this$checker.equals(other$checker)) {
            return false;
        }
        String this$sts = this.getSts();
        String other$sts = other.getSts();
        if (this$sts == null ? other$sts != null : !this$sts.equals(other$sts)) {
            return false;
        }
        String this$Area_Code = this.getArea_Code();
        String other$Area_Code = other.getArea_Code();
        if (this$Area_Code == null
                ? other$Area_Code != null
                : !this$Area_Code.equals(other$Area_Code)) {
            return false;
        }
        String this$Area_English_Name = this.getArea_English_Name();
        String other$Area_English_Name = other.getArea_English_Name();
        if (this$Area_English_Name == null
                ? other$Area_English_Name != null
                : !this$Area_English_Name.equals(other$Area_English_Name)) {
            return false;
        }
        String this$Area_Name = this.getArea_Name();
        String other$Area_Name = other.getArea_Name();
        if (this$Area_Name == null
                ? other$Area_Name != null
                : !this$Area_Name.equals(other$Area_Name)) {
            return false;
        }
        String this$Area_Level = this.getArea_Level();
        String other$Area_Level = other.getArea_Level();
        if (this$Area_Level == null
                ? other$Area_Level != null
                : !this$Area_Level.equals(other$Area_Level)) {
            return false;
        }
        String this$Area_Upper_Grade_Code = this.getArea_Upper_Grade_Code();
        String other$Area_Upper_Grade_Code = other.getArea_Upper_Grade_Code();
        return !(this$Area_Upper_Grade_Code == null
                ? other$Area_Upper_Grade_Code != null
                : !this$Area_Upper_Grade_Code.equals(other$Area_Upper_Grade_Code));
    }

    protected boolean canEqual(Object other) {
        return other instanceof TProvinceInfo;
    }

    public int hashCode() {
        int PRIME = 59;
        int result = 1;
        String $ID = this.getID();
        result = result * 59 + ($ID == null ? 43 : $ID.hashCode());
        String $delflag = this.getDelflag();
        result = result * 59 + ($delflag == null ? 43 : $delflag.hashCode());
        Timestamp $create_date = this.getCreate_date();
        result = result * 59 + ($create_date == null ? 43 : ((Object) $create_date).hashCode());
        Timestamp $modify_date = this.getModify_date();
        result = result * 59 + ($modify_date == null ? 43 : ((Object) $modify_date).hashCode());
        String $username = this.getUsername();
        result = result * 59 + ($username == null ? 43 : $username.hashCode());
        String $inputer = this.getInputer();
        result = result * 59 + ($inputer == null ? 43 : $inputer.hashCode());
        String $checker = this.getChecker();
        result = result * 59 + ($checker == null ? 43 : $checker.hashCode());
        String $sts = this.getSts();
        result = result * 59 + ($sts == null ? 43 : $sts.hashCode());
        String $Area_Code = this.getArea_Code();
        result = result * 59 + ($Area_Code == null ? 43 : $Area_Code.hashCode());
        String $Area_English_Name = this.getArea_English_Name();
        result = result * 59 + ($Area_English_Name == null ? 43 : $Area_English_Name.hashCode());
        String $Area_Name = this.getArea_Name();
        result = result * 59 + ($Area_Name == null ? 43 : $Area_Name.hashCode());
        String $Area_Level = this.getArea_Level();
        result = result * 59 + ($Area_Level == null ? 43 : $Area_Level.hashCode());
        String $Area_Upper_Grade_Code = this.getArea_Upper_Grade_Code();
        result =
                result * 59
                        + ($Area_Upper_Grade_Code == null ? 43 : $Area_Upper_Grade_Code.hashCode());
        return result;
    }

    public String toString() {
        return "TProvinceInfo(ID="
                + this.getID()
                + ", delflag="
                + this.getDelflag()
                + ", create_date="
                + this.getCreate_date()
                + ", modify_date="
                + this.getModify_date()
                + ", username="
                + this.getUsername()
                + ", inputer="
                + this.getInputer()
                + ", checker="
                + this.getChecker()
                + ", sts="
                + this.getSts()
                + ", Area_Code="
                + this.getArea_Code()
                + ", Area_English_Name="
                + this.getArea_English_Name()
                + ", Area_Name="
                + this.getArea_Name()
                + ", Area_Level="
                + this.getArea_Level()
                + ", Area_Upper_Grade_Code="
                + this.getArea_Upper_Grade_Code()
                + ")";
    }
}
