package ficc.common;

import static org.junit.jupiter.api.Assertions.*;

import org.junit.jupiter.api.Test;

import sc.app.stc.utils.Consts;

import java.util.HashSet;
import java.util.Set;

class IdGeneratorTest {
    @Test
    void usesTheWorkerInitializedForEachStcTransport() {
        String previousTransport = Consts.NetType;
        String commonPrefix = com.app.common.utils.IdUtil.Prefix;
        String cedaPrefix = sc.app.stc.utils.IdUtil.Prefix;
        try {
            com.app.common.utils.IdUtil.Prefix = "";
            sc.app.stc.utils.IdUtil.Prefix = "";
            com.app.common.utils.IdUtil.setWorkerId(7);
            sc.app.stc.utils.IdUtil.setWorkerId(11);
            Consts.NetType = "dps";
            verifyWorker(7);
            Consts.NetType = "ceda";
            verifyCedaDelegation();
        } finally {
            Consts.NetType = previousTransport;
            com.app.common.utils.IdUtil.Prefix = commonPrefix;
            sc.app.stc.utils.IdUtil.Prefix = cedaPrefix;
        }
    }

    private void verifyCedaDelegation() {
        // The bundled CEDA IdUtil trims its decimal result; do not assume bit layout.
        long before = Long.parseLong(sc.app.stc.utils.IdUtil.getId());
        String generated = IdGenerator.newId("Q-");
        long after = Long.parseLong(sc.app.stc.utils.IdUtil.getId());
        assertTrue(generated.matches("Q-[0-9]+"));
        long value = Long.parseLong(generated.substring(2));
        assertTrue(before < value);
        assertTrue(value < after);
    }

    private void verifyWorker(int expectedWorker) {
        Set<String> ids = new HashSet<>();
        long previous = -1;
        for (int index = 0; index < 32; index++) {
            String id = IdGenerator.newId("Q-");
            assertTrue(id.matches("Q-[0-9]+"));
            assertTrue(ids.add(id));
            long value = Long.parseLong(id.substring(2));
            assertEquals(expectedWorker, (value >>> 12) & 31);
            assertEquals(0, (value >>> 17) & 31);
            long time = (value >>> 22) + 1420041600000L;
            assertTrue(Math.abs(System.currentTimeMillis() - time) < 1000);
            assertTrue(value > previous);
            previous = value;
        }
    }
}
