package ficc.api;

import java.util.List;
import java.util.Map;

/** MarketEntry */
public class MarketEntry extends ProtocolModel {
    public String MDEntryID;
    public String MDEntryType;
    public String MDEntryPx;
    public String MDEntrySize;
    public String PriceType;
    public String ExerciseFlag;
    /** 本方报价所有者的 STC userid；客户端解析其 STC 姓名，显示为“长城交易员”。 */
    public String UserID;
    public String MDPriceLevel;
    public String SettlType;
    public String MDEntryPositionNo;
    public String MDEntryDate;
    public String MDEntryTime;
    public String Yield;
    public String YieldToExecution;
    public String InstitutionID;
    /** 报价对手机构名称；与 InstitutionID 使用相同的可见性规则。 */
    public String InstitutionName;
    public String TraderID;
    /** 报价对手联系人姓名，不是 UserID 对应的本方报价归属人。 */
    public String TraderName;
}
