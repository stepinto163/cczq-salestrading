package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteCreateRequest */
public class QuoteCreateRequest extends Request {
    public List<QuoteCreateItem> quotes;
    public IntakeIdentity intake;
}
