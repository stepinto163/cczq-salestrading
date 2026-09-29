package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationReadRequest */
public class NotificationReadRequest extends Request {
    public List<String> notificationIds;
    public Boolean read;
}
