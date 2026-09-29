package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationEventBatch */
public class NotificationEventBatch extends ProtocolModel {
    public String Code;
    public String Msg;
    public String kind;
    public String epoch;
    public String sequence;
    public String currentSequence;
    public String fromSequence;
    public String toSequence;
    public String nextSequence;
    public String seenSequence;
    public Boolean hasMore;
    public List<NotificationEvent> events;
}
