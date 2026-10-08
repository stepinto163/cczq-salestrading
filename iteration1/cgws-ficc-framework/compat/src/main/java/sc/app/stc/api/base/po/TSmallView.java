// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import com.alibaba.fastjson.annotation.JSONField;

import org.apache.http.util.TextUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.math.BigDecimal;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;

public class TSmallView {
    static Hashtable<String, List<String>> htBondSubType = new Hashtable();
    private static Logger logger;
    private static SimpleDateFormat format;

    @JSONField(name = "b", serialize = false)
    public Bond b;

    @JSONField(name = "info", serialize = false)
    public BondListInfo info;

    private boolean isRate(String subType) {
        boolean flag = false;
        if (!TextUtils.isBlank((CharSequence) subType)) {
            flag = htBondSubType.get("2").contains(subType);
        }
        return flag;
    }

    private boolean isABS(String subType) {
        boolean flag = false;
        if (!TextUtils.isBlank((CharSequence) subType)) {
            flag = htBondSubType.get("11").contains(subType);
        }
        return flag;
    }

    private boolean isNCD(String subType) {
        boolean flag = false;
        if (!TextUtils.isBlank((CharSequence) subType)) {
            flag = htBondSubType.get("12").contains(subType);
        }
        return flag;
    }

    private boolean isCredits(String subType, String ent_cor) {
        boolean flag = false;
        if (!TextUtils.isBlank((CharSequence) subType)) {
            flag = htBondSubType.get("1").contains(subType);
        }
        return flag;
    }

    private String getGoodsType(String subType, String ent_cor) {
        String type = "";
        if (this.isRate(subType)) {
            type = "2";
        } else if (this.isCredits(subType, ent_cor)) {
            type = "1";
        } else if (this.isNCD(subType)) {
            type = "12";
        } else if (this.isABS(subType)) {
            type = "11";
        }
        return type;
    }

    @JSONField(name = "goodstype", serialize = true)
    public String getGoodsType() {
        String goodstype = "";
        if (this.b != null) {
            goodstype = this.getGoodsType(this.b.Bond_Subtype, this.b.Ent_Cor);
        }
        return goodstype;
    }

    @JSONField(name = "bondkeylistedmarket", serialize = true)
    public String getBond_key_listed_market() {
        return this.info.Bond_Key + this.info.Listed_Market;
    }

    @JSONField(name = "delflag", serialize = true)
    public String getDelflag() {
        return this.info.delflag;
    }

    @JSONField(name = "BondKey", serialize = true)
    public String getBond_Key() {
        return this.info.Bond_Key;
    }

    @JSONField(name = "BondID", serialize = true)
    public String getBond_ID() {
        if (!this.info.Bond_ID.contains(".")) {
            if (this.info.Listed_Market.equals("CIB")) {
                return String.format("%s.%s", this.info.Bond_ID, "IB");
            }
            if (this.info.Listed_Market.equals("SSE")) {
                return String.format("%s.%s", this.info.Bond_ID, "SH");
            }
            if (this.info.Listed_Market.equals("SZE")) {
                return String.format("%s.%s", this.info.Bond_ID, "SZ");
            }
        }
        return this.info.Bond_ID;
    }

    @JSONField(name = "BondSubtype", serialize = true)
    public String getBond_Subtype() {
        return this.info.Bond_Subtype;
    }

    @JSONField(name = "ShortName", serialize = true)
    public String getShort_Name() {
        if (this.info != null) {
            return this.info.Short_Name;
        }
        return null;
    }

    @JSONField(name = "pinYin", serialize = true)
    public String getPin_Yin() {
        return this.info.pinyin;
    }

    @JSONField(name = "pinYinFull", serialize = true)
    public String getPin_Yin_Full() {
        return this.info.pinyin_full;
    }

    @JSONField(name = "IssuerRatingCurrent", serialize = true)
    public String getIssuer_Rating_Current() {
        if (this.b != null) {
            return this.b.Issuer_Rating_Current;
        }
        return null;
    }

