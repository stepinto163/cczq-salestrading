package ficc.quote;

import ficc.api.QuoteRecord;
import ficc.api.QuoteQueryRequest;
import ficc.api.QuoteHistorySearchRequest;
import ficc.api.QuoteHistoryRow;

import java.util.List;

/** Internal query boundary. No frontend topics, subscriptions, or quote commands. */
interface SearchSvrClient {
    List<QuoteRecord> queryQuotes(String userId, QuoteQueryRequest query);

    List<QuoteHistoryRow> searchHistory(String userId, QuoteHistorySearchRequest query) throws Exception;
}
