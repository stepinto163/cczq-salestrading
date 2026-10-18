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
    /** 合并展示行中的独立触发实例；不能用合并行 ID 判断是否应弹窗。 */
    public List<Trigger> triggers;

    public static class Trigger extends ProtocolModel {
        public String alertKey;
        public String type;
        public List<String> quoteIds;
        public List<QuoteRecord> quotes;
    }
}
