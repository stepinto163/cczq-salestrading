package ficc.api;

import java.util.List;
import java.util.Map;

/** Confirmed-price projection for the authenticated stream/query user.
 * Visibility is applied before FULL/BEST/DEPTH selection. quoteRefs point to local
 * confirmed quote versions; neither pending proposals nor other users' QT conversations
 * participate in price selection. The internal service feed is privileged and unfiltered.
 */
public class PriceBook extends ProtocolModel {
    public String securityId;
    public String viewId;
    public String updateTime;
    public String bookRevision;
    public MarketData marketData;
    public Map<String,QuoteReference> quoteRefs;
}
