package cgws.ficc.parser.parse;

import cgws.ficc.parser.dict.BondDictionary;
import cgws.ficc.parser.model.ParseResponse;
import cgws.ficc.parser.model.QuoteInfo;
import cgws.ficc.parser.text.TextNormalizer;

/**
 * Entry point of the pure Java parse path.
 *
 * <p>Comparison boundary (kept identical to the baseline produced by {@code BaselineRunner}):
 * <ol>
 *   <li>{@code preprocess(text)} on the whole request text,</li>
 *   <li>split the working text into lines with {@code \r\n|\n|\r},</li>
 *   <li>{@code ParseBond} equivalent once per line; every line contributes one entry to
 *       {@link ParseResponse#lineResults}, empty when the line yields nothing.</li>
 * </ol>
 *
 * <p>No database, no clock, no static initialisation side effects: the dictionary snapshot, the
 * business date and the options are all passed in, so a case is reproducible.
 */
public final class ParserCore {

    private ParserCore() {
    }

    /**
     * @param text          request text, may contain several lines
     * @param dictionary    published snapshot; a missing or empty snapshot is a hard failure
     * @param businessDate  yyyyMMdd, explicit so results do not depend on the machine clock
     * @param inheritTitle  when true a header line supplies direction / volume to following quotes
     */
    public static ParseResponse parse(String text, BondDictionary dictionary, String businessDate,
                                      boolean inheritTitle) {
        ParseResponse response = new ParseResponse();
        response.dictionaryVersion = dictionary == null ? null : dictionary.version();
        if (dictionary == null || dictionary.isEmpty()) {
            // Hard failure: without a dictionary the result would look like "bond not recognised".
            response.fail("DICTIONARY_EMPTY", "parse called without a published dictionary snapshot");
            return response;
        }
        if (text == null || text.trim().isEmpty()) {
            response.fail("INPUT_EMPTY", "request text is empty");
            return response;
        }
        String working = TextNormalizer.preProcess(text);
        response.preprocessed = working;
        String[] lines = working.split("\r\n|\n|\r");
        TitleContext title = new TitleContext();
        for (int i = 0; i < lines.length; i++) {
            String line = lines[i];
            String piece = line.trim();
            if (piece.isEmpty()) {
                response.lineResults.add("");
                continue;
            }
            if (QuoteFieldParser.looksLikeHeader(piece, dictionary)) {
                QuoteInfo header = QuoteFieldParser.parseLine(line, dictionary);
                if (header != null && inheritTitle) {
                    title.merge(header);
                }
                response.lineResults.add("");
                continue;
            }
            QuoteInfo quote = QuoteFieldParser.parseLine(line, dictionary);
            if (quote == null) {
                response.lineResults.add("");
                continue;
            }
            quote.lineNo = i + 1;
            title.apply(quote);
            String rendered = LegacyResultCodec.render(quote);
            response.quotes.add(quote);
            response.rawResults.add(rendered);
            response.lineResults.add(rendered);
        }
        if (response.quotes.isEmpty()) {
            response.addDiagnostic("NO_QUOTE", "no quote recognised in " + lines.length + " line(s)");
        }
        return response;
    }

    /** Convenience for callers that only need diagnostics formatted for logs. */
    public static String formatDiagnostics(ParseResponse response) {
        return response == null ? "" : response.diagnosticsAsString();
    }

    /** Fields a header line can pass on to the quotes below it. */
    private static final class TitleContext {

        private String direction;
        private String volume;

        void merge(QuoteInfo header) {
            if (header.Direction != null) {
                direction = header.Direction;
            }
            if (header.Volume != null) {
                volume = header.Volume;
            }
        }

        void apply(QuoteInfo quote) {
            if (quote.Direction == null && direction != null) {
                quote.Direction = direction;
            }
            if (quote.Volume == null && volume != null) {
                quote.Volume = volume;
            }
        }
    }
}
