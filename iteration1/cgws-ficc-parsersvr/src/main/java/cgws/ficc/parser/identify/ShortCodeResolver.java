package cgws.ficc.parser.identify;

import java.time.LocalDate;
import java.util.Collections;
import java.util.HashSet;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Three-to-five-digit abbreviations from the legacy BondQuoteParser Java layer.
 * Eligible codes must be limited by the caller to BGB/PBB/PSB bonds, as in loadData.
 * Separate from native ParseBond: the service coordinator decides whether short codes
 * are enabled and suppresses them when a message already contains a full bond code.
 */
public final class ShortCodeResolver {
    private static final Pattern SHORT_CODE = Pattern.compile("\\d{3,5}");
    private static final Pattern NUMBER = Pattern.compile("\\d+(?:\\.\\d+)?");
    private static final String[] MARKETS = {".ib", ".sh", ".sz"};
    private final Set<String> eligibleCodes;
    private final int businessYear;

    public ShortCodeResolver(Set<String> eligibleCodes, LocalDate businessDate) {
        if (eligibleCodes == null || businessDate == null) {
            throw new IllegalArgumentException("Eligible codes and business date are required");
        }
        this.eligibleCodes = Collections.unmodifiableSet(new HashSet<String>(eligibleCodes));
        this.businessYear = businessDate.getYear();
    }

    /** Returns a replacement, or null when the legacy wrapper would not retry parsing. */
    public String replaceFirst(String text) {
        if (text == null) {
            return null;
        }
        String[] tokens = text.split("\\s+");
        for (int index = 0; index < tokens.length; index++) {
            String resolved = resolve(tokens[index]);
            if (resolved != null) {
                tokens[index] = resolved;
                return String.join(" ", tokens);
            }
        }
        Matcher numbers = NUMBER.matcher(text);
        while (numbers.find()) {
            String number = numbers.group();
            if (numbers.end() < text.length() && excludesFollowingCharacter(text.charAt(numbers.end()))) {
                continue;
            }
            String resolved = resolve(number);
            if (resolved != null) {
                // The wrapper's embedded-token fallback replaces all equal occurrences.
                return text.replace(number, resolved);
            }
        }
        return null;
    }

    public String resolve(String shortCode) {
        if (shortCode == null || !SHORT_CODE.matcher(shortCode).matches()) {
            return null;
        }
        if (shortCode.length() == 3) {
            char leading = shortCode.charAt(0);
            return leading >= '2' && leading <= '4' ? findRecent("0" + shortCode) : null;
        }
        String fullCode = shortCode.substring(0, 2) + "00" + shortCode.substring(2);
        String resolved = findMarket(fullCode);
        if (resolved != null) {
            return resolved;
        }
        return shortCode.length() == 4 && shortCode.startsWith("99") ? findRecent(shortCode) : null;
    }

    private String findRecent(String suffix) {
        for (int offset = 0; offset < 10; offset++) {
            // Preserve GetLastYears' numeric formatting; do not silently invent a century rule.
            String resolved = findMarket((businessYear % 100 - offset) + suffix);
            if (resolved != null) {
                return resolved;
            }
        }
        return null;
    }

    private String findMarket(String code) {
        for (String market : MARKETS) {
            if (eligibleCodes.contains(code + market)) {
                return code + market;
            }
        }
        return null;
    }

    private static boolean excludesFollowingCharacter(char character) {
        return ".-+/*%".indexOf(character) >= 0
                || character >= 'a' && character <= 'z'
                || character >= 'A' && character <= 'Z';
    }
}
