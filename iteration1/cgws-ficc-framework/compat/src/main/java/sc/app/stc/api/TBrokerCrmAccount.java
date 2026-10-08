// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import com.alibaba.fastjson.annotation.JSONField;

import java.util.Objects;

public class TBrokerCrmAccount {
    @JSONField(name = "idb_key", serialize = true)
    public String idbkey;

    @JSONField(name = "crm_id", serialize = true)
    public String crm_id;

    @JSONField(name = "accountId", serialize = true)
    public String account_id;

    @JSONField(name = "account", serialize = true)
    public String account;

    @JSONField(name = "accountName", serialize = true)
    public String account_name;

    @JSONField(name = "serialNumber", serialize = true)
    public String serial_number;

    @JSONField(name = "roleCode", serialize = true)
    public String role_code;

    @JSONField(name = "password", serialize = true)
    public String password;

    @JSONField(name = "qq", serialize = true)
    public String qq;

    @JSONField(name = "createtime", serialize = true)
    public String createtime;

    @JSONField(name = "updatetime", serialize = true)
    public String updatetime;

    @JSONField(name = "idcCreatetime", serialize = true)
    public String idc_createtime;

    @JSONField(name = "idcUpdatetime", serialize = true)
    public String idc_updatetime;

    @JSONField(name = "term_ids", serialize = true)
    public String term_ids;

    @JSONField(name = "prods", serialize = true)
    public String prods;

    @JSONField(name = "infostatus", serialize = true)
    public String infostatus;

    public String toString() {
        return "TBrokerCrmAccount [idbkey="
                + this.idbkey
                + ", crm_id="
                + this.crm_id
                + ", account_id="
                + this.account_id
                + ", account="
                + this.account
                + ", account_name="
                + this.account_name
                + ", serial_number="
                + this.serial_number
                + ", role_code="
                + this.role_code
                + ", password="
                + this.password
                + ", qq="
                + this.qq
                + ", createtime="
                + this.createtime
                + ", updatetime="
                + this.updatetime
                + ", idc_createtime="
                + this.idc_createtime
                + ", idc_updatetime="
                + this.idc_updatetime
                + ", term_ids="
                + this.term_ids
                + ", prods="
                + this.prods
                + ", infostatus="
                + this.infostatus
                + "]";
    }

    public boolean equals(Object o) {
        if (this == o) {
            return true;
        }
        if (o == null || this.getClass() != o.getClass()) {
            return false;
        }
        TBrokerCrmAccount that = (TBrokerCrmAccount) o;
        return Objects.equals(this.idbkey, that.idbkey)
                && Objects.equals(this.account, that.account)
                && Objects.equals(this.account_name, that.account_name)
                && Objects.equals(this.qq, that.qq)
                && Objects.equals(this.infostatus, that.infostatus);
    }

    public int hashCode() {
        return Objects.hash(this.idbkey, this.account, this.account_name, this.qq, this.infostatus);
    }
}
