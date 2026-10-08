// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import com.alibaba.fastjson.annotation.JSONField;

import java.util.Objects;

public class TBrokerCrmInst {
    @JSONField(name = "idbkey", serialize = true)
    public String idbkey = "";

    @JSONField(name = "crmid", serialize = true)
    public String crmid;

    @JSONField(name = "id", serialize = true)
    public String id;

    @JSONField(name = "shortnamecn", serialize = true)
    public String shortnamecn;

    @JSONField(name = "fullnamecn", serialize = true)
    public String fullnamecn;

    @JSONField(name = "shortnameen", serialize = true)
    public String shortnameen;

    @JSONField(name = "instcode", serialize = true)
    public String instcode;

    @JSONField(name = "cityvalue", serialize = true)
    public String cityvalue;

    @JSONField(name = "insttypevalue", serialize = true)
    public String insttypevalue;

    @JSONField(name = "industrytypevalue", serialize = true)
    public String industrytypevalue;

    @JSONField(name = "fundtypevalue", serialize = true)
    public String fundtypevalue;

    @JSONField(name = "countryareavalue", serialize = true)
    public String countryareavalue;

    @JSONField(name = "managerdisplay", serialize = true)
    public String managerdisplay;

    @JSONField(name = "description", serialize = true)
    public String description;

    @JSONField(name = "contactcount", serialize = true)
    public String contactcount;

    @JSONField(name = "createtime", serialize = true)
    public String createtime;

    @JSONField(name = "updatetime", serialize = true)
    public String updatetime;

    @JSONField(name = "updateaccount", serialize = true)
    public String updateaccount;

    @JSONField(name = "issuer_code", serialize = true)
    public String issuer_code;

    @JSONField(name = "isvip", serialize = true)
    public String isvip;

    @JSONField(name = "fullnameen", serialize = true)
    public String fullnameen;

    @JSONField(name = "citycode", serialize = true)
    public String citycode;

    @JSONField(name = "companyid", serialize = true)
    public String companyid;

    @JSONField(name = "pinyin", serialize = true)
    public String pinyin;

    @JSONField(name = "pinyinfull", serialize = true)
    public String pinyinfull;

    @JSONField(name = "stc_createtime", serialize = true)
    public String stc_createtime;

    @JSONField(name = "stc_updatetime", serialize = true)
    public String stc_updatetime;

    @JSONField(name = "bncshortnamelist", serialize = true)
    public String bncshortnamelist;

    @JSONField(name = "bcoshortnamelist", serialize = true)
    public String bcoshortnamelist;

    @JSONField(name = "ncdshortnamelist", serialize = true)
    public String ncdshortnamelist;

    @JSONField(name = "irsshortnameList", serialize = true)
    public String irsshortnameList;

    @JSONField(name = "taglist", serialize = true)
    public String taglist;

    @JSONField(name = "useType", serialize = true)
    public String useType;

    @JSONField(name = "useStatus", serialize = true)
    public Integer useStatus;

    @JSONField(name = "cfetsLocalCode", serialize = true)
    public String cfetsLocalCode;

    @JSONField(name = "cfetsLocalShortName", serialize = true)
    public String cfetsLocalShortName;

    @JSONField(name = "locationValue", serialize = true)
    public String locationValue;

    @JSONField(name = "idc_createtime", serialize = true)
    public String idc_createtime;

    @JSONField(name = "idc_updatetime", serialize = true)
    public String idc_updatetime;

    @JSONField(name = "sseinstitutioncode", serialize = true)
    public String sseinstitutioncode;

    @JSONField(name = "szedealercode", serialize = true)
    public String szedealercode;

    @JSONField(name = "ssetradingentitycode", serialize = true)
    public String ssetradingentitycode;

    public boolean equals(Object o) {
        if (this == o) {
            return true;
        }
        if (o == null || this.getClass() != o.getClass()) {
            return false;
        }
        TBrokerCrmInst that = (TBrokerCrmInst) o;
        return Objects.equals(this.idbkey, that.idbkey)
                && Objects.equals(this.crmid, that.crmid)
                && Objects.equals(this.shortnamecn, that.shortnamecn)
                && Objects.equals(this.fullnamecn, that.fullnamecn)
                && Objects.equals(this.shortnameen, that.shortnameen)
                && Objects.equals(this.instcode, that.instcode)
                && Objects.equals(this.cityvalue, that.cityvalue)
                && Objects.equals(this.insttypevalue, that.insttypevalue)
                && Objects.equals(this.description, that.description)
                && Objects.equals(this.contactcount, that.contactcount)
                && Objects.equals(this.issuer_code, that.issuer_code)
                && Objects.equals(this.isvip, that.isvip)
                && Objects.equals(this.fullnameen, that.fullnameen)
                && Objects.equals(this.citycode, that.citycode)
                && Objects.equals(this.pinyin, that.pinyin)
                && Objects.equals(this.pinyinfull, that.pinyinfull);
    }

