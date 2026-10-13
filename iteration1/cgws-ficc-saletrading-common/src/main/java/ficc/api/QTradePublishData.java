package ficc.api;

import java.util.ArrayList;
import java.util.List;

/** FIX 发送不等于业务成功；仅 U29 code=0 表示 QTrade 全部受理成功。 */
public final class QTradePublishData extends ProtocolModel {
    public String messageId;
    /** ACKNOWLEDGED / PARTIAL / REJECTED / UNKNOWN。UNKNOWN 不可自动重发。 */
    public String status;
    public String applicationCode;
    public Integer fixSequence;
    public String description;
    public List<RecipientResult> recipients = new ArrayList<>();

    public static final class RecipientResult extends ProtocolModel {
        public Integer routingType;
        public String routingId;
        public String description;
        public Integer errorType;
        public String clientQuoteId;
    }
}
