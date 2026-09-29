package ficc.api;

import java.util.List;
import java.util.Map;

/** PriceStreamBatch */
public class PriceStreamBatch extends StreamMessage {
    public List<PriceBook> books;
    public List<ViewDelta> views;
}
