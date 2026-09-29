package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteEvent */
public class QuoteEvent extends ProtocolModel {
    public String sequence;
    public String eventId;
    public String type;
    public String action;
    public String quoteId;
    public QuoteRecord quote;
    public SettingRecord record;
    public String requestId;
}
