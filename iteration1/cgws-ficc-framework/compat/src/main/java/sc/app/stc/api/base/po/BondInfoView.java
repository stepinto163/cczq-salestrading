// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import com.alibaba.fastjson.annotation.JSONField;

import java.io.Serializable;
import java.math.BigDecimal;
import java.sql.Timestamp;

public class BondInfoView implements Serializable {
    public String bond_key_listed_market = "";
    public String Bond_Key;
    public String Listed_Market;
    public String MarketIndicator;
    public String Bond_ID;
    public String Bond_ID_Market;
    public String Short_Name;
    public String Is_Cross_Mkt;
    public String Is_Mortgage;
    public String pinyin;
    public String pinyin_full;
    public BigDecimal Listed_Date;
    public BigDecimal Delisted_Date;
    public String Mkt_Type;
    public String delflag;
    public String TimeToMaturity;
    public Timestamp bond_modify_date = new Timestamp(System.currentTimeMillis());
    public Timestamp bondlistinfo_modify_date = new Timestamp(System.currentTimeMillis());
    public String flag;
    public String Issuer_Code;
    public String Underwriter_Code;
    public String Bond_Subtype;
    public BigDecimal Maturity_Term;
    public BigDecimal Maturity_Term_Y;
    public String Term_Unit;
    public BigDecimal Interest_Start_Date;
    public BigDecimal Maturity_Date;
    public BigDecimal Next_Coupon_Date;
    public String Option_Type;
    public BigDecimal Option_Date;
    public String Coupon_Type;
    public BigDecimal Coupon_Rate_Spread;
    public BigDecimal Coupon_Rate_Current;
    public String FRN_Index_ID;
    public BigDecimal Fixing_MA_Days;
    public BigDecimal Issue_Amount;
    public BigDecimal Outstanding_Amount;
    public BigDecimal Redemption_No;
    public String Rating_Current;
    public String Rating_Institution_Code;
    public BigDecimal Rating_Date;
    public String Rating_Augment;
    public String Warranter;
    public String Issuer_Rating_Current;
    public String Issuer_Rating_Institution_Code;
    public BigDecimal Issue_Year;
    public String Is_Municipal;
    public String Issuer_Rating_Current_NPY;
    public String Rating_Current_NPY;
    public String Ent_Cor;
    public String TRADE_PUBLISH_INDICATOR;
    public String SECURITY_SUPER_TYPE;
    public String SECURITY_DEC;
    public BigDecimal ACCRUED_INTEREST_AMT;
    public String DAY_COUNT;
    public BigDecimal CIRCULATION_SIZE;
    public String TERM_TO_MATURITY_STRING;
    public String MARKET_SUFFIX;
    public String SYMBOL;
    public String ISSUER;
    public String SECURITY_TYPE_ID;
    public String SECURITY_TYPE;
    public String ISSUE_DATE;
    public String SECURITY_TERM;
    public String UPDATE_TIME;
    public String issuer2;
    public String issuerratingcurrent2;
    public String issuerratinginstitution2;
    public String bondsubtype2;
    public String CN_FULL_NAME;
    public String CN_SHORT_NAME;
    public String EN_FULL_NAME;
    public String Bond_ID_CIB;
    public String Bond_ID_SSE;
    public String Bond_ID_SZE;
    public String Face_value;
    public String bondtype;
    public String Min_change;
    public String Last_trading_day;
    public String Coupon_Frequency;
    public String Simple_Compound;
    public String daycount;
    public String Issue_Price;
    public String First_Index_Rate;
    public String Asset_Status;
    public String CDC_Implied_Rating;
    public String Issuer_Rating_Current_CBR;
    public String CSI_Implied_Rating;
    public String Liquidity_Supporter;
    public BigDecimal Issue_Start_Date;
    public String Sceniority;
    public BigDecimal issue_rate;
    public String timeToMaturityToWorkDay;
    public String swapCurveValue;

    @JSONField(name = "pricpl_repayment", serialize = true)
    public String pricpl_repayment;

    public String Purpose_of_Issue;
    public String Full_Name;
    public String Issuer_Outlook_Current;
    public String Put_Str;
    public String Call_Str;
    public String Fund_Objectives_Category;
    public String Category_flag;
    public String symbolType;
    public String custodian;
    public String funduse;
    public String clause;
    public String clause_abbr;
    public String Term_structure;
    public String Maturity_Term_Structure;
    public String option_sign;
    public String Day_Off_Type;
    public String Compensate_Rate;
    public String CrossMktBondID;
    public Long sign;
    public Long sign1;
    public Long sign2;
    public Long sign3;
    public Long sign4;
    public String BondSigns;
    public String cdcAuth;
    public String issuerName;
    public String aftchanged_amt;
    public String info;
    public String bond_tag;
    public String tag_info;

