package ficc.common;

import com.alibaba.fastjson2.JSONArray;

import java.util.Arrays;
import java.util.Collections;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Set;

/** Codes from sc.app.stc.enums.SettlTypeEnum; values are not sequential after T+4. */
public final class SettlementTypes {
    public static final List<String> CODES =
            Collections.unmodifiableList(
                    Arrays.asList(
                            "1", "2", "3", "4", "5", "9", "10", "11", "12", "13", "14", "15",
                            "16"));
    public static final Set<String> SUPPORTED =
            Collections.unmodifiableSet(new LinkedHashSet<>(CODES));

    private SettlementTypes() {}

    public static JSONArray dictionary() {
        JSONArray result = new JSONArray();
        for (int days = 0; days < CODES.size(); days++) {
            result.add(JsonUtils.object("value", CODES.get(days), "label", "T+" + days));
        }
        return result;
    }
}
