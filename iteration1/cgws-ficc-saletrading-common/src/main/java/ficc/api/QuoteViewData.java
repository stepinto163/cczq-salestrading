package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteViewData */
public class QuoteViewData extends ProtocolModel {
    public String streamId;
    public String sequence;
    public ViewPage view;
    public List<QuoteRecord> quotes;
    public List<ReferenceRecord> reference;
}
