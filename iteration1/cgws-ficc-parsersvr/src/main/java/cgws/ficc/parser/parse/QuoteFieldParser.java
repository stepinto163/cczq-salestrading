package cgws.ficc.parser.parse;

import cgws.ficc.parser.dict.BondDictionary;
import cgws.ficc.parser.identify.BondIdentifier;
import cgws.ficc.parser.model.QuoteInfo;

import java.util.ArrayList;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Set;
import java.util.Locale;

/**
 * Extracts the quote record out of one assembled line.
 *
 * <p>Every rule carries the evidence it was derived from in {@code coverage.zh-CN.md}. Rules that
 * are not confirmed yet are marked in comments with INFERRED and are the first candidates for the
 * next comparison round.
 */
public final class QuoteFieldParser {

    private static final Set<String> BID_WORDS = new LinkedHashSet<String>();
    private static final Set<String> OFR_WORDS = new LinkedHashSet<String>();

    static {
        BID_WORDS.add("bid");
        BID_WORDS.add("buy");
        BID_WORDS.add("gvn");
        BID_WORDS.add("买");
        BID_WORDS.add("买入");
        OFR_WORDS.add("ofr");
        OFR_WORDS.add("sell");
        OFR_WORDS.add("tkn");
        OFR_WORDS.add("卖");
        OFR_WORDS.add("卖出");
    }

    private QuoteFieldParser() {
    }

    /** Returns null when the line carries no parseable content (legacy returns an empty string). */
    public static QuoteInfo parseLine(String line, BondDictionary dict) {
        String text = line == null ? "" : line;
        String trimmed = text.trim();
        if (trimmed.isEmpty()) {
            return null;
        }
        // The working text keeps its trailing space (legacy PreProcess appends one) and is what
        // appears in debug messages, so tokenising happens on the trimmed copy only.
        String[] tokens = trimmed.split("\\s+");
        QuoteInfo q = new QuoteInfo();
        q.Text = text;

        List<String> priceCandidates = new ArrayList<String>();
        List<String> volumeCandidates = new ArrayList<String>();
        List<String> unresolvedCodeLike = new ArrayList<String>();
        List<String> codeShapedUnresolved = new ArrayList<String>();
        List<String> unknownWords = new ArrayList<String>();
        Set<String> codes = new LinkedHashSet<String>();
        String nameFromNameToken = null;
        String codeFromCodeToken = null;

        for (String raw : tokens) {
            String token = raw.trim();
            if (token.isEmpty()) {
                continue;
            }
            String lower = token.toLowerCase(Locale.ROOT);
            if ("gvn".equals(lower) || "given".equals(lower)
                    || "tkn".equals(lower) || "taken".equals(lower)) {
                // Deal messages do not become new bond quote records at this boundary.
                return null;
            }
            if ("+0".equals(token) || "+1".equals(token)) {
                q.tradeDay = token;
                continue;
            }
            if (!isNumber(token)) {
                String amount = MoneyMarketFields.amount(token);
                if (amount.endsWith(":0")) {
                    volumeCandidates.add(amount.substring(0, amount.length() - 2));
                    continue;
                }
            }
            if (BID_WORDS.contains(lower)) {
                q.Direction = "bid";
                continue;
            }
            if (OFR_WORDS.contains(lower)) {
                q.Direction = "ofr";
                continue;
            }
            if (token.indexOf('/') > 0) {
                String[] sides = token.split("/");
                if (sides.length == 2 && isNumber(sides[0]) && isNumber(sides[1])) {
                    q.BidPrice = sides[0];
                    q.OfrPrice = sides[1];
                    continue;
                }
            }
            if (lower.endsWith("bp") && isNumber(lower.substring(0, lower.length() - 2))) {
                // INFERRED: bp floats are kept out of Single, matching the observed 2.35bp sample.
                q.BPFloat = lower.substring(0, lower.length() - 2);
                continue;
            }
            if (isRange(token)) {
                // Observed: "2.35-2.30" produces no price fields at all.
                continue;
            }
            if (isNumber(token)) {
                if (token.indexOf('.') >= 0) {
                    priceCandidates.add(token);
                } else if (token.length() >= 6) {
                    // Real six- and nine-digit identifiers resolve when the market-qualified
                    // dictionary entry exists. A plain numeric dictionary row alone is insufficient.
                    List<String> resolved = BondIdentifier.resolveNumericCode(token, dict);
                    if (!resolved.isEmpty()) {
                        codes.addAll(resolved);
                        if (codeFromCodeToken == null) {
                            codeFromCodeToken = resolved.get(0);
                        }
                    } else {
                        if (q.Direction != null || token.length() == 8) {
                            unresolvedCodeLike.add(token);
                        }
                        if (token.length() == 7) {
                            codeShapedUnresolved.add(token);
                        }
                    }
                } else {
                    volumeCandidates.add(token);
                }
                continue;
            }
            List<String> byName = BondIdentifier.resolveName(token, dict);
            if (!byName.isEmpty()) {
                codes.addAll(byName);
                if (nameFromNameToken == null) {
                    nameFromNameToken = token;
                }
                continue;
            }
            if (BondIdentifier.looksLikeCodeToken(token)) {
                List<String> resolved = BondIdentifier.resolveCode(token, dict);
                if (resolved.isEmpty()) {
                    unresolvedCodeLike.add(token);
                    codeShapedUnresolved.add(token);
                } else {
                    codes.addAll(resolved);
                    if (codeFromCodeToken == null) {
                        codeFromCodeToken = resolved.get(0);
                    }
                }
                continue;
            }
            if (!containsDigit(token)) {
                unknownWords.add(token);
            }
        }

        boolean hasNumericFinding = !priceCandidates.isEmpty() || !volumeCandidates.isEmpty()
                || q.BidPrice != null || q.BPFloat != null;
        if (!hasNumericFinding && codes.isEmpty() && q.Direction == null && unresolvedCodeLike.isEmpty()) {
            return null;
        }

        if (!codes.isEmpty()) {
            q.lstCode = new ArrayList<String>(codes);
            q.Code = q.lstCode.get(0);
            q.Name = nameFromNameToken != null ? nameFromNameToken : dict.nameOf(q.Code);
        }
        if (!volumeCandidates.isEmpty()) {
            q.Volume = volumeCandidates.get(volumeCandidates.size() - 1);
        }
        if (!priceCandidates.isEmpty()) {
            q.lstSingle = new ArrayList<String>(priceCandidates);
        }
        // Numeric fallback is decided at token position; dates also survive without a direction.
        if (!unresolvedCodeLike.isEmpty()) {
            q.lstNumber = new ArrayList<String>(unresolvedCodeLike);
        }
        if (!unknownWords.isEmpty()) {
            q.traderName = unknownWords.get(0);
        }
        if (!codeShapedUnresolved.isEmpty()) {
            q.debug = "Can't find code " + codeShapedUnresolved.get(0) + " in bond code database.";
        }
        if (codeFromCodeToken != null && nameFromNameToken != null && codes.size() > 1) {
            q.debug = "Bond code and name conflict:" + codeFromCodeToken + " " + nameFromNameToken
                    + " source:" + text;
        }
        return q;
    }