    @JSONField(name = "bondmaster_modify_date", serialize = true)
    public Timestamp bondmaster_modify_date = new Timestamp(System.currentTimeMillis());

    public Long getSign4() {
        return this.sign4;
    }

    public void setSign4(Long sign4) {
        this.sign4 = sign4;
    }

    public String getTag_info() {
        return this.tag_info;
    }

    public void setTag_info(String tag_info) {
        this.tag_info = tag_info;
    }

    public String getBond_tag() {
        return this.bond_tag;
    }

    public void setBond_tag(String bond_tag) {
        this.bond_tag = bond_tag;
    }

    public String getAftchanged_amt() {
        return this.aftchanged_amt;
    }

    public void setAftchanged_amt(String aftchanged_amt) {
        this.aftchanged_amt = aftchanged_amt;
    }

    public String getInfo() {
        return this.info;
    }

    public void setInfo(String info) {
        this.info = info;
    }

    public String getIssuerName() {
        return this.issuerName;
    }

    public void setIssuerName(String issuerName) {
        this.issuerName = issuerName;
    }

    public String getCdcAuth() {
        return this.cdcAuth;
    }

    public void setCdcAuth(String cdcAuth) {
        this.cdcAuth = cdcAuth;
    }

    public String getBondSigns() {
        return this.BondSigns;
    }

    public void setBondSigns(String bondSigns) {
        this.BondSigns = bondSigns;
    }

    public Long getSign() {
        return this.sign;
    }

    public void setSign(Long sign) {
        this.sign = sign;
    }

    public Long getSign1() {
        return this.sign1;
    }

    public void setSign1(Long sign1) {
        this.sign1 = sign1;
    }

    public Long getSign2() {
        return this.sign2;
    }

    public void setSign2(Long sign2) {
        this.sign2 = sign2;
    }

    public Long getSign3() {
        return this.sign3;
    }

    public void setSign3(Long sign3) {
        this.sign3 = sign3;
    }

    public String getCrossMktBondID() {
        return this.CrossMktBondID;
    }

    public void setCrossMktBondID(String crossMktBondID) {
        this.CrossMktBondID = crossMktBondID;
    }

    public String getCompensate_Rate() {
        return this.Compensate_Rate;
    }

    public void setCompensate_Rate(String compensate_Rate) {
        this.Compensate_Rate = compensate_Rate;
    }

    public String getCall_Str() {
        return this.Call_Str;
    }

    public void setCall_Str(String call_Str) {
        this.Call_Str = call_Str;
    }

    public String getDay_Off_Type() {
        return this.Day_Off_Type;
    }

    public void setDay_Off_Type(String day_Off_Type) {
        this.Day_Off_Type = day_Off_Type;
    }

    public String getOption_sign() {
        return this.option_sign;
    }

    public void setOption_sign(String option_sign) {
        this.option_sign = option_sign;
    }

    public String getTerm_structure() {
        return this.Term_structure;
    }

    public void setTerm_structure(String term_structure) {
        this.Term_structure = term_structure;
    }

    public String getMaturity_Term_Structure() {
        return this.Maturity_Term_Structure;
    }

    public void setMaturity_Term_Structure(String maturity_Term_Structure) {
        this.Maturity_Term_Structure = maturity_Term_Structure;
    }

    public String getClause() {
        return this.clause;
    }

    public void setClause(String clause) {
        this.clause = clause;
    }

    public String getClause_abbr() {
        return this.clause_abbr;
    }

    public void setClause_abbr(String clause_abbr) {
        this.clause_abbr = clause_abbr;
    }

    public String getFunduse() {
        return this.funduse;
    }

    public void setFunduse(String funduse) {
        this.funduse = funduse;
    }

    public String getCustodian() {
        return this.custodian;
    }

    public void setCustodian(String custodian) {
        this.custodian = custodian;
    }

    public String getSymbolType() {
        return this.symbolType;
    }

    public void setSymbolType(String symbolType) {
        this.symbolType = symbolType;
    }

    public String getFund_Objectives_Category() {
        return this.Fund_Objectives_Category;
    }

