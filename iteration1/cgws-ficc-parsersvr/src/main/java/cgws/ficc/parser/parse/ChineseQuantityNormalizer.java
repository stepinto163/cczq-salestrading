package cgws.ficc.parser.parse;

import java.util.regex.Matcher;
import java.util.regex.Pattern;

/** Converts Chinese quantity tokens before quote extraction; leaves bond names and decimals intact. */
public final class ChineseQuantityNormalizer {
    private static final String DIGITS = "零一二两三四五六七八九壹贰叁肆伍陆柒捌玖";
    private static final Pattern QUANTITY = Pattern.compile(
            "[" + DIGITS + "十拾百佰千仟万亿]+[千仟万亿]");

    private ChineseQuantityNormalizer() {
    }

    public static String normalize(String text) {
        Matcher matcher = QUANTITY.matcher(text);
        StringBuffer output = new StringBuffer();
        while (matcher.find()) {
            String token = matcher.group();
            long value = number(token);
            String replacement = token.contains("万") || token.contains("亿")
                    ? java.math.BigDecimal.valueOf(value, 4).stripTrailingZeros().toPlainString() + "w"
                    : Long.toString(value);
            matcher.appendReplacement(output, Matcher.quoteReplacement(" " + replacement + " "));
        }
        matcher.appendTail(output);
        return output.toString();
    }

    private static long number(String token) {
        long total = 0;
        long section = 0;
        long digit = 0;
        for (char character : token.toCharArray()) {
            int index = DIGITS.indexOf(character);
            if (index >= 0) {
                digit = index == 3 ? 2 : index <= 10 ? (index > 3 ? index - 1 : index) : index - 10;
                continue;
            }
            long unit = "十拾".indexOf(character) >= 0 ? 10
                    : "百佰".indexOf(character) >= 0 ? 100
                    : "千仟".indexOf(character) >= 0 ? 1000 : character == '万' ? 10000 : 100000000;
            if (unit < 10000) {
                section = Math.addExact(section, Math.multiplyExact(digit == 0 ? 1 : digit, unit));
            } else {
                section = Math.addExact(section, digit);
                section = Math.multiplyExact(section == 0 ? 1 : section, unit);
                total = Math.addExact(total, section);
                section = 0;
            }
            digit = 0;
        }
        return Math.addExact(total, Math.addExact(section, digit));
    }
}