    public int hashCode() {
        return Objects.hash(
                this.idbkey,
                this.crmid,
                this.shortnamecn,
                this.fullnamecn,
                this.shortnameen,
                this.instcode,
                this.cityvalue,
                this.insttypevalue,
                this.description,
                this.contactcount,
                this.isvip,
                this.fullnameen,
                this.citycode,
                this.pinyin,
                this.pinyinfull);
    }

    public String getIdbkey() {
        return this.idbkey;
    }

    public String getCrmid() {
        return this.crmid;
    }

    public String getId() {
        return this.id;
    }

    public String getShortnamecn() {
        return this.shortnamecn;
    }

    public String getFullnamecn() {
        return this.fullnamecn;
    }

    public String getShortnameen() {
        return this.shortnameen;
    }

    public String getInstcode() {
        return this.instcode;
    }

    public String getCityvalue() {
        return this.cityvalue;
    }

    public String getInsttypevalue() {
        return this.insttypevalue;
    }

    public String getIndustrytypevalue() {
        return this.industrytypevalue;
    }

    public String getFundtypevalue() {
        return this.fundtypevalue;
    }

    public String getCountryareavalue() {
        return this.countryareavalue;
    }

    public String getManagerdisplay() {
        return this.managerdisplay;
    }

    public String getDescription() {
        return this.description;
    }

    public String getContactcount() {
        return this.contactcount;
    }

    public String getCreatetime() {
        return this.createtime;
    }

    public String getUpdatetime() {
        return this.updatetime;
    }

    public String getUpdateaccount() {
        return this.updateaccount;
    }

    public String getIssuer_code() {
        return this.issuer_code;
    }

    public String getIsvip() {
        return this.isvip;
    }

    public String getFullnameen() {
        return this.fullnameen;
    }

    public String getCitycode() {
        return this.citycode;
    }

    public String getCompanyid() {
        return this.companyid;
    }

    public String getPinyin() {
        return this.pinyin;
    }

    public String getPinyinfull() {
        return this.pinyinfull;
    }

    public String getStc_createtime() {
        return this.stc_createtime;
    }

    public String getStc_updatetime() {
        return this.stc_updatetime;
    }

    public String getBncshortnamelist() {
        return this.bncshortnamelist;
    }

    public String getBcoshortnamelist() {
        return this.bcoshortnamelist;
    }

    public String getNcdshortnamelist() {
        return this.ncdshortnamelist;
    }

    public String getIrsshortnameList() {
        return this.irsshortnameList;
    }

    public String getTaglist() {
        return this.taglist;
    }

    public String getUseType() {
        return this.useType;
    }

    public Integer getUseStatus() {
        return this.useStatus;
    }

    public String getCfetsLocalCode() {
        return this.cfetsLocalCode;
    }

    public String getCfetsLocalShortName() {
        return this.cfetsLocalShortName;
    }

    public String getLocationValue() {
        return this.locationValue;
    }

    public String getIdc_createtime() {
        return this.idc_createtime;
    }

    public String getIdc_updatetime() {
        return this.idc_updatetime;
    }

    public String getSseinstitutioncode() {
        return this.sseinstitutioncode;
    }

    public String getSzedealercode() {
        return this.szedealercode;
    }

    public String getSsetradingentitycode() {
        return this.ssetradingentitycode;
    }

    public void setIdbkey(String idbkey) {
        this.idbkey = idbkey;
    }

    public void setCrmid(String crmid) {
        this.crmid = crmid;
    }

    public void setId(String id) {
        this.id = id;
    }

    public void setShortnamecn(String shortnamecn) {
        this.shortnamecn = shortnamecn;
    }

    public void setFullnamecn(String fullnamecn) {
        this.fullnamecn = fullnamecn;
    }

    public void setShortnameen(String shortnameen) {
        this.shortnameen = shortnameen;
    }

    public void setInstcode(String instcode) {
        this.instcode = instcode;
    }

    public void setCityvalue(String cityvalue) {
        this.cityvalue = cityvalue;
    }

    public void setInsttypevalue(String insttypevalue) {
        this.insttypevalue = insttypevalue;
    }

    public void setIndustrytypevalue(String industrytypevalue) {
        this.industrytypevalue = industrytypevalue;
    }

    public void setFundtypevalue(String fundtypevalue) {
        this.fundtypevalue = fundtypevalue;
    }

    public void setCountryareavalue(String countryareavalue) {
        this.countryareavalue = countryareavalue;
    }

