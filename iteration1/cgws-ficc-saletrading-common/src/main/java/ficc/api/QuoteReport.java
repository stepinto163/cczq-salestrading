package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteReport */
public class QuoteReport extends ProtocolModel {
    public String quoteId;
    public String revision;
    public String action;
    public String requestId;
    public String clientQuoteId;
    public QuoteRecord quote;
    public String eventId;
    public String operatorUserId;
    public String operationTime;
}
