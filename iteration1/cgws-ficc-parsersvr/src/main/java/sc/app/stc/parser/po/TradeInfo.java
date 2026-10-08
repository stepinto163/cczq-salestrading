package sc.app.stc.parser.po;

import lombok.Data;

@Data
public class TradeInfo {
    private String msgTime;
    private String securityID;
    private String symbol;
    private String bondShortName;
    private String securityType;
    private String optionDateShort;
    private String price;
    private String priceType;
    private String allocQty;
    private String settlType;
    private String tradeDate;
    private String settlDate;
    private String trdType;
    private String side;
    private String bridgePaymentFixedRate;
    private String allocText;
    private String partyName;
    private String realCounterPartyName;
    private String counterPartyName;
    private String counterPartySubName;
    private String oppSeatNO;
    private String trdSubType;
    private String fundAccount;
    private String trdMatchID;
    private String oppFundAccount;
    private String oppEntityNO;
    private String oppEntityFullName;
    private String oppEntityShortName;
    private String oppDealerNO;
    private String oppDealerShortName;
    private String oppTraderNO;
}
