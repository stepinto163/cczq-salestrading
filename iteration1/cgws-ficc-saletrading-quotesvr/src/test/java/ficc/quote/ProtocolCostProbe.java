package ficc.quote;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.api.*;
import java.util.function.Predicate;
import java.util.stream.Collectors;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import java.lang.management.ManagementFactory;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.*;

/** Explicitly authorized, bounded microbenchmark. Run with an external 15-second watchdog. */
public final class ProtocolCostProbe {
    private static volatile int consumed;

    public static void main(String[] args) throws Exception {
        boolean typed = "typed".equals(args[0]);
        List<JSONObject> quotes = new ArrayList<>();
        for (int index = 0; index < 1000; index++) {
            quotes.add(JsonUtils.object("quoteId", "Q" + index, "clientQuoteId", "C" + index,
                    "securityId", "250001.IB", "ownerUserId", "U-001", "revision", "9007199254740993",
                    "status", "ACTIVE", "side", "1", "quoteType", "1", "yieldType", "1",
                    "price", index % 2 == 0 ? "2.5000" : "2.7000", "quantity", "1000",
                    "quantityUnit", "WAN", "currency", "CNY", "settlType", "2", "tradeDate", "2026-09-29",
                    "institutionId", null, "institutionName", "Institution", "contactId", null,
                    "contactName", "Contact", "urgent", false, "packing", false,
                    "updateTime", "2026-09-29T09:00:00+08:00", "source", JsonUtils.object("type", "MANUAL")));
        }
        LegacyQuoteMatcher legacy = new LegacyQuoteMatcher(new ReferenceData(), new LinkedHashMap<>());
        QuoteMatcher matcher = new QuoteMatcher(new ReferenceData(), new LinkedHashMap<>());
        List<QuoteRecord> typedQuotes = quotes.stream().map(row -> ProtocolCodec.decode(row, QuoteRecord.class))
                .collect(Collectors.toList());
        JSONObject filter = JsonUtils.object("conditions", JsonUtils.array(
                JsonUtils.object("field", "price", "operator", "BETWEEN", "value", JsonUtils.array("2.4", "2.6"))));
        String query = JsonUtils.toJson(JsonUtils.object("requestId", "R", "viewId", "V", "pageSize", 50, "filter", filter));
        JSONObject response = JsonUtils.object("Code", "0000", "requestId", "R", "Data",
                JsonUtils.object("streamId", "S", "sequence", "1", "view",
                        JsonUtils.object("viewId", "V", "totalCount", 500, "ids", new JSONArray()),
                        "quotes", new JSONArray(quotes.subList(0, 50))));
        Map<String, Runnable> operations = new LinkedHashMap<>();
        operations.put("requestDecode", () -> {
            JSONObject request = JsonUtils.parseObject(query);
            if (typed) {
                request = ProtocolBoundary.request("ficc.quote.view.open", request);
            }
            consumed = request.size();
        });
        operations.put("filter1000", () -> {
            int count = 0;
            if (typed) {
                Predicate<QuoteRecord> matches = matcher.prepare("U-001", ProtocolCodec.decode(filter, Filter.class));
                for (QuoteRecord quote : typedQuotes) {
                    if (matches.test(quote)) {
                        count++;
                    }
                }
            } else {
                for (JSONObject quote : quotes) {
                    if (legacy.matches("U-001", quote, filter)) {
                        count++;
                    }
                }
            }
            if (count != 500) {
                throw new AssertionError("Unexpected matched count: " + count);
            }
            consumed = count;
        });
        operations.put("responseEncode50", () -> {
            JSONObject value = typed ? ProtocolBoundary.reply("ficc.quote.view.open", response) : response;
            consumed = JsonUtils.toJson(value).length();
        });
        operations.put("combined", () -> {
            for (Map.Entry<String, Runnable> operation : operations.entrySet()) {
                if (!"combined".equals(operation.getKey())) {
                    operation.getValue().run();
                }
            }
        });
        JSONObject results = new JSONObject();
        for (Map.Entry<String, Runnable> operation : operations.entrySet()) {
            results.put(operation.getKey(), measure(operation.getValue()));
        }
        JSONObject report = JsonUtils.object("mode", args[0], "quotes", 1000, "pageRows", 50,
                "warmup", 8, "samples", 20, "results", results,
                "scope", "in-memory Java only; no DB/GW/client UI; baseline bypasses new DTO boundaries");
        Files.write(Paths.get(args[1]), JsonUtils.toJson(report).getBytes(StandardCharsets.UTF_8));
        System.out.println(JsonUtils.toJson(report));
    }

    private static JSONObject measure(Runnable action) {
        for (int index = 0; index < 8; index++) {
            action.run();
        }
        com.sun.management.ThreadMXBean threads = (com.sun.management.ThreadMXBean) ManagementFactory.getThreadMXBean();
        threads.setThreadAllocatedMemoryEnabled(true);
        long thread = Thread.currentThread().getId();
        long allocated = threads.getThreadAllocatedBytes(thread);
        double[] milliseconds = new double[20];
        for (int index = 0; index < milliseconds.length; index++) {
            long started = System.nanoTime();
            action.run();
            milliseconds[index] = (System.nanoTime() - started) / 1_000_000.0;
        }
        long bytes = (threads.getThreadAllocatedBytes(thread) - allocated) / milliseconds.length;
        Arrays.sort(milliseconds);
        return JsonUtils.object("p50Millis", milliseconds[9], "p95Millis", milliseconds[18],
                "allocatedBytesPerOperation", bytes);
    }
}
