package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteStreamBatch */
public class QuoteStreamBatch extends StreamMessage {
    public List<QuoteRecord> quotes;
    public List<QuoteReport> reports;
    public List<ViewDelta> views;
    public List<ReferenceRecord> reference;
}
