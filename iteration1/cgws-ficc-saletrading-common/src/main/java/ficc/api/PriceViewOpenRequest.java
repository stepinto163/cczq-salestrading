package ficc.api;

import java.util.List;
import java.util.Map;

/** PriceViewOpenRequest */
public class PriceViewOpenRequest extends Request {
    public String viewId;
    public Integer pageSize;
    public String mode;
    public Integer depth;
    public Filter filter;
}
