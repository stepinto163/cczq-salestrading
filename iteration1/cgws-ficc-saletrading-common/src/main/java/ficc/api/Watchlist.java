package ficc.api;

import java.util.List;
import java.util.Map;

/** Watchlist */
public class Watchlist extends ProtocolModel {
    public String watchlistId;
    public String name;
    public String market;
    public String scope;
    public String revision;
    public String userId;
    public String createTime;
    public String updateTime;
    public List<WatchlistSecurity> securities;
}
