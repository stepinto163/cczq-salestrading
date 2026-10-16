package ficc.quote;

import com.alibaba.fastjson2.*;

import ficc.common.*;
import ficc.api.*;
import java.util.function.Predicate;
import ficc.common.client.AccessPolicy;

import java.time.*;
import java.util.*;
import java.util.stream.Collectors;

/**
 * SearchSvrClient 的进程内实现，名称中的 Client 不表示当前有独立 SearchSvr 网络服务。
 * 当前报价读内存快照，历史读持久化记录，价格查询复用 PriceBookService。
 * 调用由 QuoteSvr 串行调度协调；这里不持有网络连接或订阅状态。
 */
final class LocalSearchSvrClient implements SearchSvrClient {
    final AccessPolicy access;
    final PgStore store;
    final ReferenceData refs;
    final Map<String, QuoteRecord> quotes;
    final QuoteMatcher matcher;
    private final PriceBookService priceBooks;

    LocalSearchSvrClient(
            PgStore store,
            ReferenceData refs,
            Map<String, QuoteRecord> quotes,
            QuoteMatcher matcher,
            AccessPolicy access,
            PriceBookService priceBooks) {
        this.access = access;
        this.store = store;
        this.refs = refs;
        this.quotes = quotes;
        this.matcher = matcher;
        this.priceBooks = priceBooks;
    }

    @Override
    public List<QuoteRecord> queryQuotes(String user, QuoteQueryRequest req) {
        Predicate<QuoteRecord> matches = matcher.prepareQuery(user, req, access);
        return quotes.values().stream()
                .filter(matches)
                .sorted(
                        Comparator.<QuoteRecord, String>comparing(q -> q.updateTime)
                                .reversed()
                                .thenComparing(q -> q.quoteId))
                .collect(Collectors.toList());
    }

    @Override
    public List<PriceBook> queryPrices(String user, PriceQueryRequest query) {
        return priceBooks.query(user, query);
    }

    @Override
    public List<QuoteHistoryRow> searchHistory(String user, QuoteHistorySearchRequest query) throws Exception {
        LocalDate from;
        LocalDate to;
        try {
            from = LocalDate.parse(query.fromDate);
            to = LocalDate.parse(query.toDate);
        } catch (java.time.format.DateTimeParseException | NullPointerException error) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid date range", null);
        }
        if (from.isAfter(to) || java.time.temporal.ChronoUnit.DAYS.between(from, to) > 366) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid date range", null);
        }
        Filter filter = query.filter;
        FilterEngine.validate(filter == null ? null : filter.toJson());
        Predicate<QuoteRecord> matches = matcher.prepare(user, filter);
        boolean allOwners = user != null && (access.administrator(user) || access.internal(user));
        List<QuoteHistoryRow> result = new ArrayList<>();
        // 先选当日最后状态，再判断价格等条件，不能让满足条件的旧版本重新出现。
        // 基础资料及分组继续使用公共筛选器，保持历史页面使用当前基础资料的契约。
        store.scanRows(HistorySearchSql.latestDaily(allOwners), snapshot -> {
            QuoteRecord quote = ProtocolCodec.decode(snapshot.getJSONObject("quote"), QuoteRecord.class);
            if (!access.owns(user, quote.ownerUserId) || !matches.test(quote)) {
                return;
            }
            if (result.size() == 10000) {
                throw ApiException.forField("1008", "RESOURCE_LIMIT",
                        "Client result cap: narrow the filter", "filter");
            }
            QuoteHistoryRow row = new QuoteHistoryRow();
            row.tradeDate = snapshot.getString("tradeDate");
            row.quote = quote;
            JSONObject security = quote.securityId == null ? null : refs.security(quote.securityId);
            row.reference = security == null ? null : ProtocolCodec.decode(security, ReferenceRecord.class);
            result.add(row);
        }, allOwners ? new Object[] {from.toString(), to.toString()}
                : new Object[] {from.toString(), to.toString(), user});
        return result;
    }
}
