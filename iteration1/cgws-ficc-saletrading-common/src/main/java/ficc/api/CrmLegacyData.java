package ficc.api;

import java.util.List;
import java.util.Map;

/** CrmLegacyData */
public class CrmLegacyData extends ProtocolModel {
    public String Code;
    public String Msg;
    public Integer totalSize;
    public Integer pageSize;
    public List<CrmLegacyRecord> datas;
}
