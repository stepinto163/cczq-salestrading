package ficc.api;

import java.util.List;
import java.util.Map;

/**
 * 报价的上游来源信息。下面注明 FIX tag 的解释适用于 QT 来源；其他来源可不提供这些字段。
 * 本地报价主键是 QuoteRecord.quoteId（独立生成的 Q-...），本地原文记录主键是
 * QuoteRecord.sourceMessageId（SM-...）；两者均不等同于这里的上游标识或推导键。
 */
public class QuoteSource extends ProtocolModel {
    public String type;
    public String originService;

    /** 本方渠道账号。QT 对应 OwnQtNO(18004)，当前接收链路用于识别接收账号并解析内部归属人；不是对手账号。 */
    public String sourceUserId;

    /** 内部归属交易员的 STC userid。STC 模式通过 stc_tbook_users.inf1 匹配 sourceUserId 后取得 userid。 */
    public String ownerUserId;

    /**
     * 来源事件标识。当前 QtIntake 按本方账号、意向/会话、来源时间和事件类型生成稳定哈希，
     * 用于派生 requestId/clientQuoteId 并识别重复事件；不是 FIX MsgSeqNum，也不是本地 quoteId。
     */
    public String externalMessageId;

    /**
     * 原始消息分组键，用于将同一原文拆出的多条报价归组。当前 QT Quote(S) 没有原生原文消息 ID，
     * 因而按账号、时间、动作、发送方、会话/群与原文推导 QT-MESSAGE-...；无原文时退到 QT-EVENT-...。
     * 这是推导键；QuoteSvr 保存分组时还按本方账号及内部归属人隔离，不等同于聊天群 ID 或 SM-... 主键。
     */
    public String originalMessageId;

    /**
     * 上游报价标识。QT 对应 QuoteID(117)，标识发送方意向，一个意向可派生多个双边会话。
     * 接收报文中的 -10000 表示不可用，不能作为有效报价关联键；不用于生成本地 quoteId。
     */
    public String externalQuoteId;

    /**
     * QT NegotiationID(18033)，标识双边会话。与内部归属人、本方账号、Bid/Ofr 一起关联该方的报价提案。
     * 同一会话可包含多个本地 quoteId（不同方向或先后提案）；它不是本地报价记录的主键。
     */
    public String negotiationId;

    /** QT InterloNickID(18058)，原样保留的对手标识；不假定为明文 QT 账号，也不是 STC userid 或 CRM contactId。 */
    public String counterpartyId;
    public String chatType;

    /** QT 聊天群来源标识，当前优先读取 FIX 18474，缺失时读取 18136；不是原文分组键或双边 NegotiationID。 */
    public String chatGroupId;

    /** 上游报价版本，供支持该字段的 intake 接口检查更新顺序；不同于 QuoteRecord.revision，当前 QT FIX 映射不填此字段。 */
    public String externalRevision;
    public String sourceTime;
    public String transType;
    public String text;
}
