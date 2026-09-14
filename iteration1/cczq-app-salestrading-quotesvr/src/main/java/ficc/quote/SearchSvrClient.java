package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import java.util.List;

/** Internal query boundary. No frontend topics, subscriptions, or quote commands. */
interface SearchSvrClient {
    List<JSONObject> queryQuotes(String userId, JSONObject query);
    List<JSONObject> searchHistory(String userId, JSONObject query) throws Exception;
}
