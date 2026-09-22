package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;

import java.util.function.Consumer;

/** Thin test seam; production implementation delegates transport/discovery to STC. */
public interface StcTransport {
    JSONObject request(String topic, String sessionId, JSONObject request);

    boolean online();

    void subscribe(String topic, Consumer<JSONObject> listener);

    void connectionChanged(Runnable listener);
}
