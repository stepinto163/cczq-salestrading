package ficc.api;

import com.alibaba.fastjson2.JSONObject;

/** 编译期绑定 topic、请求和成功 Data 类型。错误仍使用统一 Reply。 */
public final class Endpoint<Q extends Request, R extends ProtocolModel> {
    public final String topic;
    public final Class<Q> requestType;
    public final Class<R> dataType;

    public Endpoint(String topic, Class<Q> requestType, Class<R> dataType) {
        this.topic = topic;
        this.requestType = requestType;
        this.dataType = dataType;
    }

    public Q request(JSONObject value) {
        return ProtocolCodec.decode(value, requestType);
    }

    public R data(JSONObject value) {
        return ProtocolCodec.decode(value, dataType);
    }

    public ApiReply<R> reply(JSONObject value) {
        Reply envelope = ProtocolCodec.decode(value, Reply.class);
        ApiReply<R> result = new ApiReply<>();
        result.Code = envelope.Code;
        result.Msg = envelope.Msg;
        result.requestId = envelope.requestId;
        result.error = envelope.error;
        if ("0000".equals(result.Code) && value.get("Data") instanceof JSONObject) {
            result.Data = data(value.getJSONObject("Data"));
        }
        return result;
    }
}
