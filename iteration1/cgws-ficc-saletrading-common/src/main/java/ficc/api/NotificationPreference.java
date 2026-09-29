package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationPreference */
public class NotificationPreference extends ProtocolModel {
    public String revision;
    public Boolean watchlistEnabled;
    public Boolean filterGroupEnabled;
    public Boolean counterQuoteEnabled;
    public Boolean myBestEnabled;
    public String userId;
    public String updateTime;
}
