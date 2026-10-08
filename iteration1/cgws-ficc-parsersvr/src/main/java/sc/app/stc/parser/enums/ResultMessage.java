package sc.app.stc.parser.enums;

/**
 * 接口返回结果
 *
 * @author lemon
 */
@SuppressWarnings("unused")
public enum ResultMessage {
    SUCCESS("0000", "成功"),
    REQ_PARAM_ERROR("2000", "参数解析失败"),
    REQ_USER_EMPTY("2001", "用户名为空"),
    REG_CONTENT_EMPTY("2002", "识别文本为空"),
    REG_QUOTE_EMPTY("2003", "未识别到报价"),
    REG_QUOTE_LIMIT("2004", "最大报价条数200，请检查"),
    ERROR("3999", "未知错误");
    /**
     * 错误码
     */
    private final String code;
    /**
     * 错误信息
     */
    private final String message;

    ResultMessage(String code, String message) {
        this.code = code;
        this.message = message;
    }

    public String getCode() {
        return code;
    }

    public String getMessage() {
        return message;
    }
}
