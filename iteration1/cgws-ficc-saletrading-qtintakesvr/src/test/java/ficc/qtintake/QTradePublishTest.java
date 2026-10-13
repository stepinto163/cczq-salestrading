package ficc.qtintake;

import ficc.api.QTradePublishData;
import ficc.api.QTradePublishRequest;
import ficc.common.ApiException;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;
import quickfix.Group;
import quickfix.Message;
import java.nio.file.Path;
import java.time.LocalDateTime;
import java.util.Arrays;
import java.util.Collections;
import java.util.concurrent.atomic.AtomicInteger;
import static org.junit.jupiter.api.Assertions.*;

class QTradePublishTest {
    @TempDir Path directory;

    @Test void onlySenderSessionOrExistingServiceCredentialCanPublish() throws Exception {
        QTradePublishRequest input = request("APP-123456789");
        String token = "01234567890123456789012345678901";
        assertEquals("owner", QTradePublishAuthorization.authorize(input, "sid", token,
                sid -> "owner", account -> "owner"));
        assertThrows(ApiException.class, () -> QTradePublishAuthorization.authorize(input, "sid", token,
                sid -> "other", account -> "owner"));
        assertThrows(ApiException.class, () -> QTradePublishAuthorization.authorize(input, "sid", token,
                sid -> null, account -> "owner"));
        input.token = "incorrect";
        assertThrows(ApiException.class, () -> QTradePublishAuthorization.authorize(input, "sid", token,
                sid -> "owner", account -> "owner"));
        input.token = token;
        assertEquals("service", QTradePublishAuthorization.authorize(input, null, token,
                sid -> { fail("service call must not impersonate a user"); return null; },
                account -> { fail("external sender need not log into STC"); return null; }));
    }

    static QTradePublishRequest request(String id) {
        QTradePublishRequest request = new QTradePublishRequest();
        request.messageId = id;
        request.senderQtAccount = "100000001";
        request.mode = "TEXT";
        request.preText = "Bid 240210 2.5 1w T+1\nOfr 240215 2.6 1w T+0";
        QTradePublishRequest.Recipient recipient = new QTradePublishRequest.Recipient();
        recipient.routingType = 58;
        recipient.routingId = "100000002";
        request.recipients = Collections.singletonList(recipient);
        return request;
    }

    static Message ack(String id, String code, int count) {
        Message ack = new Message();
        ack.getHeader().setString(8, "FIX.4.4");
        ack.getHeader().setString(35, "U29");
        ack.setString(18044, id);
        ack.setString(11251, code);
        for (int i = 0; i < count; i++) {
            Group group = new Group(11261, 216, new int[] {216, 217, 58, 18218, 18031, 0});
            group.setInt(216, count == 1 ? 56 : 58);
            group.setString(217, count == 1 ? "ALL" : "failed-" + i);
            group.setString(58, "0".equals(code) ? "succes" : "发布失败" + i);
            if (!"0".equals(code)) {
                group.setInt(18218, 1);
            }
            ack.addGroup(group);
        }
        return ack;
    }

    @Test void textUsesU41AndExplicitGroupRoutingWithoutStructuredQuotes() throws Exception {
        QTradePublishRequest input = request("APP-123456789");
        input.recipients.get(0).routingType = 53;
        Message message = QTradePublishMessage.create(input, LocalDateTime.of(2026, 10, 13, 10, 20));
        assertEquals("U41", message.getHeader().getString(35));
        assertEquals("20261013-10:20:00", message.getString(60));
        assertEquals(input.preText, message.getString(18115));
        assertFalse(message.isSetField(18117));
        assertEquals(53, message.getGroup(1, 215).getInt(216));
        assertEquals("100000002", message.getGroup(1, 215).getString(217));
    }

    @Test void structuredTwoSidedQuoteKeepsYuanAndEachSidesSettlement() throws Exception {
        QTradePublishRequest input = request("APP-123456789");
        input.mode = "STRUCTURED";
        QTradePublishRequest.Quote quote = new QTradePublishRequest.Quote();
        quote.clientQuoteId = "intent-1";
        quote.securityId = "240210";
        quote.exchange = "XCFE";
        quote.symbol = "24国开10";
        quote.side = "y";
        quote.bidPriceType = 1;
        quote.bidPrice = "2.5000";
        quote.bidQuantityYuan = "10000";
        quote.bidTradeDate = "2026-10-13";
        quote.bidSettlementType = "2";
        quote.offerPriceType = 2;
        quote.offerPrice = "2.4000";
        quote.offerQuantityYuan = "20000";
        quote.offerTradeDate = "2026-10-14";
        quote.offerSettlementType = "1";
        input.quotes = Collections.singletonList(quote);
        Group wire = QTradePublishMessage.create(input, LocalDateTime.now()).getGroup(1, 18117);
        assertEquals(55, wire.delim());
        assertTrue(QTradePublishMessage.create(input, LocalDateTime.now()).toString().contains("18117=1\u000155="));
        String encoded = QTradePublishMessage.create(input, LocalDateTime.now()).toString();
        int previousPosition = -1;
        // 独立记录对端字典要求的顺序，防止照表格顺序编码造成 FIX Reject。
        for (int tag : new int[] {55, 18031, 117, 48, 100, 54, 18042, 132, 18043, 133,
                134, 135, 18167, 18169, 18166, 18168}) {
            int position = encoded.indexOf("\u0001" + tag + "=");
            assertTrue(position > previousPosition, "Wire order for tag " + tag);
            previousPosition = position;
        }
        assertEquals("-10000", wire.getString(117));
        assertEquals("10000", wire.getString(134));
        assertEquals("20000", wire.getString(135));
        assertEquals("20261013", wire.getString(18166));
        assertEquals("20261014", wire.getString(18168));
        assertEquals("2", wire.getString(18167));
        assertEquals("1", wire.getString(18169));
        quote.bidPrice = "-2";
        assertThrows(ApiException.class, () -> QTradePublishMessage.create(input, LocalDateTime.now()));
    }

