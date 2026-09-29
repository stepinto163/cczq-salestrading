package ficc.api;

import java.util.List;
import java.util.Map;

/** PriceBook */
public class PriceBook extends ProtocolModel {
    public String securityId;
    public String viewId;
    public String updateTime;
    public String bookRevision;
    public MarketData marketData;
    public Map<String,QuoteReference> quoteRefs;
}
