package ficc.api;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.concurrent.ConcurrentHashMap;

/**
 * JSON 与强类型协议模型的转换边界，保留未知字段、字段是否出现以及原始标量表示。
 * 更新请求中“未提供字段”与“显式清空字段”含义不同；后者通过 ProtocolModel.clearField 表达。
 * 兼容信息用于协议往返，不应在逐条报价的筛选热路径中反复编解码。
 */
public final class ProtocolCodec {
    private static final Map<Class<?>, Field[]> FIELDS = new ConcurrentHashMap<>();

    private ProtocolCodec() {}

    private static Field[] fields(Class<?> type) {
        return FIELDS.computeIfAbsent(type, Class::getFields);
    }

    public static <T extends ProtocolModel> T decode(JSONObject input, Class<T> type) {
        if (input == null) {
            throw new IllegalArgumentException("Expected a JSON object for " + type.getSimpleName());
        }
        T result = input.toJavaObject(type);
        // One detached wire tree per message, shared by its nested DTO metadata.
        attach(result, copy(input));
        return result;
    }

    private static void attach(Object value, Object input) {
        try {
            if (value instanceof ProtocolModel && input instanceof Map) {
                ProtocolModel model = (ProtocolModel) value;
                model.original = (JSONObject) input;
                model.baseline = new JSONObject();
                for (Field field : fields(model.getClass())) {
                    Object current = field.get(model);
                    model.baseline.put(field.getName(), baseline(current));
                    attach(current, model.original.get(field.getName()));
                }
            } else if (value instanceof List && input instanceof List) {
                List<?> values = (List<?>) value;
                List<?> originals = (List<?>) input;
                for (int index = 0; index < values.size(); index++) {
                    attach(values.get(index), originals.get(index));
                }
            } else if (value instanceof Map && input instanceof Map) {
                for (Map.Entry<?, ?> entry : ((Map<?, ?>) value).entrySet()) {
                    attach(entry.getValue(), ((Map<?, ?>) input).get(entry.getKey()));
                }
            }
        } catch (IllegalAccessException error) {
            throw new IllegalStateException("Cannot read protocol fields", error);
        }
    }

    private static Object baseline(Object value) {
        if (value instanceof List) {
            return new ArrayList<>((List<?>) value);
        }
        if (value instanceof Map) {
            return new java.util.LinkedHashMap<>((Map<?, ?>) value);
        }
        return value;
    }

    public static JSONObject encode(ProtocolModel model) {
        JSONObject result = new JSONObject();
        if (model.original != null) {
            result.putAll(model.original);
        }
        try {
            for (Field field : fields(model.getClass())) {
                String name = field.getName();
                Object value = field.get(model);
                Object previous = model.baseline == null ? null : model.baseline.get(name);
                Object original = model.original == null ? null : model.original.get(name);
                if (value != null) {
                    result.put(name, encodeValue(value, previous, original));
                } else if (previous != null || model.explicitNulls.contains(name)) {
                    result.put(name, null);
                }
            }
        } catch (IllegalAccessException error) {
            throw new IllegalStateException("Cannot serialize protocol model", error);
        }
        return result;
    }

    private static Object encodeValue(Object value, Object previous, Object original) {
        if (value instanceof ProtocolModel) {
            return encode((ProtocolModel) value);
        }
        if (value instanceof Collection) {
            JSONArray result = new JSONArray();
            List<?> before = previous instanceof List ? (List<?>) previous : java.util.Collections.emptyList();
            List<?> wire = original instanceof List ? (List<?>) original : java.util.Collections.emptyList();
            int index = 0;
            for (Object item : (Collection<?>) value) {
                result.add(encodeValue(item, index < before.size() ? before.get(index) : null,
                        index < wire.size() ? wire.get(index) : null));
                index++;
            }
            return result;
        }
        if (value instanceof Map) {
            JSONObject result = new JSONObject();
            Map<?, ?> before = previous instanceof Map ? (Map<?, ?>) previous : java.util.Collections.emptyMap();
            Map<?, ?> wire = original instanceof Map ? (Map<?, ?>) original : java.util.Collections.emptyMap();
            for (Map.Entry<?, ?> entry : ((Map<?, ?>) value).entrySet()) {
                Object key = entry.getKey();
                result.put(key.toString(), encodeValue(entry.getValue(), before.get(key), wire.get(key)));
            }
            return result;
        }
        return Objects.equals(value, previous) ? original : value;
    }

    private static Object copy(Object value) {
        if (value instanceof Map) {
            JSONObject result = new JSONObject();
            for (Map.Entry<?, ?> entry : ((Map<?, ?>) value).entrySet()) {
                result.put(entry.getKey().toString(), copy(entry.getValue()));
            }
            return result;
        }
        if (value instanceof Collection) {
            JSONArray result = new JSONArray();
            for (Object item : (Collection<?>) value) {
                result.add(copy(item));
            }
            return result;
        }
        return value;
    }
}
