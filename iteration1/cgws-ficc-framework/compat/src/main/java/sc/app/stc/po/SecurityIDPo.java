// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.po;

import sc.app.stc.annotation.Column4Reader;
import sc.app.stc.annotation.Column4Writer;

public class SecurityIDPo {
    @Column4Reader(value = "SecurityID")
    @Column4Writer(value = "SecurityID")
    private String securityId;

    @Column4Reader(value = "IMIXSecurityID")
    @Column4Writer(value = "IMIXSecurityID")
    private String imixSecurityId;

    @Column4Reader(value = "SecurityDec")
    @Column4Writer(value = "SecurityDec")
    private String securityDec;

    @Column4Reader(value = "SecurityTypeId")
    @Column4Writer(value = "SecurityTypeId")
    private String securityTypeId;

    @Column4Reader(value = "TermToMaturityString")
    @Column4Writer(value = "TermToMaturityString")
    private String termToMaturity;

    @Column4Reader(value = "TradePublishIndicator")
    @Column4Writer(value = "TradePublishIndicator")
    private String tradePublishIndicator;

    @Column4Reader(value = "Issuer")
    @Column4Writer(value = "Issuer")
    private String issuer;

    @Column4Reader(value = "Symbol")
    @Column4Writer(value = "Symbol")
    private String symbol;

    @Column4Reader(value = "Flag")
    @Column4Writer(value = "Flag")
    private String flag;

    public String getSecurityId() {
        return this.securityId;
    }

    public String getImixSecurityId() {
        return this.imixSecurityId;
    }

    public String getSecurityDec() {
        return this.securityDec;
    }

    public String getSecurityTypeId() {
        return this.securityTypeId;
    }

    public String getTermToMaturity() {
        return this.termToMaturity;
    }

    public String getTradePublishIndicator() {
        return this.tradePublishIndicator;
    }

    public String getIssuer() {
        return this.issuer;
    }

    public String getSymbol() {
        return this.symbol;
    }

    public String getFlag() {
        return this.flag;
    }

    public void setSecurityId(String securityId) {
        this.securityId = securityId;
    }

    public void setImixSecurityId(String imixSecurityId) {
        this.imixSecurityId = imixSecurityId;
    }

    public void setSecurityDec(String securityDec) {
        this.securityDec = securityDec;
    }

    public void setSecurityTypeId(String securityTypeId) {
        this.securityTypeId = securityTypeId;
    }

    public void setTermToMaturity(String termToMaturity) {
        this.termToMaturity = termToMaturity;
    }

    public void setTradePublishIndicator(String tradePublishIndicator) {
        this.tradePublishIndicator = tradePublishIndicator;
    }

    public void setIssuer(String issuer) {
        this.issuer = issuer;
    }

    public void setSymbol(String symbol) {
        this.symbol = symbol;
    }

    public void setFlag(String flag) {
        this.flag = flag;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof SecurityIDPo)) {
            return false;
        }
        SecurityIDPo other = (SecurityIDPo) o;
        if (!other.canEqual(this)) {
            return false;
        }
        String this$securityId = this.getSecurityId();
        String other$securityId = other.getSecurityId();
        if (this$securityId == null
                ? other$securityId != null
                : !this$securityId.equals(other$securityId)) {
            return false;
        }
        String this$imixSecurityId = this.getImixSecurityId();
        String other$imixSecurityId = other.getImixSecurityId();
        if (this$imixSecurityId == null
                ? other$imixSecurityId != null
                : !this$imixSecurityId.equals(other$imixSecurityId)) {
            return false;
        }
        String this$securityDec = this.getSecurityDec();
        String other$securityDec = other.getSecurityDec();
        if (this$securityDec == null
                ? other$securityDec != null
                : !this$securityDec.equals(other$securityDec)) {
            return false;
        }
        String this$securityTypeId = this.getSecurityTypeId();
        String other$securityTypeId = other.getSecurityTypeId();
        if (this$securityTypeId == null
                ? other$securityTypeId != null
                : !this$securityTypeId.equals(other$securityTypeId)) {
            return false;
        }
        String this$termToMaturity = this.getTermToMaturity();
        String other$termToMaturity = other.getTermToMaturity();
        if (this$termToMaturity == null
                ? other$termToMaturity != null
                : !this$termToMaturity.equals(other$termToMaturity)) {
            return false;
        }
        String this$tradePublishIndicator = this.getTradePublishIndicator();
        String other$tradePublishIndicator = other.getTradePublishIndicator();
        if (this$tradePublishIndicator == null
                ? other$tradePublishIndicator != null
                : !this$tradePublishIndicator.equals(other$tradePublishIndicator)) {
            return false;
        }
        String this$issuer = this.getIssuer();
        String other$issuer = other.getIssuer();
        if (this$issuer == null ? other$issuer != null : !this$issuer.equals(other$issuer)) {
            return false;
        }
        String this$symbol = this.getSymbol();
        String other$symbol = other.getSymbol();
        if (this$symbol == null ? other$symbol != null : !this$symbol.equals(other$symbol)) {
            return false;
        }
        String this$flag = this.getFlag();
        String other$flag = other.getFlag();
        return !(this$flag == null ? other$flag != null : !this$flag.equals(other$flag));
    }

    protected boolean canEqual(Object other) {
        return other instanceof SecurityIDPo;
    }

    public int hashCode() {
        int PRIME = 59;
        int result = 1;
        String $securityId = this.getSecurityId();
        result = result * 59 + ($securityId == null ? 43 : $securityId.hashCode());
        String $imixSecurityId = this.getImixSecurityId();
        result = result * 59 + ($imixSecurityId == null ? 43 : $imixSecurityId.hashCode());
        String $securityDec = this.getSecurityDec();
        result = result * 59 + ($securityDec == null ? 43 : $securityDec.hashCode());
        String $securityTypeId = this.getSecurityTypeId();
        result = result * 59 + ($securityTypeId == null ? 43 : $securityTypeId.hashCode());
        String $termToMaturity = this.getTermToMaturity();
        result = result * 59 + ($termToMaturity == null ? 43 : $termToMaturity.hashCode());
        String $tradePublishIndicator = this.getTradePublishIndicator();
        result =
                result * 59
                        + ($tradePublishIndicator == null ? 43 : $tradePublishIndicator.hashCode());
        String $issuer = this.getIssuer();
        result = result * 59 + ($issuer == null ? 43 : $issuer.hashCode());
        String $symbol = this.getSymbol();
        result = result * 59 + ($symbol == null ? 43 : $symbol.hashCode());
        String $flag = this.getFlag();
        result = result * 59 + ($flag == null ? 43 : $flag.hashCode());
        return result;
    }

    public String toString() {
        return "SecurityIDPo(securityId="
                + this.getSecurityId()
                + ", imixSecurityId="
                + this.getImixSecurityId()
                + ", securityDec="
                + this.getSecurityDec()
                + ", securityTypeId="
                + this.getSecurityTypeId()
                + ", termToMaturity="
                + this.getTermToMaturity()
                + ", tradePublishIndicator="
                + this.getTradePublishIndicator()
                + ", issuer="
                + this.getIssuer()
                + ", symbol="
                + this.getSymbol()
                + ", flag="
                + this.getFlag()
                + ")";
    }
}
