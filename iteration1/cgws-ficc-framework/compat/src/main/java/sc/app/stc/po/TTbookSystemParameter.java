// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.po;

import com.alibaba.fastjson.annotation.JSONField;

public class TTbookSystemParameter {
    @JSONField(name = "param_key", serialize = true)
    public String param_key = "";

    @JSONField(name = "username", serialize = true)
    public String username = "";

    @JSONField(name = "param_value", serialize = true)
    public String param_value;

    @JSONField(name = "param_desc", serialize = true)
    public String param_desc;

    @JSONField(name = "createtime", serialize = true)
    public String createtime;

    @JSONField(name = "updatetime", serialize = true)
    public String updatetime;
}
