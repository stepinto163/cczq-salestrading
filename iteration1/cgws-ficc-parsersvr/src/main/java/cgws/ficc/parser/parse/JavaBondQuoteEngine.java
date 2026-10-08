package cgws.ficc.parser.parse;

import cgws.ficc.parser.model.AllQuote;
import java.time.LocalDate;

/** Refresh publishes a complete snapshot; an in-flight request keeps its previous snapshot. */
public final class JavaBondQuoteEngine implements BondQuoteEngine {
    public interface ReferenceLoader {
        MessageReferenceData load() throws Exception;
    }

    private final ReferenceLoader loader;
    private final boolean shortCodesEnabled;
    private volatile QuoteMessageParser parser;

    public JavaBondQuoteEngine(ReferenceLoader loader, boolean shortCodesEnabled) {
        if (loader == null) {
            throw new IllegalArgumentException("Reference loader is required");
        }
        this.loader = loader;
        this.shortCodesEnabled = shortCodesEnabled;
        reload();
    }

    @Override
    public AllQuote parse(String text, LocalDate businessDate) {
        return parser.parse(text, businessDate, shortCodesEnabled);
    }

    @Override
    public synchronized void reload() {
        try {
            MessageReferenceData next = loader.load();
            parser = new QuoteMessageParser(next);
        } catch (Exception error) {
            // Startup fails; a refresh failure propagates while preserving the previous snapshot.
            throw new IllegalStateException("Parser reference snapshot load failed", error);
        }
    }
}
