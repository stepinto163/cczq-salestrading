package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteHistoryRow */
public class QuoteHistoryRow extends ProtocolModel {
    public String tradeDate;
    public QuoteRecord quote;
    public ReferenceRecord reference;
}
