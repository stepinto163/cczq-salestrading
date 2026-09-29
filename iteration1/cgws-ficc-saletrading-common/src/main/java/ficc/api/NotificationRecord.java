package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationRecord */
public class NotificationRecord extends ProtocolModel {
    public String notificationId;
    public String userId;
    public String securityId;
    public String content;
    public String contentToken;
    public String alertKey;
    public Boolean active;
    public String revision;
    public String createTime;
    public String updateTime;
    public String readTime;
    public List<NotificationReason> reasons;
    public List<String> quoteIds;
}
