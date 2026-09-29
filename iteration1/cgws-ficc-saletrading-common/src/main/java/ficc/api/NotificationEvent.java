package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationEvent */
public class NotificationEvent extends ProtocolModel {
    public String sequence;
    public String eventId;
    public String type;
    public String action;
    public String notificationId;
    public NotificationEventRecord record;
}
