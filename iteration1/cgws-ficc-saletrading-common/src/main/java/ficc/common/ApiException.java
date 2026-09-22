package ficc.common;

import com.alibaba.fastjson2.JSONObject;

/** Business error code and structured details returned in an API failure reply. */
public final class ApiException extends RuntimeException {
    public final String code;
    public final JSONObject details;

    public ApiException(String code, String message, JSONObject details) {
        super(message);
        this.code = code;
        this.details = details;
    }

    public static ApiException forField(String code, String type, String message, String field) {
        return new ApiException(code, message, JsonUtils.object("type", type, "field", field));
    }
}