    public void setFund_Objectives_Category(String fund_Objectives_Category) {
        this.Fund_Objectives_Category = fund_Objectives_Category;
    }

    public String getCategory_flag() {
        return this.Category_flag;
    }

    public void setCategory_flag(String category_flag) {
        this.Category_flag = category_flag;
    }

    public String getPut_Str() {
        return this.Put_Str;
    }

    public void setPut_Str(String put_Str) {
        this.Put_Str = put_Str;
    }

    public String getPricpl_repayment() {
        return this.pricpl_repayment;
    }

    public void setPricpl_repayment(String pricpl_repayment) {
        this.pricpl_repayment = pricpl_repayment;
    }

    public String getFull_Name() {
        return this.Full_Name;
    }

    public void setFull_Name(String full_Name) {
        this.Full_Name = full_Name;
    }

    public String getPurpose_of_Issue() {
        return this.Purpose_of_Issue;
    }

    public void setPurpose_of_Issue(String purpose_of_Issue) {
        this.Purpose_of_Issue = purpose_of_Issue;
    }

    public String getIssuer_Outlook_Current() {
        return this.Issuer_Outlook_Current;
    }

    public void setIssuer_Outlook_Current(String issuer_Outlook_Current) {
        this.Issuer_Outlook_Current = issuer_Outlook_Current;
    }

    public String getTimeToMaturityToWorkDay() {
        return this.timeToMaturityToWorkDay;
    }

    public void setTimeToMaturityToWorkDay(String timeToMaturityToWorkDay) {
        this.timeToMaturityToWorkDay = timeToMaturityToWorkDay;
    }

    public BigDecimal getIssue_rate() {
        return this.issue_rate;
    }

    public void setIssue_rate(BigDecimal issue_rate) {
        this.issue_rate = issue_rate;
    }

    public String getSceniority() {
        return this.Sceniority;
    }

    public void setSceniority(String sceniority) {
        this.Sceniority = sceniority;
    }

    public String getAsset_Status() {
        return this.Asset_Status;
    }

    public void setAsset_Status(String asset_Status) {
        this.Asset_Status = asset_Status;
    }

    public String getCDC_Implied_Rating() {
        return this.CDC_Implied_Rating;
    }

    public void setCDC_Implied_Rating(String CDC_Implied_Rating) {
        this.CDC_Implied_Rating = CDC_Implied_Rating;
    }

    public String getIssuer_Rating_Current_CBR() {
        return this.Issuer_Rating_Current_CBR;
    }

    public void setIssuer_Rating_Current_CBR(String issuer_Rating_Current_CBR) {
        this.Issuer_Rating_Current_CBR = issuer_Rating_Current_CBR;
    }

    public String getCSI_Implied_Rating() {
        return this.CSI_Implied_Rating;
    }

    public void setCSI_Implied_Rating(String CSI_Implied_Rating) {
        this.CSI_Implied_Rating = CSI_Implied_Rating;
    }

    public String getLiquidity_Supporter() {
        return this.Liquidity_Supporter;
    }

    public void setLiquidity_Supporter(String liquidity_Supporter) {
        this.Liquidity_Supporter = liquidity_Supporter;
    }

    public BigDecimal getIssue_Start_Date() {
        return this.Issue_Start_Date;
    }

    public void setIssue_Start_Date(BigDecimal issue_Start_Date) {
        this.Issue_Start_Date = issue_Start_Date;
    }

    public String getIssue_Price() {
        return this.Issue_Price;
    }

    public void setIssue_Price(String issue_Price) {
        this.Issue_Price = issue_Price;
    }

    public String getFirst_Index_Rate() {
        return this.First_Index_Rate;
    }

    public void setFirst_Index_Rate(String first_Index_Rate) {
        this.First_Index_Rate = first_Index_Rate;
    }

    public String getSimple_Compound() {
        return this.Simple_Compound;
    }

    public void setSimple_Compound(String simple_Compound) {
        this.Simple_Compound = simple_Compound;
    }

    public String getDaycount() {
        return this.daycount;
    }

    public void setDaycount(String daycount) {
        this.daycount = daycount;
    }

    @JSONField(name = "Min_change", serialize = true)
    public String getMin_change() {
        return this.Min_change;
    }

    public void setMin_change(String min_change) {
        this.Min_change = min_change;
    }

    public String getTRADE_PUBLISH_INDICATOR() {
        return this.TRADE_PUBLISH_INDICATOR;
    }

