package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteSource */
public class QuoteSource extends ProtocolModel {
    public String type;
    public String originService;
    public String sourceUserId;
    public String ownerUserId;
    public String externalMessageId;
    public String originalMessageId;
    /** QTrade QuoteID: sender's intention ID; -10000 means unavailable on a receiver message. */
    public String externalQuoteId;
    /** QTrade NegotiationID: bilateral conversation; one intention can have several. */
    public String negotiationId;
    public String counterpartyId;
    public String chatType;
    public String chatGroupId;
    public String externalRevision;
    public String sourceTime;
    public String transType;
    public String text;
}
