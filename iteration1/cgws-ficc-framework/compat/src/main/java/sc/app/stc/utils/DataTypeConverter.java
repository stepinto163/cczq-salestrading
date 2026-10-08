// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import java.math.BigDecimal;

public class DataTypeConverter {
    public static String toString(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number) {
            return new BigDecimal(objValue.toString()).toPlainString();
        }
        if (objValue instanceof Boolean) {
            return (Boolean) objValue != false ? "Y" : "N";
        }
        return objValue.toString();
    }

    public static Float toFloat(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number) {
            return Float.valueOf(((Number) objValue).floatValue());
        }
        if (objValue instanceof Boolean) {
            return Float.valueOf((Boolean) objValue != false ? 1.0f : 0.0f);
        }
        if (objValue instanceof String) {
            return Float.valueOf(objValue.toString());
        }
        return (Float) objValue;
    }

    public static Double toDouble(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number) {
            return ((Number) objValue).doubleValue();
        }
        if (objValue instanceof Boolean) {
            return (Boolean) objValue != false ? 1.0 : 0.0;
        }
        if (objValue instanceof String) {
            return new BigDecimal(objValue.toString()).doubleValue();
        }
        return (Double) objValue;
    }

    public static Byte toByte(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number) {
            return ((Number) objValue).byteValue();
        }
        if (objValue instanceof Boolean) {
            return (byte) ((Boolean) objValue != false ? 1 : 0);
        }
        if (objValue instanceof String) {
            return Byte.valueOf(objValue.toString());
        }
        return (Byte) objValue;
    }

    public static Integer toInt(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number) {
            return ((Number) objValue).intValue();
        }
        if (objValue instanceof Boolean) {
            return (Boolean) objValue != false ? 1 : 0;
        }
        if (objValue instanceof String) {
            return Integer.valueOf(objValue.toString());
        }
        return (Integer) objValue;
    }

    public static Long toLong(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number) {
            return ((Number) objValue).longValue();
        }
        if (objValue instanceof Boolean) {
            return (Boolean) objValue != false ? 1L : 0L;
        }
        if (objValue instanceof String) {
            return Long.valueOf(objValue.toString());
        }
        return (Long) objValue;
    }

    public static Short toShort(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number) {
            return ((Number) objValue).shortValue();
        }
        if (objValue instanceof Boolean) {
            return (short) ((Boolean) objValue != false ? 1 : 0);
        }
        if (objValue instanceof String) {
            return Short.valueOf(objValue.toString());
        }
        return (Short) objValue;
    }

    public static Character toChar(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number) {
            return Character.valueOf((char) ((Number) objValue).intValue());
        }
        if (objValue instanceof Boolean) {
            return Character.valueOf((Boolean) objValue != false ? (char) 'Y' : 'N');
        }
        if (objValue instanceof String) {
            String s = (String) objValue;
            return Character.valueOf(s.length() == 0 ? (char) '\u0000' : s.charAt(0));
        }
        return (Character) objValue;
    }

    public static Boolean toBoolean(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number) {
            Number nValue = (Number) objValue;
            if (nValue.intValue() == 0) {
                return false;
            }
            if (nValue.intValue() == 1) {
                return true;
            }
        } else {
            if (objValue instanceof Boolean) {
                return (Boolean) objValue;
            }
            if (objValue instanceof String) {
                String vString = objValue.toString();
                if (vString.equalsIgnoreCase("TRUE")
                        || vString.equalsIgnoreCase("YES")
                        || vString.equalsIgnoreCase("Y")
                        || vString.equals("1")) {
                    return true;
                }
                if (vString.equalsIgnoreCase("FALSE")
                        || vString.equalsIgnoreCase("NO")
                        || vString.equalsIgnoreCase("N")
                        || vString.equals("0")) {
                    return false;
                }
            }
        }
        return (Boolean) objValue;
    }

    public static BigDecimal toBigDecimal(Object objValue) {
        if (objValue == null) {
            return null;
        }
        if (objValue instanceof Number || objValue instanceof String) {
            return new BigDecimal(objValue.toString());
        }
        if (objValue instanceof Boolean) {
            return new BigDecimal((Boolean) objValue != false ? "1" : "0");
        }
        return (BigDecimal) objValue;
    }
}
