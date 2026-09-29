import cgws.ficc.parser.compare.Json;
import java.lang.reflect.*;
import java.nio.file.*;
import java.nio.charset.StandardCharsets;
import java.util.*;

/** Isolated legacy message-level oracle; constructors are skipped to avoid DB and scheduler. */
public final class MessageProbe {
    public static void main(String[] args) throws Exception {
        Class<?> unsafeType = Class.forName("sun.misc.Unsafe");
        Field singleton = unsafeType.getDeclaredField("theUnsafe");
        singleton.setAccessible(true);
        Class<?> type = Class.forName("com.sumscope.BondQuoteParser");
        Object legacy = unsafeType.getMethod("allocateInstance", Class.class).invoke(singleton.get(null), type);
        for (Field field : type.getDeclaredFields()) {
            if (Modifier.isStatic(field.getModifiers())) {
                continue;
            }
            field.setAccessible(true);
            if (field.getType() == HashMap.class) {
                field.set(legacy, new HashMap<Object, Object>());
            } else if (field.getType() == HashSet.class) {
                field.set(legacy, new HashSet<Object>());
            }
        }
        Class<?> nativeType = Class.forName("com.sumscope.Parser");
        Object nativeParser = nativeType.getMethod("getInstance").invoke(null);
        set(type, legacy, "parser", nativeParser);
        set(type, null, "lastYears", new String[]{"26", "25", "24", "23", "22", "21", "20", "19", "18", "17"});
        Class<?> config = Class.forName("com.sumscope.FunctionConfig");
        set(config, null, "IsShortCode", true);
        set(config, null, "IsCleanData", true);
        Method register = nativeType.getMethod("SetCodeName", String.class, String.class, String.class, String.class);
        register.invoke(nativeParser, "start", "start", "", "");
        for (String[] bond : new String[][]{{"240210.ib", "24国开10"}, {"240215.ib", "24国开15"}, {"240001.ib", "24国债01"}}) {
            register.invoke(nativeParser, bond[0], bond[1], "", "");
            map(type, legacy, "mapIdName").put(bond[0], bond[1]);
            map(type, legacy, "mapIdKey").put(bond[0], "KEY-" + bond[0]);
            map(type, legacy, "mapIdYield").put(bond[0], "2.5000");
            map(type, legacy, "mapIdYieldFull").put(bond[0], "2.5000");
            for (String field : new String[]{"setIdBNC", "setShortCode"}) {
                Field member = type.getDeclaredField(field);
                member.setAccessible(true);
                ((Set) member.get(legacy)).add(bond[0]);
            }
        }
        register.invoke(nativeParser, "end", "end", "", "");
        Method parse = type.getMethod("parsingAll", String.class, String.class, boolean.class);
        List<Object> cases = new ArrayList<Object>();
        String[] inputs = {
            "240210 2.5 1000 +1 bid", "240210 2.5 1000\n240215 2.4 2000", "bid\n240210 2.5 1000\n240215 2.4 2000",
            "bid 2.6 3000 +0\n240210 2.5 1000\n240215 2.4 2000", "240210 2.5 1000\n240215 2.4 2000\nbid",
            "240210 2.5/2.4 1000", "240210 2.5 1000;240215 2.4 2000", "240210 2.5\n1000",
            "长城证券 田老师 13:30:00\n240210 2.5 1000 bid", "长城证券 田老师\n240210 2.5 1000 bid",
            "210 2.5 1000 bid", "210 2.5 1000 bid\n240215 2.4 2000 ofr", "240210 99.5 净价 1000 bid",
            "240210 101.5 全价 1000 bid", "240210 2.5 行权 1000 bid", "240210 中债+5bp 1000 bid",
            "240210 2.5 一千万 bid", "bid\n240210 2.5 1000\nofr\n240215 2.4 2000",
            "240210 2.5 1000 oco 打包 bid", "240210 2.5 1000 T+0 bid", "240210 2.5 1000 gvn",
            "240210 2 1000 bid", "240210\n2.5 1000 bid", "not a quote", "240210 2.5 1000\n\n240215 2.4 2000"
        };
        Method json = Class.forName("com.alibaba.fastjson.JSON").getMethod("toJSONString", Object.class);
        for (String text : inputs) {
            Map<String, Object> row = new LinkedHashMap<String, Object>();
            row.put("input", text);
            try {
                Object result = parse.invoke(legacy, text, "2026-09-29", true);
                row.put("expected", Json.parse((String) json.invoke(null, result)));
            } catch (InvocationTargetException error) {
                row.put("error", error.getCause().toString());
            }
            cases.add(row);
        }
        Files.write(Paths.get(args[0]), Json.write(cases).getBytes(StandardCharsets.UTF_8));
    }

    private static void set(Class<?> type, Object target, String name, Object value) throws Exception {
        Field field = type.getDeclaredField(name);
        field.setAccessible(true);
        field.set(target, value);
    }

    private static Map map(Class<?> type, Object target, String name) throws Exception {
        Field field = type.getDeclaredField(name);
        field.setAccessible(true);
        return (Map) field.get(target);
    }
}
