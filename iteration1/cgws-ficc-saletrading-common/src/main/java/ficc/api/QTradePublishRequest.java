package ficc.api;

import java.util.List;

/** 机构价格广播；普通用户的发送账号须匹配 inf1，可信内部调用使用现有服务凭据。 */
public final class QTradePublishRequest extends Request {
    /** QTrade MsgID(18044)，9～30 位；机构内唯一，调用方用 appId、时间和随机数生成。 */
    public String messageId;
    public String senderQtAccount;
    /** 仅内部服务/自动化使用现有 intake.token；普通客户端用 DPS 登录身份，不填此项。 */
    public String token;
    /** TEXT 或 STRUCTURED。文本通过 QTrade NLP，结构化报价直接发送。 */
    public String mode;
    public String preText;
    public String postText;
    public List<Recipient> recipients;
    public List<Quote> quotes;

    public static final class Recipient extends ProtocolModel {
        /** FIX RoutingType：52 加密QT，53 QT群，58 QT号，59 QT/QQ号或加密ID。 */
        public Integer routingType;
        public String routingId;
    }

    public static final class Quote extends ProtocolModel {
        /** ClQuoteID(18031)，调用方的意向标识，不是 QuoteSvr 内部 quoteId。 */
        public String clientQuoteId;
        /** 不带市场后缀的代码；市场由 exchange 明确指定。 */
        public String securityId;
        public String exchange;
        public String symbol;
        /** 1 Bid，2 Ofr，y 双边。 */
        public String side;
        public Integer bidPriceType;
        public String bidPrice;
        public String bidQuantityYuan;
        public String bidTradeDate;
        public String bidSettlementType;
        public Integer offerPriceType;
        public String offerPrice;
        public String offerQuantityYuan;
        public String offerTradeDate;
        public String offerSettlementType;
        public String remark;
    }
}
