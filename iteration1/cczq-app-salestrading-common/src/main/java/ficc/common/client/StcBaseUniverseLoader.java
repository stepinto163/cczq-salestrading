package ficc.common.client;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import java.time.Instant;
import java.time.LocalDate;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.Properties;
import java.util.function.Consumer;

/** Bulk reference loading through existing BaseSvr page handlers and STC transport. */
final class StcBaseUniverseLoader {
    private final StcTransport transport;
    private final int pageSize;
    private final int maxRecords;
    private final int loadTimeoutSeconds;

    record Universe(Map<String, JSONObject> bonds, Map<String, JSONObject> issuers,
            Map<String, JSONObject> valuations) {}

    StcBaseUniverseLoader(StcTransport transport, Properties configuration) {
        this.transport = transport;
        pageSize = integer(configuration, "base.pageSize", 500, 1, 5000);
        maxRecords = integer(configuration, "base.maxRecordsPerTable", 500000, 1, 2000000);
        loadTimeoutSeconds = integer(configuration, "base.loadTimeoutSeconds", 300, 1, 3600);
    }

    Universe load(Runnable checkGeneration) {
        long deadline = System.nanoTime() + loadTimeoutSeconds * 1_000_000_000L;
        Map<String, JSONObject> bonds = new LinkedHashMap<>();
        Map<String, JSONObject> issuers = new LinkedHashMap<>();
        Map<String, JSONObject> valuations = new LinkedHashMap<>();
        // Include matured bonds: historical quotes also need current reference attributes.
        pages("stc.query.page.securityInfo", deadline, checkGeneration, row -> {
            String securityId = StcBaseMapper.securityId(row);
            if (securityId != null) {
                unique(bonds, securityId, row, "securityInfo");
            }
        });
        pages("stc.query.page.issuerInfo", deadline, checkGeneration, row -> {
            String issuerId = StcBaseMapper.text(row, "institution_code");
            if (issuerId == null) {
                throw new IllegalStateException("BaseSvr issuerInfo is missing institution_code");
            }
            unique(issuers, issuerId, row, "issuerInfo");
        });
        pages("stc.query.page.cdcValuation", deadline, checkGeneration, row -> {
            String securityId = StcBaseMapper.securityId(row);
            if (securityId == null || !bonds.containsKey(securityId)) {
                return;
            }
            JSONObject valuation = cdcValuation(row);
            JSONObject previous = valuations.get(securityId);
            String date = valuation.getString("valuation_date");
            if (previous == null || date.compareTo(previous.getString("valuation_date")) > 0) {
                valuations.put(securityId, valuation);
            } else if (date.equals(previous.getString("valuation_date")) && !previous.equals(valuation)) {
                throw new IllegalStateException("Conflicting BaseSvr CDC valuations: " + securityId + " / " + date);
            }
        });
        return new Universe(bonds, issuers, valuations);
    }

    private void pages(String topic, long deadline, Runnable checkGeneration, Consumer<JSONObject> receive) {
        int received = 0;
        Integer expectedTotal = null;
        String previousPage = null;
        for (int page = 1; ; page++) {
            checkLoad(deadline, checkGeneration);
            JSONObject reply = transport.request(topic, null,
                    JsonUtils.object("pageNum", page, "pageSize", pageSize, "onlyValid", false));
            checkLoad(deadline, checkGeneration);
            if (!"0000".equals(reply.getString("Code"))) {
                throw new IllegalStateException("BaseSvr " + topic + " failed: " + reply.getString("Code"));
            }
            JSONArray rows = reply.getJSONArray("Datas");
            if (rows == null || rows.size() > pageSize) {
                throw new IllegalStateException("Invalid BaseSvr page: " + topic + " / " + page);
            }
            if (reply.containsKey("Total")) {
                Integer total = reply.getInteger("Total");
                if (total == null || total < 0 || total > maxRecords
                        || (expectedTotal != null && !expectedTotal.equals(total))) {
                    throw new IllegalStateException("BaseSvr Total changed or exceeds limit: " + topic);
                }
                expectedTotal = total;
            }
            received += rows.size();
            if (received > maxRecords || (expectedTotal != null && received > expectedTotal)) {
                throw new IllegalStateException("BaseSvr row count exceeds limit: " + topic);
            }
            String currentPage = rows.toJSONString();
            if (!rows.isEmpty() && currentPage.equals(previousPage)) {
                throw new IllegalStateException("BaseSvr repeated a page: " + topic + " / " + page);
            }
            previousPage = currentPage;
            for (int index = 0; index < rows.size(); index++) {
                JSONObject row = rows.getJSONObject(index);
                if (row == null) {
                    throw new IllegalStateException("BaseSvr returned a null row: " + topic);
                }
                receive.accept(row);
            }
            if (expectedTotal != null && received == expectedTotal) {
                return;
            }
            if (rows.size() < pageSize) {
                if (expectedTotal != null) {
                    throw new IllegalStateException("BaseSvr returned an incomplete table: " + topic);
                }
                return;
            }
        }
    }

    private void checkLoad(long deadline, Runnable checkGeneration) {
        checkGeneration.run();
        if (Thread.currentThread().isInterrupted() || System.nanoTime() >= deadline || !transport.online()) {
            throw new IllegalStateException("BaseSvr bulk load interrupted, offline or timed out");
        }
    }

    private static void unique(Map<String, JSONObject> rows, String key, JSONObject row, String table) {
        if (rows.putIfAbsent(key, row) != null) {
            throw new IllegalStateException("Duplicate BaseSvr " + table + " key: " + key);
        }
    }

    private static JSONObject cdcValuation(JSONObject row) {
        String value = StcBaseMapper.text(row, "valuate_date");
        if (value == null) {
            throw new IllegalStateException("BaseSvr CDC valuation has no valuate_date");
        }
        String date;
        if (value.matches("[0-9]{8}")) {
            date = LocalDate.parse(value, DateTimeFormatter.BASIC_ISO_DATE).toString();
        } else if (value.matches("[0-9]{12,13}")) {
            date = Instant.ofEpochMilli(Long.parseLong(value)).atZone(ZoneId.of("Asia/Shanghai"))
                    .toLocalDate().toString();
        } else {
            date = LocalDate.parse(value.substring(0, 10)).toString();
        }
        // CDC's page model has no explicit exercise/maturity basis; do not invent one.
        return JsonUtils.object("valuation_date", date,
                "valuation_yield", StcBaseMapper.text(row, "val_yield"),
                "valuation_net_price", StcBaseMapper.text(row, "val_clean_price"),
                "remaining_year", StcBaseMapper.text(row, "remaining_year"));
    }

    private static int integer(Properties configuration, String key, int fallback, int minimum, int maximum) {
        int value = Integer.parseInt(configuration.getProperty(key, Integer.toString(fallback)));
        if (value < minimum || value > maximum) {
            throw new IllegalArgumentException(key + " must be between " + minimum + " and " + maximum);
        }
        return value;
    }
}
