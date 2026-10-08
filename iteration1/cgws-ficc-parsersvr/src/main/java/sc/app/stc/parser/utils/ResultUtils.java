package sc.app.stc.parser.utils;

import sc.app.stc.algo.PResult;
import sc.app.stc.parser.enums.ResultMessage;
import sc.app.stc.utils.Consts;

import java.util.HashMap;
import java.util.Map;

/**
 * 返回结果工具
 *
 * @author lemon
 **/
public class ResultUtils {

    /**
     * 把错误码放入返回结果
     */
    public static Map<String, Object> getMapResult(String code, String msg) {
        Map<String, Object> result = new HashMap<>();
        result.put(Consts.Code, code);
        result.put(Consts.Msg, msg);
        return result;
    }

    /**
     * 把错误码放入返回结果
     */
    public static Map<String, Object> getMapResult(ResultMessage message) {
        return getMapResult(message.getCode(), message.getMessage());
    }

    /**
     * 把错误码放入返回结果
     */
    public static Map<String, Object> getMapError(String msg) {
        return getMapResult(ResultMessage.ERROR.getCode(), msg);
    }

    /**
     * 把成功码放入返回结果
     */
    public static Map<String, Object> getMapSuccess() {
        return getMapResult(ResultMessage.SUCCESS);
    }

    /**
     * 把错误码放入返回结果
     */
    public static Map<String, Object> getMapResult(PResult pResult) {
        return getMapResult(pResult.Code, pResult.Msg);
    }

    /**
     * 把错误码放入返回结果
     */
    public static<T> Map<String, Object> getMapResult(String code, String msg, T data) {
        Map<String, Object> result = getMapResult(code, msg);
        result.put(Consts.DATAS, data);
        return result;
    }

    /**
     * 根据结果码获取返回信息
     */
    public static PResult getResult(String code, String msg) {
        PResult result = new PResult();
        result.Code = code;
        result.Msg = msg;
        return result;
    }

    /**
     * 根据结果码获取返回信息
     */
    public static PResult getResult(ResultMessage message) {
        return getResult(message.getCode(), message.getMessage());
    }

    /**
     * 返回成功结果
     */
    public static PResult getError(String msg) {
        return getResult(ResultMessage.ERROR.getCode(), msg);
    }

    /**
     * 返回成功结果
     */
    public static PResult getSuccess() {
        return getResult(ResultMessage.SUCCESS);
    }
}