    public void setTRADE_PUBLISH_INDICATOR(String TRADE_PUBLISH_INDICATOR) {
        this.TRADE_PUBLISH_INDICATOR = TRADE_PUBLISH_INDICATOR;
    }

    public String getUPDATE_TIME() {
        return this.UPDATE_TIME;
    }

    public String getISSUE_DATE() {
        return this.ISSUE_DATE;
    }

    public void setISSUE_DATE(String ISSUE_DATE) {
        this.ISSUE_DATE = ISSUE_DATE;
    }

    public String getSECURITY_TERM() {
        return this.SECURITY_TERM;
    }

    public void setSECURITY_TERM(String SECURITY_TERM) {
        this.SECURITY_TERM = SECURITY_TERM;
    }

    public Timestamp getBondmaster_modify_date() {
        return this.bondmaster_modify_date;
    }

    public void setBondmaster_modify_date(Timestamp bondmaster_modify_date) {
        this.bondmaster_modify_date = bondmaster_modify_date;
    }

    public void setUPDATE_TIME(String UPDATE_TIME) {
        this.UPDATE_TIME = UPDATE_TIME;
    }

    @JSONField(name = "SECURITY_TYPE", serialize = true)
    public String getSECURITY_TYPE() {
        return this.SECURITY_TYPE;
    }

    public void setSECURITY_TYPE(String SECURITY_TYPE) {
        this.SECURITY_TYPE = SECURITY_TYPE;
    }

    @JSONField(name = "bond_key_listed_market", serialize = true)
    public String getBond_key_listed_market() {
        return this.bond_key_listed_market;
    }

    public void setBond_key_listed_market(String bond_key_listed_market) {
        this.bond_key_listed_market = bond_key_listed_market;
    }

    public String getTimeToMaturity() {
        return this.TimeToMaturity;
    }

    public void setTimeToMaturity(String timeToMaturity) {
        this.TimeToMaturity = timeToMaturity;
    }

    public String getMarketIndicator() {
        return this.MarketIndicator;
    }

    public void setMarketIndicator(String marketIndicator) {
        this.MarketIndicator = marketIndicator;
    }

    @JSONField(name = "Bond_Key", serialize = true)
    public String getBond_Key() {
        return this.Bond_Key;
    }

    public void setBond_Key(String bond_Key) {
        this.Bond_Key = bond_Key;
    }

    @JSONField(name = "Listed_Market", serialize = true)
    public String getListed_Market() {
        return this.Listed_Market;
    }

    public void setListed_Market(String listed_Market) {
        this.Listed_Market = listed_Market;
    }

    @JSONField(name = "Bond_ID", serialize = true)
    public String getBond_ID() {
        return this.Bond_ID;
    }

    public void setBond_ID(String bond_ID) {
        this.Bond_ID = bond_ID;
    }

    @JSONField(name = "Bond_ID_Market", serialize = true)
    public String getBond_ID_Market() {
        return this.Bond_ID_Market;
    }

    public void setBond_ID_Market(String bond_ID_Market) {
        this.Bond_ID_Market = bond_ID_Market;
    }

    @JSONField(name = "Short_Name", serialize = true)
    public String getShort_Name() {
        return this.Short_Name;
    }

    public void setShort_Name(String short_Name) {
        this.Short_Name = short_Name;
    }

    @JSONField(name = "Is_Cross_Mkt", serialize = true)
    public String getIs_Cross_Mkt() {
        return this.Is_Cross_Mkt;
    }

    public void setIs_Cross_Mkt(String is_Cross_Mkt) {
        this.Is_Cross_Mkt = is_Cross_Mkt;
    }

    @JSONField(name = "Is_Mortgage", serialize = true)
    public String getIs_Mortgage() {
        return this.Is_Mortgage;
    }

    public void setIs_Mortgage(String is_Mortgage) {
        this.Is_Mortgage = is_Mortgage;
    }

    @JSONField(name = "pinyin", serialize = true)
    public String getPinyin() {
        return this.pinyin;
    }

    public void setPinyin(String pinyin) {
        this.pinyin = pinyin;
    }

    @JSONField(name = "pinyin_full", serialize = true)
    public String getPinyin_full() {
        return this.pinyin_full;
    }

    public void setPinyin_full(String pinyin_full) {
        this.pinyin_full = pinyin_full;
    }

    @JSONField(name = "Listed_Date", serialize = true)
    public BigDecimal getListed_Date() {
        return this.Listed_Date;
    }

