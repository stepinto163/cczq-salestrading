package cgws.ficc.parser.identify;

import cgws.ficc.parser.dict.BondDictionary;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/** Dictionary-indexed bond identification. Market-qualified entries are required for bare numbers. */
public final class BondIdentifier {

    /** Market suffixes seen in the dictionary loader ({@code .ib}, {@code .bj}) and in ParserSvr. */
    private static final String[] SUFFIXES = new String[]{".ib", ".sh", ".sz", ".bj"};

    private BondIdentifier() {
    }

    public static boolean looksLikeCodeToken(String token) {
        if (token == null) {
            return false;
        }
        String t = BondDictionary.normalize(token);
        if (t.isEmpty()) {
            return false;
        }
        for (String suffix : SUFFIXES) {
            if (t.endsWith(suffix)) {
                return true;
            }
        }
        return isDigits(t) && t.length() == 7;
    }

    /** True for numeric tokens that the legacy binary emits as {@code Number} instead of a code. */
    public static boolean looksLikeCodeButUnresolved(String token, BondDictionary dict) {
        String t = BondDictionary.normalize(token);
        return isDigits(t) && t.length() >= 6 && resolveCode(t, dict).isEmpty();
    }

    /**
     * Candidate codes for one token, best first. Never scans the whole dictionary: lookups are
     * map based, so cost does not grow with the number of bonds.
     */
    public static List<String> resolveCode(String token, BondDictionary dict) {
        String t = BondDictionary.normalize(token);
        List<String> out = new ArrayList<String>();
        if (t.isEmpty() || dict == null || dict.isEmpty()) {
            return out;
        }
        if (hasSuffix(t)) {
            return dict.containsCode(t) ? single(t) : out;
        }
        String ib = t + ".ib";
        if (dict.containsCode(ib)) {
            // Confirmed: the legacy binary emits only the .ib variant for a bare 7 digit token,
            // even when the plain code is also present (case C001).
            return single(ib);
        }
        return dict.containsCode(t) ? single(t) : out;
    }

    /** Bare numeric codes require a market entry; confirmed for six, seven and nine digits. */
    public static List<String> resolveNumericCode(String token, BondDictionary dictionary) {
        String normalized = BondDictionary.normalize(token);
        if (dictionary.containsCode(normalized + ".ib")) {
            return single(normalized + ".ib");
        }
        return Collections.emptyList();
    }

    private static List<String> single(String code) {
        List<String> out = new ArrayList<String>();
        out.add(code);
        return out;
    }

    /** Candidate codes for a bond name / alias. */
    public static List<String> resolveName(String token, BondDictionary dict) {
        String t = BondDictionary.normalize(token);
        if (t.isEmpty() || dict == null || dict.isEmpty()) {
            return Collections.emptyList();
        }
        return order(new ArrayList<String>(dict.codesForName(t)));
    }

    public static boolean isDigits(String s) {
        if (s.isEmpty()) {
            return false;
        }
        for (int i = 0; i < s.length(); i++) {
            if (!Character.isDigit(s.charAt(i))) {
                return false;
            }
        }
        return true;
    }

    private static boolean hasSuffix(String t) {
        for (String suffix : SUFFIXES) {
            if (t.endsWith(suffix)) {
                return true;
            }
        }
        return false;
    }

    private static List<String> order(List<String> codes) {
        if (codes.size() < 2) {
            return codes;
        }
        Collections.sort(codes, new java.util.Comparator<String>() {
            @Override
            public int compare(String a, String b) {
                int ra = rank(a);
                int rb = rank(b);
                if (ra != rb) {
                    return ra - rb;
                }
                return a.compareTo(b);
            }
        });
        return codes;
    }

    /** .ib first (legacy resolves 2400001 to 2400001.ib), then exchange suffixes, then plain. */
    private static int rank(String code) {
        if (code.endsWith(".ib")) {
            return 0;
        }
        if (hasSuffix(code)) {
            return 1;
        }
        return 2;
    }
}
