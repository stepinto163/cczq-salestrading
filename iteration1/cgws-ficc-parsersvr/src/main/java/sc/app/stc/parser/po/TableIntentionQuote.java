package sc.app.stc.parser.po;

import lombok.Data;
import lombok.ToString;

import java.io.Serializable;
import java.math.BigDecimal;

@Data
@ToString
public class TableIntentionQuote implements Serializable {

    public String intentionId;
    public String institutionId;
    public String institutionName;
    public String traderId;
    public String traderName;
    public String userId;
    public String userName;
    public String qqNumber;
    public String securityId;
    public String symbol;
    public String side;
    public BigDecimal price;
    public BigDecimal cleanPrice;
    public BigDecimal fullPrice;
    public BigDecimal qty;
    public String settlType;
    public String text;
    public String status;
    public String type;
    public String orderId;
    public String createTime;
    public String updateTime;
    public String closeBy;
    public String wholeflag;//是否整量

    public String roleid;
    public String rolename;

    public String partysubname;//本交易员Name

    public String parseTraderIdbkey;
    public String parseCompanyIdbkey;

    public String accountID;
    public String accountName;
    public String vipCode;
    public String orgType;
    public String ifSubBond;
    public String validuntitime;
    public String inf1;//扩展字段1
    public Long sign;
    public Long sign1;
    public Long sign2;
    public Long sign3;
    public String reftype;
    public String roletype;
    //期限（天）
    public BigDecimal termDays;
    public String optionType;
    public String optionDate;
    //最新票面利率
    public BigDecimal couponRateCurrent;
    //发行人代码
    public String issuerCode;
    //发行人名称
    public String issuerName;
    //担保人代码
    public String warranterCode;
    //担保人名称
    public String warranterName;
    //中债估值
    public BigDecimal chinaBondValuation;
    public String chinaBondValuationFull;
    //麦氏久期
    public String macaulayDuration;
    //修正久期
    public String modifiedDuration;
    //pvbp
    public String pvbp;
    //凸性
    public String convexity;
    //到期日距离下个工作日天数（休n/x）
    public String dayOffStr;
    //发行总量
    public BigDecimal issueAmount;
    public String firmflag;
    public String marketindicator;
    public BigDecimal executionqty;
    public String clordid;
    //中证估值
    public BigDecimal csiBondValuation;
    //中证估值全价
    public String csiBondValuationFull;
    //期限
    public String tenor;
    //暗盘标记 1暗盘，2明盘
    public String handlinst;
    //净价:"1" 全价:"2",收益率:"3"
    public String priceType;


    //到期时间
    public String timeToMaturity;
    //到期日距离下个工作日天数
    public String timeToMaturityToWorkDay;
    public String actionType;
    public String quoteType;
    public String name;
    public String closeByName;
    public String failMessage;

    public Boolean isInternal;//是否为识别债券

    /**
     * 报价来源
     */
    public String channel;

    public String info2;
    public String info3;
    public String info4;
    public String info5;
    public String info6;

    /**
     * 急
     */
    public String hurryFlag;
    /**
     * 荐
     */
    public String suggestFlag;
    /**
     * 火
     */
    public String fireFlag;
    /**
     * 底价
     */
    public String floorPrice;
    /**
     * 客户报价
     */
    public BigDecimal customerPrice;
    /**
     * 点差
     */
    public BigDecimal customerSpread;

    /**
     * 公开备注
     */
    public String publicRemarks;

    /**
     * 个人备注
     */
    public String personalRemarks;

    public String ocoflag;//oco标记

    public String biztype;

    public String biztypeid;

    public String inf3;

}