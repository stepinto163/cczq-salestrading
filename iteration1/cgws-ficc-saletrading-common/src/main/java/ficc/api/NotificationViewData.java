package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationViewData */
public class NotificationViewData extends ProtocolModel {
    public String streamId;
    public String sequence;
    public ViewPage view;
    public List<NotificationRecord> notifications;
    public NotificationPreference preference;
}
