import cgws.ficc.parser.compare.Json;
import java.io.*;
import java.lang.reflect.Method;
import java.util.*;

/** Isolated synthetic functional reference capture. Does not construct BondQuoteParser or load a database. */
public final class NativeHelperProbe {
    public static void main(String[] args) throws Exception {
        Class<?> parserClass = Class.forName("com.sumscope.Parser");
        Object parser = parserClass.getMethod("getInstance").invoke(null);
        Method set = parserClass.getMethod("SetCodeName", String.class, String.class, String.class, String.class);
        set.invoke(parser, "start", "start", "", "");
        set.invoke(parser, "240210.ib", "24国开10", "", "");
        set.invoke(parser, "240215.ib", "24国开15", "", "");
        set.invoke(parser, "240210", "24国开10", "", "");
        set.invoke(parser, "end", "end", "", "");
        Map<String, String[]> inputs = new LinkedHashMap<String, String[]>();
        inputs.put("ParsePrice", new String[]{"2.5", "2.5000", "99.50", "100", "0", "-1.2", "2.5/2.4", "2.5-2.4", "2.5bp", "2.5%", "净价99.5", "收益率2.5", "bid 2.5", "", "abc"});
        inputs.put("ParseTenor", new String[]{"1Y", "1y", "3M", "3m", "30D", "30d", "1.5Y", "1年", "3个月", "隔夜", "1+1Y", "1Y+2Y", "7.60Y", "1", "", "abc"});
        inputs.put("ParseAmount", new String[]{"1000", "1000万", "1000w", "1亿", "1e", "1.5亿", "5000/1000", "1000-2000", "1000整", "一千万", "0", "-1000", "1,000", "", "abc"});
        inputs.put("ReplaceNumber", new String[]{"三千", "一千万", "一点五", "一亿两千万", "一百零五", "二〇二六", "两千", "1000万", "报价三千", ""});
        List<Object> results = new ArrayList<Object>();
        for (Map.Entry<String, String[]> group : inputs.entrySet()) {
            Method method = parserClass.getMethod(group.getKey(), String.class);
            for (String input : group.getValue()) {
                Map<String, Object> entry = new LinkedHashMap<String, Object>();
                entry.put("method", group.getKey());
                entry.put("input", input);
                entry.put("expected", method.invoke(parser, input));
                results.add(entry);
            }
        }
        Method parse = parserClass.getMethod("ParseBond", String.class, String.class, String.class);
        for (String input : new String[]{"240210 2.5 1000 +1 bid", "240210.ib 2.5 1000 +1 bid", "24国开10 2.5 1000 +1 bid", "240210.IB 2.5 1000", "240210.ib 2.5 1000 T+0", "240210.ib 2.5 1000 20260930", "240210.ib 2.5 1亿", "240210.ib 99.5 净价 1000", "240210.ib 2.5 行权 1000", "240210.ib 2.5 1000\n240215.ib 2.4 2000"}) {
            Map<String, Object> entry = new LinkedHashMap<String, Object>();
            entry.put("method", "ParseBond");
            entry.put("input", input);
            entry.put("expected", parse.invoke(parser, input, "", ""));
            results.add(entry);
        }
        try (Writer writer = new OutputStreamWriter(new FileOutputStream(args[0]), "UTF-8")) {
            writer.write(Json.write(results));
        }
    }
}
