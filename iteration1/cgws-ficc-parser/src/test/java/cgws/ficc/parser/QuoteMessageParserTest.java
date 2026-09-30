package cgws.ficc.parser;

import cgws.ficc.parser.compare.Json;
import cgws.ficc.parser.model.AllQuote;
import cgws.ficc.parser.model.BondQuote;
import cgws.ficc.parser.model.BondReference;
import cgws.ficc.parser.parse.JavaBondQuoteEngine;
import cgws.ficc.parser.parse.MessageReferenceData;
import cgws.ficc.parser.parse.QuoteMessageParser;
import org.junit.jupiter.api.DynamicTest;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestFactory;
import java.lang.reflect.Field;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.time.LocalDate;
import java.util.*;
import java.util.concurrent.atomic.AtomicBoolean;
import static org.junit.jupiter.api.Assertions.*;

class QuoteMessageParserTest {
    private static final LocalDate DATE = LocalDate.of(2026, 9, 29);

    static MessageReferenceData reference() {
        return new MessageReferenceData(Arrays.asList(
                new BondReference("240210.ib", "24国开10", "KEY-240210.ib", "BNC", true, false, false, "2.5000"),
                new BondReference("240215.ib", "24国开15", "KEY-240215.ib", "BNC", true, false, false, "2.5000"),
                new BondReference("240001.ib", "24国债01", "KEY-240001.ib", "BNC", true, false, false, "2.5000")), "fixture");
    }

    private AllQuote parse(String text) {
        return new QuoteMessageParser(reference()).parse(text, DATE, true);
    }

    @TestFactory
    List<DynamicTest> completeMessageMatchesRecordedLegacyResults() throws Exception {
        String data = new String(Files.readAllBytes(Paths.get(getClass()
                .getResource("/legacy-messages.json").toURI())), StandardCharsets.UTF_8);
        List<Object> records = Json.asArray(Json.parse(data));
        // Other cases are separately tested as explicit corrections, not falsely labelled matches.
        Set<Integer> matching = new HashSet<Integer>(Arrays.asList(0, 1, 2, 3, 4, 7, 8, 9, 10, 16, 17, 20, 22, 23, 24));
        List<DynamicTest> cases = new ArrayList<DynamicTest>();
        for (int index = 0; index < records.size(); index++) {
            if (!matching.contains(index)) {
                continue;
            }
            Map<String, Object> row = Json.asObject(records.get(index));
            String input = Json.str(row, "input");
            cases.add(DynamicTest.dynamicTest("Legacy message " + index,
                    () -> assertEquals(row.get("expected"), Json.parse(Json.write(structured(parse(input)))))));
        }
        return cases;
    }

    private static Object structured(Object value) throws IllegalAccessException {
        if (value instanceof AllQuote || value instanceof BondQuote) {
            Map<String, Object> fields = new TreeMap<String, Object>();
            for (Field field : value.getClass().getFields()) {
                Object member = field.get(value);
                if (member != null) {
                    fields.put(field.getName(), structured(member));
                }
            }
            return fields;
        }
        if (value instanceof List) {
            List<Object> items = new ArrayList<Object>();
            for (Object item : (List<?>) value) {
                items.add(structured(item));
            }
            return items;
        }
        return value;
    }

    @Test
    void suppressedShortCodeDoesNotBecomeHeading() {
        AllQuote result = parse("210 2.6 3000 bid\n240215 2.4 1000 ofr");
        assertEquals(1, result.lstQuote.size());
        assertEquals("240215.ib", result.lstQuote.get(0).Code);
        assertEquals("2.4", result.lstQuote.get(0).Price);
        assertEquals("1000", result.lstQuote.get(0).Volume);
        assertEquals("ofr", result.lstQuote.get(0).Direction);
    }

    @Test
    void pairedPricesDoNotConsumeVolume() {
        List<BondQuote> quotes = parse("240210 2.5/2.4 1000").lstQuote;
        assertEquals(2, quotes.size());
        assertEquals("2.5", quotes.get(0).Price);
        assertEquals("bid", quotes.get(0).Direction);
        assertEquals("2.4", quotes.get(1).Price);
        assertEquals("ofr", quotes.get(1).Direction);
        assertEquals("1000", quotes.get(0).Volume);
        assertEquals("1000", quotes.get(1).Volume);
    }

    @Test
    void semicolonQuotesKeepSourceLineAndBothBonds() {
        String input = "240210 2.5 1000;240215 2.4 2000";
        List<BondQuote> quotes = parse(input).lstQuote;
        assertEquals(2, quotes.size());
        assertEquals("240215.ib", quotes.get(1).Code);
        assertEquals("2000", quotes.get(1).Volume);
        assertEquals(input, quotes.get(1).Text);
        assertEquals(0, quotes.get(1).lineNo);
    }

    @Test
    void qualifiersDoNotBecomeTraderNames() {
        AllQuote message = parse("240210 101.5 全价 1000 bid oco 打包");
        assertEquals("", message.trader);
        assertEquals(2, message.lstQuote.get(0).priceType);
        assertEquals("oco|打包", message.lstQuote.get(0).PriceTag);
    }

