package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteStateData */
public class QuoteStateData extends ProtocolModel {
    public String epoch;
    public String sequence;
    public Integer totalCount;
    public QuoteStateRecords records;
    public String nextCursor;
    public Boolean done;
}
