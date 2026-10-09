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
        LocalDate from = LocalDate.parse(query.fromDate),
                to = LocalDate.parse(query.toDate);
        if (from.isAfter(to) || java.time.temporal.ChronoUnit.DAYS.between(from, to) > 366)
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid date range", null);
        Filter f = query.filter;
        FilterEngine.validate(f == null ? null : f.toJson());
        Predicate<QuoteRecord> matches = matcher.prepare(user, f);
        List<JSONObject> found =
                store.rows(
                        "SELECT payload FROM history WHERE operation_time>=?::date AND"
                            + " operation_time<(?::date+interval '1 day') ORDER BY operation_time",
                        from.toString(),
                        to.toString());
        Map<String, QuoteHistoryRow> daily = new LinkedHashMap<>();
        for (JSONObject history : found) {
            QuoteRecord quote = history.getJSONObject("after") == null ? null
                    : ProtocolCodec.decode(history.getJSONObject("after"), QuoteRecord.class);
            if (quote == null || !access.owns(user, quote.ownerUserId)) {
                continue;
            }
            String date = history.getString("operationTime").substring(0, 10);
            QuoteHistoryRow row = new QuoteHistoryRow();
            row.tradeDate = date;
            row.quote = quote;
            row.reference = quote.securityId == null || refs.security(quote.securityId) == null
                    ? null : ProtocolCodec.decode(refs.security(quote.securityId), ReferenceRecord.class);
            daily.put(date + quote.quoteId, row);
        }
        return daily.values().stream()
                .filter(row -> matches.test(row.quote))
                .collect(Collectors.toList());
    }
}
