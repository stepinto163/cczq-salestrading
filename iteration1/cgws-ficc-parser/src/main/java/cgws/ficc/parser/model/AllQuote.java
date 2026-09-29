package cgws.ficc.parser.model;

import java.util.ArrayList;
import java.util.List;

/** Mirrors {@code com.sumscope.BondQuoteParser$AllQuote}. */
public class AllQuote {

    public List<BondQuote> lstQuote = new ArrayList<BondQuote>();
    public String trader = "";
    public String company = "";
    public boolean isDeal;
    public String traderSuffix;
    public boolean isShortCode;
    public boolean isFullCode;
}