    public void setManagerdisplay(String managerdisplay) {
        this.managerdisplay = managerdisplay;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public void setContactcount(String contactcount) {
        this.contactcount = contactcount;
    }

    public void setCreatetime(String createtime) {
        this.createtime = createtime;
    }

    public void setUpdatetime(String updatetime) {
        this.updatetime = updatetime;
    }

    public void setUpdateaccount(String updateaccount) {
        this.updateaccount = updateaccount;
    }

    public void setIssuer_code(String issuer_code) {
        this.issuer_code = issuer_code;
    }

    public void setIsvip(String isvip) {
        this.isvip = isvip;
    }

    public void setFullnameen(String fullnameen) {
        this.fullnameen = fullnameen;
    }

    public void setCitycode(String citycode) {
        this.citycode = citycode;
    }

    public void setCompanyid(String companyid) {
        this.companyid = companyid;
    }

    public void setPinyin(String pinyin) {
        this.pinyin = pinyin;
    }

    public void setPinyinfull(String pinyinfull) {
        this.pinyinfull = pinyinfull;
    }

    public void setStc_createtime(String stc_createtime) {
        this.stc_createtime = stc_createtime;
    }

    public void setStc_updatetime(String stc_updatetime) {
        this.stc_updatetime = stc_updatetime;
    }

    public void setBncshortnamelist(String bncshortnamelist) {
        this.bncshortnamelist = bncshortnamelist;
    }

    public void setBcoshortnamelist(String bcoshortnamelist) {
        this.bcoshortnamelist = bcoshortnamelist;
    }

    public void setNcdshortnamelist(String ncdshortnamelist) {
        this.ncdshortnamelist = ncdshortnamelist;
    }

    public void setIrsshortnameList(String irsshortnameList) {
        this.irsshortnameList = irsshortnameList;
    }

    public void setTaglist(String taglist) {
        this.taglist = taglist;
    }

    public void setUseType(String useType) {
        this.useType = useType;
    }

    public void setUseStatus(Integer useStatus) {
        this.useStatus = useStatus;
    }

    public void setCfetsLocalCode(String cfetsLocalCode) {
        this.cfetsLocalCode = cfetsLocalCode;
    }

    public void setCfetsLocalShortName(String cfetsLocalShortName) {
        this.cfetsLocalShortName = cfetsLocalShortName;
    }

    public void setLocationValue(String locationValue) {
        this.locationValue = locationValue;
    }

    public void setIdc_createtime(String idc_createtime) {
        this.idc_createtime = idc_createtime;
    }

    public void setIdc_updatetime(String idc_updatetime) {
        this.idc_updatetime = idc_updatetime;
    }

    public void setSseinstitutioncode(String sseinstitutioncode) {
        this.sseinstitutioncode = sseinstitutioncode;
    }

    public void setSzedealercode(String szedealercode) {
        this.szedealercode = szedealercode;
    }

    public void setSsetradingentitycode(String ssetradingentitycode) {
        this.ssetradingentitycode = ssetradingentitycode;
    }

    public String toString() {
        return "TBrokerCrmInst(idbkey="
                + this.getIdbkey()
                + ", crmid="
                + this.getCrmid()
                + ", id="
                + this.getId()
                + ", shortnamecn="
                + this.getShortnamecn()
                + ", fullnamecn="
                + this.getFullnamecn()
                + ", shortnameen="
                + this.getShortnameen()
                + ", instcode="
                + this.getInstcode()
                + ", cityvalue="
                + this.getCityvalue()
                + ", insttypevalue="
                + this.getInsttypevalue()
                + ", industrytypevalue="
                + this.getIndustrytypevalue()
                + ", fundtypevalue="
                + this.getFundtypevalue()
                + ", countryareavalue="
                + this.getCountryareavalue()
                + ", managerdisplay="
                + this.getManagerdisplay()
                + ", description="
                + this.getDescription()
                + ", contactcount="
                + this.getContactcount()
                + ", createtime="
                + this.getCreatetime()
                + ", updatetime="
                + this.getUpdatetime()
                + ", updateaccount="
                + this.getUpdateaccount()
                + ", issuer_code="
                + this.getIssuer_code()
                + ", isvip="
                + this.getIsvip()
                + ", fullnameen="
                + this.getFullnameen()
                + ", citycode="
                + this.getCitycode()
                + ", companyid="
                + this.getCompanyid()
                + ", pinyin="
                + this.getPinyin()
                + ", pinyinfull="
                + this.getPinyinfull()
                + ", stc_createtime="
                + this.getStc_createtime()
                + ", stc_updatetime="
                + this.getStc_updatetime()
                + ", bncshortnamelist="
                + this.getBncshortnamelist()
                + ", bcoshortnamelist="
                + this.getBcoshortnamelist()
                + ", ncdshortnamelist="
                + this.getNcdshortnamelist()
                + ", irsshortnameList="
                + this.getIrsshortnameList()
                + ", taglist="
                + this.getTaglist()
                + ", useType="
                + this.getUseType()
                + ", useStatus="
                + this.getUseStatus()
                + ", cfetsLocalCode="
                + this.getCfetsLocalCode()
                + ", cfetsLocalShortName="
                + this.getCfetsLocalShortName()
                + ", locationValue="
                + this.getLocationValue()
                + ", idc_createtime="
                + this.getIdc_createtime()
                + ", idc_updatetime="
                + this.getIdc_updatetime()
                + ", sseinstitutioncode="
                + this.getSseinstitutioncode()
                + ", szedealercode="
                + this.getSzedealercode()
                + ", ssetradingentitycode="
                + this.getSsetradingentitycode()
                + ")";
    }
}
