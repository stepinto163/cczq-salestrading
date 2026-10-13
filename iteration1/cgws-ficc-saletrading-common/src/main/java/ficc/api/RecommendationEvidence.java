package ficc.api;

import java.util.List;

/** 智能推荐协议；机构与交易员仅在证据归属本人时返回。 */
public class RecommendationEvidence extends ProtocolModel {
    public String quoteId;
    public String ownerUserId;
    public String securityId;
    public String securityName;
    public String side;
    public String price;
    public String quoteType;
    public String yieldType;
    public String quantity;
    public String updateTime;
    public String institutionName;
    public String contactName;
}
