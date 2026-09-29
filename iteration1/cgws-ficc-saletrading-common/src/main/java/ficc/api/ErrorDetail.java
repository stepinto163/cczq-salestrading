package ficc.api;

import java.util.List;
import java.util.Map;

/** ErrorDetail */
public class ErrorDetail extends ProtocolModel {
    public String type;
    public String field;
    public String quoteId;
    public String expectedRevision;
    public String currentRevision;
}