    @Test void invalidRecipientsAndDelimiterAreRejectedBeforeSend() {
        QTradePublishRequest input = request("APP-123456789");
        input.recipients.get(0).routingId = input.senderQtAccount;
        assertThrows(ApiException.class, () -> QTradePublishMessage.create(input, LocalDateTime.now()));
        input.recipients.get(0).routingId = "other\u0001";
        assertThrows(ApiException.class, () -> QTradePublishMessage.create(input, LocalDateTime.now()));
        input.recipients.get(0).routingId = "100000002";
        input.recipients = Arrays.asList(input.recipients.get(0), input.recipients.get(0));
        assertThrows(ApiException.class, () -> QTradePublishMessage.create(input, LocalDateTime.now()));
    }

    @Test void ackSuccessIsDurableAndRepeatDoesNotPublishAgain() throws Exception {
        QTradePublisher publisher = new QTradePublisher(directory, 100);
        QTradePublishRequest input = request("APP-123456789");
        AtomicInteger sends = new AtomicInteger();
        QTradePublishData result = publisher.publish(input, message -> {
            sends.incrementAndGet();
            message.getHeader().setInt(34, 42);
            publisher.sending(message);
            publisher.acknowledge(ack(input.messageId, "0", 1));
            return true;
        });
        assertEquals("ACKNOWLEDGED", result.status);
        assertEquals(42, result.fixSequence);
        QTradePublisher restarted = new QTradePublisher(directory, 100);
        assertEquals("ACKNOWLEDGED", restarted.publish(input, message -> { fail("duplicate send"); return false; }).status);
        assertEquals(1, sends.get());
        input.preText = "different";
        assertThrows(ApiException.class, () -> restarted.publish(input, message -> true));
    }

    @Test void rawPartialAcknowledgementPreservesAllRecipientFailures() throws Exception {
        QTradePublisher publisher = new QTradePublisher(directory, 100);
        QTradePublishRequest input = request("APP-123456789");
        QTradePublishData result = publisher.publish(input, message -> {
            Message raw = new Message();
            raw.fromString(ack(input.messageId, "1", 2).toString(), null, false);
            publisher.acknowledge(raw);
            return true;
        });
        assertEquals("PARTIAL", result.status);
        assertEquals(2, result.recipients.size());
        assertEquals("failed-1", result.recipients.get(1).routingId);
        assertEquals("发布失败1", result.recipients.get(1).description);
    }

    @Test void timeoutIsUnknownAndLateAckUpdatesReceiptWithoutResend() throws Exception {
        QTradePublisher publisher = new QTradePublisher(directory, 100);
        QTradePublishRequest input = request("APP-123456789");
        assertEquals("UNKNOWN", publisher.publish(input, message -> true).status);
        publisher.acknowledge(ack(input.messageId, "2", 1));
        assertEquals("REJECTED", publisher.publish(input, message -> { fail("resend"); return true; }).status);
    }

    @Test void sessionRejectCorrelatesBySequence() throws Exception {
        QTradePublisher publisher = new QTradePublisher(directory, 100);
        QTradePublishRequest input = request("APP-123456789");
        assertEquals("REJECTED", publisher.publish(input, message -> {
            message.getHeader().setInt(34, 77);
            publisher.sending(message);
            Message reject = new Message();
            reject.setInt(45, 77);
            reject.setString(58, "Bad tag");
            publisher.reject(reject);
            return true;
        }).status);
    }

    @Test void offlineDoesNotReportSuccessAndExcessRateDoesNotSend() throws Exception {
        QTradePublisher publisher = new QTradePublisher(directory, 100);
        assertEquals("REJECTED", publisher.publish(request("APP-123456781"), message -> false).status);
        assertEquals("REJECTED", publisher.publish(request("APP-123456782"), message -> false).status);
        ApiException error = assertThrows(ApiException.class,
                () -> publisher.publish(request("APP-123456783"), message -> { fail("over limit"); return true; }));
        assertEquals("RATE_LIMITED", error.details.getString("type"));
    }

    @Test void disconnectDoesNotTurnAmbiguousSendIntoFailureOrSuccess() throws Exception {
        QTradePublisher publisher = new QTradePublisher(directory, 100);
        assertEquals("UNKNOWN", publisher.publish(request("APP-123456789"), message -> {
            publisher.disconnected();
            return true;
        }).status);
    }
}
