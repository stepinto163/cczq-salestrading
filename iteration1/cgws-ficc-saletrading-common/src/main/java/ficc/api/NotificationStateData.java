package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationStateData */
public class NotificationStateData extends ProtocolModel {
    public String epoch;
    public String sequence;
    public Integer totalCount;
    public NotificationStateRecords records;
    public String nextCursor;
    public Boolean done;
}
