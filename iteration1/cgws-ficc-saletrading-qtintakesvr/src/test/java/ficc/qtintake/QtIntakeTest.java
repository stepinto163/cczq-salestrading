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
        message.setString(18035, "2");
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
    void groupsOneOriginalMessageWithoutCollapsingItsQuotes() throws Exception {
        QTradeQuoteMapper mapper = new QTradeQuoteMapper();
        Message first = quote("1", "1");
        first.setString(18058, "sender-a");
        first.setString(18026, "250001 bid 2.5\n240210 ofr 2.6");
        JSONObject firstRequest = mapper.map(first);
        Message second = quote("2", "1");
        second.setString(18058, "sender-a");
        second.setString(18026, first.getString(18026));
        second.setString(117, "QT-QUOTE-456");
        second.setString(48, "240210");
        JSONObject secondRequest = mapper.map(second);
        String group = firstRequest.getJSONObject("intake").getString("originalMessageId");
        assertEquals(group, secondRequest.getJSONObject("intake").getString("originalMessageId"));
        assertNotEquals(firstRequest.getString("requestId"), secondRequest.getString("requestId"));
        second.getHeader().setString(34, "999");
        second.getHeader().setString(52, "20260924-12:00:00");
        assertEquals(secondRequest, mapper.map(second), "transport replay preserves both identities");

        for (int tag : new int[] {18004, 18058, 18136, 18474, 60, 18026}) {
            Message other = new Message();
            other.fromString(second.toString(), null, false);
            other.setString(tag, tag == 60 ? "20260922-01:02:04.123" : "different");
            assertNotEquals(group, mapper.map(other).getJSONObject("intake").getString("originalMessageId"),
                    "different account/sender/chat/time/text must not merge: " + tag);
        }
        first.removeField(18026);
        second.removeField(18026);
        assertNotEquals(mapper.map(first).getJSONObject("intake").getString("originalMessageId"),
                mapper.map(second).getJSONObject("intake").getString("originalMessageId"));
    }

    @Test
    void replayOfPreGroupingInboxRetainsItsOriginalRequest() throws Exception {
        JSONObject current = new QTradeQuoteMapper().map(quote("1", "1"));
        JSONObject old = JsonUtils.deepCopy(current);
        old.getJSONObject("intake").remove("originalMessageId");
        try (DurableInbox inbox = new DurableInbox(directory, 2)) {
            inbox.enqueue(old);
            inbox.enqueue(current);
            assertEquals(old, inbox.request(inbox.next()));
            JSONObject changed = JsonUtils.deepCopy(current);
            changed.getJSONArray("quotes").getJSONObject(0).put("price", "9");
            assertThrows(java.io.IOException.class, () -> inbox.enqueue(changed));
        }
    }

    @Test
    void emptyQuoteIdUsesNegotiationIdentityForEachQuoteInTheMessage() throws Exception {
        Message message = quote("1", "1");
        message.setString(117, "-10000");
        message.setString(18033, "NEG-A");
        QTradeQuoteMapper mapper = new QTradeQuoteMapper();
        JSONObject first = mapper.map(message);
        message.setString(18033, "NEG-B");
        JSONObject second = mapper.map(message);
        assertNotEquals(first.getString("requestId"), second.getString("requestId"));
        assertEquals(first.getJSONObject("intake").getString("originalMessageId"),
                second.getJSONObject("intake").getString("originalMessageId"));
        message.removeField(18033);
        assertThrows(IllegalArgumentException.class, () -> mapper.map(message));
    }

    @Test
    void mapsCapturedPrlBusinessDateAndBidSettlement() throws Exception {
        // The original capture is self-sent. Exercise its settlement shape as incoming.
        Message incoming = capturedPrlQuote();
        incoming.setString(18035, "2");
        JSONObject request = new QTradeQuoteMapper().map(incoming);
        JSONObject row = request.getJSONArray("quotes").getJSONObject(0);
        assertEquals("240210.IB", row.getString("securityId"));
        assertEquals("2026-09-22", row.getString("tradeDate"));
        assertEquals("2", row.getString("settlType"));
        assertEquals("3", row.getString("price"));
        assertEquals("1000", row.getString("quantity"));
        assertEquals("20260922-18:06:13", request.getJSONObject("intake").getString("sourceTime"));
    }

    @Test
    void senderDirectionIsIndependentOfBidOfferAndNewResponseActions() throws Exception {
        QTradeQuoteMapper mapper = new QTradeQuoteMapper();
        for (String side : new String[] {"1", "2"}) {
            for (String action : new String[] {"N", "H"}) {
                Message message = quote(side, "1");
                message.setString(18045, action);
                assertNotNull(mapper.map(message), "Both incoming sides/actions must be accepted");
                message.setString(18035, "1");
                assertNull(mapper.map(message), "Never attribute a self-sent quote to its recipient");
                assertEquals("SELF_SENT_QUOTE", mapper.ignoredReason(message));
            }
        }
        assertNull(mapper.map(capturedPrlQuote()), "Original PRL sample is also self-sent");
    }

    @Test
    void refusesMissingOrUnknownSenderDirectionInsteadOfGuessingFromCounterparty() {
        QTradeQuoteMapper mapper = new QTradeQuoteMapper();
        Message message = quote("1", "1");
        message.removeField(18035);
        assertTrue(assertThrows(IllegalArgumentException.class, () -> mapper.map(message))
                .getMessage().contains("18035"));
        message.setString(18035, "9");
        assertTrue(assertThrows(IllegalArgumentException.class, () -> mapper.map(message))
                .getMessage().contains("18035"));
    }

    @Test
    void chatReplayQueuesOnlyIncomingBidAndLogsOutgoingOfferAtInfo() throws Exception {
        // Actual 2026-09-29 chat shape: same negotiation and counterparty in both directions.
        Message bid = quote("1", "1");
        bid.setString(117, "-10000");
        bid.setString(18033, "CHAT-NEGOTIATION-1");
        bid.setString(48, "210210");
        bid.setString(132, "3.126");
        bid.setString(134, "4000000");
        bid.setString(60, "20260929-17:56:02");
        bid.setString(18026, "bid 210210 3.126 400W\\nremark");
        Message offer = quote("2", "1");
        offer.setString(117, "-10000");
        offer.setString(18033, "CHAT-NEGOTIATION-1");
        offer.setString(48, "210210");
        offer.setString(133, "3.106");
        offer.setString(135, "1000000");
        offer.setString(60, "20260929-19:09:16");
        offer.setString(18045, "H");
        offer.setString(18035, "1");
        offer.setString(18026, "Ofr 210210.IB 3.106 100 +1");

        java.io.StringWriter logOutput = new java.io.StringWriter();
        org.apache.log4j.Logger logger = org.apache.log4j.Logger.getLogger(QTradeFixReceiver.class);
        org.apache.log4j.Level previousLevel = logger.getLevel();
        org.apache.log4j.WriterAppender appender = new org.apache.log4j.WriterAppender(
                new org.apache.log4j.PatternLayout("%p %m%n"), logOutput);
        logger.addAppender(appender);
        logger.setLevel(org.apache.log4j.Level.INFO);
        try (DurableInbox inbox = new DurableInbox(directory, 4)) {
            java.util.Properties configuration = new java.util.Properties();
            configuration.setProperty("fix.username", "fixture");
            configuration.setProperty("fix.password", "fixture");
            QTradeFixReceiver receiver = new QTradeFixReceiver(configuration, inbox);
            quickfix.SessionID session = new quickfix.SessionID("FIX.4.4", "TEST", "QTRADE");
            receiver.fromApp(bid, session);
            receiver.fromApp(offer, session);
            receiver.fromApp(bid, session); // FIX replay must not duplicate the incoming quote.
            JSONObject request = inbox.request(inbox.next());
            JSONObject row = request.getJSONArray("quotes").getJSONObject(0);
            assertEquals("1", row.getString("side"));
            assertEquals("3.126", row.getString("price"));
            assertEquals("400", row.getString("quantity"));
            assertEquals("Counterparty contact", row.getString("contactName"));
            try (java.util.stream.Stream<Path> files = Files.list(directory.resolve("pending"))) {
                assertEquals(1, files.count());
            }
            FakeTransport transport = new FakeTransport();
            transport.reply = JsonUtils.object("Code", "0000", "Data", JsonUtils.object(
                    "results", JsonUtils.array(JsonUtils.object("Code", "0000"))));
            new QuoteForwarder(inbox, transport, "test-token", account -> "STC-1").tick();
            assertEquals("3.126", transport.last.getJSONArray("quotes")
                    .getJSONObject(0).getString("price"));
            assertNull(inbox.next(), "There must be no outgoing offer left to forward");
            assertTrue(logOutput.toString().contains("INFO FIX state=IGNORED"));
            assertTrue(logOutput.toString().contains("reason=SELF_SENT_QUOTE operator=1"));
            assertTrue(logOutput.toString().contains("negotiationId=CHAT-NEGOTIATION-1"));
        } finally {
            logger.removeAppender(appender);
            logger.setLevel(previousLevel);
            appender.close();
        }
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
            for (String retryCode : new String[] {"1009", "1099"}) {
                transport.reply = JsonUtils.object("Code", "0000", "Data",
                        JsonUtils.object("results", JsonUtils.array(
                                JsonUtils.object("Code", retryCode))));
                forwarder.tick();
                assertNotNull(inbox.next());
                assertEquals(first, transport.last);
            }
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
