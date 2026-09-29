package ficc.api;

import java.util.List;
import java.util.Map;

/** CrmSearchRequest */
public class CrmSearchRequest extends Request {
    public String kind;
    public String text;
    public String institutionId;
    public Integer offset;
    public Integer limit;
}
