// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import com.alibaba.fastjson.JSON;

import java.io.PrintWriter;
import java.io.StringWriter;
import java.io.UnsupportedEncodingException;
import java.lang.reflect.Field;
import java.math.BigDecimal;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.text.DecimalFormat;
import java.text.NumberFormat;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.atomic.AtomicLong;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class StringUtil {
    private static Pattern p = Pattern.compile("\\s*|\t|\r|\n");
    private static NumberFormat nf = new DecimalFormat("0.00");
    private static String localeIP = null;
    private static AtomicLong mdReqID = new AtomicLong();
    private static String hostName = null;

    public static <T> String parseJson(T obj) {
        if (obj == null) {
            return null;
        }
        if (obj instanceof String) {
            return (String) obj;
        }
        return JSON.toJSONString(obj);
    }

    public static Set<String> parse(String exp) {
        int length = exp.length();
        int start = 0;
        int end = 0;
        HashSet<String> expSet = new HashSet<String>();
        while (start > -1
                && end > -1
                && start < length
                && (start = exp.indexOf(123, end)) >= 0
                && (end = exp.indexOf(125, start)) >= 0) {
            String clause = exp.substring(start + 1, end);
            expSet.add(clause);
            start = end;
        }
        return expSet;
    }

    public static boolean contain(String srcMsg, String findedMsg, String split) {
        if (StringUtil.isNotEmpty(srcMsg)) {
            String[] srcValues = srcMsg.split(split);
            if (srcValues != null && srcValues.length > 0) {
                for (String s : srcValues) {
                    if (!s.equals(findedMsg)) continue;
                    return true;
                }
                return false;
            }
            return StringUtil.isEmpty(findedMsg);
        }
        return StringUtil.isEmpty(findedMsg);
    }

    public static List<String> rmSameAndNull(List<String> arrys) {
        if (arrys != null && arrys.size() > 0) {
            ArrayList<String> list = new ArrayList<String>();
            for (String a : arrys) {
                if (!StringUtil.isNotEmpty(a) || list.contains(a)) continue;
                list.add(a);
            }
            return list;
        }
        return arrys;
    }

    public static String rmSameAndNull(String arrys, String split) {
        if (StringUtil.isNotEmpty(arrys)) {
            String[] result = StringUtil.rmSameAndNull(arrys.split(split));
            if (result != null && result.length > 0) {
                StringBuilder strB = new StringBuilder();
                strB.append(result[0]);
                for (int i = 1; i < result.length; ++i) {
                    strB.append(split).append(result[i]);
                }
                return strB.toString();
            }
            return "";
        }
        return arrys;
    }

    public static String[] rmSameAndNull(String[] arrys) {
        if (arrys != null && arrys.length > 0) {
            ArrayList<String> list = new ArrayList<String>();
            for (String a : arrys) {
                if (!StringUtil.isNotEmpty(a) || list.contains(a)) continue;
                list.add(a);
            }
            String[] result = new String[list.size()];
            return list.toArray(result);
        }
        return arrys;
    }

    public static String getUnqiueIDLog(long uniqueID) {
        return "[" + uniqueID + "]";
    }

    public static String replaceBlank(String str) {
        String dest = "";
        if (str != null) {
            Matcher m = p.matcher(str);
            dest = m.replaceAll("");
        }
        return dest;
    }

    public static boolean isDigital(String str) {
        if (StringUtil.isBlank(str)) {
            return false;
        }
        return str.matches("\\d+");
    }

    public static boolean isAlpha(String str) {
        if (StringUtil.isBlank(str)) {
            return false;
        }
        return str.matches("^[A-Za-z0-9]+$");
    }

    public static boolean isMoney(String str) {
        try {
            new BigDecimal(str);
        } catch (Exception e) {
            return false;
        }
        return true;
    }

    public static boolean isBlank(String str) {
        int length;
        if (str == null || (length = str.length()) == 0) {
            return true;
        }
        for (int i = 0; i < length; ++i) {
            if (Character.isWhitespace(str.charAt(i))) continue;
            return false;
        }
        return true;
    }

    public static boolean isNotBlank(String str) {
        int length;
        if (str == null || (length = str.length()) == 0) {
            return false;
        }
        for (int i = 0; i < length; ++i) {
            if (Character.isWhitespace(str.charAt(i))) continue;
            return true;
        }
        return false;
    }

    public static void copyValue(Map<String, Object> src, Map<String, Object> des, String key) {
        if (!des.containsKey(key) && src.containsKey(key)) {
            des.put(key, src.get(key));
        }
    }

    public static void copyValue(Map<String, Object> src, Map<String, Object> des, String[] keys) {
        for (String key : keys) {
            if (des.containsKey(key) || !src.containsKey(key)) continue;
            des.put(key, src.get(key));
        }
    }

    public static boolean isEmpty(String str) {
        return str == null || str.trim().length() == 0;
    }

    public static boolean isNotEmpty(String str) {
        return str != null && str.trim().length() > 0;
    }

    public static boolean equalsIgnoreCase(String str1, String str2) {
        if (str1 == null) {
            return str2 == null;
        }
        return str1.equalsIgnoreCase(str2);
    }

    public static boolean equals(String str1, String str2) {
        if (str1 == null) {
            return str2 == null;
        }
        return str1.equals(str2);
    }

    public static String bytesToHexString(byte[] src) {
        StringBuilder stringBuilder = new StringBuilder("");
        if (src == null || src.length <= 0) {
            return null;
        }
        for (int i = 0; i < src.length; ++i) {
            int v = src[i] & 0xFF;
            String hv = Integer.toHexString(v);
            if (hv.length() < 2) {
                stringBuilder.append(0);
            }
            stringBuilder.append(hv);
        }
        return stringBuilder.toString();
    }

    public static String byteToString(byte[] bytes) {
        if (null == bytes || bytes.length == 0) {
            return "";
        }
        String strContent = "";
        try {
            strContent = new String(bytes, "utf-8");
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
        }
        return strContent;
    }

    public static byte[] hexStringToBytes(String hexString) {
        if (hexString == null || hexString.equals("")) {
            return null;
        }
        hexString = hexString.toUpperCase();
        int length = hexString.length() / 2;
        char[] hexChars = hexString.toCharArray();
        byte[] d = new byte[length];
        for (int i = 0; i < length; ++i) {
            int pos = i * 2;
            d[i] =
                    (byte)
                            (StringUtil.charToByte(hexChars[pos]) << 4
                                    | StringUtil.charToByte(hexChars[pos + 1]));
        }
        return d;
    }

    private static byte charToByte(char c) {
        return (byte) "0123456789ABCDEF".indexOf(c);
    }

    public static String toAmountString(float amount) {
        return nf.format(amount);
    }

    public static String toAmountString(BigDecimal amount) {
        return nf.format(amount);
    }

    public static String toAmountString(double amount) {
        return nf.format(amount);
    }

    public static String arrayToString(String[] strs) {
        if (strs == null || strs.length < 1) {
            return null;
        }
        int length = strs.length - 1;
        StringBuilder strB = new StringBuilder();
        for (int i = 0; i <= length; ++i) {
            strB.append(strs[i]);
            if (i == length) continue;
            strB.append(",");
        }
        return strB.toString();
    }

    public static String[] arrayTrim(String[] strs) {
        if (strs == null || strs.length < 1) {
            return strs;
        }
        ArrayList<String> list = new ArrayList<String>();
        for (int i = 0; i < strs.length; ++i) {
            if (StringUtil.isNull(strs[i])) continue;
            list.add(strs[i]);
        }
        return list.toArray(new String[list.size()]);
    }

    public static String[] arrayTrimNullAndSame(String[] strs) {
        if (strs == null || strs.length < 1) {
            return strs;
        }
        ArrayList<String> list = new ArrayList<String>();
        for (int i = 0; i < strs.length; ++i) {
            if (StringUtil.isNull(strs[i]) || list.contains(strs[i])) continue;
            list.add(strs[i]);
        }
        return list.toArray(new String[list.size()]);
    }

    public static String arrayToString(ArrayList<String> strs) {
        if (strs == null || strs.size() < 1) {
            return null;
        }
        int length = strs.size() - 1;
        StringBuilder strB = new StringBuilder();
        for (int i = 0; i <= length; ++i) {
            strB.append(strs.get(i));
            if (i == length) continue;
            strB.append(",");
        }
        return strB.toString();
    }

    public static int parseIn(String value, int defaultValue) {
        try {
            return Integer.parseInt(value);
        } catch (Exception e) {
            return defaultValue;
        }
    }

    public static String getLocaleIP() {
        if (localeIP == null) {
            try {
                InetAddress addr = InetAddress.getLocalHost();
                localeIP = addr.getHostAddress().toString();
                System.out.println("Locale IP:" + localeIP);
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        return localeIP;
    }

    public static String getHostName() {
        if (hostName == null) {
            hostName = StringUtil.getLocalHostName();
        }
        return hostName;
    }

    public static String getLocalHostName() {
        String localObject;
        try {
            localObject = InetAddress.getLocalHost().getHostName();
        } catch (Exception localException) {
            localObject = "";
        }
        return localObject;
    }

    public static String charsetConvert(String str) {
        return StringUtil.charsetConvert(str, "ISO-8859-1", "UTF-8");
    }

    public static String charsetConvert(String str, String srcEncode, String destEncode) {
        try {
            if (str == null) {
                return null;
            }
            return new String(str.getBytes(srcEncode), destEncode);
        } catch (UnsupportedEncodingException e) {
            return str;
        }
    }

    public static String getHostAndPort(InetSocketAddress addr) {
        return addr.getHostName() + ":" + addr.getPort();
    }

    public static String convStr(Object obj) {
        if (obj != null) {
            return String.valueOf(obj);
        }
        return null;
    }

    public static Map<String, Object> objectToMap(Object obj) {
        HashMap<String, Object> map = new HashMap<String, Object>();
        Class<?> clazz = obj.getClass();
        for (Field field : clazz.getDeclaredFields()) {
            field.setAccessible(true);
            String fieldName = field.getName();
            Object value = null;
            try {
                value = field.get(obj);
            } catch (IllegalArgumentException e) {
                e.printStackTrace();
            } catch (IllegalAccessException e) {
                e.printStackTrace();
            }
            map.put(fieldName, value);
        }
        return map;
    }

    public static boolean isNull(String str) {
        return str == null;
    }

    public static String printStackTraceToString(Throwable t) {
        StringWriter sw = new StringWriter();
        t.printStackTrace(new PrintWriter(sw, true));
        return sw.getBuffer().toString();
    }
}
