package ficc.qtintake;

import static org.junit.jupiter.api.Assertions.*;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.JsonUtils;
import ficc.common.client.StcTransport;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import quickfix.Message;

import java.nio.file.*;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.function.Consumer;

class QtIntakeTest {
    @TempDir Path directory;

    static Message quote(String side, String priceType) {
        Message message = new Message();
        message.getHeader().setString(35, "S");
        message.setString(117, "QT-QUOTE-123");
        message.setString(18045, "N");
        message.setString(18004, "QT-ACCOUNT-1");
        message.setString(60, "20260922-01:02:03.123");
        message.setString(48, "250001");
        message.setString(100, "XCFE");
        message.setString(54, side);
        boolean bid = "1".equals(side);
        message.setString(bid ? 18042 : 18043, priceType);
        message.setString(bid ? 132 : 133, "2.5000");
        message.setString(bid ? 134 : 135, "12345678");
        message.setString(63, "2");
        message.setString(18036, "Institution from QTrade");
        message.setString(18006, "Counterparty contact");
        message.setString(18026, "Original source text");
        return message;
    }

    static Message capturedPrlQuote() throws Exception {
        // Real PRL message shape from 2026-09-22; account/quote identifiers anonymized.
        String fix =
                "8=FIX.4.4|9=375|35=S|34=5|49=QTRADE|52=20260922-10:06:13.372|"
                        + "56=PRL-TEST-CLIENT|48=240210|54=1|55=24国开10|"
                        + "60=20260922-18:06:13|100=XCFE|117=QT-CAPTURE-1|132=3|"
                        + "134=10000000|215=1|216=52|537=0|10176=4|11050=7.6192Y|"
                        + "18004=QT-ACCOUNT-1|18026=240210 3 1000 +1 bid|18034=2|"
                        + "18035=1|18042=1|18045=N|18050=1000|18052=3|"
                        + "18167=2|18519=2|10=189|";
        Message message = new Message();
        // Body length/checksum change when identifiers are anonymized.
        message.fromString(fix.replace('|', '\u0001'), null, false);
        return message;
    }

    @Test
    void mapsCapturedPrlBusinessDateAndBidSettlement() throws Exception {
        JSONObject request = new QTradeQuoteMapper().map(capturedPrlQuote());
        JSONObject row = request.getJSONArray("quotes").getJSONObject(0);
        assertEquals("240210.IB", row.getString("securityId"));
        assertEquals("2026-09-22", row.getString("tradeDate"));
        assertEquals("2", row.getString("settlType"));
        assertEquals("3", row.getString("price"));
        assertEquals("1000", row.getString("quantity"));
        assertEquals("20260922-18:06:13", request.getJSONObject("intake").getString("sourceTime"));
    }

    @Test
    void usesMatchingSideSettlementBeforeCommonSettlement() throws Exception {
        for (String side : new String[] {"1", "2"}) {
            Message message = quote(side, "1");
            message.setString(18167, "1");
            message.setString(18169, "2");
            message.setString(63, "3");
            JSONObject row =
                    new QTradeQuoteMapper().map(message).getJSONArray("quotes").getJSONObject(0);
            assertEquals("1".equals(side) ? "1" : "2", row.getString("settlType"));
            message.removeField("1".equals(side) ? 18167 : 18169);
            row = new QTradeQuoteMapper().map(message).getJSONArray("quotes").getJSONObject(0);
            assertEquals("3", row.getString("settlType"));
            message.removeField(63);
            message.setString(18026, "free text +1");
            row = new QTradeQuoteMapper().map(message).getJSONArray("quotes").getJSONObject(0);
            assertNull(row.get("settlType"), "Do not use the other side or guess from text");
        }
    }

