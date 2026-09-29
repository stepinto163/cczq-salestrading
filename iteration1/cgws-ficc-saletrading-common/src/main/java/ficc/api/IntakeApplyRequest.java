package ficc.api;

import java.util.List;
import java.util.Map;

/** IntakeApplyRequest */
public class IntakeApplyRequest extends Request {
    public String sourceType;
    public String externalMessageId;
    public String sourceUserId;
    public String sourceTime;
    public String text;
    public List<IntakeQuoteItem> quotes;
}
