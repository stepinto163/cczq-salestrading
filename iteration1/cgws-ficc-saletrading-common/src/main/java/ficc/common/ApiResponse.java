package ficc.common;

import com.alibaba.fastjson2.JSONObject;

/** STC request/reply envelopes; preserves the existing wire field names. */
public final class ApiResponse {
    private ApiResponse() {}

    public static JSONObject success(String requestId, Object data) {
        return JsonUtils.object("Code", "0000", "Msg", "成功", "requestId", requestId, "Data", data);
    }

    public static JSONObject failure(String requestId, ApiException exception) {
        return JsonUtils.object(
                "Code",
                exception.code,
                "Msg",
                exception.getMessage(),
                "requestId",
                requestId,
                "Data",
                null,
                "error",
                exception.details);
    }
}
