package ficc.api;

import java.util.List;
import java.util.Map;

/** ViewDelta */
public class ViewDelta extends ProtocolModel {
    public String viewId;
    public Integer totalCount;
    public List<String> upsertIds;
    public List<String> removeIds;
}
