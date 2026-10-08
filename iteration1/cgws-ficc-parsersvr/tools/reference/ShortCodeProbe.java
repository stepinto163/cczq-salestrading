import cgws.ficc.parser.compare.Json;
import java.lang.reflect.*;
import java.nio.file.*;
import java.nio.charset.StandardCharsets;
import java.util.*;

/** Test-only constructor bypass: real constructors load databases and start schedulers. */
public final class ShortCodeProbe {
    public static void main(String[] args) throws Exception {
        Class<?> unsafeType = Class.forName("sun.misc.Unsafe");
        Field singleton = unsafeType.getDeclaredField("theUnsafe");
        singleton.setAccessible(true);
        Class<?> type = Class.forName("com.sumscope.BondQuoteParser");
        Object parser = unsafeType.getMethod("allocateInstance", Class.class).invoke(singleton.get(null), type);
        Field codes = type.getDeclaredField("setShortCode");
        codes.setAccessible(true);
        codes.set(parser, new HashSet<String>(Arrays.asList("260210.ib", "240210.ib", "240210.sh", "260301.sh", "260401.sz", "240001.ib", "240001.sh", "2400123.ib", "269901.ib")));
        Field years = type.getDeclaredField("lastYears");
        years.setAccessible(true);
        years.set(null, new String[] {"26", "25", "24", "23", "22", "21", "20", "19", "18", "17"});
        Method method = type.getDeclaredMethod("ReplaceShortCode", String.class);
        method.setAccessible(true);
        List<Object> output = new ArrayList<Object>();
        for (String text : new String[] {"210 2.5 1000", "301 2.5", "401 2.5", "101 2.5", "2401 2.5", "24123 2.5", "9901 2.5", "240210 2.5", "债券210 报价2.5", "210%", "210bp", "210.5", "210/215", "210+1", "210-215", "210 301", "债券210、210", "no quote", ""}) {
            Map<String, Object> row = new LinkedHashMap<String, Object>();
            row.put("input", text);
            row.put("expected", method.invoke(parser, text));
            output.add(row);
        }
        Files.write(Paths.get(args[0]), Json.write(output).getBytes(StandardCharsets.UTF_8));
    }
}
