package ficc.api;

import java.util.List;
import java.util.Map;

/** ViewPage */
public class ViewPage extends ProtocolModel {
    public String viewId;
    public Integer pageNumber;
    public Integer pageSize;
    public Integer totalCount;
    public List<String> ids;
    public Boolean hasMore;
}