    public void setListed_Date(BigDecimal listed_Date) {
        this.Listed_Date = listed_Date;
    }

    @JSONField(name = "Delisted_Date", serialize = true)
    public BigDecimal getDelisted_Date() {
        return this.Delisted_Date;
    }

    public void setDelisted_Date(BigDecimal delisted_Date) {
        this.Delisted_Date = delisted_Date;
    }

    @JSONField(name = "Mkt_Type", serialize = true)
    public String getMkt_Type() {
        return this.Mkt_Type;
    }

    public void setMkt_Type(String mkt_Type) {
        this.Mkt_Type = mkt_Type;
    }

    @JSONField(name = "delflag", serialize = true)
    public String getDelflag() {
        return this.delflag;
    }

    public void setDelflag(String delflag) {
        this.delflag = delflag;
    }

    @JSONField(name = "bond_modify_date", serialize = true)
    public Timestamp getBond_modify_date() {
        return this.bond_modify_date;
    }

    public void setBond_modify_date(Timestamp bond_modify_date) {
        this.bond_modify_date = bond_modify_date;
    }

    @JSONField(name = "bondlistinfo_modify_date", serialize = true)
    public Timestamp getBondlistinfo_modify_date() {
        return this.bondlistinfo_modify_date;
    }

    public void setBondlistinfo_modify_date(Timestamp bondlistinfo_modify_date) {
        this.bondlistinfo_modify_date = bondlistinfo_modify_date;
    }

    @JSONField(name = "flag", serialize = true)
    public String getFlag() {
        return this.flag;
    }

    public void setFlag(String flag) {
        this.flag = flag;
    }

    @JSONField(name = "Issuer_Code", serialize = true)
    public String getIssuer_Code() {
        return this.Issuer_Code;
    }

    public void setIssuer_Code(String issuer_Code) {
        this.Issuer_Code = issuer_Code;
    }

    @JSONField(name = "Underwriter_Code", serialize = true)
    public String getUnderwriter_Code() {
        return this.Underwriter_Code;
    }

    public void setUnderwriter_Code(String underwriter_Code) {
        this.Underwriter_Code = underwriter_Code;
    }

    @JSONField(name = "Bond_Subtype", serialize = true)
    public String getBond_Subtype() {
        return this.Bond_Subtype;
    }

    public void setBond_Subtype(String bond_Subtype) {
        this.Bond_Subtype = bond_Subtype;
    }

    @JSONField(name = "Maturity_Term", serialize = true)
    public BigDecimal getMaturity_Term() {
        return this.Maturity_Term;
    }

    public void setMaturity_Term(BigDecimal maturity_Term) {
        this.Maturity_Term = maturity_Term;
    }

    @JSONField(name = "Maturity_Term_Y", serialize = true)
    public BigDecimal getMaturity_Term_Y() {
        return this.Maturity_Term_Y;
    }

    public void setMaturity_Term_Y(BigDecimal maturity_Term_Y) {
        this.Maturity_Term_Y = maturity_Term_Y;
    }

    @JSONField(name = "Term_Unit", serialize = true)
    public String getTerm_Unit() {
        return this.Term_Unit;
    }

    public void setTerm_Unit(String term_Unit) {
        this.Term_Unit = term_Unit;
    }

    @JSONField(name = "Interest_Start_Date", serialize = true)
    public BigDecimal getInterest_Start_Date() {
        return this.Interest_Start_Date;
    }

    public void setInterest_Start_Date(BigDecimal interest_Start_Date) {
        this.Interest_Start_Date = interest_Start_Date;
    }

    @JSONField(name = "Maturity_Date", serialize = true)
    public BigDecimal getMaturity_Date() {
        return this.Maturity_Date;
    }

    public void setMaturity_Date(BigDecimal maturity_Date) {
        this.Maturity_Date = maturity_Date;
    }

    @JSONField(name = "Next_Coupon_Date", serialize = true)
    public BigDecimal getNext_Coupon_Date() {
        return this.Next_Coupon_Date;
    }

    public void setNext_Coupon_Date(BigDecimal next_Coupon_Date) {
        this.Next_Coupon_Date = next_Coupon_Date;
    }

    @JSONField(name = "Option_Type", serialize = true)
    public String getOption_Type() {
        return this.Option_Type;
    }

    public void setOption_Type(String option_Type) {
        this.Option_Type = option_Type;
    }

