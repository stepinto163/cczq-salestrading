package ficc.api;

import java.util.List;
import java.util.Map;

/** NotificationPreferenceSaveRequest */
public class NotificationPreferenceSaveRequest extends Request {
    public String expectedRevision;
    public Boolean watchlistEnabled;
    public Boolean filterGroupEnabled;
    public Boolean counterQuoteEnabled;
    public Boolean myBestEnabled;
}
