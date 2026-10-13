package ficc.api;

import java.util.List;

/** 智能推荐协议；机构与交易员仅在证据归属本人时返回。 */
public class RecommendationRecord extends ProtocolModel {
    public String recommendationId;
    public String securityId;
    public String securityName;
    public String side;
    public Double score;
    public Integer quoteCount;
    public String institutionName;
    public String contactName;
    public String ownerUserId;
    public String latestPrice;
    public String quoteType;
    public String yieldType;
    public String latestQuantity;
    public String latestTime;
    public String feedback;
    public List<RecommendationEvidence> evidence;
}
