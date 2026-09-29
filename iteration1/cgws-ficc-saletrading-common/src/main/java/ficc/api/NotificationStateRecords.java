package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationStateRecords */
public class NotificationStateRecords extends ProtocolModel {
    public List<NotificationRecord> notifications;
    public List<NotificationPreference> preferences;
    public List<FilterGroup> filterGroups;
}