    @Test
    void valuationSpreadUsesReferenceAndDoesNotSilentlyInventMissingData() {
        assertEquals("2.55", parse("240210 中债+5bp 1000 bid").lstQuote.get(0).Price);
        MessageReferenceData missing = new MessageReferenceData(Collections.singletonList(
                new BondReference("240210.ib", "24国开10", "key", "BNC", true, false, false, null)), "missing");
        assertThrows(IllegalStateException.class,
                () -> new QuoteMessageParser(missing).parse("240210 中债+5bp 1000 bid", DATE, true));
    }

    @Test
    void chineseQuantitiesAndSettlement() {
        BondQuote quote = parse("240210 2.5 一亿两千万 t+0 bid").lstQuote.get(0);
        assertEquals("12000", quote.Volume);
        assertEquals("+0", quote.settlementDay);
    }

    @Test
    void headersDoNotLeakBetweenRequests() {
        QuoteMessageParser parser = new QuoteMessageParser(reference());
        parser.parse("bid 2.6 3000\n240210 2.5 1000", DATE, true);
        BondQuote next = parser.parse("240210 2.5 1000", DATE, true).lstQuote.get(0);
        assertEquals("ofr", next.Direction);
        assertEquals("2.5", next.Price);
        assertEquals("1000", next.Volume);
    }

    @Test
    void failedRefreshRetainsSnapshotAndPropagates() {
        AtomicBoolean fail = new AtomicBoolean();
        JavaBondQuoteEngine engine = new JavaBondQuoteEngine(() -> {
            if (fail.get()) {
                throw new IllegalStateException("test load failure");
            }
            return reference();
        }, true);
        fail.set(true);
        assertThrows(IllegalStateException.class, engine::reload);
        assertEquals(1, engine.parse("240210 2.5 1000", DATE).lstQuote.size());
    }

    @Test
    void startupFailureCannotBecomeReady() {
        assertThrows(IllegalStateException.class, () -> new JavaBondQuoteEngine(() -> null, true));
    }

    @Test
    void expiredBondsRemainInPreviewWithExplicitWarning() {
        MessageReferenceData invalid = new MessageReferenceData(Collections.singletonList(
                new BondReference("240210.ib", "24国开10", "key", "BNC", true, true, false, null)), "invalid");
        AllQuote result = new QuoteMessageParser(invalid).parse("240210 2.5 1000", DATE, true);
        assertEquals(1, result.lstQuote.size());
        assertEquals(1, result.warnings.size());
        assertTrue(result.warnings.get(0).contains("240210.IB 已到期或摘牌"));
    }

    @Test
    void cdQuoteWithUnitQuantityBeforeIntegerYield() {
        MessageReferenceData reference = new MessageReferenceData(Collections.singletonList(
                new BondReference("112508306.ib", "25中信银行CD306", "cd-key", "NCD", false, false, false, "1.8")), "valid-cd");
        QuoteMessageParser parser = new QuoteMessageParser(reference);
        for (String body : Arrays.asList("2000w 2", "2 2000w", "2000万 2")) {
            AllQuote result = parser.parse("Bid 112508306.IB 25中信银行CD306 " + body + " 到期 明天+0", DATE, true);
            assertEquals(1, result.lstQuote.size());
            BondQuote quote = result.lstQuote.get(0);
            assertEquals("112508306.ib", quote.Code);
            assertEquals("bid", quote.Direction);
            assertEquals("2", quote.Price);
            assertEquals("2000", quote.Volume);
            assertEquals(0, quote.priceType);
            assertEquals("明天+0", quote.settlementDay);
            assertNull(result.warnings);
        }
    }

    @Test
    void integerYieldIsRecognizedInsteadOfRepeatingLegacyEmptyPrice() {
        BondQuote quote = parse("240210 2 1000 bid").lstQuote.get(0);
        assertEquals("2", quote.Price);
        assertEquals("1000", quote.Volume);
    }

    @Test
    void multilineIntegerPricesAndSettlementsStayWithTheirOwnQuote() {
        MessageReferenceData data = new MessageReferenceData(Arrays.asList(
                new BondReference("199385.ib", "25山东债91", "a", "BNC", false, false, false, null),
                new BondReference("250210.ib", "25国开10", "b", "BNC", true, false, false, null)), "two-lines");
        QuoteMessageParser parser = new QuoteMessageParser(data);
        for (String separator : Arrays.asList("\n", "\r\n", "\\r\\n", "\\n", ";")) {
            AllQuote result = parser.parse("199385 3 1000 +1" + separator + "250210 2 2000 +0", DATE, true);
            assertEquals(2, result.lstQuote.size(), separator);
            BondQuote first = result.lstQuote.get(0);
            BondQuote second = result.lstQuote.get(1);
            assertEquals("199385.ib", first.Code);
            assertEquals("3", first.Price);
            assertEquals("1000", first.Volume);
            assertEquals("+1", first.settlementDay);
            assertEquals("250210.ib", second.Code);
            assertEquals("2", second.Price);
            assertEquals("2000", second.Volume);
            assertEquals("+0", second.settlementDay);
        }
    }

    @Test
    void snapshotReplacesDeletedReferences() {
        MessageReferenceData smaller = new MessageReferenceData(Collections.singletonList(
                new BondReference("240215.ib", "24国开15", "key", "BNC", true, false, false, null)), "next");
        AtomicBoolean changed = new AtomicBoolean();
        JavaBondQuoteEngine engine = new JavaBondQuoteEngine(() -> changed.get() ? smaller : reference(), true);
        changed.set(true);
        engine.reload();
        assertTrue(engine.parse("240210 2.5 1000", DATE).lstQuote.isEmpty());
    }
}
