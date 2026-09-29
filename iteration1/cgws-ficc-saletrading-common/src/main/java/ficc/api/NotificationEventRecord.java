package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationEventRecord */
public class NotificationEventRecord extends NotificationRecord {
    public String filterGroupId;
    public String name;
    public String market;
    public String scope;
    public List<FilterSubgroup> subgroups;
    public Boolean watchlistEnabled;
    public Boolean filterGroupEnabled;
    public Boolean counterQuoteEnabled;
    public Boolean myBestEnabled;
}
