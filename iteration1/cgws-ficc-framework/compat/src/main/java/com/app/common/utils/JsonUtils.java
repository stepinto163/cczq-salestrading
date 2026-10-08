// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.utils;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.parser.Feature;
import com.alibaba.fastjson.serializer.SerializerFeature;

import java.util.List;

public class JsonUtils {
    public static <T> String Serializer(T paramT) {
        return JSON.toJSONString(paramT);
    }

    public static <T> byte[] SerializerBytes(T paramT) {
        return JSON.toJSONBytes(paramT, (SerializerFeature[]) new SerializerFeature[0]);
    }

    public static <T> T Deserialize(String paramString, Class<T> paramClass) {
        return (T) JSON.parseObject((String) paramString, paramClass);
    }

    public static <T> T Deserialize(byte[] paramString, Class<T> paramClass) {
        return (T) JSON.parseObject((byte[]) paramString, paramClass, (Feature[]) new Feature[0]);
    }

    public static <T> List<T> Deserialize2(String paramString, Class<T> paramClass) {
        return JSON.parseArray((String) paramString, paramClass);
    }

    static {
        JSON.DEFAULT_PARSER_FEATURE =
                0
                        | Feature.AutoCloseSource.getMask()
                        | Feature.InternFieldNames.getMask()
                        | Feature.UseBigDecimal.getMask()
                        | Feature.AllowUnQuotedFieldNames.getMask()
                        | Feature.AllowSingleQuotes.getMask()
                        | Feature.AllowArbitraryCommas.getMask()
                        | Feature.SortFeidFastMatch.getMask()
                        | Feature.IgnoreNotMatch.getMask();
        JSON.DEFFAULT_DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";
        JSON.DEFAULT_GENERATE_FEATURE =
                0
                        | SerializerFeature.QuoteFieldNames.getMask()
                        | SerializerFeature.SkipTransientField.getMask()
                        | SerializerFeature.WriteEnumUsingToString.getMask()
                        | SerializerFeature.SortField.getMask()
                        | SerializerFeature.WriteDateUseDateFormat.getMask()
                        | SerializerFeature.DisableCircularReferenceDetect.getMask();
    }
}
