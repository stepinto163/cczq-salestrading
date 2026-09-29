package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationStreamBatch */
public class NotificationStreamBatch extends StreamMessage {
    public List<NotificationRecord> notifications;
    public List<ViewDelta> views;
    public NotificationPreference preference;
}
