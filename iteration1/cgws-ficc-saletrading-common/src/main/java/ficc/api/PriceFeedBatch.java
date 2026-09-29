package ficc.api;

import java.util.List;
import java.util.Map;

/** PriceFeedBatch */
public class PriceFeedBatch extends ProtocolModel {
    public String Code;
    public String Msg;
    public String kind;
    public String epoch;
    public String sequence;
    public String currentSequence;
    public String streamId;
    public EventPosition quotePosition;
    public List<PriceBook> prices;
}
