package cgws.ficc.parser.compare;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/**
 * Minimal JSON reader/writer (no third party dependency, Java 8).
 * Only used by the comparison tooling, never on the request path.
 */
public final class Json {

    private final String src;
    private int pos;

    private Json(String src) {
        this.src = src;
    }

    public static Object parse(String text) {
        Json json = new Json(text);
        json.skipWs();
        Object value = json.readValue();
        return value;
    }

    @SuppressWarnings("unchecked")
    public static Map<String, Object> asObject(Object value) {
        return value instanceof Map ? (Map<String, Object>) value : new LinkedHashMap<String, Object>();
    }

    @SuppressWarnings("unchecked")
    public static List<Object> asArray(Object value) {
        return value instanceof List ? (List<Object>) value : new ArrayList<Object>();
    }

    public static String str(Map<String, Object> obj, String key) {
        Object value = obj.get(key);
        return value == null ? null : String.valueOf(value);
    }

    public static String write(Object value) {
        StringBuilder b = new StringBuilder();
        writeValue(b, value);
        return b.toString();
    }

    private static void writeValue(StringBuilder b, Object value) {
        if (value == null) {
            b.append("null");
        } else if (value instanceof String) {
            writeString(b, (String) value);
        } else if (value instanceof Map) {
            b.append('{');
            boolean first = true;
            for (Map.Entry<?, ?> e : ((Map<?, ?>) value).entrySet()) {
                if (!first) {
                    b.append(',');
                }
                first = false;
                writeString(b, String.valueOf(e.getKey()));
                b.append(':');
                writeValue(b, e.getValue());
            }
            b.append('}');
        } else if (value instanceof List) {
            b.append('[');
            boolean first = true;
            for (Object item : (List<?>) value) {
                if (!first) {
                    b.append(',');
                }
                first = false;
                writeValue(b, item);
            }
            b.append(']');
        } else {
            b.append(String.valueOf(value));
        }
    }

    private static void writeString(StringBuilder b, String value) {
        b.append('"');
        for (int i = 0; i < value.length(); i++) {
            char c = value.charAt(i);
            switch (c) {
                case '"':
                    b.append("\\\"");
                    break;
                case '\\':
                    b.append("\\\\");
                    break;
                case '\n':
                    b.append("\\n");
                    break;
                case '\r':
                    b.append("\\r");
                    break;
                case '\t':
                    b.append("\\t");
                    break;
                default:
                    if (c < 0x20) {
                        b.append(String.format("\\u%04x", (int) c));
                    } else {
                        b.append(c);
                    }
            }
        }
        b.append('"');
    }

    private Object readValue() {
        char c = peek();
        if (c == '{') {
            return readObject();
        }
        if (c == '[') {
            return readArray();
        }
        if (c == '"') {
            return readString();
        }
        return readLiteral();
    }

    private Map<String, Object> readObject() {
        Map<String, Object> map = new LinkedHashMap<String, Object>();
        expect('{');
        skipWs();
        if (peek() == '}') {
            pos++;
            return map;
        }
        while (true) {
            skipWs();
            String key = readString();
            skipWs();
            expect(':');
            skipWs();
            map.put(key, readValue());
            skipWs();
            char c = peek();
            if (c == ',') {
                pos++;
                continue;
            }
            expect('}');
            return map;
        }
    }

    private List<Object> readArray() {
        List<Object> list = new ArrayList<Object>();
        expect('[');
        skipWs();
        if (peek() == ']') {
            pos++;
            return list;
        }
        while (true) {
            skipWs();
            list.add(readValue());
            skipWs();
            char c = peek();
            if (c == ',') {
                pos++;
                continue;
            }
            expect(']');
            return list;
        }
    }

    private String readString() {
        expect('"');
        StringBuilder b = new StringBuilder();
        while (true) {
            char c = src.charAt(pos++);
            if (c == '"') {
                return b.toString();
            }
            if (c != '\\') {
                b.append(c);
                continue;
            }
            char esc = src.charAt(pos++);
            switch (esc) {
                case 'n':
                    b.append('\n');
                    break;
                case 'r':
                    b.append('\r');
                    break;
                case 't':
                    b.append('\t');
                    break;
                case 'u':
                    b.append((char) Integer.parseInt(src.substring(pos, pos + 4), 16));
                    pos += 4;
                    break;
                default:
                    b.append(esc);
            }
        }
    }

    private Object readLiteral() {
        int start = pos;
        while (pos < src.length() && ",}] \t\r\n".indexOf(src.charAt(pos)) < 0) {
            pos++;
        }
        String literal = src.substring(start, pos).trim();
        if ("true".equals(literal)) {
            return Boolean.TRUE;
        }
        if ("false".equals(literal)) {
            return Boolean.FALSE;
        }
        if ("null".equals(literal)) {
            return null;
        }
        try {
            return Double.parseDouble(literal);
        } catch (NumberFormatException e) {
            return literal;
        }
    }

    private char peek() {
        skipWs();
        return pos < src.length() ? src.charAt(pos) : '\0';
    }

    private void expect(char c) {
        skipWs();
        if (pos >= src.length() || src.charAt(pos) != c) {
            throw new IllegalStateException("expected '" + c + "' at " + pos + " in " + src);
        }
        pos++;
    }

    private void skipWs() {
        while (pos < src.length() && Character.isWhitespace(src.charAt(pos))) {
            pos++;
        }
    }
}
