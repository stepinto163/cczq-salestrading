package ficc.common;

import java.util.UUID;

/** Generates the existing prefix + UUID identifiers used by business messages. */
public final class IdGenerator {
    private IdGenerator() {}

    public static String newId(String prefix) {
        return prefix + UUID.randomUUID().toString();
    }
}
