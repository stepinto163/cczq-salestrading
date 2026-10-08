package cgws.ficc.parser.model;

import java.util.ArrayList;
import java.util.List;

/**
 * Result record exposed to the service layer; field names kept identical to
 * {@code com.sumscope.BondQuoteParser$BondQuote} so ParserSvr / ParserManager code keeps compiling.
 */
public class BondQuote implements Cloneable {

    public String Text = "";
    public int lineNo;
    public String Direction = "";
    public String Code = "";
    public String Key = "";
    public String Name = "";
    public String Volume = "";
    public String Price = "";
    public String PriceTag = "";
    public String Notes = "";
    public boolean hasOption;
    public String netPrice = "";
    public boolean IsInternal;
    public int probolity = 100;
    public String tradeName = "";
    public String tradeDay = "";
    public String settlementDay = "";
    /** 0 = yield, 1 = net price, 2 = full price (legacy priceType semantics). */
    public int priceType;
    public int expiryType;
    public List<String> otherInfos = new ArrayList<String>();
    public String BondType = "";

    @Override
    public BondQuote clone() {
        try {
            BondQuote c = (BondQuote) super.clone();
            c.otherInfos = new ArrayList<String>(otherInfos);
            return c;
        } catch (CloneNotSupportedException e) {
            throw new IllegalStateException(e);
        }
    }

    @Override
    public String toString() {
        return "BondQuote{Code=" + Code + ",Name=" + Name + ",Direction=" + Direction
                + ",Price=" + Price + ",Volume=" + Volume + ",Text=" + Text + "}";
    }
}
