// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import com.alibaba.fastjson.annotation.JSONField;

import java.sql.Timestamp;
import java.util.Date;

public class Holiday {
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

    @JSONField(name = "country", serialize = true)
    public String country;

    @JSONField(name = "market_type", serialize = true)
    public String market_type;

    @JSONField(name = "holiday_date", serialize = true)
    public Date holiday_date;

    @JSONField(name = "holiday_reason", serialize = true)
    public String holiday_reason;

    public String toString() {
        return "Holiday [ID="
                + this.ID
                + ", delflag="
                + this.delflag
                + ", create_date="
                + this.create_date
                + ", modify_date="
                + this.modify_date
                + ", username="
                + this.username
                + ", inputer="
                + this.inputer
                + ", checker="
                + this.checker
                + ", sts="
                + this.sts
                + ", country="
                + this.country
                + ", market_type="
                + this.market_type
                + ", holiday_date="
                + this.holiday_date
                + ", holiday_reason="
                + this.holiday_reason
                + "]";
    }
}
