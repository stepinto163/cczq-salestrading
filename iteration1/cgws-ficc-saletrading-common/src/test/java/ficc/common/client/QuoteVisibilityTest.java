package ficc.common.client;

import org.junit.jupiter.api.Test;
import java.util.Properties;
import static org.junit.jupiter.api.Assertions.*;

class QuoteVisibilityTest {
    private final AccessPolicy policy = new AccessPolicy(new Properties());

    @Test
    void confirmedQuotesAreSharedButWriteOwnershipIsUnchanged() {
        for (String source : new String[] {"QT", "MANUAL", null}) {
            assertTrue(policy.canViewQuote("B", "A", source, "ACTIVE"));
            assertTrue(policy.canViewQuote("B", "A", source, "REFERRED"));
            assertTrue(policy.canViewQuote("B", "A", source, "DELETED", "2026-10-16T10:00:00+08:00"));
            assertFalse(policy.owns("B", "A"));
            assertFalse(policy.canViewQuote(null, "A", source, "ACTIVE"));
        }
    }

    @Test
    void pendingAndNeverConfirmedDeletedQuotesRemainPrivate() {
        assertFalse(policy.canViewQuote("B", "A", "QT", "PENDING_CONFIRM"));
        assertFalse(policy.canViewQuote("B", "A", "QT", "DELETED", null));
        assertFalse(policy.canViewQuote("B", "A", "QT", "DELETED", " "));
        assertTrue(policy.canViewQuote("A", "A", "QT", "PENDING_CONFIRM"));
        assertTrue(policy.canViewQuote("U-ADMIN", "A", "QT", "PENDING_CONFIRM"));
        assertTrue(policy.canViewQuote("S-SEARCH", "A", "QT", "PENDING_CONFIRM"));
    }
}
