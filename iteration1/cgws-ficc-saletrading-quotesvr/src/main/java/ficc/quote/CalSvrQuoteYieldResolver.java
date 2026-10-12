package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.QuoteRecord;
import ficc.common.JsonUtils;
import ficc.common.client.StcTransport;
import org.apache.log4j.Logger;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.Map;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.RejectedExecutionException;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;
import java.util.function.Consumer;

/** CalSvr 的 DPS 适配。网络请求只在有界后台队列执行，不占用 QuoteSvr 查询/写入锁。 */
final class CalSvrQuoteYieldResolver implements QuoteYieldResolver, AutoCloseable {
    private static final Logger LOGGER = Logger.getLogger(CalSvrQuoteYieldResolver.class);
    private final StcTransport transport;
    private final Consumer<String> changed;
    private final ThreadPoolExecutor worker;
    private final Map<String, Calculation> cache = new ConcurrentHashMap<>();
    private volatile boolean closed;

    private static final class Calculation {
        final String key;
        final String securityId;
        volatile Result result = new Result(null, "CALCULATION_PENDING");

        Calculation(String key, String securityId) {
            this.key = key;
            this.securityId = securityId;
        }
    }

    CalSvrQuoteYieldResolver(StcTransport transport, int queueCapacity, Consumer<String> changed) {
        this.transport = transport;
        this.changed = changed;
        worker = new ThreadPoolExecutor(1, 1, 0, TimeUnit.MILLISECONDS,
                new ArrayBlockingQueue<>(queueCapacity), task -> {
                    Thread thread = new Thread(task, "quote-calculation");
                    thread.setDaemon(true);
                    return thread;
                });
        transport.connectionChanged(this::invalidate);
        transport.subscribe("stc.cal.clear", ignored -> invalidate());
    }

    @Override
    public Result resolve(QuoteRecord quote, String yieldType, String referenceVersion) {
        if (closed) {
            return new Result(null, "CALCULATOR_STOPPED");
        }
        final JSONObject request;
        try {
            request = request(quote);
        } catch (RuntimeException error) {
            LOGGER.warn("PRICE_CALCULATION state=INVALID_INPUT quoteId=" + quote.quoteId
                    + " securityId=" + quote.securityId + " reason=" + error.getMessage());
            return new Result(null, "CALCULATION_INVALID_INPUT");
        }
        String key = JsonUtils.canonicalHash(JsonUtils.object("request", request,
                "yieldType", yieldType, "referenceVersion", referenceVersion));
        Calculation previous = cache.get(quote.quoteId);
        if (previous != null && previous.key.equals(key)) {
            return previous.result;
        }
        Calculation calculation = new Calculation(key, quote.securityId);
        cache.put(quote.quoteId, calculation);
        if (!transport.online()) {
            calculation.result = new Result(null, "CALCULATOR_OFFLINE");
            LOGGER.warn("PRICE_CALCULATION state=OFFLINE quoteId=" + quote.quoteId
                    + " securityId=" + quote.securityId);
            return calculation.result;
        }
        try {
            String quoteId = quote.quoteId;
            worker.execute(() -> calculate(quoteId, calculation, request, yieldType));
        } catch (RejectedExecutionException full) {
            cache.remove(quote.quoteId, calculation);
            return new Result(null, "CALCULATOR_BUSY");
        }
        return calculation.result;
    }

    private void calculate(String quoteId, Calculation calculation, JSONObject request, String yieldType) {
        if (closed || cache.get(quoteId) != calculation) {
            return;
        }
        try {
            JSONObject reply = transport.request("stc.broker.calbond", "", request);
            if (!"0000".equals(reply.getString("Code"))) {
                throw new IllegalStateException("CalSvr Code=" + reply.getString("Code")
                        + " Msg=" + reply.getString("Msg"));
            }
            JSONObject data = reply.getJSONObject("Data");
            BigDecimal yield = data == null ? null
                    : data.getBigDecimal("2".equals(yieldType) ? "yieldToExecution" : "yield");
            if (yield == null) {
                throw new IllegalStateException("CalSvr reply has no requested yield, yieldType=" + yieldType);
            }
            // 沿用 STC 调用方的单位转换，不自行决定额外四舍五入规则。
            calculation.result = new Result(yield.movePointRight(2).stripTrailingZeros().toPlainString(), null);
            LOGGER.info("PRICE_CALCULATION state=READY quoteId=" + quoteId
                    + " securityId=" + calculation.securityId + " yieldType=" + yieldType);
        } catch (Exception error) {
            calculation.result = new Result(null, "CALCULATION_FAILED");
            LOGGER.warn("PRICE_CALCULATION state=FAILED quoteId=" + quoteId
                    + " securityId=" + calculation.securityId + " request=" + request, error);
        }
        if (!closed && cache.get(quoteId) == calculation) {
            try {
                changed.accept(calculation.securityId);
            } catch (RuntimeException error) {
                LOGGER.error("PRICE_CALCULATION publication failed securityId=" + calculation.securityId, error);
            }
        }
    }

    /** 显式打开新行情视图时重试失败项；无定时无限重试，成功值与在途任务保持复用。 */
    void retryFailures() {
        cache.entrySet().removeIf(entry -> entry.getValue().result.price == null
                && !"CALCULATION_PENDING".equals(entry.getValue().result.code));
    }

    private void invalidate() {
        java.util.Set<String> securities = new java.util.HashSet<>();
        cache.values().forEach(value -> securities.add(value.securityId));
        cache.clear();
        if (!closed) {
            securities.forEach(changed);
        }
    }

    @Override
    public void forget(String quoteId) {
        cache.remove(quoteId);
    }

    /** 已与 QA CalHandler 的 CalReq / SettleType 字节码核对，日期交给 CalSvr 交易日历处理。 */
    static JSONObject request(QuoteRecord quote) {
        String calType;
        BigDecimal value = new BigDecimal(quote.price);
        if ("1".equals(quote.quoteType)) {
            if (!"1".equals(quote.yieldType) && !"2".equals(quote.yieldType)) {
                throw new IllegalArgumentException("Unknown yieldType");
            }
            calType = "2".equals(quote.yieldType) ? "7" : "4";
            value = value.movePointLeft(2);
        } else if ("2".equals(quote.quoteType) || "3".equals(quote.quoteType)) {
            calType = quote.quoteType;
        } else {
            throw new IllegalArgumentException("Unknown quoteType");
        }
        String tradeDate = LocalDate.parse(quote.tradeDate).format(DateTimeFormatter.BASIC_ISO_DATE);
        if (!"1".equals(quote.settlType) && !"2".equals(quote.settlType)) {
            throw new IllegalArgumentException("Unsupported settlement for conversion: " + quote.settlType);
        }
        return JsonUtils.object("SecurityID", quote.securityId, "Value", value, "CalType", calType,
                "TradeDate", tradeDate, "SettlType", quote.settlType, "Scale", 4);
    }

    @Override
    public void close() {
        closed = true;
        cache.clear();
        worker.shutdownNow();
    }
}
