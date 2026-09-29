package ficc.api;

import java.util.List;
import java.util.Map;

/** MarketData */
public class MarketData extends ProtocolModel {
    public String SecurityID;
    public String Symbol;
    public String MarketIndicator;
    public String VenueTypeGW;
    public String Venues;
    public List<MarketEntry> NoMDEntries;
}
