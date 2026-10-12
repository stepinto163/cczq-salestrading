package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.QuoteRecord;
import ficc.common.JsonUtils;
import ficc.common.client.StcTransport;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.function.Consumer;

import static org.junit.jupiter.api.Assertions.*;

class CalSvrQuoteYieldResolverTest {
    private static final class Transport implements StcTransport {
        volatile boolean online = true;
        volatile JSONObject reply = JsonUtils.object("Code", "0000", "Data",
                JsonUtils.object("yield", "0.025", "yieldToExecution", "0.027"));
        volatile JSONObject lastRequest;
        final AtomicInteger calls = new AtomicInteger();
        Runnable connection;
        Consumer<JSONObject> clear;
        CountDownLatch release = new CountDownLatch(0);

        public boolean online() { return online; }
        public void connectionChanged(Runnable listener) { connection = listener; }
        public void subscribe(String topic, Consumer<JSONObject> listener) { clear = listener; }
        public JSONObject request(String topic, String session, JSONObject request) {
            assertEquals("stc.broker.calbond", topic);
            lastRequest = request;
            calls.incrementAndGet();
            try {
                if (!release.await(3, TimeUnit.SECONDS)) throw new IllegalStateException("test timeout");
            } catch (InterruptedException error) {
                Thread.currentThread().interrupt();
                throw new IllegalStateException(error);
            }
            if (reply == null) throw new IllegalStateException("simulated DPS timeout");
            return reply;
        }
    }

    private QuoteRecord quote() {
        QuoteRecord quote = new QuoteRecord();
        quote.quoteId = "Q-CAL";
        quote.securityId = "240210.IB";
        quote.quoteType = "2";
        quote.price = "101.25";
        quote.tradeDate = "2026-10-12";
        quote.settlType = "2";
        return quote;
    }

    @Test
    void mapsTypesUnitsAndTradeDate() {
        QuoteRecord quote = quote();
        JSONObject request = CalSvrQuoteYieldResolver.request(quote);
        assertEquals("2", request.getString("CalType"));
        assertEquals(new BigDecimal("101.25"), request.getBigDecimal("Value"));
        assertEquals("20261012", request.getString("TradeDate"));
        assertEquals("2", request.getString("SettlType"));
        assertFalse(request.containsKey("SettlementDate"));
        quote.quoteType = "3";
        assertEquals("3", CalSvrQuoteYieldResolver.request(quote).getString("CalType"));
        quote.quoteType = "1";
        quote.yieldType = "1";
        quote.price = "2.5";
        request = CalSvrQuoteYieldResolver.request(quote);
        assertEquals("4", request.getString("CalType"));
        assertEquals(0, new BigDecimal("0.025").compareTo(request.getBigDecimal("Value")));
        quote.yieldType = "2";
        assertEquals("7", CalSvrQuoteYieldResolver.request(quote).getString("CalType"));
        quote.settlType = "3";
        assertThrows(IllegalArgumentException.class, () -> CalSvrQuoteYieldResolver.request(quote));
    }

    @Test
    void slowCalculatorDoesNotBlockQueryAndReusesResult() throws Exception {
        Transport transport = new Transport();
        transport.release = new CountDownLatch(1);
        LinkedBlockingQueue<String> updates = new LinkedBlockingQueue<>();
        try (CalSvrQuoteYieldResolver resolver = new CalSvrQuoteYieldResolver(transport, 4, updates::add)) {
            QuoteRecord quote = quote();
            assertEquals("CALCULATION_PENDING", resolver.resolve(quote, "1", "V1").code);
            assertEquals("CALCULATION_PENDING", resolver.resolve(quote, "1", "V1").code);
            transport.release.countDown();
            assertEquals(quote.securityId, updates.poll(3, TimeUnit.SECONDS));
            assertEquals("2.5", resolver.resolve(quote, "1", "V1").price);
            assertEquals(1, transport.calls.get());
            assertEquals("101.25", quote.price);
            assertEquals("2", quote.quoteType);
            resolver.resolve(quote, "2", "V1");
            assertNotNull(updates.poll(3, TimeUnit.SECONDS));
            assertEquals("2.7", resolver.resolve(quote, "2", "V1").price);
        }
    }

    @Test
    void offlineAndTimeoutRecoverWithoutPermanentNegativeCache() throws Exception {
        Transport transport = new Transport();
        transport.online = false;
        LinkedBlockingQueue<String> updates = new LinkedBlockingQueue<>();
        try (CalSvrQuoteYieldResolver resolver = new CalSvrQuoteYieldResolver(transport, 4, updates::add)) {
            QuoteRecord quote = quote();
            assertEquals("CALCULATOR_OFFLINE", resolver.resolve(quote, "1", "V1").code);
            assertEquals(0, transport.calls.get());
            transport.online = true;
            transport.connection.run();
            updates.clear();
            transport.reply = null;
            resolver.resolve(quote, "1", "V1");
            assertNotNull(updates.poll(3, TimeUnit.SECONDS));
            assertEquals("CALCULATION_FAILED", resolver.resolve(quote, "1", "V1").code);
            assertEquals(1, transport.calls.get());
            transport.reply = JsonUtils.object("Code", "0000", "Data", JsonUtils.object("yield", "0.021"));
            resolver.retryFailures();
            resolver.resolve(quote, "1", "V1");
            assertNotNull(updates.poll(3, TimeUnit.SECONDS));
            assertEquals("2.1", resolver.resolve(quote, "1", "V1").price);
        }
    }

    @Test
    void missingYieldErrorReplyAndInvalidInputStayIsolated() throws Exception {
        Transport transport = new Transport();
        LinkedBlockingQueue<String> updates = new LinkedBlockingQueue<>();
        try (CalSvrQuoteYieldResolver resolver = new CalSvrQuoteYieldResolver(transport, 4, updates::add)) {
            QuoteRecord quote = quote();
            for (JSONObject reply : new JSONObject[] {
                    JsonUtils.object("Code", "0000", "Data", JsonUtils.object("yield", "0.02")),
                    JsonUtils.object("Code", "1009", "Msg", "not ready")}) {
                transport.reply = reply;
                resolver.retryFailures();
                resolver.resolve(quote, "2", "V1");
                assertNotNull(updates.poll(3, TimeUnit.SECONDS));
                assertEquals("CALCULATION_FAILED", resolver.resolve(quote, "2", "V1").code);
            }
            quote.tradeDate = null;
            assertEquals("CALCULATION_INVALID_INPUT", resolver.resolve(quote, "2", "V1").code);
        }
    }

    @Test
    void priceProjectionRegression() {
        PriceSearchSmoke.main(new String[0]);
    }
}
