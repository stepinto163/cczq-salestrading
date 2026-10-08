package ficc.qtintake;

import ficc.api.QuoteCreateRequest;
import org.junit.jupiter.api.Test;
import quickfix.Message;
import static org.junit.jupiter.api.Assertions.*;

/** Shapes from the 2026-10-08 three-trader group/counteroffer capture. */
class QTradeNegotiationTest {
    @Test
    void receiverKeepsConversationAndMissingIntentionSeparate() throws Exception {
        Message message = received("B", "-10000", "A-B", "1", "2");
        message.setString(18039, "2");
        message.setString(18474, "CHAT-GROUP-1");
        QuoteCreateRequest request = new QTradeQuoteMapper().mapRequest(message);
        assertEquals("-10000", request.intake.externalQuoteId);
        assertEquals("A-B", request.intake.negotiationId);
        assertEquals("CHAT-GROUP-1", request.intake.chatGroupId);
        assertEquals("2", request.intake.chatType);
        assertEquals("counterparty-A-B", request.intake.counterpartyId);
    }

    @Test
    void ownPublishedIntentionIsNotAnIncomingCounterpartyQuote() throws Exception {
        Message message = received("A", "INTENTION-1", "A-B", "1", "2");
        message.removeField(18033);
        message.setString(18035, "1");
        assertNull(new QTradeQuoteMapper().mapRequest(message));
    }

    @Test
    void recipientsAndConversationsRemainDistinctEvenAtTheSameSecond() throws Exception {
        QTradeQuoteMapper mapper = new QTradeQuoteMapper();
        Message toB = received("B", "-10000", "A-B", "1", "2");
        Message toC = received("C", "-10000", "A-C", "1", "2");
        QuoteCreateRequest b = mapper.mapRequest(toB);
        QuoteCreateRequest c = mapper.mapRequest(toC);
        assertNotEquals(b.requestId, c.requestId);
        assertEquals("B", b.intake.sourceUserId);
        assertEquals("A-B", b.intake.negotiationId);

        // Publisher A receives both counteroffers as N with the SAME original QuoteID.
        Message replyB = received("A", "INTENTION-1", "A-B", "2", "1.99");
        Message replyC = received("A", "INTENTION-1", "A-C", "2", "2.01");
        QuoteCreateRequest ab = mapper.mapRequest(replyB);
        QuoteCreateRequest ac = mapper.mapRequest(replyC);
        assertNotEquals(ab.requestId, ac.requestId);
        assertEquals("INTENTION-1", ab.intake.externalQuoteId);
        assertEquals("A-B", ab.intake.negotiationId);
        assertEquals("1.99", ab.quotes.get(0).price);
        assertEquals("A-C", ac.intake.negotiationId);
        assertEquals("2.01", ac.quotes.get(0).price);
        replyB.getHeader().setString(34, "999");
        assertEquals(ab.requestId, mapper.mapRequest(replyB).requestId);

        // The reply sender's own H representation must not become an incoming quote.
        replyC.setString(18035, "1");
        replyC.setString(18045, "H");
        assertNull(mapper.mapRequest(replyC));
    }

    private static Message received(String account, String intention, String conversation,
            String side, String price) {
        Message message = QtIntakeTest.quote(side, "1");
        message.setString(117, intention);
        message.setString(18033, conversation);
        message.setString(18004, account);
        message.setString(18058, "counterparty-" + conversation);
        message.setString(18039, "1");
        message.setString("1".equals(side) ? 132 : 133, price);
        message.setString(60, "20261008-14:36:46");
        return message;
    }
}
