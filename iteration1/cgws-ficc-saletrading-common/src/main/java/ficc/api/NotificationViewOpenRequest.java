package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationViewOpenRequest */
public class NotificationViewOpenRequest extends Request {
    public String viewId;
    public Integer pageSize;
    public String readState;
    public List<String> types;
    public Filter filter;
}
