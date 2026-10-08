package cgws.ficc.framework.dps;

import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import java.util.Arrays;
import java.util.HashSet;
import java.util.Locale;
import java.util.Set;

/** Masks diagnostic copies only; the DPS packet and application request are untouched. */
public final class DpsPayloadLog {
    private static final Set<String> SECRETS =
            new HashSet<>(
                    Arrays.asList(
                            "password",
                            "passwd",
                            "pwd",
                            "sid",
                            "sessionid",
                            "authorization",
                            "accesstoken",
                            "refreshtoken",
                            "token",
                            "param1",
                            "param2",
                            "validcode"));

    private DpsPayloadLog() {}

    public static String body(String content) {
        if (content == null) {
            return "null";
        }
        try {
            return JSON.toJSONString(mask(JSON.parse(content)));
        } catch (RuntimeException malformed) {
            return "[UNPARSEABLE_JSON characters=" + content.length() + "]";
        }
    }

    private static Object mask(Object value) {
        if (value instanceof JSONObject) {
            JSONObject object = (JSONObject) value;
            for (String key : object.keySet()) {
                String normalized = key.replace("_", "").replace("-", "").toLowerCase(Locale.ROOT);
                object.put(
                        key, SECRETS.contains(normalized) ? "[REDACTED]" : mask(object.get(key)));
            }
        } else if (value instanceof JSONArray) {
            JSONArray array = (JSONArray) value;
            for (int index = 0; index < array.size(); index++) {
                array.set(index, mask(array.get(index)));
            }
        } else if (value instanceof String) {
            String text = ((String) value).trim();
            if (text.startsWith("{") || text.startsWith("[")) {
                return body((String) value);
            }
        }
        return value;
    }
}
