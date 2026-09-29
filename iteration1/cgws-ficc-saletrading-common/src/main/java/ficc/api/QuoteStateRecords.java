package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteStateRecords */
public class QuoteStateRecords extends ProtocolModel {
    public List<QuoteRecord> quotes;
    public List<Watchlist> watchlists;
    public List<FilterGroup> filterGroups;
}