    /** True when the segment carries a bond but no numbers: legacy calls this a header line. */
    public static boolean looksLikeHeader(String segment, BondDictionary dict) {
        String[] tokens = segment == null ? new String[0] : segment.trim().split("\\s+");
        boolean bond = false;
        boolean number = false;
        for (String token : tokens) {
            if (isNumber(token)) {
                number = true;
                continue;
            }
            if (!BondIdentifier.resolveName(token, dict).isEmpty()
                    || !BondIdentifier.resolveCode(token, dict).isEmpty()) {
                bond = true;
            }
        }
        return bond && !number;
    }

    private static boolean isRange(String token) {
        int dash = token.indexOf('-');
        if (dash <= 0 || dash == token.length() - 1) {
            return false;
        }
        return isNumber(token.substring(0, dash)) && isNumber(token.substring(dash + 1));
    }

    private static boolean containsDigit(String token) {
        for (int i = 0; i < token.length(); i++) {
            if (Character.isDigit(token.charAt(i))) {
                return true;
            }
        }
        return false;
    }

    private static boolean isNumber(String token) {
        if (token == null || token.isEmpty()) {
            return false;
        }
        boolean dot = false;
        for (int i = 0; i < token.length(); i++) {
            char c = token.charAt(i);
            if (c == '.') {
                if (dot || i == 0 || i == token.length() - 1) {
                    return false;
                }
                dot = true;
            } else if (!Character.isDigit(c)) {
                return false;
            }
        }
        return true;
    }
}
