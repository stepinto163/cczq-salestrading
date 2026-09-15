package ficc.common;

import java.time.OffsetDateTime;
import java.time.ZoneId;

/** Business timestamps retain the client's Asia/Shanghai zone and ISO format. */
public final class BusinessClock {
    private BusinessClock() {}

    public static String currentTime() {
        return OffsetDateTime.now(ZoneId.of("Asia/Shanghai")).toString();
    }
}