    @JSONField(name = "Option_Date", serialize = true)
    public BigDecimal getOption_Date() {
        return this.Option_Date;
    }

    public void setOption_Date(BigDecimal option_Date) {
        this.Option_Date = option_Date;
    }

    @JSONField(name = "Coupon_Type", serialize = true)
    public String getCoupon_Type() {
        return this.Coupon_Type;
    }

    public void setCoupon_Type(String coupon_Type) {
        this.Coupon_Type = coupon_Type;
    }

    @JSONField(name = "Coupon_Rate_Spread", serialize = true)
    public BigDecimal getCoupon_Rate_Spread() {
        return this.Coupon_Rate_Spread;
    }

    public void setCoupon_Rate_Spread(BigDecimal coupon_Rate_Spread) {
        this.Coupon_Rate_Spread = coupon_Rate_Spread;
    }

    @JSONField(name = "Coupon_Rate_Current", serialize = true)
    public BigDecimal getCoupon_Rate_Current() {
        return this.Coupon_Rate_Current;
    }

    public void setCoupon_Rate_Current(BigDecimal coupon_Rate_Current) {
        this.Coupon_Rate_Current = coupon_Rate_Current;
    }

    @JSONField(name = "FRN_Index_ID", serialize = true)
    public String getFRN_Index_ID() {
        return this.FRN_Index_ID;
    }

    public void setFRN_Index_ID(String FRN_Index_ID) {
        this.FRN_Index_ID = FRN_Index_ID;
    }

    @JSONField(name = "Fixing_MA_Days", serialize = true)
    public BigDecimal getFixing_MA_Days() {
        return this.Fixing_MA_Days;
    }

    public void setFixing_MA_Days(BigDecimal fixing_MA_Days) {
        this.Fixing_MA_Days = fixing_MA_Days;
    }

    @JSONField(name = "Issue_Amount", serialize = true)
    public BigDecimal getIssue_Amount() {
        return this.Issue_Amount;
    }

    public void setIssue_Amount(BigDecimal issue_Amount) {
        this.Issue_Amount = issue_Amount;
    }

    @JSONField(name = "Redemption_No", serialize = true)
    public BigDecimal getRedemption_No() {
        return this.Redemption_No;
    }

    public void setRedemption_No(BigDecimal redemption_No) {
        this.Redemption_No = redemption_No;
    }

    @JSONField(name = "Rating_Current", serialize = true)
    public String getRating_Current() {
        return this.Rating_Current;
    }

    public void setRating_Current(String rating_Current) {
        this.Rating_Current = rating_Current;
    }

    @JSONField(name = "Rating_Institution_Code", serialize = true)
    public String getRating_Institution_Code() {
        return this.Rating_Institution_Code;
    }

    public void setRating_Institution_Code(String rating_Institution_Code) {
        this.Rating_Institution_Code = rating_Institution_Code;
    }

    @JSONField(name = "Rating_Date", serialize = true)
    public BigDecimal getRating_Date() {
        return this.Rating_Date;
    }

    public void setRating_Date(BigDecimal rating_Date) {
        this.Rating_Date = rating_Date;
    }

    @JSONField(name = "Rating_Augment", serialize = true)
    public String getRating_Augment() {
        return this.Rating_Augment;
    }

    public void setRating_Augment(String rating_Augment) {
        this.Rating_Augment = rating_Augment;
    }

    @JSONField(name = "Warranter", serialize = true)
    public String getWarranter() {
        return this.Warranter;
    }

    public void setWarranter(String warranter) {
        this.Warranter = warranter;
    }

    @JSONField(name = "Issuer_Rating_Current", serialize = true)
    public String getIssuer_Rating_Current() {
        return this.Issuer_Rating_Current;
    }

    public void setIssuer_Rating_Current(String issuer_Rating_Current) {
        this.Issuer_Rating_Current = issuer_Rating_Current;
    }

    @JSONField(name = "Issuer_Rating_Institution_Code", serialize = true)
    public String getIssuer_Rating_Institution_Code() {
        return this.Issuer_Rating_Institution_Code;
    }

    public void setIssuer_Rating_Institution_Code(String issuer_Rating_Institution_Code) {
        this.Issuer_Rating_Institution_Code = issuer_Rating_Institution_Code;
    }

    @JSONField(name = "Issue_Year", serialize = true)
    public BigDecimal getIssue_Year() {
        return this.Issue_Year;
    }

    public void setIssue_Year(BigDecimal issue_Year) {
        this.Issue_Year = issue_Year;
    }

