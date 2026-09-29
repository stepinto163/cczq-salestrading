package ficc.api;

import java.util.List;
import java.util.Map;

/** 筛选条件；value 按 operator/字段字典为标量或数组。 */
public class FilterCondition extends ProtocolModel {
    public String conditionId;
    public String field;
    public String operator;
    public Object value;
    public Integer displayOrder;
}
