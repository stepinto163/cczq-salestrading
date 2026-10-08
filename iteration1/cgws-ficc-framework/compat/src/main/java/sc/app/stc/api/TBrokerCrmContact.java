// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import com.alibaba.fastjson.annotation.JSONField;

import java.util.Objects;

public class TBrokerCrmContact {
    @JSONField(name = "idbkey", serialize = true)
    public String idbkey = "";

    @JSONField(name = "crmid", serialize = true)
    public String crmid;

    @JSONField(name = "id", serialize = true)
    public String id;

    @JSONField(name = "companyid", serialize = true)
    public String companyid;

    @JSONField(name = "namecn", serialize = true)
    public String namecn;

    @JSONField(name = "pinyin", serialize = true)
    public String pinyin;

    @JSONField(name = "pinyinfull", serialize = true)
    public String pinyinfull;

    @JSONField(name = "nameen", serialize = true)
    public String nameen;

    @JSONField(name = "instid", serialize = true)
    public String instid;

    @JSONField(name = "instcode", serialize = true)
    public String instcode;

    @JSONField(name = "instname", serialize = true)
    public String instname;

    @JSONField(name = "insttypevalue", serialize = true)
    public String insttypevalue;

    @JSONField(name = "instcityvalue", serialize = true)
    public String instcityvalue;

    @JSONField(name = "ctctdept", serialize = true)
    public String ctctdept;

    @JSONField(name = "ctctdeptvalue", serialize = true)
    public String ctctdeptvalue;

    @JSONField(name = "ctctposition", serialize = true)
    public String ctctposition;

    @JSONField(name = "ctctpositionvalue", serialize = true)
    public String ctctpositionvalue;

    @JSONField(name = "ctctcode", serialize = true)
    public String ctctcode;

    @JSONField(name = "biztype", serialize = true)
    public String biztype;

    @JSONField(name = "biztypevalue", serialize = true)
    public String biztypevalue;

    @JSONField(name = "gender", serialize = true)
    public String gender;

    @JSONField(name = "birthday", serialize = true)
    public String birthday;

    @JSONField(name = "university", serialize = true)
    public String university;

    @JSONField(name = "commaddress", serialize = true)
    public String commaddress;

    @JSONField(name = "homeaddress", serialize = true)
    public String homeaddress;

    @JSONField(name = "hobby", serialize = true)
    public String hobby;

    @JSONField(name = "maillist", serialize = true)
    public String maillist;

    @JSONField(name = "mobilelist", serialize = true)
    public String mobilelist;

    @JSONField(name = "telephonelist", serialize = true)
    public String telephonelist;

    @JSONField(name = "faxlist", serialize = true)
    public String faxlist;

    @JSONField(name = "qqlist", serialize = true)
    public String qqlist;

    @JSONField(name = "tags", serialize = true)
    public String tags;

    @JSONField(name = "createtime", serialize = true)
    public String createtime;

    @JSONField(name = "updatetime", serialize = true)
    public String updatetime;

    @JSONField(name = "updateaccount", serialize = true)
    public String updateaccount;

    @JSONField(name = "contact_qb_id", serialize = true)
    public String contact_qb_id;

    @JSONField(name = "inst_qb_id", serialize = true)
    public String inst_qb_id;

    @JSONField(name = "is_vip", serialize = true)
    public String is_vip;

    @JSONField(name = "isonboard", serialize = true)
    public String isonboard;

    @JSONField(name = "managers", serialize = true)
    public String managers;

    @JSONField(name = "agentManagers", serialize = true)
    public String agentManagers;

    @JSONField(name = "idc_createtime", serialize = true)
    public String idc_createtime;

    @JSONField(name = "idc_updatetime", serialize = true)
    public String idc_updatetime;

    @JSONField(name = "stc_createtime", serialize = true)
    public String stc_createtime;

    @JSONField(name = "stc_updatetime", serialize = true)
    public String stc_updatetime;

    @JSONField(name = "term_ids", serialize = true)
    public String term_ids;

    @JSONField(name = "username", serialize = true)
    public String username;

    @JSONField(name = "password", serialize = true)
    public String password;

    @JSONField(name = "roleid", serialize = true)
    public String roleid;

