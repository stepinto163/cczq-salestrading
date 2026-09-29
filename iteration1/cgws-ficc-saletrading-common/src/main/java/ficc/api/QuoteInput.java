package ficc.api;

import java.util.List;
import java.util.Map;

/** 可编辑报价字段。代码使用 STC 字典值；金额、价格使用十进制字符串。 */
public class QuoteInput extends ProtocolModel {
    public String securityId;
    public String institutionId;
    public String institutionName;
    public String contactId;
    public String contactName;
    public String side;
    public String quoteType;
    public String yieldType;
    public String price;
    public String quantity;
    public String quantityUnit;
    public String currency;
    public String tradeDate;
    public String settlType;
    public Boolean urgent;
    public Boolean recommended;
    public Boolean whole;
    public Boolean oco;
    public Boolean packing;
    public String remark;
}
