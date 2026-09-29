package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteHistorySearchRequest */
public class QuoteHistorySearchRequest extends PageRequest {
    public String fromDate;
    public String toDate;
    public String ownerScope;
    public Filter filter;
}
