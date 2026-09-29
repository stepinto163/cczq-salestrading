package ficc.api;

import java.util.List;
import java.util.Map;

/** FilterGroupSaveRequest */
public class FilterGroupSaveRequest extends Request {
    public String filterGroupId;
    public String expectedRevision;
    public String name;
    public String market;
    public String scope;
    public List<FilterSubgroup> subgroups;
}
