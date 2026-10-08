package cgws.ficc.parser.parse;

import cgws.ficc.parser.model.AllQuote;
import java.time.LocalDate;

/** Service-facing contract; no JNI or STC classes cross this boundary. */
public interface BondQuoteEngine {
    AllQuote parse(String text, LocalDate businessDate);
    void reload();
}
