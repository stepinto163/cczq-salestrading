package ficc.api;

import com.alibaba.fastjson2.JSONObject;
import java.util.LinkedHashSet;
import java.util.Set;

/** DTO 基类。JSON 兼容信息仅由编解码器使用，不属于业务字段。 */
public class ProtocolModel {
    transient JSONObject original;
    transient JSONObject baseline;
    final transient Set<String> explicitNulls = new LinkedHashSet<>();

    /** 新建修改请求时显式清空字段；普通 null 属性表示未提供。 */
    public void clearField(String wireField) {
        try {
            getClass().getField(wireField).set(this, null);
            explicitNulls.add(wireField);
        } catch (ReflectiveOperationException error) {
            throw new IllegalArgumentException("Unknown protocol field: " + wireField, error);
        }
    }

    public final JSONObject toJson() {
        return ProtocolCodec.encode(this);
    }
}
