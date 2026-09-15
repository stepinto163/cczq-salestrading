package ficc.gateway;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotEquals;

final class FiccSecurityCheckTest {
    private final FiccSecurityCheck security = new FiccSecurityCheck();

    @Test
    void blocksInternalSynchronizationRequestsThroughFrontendGateway() {
        for (String topic : new String[] {"ficc.quote.internal.state.query", "ficc.price.internal.snapshot.query"}) {
            assertEquals("1003", security.check(null, null, "QuoteSvr", topic, "{}").Code);
        }
    }

    @Test
    void leavesUserBusinessRequestAuthenticationToExistingLoginAndServiceChecks() {
        for (String topic : new String[] {"ficc.quote.create", "ficc.quote.query", "ficc.price.query"}) {
            assertNotEquals("1003", security.check(null, null, "QuoteSvr", topic, "{}").Code);
        }
    }
}
