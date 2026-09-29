package ficc.api;

import java.util.List;
import java.util.Map;

/** BaseQueryRequest */
public class BaseQueryRequest extends PageRequest {
    public String kind;
    public String text;
    public String institutionId;
    public Integer offset;
}
