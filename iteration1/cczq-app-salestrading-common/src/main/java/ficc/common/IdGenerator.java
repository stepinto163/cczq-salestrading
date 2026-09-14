package ficc.common;

import java.util.Objects;
import sc.app.stc.utils.Consts;

/** Uses the ID worker initialized by STC's selected server startup wrapper. */
public final class IdGenerator {
    private IdGenerator() {}

    public static synchronized String newId(String prefix) {
        Objects.requireNonNull(prefix, "ID prefix");
        String id;
        if ("ceda".equals(Consts.getNetType())) {
            id = sc.app.stc.utils.IdUtil.getId();
        } else {
            id = com.app.common.utils.IdUtil.getId();
        }
        return prefix + id;
    }
}
