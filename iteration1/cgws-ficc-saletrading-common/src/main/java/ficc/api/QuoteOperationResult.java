package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteOperationResult */
public class QuoteOperationResult extends ProtocolModel {
    public String quoteId;
    public String clientQuoteId;
    public String revision;
    public String Code;
    public String Msg;
    public ErrorDetail error;
}
