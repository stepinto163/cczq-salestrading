package cgws.ficc.parser.parse;

import java.math.BigDecimal;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Legacy money-market helper formats, distinct from bond quote price extraction.
 * Rules come from MMParser in the Linux library and recorded Windows reference calls.
 * Returned amounts use ten-thousand yuan; tenor endpoints preserve legacy sentinels.
 * This class does not trim or change case: callers must preserve the native API boundary.
 */
public final class MoneyMarketFields {
    private static final Pattern PRICE_RANGE = Pattern.compile("[1-8]\\.\\d{1,6}%?-[1-8]\\.\\d{1,6}%?");
    private static final Pattern PRICE_PERCENT = Pattern.compile("[1-8](\\.\\d{1,6})?%");
    private static final Pattern DAY_RANGE = Pattern.compile("(\\d{1,2})[Dd]?-{1,4}(\\d{1,2})[Dd]");
    private static final Pattern MONTH_RANGE = Pattern.compile("(\\d{1,2})[Mm]?-{1,4}(\\d)[Mm]");
    private static final Pattern DAY_MONTH_RANGE = Pattern.compile("(\\d{1,2})[Dd]-{1,4}(\\d)[Mm]");
    private static final Pattern DAYS = Pattern.compile("(\\d{1,2})[天Dd](\\+)?");
    private static final Pattern MONTHS = Pattern.compile("(\\d)[月Mm]");
    private static final Pattern WEEKS = Pattern.compile("(\\d)[周Ww]");
    private static final Pattern AMOUNT = Pattern.compile("[1-9](\\d{0,2}0|\\d{2}00)\\.?[Ww万]?|\\d{1,2}[Kk][Ww]?");
    private static final Pattern HUNDRED_MILLION = Pattern.compile("([1-4]?\\d(?:\\.\\d{1,3})?)[Ee亿个]?");
    private static final Pattern AMOUNT_RANGE = Pattern.compile("(\\d+)([Kk]?)[Ww]?-{1,3}(\\d+)([Kk]?)[Ww]");
    private static final Pattern MIXED_RANGE = Pattern.compile("(\\d+)([Kk]?)[Ww]-{1,3}((?:\\d+\\.)?\\d+)[Ee]");
    private static final Pattern TEN_MILLION = Pattern.compile("(\\d)千万");
    private static final Pattern MILLION = Pattern.compile("(\\d)[Bb][Ww]?");
    private static final Pattern FIRST_NUMBER = Pattern.compile("\\d+");

    private MoneyMarketFields() {
    }

    public static String price(String text) {
        requireText(text);
        return PRICE_RANGE.matcher(text).matches() || PRICE_PERCENT.matcher(text).matches() ? text : "";
    }

    public static String tenor(String text) {
        requireText(text);
        Matcher match = DAY_RANGE.matcher(text);
        if (match.matches()) {
            return endpoints(match, 1, 1);
        }
        match = MONTH_RANGE.matcher(text);
        if (match.matches()) {
            return endpoints(match, 30, 30);
        }
        match = DAY_MONTH_RANGE.matcher(text);
        if (match.matches()) {
            return endpoints(match, 1, 30);
        }
        match = DAYS.matcher(text);
        if (match.matches()) {
            return Integer.parseInt(match.group(1)) + (match.group(2) == null ? ":9999" : ":10000");
        }
        match = MONTHS.matcher(text);
        if (match.matches()) {
            return Integer.parseInt(match.group(1)) * 30 + ":9999";
        }
        match = WEEKS.matcher(text);
        if (match.matches()) {
            return Integer.parseInt(match.group(1)) * 7 + ":9999";
        }
        return "";
    }

    public static String amount(String text) {
        requireText(text);
        if (AMOUNT.matcher(text).matches()) {
            Matcher number = FIRST_NUMBER.matcher(text);
            number.find();
            long value = Long.parseLong(number.group());
            // The original helper recognizes K in the grammar but multiplies lowercase k only.
            if (text.contains("k")) {
                value *= 1000;
            }
            return value > 99 ? value + ":0" : "";
        }
        Matcher match = HUNDRED_MILLION.matcher(text);
        if (match.matches()) {
            if (text.contains("e") || text.contains("亿") || text.contains("个")) {
                return new BigDecimal(match.group(1)).multiply(BigDecimal.valueOf(10000))
                        .stripTrailingZeros().toPlainString() + ":0";
            }
            return "";
        }
        match = AMOUNT_RANGE.matcher(text);
        if (match.matches()) {
            long first = scaledInteger(match.group(1), match.group(2));
            long second = scaledInteger(match.group(3), match.group(4));
            return first + ":" + second;
        }
        match = MIXED_RANGE.matcher(text);
        if (match.matches()) {
            long first = scaledInteger(match.group(1), match.group(2));
            long second = new BigDecimal(match.group(3)).multiply(BigDecimal.valueOf(10000)).longValueExact();
            return first + ":" + second;
        }
        match = TEN_MILLION.matcher(text);
        if (match.matches()) {
            return Integer.parseInt(match.group(1)) * 1000 + ":0";
        }
        match = MILLION.matcher(text);
        if (match.matches()) {
            return Integer.parseInt(match.group(1)) * 100 + ":0";
        }
        return "";
    }

    private static long scaledInteger(String value, String unit) {
        return Math.multiplyExact(Long.parseLong(value), "k".equals(unit) ? 1000L : 1L);
    }

    private static String endpoints(Matcher match, int firstScale, int secondScale) {
        return Integer.parseInt(match.group(1)) * firstScale + ":"
                + Integer.parseInt(match.group(2)) * secondScale;
    }

    private static void requireText(String text) {
        if (text == null) {
            throw new IllegalArgumentException("Parser input must not be null");
        }
    }
}
