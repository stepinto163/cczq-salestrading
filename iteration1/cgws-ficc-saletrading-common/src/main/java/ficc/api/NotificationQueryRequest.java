package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationQueryRequest */
public class NotificationQueryRequest extends PageRequest {
    public String readState;
    public List<String> types;
    public Filter filter;
}
