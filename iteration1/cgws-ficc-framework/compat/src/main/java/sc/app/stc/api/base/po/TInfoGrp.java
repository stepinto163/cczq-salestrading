// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import com.alibaba.fastjson.annotation.JSONField;

import java.sql.Timestamp;

public class TInfoGrp {
    @JSONField(name = "ID", serialize = true)
    public int ID = 0;

    @JSONField(name = "PARTY_CODE6", serialize = true)
    public String PARTY_CODE6 = "";

    @JSONField(name = "PARTY_CODE21", serialize = true)
    public String PARTY_CODE21 = "";

    @JSONField(name = "CN_FULL_NAME", serialize = true)
    public String CN_FULL_NAME;

    @JSONField(name = "CN_SHORT_NAME", serialize = true)
    public String CN_SHORT_NAME;

    @JSONField(name = "EN_FULL_NAME", serialize = true)
    public String EN_FULL_NAME;

    @JSONField(name = "EN_SHORT_NAME", serialize = true)
    public String EN_SHORT_NAME;

    @JSONField(name = "UPDATE_TIME", serialize = true)
    public Timestamp UPDATE_TIME = new Timestamp(System.currentTimeMillis());
}
