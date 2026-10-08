package ficc.common.client;

import org.junit.jupiter.api.Test;
import java.util.Properties;
import static org.junit.jupiter.api.Assertions.*;

class QuoteVisibilityTest {
    private final AccessPolicy policy = new AccessPolicy(new Properties());

    @Test
    void qtradeRecipientVisibilitySurvivesConfirmation() {
        assertTrue(policy.canViewQuote("A", "A", "QT", "ACTIVE"));
        assertFalse(policy.canViewQuote("B", "A", "QT", "ACTIVE"));
        assertFalse(policy.canViewQuote("B", "A", "QT", "PENDING_CONFIRM"));
        assertTrue(policy.canViewQuote("U-ADMIN", "A", "QT", "ACTIVE"));
        assertTrue(policy.canViewQuote("S-SEARCH", "A", "QT", "ACTIVE"));
        assertFalse(policy.canViewQuote(null, "A", "QT", "ACTIVE"));
    }

    @Test
    void existingManualQuoteVisibilityIsPreserved() {
        assertTrue(policy.canViewQuote("B", "A", "MANUAL", "ACTIVE"));
        assertFalse(policy.canViewQuote("B", "A", "MANUAL", "PENDING_CONFIRM"));
    }
}
