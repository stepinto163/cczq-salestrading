// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api.base.po;

import com.alibaba.fastjson.annotation.JSONField;

import java.math.BigDecimal;
import java.sql.Timestamp;

public class TInstitution {
    @JSONField(name = "ID", serialize = true)
    public String ID = "";

    @JSONField(name = "delflag", serialize = true)
    public String delflag = "";

    @JSONField(name = "create_date", serialize = true)
    public Timestamp create_date;

    @JSONField(name = "modify_date", serialize = true)
    public Timestamp modify_date = new Timestamp(System.currentTimeMillis());

    @JSONField(name = "username", serialize = true)
    public String username;

    @JSONField(name = "inputer", serialize = true)
    public String inputer;

    @JSONField(name = "checker", serialize = true)
    public String checker;

    @JSONField(name = "sts", serialize = true)
    public String sts = "";

    @JSONField(name = "Institution_Code", serialize = true)
    public String Institution_Code;

    @JSONField(name = "Institution_Type", serialize = true)
    public String Institution_Type;

    @JSONField(name = "Institution_Rating", serialize = true)
    public String Institution_Rating;

    @JSONField(name = "Establishment_Date", serialize = true)
    public BigDecimal Establishment_Date;

    @JSONField(name = "Change_Date", serialize = true)
    public BigDecimal Change_Date;

    @JSONField(name = "Full_Name_C", serialize = true)
    public String Full_Name_C;

    @JSONField(name = "Short_Name_C", serialize = true)
    public String Short_Name_C;

    @JSONField(name = "Full_Name_E", serialize = true)
    public String Full_Name_E;

    @JSONField(name = "Short_Name_E", serialize = true)
    public String Short_Name_E;

    @JSONField(name = "Country", serialize = true)
    public String Country;

    @JSONField(name = "Province", serialize = true)
    public String Province;

    @JSONField(name = "City", serialize = true)
    public String City;

    @JSONField(name = "Largest_Shareholder", serialize = true)
    public String Largest_Shareholder;

    @JSONField(name = "Legal_Representative", serialize = true)
    public String Legal_Representative;

    @JSONField(name = "Registered_Capital", serialize = true)
    public String Registered_Capital;

    @JSONField(name = "Registered_Address", serialize = true)
    public String Registered_Address;

    @JSONField(name = "Business_Scope", serialize = true)
    public String Business_Scope;

    @JSONField(name = "Product", serialize = true)
    public String Product;

    @JSONField(name = "Rating_Date", serialize = true)
    public BigDecimal Rating_Date;

    @JSONField(name = "Rating", serialize = true)
    public String Rating;

    @JSONField(name = "Rating_Institution_Code", serialize = true)
    public String Rating_Institution_Code;

    @JSONField(name = "Former_Name", serialize = true)
    public String Former_Name;

    @JSONField(name = "Is_Municipal", serialize = true)
    public String Is_Municipal;

    @JSONField(name = "Issuer_Type", serialize = true)
    public String Issuer_Type;

    @JSONField(name = "Org_Type", serialize = true)
    public String Org_Type;

    @JSONField(name = "SW_Sector", serialize = true)
    public String SW_Sector;

    @JSONField(name = "SW_Subsector", serialize = true)
    public String SW_Subsector;

    @JSONField(name = "Regd_capital", serialize = true)
    public BigDecimal Regd_capital;

    @JSONField(name = "Regd_capital_Currency", serialize = true)
    public String Regd_capital_Currency;

    @JSONField(name = "Stockholder_Name", serialize = true)
    public String Stockholder_Name;

    @JSONField(name = "Stockholder_Type", serialize = true)
    public String Stockholder_Type;

    @JSONField(name = "Stockholding_Percentage", serialize = true)
    public String Stockholding_Percentage;

    @JSONField(name = "Actual_Controller_Name", serialize = true)
    public String Actual_Controller_Name;

    @JSONField(name = "Actual_Controller_Type", serialize = true)
    public String Actual_Controller_Type;

    @JSONField(name = "Ultimate_Actual_Controller_Name", serialize = true)
    public String Ultimate_Actual_Controller_Name;

    @JSONField(name = "Ultimate_Actual_Controller_Type", serialize = true)
    public String Ultimate_Actual_Controller_Type;

    @JSONField(name = "CBRC_Financing_Platform", serialize = true)
    public String CBRC_Financing_Platform;

    @JSONField(name = "Endowment", serialize = true)
    public String Endowment;

    @JSONField(name = "Municipal_Business", serialize = true)
    public String Municipal_Business;

    @JSONField(name = "Pin_Yin", serialize = true)
    public String Pin_Yin;

    @JSONField(name = "Pin_Yin_Full", serialize = true)
    public String Pin_Yin_Full;

    @JSONField(name = "Institution_Subtype", serialize = true)
    public String Institution_Subtype;

    public String getID() {
        return this.ID;
    }

    public String getDelflag() {
        return this.delflag;
    }

