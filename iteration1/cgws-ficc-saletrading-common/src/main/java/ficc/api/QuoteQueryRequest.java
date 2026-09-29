package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteQueryRequest */
public class QuoteQueryRequest extends PageRequest {
    public String ownerScope;
    public List<String> status;
    public Filter filter;
}
