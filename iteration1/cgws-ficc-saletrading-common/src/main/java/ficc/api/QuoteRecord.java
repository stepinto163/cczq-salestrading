package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteRecord */
public class QuoteRecord extends QuoteInput {
    public String quoteId;
    public String clonedFromQuoteId;
    public String clientQuoteId;
    public String sourceMessageId;
    public QuoteSource source;
    public String ownerUserId;
    public String status;
    public String revision;
    public String createdBy;
    public String updatedBy;
    public String confirmedBy;
    public String createTime;
    public String updateTime;
    public String quoteTime;
    public String confirmTime;
}