    @JSONField(name = "Is_Municipal", serialize = true)
    public String getIs_Municipal() {
        return this.Is_Municipal;
    }

    public void setIs_Municipal(String is_Municipal) {
        this.Is_Municipal = is_Municipal;
    }

    @JSONField(name = "Issuer_Rating_Current_NPY", serialize = true)
    public String getIssuer_Rating_Current_NPY() {
        return this.Issuer_Rating_Current_NPY;
    }

    public void setIssuer_Rating_Current_NPY(String issuer_Rating_Current_NPY) {
        this.Issuer_Rating_Current_NPY = issuer_Rating_Current_NPY;
    }

    @JSONField(name = "Rating_Current_NPY", serialize = true)
    public String getRating_Current_NPY() {
        return this.Rating_Current_NPY;
    }

    public void setRating_Current_NPY(String rating_Current_NPY) {
        this.Rating_Current_NPY = rating_Current_NPY;
    }

    @JSONField(name = "Ent_Cor", serialize = true)
    public String getEnt_Cor() {
        return this.Ent_Cor;
    }

    public void setEnt_Cor(String ent_Cor) {
        this.Ent_Cor = ent_Cor;
    }

    @JSONField(name = "SECURITY_SUPER_TYPE", serialize = true)
    public String getSECURITY_SUPER_TYPE() {
        return this.SECURITY_SUPER_TYPE;
    }

    public void setSECURITY_SUPER_TYPE(String SECURITY_SUPER_TYPE) {
        this.SECURITY_SUPER_TYPE = SECURITY_SUPER_TYPE;
    }

    @JSONField(name = "SECURITY_DEC", serialize = true)
    public String getSECURITY_DEC() {
        return this.SECURITY_DEC;
    }

    public void setSECURITY_DEC(String SECURITY_DEC) {
        this.SECURITY_DEC = SECURITY_DEC;
    }

    @JSONField(name = "ACCRUED_INTEREST_AMT", serialize = true)
    public BigDecimal getACCRUED_INTEREST_AMT() {
        return this.ACCRUED_INTEREST_AMT;
    }

    public void setACCRUED_INTEREST_AMT(BigDecimal ACCRUED_INTEREST_AMT) {
        this.ACCRUED_INTEREST_AMT = ACCRUED_INTEREST_AMT;
    }

    @JSONField(name = "DAY_COUNT", serialize = true)
    public String getDAY_COUNT() {
        return this.DAY_COUNT;
    }

    public void setDAY_COUNT(String DAY_COUNT) {
        this.DAY_COUNT = DAY_COUNT;
    }

    @JSONField(name = "CIRCULATION_SIZE", serialize = true)
    public BigDecimal getCIRCULATION_SIZE() {
        return this.CIRCULATION_SIZE;
    }

    public void setCIRCULATION_SIZE(BigDecimal CIRCULATION_SIZE) {
        this.CIRCULATION_SIZE = CIRCULATION_SIZE;
    }

    @JSONField(name = "TERM_TO_MATURITY_STRING", serialize = true)
    public String getTERM_TO_MATURITY_STRING() {
        return this.TERM_TO_MATURITY_STRING;
    }

    public void setTERM_TO_MATURITY_STRING(String TERM_TO_MATURITY_STRING) {
        this.TERM_TO_MATURITY_STRING = TERM_TO_MATURITY_STRING;
    }

    @JSONField(name = "MARKET_SUFFIX", serialize = true)
    public String getMARKET_SUFFIX() {
        return this.MARKET_SUFFIX;
    }

    public void setMARKET_SUFFIX(String MARKET_SUFFIX) {
        this.MARKET_SUFFIX = MARKET_SUFFIX;
    }

    @JSONField(name = "SYMBOL", serialize = true)
    public String getSYMBOL() {
        return this.SYMBOL;
    }

    public void setSYMBOL(String SYMBOL) {
        this.SYMBOL = SYMBOL;
    }

    @JSONField(name = "ISSUER", serialize = true)
    public String getISSUER() {
        return this.ISSUER;
    }

    public void setISSUER(String ISSUER) {
        this.ISSUER = ISSUER;
    }

    @JSONField(name = "SECURITY_TYPE_ID", serialize = true)
    public String getSECURITY_TYPE_ID() {
        return this.SECURITY_TYPE_ID;
    }

    public void setSECURITY_TYPE_ID(String SECURITY_TYPE_ID) {
        this.SECURITY_TYPE_ID = SECURITY_TYPE_ID;
    }

