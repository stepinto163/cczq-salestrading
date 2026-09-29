package ficc.api;

import java.util.List;
import java.util.Map;

/** Reply */
public class Reply extends ProtocolModel {
    public String Code;
    public String Msg;
    public String requestId;
    public ErrorDetail error;
}
