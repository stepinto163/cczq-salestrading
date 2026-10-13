package ficc.common.client;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ComponentLog;
import ficc.common.JsonUtils;

import java.time.LocalDate;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;
import java.util.Properties;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * BaseSvr 的结构化中证接口只有分页读取；在后台构建紧凑索引，查询路径不发分页 RPC。
 * 完整加载后原子发布，失败不发布半份数据。重连、清库通知或跨日触发重新加载。
 */
final class StcCsiValuationCache implements AutoCloseable {
    static final String TOPIC = "stc.query.page.csiValuation";
    private final StcTransport transport;
    private final Properties configuration;
    private final int pageSize;
    private final ExecutorService worker = Executors.newSingleThreadExecutor(task -> {
        Thread thread = new Thread(task, "stc-csi-valuations");
        thread.setDaemon(true);
        return thread;
    });
    private volatile Map<String, JSONObject> records = Collections.emptyMap();
    private volatile String status = "LOADING";
    private volatile long revision;
    private LocalDate requestedDate;
    private long generation;
    private boolean running;
    private boolean closed;

    StcCsiValuationCache(StcTransport transport, Properties configuration) {
        this(transport, configuration, 1000);
    }

    StcCsiValuationCache(StcTransport transport, Properties configuration, int pageSize) {
        this.transport = transport;
        this.configuration = configuration;
        if (pageSize < 1 || pageSize > 1000) {
            throw new IllegalArgumentException("Invalid CSI page size");
        }
        this.pageSize = pageSize;
    }

    synchronized void refresh() {
        if (closed) {
            return;
        }
        generation++;
        requestedDate = LocalDate.now();
        status = "LOADING";
        records = Collections.emptyMap();
        revision++;
        if (!running && transport.online()) {
            running = true;
            worker.execute(this::loadPending);
        }
    }

    private void loadPending() {
        while (true) {
            long loadingGeneration;
            synchronized (this) {
                if (closed || !transport.online()) {
                    running = false;
                    return;
                }
                loadingGeneration = generation;
            }
            Map<String, JSONObject> loaded = null;
            try {
                loaded = ComponentLog.initialize(configuration, "CSI_VALUATIONS", "stc",
                        "StcCsiValuationCache", "BaseSvr/" + TOPIC, this::readPages);
            } catch (Exception error) {
                // ComponentLog 已记录 ERROR/堆栈。异常状态明确返回给基础资料，不能冒充“没有估值”。
            }
            synchronized (this) {
                if (closed) {
                    running = false;
                    return;
                }
                if (generation != loadingGeneration) {
                    continue;
                }
                records = loaded == null ? Collections.emptyMap() : loaded;
                status = loaded == null ? "FAILED" : "READY";
                revision++;
                running = false;
                return;
            }
        }
    }

    private Map<String, JSONObject> readPages() {
        Map<String, JSONObject> loaded = new HashMap<>();
        java.util.Set<String> seen = new java.util.HashSet<>();
        for (int page = 1; page <= 500000 / pageSize + 1; page++) {
            if (Thread.currentThread().isInterrupted()) {
                throw new IllegalStateException("CSI valuation loading interrupted");
            }
            JSONObject reply = transport.request(TOPIC, null,
                    JsonUtils.object("PageNum", page, "PageSize", pageSize));
            JSONArray rows = reply == null ? null : reply.getJSONArray("Datas");
            if (reply == null || !"0000".equals(reply.getString("Code"))
                    || rows == null || rows.size() > pageSize) {
                throw new IllegalStateException("Invalid BaseSvr CSI valuation page: " + page);
            }
            for (int index = 0; index < rows.size(); index++) {
                JSONObject raw = rows.getJSONObject(index);
                String identity = JsonUtils.canonicalHash(raw);
                if (!seen.add(identity)) {
                    throw new IllegalStateException("Duplicate BaseSvr CSI valuation row on page "
                            + page + "; check stable ordering in BaseSvr CSI pagination SQL");
                }
                JSONObject value = StcBaseMapper.csiValuation(raw);
                put(loaded, raw.getString("interbank_code"), ".IB", value);
                put(loaded, raw.getString("shh_code"), ".SH", value);
                put(loaded, raw.getString("shz_code"), ".SZ", value);
            }
            if (rows.size() < pageSize) {
                return Collections.unmodifiableMap(loaded);
            }
        }
        throw new IllegalStateException("BaseSvr CSI valuation paging exceeded 500000 records");
    }

    private static void put(Map<String, JSONObject> target, String code, String suffix, JSONObject value) {
        if (code == null || code.trim().isEmpty()) {
            return;
        }
        String securityId = code.trim().endsWith(suffix) ? code.trim() : code.trim() + suffix;
        JSONObject previous = target.get(securityId);
        String date = value.getString("csiValuationDate");
        String previousDate = previous == null ? null : previous.getString("csiValuationDate");
        if (previous == null || (date != null && (previousDate == null || date.compareTo(previousDate) >= 0))) {
            target.put(securityId, value);
        }
    }

    synchronized JSONObject get(String securityId) {
        if (!LocalDate.now().equals(requestedDate)) {
            refresh();
        }
        JSONObject value = records.get(securityId);
        JSONObject result = value == null ? new JSONObject() : new JSONObject(value);
        result.put("csiValuationStatus", "READY".equals(status) && value == null ? "MISSING" : status);
        return result;
    }

    synchronized long revision() {
        if (!LocalDate.now().equals(requestedDate)) {
            refresh();
        }
        return revision;
    }

    @Override
    public synchronized void close() {
        closed = true;
        worker.shutdownNow();
    }
}