    @JSONField(name = "rolename", serialize = true)
    public String rolename;

    @JSONField(name = "mac", serialize = true)
    public String mac;

    @JSONField(name = "enable", serialize = true)
    public String enable;

    @JSONField(name = "stcuser", serialize = true)
    public String stcuser;

    @JSONField(name = "iskyc", serialize = true)
    public String iskyc;

    @JSONField(name = "instfullnamecn", serialize = true)
    public String instfullnamecn;

    @JSONField(name = "market", serialize = true)
    public String market;

    @JSONField(name = "commonuse", serialize = true)
    public String commonuse;

    public boolean equals(Object o) {
        if (this == o) {
            return true;
        }
        if (o == null || this.getClass() != o.getClass()) {
            return false;
        }
        TBrokerCrmContact that = (TBrokerCrmContact) o;
        return Objects.equals(this.idbkey, that.idbkey)
                && Objects.equals(this.crmid, that.crmid)
                && Objects.equals(this.namecn, that.namecn)
                && Objects.equals(this.pinyin, that.pinyin)
                && Objects.equals(this.pinyinfull, that.pinyinfull)
                && Objects.equals(this.instid, that.instid)
                && Objects.equals(this.instcode, that.instcode)
                && Objects.equals(this.instname, that.instname)
                && Objects.equals(this.ctctcode, that.ctctcode)
                && Objects.equals(this.biztype, that.biztype)
                && Objects.equals(this.qqlist, that.qqlist);
    }

