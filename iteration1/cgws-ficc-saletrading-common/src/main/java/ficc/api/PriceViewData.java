package ficc.api;

import java.util.List;
import java.util.Map;

/** PriceViewData */
public class PriceViewData extends ProtocolModel {
    public String streamId;
    public String sequence;
    public ViewPage view;
    public List<PriceBook> books;
}
