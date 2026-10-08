package sc.app.stc.parser.utils;

import cgws.ficc.parser.model.BondReference;
import cgws.ficc.parser.parse.BondQuoteEngine;
import cgws.ficc.parser.parse.JavaBondQuoteEngine;
import cgws.ficc.parser.parse.MessageReferenceData;
import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import org.apache.log4j.FileAppender;
import org.apache.log4j.ConsoleAppender;
import org.apache.log4j.PatternLayout;
import org.junit.Test;
import sc.app.stc.parser.handler.BondParserHandler;
import java.lang.reflect.Field;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.Map;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.List;
import java.time.LocalDate;
import cgws.ficc.parser.model.AllQuote;
import static org.junit.Assert.*;

public class JavaParserIntegrationTest {
    public static final class ProtocolWorker {
        public static void main(String[] args) throws Exception {
            System.out.println(NativeShadowProcess.PREFIX + "{\"ready\":true}");
            java.io.BufferedReader reader = new java.io.BufferedReader(new java.io.InputStreamReader(System.in, "UTF-8"));
            String line;
            while ((line = reader.readLine()) != null) {
                JSONObject request = JSON.parseObject(line);
                String reply = "fail".equals(request.getString("text")) ? "{\"error\":\"controlled parse error\"}"
                        : "{\"output\":{\"lstQuote\":[]}}";
                System.out.println(NativeShadowProcess.PREFIX + reply);
                System.out.flush();
            }
        }
    }

    @Test
    public void shadowProcessSurvivesAReportedParseError() {
        try (NativeShadowProcess process = new NativeShadowProcess(ProtocolWorker.class.getName(), 5)) {
            try {
                process.parse("fail", LocalDate.now());
                fail("Expected native error to be reported");
            } catch (IllegalStateException expected) {
                assertTrue(expected.getMessage().contains("controlled parse error"));
            }
            assertTrue(process.parse("next input", LocalDate.now()).lstQuote.isEmpty());
        }
    }

    @Test
    public void javaReplyDoesNotWaitForNativeAndBothPayloadsAreLogged() throws Exception {
        CountDownLatch allowNative = new CountDownLatch(1);
        CountDownLatch compared = new CountDownLatch(1);
        List<String> events = new CopyOnWriteArrayList<String>();
        org.apache.log4j.Logger logger = org.apache.log4j.Logger.getLogger("ficc.parser.compare");
        org.apache.log4j.AppenderSkeleton appender = new org.apache.log4j.AppenderSkeleton() {
            protected void append(org.apache.log4j.spi.LoggingEvent event) {
                events.add(event.getRenderedMessage());
                if (event.getRenderedMessage().contains("\"state\":\"DIFFERENT\"")) {
                    compared.countDown();
                }
            }
            public void close() { }
            public boolean requiresLayout() { return false; }
        };
        logger.addAppender(appender);
        logger.setLevel(org.apache.log4j.Level.INFO);
        try (ComparativeBondQuoteEngine engine = new ComparativeBondQuoteEngine(engine(), () -> {
            try {
                if (!allowNative.await(3, TimeUnit.SECONDS)) {
                    throw new IllegalStateException("Java was blocked by shadow initialization");
                }
            } catch (InterruptedException error) {
                throw new IllegalStateException(error);
            }
            return new BondQuoteEngine() {
                public AllQuote parse(String text, LocalDate date) { return new AllQuote(); }
                public void reload() { }
            };
        })) {
            AllQuote reply = engine.parse("240210 2.5/2.4 1000", LocalDate.now());
            assertEquals(2, reply.lstQuote.size());
            assertEquals("2.4", reply.lstQuote.get(1).Price);
            allowNative.countDown();
            assertTrue(compared.await(3, TimeUnit.SECONDS));
            assertEquals(2, events.stream().filter(line -> line.contains("\"state\":\"INPUT\"")) .count());
            assertEquals(2, events.stream().filter(line -> line.contains("\"state\":\"OUTPUT\"")) .count());
            assertTrue(events.stream().anyMatch(line -> line.contains("\"Price\":\"2.4\"")));
        } finally {
            allowNative.countDown();
            logger.removeAppender(appender);
        }
    }

    @Test
    public void missingEngineSettingSelectsJava() {
        BondQuoteEngine expected = engine();
        assertSame(expected, ParserEngineFactory.create(null, () -> expected,
                () -> { throw new AssertionError("Native is no longer the default"); }));
        assertSame(expected, ParserEngineFactory.create("", () -> expected,
                () -> { throw new AssertionError("Native is no longer the default"); }));
    }
    private static BondQuoteEngine engine() {
        return new JavaBondQuoteEngine(() -> new MessageReferenceData(Arrays.asList(
                new BondReference("240210.ib", "24国开10", "key-1", "BNC", true, false, false, "2.5"),
                new BondReference("240215.ib", "24国开15", "key-2", "BNC", true, false, false, "2.4")), "fixture"), true);
    }

