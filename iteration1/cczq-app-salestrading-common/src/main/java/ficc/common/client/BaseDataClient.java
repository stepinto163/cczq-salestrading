package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import java.util.List;
import java.util.Map;

/** FICC reference model boundary; implementations hide STC/vendor dictionaries. */
public interface BaseDataClient extends AutoCloseable {
    Map<String, JSONObject> securities();
    List<JSONObject> records(String kind);
    String version();
    default void start() throws Exception {}
    @Override
    default void close() throws Exception {}
}
