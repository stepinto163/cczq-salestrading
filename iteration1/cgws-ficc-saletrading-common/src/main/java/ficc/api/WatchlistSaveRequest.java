package ficc.api;

import java.util.List;
import java.util.Map;

/** WatchlistSaveRequest */
public class WatchlistSaveRequest extends Request {
    public String watchlistId;
    public String name;
    public String market;
    public String scope;
    public String expectedRevision;
    public List<WatchlistSecurity> securities;
}
