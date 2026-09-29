package cgws.ficc.parser.model;

import cgws.ficc.parser.parse.LegacyResultCodec;

import java.util.ArrayList;
import java.util.List;

/**
 * Legacy-compatible parse record: mirrors the field set that
 * {@code com.sumscope.BondQuoteParser$QuoteInfo} exposes today, so the ParserSvr
 * integration keeps the same names and the comparison runner can diff field by field.
 *
 * <p>Null means "not produced by the parser"; empty string means "produced but empty".
 */
public class QuoteInfo {

    public String Direction;
    public String Code;
    public List<String> lstCode = new ArrayList<String>();
    public String Key;
    public String Name;
    public String Tenor;
    public String BidVolume;
    public String OfrVolume;
    public String Volume;
    public String BidPrice;
    public String OfrPrice;
    public String BidTag;
    public String OfrTag;
    public List<String> lstPrice = new ArrayList<String>();
    public String PriceTag;
    public List<String> lstSingle = new ArrayList<String>();
    public List<String> lstNumber = new ArrayList<String>();
    public String BPFloat;
    public boolean bSecondBP;
    public String BigNumPrice;
    public String traderName;
    public String tradeDay;
    public String IsPrice;
    /** Native debug channel, e.g. "Can't find code ... in bond code database." */
    public String debug;
    public boolean isDeal;
    /** Original text handed to the parser (native contract: one assembled line). */
    public String Text;
    /** 1-based line number inside the request text. */
    public int lineNo;

    public QuoteInfo copy() {
        QuoteInfo c = new QuoteInfo();
        c.Direction = Direction;
        c.Code = Code;
        c.lstCode = new ArrayList<String>(lstCode);
        c.Key = Key;
        c.Name = Name;
        c.Tenor = Tenor;
        c.BidVolume = BidVolume;
        c.OfrVolume = OfrVolume;
        c.Volume = Volume;
        c.BidPrice = BidPrice;
        c.OfrPrice = OfrPrice;
        c.BidTag = BidTag;
        c.OfrTag = OfrTag;
        c.lstPrice = new ArrayList<String>(lstPrice);
        c.PriceTag = PriceTag;
        c.lstSingle = new ArrayList<String>(lstSingle);
        c.lstNumber = new ArrayList<String>(lstNumber);
        c.BPFloat = BPFloat;
        c.bSecondBP = bSecondBP;
        c.BigNumPrice = BigNumPrice;
        c.traderName = traderName;
        c.tradeDay = tradeDay;
        c.IsPrice = IsPrice;
        c.debug = debug;
        c.isDeal = isDeal;
        c.Text = Text;
        c.lineNo = lineNo;
        return c;
    }

    @Override
    public String toString() {
        return "QuoteInfo{" + LegacyResultCodec.render(this) + "}";
    }
}
