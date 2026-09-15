package ficc.common.client;

import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import java.util.Locale;
import java.util.Set;

/** Diagnostic copies only: never change the message sent to DPS. */
final class DpsMessageLog {
    private static final Set<String> SECRET_FIELDS = Set.of(
            "password", "passwd", "pwd", "sid", "sessionid", "authorization",
            "accesstoken", "refreshtoken", "token", "param1", "param2", "validcode");

    private DpsMessageLog() {}

    static String body(String content) {
        if (content == null) {
            return "null";
        }
        try {
            return JSON.toJSONString(mask(JSON.parse(content)));
        } catch (RuntimeException error) {
            // Unparseable payloads cannot be reliably stripped of credentials.
            return "[UNPARSEABLE_JSON characters=" + content.length() + "]";
        }
    }

    private static Object mask(Object value) {
        if (value instanceof JSONObject object) {
            for (String key : object.keySet()) {
                String normalized = key.replace("_", "").replace("-", "").toLowerCase(Locale.ROOT);
                object.put(key, SECRET_FIELDS.contains(normalized) ? "[REDACTED]" : mask(object.get(key)));
            }
        } else if (value instanceof JSONArray array) {
            for (int index = 0; index < array.size(); index++) {
                array.set(index, mask(array.get(index)));
            }
        } else if (value instanceof String text && (text.stripLeading().startsWith("{")
                || text.stripLeading().startsWith("["))) {
            return body(text);
        }
        return value;
    }
}
