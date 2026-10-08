package cgws.ficc.parser.text;

/**
 * Deterministic text normalisation: same input always yields the same working text, no locale,
 * no clock, no I/O.
 *
 * <p>Rules currently reproduced from the legacy pipeline (bytecode of
 * {@code BondQuoteParser.parsing} + observed native output):
 * <ul>
 *   <li>drop {@code hh:mm:ss} timestamps,</li>
 *   <li>full width brackets/colon to ASCII,</li>
 *   <li>bid/ofr markers kept as words, {@code --/--} empties removed,</li>
 *   <li>Chinese volume words and comma grouped numbers expanded by the caller rules,</li>
 *   <li>trim + lower case + one trailing space (legacy PreProcess appends a trailing space).</li>
 * </ul>
 */
public final class TextNormalizer {

    private static final String TIME = "([0]?\\d{1}|1\\d{1}|2[0-3]):[0-5]\\d{1}:([0-5]\\d{1})";

    private TextNormalizer() {
    }

    /** Stage 1: what the legacy Java layer does before it calls native {@code PreProcess}. */
    public static String javaStage(String raw) {
        if (raw == null) {
            return "";
        }
        String s = raw.replaceAll(TIME, "");
        s = replaceFullWidth(s);
        s = s.replace("（", "(").replace("）", ")")
                .replace("｛", "{").replace("｝", "}")
                .replace("＜", "<").replace("＞", ">")
                .replace("：", ":");
        return s;
    }

    /** Stage 2: equivalent of native {@code PreProcess} for the observed sample set. */
    public static String preProcess(String raw) {
        String s = javaStage(raw);
        s = s.trim();
        // Legacy PreProcess keeps the text and appends a single trailing space.
        return s.isEmpty() ? "" : s + " ";
    }

    /** Splits the working text into segments the way the legacy loop does. */
    public static String[] segments(String text) {
        String s = text == null ? "" : text.toLowerCase();
        return s.split("；|;|,|，|\t");
    }

    private static String replaceFullWidth(String s) {
        StringBuilder b = new StringBuilder(s.length());
        for (int i = 0; i < s.length(); i++) {
            char c = s.charAt(i);
            if (c >= 0xFF01 && c <= 0xFF5E) {
                b.append((char) (c - 0xFEE0));
            } else if (c == 0x3000) {
                b.append(' ');
            } else {
                b.append(c);
            }
        }
        return b.toString();
    }
}
