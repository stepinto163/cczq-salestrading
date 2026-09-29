package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteHistoryRecord */
public class QuoteHistoryRecord extends ProtocolModel {
    public String eventId;
    public String quoteId;
    public String revision;
    public String action;
    public String operatorUserId;
    public String operationTime;
    public QuoteRecord before;
    public QuoteRecord after;
    public String reason;
}
