package ficc.api;

import java.util.List;
import java.util.Map;

/** PriceQueryRequest */
public class PriceQueryRequest extends PageRequest {
    public List<String> securityIds;
    public String mode;
    public Integer depth;
    public Filter filter;
}