    @Test
    public void engineStartupLogsSuccessAndFailureToConsoleAndFile() throws Exception {
        Path path = Files.createTempFile("parser-startup-", ".log");
        org.apache.log4j.Logger logger = org.apache.log4j.Logger.getLogger("ficc.startup");
        PatternLayout layout = new PatternLayout("%p %m%n");
        FileAppender file = new FileAppender(layout, path.toString());
        ConsoleAppender console = new ConsoleAppender(layout);
        logger.addAppender(file);
        logger.addAppender(console);
        logger.setLevel(org.apache.log4j.Level.INFO);
        try {
            BondQuoteEngine expected = engine();
            assertSame(expected, ParserEngineFactory.create("java", () -> expected,
                    () -> { throw new AssertionError("Native engine must not be called"); }));
            IllegalStateException failure = new IllegalStateException("controlled reference failure");
            try {
                ParserEngineFactory.create("java", () -> { throw failure; }, () -> expected);
                fail("Failure must propagate");
            } catch (IllegalStateException actual) {
                assertSame(failure, actual);
            }
            file.close();
            String contents = new String(Files.readAllBytes(path), StandardCharsets.UTF_8);
            assertTrue(contents.contains("\"state\":\"INITIALIZED\""));
            assertTrue(contents.contains("\"state\":\"FAILED\""));
            assertTrue(contents.contains("\"mode\":\"java\""));
            assertTrue(contents.contains("java.lang.IllegalStateException: controlled reference failure"));
        } finally {
            logger.removeAppender(file);
            logger.removeAppender(console);
            file.close();
            console.close();
        }
    }

    @Test
    public void dpsHandlerReturnsBothSidesAndBothLinesWithoutNativeLibrary() throws Exception {
        Field singleton = BondQuoteHelper.class.getDeclaredField("instance");
        singleton.setAccessible(true);
        Object previous = singleton.get(null);
        int oldCrm = Consts.CRMConnSwitch;
        try {
            Consts.CRMConnSwitch = 0;
            singleton.set(null, new BondQuoteHelper(engine()));
            JSONObject request = new JSONObject();
            request.put("userName", "parser-contract-test");
            request.put("content", "240210 2.5/2.4 1000;240215 2.3 2000 bid");
            Map<String, Object> reply = new BondParserHandler().handle("stc.parser.bond", null,
                    request.toJSONString(), null, false);
            JSONObject json = JSON.parseObject(JSON.toJSONString(reply));
            assertEquals(json.toJSONString(), "0000", json.getString("Code"));
            assertEquals(3, json.getJSONArray("Datas").size());
            JSONObject offer = json.getJSONArray("Datas").getJSONObject(1);
            assertEquals("240210.IB", offer.getString("securityId"));
            assertEquals("2.4", offer.getBigDecimal("price").stripTrailingZeros().toPlainString());
            assertEquals("1000", offer.getBigDecimal("qty").stripTrailingZeros().toPlainString());
        } finally {
            singleton.set(null, previous);
            Consts.CRMConnSwitch = oldCrm;
        }
    }

    @Test
    public void expiredCdReturnsPreviewWarningAndTomorrowSettlement() throws Exception {
        Field singleton = BondQuoteHelper.class.getDeclaredField("instance");
        singleton.setAccessible(true);
        Object previous = singleton.get(null);
        int oldCrm = Consts.CRMConnSwitch;
        try {
            Consts.CRMConnSwitch = 0;
            BondQuoteEngine parser = new JavaBondQuoteEngine(() -> new MessageReferenceData(Arrays.asList(
                    new BondReference("112508306.ib", "25中信银行CD306", "cd", "NCD", false, true, false, null)), "expired"), true);
            singleton.set(null, new BondQuoteHelper(parser));
            JSONObject request = new JSONObject();
            request.put("userName", "parser-contract-test");
            request.put("content", "Bid 112508306.IB 25中信银行CD306 2000w 2 到期 明天+0");
            Map<String, Object> reply = new BondParserHandler().handle("stc.parser.bond", null,
                    request.toJSONString(), null, false);
            JSONObject json = JSON.parseObject(JSON.toJSONString(reply));
            assertEquals(json.toJSONString(), "0000", json.getString("Code"));
            assertEquals(1, json.getJSONArray("Datas").size());
            assertTrue(json.getJSONArray("Warnings").getString(0).contains("112508306.IB 已到期或摘牌"));
            JSONObject quote = json.getJSONArray("Datas").getJSONObject(0);
            assertEquals("3", quote.getString("settlType"));
            assertEquals("2", quote.getBigDecimal("price").toPlainString());
            assertEquals("2000", quote.getBigDecimal("qty").toPlainString());
        } finally {
            singleton.set(null, previous);
            Consts.CRMConnSwitch = oldCrm;
        }
    }

    @Test
    public void invalidEngineSelectionFailsWithoutFallback() {
        try {
            ParserEngineFactory.create("unknown", JavaParserIntegrationTest::engine,
                    () -> { throw new AssertionError("No silent fallback"); });
            fail("Unknown selection must fail");
        } catch (IllegalArgumentException expected) {
            assertTrue(expected.getMessage().contains("java or native"));
        }
    }
}
