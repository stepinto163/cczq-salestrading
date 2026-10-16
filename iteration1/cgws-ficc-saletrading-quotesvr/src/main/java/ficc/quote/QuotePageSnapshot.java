package ficc.quote;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.api.QuoteRecord;
import ficc.common.client.AccessPolicy;
import java.util.ArrayList;
import java.util.List;

/** Detached page metadata and stable record versions; no live view state is read during encoding. */
final class QuotePageSnapshot {
    private final JSONObject metadata;
    private final List<QuoteRecord> quotes;
    private final String user;
    private final AccessPolicy access;

    QuotePageSnapshot(JSONObject metadata, List<QuoteRecord> quotes, String user, AccessPolicy access) {
        this.metadata = metadata;
        this.quotes = new ArrayList<>(quotes);
        this.user = user;
        this.access = access;
    }

    JSONObject materialize() {
        JSONArray rows = new JSONArray();
        for (QuoteRecord quote : quotes) {
            rows.add(visibleJson(quote, user, access));
        }
        metadata.put("quotes", rows);
        return metadata;
    }

    static JSONObject visibleJson(QuoteRecord quote, String user, AccessPolicy access) {
        JSONObject result = quote.toJson();
        return result;
    }
}
