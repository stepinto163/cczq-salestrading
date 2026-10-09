package ficc.quote;

import ficc.api.QuoteRecord;
import ficc.api.QuoteQueryRequest;
import ficc.api.QuoteHistorySearchRequest;
import ficc.api.QuoteHistoryRow;
import ficc.api.PriceBook;
import ficc.api.PriceQueryRequest;

import java.util.List;

/**
 * 报价、价格簿、历史查询的内部强类型接口；便于将查询实现与命令处理分开维护。
 * 当前由 LocalSearchSvrClient 在进程内实现；不在此定义前端 topic、订阅或报价修改操作。
 */
interface SearchSvrClient {
    List<QuoteRecord> queryQuotes(String userId, QuoteQueryRequest query);

    List<PriceBook> queryPrices(String userId, PriceQueryRequest query);

    List<QuoteHistoryRow> searchHistory(String userId, QuoteHistorySearchRequest query) throws Exception;
}