    @Test
    void businessDatePrefersMatchingSideThenCommonDateThenLocalTimestamp() throws Exception {
        for (String side : new String[] {"1", "2"}) {
            Message message = quote(side, "1");
            message.setString(60, "20260922-23:59:59.999");
            message.setString(75, "20260923");
            message.setString(18166, "20260924");
            message.setString(18168, "20260925");
            QTradeQuoteMapper mapper = new QTradeQuoteMapper();
            JSONObject row = mapper.map(message).getJSONArray("quotes").getJSONObject(0);
            assertEquals(
                    "1".equals(side) ? "2026-09-24" : "2026-09-25", row.getString("tradeDate"));
            message.removeField("1".equals(side) ? 18166 : 18168);
            row = mapper.map(message).getJSONArray("quotes").getJSONObject(0);
            assertEquals("2026-09-23", row.getString("tradeDate"));
            message.removeField(75);
            row = mapper.map(message).getJSONArray("quotes").getJSONObject(0);
            assertEquals("2026-09-22", row.getString("tradeDate"));
        }
    }

    @Test
    void mapsBothSidesAndAllFourPriceTypes() throws Exception {
        for (String side : new String[] {"1", "2"}) {
            for (String type : new String[] {"1", "2", "3", "4"}) {
                JSONObject request = new QTradeQuoteMapper().map(quote(side, type));
                JSONObject row = request.getJSONArray("quotes").getJSONObject(0);
                assertEquals("250001.IB", row.getString("securityId"));
                assertEquals(side, row.getString("side"));
                assertEquals("1234.5678", row.getString("quantity"));
                assertEquals("Institution from QTrade", row.getString("institutionName"));
                assertEquals("Counterparty contact", row.getString("contactName"));
                assertNull(row.get("institutionId"));
                assertNull(row.get("contactId"));
                assertEquals("2", row.getString("settlType"));
                assertEquals(
                        "QT-ACCOUNT-1", request.getJSONObject("intake").getString("sourceUserId"));
                if ("1".equals(type) || "2".equals(type)) {
                    assertEquals("1", row.getString("quoteType"));
                    assertEquals(type, row.getString("yieldType"));
                } else {
                    assertEquals("3".equals(type) ? "2" : "3", row.getString("quoteType"));
                    assertNull(row.get("yieldType"));
                }
            }
        }
    }

    @Test
    void replayIgnoresTransportSequenceAndSendingTime() throws Exception {
        Message message = quote("1", "1");
        QTradeQuoteMapper mapper = new QTradeQuoteMapper();
        JSONObject first = mapper.map(message);
        message.getHeader().setString(34, "100");
        message.getHeader().setString(43, "Y");
        message.getHeader().setString(52, "20260923-02:03:04.000");
        assertEquals(first, mapper.map(message));
        message.setString(18004, "QT-ACCOUNT-2");
        assertNotEquals(first.getString("requestId"), mapper.map(message).getString("requestId"));
    }

    @Test
    void excludesNonQuoteEventsAndRejectsInvalidInput() throws Exception {
        QTradeQuoteMapper mapper = new QTradeQuoteMapper();
        Message message = quote("1", "1");
        message.getHeader().setString(35, "8");
        assertNull(mapper.map(message));
        message = quote("1", "1");
        message.setString(18045, "S");
        assertNull(mapper.map(message));
        Message invalid = quote("1", "1");
        invalid.setString(132, "2.50005");
        assertThrows(IllegalArgumentException.class, () -> mapper.map(invalid));
        invalid.setString(132, "2.5000");
        invalid.removeField(18004);
        assertThrows(IllegalArgumentException.class, () -> mapper.map(invalid));
    }

