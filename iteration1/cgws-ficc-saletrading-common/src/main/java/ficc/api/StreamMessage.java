package ficc.api;

import java.util.List;
import java.util.Map;

/** StreamMessage */
public class StreamMessage extends ProtocolModel {
    public String Code;
    public String Msg;
    public String kind;
    public String streamId;
    public String sequence;
    public String reason;
}
