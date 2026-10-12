package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.Filter;
import ficc.api.MarketData;
import ficc.api.MarketEntry;
import ficc.api.PriceBook;
import ficc.api.PriceCalculationIssue;
import ficc.api.ProtocolCodec;
import ficc.api.PriceQueryRequest;
import ficc.api.QuoteRecord;
import ficc.api.QuoteReference;
import ficc.common.ApiException;
import ficc.common.FilterEngine;
import ficc.common.ReferenceData;
import ficc.common.RequestValidator;
import ficc.common.client.AccessPolicy;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Set;
import java.util.function.Predicate;
import java.util.stream.Collectors;

/**
 * 查询与订阅共用的价格簿投影，由 QuoteSvr 串行调度路径持有，不是独立存储或线程安全容器。
 * 按债券维护报价索引，先按用户可见性和条件筛选 ACTIVE 报价，再生成 FULL/BEST/DEPTH 价格档。
 * 待确认提案不进入价格簿；同一债券在不同用户视角下可以有不同的最优价与可见报价。
 */
final class PriceBookService {
    private static final DateTimeFormatter ENTRY_DATE = DateTimeFormatter.ofPattern("yyyyMMdd");
    private static final DateTimeFormatter ENTRY_TIME = DateTimeFormatter.ofPattern("HH:mm:ss.SSS");

    private final ReferenceData references;
    private final QuoteMatcher matcher;
    private final AccessPolicy access;
    private final Map<String, Map<String, QuoteRecord>> quotesBySecurity = new HashMap<>();
    private final Map<String, Long> revisions = new HashMap<>();
    QuoteYieldResolver yieldResolver = (quote, yieldType, version) ->
            new QuoteYieldResolver.Result(null, "CALCULATOR_NOT_CONFIGURED");

    void calculationChanged(String securityId) {
        revisions.merge(securityId, 1L, Long::sum);
    }

    PriceBookService(ReferenceData references, QuoteMatcher matcher, AccessPolicy access,
            Collection<QuoteRecord> initialQuotes) {
        this.references = references;
        this.matcher = matcher;
        this.access = access;
        for (QuoteRecord quote : initialQuotes) {
            indexQuote(quote, null);
        }
    }

    QuoteRecord previousQuote(QuoteRecord quote, String previousSecurity) {
        String security = previousSecurity == null ? quote.securityId : previousSecurity;
        Map<String, QuoteRecord> previous = quotesBySecurity.get(security);
        return previous == null ? null : previous.get(quote.quoteId);
    }

    /**
     * 更新索引并返回需要重建价格簿的债券；修改债券代码时新旧两只债券都需处理。
     * 从 ACTIVE 撤出也会影响价格；纯待确认记录更新只更新索引，不增加价格簿版本。
     */
    Set<String> changed(QuoteRecord quote, String previousSecurity) {
        QuoteRecord previous = previousQuote(quote, previousSecurity);
        boolean affectsPrice = "ACTIVE".equals(quote.status)
                || (previous != null && "ACTIVE".equals(previous.status));
        indexQuote(quote, previousSecurity);
        Set<String> affectedSecurities = new LinkedHashSet<>();
        if (affectsPrice && quote.securityId != null) {
            affectedSecurities.add(quote.securityId);
        }
        if (affectsPrice && previousSecurity != null) {
            affectedSecurities.add(previousSecurity);
        }
        for (String security : affectedSecurities) {
            revisions.merge(security, 1L, Long::sum);
        }
        return affectedSecurities;
    }

    private void indexQuote(QuoteRecord quote, String previousSecurity) {
        if (!"ACTIVE".equals(quote.status)) {
            yieldResolver.forget(quote.quoteId);
        }
        if (previousSecurity != null && !Objects.equals(previousSecurity, quote.securityId)) {
            Map<String, QuoteRecord> previous = quotesBySecurity.get(previousSecurity);
            if (previous != null) {
                previous.remove(quote.quoteId);
                if (previous.isEmpty()) {
                    quotesBySecurity.remove(previousSecurity);
                }
            }
        }
        if (quote.securityId != null) {
            quotesBySecurity.computeIfAbsent(quote.securityId, ignored -> new LinkedHashMap<>())
                    .put(quote.quoteId, quote);
        }
    }

