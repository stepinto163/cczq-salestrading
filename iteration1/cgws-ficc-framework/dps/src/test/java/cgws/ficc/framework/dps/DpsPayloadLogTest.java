package cgws.ficc.framework.dps;

import static org.junit.jupiter.api.Assertions.*;

import org.junit.jupiter.api.Test;

class DpsPayloadLogTest {
    @Test
    void retainsBusinessFieldsAndMasksNestedCredentialsWithoutChangingInput() {
        String message =
                "{\"price\":\"2.5\",\"intake\":{\"token\":\"private-value\"},\"items\":[{\"pass_word\":\"hidden\"}]}";
        String log = DpsPayloadLog.body(message);
        assertTrue(log.contains("2.5"));
        assertFalse(log.contains("private-value"));
        assertFalse(log.contains("hidden"));
        assertTrue(message.contains("private-value"));
        assertTrue(DpsPayloadLog.body("password=hidden").startsWith("[UNPARSEABLE_JSON"));
    }
}
