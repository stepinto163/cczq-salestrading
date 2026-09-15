package ficc.quote;

import ficc.common.*;
import ficc.common.client.AccessPolicy;

import com.alibaba.fastjson2.*;
import java.util.*;
import java.time.*;

/** Iteration-one adapter, invoked on QuoteSvr ordered dispatcher. No network/subscription state. */
final class LocalSearchSvrClient implements SearchSvrClient {
    final AccessPolicy access;
    final PgStore store; final ReferenceData refs; final Map<String,JSONObject> quotes; final QuoteMatcher matcher;
    LocalSearchSvrClient(PgStore store,ReferenceData refs,Map<String,JSONObject> quotes,QuoteMatcher matcher,AccessPolicy access){this.access=access;this.store=store;this.refs=refs;this.quotes=quotes;this.matcher=matcher;}
    public List<JSONObject> queryQuotes(String user,JSONObject req){
        JSONObject f=req.getJSONObject("filter");FilterEngine.validate(f);
        String scope=JsonUtils.getStringOrDefault(req,"ownerScope","VISIBLE");if(!Set.of("VISIBLE","MINE").contains(scope))throw ApiException.forField("1001","VALIDATION_FAILED","Invalid ownerScope","ownerScope");
        JSONArray status=req.getJSONArray("status");Set<String> sts=status==null?Set.of("ACTIVE"):new HashSet<>(status.toJavaList(String.class));if(!Set.of("ACTIVE","PENDING_CONFIRM","REFERRED","DELETED").containsAll(sts))throw ApiException.forField("1001","VALIDATION_FAILED","Unknown quote status","status");
        return quotes.values().stream().filter(q->sts.contains(q.getString("status"))).filter(q->q.getString("status").equals("ACTIVE")||access.owns(user,q.getString("ownerUserId"))).filter(q->!scope.equals("MINE")||user.equals(q.getString("ownerUserId"))).filter(q->matcher.matches(user,q,f)).sorted(Comparator.<JSONObject,String>comparing(q->q.getString("updateTime")).reversed().thenComparing(q->q.getString("quoteId"))).toList();
    }
    public List<JSONObject> searchHistory(String user,JSONObject query)throws Exception{
        LocalDate from=LocalDate.parse(RequestValidator.requireString(query,"fromDate")),to=LocalDate.parse(RequestValidator.requireString(query,"toDate"));if(from.isAfter(to)||java.time.temporal.ChronoUnit.DAYS.between(from,to)>366)throw ApiException.forField("1001","VALIDATION_FAILED","Invalid date range",null);
        JSONObject f=query.getJSONObject("filter");FilterEngine.validate(f);
        List<JSONObject> found=store.rows("SELECT payload FROM history WHERE operation_time>=?::date AND operation_time<(?::date+interval '1 day') ORDER BY operation_time",from.toString(),to.toString());
        Map<String, JSONObject> daily = new LinkedHashMap<>();
        for (JSONObject history : found) {
            JSONObject quote = history.getJSONObject("after");
            if (quote == null || !access.owns(user, quote.getString("ownerUserId"))) {
                continue;
            }
            String date = history.getString("operationTime").substring(0, 10);
            daily.put(date + quote.getString("quoteId"), JsonUtils.object("tradeDate", date,
                    "quote", quote, "reference", refs.security(quote.getString("securityId"))));
        }
        return daily.values().stream().filter(r->matcher.matches(user,r.getJSONObject("quote"),f)).map(JsonUtils::deepCopy).toList();
    }
}
