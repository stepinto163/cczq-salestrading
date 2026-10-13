package ficc.api;

import java.util.List;

/** 智能推荐协议；机构与交易员仅在证据归属本人时返回。 */
public class RecommendationQueryData extends ProtocolModel {
    public String securityId;
    public String startDate;
    public String endDate;
    public String scoringVersion;
    public List<RecommendationRecord> recommendations;
}
