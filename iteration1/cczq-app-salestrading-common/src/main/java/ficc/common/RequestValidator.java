package ficc.common;

import com.alibaba.fastjson2.JSONObject;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

/** Shared checks for required request values and supported fields. */
public final class RequestValidator {
    private RequestValidator() {}

    public static String requireString(JSONObject request, String key) {
        String value = request.getString(key);
        if (value == null || value.isBlank()) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", key + " is required", key);
        }
        return value;
    }

    public static void allowOnlyFields(JSONObject request, String... keys) {
        Set<String> allowed = new HashSet<>(Arrays.asList(keys));
        for (String key : request.keySet()) {
            if (!allowed.contains(key)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Unknown field: " + key, key);
            }
        }
    }
}
