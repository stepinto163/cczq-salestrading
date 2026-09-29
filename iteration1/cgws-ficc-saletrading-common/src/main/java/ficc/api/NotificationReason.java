package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationReason */
public class NotificationReason extends ProtocolModel {
    public String type;
    public List<String> groupIds;
    public List<String> quoteIds;
    public String ownQuoteId;
    public String otherQuoteId;
    public String text;
}