    @JSONField(name = "MaturityDate", serialize = true)
    public String getMaturity_Date() {
        if (this.b != null && this.b.Maturity_Date != null) {
            return String.valueOf(this.b.Maturity_Date.intValue());
        }
        return null;
    }

    @JSONField(name = "BondModifyDate", serialize = true)
    public String getBond_modify_date() {
        if (this.b != null && this.b.modify_date != null) {
            return String.valueOf(this.b.modify_date);
        }
        return null;
    }

    @JSONField(name = "BondListInfoModifyDate", serialize = true)
    public String getBondlistinfo_modify_date() {
        if (this.info != null && this.info.modify_date != null) {
            return String.valueOf(this.info.modify_date);
        }
        return null;
    }

    @JSONField(name = "BondType", serialize = true)
    public String getBondType() {
        if (this.b != null) {
            return this.b.Bond_Type;
        }
        return null;
    }

    @JSONField(name = "BondSubType", serialize = true)
    public String getBondSubType() {
        if (this.b != null) {
            return this.b.Bond_Subtype;
        }
        return null;
    }

    @JSONField(name = "IssuerCode", serialize = true)
    public String getIssuerCode() {
        if (this.b != null) {
            return this.b.Issuer_Code;
        }
        return null;
    }

    @JSONField(name = "DelistedDate", serialize = true)
    public BigDecimal getDelisted_Date() {
        if (this.b != null) {
            return this.b.Delisted_Date;
        }
        return null;
    }

    @JSONField(name = "OptionDate", serialize = true)
    public BigDecimal getOption_Date() {
        if (this.b != null) {
            return this.b.Option_Date;
        }
        return null;
    }

    @JSONField(name = "ListedMarket", serialize = true)
    public String getListed_Market() {
        if (this.info != null) {
            return this.info.Listed_Market;
        }
        return null;
    }

    @JSONField(name = "FRNIndexID", serialize = true)
    public String getFRN_Index_ID() {
        if (this.b != null) {
            return this.b.FRN_Index_ID;
        }
        return null;
    }

    @JSONField(name = "IsMunicipal", serialize = true)
    public String getIs_Municipal() {
        if (this.b != null) {
            return this.b.Is_Municipal;
        }
        return null;
    }

    @JSONField(name = "EntCor", serialize = true)
    public String getEnt_Cor() {
        if (this.b != null) {
            return this.b.Ent_Cor;
        }
        return null;
    }

    @JSONField(name = "InstitutionRating", serialize = true)
    public String getInstitution_Rating() {
        if (this.b != null) {
            return this.b.Institution_Rating;
        }
        return null;
    }

    @JSONField(name = "OptionType", serialize = true)
    public String getOption_Type() {
        if (this.b != null) {
            return this.b.Option_Type;
        }
        return null;
    }

    static {
        ArrayList<String> rateLt = new ArrayList<String>();
        for (String subType : "BGB|EGB|SGB|SCB|PBB|PSB|TLB|LLB".split("\\|")) {
            rateLt.add(subType);
        }
        ArrayList<String> creditsLt = new ArrayList<String>();
        for (String subType :
                "CCB|CCP|CEB|CMN|COB|CSP|CVB|LCP|LEB|LMN|LSP|PPN|SCV|CRM|CLN|PSB|CBB|CSB|CXB|NCB|SEB|SES|SSB|TET|HJB|RAB|PDB|INT"
                        .split("\\|")) {
            creditsLt.add(subType);
        }
        ArrayList<String> absLt = new ArrayList<String>();
        for (String subType : "AMP|LBS|CBS|MBS".split("\\|")) {
            absLt.add(subType);
        }
        ArrayList<String> ncdLt = new ArrayList<String>();
        for (String subType : "CCD|FRD|MCD|OTD|RRD|RTD|SHD|SPD".split("\\|")) {
            ncdLt.add(subType);
        }
        htBondSubType.put("2", rateLt);
        htBondSubType.put("1", creditsLt);
        htBondSubType.put("11", absLt);
        htBondSubType.put("12", ncdLt);
        logger = LoggerFactory.getLogger(TSmallView.class);
        format = new SimpleDateFormat("yyyy-MM-dd 00:00:00");
    }
}
