// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.enums;

import cn.hutool.core.util.StrUtil;

import java.beans.ConstructorProperties;
import java.util.HashMap;
import java.util.Map;

public enum SymbolTypeEnum {
    CREDIT("1", "BCO"),
    RATE("2", "BNC"),
    ABS("11", "ABS"),
    NCD("12", "NCD");

    private String key;
    private String value;
    static Map<String, SymbolTypeEnum> map;

    public static SymbolTypeEnum getSymbolTypeEnum(String value) {
        if (StrUtil.isBlank((CharSequence) value)) {
            return null;
        }
        return map.get(value);
    }

    public static SymbolTypeEnum getByValue(String value) {
        if (StrUtil.isBlank((CharSequence) value)) {
            return null;
        }
        for (SymbolTypeEnum symbolTypeEnum : SymbolTypeEnum.values()) {
            if (!symbolTypeEnum.value.equals(value)) continue;
            return symbolTypeEnum;
        }
        return null;
    }

    public static SymbolTypeEnum getByKey(String key) {
        for (SymbolTypeEnum symbolTypeEnum : SymbolTypeEnum.values()) {
            if (!symbolTypeEnum.key.equals(key)) continue;
            return symbolTypeEnum;
        }
        return null;
    }

    public static String getValueByKey(String key) {
        for (SymbolTypeEnum symbolTypeEnum : SymbolTypeEnum.values()) {
            if (!symbolTypeEnum.key.equals(key)) continue;
            return symbolTypeEnum.value;
        }
        return null;
    }

    public static String getKeyByValue(String value) {
        for (SymbolTypeEnum symbolTypeEnum : SymbolTypeEnum.values()) {
            if (!symbolTypeEnum.value.equals(value)) continue;
            return symbolTypeEnum.key;
        }
        return null;
    }

    public String getKey() {
        return this.key;
    }

    public String getValue() {
        return this.value;
    }

    @ConstructorProperties(value = {"key", "value"})
    private SymbolTypeEnum(String key, String value) {
        this.key = key;
        this.value = value;
    }

    static {
        map = new HashMap<String, SymbolTypeEnum>();
        for (SymbolTypeEnum symbolTypeEnum : SymbolTypeEnum.values()) {
            map.put(symbolTypeEnum.name(), symbolTypeEnum);
        }
    }
}