    public Timestamp getCreate_date() {
        return this.create_date;
    }

    public Timestamp getModify_date() {
        return this.modify_date;
    }

    public String getUsername() {
        return this.username;
    }

    public String getInputer() {
        return this.inputer;
    }

    public String getChecker() {
        return this.checker;
    }

    public String getSts() {
        return this.sts;
    }

    public String getInstitution_Code() {
        return this.Institution_Code;
    }

    public String getInstitution_Type() {
        return this.Institution_Type;
    }

    public String getInstitution_Rating() {
        return this.Institution_Rating;
    }

    public BigDecimal getEstablishment_Date() {
        return this.Establishment_Date;
    }

    public BigDecimal getChange_Date() {
        return this.Change_Date;
    }

    public String getFull_Name_C() {
        return this.Full_Name_C;
    }

    public String getShort_Name_C() {
        return this.Short_Name_C;
    }

    public String getFull_Name_E() {
        return this.Full_Name_E;
    }

    public String getShort_Name_E() {
        return this.Short_Name_E;
    }

    public String getCountry() {
        return this.Country;
    }

    public String getProvince() {
        return this.Province;
    }

    public String getCity() {
        return this.City;
    }

    public String getLargest_Shareholder() {
        return this.Largest_Shareholder;
    }

    public String getLegal_Representative() {
        return this.Legal_Representative;
    }

    public String getRegistered_Capital() {
        return this.Registered_Capital;
    }

    public String getRegistered_Address() {
        return this.Registered_Address;
    }

    public String getBusiness_Scope() {
        return this.Business_Scope;
    }

    public String getProduct() {
        return this.Product;
    }

    public BigDecimal getRating_Date() {
        return this.Rating_Date;
    }

    public String getRating() {
        return this.Rating;
    }

    public String getRating_Institution_Code() {
        return this.Rating_Institution_Code;
    }

    public String getFormer_Name() {
        return this.Former_Name;
    }

    public String getIs_Municipal() {
        return this.Is_Municipal;
    }

    public String getIssuer_Type() {
        return this.Issuer_Type;
    }

    public String getOrg_Type() {
        return this.Org_Type;
    }

    public String getSW_Sector() {
        return this.SW_Sector;
    }

    public String getSW_Subsector() {
        return this.SW_Subsector;
    }

    public BigDecimal getRegd_capital() {
        return this.Regd_capital;
    }

    public String getRegd_capital_Currency() {
        return this.Regd_capital_Currency;
    }

    public String getStockholder_Name() {
        return this.Stockholder_Name;
    }

    public String getStockholder_Type() {
        return this.Stockholder_Type;
    }

    public String getStockholding_Percentage() {
        return this.Stockholding_Percentage;
    }

    public String getActual_Controller_Name() {
        return this.Actual_Controller_Name;
    }

    public String getActual_Controller_Type() {
        return this.Actual_Controller_Type;
    }

    public String getUltimate_Actual_Controller_Name() {
        return this.Ultimate_Actual_Controller_Name;
    }

    public String getUltimate_Actual_Controller_Type() {
        return this.Ultimate_Actual_Controller_Type;
    }

    public String getCBRC_Financing_Platform() {
        return this.CBRC_Financing_Platform;
    }

    public String getEndowment() {
        return this.Endowment;
    }

    public String getMunicipal_Business() {
        return this.Municipal_Business;
    }

    public String getPin_Yin() {
        return this.Pin_Yin;
    }

    public String getPin_Yin_Full() {
        return this.Pin_Yin_Full;
    }

    public String getInstitution_Subtype() {
        return this.Institution_Subtype;
    }

    public void setID(String ID) {
        this.ID = ID;
    }

    public void setDelflag(String delflag) {
        this.delflag = delflag;
    }

    public void setCreate_date(Timestamp create_date) {
        this.create_date = create_date;
    }