    @JSONField(name = "Outstanding_Amount", serialize = true)
    public BigDecimal getOutstanding_Amount() {
        return this.Outstanding_Amount;
    }

    public void setOutstanding_Amount(BigDecimal outstanding_Amount) {
        this.Outstanding_Amount = outstanding_Amount;
    }

    @JSONField(name = "issuer2", serialize = true)
    public String getIssuer2() {
        return this.issuer2;
    }

    public void setIssuer2(String issuer2) {
        this.issuer2 = issuer2;
    }

    @JSONField(name = "issuerratingcurrent2", serialize = true)
    public String getIssuerratingcurrent2() {
        return this.issuerratingcurrent2;
    }

    public void setIssuerratingcurrent2(String issuerratingcurrent2) {
        this.issuerratingcurrent2 = issuerratingcurrent2;
    }

    @JSONField(name = "issuerratinginstitution2", serialize = true)
    public String getIssuerratinginstitution2() {
        return this.issuerratinginstitution2;
    }

    public void setIssuerratinginstitution2(String issuerratinginstitution2) {
        this.issuerratinginstitution2 = issuerratinginstitution2;
    }

    @JSONField(name = "bondsubtype2", serialize = true)
    public String getBondsubtype2() {
        return this.bondsubtype2;
    }

    public void setBondsubtype2(String bondsubtype2) {
        this.bondsubtype2 = bondsubtype2;
    }

    @JSONField(name = "CN_FULL_NAME", serialize = true)
    public String getCN_FULL_NAME() {
        return this.CN_FULL_NAME;
    }

    public void setCN_FULL_NAME(String CN_FULL_NAME) {
        this.CN_FULL_NAME = CN_FULL_NAME;
    }

    @JSONField(name = "CN_SHORT_NAME", serialize = true)
    public String getCN_SHORT_NAME() {
        return this.CN_SHORT_NAME;
    }

    public void setCN_SHORT_NAME(String CN_SHORT_NAME) {
        this.CN_SHORT_NAME = CN_SHORT_NAME;
    }

    @JSONField(name = "EN_FULL_NAME", serialize = true)
    public String getEN_FULL_NAME() {
        return this.EN_FULL_NAME;
    }

    public void setEN_FULL_NAME(String EN_FULL_NAME) {
        this.EN_FULL_NAME = EN_FULL_NAME;
    }

    @JSONField(name = "Bond_ID_CIB", serialize = true)
    public String getBond_ID_CIB() {
        return this.Bond_ID_CIB;
    }

    public void setBond_ID_CIB(String bond_ID_CIB) {
        this.Bond_ID_CIB = bond_ID_CIB;
    }

    @JSONField(name = "Bond_ID_SSE", serialize = true)
    public String getBond_ID_SSE() {
        return this.Bond_ID_SSE;
    }

    public void setBond_ID_SSE(String bond_ID_SSE) {
        this.Bond_ID_SSE = bond_ID_SSE;
    }

    @JSONField(name = "Bond_ID_SZE", serialize = true)
    public String getBond_ID_SZE() {
        return this.Bond_ID_SZE;
    }

    public void setBond_ID_SZE(String bond_ID_SZE) {
        this.Bond_ID_SZE = bond_ID_SZE;
    }

    @JSONField(name = "Face_value", serialize = true)
    public String getFace_value() {
        return this.Face_value;
    }

    public void setFace_value(String face_value) {
        this.Face_value = face_value;
    }

    @JSONField(name = "bondtype", serialize = true)
    public String getBondtype() {
        return this.bondtype;
    }

    public void setBondtype(String bondtype) {
        this.bondtype = bondtype;
    }

    @JSONField(name = "Last_trading_day", serialize = true)
    public String getLast_trading_day() {
        return this.Last_trading_day;
    }

    public void setLast_trading_day(String last_trading_day) {
        this.Last_trading_day = last_trading_day;
    }

    @JSONField(name = "Coupon_Frequency", serialize = true)
    public String getCoupon_Frequency() {
        return this.Coupon_Frequency;
    }

    public void setCoupon_Frequency(String coupon_Frequency) {
        this.Coupon_Frequency = coupon_Frequency;
    }

    public String getSwapCurveValue() {
        return this.swapCurveValue;
    }

    public void setSwapCurveValue(String swapCurveValue) {
        this.swapCurveValue = swapCurveValue;
    }
}
