package ficc.api;

import java.util.List;
import java.util.Map;

/** FilterSubgroup */
public class FilterSubgroup extends ProtocolModel {
    public String subgroupId;
    public String name;
    public Integer displayOrder;
    public List<FilterCondition> conditions;
}
