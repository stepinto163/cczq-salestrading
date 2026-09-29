package ficc.api;

import java.util.List;
import java.util.Map;

/** 共享筛选 DSL；字段与枚举见 product-filter-contract.zh-CN。 */
public class Filter extends ProtocolModel {
    public List<FilterCondition> conditions;
    public String market;
    public List<String> watchlistIds;
    public List<String> filterGroupIds;
}
