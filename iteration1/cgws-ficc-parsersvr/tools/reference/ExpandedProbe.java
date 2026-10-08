import cgws.ficc.parser.compare.Json;
import java.nio.file.*;
import java.nio.charset.StandardCharsets;
import java.lang.reflect.Method;
import java.util.*;

public final class ExpandedProbe {
    public static void main(String[] args) throws Exception {
        Class<?> type = Class.forName("com.sumscope.Parser");
        Object parser = type.getMethod("getInstance").invoke(null);
        Method set = type.getMethod("SetCodeName", String.class, String.class, String.class, String.class);
        set.invoke(parser, "start", "start", "", "");
        for (String[] row : new String[][] {{"240210.ib", "24国开10"}, {"240215.ib", "24国开15"}, {"240001", "24国债01"}, {"092318001.ib", "23农发01"}}) {
            set.invoke(parser, row[0], row[1], "", "");
        }
        set.invoke(parser, "end", "end", "", "");
        Map<String, String[]> cases = new LinkedHashMap<String, String[]>();
        cases.put("ParsePrice", new String[] {"1%", "8%", "9%", "0.5%", "2.5-2.4", "2%-3%", "2.5%-3.5%", "2.1234567%", " 2.5%", "2.5% ", "2.5"});
        cases.put("ParseTenor", new String[] {"1d", "1D+", "1天", "7天+", "2w", "2W", "2周", "3月", "12m", "100d", "999d+", "1-7d", "1d--7d", "1-3m", "30d-3m", "1d+2d", "1d-2d", "1d+2d+", "1d-+2d", "1d+2天", "1d到2d", "1.5y", "1-2y", "0d", "99d+", "1d - 2d"});
        cases.put("ParseAmount", new String[] {"10", "99", "100", "101", "150", "999", "9999", "10000", "10001", "99999", "100000", "100.0", "1000.", "1", "1.5", "1E", "1e", "50亿", "49亿", "1.2345亿", "2k", "2K", "12k", "2kw", "2KW", "2千万", "2BW", "2Bw", "2bw", "500-1000w", "500w-1000w", "1k-2kw", "1000w-1e", "1000w-1.5E", "1000万", "10万", "1万", "1000到2000w", "1亿+2千万"});
        cases.put("Bond", new String[] {"240210 2.5 1000 +1 bid", "240001 2.5 1000 bid", "092318001 2.5 1000 bid", "999999 2.5 1000 bid", "240210.ib 2.5 1000 t+0", "240210.ib 2.5 1000 t+2", "240210.ib 2.5 1000 +2", "240210.ib 2.5 1000 明天+0", "240210.ib 2.5 1亿", "240210.ib 2.5 1.5亿", "240210.ib 2.5 2k", "240210.ib 2.5 1000w", "240210.ib 2.5 1000万", "240210.ib 2 1000", "240210.ib 2.5 1000 20260930", "240210.ib 2.5 1000 买入", "240210.ib 2.5 1000 gvn", "240210.ib 2.5 1000 tkn", "240210.ib 2.5 1000 给我", "240210.ib 2.5 1000 给你"});
        List<Object> output = new ArrayList<Object>();
        for (Map.Entry<String, String[]> group : cases.entrySet()) {
            for (String input : group.getValue()) {
                Map<String, Object> row = new LinkedHashMap<String, Object>();
                row.put("method", group.getKey());
                row.put("input", input);
                Object expected;
                if ("Bond".equals(group.getKey())) {
                    String normalized = (String) type.getMethod("PreProcess", String.class).invoke(parser, input);
                    row.put("preprocessed", normalized);
                    expected = type.getMethod("ParseBond", String.class, String.class, String.class).invoke(parser, normalized, "", "");
                } else {
                    expected = type.getMethod(group.getKey(), String.class).invoke(parser, input);
                }
                row.put("expected", expected);
                output.add(row);
            }
        }
        Files.write(Paths.get(args[0]), Json.write(output).getBytes(StandardCharsets.UTF_8));
    }
}