    private Collection<QuoteRecord> quotesForSecurity(String security) {
        Map<String, QuoteRecord> quotes = quotesBySecurity.get(security);
        return quotes == null ? Collections.emptyList() : quotes.values();
    }

    List<PriceBook> query(String user, PriceQueryRequest request) {
        if (request.securityIds == null || request.securityIds.size() > 100) {
            throw ApiException.forField("1008", "RESOURCE_LIMIT", "securityIds required, max 100", null);
        }
        validateMode(request.mode, request.depth);
        FilterEngine.validate(request.filter == null ? null : request.filter.toJson());
        List<PriceBook> result = new ArrayList<>();
        for (String security : request.securityIds) {
            references.require(security);
            result.add(book(user, security, request.filter, request.mode,
                    request.depth == null ? 0 : request.depth));
        }
        return result;
    }

    static String wireMode(JSONObject request) {
        String mode = RequestValidator.requireString(request, "mode");
        validateMode(mode, "DEPTH".equals(mode) ? request.getIntValue("depth") : null);
        if (!"DEPTH".equals(mode) && request.containsKey("depth")) {
            throw ApiException.forField(
                    "1001", "VALIDATION_FAILED", "depth only allowed for DEPTH", "depth");
        }
        return mode;
    }

    static void validateMode(String mode, Integer depth) {
        if (!Arrays.asList("FULL", "BEST", "DEPTH").contains(mode)) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid mode", "mode");
        }
        if ("DEPTH".equals(mode) && (depth == null || depth < 1 || depth > 1000)) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Positive depth required", "depth");
        }
        if (!"DEPTH".equals(mode) && depth != null) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "depth only allowed for DEPTH", "depth");
        }
    }

    PriceBook book(String user, String security, Filter filter, String mode, int depth) {
        long started = QuotePerformanceTrace.searchStart();
        try {
            return buildBook(user, security, filter, mode, depth);
        } finally {
            QuotePerformanceTrace.bookFinished(started);
        }
    }

    private PriceBook buildBook(String user, String security, Filter filter, String mode, int depth) {
        Predicate<QuoteRecord> matches = matcher.prepare(user, filter);
        // Null is reserved for the already-authorized internal feed.
        List<QuoteRecord> quotes = quotesForSecurity(security).stream()
                .filter(quote -> security.equals(quote.securityId) && "ACTIVE".equals(quote.status)
                        && (user == null || access.canViewQuote(user, quote.ownerUserId,
                                quote.source == null ? null : quote.source.type, quote.status))
                        && matches.test(quote))
                .collect(Collectors.toList());
        // 价格排序只使用同一收益率口径。单条换算失败不能使整个行情或内部快照失败。
        boolean exercise = !quotes.isEmpty()
                && references.require(security).getBooleanValue("hasExerciseOption");
        List<QuoteRecord> comparableQuotes = new ArrayList<>();
        List<PriceCalculationIssue> issues = new ArrayList<>();
        for (QuoteRecord quote : quotes) {
            if (!"1".equals(quote.quoteType)
                    || !Objects.equals(quote.yieldType, exercise ? "2" : "1")) {
                QuoteYieldResolver.Result calculation = yieldResolver.resolve(quote,
                        exercise ? "2" : "1", references.version());
                if (calculation.price == null) {
                    PriceCalculationIssue issue = new PriceCalculationIssue();
                    issue.quoteId = quote.quoteId;
                    issue.code = calculation.code;
                    issues.add(issue);
                    continue;
                }
                // 只修改行情投影副本，保留报价原始价格、版本、类型和清算信息。
                QuoteRecord projection = ProtocolCodec.decode(quote.toJson(), QuoteRecord.class);
                projection.price = calculation.price;
                projection.quoteType = "1";
                projection.yieldType = exercise ? "2" : "1";
                comparableQuotes.add(projection);
            } else {
                comparableQuotes.add(quote);
            }
        }

        PriceBook result = new PriceBook();
        if (!issues.isEmpty()) {
            result.calculationIssues = issues;
        }
        result.securityId = security;
        result.updateTime = quotes.stream().map(quote -> quote.updateTime).max(String::compareTo).orElse(null);
        if (result.updateTime == null) {
            result.clearField("updateTime");
        }
        result.bookRevision = Long.toString(revisions.getOrDefault(security, 0L));
        result.quoteRefs = new LinkedHashMap<>();
        MarketData market = new MarketData();
        result.marketData = market;
        market.SecurityID = security;
        market.Symbol = references.require(security).getString("securityName");
        if (market.Symbol == null) {
            market.clearField("Symbol");
        }
        market.MarketIndicator = "4";
        market.VenueTypeGW = "FICC";
        market.Venues = "FICC";
        market.NoMDEntries = new ArrayList<>();
        for (String side : Arrays.asList("1", "2")) {
            Comparator<QuoteRecord> comparator = Comparator.comparing(quote -> new BigDecimal(quote.price));
            if ("2".equals(side)) {
                comparator = comparator.reversed();
            }
            comparator = comparator.thenComparing(
                    Comparator.<QuoteRecord, String>comparing(quote -> quote.updateTime).reversed())
                    .thenComparing(quote -> quote.quoteId);
            List<QuoteRecord> sorted = comparableQuotes.stream().filter(quote -> side.equals(quote.side))
                    .sorted(comparator).collect(Collectors.toList());
            BigDecimal previousPrice = null;
            int level = 0;
            int ordinal = 0;
            int maximumLevel = "FULL".equals(mode) ? Integer.MAX_VALUE : "BEST".equals(mode) ? 1 : depth;
            for (QuoteRecord quote : sorted) {
                BigDecimal value = new BigDecimal(quote.price);
                if (previousPrice == null || previousPrice.compareTo(value) != 0) {
                    level++;
                    previousPrice = value;
                }
                if (level > maximumLevel) {
                    break;
                }
                MarketEntry entry = entry(user, quote, ++ordinal);
                market.NoMDEntries.add(entry);
                QuoteReference reference = new QuoteReference();
                reference.quoteId = quote.quoteId;
                reference.quoteRevision = quote.revision;
                result.quoteRefs.put(entry.MDEntryID, reference);
            }
        }
        return result;
    }

    private MarketEntry entry(String user, QuoteRecord quote, int ordinal) {
        OffsetDateTime time = OffsetDateTime.parse(quote.updateTime);
        MarketEntry entry = new MarketEntry();
        entry.MDEntryID = "FICC:" + quote.quoteId;
        entry.MDEntryType = "1".equals(quote.side) ? "0" : "1";
        entry.MDEntryPx = quote.price;
        entry.MDEntrySize = quote.quantity;
        entry.PriceType = "3";
        entry.ExerciseFlag = "2".equals(quote.yieldType) ? "0" : "1";
        entry.SettlType = quote.settlType;
        entry.UserID = quote.ownerUserId;
        entry.MDPriceLevel = Integer.toString(ordinal);
        entry.MDEntryDate = time.format(ENTRY_DATE);
        entry.MDEntryTime = time.format(ENTRY_TIME);
        if ("2".equals(quote.yieldType)) {
            entry.YieldToExecution = quote.price;
        } else {
            entry.Yield = quote.price;
        }
        if (user == null || access.owns(user, quote.ownerUserId)) {
            entry.InstitutionID = quote.institutionId;
            entry.TraderID = quote.contactId;
            if (entry.InstitutionID == null) {
                entry.clearField("InstitutionID");
            }
            if (entry.TraderID == null) {
                entry.clearField("TraderID");
            }
        }
        return entry;
    }
}