    @Test
    void inboxSurvivesRestartAndDoesNotDuplicateSentReplay() throws Exception {
        JSONObject request = new QTradeQuoteMapper().map(quote("1", "1"));
        try (DurableInbox inbox = new DurableInbox(directory, 2)) {
            inbox.enqueue(request);
            inbox.enqueue(request);
            assertThrows(Exception.class, () -> new DurableInbox(directory, 2));
        }
        try (DurableInbox inbox = new DurableInbox(directory, 2)) {
            Path pending = inbox.next();
            assertEquals(request, inbox.request(pending));
            JSONObject resolved = JsonUtils.deepCopy(request);
            resolved.getJSONObject("intake").put("ownerUserId", "STC-1");
            inbox.saveRequest(pending, resolved);
            inbox.finish(pending, JsonUtils.object("Code", "0000"), true);
            inbox.enqueue(request);
            assertNull(inbox.next());
            JSONObject changed = JsonUtils.deepCopy(request);
            changed.getJSONArray("quotes").getJSONObject(0).put("price", "9");
            assertThrows(java.io.IOException.class, () -> inbox.enqueue(changed));
        }
    }

    @Test
    void retriesSameRequestAndOwnerAfterTimeoutAndChecksNestedReply() throws Exception {
        JSONObject request = new QTradeQuoteMapper().map(quote("1", "1"));
        AtomicInteger resolutions = new AtomicInteger();
        FakeTransport transport = new FakeTransport();
        try (DurableInbox inbox = new DurableInbox(directory, 2)) {
            inbox.enqueue(request);
            QuoteForwarder forwarder =
                    new QuoteForwarder(
                            inbox,
                            transport,
                            "test-token",
                            account -> {
                                resolutions.incrementAndGet();
                                return "STC-1";
                            });
            transport.fail = true;
            assertThrows(IllegalStateException.class, forwarder::tick);
            JSONObject first = JsonUtils.deepCopy(transport.last);
            transport.fail = false;
            transport.reply =
                    JsonUtils.object(
                            "Code",
                            "0000",
                            "Data",
                            JsonUtils.object(
                                    "results",
                                    JsonUtils.array(
                                            JsonUtils.object(
                                                    "Code", "1001", "Msg", "Invalid bond"))));
            forwarder.tick();
            assertEquals(first, transport.last);
            assertEquals(1, resolutions.get());
            assertNull(inbox.next());
            try (java.util.stream.Stream<Path> files = Files.list(directory.resolve("rejected"))) {
                assertEquals(1, files.count());
            }
            try (java.util.stream.Stream<Path> files = Files.list(directory.resolve("rejected"))) {
                String persisted =
                        new String(
                                Files.readAllBytes(files.findFirst().get()),
                                java.nio.charset.StandardCharsets.UTF_8);
                assertFalse(persisted.contains("test-token"));
            }
        }
    }

    @Test
    void rejectsOverflowWithoutDiscardingExistingWork() throws Exception {
        QTradeQuoteMapper mapper = new QTradeQuoteMapper();
        try (DurableInbox inbox = new DurableInbox(directory, 1)) {
            inbox.enqueue(mapper.map(quote("1", "1")));
            Message another = quote("1", "1");
            another.setString(117, "another");
            assertThrows(java.io.IOException.class, () -> inbox.enqueue(mapper.map(another)));
            assertNotNull(inbox.next());
        }
    }

    @Test
    void fixLoggingRedactsPasswords() {
        Message login = new Message();
        login.getHeader().setString(35, "A");
        login.setString(553, "user");
        login.setString(554, "secret-value");
        assertFalse(QTradeFixReceiver.safeFix(login).contains("secret-value"));
        assertTrue(QTradeFixReceiver.safeFix(login).contains("554=[REDACTED]"));
    }

    static final class FakeTransport implements StcTransport {
        JSONObject reply;
        JSONObject last;
        boolean fail;

        public boolean online() {
            return true;
        }

        public void subscribe(String topic, Consumer<JSONObject> listener) {}

        public void connectionChanged(Runnable listener) {}

        public JSONObject request(String topic, String sessionId, JSONObject request) {
            assertEquals("ficc.quote.create", topic);
            assertNull(sessionId);
            last = JsonUtils.deepCopy(request);
            if (fail) {
                throw new IllegalStateException("timeout after possible server commit");
            }
            return reply;
        }
    }
}
