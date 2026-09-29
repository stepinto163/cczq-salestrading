package ficc.api;

import java.util.List;
import java.util.Map;

/** FilterGroup */
public class FilterGroup extends ProtocolModel {
    public String filterGroupId;
    public String name;
    public String market;
    public String scope;
    public String revision;
    public String userId;
    public String createTime;
    public String updateTime;
    public List<FilterSubgroup> subgroups;
}
