package ficc.api;

import java.util.List;
import java.util.Map;

/** EventQueryRequest */
public class EventQueryRequest extends Request {
    public String epoch;
    public String afterSequence;
    public Integer limit;
}
