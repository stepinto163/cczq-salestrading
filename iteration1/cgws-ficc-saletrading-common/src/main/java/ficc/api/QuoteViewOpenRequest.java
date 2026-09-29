package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteViewOpenRequest */
public class QuoteViewOpenRequest extends Request {
    public String viewId;
    public Integer pageSize;
    public String ownerScope;
    public List<String> status;
    public Filter filter;
}
