package ficc.api;

import java.util.List;
import java.util.Map;

/** WatchlistData */
public class WatchlistData extends ProtocolModel {
    public Watchlist watchlist;
    public List<Watchlist> watchlists;
    public String watchlistId;
    public Boolean deleted;
}
