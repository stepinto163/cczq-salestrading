package cgws.ficc.parser;

import cgws.ficc.parser.compat.JavaParserFacade;
import cgws.ficc.parser.dict.BondDictionary;
import cgws.ficc.parser.dict.DictionaryRow;
import cgws.ficc.parser.dict.DictionaryStore;
import cgws.ficc.parser.identify.BondIdentifier;
import cgws.ficc.parser.model.ParseResponse;
import cgws.ficc.parser.model.QuoteInfo;
import cgws.ficc.parser.parse.LegacyResultCodec;
import cgws.ficc.parser.parse.ParserCore;
import org.junit.jupiter.api.Test;

import java.util.Arrays;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertTrue;

/** Behaviour of the parse path that the Windows baseline confirmed. */
class ParserBehaviorTest {

    private static BondDictionary sampleDictionary() {
        BondDictionary.Builder builder = new BondDictionary.Builder();
        builder.put("240001", "24国债01", null, false);
        builder.put("2400001", "24国债02", null, false);
        builder.put("2400001.ib", "24国债03", null, false);
        builder.put("092318001", "23农发01", null, false);
        builder.put("190001", "旧券01", null, true);
        return builder.build("test-v1");
    }

    @Test
    void dictionarySnapshotKeepsFailingRefresh() {
        DictionaryStore store = new DictionaryStore();
        assertFalse(store.isReady(), "store must not be ready before a snapshot is published");
        BondDictionary snapshot = sampleDictionary();
        store.publish(snapshot);
        assertTrue(store.isReady());
        assertEquals(5, snapshot.size());
        assertTrue(snapshot.isInvalid("190001"));
        assertFalse(snapshot.isInvalid("240001"));
        store.recordFailure("refresh boom");
        assertTrue(store.isReady(), "a failed refresh must keep the previous snapshot");
        assertEquals(snapshot, store.current());
        assertEquals("refresh boom", store.lastLoadError());
    }

    @Test
    void identificationRules() {
        BondDictionary dict = sampleDictionary();
        List<String> resolved = BondIdentifier.resolveCode("2400001", dict);
        assertEquals(1, resolved.size(), "bare 7 digit code resolves to the .ib variant only");
        assertEquals("2400001.ib", resolved.get(0));
        assertEquals("2400001.ib", BondIdentifier.resolveCode("2400001.ib", dict).get(0));
        assertEquals("240001", BondIdentifier.resolveName("24国债01", dict).get(0));
        assertTrue(BondIdentifier.resolveCode("999999", dict).isEmpty());
    }

    @Test
    void quoteFields() {
        BondDictionary dict = sampleDictionary();
        ParseResponse response = ParserCore.parse("2400001 2.35 1000", dict, "20260929", true);
        assertEquals(1, response.quotes.size());
        QuoteInfo quote = response.quotes.get(0);
        assertEquals("2400001.ib", quote.Code);
        assertEquals("24国债03", quote.Name);
        assertEquals("1000", quote.Volume);
        assertEquals("2.35", quote.lstSingle.get(0));

        ParseResponse unresolved = ParserCore.parse("买 240001 2.35 1000", dict, "20260929", true);
        assertEquals("bid", unresolved.quotes.get(0).Direction);
        assertEquals("240001", unresolved.quotes.get(0).lstNumber.get(0));
        assertEquals("1000", unresolved.quotes.get(0).Volume);

        ParseResponse pair = ParserCore.parse("2400001.ib 2.35/2.30 1000", dict, "20260929", true);
        assertEquals("2.35", pair.quotes.get(0).BidPrice);
        assertEquals("2.30", pair.quotes.get(0).OfrPrice);
    }

    @Test
    void sixDigitTokenIsNotACode() {
        BondDictionary dict = sampleDictionary();
        ParseResponse response = ParserCore.parse("240001 2.35 1000", dict, "20260929", true);
        assertNull(response.quotes.get(0).Code, "6 digit numbers stay unresolved (baseline C004/C009)");
        assertEquals("1000", response.quotes.get(0).Volume);
    }

    @Test
    void negativeCaseRecordsDiagnostic() {
        BondDictionary dict = sampleDictionary();
        ParseResponse response = ParserCore.parse("no bond text here", dict, "20260929", true);
        assertTrue(response.quotes.isEmpty());
        assertTrue(response.hasDiagnostics());
    }

    @Test
    void legacyStringRoundTrip() {
        String raw = "Code:2400001.ib;Name:24国债03;CodeList:2400001.ib,;Volume:1000;Single:2.35,;";
        QuoteInfo quote = LegacyResultCodec.parse(raw);
        assertEquals("2400001.ib", quote.Code);
        assertEquals("1000", quote.Volume);
        assertEquals(raw, LegacyResultCodec.render(quote));
    }

    @Test
    void multiLineKeepsLineNumbersAndPerLineResults() {
        BondDictionary dict = sampleDictionary();
        ParseResponse response = ParserCore.parse(
                "2400001.ib 2.35 1000\n240001 24国债01 3.10 2000", dict, "20260929", true);
        assertEquals(2, response.quotes.size());
        assertEquals(1, response.quotes.get(0).lineNo);
        assertEquals(2, response.quotes.get(1).lineNo);
        assertEquals(2, response.lineResults.size(), "one result entry per line");
    }

    @Test
    void facadeLifecycleAndChineseNumbers() {
        JavaParserFacade facade = JavaParserFacade.getInstance();
        assertEquals("start:start", facade.SetCodeName("start", "start", "", ""));
        assertEquals("240001:24国债01", facade.SetCodeName("240001", "24国债01", "", ""));
        assertEquals("", facade.SetCodeName("240001", "24国债01", "", ""), "duplicate insert is empty");
        assertEquals("end:end", facade.SetCodeName("end", "end", "", ""));
        assertTrue(facade.IsCode("240001"));
        assertFalse(facade.IsCode("999999"));
        assertEquals("3000", facade.ReplaceNumber("三千"));
        List<DictionaryRow> rows = Arrays.asList(new DictionaryRow("2400001.ib", "24国债03", null, false));
        facade.loadRows(rows);
        assertTrue(facade.isReady());
        String raw = facade.ParseBond("2400001.ib 2.35 1000", "", "");
        assertTrue(raw.contains("Code:2400001.ib"), "raw=" + raw);
        assertTrue(raw.contains("Volume:1000"), "raw=" + raw);
    }

    @Test
    void preprocessedTextIsPartOfTheBoundary() {
        BondDictionary dict = sampleDictionary();
        ParseResponse response = ParserCore.parse("2400001.ib 2.35 1000", dict, "20260929", true);
        assertNotNull(response.preprocessed);
        assertEquals("2400001.ib 2.35 1000 ", response.preprocessed);
    }
}
