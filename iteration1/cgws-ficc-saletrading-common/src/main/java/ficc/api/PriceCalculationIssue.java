package ficc.api;

/** 单条已确认报价尚不能参与收益率排序；不影响同页其他报价，也不修改原报价。 */
public class PriceCalculationIssue extends ProtocolModel {
    public String quoteId;
    public String code;
}