    public void setModify_date(Timestamp modify_date) {
        this.modify_date = modify_date;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public void setInputer(String inputer) {
        this.inputer = inputer;
    }

    public void setChecker(String checker) {
        this.checker = checker;
    }

    public void setSts(String sts) {
        this.sts = sts;
    }

    public void setInstitution_Code(String Institution_Code) {
        this.Institution_Code = Institution_Code;
    }

    public void setInstitution_Type(String Institution_Type) {
        this.Institution_Type = Institution_Type;
    }

    public void setInstitution_Rating(String Institution_Rating) {
        this.Institution_Rating = Institution_Rating;
    }

    public void setEstablishment_Date(BigDecimal Establishment_Date) {
        this.Establishment_Date = Establishment_Date;
    }

    public void setChange_Date(BigDecimal Change_Date) {
        this.Change_Date = Change_Date;
    }

    public void setFull_Name_C(String Full_Name_C) {
        this.Full_Name_C = Full_Name_C;
    }

    public void setShort_Name_C(String Short_Name_C) {
        this.Short_Name_C = Short_Name_C;
    }

    public void setFull_Name_E(String Full_Name_E) {
        this.Full_Name_E = Full_Name_E;
    }

    public void setShort_Name_E(String Short_Name_E) {
        this.Short_Name_E = Short_Name_E;
    }

    public void setCountry(String Country) {
        this.Country = Country;
    }

    public void setProvince(String Province) {
        this.Province = Province;
    }

    public void setCity(String City) {
        this.City = City;
    }

    public void setLargest_Shareholder(String Largest_Shareholder) {
        this.Largest_Shareholder = Largest_Shareholder;
    }

    public void setLegal_Representative(String Legal_Representative) {
        this.Legal_Representative = Legal_Representative;
    }

    public void setRegistered_Capital(String Registered_Capital) {
        this.Registered_Capital = Registered_Capital;
    }

    public void setRegistered_Address(String Registered_Address) {
        this.Registered_Address = Registered_Address;
    }

    public void setBusiness_Scope(String Business_Scope) {
        this.Business_Scope = Business_Scope;
    }

    public void setProduct(String Product) {
        this.Product = Product;
    }

    public void setRating_Date(BigDecimal Rating_Date) {
        this.Rating_Date = Rating_Date;
    }

    public void setRating(String Rating) {
        this.Rating = Rating;
    }

    public void setRating_Institution_Code(String Rating_Institution_Code) {
        this.Rating_Institution_Code = Rating_Institution_Code;
    }

    public void setFormer_Name(String Former_Name) {
        this.Former_Name = Former_Name;
    }

    public void setIs_Municipal(String Is_Municipal) {
        this.Is_Municipal = Is_Municipal;
    }

    public void setIssuer_Type(String Issuer_Type) {
        this.Issuer_Type = Issuer_Type;
    }

    public void setOrg_Type(String Org_Type) {
        this.Org_Type = Org_Type;
    }

    public void setSW_Sector(String SW_Sector) {
        this.SW_Sector = SW_Sector;
    }

    public void setSW_Subsector(String SW_Subsector) {
        this.SW_Subsector = SW_Subsector;
    }

    public void setRegd_capital(BigDecimal Regd_capital) {
        this.Regd_capital = Regd_capital;
    }

    public void setRegd_capital_Currency(String Regd_capital_Currency) {
        this.Regd_capital_Currency = Regd_capital_Currency;
    }

    public void setStockholder_Name(String Stockholder_Name) {
        this.Stockholder_Name = Stockholder_Name;
    }

    public void setStockholder_Type(String Stockholder_Type) {
        this.Stockholder_Type = Stockholder_Type;
    }

    public void setStockholding_Percentage(String Stockholding_Percentage) {
        this.Stockholding_Percentage = Stockholding_Percentage;
    }

    public void setActual_Controller_Name(String Actual_Controller_Name) {
        this.Actual_Controller_Name = Actual_Controller_Name;
    }

    public void setActual_Controller_Type(String Actual_Controller_Type) {
        this.Actual_Controller_Type = Actual_Controller_Type;
    }

    public void setUltimate_Actual_Controller_Name(String Ultimate_Actual_Controller_Name) {
        this.Ultimate_Actual_Controller_Name = Ultimate_Actual_Controller_Name;
    }

    public void setUltimate_Actual_Controller_Type(String Ultimate_Actual_Controller_Type) {
        this.Ultimate_Actual_Controller_Type = Ultimate_Actual_Controller_Type;
    }

    public void setCBRC_Financing_Platform(String CBRC_Financing_Platform) {
        this.CBRC_Financing_Platform = CBRC_Financing_Platform;
    }

    public void setEndowment(String Endowment) {
        this.Endowment = Endowment;
    }

    public void setMunicipal_Business(String Municipal_Business) {
        this.Municipal_Business = Municipal_Business;
    }

    public void setPin_Yin(String Pin_Yin) {
        this.Pin_Yin = Pin_Yin;
    }

    public void setPin_Yin_Full(String Pin_Yin_Full) {
        this.Pin_Yin_Full = Pin_Yin_Full;
    }

    public void setInstitution_Subtype(String Institution_Subtype) {
        this.Institution_Subtype = Institution_Subtype;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof TInstitution)) {
            return false;
        }
        TInstitution other = (TInstitution) o;
        if (!other.canEqual(this)) {
            return false;
        }
        String this$ID = this.getID();
        String other$ID = other.getID();
        if (this$ID == null ? other$ID != null : !this$ID.equals(other$ID)) {
            return false;
        }
        String this$delflag = this.getDelflag();
        String other$delflag = other.getDelflag();
        if (this$delflag == null ? other$delflag != null : !this$delflag.equals(other$delflag)) {
            return false;
        }
        Timestamp this$create_date = this.getCreate_date();
        Timestamp other$create_date = other.getCreate_date();
        if (this$create_date == null
                ? other$create_date != null
                : !((Object) this$create_date).equals(other$create_date)) {
            return false;
        }
        Timestamp this$modify_date = this.getModify_date();
        Timestamp other$modify_date = other.getModify_date();
        if (this$modify_date == null
                ? other$modify_date != null
                : !((Object) this$modify_date).equals(other$modify_date)) {
            return false;
        }
        String this$username = this.getUsername();
        String other$username = other.getUsername();
        if (this$username == null
                ? other$username != null
                : !this$username.equals(other$username)) {
            return false;
        }
        String this$inputer = this.getInputer();
        String other$inputer = other.getInputer();
        if (this$inputer == null ? other$inputer != null : !this$inputer.equals(other$inputer)) {
            return false;
        }
        String this$checker = this.getChecker();
        String other$checker = other.getChecker();
        if (this$checker == null ? other$checker != null : !this$checker.equals(other$checker)) {
            return false;
        }
        String this$sts = this.getSts();
        String other$sts = other.getSts();
        if (this$sts == null ? other$sts != null : !this$sts.equals(other$sts)) {
            return false;
        }
        String this$Institution_Code = this.getInstitution_Code();
        String other$Institution_Code = other.getInstitution_Code();
        if (this$Institution_Code == null
                ? other$Institution_Code != null
                : !this$Institution_Code.equals(other$Institution_Code)) {
            return false;
        }
        String this$Institution_Type = this.getInstitution_Type();
        String other$Institution_Type = other.getInstitution_Type();
        if (this$Institution_Type == null
                ? other$Institution_Type != null
                : !this$Institution_Type.equals(other$Institution_Type)) {
            return false;
        }
        String this$Institution_Rating = this.getInstitution_Rating();
        String other$Institution_Rating = other.getInstitution_Rating();
        if (this$Institution_Rating == null
                ? other$Institution_Rating != null
                : !this$Institution_Rating.equals(other$Institution_Rating)) {
            return false;
        }
        BigDecimal this$Establishment_Date = this.getEstablishment_Date();
        BigDecimal other$Establishment_Date = other.getEstablishment_Date();
        if (this$Establishment_Date == null
                ? other$Establishment_Date != null
                : !((Object) this$Establishment_Date).equals(other$Establishment_Date)) {
            return false;
        }
        BigDecimal this$Change_Date = this.getChange_Date();
        BigDecimal other$Change_Date = other.getChange_Date();
        if (this$Change_Date == null
                ? other$Change_Date != null
                : !((Object) this$Change_Date).equals(other$Change_Date)) {
            return false;
        }
        String this$Full_Name_C = this.getFull_Name_C();
        String other$Full_Name_C = other.getFull_Name_C();
        if (this$Full_Name_C == null
                ? other$Full_Name_C != null
                : !this$Full_Name_C.equals(other$Full_Name_C)) {
            return false;
        }
        String this$Short_Name_C = this.getShort_Name_C();
        String other$Short_Name_C = other.getShort_Name_C();
        if (this$Short_Name_C == null
                ? other$Short_Name_C != null
                : !this$Short_Name_C.equals(other$Short_Name_C)) {
            return false;
        }
        String this$Full_Name_E = this.getFull_Name_E();
        String other$Full_Name_E = other.getFull_Name_E();
        if (this$Full_Name_E == null
                ? other$Full_Name_E != null
                : !this$Full_Name_E.equals(other$Full_Name_E)) {
            return false;
        }
        String this$Short_Name_E = this.getShort_Name_E();
        String other$Short_Name_E = other.getShort_Name_E();
        if (this$Short_Name_E == null
                ? other$Short_Name_E != null
                : !this$Short_Name_E.equals(other$Short_Name_E)) {
            return false;
        }
        String this$Country = this.getCountry();
        String other$Country = other.getCountry();
        if (this$Country == null ? other$Country != null : !this$Country.equals(other$Country)) {
            return false;
        }
        String this$Province = this.getProvince();
        String other$Province = other.getProvince();
        if (this$Province == null
                ? other$Province != null
                : !this$Province.equals(other$Province)) {
            return false;
        }
        String this$City = this.getCity();
        String other$City = other.getCity();
        if (this$City == null ? other$City != null : !this$City.equals(other$City)) {
            return false;
        }
        String this$Largest_Shareholder = this.getLargest_Shareholder();
        String other$Largest_Shareholder = other.getLargest_Shareholder();
        if (this$Largest_Shareholder == null
                ? other$Largest_Shareholder != null
                : !this$Largest_Shareholder.equals(other$Largest_Shareholder)) {
            return false;
        }
        String this$Legal_Representative = this.getLegal_Representative();
        String other$Legal_Representative = other.getLegal_Representative();
        if (this$Legal_Representative == null
                ? other$Legal_Representative != null
                : !this$Legal_Representative.equals(other$Legal_Representative)) {
            return false;
        }
        String this$Registered_Capital = this.getRegistered_Capital();
        String other$Registered_Capital = other.getRegistered_Capital();
        if (this$Registered_Capital == null
                ? other$Registered_Capital != null
                : !this$Registered_Capital.equals(other$Registered_Capital)) {
            return false;
        }
        String this$Registered_Address = this.getRegistered_Address();
        String other$Registered_Address = other.getRegistered_Address();
        if (this$Registered_Address == null
                ? other$Registered_Address != null
                : !this$Registered_Address.equals(other$Registered_Address)) {
            return false;
        }
        String this$Business_Scope = this.getBusiness_Scope();
        String other$Business_Scope = other.getBusiness_Scope();
        if (this$Business_Scope == null
                ? other$Business_Scope != null
                : !this$Business_Scope.equals(other$Business_Scope)) {
            return false;
        }
        String this$Product = this.getProduct();
        String other$Product = other.getProduct();
        if (this$Product == null ? other$Product != null : !this$Product.equals(other$Product)) {
            return false;
        }
        BigDecimal this$Rating_Date = this.getRating_Date();
        BigDecimal other$Rating_Date = other.getRating_Date();
        if (this$Rating_Date == null
                ? other$Rating_Date != null
                : !((Object) this$Rating_Date).equals(other$Rating_Date)) {
            return false;
        }
        String this$Rating = this.getRating();
        String other$Rating = other.getRating();
        if (this$Rating == null ? other$Rating != null : !this$Rating.equals(other$Rating)) {
            return false;
        }
        String this$Rating_Institution_Code = this.getRating_Institution_Code();
        String other$Rating_Institution_Code = other.getRating_Institution_Code();
        if (this$Rating_Institution_Code == null
                ? other$Rating_Institution_Code != null
                : !this$Rating_Institution_Code.equals(other$Rating_Institution_Code)) {
            return false;
        }
        String this$Former_Name = this.getFormer_Name();
        String other$Former_Name = other.getFormer_Name();
        if (this$Former_Name == null
                ? other$Former_Name != null
                : !this$Former_Name.equals(other$Former_Name)) {
            return false;
        }
        String this$Is_Municipal = this.getIs_Municipal();
        String other$Is_Municipal = other.getIs_Municipal();
        if (this$Is_Municipal == null
                ? other$Is_Municipal != null
                : !this$Is_Municipal.equals(other$Is_Municipal)) {
            return false;
        }
        String this$Issuer_Type = this.getIssuer_Type();
        String other$Issuer_Type = other.getIssuer_Type();
        if (this$Issuer_Type == null
                ? other$Issuer_Type != null
                : !this$Issuer_Type.equals(other$Issuer_Type)) {
            return false;
        }
        String this$Org_Type = this.getOrg_Type();
        String other$Org_Type = other.getOrg_Type();
        if (this$Org_Type == null
                ? other$Org_Type != null
                : !this$Org_Type.equals(other$Org_Type)) {
            return false;
        }
        String this$SW_Sector = this.getSW_Sector();
        String other$SW_Sector = other.getSW_Sector();
        if (this$SW_Sector == null
                ? other$SW_Sector != null
                : !this$SW_Sector.equals(other$SW_Sector)) {
            return false;
        }
        String this$SW_Subsector = this.getSW_Subsector();
        String other$SW_Subsector = other.getSW_Subsector();
        if (this$SW_Subsector == null
                ? other$SW_Subsector != null
                : !this$SW_Subsector.equals(other$SW_Subsector)) {
            return false;
        }
        BigDecimal this$Regd_capital = this.getRegd_capital();
        BigDecimal other$Regd_capital = other.getRegd_capital();
        if (this$Regd_capital == null
                ? other$Regd_capital != null
                : !((Object) this$Regd_capital).equals(other$Regd_capital)) {
            return false;
        }
        String this$Regd_capital_Currency = this.getRegd_capital_Currency();
        String other$Regd_capital_Currency = other.getRegd_capital_Currency();
        if (this$Regd_capital_Currency == null
                ? other$Regd_capital_Currency != null
                : !this$Regd_capital_Currency.equals(other$Regd_capital_Currency)) {
            return false;
        }
        String this$Stockholder_Name = this.getStockholder_Name();
        String other$Stockholder_Name = other.getStockholder_Name();
        if (this$Stockholder_Name == null
                ? other$Stockholder_Name != null
                : !this$Stockholder_Name.equals(other$Stockholder_Name)) {
            return false;
        }
        String this$Stockholder_Type = this.getStockholder_Type();
        String other$Stockholder_Type = other.getStockholder_Type();
        if (this$Stockholder_Type == null
                ? other$Stockholder_Type != null
                : !this$Stockholder_Type.equals(other$Stockholder_Type)) {
            return false;
        }
        String this$Stockholding_Percentage = this.getStockholding_Percentage();
        String other$Stockholding_Percentage = other.getStockholding_Percentage();
        if (this$Stockholding_Percentage == null
                ? other$Stockholding_Percentage != null
                : !this$Stockholding_Percentage.equals(other$Stockholding_Percentage)) {
            return false;
        }
        String this$Actual_Controller_Name = this.getActual_Controller_Name();
        String other$Actual_Controller_Name = other.getActual_Controller_Name();
        if (this$Actual_Controller_Name == null
                ? other$Actual_Controller_Name != null
                : !this$Actual_Controller_Name.equals(other$Actual_Controller_Name)) {
            return false;
        }
        String this$Actual_Controller_Type = this.getActual_Controller_Type();
        String other$Actual_Controller_Type = other.getActual_Controller_Type();
        if (this$Actual_Controller_Type == null
                ? other$Actual_Controller_Type != null
                : !this$Actual_Controller_Type.equals(other$Actual_Controller_Type)) {
            return false;
        }
        String this$Ultimate_Actual_Controller_Name = this.getUltimate_Actual_Controller_Name();
        String other$Ultimate_Actual_Controller_Name = other.getUltimate_Actual_Controller_Name();
        if (this$Ultimate_Actual_Controller_Name == null
                ? other$Ultimate_Actual_Controller_Name != null
                : !this$Ultimate_Actual_Controller_Name.equals(
                        other$Ultimate_Actual_Controller_Name)) {
            return false;
        }
        String this$Ultimate_Actual_Controller_Type = this.getUltimate_Actual_Controller_Type();
        String other$Ultimate_Actual_Controller_Type = other.getUltimate_Actual_Controller_Type();
        if (this$Ultimate_Actual_Controller_Type == null
                ? other$Ultimate_Actual_Controller_Type != null
                : !this$Ultimate_Actual_Controller_Type.equals(
                        other$Ultimate_Actual_Controller_Type)) {
            return false;
        }
        String this$CBRC_Financing_Platform = this.getCBRC_Financing_Platform();
        String other$CBRC_Financing_Platform = other.getCBRC_Financing_Platform();
        if (this$CBRC_Financing_Platform == null
                ? other$CBRC_Financing_Platform != null
                : !this$CBRC_Financing_Platform.equals(other$CBRC_Financing_Platform)) {
            return false;
        }
        String this$Endowment = this.getEndowment();
        String other$Endowment = other.getEndowment();
        if (this$Endowment == null
                ? other$Endowment != null
                : !this$Endowment.equals(other$Endowment)) {
            return false;
        }
        String this$Municipal_Business = this.getMunicipal_Business();
        String other$Municipal_Business = other.getMunicipal_Business();
        if (this$Municipal_Business == null
                ? other$Municipal_Business != null
                : !this$Municipal_Business.equals(other$Municipal_Business)) {
            return false;
        }
        String this$Pin_Yin = this.getPin_Yin();
        String other$Pin_Yin = other.getPin_Yin();
        if (this$Pin_Yin == null ? other$Pin_Yin != null : !this$Pin_Yin.equals(other$Pin_Yin)) {
            return false;
        }
        String this$Pin_Yin_Full = this.getPin_Yin_Full();
        String other$Pin_Yin_Full = other.getPin_Yin_Full();
        if (this$Pin_Yin_Full == null
                ? other$Pin_Yin_Full != null
                : !this$Pin_Yin_Full.equals(other$Pin_Yin_Full)) {
            return false;
        }
        String this$Institution_Subtype = this.getInstitution_Subtype();
        String other$Institution_Subtype = other.getInstitution_Subtype();
        return !(this$Institution_Subtype == null
                ? other$Institution_Subtype != null
                : !this$Institution_Subtype.equals(other$Institution_Subtype));
    }

    protected boolean canEqual(Object other) {
        return other instanceof TInstitution;
    }

    public int hashCode() {
        int PRIME = 59;
        int result = 1;
        String $ID = this.getID();
        result = result * 59 + ($ID == null ? 43 : $ID.hashCode());
        String $delflag = this.getDelflag();
        result = result * 59 + ($delflag == null ? 43 : $delflag.hashCode());
        Timestamp $create_date = this.getCreate_date();
        result = result * 59 + ($create_date == null ? 43 : ((Object) $create_date).hashCode());
        Timestamp $modify_date = this.getModify_date();
        result = result * 59 + ($modify_date == null ? 43 : ((Object) $modify_date).hashCode());
        String $username = this.getUsername();
        result = result * 59 + ($username == null ? 43 : $username.hashCode());
        String $inputer = this.getInputer();
        result = result * 59 + ($inputer == null ? 43 : $inputer.hashCode());
        String $checker = this.getChecker();
        result = result * 59 + ($checker == null ? 43 : $checker.hashCode());
        String $sts = this.getSts();
        result = result * 59 + ($sts == null ? 43 : $sts.hashCode());
        String $Institution_Code = this.getInstitution_Code();
        result = result * 59 + ($Institution_Code == null ? 43 : $Institution_Code.hashCode());
        String $Institution_Type = this.getInstitution_Type();
        result = result * 59 + ($Institution_Type == null ? 43 : $Institution_Type.hashCode());
        String $Institution_Rating = this.getInstitution_Rating();
        result = result * 59 + ($Institution_Rating == null ? 43 : $Institution_Rating.hashCode());
        BigDecimal $Establishment_Date = this.getEstablishment_Date();
        result =
                result * 59
                        + ($Establishment_Date == null
                                ? 43
                                : ((Object) $Establishment_Date).hashCode());
        BigDecimal $Change_Date = this.getChange_Date();
        result = result * 59 + ($Change_Date == null ? 43 : ((Object) $Change_Date).hashCode());
        String $Full_Name_C = this.getFull_Name_C();
        result = result * 59 + ($Full_Name_C == null ? 43 : $Full_Name_C.hashCode());
        String $Short_Name_C = this.getShort_Name_C();
        result = result * 59 + ($Short_Name_C == null ? 43 : $Short_Name_C.hashCode());
        String $Full_Name_E = this.getFull_Name_E();
        result = result * 59 + ($Full_Name_E == null ? 43 : $Full_Name_E.hashCode());
        String $Short_Name_E = this.getShort_Name_E();
        result = result * 59 + ($Short_Name_E == null ? 43 : $Short_Name_E.hashCode());
        String $Country = this.getCountry();
        result = result * 59 + ($Country == null ? 43 : $Country.hashCode());
        String $Province = this.getProvince();
        result = result * 59 + ($Province == null ? 43 : $Province.hashCode());
        String $City = this.getCity();
        result = result * 59 + ($City == null ? 43 : $City.hashCode());
        String $Largest_Shareholder = this.getLargest_Shareholder();
        result =
                result * 59 + ($Largest_Shareholder == null ? 43 : $Largest_Shareholder.hashCode());
        String $Legal_Representative = this.getLegal_Representative();
        result =
                result * 59
                        + ($Legal_Representative == null ? 43 : $Legal_Representative.hashCode());
        String $Registered_Capital = this.getRegistered_Capital();
        result = result * 59 + ($Registered_Capital == null ? 43 : $Registered_Capital.hashCode());
        String $Registered_Address = this.getRegistered_Address();
        result = result * 59 + ($Registered_Address == null ? 43 : $Registered_Address.hashCode());
        String $Business_Scope = this.getBusiness_Scope();
        result = result * 59 + ($Business_Scope == null ? 43 : $Business_Scope.hashCode());
        String $Product = this.getProduct();
        result = result * 59 + ($Product == null ? 43 : $Product.hashCode());
        BigDecimal $Rating_Date = this.getRating_Date();
        result = result * 59 + ($Rating_Date == null ? 43 : ((Object) $Rating_Date).hashCode());
        String $Rating = this.getRating();
        result = result * 59 + ($Rating == null ? 43 : $Rating.hashCode());
        String $Rating_Institution_Code = this.getRating_Institution_Code();
        result =
                result * 59
                        + ($Rating_Institution_Code == null
                                ? 43
                                : $Rating_Institution_Code.hashCode());
        String $Former_Name = this.getFormer_Name();
        result = result * 59 + ($Former_Name == null ? 43 : $Former_Name.hashCode());
        String $Is_Municipal = this.getIs_Municipal();
        result = result * 59 + ($Is_Municipal == null ? 43 : $Is_Municipal.hashCode());
        String $Issuer_Type = this.getIssuer_Type();
        result = result * 59 + ($Issuer_Type == null ? 43 : $Issuer_Type.hashCode());
        String $Org_Type = this.getOrg_Type();
        result = result * 59 + ($Org_Type == null ? 43 : $Org_Type.hashCode());
        String $SW_Sector = this.getSW_Sector();
        result = result * 59 + ($SW_Sector == null ? 43 : $SW_Sector.hashCode());
        String $SW_Subsector = this.getSW_Subsector();
        result = result * 59 + ($SW_Subsector == null ? 43 : $SW_Subsector.hashCode());
        BigDecimal $Regd_capital = this.getRegd_capital();
        result = result * 59 + ($Regd_capital == null ? 43 : ((Object) $Regd_capital).hashCode());
        String $Regd_capital_Currency = this.getRegd_capital_Currency();
        result =
                result * 59
                        + ($Regd_capital_Currency == null ? 43 : $Regd_capital_Currency.hashCode());
        String $Stockholder_Name = this.getStockholder_Name();
        result = result * 59 + ($Stockholder_Name == null ? 43 : $Stockholder_Name.hashCode());
        String $Stockholder_Type = this.getStockholder_Type();
        result = result * 59 + ($Stockholder_Type == null ? 43 : $Stockholder_Type.hashCode());
        String $Stockholding_Percentage = this.getStockholding_Percentage();
        result =
                result * 59
                        + ($Stockholding_Percentage == null
                                ? 43
                                : $Stockholding_Percentage.hashCode());
        String $Actual_Controller_Name = this.getActual_Controller_Name();
        result =
                result * 59
                        + ($Actual_Controller_Name == null
                                ? 43
                                : $Actual_Controller_Name.hashCode());
        String $Actual_Controller_Type = this.getActual_Controller_Type();
        result =
                result * 59
                        + ($Actual_Controller_Type == null
                                ? 43
                                : $Actual_Controller_Type.hashCode());
        String $Ultimate_Actual_Controller_Name = this.getUltimate_Actual_Controller_Name();
        result =
                result * 59
                        + ($Ultimate_Actual_Controller_Name == null
                                ? 43
                                : $Ultimate_Actual_Controller_Name.hashCode());
        String $Ultimate_Actual_Controller_Type = this.getUltimate_Actual_Controller_Type();
        result =
                result * 59
                        + ($Ultimate_Actual_Controller_Type == null
                                ? 43
                                : $Ultimate_Actual_Controller_Type.hashCode());
        String $CBRC_Financing_Platform = this.getCBRC_Financing_Platform();
        result =
                result * 59
                        + ($CBRC_Financing_Platform == null
                                ? 43
                                : $CBRC_Financing_Platform.hashCode());
        String $Endowment = this.getEndowment();
        result = result * 59 + ($Endowment == null ? 43 : $Endowment.hashCode());
        String $Municipal_Business = this.getMunicipal_Business();
        result = result * 59 + ($Municipal_Business == null ? 43 : $Municipal_Business.hashCode());
        String $Pin_Yin = this.getPin_Yin();
        result = result * 59 + ($Pin_Yin == null ? 43 : $Pin_Yin.hashCode());
        String $Pin_Yin_Full = this.getPin_Yin_Full();
        result = result * 59 + ($Pin_Yin_Full == null ? 43 : $Pin_Yin_Full.hashCode());
        String $Institution_Subtype = this.getInstitution_Subtype();
        result =
                result * 59 + ($Institution_Subtype == null ? 43 : $Institution_Subtype.hashCode());
        return result;
    }

    public String toString() {
        return "TInstitution(ID="
                + this.getID()
                + ", delflag="
                + this.getDelflag()
                + ", create_date="
                + this.getCreate_date()
                + ", modify_date="
                + this.getModify_date()
                + ", username="
                + this.getUsername()
                + ", inputer="
                + this.getInputer()
                + ", checker="
                + this.getChecker()
                + ", sts="
                + this.getSts()
                + ", Institution_Code="
                + this.getInstitution_Code()
                + ", Institution_Type="
                + this.getInstitution_Type()
                + ", Institution_Rating="
                + this.getInstitution_Rating()
                + ", Establishment_Date="
                + this.getEstablishment_Date()
                + ", Change_Date="
                + this.getChange_Date()
                + ", Full_Name_C="
                + this.getFull_Name_C()
                + ", Short_Name_C="
                + this.getShort_Name_C()
                + ", Full_Name_E="
                + this.getFull_Name_E()
                + ", Short_Name_E="
                + this.getShort_Name_E()
                + ", Country="
                + this.getCountry()
                + ", Province="
                + this.getProvince()
                + ", City="
                + this.getCity()
                + ", Largest_Shareholder="
                + this.getLargest_Shareholder()
                + ", Legal_Representative="
                + this.getLegal_Representative()
                + ", Registered_Capital="
                + this.getRegistered_Capital()
                + ", Registered_Address="
                + this.getRegistered_Address()
                + ", Business_Scope="
                + this.getBusiness_Scope()
                + ", Product="
                + this.getProduct()
                + ", Rating_Date="
                + this.getRating_Date()
                + ", Rating="
                + this.getRating()
                + ", Rating_Institution_Code="
                + this.getRating_Institution_Code()
                + ", Former_Name="
                + this.getFormer_Name()
                + ", Is_Municipal="
                + this.getIs_Municipal()
                + ", Issuer_Type="
                + this.getIssuer_Type()
                + ", Org_Type="
                + this.getOrg_Type()
                + ", SW_Sector="
                + this.getSW_Sector()
                + ", SW_Subsector="
                + this.getSW_Subsector()
                + ", Regd_capital="
                + this.getRegd_capital()
                + ", Regd_capital_Currency="
                + this.getRegd_capital_Currency()
                + ", Stockholder_Name="
                + this.getStockholder_Name()
                + ", Stockholder_Type="
                + this.getStockholder_Type()
                + ", Stockholding_Percentage="
                + this.getStockholding_Percentage()
                + ", Actual_Controller_Name="
                + this.getActual_Controller_Name()
                + ", Actual_Controller_Type="
                + this.getActual_Controller_Type()
                + ", Ultimate_Actual_Controller_Name="
                + this.getUltimate_Actual_Controller_Name()
                + ", Ultimate_Actual_Controller_Type="
                + this.getUltimate_Actual_Controller_Type()
                + ", CBRC_Financing_Platform="
                + this.getCBRC_Financing_Platform()
                + ", Endowment="
                + this.getEndowment()
                + ", Municipal_Business="
                + this.getMunicipal_Business()
                + ", Pin_Yin="
                + this.getPin_Yin()
                + ", Pin_Yin_Full="
                + this.getPin_Yin_Full()
                + ", Institution_Subtype="
                + this.getInstitution_Subtype()
                + ")";
    }
}
