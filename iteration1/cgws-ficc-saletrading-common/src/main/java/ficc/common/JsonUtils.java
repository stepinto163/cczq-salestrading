package ficc.common;

import com.alibaba.fastjson2.*;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.*;
import java.util.stream.Collectors;

/** JSON construction, serialization, copying and deterministic content hashing. */
public final class JsonUtils {
    private JsonUtils() {}

    public static JSONObject object(Object... keyValues) {
        JSONObject result = new JSONObject();
        for (int index = 0; index < keyValues.length; index += 2) {
            result.put(keyValues[index].toString(), keyValues[index + 1]);
        }
        return result;
    }

    public static JSONArray array(Object... values) {
        JSONArray result = new JSONArray();
        Collections.addAll(result, values);
        return result;
    }

    public static String toJson(Object value) {
        return JSON.toJSONString(value, JSONWriter.Feature.WriteNulls);
    }

    public static JSONObject deepCopy(JSONObject value) {
        return value == null ? null : JSON.parseObject(toJson(value));
    }

    public static JSONObject parseObject(String json) {
        return JSON.parseObject(json);
    }

    public static String getStringOrDefault(JSONObject object, String key, String defaultValue) {
        String value = object.getString(key);
        return value == null ? defaultValue : value;
    }

    public static String canonicalHash(Object value) {
        try {
            byte[] bytes = toJson(canonicalize(value)).getBytes(StandardCharsets.UTF_8);
            return toHex(MessageDigest.getInstance("SHA-256").digest(bytes));
        } catch (Exception exception) {
            throw new IllegalStateException(exception);
        }
    }

    public static Object canonicalize(Object value) {
        if (value instanceof Map<?, ?>) {
            Map<?, ?> map = (Map<?, ?>) value;
            TreeMap<String, Object> sorted = new TreeMap<>();
            map.forEach((key, item) -> sorted.put(key.toString(), canonicalize(item)));
            return sorted;
        }
        if (value instanceof List<?>) {
            List<?> list = (List<?>) value;
            return list.stream().map(JsonUtils::canonicalize).collect(Collectors.toList());
        }
        return value;
    }

    private static String toHex(byte[] bytes) {
        StringBuilder text = new StringBuilder(bytes.length * 2);
        for (byte item : bytes) {
            String hex = Integer.toHexString(item & 0xff);
            if (hex.length() == 1) {
                text.append('0');
            }
            text.append(hex);
        }
        return text.toString();
    }
}