    public int hashCode() {
        return Objects.hash(
                this.idbkey,
                this.crmid,
                this.namecn,
                this.pinyin,
                this.pinyinfull,
                this.instid,
                this.instcode,
                this.instname,
                this.ctctcode,
                this.biztype,
                this.qqlist);
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

    public String getCompanyid() {
        return this.companyid;
    }

    public String getNamecn() {
        return this.namecn;
    }

    public String getPinyin() {
        return this.pinyin;
    }

    public String getPinyinfull() {
        return this.pinyinfull;
    }

    public String getNameen() {
        return this.nameen;
    }

    public String getInstid() {
        return this.instid;
    }

    public String getInstcode() {
        return this.instcode;
    }

    public String getInstname() {
        return this.instname;
    }

    public String getInsttypevalue() {
        return this.insttypevalue;
    }

    public String getInstcityvalue() {
        return this.instcityvalue;
    }

    public String getCtctdept() {
        return this.ctctdept;
    }

    public String getCtctdeptvalue() {
        return this.ctctdeptvalue;
    }

    public String getCtctposition() {
        return this.ctctposition;
    }

    public String getCtctpositionvalue() {
        return this.ctctpositionvalue;
    }

    public String getCtctcode() {
        return this.ctctcode;
    }

    public String getBiztype() {
        return this.biztype;
    }

    public String getBiztypevalue() {
        return this.biztypevalue;
    }

    public String getGender() {
        return this.gender;
    }

    public String getBirthday() {
        return this.birthday;
    }

    public String getUniversity() {
        return this.university;
    }

    public String getCommaddress() {
        return this.commaddress;
    }

    public String getHomeaddress() {
        return this.homeaddress;
    }

    public String getHobby() {
        return this.hobby;
    }

    public String getMaillist() {
        return this.maillist;
    }

    public String getMobilelist() {
        return this.mobilelist;
    }

    public String getTelephonelist() {
        return this.telephonelist;
    }

    public String getFaxlist() {
        return this.faxlist;
    }

    public String getQqlist() {
        return this.qqlist;
    }

    public String getTags() {
        return this.tags;
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

    public String getContact_qb_id() {
        return this.contact_qb_id;
    }

    public String getInst_qb_id() {
        return this.inst_qb_id;
    }

    public String getIs_vip() {
        return this.is_vip;
    }

    public String getIsonboard() {
        return this.isonboard;
    }

    public String getManagers() {
        return this.managers;
    }

    public String getAgentManagers() {
        return this.agentManagers;
    }

    public String getIdc_createtime() {
        return this.idc_createtime;
    }

    public String getIdc_updatetime() {
        return this.idc_updatetime;
    }

    public String getStc_createtime() {
        return this.stc_createtime;
    }

    public String getStc_updatetime() {
        return this.stc_updatetime;
    }

    public String getTerm_ids() {
        return this.term_ids;
    }

    public String getUsername() {
        return this.username;
    }

    public String getPassword() {
        return this.password;
    }

    public String getRoleid() {
        return this.roleid;
    }

    public String getRolename() {
        return this.rolename;
    }

    public String getMac() {
        return this.mac;
    }

    public String getEnable() {
        return this.enable;
    }

    public String getStcuser() {
        return this.stcuser;
    }

    public String getIskyc() {
        return this.iskyc;
    }

    public String getInstfullnamecn() {
        return this.instfullnamecn;
    }

    public String getMarket() {
        return this.market;
    }

    public String getCommonuse() {
        return this.commonuse;
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

    public void setCompanyid(String companyid) {
        this.companyid = companyid;
    }

    public void setNamecn(String namecn) {
        this.namecn = namecn;
    }

    public void setPinyin(String pinyin) {
        this.pinyin = pinyin;
    }

    public void setPinyinfull(String pinyinfull) {
        this.pinyinfull = pinyinfull;
    }

    public void setNameen(String nameen) {
        this.nameen = nameen;
    }

    public void setInstid(String instid) {
        this.instid = instid;
    }

    public void setInstcode(String instcode) {
        this.instcode = instcode;
    }

    public void setInstname(String instname) {
        this.instname = instname;
    }

    public void setInsttypevalue(String insttypevalue) {
        this.insttypevalue = insttypevalue;
    }

    public void setInstcityvalue(String instcityvalue) {
        this.instcityvalue = instcityvalue;
    }

    public void setCtctdept(String ctctdept) {
        this.ctctdept = ctctdept;
    }

    public void setCtctdeptvalue(String ctctdeptvalue) {
        this.ctctdeptvalue = ctctdeptvalue;
    }

    public void setCtctposition(String ctctposition) {
        this.ctctposition = ctctposition;
    }

    public void setCtctpositionvalue(String ctctpositionvalue) {
        this.ctctpositionvalue = ctctpositionvalue;
    }

    public void setCtctcode(String ctctcode) {
        this.ctctcode = ctctcode;
    }

    public void setBiztype(String biztype) {
        this.biztype = biztype;
    }

    public void setBiztypevalue(String biztypevalue) {
        this.biztypevalue = biztypevalue;
    }

    public void setGender(String gender) {
        this.gender = gender;
    }

    public void setBirthday(String birthday) {
        this.birthday = birthday;
    }

    public void setUniversity(String university) {
        this.university = university;
    }

    public void setCommaddress(String commaddress) {
        this.commaddress = commaddress;
    }

    public void setHomeaddress(String homeaddress) {
        this.homeaddress = homeaddress;
    }

    public void setHobby(String hobby) {
        this.hobby = hobby;
    }

    public void setMaillist(String maillist) {
        this.maillist = maillist;
    }

    public void setMobilelist(String mobilelist) {
        this.mobilelist = mobilelist;
    }

    public void setTelephonelist(String telephonelist) {
        this.telephonelist = telephonelist;
    }

    public void setFaxlist(String faxlist) {
        this.faxlist = faxlist;
    }

    public void setQqlist(String qqlist) {
        this.qqlist = qqlist;
    }

    public void setTags(String tags) {
        this.tags = tags;
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

    public void setContact_qb_id(String contact_qb_id) {
        this.contact_qb_id = contact_qb_id;
    }

    public void setInst_qb_id(String inst_qb_id) {
        this.inst_qb_id = inst_qb_id;
    }

    public void setIs_vip(String is_vip) {
        this.is_vip = is_vip;
    }

    public void setIsonboard(String isonboard) {
        this.isonboard = isonboard;
    }

    public void setManagers(String managers) {
        this.managers = managers;
    }

    public void setAgentManagers(String agentManagers) {
        this.agentManagers = agentManagers;
    }

    public void setIdc_createtime(String idc_createtime) {
        this.idc_createtime = idc_createtime;
    }

    public void setIdc_updatetime(String idc_updatetime) {
        this.idc_updatetime = idc_updatetime;
    }

    public void setStc_createtime(String stc_createtime) {
        this.stc_createtime = stc_createtime;
    }

    public void setStc_updatetime(String stc_updatetime) {
        this.stc_updatetime = stc_updatetime;
    }

    public void setTerm_ids(String term_ids) {
        this.term_ids = term_ids;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public void setPassword(String password) {
        this.password = password;
    }

    public void setRoleid(String roleid) {
        this.roleid = roleid;
    }

    public void setRolename(String rolename) {
        this.rolename = rolename;
    }

    public void setMac(String mac) {
        this.mac = mac;
    }

    public void setEnable(String enable) {
        this.enable = enable;
    }

    public void setStcuser(String stcuser) {
        this.stcuser = stcuser;
    }

    public void setIskyc(String iskyc) {
        this.iskyc = iskyc;
    }

    public void setInstfullnamecn(String instfullnamecn) {
        this.instfullnamecn = instfullnamecn;
    }

    public void setMarket(String market) {
        this.market = market;
    }

    public void setCommonuse(String commonuse) {
        this.commonuse = commonuse;
    }

    public String toString() {
        return "TBrokerCrmContact(idbkey="
                + this.getIdbkey()
                + ", crmid="
                + this.getCrmid()
                + ", id="
                + this.getId()
                + ", companyid="
                + this.getCompanyid()
                + ", namecn="
                + this.getNamecn()
                + ", pinyin="
                + this.getPinyin()
                + ", pinyinfull="
                + this.getPinyinfull()
                + ", nameen="
                + this.getNameen()
                + ", instid="
                + this.getInstid()
                + ", instcode="
                + this.getInstcode()
                + ", instname="
                + this.getInstname()
                + ", insttypevalue="
                + this.getInsttypevalue()
                + ", instcityvalue="
                + this.getInstcityvalue()
                + ", ctctdept="
                + this.getCtctdept()
                + ", ctctdeptvalue="
                + this.getCtctdeptvalue()
                + ", ctctposition="
                + this.getCtctposition()
                + ", ctctpositionvalue="
                + this.getCtctpositionvalue()
                + ", ctctcode="
                + this.getCtctcode()
                + ", biztype="
                + this.getBiztype()
                + ", biztypevalue="
                + this.getBiztypevalue()
                + ", gender="
                + this.getGender()
                + ", birthday="
                + this.getBirthday()
                + ", university="
                + this.getUniversity()
                + ", commaddress="
                + this.getCommaddress()
                + ", homeaddress="
                + this.getHomeaddress()
                + ", hobby="
                + this.getHobby()
                + ", maillist="
                + this.getMaillist()
                + ", mobilelist="
                + this.getMobilelist()
                + ", telephonelist="
                + this.getTelephonelist()
                + ", faxlist="
                + this.getFaxlist()
                + ", qqlist="
                + this.getQqlist()
                + ", tags="
                + this.getTags()
                + ", createtime="
                + this.getCreatetime()
                + ", updatetime="
                + this.getUpdatetime()
                + ", updateaccount="
                + this.getUpdateaccount()
                + ", contact_qb_id="
                + this.getContact_qb_id()
                + ", inst_qb_id="
                + this.getInst_qb_id()
                + ", is_vip="
                + this.getIs_vip()
                + ", isonboard="
                + this.getIsonboard()
                + ", managers="
                + this.getManagers()
                + ", agentManagers="
                + this.getAgentManagers()
                + ", idc_createtime="
                + this.getIdc_createtime()
                + ", idc_updatetime="
                + this.getIdc_updatetime()
                + ", stc_createtime="
                + this.getStc_createtime()
                + ", stc_updatetime="
                + this.getStc_updatetime()
                + ", term_ids="
                + this.getTerm_ids()
                + ", username="
                + this.getUsername()
                + ", password="
                + this.getPassword()
                + ", roleid="
                + this.getRoleid()
                + ", rolename="
                + this.getRolename()
                + ", mac="
                + this.getMac()
                + ", enable="
                + this.getEnable()
                + ", stcuser="
                + this.getStcuser()
                + ", iskyc="
                + this.getIskyc()
                + ", instfullnamecn="
                + this.getInstfullnamecn()
                + ", market="
                + this.getMarket()
                + ", commonuse="
                + this.getCommonuse()
                + ")";
    }
}
